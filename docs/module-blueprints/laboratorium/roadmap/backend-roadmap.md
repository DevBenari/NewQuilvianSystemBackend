# Roadmap Delivery Backend — Modul Laboratorium

| Field | Value |
|---|---|
| `blueprint_id` | `LAB-BP-001` |
| Roadmap revision | `13` |
| Status | `DRAFT` |
| Bentuk blueprint | `SINGLE` |
| Ditulis oleh | `plan-module-delivery` |
| Tanggal | 2026-09-02 |
| Manifest | `blueprint-manifest.md` revision `24` |
| Backend SHA | `c87d9c0` |
| Frontend SHA | `688daff90` |
| Contract version | `LAB-API-v1` r5 (amandemen r4 aditif dan r5 breaking atas r3, 2026-09-03), `LAB-STATE-v1` r2, `LAB-VAL-v1` r3, `LAB-INT-v1` r3, `LAB-PERM-v1` r3 — seluruhnya `approved`, dikunci 2026-09-02 |
| Masukan | Decisions rev `21`; capability map rev `2`; `LAB-RCG-001` rev 5; `LAB-DA-001` rev 4 |
| Input hash | `sha256:6504b18a327b9966526bd1df8f3cb878d7f6d6519dacc1f7df16b1066729ae82` (decisions), dihitung 2026-09-02 |
| Slice in scope | `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |

> **Dokumen ini bukan izin menulis kode.** Ia daftar pekerjaan beserta syaratnya. Satu task baru
> boleh dikerjakan setelah disetujui satu per satu, lewat
> `quilvian-engineering-skills:build-module-backend`.

---

## 1. Gerbang yang Berlaku untuk Seluruh Task

Empat penghambat berikut dulu menghalangi **eksekusi** roadmap ini. **Keempatnya ditutup pada
2026-09-02.** Yang tersisa hanya `LAB-OPEN-018b`, sebuah utang pemeliharaan yang tidak menahan
satu task pun.

| ID | Isi | Siapa yang mencabut | Yang tertahan |
|---|---|---|---|
| ~~`LAB-OPEN-018`~~ | ~~Rules root runtime tidak memuat `GLOBAL_RULES.md` maupun `rules/backend/engineering/`~~ | ✅ **Ditutup 2026-09-02** | Rules root runtime kini memuat **32 berkas**, naik dari 13. Gerbang `AGENTS.md` tidak lagi aktif |
| `LAB-OPEN-018b` | Marketplace `quilvian` masih terdaftar ke `MHamzah1/QuilvianEngineeringSkillsClaude` | **Muhammad Hamzah** atau pemilik mesin | **Tidak menahan implementasi.** Tetapi `/plugin update` berikutnya akan mengembalikan rules root ke 13 berkas dan menghidupkan lagi gerbangnya |
| ~~`LAB-OPEN-019`~~ | ~~Lifecycle registry masih `PLANNED`~~ | ✅ **Ditutup 2026-09-02** oleh Muhammad Hamzah | Baris registry kini `ACTIVE`. Entity `Lab*` dan migration tidak lagi tertahan `QBE-MOD-002` |
| ~~`LAB-OPEN-020`~~ | ~~Checker QBE gagal `TOOL ERROR`~~ | ✅ **Ditutup 2026-09-02** atas persetujuan Andry Zain | Empat rujukan `agents/rules/engineering/` diganti `docs/engineering/`. Checker dijalankan ulang: `Final result: PASS`, exit 0 |
| ~~`LAB-OPEN-021`~~ | ~~Prefix dua tabel batas nilai~~ | ✅ **Ditutup 2026-09-02** oleh Muhammad Hamzah | Ditetapkan `Lab`. Kedua tabel bernama `LabValueBound` dan `LabValueOption` |

**Keempat gerbang eksekusi sudah terbuka.** Task backend kini boleh dikerjakan satu per satu
lewat `quilvian-engineering-skills:build-module-backend`, dengan approval per task.

> **Cara `LAB-OPEN-018` ditutup, dan utang yang menyertainya.** Marketplace `quilvian` yang
> terpasang menunjuk `MHamzah1/QuilvianEngineeringSkillsClaude` — repo dua commit yang **tidak
> pernah** memuat `rules/backend/engineering/` maupun `GLOBAL_RULES.md` di commit mana pun. Jadi
> `/plugin update` memang tidak akan menolong.
>
> Atas persetujuan pilihan **B**, rules root runtime disegarkan langsung dari sumber canonical
> `DevBenari/QuilvianEngineeringSkills` yang ada sebagai clone lokal. Hasilnya 32 berkas, naik
> dari 13, termasuk `GLOBAL_RULES.md`, kedua dokumen tata kelola, `rule-output/bentuk-blueprint.md`,
> dan 10 rules frontend yang sebelumnya hilang.
>
> **Ini penyegaran manual, bukan pemasangan ulang.** Pendaftaran marketplace belum berubah,
> sehingga `/plugin update` berikutnya akan menimpanya kembali menjadi 13 berkas. Perbaikan
> tetapnya dicatat sebagai `LAB-OPEN-018b`: daftarkan ulang marketplace ke sumber canonical.

**Yang tetap perlu diperhatikan sebelum mengeksekusi task:** `CLAUDE.md` backend mewajibkan
setiap implementasi berjalan lewat `build-module-backend`, dan **pembuatan maupun eksekusi
migration memerlukan konfirmasi terpisah** untuk masing-masing tindakan.

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
| `MVP-1` | `BE-LAB-08` .. `BE-LAB-10`, `BE-LAB-16`, `BE-EXT-02`, `BE-EXT-03` | `S13a`, `S13b`, `S1a` | Pendaftaran adalah hulu alur; penanda cito melekat pada pemeriksaan yang dibuat di situ |
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

> **Status: `SELESAI` — 2026-09-02.** Seluruh butir DoD terpenuhi. Source, test, pembuatan
> migration, dan eksekusi migration ke `QuilvianNewDevYoga` selesai dan terverifikasi; jalur
> `Down` ikut dibuktikan. Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-01.md`](../task/report/backend/BE-LAB-01.md).
>
> Dua butir yang berada **di luar** DoD task ini tetap perlu keputusan pemilik, dicatat pada
> laporan bagian 3.3 dan 6.2: ruas `discipline` pada `CreateLabOrderRequest` sengaja dibuat
> **tidak wajib** agar `LAB-API-v1` r3 tidak dilanggar, sehingga `INV-21` bagian "wajib memiliki
> tepat satu disiplin" belum tegak penuh; dan `AC-41` baru terpenuhi separuh karena daftar pantau
> per disiplin adalah cakupan `BE-LAB-15`.

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

