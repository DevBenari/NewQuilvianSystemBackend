# Roadmap Delivery Backend — Modul Laboratorium

| Field | Value |
|---|---|
| `blueprint_id` | `LAB-BP-001` |
| Roadmap revision | `64` — gelombang `MVP-9` ditambahkan 2026-09-25, bagian 6ak; `BE-LAB-68` diperluas. Sebelumnya `63` — gelombang `MVP-8` ditambahkan 2026-09-24, bagian 6aj. *Baris ini sempat tertinggal di `57` sementara riwayat sudah sampai `62`; dirapikan 2026-09-24* |
| Status | `DRAFT` |
| Bentuk blueprint | `SINGLE` |
| Ditulis oleh | `plan-module-delivery` |
| Tanggal | 2026-09-02; **gelombang `MVP-5a` ditambahkan 2026-09-14** |
| Manifest | `blueprint-manifest.md` revision `31` |
| Backend SHA | `e2152709` (revision 32). Revision 15-31 ditulis pada `466a7127` lalu `9067fa73`; kedelapan berkas yang menjadi dasar amandemen putaran 4 diverifikasi tidak berubah di antara ketiganya. Revision 1-14 ditulis pada `c87d9c0` |
| Frontend SHA | `9cd4cd03f` (revision 1-14 ditulis pada `688daff90`) |
| Contract version | `LAB-API-v1` **r12**, `LAB-STATE-v1` **r3**, `LAB-VAL-v1` **r6**, `LAB-INT-v1` r3, `LAB-PERM-v1` **rev 5** — seluruhnya `approved`. r3 dan sebelumnya dikunci 2026-09-02; amandemen `MVP-5a` disetujui pemilik modul 2026-09-14; amandemen `MVP-5b` — `r10`, `VAL-v1` `r5`, `PERM-v1` rev 5 — disetujui 2026-09-15; ruas `clinicalNote` **dicabut** lewat `r11`, disetujui pemilik modul 2026-09-15 | **Amandemen `MVP-5c` — `r12`, `STATE r3`, `VAL r6` — disetujui pemilik modul 2026-09-15.**
| Masukan | Decisions rev `32`; capability map rev `3`; `LAB-RCG-001` rev 5; `LAB-DA-001` rev 4 |
| Input hash | `sha256:6504b18a327b9966526bd1df8f3cb878d7f6d6519dacc1f7df16b1066729ae82` (decisions), dihitung 2026-09-02 |
| Slice tambahan | `EPIC-LAB-12` lewat gelombang `MVP-5c`, ditambahkan 2026-09-15 |
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
| `MVP-5b` | `BE-LAB-26` ✅, `BE-LAB-27` ✅, `BE-LAB-28` ✅, `BE-LAB-29` ✅, `BE-EXT-04` ✅, `BE-EXT-04b` ✅, `BE-EXT-05` ✅ | `EPIC-LAB-11` | **Ditambahkan 2026-09-15.** Pemesanan per disiplin dan pendaftaran lewat kiosk, di bawah `LAB-REQ-006`. Berdiri sesudah `MVP-5a` karena `BE-LAB-28` mengetatkan jalur wadah yang baru selesai dibangun `BE-LAB-21`. Lihat bagian 6c |
| `MVP-5c` | `BE-LAB-30` ✅, `BE-LAB-31` ✅, `BE-LAB-32` ✅ | `EPIC-LAB-12` | **Ditambahkan 2026-09-15.** Konfirmasi pesanan dan pembatalan beralasan, menurunkan `LAB-DEC-061` dan `LAB-DEC-063` dari rekonsiliasi bukti putaran 2. Berdiri sesudah `MVP-5b` karena `BE-LAB-32` mengetatkan pembatalan pesanan. Lihat bagian 6d |
| `MVP-5d` | `BE-LAB-33` ✅, `BE-LAB-34` ✅ | `EPIC-LAB-12` | **Ditambahkan 2026-09-16.** Ruas respons konfirmasi, menurunkan `LAB-API-v1` `r13`. Berdiri sesudah `MVP-5c` karena celahnya baru terlihat ketika `BE-LAB-31` rampung: nilainya sudah tersimpan tetapi tidak punya jalan keluar. Lihat bagian 6e |
| `MVP-5g` | `BE-LAB-37` ✅, `BE-LAB-38` ✅ | `EPIC-LAB-11` | **Ditambahkan 2026-09-17.** Melaksanakan `r16` dan `r17` yang disetujui pemilik modul hari itu. `BE-LAB-37` membuka nomor order agar terbaca dan menegakkan `VAL-59` penuh; `BE-LAB-38` mendirikan daftar penerimaan lintas pesanan yang menahan `FE-LAB-12`. Lihat bagian 6h |
| `MVP-5f` | `BE-LAB-36` ✅ | `EPIC-LAB-11` | **Ditambahkan 2026-09-17.** Nomor order yang dapat disebut manusia, menurunkan `LAB-DEC-072`. Berdiri sendiri: **nol dependency**, dan ketiga penahan modul nol menyentuhnya. Perancangannya ditulis lebih dulu di bagian 6g, sebagaimana diminta rekonsiliasi bukti putaran 3. Lihat bagian 6g |
| `MVP-5e` | `BE-LAB-35` ✅ | `EPIC-LAB-12` | **Ditambahkan 2026-09-16.** Daftar pemeriksaan terpesan dapat dibaca, menurunkan usul `LAB-API-v1` `r15`. Berdiri sesudah `MVP-5d` karena celahnya baru terlihat ketika `FE-LAB-17` hendak dimulai — pola yang **sama persis** dengan `MVP-5d`: nilainya sudah tersimpan sejak `BE-LAB-27` tetapi tidak punya jalan keluar. Lihat bagian 6f |
| `MVP-5a` | `BE-LAB-20` ✅, `BE-LAB-21` ✅, `BE-LAB-22` ◐, `BE-LAB-23` ⛔ dibatalkan, `BE-LAB-24` ✅, `BE-LAB-25` ✅ | `EPIC-LAB-11` | **Ditambahkan 2026-09-14.** Berdiri sebagai gelombang tersendiri agar penahan `LAB-REQ-005` tidak menular ke gelombang yang sudah siap jalan. Lihat bagian 6b |

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

> **Status: `SELESAI` — 2026-09-04.** Ketiga endpoint tersedia dan seluruhnya baca saja,
> `AC-43`, `AC-47`, `AC-48`, dan `AC-51` terbukti, dan `INV-22` ditegakkan pada jalur
> menambah pemeriksaan. Laporan lengkap:
> [`task/report/backend/BE-LAB-07.md`](../task/report/backend/BE-LAB-07.md).
>
> **`VAL-50` tidak butuh penjaga.** Tarif tidak dapat diubah lewat modul Laboratorium bukan
> karena ada kode yang menolaknya, melainkan karena jalurnya memang tidak pernah dibuat. Ada uji
> yang menjaga ketiadaan itu: nol `POST`, `PUT`, `DELETE`, dan `PATCH` pada grup ini.
>
> **`INV-22` ditegakkan hanya ketika kedua disiplin diketahui.** Pesanan peninggalan sebelum
> kolom disiplin ada, dan katalog yang belum digolongkan, keduanya bernilai kosong. Menolaknya
> akan mematikan pemesanan pada rumah sakit yang data induknya belum lengkap — padahal yang
> belum lengkap adalah data induknya, bukan permintaannya.

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
| **DoD** | Tiga endpoint tersedia dan seluruhnya baca saja **(terpenuhi)**; `AC-47` **(terpenuhi)** dan `AC-48` **(terpenuhi)**; `VAL-46` terbukti setelah `BE-EXT-01` selesai **(terpenuhi — `BE-EXT-01` selesai pada hari yang sama)** |

### `BE-EXT-01` — [Master Data] Kolom disiplin pada `MstProcedure`

> **Status: `SELESAI` — 2026-09-04**, dikerjakan atas instruksi pemilik modul yang juga
> kontributor `master-data`. Kolom `LabDiscipline` ada beserta index bersyaratnya, dan
> migration jalan dua arah pada dev pemilik. Laporan lengkap:
> [`task/report/backend/BE-EXT-01.md`](../task/report/backend/BE-EXT-01.md).
>
> **Pengisian nilainya belum dilakukan, dan itu disengaja.** Menggolongkan setiap pemeriksaan
> yang sudah ada ke Patologi Klinik, Patologi Anatomi, atau Mikrobiologi adalah keputusan
> klinis, bukan turunan teknis. Menebaknya akan menghasilkan katalog yang tampak lengkap
> tetapi salah golong, dan `INV-22` kemudian menolak pesanan yang sebenarnya sah.
>
> **Penahannya ternyata bukan hanya penggolongan klinis — jalur pengisiannya pun tidak ada.
> Dibangun 2026-09-08.** Kolom `LabDiscipline` memang bertambah pada model dan tabel, tetapi
> tidak pernah dibuka pada `ProcedureDtos.cs` maupun `ProcedureController.cs`. Telusur seluruh
> backend menemukan nol jalur tulis: bukan lewat API, bukan lewat layar, bukan lewat seeder.
> Artinya seandainya daftar penggolongan dari pihak klinis sudah ada sejak 2026-09-04, nilainya
> tetap **tidak dapat dimasukkan**. Butir DoD *"nilainya terisi"* karena itu tidak pernah dapat
> dipenuhi siapa pun, dan alasan yang tercatat selama ini hanya menyebut separuh sebabnya.
>
> Yang dibangun: `labDiscipline` diterima `POST` dan `PUT`, terbit pada respons daftar, detail,
> dan opsi beserta `labDisciplineName` siap baca, masuk ke `filters/metadata` sebagai
> `labDisciplineOptions` dan sebagai ruas form bernomor urut 11, serta muncul sebagai pilihan
> **Disiplin Laboratorium** pada layar Master Data → Prosedur. Daftar disiplin yang sah diambil
> dari `Enum.GetNames<LabDiscipline>()`, bukan konstanta teks tersendiri, supaya controller ini
> tidak dapat menerima golongan yang tidak dikenali Laboratorium. Dua aturan ditegakkan:
> golongan pada tindakan non-laboratorium **ditolak** alih-alih dikosongkan diam-diam, dan
> mengirimnya kosong mencabut golongan sehingga penanda `IsLaboratory` tidak pernah mati
> sambil meninggalkan golongan yatim. Delapan uji menjaganya
> (`ProcedureLabDisciplineTests`), enam uji lagi di sisi layar.
>
> **Yang masih menahan tinggal satu: daftar penggolongannya.** Itu tetap keputusan klinis, dan
> tetap tidak boleh ditebak.

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
| **DoD** | Kolom ada **(terpenuhi)**, nilainya terisi **(belum — jalur pengisiannya sudah ada sejak 2026-09-08; yang tersisa daftar penggolongan dari pihak klinis)**, penyaringan katalog per disiplin terbukti bekerja **(terpenuhi lewat uji)** |

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

> **Status: `SELESAI` — 2026-09-04.** `MstReferralInstitution` dan `MstReferralDoctor` ada,
> dokter tertaut ke instansinya dengan `Restrict`, kode instansi unik, dan migration jalan dua
> arah pada dev pemilik. Laporan lengkap:
> [`task/report/backend/BE-EXT-02.md`](../task/report/backend/BE-EXT-02.md).
>
> **Satu gerbang tata kelola ikut tercabut.** Checker QBE menolak kedua entity dengan
> `QBE-MOD-002`: baris registry `Master / Reference` tidak pernah cocok dengan folder
> `Areas/HealthServices/MasterData`, dan `Category`-nya bukan `BUSINESS DOMAIN`. Akibatnya
> **tidak ada** modul yang berwenang membuat satu pun data induk baru. Registry diperbaiki dan
> keputusannya dicatat bertanggal — lihat `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.
>
> **Butir Verifikasi ini sempat tidak terpenuhi selama tiga hari, dan baru ketahuan 2026-09-07.**
> Bunyinya *"kedua data induk dapat dipilih dari daftar"*, sementara task ini **tidak membuat
> satu pun endpoint** — laporannya menyatakan itu apa adanya. Selama endpointnya tidak ada,
> tidak ada layar mana pun yang dapat menawarkan daftarnya, sehingga satu-satunya cara mengisi
> perujuk adalah mengetiknya — persis yang dilarang `LAB-DEC-035` dan `AC-50`. Celah itu tidak
> terlihat sampai konsumen pertamanya dibangun.
>
> **Ditutup 2026-09-07 lewat `FE-LAB-05`.** Dua endpoint baca ditambahkan —
> `GET /master-data/referral-institutions/options` dan
> `GET /master-data/referral-doctors/options` — beserta service, DTO, dan delapan ujinya.
> Keduanya **baca saja**; penambahan dan penyuntingan data induk perujuk tetap pekerjaan modul
> Data Induk. Bukti: [`FE-LAB-05.md`](../task/report/frontend/FE-LAB-05.md) bagian 1.2 dan 3.3.
>
> **Yang masih tersisa dan bukan pekerjaan kode:** kedua tabel itu **masih kosong**. Selama
> belum diisi, formulir pendaftaran rujukan luar tidak dapat dipakai walaupun layar dan
> endpointnya sudah benar.

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
| **DoD** | Kedua tabel ada **(terpenuhi)**, dokter tertaut ke instansinya **(terpenuhi)**, keduanya dapat dibaca modul mana pun **(terpenuhi — `DbSet` pada `ApplicationDbContext`)** |

### `BE-EXT-03` — [Registrasi] Penunjuk perujuk pada kunjungan dan kontrak pemanggilan

> **Status: `SELESAI` untuk kolom dan kontrak — 2026-09-04.** `ReferralInstitutionId` dan
> `ReferralDoctorId` ada pada `TrxPatientEncounter`, keduanya boleh kosong dan bertaut
> `Restrict`, dan migration jalan dua arah pada dev pemilik. Bentuk teknis `INT-05` ditulis
> pada `contracts/integration-contract.md` bagian 2b. Laporan lengkap:
> [`task/report/backend/BE-EXT-03.md`](../task/report/backend/BE-EXT-03.md).
>
> **Endpoint pelaksananya menyusul 2026-09-07, lewat `BE-LAB-08`.** Jalur pemanggilan Registrasi
> beserta penyimpanan kunci idempotensinya kini ada sebagai `EncounterIntakeService` di
> `Areas/HealthServices/RegistrationManagement/Services/`, dengan kunci disimpan pada
> `TrxPatientEncounter.RegistrationIdempotencyKey` ber-unique index tersaring. Dengan itu butir
> DoD *"idempotensi terbukti lewat uji"* yang semula belum terpenuhi **kini terpenuhi** —
> buktinya ada pada [`BE-LAB-08.md`](../task/report/backend/BE-LAB-08.md), bukan pada laporan
> task ini. **Kartu ini dengan demikian `SELESAI` seluruhnya.**

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
| **DoD** | Dua kolom ada **(terpenuhi)**, kontrak `INT-05` disepakati tertulis **(terpenuhi)**, idempotensi terbukti lewat uji **(terpenuhi 2026-09-07 lewat `BE-LAB-08`; buktinya pada [`BE-LAB-08.md`](../task/report/backend/BE-LAB-08.md) bagian 5)** |

### `BE-LAB-08` — Endpoint pendaftaran pasien laboratorium

> **Status: `SELESAI` — 2026-09-07.** Seluruh butir DoD terpenuhi. Ketiga endpoint tersedia
> dengan route, verb, dan `[AccessPermission]` yang cocok satu per satu dengan `LAB-API-v1` r3.
> Laporan lengkap: [`task/report/backend/BE-LAB-08.md`](../task/report/backend/BE-LAB-08.md).
>
> **Penahannya dicabut pada sesi yang sama.** Pelaksana `INT-05` yang selama ini belum ada kini
> dibangun **di sisi Registrasi** sebagai `EncounterIntakeService`, atas instruksi eksplisit
> pemilik modul dan di bawah persetujuan `LAB-REQ-003` bagian 3b yang memang sudah mencakup
> idempotensi sebagai bukti selesai. Laboratorium tidak menulis satu baris pun ke tabel
> kunjungan.
>
> **`AC-45` dibuktikan dua kali, dengan cara yang berbeda.** Pertama lewat telusur seluruh
> source modul Laboratorium — nol pembentukan maupun pengubahan kunjungan dan data induk
> pasien. Kedua lewat perilaku: Laboratorium dan Registrasi diberi **dua penyimpanan terpisah**,
> lalu satu pendaftaran dijalankan; kunjungan hanya muncul pada penyimpanan milik Registrasi.
>
> **Idempotensi bersandar pada basis data, bukan pada kode.** Kolom
> `TrxPatientEncounter.RegistrationIdempotencyKey` ber-unique index tersaring; migration
> `20260907072413_AddRegistrationIdempotencyKeyToPatientEncounter` **terbukti jalan dua arah**
> terhadap `QuilvianNewDevYoga`: `Up`, lalu `Down`, lalu `Up` kembali.
>
> **Satu selisih dibuka.** `VAL-42` ditulis untuk pemanggilan jarak jauh, sementara `INT-05`
> dilaksanakan sejalur proses. Pemetaannya dijelaskan pada laporan bagian 3.3. Ditambah satu
> hal yang sengaja tidak diikutkan: kunjungan dari jalur ini **tidak** membawa snapshot kategori
> umur seperti jalur loket — lihat Risiko tersisa pada laporan.

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

> **Status: `SELESAI` — 2026-09-04.** Kedua endpoint tersedia, `VAL-03` dan `VAL-04` terbukti,
> dan setiap penandaan menghasilkan satu baris riwayat berlingkup `LabExamination`. Laporan
> lengkap: [`task/report/backend/BE-LAB-10.md`](../task/report/backend/BE-LAB-10.md).
>
> **Satu pertentangan dokumen diselesaikan.** Riwayat berlingkup `LabExamination` tidak dapat
> ditulis tanpa kolom penunjuknya. `erd/data-dictionary.md` bagian 4 dan bagian 6 roadmap ini
> sama-sama menuntut `LabTransitionHistory` bertambah `LabExaminationId`, sementara bagian 8.3
> menyatakan tabel itu tanpa pekerjaan struktur. Pertentangan itu sudah dicatat terbuka sejak
> `BE-LAB-09`. Yang dipakai adalah kamus data dan bagian 6; bagian 8.3 diperbaiki mengikutinya,
> dan kolomnya ditambahkan lewat migration aditif `AddLabExaminationIdToLabTransitionHistory`.
>
> **`AC-40` dijaga dua arah.** Selain membuktikan penanda duplo hanya mengenai baris yang
> ditandai, ada satu uji yang membuktikan grup `Lab Order` **tidak memiliki** endpoint
> kesegeraan sama sekali — endpoint yang dibatalkan `LAB-DEC-026` mudah dipasang kembali oleh
> siapa pun yang mengira ia hilang karena kelupaan.

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
| **DoD** | Dua endpoint tersedia **(terpenuhi)**, `VAL-03` dan `VAL-04` terbukti **(terpenuhi)**, `AC-40` terbukti **(terpenuhi)**, riwayat terbentuk pada setiap penandaan **(terpenuhi)** |

---

## 5. Task Gelombang `MVP-2`

### `BE-LAB-11` — Migration pemisahan wadah dan pemeriksaan

> **Status: `SELESAI` — 2026-09-04.** Keenam kolom lepas dari `LabSpecimen` dan utuh pada
> `LabExamination`. Migration `SplitLabSpecimenIntoExamination` **dijalankan dua arah** terhadap
> `QuilvianNewDevYoga` atas instruksi pemilik modul — maju, mundur, lalu maju lagi — dan database
> ditinggalkan pada keadaan target. Eksekusi ke lingkungan di luar dev pemilik tetap wewenang
> terpisah. Laporan lengkap:
> [`task/report/backend/BE-LAB-11.md`](../task/report/backend/BE-LAB-11.md).
>
> **Kode ternyata masih membacanya di lima tempat.** Laporan `BE-LAB-13` menyatakan tidak ada
> lagi yang membaca keenam kolom itu. Itu benar untuk **muatan fakta**, tetapi tidak untuk
> selebihnya: `CreateSpecimenAsync` masih menuliskannya, pengambilan ulang masih membaca
> `ProcedureId` wadah, dan `GetByOrderAsync` beserta `MapResponse` masih memproyeksikannya ke
> jawaban API. Kelimanya dilepas pada task ini.
>
> **Prasyarat mutlaknya ditulis sebagai kode, dan penjaganya sekaligus menjadi alat ukur.**
> Migration diawali penjaga yang menolak berjalan bila tabel `LabSpecimen` masih memuat baris.
> Penjaga itu lolos **dua kali** pada dev pemilik, sehingga jawaban `0` atas `LAB-OPEN-012`
> terverifikasi ulang oleh mesin, bukan oleh ingatan. Pada basis data yang masih berisi ia
> berhenti **sebelum** satu kolom pun dihapus.
>
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
| **Cakupan** | Migration menghapus `ProcedureId`, `ProcedureCodeSnapshot`, `ProcedureNameSnapshot`, `TariffId`, `TariffCodeSnapshot`, dan `UnitPriceSnapshot` dari `LabSpecimen` — nama tabelnya sejak `BE-LAB-19` — setelah memindahkan isinya ke `LabExamination`. Termasuk melepas relasi ke `MstProcedure` beserta index-nya, dan melepas empat ruas dari `LabSpecimenResponse` |
| **Dependency** | `BE-LAB-09`. **`LAB-OPEN-012` wajib dijawab lebih dulu** |
| **Acceptance criteria** | `AC-35`, `AC-38` |
| **Verifikasi** | Perhitungan baris sebelum dan sesudah wajib cocok; tidak ada fakta kelayakan tagih yang kehilangan sumbernya |
| **Risiko/pemilik** | **Sedang.** Ini satu-satunya perubahan struktural yang menghapus kolom berisi data, dan jumlah baris di produksi masih belum diketahui. Sejak 2026-09-04 risiko itu tidak lagi bergantung pada ketelitian pelaksana: migration menolak berjalan pada tabel yang masih berisi. Pemilik eksekusi: pemilik repository backend atau DBA |
| **DoD** | Jumlah baris produksi diketahui **(belum — ditegakkan penjaga migration sebagai gantinya)**, rencana pemindahan disusun sesuai angka itu **(terpenuhi)**, migration jalan dua arah **(terbukti pada tingkat skrip; eksekusi belum)**, tidak ada tautan tagihan yang putus **(terpenuhi)** |

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

> **Status: `SELESAI` — 2026-09-03, ditutup 2026-09-04.** Fakta kini terbit satu per
> pemeriksaan, dan idempotensinya terbukti. Keempat butir DoD terpenuhi. Dua di antaranya
> sempat dilaporkan terhalang `QUILVIAN_BILLING_TEST_DB`; **laporan itu keliru** —
> `LaboratoryAuthorityTests` tidak memakai database sama sekali, dan yang menghalanginya adalah
> project ujinya yang tidak dapat dikompilasi sesudah tipe kembalian berubah. Sesudah sebelas
> baris diperbaiki, uji itu lulus 18 dari 18. Laporan lengkap:
> [`task/report/backend/BE-LAB-13.md`](../task/report/backend/BE-LAB-13.md).
>
> **Dependency melingkar diselesaikan.** Kartu ini menyebut dependency `BE-LAB-11`, sementara
> `BE-LAB-11` tidak dapat menghapus keenam kolom selama muatan fakta masih membacanya. Keduanya
> saling menunggu. Urutan yang dipakai: **pembaca dulu, schema terakhir** — `BE-LAB-13`
> memindahkan sumber muatan ke `LabExamination`, lalu `BE-LAB-11` menghapus kolom yang sudah
> tidak dibaca siapa pun. Penyelarasan kedua kartu menjadi utang pemilik blueprint.
>
> **Yang berubah hanya satuannya.** `SourceItemId` menunjuk `LabExamination.Id`, bukan
> `LabSpecimen.Id`. Producer, enum, dan jalur dispatch tidak disentuh sama sekali — sesuai
> `CAP-11` yang menyatakan seluruhnya `Ready to reuse`.

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

> **Status: `SELESAI` — 2026-09-04.** Kedua endpoint tersedia, urutan cito terbukti, dan
> keterlambatan terbukti pada kedua jalur. **Tidak ada tabel daftar kerja yang dibuat**, dan ada
> uji yang menjaganya tetap begitu. Laporan lengkap:
> [`task/report/backend/BE-LAB-14.md`](../task/report/backend/BE-LAB-14.md).
>
> **Satu turunan dicatat.** `LabValueBound` dipecah menurut jenis kelamin dan kelompok umur untuk
> keperluan batas nilai, sementara batas waktu cito adalah janji layanan yang tidak bergantung
> pada keduanya. Blueprint tidak menyebut baris mana yang berlaku. Yang dipakai adalah baris umum
> — `All` tanpa kelompok umur — dan bila baris itu tidak mengisinya, nilai terkecil di antara
> baris aktif lainnya. Menegaskannya adalah utang pemilik blueprint.
>
> **`AC-17` menutup sisa `FR-01.4`.** Kolom `CitoTurnaroundMinutes` sudah ada sejak `BE-LAB-02`,
> tetapi belum pernah dipakai menghitung apa pun. Sejak task ini ia benar-benar menentukan.

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
| **DoD** | Dua endpoint tersedia **(terpenuhi)**, urutan cito terbukti **(terpenuhi)**, perhitungan keterlambatan terbukti pada kedua jalur **(terpenuhi)**, tidak ada tabel daftar kerja yang dibuat **(terpenuhi, beserta ujinya)** |

### `BE-LAB-15` — Monitoring tiga disiplin

> **Status: `SELESAI` — 2026-09-04.** Ketiga endpoint tersedia beserta
> `GET /lab-orders/by-discipline/{discipline}`, dan `AC-41` serta `AC-42` terbukti. Laporan
> lengkap: [`task/report/backend/BE-LAB-15.md`](../task/report/backend/BE-LAB-15.md).
>
> **Tiga jalur, satu perilaku.** Yang tiga adalah jalurnya di controller; penyaring, proyeksi,
> dan pengurutannya ditulis satu kali dan dipakai bertiga. Dengan begitu "penyaring identik"
> pada DoD benar-benar identik, bukan tiga salinan yang lambat laun menyimpang.
>
> **Disiplin bukan penyaring.** `LabMonitoringQuery` sengaja **tidak** memiliki ruas disiplin —
> ia ditentukan jalur yang dipanggil. Ada uji yang menjaganya, karena begitu disiplin menjadi
> ruas biasa, tiga menu terpisah kehilangan alasan keberadaannya.
>
> **Satu ruas kontrak tidak dapat dipenuhi.** Penyaring "nomor pesanan" menuntut kolom yang
> tidak ada: `LabOrder` tidak memiliki nomor pesanan sama sekali. Yang tersedia dan dipakai
> adalah nomor kunjungan. Menambah nomor pesanan adalah perubahan kontrak dan schema tersendiri.

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
| **DoD** | Tiga endpoint tersedia dengan penyaring identik **(terpenuhi)**, `AC-41` **(terpenuhi)** dan `AC-42` **(terpenuhi, sekaligus `AC-19`)** |

---

## 6b. Task Gelombang `MVP-5a` — Penerimaan Sampling/Specimen

Ditambahkan 2026-09-14. Menurunkan `EPIC-LAB-11` `FR-11.1` sampai `FR-11.8` dari
`04-prd-to-mvp.md` revision 4 bagian 16.

> **`FR-11.9` dan `FR-11.10` tidak punya task di sini.** Keduanya berstatus `OPEN DECISION`,
> tertahan `LAB-COORD-006` dan `LAB-COORD-007` yang diajukan sebagai `LAB-REQ-005`. Kontrak PRD
> melarang epic berstatus itu masuk gelombang pengiriman mana pun, sehingga keduanya **tidak
> diberi ID task** — bukan diberi ID lalu ditandai `BLOCKED`. ID task yang sudah ada cenderung
> ikut masuk rencana kapasitas walaupun bertanda tertahan.

**Gerbang prefix sudah terbuka.** `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 21 mencatat
`LaboratoryManagement / Laboratory` berprefix `Lab` dan berlifecycle `ACTIVE` sejak 2026-09-02.
`QBE-MOD-002` **tidak** menahan pembuatan entity `Lab*` maupun migration modul ini.

### Grafik urutan dependency — `MVP-5a`

```text
{DATA-MST-MEASUREMENT} ─┐
                        ├──> BE-LAB-21 ─┬──> BE-LAB-22
BE-LAB-20 ✅ ───────────┘               │
                                        └──> BE-LAB-25

BE-LAB-23 ⛔ (dibatalkan)

BE-LAB-24 ✅
```

Legenda:

- `{DATA-MST-MEASUREMENT}` — gerbang **data**, bukan gerbang kode: lima baris `MstMeasurement`
  ber-`IsForLaboratory` yang diisi Master Data. Ia menahan **verifikasi** volume pada
  `BE-LAB-21`, bukan penulisan kodenya. Tidak diberi nomor gelombang.
- `BE-LAB-23` ⛔ **dibatalkan** `LAB-DEC-050`; pekerjaannya sudah tidak ada. `BE-LAB-24` berdiri sendiri dan **terbuka kembali** setelah `LAB-DEC-049` mempersempit cakupannya.

| Gelombang eksekusi | Task | Kenapa di sini |
|---:|---|---|
| 1 | `BE-LAB-20` ✅, `BE-LAB-23` ⛔ dibatalkan, `BE-LAB-24` ✅ | Ketiganya tanpa prasyarat. `BE-LAB-23` **dibatalkan** `LAB-DEC-050`; `BE-LAB-24` terbuka kembali setelah `LAB-DEC-049` dan tinggal menulis penjaga jalur tambah |
| 2 | `BE-LAB-21` ✅ | Memerlukan tabel jenis specimen dari `BE-LAB-20`. **Selesai 2026-09-15**; migration diterapkan ke `QuilvianNewDevYoga`, `T-M2` dan `T-M4` terbukti |
| 3 | `BE-LAB-22` ◐, `BE-LAB-25` ✅ | `BE-LAB-22` menyerialkan migration pada tabel yang sama — **selesai sebagian 2026-09-15**, `VAL-59` tertahan kontrak. `BE-LAB-25` memerlukan kolom keterangan beserta datanya, dan kolomnya sudah ada pada source sejak `BE-LAB-21` |

**Tidak ada siklus.** Setiap task muncul tepat satu kali, dan jumlah pasangan prasyarat→task
pada grafik sama dengan isi kolom **Dependency** pada keenam task di bawah.

> **Batas yang dilaporkan apa adanya.** Grafik di atas mencakup **`MVP-5a` saja**. Gelombang
> `MVP-0` sampai `MVP-3` ditulis sebelum grafik urutan dependency menjadi kewajiban, dan
> roadmap ini tidak memilikinya untuk kesembilan belas task lamanya. Menurunkan grafik itu
> sekarang berarti menyimpulkan ulang dependency sembilan belas task dari ingatan dokumen —
> pekerjaan tersendiri yang berisiko keliru, dan berada di luar cakupan amandemen ini.
> Dicatat sebagai gap, bukan dikerjakan diam-diam.

### `BE-LAB-20` ✅ — Data induk jenis specimen

> **Status: ✅ `SELESAI` — 2026-09-14.** Source, kedua migration, dan eksekusi database selesai; `dotnet build
> -p:RunAnalyzers=False` **0 Error, 0 Warning**, tanpa satu pun warning dari berkas baru task
> ini. Delapan endpoint berdiri — tujuh dari `r7` dikurangi `GET /other-usage` yang ditunda ke
> `BE-LAB-25` karena membaca kolom milik `BE-LAB-21`, ditambah `GET /filters/metadata` dan
> `GET /summary` sebagai permukaan baseline master data.
>
> **Eksekusi database selesai 2026-09-14** atas wewenang pemilik modul yang menyebut targetnya
> secara tegas: `QuilvianNewDevYoga` di `160.22.250.77`. Terbukti terhadap database: **7 baris
> baseline** terisi, kedua index unik parsial terbaca dari `pg_indexes` beserta filternya,
> **`T-M3`** dan **`VAL-61`** ditolak `23505` pada constraint yang benar, dan **jalur `Down`
> lalu `Up` lagi** dibuktikan dengan verifikasi ulang yang tetap lulus. Nol baris uji
> tertinggal.
>
> **Dua butir tetap belum dibuktikan runtime, dan dicatat apa adanya:** `T-M4` menunggu foreign
> key yang baru dibuat `BE-LAB-21`, dan `VAL-63` adalah aturan tingkat service yang terverifikasi
> lewat source. `AC-58` terpenuhi sebagian — penolakan teks bebas saat mencatat wadah adalah
> cakupan `BE-LAB-21`; `AC-60` cakupan `BE-LAB-25`.
>
> Kontrak sudah dinaikkan ke **`r8`** mencatat dua endpoint baseline yang ditambahkan.
>
> Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-20.md`](../task/report/backend/BE-LAB-20.md).

| Butir | Isi |
|---|---|
| **Status** | ✅ `SELESAI` — 2026-09-14. Migration diterapkan ke `QuilvianNewDevYoga`; `T-M3`, `VAL-61`, dan jalur `Down` terbukti |
| **Outcome** | Kepala instalasi dapat melihat dan mengelola daftar jenis specimen; petugas penerimaan dapat memilih dari daftar itu |
| **Requirement/decision** | `FR-11.1`, `LAB-DEC-040`, BR-35 |
| **Kontrak** | `LAB-API-v1` `r7` grup Lab Specimen Type; `LAB-VAL-v1` `r4` `VAL-61`..`VAL-63`; `LAB-PERM-v1` rev 4 resource `LabSpecimenType` |
| **Reuse** | `Missing`. Pola mengikuti `MstLabRejectionReason` yang sudah ada — tabel data induk beserta enam endkoint kelola dan pilihan |
| **Cakupan** | Entity `LabSpecimenType` di `Areas/HealthServices/LaboratoryManagement/Models/`, configuration di `Repositories/Configurations/HealthServices/LaboratoryManagement/`, DbSet, migration tabel, migration seed tujuh baris, service, controller, DTO |
| **Dependency** | — |
| **Acceptance criteria** | `AC-58`, `AC-60` |
| **Verifikasi** | QBE preflight dan conformance; review diff/scope; `dotnet restore` dan `dotnet build`; verifikasi kontrak API terhadap `r7`; verifikasi proses bisnis: tujuh baris terisi, `Lainnya` ada dan aktif, baris `Lainnya` kedua ditolak `VAL-62`, menonaktifkan satu-satunya `Lainnya` ditolak `VAL-63` |
| **Risiko/pemilik** | Rendah. Tabel baru tanpa yang menunjuk padanya. **Prefix `Lab`, bukan `Mst`** — memakai `Mst` melanggar baris riwayat registry 2026-09-02 dan akan dilaporkan checker. Pemilik: Laboratorium |
| **DoD** | Tabel berdiri beserta unique parsial `Lainnya`; tujuh baris terisi; ketujuh endpoint menjawab sesuai `r7`; `VAL-61`..`VAL-63` menolak dengan pesan yang tertulis di matriks; migration jalan maju dan mundur; tidak ada endpoint lama yang berubah perilakunya |

**Kenapa unique parsial `Lainnya` ditegakkan di basis data, bukan hanya di service.** Aturan
yang hanya dijaga service akan bocor lewat seeder, skrip perbaikan data, atau migration
berikutnya — dan bocornya diam-diam. `T-M3` menguji penjagaan itu langsung di basis data.

### `BE-LAB-21` — Jenis dan volume pada wadah

> **Status: ✅ `SELESAI` — 2026-09-15.** Source, configuration, migration, **dan eksekusi ke
> `QuilvianNewDevYoga`** selesai dan terverifikasi; jalur `Down` ikut dibuktikan.
> `dotnet build -p:RunAnalyzers=False --no-incremental` **0 Error**, dan nol dari 191 warning
> repository berasal dari kelima berkas task ini.
>
> **Terbukti terhadap database:** keempat kolom ada — `VolumeAmount` sebagai `numeric(12,3)` —
> kedua foreign key ber-`delete_rule = RESTRICT`, kedua index terbaca dari `pg_indexes`,
> **`T-M2`** lulus dengan 5 baris lama utuh beserta 4 keterangan lamanya, dan **`T-M4`** ditolak
> `23503` pada constraint yang benar. Inilah foreign key yang ditunggu `BE-LAB-20`. **Jalur
> `Down` lalu `Up`** dibuktikan dengan verifikasi ulang yang tetap lulus seluruhnya. Nol baris
> uji tertinggal — percobaan `T-M4` dijalankan di dalam transaksi yang selalu di-`ROLLBACK`.
>
> **`T-63c` terbukti.** Seluruh rujukan `VolumeAmount` pada source ditelusuri: tidak ada satu pun
> `<`, `>`, `<=`, maupun `>=` yang mengenainya. `RULE-021` tegak.
>
> **`DATA-MST-MEASUREMENT` terbantah sebagian oleh data sebenarnya.** Dugaan bahwa
> `MstMeasurement` kosong **keliru**: 17 satuan ber-`IsForLaboratory` yang aktif sudah ada, dan
> **`mL` serta `gram` termasuk di dalamnya**. Volume bersatuan mililiter dan gram karena itu
> **dapat dipakai sekarang juga**. Yang masih tertahan hanya **tiga** satuan — `µL`, `blok`, dan
> `slide` — sehingga `AC-64` terpenuhi sebagian, bukan nol. Permintaan ke `master-data` perlu
> **ditulis ulang**: tiga baris, bukan lima, dan tanpa mendikte kode karena `MstMeasurement`
> memakai seri tergenerasi `STN`.
>
> **Konsekuensi penerapan yang sudah diterima pemilik modul.** `specimenTypeId` kini **wajib**
> sesuai `r7`, dan layar wadah yang sudah berjalan belum mengirimnya — sehingga
> `lab-orders/{slug}/specimens` pada `QuilvianNewDevYoga` **kini menjawab `422`** sampai
> `FE-LAB-11` selesai. Backend tidak dilonggarkan untuk menutupinya; melonggarkannya akan
> membatalkan `AC-58`.
>
> Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-21.md`](../task/report/backend/BE-LAB-21.md).

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-15. Migration diterapkan ke `QuilvianNewDevYoga`; `T-M2`, `T-M4`, dan jalur `Down` terbukti. `mL` dan `gram` tersedia; `µL`/`blok`/`slide` menunggu Master Data |
| **Outcome** | Petugas mencatat wadah beserta jenis specimennya dan volumenya, dan volume itu selalu membawa satuannya |
| **Requirement/decision** | `FR-11.1`, `FR-11.3`, `FR-11.4`, `LAB-DEC-040`, `LAB-DEC-041`, BR-35, BR-36 |
| **Kontrak** | `LAB-API-v1` `r7` perluasan `POST /lab-specimens/by-order/{labOrderId}`; `LAB-VAL-v1` `r4` `VAL-51`..`VAL-57` |
| **Reuse** | `LabSpecimen` `Extend`. **`MstMeasurement` `Ready to reuse`** — penanda `IsForLaboratory` sudah ada; Laboratorium tidak membuat daftar satuan sendiri |
| **Cakupan** | Empat kolom nullable pada `LabSpecimen` (`SpecimenTypeId`, `SpecimenTypeOtherNote`, `VolumeAmount`, `VolumeUnitId`), dua FK `Restrict`, dua index, satu migration, penyesuaian `PlanLabSpecimenRequest` dan `LabSpecimenResponse` |
| **Dependency** | `BE-LAB-20`. **Gerbang data:** lima baris `MstMeasurement` ber-`IsForLaboratory` |
| **Acceptance criteria** | `AC-58`, `AC-59`, `AC-61`, `AC-62`, `AC-63`, `AC-64` |
| **Verifikasi** | QBE preflight; review diff/scope; build; verifikasi kontrak terhadap `r7`; verifikasi proses bisnis: wadah `Blood` `3` `mL` tersimpan; `Lainnya` tanpa keterangan ditolak `VAL-53`; **`Lainnya` berketerangan tersimpan tanpa penolakan**; volume tanpa satuan ditolak `VAL-56`; satuan bukan laboratorium ditolak `VAL-57`; baris `LabSpecimen` lama tetap terbaca dengan kolom baru kosong |
| **Risiko/pemilik** | Sedang. Mengubah tabel yang sudah berisi data — seluruh kolom nullable sehingga migration tidak menulis ulang satu baris pun. Pemilik: Laboratorium |
| **DoD** | Keempat kolom ada; kedua FK `Restrict`; `VAL-51`..`VAL-57` menolak sesuai matriks; **tidak ada satu pun jalur kode yang membandingkan volume terhadap batas minimum**; data lama utuh; migration jalan maju dan mundur |

> **Batas yang perlu ditulis terang: task ini `SELESAI` secara kode sebelum verifikasi
> volumenya dapat dijalankan.** Lima baris satuan `MstMeasurement` ber-`IsForLaboratory`
> (`mL`, `µL`, `gram`, `blok`, `slide`) adalah pekerjaan **Master Data**, bukan Laboratorium —
> mengisinya dari seeder Laboratorium akan mengulang `LAB-DEBT-001`.
>
> Selama kelima baris itu kosong, kolom volume **tidak dapat dipakai** walaupun kodenya benar.
> Ini pola yang sudah pernah terjadi di modul ini: `FE-LAB-05` selesai 2026-09-07 dan delapan
> skenario verifikasi manualnya masih menunggu sampai hari ini karena daftar perujuk kosong.
> Statusnya **menunggu data, bukan menunggu kode**, dan wajib ditulis begitu pada laporannya.

**Butir DoD yang bentuknya ketiadaan aturan.** `RULE-021` menyatakan tidak ada batas minimum
maupun maksimum volume, dan `LAB-DEC-041` menerimanya apa adanya. Butir "tidak ada satu pun
jalur kode yang membandingkan volume terhadap batas minimum" ada justru karena keputusan
semacam ini paling mudah dilanggar tanpa sengaja — seorang implementer yang bermaksud baik
menambahkan peringatan "volume terlalu sedikit", dan aturannya hilang tanpa seorang pun
memutuskannya.

### `BE-LAB-22` — Waktu penerimaan fisik

> **Status: `SELESAI SEBAGIAN` — 2026-09-15, DITUTUP PENUH 2026-09-17 oleh `BE-LAB-37`.** Uraian di bawah adalah keadaan saat ia ditulis dan sengaja dibiarkan sebagai jejak. Kolom `PhysicallyReceivedAt` beserta indexnya
> berdiri, migration dibuat, dan `dotnet build -p:RunAnalyzers=False --no-incremental`
> **0 Error** tanpa satu pun warning dari berkas task ini.
>
> **Tiga AC terpenuhi, satu terpenuhi separuh.** `AC-65` terbukti **secara struktural**: nol ruas
> permintaan bernama `ReceivedAt` pada seluruh DTO Laboratorium, dan satu-satunya jalur tulisnya
> diisi server. `AC-67` terpenuhi — rekap penerimaan beralih ke waktu nyata dengan cadangan
> `CreateDateTime`, dan selisih kedua waktu tercatat pada jejak audit. **`AC-17` terbukti dua
> kali**: nol rujukan `PhysicallyReceivedAt` pada `LabWorklistService`, dan nol diff pada ketiga
> berkas perhitungan cito.
>
> **`VAL-59` tidak dapat ditegakkan pada kontrak `r7` sebagaimana disetujui, dan itu temuan
> bukan kelalaian.** Aturannya membandingkan waktu penerimaan fisik terhadap **waktu
> pengambilan** — tetapi satu-satunya permintaan yang membawa waktu penerimaan fisik adalah
> `PlanLabSpecimenRequest`, dan pada saat wadah direncanakan `CollectedAt` masih kosong karena
> diisi server nanti. Menegakkannya pada tindakan pengambilan **justru menolak skenario yang
> menjadi alasan `LAB-DEC-042` dibuat**: wadah tiba Senin 21.10, diregistrasi Selasa 08.05.
> `AC-66` karena itu terpenuhi separuh — `VAL-58` tegak penuh. Tiga pilihan disiapkan untuk
> pemilik modul pada laporan bagian 6; **tidak satu pun dipilih sendiri**.
>
> Migration **belum diterapkan**. Dua migration kini menunggu pada tabel yang sama dan dapat
> diterapkan dalam satu jendela wewenang.
>
> Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-22.md`](../task/report/backend/BE-LAB-22.md).

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — **ditutup PENUH 2026-09-17** oleh `BE-LAB-37` yang melaksanakan `r16`; `VAL-59` kini menyala dan `AC-66` terpenuhi penuh. *(status diperbaiki 2026-09-22; blok ini tertinggal `SELESAI SEBAGIAN` sejak 2026-09-15, padahal penutupannya sudah tercatat pada tabel gelombang)* |
| **Outcome** | Wadah yang datang setelah jam operasional tercatat pada hari kedatangannya, sementara jejak kapan datanya masuk sistem tetap utuh |
| **Requirement/decision** | `FR-11.5`, `LAB-DEC-042`, BR-37 |
| **Kontrak** | `LAB-API-v1` `r7`; `LAB-VAL-v1` `r4` `VAL-58`, `VAL-59` |
| **Reuse** | `LabSpecimen` `Extend`. `ReceivedAt` yang sudah ada **tidak disentuh** |
| **Cakupan** | Satu kolom `PhysicallyReceivedAt` nullable beserta index, satu migration, penyesuaian DTO, dan jejak audit yang mencatat kedua waktu berdampingan |
| **Dependency** | `BE-LAB-21` — menyerialkan migration pada tabel yang sama |
| **Acceptance criteria** | `AC-65`, `AC-66`, `AC-67`, dan `AC-17` sebagai regresi |
| **Verifikasi** | QBE preflight; build; verifikasi proses bisnis: wadah diterima Senin 21.10 dan dicatat Selasa 08.05 muncul pada laporan penerimaan **hari Senin**; waktu di masa depan ditolak `VAL-58`; waktu mendahului pengambilan ditolak `VAL-59`; mengirim `ReceivedAt` dari luar **diabaikan**; perhitungan keterlambatan cito **tidak berubah** |
| **Risiko/pemilik** | Rendah untuk kodenya, **sedang untuk maknanya**. Bila laporan keliru memakai `ReceivedAt`, seluruh gunanya hilang tanpa satu pun kesalahan yang terlihat. Pemilik: Laboratorium |
| **DoD** | Kolom ada; `ReceivedAt` tidak dapat diubah endpoint mana pun; tidak ada ruas permintaan bernama `ReceivedAt` pada DTO mana pun; laporan penerimaan memakai waktu nyata; selisih kedua waktu tercatat pada jejak audit; `AC-17` terbukti tidak berubah |

### `BE-LAB-23` ⛔ — Jumlah pemeriksaan memperbanyak baris

> **Status: `TERBLOKIR` — 2026-09-14.** Nol baris source diubah. `LAB-DEC-038` **tidak dapat
> dilaksanakan** seperti tertulis.
>
> `LabExamination` punya **index unik di tingkat database** atas `(SpecimenId, ProcedureId)`,
> dipasang atas dasar `BR-20` dan `AC-35` pada 2026-09-01. `Quantity` 3 untuk satu jenis
> pemeriksaan pada satu wadah karena itu mustahil — baris kedua dan ketiga ditolak service, dan
> bila lolos, ditolak database.
>
> Contoh pada `BR-33` sendiri keliru: Glukosa Puasa dan Glukosa 2 Jam PP adalah **dua
> `MstProcedure` berbeda**, sehingga petugas memilih dua butir katalog dan Qty tidak diperlukan.
>
> **Ditutup hari yang sama.** Pemilik modul memilih mencabut `LAB-DEC-038` lewat `LAB-DEC-050`:
> **kolom Jumlah tidak dibuat sama sekali.** Petugas memilih dua butir katalog berbeda bila
> memang perlu dua pemeriksaan. `BR-20`, `AC-35`, dan index uniknya tetap utuh.
>
> **Task ini karena itu dibatalkan, bukan ditunda** — pekerjaannya sudah tidak ada. `AC-52`
> sampai `AC-54` dicabut, `VAL-60` dicabut, dan ruas `Quantity` dicabut dari kontrak lewat
> `LAB-API-v1` `r9` sebelum sempat dibangun. `POST /lab-examinations` tidak berubah sama sekali
> dari `r6`.
>
> Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-23.md`](../task/report/backend/BE-LAB-23.md).

| Butir | Isi |
|---|---|
| **Status** | ⛔ `DIBATALKAN` — 2026-09-14 oleh `LAB-DEC-050`. Nol baris source diubah, dan tidak akan ada |
| **Outcome** | Petugas mengisi jumlah satu kali, dan sistem membuat sebanyak itu baris pemeriksaan yang masing-masing dapat diisi hasil berbeda |
| **Requirement/decision** | `FR-11.6`, `LAB-DEC-038`, BR-33 |
| **Kontrak** | `LAB-API-v1` `r7` perluasan `POST /lab-examinations`; `LAB-VAL-v1` `r4` `VAL-60` |
| **Reuse** | `LabExamination` `Ready to reuse`. **Tabelnya tidak berubah sama sekali** |
| **Cakupan** | Satu ruas `Quantity` bernilai bawaan `1` pada `CreateLabExaminationRequest`, dan perubahan `LabExaminationService` agar memperbanyak baris. **Nol migration** |
| **Dependency** | — |
| **Acceptance criteria** | `AC-52`, `AC-53`, `AC-54` |
| **Verifikasi** | Build; verifikasi kontrak terhadap `r7`; verifikasi proses bisnis: Glukosa ber-jumlah 2 menghasilkan **dua baris** yang dapat diisi `96` dan `143` tanpa saling menimpa; wadah layak yang menopang tiga baris menerbitkan **tiga** fakta kelayakan tagih; permintaan **tanpa** ruas `Quantity` berperilaku persis seperti sebelum `r7` |
| **Risiko/pemilik** | Rendah pada kode, **perlu ketelitian pada uang**. Satu kesalahan di sini menagihkan tiga kali lipat atau sepertiga. Pemilik: Laboratorium |
| **DoD** | Tidak ada satu pun properti bernama `Qty` atau `Quantity` pada model Laboratorium mana pun; jumlah `3` menghasilkan tiga baris bertarif masing-masing; `VAL-60` menolak jumlah nol atau kurang; **konsumen lama tanpa ruas `Quantity` tidak berubah perilakunya**; `IsDuplo` tetap ada dan tidak digantikan |

**Kenapa "konsumen lama tidak berubah perilakunya" masuk DoD.** Perluasan yang diklaim aditif
dapat diam-diam mengubah perilaku pemanggil lama. Tanpa butir ini, klaim aditif pada `r7` tidak
pernah benar-benar diperiksa.

### `BE-LAB-24` ✅ — Penguncian daftar pemeriksaan pada jalur hapus

> **Status: ✅ `SELESAI` — 2026-09-14.** Nol baris source diubah, **dan memang tidak perlu**.
> Riwayat lengkapnya ditulis apa adanya di bawah, termasuk tahap ketika task ini sempat terblokir.
>
> Pemeriksaan menemukan tiga hal. Jalur tambah **sudah terjaga** `VAL-18` sesuai dugaan. Jalur
> hapus **bukan `DELETE`** melainkan `POST /{id}/cancel`. Dan ketiadaan penjagaan di sana
> **disengaja**: `VAL-18` pada matriks bertuliskan "Berlaku pada: Menambah pemeriksaan",
> `LAB-INH-001` memuat `Cancelled` sebagai pengecualian sah, `LAB-INH-006` mengatur jalur
> pengajuan pembatalan, dan `LAB-INH-010` menyerahkan koreksi tagihan kepada Billing.
>
> **`AC-55` dan `AC-57` yang ditulis amandemen kemarin bertentangan dengan rancangan warisan
> itu.** Menambahkan penjagaannya berarti mengubah kebijakan pembatalan, bukan menambal
> kelalaian — di luar wewenang task ini. `AC-56` terpenuhi.
>
> **Ditutup hari yang sama oleh `LAB-DEC-049`.** Pemilik modul memilih **mempersempit
> `AC-55`/`AC-57` ke penambahan saja**: pembatalan tetap terbuka sesudah kelayakan, dan koreksi
> tagihannya wewenang Billing. `RJ-BIL-GATE-DEC-003` tidak tersentuh.
>
> **Cakupan task ini karena itu menyusut drastis.** Jalur tambah **sudah terjaga** `VAL-18`,
> yang tidak boleh dikunci memang tidak terkunci. Keempat AC terverifikasi terhadap source, dan
> automated test tidak dibuat sesuai `rules/backend/TEST_POLICY.md`.
>
> Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-24.md`](../task/report/backend/BE-LAB-24.md).

| Butir | Isi |
|---|---|
| **Status** | ✅ `SELESAI` — 2026-09-14. Keempat AC terpenuhi oleh kode yang sudah berjalan; **nol baris source diubah, dan memang tidak perlu** |
| **Outcome** | Setelah kelayakan wadah ditetapkan, daftar pemeriksaan tidak dapat ditambah **maupun dihapus** |
| **Requirement/decision** | `FR-11.7`, `LAB-DEC-039`, BR-34 |
| **Kontrak** | `LAB-VAL-v1` `r3` `VAL-18` — **tidak berubah** |
| **Reuse** | **`Ready to reuse`.** `LabExaminationService.cs:120-127@466a7127` sudah menolak penambahan pada wadah `Accepted` atau `Rejected` |
| **Cakupan** | **Pemeriksaan, bukan pembangunan.** Telusuri jalur hapus baris pemeriksaan dan pastikan memakai penjagaan `VAL-18` yang sama. Bila ternyata belum, tambahkan penjagaannya. Tulis pengujian penjaga `T-55d` |
| **Dependency** | — |
| **Acceptance criteria** | `AC-55`, `AC-56`, `AC-57` |
| **Verifikasi** | Review diff/scope; verifikasi proses bisnis: menambah dan menghapus baris **berhasil** selama kelayakan belum ditetapkan; menambah pada wadah `Accepted` ditolak `409`; menambah pada wadah `Rejected` ditolak `409`; **menghapus** pada wadah yang sudah diputuskan ditolak; telusuri seluruh controller Laboratorium dan pastikan tidak ada route bernama `process` atau sejenisnya |
| **Risiko/pemilik** | Rendah dalam ukuran, **tinggi dalam akibat**. Bila jalur hapus ternyata longgar, pemeriksaan yang sudah terbit kelayakan tagihnya dapat hilang sementara tagihannya tetap berjalan. Pemilik: Laboratorium |
| **DoD** | Jalur tambah dan jalur hapus memakai penjagaan yang sama; `T-55d` hijau; **tidak ada aksi `Pemeriksaan Diproses` tersendiri yang dibuat**; `VAL-18` tidak berubah bunyinya |

> **Task ini boleh gagal pada percobaan pertama, dan itu memang gunanya.** `LAB-DEC-039`
> menaikkan perilaku yang sudah berjalan menjadi keputusan. Yang belum diperiksa adalah jalur
> hapus. Bila ternyata sudah terjaga, task ini selesai dengan menambah pengujian penjaganya
> saja — dan itu hasil yang sah, bukan task yang sia-sia.

### `BE-LAB-25` ✅ — Daftar pantau pemakaian `Lainnya`

> **Status: ✅ `SELESAI` — 2026-09-15.** Endpoint `GET /other-usage` berdiri, `dotnet build`
> **0 Error** tanpa satu pun warning dari berkas task ini, dan **verifikasi proses bisnisnya
> dijalankan terhadap `QuilvianNewDevYoga`**, bukan sekadar ditelusuri pada source.
>
> **`T-60d` terbukti persis seperti butir Verifikasi:** tiga wadah berketerangan `cairan kista`
> muncul sebagai **satu baris berjumlah tiga** beserta waktu pemakaian terakhirnya yang benar.
> Satu wadah berketerangan `Cairan Kista` sengaja ikut disisipkan dan **tetap berdiri sebagai
> baris tersendiri** — membuktikan ejaan tidak digabung diam-diam, yang justru menjadi alasan
> layar ini dibuat. Seluruhnya di dalam transaksi yang di-`ROLLBACK`; nol baris uji tertinggal.
>
> **Pemeriksaan yang tidak dapat ditangkap `dotnet build` ikut dijalankan.** `ToQueryString()`
> membuktikan pengelompokannya diterjemahkan menjadi **satu pernyataan `GROUP BY` penuh di
> PostgreSQL** — nol evaluasi sisi klien, sehingga tabel `LabSpecimen` tidak pernah ditarik ke
> memori. Waktu efektifnya `COALESCE(PhysicallyReceivedAt, CreateDateTime)`, **sama persis**
> dengan rekap penerimaan `BE-LAB-22`.
>
> **Nol tabel, nol migration, nol permission baru.** `AC-60` — AC terakhir `LAB-DEC-040` yang
> masih terbuka sejak `BE-LAB-20` — kini terpenuhi.
>
> Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-25.md`](../task/report/backend/BE-LAB-25.md).

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-15. `AC-60` terpenuhi dan terbukti terhadap database; nol tabel ringkasan |
| **Outcome** | Kepala instalasi melihat keterangan `Lainnya` yang sering muncul, lalu menaikkannya menjadi jenis tetap |
| **Requirement/decision** | `FR-11.2`, `LAB-DEC-040` butir 4-5, BR-35 |
| **Kontrak** | `LAB-API-v1` `r7` `GET /lab-specimen-types/other-usage` |
| **Reuse** | `Missing`. **Tanpa tabel baru** — rekapnya diturunkan dari `LabSpecimen` |
| **Cakupan** | Satu endpoint baca beserta DTO responsnya dan pengelompokan pada service |
| **Dependency** | `BE-LAB-21` — memerlukan kolom keterangan `Lainnya` beserta datanya |
| **Acceptance criteria** | `AC-60` |
| **Verifikasi** | Build; verifikasi kontrak terhadap `r7`; verifikasi proses bisnis: tiga wadah berketerangan "cairan kista" muncul sebagai satu baris berjumlah tiga beserta waktu pemakaian terakhirnya |
| **Risiko/pemilik** | Rendah. Baca saja, nol tabel, nol migration. Pemilik: Laboratorium |
| **DoD** | Endpoint menjawab sesuai `r7`; **tidak ada tabel ringkasan yang dibuat**; rekapnya berubah seketika ketika wadah baru dicatat |

**Kenapa tanpa tabel ringkasan.** Tabel ringkasan adalah salinan yang bisa basi tanpa menambah
satu pun jawaban baru. Yang ditanyakan kepala instalasi — keterangan apa yang sering muncul —
sudah seluruhnya ada pada `LabSpecimen`.

## 6c. Task Gelombang `MVP-5b` — Pemesanan per Disiplin dan Pendaftaran Kiosk

Ditambahkan 2026-09-15. Menurunkan `LAB-DEC-051` sampai `LAB-DEC-058` dari decision log
revision 30, di bawah persetujuan lintas modul
[`LAB-REQ-006`](../approval-requests/2026-09-15-persetujuan-bagian-lab-di-kiosk.md).

**Kontrak yang berlaku:** `LAB-API-v1` **`r10`**, `LAB-VAL-v1` **`r5`**, `LAB-PERM-v1` **rev 5** —
seluruhnya `approved` 2026-09-15.

### Grafik urutan dependency — `MVP-5b`

```text
BE-LAB-26 ──> BE-LAB-27 ──> BE-LAB-28
                  │
                  └──────────────┐
BE-EXT-04 ──> BE-EXT-05          ├──> FE-LAB-14
     │                           │
     └──> FE-LAB-13 ─────────────┘

BE-LAB-29  (berdiri sendiri)
```

| Gelombang eksekusi | Task | Kenapa di sini |
|---:|---|---|
| 1 | `BE-LAB-26` ✅, `BE-LAB-29` ✅, `BE-EXT-04` ✅ | **Gelombang 1 selesai 2026-09-15.** Ketiganya tanpa prasyarat. `BE-LAB-29` menutup `LAB-CONFLICT-007` dan tidak bergantung pada apa pun di gelombang ini |
| 2 | `BE-LAB-27` ✅, `BE-EXT-05` | `BE-LAB-27` memerlukan tabelnya; `BE-EXT-05` memerlukan ruas tujuan layanan |
| 3 | `BE-LAB-28` ✅, `FE-LAB-13`, `FE-LAB-14` | Penjagaan wadah memerlukan daftar terpesan; kedua layar memerlukan endpointnya |

**Tidak ada siklus.** Setiap task muncul tepat satu kali, dan jumlah pasangan prasyarat-ke-task
pada grafik sama dengan isi kolom **Dependency** pada keenam task di bawah.

> **Dua task berawalan `BE-EXT` menyentuh milik `registration-management`.** Polanya mengikuti
> `BE-EXT-01` sampai `BE-EXT-03`: task berada pada roadmap ini, dikerjakan atas wewenang
> `LAB-REQ-006`, **bukan** atas asumsi kepemilikan. Bila pemilik modul itu menghendaki
> pengerjaannya di repo atau urutan lain, itu keputusannya.

### `BE-LAB-26` ✅ — Tabel pemeriksaan terpesan

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-15. Diterapkan ke `QuilvianNewDevYoga`; unique parsial, `Restrict`, dan jalur `Down` terbukti |
| **Outcome** | Pemeriksaan yang dipilih petugas saat pendaftaran punya tempat tinggal, sebelum wadah fisiknya ada |
| **Requirement/decision** | `FR-11.2` turunan; `LAB-DEC-057` |
| **Kontrak** | Tidak ada endpoint. Struktur saja |
| **Reuse** | `Missing`. Pola mengikuti `LabExamination` untuk salinan kode dan nama, tanpa kolom tarif |
| **Cakupan** | Entity `LabOrderedProcedure`, enum `LabOrderedProcedureStatus`, configuration, DbSet, satu migration |
| **Dependency** | — |
| **Acceptance criteria** | `AC-91` |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi bentuk SQL migration; unique `(LabOrderId, ProcedureId)` menolak baris kembar di tingkat database |
| **Risiko/pemilik** | Rendah. Tabel baru tanpa yang menunjuk padanya. Pemilik: Laboratorium |
| **DoD** | Tabel berdiri beserta unique parsial dan ketiga FK `Restrict`; migration jalan maju dan mundur; **`LabExamination` tidak berubah satu baris pun** |

**Kenapa `LabExamination` disebut eksplisit pada DoD.** Seluruh alasan tabel ini ada adalah agar
`LabExamination` tidak disentuh. Bila task ini berakhir dengan `SpecimenId` menjadi nullable,
ia gagal walaupun tabelnya berdiri.

### `BE-LAB-27` ✅ — Endpoint pemesanan per disiplin

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-15. Pemecahan terbukti terhadap database; satu ruas kontrak dilaporkan tidak dibangun |
| **Outcome** | Petugas memilih pemeriksaan sekali, dan pasien muncul di menu Pemeriksaan yang benar — berapa pun disiplin yang terlibat |
| **Requirement/decision** | `FR-11.2`; `LAB-DEC-055`, `LAB-DEC-056` |
| **Kontrak** | `LAB-API-v1` `r10` `POST /lab-orders/by-examinations`; `LAB-VAL-v1` `r5` `VAL-64`..`VAL-67` |
| **Reuse** | `LabOrderService` `Extend`. `CreateAsync` yang sudah ada **tidak disentuh** |
| **Cakupan** | Satu DTO permintaan, satu method service yang memecah per disiplin, satu endpoint. Nol migration |
| **Dependency** | `BE-LAB-26` |
| **Acceptance criteria** | `AC-86`, `AC-87`, `AC-88` |
| **Verifikasi** | QBE preflight; build; verifikasi kontrak terhadap `r10`; verifikasi proses bisnis: Hemoglobin ditambah kultur darah menghasilkan **dua** pesanan berdisiplin tunggal dan keduanya muncul di menunya masing-masing; tiga pemeriksaan sedisiplin menghasilkan **satu** pesanan; pemeriksaan belum digolongkan berkumpul jadi satu pesanan tanpa disiplin; **`POST /lab-orders` lama dipanggil dengan muatan lama tetap mengembalikan tepat satu pesanan** |
| **Risiko/pemilik** | Sedang pada maknanya. Satu tindakan petugas menghasilkan lebih dari satu objek bisnis — bila pemecahannya keliru, pasien hilang dari menu tanpa pesan kesalahan. Pemilik: Laboratorium |
| **DoD** | Endpoint menjawab sesuai `r10`; `VAL-64`..`VAL-67` menolak sesuai matriks; pemecahan benar untuk lintas disiplin, sedisiplin, dan tanpa disiplin; **satu pemeriksaan ditolak berarti nol pesanan terbentuk**; endpoint lama terbukti tidak berubah |

### `BE-LAB-28` ✅ — Wadah hanya memuat yang dipesan

> **Status: ✅ `SELESAI` — 2026-09-15.** `AC-91` ditutup. Satu berkas source, nol migration, nol
> endpoint baru, nol permission baru. `dotnet build -p:RunAnalyzers=False --no-incremental`
> **0 Error**, nol warning dari berkas task ini.
>
> **Dijalankan terhadap `QuilvianNewDevYoga`, bukan ditelusuri pada source:** `VAL-68` menolak
> `422` dan `VAL-69` menolak `409`, keduanya dengan pesan yang dibandingkan **kata demi kata**
> terhadap matriks. Permintaan yang sudah berwadah ditandai `Fulfilled` beserta tautan ke baris
> pemeriksaan yang mengerjakannya; yang belum terbaca `Ordered` — **menunggu wadah, bukan hilang**.
>
> **Butir DoD yang paling mudah dilewatkan dibuktikan beserta baris kontrolnya:** pesanan lama
> tanpa baris terpesan **menerima wadah seperti sebelumnya**, dan permintaan yang **sama persis**
> ditolak pada pesanan yang punya baris terpesan. Tanpa baris kontrol itu, "pesanan lama tidak
> tersentuh" bisa lulus hanya karena penjagaannya tidak pernah berjalan.
>
> **Satu jebakan ditemukan sebelum ditulis, bukan sesudah:** membatalkan wadah **tidak**
> membatalkan pemeriksaan di dalamnya. Menegakkan `VAL-69` dari penanda `Fulfilled` akan mengunci
> permintaan yang wadahnya dibatalkan — sah, tetapi tidak akan pernah bisa diwadahi ulang.
> Penjagaannya membaca keadaan yang sebenarnya, sehingga pulih sendiri; pelepasan penanda saat
> wadah dibatalkan ditambahkan pada satu tempat dan **dilaporkan sebagai tambahan cakupan**.
>
> **Satu batas sengaja tidak dilewati:** pembatalan **pesanan** tidak menyentuh baris terpesan —
> menutupnya menuntut keputusan yang belum pernah diambil.
>
> Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-28.md`](../task/report/backend/BE-LAB-28.md).


| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-15. Kedua penjagaan berdiri dan terbukti terhadap database; `AC-91` ditutup. Satu tambahan cakupan dilaporkan: pelepasan penanda saat wadah dibatalkan |
| **Outcome** | Pemeriksaan yang masuk wadah selalu berasal dari yang benar-benar dipesan, dan yang belum berwadah terlihat sebagai menunggu |
| **Requirement/decision** | `LAB-DEC-057` |
| **Kontrak** | `LAB-VAL-v1` `r5` `VAL-68`, `VAL-69` |
| **Reuse** | `LabSpecimenService` `Extend` |
| **Cakupan** | Penjagaan pada jalur rencana wadah, penandaan `Fulfilled` beserta tautan ke baris pemeriksaannya. Nol migration |
| **Dependency** | `BE-LAB-27` |
| **Acceptance criteria** | `AC-91` |
| **Verifikasi** | Build; verifikasi proses bisnis: pemeriksaan di luar daftar terpesan ditolak `422` `VAL-68`; pemeriksaan terpesan yang sudah berwadah ditolak `409` `VAL-69`; **pesanan lama tanpa baris terpesan tetap menerima wadah apa pun seperti sebelumnya** |
| **Risiko/pemilik** | Sedang. Mengetatkan endpoint yang sedang dipakai. Pemilik: Laboratorium |
| **DoD** | `VAL-68` dan `VAL-69` menolak sesuai matriks; **keduanya terbukti tidak menyentuh pesanan yang tidak punya baris terpesan**; baris terpesan tertaut ke pemeriksaan yang memenuhinya |

**Butir DoD yang paling mudah dilewatkan.** "Terbukti tidak menyentuh pesanan lama" adalah
pengujian yang membuktikan **ketiadaan perubahan**. Tanpanya, klaim aditif pada task ini tidak
pernah benar-benar diperiksa — dan `BE-LAB-21` baru saja menunjukkan harganya.

### `BE-LAB-29` ✅ — Disiplin diturunkan pada endpoint pesanan lama

> **Status: ✅ `SELESAI` — 2026-09-15.** Satu ekspresi diubah, `LAB-CONFLICT-007` ditutup.
> `dotnet build --no-incremental` **0 Error**, nol warning dari modul Laboratorium.
>
> **Penutupan lubangnya dibuktikan dengan pencarian, bukan dengan kepercayaan:** penelusuran
> `new LabOrder` dan `LabOrders.Add` di seluruh aplikasi menemukan **tepat satu** jalur tulis,
> dan itulah yang diubah. Tidak ada seeder, service lain, maupun controller yang membuat pesanan
> sendiri.
>
> **Bukti terhadap database yang paling berbicara:** kedua pesanan tanpa disiplin ternyata atas
> pemeriksaan yang **katalognya sudah tahu disiplinnya** — Glukosa Darah Sewaktu berdisiplin
> Patologi Klinik, Sitologi FNAB berdisiplin Patologi Anatomi. Informasinya selalu ada; yang
> hilang hanya penyalinannya. Katalog lab **10 dari 10 tergolong**, sehingga setiap pesanan baru
> mulai sekarang akan berdisiplin apa pun jalur pemanggilnya.
>
> **Dua temuan gate ditutup lebih dulu, keduanya kelalaian pembukuan sesi ini sendiri:** `AC-83`
> **tidak punya satu pun baris uji** sejak ditulis 2026-09-14 — itu sebab teknis mengapa
> pelanggarannya bertahan tanpa disadari — dan kepala roadmap ini masih menyebut manifest
> revision `26` beserta SHA `466a7127`.
>
> **Dua baris data lama sengaja tidak diperbaiki.** Mengisinya perubahan data, bukan perubahan
> kode, dan memerlukan wewenang tersendiri.
>
> Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-29.md`](../task/report/backend/BE-LAB-29.md).

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-15. `LAB-CONFLICT-007` ditutup; dua baris data lama di luar cakupan |
| **Outcome** | Pesanan tidak lagi dapat tersimpan tanpa disiplin ketika pemeriksaannya sudah digolongkan, dari jalur mana pun — bukan hanya dari layar |
| **Requirement/decision** | `LAB-DEC-048` butir 6; `AC-83` sebagaimana diamandemen `LAB-DEC-055` |
| **Kontrak** | Tidak ada perubahan kontrak. Perilaku yang dikoreksi, bukan bentuk |
| **Reuse** | `LabOrderService.CreateAsync` `Extend` |
| **Cakupan** | Bila permintaan tidak membawa disiplin, turunkan dari `MstProcedure.LabDiscipline` milik `ProcedureId`-nya. Nol migration |
| **Dependency** | — |
| **Acceptance criteria** | `AC-83` |
| **Verifikasi** | Build; verifikasi proses bisnis: pesanan yang dibuat tanpa ruas disiplin **kini berdisiplin** dan muncul di menu yang benar; pesanan atas pemeriksaan yang belum digolongkan tetap tersimpan tanpa disiplin (`AC-85`); **permintaan yang memang membawa disiplin tetap dihormati apa adanya** |
| **Risiko/pemilik** | Rendah. Menutup lubang tanpa menolak satu pun permintaan yang sebelumnya diterima. Pemilik: Laboratorium |
| **DoD** | Tidak ada jalur kode yang menyimpan pesanan tanpa disiplin ketika prosedurnya digolongkan; **nol permintaan yang sebelumnya berhasil menjadi gagal**; `LAB-CONFLICT-007` ditutup |

> **Dua baris data lama di luar cakupan.** Dua pesanan tanpa disiplin yang sudah ada pada
> `QuilvianNewDevYoga` **tidak** diperbaiki task ini. Mengisinya adalah perubahan data, bukan
> perubahan kode, dan memerlukan wewenang tersendiri. Dicatat, bukan dikerjakan diam-diam.

### `BE-EXT-04` ✅ — [Registrasi] Kiosk mengetahui tujuan layanan

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-15. Diterapkan ke `QuilvianNewDevYoga`; 16 sesi lama terbukti utuh |
| **Outcome** | Pasien memilih Laboratorium sendiri di kiosk, dan petugas lab dapat melihat sesi yang menunggu |
| **Requirement/decision** | `LAB-DEC-051`, `LAB-DEC-052`; wewenang `LAB-REQ-006` |
| **Kontrak** | Dikontrakkan di sisi `registration-management`. `LAB-API-v1` tidak bertambah endpoint |
| **Reuse** | `TrxKioskScanSession` `Extend`, milik `registration-management` |
| **Cakupan** | Dua ruas nullable — tujuan layanan dan jalur permintaan dokter — beserta satu jalur baca sesi bertujuan Laboratorium yang belum diproses. Satu migration |
| **Dependency** | — |
| **Acceptance criteria** | `AC-93` |
| **Verifikasi** | QBE preflight; build; verifikasi proses bisnis: **16 sesi yang sudah tersimpan tetap terbaca** dengan kedua ruas baru kosong; sesi baru bertujuan Laboratorium muncul pada jalur bacanya; migration jalan maju dan mundur |
| **Risiko/pemilik** | Sedang. Menyentuh tabel milik modul lain yang sudah berisi data nyata. Pemilik: `registration-management`, dikerjakan atas wewenang `LAB-REQ-006` |
| **DoD** | Kedua ruas nullable; **nol perilaku sesi kiosk yang sudah ada berubah**; data lama utuh; migration jalan maju dan mundur |

### `BE-EXT-04b` ✅ — [Registrasi] Jalur tulis tujuan layanan pada sesi kiosk

> **Status: ✅ `SELESAI` — 2026-09-15.** Susulan atas `BE-EXT-04`, lahir dari pemeriksaan
> pra-implementasi `BE-EXT-05`.
>
> **Kenapa task ini ada.** `BE-EXT-04` mendirikan `TargetService` dan `HasPhysicianRequest`
> beserta penyaring bacanya, tetapi **jalur tulisnya tidak ikut dibuka** —
> `CreateKioskScanSessionRequest` nol memuat keduanya. Kolomnya berdiri tanpa satu pun cara
> mengisinya, dan datanya membenarkan: **16 dari 16 sesi bernilai `null`**, termasuk yang dibuat
> sesudah kolomnya ada. Dua task tertahan karenanya: `BE-EXT-05` tidak punya pemicu, dan
> `FE-LAB-14` akan selalu menerima daftar kosong.
>
> **Dibuktikan dengan memanggil controller yang sebenarnya:** `TargetService=Laboratory`
> tersimpan dan terbaca kembali; penyaring `GET /options` menemukan **1** sesi dari sebelumnya
> selalu **0**; nilai di luar daftar ditolak `400`; dan **muatan lama tetap diterima** dengan
> kedua ruas tetap `null`. Nol migration — kolomnya sudah ada.
>
> **Cara pembersihannya berbeda dan itu dicatat:** controller membuka transaksinya sendiri,
> sehingga `ROLLBACK` dari luar mustahil. Dua baris uji benar-benar tersimpan lalu dihapus
> permanen, dan kebersihannya dibuktikan dengan hitungan ulang dari koneksi baru — 16/0 sebelum
> dan sesudah.
>
> Laporan: [`task/report/backend/BE-EXT-04b.md`](../task/report/backend/BE-EXT-04b.md).

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-15 |
| **Outcome** | Kiosk dapat menyatakan layanan yang dituju pasien, sehingga kolom `BE-EXT-04` berhenti menjadi kolom yang mustahil terisi |
| **Requirement/decision** | `LAB-DEC-052`; wewenang `LAB-REQ-006` §1.1 |
| **Kontrak** | Dikontrakkan di sisi `registration-management`; bentuknya ditetapkan `LAB-REQ-006` §1.1 |
| **Reuse** | `KioskScanSessionController` `Extend`. Endpoint `POST /scan-result` tidak berubah bentuk maupun perilakunya bagi pemanggil lama |
| **Cakupan** | Dua ruas opsional pada DTO permintaan, satu pemeriksaan nilai enum, dua penyalinan ke entity. Nol migration |
| **Dependency** | `BE-EXT-04` ✅ |
| **Verifikasi** | Build; controller sebenarnya dipanggil: tersimpan, terbaca penyaring, nilai di luar daftar `400`, **muatan lama tetap diterima** |
| **Risiko/pemilik** | Rendah. Aditif dan terbukti tidak mengetatkan pemanggil lama. Pemilik: `registration-management` |
| **DoD** | Kedua ruas dapat diisi; penyaring `BE-EXT-04` terbukti menemukan sesi; pemanggil lama tidak ikut diketatkan |

**Yang sengaja tidak dibangun.** Jalur **ubah** sesi kiosk. Kedua ruas hanya dapat dikirim saat
sesi dibuat, karena itulah satu-satunya jalur tulis yang ada. Bila di lapangan pasien memilih
layanan **sesudah** kartunya dipindai, diperlukan satu jalur ubah tersendiri — dan yang
menentukannya adalah alur layar kiosk milik `registration-management`, bukan tebakan dari sini.

### `BE-EXT-05` ✅ — [Registrasi] Kunjungan dari kiosk dan penutupan otomatisnya

> **Status: ✅ `SELESAI` — 2026-09-17**, dikerjakan atas wewenang pemilik modul
> `registration-management` **Andry Zain**, pilihan **A** pada `LAB-REQ-012` bagian 3.3.
> Laporan: [`task/report/backend/BE-EXT-05.md`](../task/report/backend/BE-EXT-05.md).
>
> **Kedua penahannya terbantah, dan keduanya salah dengan cara yang sama: menyebut nama yang
> benar pada jalur yang salah.**
>
> `LAB-OPEN-025` menunjuk `EncounterIntakeService.RegisterAsync` yang memang menuntut
> `PatientEncounter:Create` — tetapi **bukan itu yang dipakai kiosk.** Kiosk menempuh
> `POST /patient-encounters/kiosk`, yang dijaga **hanya** `[Authorize(Policy = KioskReadPolicy)]`
> dan **nol** memikul `[AccessPermission]`. Route itu **sudah ada sebelum task ini**, dan sudah
> mengerjakan hampir seluruh butir 3 `BR-46`: menerima `KioskScanSessionId`, menandai
> `IsFromKiosk`, membentuk nomor antrean, menandai sesi terpakai, dan menolak Penjamin
> Perusahaan. **Pilihan A yang disetujui Andry karena itu sudah berdiri di source.**
>
> `LAB-OPEN-026` menunjuk `IsAvailableForKiosk` — kolom yang **nol disebut** oleh
> `PatientEncounterController` maupun `EncounterIntakeService`. Yang benar-benar dituntut jalur
> pembentukan kunjungan adalah **`IsAvailableForRegistration`**, dan pada `SU-LAB-001` nilainya
> **`true`**. Pertanyaannya tidak gugur, tetapi sifatnya berubah: **bukan penahan**, melainkan
> syarat agar Laboratorium tampil pada daftar pilihan layar kiosk. Tetap milik `master-data`.
>
> **Sisa yang benar-benar belum ada karena itu satu hal:** butir 4 `BR-46` — penutupan otomatis
> pada akhir hari layanan. Empat berkas baru di Registrasi, satu di Laboratorium, nol migration,
> nol kolom baru, nol endpoint baru.
>
> **Penyaringnya dibangun agar tidak mungkin melebar, bukan sekadar ditulis sempit.** Sasaran
> diambil dari `IEncounterContinuationProbe.TargetService`, bukan dari konfigurasi; **nol
> penjawab terdaftar berarti nol kunjungan ditutup.** Mencabut pendaftaran penjawab membuat unit
> itu berhenti ikut ditutup — bukan ditutup membabi buta. Arah ketergantungan antar modul
> terjaga dan **diukur**: `RegistrationManagement` tetap **nol** menyebut `LaboratoryManagement`
> dalam kode — nol `using`, nol tipe, nol pemanggilan. Satu-satunya kemunculan namanya ada di
> dalam komentar yang menjelaskan mengapa arahnya dijaga.
>
> **Selektivitasnya diukur terhadap `QuilvianNewDevYoga`, bukan diperkirakan** — nol baris
> diubah: penyaring sebagaimana ditulis cocok **0** kunjungan; tanpa klausa tujuan **14**; tanpa
> klausa kiosk **157**, yang **91** di antaranya `WaitingForNurse` — pasien poliklinik yang
> sedang menunggu dipanggil.
>
> **Empat cabang dibuktikan lewat aplikasi yang benar-benar menyala** — satu kunjungan yang harus
> tertutup dan **tiga yang harus tetap utuh** (sudah punya `LabOrder`; bertujuan poliklinik; sudah
> `CheckedInAt`). Ketiganya bertahan; yang pertama berpindah `5 → 11` beserta antreannya `4 → 8`.
> 15 kunjungan kiosk nyata dan 91 `WaitingForNurse` **tidak bergeser satu pun**; idempotensi
> terbukti pada putaran kedua; nol baris uji tertinggal, dihitung ulang dari koneksi baru. Pukul
> batasnya diturunkan lewat **variabel lingkungan**, yang sekaligus membuktikan tuntutan
> `LAB-DEC-059` bahwa angkanya dapat diubah tanpa rilis ulang.
>
> **Uji itulah yang menemukan satu cacat, dan cacatnya persis kelas yang paling berbahaya pada
> modul ini.** Putaran pertama **gagal seluruhnya**: `NoShowByUserId` ber-foreign key ke
> `AspNetUsers`, dan aktor sistem default `Guid.Empty` menunjuk pengguna yang tidak ada.
> Transaksinya ter-rollback dengan benar dan penjadwalnya mencoba lagi — **tidak ada satu pun
> yang tampak rusak dari luar**, sementara penutupan otomatis tidak akan pernah terjadi, setiap
> malam, tanpa tanda apa pun kecuali baris log. Perbaikannya bukan GUID karangan melainkan nilai
> yang jujur: kedua kolom itu **nullable**, dan `null` memang artinya — penutupan ini tidak
> dilakukan orang. Aktor yang disetel tetapi tidak ada pun **tidak menghentikan** penutupan; ia
> dicatat dan penutupannya jalan terus.
>
> **Dua hal diserahkan ke luar, ditulis supaya tidak hilang:** `EncounterStatus` kunjungan
> laboratorium menjadi `WaitingForDoctor` walaupun unitnya ber-`IsDoctorRequired = false`
> (`PatientEncounterController.cs:671` hanya memeriksa screening) — **tidak diperbaiki**, karena
> cabang itu berlaku bagi seluruh unit ber-`IsScreeningRequired = false`; dan pukul 21:00 masih
> angka yang belum dikonfirmasi terhadap jam operasional resmi.

<details>
<summary>Catatan penahan 2026-09-15, disimpan sebagai riwayat — <b>kedua penahannya terbantah 2026-09-17</b></summary>

> **Status: ⛔ `TERTAHAN` — dihentikan 2026-09-15 sebelum satu baris pun ditulis.**
>
> **Bukan kehati-hatian umum; penahannya konkret.** Pemeriksaan pra-implementasi menemukan
> `TargetService` — sinyal yang menjadi pemicu seluruh task ini — **tidak dapat diisi oleh siapa
> pun**: jalur tulis sesi kiosk nol memuatnya, dan 16 dari 16 sesi bernilai `null`. Penahan itu
> **sudah ditutup** `BE-EXT-04b`.
>
> **Bahaya yang dihindari terukur, bukan diperkirakan.** Penyaring penutupan otomatis yang aman
> (`TargetService = Laboratory`) hari ini cocok **nol baris**. Penyaring yang lebih longgar akan
> menyapu **15 kunjungan kiosk nyata**, dan 91 kunjungan pada database berstatus
> `WaitingForNurse` — pasien poliklinik yang sedang menunggu dipanggil. Itu persis kegagalan yang
> ditulis pada baris risiko task ini, dan ia akan terjadi pada hari pertama.
>
> **Dua penahan tersisa, keduanya milik `registration-management`:**
>
> | Penahan | Isi |
> |---|---|
> | `LAB-OPEN-025` | Apakah prinsipal kiosk boleh membentuk kunjungan. Kiosk berjalan di bawah `KioskReadPolicy`; `EncounterIntakeService.RegisterAsync` menuntut `PatientEncounter:Create`. Bila tidak dipegang, setiap pasien kiosk gagal didaftarkan — dan gagalnya tidak terlihat sebagai galat sistem |
> | `LAB-OPEN-026` | Unit `SU-LAB-001 Laboratorium Klinik` ber-`IsAvailableForKiosk = false`. Perubahan data induk, wewenangnya terpisah |
>
> **Satu ketentuan yang selama ini tidak bernilai kini ada:** `LAB-DEC-059` — hari layanan
> berakhir **21:00 WIB**, ditulis sebagai konfigurasi. `LAB-DEC-058` menetapkan *kapan* tetapi
> tidak pernah *pukul berapa*. Angkanya **belum dikonfirmasi** terhadap jam operasional resmi
> maupun terhadap pemilik `registration-management`.
>
> **Satu butir DoD ternyata sudah terpenuhi tanpa kode apa pun.** "Biaya pendaftaran gugur":
> Registrasi **nol** menerbitkan fakta kelayakan tagih — penerbitnya hanya Klinis, Laboratorium,
> Farmasi, dan Radiologi — dan `DefaultRegistrationFee` hanya hidup sebagai data induk. Tidak ada
> tagihan yang perlu digugurkan karena tidak pernah ada yang terbit. Ini perlu diketahui sebelum
> task ini dilanjutkan, supaya tidak ada yang menulis kode pembatalan tagihan yang tidak pernah
> punya sasaran.

</details>

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-17. `LAB-OPEN-025` dan `LAB-OPEN-026` **terbantah**, bukan dijawab: keduanya menyebut jalur yang tidak dipakai kiosk. Empat berkas baru di Registrasi, satu di Laboratorium; nol migration, nol kolom baru, nol endpoint baru. **Seluruh butir DoD terpenuhi**, dan satu cacat ditemukan ujinya lalu diperbaiki |
| **Outcome** | Pasien yang selesai di kiosk langsung punya kunjungan; yang pergi tanpa diperiksa tidak meninggalkan kunjungan menggantung maupun tagihan |
| **Requirement/decision** | `LAB-DEC-053`, `LAB-DEC-054`, `LAB-DEC-058`; wewenang `LAB-REQ-006` |
| **Kontrak** | Dikontrakkan di sisi `registration-management` |
| **Reuse** | `EncounterIntakeService` `Extend` — pelaksana `INT-05` yang dibangun `BE-LAB-08` |
| **Cakupan** | Pembentukan kunjungan dari sesi kiosk, dan penutupan otomatis pada akhir hari layanan dengan sebab tidak dilanjutkan beserta pengguguran biaya pendaftarannya |
| **Dependency** | `BE-EXT-04` |
| **Acceptance criteria** | `AC-92` |
| **Verifikasi** | Build **0 error**, nol peringatan dari kelima berkas baru. **Selektivitas penyaring diukur terhadap `QuilvianNewDevYoga`, nol baris diubah:** sebagaimana ditulis cocok **0**; tanpa klausa tujuan **14**; tanpa klausa kiosk **157**, yang **91** di antaranya `WaitingForNurse`. **Empat cabang dijalankan lewat aplikasi yang benar-benar menyala:** satu ditutup (`5 → 11`, antrean `4 → 8`), **tiga bertahan** — punya `LabOrder`, bertujuan poliklinik, sudah `CheckedInAt`. Pagar data nyata tidak bergeser (15 dan 91); idempotensi terbukti; nol baris uji tertinggal dari koneksi baru. **Satu cacat ditemukan uji ini dan diperbaiki** — FK `NoShowByUserId` terhadap aktor `Guid.Empty` |
| **Risiko/pemilik** | **Tinggi pada maknanya.** Penutupan otomatis yang terlalu rakus akan menutup kunjungan pasien yang sedang antre. Dijawab dengan bentuk kode, bukan dengan kehati-hatian: sasaran diambil dari penjawab unit, dan **nol penjawab berarti nol penutupan**. Pemilik: `registration-management` |
| **DoD** | ✅ Kunjungan terbentuk dari sesi kiosk — **sudah berdiri sebelum task ini**, diverifikasi dari source; ✅ penutupan otomatis hanya mengenai yang **tidak pernah dilanjutkan**, dibuktikan empat cabang; ✅ biaya pendaftaran gugur tanpa kode, alasannya kini tertulis di dalam source; ✅ **`AC-45` tetap tegak** — berkas Laboratorium satu-satunya hanya **membaca** `LabOrder`; ✅ verifikasi proses bisnis dengan baris nyata **terpenuhi**, dan nol baris uji tertinggal |

**Kenapa risikonya ditulis tinggi padahal kodenya sederhana.** Kesalahan di sini tidak muncul
sebagai galat. Ia muncul sebagai pasien yang pendaftarannya hilang saat ia sedang duduk menunggu
dipanggil — dan yang pertama mengetahuinya adalah pasien itu, bukan sistem.

---

## 6d. Gelombang `MVP-5c` — Konfirmasi Pesanan dan Pembatalan Beralasan

**Ditambahkan 2026-09-15**, menurunkan `LAB-DEC-061` dan `LAB-DEC-063` dari rekonsiliasi bukti
putaran 2. Kontrak `LAB-STATE-v1` `r3`, `LAB-VAL-v1` `r6`, dan `LAB-API-v1` `r12` seluruhnya
`approved` pada tanggal yang sama.

**Urutan dependency:**

```
BE-LAB-30 ──> BE-LAB-31 ──> FE-LAB-15
          └─> BE-LAB-32 ──> FE-LAB-16
FE-LAB-17  (berdiri sendiri)
```

### `BE-LAB-30` ✅ — Status `Confirmed` dan tiga kolomnya

> **Status: `SELESAI` — 2026-09-16.** Seluruh butir DoD terpenuhi. Source, configuration,
> pembuatan migration, dan eksekusi migration ke `QuilvianNewDevYoga` selesai dan terverifikasi;
> jalur `Down` lalu `Up` ikut dibuktikan. Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-30.md`](../task/report/backend/BE-LAB-30.md).
>
> **Nilai enumnya `Confirmed = 9`, bukan `3`.** `LAB-STATE-v1` menempatkan `Confirmed` *antara*
> `Requested` dan `Accepted` pada **alur kerja**; itu bukan urutan angka. Nilai `LabOrderStatus`
> dipersistensi sebagai `integer`, sehingga menyisipkannya sebagai `3` akan menggeser seluruh
> nilai sesudahnya — dua pesanan `Accepted` akan terbaca `Confirmed` dan satu pesanan `InProcess`
> akan terbaca `Accepted`, tanpa satu baris pun berubah isinya. Urutan alur kerja tetap dijaga
> matriks transisi.
>
> **Butir "nol perilaku berubah" pada baris risiko terpenuhi dengan satu pengecualian yang
> dilaporkan:** daftar pilihan penyaring status pada `GET /lab-orders/filters/metadata` kini
> memuat **9** pilihan, bukan 8 — karena `LabFilterMetadataFactory` menelusuri seluruh nilai enum.
> Labelnya ditambahkan pada berkas yang sama supaya berbunyi `Dikonfirmasi`, bukan `Confirmed`.
> Bentuk pesannya tidak berubah, dan `LAB-API-v1` tidak perlu dinaikkan revisinya.
>
> **Satu temuan di luar DoD yang perlu diketahui `BE-LAB-31`:** `LabOrderService.GetSummaryAsync`
> mencacah pesanan ke delapan ember status yang **tetap**, sementara `TotalPesanan` mencacah
> seluruhnya. Begitu konfirmasi dapat dijalankan, jumlah kedelapan ember itu tidak akan lagi sama
> dengan `TotalPesanan`. Menambah embernya adalah perubahan `LAB-API-v1`, dan itu bukan wewenang
> task ini.

| Butir | Isi |
|---|---|
| **Outcome** | Sistem punya tempat menyimpan siapa yang mengonfirmasi, kapan, dan dokter pemeriksa mana yang dipilih |
| **Requirement/decision** | `LAB-DEC-061` |
| **Kontrak** | `LAB-STATE-v1` `r3` bagian 1a |
| **Reuse** | `LabOrder` `Extend`; `LabOrderStatus` bertambah satu nilai |
| **Cakupan** | Satu nilai enum `Confirmed`, tiga kolom nullable pada `LabOrder`, satu migration, satu configuration |
| **Dependency** | — |
| **Acceptance criteria** | Prasyarat `AC-94`, `AC-95` |
| **Verifikasi** | Build; migration diterapkan; **jalur `Down` lalu `Up` dibuktikan**; seluruh pesanan yang sudah ada terbukti utuh dan ketiga kolom barunya `null` |
| **Risiko/pemilik** | Rendah. Ketiganya nullable dan nol perilaku berubah. Pemilik: Laboratorium |
| **DoD** | Ketiga kolom berdiri; nilai enum `Confirmed` ada; **nol pesanan lama berubah nilainya**; `Down` terbukti |

> **Kenapa ketiganya nullable.** Seluruh pesanan yang sudah ada tidak pernah dikonfirmasi. Kolom
> wajib akan menggagalkan migrationnya atau memaksa pengisian tebakan atas pesanan yang benar-benar
> sudah terjadi — pelajaran yang sama dengan `BE-EXT-04`.

### `BE-LAB-31` ✅ — Endpoint konfirmasi pesanan

> **Status: `SELESAI` — 2026-09-16.** Seluruh butir DoD terpenuhi. `VAL-70` sampai `VAL-73`
> keempatnya terbukti menolak sesuai matriks — pesan dan kodenya dibandingkan kata demi kata —
> dan `T-97c` dibuktikan dari database, bukan ditelusuri pada source. Laporan lengkap beserta
> buktinya: [`task/report/backend/BE-LAB-31.md`](../task/report/backend/BE-LAB-31.md).
>
> **Satu celah gelombang ini ditutup di sini, dan ia tidak dimiliki task mana pun.**
> `LAB-STATE-v1` `r3` bagian 1a menuliskan `Confirmed` → `Accepted` sebagai turunan otomatis
> sistem, tetapi tidak satu pun dari `BE-LAB-30`, `BE-LAB-31`, maupun `BE-LAB-32` menyebutnya pada
> cakupannya. Tanpa turunan itu **mengonfirmasi pesanan justru membuatnya tidak dapat dikerjakan**:
> `StartProcessAsync` hanya menerima `Accepted`, sehingga pesanan berhenti selamanya di
> `Confirmed`. Jebakannya dibuktikan, bukan diperkirakan — pesanan `Confirmed` ditolak
> `StartProcessAsync` dengan pesannya sendiri. Satu kondisi pada `LabSpecimenService` ditambah
> `Confirmed`; penambahannya aditif, dan nol pesanan lama berstatus itu.
>
> **Dua acceptance criteria terpenuhi sebagian, dan penyebabnya bukan pekerjaan yang kurang.**
> `AC-94` bagian "terbaca pada daftar" dan `AC-95` bagian "tampil pada daftar serta ringkasan
> cetak" **belum** terpenuhi karena `LAB-API-v1` `r12` §7.1 hanya mendefinisikan badan permintaan
> dan **tidak menambah satu pun ruas respons**. Akibatnya `confirmedByName`, `confirmedAt`, dan
> `examinerDoctorName` tidak dikembalikan endpoint mana pun, sehingga **`FE-LAB-15` tertahan** —
> layarnya diwajibkan menampilkan tepat ketiga nilai itu. Usul `r13` berisi lima ruas sudah
> ditulis lengkap pada laporan bagian 7.2, tinggal disetujui atau ditolak pemilik modul.
>
> **Satu kegagalan uji yang justru membuktikan aturan bisnis bekerja:** rantai wadah gagal
> `VAL-09` — petugas yang mengambil sampel tidak boleh menyatakan kelayakannya. Yang salah adalah
> harnessnya, yang memakai satu aktor untuk seluruh langkah; ia diperbaiki memakai dua aktor.

| Butir | Isi |
|---|---|
| **Outcome** | Petugas mengonfirmasi pesanan sekali, memilih dokter pemeriksa, dan namanya tercatat sebagai konfirmator |
| **Requirement/decision** | `LAB-DEC-061` |
| **Kontrak** | `LAB-API-v1` `r12` §7.1 `POST /lab-orders/{id}/confirm`; `LAB-VAL-v1` `r6` `VAL-70`..`VAL-73` |
| **Reuse** | `LabOrderService` `Extend`. `CreateAsync` dan `CreateByExaminationsAsync` **tidak disentuh** |
| **Cakupan** | Satu DTO permintaan, satu method service, satu endpoint. Nol migration |
| **Dependency** | `BE-LAB-30` |
| **Acceptance criteria** | `AC-94`, `AC-95` |
| **Baris uji** | `T-94a`, `T-94b`, `T-94c`, `T-95a`, `T-95b`, **`T-97c`** |
| **Verifikasi** | Build; verifikasi kontrak terhadap `r12`; `VAL-70`..`VAL-73` menolak sesuai matriks; **jalur `Requested` → `Accepted` tanpa konfirmasi terbukti masih berjalan** |
| **Risiko/pemilik** | Sedang. Menambah tahap pada alur yang sedang dipakai. Pemilik: Laboratorium |
| **DoD** | Endpoint menjawab sesuai `r12`; konfirmator dan waktu **diturunkan server**, tidak dari badan permintaan; konfirmasi kedua ditolak; **`T-97c` membuktikan jalur lama tidak tertutup** |

### `BE-LAB-32` ✅ — Pembatalan pesanan wajib beralasan

> **Status: `SELESAI` — 2026-09-16.** Seluruh butir DoD terpenuhi. `VAL-74` menolak `422` dan
> `VAL-75` menolak `409`, pesannya dibandingkan kata demi kata terhadap matriks; alasan terbaca
> kembali dari jejak audit; dan **`T-97b` membuktikan jalur yang sah tidak ikut tertutup**.
> Laporan lengkap beserta buktinya:
> [`task/report/backend/BE-LAB-32.md`](../task/report/backend/BE-LAB-32.md).
>
> **Butir "wajib dikerjakan lebih dulu" benar-benar dikerjakan lebih dulu**, sebelum satu baris
> pun diubah:
>
> | Yang diukur | Hasil |
> |---|---|
> | Pesanan kehilangan jalur pembatalan | **3** — `Accepted` 2, `InProcess` 1, `OnHold` 0 |
> | Pesanan yang tetap dapat dibatalkan | 2 — keduanya `Requested` |
> | Pembatalan pesanan yang **pernah terjadi** | **0** — nol baris `Order.Cancel` pada seluruh jejak audit |
> | Pemanggil backend | **1** — hanya `LabOrderController.Cancel` |
> | Layar frontend yang memanggilnya | **0** — 12 berkas modul Laboratorium diperiksa read-only |
>
> **Artinya pengetatan ini mengenai jalur yang belum pernah dipakai satu kali pun.** Risiko
> `Tinggi` yang ditulis roadmap adalah kehati-hatian yang benar; pengukurannya menunjukkan
> ledakannya hari ini **nol**, dengan **3 pesanan** sebagai biaya di masa depan. Angka itu tidak
> diperkecil: bila salah satunya perlu dibatalkan besok, tidak ada jalur tersisa sampai aturan
> koreksi diputuskan (`LAB-P0-003`).
>
> **Satu penjaga digantikan tiga, dan itu diminta `T-97a`.** Dua penjaga lama — "sudah
> dibatalkan" dan "sudah selesai", keduanya `400` — digantikan satu penjaga `VAL-75` yang menutup
> tujuh status sekaligus dengan `409`. Akibatnya dilaporkan apa adanya: pesanan yang sudah
> dibatalkan kini dijawab "Pesanan yang sudah diproses tidak dapat dibatalkan.", kalimat yang
> kurang tepat untuk keadaan itu. Pesannya diambil kata demi kata dari matriks; memperbaikinya
> adalah perubahan `LAB-VAL-v1` tersendiri.
>
> **Nol kolom baru, seperti dijanjikan.** Alasannya tetap disimpan sebagai `ReasonNote` pada
> `LabTransitionHistory`, dan terbukti **dirapikan sebelum disimpan** — spasi di awal dan akhir
> dibuang, sehingga alasan yang sama tidak terbaca sebagai dua alasan berbeda.

| Butir | Isi |
|---|---|
| **Outcome** | Pesanan yang dibatalkan selalu punya alasan yang dapat ditelusuri, dan pesanan yang sudah dikerjakan tidak lagi dapat dibatalkan diam-diam |
| **Requirement/decision** | `LAB-DEC-063` |
| **Kontrak** | `LAB-API-v1` `r12` §7.2; `LAB-VAL-v1` `r6` `VAL-74`, `VAL-75` |
| **Reuse** | `LabOrderService.CancelAsync` `Extend`. **Nol kolom baru** — alasannya sudah tersimpan sebagai `ReasonNote` pada `LabTransitionHistory` |
| **Cakupan** | Dua aturan validasi pada method yang sudah ada. Nol migration |
| **Dependency** | `BE-LAB-30` |
| **Acceptance criteria** | `AC-96`, `AC-97` |
| **Baris uji** | `T-96a`, `T-97a`, **`T-97b`** |
| **Verifikasi** | Build; `VAL-74` `422` dan `VAL-75` `409` sesuai matriks; alasan terbaca kembali dari jejak audit; **pesanan `Requested` dan `Confirmed` terbukti tetap dapat dibatalkan** |
| **Risiko/pemilik** | **Tinggi.** Ini satu-satunya pengetatan pada gelombang ini, dan ia mengenai endpoint yang sedang dipakai. Pemilik: Laboratorium |
| **DoD** | Kedua aturan menolak sesuai matriks; **`T-97b` membuktikan jalur yang sah tidak ikut tertutup**; hitungan pesanan berstatus `Accepted`/`InProcess`/`OnHold` dilaporkan sebelum aturan ditegakkan |

> **Butir yang wajib dikerjakan lebih dulu, bukan sesudah.** Sebelum `VAL-75` ditegakkan,
> hitung berapa pesanan hari ini berstatus `Accepted`, `InProcess`, atau `OnHold`, dan periksa
> apakah ada pemanggil yang masih membatalkannya. Mereka kehilangan jalur pembatalannya begitu
> aturan ini hidup — dan `BE-LAB-21` sudah menunjukkan bahwa pengetatan yang tidak dihitung
> dampaknya lebih mahal daripada pengetatan yang ditunda.

### `FE-LAB-15` — Kolom Konfirmasi dan pop-up konfirmasi

| Butir | Isi |
|---|---|
| **Outcome** | Petugas melihat mana yang belum terkonfirmasi, dan mengonfirmasi lewat satu pop-up berisi ringkasan pasien serta pilihan dokter pemeriksa |
| **Kontrak** | `LAB-API-v1` `r12` §7.1 |
| **Dependency** | `BE-LAB-31` |
| **Kewenangan UI** | Kolom Konfirmasi berisi `Belum Terkonfirmasi` sebelum konfirmasi, lalu **nama konfirmator beserta tanggal dan waktu** — tanpa label `Terkonfirmasi` tambahan. Tombol Konfirmasi **nonaktif** sesudah berhasil sekali. Nol kotak isian konfirmator: namanya datang dari server |
| **Acceptance criteria** | `AC-94`, `AC-95` |
| **DoD** | Pop-up menampilkan ringkasan, konfirmator, dan pemilih dokter; tombol nonaktif sesudah konfirmasi; kolom menampilkan nama dan waktu |

### `FE-LAB-16` — Pop-up pembatalan beralasan dan alert konfirmasi akhir

| Butir | Isi |
|---|---|
| **Outcome** | Petugas tidak dapat membatalkan pesanan tanpa menuliskan alasannya, dan tidak dapat membatalkan karena salah klik |
| **Kontrak** | `LAB-API-v1` `r12` §7.2 |
| **Dependency** | `BE-LAB-32` |
| **Kewenangan UI** | Pop-up **Batalkan Pemeriksaan** memuat isian Alasan Pembatalan wajib dan tombol `Lanjut Pembatalan`; sesudahnya muncul **alert konfirmasi akhir** sebelum permintaan dikirim. **Label tombol di dalam alert belum ditetapkan** dan tidak boleh direkayasa — tanyakan pemilik modul |
| **Acceptance criteria** | `AC-96`, `AC-97` |
| **DoD** | Alasan wajib ditegakkan di layar **dan** ditegakkan backend; aksi Batalkan tidak tampil pada pesanan yang sudah diproses |

### `FE-LAB-17` — Print membuka preview lebih dulu

| Butir | Isi |
|---|---|
| **Outcome** | Petugas melihat ringkasan order sebelum kertas keluar |
| **Requirement/decision** | `REC2-NEW-006` — kewenangan UI, nol kontrak backend |
| **Dependency** | — |
| **Kewenangan UI** | Tombol Print membuka preview; pencetakan dilakukan dari preview. Tanda tangan pembuat order, konfirmator, dan dokter pemeriksa tetap tampil |
| **DoD** | Print tidak lagi langsung mencetak; preview dapat dicetak |

### Yang **tidak** masuk gelombang ini

| Hal | Sebab |
|---|---|
| Status pembayaran mengunci tombol Proses (`LAB-DEC-062`) | Tertahan **`LAB-COORD-010`** — jalur baca milik Billing belum ada. Menulisnya sekarang berarti mengunci tombol berdasarkan nilai yang tidak dapat dibaca |
| Konfirmasi menjadi **wajib** sebelum `Accepted` | Tertahan **`LAB-OPEN-027`** — menuntut perlakuan atas pesanan yang sedang berjalan |
| Alert order baru 10 detik (`REC2-NEW-007`) | Kewenangan UI murni; belum diprioritaskan pemilik modul |

---

## 6e. Gelombang `MVP-5d` — Ruas Respons Konfirmasi

**Ditambahkan 2026-09-16**, menurunkan amandemen `LAB-API-v1` `r13` yang disetujui pemilik modul
pada tanggal yang sama.

**Kenapa gelombang ini ada, dan kenapa ia tidak diramalkan sebelumnya.** `BE-LAB-30` mendirikan
tiga kolom konfirmasi dan `BE-LAB-31` mengisinya; keduanya selesai dan terbukti. Yang tidak
terlihat sampai `BE-LAB-31` rampung adalah bahwa `r12` §7.1 mendefinisikan **badan permintaan**
saja — nilai yang sudah tersimpan tidak punya jalan keluar. Dua layar tertahan karenanya, dan
`AC-94` serta `AC-95` berhenti pada "terpenuhi sebagian".

**Satu task, seluruhnya aditif, nol migration.**

### `BE-LAB-33` ✅ — Ruas respons konfirmasi pada daftar dan detail pesanan

> **Status: `SELESAI` — 2026-09-16.** Seluruh butir DoD terpenuhi. Kelima ruas terbaca dari
> database pada jalur daftar maupun detail, dan pesanan yang belum dikonfirmasi terbukti
> mengembalikan kelimanya `null`. Sepuluh pemeriksaan, sepuluh `PASS`. Laporan lengkap:
> [`task/report/backend/BE-LAB-33.md`](../task/report/backend/BE-LAB-33.md).
>
> **Tiga jalur pembangun respons disentuh, dan isinya sengaja berbeda.** `ProyeksikanDaftarAsync`
> mengisi ketiga ruas daftar; `GetDetailAsync` mengisi kelimanya; `MapDetailResponse` mengisi
> ketiga ruas dari entity tetapi **membiarkan kedua namanya kosong** — mapper itu hanya dipakai
> tepat sesudah pesanan dibuat, dan pesanan yang baru lahir belum mungkin dikonfirmasi.
> Menerjemahkan dua penunjuk yang pasti kosong berarti dua perjalanan ke database untuk
> menghasilkan `null`. Alasannya ditulis sebagai komentar pada source.
>
> **Satu method berhenti menjadi `static`.** `ProyeksikanDaftarAsync` kini membutuhkan
> `DbContext` untuk menerjemahkan kedua nama **di dalam proyeksi yang sama**. Menerjemahkannya
> per baris sesudah proyeksi akan mengubah daftar 25 pesanan menjadi **51 perjalanan** ke
> database. Method itu privat, sehingga nol pemanggil di luar kelasnya terpengaruh.
>
> **Satu warning muncul dan diperbaiki, bukan dibiarkan:** komentar merujuk `RequestedByName`
> sebagai `cref` dari kelas **dasar**, padahal ruas itu tinggal di kelas turunannya. Build
> sempat 208 warning, lalu kembali ke baseline 207.
>
> **Satu pemeriksaan sengaja menguji ketiadaan:** baris lain pada daftar yang sama dipastikan
> **tidak** ikut terisi nama konfirmator. Tanpa itu, sub-query yang keliru mengikat dapat mengisi
> setiap baris dengan nama yang sama dan tetap terlihat benar pada baris pertama.

| Butir | Isi |
|---|---|
| **Outcome** | Nama konfirmator, waktu konfirmasi, dan dokter pemeriksa dapat dibaca layar — bukan hanya tersimpan di database |
| **Requirement/decision** | `LAB-DEC-061`; celah ditemukan `BE-LAB-31` |
| **Kontrak** | `LAB-API-v1` `r13` bagian 8 |
| **Reuse** | `LabOrderService.MapDetailResponse` dan `ResolveUserNameAsync` `Extend`. Pola `RequestedByName` diikuti apa adanya |
| **Cakupan** | Lima ruas DTO, satu penyusunan nama dokter, penyesuaian pemetaan. **Nol migration, nol endpoint baru, nol permission baru** |
| **Dependency** | `BE-LAB-30` ✅, `BE-LAB-31` ✅ |
| **Acceptance criteria** | Melengkapi `AC-94` dan `AC-95` — bagian "terbaca pada daftar" dan "tampil pada daftar serta ringkasan cetak" |
| **Verifikasi** | Build; kelima ruas terbaca dari database pada pesanan yang sudah dikonfirmasi; **pesanan yang belum dikonfirmasi mengembalikan kelimanya `null`**; nama konfirmator memakai jalur yang sama dengan `RequestedByName` |
| **Risiko/pemilik** | **Rendah.** Seluruhnya penambahan ruas; pembaca lama tidak terpengaruh. Pemilik: Laboratorium |
| **DoD** | Kelima ruas ada pada respons sesuai `r13`; nama siap tampil, bukan penunjuk; pesanan lama mengembalikan `null` tanpa galat; nol endpoint yang sudah ada berubah perilakunya |

> **Kenapa nama dan penunjuk dipisah tempatnya.** Nama ada di daftar karena layar tidak boleh
> menampilkan penunjuk (`no-uuid-display`), dan daftar yang hanya membawa penunjuk memaksa layar
> memanggil endpoint kedua per baris hanya untuk menerjemahkannya. Penunjuk ada di detail karena
> aksi lanjutan membutuhkan nilai yang dapat dikirim balik. Pembagian yang sama sudah berlaku
> untuk `RequestedByUserId` dan `RequestedByName` sejak `r3`.

**Yang dibuka task ini:** bagian tanda tangan `FE-LAB-17` ⛔, dan **sebagian** `FE-LAB-15` —
pop-up konfirmasinya, bukan kolomnya. Lihat `BE-LAB-34`.

### `BE-LAB-34` ✅ — Ruas konfirmasi pada daftar pantau

> **Status: `SELESAI` — 2026-09-16.** Ketiga ruas terbaca dari database lewat endpoint daftar
> pantau yang sebenarnya dipakai ketiga menu pemeriksaan; pesanan yang belum dikonfirmasi
> terbukti mengembalikan ketiganya `null`. Lima belas pemeriksaan, lima belas `PASS` — sebelas di
> antaranya pemeriksaan `BE-LAB-33` yang dijalankan ulang untuk memastikan jalur `r13` tidak
> tersentuh. Laporan: [`task/report/backend/BE-LAB-34.md`](../task/report/backend/BE-LAB-34.md).
>
> **`FE-LAB-15` kini nol penahan.**

| Butir | Isi |
|---|---|
| **Outcome** | Kolom Konfirmasi pada ketiga menu pemeriksaan punya nilai yang ditampilkannya |
| **Requirement/decision** | `LAB-DEC-061`; koreksi atas cakupan `BE-LAB-33` |
| **Kontrak** | `LAB-API-v1` `r14` bagian 9 |
| **Reuse** | Proyeksi `LabMonitoringService` `Extend`. Jalur terjemahan nama yang sama dengan `BE-LAB-33` |
| **Cakupan** | Tiga ruas DTO pada `LabMonitoringItemResponse` beserta pengisiannya. **Nol migration, nol endpoint baru, nol permission baru** |
| **Dependency** | `BE-LAB-30` ✅, `BE-LAB-31` ✅ |
| **Acceptance criteria** | Melengkapi `AC-94` dan `AC-95` bagian "terbaca pada daftar" |
| **Verifikasi** | Build; ketiga ruas terbaca dari database lewat endpoint daftar pantau; **pesanan yang belum dikonfirmasi mengembalikan ketiganya `null`**; nol ruas daftar pantau yang sudah ada berubah |
| **Risiko/pemilik** | **Rendah.** Penambahan ruas pada layar baca. Pemilik: Laboratorium |
| **DoD** | Ketiga ruas ada pada respons daftar pantau sesuai `r14`; nama siap tampil, bukan penunjuk; pesanan lama mengembalikan `null`; nol ruas lama berubah |

> **Kenapa task ini ada, dan ia pantas dibaca sebagai koreksi.** `BE-LAB-33` benar terhadap
> `r13` — dan `r13` menyebut DTO yang keliru. Usulnya disusun dengan membaca **apa yang
> dibutuhkan layar**, tetapi tanpa memeriksa **endpoint mana yang layar itu benar-benar panggil**.
> Ketiga menu pemeriksaan membaca grup `Lab Monitoring`, sedangkan
> `GET /lab-orders/by-discipline/{discipline}` yang menerima kelima ruas `r13` **nol dipakai
> frontend**. Pelajarannya dicatat pada kontrak bagian 9.6.

**Yang dibuka task ini:** `FE-LAB-15` sepenuhnya.

## 6f. Gelombang `MVP-5e` — Isi Pesanan yang Dapat Dibaca

Ditambahkan 2026-09-16. Satu task, menurunkan usul `LAB-API-v1` `r15`. Berdiri sesudah `MVP-5d`
karena celahnya baru terlihat ketika `FE-LAB-17` hendak dimulai: tabelnya berdiri sejak
`BE-LAB-26` dan terisi sejak `BE-LAB-27`, tetapi isinya tidak pernah dapat dibaca siapa pun di
luar backend.

### `BE-LAB-35` ✅ — Daftar pemeriksaan terpesan pada detail pesanan

> **Status: ✅ `SELESAI` — 2026-09-16.** `LAB-API-v1` `r15` disetujui dan dilaksanakan pada hari
> yang sama ia diusulkan. Kelima butir DoD terpenuhi dan **terbukti dari database**: 12
> pemeriksaan, 12 `PASS`. Laporan:
> [`task/report/backend/BE-LAB-35.md`](../task/report/backend/BE-LAB-35.md).
>
> **Empat pemeriksaan sengaja menguji ketiadaan, bukan keberadaan.** Baris ber-`IsDelete` tidak
> ikut terbaca; pesanan lama mengembalikan array **kosong** dan bukan `null`; daftar terpesan
> tidak ikut menimpa ruas wakil; dan DTO-nya **nol memuat penunjuk**, diperiksa lewat refleksi.
> Tanpa yang ketiga, proyeksi yang keliru mengikat dapat menimpa `ProcedureName` wakil dan tetap
> terlihat benar pada pandangan pertama.
>
> **Satu pemeriksaan dirancang agar gagal bila implementasinya salah:** nama snapshot baris uji
> sengaja dibuat berbeda dari nama katalognya. Proyeksi yang menoleh ke `MstProcedure` akan
> mengembalikan nama katalog, dan pemeriksaan itu akan menangkapnya.
>
> **Satu temuan yang wajib diketahui `FE-LAB-17` sebelum dimulai:** `LabOrderedProcedure` berisi
> **0 baris** pada database. Seluruh 5 pesanan nyata hari ini karena itu menempuh jalur **array
> kosong**, dan layar cetak wajib menangani jalur itu dengan benar — bukan memperlakukannya
> sebagai data rusak.
>
> **Kenapa task ini ada.** `BR-47` menetapkan pemeriksaan sedisiplin **berkumpul pada satu
> pesanan**, dan `LabOrderedProcedure` menyimpan daftarnya sejak `BE-LAB-26`. Yang tidak pernah
> dikerjakan adalah mengembalikannya: **nol DTO dan nol endpoint** memuat daftar itu — pencarian
> `ProcedureNameSnapshot` pada seluruh area `LaboratoryManagement` menghasilkan nol kemunculan.
> Sementara itu `LabOrder.ProcedureId` hanyalah **penunjuk wakil**, dinyatakan oleh komentar
> kodenya sendiri.
>
> **Kenapa celah itu tidak memutus apa pun selama dua hari.** Ketiga menu pemeriksaan adalah
> layar **antrean** dan memang tidak menampilkan isi pesanan; sedangkan `VAL-68` dan `VAL-69`
> membaca tabel itu **di dalam backend**, sehingga kedua aturan tetap tegak tanpa satu pun ruas
> respons. Yang pertama membutuhkannya adalah konsumen yang **mencetak**.
>
> **Yang ditahan olehnya:** `FE-LAB-17`. Pemilik modul memilih **menunda** layar itu sampai
> amandemen ini jalan, bukan menurunkannya menjadi versi sebagian — karena versi sebagian berarti
> dokumen resmi yang menyebut satu pemeriksaan padahal pesanannya memuat beberapa.

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-16 |
| **Outcome** | Konsumen dapat mengetahui pemeriksaan apa saja yang benar-benar dipesan pada sebuah pesanan |
| **Requirement/decision** | `BR-47`, `LAB-DEC-055`; keputusan pemilik modul 2026-09-16 atas cakupan `FE-LAB-17` |
| **Kontrak** | `LAB-API-v1` `r15` bagian 10 — **`approved` 2026-09-16** |
| **Reuse** | `LabOrderService.GetDetailAsync` `Extend`. Tabel, keempat kolom snapshot, dan endpointnya sudah ada |
| **Cakupan** | Satu DTO baru `LabOrderedProcedureResponse` berisi empat ruas snapshot, satu ruas `orderedProcedures` pada `LabOrderDetailResponse`, dan proyeksinya. **Nol migration, nol endpoint baru, nol permission baru** |
| **Dependency** | `BE-LAB-26` ✅, `BE-LAB-27` ✅, persetujuan `r15` ✅ — **nol penahan tersisa** |
| **Acceptance criteria** | Melengkapi prasyarat `FE-LAB-17`; tidak menambah AC baru |
| **Verifikasi** | Build; daftar terbaca **dari database** untuk pesanan berpemeriksaan jamak, dan urutannya sesuai penyimpanan; **pesanan lama tanpa baris terpesan mengembalikan array kosong, bukan galat**; nol ruas `LabOrderDetailResponse` yang sudah ada berubah; nol `procedureId` dikirim |
| **Risiko/pemilik** | **Rendah.** Penambahan ruas baca pada endpoint yang sudah dipakai. Pemilik: Laboratorium |
| **DoD** | `orderedProcedures` ada pada detail sesuai `r15`; keempat ruasnya dibaca dari kolom **snapshot**, bukan katalog hari ini; pesanan lama mengembalikan array kosong; nol penunjuk dikirim; nol ruas lama berubah |

**Yang akan dibuka task ini:** `FE-LAB-17`, yang sesudahnya nol penahan.

---

## 6g. Gelombang `MVP-5f` — Nomor Order yang Dapat Disebut Manusia

Ditambahkan 2026-09-17. Menurunkan `LAB-DEC-072`, yang menutup `LAB-OPEN-033` pada 2026-09-16
tetapi **sengaja tidak langsung diturunkan menjadi task** — rekonsiliasi bukti putaran 3 mencatat
bahwa perancangannya belum ada, dan bahwa meniru pola acuannya tanpa meniru kelemahannya adalah
pekerjaan perancangan, bukan detail implementasi.

Perancangan itu dikerjakan pada 2026-09-17 dan hasilnya ada di bawah.

> **Kenapa gelombang ini boleh berjalan sementara `S17` tertahan.** `LAB-DEC-072` sudah
> `approved`, dan kolom nomor order **tidak** bergantung pada hasil pemeriksaan. Ketiga penahan
> modul — `LAB-SIGN-001`, `LAB-REQ-007`, `LAB-COORD-010` — seluruhnya mengunci `S4` dan `S17`,
> dan **nol menyentuh** pekerjaan ini. Yang tertahan adalah **pemakaian** nomor itu pada Menu
> Hasil dan Label Lab, bukan keberadaannya.

### Perancangan — empat keputusan sebelum satu baris kode ditulis

#### 1. Bentuk nomornya

`LAB-RSMMC-000001`, mengikuti pola `ENC-RSMMC-00001` milik `PatientEncounterNumberService`:
awalan tetap, nomor urut berpadding.

**Enam digit, bukan lima.** Pola acuannya memakai lima, cukup untuk 99.999 kunjungan. Pesanan
laboratorium bertambah jauh lebih cepat daripada kunjungan — satu kunjungan dapat melahirkan
beberapa pesanan sekaligus sejak `BR-47` memecah per disiplin. Lima digit adalah utang yang
jatuh temponya tidak terlihat sampai ia jatuh.

#### 2. Nomor yang sudah terpakai **tidak pernah** dipakai ulang

Inilah selisih paling penting terhadap pola acuannya, dan alasannya bukan performa.

`AllocateEncounterNumberAsync` memuat seluruh nomor terpakai ke memori lalu **memindai celah
pertama** — artinya nomor bekas baris yang hilang akan diberikan kepada baris baru. Untuk nomor
yang **dicetak pada amplop hasil pasien**, perilaku itu berbahaya: amplop lama bernomor
`LAB-RSMMC-000042` dapat berada di tangan pasien A, sementara nomor yang sama diberikan kepada
pasien B. Dua benda fisik, satu nomor, dan tidak ada yang melihat kesalahannya.

Alokasi karena itu memakai **`MAX + 1`**, bukan pemindaian celah. Celah **dibiarkan ada** dan itu
disengaja.

> **Batas jaminannya dikoreksi 2026-09-17, sesudah diuji.** Perancangan ini semula menyatakan
> nomor "tidak pernah dipakai ulang". Pernyataan itu **terlalu kuat**. `MAX + 1` menjamin nomor
> tidak kembali **selama barisnya tetap ada di tabel** — termasuk baris ber-`IsDelete`, yang tetap
> terbaca agregatnya. Penghapusan **fisik** atas baris bernomor tertinggi mengembalikan nomornya,
> dan itu dibuktikan saat `BE-LAB-36` diuji: empat pesanan uji dihapus lewat SQL, lalu pesanan
> berikutnya memperoleh `LAB-RSMMC-000009` kembali.
>
> **Dalam pemakaian aplikasi, keadaan itu tidak terjadi.** Penelusuran seluruh area Laboratorium
> menghasilkan **nol** `Remove`, **nol** endpoint `DELETE` pada `LabOrderController`, dan
> pembatalan hanya memindahkan status. Yang dapat mengembalikan sebuah nomor hanyalah penghapusan
> fisik dari luar aplikasi. Dicatat sebagai batas yang diketahui, bukan sebagai jaminan yang
> dilebihkan.

#### 3. Biayanya satu agregat, bukan seluruh tabel

| | Pola acuan | Yang dipakai di sini |
|---|---|---|
| Baris yang dimuat ke memori | **Seluruhnya** | **Nol** |
| Bentuk kueri | `SELECT` seluruh kode, lalu `HashSet` dan pemindaian di aplikasi | Satu `SELECT MAX(...)` |
| Biaya seiring pertumbuhan | Tumbuh linear | Tetap, dan dapat diindeks |

Bagian angkanya diambil dengan `SUBSTRING` lalu di-`CAST` ke `BIGINT`, dan barisnya disaring
`~ '^LAB-RSMMC-[0-9]+$'` lebih dulu. Penyaring itu bukan hiasan: tanpanya, satu baris berformat
lain membuat `CAST` gagal dan **seluruh** pembuatan pesanan berhenti.

> **Satu alternatif yang ditolak dan alasannya dicatat.** `MAX` atas kolom teksnya langsung lebih
> murah dan tetap benar selama lebarnya seragam — tetapi ia **pecah diam-diam** pada digit ke-7:
> `LAB-RSMMC-1000000` berurutan **sebelum** `LAB-RSMMC-999999` secara leksikografis, sehingga
> nomor berikutnya akan mundur dan bertabrakan. Kegagalannya tidak menimbulkan galat pada hari ia
> terjadi; ia hanya mulai memberi nomor yang salah.

#### 4. Alokasi **berblok**, bukan satu per satu

Ini temuan perancangan yang paling mudah terlewat, dan akibatnya senyap.

`POST /lab-orders/by-examinations` membentuk **beberapa pesanan sekaligus** dalam satu
`SaveChangesAsync`. Entity yang belum tersimpan **tidak terlihat** oleh kueri SQL mentah, sehingga
memanggil alokasi satu per satu di dalam transaksi yang sama akan mengembalikan **nomor yang sama
berulang kali** — lalu ditolak index unik, dan seluruh permintaan gagal.

Layanannya karena itu menyediakan `AllocateAsync(count)` yang mengembalikan `count` nomor
berurutan sekaligus.

#### 5. Kunci konkurensinya hanya berarti di dalam transaksi

`pg_advisory_xact_lock` dilepas saat transaksi berakhir. Bila alokasi dipanggil di luar transaksi
eksplisit, ia memperoleh dan melepas kuncinya **seketika**, dan kuncinya tidak menjaga apa pun.

Pemanggil pola acuannya sudah benar — `PatientEncounterController` membungkusnya
`BeginTransactionAsync`. `LabOrderService` **tidak punya transaksi eksplisit** pada kedua jalur
pembuatannya hari ini, sehingga keduanya wajib dibungkus.

**Index unik tetap menjadi jaring pengaman terakhir.** Kunci mengurangi tabrakan; index yang
membuatnya mustahil.

### `BE-LAB-36` ✅ — Kolom nomor order beserta layanan alokasinya

> **Status: ✅ `SELESAI` — 2026-09-17.** Seluruh butir DoD terpenuhi dan **terbukti terhadap
> database**: 9 pemeriksaan, seluruhnya `PASS`. Laporan:
> [`task/report/backend/BE-LAB-36.md`](../task/report/backend/BE-LAB-36.md).
>
> **Pemeriksaan yang paling menentukan dirancang agar gagal bila implementasinya salah.**
> `POST /by-examinations` lintas tiga disiplin memperoleh `000010`, `000011`, `000012` — tiga
> nomor **berbeda dan berurutan** dari satu panggilan. Alokasi per pesanan, bentuk yang paling
> wajar ditulis orang, akan memberi nomor yang sama tiga kali lalu ditolak index unik.
>
> **Jalur `Down` lalu `Up` membuktikan lebih dari sekadar dapat dibalik:** penomorannya
> **identik** pada penjalanan kedua. Tanpa pemecah seri `Id`, tiga baris berwaktu identik akan
> memperoleh nomor berbeda setiap kali.
>
> **Satu klaim perancangan dikoreksi sesudah diuji, dan koreksinya dibawa ke kode maupun
> roadmap.** Lihat catatan batas jaminan pada bagian perancangan di atas.
>
> **Database kembali persis seperti semula** — 8 pesanan, `000001`..`000008`, nol baris uji
> tersisa, diperiksa dari koneksi baru.

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-17 |
| **Outcome** | Setiap pesanan laboratorium punya nomor yang dapat dibaca, dicetak, dan **disebut lewat telepon** |
| **Requirement/decision** | `LAB-DEC-072` (`approved` 2026-09-16); menutup `LAB-OPEN-033` |
| **Kontrak** | **Nol perubahan kontrak pada task ini.** Menampilkan nomornya menuntut `LAB-API-v1` `r16`, yang diusulkan pada laporan task ini dan **belum disetujui** |
| **Cakupan** | Satu kolom `OrderNumber` pada `LabOrder`; satu `LabOrderNumberService`; satu index unik; satu migration yang **mengisi 5 pesanan lama** lalu menjadikan kolomnya `NOT NULL`; kedua jalur pembuatan dibungkus transaksi eksplisit |
| **Dependency** | Nol. Tidak menunggu task mana pun |
| **Acceptance criteria** | Tidak menambah AC baru; melengkapi prasyarat Label Lab dan kolom `No. Order` pada `BR-50` |
| **Verifikasi** | Migration jalan **maju dan mundur**; seluruh pesanan lama memperoleh nomor berurutan menurut `CreateDateTime`; pesanan baru memperoleh nomor berikutnya; `POST /by-examinations` yang membentuk 3 pesanan memperoleh **3 nomor berbeda dan berurutan**; index unik menolak duplikat; **nol baris dimuat ke memori** dibuktikan dari bentuk kuerinya |
| **Risiko/pemilik** | **Sedang.** Kolom `NOT NULL` pada tabel yang sudah berisi, dan dua jalur tulis yang dibungkus transaksi. Pemilik: Laboratorium |
| **DoD** | Kolom berdiri `NOT NULL` dan unik; seluruh pesanan lama terisi; kedua jalur pembuatan mengalokasikan nomor; alokasi berblok terbukti pada jalur `by-examinations`; celah **tidak** diisi ulang, beserta batas jaminannya yang dilaporkan apa adanya; jalur `Down` terbukti; usul `r16` ditulis lengkap pada laporan |

> **Kenapa `NOT NULL`, padahal `BE-LAB-30` memilih `nullable`.** Ketiga kolom konfirmasi memang
> boleh kosong — pesanan yang belum dikonfirmasi tidak punya konfirmator. Nomor order **tidak**
> punya keadaan "belum": setiap pesanan punya satu sejak lahir. Kolom `nullable` di sini hanya
> akan menyembunyikan jalur tulis yang lupa mengalokasikan, dan modul ini sudah tiga kali
> tertimpa kelas kesalahan yang sama — sesuatu yang berdiri tanpa terisi dan tidak menimbulkan
> galat apa pun sampai seseorang membutuhkannya. **Jalur tulis `LabOrder` terbukti tepat dua**,
> keduanya di `LabOrderService`, dan keduanya disentuh task ini.

**Yang akan dibuka task ini:** Label Lab dan kolom `No. Order` pada `BR-50` — keduanya tetap
tertahan `LAB-SIGN-001` untuk sebab lain, tetapi **bahan nomornya tidak lagi menjadi penahan**.

---

## 6h. Gelombang `MVP-5g` — `r16` dan `r17` dilaksanakan

Ditambahkan 2026-09-17, sesudah pemilik modul menyetujui kedua amandemen pada hari yang sama.

### `BE-LAB-37` ✅ — `r16`: nomor order terbaca dan waktu pengambilan yang dinyatakan

> **Status: ✅ `SELESAI` — 2026-09-17.** 5 pemeriksaan runtime, seluruhnya sesuai. Laporan:
> [`task/report/backend/BE-LAB-37.md`](../task/report/backend/BE-LAB-37.md).
>
> **Temuan yang menghemat pekerjaan:** `VAL-59` **sudah terimplementasi dengan benar** pada
> `ResolvePhysicalReceipt` sejak `BE-LAB-22`; yang kurang hanya pembandingnya, yang diteruskan
> sebagai `collectedAt: null` secara harfiah. Aturannya ada, kodenya benar, datanya yang tidak
> pernah datang.
>
> **Satu batas dilaporkan:** `collectedAt` dipakai sebagai **pembanding**, belum disimpan.
> Menyimpannya ke `LabSpecimen.CollectedAt` akan **ditimpa** tindakan pengambilan — justru
> menghapus data yang hendak diselamatkan. Tiga pilihan jalan keluarnya ada pada laporan §6.

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-17 |
| **Outcome** | Nomor order dapat dibaca konsumen mana pun, dan `VAL-59` dapat ditegakkan penuh |
| **Requirement/decision** | `LAB-DEC-072`; **`LAB-CONFLICT-006` pilihan A**, diputuskan 2026-09-16 |
| **Kontrak** | `LAB-API-v1` **`r16`** bagian 11 — **`approved` 2026-09-17** |
| **Reuse** | Proyeksi `LabOrderService` dan `LabMonitoringService` yang sudah ada — `Extend`, bukan jalur baru |
| **Cakupan** | Ruas `orderNumber` pada `LabOrderListResponse` dan `LabMonitoringItemResponse`; ruas `collectedAt` opsional pada `PlanLabSpecimenRequest` beserta penegakan `VAL-59`. **Nol migration, nol endpoint baru, nol permission baru** |
| **Dependency** | `BE-LAB-36` ✅ — kolomnya sudah berdiri |
| **Acceptance criteria** | `AC-66` menjadi terpenuhi **penuh**; melengkapi prasyarat `FE-LAB-12` |
| **Verifikasi** | `orderNumber` terbaca **dari database** pada kedua jalur; `collectedAt` yang dikirim tersimpan dan `VAL-59` terbukti menolak sesuai matriks; **pemanggil lama yang tidak mengirim `collectedAt` terbukti tetap diterima**; nol ruas lama berubah |
| **Risiko/pemilik** | **Rendah.** Seluruhnya aditif. Pemilik: Laboratorium |
| **DoD** | Kedua ruas respons terbaca dari database; `collectedAt` tersimpan; `VAL-59` tegak beserta pesannya yang dibandingkan kata demi kata terhadap matriks; muatan lama tetap diterima |

> **Satu hal yang wajib diperiksa, bukan diasumsikan.** `VAL-59` membandingkan waktu penerimaan
> fisik terhadap waktu pengambilan. Sesudah `r16`, pembandingnya ada — tetapi **hanya ketika
> pemanggil mengirimnya**. Ketika `collectedAt` kosong, aturannya **tidak menyala**, dan itu sah:
> yang dilarang adalah membandingkan terhadap cap waktu server yang justru lebih akhir. Perilaku
> "tidak menyala" itu wajib **dibuktikan sengaja**, bukan tertinggal sebagai cabang yang tidak
> pernah diuji.

### `BE-LAB-38` ✅ — `r17`: daftar penerimaan lintas pesanan

> **Status: ✅ `SELESAI` — 2026-09-17.** 6 pemeriksaan runtime, 6 `PASS`. Laporan:
> [`task/report/backend/BE-LAB-38.md`](../task/report/backend/BE-LAB-38.md).
>
> **Butir DoD yang ditebalkan dibuktikan DUA ARAH atas satu wadah yang sama:** wadah yang tiba
> 15 Sep dan dicatat 17 Sep **muncul** pada rentang 15 Sep, dan **tidak muncul** pada rentang
> 17 Sep. Implementasi yang keliru memakai `CreateDateTime` akan memberi hasil terbalik persis,
> dan implementasi yang menyaring pada kedua kolom sekaligus akan lolos tanpa pemeriksaan kedua.
>
> **Jalur cadangan ikut teruji:** kelima wadah lama ber-`PhysicallyReceivedAt` `null` terbaca
> pada rentang tanggal pencatatannya.

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** — 2026-09-17 |
| **Outcome** | Petugas dapat menelusuri penerimaan yang sudah dicatat, lintas pesanan, menurut waktu kedatangan sebenarnya |
| **Requirement/decision** | `FR-11.5`, `LAB-DEC-042`; `AC-67` |
| **Kontrak** | `LAB-API-v1` **`r17`** bagian 12 — **`approved` 2026-09-17** |
| **Reuse** | Bentuk `LabOrderPagedQuery` beserta proyeksi pagingnya yang sudah berjalan sejak `r5`; penyaring rentang **disalin polanya dari `GetSummaryAsync`** |
| **Cakupan** | Satu endpoint `GET /lab-specimens`, satu query DTO, satu response DTO, satu proyeksi. **Nol migration, nol permission baru** |
| **Dependency** | `BE-LAB-37` — ruas `orderNumber` pada responsnya berasal dari sana |
| **Acceptance criteria** | `AC-67` pada sisi daftar; melengkapi prasyarat `FE-LAB-12` |
| **Verifikasi** | **Wadah yang tiba Senin malam dan dicatat Selasa pagi terbukti muncul pada rentang hari Senin**; `createDateTime` ikut terbaca sehingga selisihnya dapat dihitung; paging dan penyaring status terbukti; baris ber-`IsDelete` tidak ikut |
| **Risiko/pemilik** | **Rendah pada kode, tinggi bila penyaringnya keliru.** Pemilik: Laboratorium |
| **DoD** | Endpoint berdiri sesuai `r17`; **rentang disaring pada `PhysicallyReceivedAt ?? CreateDateTime`, dibuktikan dengan wadah yang kedua waktunya berbeda hari**; kelima ruas tambahan terbaca; nol endpoint lama berubah |

> **Butir DoD yang ditebalkan itu adalah seluruh isi task ini.** Bila penyaringnya keliru memakai
> `CreateDateTime` saja, endpointnya tetap berjalan, tetap mengembalikan baris, dan tetap terlihat
> benar — hanya tanggalnya yang salah. Kegagalan yang tidak menimbulkan galat tidak akan
> ditemukan siapa pun sampai ada yang membandingkannya dengan kertas.

**Yang akan dibuka gelombang ini:** `FE-LAB-12`, dan dengan itu `MVP-5a` tuntas.

---

## 6h-B. `BE-LAB-41` ⛔ — Penyimpanan pengiriman hasil ke pasien

> **Dinomori ulang 2026-09-22.** Bagian ini sebelumnya bernomor `6h`, sama dengan gelombang
> `MVP-5g` di atasnya — dua bagian bernomor sama pada satu dokumen 3.800 baris. Ditemukan oleh
> audit kesiapan. **Yang diubah hanya bagian ini**, sebab ketiga rujukan silang yang ada
> (`backend-roadmap` baris 81, laporan `BE-LAB-37`, laporan `BE-LAB-38`) seluruhnya menunjuk
> `6h` yang pertama; bagian ini nol dirujuk siapa pun.

> **Status: ⛔ `TERTAHAN` — dirancang 2026-09-17, sengaja belum dilaksanakan.**
>
> **Rancangannya lengkap** dan ada pada
> [`02-backend-architecture.md`](../02-backend-architecture.md) bagian 13. Yang menahannya bukan
> perancangan, melainkan **dua sisi yang sama-sama kosong**.
>
> **Kenapa tidak dikerjakan sekarang, walaupun kodenya kecil.** Tabel ini hari ini **nol punya
> penulis** — pengirimannya tertahan `LAB-COORD-011`, gerbang pesan dan pembangkit PDF keduanya
> nol pada platform — **dan nol punya pembaca**, karena kolom `Terkirim ke Pasien` adalah kolom
> Datatable Hasil, yaitu `S17`, yang berdiri di atas hasil yang **sudah dirilis**. `LAB-SIGN-001`
> yang dahulu menahannya ditutup 2026-09-17, tetapi `S4` baru boleh **dirancang** — nol hasil
> dirilis hari ini, sehingga pembacanya tetap nol.
>
> **Modul ini sudah membayar harga persis kesalahan itu.** `BE-EXT-04` mendirikan dua kolom
> tanpa jalur tulisnya; 16 dari 16 sesi bernilai `null`, dan `BE-EXT-04b` harus dibuat menyusul
> untuk menutupnya sementara dua task tertahan. Mendirikan tabel ini sekarang mengulangnya
> dengan **kedua sisi** kosong sekaligus — yang tertinggal hanya satu tabel kosong beserta
> migration yang perlu dirawat, untuk kemampuan yang nol dapat dipakai siapa pun.
>
> **Tiga keputusan perancangan yang sudah diambil dan tidak perlu diulang:** counter adalah
> **turunan** dari log, bukan kolom — sehingga angkanya tidak dapat melenceng, dan kegagalan
> tetap meninggalkan jejak walaupun tidak menambah angka; nomor tujuan disimpan sebagai
> **snapshot**, karena nomor pasien yang berubah akan mengubah arti catatan lama tanpa satu pun
> baris disunting; dan **status ketiga sengaja tidak dirancang**, karena ada-tidaknya `Queued`
> ditentukan bentuk gerbang yang belum ada.

| Butir | Isi |
|---|---|
| **Status** | ⛔ **`TERTAHAN`** — rancangan selesai 2026-09-17; nol baris source, nol migration |
| **Outcome** | Setiap pengiriman hasil ke pasien terekam beserta pelaku, waktu, nomor tujuan, dan sebab kegagalannya; angka `Terkirim ke Pasien` diturunkan darinya |
| **Requirement/decision** | `LAB-DEC-066`, `LAB-DEC-067`; `BR-50` butir 6; `RULE-014`, `RULE-016` |
| **Kontrak** | Belum. Ruas counter pada respons dan jalur kirim lahir bersama endpointnya |
| **Reuse** | Pola sub-query pencacahan `SpecimenCount`/`AcceptedSpecimenCount`; pola snapshot `ProcedureNameSnapshot` |
| **Cakupan** | Satu entity `LabResultDelivery`, dua enum, satu configuration, satu migration. **Tanpa** endpoint dan **tanpa** pengirim |
| **Dependency** | **`LAB-COORD-011`** (penulis) dan **`S17`** (pembaca). **Diperbarui 2026-09-17:** `LAB-SIGN-001` yang dahulu menahan `S17` sudah **ditutup** (`LAB-DEC-079`), tetapi sisi pembacanya **belum bergerak** — `S17` menampilkan hasil yang dirilis, dan `S4` baru boleh **dirancang**, belum dibangun. Penahan penulisnya pun utuh. **Alasan menundanya karena itu tidak berubah sedikit pun** |
| **Acceptance criteria** | Diturunkan dari `BR-50` butir 6 ketika slice dibuka |
| **Risiko/pemilik** | Rendah pada kodenya. **Sedang pada waktunya** — dibangun terlalu dini ia mengulang `BE-EXT-04` |
| **DoD** | Log terisi dari jalur kirim yang **benar-benar ada**; counter terbukti cocok dengan isi log; `RequestedByUserId` menulis `null` bukan `Guid.Empty` (peringatan `BE-EXT-05`) |

**Satu peringatan dibawa dari `BE-EXT-05`.** `RequestedByUserId` ber-foreign key ke `AspNetUsers`
dan nullable. Pelaksananya wajib menulis **`null`** ketika pelakunya bukan orang — `Guid.Empty`
melanggar FK, dan kegagalannya **tidak terlihat dari mana pun kecuali log**.

---

## 6i. Gelombang `MVP-6` — Pengisian hasil Mikrobiologi dan Patologi Anatomi

Menurunkan `EPIC-LAB-13` pada [`04-prd-to-mvp.md`](../04-prd-to-mvp.md) bagian 18, dan
[`02-backend-architecture.md`](../02-backend-architecture.md) bagian 14.

| Field | Nilai |
|---|---|
| Kontrak yang berlaku | `LAB-API-v1` **`r24`**, `LAB-VAL-v1` **`r7`**, `LAB-PERM-v1` **rev 6** — ketiganya `approved` 2026-09-18 |
| Kesiapan requirement | `READY_FOR_DOMAIN_DESIGN` (`LAB-RCG-001-r7`) |
| Kesiapan arsitektur domain | `DOMAIN_ARCHITECTURE_READY` (`LAB-DA-001` rev 6) |
| Backend SHA saat direncanakan | `5ee03294` |

**Urutan gelombang sudah diputuskan dan berbasis risiko, bukan abjad:**

```text
MVP-6a  BE-LAB-44  data induk        ──┐
        BE-LAB-45  enum + kolom      ──┼── keduanya prasyarat
                                       │
MVP-6b  BE-LAB-46  hasil PA          ←─┘   (butuh BE-LAB-45 saja)
                                       
MVP-6c  BE-LAB-47  tabel Mikro       ←─┘   (butuh BE-LAB-44 + BE-LAB-45)
        BE-LAB-48  jalur hasil Mikro
        BE-LAB-49  pembuktian index parsial
```

> **`MVP-6b` sengaja mendahului `MVP-6c`.** Patologi Anatomi nol bergantung pada data induk mana
> pun, sehingga ia dapat selesai penuh tanpa menunggu siapa pun mengisi daftar organisme.
> Mikrobiologi tidak bisa — layar yang jadi tetapi daftarnya kosong adalah layar yang **tidak
> dapat dipakai**, dan itu keadaan yang sudah dua kali terjadi di modul ini lewat
> `LAB-COORD-006` dan `MST-POS-WRITE`.

> **Dua penahan dibawa ke seluruh task gelombang ini, dan keduanya bukan pekerjaan pelaksana:**
> `LAB-SRC-UNCOMMITTED` — `S4a` yang menjadi pola acuan **nol ada pada commit mana pun**,
> sehingga pelaksana wajib membaca polanya dari working tree, bukan dari Git; dan `LAB-RDY-C04` —
> bukti uji backend tidak dapat dijalankan ulang dari repository, sehingga verifikasi setiap task
> di bawah **wajib berupa pemeriksaan sungguhan terhadap database**, bukan berkas uji yang tidak
> dapat diperiksa siapa pun.

### 6i.1 `BE-LAB-44` — Dua data induk: `LabOrganism` dan `LabAntibiotic`

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-18 — gelombang `MVP-6a`. Dua tabel berdiri, delapan endpoint berjalan, migration **diterapkan**. `AC-98`..`AC-100` terbukti pada database **dan** terhadap aplikasi yang berjalan. **Satu selisih cakupan dilaporkan**: roadmap menulis "tanpa service", tetapi `BACKEND_ENGINEERING_CONTRACT.md` menetapkan Controller → Module Service → DbContext dan berada di atas roadmap pada urutan wewenang. Lihat [`BE-LAB-44.md`](../task/report/backend/BE-LAB-44.md) |
| **Outcome** | Kepala instalasi dapat menambah, mengubah, dan menonaktifkan organisme serta antibiotik lewat aplikasi — **bukan lewat SQL langsung** |
| **Requirement/decision** | `FR-13.6`, `FR-13.7`; `LAB-DEC-084`; `LAB-DC-041`, `LAB-DC-042` |
| **Kontrak** | `LAB-API-v1` `r24` bagian 19.4; `LAB-PERM-v1` rev 6 bagian 8.2; `LAB-VAL-v1` `r7` (`VAL-91`) |
| **Reuse** | Pola `LabSpecimenType` sepenuhnya — prefix `Lab`, di dalam submodul, `IsActive` bukan hapus, unique parsial pada kode |
| **Cakupan** | Dua model, dua configuration, satu migration, dua controller (8 endpoint). CRUD sederhana memakai `ApplicationDbContext` langsung sesuai konvensi — **tanpa** service |
| **Dependency** | Nol. Ini task paling hulu gelombang ini |
| **Acceptance criteria** | `AC-98` kode organisme dan antibiotik unik dan ditolak bila berulang (`VAL-91`); `AC-99` baris dinonaktifkan **tidak hilang** dan tetap terbaca; `AC-100` **nol endpoint `DELETE`** tersedia pada kedua resource |
| **Verifikasi** | Pemeriksaan sungguhan terhadap database: dua tabel berdiri, index unik parsial pada kedua kode, dan `POST` berulang dengan kode sama menjawab `409` |
| **Risiko/pemilik** | **Rendah pada kodenya, sedang pada kelanjutannya.** Tabel berdiri tidak berarti terisi — lihat 6i.7 |
| **DoD** | Kedua tabel berdiri; kedelapan endpoint berjalan; `AC-98`..`AC-100` terbukti; **nol `DELETE`** |

### 6i.2 `BE-LAB-45` — Perluasan bentuk hasil dan empat kolom `LabExamination`

| Butir | Isi |
|---|---|
| **Status** | ⛔ **`DILEBUR`** ke `BE-LAB-53` pada 2026-09-21 — digantikan, bukan dibatalkan. Lihat bagian 6m. *(status diperbaiki 2026-09-22; blok ini sempat tertinggal `SIAP DIKERJAKAN` sesudah peleburannya)* |
| **Outcome** | Sistem mengenal empat bentuk hasil, dan `LabExamination` punya tempat bagi status temuan serta ketiga ruas narasi Patologi Anatomi |
| **Requirement/decision** | `LAB-DEC-027` (BR-23); `LAB-DEC-080`; `LAB-DC-040` |
| **Kontrak** | `LAB-API-v1` `r24`; `02-backend-architecture.md` bagian 14.9 dan 14.10 |
| **Reuse** | Pola kolom hasil `S4a` pada entity yang sama |
| **Cakupan** | ⚠ **DIPERSEMPIT 2026-09-18 sore.** `LabResultForm` bertambah dua nilai; dua enum baru (`LabMicrobiologyFinding`, `LabSusceptibilityResult`); **SATU** kolom nullable pada `LabExamination` — `MicrobiologyFinding` saja; satu migration. **Ketiga kolom `Pathology*` DICABUT** oleh `LAB-DEC-085` yang memindahkan hasil PA ke tingkat pesanan — lihat `02-backend-architecture.md` bagian 15.1. **Pencabutan ini nol biaya karena task belum dikerjakan**; bila sudah, yang tertinggal tiga kolom `varchar(4000)` tanpa penulis dan tanpa pembaca, persis `BE-EXT-04` |
| **Dependency** | Nol |
| **Acceptance criteria** | `AC-101` `Numeric = 1` dan `Choice = 2` **tidak bergeser**, sehingga baris `LabValueBound` yang sudah ada nol terdampak; `AC-102` keempat kolom nullable dan migration berjalan pada tabel berisi data **tanpa menulis ulang satu baris pun**; `AC-103` **nol kolom status hasil bertambah** pada `LabExaminationStatus` |
| **Verifikasi** | Pemeriksaan database: nilai enum lama utuh, keempat kolom ada dan nullable, `LabExaminationStatus` masih berisi **empat** nilai |
| **Risiko/pemilik** | Rendah. Seluruhnya aditif |
| **DoD** | Migration berjalan dan dapat dimundurkan; `AC-101`..`AC-103` terbukti |

> **`AC-103` berbentuk ketiadaan, dan itu disengaja.** Menambahkan `Validated`/`Released` ke
> `LabExaminationStatus` adalah hal yang paling mungkin dilakukan pelaksana dengan niat baik —
> enum itu terasa belum lengkap. `LAB-DEC-080` menolaknya, dan `INV-29` menegakkannya.

### 6i.3 `BE-LAB-46` — Jalur pengisian hasil Patologi Anatomi

| Butir | Isi |
|---|---|
| **Status** | ❌ **`DIBATALKAN` 2026-09-18** — *(dikoreksi 2026-09-23; blok ini tertinggal `DIBEKUKAN`)*. Ia sempat dibekukan rekonsiliasi bukti putaran 4 (`LAB-EVD-003`) pagi itu, lalu **dibatalkan pada hari yang sama** dan digantikan `BE-LAB-50`, `BE-LAB-51`, `BE-LAB-52` — lihat bagian 6i.6b, yang sudah menuliskannya sejak 2026-09-18. **Blok ini yang tertinggal, bukan keputusannya.** Akibatnya nyata: sapuan status 6ab pada 2026-09-22 membaca blok ini dan mencatat `BE-LAB-46` sebagai satu dari dua task yang "benar-benar masih terbuka", padahal ia nol terbuka sejak empat hari sebelumnya |
| **Outcome** | Patolog dapat menyimpan laporan makroskopik, mikroskopik, dan kesimpulan, lalu membacanya ulang utuh |
| **Requirement/decision** | `FR-13.5`; `LAB-DEC-027` (BR-23 butir 3); `LAB-DC-038` |
| **Kontrak** | `LAB-API-v1` `r24` bagian 19.3; `LAB-VAL-v1` `r7` (`VAL-83`, `VAL-84`, `VAL-88`, `VAL-89`) |
| **Reuse** | `PUT /lab-examinations/{id}/result` dari `BE-LAB-43` — bentuk permintaan, penurunan waktu dan pelaku dari sesi, dan pola respons |
| **Cakupan** | Dua endpoint, dua DTO, perluasan `LabExaminationService`. **Nol tabel baru, nol migration** |
| **Dependency** | `BE-LAB-45` |
| **Acceptance criteria** | `AC-104` laporan dengan salah satu dari tiga ruas kosong atau hanya spasi **ditolak** `422` (`VAL-88`); `AC-105` jalur `/pathology` pada pemeriksaan berbentuk `Numeric` ditolak `422` (`VAL-84`); `AC-106` `examinedAt` di masa depan ditolak `422` (`VAL-89`); `AC-107` **nol jalur simpan sebagian** — tidak ada endpoint draft |
| **Verifikasi** | Pemeriksaan sungguhan: simpan lalu baca ulang, dan ketiga penolakan di atas benar-benar terjadi |
| **Risiko/pemilik** | Rendah |
| **DoD** | Kedua endpoint berjalan; `AC-104`..`AC-107` terbukti; ketiga ruas narasi **tidak muncul** pada payload logger |

> ### 🧊 DIBEKUKAN 2026-09-18 — jangan dikerjakan sebelum tiga pertentangan diputuskan
>
> Artifact `LAB-EVD-003` *Detail Hasil Patologi Anatomi* dilampirkan pemilik modul pada
> 2026-09-18 dan membantah **tiga hal sekaligus** yang menjadi dasar task ini. Rinciannya pada
> [`05-evidence-reconciliation.md`](../05-evidence-reconciliation.md) bagian 12.
>
> | ID | Yang dibantah |
> |---|---|
> | `REC4-CONF-002` | **Bentuk hasil PA bukan tiga ruas.** Ia bergantung kategori: Histologi/Sitologi Non-Ginekologi 3 ruas, Sitologi Ginekologi 3 ruas berbeda, dan **IHK 10 ruas**. BR-23 hanya mengenal satu bentuk |
> | `REC4-CONF-003` | **Hasil mungkin melekat pada ORDER, bukan pada PEMERIKSAAN.** `RULE-014` artifact: satu order punya **satu hasil terintegrasi** |
> | `REC4-CONF-001` | **Draft / Final / Reopen adalah status hasil**, dan `LAB-DEC-080` yang berumur satu hari menetapkan nol status hasil |
>
> **Nol baris kode sudah ditulis, dan itu satu-satunya sebab temuan ini masih murah.** Mengerjakan
> task ini sekarang berarti mendirikan tiga kolom yang bentuknya, tempatnya, dan siklus hidupnya
> sedang dipertanyakan sekaligus.
>
> **`BE-LAB-45` TIDAK ikut dibekukan**: keempat kolomnya tidak terbantah, yang terbantah
> kecukupannya. Menambah kolom kemudian lebih murah daripada menunda seluruh gelombang.
>
> ---
>
> ### Diperbarui 2026-09-18 sore — ketiga pertentangan SUDAH diputuskan, dan task ini berubah
>
> Amendment pass putaran 8 menutup ketujuh pertentangan `LAB-EVD-003`. **Task ini tetap
> dibekukan, tetapi sebabnya berubah:** bukan lagi menunggu keputusan, melainkan karena
> **keputusannya mengubah rancangannya**.
>
> | Keputusan | Akibat pada task ini |
> |---|---|
> | `LAB-DEC-085` | Hasil PA melekat pada **order**, bukan pemeriksaan. Path endpoint **salah alamat** |
> | `LAB-DEC-086` | Ruas hasil bukan tiga kolom, melainkan **nilai per parameter** dari data induk — **15** parameter |
> | `LAB-DEC-087` | Butuh **data induk pemetaan** `procedure → kategori PA` |
> | `LAB-DEC-088` | Bertambah `FinalizedAt`/`FinalizedByUserId` dan jalur `Reopen`; **bukan** status |
> | `LAB-DEC-090` | Pengisinya **Dokter Lab**, bukan analis |
>
> **Empat kolom `LabExamination` pada `BE-LAB-45` yang menampung narasi PA kini nol dipakai
> Patologi Anatomi.** `MicrobiologyFinding` tetap terpakai. Ketiga kolom narasi perlu ditinjau
> ulang saat `BE-LAB-46` dirancang ulang — **jangan dibangun sebelum itu**.
>
> **Urutan yang wajib dilalui sebelum task ini hidup lagi:** `hospital-domain-architect`
> merancang ulang `S4c` → amandemen `LAB-API-v1` `r25` → `plan-module-delivery` memecah task ini
> menjadi bentuknya yang baru. `r24` bagian 19.3 sudah ditandai `superseded`.

### 6i.4 `BE-LAB-47` — Dua tabel Mikrobiologi beserta index parsialnya

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-21 — lihat bagian 6o dan [`BE-LAB-47.md`](../task/report/backend/BE-LAB-47.md). *(status diperbaiki 2026-09-22; blok ini sempat tertinggal `SIAP DIKERJAKAN`)* |
| **Outcome** | Isolat dan kepekaan antibiotik punya tempat tersimpan, beserta penjaga yang mencegah satu antibiotik diuji dua kali pada isolat yang sama |
| **Requirement/decision** | `FR-13.2`, `FR-13.3`; `LAB-DC-036`, `LAB-DC-037`; `INV-27`, `INV-30`, `INV-31` |
| **Kontrak** | `02-backend-architecture.md` bagian 14.7, 14.8, 14.11; `erd/data-dictionary.md` bagian 14.3, 14.4 |
| **Reuse** | Pola snapshot `ProcedureNameSnapshot`; pola index unik parsial `LabOrderedProcedure` |
| **Cakupan** | Dua model, dua configuration, satu migration. **Tanpa** endpoint |
| **Dependency** | `BE-LAB-44` (FK ke kedua data induk), `BE-LAB-45` |
| **Acceptance criteria** | `AC-108` seluruh FK ber-`DeleteBehavior.Restrict`; `AC-109` index unik `(LabMicrobiologyIsolateId, LabAntibioticId)` **berbentuk parsial** dengan pembatas `IsDelete = false`; `AC-110` snapshot nama organisme dan antibiotik wajib terisi saat baris dibuat |
| **Verifikasi** | Pemeriksaan sungguhan terhadap definisi index di database — **bukan** terhadap kode configuration-nya |
| **Risiko/pemilik** | **Sedang.** Lihat peringatan di bawah |
| **DoD** | Kedua tabel berdiri; `AC-108`..`AC-110` terbukti pada database |

> ### ⚠ `AC-109` adalah butir paling mudah dilewatkan pada seluruh gelombang ini
>
> Penghapusan di sistem ini bersifat **penandaan** (`IsDelete`), bukan penghapusan baris. Index
> unik biasa akan membuat baris yang sudah ditandai hapus **tetap menempati kuncinya** —
> sehingga analis yang salah memilih antibiotik, menghapusnya, lalu memilih antibiotik yang
> **sama** lagi akan ditolak basis data tanpa sebab yang masuk akal baginya.
>
> **Modul ini sudah pernah membayar persis kesalahan ini** lewat `LAB-CONFLICT-005` pada index
> `(SpecimenId, ProcedureId)`. Pembuktiannya dipisahkan menjadi `BE-LAB-49` justru supaya tidak
> ikut tertelan "migration sudah jalan, berarti beres".

### 6i.5 `BE-LAB-48` — Jalur pengisian hasil Mikrobiologi

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-21 — lihat bagian 6r dan [`BE-LAB-48.md`](../task/report/backend/BE-LAB-48.md). *(status diperbaiki 2026-09-22; blok ini sempat tertinggal `SIAP DIKERJAKAN`)* |
| **Outcome** | Analis dapat menyimpan status temuan beserta seluruh isolat dan kepekaannya dalam satu tindakan, lalu membacanya ulang utuh |
| **Requirement/decision** | `FR-13.1`..`FR-13.4`, `FR-13.7`, `FR-13.8` |
| **Kontrak** | `LAB-API-v1` `r24` bagian 19.2; `LAB-VAL-v1` `r7` (`VAL-83`..`VAL-87`, `VAL-89`, `VAL-90`) |
| **Reuse** | Pola transaksi `LabExaminationService`; pola idempoten `PUT` dari `BE-LAB-43` |
| **Cakupan** | Dua endpoint, enam DTO, perluasan `LabExaminationService`. **Nol migration** |
| **Dependency** | `BE-LAB-47` |
| **Acceptance criteria** | `AC-111` permintaan dengan `isolates` **kosong diterima** sebagai hasil yang sah (`FR-13.4`); `AC-112` satu antibiotik dua kali pada isolat yang sama ditolak `422` dan **nol baris tersimpan sebagian** (`VAL-87`); `AC-113` organisme atau antibiotik nonaktif ditolak pada baris **baru**, tetapi hasil lama yang menunjuknya **tetap terbaca utuh** (`VAL-85`, `VAL-86`, `INV-31`); `AC-114` `PUT` berulang **mengganti** seluruh isi, bukan menambah |
| **Verifikasi** | Pemeriksaan sungguhan terhadap database untuk keempatnya, termasuk membuktikan `AC-112` **tidak** meninggalkan baris separuh |
| **Risiko/pemilik** | **Sedang.** `AC-113` menuntut dua perlakuan berbeda untuk data yang sama menurut umurnya, dan itu mudah disederhanakan keliru menjadi satu aturan |
| **DoD** | Kedua endpoint berjalan; `AC-111`..`AC-114` terbukti; payload logger **nol** memuat nama organisme maupun pola resistensi |

### 6i.6 `BE-LAB-49` — Pembuktian index parsial: hapus lalu pilih ulang

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-22 — lihat bagian 6z dan [`BE-LAB-49.md`](../task/report/backend/BE-LAB-49.md). `AC-115` sempat terbukti separuh; **`LAB-CONFLICT-011` ditutup pemilik modul lewat jalan B pada hari yang sama**, dan AC-nya dipersempit sehingga kini terbukti utuh |
| **Outcome** | Terbukti bahwa analis dapat menghapus satu baris kepekaan lalu memilih antibiotik yang **sama** lagi tanpa ditolak basis data |
| **Requirement/decision** | `INV-27`; `LAB-CONFLICT-005` sebagai pelajaran |
| **Kontrak** | `LAB-VAL-v1` `r7` (`VAL-87`) |
| **Reuse** | — |
| **Cakupan** | **Nol source baru.** Task ini seluruhnya verifikasi berbukti |
| **Dependency** | `BE-LAB-47`, `BE-LAB-48` |
| **Acceptance criteria** | `AC-115` **(dipersempit 2026-09-22, `LAB-CONFLICT-011` jalan B)** urutan **simpan → hapus baris → simpan lagi dengan antibiotik yang sama** berhasil. ~~dan baris lama tetap ada bertanda `IsDelete = true`~~ — **klausa itu DICABUT**: koleksi anak pada tabel ini memang diganti utuh secara fisik, dan jejak perubahannya milik `LabFieldChangeLog` (`LAB-DEC-112`), bukan baris nisan |
| **Verifikasi** | Dijalankan terhadap database sungguhan, dan **hasilnya dilampirkan pada laporan task** |
| **Risiko/pemilik** | **Rendah pada kodenya, tinggi pada akibat bila dilewatkan.** Kegagalannya tidak terlihat sampai analis pertama mengalaminya di meja kerja |
| **DoD** | `AC-115` terbukti dengan bukti yang dapat dibaca ulang |

> **Kenapa ini task tersendiri, bukan satu baris acceptance di dalam `BE-LAB-47`.** Ia memang
> diminta berdiri sendiri — tetapi **bukan** sebagai migration terpisah. Memisahkan index dari
> tabelnya akan meninggalkan jendela waktu ketika duplikat dapat tertulis, dan itu lebih
> berbahaya daripada yang hendak dicegah. Yang dipisahkan adalah **pembuktiannya**, sehingga ia
> tidak dapat ikut tertelan anggapan "migration sudah jalan, berarti beres".

### 6i.6b `BE-LAB-46` ❌ **DIBATALKAN** — digantikan `BE-LAB-50`, `BE-LAB-51`, `BE-LAB-52`

> **`BE-LAB-46` tidak dibekukan lagi; ia dibatalkan.** Rancangannya — dua endpoint atas tiga
> kolom `LabExamination` — **tidak lagi punya dasar**: `LAB-DEC-085` memindahkan hasil PA ke
> tingkat pesanan, dan `LAB-DEC-086` mengganti tiga kolom dengan data induk parameter.
>
> **Nol baris kode terbuang**, sebab nol baris pernah ditulis.

### 6i.6c Gelombang `MVP-6b1` dan `MVP-6b2` — Patologi Anatomi sesudah dirancang ulang

Menurunkan `04-prd-to-mvp.md` bagian 19 dan `02-backend-architecture.md` bagian 15.
Kontrak: **`LAB-API-v1` `r25`**, **`LAB-VAL-v1` `r8`**, **`LAB-PERM-v1` rev 7** —
**ketiganya `approved` 2026-09-18** oleh Yoga Aji Pratama selaku pemilik modul.

> **Gerbang kontrak terbuka; urutannya tetap tidak boleh ditukar.** `BE-LAB-50` kini `SIAP
> DIKERJAKAN`. `BE-LAB-51` dan `BE-LAB-52` **tidak lagi menunggu kontrak** — keduanya menunggu
> **pendahulunya**, dan itu penahan yang berbeda jenis. `MVP-6b1` (data induk) mendahului
> `MVP-6b2` (laporan) karena laporan menunjuk parameter; membalik urutannya berarti mendirikan
> tabel nilai yang menunjuk data induk yang belum ada.

> **Yang TIDAK ikut terbuka oleh persetujuan ini.** `S4e` — memvalidasi dan merilis laporan —
> tetap tertahan `DEC-LAB-011` (`LAB-REQ-013`, dr. Bima Prasetya). Penyimpanan **gambar** PA
> tetap tertahan `DEC-LAB-016`. Ruas sumber HL7 tertahan `LAB-COORD-012`, dan cetak laporan
> bahasa Inggris tertahan `LAB-COORD-013`. **Keempatnya BAGIAN, bukan slice**, dan **nol di
> antaranya menahan `BE-LAB-50`, `BE-LAB-51`, atau `BE-LAB-52`**.

#### `BE-LAB-50` — Empat data induk Patologi Anatomi

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-18 — gelombang `MVP-6b1`. Build hijau 0 error; migration **diterapkan** dan seeder **dijalankan** atas wewenang eksplisit pemilik modul. **`AC-128`..`AC-131` keempatnya terbukti pada database sungguhan**: 4 tabel, 4 index unik yang seluruhnya berpredikat `("IsDelete" = false)`, isi 4 / 15 / **19**, dan pemetaan `0` yang memang disengaja. Lihat [`BE-LAB-50.md`](../task/report/backend/BE-LAB-50.md) |
| **Outcome** | Kepala instalasi dapat mengelola parameter, kategori, keberlakuan, dan **pemetaan jenis pemeriksaan ke kategori PA** lewat aplikasi |
| **Requirement/decision** | `FR-13.11`; `LAB-DEC-086`, `LAB-DEC-087`; `LAB-DC-045`..`048` |
| **Kontrak** | `r25` bagian 20.3; `r8` (`VAL-101`); `rev 7` bagian 9.2 |
| **Reuse** | Pola `LabSpecimenType` dan `BE-LAB-44` sepenuhnya |
| **Cakupan** | Empat model, empat configuration, satu migration, dua controller, **satu seeder untuk tiga data induk tetap**, dan `GET /suggestions` |
| **Dependency** | ✅ Terpenuhi — kontrak `r25`/`r8`/`rev 7` disetujui 2026-09-18 |
| **Acceptance criteria** | `AC-128` keempat index unik **berbentuk parsial**; `AC-129` seeder mengisi kategori 4 baris, parameter 15 baris, keberlakuan **19 pasangan** — dengan **empat** parameter dipakai **dua** kategori; `AC-130` **nol endpoint `DELETE`**; `AC-131` `GET /suggestions` **mengusulkan**, bukan menyimpan sendiri |
| **Verifikasi** | Pemeriksaan sungguhan pada database: keempat tabel, keempat index parsial, dan **isi ketiga data induk tetap** |
| **Risiko/pemilik** | **Sedang.** `AC-129` mudah dibaca sebagai 15 pasangan, bukan 19 — dan selisihnya justru parameter yang dipakai dua kategori |

> ### ⚠ `AC-129` DIKOREKSI 2026-09-18 — dari `21` menjadi `19`, disetujui pemilik modul
>
> **Angka 21 keliru, dan rinciannya sendiri yang membuktikannya.** Baris ini dan
> `02-backend-architecture.md` bagian 15.12 sama-sama membawa rincian *"Histologi 3, Sitologi
> Non-Gin 3, Sitologi Gin 3, IHK 10"* — dan ketiganya ditambah sepuluh berjumlah **19**.
> `LAB-EVD-003` bagian 5.6, satu-satunya bukti sumbernya, juga menghasilkan 19.
>
> **Selisih 19 terhadap 15 ruas adalah EMPAT, bukan enam.** Yang dipakai dua kategori bukan hanya
> `Anjuran` (Sitologi Ginekologi + Imunohistokimia) melainkan juga `Makroskopik`, `Mikroskopik`,
> dan `Kesimpulan` — ketiganya dipakai Histologi **dan** Sitologi Non-Ginekologi. Karena itu
> 15 + 4 = **19**. Catatan risiko di atas benar arahnya sejak semula dan salah angkanya.
>
> Ditemukan `BE-LAB-50` sebelum satu baris seeder ditulis, dilaporkan alih-alih ditambal, dan
> **nol pasangan dikarang** agar cocok dengan 21 — mengarang keberlakuan berarti memunculkan ruas
> pada formulir diagnostik yang nol pernah diminta siapa pun. Lihat
> [`BE-LAB-50.md`](../task/report/backend/BE-LAB-50.md) bagian 2.
| **DoD** | Keempat tabel berdiri dan **tiga di antaranya terisi**; `AC-128`..`AC-131` terbukti |

#### `BE-LAB-51` — Laporan Patologi Anatomi dan konteks klinis

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-18 — gelombang `MVP-6b2`. Build hijau 0 error; migration `AddLabPathologyReport` **diterapkan**. **`AC-132`..`AC-135` terbukti pada database sungguhan, dan `AC-133`/`AC-134`/`AC-135` dibuktikan TERBALIK**: nol `IssuedAt`/`EffectiveAt`/status lifecycle, nol kolom `Pathology*` pada `LabExamination`, nol kolom konteks pada `LabOrder`, nol FK menyentuh kolom pelaku. Lihat [`BE-LAB-51.md`](../task/report/backend/BE-LAB-51.md) |
| **Outcome** | Laporan PA per pesanan, nilai per parameter, dan konteks klinis punya tempat tersimpan |
| **Requirement/decision** | `FR-13.10`, `FR-13.12`; `LAB-DEC-085`, `LAB-DEC-088`, `LAB-DEC-091`..`094` |
| **Kontrak** | `02-backend-architecture.md` bagian 15.5-15.7; `erd/data-dictionary.md` bagian 15 |
| **Reuse** | Pola snapshot `ProcedureNameSnapshot`; pola pelaku nullable tanpa FK |
| **Cakupan** | Tiga model, tiga configuration, satu enum, satu migration. **Tanpa** endpoint |
| **Dependency** | ✅ Terpenuhi — `BE-LAB-50` selesai dan migration-nya terterap 2026-09-18 |
| **Acceptance criteria** | `AC-132` ketiga index unik **parsial**; `AC-133` **nol kolom status hasil**, **nol `IssuedAt`/`EffectiveAt`**; `AC-134` **nol `ALTER TABLE`** pada tabel yang sudah berisi data — termasuk `LabOrder` dan `LabExamination`; `AC-135` `AnalystUserId` dan `FinalizedByUserId` **nullable tanpa foreign key**, dan penulisnya menulis `null` bukan `Guid.Empty` |
| **Verifikasi** | Pemeriksaan database; `AC-133` dan `AC-134` dibuktikan **terbalik** |
| **Risiko/pemilik** | Sedang. `AC-135` membawa peringatan `REG-ACTOR-FK` dan `BE-EXT-05` |
| **DoD** | Ketiga tabel berdiri; `AC-132`..`AC-135` terbukti |

#### `BE-LAB-52` — Jalur laporan Patologi Anatomi

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-18 — gelombang `MVP-6b2`. Enam endpoint berjalan; **`AC-136`..`AC-142` ketujuhnya terbukti terhadap aplikasi yang berjalan**, dan DoD logger terbukti **nol** memuat isi laporan maupun konteks klinis. **`AC-136` dibuktikan pada pesanan EMPAT golongan**, bukan dua: 19 pasangan → **15 ruas**, 4 di antaranya bergolongan ganda. **Dua cacat ditemukan pengujian dan diperbaiki**: usulan menggolongkan Imunohistokimia ke Histologi, dan `reopen` tanpa alasan menjawab `400` alih-alih `422`. Lihat [`BE-LAB-52.md`](../task/report/backend/BE-LAB-52.md) |
| **Outcome** | Patolog dapat membaca formulir sesuai kategori, menyimpan, menyelesaikan, dan membuka kembali laporan; dokter pemesan dapat menulis konteks klinis |
| **Requirement/decision** | `FR-13.10`, `FR-13.12`..`FR-13.17` |
| **Kontrak** | `r25` bagian 20.2; `r8` (`VAL-92`..`VAL-100`, `VAL-102`); `rev 7` bagian 9.1 dan 9.3 |
| **Reuse** | Pola transaksi `LabExaminationService`; pola `PUT` idempoten |
| **Cakupan** | Enam endpoint, delapan DTO, satu service baru. **Nol migration** |
| **Dependency** | ✅ Terpenuhi — `BE-LAB-51` selesai dan migration-nya terterap 2026-09-18 |
| **Acceptance criteria** | `AC-136` `GET` mengembalikan **hanya parameter yang berlaku**, dan `Anjuran` muncul **sekali** walau dua kategori memakainya; `AC-137` `finalize` ditolak `422` **beserta daftar ruas yang kosong** (`VAL-95`); `AC-138` `PUT` ditolak `409` ketika sudah final; `AC-139` `reopen` menuntut alasan dan **menaikkan `ReopenCount`** serta meninggalkan jejak; `AC-140` `issuedAt`/`effectiveAt` **hanya keluar sebagai turunan** dan **ditolak bila dikirim pemanggil**; `AC-141` konteks klinis memakai `LabOrder : Update`, **bukan** `LabExamination : Update`; `AC-142` pesanan tanpa pemetaan menjawab daftar parameter kosong **beserta sebabnya** (`VAL-100`) |
| **Verifikasi** | Pemeriksaan sungguhan untuk ketujuhnya, termasuk membuktikan `AC-136` pada pesanan **lintas kategori** — dikerjakan dengan **empat** golongan sekaligus |
| **Risiko/pemilik** | **Sedang-tinggi.** `AC-136` adalah inti `LAB-DEC-086`, dan satu-satunya cara membuktikannya adalah menyiapkan pesanan lintas golongan yang sungguhan |

> ### ⚠ Pasangan golongan pada baris Risiko DIKOREKSI 2026-09-18
>
> Baris itu semula menyebut **"pesanan Histologi + IHK"**, dan pasangan itu **nol membuktikan
> apa pun**: Histologi memakai `{Makroskopik, Mikroskopik, Kesimpulan}`, Imunohistokimia memakai
> sepuluh ruas termasuk `Anjuran`, dan **keduanya nol beririsan**.
>
> Yang benar-benar beririsan ada dua pasang: **Histologi ↔ Sitologi Non-Ginekologi** berbagi
> ketiga ruasnya, dan **Sitologi Ginekologi ↔ IHK** berbagi `Anjuran`. `BE-LAB-52` karena itu
> mengujinya dengan pesanan **empat golongan sekaligus**, yang membuktikan seluruh irisannya
> dalam satu pemeriksaan: **19 pasangan keberlakuan menghasilkan 15 ruas**, empat di antaranya
> bergolongan ganda. Lihat [`BE-LAB-52.md`](../task/report/backend/BE-LAB-52.md) bagian 3.1.
| **DoD** | Keenam endpoint berjalan; `AC-136`..`AC-142` terbukti; **payload logger nol memuat isi laporan maupun konteks klinis** |

### 6i.7 Satu pekerjaan yang bukan pekerjaan programmer, dan ia memblokir `MVP-6c`

| Butir | Isi |
|---|---|
| **Yang dibutuhkan** | Daftar organisme dan panel antibiotik **terisi** |
| **Pemilik** | Kepala instalasi laboratorium bersama `DR-LAB-002` |
| **Kenapa memblokir** | Layar pengisian hasil Mikrobiologi dengan daftar organisme kosong **tidak dapat dipakai sama sekali**, dan tidak dapat diuji selain pada keadaan kosongnya |
| **Kenapa dicatat di roadmap** | `LAB-COORD-006` dan `MST-POS-WRITE` membuktikan tabel data induk yang berdiri tanpa terisi adalah kegagalan yang **sudah berulang dua kali** di modul ini. Menyerahkannya pada "nanti diisi" adalah cara ketiganya terjadi |

**`BE-LAB-44` menyediakan jalannya; ia tidak menyediakan isinya.** Keduanya berbeda, dan hanya
yang pertama dapat diselesaikan pelaksana task.

### 6i.8 Yang sengaja **tidak** menjadi task

| Yang dipertimbangkan | Kenapa tidak |
|---|---|
| Endpoint validasi dan rilis hasil | `S4`, `S4d`, `S4e` tertahan `DEC-LAB-011` |
| Kolom atau enum status hasil | `LAB-DEC-080`, `INV-29`. Ditolak **eksplisit** |
| Penanda `Definitif` pada hasil Mikrobiologi | `LAB-DEC-081` mengeluarkannya dari Rilis 1 |
| Penilaian kritis otomatis | `INV-28`. Percabangannya milik `S5` |
| Tabel dan endpoint lampiran gambar Patologi Anatomi | `DEC-LAB-016` belum dijawab |
| Penomoran blok parafin dan slide Patologi Anatomi | `S2b` belum siap |
| Endpoint antibiogram | Ia **laporan**, dan datanya baru terkumpul sesudah gelombang ini berjalan |

---


---

## 6j. Gelombang `MVP-7` — `S4b` sesudah putaran 9 dan 10

Menurunkan [`02-backend-architecture.md`](../02-backend-architecture.md) **bagian 16** dan
[`erd/laboratory-operations.md`](../erd/laboratory-operations.md) amandemen 2026-09-21.

| Field | Nilai |
|---|---|
| Kontrak yang berlaku | `LAB-API-v1` **`r26`**, `LAB-VAL-v1` **`r9`**, `LAB-PERM-v1` **rev 8** — ketiganya `approved` 2026-09-21 |
| Masukan | decisions **rev 50**; capability map **rev 4** |
| Kesiapan arsitektur domain | `DOMAIN_ARCHITECTURE_READY` (`LAB-DA-001` rev 6) |
| Backend SHA saat direncanakan | `981e002c` |
| Slice | **`S4b` saja.** `S4d` nol task — tertahan `DEC-LAB-011` |

**Urutan gelombang berbasis risiko:**

```text
MVP-7a  BE-LAB-53  enam kolom LabExamination + enum   ──┐  prasyarat semua
                                                        │
MVP-7b  BE-LAB-54  finalize / reopen / consultation   ←─┘
                                                        
MVP-7c  BE-LAB-55  data induk Spesifik Specimen       ──┐
        BE-LAB-56  aturan kritis + evaluator          ──┼── saling bebas
        BE-LAB-57  LabFieldChangeLog + koreksi        ──┘
                                                        
MVP-7d  BE-LAB-58  ruas turunan pada jalur baca       ← butuh 54, 56
        BE-LAB-59  pilihan dokter konfirmator         ← bebas
```

> **`BE-LAB-53` sendirian di gelombang pertama, dan itu disengaja.** Enam kolom dan satu enum
> adalah satu migration yang menyentuh tabel **berisi data**. Menggabungkannya dengan pekerjaan
> lain membuat kegagalan migration menyeret task yang sebenarnya tidak bersalah.

> **Tiga penahan dibawa seluruh gelombang ini, dan nol di antaranya pekerjaan pelaksana:**
> `LAB-OPEN-040` dan `LAB-OPEN-041` menahan **pengisian** dua data induk — tabelnya tetap
> dibangun; `LAB-COORD-014` menahan ketepatan daftar dokter jaga — kolomnya tetap dibangun
> karena jalur jatuhnya sudah dirancang; dan `LAB-RDY-C04` tetap berlaku, sehingga verifikasi
> setiap task **wajib pemeriksaan sungguhan terhadap database**.

### 6j.1 `BE-LAB-53` — Enam kolom `LabExamination` dan enum status temuan

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-21 — lihat bagian 6m dan [`BE-LAB-53.md`](../task/report/backend/BE-LAB-53.md). Cakupannya bertambah oleh peleburan `BE-LAB-45`. *(status diperbaiki 2026-09-22; blok ini tertinggal `SIAP DIKERJAKAN`)* |
| **Outcome** | Hasil Mikrobiologi punya tempat menyatakan dirinya selesai ditulis, dan punya tempat mencatat fakta konsultasi |
| **Requirement/decision** | `LAB-DEC-097`, `LAB-DEC-106`, `LAB-DEC-113`; menutup `ARCH-GAP-LAB-04` |
| **Kontrak** | `LAB-API-v1` `r26` bagian 21.2 |
| **Reuse** | Pola `LabPathologyReport.FinalizedAt`/`FinalizedByUserId`/`ReopenCount` yang **sudah berdiri** — disalin apa adanya, nol bentuk baru ditemukan |
| **Cakupan** | Enam kolom pada `LabExamination`; enum `LabMicrobiologyFinding` (`Normal`/`Positive`/`Negative`); satu migration `AddLabMicrobiologyResultCompletion` |
| **Dependency** | Nol |
| **Acceptance criteria** | `AC-158` `FinalizedAt` terisi saat finalize; `AC-176` enum menawarkan tepat tiga nilai dan **tidak** memuat `NeedsAttention` maupun `Critical` |
| **Verifikasi** | Pemeriksaan database: keenam kolom berdiri, `ReopenCount` berdefault `0`, dan migration berjalan pada tabel berisi data **tanpa menulis ulang satu baris pun** |
| **Risiko/pemilik** | **Sedang.** `LabExamination` sudah berisi data produksi; kolom wajib tanpa default akan menolak migration |
| **DoD** | Migration diterapkan; keenam kolom nullable kecuali `ReopenCount`; enum terpisah dari `LabPathologyFindingStatus`; `AC-158` dan `AC-176` terbukti |

### 6j.2 `BE-LAB-54` — `finalize`, `reopen`, dan `consultation`

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-21 — lihat bagian 6n dan [`BE-LAB-54.md`](../task/report/backend/BE-LAB-54.md). *(status diperbaiki 2026-09-22; blok ini tertinggal `MENUNGGU PENDAHULU`)* |
| **Outcome** | Analis dapat menyatakan penulisan selesai, membukanya kembali sebelum rilis, dan mencatat konsultasi |
| **Requirement/decision** | `LAB-DEC-097`, `LAB-DEC-106` |
| **Kontrak** | `r26` bagian 21.2; `LAB-VAL-v1` `r9` (`VAL-107`, `VAL-108`); `LAB-PERM-v1` rev 8 bagian 10.1 |
| **Reuse** | `LabExamination : Update` dipakai ulang — **nol resource permission baru** |
| **Cakupan** | Tiga endpoint pada `LabExaminationController`; tiga metode pada `LabExaminationService`; dua DTO request |
| **Dependency** | `BE-LAB-53` |
| **Acceptance criteria** | `AC-158` hasil Final **tetap ditolak** ketika dicoba dikirim, dengan sebab menyebut belum dirilis; `AC-159` reopen mengosongkan `FinalizedAt` dan **nol** baris `S6` tercipta; `AC-169` konsultasi menyimpan tiga fakta dan **nol** tombol pengiriman berubah keadaan |
| **Verifikasi** | Pemeriksaan database dan panggilan sungguhan: `reopen` pada hasil yang belum pernah `finalize` menjawab `422` (`VAL-107`); `consultedAt` masa depan menjawab `422` (`VAL-108`) |
| **Risiko/pemilik** | **Sedang, dan bukan pada kodenya.** Godaan terbesar adalah memperlakukan `Simpan Final` sebagai rilis. `AC-158` sengaja menguji **penolakan pengirimannya**, bukan hanya terisinya kolom |
| **DoD** | Ketiga endpoint berjalan; `consultedByUserId` dan `finalizedByUserId` **diturunkan dari sesi**, bukan dari request; `AC-158`, `AC-159`, `AC-169` terbukti |

### 6j.3 `BE-LAB-55` — Data induk Spesifik Specimen

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-21 — lihat bagian 6t dan [`BE-LAB-55.md`](../task/report/backend/BE-LAB-55.md). *(status diperbaiki 2026-09-22; blok ini tertinggal `MENUNGGU PENDAHULU`)* |
| **Outcome** | Kepala instalasi dapat mengelola rincian specimen, dan satu specimen dapat menunjuk lebih dari satu rincian |
| **Requirement/decision** | `LAB-DEC-098`, `LAB-DEC-099`; menegakkan `LAB-DEC-040` |
| **Kontrak** | `r26` bagian 21.5; `LAB-VAL-v1` `r9` (`VAL-104`, `VAL-105`, `VAL-111`); `LAB-PERM-v1` rev 8 |
| **Reuse** | Pola `LabSpecimenTypeService` sepenuhnya — `master-data-endpoint-standard`, `IsActive` bukan hapus, unique **parsial** pada kode |
| **Cakupan** | `LabSpecimenDetailType` + `LabSpecimenDetail`; dua configuration; satu service; satu controller (8 endpoint) |
| **Dependency** | `BE-LAB-53` |
| **Acceptance criteria** | `AC-160` satu specimen menunjuk lebih dari satu rincian dan nilai ketikan bebas ditolak; `AC-161` `Lainnya` **tidak** membuat nilai tetap baru — nol baris `LabSpecimenDetailType` bertambah |
| **Verifikasi** | Pemeriksaan database: index unik **parsial** pada `DetailTypeCode` dan pada `(LabSpecimenId, LabSpecimenDetailTypeId)`; `POST` berulang dengan kode sama menjawab `409` |
| **Risiko/pemilik** | **Rendah pada kodenya, sedang pada kelanjutannya.** Tabel berdiri tidak berarti terisi — `LAB-OPEN-040` menahan isinya, dan datasetnya belum pernah dibaca blueprint |
| **DoD** | Kedua tabel berdiri; kedelapan endpoint berjalan; **nol** endpoint yang membuat baris dari halaman hasil; `AC-160` dan `AC-161` terbukti |

### 6j.4 `BE-LAB-56` — Aturan kritis dan evaluatornya

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-21 — lihat bagian 6u dan [`BE-LAB-56.md`](../task/report/backend/BE-LAB-56.md). *(status diperbaiki 2026-09-22; blok ini tertinggal `MENUNGGU PENDAHULU`)* |
| **Outcome** | `DR-LAB-002` dapat menetapkan kombinasi yang kritis, dan penanda menyala tanpa satu angka pun dihardcode |
| **Requirement/decision** | `LAB-DEC-103`; mempersempit `INV-28` |
| **Kontrak** | `r26` bagian 21.6; `LAB-VAL-v1` `r9` (`VAL-106`); `LAB-PERM-v1` rev 8 bagian 10.2 |
| **Reuse** | Pola data induk `LabOrganism`; **tetapi hak aksesnya berbeda** — tulisnya dipegang wewenang klinis, bukan kepala instalasi |
| **Cakupan** | `LabMicrobiologyCriticalRule`; satu configuration; `LabMicrobiologyCriticalRuleService` termasuk `EvaluateAsync`; satu controller (5 endpoint) |
| **Dependency** | `BE-LAB-53` |
| **Acceptance criteria** | `AC-166` penanda **tidak menyala** saat aturan kosong **dan** `criticalRuleAvailable` bernilai salah; `AC-167` `R` yang tidak cocok aturan **tidak** menyalakan penanda |
| **Verifikasi** | Pemeriksaan database dan panggilan sungguhan: baris beraturan ketiga ruas kosong menjawab `422` (`VAL-106`); dengan nol baris aturan, respons hasil memuat `criticalRuleAvailable` bernilai salah |
| **Risiko/pemilik** | **Tinggi, dan bukan teknis.** Ini permukaan keselamatan pasien. Dua kesalahan yang paling mungkin: menyimpan `IsCritical` sebagai kolom, dan membiarkan layar diam ketika aturan kosong. Keduanya diuji langsung |
| **DoD** | Tabel berdiri; kelima endpoint berjalan; hak tulis pada wewenang klinis; **`IsCritical` nol disimpan** — dihitung `EvaluateAsync`; `AC-166` dan `AC-167` terbukti |

### 6j.5 `BE-LAB-57` — `LabFieldChangeLog` dan koreksi specimen

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-21 — lihat bagian 6v dan [`BE-LAB-57.md`](../task/report/backend/BE-LAB-57.md) |
| **Outcome** | Petugas dapat mengoreksi specimen dari halaman hasil, dan nilai lamanya tetap terbaca |
| **Requirement/decision** | `LAB-DEC-107`, `LAB-DEC-112` |
| **Kontrak** | `r26` bagian 21.4; `LAB-VAL-v1` `r9` (`VAL-109`, `VAL-110`); `LAB-PERM-v1` rev 8 bagian 10.3 |
| **Reuse** | `LabSpecimen : Update` dipakai ulang — nol resource baru |
| **Cakupan** | `LabFieldChangeLog` + configuration; `LabFieldChangeRecorder`; `ApplyCorrectionAsync` pada `LabSpecimenService`; dua endpoint — **dikerjakan pada `LabSpecimenCorrectionService` tersendiri, lihat 6v** |
| **Dependency** | `BE-LAB-53`, `BE-LAB-55` — `detailTypeIds` pada request menunjuk tabel yang dibangun `BE-LAB-55` |
| **Acceptance criteria** | `AC-170` koreksi berjejak dan sesudah Final menjadi baca-saja; `AC-175` satu koreksi menambah **satu** baris jejak ruas dan **nol** baris `LabTransitionHistory` |
| **Verifikasi** | Pemeriksaan database: sesudah mengubah jenis specimen, `LabFieldChangeLog` bertambah satu baris bernilai lama, dan `LabTransitionHistory` **tidak bertambah**; koreksi pada hasil yang sudah Final menjawab `422` (`VAL-109`) |
| **Risiko/pemilik** | **Sedang.** `PATCH` sebagian mudah keliru diterapkan sebagai `PUT` penuh, yang akan menghapus ruas yang tidak dikirim |
| **DoD** | Tabel berdiri tanpa cascade dari mana pun; kedua endpoint berjalan; **hanya ruas yang benar-benar berubah** yang tercatat; `AC-170` dan `AC-175` terbukti |

### 6j.6 `BE-LAB-58` — Ruas turunan pada jalur baca hasil

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-22 — lihat bagian 6y dan [`BE-LAB-58.md`](../task/report/backend/BE-LAB-58.md). **`AC-183` tetap terbuka**, tertahan `DEC-LAB-011` |
| **Outcome** | Layar hasil memperoleh waktu, nama analis, keadaan kelengkapan, dan penanda kritis tanpa menghitung sendiri |
| **Requirement/decision** | `LAB-DEC-096`, `LAB-DEC-097`, `LAB-DEC-103`, `LAB-DEC-105`, `LAB-DEC-106` |
| **Kontrak** | `r26` bagian 21.3 |
| **Reuse** | `GET /{id}/result/microbiology` yang sudah dikontrakkan `r24`; amandemen ini **memperluas responsnya**, bukan membuat jalur baru |
| **Cakupan** | Delapan ruas turunan pada `LabMicrobiologyResultResponse`; pemanggilan `EvaluateAsync` per baris kepekaan |
| **Dependency** | `BE-LAB-54` (`FinalizedAt`), `BE-LAB-56` (`EvaluateAsync`) |
| **Acceptance criteria** | `AC-157` kedua waktu **tidak dapat diketik** — dikirim pada request pun diabaikan; `AC-168` `analystName` sama dengan penyimpan dan `analystUserId` palsu diabaikan |
| **Verifikasi** | Panggilan sungguhan: kirim `effectiveAt`, `issuedAt`, dan `analystUserId` palsu pada `PUT`, lalu baca ulang — ketiganya **tidak berubah** |
| **Risiko/pemilik** | **Rendah pada bentuknya, sedang pada kinerjanya.** Setiap pembacaan ikut membaca tabel aturan; tabelnya kecil dan boleh di-cache per permintaan |
| **DoD** | Kedelapan ruas tersaji; nol di antaranya diterima pada request mana pun; `AC-157` dan `AC-168` terbukti |

> **Cakupan task ini BERTAMBAH sesudah roadmap ditulis, dan itu perlu dibaca sebelum
> dikerjakan.** `r27` bagian 22.3 menambahkan ruas turunan di luar delapan milik `r26` 21.3:
> `labReportNumber`, `printReceivedAt`, `printCompletedAt`, `consultantLabel`,
> `consultantName`, `standingNote`, `authorizingOfficerName`, `validatedByName`,
> `usesSusceptibilitySet`, `breakpointAvailable`, beserta lima ruas snapshot pada setiap baris
> kepekaan. Seluruh **sumbernya sudah berdiri** — `BE-LAB-60`..`63` membangunnya.
>
> **Dua AC ikut pindah ke sini** (dikoreksi 2026-09-22): `AC-181` tanggal cetak
> (`LAB-DEC-118`) dan `AC-183` petugas otorisasi (`LAB-DEC-120`). Traceability sempat mencatat
> keduanya milik `BE-LAB-63`, padahal 63 hanya menyediakan sumbernya.
>
> **`AC-183` tidak dapat ditutup task ini.** `authorizingOfficerName` diisi **perilis**, dan
> rilis milik `S4d` yang tertahan `DEC-LAB-011`. Ruasnya dibangun sekarang dan **tampil
> kosong** — itu memang yang diminta `LAB-DEC-120`.

### 6j.7 `BE-LAB-59` — Pilihan dokter konfirmator beserta jalur jatuhnya

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-22 — lihat bagian 6x dan [`BE-LAB-59.md`](../task/report/backend/BE-LAB-59.md) |
| **Outcome** | Petugas dapat memilih dokter konfirmator **walaupun** jadwal jaga belum pernah diisi siapa pun |
| **Requirement/decision** | `LAB-DEC-111`; menggantikan butir 2 `LAB-DEC-108` |
| **Kontrak** | `r26` bagian 21.7 |
| **Reuse** | `TrxOnCallAssignment` → `MstDoctor.WorkforceProfileId` → `MstDoctor` — **rantainya sudah lengkap** pada satu `ApplicationDbContext`. Nol tabel baru, nol jembatan baru |
| **Cakupan** | `LabConfirmingDoctorResolver`; satu endpoint baca; satu DTO response |
| **Dependency** | Nol |
| **Acceptance criteria** | `AC-173` ketika jadwal jaga terisi, hanya dokter itu ditawarkan beserta nomor WhatsApp-nya; `AC-174` ketika jadwal jaga **kosong**, pemilih tetap dapat dipakai dan `onDutyScheduleAvailable` bernilai salah |
| **Verifikasi** | Panggilan sungguhan pada **dua keadaan**: dengan satu baris `TrxOnCallAssignment` aktif, dan dengan nol baris |
| **Risiko/pemilik** | **Sedang, dan risikonya pengujian — bukan kode.** `AC-174` adalah keadaan yang **pasti terjadi lebih dulu** karena `TrxOnCallAssignment` nol punya endpoint pengisi (`LAB-COORD-014`). Menguji hanya keadaan "jadwal terisi" akan meloloskan slice yang tidak dapat dipakai pada hari pertama |
| **DoD** | Endpoint berjalan; **nol** baris disalin ke tabel Laboratorium; kedua keadaan terbukti lewat `AC-173` dan `AC-174` |

### 6j.8 Data induk awal gelombang ini

| Data induk | Isi awal | Pengisi | Penahan |
|---|---|---|---|
| `MstMeasurement` | **Enam baris** bertanda `IsForLaboratory`: `swab`, `preparat`, `potong`, `item`, `isolat`, `vial` | Seeder Laboratorium | Nol. `MeasurementController` punya `POST`/`PUT`/`DELETE` lengkap |
| `LabSpecimenDetailType` | **Nol baris** | Kepala instalasi | `LAB-OPEN-040` — dataset belum diserahkan |
| `LabMicrobiologyCriticalRule` | **Nol baris** | `DR-LAB-002` | `LAB-OPEN-041` — penilaian klinis |

> **Dua dari tiga kosong, dan itu bukan pekerjaan yang terlupa.** Keduanya memuat penilaian.
> Mengisinya dengan tebakan pelaksana berarti menaruh keputusan klinis di dalam seeder.
> `AC-166` ada justru untuk memastikan layar menyatakan kekosongan itu dengan jujur.

### 6j.9 Yang TIDAK menjadi task gelombang ini

| Yang dikecualikan | Alasan |
|---|---|
| Validasi dan rilis Mikrobiologi | `S4d`, tertahan `DEC-LAB-011` |
| Pengiriman hasil dan pembangkit PDF | `LAB-COORD-011` |
| Ruas dan endpoint `HL7` | `LAB-DEC-109` |
| Cetak dwibahasa | `LAB-COORD-013` |
| Endpoint tulis `TrxOnCallAssignment` | `LAB-COORD-014`, milik `human-resource` |
| Aturan isolat pada status temuan `Negatif` | `LAB-OPEN-042` — usulan arsitektur, bukan keputusan pemilik modul |

---

## 6k. Gelombang `MVP-7b` — `S4b` sesudah bukti cetak

Menurunkan [`02-backend-architecture.md`](../02-backend-architecture.md) **bagian 17**.

| Field | Nilai |
|---|---|
| Kontrak | `LAB-API-v1` **`r27`**, `LAB-VAL-v1` **`r10`**, `LAB-PERM-v1` **rev 9** — ketiganya `approved` 2026-09-21 |
| Masukan | decisions **rev 52**; `LAB-EVD-005`, `LAB-EVD-006` |
| Backend SHA | `981e002c` |

### ⚠ `BE-LAB-53` BERUBAH CAKUPAN — baca sebelum mengerjakannya

`BE-LAB-53` pada gelombang `MVP-7a` ditulis sebelum bukti cetak datang. Cakupannya **bertambah
tiga kolom dan tiga enum**:

| Semula (`MVP-7a`) | Bertambah (`MVP-7b`) |
|---|---|
| `FinalizedAt`, `FinalizedByUserId`, `ReopenCount` | `ResultQualifier` |
| `ConsultedByUserId`, `ConsultedToName`, `ConsultedAt` | `CultureType` |
| Enum `LabMicrobiologyFinding` | `SusceptibilityMethod` |
| — | Enum `LabResultQualifier`, `LabCultureType`, `LabSusceptibilityMethod` |

**Ketiga kolom baru nullable**, dan itu disengaja — lihat bagian 17.7. Migration-nya tetap
satu, `AddLabMicrobiologyResultCompletion`. AC-nya bertambah `AC-177`.

### Urutan gelombang

```text
MVP-7b-1  BE-LAB-60  breakpoint + DiscContentUg   ──┐
                                                    │
MVP-7b-2  BE-LAB-61  interpreter                  ←─┘  (butuh 60)
MVP-7b-3  BE-LAB-62  profil katalog Mikrobiologi     (bebas)
          BE-LAB-63  pengaturan disiplin + no. cetak (bebas)
```

### 6k.1 `BE-LAB-60` — Data induk breakpoint dan kandungan cakram

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-21 — lihat bagian 6p dan [`BE-LAB-60.md`](../task/report/backend/BE-LAB-60.md). *(status diperbaiki 2026-09-22; blok ini tertinggal `MENUNGGU PENDAHULU`)* |
| **Outcome** | `DR-LAB-002` dapat menetapkan rentang breakpoint, dan panel antibiotik membawa kandungan cakramnya |
| **Requirement/decision** | `LAB-DEC-122` |
| **Kontrak** | `r27` bagian 22.5; `LAB-VAL-v1` `r10` (`VAL-115`, `VAL-119`); `LAB-PERM-v1` rev 9 |
| **Reuse** | Pola `LabOrganism`; hak tulis mengikuti pola aturan kritis `BE-LAB-56` — **wewenang klinis, bukan kepala instalasi** |
| **Cakupan** | `LabSusceptibilityBreakpoint` + configuration; kolom `DiscContentUg` pada `LabAntibiotic`; satu service; satu controller (5 endpoint); migration `AddLabMicrobiologyPrintAndBreakpoint` |
| **Dependency** | `BE-LAB-53` |
| **Acceptance criteria** | `AC-185` mengubah breakpoint di data induk **tidak mengubah** baris hasil lama yang sudah ber-snapshot |
| **Verifikasi** | Pemeriksaan database: index unik **parsial** atas `(LabOrganismId, LabAntibioticId)`; `lowerMm > upperMm` menjawab `422`; pasangan berulang menjawab `409` |
| **Risiko/pemilik** | **Tinggi, dan bukan teknis.** Angka breakpoint menentukan pasien mendapat antibiotik yang benar. Menggeser batas bawah dari `13` ke `12` mengubah sebagian hasil dari `R` menjadi `I` **tanpa satu pun hasil disunting** |
| **DoD** | Tabel dan kolom berdiri; kelima endpoint berjalan; hak tulis pada wewenang klinis; `AC-185` terbukti |

### 6k.2 `BE-LAB-61` — Penghitung interpretasi dan penimpaannya

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-21 — lihat bagian 6q dan [`BE-LAB-61.md`](../task/report/backend/BE-LAB-61.md). *(status diperbaiki 2026-09-22; blok ini tertinggal `MENUNGGU PENDAHULU`)* |
| **Outcome** | Analis tidak lagi mengetik `S`/`I`/`R`; sistem menghitungnya, dan penimpaan tercatat beserta alasannya |
| **Requirement/decision** | `LAB-DEC-123`, `LAB-DEC-115`, `LAB-DEC-128` |
| **Kontrak** | `r27` bagian 22.2 dan 22.4; `LAB-VAL-v1` `r10` (`VAL-112`, `VAL-113`, `VAL-114`, `VAL-116`) |
| **Cakupan** | `LabSusceptibilityInterpreter`; tujuh kolom pada `LabIsolateSusceptibility`; perluasan `LabExaminationService`; ruas turunan `breakpointAvailable` |
| **Dependency** | `BE-LAB-60` |
| **Acceptance criteria** | `AC-186` ketiga batas terbukti — zona 11 pada 12-16 → `R`, 13 pada 12-15 → `I`, 32 pada 13-16 → `S`; `AC-187` penimpaan tanpa alasan ditolak dan `ComputedResult` asli tetap tersimpan; `AC-191` zona `0` menghasilkan `R`, zona kosong **nol** menghasilkan interpretasi |
| **Verifikasi** | Pengujian unit atas ketiga batas **beserta batas tepatnya** — zona persis sama dengan `lowerMm` dan persis sama dengan `upperMm`; lalu panggilan sungguhan dengan breakpoint **nol baris** yang memaksa `result` wajib (`VAL-114`) |
| **Risiko/pemilik** | **Tertinggi pada gelombang ini.** Tiga jebakan: salah tanda pada batas (`<` versus `<=`), memperlakukan zona `0` sebagai kosong, dan lupa menyimpan `ComputedResult` sehingga penimpaan kehilangan pembandingnya |
| **DoD** | Ketiga AC terbukti; `ComputedResult` tersimpan berdampingan dengan `Result`; **`result` diterima tetapi tidak wajib** kecuali breakpoint kosong |

### 6k.3 `BE-LAB-62` — Profil Mikrobiologi katalog

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-21 — lihat bagian 6s dan [`BE-LAB-62.md`](../task/report/backend/BE-LAB-62.md). *(status diperbaiki 2026-09-22; blok ini tertinggal `MENUNGGU PENDAHULU`)* |
| **Outcome** | Kepala instalasi menentukan pemeriksaan mana memakai set bakteri |
| **Requirement/decision** | `LAB-DEC-125` |
| **Kontrak** | `r27` bagian 22.6; `LAB-VAL-v1` `r10` (`VAL-118`) |
| **Reuse** | Pola `LabProcedurePathologyCategory` **sepenuhnya** — termasuk menunjuk `MstProcedure` dari sisi Laboratorium |
| **Cakupan** | `LabProcedureMicrobiologyProfile` + configuration; satu service; satu controller (5 endpoint) |
| **Dependency** | `BE-LAB-53` |
| **Acceptance criteria** | `AC-189` pemeriksaan tanpa profil set bakteri **nol menampilkan** bagian isolat, dan mengirimnya menjawab `422` |
| **Risiko/pemilik** | **Rendah pada kodenya, sedang pada kelanjutannya.** Tabel berdiri tidak berarti terpetakan — selama kosong, **nol pemeriksaan** akan menampilkan set bakteri |
| **DoD** | Tabel berdiri; kelima endpoint berjalan; **nol kolom ditambahkan ke `MstProcedure`**; `AC-189` terbukti |

### 6k.4 `BE-LAB-63` — Pengaturan disiplin dan nomor cetak

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-22 — lihat bagian 6w dan [`BE-LAB-63.md`](../task/report/backend/BE-LAB-63.md). **Satu batas diketahui: `LAB-OPEN-043`** |
| **Outcome** | Footer cetak tiap disiplin punya sumbernya, dan setiap pesanan punya nomor yang dipakai laboratorium hari ini |
| **Requirement/decision** | `LAB-DEC-117`, `LAB-DEC-119`, `LAB-DEC-127` |
| **Kontrak** | `r27` bagian 22.3 dan 22.7 |
| **Reuse** | `LabOrderNumberService` sebagai pola alokasi — **dengan satu perbedaan:** penghitungnya per disiplin per tahun, bukan global |
| **Cakupan** | `LabDisciplineSetting` + configuration + seeder tiga baris; kolom `LabReportNumber` pada `LabOrder`; `LabReportNumberService`; satu controller (4 endpoint, **nol `POST`, nol `DELETE`**) |
| **Dependency** | `BE-LAB-53` |
| **Acceptance criteria** | `AC-180` dua nomor berbeda pada satu pesanan dan keduanya terbaca; `AC-182` mengubah nama konsultan mengubah footer tetapi **tidak** mengubah pemegang wewenang klinis |
| **Verifikasi** | Pemeriksaan database: index unik parsial atas `(Discipline, Tahun, Nomor)`; `OrderNumber` **tetap utuh** dan tetap menjadi sumber barcode |
| **Risiko/pemilik** | **Sedang.** Godaan terbesar mengubah `OrderNumber` menjadi format cetak. `LAB-DEC-117` menolaknya — layanan alokasi, index, dan barcode sudah berjalan di atasnya |
| **DoD** | Tiga baris pengaturan ter-seed dari `LAB-EVD-005`; nomor cetak teralokasi per disiplin per tahun; `AC-180` dan `AC-182` terbukti |

### 6k.5 Data induk awal gelombang ini

| Data induk | Isi awal | Penahan |
|---|---|---|
| `LabDisciplineSetting` | **Tiga baris di-seed** — label dan nama konsultan terbaca langsung dari `LAB-EVD-005`, bukan tebakan | Nol |
| `MstMeasurement` | Dua baris: `ug/mL`, `mg/L` | Nol |
| `LabAntibiotic.DiscContentUg` | **Nol diisi** — angka pada `LAB-EVD-006` contoh, bukan daftar resmi | Butuh daftar panel resmi |
| `LabSusceptibilityBreakpoint` | **Nol baris** | `DR-LAB-002` |
| `LabProcedureMicrobiologyProfile` | **Nol baris** | Butuh daftar pemeriksaan ber-set-bakteri |

### 6k.6 Yang TIDAK menjadi task

Susunan cetak dua isolat berantibiogram, kalimat hasil nol pertumbuhan, pengulangan kop
halaman kedua, dan kategori Patologi Anatomi lain — **keempatnya belum pernah terlihat**
(`LAB-OPEN-039`). Menebaknya mengulang persis kesalahan `LAB-DEC-116`.
## 7. Ringkasan Status Task

| Task | Gelombang | Slice | Status rencana | Penahan spesifik |
|---|---|---|---|---|
| `BE-LAB-26` ✅ | `MVP-5b` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-15. Tabel berdiri dan diterapkan ke `QuilvianNewDevYoga`; unique parsial ditolak `23505`, `Restrict` ditolak `23503` **terisolasi beserta langkah kontrolnya**; jalur `Down` lalu `Up` terbukti — [laporan](../task/report/backend/BE-LAB-26.md) | Tidak ada. `LabExamination` terbukti tidak berubah satu baris pun |
| `BE-LAB-27` ✅ | `MVP-5b` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-15. Endpoint berdiri; pemecahan **dijalankan sungguhan**: 4 pemeriksaan lintas 3 disiplin → 3 pesanan, sedisiplin berkumpul, cito melekat per pemeriksaan; `VAL-64`..`67` keempatnya `422`; `T-88a` endpoint lama terbukti tidak berubah — [laporan](../task/report/backend/BE-LAB-27.md) | Ruas `clinicalNote` **ditutup 2026-09-15**: **dicabut** dari kontrak lewat `r11` atas keputusan pemilik modul, bukan diberi kolom. Dasarnya nol peminta — `FE-LAB-14`, satu-satunya layar yang memanggil endpoint ini, tidak memuat kotak catatan klinis. Nol dampak kode: `ClinicalNote` nol kemunculan di modul Laboratorium. Task ini **tidak perlu dikerjakan ulang** |
| `BE-LAB-28` ✅ | `MVP-5b` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-15. `VAL-68` menolak `422` dan `VAL-69` menolak `409`, keduanya dibandingkan kata demi kata terhadap matriks; permintaan yang berwadah ditandai `Fulfilled` beserta tautannya, yang belum terbaca menunggu. **Pesanan lama terbukti tidak tersentuh, beserta baris kontrolnya** — [laporan](../task/report/backend/BE-LAB-28.md) | Pembatalan **pesanan** tidak menyentuh baris terpesan — batas yang sengaja tidak dilewati, menuntut keputusan yang belum diambil. Satu penafsiran dilaporkan: permintaan yang sudah dibatalkan diperlakukan tidak ada pada daftar terpesan |
| `BE-LAB-29` ✅ | `MVP-5b` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-15. Satu ekspresi diubah; jalur tulis `LabOrder` terbukti **tepat satu** di seluruh aplikasi; katalog 10 dari 10 tergolong — [laporan](../task/report/backend/BE-LAB-29.md) | Tidak ada. `LAB-CONFLICT-007` **ditutup**. Dua baris data lama sengaja tidak diperbaiki — perubahan data, wewenangnya terpisah |
| `BE-EXT-04` ✅ | `MVP-5b` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-15. Dua kolom nullable berdiri dan diterapkan; **`T-93a` terbukti: 16 sesi kiosk utuh, 15 masih cocok ke pasien, nol kolom baru terisi**; jalur `Down` terbukti — [laporan](../task/report/backend/BE-EXT-04.md) | Tidak ada. **Jalur baca baru tidak jadi dibuat** — `GET /options` yang sudah ada tinggal ditambah penyaring tujuan |
| `BE-LAB-30` ✅ | `MVP-5c` | `EPIC-LAB-12` | **`SELESAI`** — 2026-09-16. Ketiga kolom berdiri dan **diterapkan ke `QuilvianNewDevYoga`**; ketiganya `nullable` tanpa nilai bawaan; foreign key `Restrict` ke `MstDoctor` beserta indexnya berdiri; jalur `Down` lalu `Up` terbukti; **5 pesanan lama utuh dan ketiga kolom barunya `null`** — [laporan](../task/report/backend/BE-LAB-30.md) | Tidak ada. Nilai enum sengaja `Confirmed = 9`, bukan `3`, supaya tiga dari lima pesanan yang sudah tersimpan tidak berubah artinya. Satu baris di luar cakupan: label `Dikonfirmasi` pada `LabFilterMetadataFactory`, supaya pilihan penyaring tidak berbahasa Inggris |
| `BE-LAB-31` ✅ | `MVP-5c` | `EPIC-LAB-12` | **`SELESAI`** — 2026-09-16. Endpoint `POST /lab-orders/{id}/confirm` berdiri sesuai `r12`; `VAL-70`..`VAL-73` **keempatnya terbukti menolak** sesuai matriks; konfirmator dan waktu **diturunkan server** dan ruasnya memang tidak ada pada DTO; `T-97c` terbukti dari database — [laporan](../task/report/backend/BE-LAB-31.md) | **`AC-94` dan `AC-95` terpenuhi sebagian** — bagian "terbaca/tampil pada daftar" tertahan celah kontrak: `r12` tidak menambah ruas respons, sehingga **`FE-LAB-15` belum dapat dimulai**. Usul `r13` berisi lima ruas ada pada laporan bagian 7.2. Satu celah gelombang ditutup di sini: turunan `Confirmed` → `Accepted` yang tidak dimiliki task mana pun |
| `BE-LAB-32` ✅ | `MVP-5c` | `EPIC-LAB-12` | **`SELESAI`** — 2026-09-16. `VAL-74` menolak `422` dan `VAL-75` menolak `409` sesuai matriks; alasan terbaca kembali dari jejak audit dan terbukti dirapikan; **`T-97b` membuktikan `Requested` dan `Confirmed` tetap dapat dibatalkan**. Nol kolom baru — [laporan](../task/report/backend/BE-LAB-32.md) | **Hitungan dampak dilaporkan sebelum aturan ditegakkan**, sebagaimana diwajibkan DoD: **3 pesanan** kehilangan jalur pembatalan (`Accepted` 2, `InProcess` 1, `OnHold` 0), tetapi **0 pembatalan pernah terjadi**, **0 layar frontend memanggilnya**, dan hanya **1 pemanggil backend**. Sisa risiko: ketiga pesanan itu tidak punya jalur apa pun sampai aturan koreksi `LAB-P0-003` diputuskan; dan database selain `QuilvianNewDevYoga` **belum diperiksa** |
| `BE-LAB-33` ✅ | `MVP-5d` | `EPIC-LAB-12` | **`SELESAI`** — 2026-09-16. Kelima ruas `r13` terbaca dari database pada jalur daftar maupun detail; pesanan yang belum dikonfirmasi terbukti mengembalikan kelimanya `null`; nama konfirmator terbukti memakai jalur yang **sama** dengan `RequestedByName`. Sepuluh pemeriksaan, sepuluh `PASS` — [laporan](../task/report/backend/BE-LAB-33.md) | Nol migration, nol endpoint baru, nol permission baru. **`FE-LAB-15` kini terbuka sepenuhnya.** `FE-LAB-17` **tetap tertahan** — `r13` hanya menutup penahan tanda tangannya. Sisa risiko: kedua sub-query terjemahan nama belum diukur pada daftar besar (database uji hanya 5 pesanan) |
| `BE-LAB-34` ✅ | `MVP-5d` | `EPIC-LAB-12` | **`SELESAI`** — 2026-09-16. Tiga ruas `r14` terbaca dari database lewat endpoint daftar pantau yang **benar-benar dipakai** ketiga menu pemeriksaan; pesanan belum dikonfirmasi mengembalikan ketiganya `null`; ruas lama terbukti tidak berubah. 15 pemeriksaan, 15 `PASS` — [laporan](../task/report/backend/BE-LAB-34.md) | **Koreksi atas sasaran `r13`.** `BE-LAB-33` benar terhadap kontraknya; kontraknya yang menyebut DTO keliru — ketiga menu membaca grup `Lab Monitoring`, bukan `LabOrderListResponse`. **`FE-LAB-15` kini nol penahan.** Sisa risiko: ruas `r13` pada `GET /lab-orders/by-discipline/{discipline}` belum punya pembaca |
| `BE-LAB-35` ✅ | `MVP-5e` | `EPIC-LAB-12` | **`SELESAI`** — 2026-09-16. Ruas `orderedProcedures` terbaca **dari database**: daftar terpesan lengkap dan urut, nama dari **snapshot** bukan katalog, baris `Cancelled` ikut beserta statusnya, baris terhapus tidak ikut, dan pesanan lama mengembalikan array **kosong** bukan `null`. 12 pemeriksaan, 12 `PASS`; nol migration — [laporan](../task/report/backend/BE-LAB-35.md) | Tidak ada. **`FE-LAB-17` kini nol penahan.** Satu temuan dicatat untuk layar itu: `LabOrderedProcedure` berisi **0 baris** pada database, sehingga seluruh 5 pesanan nyata menempuh jalur array kosong |
| `BE-LAB-36` ✅ | `MVP-5f` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-17. Kolom `OrderNumber` berdiri `NOT NULL` dan unik, **diterapkan ke `QuilvianNewDevYoga`**; kedelapan pesanan lama terisi `LAB-RSMMC-000001`..`000008` urut menurut `CreateDateTime`; `23505` menolak duplikat; **`POST /by-examinations` lintas 3 disiplin memperoleh 3 nomor berbeda dan berurutan**; jalur `Down` lalu `Up` menghasilkan penomoran **identik**. 9 pemeriksaan, 9 `PASS`; database kembali persis seperti semula — [laporan](../task/report/backend/BE-LAB-36.md) | Tidak ada penahan. **Satu klaim perancangan dikoreksi sesudah diuji:** `MAX + 1` menjamin nomor tidak kembali **selama barisnya tetap ada di tabel**; penghapusan **fisik** baris tertinggi mengembalikan nomornya. Aplikasi nol pernah menghapus `LabOrder` secara fisik — nol `Remove`, nol endpoint `DELETE`. **Nomornya belum dapat dibaca siapa pun di luar backend:** usul `r16` ditulis lengkap pada laporan bagian 7 dan **menunggu persetujuan pemilik modul**. **`QBE-CODE-006` tidak terpenuhi** — nol provider alokasi bersama dan nol retry `23505`; disebut apa adanya |
| `BE-LAB-37` ✅ | `MVP-5g` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-17. `r16` dilaksanakan: `orderNumber` terbaca pada `GET /lab-orders` **dan** daftar pantau — jalur yang benar-benar dipakai ketiga menu; `collectedAt` opsional membuat `VAL-59` dapat menyala. 5 pemeriksaan runtime: `422` kata demi kata sesuai matriks, rentang sah diterima, dan **muatan lama tanpa `collectedAt` terbukti tetap diterima** — [laporan](../task/report/backend/BE-LAB-37.md) | **Temuan yang menghemat pekerjaan:** `VAL-59` sudah terimplementasi benar sejak `BE-LAB-22`; yang kurang hanya pembandingnya, diteruskan sebagai `collectedAt: null` secara harfiah. **Batas dilaporkan:** `collectedAt` dipakai sebagai pembanding dan **belum disimpan** — menyimpannya ke `LabSpecimen.CollectedAt` akan ditimpa tindakan pengambilan. Tiga pilihan menunggu keputusan pemilik modul |
| `BE-LAB-38` ✅ | `MVP-5g` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-17. `r17` dilaksanakan: `GET /lab-specimens` berdiri berpaging, berpenyaring status dan pencarian. **Butir penentunya dibuktikan DUA ARAH atas satu wadah yang sama** — tiba 15 Sep, dicatat 17 Sep: muncul pada rentang 15 Sep, **tidak muncul** pada rentang 17 Sep. 6 pemeriksaan, 6 `PASS` — [laporan](../task/report/backend/BE-LAB-38.md) | **`FE-LAB-12` terbuka karenanya.** Sisa: pencarian bebas menjangkau barcode dan nomor order; nama pasien dan No. RM **ditampilkan tetapi belum ikut dicari** — menuntut `join` yang belum diukur pada daftar besar |
| `BE-EXT-04b` ✅ | `MVP-5b` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-15. Susulan atas `BE-EXT-04`: jalur **tulis** kedua ruas kiosk dibuka. Controller sebenarnya dipanggil — `TargetService=Laboratory` tersimpan dan terbaca kembali, penyaring `GET /options` menemukan **1** sesi dari sebelumnya selalu **0**, nilai di luar daftar ditolak `400`, dan **muatan lama terbukti tetap diterima** — [laporan](../task/report/backend/BE-EXT-04b.md) | Tidak ada jalur **ubah** sesi kiosk; bila kiosk menanyakan layanan sesudah kartu dipindai, diperlukan satu jalur tersendiri milik `registration-management` |
| `BE-EXT-05` | `MVP-5b` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-17, atas wewenang Andry Zain (pilihan A `LAB-REQ-012`). Kedua penahan **terbantah**: `LAB-OPEN-025` menyebut `EncounterIntakeService` yang bukan jalur kiosk — `POST /patient-encounters/kiosk` sudah ada dan nol memikul `AccessPermission`; `LAB-OPEN-026` menyebut `IsAvailableForKiosk` yang nol dibaca jalur pembentukan kunjungan, sedangkan `IsAvailableForRegistration` pada `SU-LAB-001` bernilai `true`. Yang benar-benar dibangun hanya butir 4 `BR-46` — penutupan otomatis 21:00 WIB | **Satu butir DoD belum terbukti** — pembuktian empat cabang dengan baris nyata ditolak penjaga izin sesi (database bersama). Selektivitas penyaring **sudah** diukur: 0 / 14 / 157 (91 `WaitingForNurse`). Sisa di luar modul: `IsAvailableForKiosk` milik `master-data`, dan pukul 21:00 belum dikonfirmasi |
| `BE-LAB-20` ✅ | `MVP-5a` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-14. Build 0 Error 0 Warning; 8 endpoint berdiri; migration diterapkan ke `QuilvianNewDevYoga`; 7 baris terisi; `T-M3` dan `VAL-61` terbukti `23505`; jalur `Down` lalu `Up` dibuktikan — [laporan](../task/report/backend/BE-LAB-20.md) | Tidak ada. `T-M4` menunggu foreign key dari `BE-LAB-21`; `VAL-63` terverifikasi source, pembuktian runtime menunggu aplikasi dijalankan |
| `BE-LAB-21` ✅ | `MVP-5a` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-15. Build 0 Error; migration diterapkan ke `QuilvianNewDevYoga`; `T-M2`, `T-M4` (`23503`), dan jalur `Down` lalu `Up` terbukti; `T-63c` terbukti nol pembandingan volume — [laporan](../task/report/backend/BE-LAB-21.md) | Tidak ada penahan kode. `µL`, `blok`, dan `slide` menunggu Master Data — `mL` dan `gram` **sudah ada**. Layar wadah menjawab `422` sampai `FE-LAB-11` selesai, dan itu konsekuensi `r7` yang sudah diterima |
| `BE-LAB-22` ✅ | `MVP-5a` | `EPIC-LAB-11` | **`SELESAI`** — bagian terakhirnya ditutup 2026-09-17 oleh `BE-LAB-37`. Semula 2026-09-15: build 0 Error; migration diterapkan dan jalur `Down` terbukti; `AC-65`, `AC-67`, dan `AC-17` terbukti — [laporan](../task/report/backend/BE-LAB-22.md) | **`VAL-59` tidak dapat ditegakkan** pada kontrak `r7`: pembandingnya, waktu pengambilan, belum ada saat wadah direncanakan. `AC-66` terpenuhi separuh dan **menunggu keputusan pemilik modul** — `LAB-CONFLICT-006`. **DITUTUP 2026-09-17:** keputusannya turun 2026-09-16 (pilihan A), amandemennya ditulis sebagai `r16`, dan `BE-LAB-37` melaksanakannya — `VAL-59` kini menyala dan terbukti menolak `422`. **`AC-66` terpenuhi PENUH**, dan `BE-LAB-22` tidak lagi `SELESAI SEBAGIAN` |
| `BE-LAB-23` ⛔ | `MVP-5a` | `EPIC-LAB-11` | **`DIBATALKAN`** — 2026-09-14 oleh `LAB-DEC-050`; nol baris source diubah dan tidak akan ada — [laporan](../task/report/backend/BE-LAB-23.md) | Tidak ada. Pekerjaannya dicabut bersama kolom Jumlah |
| `BE-LAB-24` ✅ | `MVP-5a` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-14, keempat AC terpenuhi kode yang sudah berjalan; nol baris source diubah — [laporan](../task/report/backend/BE-LAB-24.md) | Tidak ada |
| `BE-LAB-25` ✅ | `MVP-5a` | `EPIC-LAB-11` | **`SELESAI`** — 2026-09-15. Build 0 Error; `GET /other-usage` berdiri; `T-60d` terbukti terhadap database — tiga wadah `cairan kista` menjadi satu baris berjumlah tiga, dan `Cairan Kista` tetap baris tersendiri — [laporan](../task/report/backend/BE-LAB-25.md) | Tidak ada. Nol tabel, nol migration, nol permission baru |
| `BE-LAB-01` | `MVP-0` | `S15` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-01.md) | Tidak ada |
| `BE-LAB-02` | `MVP-0` | `S3` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-02.md) | Tidak ada |
| `BE-LAB-03` | `MVP-0` | `S3` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-03.md) | Tidak ada |
| `BE-LAB-04` | `MVP-0` | `S3` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-04.md) | Tidak ada |
| `BE-LAB-05` | `MVP-0` | `S3` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-05.md) | Dibangun; belum dapat dipakai sampai peran penyetuju ditetapkan manajemen |
| `BE-LAB-06` | `MVP-0` | `S11` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-06.md) | Dibangun; penanda biaya belum dapat disetel sampai pemegang `LabRejectionReason : SystemFlag` ditetapkan manajemen |
| `BE-LAB-07` | `MVP-0` | `S14` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-07.md) | Tidak ada |
| `BE-LAB-17` | `MVP-0` | `S3`, `S11` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-17.md) | Tidak ada |
| `BE-LAB-18` | `MVP-0` | `S3` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-18.md) | Tidak ada |
| `BE-LAB-19` | `MVP-0` | `S2` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-19.md) | Tidak ada |
| `BE-EXT-01` | `MVP-0` | `S14` | **`SELESAI`** — [laporan](../task/report/backend/BE-EXT-01.md) | Jalur pengisian dibangun 2026-09-08. Nilainya menunggu daftar penggolongan dari pihak klinis |
| `BE-EXT-02` | `MVP-1` | `S13b` | **`SELESAI`** — [laporan](../task/report/backend/BE-EXT-02.md) | Tidak ada |
| `BE-EXT-03` | `MVP-1` | `S13a`, `S13b` | **`SELESAI`** — [laporan](../task/report/backend/BE-EXT-03.md) | Tidak ada. Pelaksana `INT-05` menyusul 2026-09-07 lewat `BE-LAB-08` |
| `BE-LAB-08` | `MVP-1` | `S13a`, `S13b` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-08.md) | Tidak ada. Pelaksana `INT-05` dibangun di sisi Registrasi pada sesi yang sama; migration terbukti dua arah pada dev pemilik |
| `BE-LAB-09` | `MVP-1` | `S2` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-09.md) | Tidak ada |
| `BE-LAB-16` | `MVP-1` | `S2` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-16.md) | Tidak ada |
| `BE-LAB-10` | `MVP-1` | `S1a` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-10.md) | Tidak ada |
| `BE-LAB-11` | `MVP-2` | `S2` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-11.md) | Tidak ada. Migration terbukti dua arah pada dev pemilik |
| `BE-LAB-12` | `MVP-2` | `S2` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-12.md) | Tidak ada |
| `BE-LAB-13` | `MVP-2` | `S10` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-13.md) | Tidak ada. `AC-13` terbukti 2026-09-04 |
| `BE-LAB-14` | `MVP-3` | `S7` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-14.md) | Tidak ada |
| `BE-LAB-15` | `MVP-3` | `S15` | **`SELESAI`** — [laporan](../task/report/backend/BE-LAB-15.md) | Tidak ada |

**Dua puluh dua dari dua puluh dua task backend selesai per 2026-09-07.** Ketiga dependency
eksternal — `BE-EXT-01` sampai `BE-EXT-03` — dikerjakan atas instruksi pemilik modul yang
juga kontributor `master-data`; persetujuannya sudah ada sejak 2026-09-01 lewat `LAB-REQ-001`,
yang belum ada hanya pelaksanaannya.

Butir terakhir yang menggantung, **pelaksana `INT-05`**, ditutup bersamaan dengan `BE-LAB-08`
pada 2026-09-07. Ia dibangun di sisi Registrasi sebagai `EncounterIntakeService`, sehingga butir
DoD `BE-EXT-03` *"idempotensi terbukti lewat uji"* yang semula belum terpenuhi kini **terpenuhi**
— buktinya ada pada [`BE-LAB-08.md`](../task/report/backend/BE-LAB-08.md) bagian 5.

| Task | Keadaan |
|---|---|
| `BE-LAB-08` | **Selesai.** Ketiga endpoint tersedia, `AC-45` terbukti dua kali, idempotensi bersandar pada unique index tersaring, dan penolakan Registrasi diteruskan tanpa data setengah jadi |

Di luar itu tersisa satu penahan lingkungan yang berlaku untuk seluruh repository: 52 uji pada
`IntegrationTests.Postgres` menunggu database test tersendiri. Akun aplikasi tidak memiliki hak
`CREATEDB`, sehingga penyediaannya wewenang DBA. Tidak satu pun penahan itu dapat dicabut oleh
modul Laboratorium sendiri.

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
| `LabTransitionHistory` | `BE-LAB-10` | Sudah ada dan dipakai apa adanya (`CAP-04`), **kecuali satu kolom aditif** `LabExaminationId` yang dibutuhkan riwayat berlingkup pemeriksaan. Baris ini sebelumnya menyatakan tidak ada pekerjaan struktur dan bertentangan dengan kamus data bagian 4 serta bagian 6 di atas; diperbaiki 2026-09-04 |

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
| 64 | 2026-09-25 | **Gelombang `MVP-9` diturunkan dari `EPIC-LAB-15` — delapan task backend `BE-LAB-70`..`BE-LAB-77`** (bagian 6ak), di atas `LAB-API-v1` `r34`, `LAB-VAL-v1` `r12`, `LAB-PERM-v1` rev 11, `LAB-STATE-v1` `r5`, dan `LAB-INT-v1` `r4` yang disetujui pemilik modul pada hari yang sama beserta kesepuluh butir `02-backend-architecture.md` 20.10. **Seluruhnya `MENUNGGU PENDAHULU`**: gelombang ini dimulai sesudah `MVP-8`. Satu migration, milik `BE-LAB-70`. **`BE-LAB-72` — pembaca kredensial Human Resource — adalah penjaga keselamatan utama epic ini** dan sengaja **tidak** meniru keputusan `OperatingRoomCredentialResolver`, yang mengizinkan data kosong. **Langkah rilis `MVP-9d` `BLOCKED`** oleh `DEC-LAB-011` sisa, `DEC-LAB-017`, `DEC-LAB-018`, `LAB-COORD-016`, dan `UNK-P14-03` (6ak.10) — dan karena resolver fail-closed, deploy kode sebelum itu **tidak membuka** pemakaian. **`BE-LAB-68` diperluas** dengan temuan desain 20.1: keempat penulisan hasil kini menaikkan `Version` dan menjawab `409` saat bentrok — janji `r33` 28.2 yang belum benar pada kode. `BE-LAB-67`..`69` belum mulai dikerjakan, jadi perluasan ini nol membongkar pekerjaan | `DRAFT` |
| 63 | 2026-09-24 | **Gelombang `MVP-8` diturunkan dari `EPIC-LAB-14` — tiga task backend `BE-LAB-67`..`BE-LAB-69`** (bagian 6aj), di atas `LAB-API-v1` `r33`, `LAB-VAL-v1` `r11`, `LAB-PERM-v1` rev 10, dan `LAB-STATE-v1` `r4` yang disetujui pemilik modul pada hari yang sama. `BE-LAB-67` **SIAP DIKERJAKAN**; `BE-LAB-68` dan `BE-LAB-69` menunggu pendahulunya — ketiganya menyentuh `LabExaminationService`, sehingga dikerjakan berurutan. **Nol migration pada seluruh gelombang.** Yang paling perlu dijaga: `BE-LAB-67` membuat **nol analis** dapat menulis hasil sampai admin memberi izin baru, sehingga **langkah rilis 6aj.5 wajib** dan `FE-LAB-35` wajib dirilis bersama. `FR-14.9` sengaja nol task. `S4` validasi dan rilis nol task — belum lolos gerbang. **Pembukuan:** header dokumen tertinggal di `57` sejak revision 58 dan dirapikan | `DRAFT` |
| 62 | 2026-09-23 | **Celah izin `FE-LAB-27` DITUTUP, dan `BE-LAB-66` naik menjadi ✅ `SELESAI`** (bagian 6ai). Enam pasangan izin diberikan bagi jabatan Kepala Instalasi lewat API role-access. **Satu jebakan dihindari lebih dulu dan pantas diingat: endpoint itu memakai `overwriteTarget: true` — ia MENGGANTI seluruh set, sehingga mengirim hanya enam pasangan baru akan menghapus 21 izin yang sudah ada.** Set lama dibaca dan dikirim ulang utuh; `totalAllowed` 21 → 27. **Grup pemetaan sengaja nol diberi izin sendiri** sebab `[AccessPermission]`-nya menunjuk `LabPathologyCategory`, persis yang `LAB-PERM-v1` rev 7 minta — kodenya benar, dan baris registry `LabProcedurePathologyCategory` yang lahir dari `[AccessController]` justru **nol pernah dibaca siapa pun**. **`AC-192` dan `AC-193` terbukti penuh**: `GET /{id}` dua arah pada ketiga grup, dan `PATCH status` dibuktikan dari tiga sisi — keluar dari `/options`, tetap pada `GET /`, ringkasan `15→14` — lalu **dipulihkan persis**. **`AC-194` terbukti separuh, dan batasnya disebut apa adanya:** `10/4/6` cocok persis dengan kenyataan yang sudah tercatat roadmap frontend, tetapi penurunannya nol diuji sebab grup pemetaan **nol punya `DELETE`** sehingga pemetaan uji nol dapat dibatalkan pada database bersama. **Alur pendaftaran lab lewat kiosk BERJALAN** — sesi `KSC-RSMMC-00018` terbentuk dengan `targetService: 2` dan `hasPhysicianRequest: true`, dan keduanya **terbaca utuh** oleh panel sesi `FE-LAB-14`; satu baris uji beridentitas `ZZTEST9999999999` tertinggal dan didaftar terbuka. **Sapuan superadmin atas 331 route: 256 `200`, 58 `404` yang benar, 10 `405`, 6 `400` yang keenamnya tepat, dan SATU `500`** — `finance-management/master-data/bank-accounts`, akarnya proyeksi `ValueTuple` yang nol dapat diterjemahkan EF Core; **milik Finance Management, nol disentuh** sebab di luar cakupan. **Modul Laboratorium: 23 dari 23 jalur `200`, nol galat.** Satu butir menunggu keputusan pemilik: matriks memberi `Read` kepada Dokter Lab dan Petugas Lab, bukan kepada kepala instalasi, padahal layar kelola wajib memuat daftar sebelum dapat mengubah | `DRAFT` |
| 61 | 2026-09-23 | **Modul Laboratorium diperiksa dengan akun sungguhan, dan pemeriksaan itu membantah penahan yang sudah lima kali disalin** (bagian 6ah). Alamat `LAB-REQ-013` **dikoreksi**: Kepala Instalasi Laboratorium adalah **dr. Bima Prasetya, Sp.PK**, bukan dr. Arya Wicaksana, Sp.Rad — beliau Radiologi, dan manifest sendiri sudah mencatatnya begitu. **Nama yang benar bahkan sudah ada di dua dokumen lain** — `testing/readiness-report.md` dan laporan `FE-LAB-34` — sehingga yang keliru hanyalah rantai nota `DEC-LAB-011`. **Itu menjelaskan kenapa nota tak berjawab lima hari: ia di tangan yang bukan pemiliknya.** Koreksi ini juga **menjawab satu dari dua hal terbuka pada nota** — yang dipimpin adalah Instalasi Laboratorium — dan **menajamkan** yang kedua, sebab `Sp.PK` adalah satu dari tiga disiplin, sehingga menetapkan bagi Mikrobiologi dan Patologi Anatomi berarti menetapkan di luar disiplin beliau sendiri. **`Jwt:Key` NOL pernah menjadi penahan:** kuncinya sudah ada di `appsettings.Development.json`, dan aplikasi hidup pada percobaan pertama — lima laporan menandai `NOT RUN` dengan alasan yang salah, dan bentuk kesalahannya adalah penahan yang dicatat sekali lalu **disalin lima kali tanpa dicoba ulang**. **Hasil sapuan 22 jalur baca: NOL `500`** — 8 grup `200`, 11 `403`, 5 `404` yang memang benar sebab controller-nya nol punya `GET /`. **Kesebelas endpoint `BE-LAB-66` terbukti terdaftar dan ter-routing**, dibedakan dari `404` jalur yang memang tak ada. **SATU CELAH NYATA DITEMUKAN, DAN IA MENAHAN `FE-LAB-27`:** akun Kepala Instalasi ditolak `403` pada ketiga grup Patologi Anatomi padahal `LAB-PERM-v1` rev 7 menugaskannya kepada beliau; hak akses datang dari `SysAccessPolicies` per `(DepartmentId, PositionId)` dan **nol baris** ada bagi pasangan jabatan ini. **Ketiga layar dapat dibangun, tetapi orang yang layar itu dibuat untuknya nol dapat membukanya** — dan itu nol akan terlihat dari uji unit maupun build. Satu pertanyaan ikut diangkat: matriks memberi `Read` kepada Dokter Lab dan Petugas Lab, bukan kepada kepala instalasi, padahal layar kelola wajib memuat daftar sebelum dapat mengubah. **Catatan kesiapan ikut diperbarui** — `Kepala Instalasi Laboratorium` nol lagi 0 pengguna. Nol baris source disentuh; pemeriksaan **baca-saja**, nol data dev tersentuh | `DRAFT` |
| 60 | 2026-09-23 | **Papan kendali status seluruh task backend ditambahkan (bagian 6ag), dan ketiga penahan non-kode diverifikasi ulang.** **72 task pernah ada: 68 selesai** (4 berkatatan), **1 tertahan**, **3 dibatalkan/dilebur**, dan **NOL siap dikerjakan**. Angkanya diturunkan dari **laporan task** — artefak yang lahir bersama pekerjaannya — lalu dicocokkan terhadap blok roadmap, bukan sebaliknya, dan cara memperolehnya dituliskan supaya dapat diperiksa ulang. **Pemeriksaan itu menemukan sapuan 6ab memuat DUA kekeliruan, bukan nol:** `BE-LAB-46` dicatat 🧊 `DIBEKUKAN` padahal dibatalkan empat hari sebelumnya *(sudah dikoreksi revision 59)*, dan `BE-LAB-23` dicatat ✅ *"selesai, berlaporan sendiri"* padahal laporannya berisi **pembatalan**. **Keduanya lahir dari cara yang sama — menyimpulkan status dari keberadaan artefak, bukan dari isinya** — dan sapuan itu sendiri sudah menuliskan peringatan atas kelas ini sebelum jatuh ke dalamnya pada kedua tabel terakhirnya. **Ketiga penahan non-kode diverifikasi ulang terhadap bukti hari ini, dan ketiganya masih hidup — nol yang basi:** `LAB-COORD-011` masih nol gerbang pesan dan nol pustaka PDF, tujuh hari sejak verifikasi 2026-09-16 dan nol bergerak; `LAB-OPEN-039` masih nol berkas gambar berformat apa pun di seluruh folder blueprint, `evidence/` memuat empat berkas `.md` berisi uraian; `DEC-LAB-011` masih belum dijawab, lima hari sejak diajukan lewat `LAB-REQ-013`. **Ketiganya nol dapat ditutup dari sisi rekayasa, dan itu dinyatakan apa adanya** — yang ditanyakan `DEC-LAB-011` adalah siapa berwenang menilai kompetensi seseorang menyatakan angka hasil laboratorium benar, `LAB-COORD-011` adalah pengadaan milik pemilik platform, dan `LAB-OPEN-039` adalah penyerahan berkas. **Kesimpulan yang pantas dibaca dari entri ini: yang menghalangi modul ini maju bukan lagi backend.** Nol baris source disentuh pada revisi ini | `DRAFT` |
| 59 | 2026-09-23 | **`BE-LAB-66` selesai pada hari yang sama ia dibuka — ketiga grup Patologi Anatomi mencapai permukaan penuh, dan penahan `FE-LAB-27` hilang** (bagian 6af). `r32` disetujui pemilik modul lebih dulu; tanpa itu task ini berhenti pada langkah pertama, sebab kontrak yang masih draft adalah gerbang keras. Sebelas endpoint pada sembilan berkas source, **nol migration, nol entity, nol permission baru**. **Satu jalur sengaja NOL ditulis ulang:** `GetByIdAsync` grup pemetaan sudah ada sejak `BE-LAB-50` sebagai pembantu internal, dan bentuk maupun penolakan `404`-nya sudah persis yang dituntut baseline — yang berubah hanya pengubah aksesnya, sebab menulis jalur kedua untuk bentuk yang sama melahirkan dua kebenaran yang pasti bercabang. **SATU SELISIH `BE-LAB-65` DITEMUKAN DAN SENGAJA NOL DITAMBAL, dan ini butir yang paling pantas dibaca:** `LabFilterMetadataFactory.LabOrganism()` beserta kembarannya mengirim `SortOptions` berisi tiga pilihan urutan, padahal kedua query DTO-nya **nol punya ruas `SortBy`** dan daftarnya diurutkan tetap di dalam service — layar yang merendernya menampilkan kendali yang **nol mengubah satu baris pun**, dan kepala instalasi akan menyimpulkan urutannya sudah diatur. Ketiga grup PA karena itu mengirim `SortOptions` **kosong**; kedua grup Mikrobiologi nol disentuh sebab memperbaikinya menyentuh cakupan task lain **dan** menambah ruas sort pada endpoint daftar yang sudah berjalan bertentangan dengan `r32` bagian 27.6 yang baru saja disetujui. **Risiko `AccessMenuSeeder` 6ac.5 diperiksa:** `LabPathologyCategoryController` kini memuat enam method bernama `Read`, tetapi `DisplayName` dan `SortOrder` sudah seragam sejak `BE-LAB-50`; hanya `Description` yang diseragamkan di sini. **BATAS VERIFIKASI DISEBUT APA ADANYA:** kesebelas endpoint **nol pernah dipanggil** — aplikasi gagal start karena `Jwt:Key` belum dikonfigurasi, nilainya rahasia pemilik dan nol ditebak; `AC-192`..`AC-194` karena itu `NOT RUN`, bukan disimpulkan dari kode yang terbaca benar. **Satu koreksi pembukuan ikut ditutup:** blok task 6i.3 tertinggal menandai `BE-LAB-46` 🧊 `DIBEKUKAN` padahal bagian 6i.6b membatalkannya 2026-09-18 — dan sapuan status 6ab pada 2026-09-22 membaca blok itu lalu mencatatnya sebagai satu dari dua task "yang benar-benar masih terbuka". **Sapuan yang dibuat untuk menutup status stale jatuh ke dalam kelas yang sama pada baris terakhirnya.** Yang benar-benar terbuka per hari ini **satu**: `BE-LAB-41` | `DRAFT` |
| 58 | 2026-09-23 | **`BE-LAB-66` DIBUKA atas instruksi pemilik modul — permukaan baseline tiga data induk Patologi Anatomi** (bagian 6ae). Ia menutup selisih yang `BE-LAB-65` bagian 6ad.1 **sengaja catat alih-alih tambal**, supaya `FE-LAB-27` nol tersandung pada hal yang sudah diketahui — dan ini kali ketiga bentuk kegagalan yang sama muncul pada modul ini: permukaan yang nol pernah dijadikan task oleh siapa pun, sehingga sapuan yang menyisir daftar task nol akan menemukannya. Selisihnya **diperiksa ulang terhadap source hari ini**, bukan disalin dari catatan, dan pemeriksaan itu **mengoreksi catatan 6ad.1**: grup pemetaan tercatat *"kurang kelimanya"*, padahal `LabProcedurePathologyCategory` **nol punya `IsActive`** — ia baris pemetaan, bukan data induk berstatus — sehingga `PATCH /{id}/status` nol berlaku dan `GET /options` nol punya pembaca. Yang benar-benar kurang di sana **tiga**, bukan lima; totalnya **sebelas endpoint**, bukan tiga belas. **Satu ruas ringkasan diusulkan karena ia menjawab penahan yang bukan kode:** `unmappedProcedure` memberi kepala instalasi angka jenis pemeriksaan Patologi Anatomi yang belum digolongkan — pekerjaan yang `FE-LAB-28` tunggu dan yang sampai sekarang nol punya cara dihitung selain membuka daftar satu per satu. **Risiko `AccessMenuSeeder` dari 6ac.5 diperiksa dan separuh sudah tertutup:** `DisplayName` dan `SortOrder` ketiga controller sudah seragam per nama aksi, `Description` belum. **Penahannya tunggal dan bukan teknis** — amandemen `LAB-API-v1` `r32` belum disetujui; `BE-LAB-50` sudah selesai sejak 2026-09-18. Nol baris kode disentuh pada revisi ini. *(Catatan pembukuan: tabel ini melompat dari revision 57 bertanggal 2026-09-18 ke sini; pekerjaan 2026-09-21 dan 2026-09-22 tercatat sebagai bagian 6s–6ad, bukan sebagai baris revisi. Selisih itu **dicatat, tidak dibackfill** — menyimpulkan ulang urutan kejadian dari ingatan dokumen adalah persis yang dilarang catatan serupa pada roadmap frontend revision 22.)* | `DRAFT` |
| 57 | 2026-09-18 | **`BE-LAB-44` selesai — gelombang `MVP-6a` dibuka, dan cara mengisi data induk Mikrobiologi berdiri sejak hari pertama.** Dua model, dua configuration, dua service, dua controller (8 endpoint), satu migration yang **diterapkan**. **`AC-98`** kode unik: `POST` berulang `409`, dan normalisasi terbukti — `  zZtEsT-oRg  ` ditolak sama dengan `ZZTEST-ORG`. **`AC-99`** baris nonaktif **tidak hilang**: `IsActive=false` dengan `IsDelete=false`, terbaca pada layar kelola (1) dan lenyap dari pilihan analis (0) — pemisahan dua audiens yang tanpanya analis dapat memilih kuman yang sudah ditarik, **atau** kepala instalasi nol dapat mengaktifkannya kembali. **`AC-100`** nol `DELETE`: `405` pada kedua resource. Index unik pada kedua kode **berbentuk parsial**, diverifikasi pada database. **SATU SELISIH CAKUPAN DILAPORKAN, DIPUTUS LEWAT URUTAN WEWENANG:** roadmap menulis *"CRUD memakai `ApplicationDbContext` langsung — tanpa service"*, sedangkan `BACKEND_ENGINEERING_CONTRACT.md` menetapkan **Controller → Module Service → DbContext** dan `AGENTS.md` menyatakan controller yang mengakses DbContext langsung **nol memberi wewenang** pada `NEW CODE`; baris **Reuse** pada roadmap yang sama juga mengandaikan service, sebab `LabSpecimenType` punya satu. Kedua service **sengaja tidak diabstraksikan** menjadi satu: nama kolomnya berbeda, dan antarmuka bersama membuat query EF nol dapat diterjemahkan. **Dua baris uji berawalan `ZZTEST-` tertinggal dalam keadaan NONAKTIF** dan didaftar pada laporannya. **Penahan `MVP-6c` yang tersisa bukan kode**: daftar organisme dan antibiotik yang sebenarnya milik kepala instalasi bersama `DR-LAB-002` | `DRAFT` |
| 56 | 2026-09-18 | **`BE-LAB-52` selesai — gelombang `MVP-6b2` tuntas, dan DUA CACAT DITEMUKAN PENGUJIAN, bukan pembacaan.** Enam endpoint, tujuh DTO, satu service, nol migration. **`AC-136`..`AC-142` ketujuhnya terbukti terhadap aplikasi yang berjalan**, dan DoD logger terbukti **nol** memuat isi laporan maupun konteks klinis — lima kata uji dicari pada log aplikasi, nol kemunculan. **CACAT PERTAMA: `GET /suggestions` menggolongkan Imunohistokimia ke Histologi**, sebab kata kunci `HISTO` termuat di dalam kata "Imuno**histo**kimia" dan pencocokannya hanya berjalan pada nama; diperbaiki dengan mencocokkan **kode dan nama**, sehingga `LAB-IHK-ER` tertangkap `IHK` lebih dulu. Cacat itu lahir dari `BE-LAB-50` dan diperbaiki di sini karena di sinilah ia terbukti. **CACAT KEDUA: `reopen` tanpa alasan menjawab `400` dengan pesan kosong**, sedangkan `VAL-97` menetapkan `422` beserta kalimatnya bagi pengguna — `[Required]` pada DTO menembak lebih dulu sehingga aturan bisnisnya **nol pernah tercapai**, kelas kesalahan yang sama dengan penjaga `VAL-76` yang dicabut pada `r18`. **`AC-136` DIBUKTIKAN PADA PESANAN EMPAT GOLONGAN**, dan pasangan "Histologi + IHK" pada baris Risiko **dikoreksi** karena keduanya nol beririsan; hasilnya **19 pasangan keberlakuan → 15 ruas**, empat bergolongan ganda. **SATU SELISIH CAKUPAN DILAPORKAN:** rencana menulis delapan DTO, yang lahir **tujuh** — DTO kedelapan tidak didirikan hanya demi cocok dengan angka, sebab bentuk tanpa pembaca adalah pola `BE-EXT-04` yang sudah pernah dibayar modul ini. **DATA UJI TERTINGGAL DI DEV dan didaftar satu per satu** pada laporannya: empat pemetaan golongan yang **perlu ditinjau kepala instalasi**, satu pesanan `LAB-RSMMC-000009`, satu laporan berisi teks jelas-jelas palsu, satu konteks klinis, dan satu wadah | `DRAFT` |
| 55 | 2026-09-18 | **`BE-LAB-51` selesai — tiga tabel laporan berdiri, dan tiga dari empat AC-nya dibuktikan TERBALIK.** `LabPathologyReport`, `LabPathologyReportValue`, dan `LabPathologyOrderContext` beserta enum `LabPathologyFindingStatus`; migration `AddLabPathologyReport` diterapkan. **`AC-132`** ketiga index unik parsial (3/3). **`AC-133`** dibuktikan dua arah: kolom `LabPathologyReport` berjumlah tujuh dan seluruhnya dikenali, sementara pencarian terbalik `IssuedAt`/`EffectiveAt`/status lifecycle menghasilkan **0 baris**. **`AC-134`** dibuktikan terbalik: `LabExamination` nol punya kolom `Pathology*` dan `LabOrder` nol punya kolom konteks klinis — **nol `ALTER TABLE` pada tabel berisi data**. **`AC-135`** dibuktikan terbalik: `LabPathologyReport` punya tepat **satu** FK (`LabOrderId`), dan nol FK menyentuh `AnalystUserId` maupun `FinalizedByUserId`. **Yang paling dijaga: nol kolom status lifecycle** — sebab bila `Final` diperlakukan sebagai rilis, `LAB-DEC-003` bertabrakan dengan `LAB-DEC-090` pada orang yang sama. **DUA TEMUAN DIBAWA KELUAR:** (1) `LabResultForm` **nol punya nilai `AnatomicPathologyNarrative`** yang diandaikan arsitektur bagian 15.9 — ia milik `BE-LAB-45` yang belum dibangun, dan nol menahan task ini; (2) **separuh `AC-135` belum dapat diuji** sebab task ini nol punya penulis — kewajiban menulis `null` alih-alih `Guid.Empty` diwariskan ke `BE-LAB-52`, dan tanpa FK, `Guid.Empty` **nol akan ditolak database**. `BE-LAB-52` naik `SIAP DIKERJAKAN` | `DRAFT` |
| 54 | 2026-09-18 | **`BE-LAB-50` dikerjakan sampai batas wewenangnya, dan dua selisih dibawa keluar alih-alih ditambal.** Empat tabel data induk berdiri beserta migration `AddLabPathologyMasterData`; **empat index unik SELURUHNYA parsial** (`AC-128` 4/4), **nol endpoint `DELETE`** pada ketiga controller (`AC-130`), dan `GET /suggestions` terbukti **nol menyimpan** (`AC-131` — satu-satunya `Add(` dan `SaveChangesAsync` berada di luar jalurnya). Build hijau 0 error; nol warning berasal dari berkas baru. **Selisih pertama, `AC-129`:** ia menulis **21** pasangan, sedangkan rinciannya sendiri berjumlah **19** — dan `LAB-EVD-003` bagian 5.6 juga menghasilkan 19. Selisih 19 terhadap 15 ruas adalah **empat** ruas yang dipakai dua golongan, bukan enam; catatan risiko benar arahnya dan salah angkanya. Seeder diisi 19 sesuai bukti, dan **nol pasangan dikarang** agar cocok dengan 21 — mengarang keberlakuan berarti memunculkan ruas pada formulir diagnostik yang nol pernah diminta siapa pun. **Selisih kedua, cakupan:** ia menulis "dua controller" sedangkan kontrak `r25` bagian 20.3 menetapkan **tiga base URL**; kontrak yang diikuti. **Satu temuan untuk frontend:** ketiga resource **nol punya `GET /{id}`** — kelas kesalahan yang sudah dibayar modul ini lewat `r6`; diusulkan sebagai `r26` bila dikehendaki, tidak ditambal sepihak. **`AC-129` DIKOREKSI menjadi 19 dan disetujui pemilik modul**, pada roadmap ini dan pada `02-backend-architecture.md` bagian 15.12 sekaligus. **MIGRATION DITERAPKAN dan seeder DIJALANKAN atas wewenang eksplisit pemilik modul**, terhadap dev miliknya sendiri; nol reset database, nol perubahan credential. **Keempat AC terbukti pada database sungguhan:** empat tabel berdiri, keempat index unik berpredikat `("IsDelete" = false)`, isi 4 / 15 / **19** dengan pecahan 3/3/3/10, dan pemetaan `0` yang memang disengaja beserta peringatan penyalaannya. **Database membuktikan aritmetikanya sendiri:** empat ruas — `MAKROSKOPIK`, `MIKROSKOPIK`, `KESIMPULAN`, `ANJURAN` — terbukti dipakai dua golongan, sehingga 15 + 4 = 19. `BE-LAB-51` naik menjadi `SIAP DIKERJAKAN` | `DRAFT` |
| 53 | 2026-09-18 | **Gerbang kontrak `MVP-6b1`/`MVP-6b2` TERBUKA.** `LAB-API-v1` `r25`, `LAB-VAL-v1` `r8`, dan `LAB-PERM-v1` rev 7 **disetujui** pemilik modul pada 2026-09-18 sore. `BE-LAB-50` naik dari ⛔ `MENUNGGU PERSETUJUAN KONTRAK` menjadi ✅ `SIAP DIKERJAKAN`. **`BE-LAB-51` dan `BE-LAB-52` nol berubah statusnya dan itu bukan kelalaian** — keduanya memang tidak pernah tertahan kontrak saja; penahan mereka adalah **pendahulunya**, dan penahan itu masih berdiri. Urutan `MVP-6b1` → `MVP-6b2` **tetap tidak boleh ditukar**. **Nol acceptance criteria, nol cakupan, dan nol dependency task yang berubah** — yang berubah hanya gerbangnya. Dicatat juga apa yang **tidak** ikut terbuka: `S4e` tetap tertahan `DEC-LAB-011`, gambar PA tetap `DEC-LAB-016`, ruas HL7 tetap `LAB-COORD-012`, cetak bahasa Inggris tetap `LAB-COORD-013` — keempatnya **BAGIAN, bukan slice**, dan nol menahan ketiga task ini | `DRAFT` |
| 52 | 2026-09-18 | **Bagian Patologi Anatomi diturunkan ulang sesudah `S4c` dirancang ulang.** `BE-LAB-46` **dibatalkan**, digantikan `BE-LAB-50`, `BE-LAB-51`, dan `BE-LAB-52`; gelombang `MVP-6b` dipecah menjadi `MVP-6b1` data induk dan `MVP-6b2` laporan. **Nol baris kode terbuang, sebab nol baris pernah ditulis** — dan itu satu-satunya sebab pembatalan ini murah. **`BE-LAB-45` DIPERSEMPIT**: ketiga kolom `Pathology*` dicabut dari cakupannya, tersisa `MicrobiologyFinding` saja. Bila task itu sudah dikerjakan, yang tertinggal tiga kolom `varchar(4000)` tanpa penulis dan tanpa pembaca — **persis `BE-EXT-04`**. **Lima belas acceptance criteria baru, `AC-128`..`AC-142`, dan lima di antaranya berbentuk ketiadaan** — nol `DELETE`, nol kolom status hasil, nol `IssuedAt`/`EffectiveAt`, nol `ALTER TABLE` pada tabel berisi data, nol tombol rilis. **`AC-129` diberi peringatan risiko tersendiri**: keberlakuan parameter berisi **21 pasangan**, bukan 15 — selisihnya justru parameter yang dipakai dua kategori, dan `Anjuran` adalah buktinya. **`AC-136` inti seluruh gelombang ini**: satu pesanan Histologi + IHK wajib menampilkan `Anjuran` **sekali**, dan satu-satunya cara membuktikannya adalah menyiapkan pesanan dua kategori yang sungguhan. **Ketiga task `MENUNGGU PERSETUJUAN KONTRAK`** — `r25`, `r8`, dan rev 7 belum disetujui | `DRAFT` |
| 51 | 2026-09-18 | **Gelombang `MVP-6` diturunkan dari `EPIC-LAB-13` — enam task backend, `BE-LAB-44` sampai `BE-LAB-49`** (bagian 6i). Seluruhnya `SIAP DIKERJAKAN`; kontrak `LAB-API-v1` `r24`, `LAB-VAL-v1` `r7`, dan `LAB-PERM-v1` rev 6 sudah `approved` 2026-09-18, sehingga nol task berstatus `TERTAHAN` oleh kontrak. **Urutan gelombang berbasis risiko, bukan abjad:** `MVP-6b` Patologi Anatomi mendahului `MVP-6c` Mikrobiologi karena PA nol bergantung pada data induk mana pun, sedangkan layar Mikrobiologi dengan daftar organisme kosong adalah layar yang **tidak dapat dipakai sama sekali**. **`BE-LAB-49` berdiri sendiri dan itu keputusan yang saya jelaskan alasannya**: ia membuktikan index unik parsial benar-benar mengizinkan hapus-lalu-pilih-ulang. Yang dipisahkan **pembuktiannya**, bukan migration-nya — memisahkan index dari tabelnya akan meninggalkan jendela waktu ketika duplikat dapat tertulis, dan itu lebih berbahaya daripada yang hendak dicegah. Modul ini sudah membayar kelas kesalahan itu lewat `LAB-CONFLICT-005`. **Tiga acceptance criteria sengaja berbentuk ketiadaan** — `AC-100` nol `DELETE`, `AC-103` nol status hasil bertambah, `AC-107` nol jalur simpan sebagian — sebab ketiganya hal yang paling mungkin ditambahkan pelaksana dengan niat baik. **Satu pekerjaan yang bukan pekerjaan programmer dicatat sebagai penahan `MVP-6c`** (bagian 6i.7): daftar organisme dan antibiotik **terisi**. `BE-LAB-44` menyediakan jalannya, bukan isinya — dan `LAB-COORD-006` serta `MST-POS-WRITE` membuktikan menyerahkannya pada "nanti diisi" adalah cara ketiganya terjadi. **Dua penahan dibawa ke seluruh gelombang:** `LAB-SRC-UNCOMMITTED` memaksa pelaksana membaca pola `S4a` dari working tree sebab ia nol ada pada commit mana pun, dan `LAB-RDY-C04` memaksa verifikasi berupa pemeriksaan sungguhan terhadap database | `DRAFT` |
| 50 | 2026-09-17 | **Utang pemeriksaan cacat tanggal ditutup, dan hasilnya lebih besar daripada yang diperkirakan.** `r18` sempat mencatat *"grup `Lab Order` dan `Lab Specimen` belum diperiksa"*; pemeriksaan itu dijalankan dan menemukan **lima endpoint** terkena, bukan dua yang diduga — daftar dan rekap pesanan, daftar dan rekap wadah, serta pantau pemakaian jenis Lainnya, seluruhnya menjawab **`500`** untuk tanggal polos `YYYY-MM-DD`. **Satu negatif palsu dicatat supaya caranya tidak ditiru:** putaran pertama menyimpulkan `lab-specimen-types` selamat, padahal yang diprobe adalah `/summary` — endpoint yang **nol menerima** ruas tanggal, sehingga parameternya diabaikan dan jawabannya `200` tanpa membuktikan apa pun. **Probe yang `200` karena tidak membaca masukannya terlihat persis sama dengan probe yang benar-benar lulus.** Aturannya dipindah ke `LabQueryDateRange` — satu tempat, enam pemanggil — karena enam salinan aturan yang sama pasti bercabang, dan cabangnya nol menimbulkan galat, hanya tanggal yang salah pada satu layar dan benar pada layar lain. Dibuktikan isinya, bukan hanya kode statusnya: pencarian satu hari pada `lab-orders` mengembalikan 3+2+3 = **8**, yaitu seluruh pesanan aktif. **Satu batas ditulis jujur:** bentuk polos dan bentuk `...Z` **bukan rentang yang identik** — yang pertama hari kalender WIB, yang kedua UTC apa adanya; keduanya sama pada data ini karena barisnya jauh dari batas hari. **`BE-LAB-41` ditambahkan sebagai task tertahan yang rancangannya sudah lengkap** (`02-backend-architecture.md` bagian 13), menurunkan `LAB-DEC-066`. **Counter telanjang ditolak dengan alasan terukur:** aturan "kegagalan tidak menambah angka" berarti kegagalan nol meninggalkan jejak, sehingga `0` tidak dapat dibedakan dari "sudah tiga kali gagal" — padahal keputusannya sendiri menyediakan tombol kirim ulang khusus untuk keadaan itu. Gantinya log dengan counter sebagai **turunan**. **Migration-nya sengaja tidak dibuat**, dan itu butir terpenting: tabelnya nol punya penulis **dan** nol punya pembaca, sehingga mendirikannya sekarang mengulang `BE-EXT-04` dengan kedua sisi kosong sekaligus | `DRAFT` |
| 49 | 2026-09-17 | **`BE-EXT-05` selesai, dan dengan itu seluruh task backend modul Laboratorium tuntas. Tetapi yang paling pantas dibaca dari entri ini bukan tasknya — melainkan bahwa kedua penahannya tidak pernah ada.** Dikerjakan atas wewenang pemilik `registration-management` **Andry Zain**, pilihan **A** pada `LAB-REQ-012`. **Kedua penahan terbantah dalam satu jam pemeriksaan, dan keduanya salah dengan cara yang persis sama: menyebut nama yang benar pada jalur yang salah.** `LAB-OPEN-025` menunjuk `EncounterIntakeService.RegisterAsync` yang memang menuntut `PatientEncounter:Create` — tetapi itu jalur **layar pendaftaran pasien lab** (`BE-LAB-08`), bukan jalur kiosk. Kiosk menempuh **`POST /patient-encounters/kiosk`**, yang **sudah ada**, dijaga hanya `[Authorize(Policy = KioskReadPolicy)]`, dan **nol** memikul `[AccessPermission]`. Route itu bahkan sudah mengerjakan hampir seluruh butir 3 `BR-46`: menerima `KioskScanSessionId`, menandai `IsFromKiosk`, membentuk nomor antrean, menandai sesi terpakai, dan menolak Penjamin Perusahaan. **Pilihan A yang baru disetujui itu sudah berdiri di source sejak sebelum diusulkan.** Ketiadaan `[AccessPermission]` diperiksa benar berarti nol pemeriksaan izin: ia `TypeFilterAttribute` yang menjalankan filter, sedangkan `AccessActionAttribute` yang dipikul route kiosk hanyalah `Attribute` metadata katalog — dan `Program.cs` nol mendaftarkan filter global. `LAB-OPEN-026` menunjuk `IsAvailableForKiosk`, kolom yang **nol disebut** `PatientEncounterController` maupun `EncounterIntakeService`; yang benar-benar dituntut adalah **`IsAvailableForRegistration`**, dan pada `SU-LAB-001` nilainya **`true`**. **Sisa yang benar-benar belum ada karena itu satu hal:** butir 4 `BR-46` — penutupan otomatis akhir hari layanan. Empat berkas baru di Registrasi, satu di Laboratorium; **nol migration, nol kolom baru, nol endpoint baru** — ketiga kolom `NoShow*` pada `RegPatientEncounter` dan `TrxQueue` ternyata sudah ada. **Penyaringnya dibangun agar tidak mungkin melebar, bukan sekadar ditulis sempit,** dan itu keputusan perancangan yang paling menentukan pada task ini: sasaran diambil dari `IEncounterContinuationProbe.TargetService` — bukan dari konfigurasi, bukan dari konstanta — sehingga **nol penjawab terdaftar berarti nol kunjungan ditutup**, dan mencabut pendaftaran penjawab membuat unit itu **berhenti** ikut ditutup alih-alih ditutup membabi buta. Antarmuka itu juga menjaga arah ketergantungan tetap satu jalan, dan itu **diukur**: `LaboratoryManagement` menyebut `RegistrationManagement` pada 6 berkas (kini 7), sebaliknya **0 dalam kode** — nol `using`, nol tipe, nol pemanggilan — dan tetap 0 sesudah task ini; satu-satunya kemunculan namanya ada di dalam komentar yang menjelaskan mengapa arahnya dijaga. **Selektivitasnya diukur terhadap `QuilvianNewDevYoga`, bukan diperkirakan, dan nol baris diubah untuk mengukurnya:** penyaring sebagaimana ditulis cocok **0** kunjungan; tanpa klausa tujuan **14**; tanpa klausa kiosk **157**, yang **91** di antaranya `WaitingForNurse` — pasien poliklinik yang sedang duduk menunggu dipanggil. Itulah harga satu klausa yang hilang, dan itulah alasan bentuk kodenya. **Empat cabang kemudian dibuktikan lewat aplikasi yang benar-benar menyala** — satu yang harus tertutup dan **tiga yang harus tetap utuh**; ketiganya bertahan, yang pertama berpindah `5 → 11` beserta antreannya, 15 kunjungan kiosk nyata dan 91 `WaitingForNurse` tidak bergeser satu pun, idempotensi terbukti, nol baris uji tertinggal. Pukul batasnya diturunkan lewat **variabel lingkungan**, sekaligus membuktikan tuntutan `LAB-DEC-059` bahwa angkanya dapat diubah tanpa rilis ulang. **Dan uji itulah yang menemukan satu cacat — persis kelas yang paling berbahaya pada modul ini.** Putaran pertama **gagal seluruhnya**: `NoShowByUserId` ber-foreign key ke `AspNetUsers`, dan aktor sistem default `Guid.Empty` menunjuk pengguna yang tidak ada. Transaksinya ter-rollback dengan benar dan penjadwalnya mencoba lagi — **nol yang tampak rusak dari luar**, sementara penutupan otomatis tidak akan pernah terjadi satu malam pun kecuali sebagai baris log yang tidak dibaca siapa pun. Perbaikannya bukan GUID karangan melainkan nilai yang jujur: kedua kolom itu **nullable**, dan `null` memang artinya. Aktor yang disetel tetapi tidak ada pun tidak menghentikan penutupan; ia dicatat dan penutupannya jalan terus, supaya satu baris konfigurasi keliru tidak mematikan seluruh fitur. **Satu peringatan sejenis dilaporkan tanpa diperbaiki:** `CancelledByUserId` juga ber-FK dan nullable, dan lolos hari ini hanya karena satu-satunya penulisnya selalu berjalan di bawah pengguna yang sedang login — ia akan muncul pada penulis berikutnya yang berjalan tanpa pengguna. **Dua hal diserahkan ke luar, ditulis supaya tidak hilang:** `PatientEncounterController.cs:671` menetapkan status kunjungan hanya dari **screening** dan tidak pernah melihat `IsDoctorRequired`, sehingga setiap kunjungan laboratorium berstatus **"Menunggu Dokter"** pada unit yang nol punya dokter — **tidak diperbaiki**, karena cabang itu berlaku bagi seluruh unit ber-`IsScreeningRequired = false` dan itu kelas pengetatan diam-diam yang mahal harganya pada `BE-LAB-21`; dan `IsAvailableForKiosk` tetap perlu dijawab `master-data`, kini **bukan sebagai penahan** melainkan sebagai syarat agar Laboratorium tampil pada daftar pilihan layar kiosk. **Pelajaran yang menguat untuk keempat kalinya dalam dua hari:** catatan penahan pada modul ini tidak pernah diverifikasi ulang terhadap source sebelum dipakai menghentikan pekerjaan — sesudah `LAB-COORD-010` dan `DATA-MST-MEASUREMENT` yang disangka sedang ditunggu padahal belum pernah diajukan, kini dua penahan yang menghentikan satu task selama dua hari ternyata **tidak ada** | `DRAFT` |
| 48 | 2026-09-16 | **`r15` disetujui dan `BE-LAB-35` selesai pada hari yang sama ia diusulkan. `FE-LAB-17` kini nol penahan, dan dengan itu seluruh backend modul Laboratorium tuntas kecuali `BE-EXT-05` yang milik modul lain.** Ruas `orderedProcedures` berdiri pada `LabOrderDetailResponse` dan **terbukti dari database**: 12 pemeriksaan, 12 `PASS`. **Empat di antaranya sengaja menguji ketiadaan, bukan keberadaan** — baris ber-`IsDelete` tidak ikut terbaca; pesanan lama mengembalikan array **kosong** dan bukan `null`; daftar terpesan tidak ikut menimpa ruas wakil; dan DTO-nya **nol memuat penunjuk**, diperiksa lewat refleksi atas propertinya. Yang ketiga pantas dicatat alasannya: proyeksi yang keliru mengikat dapat menimpa `ProcedureName` wakil dengan nama snapshot dan tetap terlihat benar pada pandangan pertama. **Satu pemeriksaan dirancang agar gagal bila implementasinya salah:** nama snapshot baris uji sengaja dibuat berbeda dari nama katalognya, sehingga proyeksi yang menoleh ke `MstProcedure` akan tertangkap. **Tiga keputusan implementasi dicatat:** dibaca dari kolom snapshot bukan katalog hari ini, supaya nama yang kelak diganti tidak mengubah dokumen yang sudah dicetak; nol penunjuk dikirim mengikuti alasan `r14` bagian 9.3; dan proyeksinya memakai sub-query, **bukan** navigation property baru — menambah navigation berarti menyentuh entity dan configuration untuk kebutuhan yang murni pembacaan. **Satu temuan diserahkan ke `FE-LAB-17` sebagai peringatan, bukan disimpan:** `LabOrderedProcedure` berisi **0 baris** pada database, sehingga seluruh 5 pesanan nyata hari ini menempuh jalur array kosong — layar cetak wajib menangani jalur itu dengan benar dan tidak memperlakukannya sebagai data rusak. Nol migration dibuat dan nol dijalankan; dua pesanan uji dihapus permanen dan hitungannya diulang dari koneksi baru — `LabOrder` 5→5, `LabOrderedProcedure` 0→0 | `DRAFT` |
| 47 | 2026-09-16 | **Gelombang `MVP-5e` ditambahkan berisi satu task tertahan, `BE-LAB-35`, dan penyebabnya adalah pola yang kini muncul untuk ketiga kalinya pada modul ini.** Pemeriksaan pra-implementasi `FE-LAB-17` menemukan bahwa **daftar pemeriksaan yang benar-benar dipesan tidak dapat dibaca siapa pun di luar backend**: `LabOrderedProcedure` berdiri sejak `BE-LAB-26`, terisi sejak `BE-LAB-27`, dan menegakkan `VAL-68` serta `VAL-69` dengan benar — tetapi **nol DTO dan nol endpoint mengembalikannya**; pencarian `ProcedureNameSnapshot` pada seluruh area `LaboratoryManagement` menghasilkan nol kemunculan. Sementara itu `LabOrder.ProcedureId` hanyalah **penunjuk wakil**, dan komentar kodenya sendiri menyatakan demikian. **Kenapa celah itu tidak memutus apa pun selama dua hari:** ketiga menu pemeriksaan adalah layar antrean yang memang tidak menampilkan isi pesanan, dan kedua aturan validasi membacanya **di dalam** backend. Yang pertama membutuhkannya adalah konsumen yang **mencetak**. **Usul `LAB-API-v1` `r15` ditulis** — satu ruas `orderedProcedures` pada `LabOrderDetailResponse`, keempat ruasnya dibaca dari kolom **snapshot** supaya nama katalog yang kelak diganti tidak mengubah dokumen yang sudah dicetak, **nol penunjuk dikirim** mengikuti alasan `r14` bagian 9.3, dan pesanan lama mengembalikan **array kosong** karena jalur `POST /lab-orders` memang tidak menghasilkan baris terpesan. **Statusnya `usulan`, belum disetujui**, dan `BE-LAB-35` sengaja ditandai ⛔ karena itu — bukan karena hambatan teknis; bahan teknisnya justru lengkap. **Keputusan pemilik modul dicatat:** `FE-LAB-17` **ditunda** sampai amandemen ini jalan, bukan diturunkan menjadi versi sebagian, karena versi sebagian berarti dokumen resmi yang menyebut satu pemeriksaan padahal pesanannya memuat beberapa — kelas bahaya yang sama dengan alasan task itu ditolak pertama kali. **Pelajarannya dicatat pada kontrak bagian 10.9:** tabel yang ditulis tanpa pembacanya tidak menghasilkan galat apa pun sampai seseorang membutuhkannya, dan task yang menghasilkan dokumen resmi wajib menelusuri setiap ruas dokumennya sampai ke sumbernya **sebelum** dimulai — karena dokumen yang salah tidak menimbulkan galat, ia hanya dipercaya orang | `DRAFT` |
| 46 | 2026-09-16 | **`BE-LAB-34` menutup celah yang ditinggalkan `r13`, dan entri ini ditulis sebagai koreksi karena memang itu adanya.** `BE-LAB-33` benar terhadap `r13`; **`r13`-nya yang menyebut DTO keliru.** Usul itu disusun dengan membaca **apa yang dibutuhkan layar**, tetapi tanpa memeriksa **endpoint mana yang layar itu benar-benar panggil** — dua pertanyaan berbeda, dan hanya yang kedua dapat dijawab dari source. Ketiga menu pemeriksaan membaca grup **`Lab Monitoring`**; `GET /lab-orders/by-discipline/{discipline}` yang menerima kelima ruas `r13` ternyata **nol dipakai frontend**. Celahnya ketahuan saat `FE-LAB-15` hendak dimulai — bukan oleh build, bukan oleh uji, bukan oleh tinjauan kontrak. **`LAB-API-v1` `r14` disetujui pemilik modul** menambahkan tiga ruas tampil pada `LabMonitoringItemResponse`, dan `BE-LAB-34` melaksanakannya: aditif, nol migration, nol permission baru. **Nol penunjuk dikirim, dan itu disengaja** — daftar pantau adalah layar baca; penunjuk hanya dibutuhkan aksi, dan aksi berjalan lewat detail pesanan yang sudah membawanya sejak `r13`. **15 pemeriksaan, 15 `PASS`**, sebelas di antaranya pemeriksaan `BE-LAB-33` yang dijalankan ulang untuk memastikan jalur `r13` tidak tersentuh. **Pelajarannya dicatat pada kontrak bagian 9.6 supaya tidak terulang:** amandemen yang menambah ruas respons wajib menyebut **endpoint dan DTO yang diverifikasi dari source konsumennya**, bukan DTO yang paling masuk akal namanya. **Yang tidak terbuang dari `BE-LAB-33`:** detail pesanan dan setiap jawaban aksi — termasuk `POST /confirm` — membawa kelima ruasnya, dan pop-up `FE-LAB-15` memakainya untuk menampilkan hasil seketika tanpa memuat ulang daftar. **`FE-LAB-15` kini nol penahan** | `DRAFT` |
| 45 | 2026-09-16 | **Bukti pelaksanaan `BE-LAB-33`. Gelombang `MVP-5d` selesai, dan `FE-LAB-15` terbuka sepenuhnya.** Kelima ruas `r13` terbaca dari database pada jalur daftar maupun detail; pesanan yang belum dikonfirmasi terbukti mengembalikan kelimanya `null` tanpa galat; dan nama konfirmator terbukti memakai **jalur yang sama** dengan `RequestedByName`, sehingga satu orang tidak terbaca dengan dua nama berbeda antar layar. Sepuluh pemeriksaan dijalankan sungguhan terhadap `QuilvianNewDevYoga`, seluruhnya `PASS`. **Aktornya sengaja pengguna nyata, bukan GUID karangan** — nama diterjemahkan dari tabel `Users`, dan aktor karangan hanya akan menghasilkan `null` yang tidak membuktikan apa pun. **Tiga jalur pembangun respons disentuh dengan isi yang sengaja berbeda,** dan alasannya ditulis pada source supaya tidak terbaca sebagai kelalaian: `MapDetailResponse` membiarkan kedua namanya kosong karena ia hanya dipakai tepat sesudah pesanan dibuat, dan pesanan yang baru lahir belum mungkin dikonfirmasi. **Satu method berhenti menjadi `static`:** `ProyeksikanDaftarAsync` kini membutuhkan `DbContext` untuk menerjemahkan kedua nama **di dalam proyeksi yang sama** — menerjemahkannya per baris akan mengubah daftar 25 pesanan menjadi 51 perjalanan ke database. **Satu warning muncul dan diperbaiki, bukan dibiarkan:** komentar merujuk `RequestedByName` sebagai `cref` dari kelas dasar, padahal ruas itu tinggal di kelas turunannya; build sempat 208 lalu kembali ke baseline 207. **Satu pemeriksaan sengaja menguji ketiadaan** — baris lain pada daftar yang sama dipastikan tidak ikut terisi nama konfirmator, karena sub-query yang keliru mengikat dapat mengisi setiap baris dengan nama yang sama dan tetap terlihat benar pada baris pertama. **Satu kejadian proses dicatat jujur:** berkas roadmap ini kembali rusak encodingnya oleh perintah penyuntingan berpola — kali ini satu lapis dan struktur barisnya utuh — lalu dipulihkan penuh dan diverifikasi baris demi baris terhadap `HEAD` sebelum pekerjaan dilanjutkan; nol pekerjaan sesi sebelumnya hilang | `DRAFT` |
| 44 | 2026-09-16 | **Gelombang `MVP-5d` ditambahkan — satu task, menurunkan `LAB-API-v1` `r13` yang disetujui pemilik modul hari ini.** `BE-LAB-33` menambahkan lima ruas respons konfirmasi: `confirmedAt`, `confirmedByName`, dan `examinerDoctorName` pada `LabOrderListResponse`; `confirmedByUserId` dan `examinerDoctorId` pada `LabOrderDetailResponse`. **Seluruhnya aditif** — nol endpoint baru, nol permission baru, nol migration, dan nol endpoint yang sudah ada berubah perilakunya. **Kenapa gelombang ini tidak diramalkan sebelumnya, dan itu pantas dicatat jujur:** `BE-LAB-30` mendirikan ketiga kolomnya dan `BE-LAB-31` mengisinya — keduanya selesai dan terbukti — tetapi baru ketika `BE-LAB-31` rampung terlihat bahwa `r12` §7.1 mendefinisikan **badan permintaan saja**, sehingga nilai yang sudah tersimpan **tidak punya jalan keluar**. Dua layar tertahan karenanya, dan `AC-94` serta `AC-95` berhenti pada "terpenuhi sebagian". Celah ini **tidak** ditemukan oleh build, uji, maupun tinjauan kontrak; ia ditemukan dengan membaca apa yang dibutuhkan layar konsumennya. **Keputusan penempatan ruasnya ditulis supaya tidak diperdebatkan ulang:** nama siap tampil ada di **daftar** karena layar tidak boleh menampilkan penunjuk (`no-uuid-display`) dan daftar yang hanya membawa penunjuk memaksa layar memanggil endpoint kedua per baris; penunjuknya ada di **detail** karena aksi lanjutan membutuhkan nilai yang dapat dikirim balik. Pembagian yang sama sudah berlaku untuk `RequestedByUserId` dan `RequestedByName` sejak `r3` — nol pola baru diperkenalkan. **Yang dibuka task ini:** `FE-LAB-15` ⛔ sepenuhnya, dan bagian tanda tangan `FE-LAB-17` ⛔ — task terakhir itu **tetap tertahan** oleh penahan lain yang tidak tersentuh `r13` | `DRAFT` |
| 43 | 2026-09-16 | **Bukti pelaksanaan `BE-LAB-32`, ditulis `build-module-backend`. Seluruh backend gelombang `MVP-5c` selesai.** `BE-LAB-32` berpindah menjadi **✅ `SELESAI`**: `VAL-74` menolak `422` dan `VAL-75` menolak `409`, pesannya dibandingkan **kata demi kata** terhadap matriks. **Dua puluh tiga pemeriksaan dijalankan sungguhan terhadap `QuilvianNewDevYoga`, seluruhnya `PASS`.** **Butir DoD "wajib dikerjakan lebih dulu" benar-benar dikerjakan lebih dulu**, sebelum satu baris pun diubah, dan angkanya mengubah penilaian risikonya: **3 pesanan** kehilangan jalur pembatalan — `Accepted` 2, `InProcess` 1, `OnHold` 0 — tetapi **nol pembatalan pesanan pernah terjadi** pada seluruh jejak audit, **nol layar frontend memanggil endpointnya** (12 berkas modul Laboratorium diperiksa read-only), dan hanya **satu pemanggil backend**. Pengetatan ini karena itu mengenai jalur yang **belum pernah dipakai satu kali pun**; risiko `Tinggi` yang ditulis roadmap adalah kehati-hatian yang benar, dan pengukurannya menunjukkan ledakannya hari ini nol. Angka 3 itu tidak diperkecil: ketiganya tidak punya jalur apa pun sampai aturan koreksi `LAB-P0-003` diputuskan. **Satu penjaga digantikan tiga, dan itu diminta `T-97a`:** dua penjaga lama yang menjawab `400` digantikan satu penjaga `VAL-75` yang menutup tujuh status dengan `409`. **Akibatnya dilaporkan apa adanya, bukan didiamkan:** pesanan yang sudah dibatalkan kini dijawab "Pesanan yang sudah diproses tidak dapat dibatalkan." — kalimat yang kurang tepat untuk keadaan itu, tetapi diambil kata demi kata dari matriks yang disetujui; memperbaikinya adalah perubahan `LAB-VAL-v1` tersendiri. **Nol kolom baru, seperti dijanjikan** — alasannya tetap `ReasonNote` pada `LabTransitionHistory`, dan terbukti **dirapikan sebelum disimpan**. **Empat bentuk alasan kosong diuji, bukan satu**, ditambah badan permintaan yang tidak dikirim sama sekali — bentuk yang akan dipakai pemanggil lama. **Setiap penolakan diikuti pemeriksaan bahwa pesanannya tidak ikut berubah**, dan kelima status tetap persis seperti semula. Tiga belas pesanan uji dibuat lalu dihapus; kebersihannya diperiksa **terpisah dari harness**. **Satu jalur kode dicatat sebagai praktis tidak lagi tercapai dan sengaja tidak dibongkar:** penyerahan fakta pembatalan ke Billing, yang tetap dipakai pembatalan tingkat wadah dan akan dibutuhkan ketika `LAB-P0-003` dijawab | `DRAFT` |
| 42 | 2026-09-16 | **Bukti pelaksanaan `BE-LAB-31`, ditulis `build-module-backend`.** `BE-LAB-31` berpindah menjadi **✅ `SELESAI`**. Endpoint `POST /lab-orders/{id}/confirm` berdiri persis seperti `LAB-API-v1` `r12` §7.1, memakai ulang hak akses `LabOrder : Update` tanpa resource permission baru. **Dua puluh pemeriksaan dijalankan sungguhan terhadap `QuilvianNewDevYoga` lewat service yang sebenarnya, seluruhnya `PASS`:** `VAL-70` dan `VAL-71` menolak `409`, `VAL-72` dan `VAL-73` menolak `422` — pesan dan kodenya dibandingkan **kata demi kata** terhadap matriks — konfirmator dan waktu terbukti diturunkan server, dan bentuk DTO permintaan terbukti hanya memuat satu ruas sehingga keduanya **tidak dapat dikirim pemanggil**. Cabang "dokter tidak aktif" pada `VAL-73` benar-benar teruji memakai dokter ber-`IsActive = false` yang ada di database, bukan dilewati. Tujuh pesanan uji dibuat lalu dihapus seluruhnya; keadaan akhir database diperiksa **terpisah dari harness** dan terbukti kembali persis: 5 pesanan, sebaran status identik, nol baris uji tersisa. **Satu celah gelombang ditutup, dan ia tidak dimiliki task mana pun.** `LAB-STATE-v1` `r3` bagian 1a menuliskan `Confirmed` → `Accepted` sebagai turunan otomatis, tetapi `BE-LAB-30`, `BE-LAB-31`, dan `BE-LAB-32` tidak satu pun menyebutnya pada cakupannya — ia jatuh di antara ketiganya. Tanpa turunan itu **mengonfirmasi pesanan justru membuatnya tidak dapat dikerjakan**, karena `StartProcessAsync` hanya menerima `Accepted`. Jebakannya **dibuktikan**: pesanan `Confirmed` ditolak `StartProcessAsync` dengan pesannya sendiri. Satu kondisi pada `LabSpecimenService` ditambah `Confirmed`, dan kedua turunan — dari `Requested` maupun dari `Confirmed` — dibuktikan lewat rantai wadah yang sesungguhnya tanpa menulis satu pun fakta tagihan. **Seluruh sepuluh tempat yang mengenumerasi status pesanan ditinjau satu per satu dan hasilnya ditulis apa adanya** pada laporan bagian 3.5; hanya satu yang perlu diubah. **Satu penahan baru dilaporkan, dan ia menahan pekerjaan orang lain:** `LAB-API-v1` `r12` §7.1 hanya mendefinisikan badan permintaan dan **tidak menambah satu pun ruas respons**, sehingga `confirmedByName`, `confirmedAt`, dan `examinerDoctorName` tidak dikembalikan endpoint mana pun — padahal `FE-LAB-15` diwajibkan menampilkan tepat ketiga nilai itu. `AC-94` dan `AC-95` karena itu **terpenuhi sebagian**, dan usul `r13` berisi lima ruas ditulis lengkap pada laporan bagian 7.2 supaya tinggal disetujui atau ditolak. **Satu kegagalan uji yang justru membuktikan aturan bisnis bekerja dicatat:** rantai wadah gagal `VAL-09` — petugas yang mengambil sampel tidak boleh menyatakan kelayakannya — dan yang salah adalah harnessnya, yang memakai satu aktor untuk seluruh langkah | `DRAFT` |
| 41 | 2026-09-16 | **Bukti pelaksanaan `BE-LAB-30`, ditulis `build-module-backend`.** `BE-LAB-30` berpindah menjadi **✅ `SELESAI`**. Tiga kolom `ConfirmedByUserId`, `ConfirmedAt`, dan `ExaminerDoctorId` berdiri pada `public."LabOrder"` dan **diterapkan ke `QuilvianNewDevYoga`**: ketiganya `is_nullable = YES` dengan `column_default = NULL`, ditambah index `IX_LabOrder_ExaminerDoctorId` dan foreign key `ON DELETE RESTRICT` ke `MstDoctor`. Jalur `Down` lalu `Up` dibuktikan, dan **5 pesanan lama utuh di keempat titik pemeriksaan** dengan sebaran status yang identik — `Requested` 2, `Accepted` 2, `InProcess` 1 — serta ketiga kolom barunya `null` pada seluruh 5 baris. Tepat satu migration `Pending` sebelum penerapan, sehingga nol migration modul lain ikut terbawa; pelajaran `BE-LAB-26` dipakai sebagai pemeriksaan **sebelum** eksekusi, bukan sesudahnya. **Satu keputusan teknis yang pantas dibaca ulang kelak:** nilai enumnya `Confirmed = 9`, bukan `3`. Kontrak menempatkan `Confirmed` *antara* `Requested` dan `Accepted` pada **alur kerja**, dan menafsirkannya sebagai urutan angka akan menggeser seluruh nilai sesudahnya — tiga dari lima pesanan yang sudah tersimpan akan berubah artinya tanpa satu baris pun berubah isinya. **Satu baris di luar daftar cakupan, dilaporkan apa adanya:** label `Dikonfirmasi` ditambahkan pada `LabFilterMetadataFactory`, karena factory itu menelusuri seluruh nilai enum — tanpa labelnya, daftar pilihan penyaring akan memunculkan satu pilihan berbahasa Inggris di tengah delapan pilihan berbahasa Indonesia. Akibatnya `GET /lab-orders/filters/metadata` kini mengembalikan 9 pilihan status, bukan 8; bentuk pesannya tidak berubah. **Satu temuan diserahkan ke `BE-LAB-31`:** `GetSummaryAsync` mencacah ke delapan ember status yang tetap sementara `TotalPesanan` mencacah seluruhnya, sehingga begitu konfirmasi dapat dijalankan, jumlah ember tidak akan lagi sama dengan totalnya. **Angka dampak `BE-LAB-32` sekalian dihitung dan dicatat pada barisnya:** `Accepted` 2, `InProcess` 1, `OnHold` 0 — 3 pesanan akan kehilangan kemampuan dibatalkan begitu `VAL-75` ditegakkan. **Satu kejadian proses dicatat jujur:** berkas roadmap ini sempat rusak oleh perintah penyuntingan baris yang salah pola dan menimpa seluruh 1786 barisnya; isinya dipulihkan penuh dan diverifikasi baris demi baris terhadap `HEAD` sebelum pekerjaan dilanjutkan, dan nol pekerjaan sesi sebelumnya hilang | `DRAFT` |
| 40 | 2026-09-15 | **Gelombang `MVP-5c` ditambahkan** — tiga task backend dan tiga task frontend menurunkan `LAB-DEC-061` dan `LAB-DEC-063` dari rekonsiliasi bukti putaran 2 atas artifact "Module Artifact - Laboratorium". Kontrak `LAB-STATE-v1` `r3`, `LAB-VAL-v1` `r6`, dan `LAB-API-v1` `r12` disetujui pemilik modul 2026-09-15, sehingga backend dan frontend boleh berjalan paralel. **`BE-LAB-30`** mendirikan status `Confirmed` beserta tiga kolom nullable — konfirmator, waktu konfirmasi, dokter pemeriksa; **`BE-LAB-31`** membangun `POST /lab-orders/{id}/confirm` dengan `VAL-70`..`VAL-73`; **`BE-LAB-32`** mewajibkan alasan pembatalan dan mempersempit statusnya lewat `VAL-74` dan `VAL-75`. **`EPIC-LAB-12` didefinisikan** pada `04-prd-to-mvp.md` bagian 17 — ia sempat dirujuk sebelum ada, dan itu ditutup pada revisi ini juga. **Yang menutup `LAB-P0-002` sebagian:** posisi `Confirmed` akhirnya ditetapkan — antara `Requested` dan `Accepted` — sesudah terbuka sejak 2026-09-01. **Satu temuan saat menulis kontrak mengurangi pekerjaan, dan ia mengoreksi catatan sesi ini sendiri:** `LAB-DEC-063` semula menuntut kolom alasan pembatalan pada `LabOrder`, padahal `LabOrderService.CancelAsync` **sudah menyimpan alasannya** sebagai `ReasonNote` pada `LabTransitionHistory` — jejak audit, tempat yang memang seharusnya — dan artifact pun tidak menampilkan alasan pembatalan pada satu pun dari 14 kolomnya. Migration turun dari empat kolom menjadi **tiga**. **Satu selisih kontrak lama ikut dikoreksi:** `r11` menulis badan permintaan `PUT /lab-orders/{id}/cancel` sebagai `—`, padahal source menerima `CancelLabSpecimenRequest?` opsional sejak jalur itu dibangun; kontraknya yang tertinggal, bukan sourcenya. **Satu-satunya pengetatan gelombang ini adalah `VAL-75`**, dan `BE-LAB-32` ditulis berisiko tinggi karenanya: pembatalan dari `Accepted`, `InProcess`, dan `OnHold` berhenti sah, sehingga hitungan pesanan pada ketiga status itu **wajib dilaporkan sebelum aturannya ditegakkan**. Dua butir DoD sengaja berbentuk ketiadaan perubahan — `T-97c` membuktikan jalur `Requested` → `Accepted` tanpa konfirmasi masih berjalan, dan `T-97b` membuktikan pembatalan yang sah tidak ikut tertutup. **Dua hal sengaja tidak masuk gelombang ini:** penguncian tombol Proses oleh status pembayaran, tertahan `LAB-COORD-010` karena jalur baca milik Billing belum ada; dan konfirmasi menjadi **wajib** sebelum `Accepted`, tertahan `LAB-OPEN-027` karena menuntut perlakuan atas pesanan yang sedang berjalan | `DRAFT` |
| 39 | 2026-09-15 | **`BE-EXT-05` dihentikan sebelum satu baris pun ditulis, dan penahannya ditutup sebagian lewat task susulan `BE-EXT-04b`.** Pemeriksaan pra-implementasi menemukan bahwa **kedua kolom yang didirikan `BE-EXT-04` tidak dapat diisi oleh siapa pun**: `CreateKioskScanSessionRequest` — satu-satunya jalur tulis sesi kiosk — nol memuat `TargetService` maupun `HasPhysicianRequest`, dan datanya membenarkan itu: **16 dari 16 sesi bernilai `null`**, termasuk yang dibuat sesudah kolomnya berdiri. Akibatnya berantai: pemicu `BE-EXT-05` "pasien selesai di kiosk bertujuan Laboratorium" tidak akan pernah menyala, dan `FE-LAB-14` akan selalu menerima daftar kosong. **Yang paling berbahaya bila diteruskan tanpa pemeriksaan ini:** penyaring penutupan otomatis yang aman (`TargetService = Laboratory`) cocok **nol baris**, sedangkan penyaring yang lebih longgar akan menyapu **15 kunjungan kiosk nyata** yang 91 di antaranya berstatus `WaitingForNurse` — pasien poliklinik yang sedang menunggu dipanggil. Itu persis kegagalan yang risiko task ini sendiri tuliskan, dan ia akan terjadi pada hari pertama, bukan sebagai kemungkinan. **`BE-EXT-04b` menutup penahan itu:** dua ruas opsional pada jalur tulis, satu pemeriksaan nilai enum, dua penyalinan ke entity. Controller yang sebenarnya dipanggil — `TargetService=Laboratory` tersimpan dan terbaca kembali, penyaring `GET /options` menemukan **1** sesi dari sebelumnya selalu **0**, nilai di luar daftar ditolak `400`, dan **muatan lama terbukti tetap diterima dengan kedua ruas tetap `null`**. Nol migration, nol baris uji tertinggal — dengan catatan cara pembersihannya berbeda: controller membuka transaksinya sendiri sehingga `ROLLBACK` dari luar mustahil, dan dua baris uji dihapus permanen lalu kebersihannya dibuktikan dengan hitungan ulang. **Satu hal yang ternyata sudah benar tanpa kode apa pun:** butir DoD "biaya pendaftaran gugur" terpenuhi dengan sendirinya — Registrasi **nol** menerbitkan fakta kelayakan tagih (penerbitnya hanya Klinis, Laboratorium, Farmasi, Radiologi), dan `DefaultRegistrationFee` hanya hidup sebagai data induk. Tidak ada tagihan yang perlu digugurkan karena tidak pernah ada yang terbit. **Dua penahan `BE-EXT-05` yang tersisa dicatat bernomor:** `LAB-OPEN-025` — apakah prinsipal kiosk yang berjalan di bawah `KioskReadPolicy` boleh membentuk kunjungan, sedangkan `EncounterIntakeService.RegisterAsync` menuntut `PatientEncounter:Create`; dan `LAB-OPEN-026` — unit `SU-LAB-001 Laboratorium Klinik` ber-`IsAvailableForKiosk = false`. **Satu ketentuan yang selama ini tidak bernilai kini ditetapkan:** `LAB-DEC-059` — hari layanan berakhir **21:00 WIB**, sebagai konfigurasi. `LAB-DEC-058` menetapkan *kapan* tetapi tidak pernah menetapkan *pukul berapa*. Angkanya ditetapkan pemilik modul Laboratorium dan **belum dikonfirmasi** terhadap jam operasional resmi maupun terhadap pemilik `registration-management`; itu ditulis terang supaya tidak terbaca sebagai SOP | `DRAFT` |
| 38 | 2026-09-15 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-28` berpindah menjadi **✅ `SELESAI`**, dan `AC-91` ditutup. Wadah kini hanya memuat pemeriksaan yang memang dipesan: `VAL-68` menolak `422` dan `VAL-69` menolak `409`, keduanya **dijalankan terhadap `QuilvianNewDevYoga`** dengan pesan yang dibandingkan **kata demi kata** terhadap matriks, bukan dinilai mirip. Permintaan yang memperoleh wadahnya ditandai `Fulfilled` beserta **tautan ke baris pemeriksaan yang mengerjakannya**; yang belum berwadah terbaca `Ordered` — menunggu wadah, bukan hilang. Satu berkas source, nol migration, nol endpoint baru, nol permission baru, nol baris uji tertinggal. **Butir DoD yang paling mudah dilewatkan dibuktikan beserta baris kontrolnya:** pesanan lama tanpa baris terpesan menerima wadah persis seperti sebelumnya, dan permintaan yang **sama persis** ditolak pada pesanan yang punya baris terpesan. Tanpa baris kontrol itu, pembuktian ketiadaan perubahan bisa lulus semata-mata karena penjagaannya tidak pernah berjalan. **Satu jebakan ditemukan saat membaca kode, sebelum penjagaannya ditulis:** `CancelSpecimenInMemory` membatalkan wadah **tanpa** membatalkan pemeriksaan di dalamnya, sehingga menegakkan `VAL-69` dari penanda `Fulfilled` akan mengunci permintaan yang wadahnya dibatalkan — permintaan dokter yang masih sah, terkunci selamanya oleh penjagaan yang dipasang untuk melindunginya. Penjagaannya dibangun membaca **keadaan yang sebenarnya** — adakah baris pemeriksaan yang masih hidup pada wadah yang masih hidup — sehingga pulih sendiri. **Satu tambahan cakupan disengaja dan dilaporkan apa adanya:** pelepasan penanda saat wadah dibatalkan tidak tertulis pada cakupan task, tetapi tanpanya penjagaan dan penanda akan menyatakan dua hal berbeda tentang permintaan yang sama. **Satu batas sengaja tidak dilewati:** pembatalan **pesanan** tidak menyentuh baris terpesan, karena menutupnya menuntut keputusan yang belum pernah diambil — apakah permintaan pada pesanan yang dibatalkan menjadi `Cancelled`. **Satu penafsiran dicatat untuk pemilik modul:** permintaan yang sudah dibatalkan diperlakukan sebagai tidak ada pada daftar terpesan, sehingga memasukkannya ke wadah ditolak `VAL-68`; matriks tidak menyebutkannya, dan penafsiran sebaliknya berarti permintaan yang sudah dicabut hidup kembali lewat pintu wadah | `DRAFT` |
| 37 | 2026-09-15 | **Keputusan pemilik modul atas satu selisih kontrak, bukan pembaruan bukti pelaksanaan.** Ruas `clinicalNote` pada `POST /lab-orders/by-examinations` **dicabut** lewat `LAB-API-v1` `r11`, menutup satu-satunya butir terbuka yang ditinggalkan `BE-LAB-27`. Dua jalan keluar diajukan — dicabut, atau diberi task tersendiri berisi satu kolom beserta migration — dan pemilik modul memilih mencabut. **Dasar yang menentukan pilihan itu ditemukan saat memeriksa, bukan saat mengusulkan:** `FE-LAB-14`, satu-satunya layar yang memanggil endpoint ini, **tidak menyebut catatan klinis sama sekali** dan tidak memuat kotak isian untuknya. Ruas itu masuk saat `r10` dirancang, tanpa peminta. **Pencabutan ini menghapus janji, bukan perilaku:** penelusuran `ClinicalNote` di seluruh modul Laboratorium menemukan **nol kemunculan**, sehingga nol baris kode berubah, nol migration, dan nol permintaan yang sebelumnya berhasil menjadi gagal. Pola ini sama persis dengan `r9` yang mencabut `Quantity` sebelum sempat dibangun — kontrak yang menjanjikan sesuatu yang tidak punya tempat disimpan lebih murah dicabut daripada dipenuhi. **Satu kelalaian pembukuan ikut ditutup:** `LAB-CONFLICT-007` masih duduk di `active_blockers` pada manifest padahal `BE-LAB-29` menutupnya pada 2026-09-15; baris itu dipindahkan ke `closed_blockers` beserta buktinya, dan manifest naik ke revision `30` | `DRAFT` |
| 36 | 2026-09-15 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-27` berpindah menjadi **✅ `SELESAI`** — inti permintaan pemilik modul kini berdiri. `POST /lab-orders/by-examinations` menerima daftar pemeriksaan lalu memecahnya menjadi satu pesanan per disiplin. **Pemecahannya dijalankan sungguhan terhadap `QuilvianNewDevYoga`**, memanggil `LabOrderService` yang sebenarnya, bukan tiruan logikanya: empat pemeriksaan lintas tiga disiplin menghasilkan **tiga** pesanan; Hemoglobin dan Kalium yang sedisiplin **berkumpul menjadi satu**, membuktikan pemecahan terjadi per disiplin dan bukan per pemeriksaan; dan penanda cito melekat **hanya pada Pewarnaan BTA**, tidak menular ke isi pesanan lain (`LAB-DEC-026`). `VAL-64` sampai `VAL-67` keempatnya menolak `422` dengan pesan yang dibandingkan kata demi kata terhadap matriks. **`T-88a` terbukti:** endpoint lama tetap mengembalikan **tepat satu** `LabOrderDetailResponse` — bukan List — dan **nol baris** `LabOrderedProcedure` ikut terbentuk. Nol migration, nol permission baru, nol baris uji tertinggal. **Satu ruas kontrak sengaja tidak dibangun dan dilaporkan apa adanya:** `clinicalNote` pada `r10` tidak dapat disimpan karena `LabOrder` **tidak memiliki kolom catatan**, sedangkan cakupan task ini nol migration. Menerima ruas itu lalu membuangnya diam-diam ditolak — pemanggil akan mengira catatannya tersimpan dan baru tahu tidak ketika seseorang mencarinya. **Ini selisih pada kontrak yang ditulis sesi ini sendiri saat merancang `r10`**, bukan temuan pada pekerjaan orang lain, dan perlu keputusan pemilik modul: dicabut, atau diberi task tersendiri. **Satu hal yang ikut terlihat:** pesanan dari endpoint lama kini berdisiplin walaupun permintaannya tidak menyebutnya — `BE-LAB-29` sedang bekerja, dan kedua task saling menguatkan tanpa saling menyentuh | `DRAFT` |
| 35 | 2026-09-15 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`. Gelombang 1 `MVP-5b` selesai.** `BE-EXT-04` berpindah menjadi **✅ `SELESAI`**: dua kolom nullable `TargetService` dan `HasPhysicianRequest` berdiri pada `TrxKioskScanSession` milik `registration-management`, diterapkan ke `QuilvianNewDevYoga` atas wewenang `LAB-REQ-006`. **`T-93a` terbukti:** ke-16 sesi kiosk yang sudah tersimpan tetap terbaca, **15 masih cocok ke pasien**, dan nol kolom baru terisi. Keduanya **tanpa nilai bawaan** — berbeda dari `ScanSource` dan `ScanStatus` pada tabel yang sama yang memang punya, karena sesi yang sudah terjadi memang tidak menyatakannya dan `column_default = NULL` adalah cara menuliskan kenyataan itu. Jalur `Down` lalu `Up` dibuktikan. **Separuh pekerjaan kartu task ternyata tidak perlu dikerjakan:** jalur baca sesi yang belum diproses **sudah ada** sebagai `GET /options?onlyUsableForRegistration=true`, lengkap dengan kolom `IsUsedForRegistration` pada modelnya. Yang ditambahkan hanya satu penyaring `targetService`; endpoint kedua sengaja tidak dibuat karena akan melahirkan **dua definisi "belum diproses"** yang dapat menyimpang diam-diam. **Satu kesalahan nyaris lolos dan pantas dicatat:** menyisipkan parameter di tengah tanda tangan memutus pemanggil yang memakai **argumen posisional**, dan yang menangkapnya hanya kebetulan ketidakcocokan tipe — seandainya tipenya sama, pemanggilan itu tetap terkompilasi sambil diam-diam mengirim nilai ke parameter yang salah. Diperbaiki menjadi argumen bernama. **Penamaan diputuskan tanpa spesifikasi pemilik modul** dan dicatat agar dapat diubah selagi murah: hari ini nol baris mengisinya. **Dua temuan gate ditutup lebih dulu:** `AC-92` dan `AC-93` tidak punya baris uji — **kejadian kedua berturut-turut** setelah `AC-83`, sehingga dicatat sebagai celah proses, bukan kelalaian sekali | `DRAFT` |
| 34 | 2026-09-15 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-26` berpindah menjadi **✅ `SELESAI`**. Tabel `LabOrderedProcedure` berdiri dan **diterapkan ke `QuilvianNewDevYoga`**: 19 kolom, 3 foreign key ber-`delete_rule = RESTRICT`, 5 index. Unique parsial `(LabOrderId, ProcedureId)` **ditolak `23505`** pada index yang benar, dan `Restrict` **ditolak `23503`** pada `FK_LabOrderedProcedure_LabOrder_LabOrderId`. Jalur `Down` lalu `Up` dibuktikan tanpa menyentuh migration modul lain, dan data lama utuh sesudahnya: 5 pesanan, 5 wadah, 7 jenis specimen. Nol baris uji tertinggal. Butir DoD **`LabExamination` tidak berubah satu baris pun** terpenuhi — berkas itu tidak muncul pada `git status`, dan unique index `(SpecimenId, ProcedureId)` yang sudah membatalkan `BE-LAB-23` tidak disentuh. **Satu penahan ditemukan saat eksekusi dan dilaporkan sebelum dilanjutkan:** `migrations list` menunjukkan **tujuh** migration `Pending`, bukan satu — enam di antaranya milik `accounting`, `rawat-inap`, dan `billing`, masuk lewat merge `origin/QuilvianIntegrationBackend` yang terjadi sesudah eksekusi database pagi ini. Karena EF menerapkan migration berurutan menurut timestamp dan milik task ini paling akhir, tidak ada cara menerapkannya tanpa menyapu keenam lainnya. Pekerjaan dihentikan; pemilik modul kemudian menyatakan **izin sudah diperoleh dari ketiga pemilik modul itu**, dan ketujuhnya diterapkan tanpa galat. **Pembukuan yang harus jujur:** izin itu disampaikan lisan dan diteruskan pemilik modul Laboratorium, sama seperti `LAB-REQ-006`. **Pelajaran pengujian dicatat:** pembuktian `Restrict` **gagal dua kali dengan cara yang terlihat seperti lulus** — pertama `25P02` karena transaksi sudah abort oleh uji unique sebelumnya, kedua `23503` tetapi pada `FK_LabSpecimen_LabOrder_LabOrderId` milik wadah, bukan milik tabel ini. Yang menyelamatkannya adalah membaca **nama constraint**, bukan hanya kode galatnya, lalu menambahkan **langkah kontrol** yang membuktikan penghapusan memang berhasil ketika baris ujinya tidak ada | `DRAFT` |
| 33 | 2026-09-15 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-29` berpindah menjadi **✅ `SELESAI`**, menutup `LAB-CONFLICT-007` dengan **satu ekspresi**: `Discipline = request.Discipline` menjadi `request.Discipline ?? procedure.LabDiscipline`. Katalognya sudah dimuat beberapa baris di atas untuk validasi, sehingga perubahan ini **tidak menambah satu query pun**. **Penutupan lubangnya dibuktikan dengan pencarian:** penelusuran `new LabOrder` dan `LabOrders.Add` di seluruh `Areas/`, `Services/`, `Controllers/`, dan `Seeders/` menemukan **tepat satu** jalur tulis — klaim "tidak ada lagi jalan menyimpan pesanan tanpa disiplin" hanya bermakna bila jalurnya memang tunggal, dan itu diperiksa, bukan diandaikan. **Bukti database yang paling berbicara:** kedua pesanan tanpa disiplin ternyata atas pemeriksaan yang **katalognya sudah tahu disiplinnya** — Glukosa Darah Sewaktu Patologi Klinik, Sitologi FNAB Patologi Anatomi. Informasinya selalu ada; yang hilang hanya penyalinannya. Katalog lab **10 dari 10 tergolong**, sehingga setiap pesanan baru akan berdisiplin apa pun jalur pemanggilnya. **Dua temuan gate ditutup lebih dulu, dan keduanya kelalaian pembukuan sesi ini sendiri:** `AC-83` **tidak punya satu pun baris uji** sejak ditulis 2026-09-14 — itulah sebab teknis mengapa pelanggarannya bertahan tanpa disadari, dan `T-83a`..`T-83d` ditambahkan untuk menutupnya; serta kepala dokumen ini yang masih menyebut manifest revision `26`, SHA `466a7127`, `LAB-VAL-v1` `r4`, dan Decisions rev `21` — seluruhnya disegarkan. **Nol permintaan yang sebelumnya berhasil menjadi gagal**: penurunan ini memperluas apa yang berhasil dengan benar, bukan memperketat apa yang ditolak, dan `AC-85` tidak dicabut. **Dua baris data lama sengaja tidak diperbaiki** — perubahan data, wewenangnya terpisah. **Pelajaran proses dicatat:** acceptance criteria tanpa baris uji adalah janji tanpa cara menagihnya | `DRAFT` |
| 32 | 2026-09-15 | **Gelombang `MVP-5b` ditambahkan** — enam task menurunkan `LAB-DEC-051` sampai `LAB-DEC-058` di bawah persetujuan lintas modul `LAB-REQ-006`. Kontrak `LAB-API-v1` `r10`, `LAB-VAL-v1` `r5`, dan `LAB-PERM-v1` rev 5 disetujui pemilik modul 2026-09-15, sehingga backend dan frontend boleh berjalan paralel. Empat task Laboratorium: `BE-LAB-26` tabel `LabOrderedProcedure` yang memisahkan **apa yang dipesan** dari **apa yang dikerjakan dari sebuah wadah**; `BE-LAB-27` endpoint pemesanan yang memecah pesanan per disiplin; `BE-LAB-28` penjagaan agar wadah hanya memuat yang dipesan; dan `BE-LAB-29` yang menutup `LAB-CONFLICT-007` dengan menurunkan disiplin pada endpoint pesanan **yang sudah ada** — lubang yang selama ini hanya ditambal frontend, dan terbukti nyata pada data: 2 dari 5 pesanan tanpa disiplin, hilang dari ketiga menu. Dua task berawalan `BE-EXT` menyentuh milik `registration-management` mengikuti pola `BE-EXT-01`..`BE-EXT-03`: `BE-EXT-04` menambahkan tujuan layanan dan jalur permintaan dokter pada `TrxKioskScanSession` secara **aditif** — 16 sesi nyata sudah tersimpan di tabel itu — dan `BE-EXT-05` membentuk kunjungan dari sesi kiosk beserta penutupan otomatisnya pada akhir hari layanan. **Risiko `BE-EXT-05` ditulis tinggi walaupun kodenya sederhana**, karena kesalahannya tidak muncul sebagai galat melainkan sebagai pasien yang pendaftarannya hilang saat ia sedang menunggu dipanggil. **Tiga butir DoD sengaja berbentuk ketiadaan perubahan** — `LabExamination` tidak disentuh, endpoint pesanan lama tidak berubah, dan pesanan tanpa baris terpesan tidak ikut diketatkan — karena `BE-LAB-21` baru saja menunjukkan berapa mahal harga pengetatan diam-diam pada endpoint yang sedang dipakai. Grafik urutan dependency ditulis; tidak ada siklus. **Satu hal dicatat di luar cakupan:** dua pesanan tanpa disiplin yang sudah ada pada `QuilvianNewDevYoga` tidak diperbaiki `BE-LAB-29`, karena mengisinya adalah perubahan data yang memerlukan wewenang tersendiri | `DRAFT` |
| 31 | 2026-09-15 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-25` berpindah menjadi **✅ `SELESAI`**, dan dengan itu **keenam task backend `MVP-5a` selesai atau ditutup**. `GET /lab-specimen-types/other-usage` berdiri sebagai satu endpoint baca dengan dua DTO dan satu method service; **nol entity, nol migration, nol tabel ringkasan, nol permission baru**. `AC-60` — acceptance criteria terakhir `LAB-DEC-040` yang masih terbuka sejak `BE-LAB-20` menandainya "belum" pada 2026-09-14 — kini **terpenuhi dan terbukti terhadap database**. **`T-60d` dijalankan terhadap `QuilvianNewDevYoga`, bukan ditelusuri pada source**: tiga wadah berketerangan `cairan kista` muncul sebagai satu baris berjumlah tiga dengan waktu pemakaian terakhir yang benar, dan satu wadah berketerangan `Cairan Kista` yang sengaja ikut disisipkan **tetap berdiri sebagai baris tersendiri** — membuktikan ejaan tidak digabung diam-diam, yang justru menjadi alasan layar ini dibuat. Seluruhnya di dalam transaksi yang di-`ROLLBACK`; nol baris uji tertinggal. **Satu pemeriksaan dijalankan yang `dotnet build` tidak akan pernah menangkapnya:** `ToQueryString()` membuktikan pengelompokannya diterjemahkan menjadi **satu pernyataan `GROUP BY` penuh di PostgreSQL** — pencacahan, nilai terbesar dan terkecil, serta pengurutannya seluruhnya di database, sehingga tabel `LabSpecimen` tidak pernah ditarik ke memori. LINQ pengelompokan yang tidak dapat diterjemahkan tetap lolos kompilasi lalu gagal saat dipanggil, dan itu sebabnya pemeriksaan ini tidak dilewati. Waktu efektifnya `COALESCE(PhysicallyReceivedAt, CreateDateTime)`, **sama persis** dengan rekap penerimaan yang ditetapkan `BE-LAB-22`, supaya dua layar tentang wadah yang sama tidak memakai batas hari yang berbeda. Satu ruas aditif `firstUsedAt` ditambahkan di luar kontrak dan dilaporkan apa adanya: keterangan berjumlah 12 yang tersebar tiga bulan berbeda maknanya dari yang terkumpul dalam satu minggu. Status `GET /other-usage` pada `contracts/api-contract.md` dikoreksi dari `Rencana (belum tersedia)` menjadi **Tersedia**; **tidak ada revisi kontrak baru**. **`FE-LAB-10` tidak lagi tertahan** — kedua prasyaratnya, `BE-LAB-20` dan `BE-LAB-25`, kini selesai | `DRAFT` |
| 30 | 2026-09-15 | **Eksekusi database `MVP-5a` putaran kedua, ditulis `build-module-backend`.** Kedua migration yang tertunda diterapkan ke **`QuilvianNewDevYoga`** dalam satu jendela atas wewenang pemilik modul yang menyebut targetnya secara tegas, sesudah `migrations list` memastikan hanya kedua migration itu yang `Pending`. **`BE-LAB-21` naik menjadi ✅ `SELESAI`**: keempat kolom terbaca dari `information_schema` — `VolumeAmount` sebagai `numeric(12,3)` — kedua foreign key ber-`delete_rule = RESTRICT`, ketiga index terbaca dari `pg_indexes`, **`T-M2`** lulus dengan 5 baris lama utuh beserta 4 keterangan lamanya, dan **`T-M4`** ditolak `23503` pada constraint yang benar — menutup butir yang digantung `BE-LAB-20` sejak 2026-09-14. **Jalur `Down` lalu `Up` dibuktikan** dengan verifikasi ulang yang tetap lulus seluruhnya; nol baris uji tertinggal karena percobaan `T-M4` dijalankan di dalam transaksi yang selalu di-`ROLLBACK`. `BE-LAB-22` tetap `SELESAI SEBAGIAN` dengan penahan yang sama, `LAB-CONFLICT-006`. **Satu dugaan blueprint terbantah oleh data sebenarnya, dan dikoreksi terang-terangan:** `DATA-MST-MEASUREMENT` mengandaikan `MstMeasurement` kosong dari satuan laboratorium, padahal **17 baris ber-`IsForLaboratory` yang aktif sudah ada** dan **`mL` serta `gram` termasuk di dalamnya**. Volume bersatuan mililiter dan gram karena itu dapat dipakai sekarang juga; yang tertahan hanya **tiga** satuan — `µL`, `blok`, `slide` — sehingga `AC-64` terpenuhi sebagian, bukan nol. Permintaan ke `master-data` **perlu ditulis ulang**: tiga baris bukan lima, dan tanpa mendikte kode karena `MstMeasurement` memakai seri tergenerasi `STN`, bukan kode buatan sendiri seperti `ML` atau `BLOK`. **Dua temuan sampingan dilaporkan, tidak diperbaiki diam-diam:** penanda `IsForLaboratory` jauh lebih longgar daripada yang diandaikan rancangan — `GALON`, `M3`, `KG`, dan `Liter/Jam` ikut membawanya, sehingga galon sah dipilih sebagai satuan volume sebuah tabung darah — dan ada **dua baris gram bersimbol sama**. `VAL-57` sengaja **tidak** dipersempit untuk menutupinya, karena penyaring tambahan berarti mengarang aturan yang tidak pernah diputuskan siapa pun dan justru akan menolak `blok` dan `slide` yang diminta `AC-64`. **Konsekuensi yang sudah diterima pemilik modul sebelum penerapan:** layar wadah pada `QuilvianNewDevYoga` kini menjawab `422` sampai `FE-LAB-11` selesai | `DRAFT` |
| 29 | 2026-09-15 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-22` berpindah menjadi **`SELESAI SEBAGIAN`**. Kolom `PhysicallyReceivedAt` beserta indexnya berdiri, migration dibuat, build bersih **0 Error** tanpa satu pun warning dari berkas task ini. **Tiga acceptance criteria terpenuhi dan dua di antaranya terbukti tanpa menunggu apa pun.** `AC-65` terbukti **secara struktural** — nol ruas permintaan bernama `ReceivedAt` pada seluruh DTO Laboratorium, dan satu-satunya jalur tulisnya diisi server; pembuktian seperti ini lebih kuat daripada satu percobaan runtime. `AC-17` terbukti **dua kali**: nol rujukan `PhysicallyReceivedAt` pada `LabWorklistService`, `LabMonitoringService`, dan `LabExaminationService`, serta nol diff pada ketiganya. `AC-67` terpenuhi pada source — rekap penerimaan beralih ke waktu nyata dengan cadangan `CreateDateTime` supaya wadah lama tidak berubah perilakunya, dan selisih kedua waktu tercatat pada jejak audit berdampingan dengan `OccurredAt` yang menjadi pembandingnya. **Satu butir tidak terpenuhi, dan sebabnya bukan pada implementasi.** `VAL-59` membandingkan waktu penerimaan fisik terhadap **waktu pengambilan**, tetapi satu-satunya permintaan yang membawa waktu penerimaan fisik adalah `PlanLabSpecimenRequest`, dan pada saat wadah direncanakan `CollectedAt` masih kosong karena diisi server pada tindakan pengambilan. Pembandingnya belum ada. Menegakkannya pada tindakan pengambilan **justru menolak skenario yang menjadi alasan `LAB-DEC-042` dibuat** — wadah tiba Senin 21.10, diregistrasi Selasa 08.05 — karena `CollectedAt` di sana adalah waktu petugas menekan tombol di laboratorium, hampir selalu lebih akhir daripada waktu kedatangan. `AC-66` karena itu **terpenuhi separuh**; `VAL-58` tegak penuh. Aturannya tetap ditulis utuh di dalam `ResolvePhysicalReceipt` dan akan langsung menyala begitu kontrak memberi jalan bagi waktu pengambilan yang dinyatakan petugas. Tiga pilihan disiapkan untuk pemilik modul — tambah ruas lewat `r10`, persempit, atau cabut seperti `VAL-60` — dan **tidak satu pun dipilih sendiri**. **Ini temuan ketiga dengan pola yang sama** setelah `BE-LAB-23` dan `BE-LAB-24`: acceptance criteria dari amandemen `LAB-EVD-001` yang lolos sampai tahap implementasi tanpa pernah diadu dengan model data yang sudah berjalan | `DRAFT` |
| 28 | 2026-09-15 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-21` berpindah menjadi **`SELESAI SECARA KODE`**. Empat kolom nullable berdiri pada `LabSpecimen` — `SpecimenTypeId`, `SpecimenTypeOtherNote`, `VolumeAmount` bertipe `numeric(12,3)`, dan `VolumeUnitId` — beserta dua foreign key `RESTRICT` ke `LabSpecimenType` dan `MstMeasurement` dan dua index. `VAL-51` sampai `VAL-57` ditegakkan `ResolveSpecimenMaterialAsync` dengan pesan yang dibandingkan kata demi kata terhadap `LAB-VAL-v1` `r4`. **`T-63c` terbukti dengan pencarian yang tidak menemukan apa-apa:** nol `<`, `>`, `<=`, maupun `>=` mengenai `VolumeAmount` pada seluruh source aplikasi, sehingga `RULE-021` tegak. Build bersih penuh **0 Error**, dan nol dari 191 warning repository berasal dari kelima berkas task ini. **Tiga batas ditulis apa adanya.** Pertama, migration **dibuat tetapi belum diterapkan** — wewenang eksekusi adalah keputusan terpisah yang belum diminta, sehingga butir DoD *"migration jalan maju dan mundur"* belum terpenuhi dan `T-M2`, `T-M4`, `T-58c`, `T-59c`, `T-61a` ikut menunggu. Kedua, verifikasi volume tetap tertahan `DATA-MST-MEASUREMENT`: selama lima baris `MstMeasurement` ber-`IsForLaboratory` kosong, setiap upaya mengisi volume ditolak `VAL-57` karena tidak ada satuan laboratorium yang dapat dipilih — **menunggu data, bukan menunggu kode**. Ketiga, `specimenTypeId` kini **wajib** sesuai `r7`, sehingga layar wadah yang sudah berjalan akan menjawab `422` begitu migration diterapkan; urutannya perlu dikoordinasikan dengan `FE-LAB-11`, bukan ditutupi dengan melonggarkan backend. **Satu pembacaan kontrak dilaporkan, bukan diputuskan diam-diam:** hanya `specimenTypeId` yang dibuat wajib, karena `LAB-VAL-v1` `r4` tidak memiliki satu pun aturan yang menolak volume yang tidak diisi — `VAL-56` hanya menolak volume **tanpa satuan**. **Satu koreksi pembukuan:** ruas `Roadmap revision` pada kepala dokumen ini tertinggal pada `15` padahal bagian 9 sudah mencapai `27`; keduanya diselaraskan pada revisi ini | `DRAFT` |
| 27 | 2026-09-14 | **Gelombang `MVP-5a` ditambahkan** — `EPIC-LAB-11` Penerimaan Sampling/Specimen, enam task `BE-LAB-20` sampai `BE-LAB-25`. Kontrak `LAB-API-v1` `r7`, `LAB-VAL-v1` `r4`, dan `LAB-PERM-v1` rev 4 disetujui pemilik modul hari yang sama, sehingga backend dan frontend boleh berjalan paralel. Satu tabel baru `LabSpecimenType` berprefix `Lab`; lima kolom nullable pada `LabSpecimen`; **`LabExamination` tidak berubah sama sekali** karena Qty memperbanyak baris di lapisan service. `BE-LAB-24` berdisposisi `EXISTING / REUSE` — `VAL-18` sudah berjalan pada `LabExaminationService.cs:123@466a7127`, pekerjaannya memeriksa jalur hapus dan menulis penjaganya. `BE-LAB-21` dicatat **menunggu data, bukan menunggu kode**: lima baris `MstMeasurement` ber-`IsForLaboratory` adalah pekerjaan `master-data`, dan tanpa itu verifikasi volumenya tidak dapat dijalankan. Grafik urutan dependency ditulis untuk `MVP-5a`; ketiadaannya pada `MVP-0`..`MVP-3` dicatat sebagai gap, tidak diturunkan ulang dari tebakan. `FR-11.9` dan `FR-11.10` **tidak diberi ID task sama sekali** karena berstatus `OPEN DECISION` | `DRAFT` |
| 26 | 2026-09-08 | **Amandemen `LAB-API-v1` `r6` disetujui pemilik modul dan dikerjakan.** Satu endpoint baca ditambahkan: `GET /lab-rejection-reasons/{id}`. Grup ini semula satu-satunya grup Laboratorium tanpa jalur detail, sehingga formulir ubah `FE-LAB-03` memuat barisnya dari halaman daftar yang sedang terbuka — bekerja selama barisnya masih ada di halaman itu, dan **diam-diam gagal** pada tautan langsung, muat ulang halaman, atau sesudah petugas berpindah halaman daftar; formulirnya terbuka kosong tanpa satu pun pesan. Amandemennya **aditif**: tidak satu pun endpoint, ruas, atau nilai enum yang berubah, berganti nama, atau hilang. Empat berkas berubah bersamaan supaya kontrak, penjaga, dan source tidak dapat menyimpang — kontrak naik ke `r6` dengan barisnya, penjaga `ControllerPengelolaan_MemakaiBaseRouteYangDikunciKontrak` naik 7 → 8 dengan komentar menyebut `r6` sebagai sumbernya, service memperoleh `GetByIdAsync` yang baca-saja dan tanpa penelusuran, dan controller memperoleh action-nya dengan hak akses `LabRejectionReason : Read` yang sama dengan daftarnya. Lima uji menjaganya: detail membawa kedua penanda sistem, penunjuk tak dikenal ditolak `404`, alasan terhapus tidak terbaca, alasan **nonaktif tetap terbaca** karena masih menempel pada riwayat penolakan yang tersimpan, dan jalur baca tidak meninggalkan entity terlacak. **Nomornya `r6`, bukan `r5`** seperti tertulis pada persetujuan lisan: `r5` sudah terpakai `BE-LAB-18`. Klasifikasi `TOUCHED LEGACY`; tanpa entity baru, tanpa migration, tanpa nomor bisnis | `DRAFT` |
| 25 | 2026-09-08 | **Koreksi pembukuan kontrak, ditulis manual atas instruksi pemilik modul. Bukan amandemen.** Dua selisih fakta ditutup. Pertama, `contracts/api-contract.md` masih menandai **16 endpoint** sebagai `Rencana (belum tersedia)` padahal seluruhnya sudah ada sejak `BE-LAB-04`, `BE-LAB-05`, dan `BE-LAB-06` selesai — 6 pada Lab Value Bound, 5 pada Lab Critical Bound Approval, 5 pada Lab Rejection Reason; keenam belasnya diverifikasi langsung dari controller sebelum dikoreksi. Selisih ini dicatat `FE-LAB-02` pada 2026-09-04 dan tidak pernah ditindaklanjuti. Kedua, `blueprint-manifest.md` masih mencatat `LAB-API-v1` pada **revision 3**, tertinggal dua amandemen dari dokumen kontraknya yang sudah **revision 5** — kekeliruan yang berakibat nyata, karena pembaca manifest akan mengira `GET /lab-orders` masih mengembalikan larik padahal `r5` mengubahnya menjadi `PagedResult`. **Tidak ada endpoint yang ditambah, dihapus, atau berubah bentuk**; `LAB-API-v1` tetap `r5` dan tetap terkunci. **Satu pekerjaan dihentikan pada revisi ini:** penambahan `GET /{id}` pada grup Lab Rejection Reason diimplementasikan lalu **dikembalikan**, karena uji `ControllerPengelolaan_MemakaiBaseRouteYangDikunciKontrak` mengunci jumlah endpoint grup itu pada tujuh sesuai kontrak. Menaikkan angka penjaga itu sama dengan mengamandemen kontrak terkunci lewat penyuntingan penjaganya sendiri, dan itu memerlukan persetujuan pemilik modul yang belum ada. Grup itu tetap tanpa jalur detail | `DRAFT` |
| 24 | 2026-09-08 | **Koreksi penahan `BE-EXT-01`, ditulis manual atas instruksi pemilik modul.** Penahan pengisian disiplin selama ini tercatat sebagai *"menunggu penggolongan dari pihak klinis"* saja. Telusur backend menemukan sebab kedua yang tidak pernah tercatat dan lebih menentukan: `MstProcedure.LabDiscipline` **tidak muncul pada satu pun DTO, service, maupun controller Master Data**, sehingga tidak ada jalur tulis apa pun — bukan API, bukan layar, bukan seeder. Butir DoD *"nilainya terisi"* karena itu tidak pernah dapat dipenuhi siapa pun, bahkan seandainya daftar penggolongannya sudah tersedia. Jalur pengisiannya dibangun pada revisi ini: `labDiscipline` diterima `POST` dan `PUT`, terbit pada respons daftar, detail, dan opsi beserta labelnya, masuk ke `filters/metadata`, dan muncul sebagai pilihan **Disiplin Laboratorium** pada layar Master Data → Prosedur. Daftar disiplin yang sah diturunkan dari enum `LabDiscipline`, bukan disalin, supaya Master Data tidak dapat menerima golongan yang tidak dikenali Laboratorium. Golongan pada tindakan non-laboratorium ditolak, bukan dikosongkan diam-diam. Delapan uji backend dan enam uji frontend menjaganya; `dotnet build` 0 error, 424 uji backend dan 539 uji frontend lolos. **Satu kekeliruan pembacaan ikut diluruskan:** mengisi `MstProcedure.LabDiscipline` membuat **penyaring katalog** berisi, bukan ketiga layar monitoring — `LabMonitoringService` menyaring `LabOrder.Discipline`, dan `LabOrderService` menyalinnya apa adanya dari permintaan tanpa pernah menurunkannya dari prosedur yang dipilih. Karena ruas Disiplin pada layar Buat Pesanan tidak wajib, setiap pesanan yang dibuat tanpa memilihnya tidak muncul di satu pun layar monitoring. Penurunan disiplin pesanan dari pemeriksaannya belum berpemilik task dan dicatat sebagai temuan terbuka | `DRAFT` |
| 1 | 2026-09-02 | Roadmap backend pertama. 15 task Laboratorium dan 3 task dependency eksternal disusun untuk empat gelombang. Diterbitkan setelah kelima kontrak dikunci dan penanda `STALE` pada capability map dicabut | `DRAFT` |
| 3 | 2026-09-02 | Audit diperluas ke empat dimensi lain: aturan validasi, entity, kewenangan, dan integrasi. Seluruhnya berpemilik, tetapi kutipannya jauh dari lengkap — 30 dari 50 aturan validasi tidak pernah disebut task mana pun. Yang paling berarti: `VAL-09`, aturan empat mata pada tingkat wadah, sempat tidak tersebut sama sekali dan kini dibebankan tegas ke `BE-LAB-12`. Bagian 8 diperluas menjadi lima sub-cakupan | `DRAFT` |
| 2 | 2026-09-02 | Audit cakupan endpoint dijalankan. Empat endpoint grup Lab Examination ternyata tanpa pemilik task; `BE-LAB-16` ditambahkan. Daftar endpoint pada `BE-LAB-06` dan `BE-LAB-15` ditulis eksplisit agar lubang sejenis tidak tersembunyi lagi. Bagian 8 Cakupan Endpoint ditambahkan | `DRAFT` |
| 23 | 2026-09-07 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-08` berpindah menjadi **`SELESAI`**, dan dengan itu **seluruh 22 task backend Laboratorium selesai**. Ketiga endpoint grup Lab Patient Registration tersedia: `GET /patient-search`, `POST /walk-in`, dan `POST /external-referral`. Penahannya — pelaksana `INT-05` yang selama ini belum berpemilik — dicabut pada sesi yang sama dengan membangunnya **di sisi Registrasi** sebagai `EncounterIntakeService`, atas instruksi eksplisit pemilik modul dan di bawah persetujuan `LAB-REQ-003` bagian 3b yang memang sudah mencakup idempotensi sebagai bukti selesai. `AC-45` terbukti **dua kali dengan cara berbeda**: telusur seluruh source modul Laboratorium menemukan nol pembentukan maupun pengubahan kunjungan dan data induk pasien, dan uji perilaku dengan **dua penyimpanan terpisah** menunjukkan kunjungan hanya muncul di penyimpanan milik Registrasi. `AC-44`, `AC-46`, `AC-50`, serta `VAL-41` sampai `VAL-45` terbukti lewat 18 uji. Idempotensi bersandar pada unique index tersaring `TrxPatientEncounter.RegistrationIdempotencyKey`, bukan pada kode aplikasi; migration `20260907072413_AddRegistrationIdempotencyKeyToPatientEncounter` terbukti jalan dua arah terhadap `QuilvianNewDevYoga`. Butir DoD `BE-EXT-03` *"idempotensi terbukti lewat uji"* yang semula menggantung ikut **terpenuhi**, sehingga kartu itu kini `SELESAI` seluruhnya. Dua selisih dibuka: `VAL-42` ditulis untuk pemanggilan jarak jauh sementara `INT-05` dilaksanakan sejalur proses, dan kunjungan dari jalur ini tidak membawa snapshot kategori umur seperti jalur loket | `DRAFT` |
| 22 | 2026-09-04 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-07` berpindah menjadi **`SELESAI`**: `GET /lab-catalog/examinations`, `/examinations/{procedureId}/price`, dan `/tariffs` tersedia, seluruhnya baca saja. `AC-43` terbukti — tiga pemeriksaan menampilkan harga satuan 35.000, 30.000, dan 40.000 dengan total 105.000, dan **nol** baris tagihan, fakta klinis, maupun pemeriksaan terbentuk karenanya. `AC-47` terbukti: Laboratorium tidak memiliki satu pun entity tarif; yang ada hanya penunjuk `TariffId` dan salinan `*Snapshot`. `AC-48` dan `VAL-50` terbukti lewat ketiadaan jalur ubah. `AC-51` dan `VAL-46` terbukti: menambahkan Hemoglobin ke pesanan Mikrobiologi ditolak `422`, sementara pesanan atau katalog yang belum berdisiplin tidak ikut tertolak. **Dengan ini 21 dari 22 task backend selesai**; yang tersisa hanya `BE-LAB-08`, yang menunggu endpoint `INT-05` milik `registration-management` | `DRAFT` |
| 21 | 2026-09-04 | **Dependency eksternal dikerjakan atas instruksi pemilik modul, ditulis `build-module-backend`.** `BE-EXT-01` dan `BE-EXT-02` menjadi **`SELESAI`**, `BE-EXT-03` **`SELESAI` untuk kolom dan kontrak**. Ketiganya sudah lama disetujui `andryzainhome` dan `sukmagp` lewat `LAB-REQ-001` pada 2026-09-01; yang belum ada hanya pelaksanaannya. Kolom `LabDiscipline` masuk ke `MstProcedure`, dua data induk perujuk dibuat, dan dua penunjuk perujuk masuk ke `TrxPatientEncounter`. Dua migration jalan dua arah pada dev pemilik. **Dua cacat tata kelola ikut ditemukan dan diperbaiki.** Pertama, `QBE-MOD-002` memblokir seluruh entity `Mst*` baru karena baris registry `Master / Reference` tidak cocok dengan folder `MasterData` dan `Category`-nya bukan `BUSINESS DOMAIN`; registry diperbaiki dan keputusannya dicatat. Kedua, `QuilvianSystemBackend.csproj` mengeluarkan `Migrations/**/*.Designer.cs` dan `ApplicationDbContextModelSnapshot.cs` dari kompilasi, sehingga EF hanya mengenali 7 dari 127 migration dan sempat menghasilkan satu migration ber-70.815 baris; kedua pengecualian dinonaktifkan. Penahan `BE-LAB-07` dicabut | `DRAFT` |
| 20 | 2026-09-04 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-15` berpindah menjadi **`SELESAI`**: tiga daftar pantau sejajar tersedia beserta `GET /lab-orders/by-discipline/{discipline}`. `AC-41` terbukti dengan data campuran — tidak ada satu baris pun yang menyeberang ke daftar tetangganya, dan pesanan tanpa disiplin tidak muncul di ketiganya. `AC-42` terbukti sekaligus dengan `AC-19` lewat empat uji penelusuran: nol tipe, anggota, entity, dan route Laboratorium yang menyentuh Bank Darah maupun stok reagen. Penyaingnya ditulis satu kali dan dipakai bertiga, dan `LabMonitoringQuery` sengaja tanpa ruas disiplin. **Satu ruas kontrak dicatat tidak dapat dipenuhi**: penyaring nomor pesanan menuntut kolom yang tidak ada pada `LabOrder` | `DRAFT` |
| 19 | 2026-09-04 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-14` berpindah menjadi **`SELESAI`**: `GET /lab-worklists/pending` dan `GET /lab-worklists/cito-overdue` tersedia. `AC-10` terbukti pada kedua bentuknya — satu cito pukul 10.05 berada di atas empat belas pesanan biasa pukul 10.00, dan dua cito di antara mereka sendiri urut menurut waktu masuk. `AC-17` terbukti pada kedua jalurnya, termasuk yang **tidak** muncul karena sudah selesai, dan `AC-39` terbukti pada daftar kerja. `VAL-39` berperilaku sebagaimana disepakati: cito tanpa batas waktu tetap ditampilkan tetapi tidak dianggap terlambat, dan diletakkan di bawah keterlambatan yang sesungguhnya. `FR-04.4` dijaga uji struktur: nol entity ber-nama `Worklist` dan nol jalur tulis pada grup ini. Satu turunan dicatat sebagai utang pemilik blueprint — baris `LabValueBound` mana yang menentukan batas waktu cito | `DRAFT` |
| 18 | 2026-09-04 | **Penutupan jejak audit atas instruksi pemilik modul, ditulis `build-module-backend`.** Dua baris audit untuk penandaan cito dan duplo ditambahkan ke `contracts/permission-audit-matrix.md` bagian 4. Sekaligus ditemukan lubang yang lebih besar: **keempat kejadian berlingkup pemeriksaan yang sudah tertulis pada matriks itu — `Examination.Add`, `Examination.ChargeEligible`, `Examination.Void`, dan `Examination.Cancel` — tidak satu pun pernah menulis baris riwayat**, karena kolom penunjuk dan nilai enum lingkupnya baru ada sejak `BE-LAB-10`. Keempatnya ditutup: `AddAsync` dan `CancelAsync` menulis barisnya sendiri, dan `MoveExaminationsAsync` menulis satu baris per pemeriksaan yang benar-benar berpindah. Enam uji baru menjaganya. Satu selisih dicatat terbuka: matriks menandai alasan pembatalan pemeriksaan sebagai wajib, sementara tidak ada `VAL-*` yang menuntutnya | `DRAFT` |
| 17 | 2026-09-04 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-10` berpindah menjadi **`SELESAI`**: `PUT /lab-examinations/{id}/urgency` dan `PUT /lab-examinations/{id}/duplo` tersedia, `VAL-03` menjawab `403` dan `VAL-04` menjawab `409`, `AC-18`, `AC-39`, dan `AC-40` terbukti, dan setiap penandaan menghasilkan satu baris `LabTransitionHistory` berlingkup `LabExamination`. **Pertentangan dokumen yang terbuka sejak `BE-LAB-09` diselesaikan**: bagian 8.3 yang menyatakan `LabTransitionHistory` tanpa pekerjaan struktur bertentangan dengan kamus data bagian 4 dan bagian 6 roadmap ini; kamus data yang dipakai, dan kolom `LabExaminationId` ditambahkan lewat migration aditif `AddLabExaminationIdToLabTransitionHistory` yang juga terbukti dua arah pada dev pemilik. `LabTransitionScope` bertambah nilai `LabExamination = 3`. Satu kawat pemicu dari `BE-LAB-16` ikut menyala dan diperbarui: jumlah endpoint grup pemeriksaan naik dari empat menjadi enam | `DRAFT` |
| 16 | 2026-09-04 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-11` berpindah menjadi **`SELESAI` untuk source dan migration**: `ProcedureId` beserta lima kolom salinan tarif lepas dari `LabSpecimen`, relasi ke `MstProcedure` dan index-nya ikut lepas, dan migration `SplitLabSpecimenIntoExamination` berikut skrip maju dan mundurnya dibuat. **Klaim `BE-LAB-13` bahwa tidak ada lagi yang membaca keenam kolom itu terbukti hanya separuh benar** — kolomnya masih ditulis `CreateSpecimenAsync`, masih dibaca jalur pengambilan ulang, dan masih diproyeksikan `GetByOrderAsync` beserta `MapResponse`; kelimanya dilepas pada task ini. `LabSpecimenResponse` kehilangan empat ruas sesuai `contracts/api-contract.md` bagian 3 yang sudah menyebutnya **breaking**. Prasyarat `LAB-OPEN-012` ditegakkan penjaga di dalam migration. **Migration dijalankan dua arah** terhadap `QuilvianNewDevYoga` atas instruksi pemilik modul — maju, mundur, lalu maju lagi — dan penjaganya lolos dua kali, sehingga jawaban `0` atas `LAB-OPEN-012` terverifikasi ulang oleh mesin. Eksekusi ke luar dev pemilik tetap wewenang terpisah | `DRAFT` |
| 15 | 2026-09-04 | **Penutupan bukti, ditulis manual.** `BE-LAB-13` berpindah dari `SELESAI` sebagian terverifikasi menjadi **`SELESAI`**. Kedua butir DoD yang tertunda ternyata **tidak pernah bergantung pada `QUILVIAN_BILLING_TEST_DB`**: `LaboratoryAuthorityTests` bekerja lewat refleksi tanpa satu pun koneksi database. Penghalang sesungguhnya adalah project `IntegrationTests.Postgres` yang gagal build dengan 11 `CS1061`, karena `LaboratorySpecimenLifecycleTests.cs` masih memanggil `Kind`, `MilestoneFactId`, dan `MilestoneFactVersion` langsung pada `Handoff` sesudah `BE-LAB-13` mengubah tipe kembaliannya menjadi `LabFactEmission`. Sesudah diperbaiki: `AC-13` **terbukti**, 18 dari 18. Pengisian `QUILVIAN_BILLING_TEST_DB` tetap dicoba dan **ditolak server** — akun aplikasi tidak memiliki hak `CREATEDB` (`42501`), sehingga 52 uji berbasis database tetap terhalang dan penyediaannya menjadi wewenang DBA. Tujuh assertion satuan lama pada `LaboratorySpecimenLifecycleTests.cs` dicatat terbuka | `DRAFT` |
| 14 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-13` menjadi **`SELESAI` sebagian terverifikasi**: fakta kelayakan tagih kini terbit satu per pemeriksaan, bukan satu per wadah, dan idempotensinya terbukti. Satu wadah dua pemeriksaan yang sebelumnya hanya menagihkan satu tarif kini menagihkan keduanya. **Dua butir DoD belum terpenuhi** — `LaboratoryAuthorityTests.cs` dan `AC-13` terhalang `QUILVIAN_BILLING_TEST_DB` yang belum diisi. Dependency melingkar `BE-LAB-11` ↔ `BE-LAB-13` diselesaikan dengan urutan pembaca-dulu-schema-terakhir; penyelarasan kedua kartu menjadi utang pemilik blueprint. `AC-38` pada `BE-LAB-12` yang sempat tertinggal separuh ikut dituntaskan | `DRAFT` |
| 13 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-12` berpindah menjadi **`SELESAI`**: ketiga endpoint wadah berperilaku baru, `VAL-05` sampai `VAL-15` masing-masing punya ujinya, dan `VAL-09` — aturan empat mata — ditulis sebagai kode di dalam service sesuai temuan `CAP-16`. `AC-36` terbukti: menolak wadah menggugurkan seluruh pemeriksaan yang ditopangnya. Kode status disesuaikan matriks: `422`, `409`, dan `403` menggantikan `400` yang seragam. **Urutan terhadap `BE-LAB-11` dibalik** karena keenam kolomnya masih dipakai service di delapan tempat; `BE-LAB-12` lebih dulu supaya kode berhenti memakainya, `BE-LAB-11` menyusul menghapus kolom mati. Penerbitan fakta per pemeriksaan tetap milik `BE-LAB-13` | `DRAFT` |
| 12 | 2026-09-03 | **`LEGACY MIGRATION` atas instruksi pemilik modul, ditulis `build-module-backend`.** `BE-LAB-19` ditambahkan dan langsung **`SELESAI`**: `TrxLabSpecimen` menjadi `LabSpecimen` dan `TrxLabTransitionHistory` menjadi `LabTransitionHistory`, lengkap dengan tabel fisiknya. Nol `Trx*` tersisa pada modul Laboratorium. **`LAB-OPEN-012` dijawab lebih dulu** — jumlah baris `TrxLabSpecimen` pada `QuilvianNewDevYoga` adalah **0** — sehingga tidak ada data yang berpindah; kartu `BE-LAB-11` diperbarui mengikutinya. Kartu `BE-LAB-16` juga diselaraskan: Verifikasi dan DoD-nya kini berbunyi `VAL-17` .. `VAL-20` sesuai bagian 8.2. Total task backend menjadi 22 | `DRAFT` |
| 11 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-16` berpindah dari `Siap direncanakan` menjadi **`SELESAI`**: empat endpoint pemeriksaan terpesan tersedia, dan `VAL-17` sampai `VAL-20` masing-masing punya ujinya. Tanpa migration. Satu selisih penulisan dicatat: kartu `BE-LAB-16` menyebut `VAL-05` dan `VAL-07` pada Verifikasi dan DoD-nya, padahal bagian 8.2 menempatkan keduanya pada `BE-LAB-12`; yang dikerjakan adalah `VAL-17` .. `VAL-20` sesuai bagian 8.2, dengan larangan jenis pemeriksaan ganda per wadah tetap ditegakkan. Penyelarasan kartu itu menjadi utang pemilik blueprint | `DRAFT` |
| 10 | 2026-09-03 | **Task baru atas instruksi pemilik modul, ditulis `build-module-backend`.** `BE-LAB-18` ditambahkan dan langsung **`SELESAI`**: `GET /lab-orders` memperoleh penyaring, pengurutan, dan pagination di sisi server. `LAB-API-v1` naik ke `r5`, dan ini **satu-satunya perubahan breaking** pada modul Laboratorium sejauh ini — bentuk respons berubah dari `List<T>` menjadi `PagedResult<T>`. Dampak konsumen dinilai: satu-satunya konsumen yang ditemukan, modul IGD, tidak putus karena pembungkusnya sudah menangani kedua bentuk, tetapi perlu mengirim `?encounterId=` agar tidak kehilangan pesanan di luar halaman pertama. Perubahan ini sekaligus menutup `IGD-DEC-105`. Dua deskripsi endpoint Swagger terakhir pada `LabSpecimenController` juga dihapus, sehingga seluruh grup Laboratorium kini bersih. Total task backend menjadi 21 | `DRAFT` |
| 9 | 2026-09-03 | **Task baru atas instruksi pemilik modul, ditulis `build-module-backend`.** `BE-LAB-17` ditambahkan dan langsung **`SELESAI`**: sepuluh endpoint `filters/metadata` dan `summary` pada kelima grup Laboratorium, menyamakan bentuknya dengan modul Rekam Medis. `LAB-API-v1` dinaikkan dari `r3` ke `r4`; amandemennya aditif sepenuhnya. Total task backend menjadi 20. Satu temuan dicatat: `GET /lab-orders` dan daftar wadah belum menerima parameter query apa pun, dan metadata keduanya menyatakan itu apa adanya alih-alih mengarang penyaring — menambahkan penyaringan sungguhan belum berpemilik task. Di luar task, empat deskripsi endpoint Swagger dihapus atas permintaan pemilik modul: tiga pada `LabOrderController` dan satu pada `MedicalRecordAccessLogController` | `DRAFT` |
| 8 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-09` berpindah dari `Siap direncanakan` menjadi **`SELESAI`**, sekaligus mencabut penahan `BE-LAB-16` dan `BE-LAB-10`. Entity `LabExamination` ada dengan nama yang benar, dan migration `20260903071535_AddLabExamination` terbukti jalan dua arah terhadap `QuilvianNewDevYoga`. Dua hal dicatat terbuka: keenam kolom yang harus pindah dari `TrxLabSpecimen` **belum dipindahkan** karena `BE-LAB-11` masih `BLOCKED`, sehingga salinan tarif untuk sementara ada di dua tempat; dan `erd/data-dictionary.md` bagian 4 bertentangan dengan bagian 8.3 roadmap ini soal `TrxLabTransitionHistory`. Di luar task, atas permintaan pemilik modul, komentar XML pada method action ketiga controller Laboratorium dihapus agar tidak lagi tampil sebagai deskripsi endpoint di Swagger | `DRAFT` |
| 7 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `BE-LAB-06` berpindah dari `Siap direncanakan` menjadi **`SELESAI`**: lima endpoint grup Lab Rejection Reason tersedia, seeder data awal terdaftar, dan `VAL-36` sampai `VAL-38` terbukti lewat 34 uji. Task ini tidak menyentuh schema sehingga tanpa migration. Satu risiko organisasi dibuka: pemegang `LabRejectionReason : SystemFlag` belum ditetapkan, sehingga penanda biaya belum dapat disetel lewat aplikasi. Tujuh selisih terhadap `master-data-endpoint-standard.md` dan `LAB-API-v1` r3 dicatat terbuka pada laporan bagian 3.4 — yang terpenting: grup ini sengaja lima endpoint, bukan sembilan, karena kontraknya mengunci demikian dan `FE-LAB-03` tidak mengonsumsi sisanya | `DRAFT` |

---

## 6l. `BE-LAB-55` BERUBAH CAKUPAN untuk ketiga kalinya — 2026-09-21

Menurunkan [`02-backend-architecture.md`](../02-backend-architecture.md) **bagian 18** dan
`LAB-API-v1` `r28`.

| Putaran | Cakupan `BE-LAB-55` |
|---|---|
| `MVP-7` semula | Dua tabel, delapan endpoint, **nol data awal** (`LAB-OPEN-040` menahannya) |
| Sesudah `LAB-EVD-007` | Dua tabel, delapan endpoint, **plus tiga kolom dan satu seeder 1.767 baris** |

### Yang bertambah

1. **`LabSpecimenType` di-seed ulang** dari 7 menjadi **31** nilai — GUID ketujuh baris lama
   **dipertahankan**, hanya namanya diselaraskan.
2. **Tiga kolom** pada `LabSpecimenDetailType`: `SubTypeName`, `SnomedCode`, dan
   `DetailTypeNameId` dilonggarkan menjadi nullable.
3. **`LabSpecimenDetailTypeSeeder`** membaca berkas CSV **tertanam** berisi 1.767 baris —
   1.601 aktif, 166 nonaktif.
4. **Empat penyaring** pada endpoint daftar, termasuk `untranslatedOnly`.

### ⚠ Dua hal yang wajib dibaca pelaksana

> **Seeder ini NOL BOLEH memperbarui baris yang sudah ada.** Ia hanya menyisipkan yang kodenya
> belum ada. Seeder yang "menyegarkan" isinya setiap kali aplikasi menyala akan **menghapus
> seluruh pekerjaan penerjemahan** kepala instalasi setiap restart. Ini kesalahan yang nol
> menimbulkan galat dan baru ketahuan berminggu-minggu kemudian.

> **Berkas CSV tertanam adalah pola PERTAMA di repository ini.** Ia dinyatakan terbuka pada
> bagian 18.2 beserta alasannya, bukan diselundupkan. Bila pemilik modul menolak polanya,
> jalan keluarnya `InsertData` pada migration — dan itu lebih buruk, sebab 1.767 baris masuk
> ke berkas yang nol boleh disunting lagi sesudah diterapkan.

### Acceptance criteria yang bertambah

`AC-192` penyaringan Spesifik Specimen per kelompok beserta `subjenis` yang ikut tersimpan;
`AC-193` 1.601 aktif dan 166 nonaktif sesudah impor; `AC-194` baris tanpa nama Indonesia tampil
berbahasa Inggris dan muncul pada penyaring belum-diterjemahkan; `AC-195` baris `Lainnya`
tersimpan dengan `SnomedCode` kosong dan tetap sah.

### Verifikasi

Pemeriksaan sungguhan terhadap database: jumlah baris **tepat 1.767**, yang aktif **tepat
1.601**; menjalankan aplikasi **dua kali** menghasilkan jumlah yang **sama** dan nama Indonesia
yang sudah diisi **tidak berubah**; index unik `SnomedCode` **parsial** sehingga dua baris
berkode SNOMED kosong tetap diterima.

---

## 6m. `BE-LAB-45` DILEBUR ke `BE-LAB-53`, dan `BE-LAB-53` DIKERJAKAN — 2026-09-21

### Kenapa dilebur

Gate `BE-LAB-53` **gagal** pada percobaan pertama, dan temuannya nyata:

| Masalah | Bukti pada `981e002c` |
|---|---|
| Enum `LabMicrobiologyFinding` diklaim **dua task** | Bagian 16.2 menaruhnya di `BE-LAB-53`; `BE-LAB-45` sudah memilikinya |
| Dependency `BE-LAB-53` tertulis **"Nol"**, dan itu salah | Ia menambah ruas hasil Mikrobiologi ke `LabExamination` yang **belum punya** `MicrobiologyFinding` |
| Dua migration pada tabel yang sama | Nol urutan ditetapkan di antara keduanya |

Kelas kesalahan yang sama dengan `BE-EXT-04` dan `DEC-LAB-013`: **satu sisi berdiri tanpa
sisi lainnya**. Pemilik modul memutuskan **melebur keduanya** pada 2026-09-21.

`BE-LAB-45` berstatus **`DILEBUR`** — digantikan `BE-LAB-53`, bukan dibatalkan. Cakupannya
pindah utuh.

### `BE-LAB-53` — cakupan gabungan dan hasilnya

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI SEBAGIAN`** 2026-09-21 — source dan migration berdiri; **migration BELUM diterapkan** |
| **Cakupan terlaksana** | `LabResultForm` 2 → **4** nilai; **lima** enum baru; **sepuluh** kolom `LabExamination`; satu configuration; satu migration |
| **Berkas tersentuh** | `Enums/LaboratoryEnums.cs`, `Models/LabExamination.cs`, `Repositories/Configurations/.../LabExaminationConfiguration.cs`, `Migrations/20260921033917_AddLabMicrobiologyResultCompletion.cs` + snapshot |
| **Build** | `dotnet build -p:RunAnalyzers=False` → **0 error**, 216 warning **seluruhnya pre-existing di modul lain** |

**Lima enum:** `LabMicrobiologyFinding`, `LabSusceptibilityResult`, `LabResultQualifier`,
`LabCultureType`, `LabSusceptibilityMethod`.

**Sepuluh kolom:** `MicrobiologyFinding`, `ResultQualifier`, `CultureType`,
`SusceptibilityMethod`, `FinalizedAt`, `FinalizedByUserId`, `ReopenCount`,
`ConsultedByUserId`, `ConsultedToName`, `ConsultedAt`.

### Bukti yang sudah terkumpul

| AC | Hasil | Cara |
|---|---|---|
| `AC-101` | ✅ `Numeric = 1`, `Choice = 2` **tidak bergeser**; dua nilai baru menempati 3 dan 4 | Pembacaan enum |
| `AC-103` | ✅ `LabExaminationStatus` masih **tepat empat** nilai | Pembacaan enum |
| `AC-176` | ✅ `LabMicrobiologyFinding` **tepat tiga** nilai, nol `NeedsAttention`/`Critical` | Pembacaan enum |
| `AC-102` sebagian | ✅ Sembilan kolom `nullable: true`; `ReopenCount` `nullable: false` berdefault `0`; migration menyentuh **hanya** `LabExamination`; `Down` lengkap — 10 `DropColumn` + 1 `DropIndex` | Pembacaan migration |

### ⛔ Yang BELUM terbukti, dan kenapa

| Butir | Penahan |
|---|---|
| `AC-102` bagian **"migration berjalan pada tabel berisi data tanpa menulis ulang satu baris pun"** | Migration **belum diterapkan**. Database dev berada pada host **remote** `160.22.250.77`, bukan lokal, dan aturan task melarang menerapkan migration ke database non-lokal |
| `AC-158` | Menuntut endpoint `finalize` — itu **`BE-LAB-54`**, bukan task ini |

**Task ini belum dapat ditandai `SELESAI` penuh.** Yang dibutuhkan: izin eksplisit pemilik
modul untuk menjalankan `dotnet ef database update` terhadap `QuilvianNewDevYoga`, lalu
pemeriksaan sungguhan bahwa kesepuluh kolom berdiri dan jumlah baris `LabExamination` **tidak
berubah** (`LAB-RDY-C04`).

### 6m.1 Migration DITERAPKAN dan terverifikasi — 2026-09-21

Izin eksplisit pemilik modul. **`dotnet ef migrations list` dijalankan lebih dulu:** tepat
**satu** migration berstatus `(Pending)` — milik task ini. Ke-209 lainnya sudah diterapkan,
sehingga `database update` **nol** membawa perubahan orang lain.

Target `QuilvianNewDevYoga` pada `160.22.250.77`.

| Ukuran | Sebelum | Sesudah |
|---|---:|---:|
| Baris `LabExamination` | 8 | **8 — tidak berubah** |
| Kolom | 33 | **43** |
| Migration diterapkan | 209 | **210** |
| `ReopenCount` `NULL` | — | **0** |
| `ReopenCount` = `0` | — | **8** |

Kesepuluh kolom berdiri dengan tipe dan nullability yang benar; `ReopenCount` `NOT NULL`
berdefault `0`; `ConsultedToName` `character varying(200)`; index
`IX_LabExamination_FinalizedAt` berdiri.

**`BE-LAB-53` berstatus ✅ `SELESAI`.** `AC-101`, `AC-102`, `AC-103`, dan `AC-176` seluruhnya
terbukti terhadap database sungguhan sesuai `LAB-RDY-C04`. `AC-158` tetap milik `BE-LAB-54`.

**Yang terbuka sekarang:** `BE-LAB-54`, `BE-LAB-55`, `BE-LAB-56`, `BE-LAB-57`, `BE-LAB-60`,
`BE-LAB-62`, dan `BE-LAB-63` — seluruhnya berpendahulu `BE-LAB-53` yang kini selesai.
`BE-LAB-47` juga terbuka, sebab prasyarat `BE-LAB-45` sudah dilebur dan terlaksana di sini.

---

## 6n. `BE-LAB-54` SELESAI — 2026-09-21

Tiga endpoint berjalan: `POST /{id}/result/microbiology/finalize`,
`POST /{id}/result/microbiology/reopen`, `PUT /{id}/result/microbiology/consultation`.
Laporan lengkap: [`BE-LAB-54.md`](../task/report/backend/BE-LAB-54.md).

### Satu cacat ditemukan pengujian, bukan pembacaan

`reopen` pada jalur sah menjawab **`500`** — `Npgsql 23503`, pelanggaran foreign key saat
menyisipkan `LabTransitionHistory`. Sebabnya kueri nol meng-`Include` `LabOrder`, sehingga
`EncounterId` jatuh ke `Guid.Empty`.

> **Jebakan yang didokumentasikan entity ini sendiri, dengan arah bahaya terbalik.** Komentar
> pada `ResultEnteredByUserId` memperingatkan kolom **tanpa** foreign key menerima
> `Guid.Empty` **diam-diam**. `EncounterId` **punya** foreign key, sehingga ia berteriak.
> Jalan mundur `?? Guid.Empty` **dihapus** bersama perbaikannya — jalan mundur itulah yang
> menyembunyikan kesalahan sejenis.

Kegagalan itu sekaligus **membuktikan rollback**: sesudah `500`, `FinalizedAt` masih terisi
dan `ReopenCount` masih `0`. Nol perubahan sebagian tersimpan.

### Delapan skenario diuji terhadap aplikasi yang berjalan

Tiga penjaga (`422`), satu konflik (`409`), tiga jalur sah (`200`), dan satu cacat yang
diperbaiki lalu lulus.

| AC | Hasil |
|---|---|
| `AC-159` | ✅ `reopen` mengosongkan `FinalizedAt`, `ReopenCount` naik ke `1`, **nol** baris koreksi `S6` |
| `AC-169` | ✅ Tiga fakta konsultasi tersimpan; pelaku **diturunkan dari sesi**; `isReleased` tetap salah |
| `AC-158` | 🟡 **Sebagian.** Bagian "ditolak ketika dicoba dikirim" **nol dapat diuji** — nol jalur pengiriman ada di kode (`LAB-COORD-011`) |

**Yang disediakan sebagai gantinya:** respons membawa `isReleased` dan
`deliveryBlockedReason`, sehingga jalur pengiriman kelak **tidak perlu menyimpulkan** bahwa
Final sama dengan rilis — kesimpulan yang justru ditolak `LAB-DEC-097`.

### Yang terbuka sekarang

`BE-LAB-47` (tabel isolat dan antibiogram), `BE-LAB-55`, `BE-LAB-56`, `BE-LAB-57`,
`BE-LAB-60`, `BE-LAB-62`, `BE-LAB-63`.

---

## 6o. `BE-LAB-47` SELESAI — 2026-09-21

Dua tabel berdiri: `LabMicrobiologyIsolate` dan `LabIsolateSusceptibility`. Migration
`20260921041046_AddLabMicrobiologyIsolateAndSusceptibility` **diterapkan** ke
`QuilvianNewDevYoga`. Laporan: [`BE-LAB-47.md`](../task/report/backend/BE-LAB-47.md).

### ⚠ Cakupan task ini BASI dan diperbarui saat dikerjakan

Roadmap 6i.4 menunjuk `02-backend-architecture.md` bagian 14.7/14.8/14.11 — rancangan `r24`
dari 2026-09-18. Empat keputusan yang disetujui 2026-09-21 sudah menyusulnya, menambah
**tujuh kolom**: `ConcentrationUnitId` (`LAB-DEC-115`); `DiscContentUgSnapshot` beserta dua
snapshot breakpoint (`LAB-DEC-122`); `ComputedResult`, `IsResultOverridden`,
`ResultOverrideReason` (`LAB-DEC-123`); dan `IsSusceptibilityTested` (`LAB-DEC-126`).

Tabel dibangun mengikuti **ERD amandemen 2026-09-21**, bukan bagian 14.

### Bukti terhadap database

| AC | Hasil |
|---|---|
| `AC-108` | ✅ Empat FK **RESTRICT**; satu **CASCADE** (isolat → baris kepekaan) sesuai rancangan |
| `AC-109` | ✅ **Kedua** index unik membawa `WHERE ("IsDelete" = false)` — benar-benar parsial |
| `AC-110` | ✅ `OrganismNameSnapshot` dan `AntibioticNameSnapshot` keduanya `NOT NULL`, panjang 200 |

Diperiksa terhadap `information_schema` dan `pg_indexes` — **bukan** terhadap kode
configuration-nya, sesuai yang diminta task.

### Yang terbuka sekarang

`BE-LAB-48` (jalur pengisian hasil Mikrobiologi — kini punya tabelnya), `BE-LAB-55`,
`BE-LAB-56`, `BE-LAB-57`, `BE-LAB-60`, `BE-LAB-62`, `BE-LAB-63`.

---

## 6p. `BE-LAB-60` SELESAI, dan urutan gelombang dikoreksi — 2026-09-21

Laporan: [`BE-LAB-60.md`](../task/report/backend/BE-LAB-60.md).

### ⚠ `BE-LAB-48` DITUNDA — gate menolaknya

Task berikutnya menurut urutan roadmap adalah `BE-LAB-48`. **Gate menolaknya.**

`BE-LAB-48` berkontrak `r24` bagian 19.2 dengan dependency hanya `BE-LAB-47`. Di bawah `r27`,
endpoint pengisian hasil wajib **menyalin snapshot kandungan cakram dan rentang breakpoint**
lalu **menghitung interpretasi** — ketiganya milik `BE-LAB-60` dan `BE-LAB-61`.

**Urutan yang benar: `BE-LAB-60` → `BE-LAB-61` → `BE-LAB-48`.** Membangun `48` lebih dulu
berarti menulisnya dua kali.

### Hasil `BE-LAB-60`

Tabel `LabSusceptibilityBreakpoint` berdiri, kolom `LabAntibiotic.DiscContentUg` bertambah,
lima endpoint berjalan. Migration `20260921042352_AddLabSusceptibilityBreakpoint`
**diterapkan**.

| Pemeriksaan | Hasil |
|---|---|
| Index unik | ✅ **parsial** — `WHERE ("IsDelete" = false)` |
| Kedua FK | ✅ **RESTRICT** |
| Baris `LabAntibiotic` lama | ✅ tidak berubah |
| `VAL-115` rentang terbalik | ✅ `400` |
| `VAL-119` pasangan duplikat | ✅ `409` |
| Kelima endpoint | ✅ berjalan |
| `DELETE` menonaktifkan bukan menghapus | ✅ `IsActive=False`, `IsDelete=False` |

**`AC-185` belum terbukti:** ia menuntut hasil lama ber-snapshot untuk dibandingkan, dan nol
baris kepekaan ada sampai `BE-LAB-48` selesai.

### Catatan kebersihan data

Baris uji tertinggal di dev: organisme `BRANH-CAT`, antibiotik `AMPI`, dan satu rentang
breakpoint nonaktif. **Bukan data resmi** — pengisian sesungguhnya milik `DR-LAB-002`
(`LAB-DEC-122`).

### Berikutnya

`BE-LAB-61` — penghitung interpretasi, task berisiko tertinggi pada gelombang ini.

---

## 6q. `BE-LAB-61` SELESAI — 2026-09-21

Satu berkas: `Services/LabSusceptibilityInterpreter.cs`. Laporan:
[`BE-LAB-61.md`](../task/report/backend/BE-LAB-61.md).

**Nol migration dibutuhkan** — ketujuh kolom sudah berdiri pada `BE-LAB-47`, sebab tabelnya
dibangun mengikuti ERD `r27` sejak awal.

### 16 dari 16 skenario lulus

Diuji lewat harness di scratchpad yang mereferensikan DLL hasil build — nol berkas tambahan
masuk ke repository, dan nol database dibutuhkan sebab `Compute` dibuat **fungsi murni**.

| Kelompok | Hasil |
|---|---|
| `AC-186` tiga contoh `LAB-EVD-006` | ✅ 3/3 |
| **Batas tepat** `<` versus `<=` | ✅ 4/4 |
| `AC-191` zona nol versus kosong | ✅ 3/3 |
| `AC-187` penimpaan dan `VAL-113`/`VAL-114` | ✅ 6/6 |

### Ketiga jebakan yang diperingatkan 6k.2 — seluruhnya diuji

> **Batas tepat adalah yang paling berbahaya.** Zona `12` pada rentang `12-15` menghasilkan
> `Intermediate`, **bukan** `Resistant`; zona `15` menghasilkan `Intermediate`, **bukan**
> `Sensitive`. Bila tandanya keliru, sebagian hasil bergeser dari `I` menjadi `R` dan **nol
> galat muncul**. Bukti lapangannya Netilmicin zona `13` pada `12-15`, dilaporkan `I`.

Zona `0` terbukti **dihitung** menjadi `Resistant`, sedangkan ruas kosong menghasilkan `null`.
`ComputedResult` terbukti tetap tersimpan meski ditimpa.

### Berikutnya

`BE-LAB-48` — jalur pengisian hasil Mikrobiologi. **Kini seluruh pendahulunya siap:** tabel
(`BE-LAB-47`), breakpoint (`BE-LAB-60`), dan penghitung (`BE-LAB-61`). Ia yang akan
membuktikan `AC-185`, `AC-111`, dan `AC-112`.

---

## 6r. `BE-LAB-48` SELESAI — halaman hasil Mikrobiologi kini punya jalurnya, 2026-09-21

Dua endpoint: `PUT` dan `GET /{id}/result/microbiology`. **Nol migration** — kolomnya sudah
berdiri pada `BE-LAB-47`. Laporan: [`BE-LAB-48.md`](../task/report/backend/BE-LAB-48.md).

### Lima acceptance criteria terbukti, dan satu di antaranya menutup utang lama

| AC | Hasil |
|---|---|
| `AC-111` | ✅ Kultur steril — `isolates: []` diterima `200` |
| `AC-112` | ✅ Antibiotik ganda ditolak `422`, **keadaan sebelumnya utuh** |
| `AC-113` | ✅ Dua arah: hasil lama tetap terbaca, baris baru ditolak `422` |
| `AC-114` | ✅ `PUT` berulang mengganti, nol menumpuk |
| **`AC-185`** | ✅ **Menutup utang `BE-LAB-60`** |

### `AC-185` — uji paling menentukan pada gelombang ini

Breakpoint diubah `13-17` → `5-8`, lalu hasil lama dibaca ulang:

```text
breakpointLowerMm=13  breakpointUpperMm=17
zoneDiameterMm=10  computedResult=1 (Resistant)  result=1
```

**Tidak berubah.** Bila sistem menghitung ulang dengan rentang baru, zona `10` berada di atas
batas atas `8` dan hasilnya menjadi **`Sensitive`** — kebalikan total dari yang dilaporkan
kepada dokter. Snapshot mencegahnya.

### ⛔ Satu aturan belum dapat ditegakkan

`VAL-118` — bagian isolat harus ditolak pada pemeriksaan yang profilnya menyatakan tidak
memakai set bakteri. `LabProcedureMicrobiologyProfile` adalah **`BE-LAB-62`** dan belum
dibangun, sehingga hari ini jalur ini menerima hasil Mikrobiologi untuk pemeriksaan apa pun.
`VAL-84` juga menunggu data induk batas nilai berbentuk `MicrobiologyStructured`.

### Keadaan slice `S4b`

| Task | Status |
|---|---|
| `BE-LAB-53` tiang kolom dan enum | ✅ |
| `BE-LAB-54` finalize/reopen/consultation | ✅ |
| `BE-LAB-47` tabel isolat dan antibiogram | ✅ |
| `BE-LAB-60` data induk breakpoint | ✅ |
| `BE-LAB-61` penghitung interpretasi | ✅ |
| `BE-LAB-48` jalur pengisian hasil | ✅ |
| `BE-LAB-55`, `56`, `57`, `62`, `63` | 🔲 terbuka |

**Inti `S4b` berdiri.** Yang tersisa: Spesifik Specimen (`55`), aturan kritis (`56`), jejak
koreksi specimen (`57`), profil katalog (`62`), dan pengaturan cetak (`63`).

---

## 6s. `BE-LAB-62` SELESAI — `VAL-118` kini ditegakkan, 2026-09-21

Laporan: [`BE-LAB-62.md`](../task/report/backend/BE-LAB-62.md).

### Kenapa task ini didahulukan

`BE-LAB-48` menutup laporannya dengan `VAL-118` yang **belum dapat ditegakkan**. Menumpuk
aturan yang tidak berlaku adalah cara paling mudah kehilangan jejak mana yang sudah jalan —
dan aturan yang tercatat tetapi tidak berlaku **lebih berbahaya daripada aturan yang belum
ada**, sebab ia terlihat sudah selesai.

`VAL-118` **sengaja ikut disambungkan** ke jalur hasil pada task ini. Tabel yang nol pernah
dibaca tidak menegakkan apa pun.

### Keputusan perilaku: pemeriksaan yang belum diprofilkan tetap diterima

Tabel pemetaan dimulai kosong. Menolak seluruh pengisian sampai kepala instalasi selesai
memetakan berarti halaman hasil **lahir dalam keadaan tidak dapat dipakai**. Yang ditolak
hanya pemeriksaan yang **sudah diprofilkan tegas** sebagai tidak memakai set bakteri — alasan
yang sama dengan jalur jatuh `LAB-DEC-111`.

### `VAL-118` diuji pada empat keadaan

| Keadaan | Hasil |
|---|---|
| Belum diprofilkan, kirim isolat | ✅ `200` |
| Diprofilkan `false`, kirim isolat | ✅ `422` |
| Diprofilkan `false`, **tanpa** isolat | ✅ `200` — hasil tetap boleh disimpan |
| Diubah menjadi `true`, kirim isolat | ✅ `200` |

Index unik **parsial** dan FK `RESTRICT` terverifikasi terhadap database.

### Keadaan slice `S4b`

Tujuh selesai: `53`, `54`, `47`, `60`, `61`, `48`, `62`. **Tersisa tiga:** `BE-LAB-55`
(Spesifik Specimen), `BE-LAB-56` (aturan kritis), `BE-LAB-57` (jejak koreksi specimen), dan
`BE-LAB-63` (pengaturan disiplin dan nomor cetak).

> **`BE-LAB-55` punya penahan yang belum dijawab:** seeder 1.767 baris membutuhkan berkas CSV
> di dalam repository, dan memasukkannya adalah penulisan ke source yang **belum diizinkan
> pemilik modul**.

---

## 6t. `BE-LAB-55` SELESAI — 1.767 baris ter-seed, 2026-09-21

Laporan: [`BE-LAB-55.md`](../task/report/backend/BE-LAB-55.md).

### Penahan `LAB-OPEN-040` dibuka pemilik modul

Berkas dataset masuk ke repository sebagai **CSV tertanam** —
`Seeders/Data/lab-specimen-detail-types.csv`, 1.767 baris, 172 KB, di dalam DLL.

### Hasil seeder

| Jalan ke | Jenis | Rincian | Aktif | Nonaktif |
|---|---:|---:|---:|---:|
| Pertama | 31 | **1.767** | **1.601** | **166** |
| Kedua | 31 | 1.767 | 1.601 | 166 |
| Ketiga | 31 | 1.767 | 1.601 | 166 |

Angkanya cocok persis dengan `LAB-DEC-130`.

### ⭐ Jebakan terbesar yang 6l peringatkan — terbukti tidak terjadi

Baris `SP-122552005` diberi nama Indonesia `Darah arteri`, lalu aplikasi **dimatikan dan
dinyalakan ulang**. Sesudah seeder berjalan untuk ketiga kalinya:

```text
rincian = 1767 | terjemahan = Darah arteri
```

**Bertahan.** Seeder hanya menyisipkan yang kodenya belum ada — ia nol memperbarui baris yang
sudah ada, sehingga pekerjaan penerjemahan kepala instalasi aman.

### Perluasan 7 → 31 aman, dan diperiksa bukan diasumsikan

**GUID ketujuh baris lama dipertahankan**; ketujuhnya punya padanan di antara 31 kelompok,
sehingga `LabSpecimen.SpecimenTypeId` yang sudah menunjuk ketujuhnya nol perlu dipetakan ulang.

### Tujuh endpoint dan empat penyaring terbukti

`untranslatedOnly` turun `1601 → 1600` sesudah satu terjemahan diisi; `includeInactive`
menampilkan 1.767; pencarian menerima **kedua bahasa**; `options` per kelompok memberi **54**
untuk BLOOD dan **434** untuk TISSUE — cocok persis dengan sebaran `LAB-EVD-007`.

Index unik `SnomedCode` memakai **dua pembatas** — `IsDelete = false` **dan**
`SnomedCode IS NOT NULL` — supaya baris lokal kedua yang berkode SNOMED kosong tidak ditolak.

### Keadaan slice `S4b`

Delapan selesai: `53`, `54`, `47`, `60`, `61`, `48`, `62`, `55`. **Tersisa dua:** `BE-LAB-56`
(aturan kritis) dan `BE-LAB-57` (jejak koreksi specimen), ditambah `BE-LAB-63` (pengaturan
disiplin dan nomor cetak).

---

## 6u. `BE-LAB-56` SELESAI — penanda kritis Mikrobiologi hidup, 2026-09-21

Laporan: [`BE-LAB-56.md`](../task/report/backend/BE-LAB-56.md). Penilainya **disambungkan ke
pembacaan hasil** pada task ini — tabel yang nol pernah dibaca tidak menegakkan apa pun.

### Tiga keadaan diuji, dan dua di antaranya terlihat sama

| Keadaan | `criticalRuleAvailable` | `isCritical` |
|---|---|---|
| **Nol aturan** (`AC-166`) | **false** | false |
| Aturan ada, **tidak cocok** (`AC-167`) | **true** | false |
| Aturan ada, **cocok** | true | **true** |

> **Dua baris pertama sama-sama berpenanda mati, dan itu bahayanya.** Tanpa
> `criticalRuleAvailable`, layar bersih terbaca sebagai *"hasil aman"* padahal yang terjadi
> adalah `DR-LAB-002` belum mengisi satu pun baris aturan. Ruas itulah yang membedakannya.

`VAL-106` terbukti: aturan yang ketiga ruas penilainya kosong ditolak `400` — ia berarti
seluruh hasil kritis, dan alarm yang berbunyi terus-menerus berhenti dibaca orang.

Ruas organisme yang **dikosongkan** terbukti berarti "apa saja": aturan *"kuman apa saja ×
Ampicillin × Resistant"* menyalakan penanda pada *Branhamella catarrhalis*.

### Satu keputusan yang tampak bertentangan tetapi tidak

**Sengaja nol index unik** atas kombinasi aturan — berbeda dari breakpoint. Dua aturan
tumpang tindih bukan kesalahan sebab penilaian bersifat **cukup satu cocok**; sedangkan dua
breakpoint atas pasangan yang sama membuat hitungan bergantung baris mana yang terbaca lebih
dulu.

Demikian pula `IsCritical` **dihitung saat dibaca** sedangkan `ComputedResult` **disimpan**:
yang pertama pertanyaan masa kini atas aturan yang boleh berubah, yang kedua fakta masa lalu.

### ⚠ Dua baris uji WAJIB dihapus sebelum dipakai sungguhan

*"Branhamella catarrhalis × Ampicillin × Sensitive"* dan *"kuman apa saja × Ampicillin ×
Resistant"*. Yang kedua **terlalu luas** untuk dibiarkan hidup. Pengisian sesungguhnya milik
`DR-LAB-002` (`LAB-DEC-103` butir 4).

### Keadaan slice `S4b`

Sembilan selesai: `53`, `54`, `47`, `60`, `61`, `48`, `62`, `55`, `56`. **Tersisa dua:**
`BE-LAB-57` (jejak koreksi specimen) dan `BE-LAB-63` (pengaturan disiplin dan nomor cetak).

---

## 6v. `BE-LAB-57` SELESAI — koreksi specimen kini berjejak, 2026-09-21

Laporan: [`BE-LAB-57.md`](../task/report/backend/BE-LAB-57.md).

### `AC-175` terbukti lewat dua cacah, bukan lewat satu

| Cacah | Sebelum sepuluh koreksi | Sesudah |
|---|---|---|
| `LabFieldChangeLog` | 0 | **9** |
| `LabTransitionHistory` | 58 | **58** |

Satu baris `LabTransitionHistory` yang bertambah setelahnya berasal dari `reopen`, bukan dari
koreksi. **Itulah inti `LAB-DEC-112`:** perpindahan keadaan dan perubahan isi adalah dua sumbu,
dan menumpuknya pada satu tabel memaksa `FromStatus`/`ToStatus` diisi nilai palsu setiap kali
sebuah keterangan diperbaiki.

### `AC-170` diuji sebagai satu siklus penuh, bukan satu tolakan

Koreksi → `finalize` → koreksi ditolak `422` → `reopen` → koreksi diterima kembali. Menguji
hanya tolakannya akan meloloskan pintu yang **terkunci selamanya** — padahal `LAB-DEC-097`
menyatakan Final bukan rilis dan karenanya masih boleh dibuka.

### Satu cacat ditemukan oleh pengujiannya sendiri

Kolom `VolumeAmount` bertipe `numeric(12,3)`, sehingga 5 yang tersimpan terbaca kembali sebagai
`5.000`. Dibandingkan apa adanya sebagai teks, **mengirim ulang volume yang sama mencatat
perubahan yang tidak pernah terjadi** — melanggar DoD task ini sendiri. Diperbaiki pada
pembentuk nilainya, bukan dengan pengecualian khusus pada pembandingnya.

Uji ulang: `"Nol ruas yang berubah."`, `ruasBerubah: 0`.

### Dua penyimpangan dari cakupan yang ditulis roadmap

| Ditulis roadmap | Yang dikerjakan | Alasan |
|---|---|---|
| `ApplyCorrectionAsync` pada `LabSpecimenService` | `LabSpecimenCorrectionService` tersendiri | Berkas asalnya sudah lebih dari 2.100 baris; **permukaan controller tetap satu**, kontrak nol berubah |
| Kontrak `r26` bagian 21.4 | `r27` bagian 21.4 | `r26` sudah diamandemen sebelum task ini dikerjakan |

### ⚠ Baris uji tertinggal di dev

Sembilan baris `LabFieldChangeLog`, **termasuk satu yang menyesatkan**: `VolumeAmount 5.000 → 5`
dari build sebelum perbaikan di atas — perubahan itu tidak pernah benar-benar terjadi.

### Keadaan slice `S4b`

Sepuluh selesai: `53`, `54`, `47`, `60`, `61`, `48`, `62`, `55`, `56`, `57`. **Tersisa satu:**
`BE-LAB-63` (pengaturan disiplin dan nomor cetak).

---

## 6w. `BE-LAB-63` SELESAI — gelombang `MVP-7b` tuntas, slice `S4b` belum, 2026-09-22

Laporan: [`BE-LAB-63.md`](../task/report/backend/BE-LAB-63.md).

### `AC-180` terbukti lewat empat pesanan, bukan satu

| `OrderNumber` | Disiplin | `LabReportNumber` |
|---|---|---|
| `LAB-RSMMC-000010` | Mikrobiologi | **`26-0001`** |
| `LAB-RSMMC-000011` | Mikrobiologi | **`26-0002`** |
| `LAB-RSMMC-000012` | Patologi Anatomi | **`26-0001`** |
| `LAB-RSMMC-000013` | Patologi Klinik | **`26-0001`** |

Satu pesanan saja hanya membuktikan kolomnya terisi. **Empat membuktikan penghitungnya memang
terpisah:** `26-0001` muncul tiga kali, sekali per disiplin, sementara `OrderNumber` tetap
berderet global. `LAB-DEC-072` utuh — nol satu pun dibongkar menjadi format cetak.

### `AC-182` dibuktikan dengan sidik jari, bukan dengan pengamatan

Nama konsultan Mikrobiologi diubah, lalu dua tabel dibandingkan sebelum dan sesudah:

| Yang diperiksa | Hasil |
|---|---|
| `LabDisciplineSetting` | **berubah** ✅ |
| `SysAccessPolicy` md5 | **identik** ✅ |
| `MstDoctor` md5, 17 baris | **identik** ✅ |

Mengamati bahwa "sepertinya tidak ada yang berubah" bukan bukti. Sidik jari seluruh baris
adalah bukti.

### Pengujian yang paling penting justru bukan salah satu AC

Sesudah nama diubah manual, aplikasi **dimatikan lalu dinyalakan ulang**, dan nama hasil
suntingan **bertahan**. Seeder yang menyegarkan isinya setiap aplikasi menyala akan
mengembalikan nama pejabat lama pada setiap penempatan ulang — pada dokumen yang dipegang
pasien.

### Satu penambahan di luar cakupan roadmap, dan alasannya

Roadmap berhenti pada tabel, kolom, layanan, dan controller. Alokasinya **ikut disambungkan ke
kedua jalur pembuatan pesanan**, sebab `AC-180` menuntut satu pesanan benar-benar memiliki dua
nomor — dan itu mustahil dibuktikan oleh kolom yang nol pernah diisi siapa pun.

### ⚠ Satu batas diketahui diangkat, bukan ditutup diam-diam

**`LAB-OPEN-043`.** Ketiga disiplin memakai bentuk nomor yang berbeda — `26-1129`, `26.0919`,
dan `25039254` — sedangkan `r27` bagian 22.7 hanya menyetujui satu ruas awalan, yang tidak
dapat menyatakan pemisah maupun lebar. Yang dibangun cocok dengan Mikrobiologi dan **tidak**
dengan dua lainnya. Nol memblokir `S4b`; memblokir cetakan dua disiplin lain ketika layarnya
dibangun.

### ⚠ Baris uji tertinggal di dev

Empat pesanan `LAB-RSMMC-000010`..`000013` pada encounter `d6fdf9f0-…`. Pengaturan disiplin
sudah dikembalikan ke nilai `LAB-EVD-005`.

### Keadaan slice `S4b` — dan satu koreksi atas pernyataan yang keliru

**Gelombang `MVP-7b` (bagian 6k) tuntas:** keempat tasknya — `60`, `61`, `62`, `63` — selesai.

**Tetapi slice `S4b` BELUM selesai, dan pernyataan pertama bagian ini sempat mengatakan
sebaliknya.** Gelombang `MVP-7` (bagian 6j) berisi **tujuh** task, dan dua di antaranya masih
terbuka:

| Task | Keadaan |
|---|---|
| `BE-LAB-58` — ruas turunan pada jalur baca hasil | 🔲 **kini SIAP** — kedua pendahulunya (`BE-LAB-54`, `BE-LAB-56`) selesai |
| `BE-LAB-59` — pilihan dokter konfirmator | 🔲 **SIAP**, bebas pendahulu sejak awal |

Selesai sejauh ini: `53`, `54`, `47`, `55`, `56`, `57`, `60`, `61`, `48`, `62`, `63` —
**sebelas task, tetapi bukan sebelas dari sebelas.** Menghitung hanya gelombang terakhir lalu
menyebut slice-nya tuntas adalah cara paling mudah kehilangan dua task yang justru menyuplai
layar hasil.

Sesudah keduanya, sisa `S4b` milik frontend (`FE-LAB-30`..`FE-LAB-33`) dan penahan di luar
modul — `DEC-LAB-011` untuk `S4d`, `LAB-OPEN-029`, `LAB-OPEN-039`, `LAB-OPEN-041`,
`LAB-OPEN-042`, `LAB-OPEN-043`, serta `LAB-COORD-011`..`014`.

---

## 6x. `BE-LAB-59` SELESAI — dan tiga kerusakan merge ditemukan di jalannya, 2026-09-22

Laporan: [`BE-LAB-59.md`](../task/report/backend/BE-LAB-59.md). **Nol tabel baru, nol
migration** — rantai `TrxOnCallAssignment` → `WorkforceProfileId` → `MstDoctor` memang sudah
lengkap, persis seperti temuan `LAB-DEC-111`.

### Keadaan yang pasti terjadi lebih dulu terbukti pada keadaan sesungguhnya

`TrxOnCallAssignment` benar-benar **nol baris** saat `AC-174` diuji — bukan dikosongkan untuk
pengujian. Itulah yang diramalkan `LAB-DEC-111`, dan jalur jatuhnya menyala: seluruh dokter
aktif tampil beserta nomor WhatsApp dan keterangan bahwa jadwal jaga belum tersedia.

`AC-173` dibuktikan sesudahnya dengan satu penugasan aktif yang disisipkan atas izin pemilik
modul: **tepat satu** dokter tampil, dan jalur jatuh **padam sendiri**.

### Dua pembuktian yang tidak diminta AC mana pun, tetapi tanpanya slice ini rapuh

| Yang diuji | Kenapa perlu |
|---|---|
| Jendela jaga digeser ke **masa lalu** | Menguji hanya "ada baris" akan meloloskan resolver yang **mengabaikan jam**. Digeser lewat, `onDutyScheduleAvailable` kembali `false` |
| `attendingDoctor` pada kunjungan **berdokter** | Seluruh pemeriksaan yang ada bernaung pada kunjungan tanpa dokter, sehingga ruas ini selalu kosong — **terlihat sama** dengan ruas yang tidak pernah bekerja |

Pada pembuktian kedua, satu respons memuat `dr. Rendy Pangalila` sebagai DPJP dan
`dr. Nabila Rahmawati` sebagai dokter jaga: **dua dokter berbeda dari dua sumber berbeda**,
persis *"dua cara memilih, bukan dua jabatan"*.

### ⚠ Merge 4ba789b2 meninggalkan HEAD tidak dapat dibuild maupun dinyalakan

Ditemukan berurutan saat mencoba menguji task ini, dan **ketiganya bukan berasal dari task
ini**:

| # | Kerusakan | Perbaikan |
|---|---|---|
| 1 | `Program.cs` memanggil `LabDummyDataSeeder` yang berkasnya **nol ada pada kedua sisi merge** | Pemanggilnya dicabut ulang |
| 2 | Blok DI Billing/Finance cabang integrasi **nol pernah dipanggil** — aplikasi gagal pada validasi DI, bukan saat dibuild | `AddBillingManagement()` disambungkan |
| 3 | **Foreign key `LabOrder.ExaminerDoctorId` → `MstDoctor` hilang** dari konfigurasi | Dipulihkan beserta index-nya |

> **Yang ketiga hampir lolos, dan itu bagian yang perlu diingat.** Ia menampakkan diri sebagai
> `PendingModelChangesWarning`. Membangkitkan migration dari drift itu — langkah yang paling
> wajar diambil — akan **MENGHAPUS foreign key-nya dari database**, mencabut integritas
> referensial atas dokter pemeriksa tanpa seorang pun memutuskannya.

**Satu kerusakan sengaja dibiarkan:** `ApplicationDbContextModelSnapshot` terbawa dari sisi
`yoga` dan nol memuat perubahan model 21 migration cabang lain, sehingga setiap
`database update` tanpa target eksplisit menolak jalan dengan drift setebal 182 KB. Ke-21
migration diterapkan dengan menyebut target secara tegas. Menyegarkan snapshot menyentuh model
seluruh modul — **milik yang melakukan merge, bukan milik task ini.**

### Keadaan slice `S4b`

Dua belas selesai: `53`, `54`, `47`, `55`, `56`, `57`, `59`, `60`, `61`, `48`, `62`, `63`.
**Tersisa satu task backend:** `BE-LAB-58` (ruas turunan pada jalur baca hasil), yang kini
bebas pendahulu dan cakupannya sudah bertambah oleh `r27` bagian 22.3.

---

## 6y. `BE-LAB-58` SELESAI — backend slice `S4b` TUNTAS, 2026-09-22

Laporan: [`BE-LAB-58.md`](../task/report/backend/BE-LAB-58.md). **Nol tabel, nol kolom, nol
migration** — seluruh sumber ruas turunan sudah berdiri lebih dulu.

### Cakupan roadmap ternyata salah di dua arah sekaligus

| Arah | Kenyataan |
|---|---|
| **Terlalu banyak** | Lima dari delapan ruas `r26` 21.3 **sudah berdiri**, dibangun `BE-LAB-48`, `61`, `62` sebagai bagian pekerjaannya sendiri |
| **Terlalu sedikit** | `r27` bagian 22.3 menambah **sembilan ruas lagi** sesudah roadmap ditulis |

Keduanya ditulis apa adanya di laporan, bukan dirapikan menjadi "sesuai rencana".

### `AC-157` dan `AC-168`: sembilan nilai palsu dikirim, nol diterima

Satu `PUT` memuat `effectiveAt` 1999, `analystName` *"dr. Palsu Sekali"*, `reopenCount` 999,
`isFinalized` true, `labReportNumber` 99-9999, dan `authorizingOfficerName`
*"dr. Pengesah Palsu"*. Permintaannya **berhasil `200`** — dan pembacaan ulang mengembalikan
**kesembilan nilai aslinya**.

Berhasil, bukan ditolak, memang yang diminta: *"dikirim pada request pun diabaikan"*.
Bentuknya ditegakkan pada DTO, yang nol memuat satu pun ruas tersebut.

### `AC-181`: tiga tanggal, dan ketiganya berbeda

Satu pemeriksaan dibawa melewati siklus penuh — collect → receive → koreksi waktu terima →
isi hasil → finalize:

| Ruas | Nilai |
|---|---|
| `effectiveAt` | `04:40:41` — pengambilan |
| `printReceivedAt` | `04:40:48` — penerimaan fisik |
| `issuedAt` = `printCompletedAt` | `04:40:49` — `FinalizedAt` |

**`effectiveAt` ≠ `printReceivedAt` adalah inti `LAB-DEC-118`.** Menyamakannya membuat bahan
yang diambil Senin dan diterima Rabu tercatat efektif hari Rabu. Nilai berbeda pada pembacaan
nyata itulah buktinya — bukan komentar di kode.

### Footer terbukti per disiplin, bukan tetapan

Pesanan Patologi Klinik menjawab `Konsultan` / `Prof.Dr.Riadi Wirawan SpPK(K)` tanpa kalimat
baku; pesanan Mikrobiologi menjawab `Konsultan Mikrobiologi Klinik` /
`Usman Chatib Warsa, PhD, SpMK-K, Prof. dr.` beserta kalimat bakunya. Satu ruas yang selalu
bernilai sama nol membuktikan pencariannya bekerja.

### ⚠ `AC-183` TIDAK ditandai tertutup

`authorizingOfficerName` dan `validatedByName` tetap kosong **bahkan sesudah `finalize`
berhasil** — dan itu memang yang diminta `LAB-DEC-120`. Tetapi sisi positifnya, nama perilis
benar-benar muncul, **nol dapat diuji sampai `S4d` dibuka `DEC-LAB-011`**.

### Keadaan slice `S4b`

**Tiga belas dari tiga belas task backend selesai:** `47`, `48`, `53`, `54`, `55`, `56`, `57`,
`58`, `59`, `60`, `61`, `62`, `63`. **Backend `S4b` tuntas.**

Yang tersisa pada slice ini seluruhnya di luar backend: `FE-LAB-30`..`FE-LAB-34`, dan penahan
`DEC-LAB-011` (`S4d`), `LAB-OPEN-029`, `LAB-OPEN-039`, `LAB-OPEN-041`, `LAB-OPEN-042`,
`LAB-OPEN-043`, serta `LAB-COORD-011`..`014`.

---

## 6z. `BE-LAB-49` SELESAI DENGAN TEMUAN, dan audit kesiapan memberi `NOT_READY` — 2026-09-22

Laporan: [`BE-LAB-49.md`](../task/report/backend/BE-LAB-49.md).
Audit: [`readiness-report.md`](../testing/readiness-report.md).

### Task ini ditemukan oleh audit, bukan oleh perencanaan

Bagian 6y menyebut *"tiga belas dari tiga belas selesai"* dan **nol memuat `BE-LAB-49`** — ia
hidup di gelombang `MVP-6c`, berdependency `BE-LAB-47` dan `BE-LAB-48` yang baru selesai
2026-09-21.

Roadmap sudah memperingatkan hal ini pada task-nya sendiri: pembuktian dipisahkan **justru
supaya tidak ikut tertelan anggapan _"migration sudah jalan, berarti beres"_**. Ia lalu
tertelan anggapan yang berbeda — perhitungan gelombang yang tidak memuatnya.

### `AC-115` punya dua bagian, dan keduanya berbeda nasib

| Bagian | Hasil |
|---|---|
| Hapus baris kepekaan → pilih **antibiotik yang sama** lagi → berhasil | ✅ **TERBUKTI** `200`, baris baru zona 14 → `Intermediate` |
| Baris lama **tetap ada** bertanda `IsDelete = true` | ⛔ **TERBANTAH** — lenyap fisik, `0` baris bertanda terhapus |

Outcome yang diminta task ini terbukti. Yang terbantah adalah **mekanisme yang diasumsikan**
menghasilkannya.

### `LAB-CONFLICT-011` — komentar index menyatakan premis yang kodenya bantah

`LabIsolateSusceptibilityConfiguration` menulis *"penghapusan berupa penandaan (IsDelete)"*,
sedangkan `LabMicrobiologyResultService` memakai `RemoveRange`/`Remove` dan **nol tempat di
codebase** mengubah `EntityState.Deleted` menjadi penandaan. Filter parsial pada index itu
karena itu **nol pernah teruji**.

Modul ini memang memakai dua gaya hapus dan keduanya disengaja — penandaan untuk data induk,
hapus fisik untuk koleksi anak yang diganti utuh. **Yang salah bukan kodenya, melainkan
komentar index beserta `AC-115` — atau sebaliknya.** Keputusannya milik pemilik modul; task
verifikasi nol berwenang memilih.

### ⚠ Audit kesiapan: `NOT_READY`

Tiga blocker, dan **nol di antaranya soal mutu kode backend**:

| # | Blocker |
|---|---|
| B1 | **Nol hak akses diberikan** untuk lima controller baru — `SysAccessPolicy` mencatat `0` kebijakan diizinkan bagi `LabDisciplineSetting`, `LabMicrobiologyCriticalRule`, `LabProcedureMicrobiologyProfile`, `LabSpecimenDetailType`, `LabSusceptibilityBreakpoint`. Seluruh pengujian memakai `superadmin`, sehingga ini nol pernah menampakkan diri |
| B2 | **Nol layar** — penelusuran frontend atas delapan penanda endpoint baru menemukan nol berkas |
| B3 | **Data induk praktis kosong** — organisme 2, antibiotik 2, breakpoint 1, profil 1, aturan kritis 2, dan **seluruhnya baris uji** |

**B1 memblokir B3:** kepala instalasi dan `DR-LAB-002` nol dapat mengisi satu baris pun sebelum
hak aksesnya ada.

### Satu laporan sebelumnya dikoreksi

Bagian 6x menyebut snapshot EF rusak sehingga `database update` menolak jalan. **Itu tidak
lagi benar.** `has-pending-model-changes` bersih, `database update` menyatakan database sudah
mutakhir, dan snapshot sama dengan commit. Pemulihan foreign key `ExaminerDoctorId` menutup
drift itu; angka 182 KB yang sempat disebut diukur terhadap snapshot yang sudah lebih dulu
ditulis ulang oleh siklus `migrations add`/`remove` itu sendiri.

### Keadaan slice `S4b`

**Empat belas task backend selesai** — `47`, `48`, `49`, `53`..`63`. Backend tuntas, **slice
tidak**. Jalan menuju `READY` ada pada bagian 7 laporan audit.

---

## 6aa. `LAB-CONFLICT-011` dan `LAB-OPEN-043` DITUTUP pemilik modul — 2026-09-22

Keduanya diputuskan pada hari yang sama, dan **keduanya memilih rekomendasi**.

### `LAB-CONFLICT-011` → jalan **B**: dokumennya yang salah, bukan kodenya

`AC-115` dipersempit menjadi *"pilih ulang antibiotik yang sama berhasil"*; klausa *"baris lama
tetap ada bertanda `IsDelete = true`"* **dicabut**. Komentar index diperbaiki agar menyatakan
kebenarannya: **filternya jaring pengaman, bukan kebutuhan** — hapus pada tabel ini memang
fisik, dan `BE-LAB-49` sudah membuktikannya.

**Filter parsialnya TETAP dipertahankan.** Bila gaya hapus tabel ini kelak berubah menjadi
penandaan, index penuh akan mulai menolak antibiotik yang sama dipilih ulang — dan
kegagalannya muncul di tangan analis, bukan di pipeline.

Jalan A ditolak karena menyimpan generasi baris mati membuat **dua tempat menjawab pertanyaan
riwayat**; `LAB-DEC-080` dan `LAB-DEC-112` sudah menolak pola itu dua kali, dan jejak
perubahan sudah punya rumahnya sendiri di `LabFieldChangeLog`.

### `LAB-OPEN-043` → jalan **A**: dua kolom, bukan satu template

`LAB-API-v1` `r29` bagian 24, `approved` hari yang sama, **aditif penuh**.
Migration `20260922064824_AddLabReportNumberShape`.

| Disiplin | Pemisah | Lebar | Nomor nyata yang dialokasikan |
|---|---|---|---|
| Mikrobiologi | `-` | 4 | **`26-0004`** |
| Patologi Anatomi | `.` | 4 | **`26.0001`** |
| Patologi Klinik | **kosong** | **6** | **`26000001`** |

Ketiganya cocok dengan `LAB-EVD-005`. Sebelumnya ketiganya dipaksa `-` dan empat digit.

**`null` dan teks kosong sengaja dibedakan** — "tanpa pemisah" adalah jawaban yang sah bagi
Patologi Klinik, bukan pertanyaan yang belum dijawab. `LabDisciplineSettingService` karena itu
**tidak** menormalkan ruas ini menjadi `null` seperti ruas teks lainnya.

### ⚠ Satu akibat yang ditemukan pengujiannya sendiri

**Mengubah bentuk nomor MENGULANG penghitung dari satu.** Nomor urut dicari lewat pencocokan
awalan tetap; mengubah pemisah mengubah awalan itu, sehingga nomor lama nol ditemukan.
Terbukti: Patologi Anatomi yang sudah punya `26-0001` memperoleh **`26.0001`**, bukan `26.0002`.

Index unik tetap menjaga nol ada dua lembar bernomor sama, tetapi **urutannya patah** dan itu
terbaca siapa pun yang mengarsipkan lembar per nomor. **Anjuran: ubah bentuk nomor hanya pada
pergantian tahun.** Ditulis pada kontrak bagian 24.5, bukan hanya di sini.

### Migration ikut mengisi sekali jalan, dan itu disengaja

Tanpa itu, ketiga baris yang sudah ada memakai bawaan `-` dan 4 digit — benar bagi Mikrobiologi,
**salah bagi dua lainnya** — dan seeder **nol akan memperbaikinya**, sebab ia hanya menyisipkan.
Pengisian ini **bukan** pelanggaran prinsip tersebut: kedua kolomnya baru lahir pada migration
yang sama, sehingga nol mungkin ada manusia yang pernah menyetelnya. Syarat
`ReportNumberSeparator IS NULL` ditulis agar ia tetap aman pada database yang sudah disetel
lebih dulu.

---

## 6ab. Sapuan status seluruh task backend — 2026-09-22

Dijalankan atas permintaan pemilik modul: *"ada task yang tertinggal di BE, cek apalagi"*.

### Delapan blok status ternyata stale, bukan tiga

Bagian 6z sempat menyebut **tiga** blok stale (`BE-LAB-45`, `47`, `48`). Sapuan menyeluruh
menemukan **delapan**, dan kelima tambahannya luput karena perbaikan sebelumnya hanya
menyisir gelombang `MVP-6` — **kesalahan yang persis sama dengan yang membuat `BE-LAB-49`
terlewat**: menyisir sebagian lalu menyimpulkan keseluruhan.

| Task | Tertulis | Sebenarnya |
|---|---|---|
| `BE-LAB-22` | `SELESAI SEBAGIAN` sejak 2026-09-15 | **Ditutup penuh 2026-09-17** oleh `BE-LAB-37`; `VAL-59` menyala, `AC-66` penuh |
| `BE-LAB-53` | `SIAP DIKERJAKAN` | ✅ bagian 6m |
| `BE-LAB-54` | `MENUNGGU PENDAHULU` | ✅ bagian 6n |
| `BE-LAB-55` | `MENUNGGU PENDAHULU` | ✅ bagian 6t |
| `BE-LAB-56` | `MENUNGGU PENDAHULU` | ✅ bagian 6u |
| `BE-LAB-60` | `MENUNGGU PENDAHULU` | ✅ bagian 6p |
| `BE-LAB-61` | `MENUNGGU PENDAHULU` | ✅ bagian 6q |
| `BE-LAB-62` | `MENUNGGU PENDAHULU` | ✅ bagian 6s |

Seluruh penyelesaiannya **sudah tercatat** pada bagian naratif masing-masing; yang tertinggal
hanya blok status di tabel task. Pembaca yang membuka tabel — bukan narasi — akan mengambil
pekerjaan yang sudah selesai. Kedelapannya kini diperbaiki dan **masing-masing bertanda kapan
diperbaiki**, supaya jejaknya tidak hilang.

### Nomor yang bolong sudah ditelusuri, dan bukan lubang

| Task | Keadaan |
|---|---|
| `BE-LAB-23`, `39`, `43` | ✅ selesai, berlaporan sendiri |
| `BE-LAB-40`, `42` | ✅ selesai — **dilaporkan bersama di dalam `BE-LAB-39.md`**, ditandai ✅ pada traceability. Nol laporan terpisah, dan itu disengaja |

### Yang benar-benar masih terbuka: dua, dan keduanya di luar `S4b`

| Task | Status | Penahan |
|---|---|---|
| `BE-LAB-41` | ⛔ `TERTAHAN` | Penyimpanan pengiriman hasil ke pasien — milik `S5`/`S6` |
| ~~`BE-LAB-46`~~ | ~~🧊 `DIBEKUKAN` 2026-09-18~~ | ~~Jalur pengisian hasil Patologi Anatomi — milik `S4e`, dibekukan rekonsiliasi bukti putaran 4~~ |

`BE-LAB-45` berstatus `DILEBUR`, bukan terbuka.

> **Koreksi 2026-09-23 — baris `BE-LAB-46` di atas KELIRU, dan sebabnya layak dibaca.** Ia nol
> dibekukan; ia **dibatalkan** 2026-09-18 dan digantikan `BE-LAB-50`/`51`/`52`, sebagaimana sudah
> tertulis pada bagian 6i.6b sejak hari itu. Sapuan ini membaca **blok task 6i.3** yang tertinggal
> `DIBEKUKAN`, lalu menyimpulkan dari blok itu.
>
> **Ini persis kelas kesalahan yang sapuan ini sendiri dibuat untuk menutupnya** — bagian atas
> halaman ini menulis *"Pembaca yang membuka tabel — bukan narasi — akan mengambil pekerjaan yang
> sudah selesai"*, lalu sapuan itu sendiri jatuh ke dalamnya pada baris terakhirnya. Kali ini
> yang tertinggal bukan delapan blok status, melainkan **satu blok yang dibaca sapuan sebagai
> sumber**. Blok 6i.3 dikoreksi 2026-09-23.
>
> **Yang benar-benar masih terbuka per 2026-09-23 karena itu satu, bukan dua:** `BE-LAB-41`.

**Kesimpulan: nol task backend `S4b` yang tertinggal.** Tujuh belas blok task kini seluruhnya
`SELESAI` kecuali yang di atas, dan mereka memang bukan milik slice ini.

---

## 6ac. `BE-LAB-64` SELESAI — permukaan baseline dua data induk Mikrobiologi, 2026-09-22

Dikerjakan atas keputusan pemilik modul **"A. Lengkapi backend dahulu"**, diambil ketika
`FE-LAB-34` hendak dimulai dan ternyata nol dapat dibangun sesuai standar.

### 6ac.1 Bagaimana selisihnya ditemukan

`FE-LAB-34` membangun tiga layar data induk Mikrobiologi. Standar frontend
`rules/frontend/master-data-feature-standard.md` menuntut setiap layar data induk memuat kartu
ringkasan, penyaring yang bentuknya **dibaca dari server**, dan sakelar aktif per baris —
ketiganya bersandar pada `GET /summary`, `GET /filters/metadata`, dan `PATCH /{id}/status`.

Nol satu pun dari ketiganya ada.

| Grup | Dibangun | Dituntut standar |
|---|---|---|
| `lab-susceptibility-breakpoints` | 5 | 9 |
| `lab-procedure-microbiology-profiles` | 5 | 9 |
| `lab-discipline-settings` | 4 | 4 — **varian sah**, lihat 6ac.4 |

### 6ac.2 Ini kelalaian pengerjaan `BE-LAB-60` dan `BE-LAB-62`, bukan temuan pada pekerjaan orang lain

Kedua task itu dibangun **ke kontrak `r27`** dan berhenti di situ. `r27` menyebut lima endpoint,
dan lima endpoint itulah yang dibuat. `rules/backend/master-data-endpoint-standard.md`
**nol dibaca** saat keduanya dikerjakan — padahal standar itulah yang menentukan berapa
endpoint yang wajib ada, dan kontrak hanya mencatat hasilnya.

Akibatnya selisih ini melewati dua task, satu audit kesiapan, dan sapuan status bagian `6ab`
tanpa terlihat. **Bagian `6ab` menyimpulkan "nol task backend `S4b` yang tertinggal", dan itu
tetap benar** — tidak ada task yang tertinggal. Yang tertinggal adalah permukaan yang nol
pernah dijadikan task oleh siapa pun. Sapuan yang menyisir daftar task nol akan menemukannya;
yang menemukannya adalah upaya membangun layar di atasnya.

### 6ac.3 Yang dikerjakan

| Berkas | Perubahan |
|---|---|
| `DTOs/LabSusceptibilityBreakpointDtos.cs` | `SummaryResponse`, `OptionResponse`, `StatusRequest` |
| `DTOs/LabFilterAndSummaryDtos.cs` | Empat DTO profil dan satu metadata breakpoint |
| `Services/LabFilterMetadataFactory.cs` | Dua pabrik metadata baru |
| `Services/LabSusceptibilityBreakpointService.cs` | `GetSummaryAsync`, `GetOptionsAsync`, `SetStatusAsync` |
| `Services/LabProcedureMicrobiologyProfileService.cs` | Ketiganya, sama |
| `Controllers/LabSusceptibilityBreakpointController.cs` | Empat endpoint; 5 → **9** |
| `Controllers/LabProcedureMicrobiologyProfileController.cs` | Empat endpoint; 5 → **9** |

Nol migration, nol kolom, nol permission baru.

### 6ac.4 `lab-discipline-settings` sengaja TIDAK disentuh

`rules/backend/master-data-endpoint-standard.md` menyebut varian sah **"master data pengaturan
tunggal"**: entity yang hanya memuat satu baris konfigurasi cukup `GET /` dan `PUT /{id}`,
sebab *"metadata, summary, options, dan delete tidak berlaku karena tidak ada koleksi data"*.

`LabDisciplineSetting` memuat **tiga baris tetap**, satu per disiplin, yang nol pernah
bertambah maupun berkurang. Menambahkan `summary` di atasnya berarti melaporkan "3 dari 3
aktif" selamanya, dan `DELETE` di atasnya berarti membuka jalan menghapus disiplin yang
dipakai penghitung nomor cetak `r29`.

### 6ac.5 Satu cacat pendaftaran hak akses ikut ditutup

`AccessMenuSeeder` memakai kunci **`(controller, ActionName)`** — satu baris `SysActionAccess`
per nama aksi, bukan per endpoint. Kedua controller sudah memuat dua method bernama `"Read"`
dengan `DisplayName`, `Description`, dan `SortOrder` **berbeda**, sehingga baris yang tersimpan
bergantung pada urutan refleksi menemukan method-nya.

Empat endpoint baru menaikkan taruhannya dari dua menjadi **lima** pesaing untuk satu baris
`Read`. Seluruh atribut `Read` dan `Update` pada kedua controller karena itu diseragamkan,
mengikuti `LabOrganismController` dan `LabAntibioticController` yang memang sudah menuliskan
`DisplayName` dan `SortOrder` identik untuk nama aksi yang sama.

Dibuktikan atas database: kedua controller kini memiliki **tepat empat** baris
`SysActionAccess` — `Read`, `Create`, `Update`, `Delete` — dengan `DisplayName` dan `SortOrder`
yang nol lagi bergantung urutan.

> `RoutePath` pada baris `Read` menunjuk `/options`, yakni method yang terakhir ditemukan.
> **Itu perilaku repo yang sudah ada, bukan cacat baru**: baris `Read` milik `LabOrganism` dan
> `LabAntibiotic` menunjuk `/options` dengan cara yang sama persis. Kolom itu keterangan pada
> layar Akses Role; yang menjaga gerbang adalah `[AccessPermission]`, dan ia berkunci pada
> `(resource, action)`.

### 6ac.6 Verifikasi

| Perintah | Hasil |
|---|---|
| `dotnet build -p:RunAnalyzers=False` | **0 error** |
| Kedelapan endpoint baru lewat HTTP | **`200`** |
| Kelima endpoint lama kedua grup | **`200`** — sekaligus mengoreksi status `r27` 22.5 |
| `PATCH` → `false` lalu `true` | Ringkasan dan `options` bergerak sesuai, data dev **pulih seperti semula** |
| `PATCH` penunjuk asing | **`404`** |
| Baris `SysActionAccess` sesudah seeder | **4 per controller**, deterministik |

Bukti lengkap: kontrak bagian **25.6**, dan [`BE-LAB-64.md`](../task/report/backend/BE-LAB-64.md).

### 6ac.7 Akibatnya bagi `FE-LAB-34`

**Penahannya hilang.** Ketiga layar kini berdiri di atas sembilan endpoint penuh dan dapat
dibangun persis sesuai `master-data-feature-standard.md`, tanpa satu pun permukaan yang
dipalsukan di sisi layar.

### 6ac.8 Status

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-22 |
| Kontrak | `LAB-API-v1` **`r30`** — `approved` 2026-09-22 |
| Sifat | Aditif; nol migration, nol permission baru |
| Batas diketahui | Nol |

---

## 6ad. `BE-LAB-65` SELESAI — dan sapuan menyeluruh menemukan gapnya SISTEMIK, 2026-09-22

Dikerjakan sebagai prasyarat `FE-LAB-24`, mengikuti keputusan pemilik modul yang sama dengan
`BE-LAB-64`: **lengkapi backend dahulu, baru bangun layarnya.**

### 6ad.1 Kali ini disapu SELURUHNYA, bukan ditambal saat tersandung

`BE-LAB-64` menutup dua controller karena `FE-LAB-34` tersandung padanya. Alih-alih menunggu
task berikutnya tersandung lagi, **kedua puluh dua controller Laboratorium diperiksa sekaligus**
terhadap baseline sembilan endpoint.

Gapnya ternyata **sistemik**:

| Grup | Http | Kurang | Menahan |
|---|---|---|---|
| `LabOrganism` | 4 | metadata, summary, `GET /{id}`, `PATCH` | **`FE-LAB-24`** — ditutup di sini |
| `LabAntibiotic` | 4 | keempatnya | **`FE-LAB-24`** — ditutup di sini |
| `LabPathologyCategory` | 6 | keempatnya | `FE-LAB-27` — **masih terbuka** |
| `LabPathologyParameter` | 4 | keempatnya | `FE-LAB-27` — **masih terbuka** |
| `LabProcedurePathologyCategory` | 4 | kelimanya | `FE-LAB-27` — **masih terbuka** |
| `LabDisciplineSetting` | 4 | — | Varian sah "pengaturan tunggal" |

**Ketiga grup Patologi Anatomi sengaja nol disentuh** — mereka milik `FE-LAB-27`, dan
mengerjakannya di sini berarti dua task dalam satu pemanggilan. Gapnya **dicatat**, bukan
ditambal diam-diam, supaya `FE-LAB-27` nol tersandung pada hal yang sudah diketahui.

### 6ad.2 `GET /{id}` — kelas kesalahan yang sudah pernah dibayar modul ini

Tanpa jalur detail, formulir ubah yang dibuka lewat tautan langsung atau sesudah halaman
disegarkan nol punya cara memuat barisnya, dan **gagalnya diam** — layarnya sekadar tampak
kosong. `r6` menutup persis kelas ini sesudah `FE-LAB-03` diam-diam gagal di luar halaman
daftar. Roadmap frontend revision 33 sudah menuliskannya sebagai peringatan bagi `FE-LAB-27`.

### 6ad.3 Satu kolom yang nol punya jalan diisi

`LabAntibiotic.DiscContentUg` sudah ada di tabel sejak `BE-LAB-60`, tetapi **nol satu pun DTO
membawanya**. Akibatnya `missingDiscContent` — angka yang baru dibangun `BE-LAB-64` sehari
sebelumnya — melaporkan pekerjaan yang tersisa **tanpa menyediakan jalan mengerjakannya**.

Ruasnya dibuka pada respons, create, dan update antibiotik. Dibuktikan menutup lingkarnya:
menyetel `discContentUg` menjadi `10` menurunkan `missingDiscContent` dari `1` menjadi `0` pada
**kedua** ringkasan, dan mengisi kolom `UG` pada daftar breakpoint.

### 6ad.4 `DELETE` tetap nol disediakan

`r24` 19.4 menolaknya atas alasan klinis: isolat yang sudah tercatat menunjuk ke baris ini.
`AC-117` menuntut layarnya nol menampilkan tombol Hapus. Kedua grup berhenti di **delapan**
endpoint, dan `filters/metadata` menyatakannya lewat `isDeletable: false` — supaya layar
membacanya alih-alih menyimpulkan dari ada-tidaknya endpoint.

Dibuktikan pada database: **3 baris `SysActionAccess` per controller** — `Read`, `Create`,
`Update` — dan **nol baris `Delete`**.

### 6ad.5 Dua cacat ditemukan saat menguji, dan diperbaiki

**`withBreakpoint` membantah dirinya sendiri.** Versi pertama menghitung breakpoint aktif tanpa
memeriksa apakah organismenya sendiri masih aktif, sehingga ringkasannya berbunyi *"nol organisme
aktif, tetapi satu tercakup"*. Kini kedua syarat diperiksa.

**Pesan jawaban `PATCH` berbahasa Inggris** — *"Lab Organism dinonaktifkan."*, nama teknis
controller. Diganti menjadi *"Organisme dinonaktifkan."*

### 6ad.6 Verifikasi

| Yang diuji | Hasil |
|---|---|
| `dotnet build -p:RunAnalyzers=False` | **0 error** |
| Kedelapan endpoint baru | `200` |
| Keempat endpoint lama kedua grup | `200` — sekaligus mengoreksi status `r24` 19.4 |
| `GET /{id}` dan `PATCH` penunjuk asing | `404` |
| `missingDiscContent` sesudah `discContentUg` diisi | 1 → **0** pada **kedua** ringkasan |
| `PATCH` → `false`: `options` / `GET /` | `0` / `1` — `VAL-85` dan `AC-118` tegak bersamaan |
| Data dev sesudah seluruh uji | **Pulih seperti semula** |

Bukti lengkap: kontrak bagian **26.6**, dan [`BE-LAB-65.md`](../task/report/backend/BE-LAB-65.md).

### 6ad.7 Status

| Butir | Isi |
|---|---|
| **Status** | ✅ **`SELESAI`** 2026-09-22 |
| Kontrak | `LAB-API-v1` **`r31`** — `approved` 2026-09-22 |
| Sifat | Aditif; nol migration, nol permission baru |
| Batas diketahui | Nol pada cakupannya. **Tiga grup Patologi Anatomi tetap kurang** — `FE-LAB-27`, bagian 6ad.1. **Ditindaklanjuti `BE-LAB-66`**, bagian 6ae |

---

## 6ae. `BE-LAB-66` DIBUKA — permukaan baseline tiga data induk Patologi Anatomi, 2026-09-23

Dibuka atas instruksi pemilik modul, mengikuti keputusan yang sama yang melahirkan `BE-LAB-64`
dan `BE-LAB-65`: **lengkapi backend dahulu, baru bangun layarnya.**

**Ia menutup selisih yang sudah dicatat, bukan yang baru ditemukan.** `BE-LAB-65` bagian 6ad.1
menyapu kedua puluh dua controller Laboratorium terhadap baseline sembilan endpoint, menemukan
ketiga grup Patologi Anatomi kurang, dan **sengaja nol menyentuhnya** — mengerjakannya di sana
berarti dua task dalam satu pemanggilan. Gapnya dituliskan supaya `FE-LAB-27` nol tersandung
pada hal yang sudah diketahui. Task ini adalah tindak lanjut yang dijanjikan catatan itu.

### 6ae.1 Selisihnya diperiksa ulang terhadap source, bukan disalin dari catatan

Dibaca 2026-09-23 dari ketiga controller di
`Areas/HealthServices/LaboratoryManagement/Controllers/`:

| Grup | Yang ada sekarang | Kurang |
|---|---|---|
| `lab-pathology-parameters` | `GET /`, `GET /options`, `POST`, `PUT /{id}` — **4** | `filters/metadata`, `summary`, `GET /{id}`, `PATCH /{id}/status` |
| `lab-pathology-categories` | `GET /`, `GET /options`, `GET /{id}/parameters`, `PUT /{id}/parameters`, `POST`, `PUT /{id}` — **6** | keempat yang sama |
| `lab-procedure-pathology-categories` | `GET /`, `GET /suggestions`, `POST`, `PUT /{id}` — **4** | `filters/metadata`, `summary`, `GET /{id}` — **dan hanya ketiganya**, lihat 6ae.4 |

Kedua endpoint tambahan di luar baseline — `{id}/parameters` pada golongan dan `suggestions`
pada pemetaan — **nol disentuh**. Keduanya sah menurut
`rules/backend/master-data-endpoint-standard.md` bagian 3, dan keduanya sudah terpakai rancangan
`FE-LAB-27`.

### 6ae.2 Kenapa ini menahan `FE-LAB-27`, dan kenapa penahannya nol terlihat dari daftar task

`rules/frontend/master-data-feature-standard.md` menuntut setiap layar data induk memuat kartu
ringkasan, penyaring yang bentuknya **dibaca dari server**, dan sakelar aktif per baris —
ketiganya bersandar pada `GET /summary`, `GET /filters/metadata`, dan `PATCH /{id}/status`.
Bagian 3 memetakan sembilan endpoint ke sembilan thunk, satu lawan satu.

**`GET /{id}` yang paling mahal di antara keempatnya.** Tanpa jalur detail, formulir ubah yang
dibuka lewat tautan langsung atau sesudah halaman disegarkan nol punya cara memuat barisnya, dan
**gagalnya diam** — layarnya sekadar tampak kosong. `LAB-API-v1` `r6` lahir menutup persis kelas
ini sesudah `FE-LAB-03` diam-diam gagal di luar halaman daftar.

Penahan ini **nol akan ditemukan oleh sapuan yang menyisir daftar task**, sebab tidak ada task
yang tertinggal — yang tertinggal adalah permukaan yang nol pernah dijadikan task oleh siapa pun.
Itu kesimpulan yang sama dengan 6ac.2, dan ini kali ketiga bentuknya muncul pada modul ini.

### 6ae.3 Yang dibangun

**`lab-pathology-parameters`** dan **`lab-pathology-categories`** — masing-masing empat endpoint,
4 → 8 dan 6 → 10:

| Method | Path | Hak akses |
|---|---|---|
| `GET` | `/filters/metadata` | `<grup> : Read` |
| `GET` | `/summary` | `<grup> : Read` |
| `GET` | `/{id}` | `<grup> : Read` |
| `PATCH` | `/{id}/status` | `<grup> : Update` |

**`lab-procedure-pathology-categories`** — tiga endpoint, 4 → 7: `filters/metadata`, `summary`,
dan `GET /{id}`, seluruhnya menumpang `LabPathologyCategory : Read` seperti keempat endpoint yang
sudah ada di controller itu.

Bentuk `summary` yang diusulkan:

| Grup | Ruas |
|---|---|
| Parameter | `totalParameter`, `activeParameter`, `inactiveParameter`, `usedInCategory` |
| Golongan | `totalCategory`, `activeCategory`, `inactiveCategory`, `withParameter` |
| Pemetaan | `totalProcedure`, `mappedProcedure`, **`unmappedProcedure`** |

**`unmappedProcedure` bukan hiasan.** Ia angka yang menjawab pertanyaan yang menahan `FE-LAB-28`
— berapa jenis pemeriksaan Patologi Anatomi yang belum digolongkan — dan `AC-152` menuntut
layar laporan menyebut **apa** yang belum diatur ketika pesanan nol terpetakan. Tanpa angka ini,
kepala instalasi nol punya cara mengetahui pekerjaannya tersisa berapa selain membuka daftar dan
menghitungnya sendiri.

### 6ae.4 Dua hal yang sengaja TIDAK dibangun, dan keduanya varian sah

**`PATCH /{id}/status` nol berlaku bagi `lab-procedure-pathology-categories`.**
`LabProcedurePathologyCategory` **nol punya `IsActive`** — dibaca dari
`Models/LabProcedurePathologyCategory.cs`, ia hanya memuat `Id`, `ProcedureId`,
`LabPathologyCategoryId`, dan dua navigasi di atas `IdentityModel`. Ia baris pemetaan, bukan
baris data induk berstatus. Menambahkan kolom status di atasnya berarti **migration**, dan itu
keluar dari sifat aditif task ini. Memindahkan penggolongan satu pemeriksaan dilakukan lewat
`PUT /{id}` yang sudah ada.

**`GET /options` nol dibangun bagi grup pemetaan.** Nol satu pun layar memilih sebuah *pemetaan*
dari dropdown — yang dipilih adalah jenis pemeriksaan dan golongannya, dan keduanya sudah punya
`/options` sendiri. Membangunnya berarti mengulang pola `BE-LAB-26`: sesuatu yang berdiri tanpa
pembaca lalu nol menghasilkan galat apa pun sampai seseorang membutuhkannya.

> **Catatan koreksi terhadap 6ad.1.** Tabel di sana mencatat grup pemetaan *"kurang kelimanya"*.
> Pembacaan entity pada 6ae.4 menunjukkan **tiga** yang benar-benar berlaku. Angka di sana
> diturunkan dari baseline sembilan tanpa memeriksa entity-nya; ini koreksinya, bukan pengurangan
> cakupan.

**`DELETE` tetap nol disediakan pada ketiganya.** Alasannya sama dengan `r24` 19.4 dan `r31`
26.4, dan di sini lebih tajam: parameter yang dihapus menarik ruas dari laporan pasien yang sudah
tersimpan, dan golongan yang dihapus membuat pesanan lama nol punya bentuk formulir. `AC-143`
menuntut layarnya nol menampilkan tombol Hapus. `filters/metadata` menyatakannya lewat
`isDeletable: false`, supaya layar membacanya alih-alih menyimpulkan dari ada-tidaknya endpoint.

### 6ae.5 Satu risiko yang sudah pernah menggigit `BE-LAB-64`, dan di sini separuh sudah tertutup

`AccessMenuSeeder` memakai kunci **`(controller, ActionName)`** — satu baris `SysActionAccess`
per nama aksi, bukan per endpoint. `LabPathologyCategoryController` sudah memuat **tiga** method
bernama `"Read"`; tiga endpoint baru menjadikannya **enam** pesaing untuk satu baris.

Diperiksa 2026-09-23: `DisplayName` dan `SortOrder` pada ketiga controller **sudah seragam** per
nama aksi — `Read`/1, `Create`/2, `Update`/3 — sehingga bagian paling berbahaya dari 6ac.5 nol
berlaku di sini. Yang **belum** seragam adalah `Description`, sehingga keterangan yang tersimpan
masih bergantung urutan refleksi. Task ini menyeragamkannya mengikuti pola `BE-LAB-64`, dan
membuktikannya atas database: **tepat tiga baris** `SysActionAccess` per controller pada kedua
grup berstatus, dan **dua** pada grup pemetaan yang nol punya `Create`.

### 6ae.6 Kontrak — amandemen `r32`, BELUM disetujui

| Butir | Isi |
|---|---|
| Usul | `LAB-API-v1` **`r32`** — permukaan baseline tiga data induk Patologi Anatomi |
| Status | ✅ **`approved` 2026-09-23** oleh pemilik modul, atas pilihan **"Setujui penuh — 11 endpoint"**. Tercatat sebagai contract bagian **27** |
| Sifat | **ADITIF.** Sebelas endpoint ditambahkan; nol endpoint berubah route, verb, bentuk, atau hak akses |
| Migration | **Nol.** Keempat tabel berdiri sejak `BE-LAB-50`; yang kurang hanya jalan keluarnya |
| Permission baru | **Nol.** Kesebelasnya menumpang `Read` dan `Update` yang sudah ada |
| Preseden | `r30` bagian 25 (`BE-LAB-64`) dan `r31` bagian 26 (`BE-LAB-65`) — amandemen sebangun, keduanya disetujui pada hari yang sama ia diusulkan |

### 6ae.7 Acceptance criteria

Melanjutkan `AC-191` milik `MVP-7b`; nol nomor lama disentuh.

| AC | Bunyi |
|---|---|
| `AC-192` | Ketiga grup menjawab `GET /{id}` dengan baris utuh bagi penunjuk yang ada, dan **`404`** bagi penunjuk asing — dibuktikan pada ketiganya, bukan pada satu sebagai wakil |
| `AC-193` | `PATCH /{id}/status` → `false` mengeluarkan baris dari `GET /options` dan **tetap menampilkannya** pada `GET /` bertanda nonaktif; data dev pulih seperti semula sesudah diuji |
| `AC-194` | `unmappedProcedure` pada ringkasan pemetaan **turun tepat satu** ketika satu penggolongan disimpan, dan kembali naik ketika dibatalkan |

`AC-192` dan `AC-193` dibuktikan **pada ketiga grup**. `AC-193` nol berlaku bagi grup pemetaan —
ketiadaan `PATCH` di sana adalah keputusan 6ae.4, dan dibuktikan **terbalik**: nol endpoint status
terdaftar pada controller itu.

### 6ae.8 Dependency, verifikasi, dan DoD

| Butir | Isi |
|---|---|
| **Status** | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI` 2026-09-23** — `r32` disetujui dan kesebelas endpoint dibangun pada hari yang sama task ini dibuka. Lihat bagian **6af** dan [`BE-LAB-66.md`](../task/report/backend/BE-LAB-66.md) |
| Dependency | ✅ `BE-LAB-50` selesai 2026-09-18 — keempat tabel berdiri, migration terterap, seeder dijalankan |
| Menahan | **`FE-LAB-27`**, dan lewat `FE-LAB-27` ikut menahan pengisian pemetaan yang `FE-LAB-28` tunggu |
| Klasifikasi | `MEDIUM` — sebelas endpoint, tiga controller, nol migration, nol permission baru |
| Verifikasi | `dotnet build -p:RunAnalyzers=False` **0 error**; kesebelas endpoint baru dan kesepuluh endpoint lama dipanggil lewat HTTP; `AC-192`..`AC-194` dibuktikan terhadap database; baris `SysActionAccess` dihitung sesudah seeder |
| DoD | Ketiga grup mencapai permukaan yang dinyatakan 6ae.3; kedua pengecualian 6ae.4 **dibuktikan terbalik**; `AC-192`..`AC-194` terbukti; data dev pulih seperti semula; **nol** endpoint lama berubah bentuk |
| Risiko | **Rendah pada kodenya, sedang pada pembukuannya.** Polanya sudah dua kali dijalankan lewat `BE-LAB-64` dan `BE-LAB-65`; yang perlu dijaga adalah `Description` seragam pada 6ae.5 dan godaan membangun `PATCH` serta `/options` pada grup pemetaan "sekalian supaya genap sembilan" |

---

## 6af. `BE-LAB-66` SELESAI DENGAN BATAS VERIFIKASI — dan satu selisih `BE-LAB-65` ikut ditemukan, 2026-09-23

Dibuka dan dikerjakan pada hari yang sama. `r32` disetujui pemilik modul lebih dulu; tanpa itu
task ini berhenti pada langkah pertama, sebab kontrak yang masih draft adalah gerbang keras.

Laporan: [`BE-LAB-66.md`](../task/report/backend/BE-LAB-66.md).

### 6af.1 Yang dibangun

| Grup | Semula | Sesudah | Ditambahkan |
|---|---:|---:|---|
| `lab-pathology-parameters` | 4 | **8** | `filters/metadata`, `summary`, `GET /{id}`, `PATCH {id}/status` |
| `lab-pathology-categories` | 6 | **10** | keempat yang sama |
| `lab-procedure-pathology-categories` | 4 | **7** | `filters/metadata`, `summary`, `GET /{id}` |

Sembilan berkas source, seluruhnya di dalam `Areas/HealthServices/LaboratoryManagement/`. Nol
migration, nol entity, nol permission baru.

### 6af.2 `GetByIdAsync` grup pemetaan nol ditulis ulang

Ia **sudah ada sejak `BE-LAB-50`** sebagai pembantu internal — ia yang menyusun jawaban
`CreateAsync` dan `UpdateAsync` — dan bentuk maupun penolakan `404`-nya sudah persis yang dituntut
baseline. Yang berubah hanya pengubah aksesnya, dari `private` menjadi `public`.

Menulis jalur kedua untuk bentuk yang sama akan melahirkan **dua kebenaran** atas satu pertanyaan,
dan keduanya pasti bercabang pada perubahan berikutnya.

### 6af.3 Satu selisih `BE-LAB-65` ditemukan, dan sengaja NOL ditambal

`LabFilterMetadataFactory.LabOrganism()` dan `LabAntibiotic()` mengirim `SortOptions` berisi tiga
pilihan urutan. **Kedua query DTO-nya nol punya ruas `SortBy` maupun `SortDirection`**, dan
daftarnya diurutkan tetap di dalam service. Layar yang merender pemilih urutan dari metadata itu
menampilkan kendali yang **nol mengubah satu baris pun** — dan kepala instalasi akan menyimpulkan
urutannya sudah diatur.

Ketiga grup Patologi Anatomi karena itu mengirim `SortOptions` **kosong**, jujur terhadap
keadaannya, beserta komentar yang menjelaskan kenapa ia berbeda dari kedua kembarannya.

**Kedua grup Mikrobiologi sengaja nol disentuh**, dua alasan: memperbaikinya berarti menyentuh
cakupan `BE-LAB-65`, dan menambahkan ruas sort pada endpoint daftar yang sudah berjalan
**bertentangan dengan `r32` bagian 27.6** yang baru saja disetujui — ia menyatakan kesepuluh
endpoint lama nol berubah bentuk. Selisihnya **dilaporkan**, dan keputusannya milik pemilik modul.

### 6af.4 Satu cacat ditangkap sebelum dikirim — kelas yang sama dengan `withBreakpoint`

Versi pertama `usedInCategory` dan `withParameter` menghitung keberlakuan yang hidup **tanpa
memeriksa apakah parameter atau golongannya sendiri masih hidup**. Bila suatu saat satu baris data
induk ditandai terhapus sementara keberlakuannya tertinggal, ringkasannya melaporkan angka
terpakai yang **lebih besar daripada totalnya**.

Ini bentuk yang sama persis dengan cacat `withBreakpoint` pada 6ad.5 — *"nol organisme aktif,
tetapi satu tercakup"*. Kedua penghitung kini memeriksa **kedua syarat**.

> Hari ini ia nol dapat menyala, sebab ketiga grup nol punya `DELETE`. Diperbaiki bukan karena
> sedang merusak, melainkan karena penghitung yang benar hanya selama jalur perusaknya belum ada
> adalah penghitung yang sedang menunggu.

### 6af.5 Risiko `AccessMenuSeeder` diperiksa, dan ternyata separuh sudah tertutup

`LabPathologyCategoryController` kini memuat **enam** method bernama `"Read"`. `DisplayName` dan
`SortOrder` ketiga controller sudah seragam sejak `BE-LAB-50`, sehingga bagian paling berbahaya
dari 6ac.5 nol berlaku. Yang belum seragam adalah `Description`; ketiganya diseragamkan pada task
ini mengikuti pola `LabOrganismController`.

### 6af.6 Verifikasi — dan batasnya disebut apa adanya

| Yang diuji | Hasil |
|---|---|
| `dotnet build -p:RunAnalyzers=False` | **0 error** |
| Rebuild penuh, peringatan disaring pada berkas yang disentuh | **Nol peringatan baru** |
| `git status --short` | 13 berkas — 9 source, 4 dokumen. Nol berkas lain |
| Kesebelas endpoint lewat HTTP | ⚠ **NOL dipanggil** |
| `AC-192`..`AC-194` | ⚠ **NOL dibuktikan** |

**Penyebabnya tunggal dan bukan kode:** aplikasi nol dapat distart karena `Jwt:Key` belum
dikonfigurasi di environment ini, dan nilainya rahasia milik pemilik — nol ditebak maupun diisi
dari task ini. Batas yang sama sudah dicatat `FE-LAB-24` dan `FE-LAB-34`.

### 6af.7 Akibatnya bagi `FE-LAB-27`

**Penahannya hilang.** Ketiga layar kini berdiri di atas permukaan penuh dan dapat dibangun persis
sesuai `master-data-feature-standard`, tanpa satu pun permukaan yang dipalsukan di sisi layar.
Dua ketiadaan yang disengaja pada grup pemetaan **dinyatakan pada metadata** lewat
`supportsStatusToggle` dan `hasOptionsEndpoint`, sehingga layar membacanya alih-alih menyimpulkan
dari endpoint yang menjawab `404`.

### 6af.8 Status

| Butir | Isi |
|---|---|
| **Status** | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** 2026-09-23 |
| Kontrak | `LAB-API-v1` **`r32`** — `approved` 2026-09-23, bagian 27 |
| Sifat | Aditif; nol migration, nol permission baru |
| Batas diketahui | Kesebelas endpoint **nol pernah dipanggil**. Yang paling patut diperiksa lebih dulu: `unmappedProcedure` — satu-satunya angka baru yang dihitung dari dua tabel, dan satu-satunya yang salahnya nol menimbulkan galat |

---

## 6ag. Papan kendali status task backend — 2026-09-23

> **Bagian ini dimaksudkan sebagai SATU tempat membaca status, dan sengaja menggantikan kebiasaan
> menyimpulkan status dari blok task.** Sapuan 6ab sudah membuktikan blok task dapat tertinggal;
> pemeriksaan hari ini menemukan sapuan itu sendiri memuat **dua** kekeliruan. Papan ini
> diturunkan dari **laporan task**, yaitu artefak yang lahir bersama pekerjaannya, lalu
> dicocokkan terhadap blok roadmap — bukan sebaliknya.

### 6ag.1 Angka pokok

| Golongan | Jumlah |
|---|---:|
| Task backend yang pernah ada | **72** |
| ✅ Selesai dan terbukti | **65** *(naik dari 64 pada 2026-09-23 — `BE-LAB-66` terbukti runtime, bagian 6ai)* |
| ⚠ Selesai dengan catatan | **3** |
| ⛔ Tertahan | **1** |
| ❌ Dibatalkan / dilebur | **3** |
| 🟢 **Siap dikerjakan** | **NOL** |

**Cara angkanya diperoleh, supaya dapat diperiksa ulang.** `BE-LAB-01`..`BE-LAB-66` adalah 66 ID,
ditambah `BE-EXT-01`, `02`, `03`, `04`, `04b`, `05` menjadi **72**. Folder laporan memuat **67**
berkas; satu di antaranya — `BE-LAB-23.md` — berisi **pembatalan**, bukan penyelesaian, sehingga
66 laporan mewakili pekerjaan selesai. Ditambah `BE-LAB-40` dan `BE-LAB-42` yang **sengaja**
berlaporan di dalam `BE-LAB-39.md`, jumlah task selesai menjadi **68**, empat di antaranya
berkatatan. Sisanya: 1 tertahan, 3 dibatalkan/dilebur. 68 + 1 + 3 = **72**.

### 6ag.2 Selesai dan terbukti — 64 task

| Kelompok | Task |
|---|---|
| Penunjang lintas modul | `BE-EXT-01`, `BE-EXT-02`, `BE-EXT-04`, `BE-EXT-04b`, `BE-EXT-05` |
| Fondasi dan data induk | `BE-LAB-01` .. `BE-LAB-21` |
| Penerimaan specimen, kiosk, konfirmasi | `BE-LAB-24` .. `BE-LAB-39`, `BE-LAB-40`, `BE-LAB-42`, `BE-LAB-43` |
| Data induk Mikrobiologi | `BE-LAB-44`, `BE-LAB-47`, `BE-LAB-48` |
| Patologi Anatomi | `BE-LAB-50`, `BE-LAB-51`, `BE-LAB-52` |
| Slice `S4b` hasil Mikrobiologi | `BE-LAB-53` .. `BE-LAB-63` |
| Permukaan baseline data induk | `BE-LAB-64`, `BE-LAB-65` |

Masing-masing punya laporan sendiri di [`task/report/backend/`](../task/report/backend/), kecuali
`BE-LAB-40` dan `BE-LAB-42` yang **sengaja** dilaporkan di dalam `BE-LAB-39.md`.

### 6ag.3 Selesai dengan catatan — 4 task

| Task | Catatan | Yang tersisa |
|---|---|---|
| `BE-EXT-03` | `SELESAI` untuk kolom dan kontrak | Endpoint pemanggilan `INT-05` **milik `registration-management`**, bukan Laboratorium |
| `BE-LAB-22` | Laporannya menulis `SELESAI SEBAGIAN` (2026-09-15) | **Sudah ditutup penuh** 2026-09-17 oleh `BE-LAB-37`; `VAL-59` menyala, `AC-66` penuh. Laporannya yang tertinggal, bukan pekerjaannya |
| `BE-LAB-49` | ⚠ `SELESAI DENGAN TEMUAN` | `AC-115` terbukti **separuh**; separuh lainnya **terbantah**, dan itu temuan nyata bukan kegagalan pengujian |
| ~~`BE-LAB-66`~~ | ✅ **Naik menjadi `SELESAI` 2026-09-23** | Kesebelas endpoint terbukti `200`; `AC-192` dan `AC-193` terbukti penuh, `AC-194` separuh. Bagian 6ai |

### 6ag.4 Tertahan — 1 task

| Task | Penahan | Sifat penahan |
|---|---|---|
| `BE-LAB-41` penyimpanan pengiriman hasil | **`LAB-COORD-011`** (sisi penulis) dan **`DEC-LAB-011`** lewat `S17` (sisi pembaca) | **Nol satu pun berupa kode.** Keduanya keputusan/pengadaan milik pihak di luar rekayasa |

**Rancangannya lengkap** (`02-backend-architecture.md` bagian 13) dan kodenya kecil. Ia tetap nol
dikerjakan **dengan sengaja**: tabelnya hari ini nol punya penulis **dan** nol punya pembaca.
`BE-EXT-04` sudah membuktikan ongkos membangun lebih dini — 16 dari 16 sesi bernilai `null`, dan
`BE-EXT-04b` harus dibuat menyusul. Membangunnya sekarang mengulangnya dengan **kedua** sisi
kosong.

### 6ag.5 Dibatalkan dan dilebur — 3 task

| Task | Status | Sebab |
|---|---|---|
| `BE-LAB-23` | ⛔ `DIBATALKAN` | `LAB-DEC-038` dicabut `LAB-DEC-050` pada hari yang sama. Nol baris source pernah ditulis |
| `BE-LAB-45` | `DILEBUR` | Cakupannya dipersempit lalu diserap task lain |
| `BE-LAB-46` | ❌ `DIBATALKAN` 2026-09-18 | Digantikan `BE-LAB-50`/`51`/`52` sesudah `S4c` dirancang ulang |

### 6ag.6 Dua kekeliruan sapuan 6ab, dan kenapa papan ini ada

| Yang ditulis 6ab | Yang sebenarnya | Akibatnya |
|---|---|---|
| `BE-LAB-46` 🧊 `DIBEKUKAN` — satu dari dua yang "benar-benar masih terbuka" | ❌ **Dibatalkan** 2026-09-18, empat hari sebelum sapuan | Pembaca menyimpulkan ada dua task terbuka; sebenarnya satu |
| `BE-LAB-23` ✅ "selesai, berlaporan sendiri" | ⛔ **Dibatalkan**. Laporannya memang ada — isinya **pembatalan**, bukan penyelesaian | "Punya laporan" dibaca sebagai "selesai" |

**Keduanya lahir dari cara yang sama:** menyimpulkan status dari keberadaan artefak, bukan dari
isinya. Sapuan itu sendiri menulis *"Pembaca yang membuka tabel — bukan narasi — akan mengambil
pekerjaan yang sudah selesai"*, lalu jatuh ke dalamnya pada kedua tabel terakhirnya.

### 6ag.7 Nol task backend yang siap dikerjakan, dan itu keadaan yang sehat

Seluruh pekerjaan backend yang punya dasar **sudah dikerjakan**. Satu-satunya yang tersisa
tertahan oleh hal yang bukan kode.

**Yang menghalangi modul ini maju bukan lagi backend.** Ia tiga hal milik orang:

| Penahan | Milik | Yang dibutuhkan | Membuka |
|---|---|---|---|
| **`DEC-LAB-011`** | Kepala instalasi + manajemen RS | Siapa berwenang menetapkan pemegang kewenangan validasi, dan sanggupkah RS menjamin **dua per shift** | `S4`, `S4d`, `S4e`; `AC-183`; sisi pembaca `BE-LAB-41`; layar `S5`/`S6` |
| **`LAB-COORD-011`** | Pemilik platform + pemilik modul | Pengadaan gerbang pesan dan pembangkit PDF. **Diverifikasi ulang 2026-09-23: masih nol keduanya** | Sisi penulis `BE-LAB-41`; seluruh `CAP-009`/`CAP-010` |
| **`LAB-OPEN-039`** | Pemilik modul | **Penyerahan berkas gambar** template cetak. Diperiksa 2026-09-23: nol berkas gambar di seluruh folder blueprint | Bentuk akhir cetak; `AC-177`, `AC-179` |

**`Jwt:Key` bukan penahan task, tetapi ia menahan pembuktian enam task sekaligus** —
`BE-LAB-66`, `FE-LAB-24`, `FE-LAB-31`, `FE-LAB-32`, `FE-LAB-33`, `FE-LAB-34`. Ia yang paling
murah di antara seluruh penahan yang tercatat.

> **Dikoreksi 2026-09-23 — `Jwt:Key` NOL pernah menjadi penahan.** Kuncinya sudah ada di
> `appsettings.Development.json`; yang kurang hanya menjalankan aplikasi dengan
> `ASPNETCORE_ENVIRONMENT=Development`. Lihat bagian **6ah**. Penahan verifikasi yang sebenarnya
> berbeda, dan baru terlihat ketika aplikasinya benar-benar dijalankan.

---

## 6ah. Modul Laboratorium diperiksa dengan akun sungguhan — 2026-09-23

Dijalankan atas permintaan pemilik modul memakai akun **`Kepala Instalasi Laboratorium`**.
Aplikasi dijalankan lokal terhadap database dev bersama, dan pemeriksaannya **dibatasi pada
permintaan baca** — nol `POST`, `PUT`, maupun `PATCH` — supaya data bersama nol tersentuh.

### 6ah.1 Penahan verifikasi yang selama ini dicatat ternyata KELIRU

Lima laporan task — `BE-LAB-66`, `FE-LAB-24`, `FE-LAB-31`..`FE-LAB-34` — menandai pembuktian
runtime `NOT RUN` dengan alasan *"backend gagal start karena `Jwt:Key` belum dikonfigurasi, dan
nilainya rahasia pemilik"*.

**Kuncinya sudah ada sejak awal**, di `appsettings.Development.json`. Yang kurang hanya
environment saat dijalankan. Aplikasi hidup pada percobaan pertama dan menjawab `/health` `200`.

**Yang pantas diambil dari ini bukan kunci yang terlewat, melainkan bentuk kesalahannya:** sebuah
penahan dicatat sekali, lalu **disalin lima kali** tanpa dicoba ulang. Ia kelas yang sama dengan
blok status stale pada 6ab — keterangan yang benar pada hari ditulis, lalu diperlakukan sebagai
fakta permanen.

### 6ah.2 Nol `500` di seluruh modul Laboratorium

Dua puluh dua jalur baca diperiksa. Sebarannya:

| Jawaban | Jumlah | Arti |
|---|---:|---|
| `200` | **8 grup** | Berjalan dan mengembalikan data |
| `403` | **11 jalur** | Route ditemukan, izin ditolak |
| `404` | **5 jalur** | Controller-nya memang nol punya `GET /` — perilaku benar |
| **`500`** | **NOL** | — |

Grup yang menjawab `200`: `lab-specimen-types`, `lab-specimen-detail-types`, `lab-organisms`,
`lab-antibiotics`, `lab-discipline-settings`, `lab-microbiology-critical-rules`,
`lab-procedure-microbiology-profiles`, `lab-susceptibility-breakpoints`.

**Kesebelas endpoint `BE-LAB-66` terbukti terdaftar dan ter-routing.** Perbedaannya dapat
dibedakan dengan jelas: jalur yang memang tidak ada menjawab `404`, sedangkan kesebelas jalur baru
menjawab `403` — jawaban filter izin yang berjalan **sesudah** route ditemukan.

### 6ah.3 Celah izin yang menahan `FE-LAB-27`, dan ia bukan kode

Akun Kepala Instalasi ditolak `403` pada **ketiga** grup Patologi Anatomi, padahal
`LAB-PERM-v1` rev 7 menugaskan keduanya kepadanya:

| Baris matriks | Isi |
|---|---|
| 415 | `LabPathologyParameter` `Create`, `Update` → **Kepala instalasi** |
| 417 | `LabPathologyCategory` `Create`, `Update` → **Kepala instalasi**, termasuk keberlakuan dan pemetaan |

Hak akses berasal dari `SysAccessPolicies` yang dijoinkan per **`(DepartmentId, PositionId)`** —
bukan dari role Identity, sehingga `"roles":[]` pada respons login adalah keadaan normal dan bukan
tanda akun rusak. **Nol baris kebijakan** ada bagi pasangan departemen-posisi jabatan ini terhadap
ketiga controller Patologi Anatomi. Akun yang sama menjawab `200` pada delapan grup lain.

**Akibatnya bagi `FE-LAB-27` langsung:** ketiga layar dapat dibangun, tetapi **orang yang layar itu
dibuat untuknya nol dapat membukanya**. Ia nol akan terlihat dari uji unit maupun dari build.

**Satu hal ikut terlihat dan pantas diperiksa pemilik modul.** Matriks yang sama memberi `Read`
kepada **Dokter Lab dan Petugas Lab**, bukan kepada kepala instalasi — padahal layar kelola wajib
memuat daftarnya lebih dulu sebelum dapat mengubah. Entah barisnya yang kurang, entah kepala
instalasi memang diandaikan memegang `Read` lewat jalur lain; keduanya perlu dinyatakan.

### 6ah.4 Jalur lain yang ditolak bagi jabatan ini

Selain trio Patologi Anatomi: `lab-orders`, `lab-rejection-reasons`, `lab-value-bounds`,
`lab-specimens`, ketiga `lab-monitoring`, kedua `lab-worklists`, dan kedua `lab-catalog`.

**Sebagiannya mungkin memang disengaja** — kepala instalasi bukan pelaksana harian pesanan dan
wadah. Yang **tidak** dapat dibenarkan dari blueprint hanyalah trio Patologi Anatomi, sebab
matriks izin menugaskannya secara eksplisit. Sisanya dicatat sebagai **pertanyaan**, bukan sebagai
cacat.

### 6ah.5 Satu catatan kesiapan ikut basi

[`testing/readiness-report.md`](../testing/readiness-report.md) baris 64 dan 342 mencatat
`Kepala Instalasi Laboratorium` **0 pengguna aktif**, dan *"dr. Bima Prasetya, Sp.PK ada sebagai
`MstDoctor` tetapi nol punya akun"*. **Akunnya kini ada dan berhasil login.** Butir `2b`/`B3`
karena itu sudah bergerak, walau izinnya belum lengkap.

---

## 6ai. Izin `FE-LAB-27` ditutup, dan tiga alur diuji dengan akun sungguhan — 2026-09-23

Dijalankan atas permintaan pemilik modul, memakai tiga akun: **Kepala Instalasi**, **kiosk**, dan
**superadmin**. Aplikasi dijalankan lokal terhadap database dev bersama.

### 6ai.1 Izin diberikan — dan satu jebakan dihindari lebih dulu

Enam pasangan diberikan lewat `POST /administrator/setting/role-access/policies`:
`LabPathologyParameter` dan `LabPathologyCategory`, masing-masing `Read`, `Create`, `Update`.

> **Endpoint itu memakai `overwriteTarget: true` — ia MENGGANTI seluruh set, bukan menambah.**
> Mengirim hanya enam pasangan baru akan **menghapus 21 izin yang sudah dimiliki jabatan ini**.
> Set lama karena itu dibaca lebih dulu dan dikirim ulang utuh. `totalAllowed` 21 → **27**.

**Grup pemetaan sengaja nol diberi izin sendiri.** `[AccessPermission]` pada
`LabProcedurePathologyCategoryController` menunjuk `LabPathologyCategory`, dan argumen pertama
atribut itulah yang dibaca filter — persis yang `LAB-PERM-v1` rev 7 minta: *"keberlakuan dan
pemetaan ikut `LabPathologyCategory : Update`, bukan resource sendiri"*. **Kodenya benar.** Baris
registry `LabProcedurePathologyCategory` yang lahir dari `[AccessController]` karena itu **nol
pernah dibaca siapa pun** — dicatat, bukan ditambal.

**Satu butir tetap perlu keputusan pemilik modul.** Matriks memberi `Read` kepada Dokter Lab dan
Petugas Lab, **bukan** kepada kepala instalasi — padahal layar kelola wajib memuat daftar sebelum
dapat mengubah, dan `GET /{id}` adalah jalur membuka formulir ubahnya. `Read` diberikan atas dasar
itu. Bila yang benar justru matriksnya, izin ini yang perlu dicabut.

### 6ai.2 `BE-LAB-66` naik menjadi ✅ `SELESAI`

Sesudah izinnya ada, kesebelas endpoint menjawab `200` dan acceptance criteria-nya dibuktikan:

| AC | Hasil |
|---|---|
| `AC-192` | ✅ **Dua arah, ketiga grup.** `200` berisi `MAKROSKOPIK`, `HISTO` (`parameterCount 3`), `Histopatologi Biopsi Besar`; penunjuk asing **`404`** pada ketiganya |
| `AC-193` | ✅ **Tiga sisi sekaligus.** `false`: keluar dari `/options` (1→0), **tetap** pada `GET /`, ringkasan `15→14`. Dikembalikan `true`: **pulih persis**. Penunjuk asing `404` |
| `AC-193` terbalik | ✅ Nol endpoint status pada grup pemetaan |
| `AC-194` | ⚠ **Separuh.** `10 / 4 / 6` **cocok persis** dengan *"empat dari sepuluh sudah dipetakan, enam sisanya belum"* pada roadmap frontend. Penurunannya nol diuji: grup pemetaan **nol punya `DELETE`**, sehingga satu pemetaan uji nol dapat dibatalkan pada database bersama |

Ketiga `summary` cocok dengan angka seeder `BE-LAB-50`: parameter `15/15/0/15`, golongan `4/4/0/4`.

### 6ai.3 Alur pendaftaran lab lewat kiosk — BERJALAN

Diuji dengan akun kiosk `Kiosk Lobby Utama`:

| Langkah | Hasil |
|---|---|
| Validasi perangkat | ✅ `200`, `isLoginEnabled: true` |
| Sesi kiosk lewat `Auth/login` | ✅ `isKioskAccount: true`, geofence **dilewati** sesuai rancangan |
| `POST kiosk/scan-result` dengan `targetService: 2` (Laboratorium) dan `hasPhysicianRequest: true` | ✅ `200`, sesi **`KSC-RSMMC-00018`** terbentuk |
| Panel sesi kiosk membaca balik | ✅ `targetService: 2` dan `hasPhysicianRequest: true` **terbaca utuh** pada `/options` |

**Satu hal yang sempat tampak seperti cacat, dan ternyata bukan.** Kedua ruas itu **nol muncul**
pada jawaban detail maupun daftar — ia hanya ada pada `KioskScanSessionOptionResponse`. Itu
**endpoint yang memang dipakai** panel sesi kiosk `FE-LAB-14`, sehingga pembacanya ada. Bukan
pola `BE-EXT-04`.

**Akun kiosk ditolak `403` pada seluruh endpoint Laboratorium**, dan itu **benar**: kiosk hanya
menghasilkan sesi scan; pemilihan pemeriksaan dan pembuatan pesanan milik petugas pendaftaran.

> **Satu baris uji tertinggal dan didaftar terbuka:** sesi `KSC-RSMMC-00018`, identitas
> `ZZTEST9999999999`, `isPatientFound: false` — sengaja memakai identitas yang nol cocok supaya
> **nol pasien sungguhan tersentuh**.

### 6ai.4 Sapuan superadmin — 331 route, satu `500`

| Jawaban | Jumlah |
|---|---:|
| `200` | **256** |
| `404` | 58 — controller nol punya `GET /` |
| `405` | 10 — jalur `POST` saja |
| `400` | 6 — **keenamnya benar** |
| **`500`** | **1** |

Keenam `400` diperiksa satu per satu: dua menuntut parameter wajib (`EncounterId`,
`PrescriptionCompoundId`), empat menolak sebab superadmin **nol punya workforce profile**.
Keempatnya jawaban yang tepat, bukan cacat.

**Modul Laboratorium: 23 dari 23 jalur `200`. Nol galat.**

### 6ai.5 Satu `500` ditemukan, dan ia DI LUAR modul ini

`GET api/v1/corporate/finance-management/master-data/bank-accounts` → **`500`**.

Akarnya `System.InvalidOperationException` dari EF Core:
`BankAccountService` memproyeksikan hasil join ke `ValueTuple<MstBankAccount, MstBank>`
(baris 177), lalu mengurutkannya lewat `x.Entity.AccountName` (baris 45-48). EF menerjemahkannya
menjadi `ValueTuple.Item1.AccountName` dan **nol dapat menerjemahkannya ke SQL**.

**Milik Finance Management, bukan Laboratorium.** Dicatat di sini semata karena ditemukan oleh
sapuan ini — **nol disentuh**, sebab memperbaikinya berada di luar cakupan dan wewenang tulis
pekerjaan Laboratorium.

**Satu galat intermiten ikut tercatat:** penjadwal absensi (`Attendance scheduler`) gagal
menyimpan pada percobaan menjalankan yang pertama, dan **nol terulang** pada percobaan kedua.
Disebut apa adanya sebagai intermiten, bukan sebagai cacat yang sudah dipastikan.

---

## 6aj. Gelombang `MVP-8` — `EPIC-LAB-14` perluasan hasil Patologi Klinik dan perbaikan `S4b`, 2026-09-24

Menurunkan [`02-backend-architecture.md`](../02-backend-architecture.md) **bagian 19** dan
[`04-prd-to-mvp.md`](../04-prd-to-mvp.md) **bagian 20**.

| Field | Nilai |
|---|---|
| Kontrak | `LAB-API-v1` **`r33`**, `LAB-VAL-v1` **`r11`**, `LAB-PERM-v1` **rev 10**, `LAB-STATE-v1` **`r4`** — keempatnya **`approved` 2026-09-24**, termasuk pencabutan tiga route `/result/microbiology/*` |
| Approval | Yoga Aji Pratama (`yogaaji452@gmail.com`), pemilik modul, 2026-09-24 |
| Masukan | decisions **rev 72**; capability map **rev 5**; `02-backend-architecture.md` rev 9; `04-prd-to-mvp.md` rev 7 |
| Backend SHA | `ddeb5ed8` (branch `yoga`) |
| Frontend SHA | `72607a087` (branch `YogaV2`) |
| Hash masukan (sha256, LF) | Delapan nilai penuh pada [`traceability.md`](traceability.md) bagian *Traceability gelombang `MVP-8`* — tidak diulang di sini supaya hanya ada satu salinan |
| Gelombang | `MVP-8a` perbaikan yang sudah berdiri — **backend dan frontend dirilis bersama**; `MVP-8b` backend perluasan Patologi Klinik |
| Nomor task | `BE-LAB-67`..`BE-LAB-69` |

**Untuk setiap task backend di bawah:** pemeriksaan awal QBE dan kesesuaian rekayasa diselesaikan
**pada waktu eksekusi**, dari `AGENTS.md` backend dan dokumen engineering canonical
(`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`). Roadmap ini tidak menggantikannya.

**Nol migration pada seluruh gelombang ini.** Bila sebuah task ternyata membutuhkan migration,
itu tanda cakupannya melenceng dari bagian 19 — berhenti dan laporkan.

### 6aj.1 `BE-LAB-67` — Izin hasil tersendiri dan route netral disiplin

| Butir | Isi |
|---|---|
| **Status** | `SIAP DIKERJAKAN` |
| **Gelombang** | `MVP-8a` |
| **Outcome** | Hanya pemegang izin hasil yang dapat menulis hasil; dokter pemesan tetap dapat menandai cito tetapi ditolak saat menulis hasil. Final, Reopen, dan konsultasi berjalan di route netral untuk Patologi Klinik dan Mikrobiologi |
| **Requirement/decision** | `FR-14.1`, `FR-14.2`, `FR-14.5`, `FR-14.6`; `LAB-DEC-146`, `LAB-DEC-135`, `LAB-DEC-141` |
| **Kontrak** | `r33` bagian 28.2 dan 28.4; `LAB-PERM-v1` rev 10 bagian 12.2-12.3; `LAB-VAL-v1` `r11` `VAL-122`; `LAB-STATE-v1` `r4` bagian 6.3 |
| **Reuse** | `FinalizeMicrobiologyResultAsync`, `ReopenMicrobiologyResultAsync`, `RecordConsultationAsync` — logikanya dipakai apa adanya, hanya diganti nama (capability map rev 5 `CAP-P14-03`, `CAP-P14-08`); mekanisme resource turunan registri (`PermissionRegistryDescriptor.cs:496-504`) |
| **Cakupan** | `LabExaminationController`: lima tindakan hasil memakai `[AccessPermission("LabExaminationResult", "Update")]` berpasangan `[AccessAction]`; tiga route `/result/microbiology/finalize\|reopen\|consultation` diganti `/result/finalize\|reopen\|consultation`. `LabExaminationService`: tiga method diganti nama menjadi netral, ditambah penjaga disiplin `VAL-122`; nama aksi riwayat Reopen menjadi `LabExamination.ReopenResult` untuk baris **baru** saja |
| **Dependency** | Nol |
| **Acceptance criteria** | `AC-221`, `AC-222`, `AC-224`; `VAL-122` — Final atas baris pemeriksaan Patologi Anatomi menjawab `422` dengan pesan yang mengarah ke laporan PA |
| **Verifikasi** | Startup Development **lolos** `PermissionRegistryValidator`; Swagger **nol memuat** `/result/microbiology/finalize`, `/reopen`, `/consultation`; panggilan sungguhan dengan dua akun — satu hanya `LabExamination : Update`, satu hanya `LabExaminationResult : Update` — membuktikan pembagiannya pada kelima tindakan dan tiga tindakan lama. Kode status dibuktikan terhadap aplikasi yang berjalan, bukan dari pembacaan kode |
| **Risiko/pemilik** | **Tinggi pada rilisnya, rendah pada kodenya.** Sesudah task ini dirilis, **tidak ada satu pun analis** yang dapat menulis hasil sampai admin memberi `LabExaminationResult : Update` (6aj.5). Frontend `FE-LAB-35` **wajib dirilis bersama** — tanpa itu halaman Mikrobiologi memanggil route yang sudah dicabut. Pemilik: implementer backend; langkah rilis milik admin sistem |
| **DoD** | Kelima tindakan beralih izin; ketiga tindakan lama tidak berubah; route lama **tidak ada lagi**; `VAL-122` ditegakkan; **nol seeder atau migration kebijakan**; laporan task pada `task/report/backend/BE-LAB-67.md` |

### 6aj.2 `BE-LAB-68` — Penjaga hasil Final

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-67` |
| **Gelombang** | `MVP-8a` |
| **Outcome** | Hasil yang sudah Final — Patologi Klinik maupun Mikrobiologi — tidak dapat berubah tanpa Reopen |
| **Requirement/decision** | `FR-14.3`, `FR-14.4`; `LAB-DEC-147` |
| **Kontrak** | `LAB-VAL-v1` `r11` `VAL-120`, `VAL-121`; `LAB-STATE-v1` `r4` bagian 6.3 |
| **Reuse** | Pola penjaga `VAL-109` pada `LabSpecimenCorrectionService.cs:63-67` dan `VAL-96` milik laporan Patologi Anatomi |
| **Cakupan** | `LabExaminationService.SetResultAsync` dan `LabMicrobiologyResultService.SetResultAsync` menolak `409` bila `FinalizedAt` terisi — pada Mikrobiologi **sebelum** satu pun baris isolat disentuh; `RecordConsultationAsync` menolak `409` bila Final. **Ditambah 2026-09-25 — temuan `02-backend-architecture.md` 20.1/20.12:** keempat penulisan — simpan hasil Patologi Klinik, simpan hasil Mikrobiologi, Final, dan konsultasi — **menaikkan `LabExamination.Version`**, dan `DbUpdateConcurrencyException` diterjemahkan menjadi `409` *"Hasil ini baru saja diubah orang lain. Muat ulang lalu ulangi."* Tanpanya token `Version` tidak pernah bentrok, dan janji `r33` 28.2 — `409` bila *baris baru saja diubah orang lain* — belum benar pada kode. Reopen **tidak** di sini; ia diperbaiki `BE-LAB-73` bersama penjaga `VAL-136` |
| **Dependency** | `BE-LAB-67` — keduanya menyentuh `LabExaminationService`, dan method yang diganti nama harus berdiri lebih dulu |
| **Acceptance criteria** | `AC-225` termasuk jalur setengah jalan, `AC-226`, `AC-227` |
| **Verifikasi** | Hasil Mikrobiologi Final dengan satu isolat dan dua belas baris antibiogram disimpan ulang dengan isolat pengganti → `409` dan basis data **tetap** satu isolat dan dua belas baris; Reopen beralasan → simpan diterima, `ReopenCount` naik satu, `FinalizedAt` baru sesudah Final ulang. **Ditambah 2026-09-25:** dua permintaan Final bersamaan atas hasil yang sama → satu `200`, satu `409`; `Version` naik tepat satu per penulisan yang berhasil |
| **Risiko/pemilik** | **Sedang.** Jebakannya letak penjaga: bila diletakkan sesudah penghapusan isolat lama, permintaan yang ditolak tetap meninggalkan kerusakan. Pemilik: implementer backend |
| **DoD** | `VAL-120` dan `VAL-121` ditegakkan pada ketiga jalur; jalur setengah jalan terbukti **nol** mengubah data; **keempat penulisan menaikkan `Version` dan bentrokannya `409`** (ditambah 2026-09-25); laporan `BE-LAB-68.md` |

### 6aj.3 `BE-LAB-69` — Jalur baca per order dan penanda rujukan

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-68` |
| **Gelombang** | `MVP-8b` |
| **Outcome** | Halaman Patologi Klinik per order memuat seluruh pemeriksaannya dalam satu panggilan, lengkap dengan penanda `L`/`H`, keadaan Final, dan konsultasi |
| **Requirement/decision** | `FR-14.7`, `FR-14.8`, `FR-14.5` bagi Patologi Klinik; `LAB-DEC-149`, `LAB-FE-015`, `LAB-DEC-135`, `LAB-DEC-141` |
| **Kontrak** | `r33` bagian 28.2-28.3; `LAB-VAL-v1` `r11` `VAL-123` |
| **Reuse** | `GetResultFormAsync` dan `ResolveOutOfNormalRange` sebagai pola; `ResultValueBoundId` sebagai snapshot batas nilai; kolom Final dan konsultasi yang sudah berdiri pada `LabExamination` |
| **Cakupan** | Enum `LabReferenceFlag` (`Normal`, `Low`, `High`, `OutOfReference`) pada `LaboratoryEnums.cs`; `ResolveReferenceFlag` dari batas **snapshot**; `GET /by-order/{labOrderId}/results` beserta `GetResultSheetByOrderAsync` — **satu** kueri `AsNoTracking`, batas nilai dimuat berkelompok; sembilan ruas tambahan pada `LabExaminationResultFormResponse`; `referenceFlag` pada `LabExaminationResultResponse`; `VAL-123` |
| **Dependency** | `BE-LAB-68` |
| **Acceptance criteria** | `AC-234` bagian backend — 19 pemeriksaan, satu dibatalkan, jawaban **18** baris; `VAL-123` pada order Mikrobiologi; `AC-219` bagian backend — `High`, `Low`, `OutOfReference` pada ketiga contoh; `AC-196`, `AC-197`, `AC-214`, `AC-236` pada pemeriksaan **Patologi Klinik** lewat route netral `BE-LAB-67` |
| **Verifikasi** | Log kueri membuktikan **satu** kueri untuk seluruh baris, bukan satu per baris; mengubah batas normal sesudah hasil disimpan **tidak** mengubah `referenceFlag` hasil lama; nol nilai `Critical` muncul di jawaban mana pun |
| **Risiko/pemilik** | **Sedang.** Dua jebakan: menghitung penanda dari batas yang berlaku hari ini, bukan snapshot; dan memuat batas per baris sehingga halaman 19 baris menjadi 19 kueri. Pemilik: implementer backend |
| **DoD** | Jalur baru berjalan; ruas tambahan terisi; `IsOutOfNormalRange` **tetap** dikirim; nol `Critical`; nol migration; laporan `BE-LAB-69.md` |

### 6aj.4 Yang sengaja tidak menjadi task backend

| Yang tidak dijadikan task | Alasan |
|---|---|
| `FR-14.9` teks penanda hasil pilihan | `OPEN DECISION` — backend sudah mengirim `OutOfReference`; yang belum disetujui hanya teks tampilnya |
| Validasi, rilis, antrean validasi, *Kembalikan ke analis*, pembaca kredensial Human Resource | `S4` belum lolos gerbang requirement dan arsitektur domain — lihat `02-requirement-completeness-assessment.md`. **Sejak 2026-09-25 menjadi gelombang `MVP-9`, bagian 6ak** |
| Penanda `KRITIS` Patologi Klinik | `S5` |

### 6aj.5 Langkah rilis `MVP-8a` — bukan task programmer

Mengikuti `02-backend-architecture.md` 19.7. **Wajib dijalankan di jendela rilis yang sama**
dengan `BE-LAB-67`, `BE-LAB-68`, dan `FE-LAB-35`.

| Langkah | Pemilik | Bukti |
|---:|---|---|
| 1 | Admin sistem membaca jabatan yang kini memegang `LabExamination : Update` dan menandai mana yang **analis** — menutup `UNK-P14-01` | Daftar jabatan tertulis sebelum rilis |
| 2 | Deploy backend dan frontend bersama | Versi keduanya tercatat |
| 3 | Admin memberi `LabExaminationResult : Update` kepada **jabatan analis saja** lewat layar Akses Role | Layar Akses Role; **dokter pemesan tidak ada di daftar** |
| 4 | Satu analis mengisi satu hasil uji; satu dokter pemesan menandai cito lalu dicoba menulis hasil | `AC-223` dan `AC-221` terbukti |

**Larangan:** menyalin kebijakan `LabExamination : Update` ke `LabExaminationResult : Update`
secara otomatis — itu membuka kembali `LAB-CONFLICT-012`.

## 6ak. Gelombang `MVP-9` — `EPIC-LAB-15` validasi dan rilis hasil Patologi Klinik, 2026-09-25

Menurunkan [`02-backend-architecture.md`](../02-backend-architecture.md) **bagian 20** dan
[`04-prd-to-mvp.md`](../04-prd-to-mvp.md) **bagian 21**.

| Field | Nilai |
|---|---|
| Kontrak | `LAB-API-v1` **`r34`**, `LAB-VAL-v1` **`r12`**, `LAB-PERM-v1` **rev 11**, `LAB-STATE-v1` **`r5`**, `LAB-INT-v1` **`r4`** — kelimanya **`approved` 2026-09-25**, beserta kesepuluh butir `02-backend-architecture.md` 20.10 |
| Approval | Yoga Aji Pratama (`yogaaji452@gmail.com`), pemilik modul, 2026-09-25 — *"Setujui kelima kontrak beserta 10 butir itu dan lanjut ke /quilvian-engineering-skills:plan-module-delivery"* |
| Masukan | decisions **rev 74**; capability map **rev 5**; `LAB-DA-001` **rev 8** bagian A5; `02-backend-architecture.md` **rev 10**; `04-prd-to-mvp.md` **rev 8** |
| Kesiapan arsitektur domain | `DOMAIN_ARCHITECTURE_READY` untuk desain — **pemakaian nyata tertahan** (6ak.10) |
| Backend SHA | `ddeb5ed8` (branch `yoga`) — diperiksa ulang 2026-09-25, tidak bergeser |
| Frontend SHA | `72607a087` (branch `YogaV2`) |
| Hash masukan (sha256, LF) | Dua belas nilai penuh pada [`traceability.md`](traceability.md) bagian *Traceability gelombang `MVP-9`* |
| Gelombang | `MVP-9a` fondasi — `BE-LAB-70`..`72`; `MVP-9b` tindakan — `BE-LAB-73`..`77`; `MVP-9d` langkah rilis — **`BLOCKED`** (6ak.10). `MVP-9c` milik frontend |
| Prasyarat gelombang | **`MVP-8` selesai** (`04-prd-to-mvp.md` 21.6): resource `LabExaminationResult`, route netral Final/Reopen, dan jalur baca per order dibangun `BE-LAB-67`..`69` |

**Untuk setiap task backend di bawah:** pemeriksaan awal QBE dan kesesuaian rekayasa diselesaikan
**pada waktu eksekusi**, dari `AGENTS.md` backend dan dokumen engineering canonical
(`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`). Roadmap ini tidak menggantikannya.

**Satu migration pada seluruh gelombang, milik `BE-LAB-70`.** Task lain yang ternyata membutuhkan
migration berarti cakupannya melenceng dari bagian 20 — berhenti dan laporkan.

**Build lokal** memakai `-p:RunAnalyzers=False`; build penuh dengan analyzer melewati batas waktu.

**Kenapa `MVP-9a`..`MVP-9b` boleh dikerjakan sebelum penahan pemakaian terjawab.** Jawaban
`DEC-LAB-011` sisa, `DEC-LAB-017`, `DEC-LAB-018`, dan `LAB-COORD-016` menentukan **siapa** dan
**kapan**, bukan **bentuk** (gerbang 0C.5). Resolver fail-closed membuat kode yang sudah dideploy
**tidak membuka** pemakaian sedikit pun sampai kebijakan dan penunjukan diberikan pada `MVP-9d`.

### 6ak.1 `BE-LAB-70` — Skema validasi dan rilis

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `MVP-8` selesai (`BE-LAB-69`). Secara teknis tidak menyentuh berkas yang diubah `MVP-8`; urutannya dijaga `04-prd-to-mvp.md` 21.6 supaya satu jendela rilis membawa satu perubahan besar |
| **Gelombang** | `MVP-9a` |
| **Outcome** | Seluruh tempat menyimpan fakta validasi dan rilis sudah berdiri, **tanpa satu pun perilaku berubah** — nol endpoint, nol aturan |
| **Requirement/decision** | Fondasi `FR-15.1`..`FR-15.13`; `LAB-DEC-080`, `LAB-DEC-150` butir 4, `LAB-DEC-017` |
| **Kontrak** | `02-backend-architecture.md` 20.4-20.7; `erd/data-dictionary.md` bagian 17 |
| **Reuse** | Pola model dan configuration `LabOrganism` — index unik **parsial** `IsDelete = false`; pola kolom pengguna tanpa FK `ResultEnteredByUserId` |
| **Cakupan** | (1) Model `LabResultCorrectionReason` dan `LabFourEyesExceptionReason` beserta configuration-nya. (2) **14 kolom nullable** pada `LabExamination`, dua FK `Restrict` ke `LabFourEyesExceptionReason`, index `(FinalizedAt, ValidatedAt)` dan `(ReleasedAt)` pada `LabExaminationConfiguration.cs`. (3) Dua `DbSet` pada `ApplicationDbContext.cs`. (4) Lima enum baru pada `LaboratoryEnums.cs` (`LabPrivilegeKind`, `LabPrivilegeDenial`, `LabResultStatus`, `LabOrderResultProgress`, `LabValidationQueueStage`). (5) **`ClinicalDocumentKind.LaboratoryResult = 14`** — enum milik Rekam Medis, disepakati `LAB-COORD-002`; **hanya** menambah nilai. (6) Migration `AddLabResultValidationAndRelease`. (7) Komentar `LabExamination.cs:107-120` dan `:188-201` yang menyatakan *"nol ValidatedAt maupun ReleasedAt"* diperbarui |
| **Dependency** | `MVP-8` selesai |
| **Acceptance criteria** | Bentuk skema sama dengan DDL kamus data 17.6; seluruh baris `LabExamination` yang sudah ada bernilai **kosong** pada ke-14 kolom — nol pengisian data lama; `LabExaminationStatus` **tetap empat nilai** (`LAB-DEC-080`) |
| **Verifikasi** | Skrip SQL migration dibaca sebelum dijalankan: kolom nullable tanpa nilai bawaan, kedua index unik memuat `WHERE "IsDelete" = false`, kedua FK `ON DELETE RESTRICT`. Migration diterapkan ke basis data pengembangan, lalu `Down` diuji **sebelum** ada data validasi. Aplikasi start tanpa galat |
| **Risiko/pemilik** | **Sedang.** Dua jebakan: index unik **tanpa** pembatas `IsDelete` — pelajaran `LAB-CONFLICT-005` yang sudah dua kali dibayar modul ini; dan menyentuh `JenisYangDitegakkan` milik Rekam Medis saat menambah nilai enum — **dilarang** (`02-backend-architecture.md` 20.9). Pemilik: implementer backend |
| **DoD** | Migration berjalan maju dan mundur; nol endpoint baru; nol perubahan perilaku; laporan `task/report/backend/BE-LAB-70.md` |

### 6ak.2 `BE-LAB-71` — Dua data induk alasan

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-70` |
| **Gelombang** | `MVP-9a` |
| **Outcome** | Kepala instalasi dapat mengisi dan mengelola daftar alasan pengembalian dan daftar alasan pengecualian; admin sistem sendirian yang menyetel *wajib catatan* |
| **Requirement/decision** | `FR-15.12`; `LAB-DEC-082`, `LAB-DEC-138`, `LAB-DEC-003`, `LAB-DEC-019`; 20.10 butir 4 |
| **Kontrak** | `r34` bagian 29.6; `LAB-VAL-v1` `r12` `VAL-140`..`VAL-142`; `LAB-PERM-v1` rev 11 bagian 13.2-13.3 |
| **Reuse** | Permukaan baseline `LabOrganismController` (`r31`) dan `LabOrganismService` di `LabMicrobiologyMasterDataService.cs`; aksi `SystemFlag` dan `PUT /{id}/system-flags` dari `LabRejectionReasonController` |
| **Cakupan** | `LabResultCorrectionReasonController.cs` dan `LabFourEyesExceptionReasonController.cs` — **sembilan endpoint masing-masing**, nol `DELETE`, setiap endpoint dengan `[AccessAction]` dan `[AccessPermission]` berpasangan; dua service di `LabResultReasonMasterDataService.cs`; satu set DTO di `LabResultReasonMasterDataDtos.cs` — **`UpdateRequest` tanpa `IsActive`**, `CreateRequest` tanpa `requiresNote`; `GET /options` hanya alasan aktif; registrasi DI di `Program.cs`. **Nol seeder** |
| **Dependency** | `BE-LAB-70` |
| **Acceptance criteria** | `VAL-140` kode ganda `409`; `VAL-141` kode berhuruf kecil `422`; `VAL-142` mengubah kode `422`; `requiresNote` pada `POST` diabaikan; `system-flags` oleh pengguna tanpa `SystemFlag` → `403`; alasan nonaktif **tidak** muncul pada `/options` |
| **Verifikasi** | Startup Development lolos `PermissionRegistryValidator`; Swagger memuat **18** endpoint di bawah dua tag `r34` 29.6 dan **nol** `DELETE`; panggilan sungguhan dengan akun kepala instalasi dan akun admin |
| **Risiko/pemilik** | **Rendah-sedang.** Jebakannya menyalin `IsActive` pada `UpdateRequest` dari pola Organisme — jebakan yang ditemukan `FE-LAB-24` dan sengaja ditutup desain. Pemilik: implementer backend |
| **DoD** | 18 endpoint berjalan; `VAL-140`..`VAL-142` ditegakkan; nol hapus; nol seeder; laporan `BE-LAB-71.md` |

### 6ak.3 `BE-LAB-72` — Pembaca kewenangan dari kredensial Human Resource

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-70` (enum). **Boleh sejajar** dengan `BE-LAB-71` — berkasnya tidak bersinggungan |
| **Gelombang** | `MVP-9a` |
| **Outcome** | Satu tempat yang menjawab *"apakah orang ini ditunjuk memvalidasi atau merilis hasil Patologi Klinik hari ini, dan bila tidak, kenapa"* — **selalu menolak** bila datanya kosong atau tidak terbaca |
| **Requirement/decision** | `FR-15.4`, `FR-15.5`; `LAB-DEC-142`, `LAB-DEC-143`, `LAB-DEC-148`, `LAB-DEC-150`; 20.10 butir 7-8 |
| **Kontrak** | `LAB-INT-v1` `r4` `INT-07`; `LAB-VAL-v1` `r12` `VAL-128` dan 14.2; `02-backend-architecture.md` 20.4 `LabClinicalPrivilegeResolver` |
| **Reuse** | **Pola membaca** `OperatingRoomCredentialResolver.cs:15-42` — `WfpClinicalPrivilege` `AsNoTracking`, status, dan masa berlaku. **Pola memutuskannya tidak ditiru**: resolver itu tidak memfilter kode dan tidak memblokir data kosong. Syarat kelayakan penempatan dari `AccessPermissionService.cs:144-176` |
| **Cakupan** | `LabClinicalPrivilegeResolver.cs` — `ResolveAsync(userId, discipline, kind, at)` dan `ResolveActingPositionAsync(userId, action)`; `Constants/LabClinicalPrivilegeCodes.cs` berisi **usulan** `LAB-VAL-PK` dan `LAB-REL-PK`. Aturan: cocok kode, `IsDelete = false`, `IsActive = true`; hanya baris yang masa berlakunya mencakup **hari tindakan menurut zona waktu rumah sakit, tanggal akhir inklusif**; satu baris `Suspended`, `Revoked`, atau `IsClinicalServiceBlocked` → tolak; satu `Active` → terima beserta `PrivilegeId`; selebihnya satu dari delapan `LabPrivilegeDenial`. Galat pembacaan dilempar sebagai pengecualian tersendiri yang **dipetakan `503` oleh pemanggil**. Registrasi DI |
| **Dependency** | `BE-LAB-70` |
| **Acceptance criteria** | `AC-229`, `AC-230`, `AC-233` pada tingkat resolver; `AC-232`; `VAL-128` tanggal inklusif, pembacaan gagal, dan kode belum ada di katalog; `AC-239` dua penempatan — **`testing/acceptance-test-matrix.md` amandemen 2026-09-25** |
| **Verifikasi** | Uji per keadaan dengan data penunjukan dari tabel *Data uji tambahan* pada matriks uji: delapan sebab penolakan menghasilkan delapan sebab berbeda; penunjukan berakhir 30 September diterima pukul 23.50 WIB tanggal 30 dan ditolak pukul 00.05 WIB tanggal 1. **Kode pada test memakai konstanta**, tidak ditulis ulang. Tinjauan kode: nol `Add`/`Update`/`Remove` pada entity Human Resource |
| **Risiko/pemilik** | **Tinggi — inilah penjaga keselamatan utama epic ini.** Tiga jebakan: menyalin keputusan `NotAvailable` Kamar Operasi yang **mengizinkan** data kosong; membandingkan tanggal sebagai jam UTC sehingga penunjukan habis tujuh belas jam terlalu cepat; dan menyimpan salinan atau cache penunjukan. **Nilai konstanta kode belum final** sampai `LAB-COORD-016` — hanya konstanta yang berubah. Pemilik: implementer backend; nilai kode: pemilik `human-resource` |
| **DoD** | Delapan sebab teruji; fail-closed terbukti pada tiga jalur (akun tanpa tenaga kerja, nol baris, pembacaan gagal); nol tulisan ke Human Resource; laporan `BE-LAB-72.md` |

### 6ak.4 `BE-LAB-73` — Validasi hasil dan penjaga Reopen

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-71`, `BE-LAB-72`; dan `BE-LAB-67` (resource `LabExaminationResult`, `ReopenResultAsync` bernama netral) |
| **Gelombang** | `MVP-9b` |
| **Outcome** | Dokter berkewenangan laboratorium yang ditunjuk dapat memvalidasi hasil Patologi Klinik yang Final; analis tidak dapat lagi Reopen sesudah validasi; dua tindakan bersamaan tidak dapat sama-sama berhasil |
| **Requirement/decision** | `FR-15.1`, `FR-15.3` bagian validasi, `FR-15.5`, `FR-15.7`, `FR-15.15`; `LAB-DEC-003`, `LAB-DEC-135`, `LAB-DEC-142`, `LAB-DEC-150` |
| **Kontrak** | `r34` 29.2-29.3; `LAB-VAL-v1` `r12` `VAL-124`..`VAL-130`, `VAL-132`, `VAL-136`; `LAB-PERM-v1` rev 11 bagian 13.2-13.6; `LAB-STATE-v1` `r5` 7.2-7.3 |
| **Reuse** | `BuildCompletionResponse` dan pola Final/Reopen `LabExaminationService`; pola riwayat `LabTransitionHistory` pada Reopen (`LabExaminationService.cs:1076-1088`) — termasuk **wajib `Include(LabOrder)`** supaya `EncounterId` tidak jatuh ke `Guid.Empty` |
| **Cakupan** | `LabResultValidationService.cs` baru dengan `ValidateAsync` — urutan pemeriksaan persis `02-backend-architecture.md` 20.4; tujuh kolom validasi, snapshot jabatan dari `ResolveActingPositionAsync`, `ValidatedByPrivilegeId`, satu baris riwayat `ValidateResult`; `Version` naik; `DbUpdateConcurrencyException` → `409`. Endpoint `POST /{id}/result/validate` dengan `[AccessAction("Validate", ...)]` dan `[AccessPermission("LabExaminationResult", "Validate")]`. `ReopenResultAsync`: `VAL-136`, `Version` naik, bentrokan → `409`. `BuildCompletionResponse`: ruas validasi dan `validationExceptionMarker` **berbunyi persis** 20.10 butir 5. Registrasi DI |
| **Dependency** | `BE-LAB-71`, `BE-LAB-72`, `BE-LAB-67` |
| **Acceptance criteria** | `AC-196`, `AC-197`, `AC-01`, `AC-215`, `AC-216`, `AC-229`, `AC-230`, `AC-233`, `AC-238`, `AC-239`; `VAL-125`, `VAL-126`, `VAL-127`, `VAL-130`, `VAL-132`; **konkurensi Reopen lawan Validasi** |
| **Verifikasi** | Panggilan sungguhan terhadap aplikasi berjalan dengan akun analis, dokter A, dan akun tanpa `WorkforceProfileId`; uji konkurensi berpasangan membuktikan satu `200` dan satu `409`, dan **nol** baris dengan `ValidatedAt` terisi sementara `FinalizedAt` kosong |
| **Risiko/pemilik** | **Tinggi.** Tiga jebakan: memeriksa kewenangan **sebelum** keadaan hasil sehingga pesan menyesatkan; lupa menaikkan `Version` pada Reopen sehingga balapan 20.1 tetap terbuka; dan menyusun bunyi penanda sendiri berbeda dari yang disetujui. Pemilik: implementer backend |
| **DoD** | Validasi berjalan; aturan-aturannya ditegakkan; Reopen terjaga; laporan `BE-LAB-73.md` |

### 6ak.5 `BE-LAB-74` — Rilis, pendaftaran rekam medis, dan penjaga batal

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-73` |
| **Gelombang** | `MVP-9b` |
| **Outcome** | Hasil tervalidasi dapat dirilis oleh orang kedua yang ditunjuk, dan pada detik yang sama menjadi dokumen tertanda tangan dan terkunci di rekam medis pasien — **atau tidak sama sekali** |
| **Requirement/decision** | `FR-15.2`, `FR-15.3` bagian rilis, `FR-15.5` bagian perilis, `FR-15.8`, `FR-15.13`; `LAB-INH-007`, `LAB-DEC-017`, `LAB-DEC-120`; 20.10 butir 1, 3, 6, 10 |
| **Kontrak** | `r34` 29.2, 29.3, 29.7; `LAB-VAL-v1` `r12` `VAL-131`, `VAL-133`, `VAL-137`, `VAL-143`; `LAB-INT-v1` `r4` `INT-08` |
| **Reuse** | `ClinicalDocumentIntegrityService.RegisterSignedAsync` (`ClinicalDocumentIntegrityService.cs:200-234`) **apa adanya**; penanganan gagal-pendaftaran `ConsultationFinalizationService.cs:180-203` milik Farmasi |
| **Cakupan** | `ReleaseAsync`: tujuh kolom rilis, satu baris riwayat `ReleaseResult`, lalu `RegisterSignedAsync(LaboratoryResult, examination.Id, PatientId dari kunjungan order, LabOrder.EncounterId, perilis sebagai penulis dan penanda tangan, perangkat, alamat jaringan, waktu rilis)` — **seluruhnya satu `SaveChangesAsync`**. `InvalidOperationException` pendaftaran → `422` `VAL-137`, nol yang tersimpan; pelanggaran unik → `409`. Endpoint `POST /{id}/result/release` (`Release`). `BuildCompletionResponse`: `IsReleased` dari `ReleasedAt` bagi Patologi Klinik, `DeliveryBlockedReason` kosong bila dirilis, `releaseExceptionMarker`. `CancelAsync`: `VAL-143` |
| **Dependency** | `BE-LAB-73` |
| **Acceptance criteria** | `AC-217` bagian rilis, `AC-198` bagian backend, `AC-231`; `VAL-131`, `VAL-133`, `VAL-143`; ketiga skenario `INT-08` — berhasil, gagal, kunjungan tertutup; konkurensi dua rilis |
| **Verifikasi** | Rilis satu hasil → **tepat satu** baris `MrcClinicalDocumentIntegrity` `LaboratoryResult`, `Signed`, `LockedAt` = waktu rilis, penulis = perilis. Rilis dengan kunjungan tidak sah → basis data **tidak berubah sama sekali**. Rekonsiliasi `INT-08`: jumlah pemeriksaan ber-`ReleasedAt` sama dengan jumlah baris `LaboratoryResult` |
| **Risiko/pemilik** | **Tinggi.** Tiga jebakan: memanggil `SaveChangesAsync` **sebelum** pendaftaran sehingga rilis tersimpan tanpa dokumen; mengambil `PatientId` dari tempat selain kunjungan order; dan menambah `LaboratoryResult` ke `JenisYangDitegakkan` milik Rekam Medis. Pemilik: implementer backend; perilaku Rekam Medis: pemilik `rekam-medis` |
| **DoD** | Rilis atomik terbukti pada jalur gagal; penjaga batal berjalan; laporan `BE-LAB-74.md` |

### 6ak.6 `BE-LAB-75` — *Kembalikan ke analis*

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-74` |
| **Gelombang** | `MVP-9b` |
| **Outcome** | Hasil tervalidasi yang ternyata keliru sebelum dirilis dapat dikembalikan beralasan tanpa menghapus jejak siapa yang pernah memvalidasinya |
| **Requirement/decision** | `FR-15.6`; `LAB-DEC-138` |
| **Kontrak** | `r34` 29.2-29.3; `LAB-VAL-v1` `r12` `VAL-134`, `VAL-135`, `VAL-138`; `LAB-STATE-v1` `r5` 7.2 |
| **Reuse** | `LabResultValidationService` (`BE-LAB-73`); pola `ReasonCode` riwayat penolakan wadah |
| **Cakupan** | `ReturnToAnalystAsync`: kolom validasi dan `FinalizedAt`/`FinalizedByUserId` dikosongkan, `ReopenCount` **tidak** naik, satu baris riwayat `ReturnResultToAnalyst` dengan `ReasonCode` = kode alasan dan `ReasonNote` bila wajib; baris `ValidateResult` **tidak disentuh**; lapis orang: kode validasi **atau** rilis; `Version` naik. Endpoint `POST /{id}/result/return` (`Return`) |
| **Dependency** | `BE-LAB-74` — jalur `409` untuk hasil yang sudah dirilis membutuhkan rilis berdiri |
| **Acceptance criteria** | `AC-205`, `AC-206`, `AC-207`; `VAL-138` |
| **Verifikasi** | Sesudah pengembalian, riwayat memuat baris validasi lama **dan** baris pengembalian; nol pemberitahuan; nol versi bernomor |
| **Risiko/pemilik** | **Sedang.** Jebakannya menghapus atau mengubah baris riwayat validasi supaya riwayat "bersih". Pemilik: implementer backend |
| **DoD** | Pengembalian berjalan; riwayat utuh; laporan `BE-LAB-75.md` |

### 6ak.7 `BE-LAB-76` — Ruas baca: pengesah, keadaan hasil, label order

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-75`; dan `BE-LAB-69` (jalur baca per order) |
| **Gelombang** | `MVP-9b` |
| **Outcome** | Halaman hasil dan daftar Pemeriksaan dapat menampilkan siapa yang mengesahkan, keadaan setiap hasil, dan label order — tanpa layar menyimpulkan apa pun sendiri |
| **Requirement/decision** | `FR-15.10`, `FR-15.14`; `LAB-DEC-008`, `LAB-DEC-080`, `LAB-DEC-120`, `LAB-DEC-135` |
| **Kontrak** | `r34` 29.3 dan 29.5 |
| **Reuse** | `GetResultFormAsync`, `GetResultSheetByOrderAsync` (`BE-LAB-69`); daftar Pemeriksaan `LabMonitoringService`; detail order `LabOrderService` |
| **Cakupan** | Ruas `resultStatus`, validasi, rilis, dan kedua penanda pada `LabExaminationResultFormResponse` dan jalur baca per order, ditambah `resultEnteredByUserId`; nama pelaku dibaca **berkelompok dalam satu kueri** untuk seluruh baris; `resultProgress` pada `GET /lab-monitoring/clinical-pathology` dan `GET /lab-orders/{id}` — **nol kolom tersimpan** |
| **Dependency** | `BE-LAB-75`, `BE-LAB-69` |
| **Acceptance criteria** | `AC-08`, `AC-198`, `AC-199`; bagian baca `AC-02` dan `AC-239` |
| **Verifikasi** | Log kueri: nama pelaku satu kueri untuk 18 baris; order dengan satu pemeriksaan batal dan sisanya dirilis → `AllReleased`; `orderStatus` **tidak berubah** oleh rilis mana pun |
| **Risiko/pemilik** | **Sedang.** Dua jebakan: satu kueri nama per baris; dan menyetel `LabOrderStatus.Completed` saat seluruh pemeriksaan dirilis — itu `LAB-CONFLICT-014` yang **belum diputuskan**. Pemilik: implementer backend |
| **DoD** | Ruas terisi; `resultProgress` benar pada tiga keadaan; `orderStatus` tak tersentuh; laporan `BE-LAB-76.md` |

### 6ak.8 `BE-LAB-77` — Antrean validasi dan Daftar Kerja

| Butir | Isi |
|---|---|
| **Status** | `MENUNGGU PENDAHULU` — `BE-LAB-76` |
| **Gelombang** | `MVP-9b` |
| **Outcome** | Pemvalidasi dan perilis melihat hasil yang menunggu mereka, cito lebih dulu; pemeriksaan yang sudah dirilis tidak lagi tercatat terlambat |
| **Requirement/decision** | `FR-15.9`, `FR-15.11`; `LAB-DEC-135` butir 2; `AC-17` |
| **Kontrak** | `r34` 29.4, 29.8; `LAB-VAL-v1` `r12` `VAL-139` |
| **Reuse** | `LabWorklistService` — `TerapkanPenyaringBersama`, pola `GET /cito-overdue` |
| **Cakupan** | `GET /lab-worklists/validation-queue` dengan `LabValidationQueueQuery` (`stage` wajib) dan `LabValidationQueueItemResponse`; disiplin selalu Patologi Klinik; urutan cito lalu paling lama menunggu; `VAL-139`. `BelumSelesai()` mengeluarkan `ReleasedAt != null` |
| **Dependency** | `BE-LAB-76` — memakai turunan `resultStatus` yang sama |
| **Acceptance criteria** | `AC-196` bagian antrean, `AC-17`; `VAL-139` |
| **Verifikasi** | Kalium cito terlambat lalu dirilis → hilang dari `/cito-overdue` dan `/pending`; hasil Mikrobiologi Final **tidak** muncul di antrean |
| **Risiko/pemilik** | **Rendah.** Jebakannya menurunkan keadaan antrean dengan rumus berbeda dari `resultStatus`. Pemilik: implementer backend |
| **DoD** | Antrean berjalan; Daftar Kerja menyesuaikan; laporan `BE-LAB-77.md` |

### 6ak.9 Yang sengaja tidak menjadi task backend

| Yang tidak dijadikan task | Alasan |
|---|---|
| Validasi dan rilis Mikrobiologi dan Patologi Anatomi | `S4d`, `S4e` — pemegangnya belum ditetapkan (`DEC-LAB-011` sisa) |
| Penanda `KRITIS`, formulir pelaporan | `S5` |
| Koreksi sesudah rilis | `S6` — `DEC-LAB-014`, `DEC-LAB-019` |
| Cetakan *Validasi oleh* dan *Otorisasi oleh* | `S17` |
| Peringatan satu pemegang per shift | Ditunda — `02-backend-architecture.md` 20.9 |
| Mengubah `PUT /lab-orders/{id}/complete` | `LAB-CONFLICT-014` belum diputuskan |
| Endpoint *"kewenangan saya"*, tombol validasi massal | Ditolak desain 20.9 |

### 6ak.10 Langkah rilis `MVP-9d` — **`BLOCKED`**, bukan task programmer

Mengikuti `02-backend-architecture.md` 20.7. **Kode `MVP-9a`/`MVP-9b` boleh sudah dideploy** —
tanpa langkah di bawah, resolver menolak setiap validasi dan rilis.

| Langkah | Pemilik | Menunggu | Bukti |
|---:|---|---|---|
| 1 | Kepala instalasi mengisi kedua daftar alasan lewat layarnya — sekurang-kurangnya satu baris aktif masing-masing, dari usulan `02-backend-architecture.md` 20.8 | Konfirmasi isi awal oleh kepala instalasi | Layar data induk |
| 2 | Pemilik Human Resource menambah dua kode Patologi Klinik ke katalog — **nilainya sama persis** dengan `LabClinicalPrivilegeCodes` | **`LAB-COORD-016`** | Katalog Human Resource |
| 3 | Penunjukan dicatat pada kredensial Human Resource: dr. Bima Prasetya, Sp.PK untuk validasi; **sekurang-kurangnya satu pemegang validasi Patologi Klinik lain** (`LAB-DEC-152`); pemegang rilis — dokter **atau** pejabat non-dokter (`LAB-DEC-153`) | ~~`DEC-LAB-018`, `DEC-LAB-011` sisa~~ — ✅ tertutup 2026-09-25. Kini menunggu **penetapan nama** oleh dr. Bima dan **`LAB-OPEN-044`** untuk perilis | Layar kredensial Human Resource; **dua** pemegang validasi PK tercatat |
| 4 | Admin membaca jabatan dokter berkewenangan laboratorium dan jabatan calon perilis, lalu memberi `Validate`, `Release`, `Return`, `LabWorklist : Read`, dan hak baca kedua daftar alasan — **hanya** kepada jabatan itu | **`UNK-P14-03`**; **`LAB-OPEN-044`** — jabatan calon perilis dari *aturan laboratorium*; **`DEC-LAB-017`** | Layar Akses Role; **nol analis** di daftar `Validate` |
| 5 | Satu hasil uji divalidasi, dirilis, dan diperiksa baris rekam medisnya | Langkah 1-4 | Laporan rilis |

**Wajib terjawab sebelum langkah 4:** `LAB-CONFLICT-014` — supaya layar tidak menampilkan dua
*Selesai* berbeda arti; dan — **diusulkan** — pertanyaan klinis *bolehkah `S4` dipakai sebelum
koreksi `S6` berdiri* (`04-prd-to-mvp.md` 21.7), yang belum ber-Decision ID.

**Larangan:** kebijakan `Validate`, `Release`, `Return` **tidak boleh disalin** dari pemegang
`LabExaminationResult : Update` — itu memberi analis kewenangan validasi tanpa satu pun galat.