> **Status: `SELESAI` — 2026-09-02.** Seluruh butir DoD terpenuhi. Source, test, pembuatan
> migration, dan eksekusi migration ke `QuilvianNewDevYoga` selesai dan terverifikasi; jalur
> `Down` ikut dibuktikan, dan index unik `VAL-21` diuji langsung menolak baris duplikat. Laporan
> lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-02.md`](../task/report/backend/BE-LAB-02.md).
>
> **Kedua temuan yang semula terbuka sudah ditutup 2026-09-02** atas persetujuan pemilik, lewat
> migration `20260902091736_AmendLabValueBoundUniquenessAndSortOrder`. Celah `NULL` pada index
> unik `VAL-21` ditutup di database dengan `NULLS NOT DISTINCT` — dua baris batas "semua umur"
> untuk kombinasi yang sama kini ditolak, dan itu diuji ulang terhadap `QuilvianNewDevYoga`.
> `LabValueBound.SortOrder` dibuang karena melanggar QBE-ENT-003; `LabValueOption.SortOrder`
> dipertahankan karena di sana urutan menyatakan tingkatan skala ordinal hasil, bukan tampilan.
> Rinciannya pada laporan bagian 10.

| Butir | Isi |
|---|---|
| **Outcome** | Satu jenis pemeriksaan dapat memiliki beberapa baris batas nilai menurut jenis kelamin dan kelompok umur, dalam dua bentuk hasil: angka dan pilihan terbatas |
| **Requirement/decision** | `FR-03.1`, `FR-03.2`, `FR-03.6`, `LAB-DEC-006`, `LAB-DEC-018`, `LAB-DEC-021` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Value Bound; `LAB-VAL-v1` r3 `VAL-21` .. `VAL-24` |
| **Reuse** | `CAP-07` `Missing`. Menunjuk `MstProcedure` dan `MstAgeCategory` yang sudah ada |
| **Cakupan** | Dua entity beserta configuration, DbSet, dan migration. **`MstProcedure` tidak bertambah satu kolom pun** (`FR-03.6`) |
| **Dependency** | — (`LAB-OPEN-021` sudah dijawab 2026-09-02: prefix `Lab`) |
| **Acceptance criteria** | `AC-24`, `AC-25`, `AC-28`, `AC-49` |
| **Verifikasi** | Uji integrasi: tiga baris batas Hemoglobin — pria dewasa, wanita dewasa, anak — tersimpan berdampingan; baris keempat berkombinasi sama ditolak `409` dengan pesan `VAL-21`. Uji unit `AC-25`: telusuri skema `MstProcedure` setelah seluruh migration, pastikan nol kolom baru |
| **Risiko/pemilik** | Sedang. Nama tabelnya **sudah ditetapkan** `LabValueBound` dan `LabValueOption` lewat `LAB-OPEN-021` pada 2026-09-02; memakai `Mst` sekarang justru melanggar keputusan itu dan akan dilaporkan checker sebagai pelanggaran `QBE-MOD-002`. Pemilik: Laboratorium |
| **DoD** | Nama tabel sesuai jawaban registry, dua entity ada beserta configuration di `Repositories/Configurations/HealthServices/LaboratoryManagement/`, migration jalan dua arah, `AC-25` terbukti, checker QBE lolos |

### `BE-LAB-03` — Riwayat dan pengajuan perubahan batas kritis

> **Status: `SELESAI` — 2026-09-02.** Seluruh butir DoD terpenuhi. Source, test, pembuatan
> migration, dan eksekusi migration ke `QuilvianNewDevYoga` selesai dan terverifikasi; jalur
> `Down` ikut dibuktikan. Terhadap database sungguhan juga dibuktikan bahwa pengajuan berstatus
> `Submitted` **tidak menggerakkan** batas kritis yang berlaku, dan bahwa batas nilai yang masih
> punya pengajuan tidak dapat dihapus. Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-03.md`](../task/report/backend/BE-LAB-03.md).
>
> Dua tambahan di luar daftar kolom `erd/data-dictionary.md` dicatat pada laporan bagian 3.3,
> keduanya penambahan dan bukan pengurangan: kolom `Version` pada `LabValueBoundChangeRequest`
> mengikuti baris **Reuse** task ini yang menunjuk `CAP-17`, dan delapan kolom fakta
> `LabValueBoundHistory` dipasangi tolak-ubah supaya "riwayat permanen" ditegakkan lapisan
> penyimpanan, bukan hanya oleh ketiadaan endpoint yang mengubahnya.

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

> **Status: `SELESAI` — 2026-09-02.** Seluruh butir DoD terpenuhi. Enam endpoint tersedia dengan
> route, verb, dan `[AccessPermission]` yang cocok satu per satu dengan `LAB-API-v1` r3, dan
> keempat jalur gagal yang diwajibkan — `VAL-22`, `VAL-23`, `VAL-24`, `VAL-28` — seluruhnya
> terbukti. Task ini **tidak menyentuh schema**, sehingga tidak ada migration dan tidak ada
> perintah database yang dijalankan. Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-04.md`](../task/report/backend/BE-LAB-04.md).
>
> Validasi yang diimplementasikan **sepuluh**, bukan empat: `VAL-21` sampai `VAL-30`, karena
> `contracts/validation-matrix.md` menempelkan seluruhnya pada tindakan membuat, mengubah, dan
> menonaktifkan batas nilai — persis keenam endpoint ini.
>
> **Yang perlu diketahui sebelum dipakai:** `VAL-28` kini menolak setiap perubahan batas kritis,
> sementara jalur penggantinya baru ada di `BE-LAB-05`. Sampai task itu selesai, batas kritis
> hanya dapat diubah lewat perintah database langsung. Ini disengaja — lebih baik tertutup rapat
> daripada terbuka diam-diam.

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

> **Status: `SELESAI` — 2026-09-03.** Seluruh butir DoD terpenuhi. Lima endpoint tersedia,
> `VAL-31` sampai `VAL-35` ditegakkan di service, dan larangan menyetujui pengajuan sendiri ada
> sebagai kode — bukan konfigurasi permission, sesuai temuan `CAP-16`. Task ini tidak menyentuh
> schema, sehingga tidak ada migration. Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-05.md`](../task/report/backend/BE-LAB-05.md).
>
> Enam perbaikan lahir dari audit adversarial atas implementasinya, dicatat pada laporan bagian
> 5.1. Yang terpenting: token konkurensi `Version` semula tidak pernah dinaikkan, sehingga
> penjaga `CAP-17` terlihat terpasang padahal tidak pernah menyala sama sekali.
>
> **Peran penyetuju tetap terbuka.** Siapa pemegang `LabCriticalBound : Approve` belum
> ditetapkan manajemen rumah sakit. Selama itu belum terjadi, tidak ada akun yang dapat
> menyetujui, sehingga batas kritis tetap tidak dapat diubah lewat aplikasi. Ini keputusan
> organisasi, bukan cacat teknik.

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

> **Status: `SELESAI` — 2026-09-03.** Seluruh butir DoD terpenuhi. Lima endpoint tersedia dengan
> route, verb, dan `[AccessPermission]` yang cocok satu per satu dengan `LAB-API-v1` r3;
> `VAL-36`, `VAL-37`, dan `VAL-38` masing-masing punya ujinya; dan seeder data awal terdaftar
> pada `Program.cs`. Task ini **tidak menyentuh schema**, sehingga tidak ada migration dan tidak
> ada perintah database yang dijalankan. Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-06.md`](../task/report/backend/BE-LAB-06.md).
>
> Seeder sengaja **hanya menambah kode yang belum ada** dan tidak pernah menimpa baris yang
> sudah tersimpan. Nama, urutan, status aktif, dan kedua penanda terkunci adalah keputusan
> pengguna; menimpanya setiap kali server menyala berarti membatalkan keputusan itu diam-diam.
>
> **Peran penyetel penanda biaya tetap terbuka.** Siapa pemegang
> `LabRejectionReason : SystemFlag` belum ditetapkan manajemen rumah sakit. Selama itu belum
> terjadi, setiap alasan baru yang ditambahkan kepala instalasi akan selalu bernilai "bukan
> kesalahan internal" — artinya pengambilan ulang untuk alasan itu **dapat ditagihkan kepada
> pasien**. Ini keadaan yang perlu diketahui Billing, bukan cacat teknik, dan bentuknya sejenis
> dengan peran penyetuju yang masih terbuka pada `BE-LAB-05`.

| Butir | Isi |
|---|---|
| **Outcome** | Kepala instalasi dapat menambah, mengubah, mengurutkan, dan menonaktifkan alasan penolakan; penanda kesalahan internal hanya dapat disetel administrator sistem |
| **Requirement/decision** | `FR-06.1` .. `FR-06.3`, `LAB-DEC-019` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Rejection Reason; `LAB-PERM-v1` r3 `LabRejectionReason : SystemFlag` |
| **Reuse** | `CAP-05` `Reuse with adapter`. `MstLabRejectionReason` sudah ada tetapi hanya punya jalur baca dan tidak punya seeder |
| **Cakupan** | Lima endpoint: `GET /`, `POST /`, `PUT /{id}`, `PUT /{id}/activation`, `PUT /{id}/system-flags`. Ditambah satu seeder data awal, dan pemisahan tegas antara kolom yang boleh diubah kepala instalasi dan kolom yang terkunci. `GET /lab-specimens/rejection-reasons` yang sudah ada **tetap dipertahankan** sebagai jalur baca saat menolak sampel |
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

### `BE-LAB-19` — `LEGACY MIGRATION`: normalisasi dua tabel `Trx*` Laboratorium

> **Status: `SELESAI` — 2026-09-03.** `TrxLabSpecimen` menjadi `LabSpecimen` dan
> `TrxLabTransitionHistory` menjadi `LabTransitionHistory` — class, berkas, configuration,
> DbSet, seluruh rujukan, **dan tabel fisiknya** dinormalkan bersama, sebagaimana dituntut
> `QBE-NAM-003`. Migration `20260903094528_RenameLaboratoryTrxTablesToLabPrefix` terbukti jalan
> dua arah terhadap `QuilvianNewDevYoga`. Laporan lengkap:
> [`task/report/backend/BE-LAB-19.md`](../task/report/backend/BE-LAB-19.md).
>
> **Tidak ada DROP+CREATE.** EF sendiri menghasilkan DROP+CREATE untuk perubahan ini dan
> memperingatkan *"may result in the loss of data"*; badan migrationnya diganti dengan skrip
> `ALTER ... RENAME` berbasis katalog Postgres, mengikuti preseden
> `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix` dan menghormati `QBE-DB-002`.
>
> **`MstLabRejectionReason` tidak ikut.** Ia master, dan catatan registry 2026-09-02
> menetapkannya diperlakukan legacy dan tidak dinamai ulang. `TrxPatientEncounter` juga tidak:
> ia milik modul Registration.

| Butir | Isi |
|---|---|
| **Outcome** | Seluruh entity operasional Laboratorium berprefix `Lab`, sehingga nama class, berkas, configuration, DbSet, dan tabel menjadi satu paket yang konsisten |
| **Requirement/decision** | Instruksi pemilik modul 2026-09-03; `QBE-NAM-001`, `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` |
| **Kontrak** | Tidak ada kontrak API yang berubah — kedua entity tidak pernah diekspos sebagai DTO |
| **Reuse** | Skrip rename berbasis katalog dari migration Rekam Medis, dipakai ulang apa adanya |
| **Cakupan** | Dua model, dua configuration, dua DbSet, 18 berkas source dan uji, dan satu migration yang menamai ulang 2 tabel, 2 PK, 8 FK, dan 9 index. **Nol perubahan kolom** |
| **Dependency** | `BE-LAB-16` — **`SELESAI`** |
| **Acceptance criteria** | Tidak ada `AC` blueprint yang menuntutnya |
| **Verifikasi** | Audit katalog sebelum rename; build solution; seluruh uji; checker QBE; migration `Up` lalu `Down` lalu `Up` terhadap dev pemilik |
| **Risiko/pemilik** | **Rendah, dan itu bukan kebetulan.** `LAB-OPEN-012` dijawab lebih dulu: `SELECT COUNT(*) FROM public."TrxLabSpecimen"` pada `QuilvianNewDevYoga` menghasilkan **0**, sehingga tidak ada data yang berpindah. Skripnya tetap memakai `RENAME` dan bukan DROP+CREATE, supaya lingkungan lain yang datanya tidak nol tetap aman. Pemilik: Laboratorium |
| **DoD** | Nol `Trx*` tersisa pada modul Laboratorium, tabel fisik ikut dinamai ulang, migration jalan dua arah, checker QBE lolos, registry mencatat wewenangnya |

---

### `BE-LAB-17` — Metadata penyaring dan rekap untuk kelima grup

> **Status: `SELESAI` — 2026-09-03.** Sepuluh endpoint baca tersedia: `GET /filters/metadata`
> dan `GET /summary` pada Lab Order, Lab Specimen, Lab Value Bound, Lab Critical Bound Approval,
> dan Lab Rejection Reason. Task ini **tidak menyentuh schema**, sehingga tanpa migration.
> Laporan lengkap: [`task/report/backend/BE-LAB-17.md`](../task/report/backend/BE-LAB-17.md).
>
> **Asal-usulnya bukan dari perencanaan ini.** Task ditambahkan atas instruksi langsung pemilik
> modul pada 2026-09-03, dan membawa amandemen `LAB-API-v1` dari `r3` menjadi `r4`.
> Amandemennya **aditif sepenuhnya** — tidak satu pun endpoint, ruas, atau nilai enum `r3` yang
> berubah, berganti nama, atau hilang.
>
> **Satu temuan dicatat.** `GET /lab-orders` dan daftar wadah belum menerima parameter query
> apa pun. Metadata keduanya menyatakan itu terbuka lewat `SupportsServerSideFiltering` bernilai
> salah dan daftar parameter kosong, bukan mengarang penyaring yang tidak ada penegakannya.
> Menambahkan penyaringan sungguhan pada kedua daftar itu adalah pekerjaan tersendiri yang
> belum berpemilik task.

| Butir | Isi |
|---|---|
| **Outcome** | Setiap layar Laboratorium dapat merender penyaring, label enum, dan kartu ringkasan tanpa menanamkan daftar nilai di dalam frontend |
| **Requirement/decision** | Instruksi pemilik modul 2026-09-03; `rules/backend/master-data-endpoint-standard.md` bagian 1 dan 2.2 |
| **Kontrak** | `LAB-API-v1` `r4` — amandemen aditif atas `r3` |
| **Reuse** | Pola `filters/metadata` dan `summary` milik modul Rekam Medis dipakai ulang, tidak disalin mentah: bentuk `EnumOption`, `SortOption`, dan `QueryParameterInfo` diadaptasi menjadi milik Laboratorium |
| **Cakupan** | Sepuluh endpoint baca, lima belas DTO baru, satu factory metadata, dan sepuluh method service. **Nol** perubahan schema |
| **Dependency** | `BE-LAB-01` .. `BE-LAB-06` — seluruhnya `SELESAI` |
| **Acceptance criteria** | Tidak ada `AC` blueprint yang menuntutnya; task lahir dari instruksi pemilik modul |
| **Verifikasi** | Uji: kesepuluh endpoint punya route, verb, dan hak akses yang benar; metadata memuat seluruh nilai enum; metadata menyatakan kemampuan penyaringan apa adanya; setiap rekap menghitung angka yang benar dan mengabaikan baris terhapus |
| **Risiko/pemilik** | Rendah. Seluruhnya baca saja dan aditif. Risiko terbesarnya adalah metadata yang menjanjikan penyaring yang tidak diproses — ditutup oleh dua uji yang membandingkan isi metadata dengan parameter endpoint yang sesungguhnya. Pemilik: Laboratorium |
| **DoD** | Sepuluh endpoint tersedia, kontrak dinaikkan ke `r4`, seluruh uji lulus, checker QBE lolos |

---

### `BE-LAB-18` — Penyaring, pengurutan, dan pagination pada daftar pesanan

> **Status: `SELESAI` — 2026-09-03.** `GET /lab-orders` kini menerima `encounterId`,
> `orderStatus`, `discipline`, `startDate`, `endDate`, `search`, `sortBy`, `sortDirection`,
> `pageNumber`, dan `pageSize`. Task ini **tidak menyentuh schema**, sehingga tanpa migration.
> Laporan lengkap: [`task/report/backend/BE-LAB-18.md`](../task/report/backend/BE-LAB-18.md).
>
> **Ini satu-satunya perubahan breaking pada modul Laboratorium sejauh ini.** Bentuk respons
> berubah dari `ApiResponse<List<LabOrderListResponse>>` menjadi
> `ApiResponse<PagedResult<LabOrderListResponse>>`, dan kontrak naik ke `r5`. Satu-satunya
> konsumen yang ditemukan — modul IGD — **tidak putus** karena pembungkusnya sudah menangani
> kedua bentuk, tetapi perilakunya berubah: ia perlu mengirim `?encounterId=` agar tidak
> kehilangan pesanan yang berada di luar halaman pertama.
>
> **Menutup `IGD-DEC-105`.** Modul IGD sebelumnya menarik seluruh pesanan rumah sakit lalu
> menyaringnya di dalam browser, sehingga pesanan pasien lain ikut terkirim ke sana. Komentar
> pada `emergency-assessment-slice.jsx` sudah menyebut perbaikannya milik Laboratorium; inilah
> perbaikan itu.

| Butir | Isi |
|---|---|
| **Outcome** | Layar yang hanya butuh pesanan satu pasien tidak lagi menerima pesanan pasien lain, dan daftar pesanan tidak lagi menarik seluruh tabel |
| **Requirement/decision** | Instruksi pemilik modul 2026-09-03; `IGD-DEC-105` sebagai temuan yang ditutup |
| **Kontrak** | `LAB-API-v1` `r5` — **breaking** pada `GET /lab-orders` |
| **Reuse** | Bentuk `PagedResult<T>` dan pola paging yang sudah dipakai `LabValueBound` serta `LabRejectionReason` |
| **Cakupan** | `LabOrderPagedQuery`, penyaringan/pengurutan/pagination pada `LabOrderService.GetListAsync`, dan penyesuaian metadata Lab Order agar tetap jujur. **Nol** perubahan schema |
| **Dependency** | `BE-LAB-17` — **`SELESAI`** |
| **Acceptance criteria** | Tidak ada `AC` blueprint yang menuntutnya |
| **Verifikasi** | Uji: penyaringan per kunjungan, per status, dan per disiplin; bentuk paging; batas ukuran halaman; pengurutan bawaan dan menaik; kolom urutan tak dikenal kembali ke bawaan; baris terhapus tidak tampil; metadata mengaku menyaring dan seluruh parameternya nyata |
| **Risiko/pemilik** | **Sedang** — satu-satunya perubahan breaking modul ini. Diturunkan oleh penilaian dampak konsumen yang membuktikan IGD tidak putus. Pemilik: Laboratorium |
| **DoD** | Sepuluh parameter diterima dan diproses, kontrak naik ke `r5`, dampak konsumen dinilai dan dilaporkan, seluruh uji lulus, checker QBE lolos |

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

> **Status: `SELESAI` — 2026-09-03.** Seluruh butir DoD terpenuhi. Entity bernama
> `LabExamination` — bukan `TrxLabExamination` — beserta configuration di folder submodul,
> DbSet, dan migration `20260903071535_AddLabExamination`. Migration **terbukti jalan dua
> arah** terhadap `QuilvianNewDevYoga`: `Up`, lalu `Down`, lalu `Up` kembali. Checker QBE
> `PASS` tanpa satu pun violation. Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-09.md`](../task/report/backend/BE-LAB-09.md).
>
> **Yang perlu diketahui sebelum `BE-LAB-16` dikerjakan.** Keenam kolom yang menurut kamus data
> harus pindah dari `TrxLabSpecimen` **belum dipindahkan** — pemindahannya milik `BE-LAB-11`
> yang masih `BLOCKED` oleh `LAB-OPEN-012`. Selama itu, `ProcedureId` dan salinan tarif ada di
> dua tempat sekaligus. Risikonya belum aktif karena belum ada kode yang menulis ke
> `LabExamination`, tetapi ia menjadi aktif begitu `BE-LAB-16` dibangun mendahului `BE-LAB-11`.
>
> **Satu selisih dokumen ditemukan.** `erd/data-dictionary.md` bagian 4 menyatakan
> `TrxLabTransitionHistory` bertambah kolom `LabExaminationId`, sementara bagian 8.3 roadmap ini
> menyatakan tabel itu dipakai apa adanya tanpa pekerjaan struktur. Task mengikuti roadmap;
> penyelarasan kedua dokumen menjadi utang pemilik blueprint.

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

### `BE-LAB-16` — Endpoint pemeriksaan terpesan

> **Status: `SELESAI` — 2026-09-03.** Empat endpoint tersedia dengan route, verb, dan
> `[AccessPermission]` yang cocok satu per satu dengan `LAB-API-v1` r3. Task ini **tidak
> menyentuh schema**, sehingga tanpa migration. Laporan lengkap:
> [`task/report/backend/BE-LAB-16.md`](../task/report/backend/BE-LAB-16.md).
>
> **Kartu ini pernah keliru, dan sudah diselaraskan 2026-09-03.** Butir Verifikasi dan DoD-nya
> semula menyebut `VAL-05` dan `VAL-07`, padahal bagian 8.2 dokumen ini menempatkan `VAL-05` ..
> `VAL-16` pada `BE-LAB-12` dan `VAL-17` .. `VAL-20` pada `BE-LAB-16`. Keduanya kini berbunyi
> `VAL-17` sampai `VAL-20`, sesuai bagian 8.2. Alasannya: `VAL-05` — *wadah tanpa satu pun
> pemeriksaan* — melekat pada endpoint perencanaan wadah yang **tidak dimiliki** task ini,
> sehingga tidak pernah dapat ditegakkan dari sini. Inti `VAL-07` — larangan jenis pemeriksaan
> yang sama dua kali pada satu wadah — tetap ditegakkan, karena jalur tambah pemeriksaan memang
> dapat melanggarnya; ia disebut apa adanya pada Verifikasi tanpa mengklaim nomor aturan milik
> `BE-LAB-12`.

> **Kenapa nomornya melompat.** Task ini ditemukan 2026-09-02 lewat audit cakupan endpoint,
> setelah `BE-LAB-01` sampai `BE-LAB-15` sudah bernomor. Nomor task adalah identitas tetap, bukan
> urutan kerja — jadi ia diberi nomor berikutnya dan diletakkan pada gelombang yang benar.

| Butir | Isi |
|---|---|
| **Outcome** | Petugas dapat melihat pemeriksaan apa saja yang ada pada satu pesanan dan pada satu wadah, menambah pemeriksaan terpesan, dan membatalkan satu pemeriksaan tanpa menyentuh yang lain |
| **Requirement/decision** | `FR-02.1`, `FR-02.2`, `LAB-DEC-024`, `LAB-DEC-026` |
| **Kontrak** | `LAB-API-v1` r3 grup Lab Examination, base `api/v1/health-services/laboratory-management/lab-examinations` |
| **Reuse** | `CAP-02` `Extend`; `CAP-13` kewenangan per aksi; `CAP-17` `Version` untuk konkurensi |
| **Cakupan** | Empat endpoint: `GET /by-order/{labOrderId}`, `GET /by-specimen/{specimenId}`, `POST /by-order/{labOrderId}`, `POST /{id}/cancel`. Hak akses `LabExamination : Read` dan `: Create` dan `: Update` |
| **Dependency** | `BE-LAB-09` |
| **Acceptance criteria** | `AC-35`, `AC-36` |
| **Verifikasi** | Uji integrasi: menambah dua pemeriksaan pada satu wadah menghasilkan dua baris yang dapat dibaca lewat `GET /by-specimen/{specimenId}` dengan satu barcode yang sama. Jalur gagal: jenis pemeriksaan bukan laboratorium ditolak `422` `VAL-17`; wadah yang sudah dinyatakan layak atau ditolak menolak pemeriksaan baru `409` `VAL-18`; membatalkan pemeriksaan yang sudah gugur bersama wadahnya ditolak `409` `VAL-19`; tarif yang belum diatur ditolak `422` `VAL-20`. Ditambah larangan jenis pemeriksaan yang sama dua kali pada satu wadah, ditolak `409` |
| **Risiko/pemilik** | Sedang. `POST /{id}/cancel` membatalkan **satu** pemeriksaan dan **tidak** boleh disalahartikan sebagai penolakan wadah — penolakan wadah menggugurkan seluruh isinya dan ditangani `BE-LAB-12`. Mencampur keduanya melanggar `VAL-13`. Pemilik: Laboratorium |
| **DoD** | Empat endpoint tersedia dan terdokumentasi Swagger, `VAL-17` sampai `VAL-20` terbukti, pembatalan satu pemeriksaan tidak mengubah status pemeriksaan lain pada wadah yang sama |

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

> **Penahannya dicabut 2026-09-03, dan risikonya ikut runtuh.** `LAB-OPEN-012` menanyakan satu
> angka: berapa baris wadah yang sudah terisi. Pemilik modul menjalankan
> `SELECT COUNT(*) FROM public."TrxLabSpecimen"` pada `QuilvianNewDevYoga` dan hasilnya **0**.
> Dugaan blueprint terbukti — tidak ada data yang perlu dipindahkan, sehingga seluruh kerumitan
> pemindahan gugur dan yang tersisa hanya migration penghapusan kolom biasa.
>
> **Yang belum tertutup.** `QuilvianNewDevYoga` adalah basis data **pengembangan** pemilik
> modul, bukan produksi. Bila produksi adalah instance lain, angkanya perlu diambil di sana
> sebelum migration ini dijalankan ke lingkungan tersebut. Wewenang menjalankan migration di
> luar dev pemilik tetap terpisah.
>
> **Namanya sudah berubah.** Sejak `BE-LAB-19`, tabelnya bernama `LabSpecimen`. Kolom yang akan
> dihapus tetap keenam kolom yang sama.

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

> **Status: `SELESAI` — 2026-09-03.** Ketiga endpoint berperilaku baru, dan `VAL-05` sampai
> `VAL-15` masing-masing punya ujinya. Task ini **tidak menyentuh schema**, sehingga tanpa
> migration. Laporan lengkap:
> [`task/report/backend/BE-LAB-12.md`](../task/report/backend/BE-LAB-12.md).
>
> **Urutan dibalik terhadap yang tertulis.** Kartu ini menyebut dependency `BE-LAB-11`, tetapi
> keenam kolom yang hendak dihapus `BE-LAB-11` masih dipakai `LabSpecimenService` di delapan
> tempat — termasuk muatan fakta tagihan. Menghapusnya lebih dulu akan mematahkan build
> sekaligus jalur tagihan. Karena itu `BE-LAB-12` dikerjakan lebih dulu supaya kode berhenti
> memakai kolom itu, dan `BE-LAB-11` menyusul menghapus kolom yang sudah mati. Setiap langkah
> tetap dapat dibangun dan diuji sendiri.
>
> **`VAL-09` ditulis sebagai kode.** Aturan empat mata pada tingkat wadah ditegakkan di dalam
> service, bukan lewat konfigurasi permission — `CAP-16` sudah membuktikan
> `AccessPermissionService.HasAccessAsync` tidak pernah membandingkan pelaku sebelumnya.
>
> **Kode status berubah.** Sebelumnya seluruh pelanggaran menjadi `400`. Kini mengikuti matriks:
> `422` untuk pelanggaran isi, `409` untuk bentrokan keadaan, `403` untuk `VAL-09`.
>
> **Yang belum: penerbitan fakta per pemeriksaan.** Penetapan layak masih menerbitkan satu fakta
> per wadah. Memecahnya menjadi satu fakta per pemeriksaan adalah cakupan `BE-LAB-13`.

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
| **Risiko/pemilik** | **Tinggi**, karena dua hal. **(a)** Perubahan ini `breaking` — bentuk permintaan dan jawaban ketiga endpoint berubah, sehingga pemakai lama wajib diidentifikasi lebih dulu. **(b)** `VAL-09` mensyaratkan penolakan bila petugas yang menyatakan wadah layak adalah orang yang sama dengan yang mengambil sampelnya. Itu aturan **empat mata pada tingkat wadah**, dan `CAP-16` sudah membuktikan sistem permission tidak dapat menegakkannya — `AccessPermissionService.HasAccessAsync` tidak pernah membandingkan pelaku sebelumnya. Aturan ini wajib ditulis di dalam service. Pemilik: Laboratorium |
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
| **Cakupan** | Tiga endpoint monitoring — `GET /clinical-pathology`, `GET /anatomic-pathology`, `GET /microbiology` — ditambah `GET /lab-orders/by-discipline/{discipline}`. Seluruhnya memakai penyaring yang sama: pasien, nomor rekam medis, nomor pesanan, periode, jenis kunjungan, unit atau ruangan, penjamin, status pesanan, status wadah, dan penanda cito |
| **Dependency** | `BE-LAB-01`, `BE-LAB-14` |
| **Acceptance criteria** | `AC-41`, `AC-42`, `AC-19` |
| **Verifikasi** | Uji integrasi: ketiga daftar dibuka dengan data campuran, masing-masing hanya menampilkan pesanan berdisiplin sesuai jalurnya. **Uji unit `AC-42`: telusuri seluruh endpoint dan tabel Laboratorium, pastikan tidak ada satu pun yang melayani Bank Darah** |
| **Risiko/pemilik** | Rendah. Tiga jalur terpisah adalah keputusan sadar, bukan duplikasi — bukti lapangan menunjukkan laboratorium memakai tiga daftar sejajar sebagai tiga menu berbeda karena petugasnya pun berbeda. Pemilik: Laboratorium |
| **DoD** | Tiga endpoint tersedia dengan penyaring identik, `AC-41` dan `AC-42` terbukti |

---

## 7. Ringkasan Status Task

| Task | Gelombang | Slice | Status rencana | Penahan spesifik |
|---|---|---|---|---|
| `BE-LAB-01` | `MVP-0` | `S15` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-01.md) | Tidak ada |
| `BE-LAB-02` | `MVP-0` | `S3` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-02.md) | Tidak ada |
| `BE-LAB-03` | `MVP-0` | `S3` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-03.md) | Tidak ada |
| `BE-LAB-04` | `MVP-0` | `S3` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-04.md) | Tidak ada |
| `BE-LAB-05` | `MVP-0` | `S3` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-05.md) | Dibangun; belum dapat dipakai sampai peran penyetuju ditetapkan manajemen |
| `BE-LAB-06` | `MVP-0` | `S11` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-06.md) | Dibangun; penanda biaya belum dapat disetel sampai pemegang `LabRejectionReason : SystemFlag` ditetapkan manajemen |
| `BE-LAB-07` | `MVP-0` | `S14` | Siap direncanakan | `BE-EXT-01` |
| `BE-LAB-17` | `MVP-0` | `S3`, `S11` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-17.md) | Tidak ada |
| `BE-LAB-18` | `MVP-0` | `S3` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-18.md) | Tidak ada |
| `BE-LAB-19` | `MVP-0` | `S2` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-19.md) | Tidak ada |
| `BE-EXT-01` | `MVP-0` | `S14` | Menunggu `master-data` | Dependency eksternal |
| `BE-EXT-02` | `MVP-1` | `S13b` | Menunggu `master-data` | Dependency eksternal |
| `BE-EXT-03` | `MVP-1` | `S13a`, `S13b` | Menunggu `registration-management` | Dependency eksternal |
| `BE-LAB-08` | `MVP-1` | `S13a`, `S13b` | Siap direncanakan | `BE-EXT-02`, `BE-EXT-03` |
| `BE-LAB-09` | `MVP-1` | `S2` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-09.md) | Tidak ada |
| `BE-LAB-16` | `MVP-1` | `S2` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-16.md) | Tidak ada |
| `BE-LAB-10` | `MVP-1` | `S1a` | Siap direncanakan | Penahan `BE-LAB-09` sudah dicabut 2026-09-03 |
| `BE-LAB-11` | `MVP-2` | `S2` | Siap direncanakan | Penahan dicabut; kode sudah berhenti memakai keenam kolomnya sejak `BE-LAB-12` |
| `BE-LAB-12` | `MVP-2` | `S2` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-12.md) | Tidak ada |
| `BE-LAB-13` | `MVP-2` | `S10` | Siap direncanakan | `BE-LAB-11`, `BE-LAB-12` |
| `BE-LAB-14` | `MVP-3` | `S7` | Siap direncanakan | `BE-LAB-10`, `BE-LAB-12` |
| `BE-LAB-15` | `MVP-3` | `S15` | Siap direncanakan | `BE-LAB-14` — penahan `BE-LAB-01` sudah dicabut 2026-09-02 |

**Dua task berstatus `BLOCKED` penuh dan satu sebagian**, di luar gerbang global yang berlaku
untuk semuanya. Tidak satu pun `BLOCKED` itu dapat dicabut oleh modul Laboratorium sendiri.

---

## 8. Cakupan

Lima dimensi diperiksa terpisah. Cakupan `FR` dan `AC` yang penuh **tidak** menjamin keempat
dimensi lain ikut penuh — pelajaran dari lubang endpoint yang ditemukan revision 2.

### 8.1 Endpoint

Ditambahkan revision 2 setelah audit menemukan satu grup endpoint tanpa pemilik task. Tabel ini
memastikan **setiap** endpoint To-Be pada `contracts/api-contract.md` punya task yang
mengerjakannya.

| Grup endpoint | Jumlah | Task pemilik |
|---|---:|---|
| Lab Order — `GET /by-discipline/{discipline}` | 1 | `BE-LAB-15` |
| Lab Examination — `GET /by-order`, `GET /by-specimen`, `POST /by-order`, `POST /{id}/cancel` | 4 | `BE-LAB-16` |
| Lab Examination — `PUT /{id}/urgency`, `PUT /{id}/duplo` | 2 | `BE-LAB-10` |
| Lab Specimen — `POST /by-order`, `POST /{id}/accept`, `POST /{id}/reject` | 3 | `BE-LAB-12` |
| Lab Value Bound | 6 | `BE-LAB-04` |
| Lab Critical Bound Approval | 5 | `BE-LAB-05` |
| Lab Worklist | 2 | `BE-LAB-14` |
| Lab Rejection Reason | 5 | `BE-LAB-06` |
| Lab Patient Registration | 3 | `BE-LAB-08` |
| Lab Catalog | 3 | `BE-LAB-07` |
| Lab Monitoring | 3 | `BE-LAB-15` |
| **Total** | **37** | **Seluruhnya terpetakan** |

**Lubang yang ditemukan audit ini.** Empat endpoint grup Lab Examination — membaca pemeriksaan
pada satu pesanan, membaca pemeriksaan pada satu wadah, menambah pemeriksaan, dan membatalkan
satu pemeriksaan — semula tidak dimiliki task mana pun. `BE-LAB-09` hanya mencakup entity-nya,
`BE-LAB-10` hanya penanda cito dan duplo. `BE-LAB-16` menutup lubang itu.

**Kenapa lubangnya sempat lolos.** Cakupan `FR` dan `AC` sudah lengkap 45 dari 45 dan 30 dari
30, sehingga terlihat aman. Endpoint adalah dimensi ketiga yang tidak ikut terperiksa oleh
kedua hitungan itu — sebuah `FR` dapat dianggap tercakup walaupun sebagian endpoint yang
melayaninya belum ada pemiliknya.

### 8.2 Aturan validasi

Kelima puluh aturan pada `contracts/validation-matrix.md` terbagi rapi per bagian, dan setiap
bagian jatuh utuh ke satu task. **Tidak ada aturan yang tanpa pemilik.**

| Bagian matriks validasi | Aturan | Task pemilik |
|---|---|---|
| 1. Pesanan dan Kesegeraan | `VAL-01` .. `VAL-04` | `BE-LAB-01` untuk `VAL-01`; `BE-LAB-10` untuk `VAL-02` .. `VAL-04` |
| 2. Wadah Fisik | `VAL-05` .. `VAL-16` | `BE-LAB-12` |
| 3. Pemeriksaan Terpesan | `VAL-17` .. `VAL-20` | `BE-LAB-16` |
| 4. Batas Nilai | `VAL-21` .. `VAL-30` | `BE-LAB-04`, dengan `BE-LAB-02` untuk yang menyangkut struktur |
| 5. Pengajuan Perubahan Batas Kritis | `VAL-31` .. `VAL-35` | `BE-LAB-05` |
| 6. Alasan Penolakan Sampel | `VAL-36` .. `VAL-38` | `BE-LAB-06` |
| 6b. Pendaftaran Pasien | `VAL-40` .. `VAL-45` | `BE-LAB-08` |
| 6c. Katalog, Harga, Cakupan | `VAL-46` .. `VAL-50` | `BE-LAB-07` |
| 7. Daftar Kerja | `VAL-39` | `BE-LAB-14` |

**Satu aturan keselamatan yang sempat tidak tersebut di mana pun.** `VAL-09` — *menyatakan
wadah layak sementara petugasnya orang yang sama dengan yang mengambil sampel* — adalah aturan
empat mata pada tingkat wadah. Ia semula tidak dikutip satu task pun, padahal `CAP-16` sudah
membuktikan sistem permission yang ada **tidak dapat** menegakkan aturan per orang atas satu
baris data. Sekarang ia dibebankan tegas ke `BE-LAB-12` dan disebut pada catatan risikonya.

### 8.3 Entity

| Entity | Task pemilik | Sifat |
|---|---|---|
| `LabOrder` | `BE-LAB-01` | Diperbarui — tambah kolom `Discipline` |
| `TrxLabSpecimen` | `BE-LAB-11`, `BE-LAB-12` | Diperbarui — enam kolom pindah ke pemeriksaan |
| `LabExamination` | `BE-LAB-09` | Baru |
| `LabValueBound`, `LabValueOption` | `BE-LAB-02` | Baru — penamaan ditetapkan `Lab` pada 2026-09-02 |
| `LabValueBoundChangeRequest`, `LabValueBoundHistory` | `BE-LAB-03` | Baru |
| `MstLabRejectionReason` | `BE-LAB-06` | Sudah ada — hanya bertambah jalur pengelolaan |
| `TrxLabTransitionHistory` | — | Sudah ada, dipakai apa adanya (`CAP-04`). Tidak ada pekerjaan struktur |

Sembilan entity, seluruhnya berpemilik.

### 8.4 Kewenangan

Dua puluh sembilan pasangan `resource : action` pada `contracts/permission-audit-matrix.md`
seluruhnya berpemilik. Yang **baru** dan karena itu perlu `[AccessPermission]` dipasang agar
`AccessMenuSeeder` mendaftarkannya sendiri (`CAP-14`):

| Resource | Action | Task |
|---|---|---|
| `LabExamination` | `Read`, `Create`, `Update` | `BE-LAB-16`, `BE-LAB-10` |
| `LabValueBound` | `Read`, `Create`, `Update` | `BE-LAB-04` |
| `LabCriticalBound` | `Read`, `Approve` | `BE-LAB-05` |
| `LabRejectionReason` | `Read`, `Create`, `Update`, `SystemFlag` | `BE-LAB-06` |
| `LabPatientRegistration` | `Read`, `Create` | `BE-LAB-08` |
| `LabCatalog` | `Read` | `BE-LAB-07` |
| `LabWorklist` | `Read` | `BE-LAB-14` |
| `LabMonitoring` | `Read` | `BE-LAB-15` |

Sisanya — `LabOrder :` dan `LabSpecimen :` — sudah terdaftar pada `c87d9c0` dan dipakai apa
adanya.

### 8.5 Integrasi

| ID | Arah | Keadaan | Task |
|---|---|---|---|
| `INT-01` | Laboratorium → Billing | Sudah ada, **satuannya berubah** | `BE-LAB-13` |
| `INT-02` | Laboratorium → Registrasi, pembacaan langsung | Sudah ada, tidak berubah | — |
| `INT-03` | Laboratorium → Data Induk, baca dan salin sesaat | Sudah ada, tidak berubah | — |
| `INT-04` | Laboratorium → Platform, pemeriksaan kewenangan | Sudah ada, tidak berubah | — |
| `INT-05` | Laboratorium → Registrasi, minta buat kunjungan | **Baru** | `BE-EXT-03`, `BE-LAB-08` |
| `INT-06` | Laboratorium → Data Induk, katalog dan harga | **Baru** | `BE-LAB-07` |

`INT-02` sampai `INT-04` sengaja tanpa task: ketiganya integrasi yang **sudah berjalan** dan
tidak disentuh Rilis 1. Dicatat di sini supaya ketiadaannya terbaca sebagai keputusan, bukan
kelalaian.

---

## 9. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-02 | Roadmap backend pertama. 15 task Laboratorium dan 3 task dependency eksternal disusun untuk empat gelombang. Diterbitkan setelah kelima kontrak dikunci dan penanda `STALE` pada capability map dicabut | `DRAFT` |
| 3 | 2026-09-02 | Audit diperluas ke empat dimensi lain: aturan validasi, entity, kewenangan, dan integrasi. Seluruhnya berpemilik, tetapi kutipannya jauh dari lengkap — 30 dari 50 aturan validasi tidak pernah disebut task mana pun. Yang paling berarti: `VAL-09`, aturan empat mata pada tingkat wadah, sempat tidak tersebut sama sekali dan kini dibebankan tegas ke `BE-LAB-12`. Bagian 8 diperluas menjadi lima sub-cakupan | `DRAFT` |
| 2 | 2026-09-02 | Audit cakupan endpoint dijalankan. Empat endpoint grup Lab Examination ternyata tanpa pemilik task; `BE-LAB-16` ditambahkan. Daftar endpoint pada `BE-LAB-06` dan `BE-LAB-15` ditulis eksplisit agar lubang sejenis tidak tersembunyi lagi. Bagian 8 Cakupan Endpoint ditambahkan | `DRAFT` |
| 13 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-12` berpindah menjadi **`SELESAI`**: ketiga endpoint wadah berperilaku baru, `VAL-05` sampai `VAL-15` masing-masing punya ujinya, dan `VAL-09` — aturan empat mata — ditulis sebagai kode di dalam service sesuai temuan `CAP-16`. `AC-36` terbukti: menolak wadah menggugurkan seluruh pemeriksaan yang ditopangnya. Kode status disesuaikan matriks: `422`, `409`, dan `403` menggantikan `400` yang seragam. **Urutan terhadap `BE-LAB-11` dibalik** karena keenam kolomnya masih dipakai service di delapan tempat; `BE-LAB-12` lebih dulu supaya kode berhenti memakainya, `BE-LAB-11` menyusul menghapus kolom mati. Penerbitan fakta per pemeriksaan tetap milik `BE-LAB-13` | `DRAFT` |
| 12 | 2026-09-03 | **`LEGACY MIGRATION` atas instruksi pemilik modul, ditulis `build-module-backend`.** `BE-LAB-19` ditambahkan dan langsung **`SELESAI`**: `TrxLabSpecimen` menjadi `LabSpecimen` dan `TrxLabTransitionHistory` menjadi `LabTransitionHistory`, lengkap dengan tabel fisiknya. Nol `Trx*` tersisa pada modul Laboratorium. **`LAB-OPEN-012` dijawab lebih dulu** — jumlah baris `TrxLabSpecimen` pada `QuilvianNewDevYoga` adalah **0** — sehingga tidak ada data yang berpindah; kartu `BE-LAB-11` diperbarui mengikutinya. Kartu `BE-LAB-16` juga diselaraskan: Verifikasi dan DoD-nya kini berbunyi `VAL-17` .. `VAL-20` sesuai bagian 8.2. Total task backend menjadi 22 | `DRAFT` |
| 11 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-16` berpindah dari `Siap direncanakan` menjadi **`SELESAI`**: empat endpoint pemeriksaan terpesan tersedia, dan `VAL-17` sampai `VAL-20` masing-masing punya ujinya. Tanpa migration. Satu selisih penulisan dicatat: kartu `BE-LAB-16` menyebut `VAL-05` dan `VAL-07` pada Verifikasi dan DoD-nya, padahal bagian 8.2 menempatkan keduanya pada `BE-LAB-12`; yang dikerjakan adalah `VAL-17` .. `VAL-20` sesuai bagian 8.2, dengan larangan jenis pemeriksaan ganda per wadah tetap ditegakkan. Penyelarasan kartu itu menjadi utang pemilik blueprint | `DRAFT` |
| 10 | 2026-09-03 | **Task baru atas instruksi pemilik modul, ditulis `build-module-backend`.** `BE-LAB-18` ditambahkan dan langsung **`SELESAI`**: `GET /lab-orders` memperoleh penyaring, pengurutan, dan pagination di sisi server. `LAB-API-v1` naik ke `r5`, dan ini **satu-satunya perubahan breaking** pada modul Laboratorium sejauh ini — bentuk respons berubah dari `List<T>` menjadi `PagedResult<T>`. Dampak konsumen dinilai: satu-satunya konsumen yang ditemukan, modul IGD, tidak putus karena pembungkusnya sudah menangani kedua bentuk, tetapi perlu mengirim `?encounterId=` agar tidak kehilangan pesanan di luar halaman pertama. Perubahan ini sekaligus menutup `IGD-DEC-105`. Dua deskripsi endpoint Swagger terakhir pada `LabSpecimenController` juga dihapus, sehingga seluruh grup Laboratorium kini bersih. Total task backend menjadi 21 | `DRAFT` |
| 9 | 2026-09-03 | **Task baru atas instruksi pemilik modul, ditulis `build-module-backend`.** `BE-LAB-17` ditambahkan dan langsung **`SELESAI`**: sepuluh endpoint `filters/metadata` dan `summary` pada kelima grup Laboratorium, menyamakan bentuknya dengan modul Rekam Medis. `LAB-API-v1` dinaikkan dari `r3` ke `r4`; amandemennya aditif sepenuhnya. Total task backend menjadi 20. Satu temuan dicatat: `GET /lab-orders` dan daftar wadah belum menerima parameter query apa pun, dan metadata keduanya menyatakan itu apa adanya alih-alih mengarang penyaring — menambahkan penyaringan sungguhan belum berpemilik task. Di luar task, empat deskripsi endpoint Swagger dihapus atas permintaan pemilik modul: tiga pada `LabOrderController` dan satu pada `MedicalRecordAccessLogController` | `DRAFT` |
| 8 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-09` berpindah dari `Siap direncanakan` menjadi **`SELESAI`**, sekaligus mencabut penahan `BE-LAB-16` dan `BE-LAB-10`. Entity `LabExamination` ada dengan nama yang benar, dan migration `20260903071535_AddLabExamination` terbukti jalan dua arah terhadap `QuilvianNewDevYoga`. Dua hal dicatat terbuka: keenam kolom yang harus pindah dari `TrxLabSpecimen` **belum dipindahkan** karena `BE-LAB-11` masih `BLOCKED`, sehingga salinan tarif untuk sementara ada di dua tempat; dan `erd/data-dictionary.md` bagian 4 bertentangan dengan bagian 8.3 roadmap ini soal `TrxLabTransitionHistory`. Di luar task, atas permintaan pemilik modul, komentar XML pada method action ketiga controller Laboratorium dihapus agar tidak lagi tampil sebagai deskripsi endpoint di Swagger | `DRAFT` |
| 7 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-06` berpindah dari `Siap direncanakan` menjadi **`SELESAI`**: lima endpoint grup Lab Rejection Reason tersedia, seeder data awal terdaftar, dan `VAL-36` sampai `VAL-38` terbukti lewat 34 uji. Task ini tidak menyentuh schema sehingga tanpa migration. Satu risiko organisasi dibuka: pemegang `LabRejectionReason : SystemFlag` belum ditetapkan, sehingga penanda biaya belum dapat disetel lewat aplikasi. Tujuh selisih terhadap `master-data-endpoint-standard.md` dan `LAB-API-v1` r3 dicatat terbuka pada laporan bagian 3.4 — yang terpenting: grup ini sengaja lima endpoint, bukan sembilan, karena kontraknya mengunci demikian dan `FE-LAB-03` tidak mengonsumsi sisanya | `DRAFT` |
