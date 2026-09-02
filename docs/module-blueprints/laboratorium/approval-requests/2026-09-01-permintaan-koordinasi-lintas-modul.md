# Permintaan Koordinasi Lintas Modul — Modul Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-001` |
| `tanggal` | 2026-09-01, diperbarui 2026-09-02 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium |
| `rujukan` | `blueprint-manifest.md` revision `19`; `04-prd-to-mvp.md` bagian 15 |
| `status` | `dijawab sebagian` — 6 selesai, 5 terbuka. Lihat bagian 0 dan 5 |
| `disetujui oleh` | `andryzainhome` (`andryzain01@gmail.com`) dan `sukmagp` — Sukma Giri Pratama (`sukmagiri11@gmail.com`), selaku pemilik repository |
| `tanggal persetujuan` | 2026-09-01 |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

Dokumen ini dapat diteruskan apa adanya. Setiap bagian berdiri sendiri: penerima cukup membaca
bagian yang menyebut modulnya.

---

## 0. Hasil — Apa yang Sudah Terjawab dan Apa yang Belum

Persetujuan diberikan `andryzainhome` dan `sukmagp` pada 2026-09-01, disampaikan lewat pemilik
modul Laboratorium.

### 0.1 Disetujui — lima butir

| No | Yang diminta | Akibat |
|---:|---|---|
| 1 | Kolom disiplin pada `MstProcedure` | `LAB-COORD-005` **ditutup**. `MVP-0` tidak lagi terhalang |
| 2 | Dua data induk perujuk | `LAB-COORD-004` bagian data induk **ditutup** |
| 3 | Kolom penunjuk perujuk pada kunjungan + kontrak pemanggilan Registrasi | `LAB-COORD-004` bagian kunjungan dan `LAB-COORD-003` **ditutup**. `MVP-1` tidak lagi terhalang |
| 7 | Pemberitahuan sebagai kemampuan platform | `LAB-COORD-001` **ditutup** |
| 8 | Satu jenis dokumen klinis baru pada `rekam-medis` | `LAB-COORD-002` **ditutup** |

### 0.2 Belum terjawab — dua butir yang membutuhkan **jawaban**, bukan persetujuan

| No | Yang dibutuhkan | Kenapa persetujuan belum cukup |
|---:|---|---|
| 4 | Jumlah baris `TrxLabSpecimen` di basis data produksi | Yang diminta adalah **satu angka**, bukan izin. Menyetujui permintaan tidak memberi tahu berapa barisnya. Migration `MVP-2` tetap tidak boleh dijalankan sebelum angkanya diketahui |
| 5 | ~~Lokasi `BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`~~ | **TERJAWAB 2026-09-01** — lihat 3.2. Keduanya masih berlaku. `LAB-OPEN-002` ditutup oleh `LAB-FACT-007` |
| 11 | Checker `Invoke-QbeConformanceCheck.ps1` gagal dijalankan | **Baru 2026-09-02.** Scriptnya mencari dokumen tata kelola pada path yang sudah tidak ada. Yang diminta perbaikan satu baris path, bukan izin. Lihat 3.5 |

> **Peringatan lokasi — sudah diperbaiki 2026-09-02.** Versi terdahulu dokumen ini menyatakan
> lokasi canonical adalah `docs/engineering/` di repository backend. **Itu keliru**, dan
> bertentangan dengan bagian 3.4 pada dokumen yang sama.
>
> Yang berlaku menurut `AGENTS.md` baris 13 adalah: lapisan operasional tata kelola **tidak
> lagi tinggal di repository backend**. Sumbernya adalah repository `QuilvianEngineeringSkills`
> yang terpasang sebagai suite Skill, dibaca lewat jalur logis `rules/backend/engineering/`.
>
> Ketiga salinan yang ada hari ini berisi **teks yang identik**, jadi belum ada perbedaan isi
> yang merugikan. Yang perlu diputuskan pemilik repository adalah salinan `docs/engineering/`
> di backend: dipertahankan sebagai cermin baca-saja, atau dihapus. Selama dibiarkan tanpa
> keterangan, ia akan menyimpang diam-diam dari sumbernya.

`LAB-OPEN-012` tetap terbuka. `LAB-OPEN-002` ditutup, tetapi pembacaan dokumennya menurunkan
dua penghambat baru — `LAB-OPEN-018` dan `LAB-OPEN-019` — yang keduanya memerlukan tindakan
pemilik registry prefix. Lihat 3.3 dan 3.4. Pemeriksaan lanjutan 2026-09-02 menurunkan satu
penghambat lagi, `LAB-OPEN-020`, yang merupakan wewenang Andry Zain selaku pemilik repository
backend. Lihat 3.5.

### 0.3 Di luar wewenang pemberi persetujuan — satu butir

| No | Yang diminta | Kenapa tidak dapat ditutup |
|---:|---|---|
| 6 | Tanda tangan klinis atas `LAB-DEC-003`, `LAB-DEC-004`, `LAB-DEC-007` | `LAB-DEC-011` — yang **disetujui pemilik modul Laboratorium sendiri** — menyatakan bahwa wewenang klinis berada di pihak lain, dan ketiganya memerlukan tanda tangan **dokter penanggung jawab laboratorium atau Komite Medis** sebelum desain final |

`andryzainhome` dan `sukmagp` adalah pemilik repository, bukan wewenang klinis. Menutup
`LAB-SIGN-001` atas persetujuan mereka justru melanggar keputusan yang dibuat pemilik modul
Laboratorium sendiri.

**Yang membuat ini penting, bukan sekadar formalitas.** Ketiga keputusan itu menentukan siapa
yang boleh menyatakan sebuah angka hasil benar, apa yang terjadi ketika pasien dalam bahaya,
dan apa yang terjadi ketika hasil yang sudah dipakai ternyata salah. Bila kelak terjadi
insiden, rumah sakit perlu menunjukkan bahwa pihak klinis ikut memutuskan.

`LAB-SIGN-001` tetap terbuka. Seluruh slice hasil pemeriksaan tetap tertahan.

> **Bila keliru:** apabila salah satu dari `andryzainhome` atau `sukmagp` memang memegang
> wewenang klinis — misalnya merangkap dokter penanggung jawab laboratorium — cukup nyatakan
> hal itu, dan `LAB-SIGN-001` akan ditutup dengan nama yang bersangkutan sebagai penanda tangan
> klinis.

---

## 1. Satu paragraf untuk yang tidak punya waktu

Modul Laboratorium sudah punya blueprint lengkap — 36 keputusan disetujui, arsitektur domain
siap, dan seluruh kontrak tersusun. **Implementasi tetap tidak bisa dimulai** karena sebelas
hal berada di luar wewenang modul Laboratorium.

Enam sudah selesai pada 2026-09-01. **Lima masih terbuka**, dan yang paling berat bukan lagi
izin menyentuh tabel modul lain, melainkan **status registry modul Laboratorium sendiri yang
masih `PLANNED`** — yang menurut registry berarti belum berwenang menjalankan implementasi
maupun migration.

Yang diminta **bukan** persetujuan desain. Desainnya sudah selesai. Yang diminta adalah izin
menyentuh milik orang lain, penetapan status registry, dan jawaban atas hal yang memang bukan
urusan Laboratorium.

> **Yang tidak ikut terblokir — supaya tidak salah tunggu.** Butir 9 menahan **implementasi**,
> bukan **perencanaan**. Registry sendiri menyatakan persetujuannya "tidak memberi wewenang
> implementasi, migration, pekerjaan database, deployment", dan yang ditahan `QBE-MOD-002`
> adalah pembuatan entity `Lab*` — bukan penyusunan roadmap.
>
> Karena itu roadmap backend dan frontend tetap boleh terbit lebih dulu, dengan task yang
> menyentuh entity `Lab*` bertanda `BLOCKED` dan menyebut `LAB-OPEN-019` sebagai penahannya.
> Menunda penerbitan roadmap sampai butir 9 dijawab adalah penundaan yang tidak diminta aturan
> mana pun.

---

## 2. Prioritas 1 — memblokir gelombang `MVP-0` dan `MVP-1`

Ketiganya menyentuh tabel yang **bukan milik Laboratorium**. Tanpa izin, Laboratorium tidak
dapat memulai gelombang pertama sama sekali.

### 2.1 Pemilik `master-data` — kolom disiplin pada `MstProcedure`

| Butir | Isi |
|---|---|
| **Yang diminta** | Izin menambah **satu kolom** klasifikasi disiplin pada `MstProcedure`, dan pengisian nilainya untuk pemeriksaan berpenanda `IsLaboratory` yang sudah ada |
| **ID koordinasi** | `LAB-COORD-005` |
| **Yang diblokir** | Gelombang `MVP-0`, `EPIC-LAB-09` katalog dan harga |
| **Akibat nyata bila tidak ada** | Sistem tidak dapat memeriksa apakah pemeriksaan yang dipilih sesuai disiplin pesanannya. Petugas dapat memasukkan Hemoglobin ke pesanan Mikrobiologi, dan sistem tidak akan menolaknya |
| **Keputusan yang mendasari** | `LAB-DEC-036` |
| **Tabel terdampak** | `MstProcedure` |

**Kenapa satu kolom ini boleh, sementara kolom lain tidak.** `MstProcedure` sudah punya
`IsLaboratory`, `IsRadiology`, `IsSurgery`, dan `IsTherapy` — seluruhnya **klasifikasi** jenis
tindakan. Yang diminta sejenis dengan itu: pembeda Patologi Klinik, Patologi Anatomi, dan
Mikrobiologi.

Yang **tidak** diminta dan memang tidak boleh masuk: satuan hasil, batas nilai, jenis wadah.
Seluruhnya berada di tabel milik Laboratorium sendiri.

### 2.2 Pemilik `master-data` — dua data induk perujuk

| Butir | Isi |
|---|---|
| **Yang diminta** | Dua data induk baru: **instansi perujuk** dan **dokter perujuk** |
| **ID koordinasi** | `LAB-COORD-004` |
| **Yang diblokir** | Gelombang `MVP-1`, `EPIC-LAB-08` pendaftaran pasien rujukan luar |
| **Akibat nyata bila tidak ada** | Nama klinik perujuk hanya dapat diketik bebas. "Klinik Sehat Sentosa", "Kl. Sehat Sentosa", dan "sehat sentosa" akan terhitung sebagai tiga institusi berbeda. Laporan dokter pengirim tidak akan pernah dapat dipercaya |
| **Keputusan yang mendasari** | `LAB-DEC-035` |

**Isi minimum yang dibutuhkan:**

| Data induk | Isi |
|---|---|
| Instansi perujuk | Nama klinik atau rumah sakit, alamat, telepon, penanda aktif |
| Dokter perujuk | Nama dokter, tertaut ke instansinya, penanda aktif |

**Kenapa global, bukan milik Laboratorium.** Rujukan bukan hal khusus laboratorium. Kunjungan
pasien sudah punya penanda `IsReferral` sejak awal, dan Rawat Jalan maupun IGD juga menerima
pasien rujukan. Menaruhnya di Laboratorium berarti modul lain kelak membuat daftar tandingan.

### 2.3 Pemilik `registration-management` — dua hal sekaligus

| Butir | Isi |
|---|---|
| **Yang diminta** | **(a)** Kolom penunjuk instansi dan dokter perujuk pada `TrxPatientEncounter`. **(b)** Kesepakatan kontrak pemanggilan: Laboratorium meminta Registrasi membuat kunjungan |
| **ID koordinasi** | `LAB-COORD-004` untuk (a), `LAB-COORD-003` untuk (b) |
| **Yang diblokir** | Gelombang `MVP-1`, `EPIC-LAB-08` seluruhnya |
| **Akibat nyata bila tidak ada** | Pasien yang datang langsung ke laboratorium **tidak dapat dilayani sama sekali**. Ia harus mengantre di loket pendaftaran lebih dulu, padahal ia hanya perlu satu pemeriksaan darah |
| **Keputusan yang mendasari** | `LAB-DEC-032`, `LAB-DEC-035` |
| **Tabel terdampak** | `TrxPatientEncounter` |

**Yang perlu ditegaskan: Laboratorium tidak akan menulis ke tabel kunjungan.** Rancangannya
justru sebaliknya — layar pendaftaran berada di modul Laboratorium supaya petugas tidak
berpindah aplikasi, tetapi **Registrasi yang membuat kunjungannya**. Laboratorium mengirim
isian, menunggu jawaban, lalu menyimpan penunjuk kunjungan yang dikembalikan.

**Kabar baiknya: sebagian besar sudah ada.** Pemeriksaan pada `c87d9c0` menemukan Registrasi
sudah memiliki `EncounterRegistrationSource.WalkIn`, kolom `IsWalkIn`, penanda `IsReferral`,
`ReferralNumber`, `IsReferralRequired`, `IsReferralVerified`, dan `PatientEncounterController`
yang sudah menangani pembuatan kunjungan datang langsung. Yang belum ada hanya kolom penunjuk
perujuk dan kesepakatan bentuk pemanggilannya.

**Yang perlu disepakati pada kontrak pemanggilan:**

| Aspek | Catatan |
|---|---|
| Bentuk permintaan dan jawaban | Isian pendaftaran masuk, penunjuk kunjungan keluar |
| Idempotensi | **Wajib.** Petugas menekan Simpan dua kali tidak boleh menghasilkan dua kunjungan untuk satu pasien pada hari yang sama |
| Perilaku saat ditolak | Penolakan diteruskan apa adanya. Laboratorium tidak menyimpan data setengah jadi |

---

## 3. Prioritas 2 — memblokir gelombang `MVP-2`

### 3.1 Pemilik repository backend atau DBA — jumlah data laboratorium

| Butir | Isi |
|---|---|
| **Yang diminta** | Jawaban satu angka: **berapa baris `TrxLabSpecimen` yang ada di basis data produksi?** |
| **ID koordinasi** | `LAB-OPEN-012` |
| **Yang diblokir** | Gelombang `MVP-2`, migration pemisahan wadah dan pemeriksaan |
| **Akibat nyata bila tidak dijawab** | Migration yang mengubah struktur tabel berjalan tanpa mengetahui berapa banyak data pasien yang terdampak |

**Kenapa ini penting dan mudah.** Frontend Laboratorium tidak ada sama sekali pada
`688daff90`, sehingga besar kemungkinan belum ada data pasien sungguhan. Bila benar nol,
seluruh kerumitan pemindahan data gugur dan migration menjadi biasa. Tetapi itu **dugaan,
bukan bukti** — dan mengubah struktur tabel berdasarkan dugaan tidak dapat diterima.

Yang dibutuhkan hanya satu perhitungan baris.

### 3.2 Pemilik repository backend — dokumen tata kelola ~~yang hilang~~ **SUDAH TERJAWAB**

| Butir | Isi |
|---|---|
| **ID koordinasi** | `LAB-OPEN-002` — **ditutup 2026-09-01** |
| **Jawabannya** | Dokumennya **tidak hilang**. Keduanya ada pada commit `c9692d0` "Repair QBE canonical governance paths" |
| **Sebab kekeliruan** | Checkout lokal berada di cabang `yoga` pada `c87d9c0`, **7 commit tertinggal** dari `origin/yoga`. Folder `docs/engineering/` memang belum ada di working copy, tetapi ada di remote |
| **Dampak pada blueprint** | Ketujuh commit itu **tidak menyentuh Laboratorium** — hanya IGD, dokumen, dan tooling. Capability map tetap sahih |
| **Tindakan yang diperlukan** | ~~Menarik ketujuh commit itu ke checkout lokal~~ — **selesai 2026-09-02.** `c9692d0` kini leluhur `HEAD`, dan `docs/engineering/` ada di working tree |

**Koreksi atas lokasi canonical — 2026-09-02.** Baris "Jawabannya" di atas benar bahwa
dokumennya tidak hilang, tetapi versi terdahulu bagian ini menyimpulkan bahwa sumber
canonical-nya adalah `docs/engineering/` di repository backend. **Kesimpulan itu keliru** dan
bertentangan dengan bagian 3.4.

Yang berlaku menurut `AGENTS.md` baris 13: lapisan operasional tata kelola tidak lagi tinggal
di repository backend, melainkan datang dari suite Skill terpasang lewat jalur logis
`rules/backend/engineering/`. Jadi hari ini ada **tiga salinan** dokumen yang sama:

| Salinan | Kedudukan |
|---|---|
| `QuilvianEngineeringSkills/agents/rules/backend/engineering/` | Sumber lintas vendor |
| `QuilvianEngineeringSkills/Claude/.claude/rules/backend/engineering/` | Edisi Claude, identik |
| `NewQuilvianSystemBackend/docs/engineering/` | **Kedudukannya belum ditetapkan** — lihat 3.5 |

Ketiganya berisi teks identik per 2026-09-02, jadi belum ada perbedaan isi yang merugikan.

**Yang ditemukan setelah dokumennya dibaca.** Rancangan Laboratorium ternyata melanggar
`QBE-NAM-001` — tiga entity baru diberi awalan `Trx*`, yang dilarang untuk kode baru. Sudah
diperbaiki menjadi `LabExamination`, `LabValueBoundChangeRequest`, dan `LabValueBoundHistory`.

Dua hal lain muncul dan **belum terjawab** — lihat bagian 3.3 dan 3.4.

---

### 3.3 Pemilik registry prefix — lifecycle Laboratorium masih `PLANNED`

| Butir | Isi |
|---|---|
| **Yang diminta** | Menaikkan Lifecycle `LaboratoryManagement / Laboratory` dari `PLANNED` menjadi `ACTIVE`, **atau** pernyataan tertulis mengenai dasar pekerjaan yang sudah berjalan |
| **ID koordinasi** | `LAB-OPEN-019` |
| **Yang diblokir** | **Seluruh gelombang MVP.** `MVP-0` sampai `MVP-4` |

**Kenapa ini serius.** `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` menyatakan tegas:

> *"Persetujuan registry hanya memberi wewenang penamaan dan kepemilikan. Ia **tidak** memberi
> wewenang implementasi, migration, pekerjaan database, deployment, maupun aktivasi modul
> berstatus `PLANNED`."*

Baris registry yang berlaku:

| Area | Module/pemilik | Category | Prefix | Lifecycle |
|---|---|---|---|---|
| HealthServices | LaboratoryManagement / Laboratory | BUSINESS DOMAIN / MODULE | Lab | **`PLANNED`** |

**Keadaan yang perlu dijelaskan.** `LabOrder`, `TrxLabSpecimen`, `TrxLabTransitionHistory`, dan
`MstLabRejectionReason` **sudah berjalan di produksi** beserta migration dan 30 pengujian
(dikoreksi dari 31 lewat impact scan 2026-09-02: 18 pada `LaboratorySpecimenLifecycleTests.cs`,
12 pada `LaboratoryAuthorityTests.cs`).
Padahal menurut registry, modul berstatus `PLANNED` belum berwenang menjalankan implementasi
maupun migration.

Dua kemungkinan, dan keduanya perlu ditutup:

| Kemungkinan | Tindakan |
|---|---|
| Lifecycle-nya yang usang — pekerjaan itu memang sudah diberi wewenang | Naikkan ke `ACTIVE`, catat pada *Catatan perubahan lifecycle* |
| Pekerjaan itu berjalan tanpa wewenang registry | Perlu ditinjau pemilik registry sebelum pekerjaan baru ditambahkan |

Selama belum dijawab, blueprint ini **tidak boleh** diteruskan ke implementasi.

---

### 3.4 Pemilik registry prefix — prefix data induk Laboratorium

| Butir | Isi |
|---|---|
| **Yang diminta** | Penetapan prefix untuk data induk yang dimiliki Laboratorium |
| **ID koordinasi** | `LAB-OPEN-021` — lihat catatan tabrakan ID di bawah |
| **Yang diblokir** | Penamaan dua tabel baru: batas nilai dan pilihan hasil |

> **Tabrakan ID — perlu dirapikan.** Versi terdahulu bagian ini memakai `LAB-OPEN-018`, padahal
> ID yang sama sudah dipakai untuk persoalan rules root di akhir bagian ini. Satu ID untuk dua
> penghambat berbeda. Pertanyaan prefix diberi ID sendiri, `LAB-OPEN-021`, per 2026-09-02.
> `blueprint-manifest.md` bagian `active_blockers` ikut menyesuaikan.

**Persoalannya.** `QBE-NAM-002` mewajibkan kode baru memakai prefix registry milik pemiliknya,
dan `QBE-NAM-004` melarang menyimpulkan prefix sendiri. Registry punya dua baris yang sama-sama
masuk akal:

| Baris registry | Prefix | Bila dipakai | Sejalan dengan |
|---|---|---|---|
| Master / Reference | `Mst` | `MstLabValueBound`, `MstLabValueOption` | `MstLabRejectionReason` yang sudah ada |
| LaboratoryManagement / Laboratory | `Lab` | `LabValueBound`, `LabValueOption` | Aturan `<PrefixPemilik><Konsep>`, karena pemiliknya Laboratorium |

Blueprint **tidak memutuskan sendiri**. Sampai dijawab, penamaan kedua tabel berstatus belum
final.

#### Usulan yang dimintakan persetujuan — 2026-09-02

Agar pertanyaan ini cukup dijawab **ya atau tidak**, berikut usulan beserta buktinya. Blueprint
tetap tidak memutuskan; pemilik registry yang menetapkan.

> **Usulan: pakai `Lab`.** Jadi `LabValueBound` dan `LabValueOption`, bukan `MstLabValueBound`
> dan `MstLabValueOption`.

Alasannya bukan selera, melainkan perilaku checker yang sudah berjalan.
`tooling/qbe/Invoke-QbeConformanceCheck.ps1` menentukan pemilik sebuah entity **dari letak
foldernya**, lalu mewajibkan prefixnya cocok:

| Langkah checker | Hasil untuk `Areas/HealthServices/LaboratoryManagement/Models/` |
|---|---|
| Ambil segmen modul dari path | `laboratorymanagement` |
| Cari baris registry yang namanya cocok | Baris `LaboratoryManagement / Laboratory`, prefix `Lab` |
| Baris `Master / Reference` ikut dipertimbangkan? | **Tidak.** Aliasnya `master` dan `reference`, tidak cocok dengan segmen path mana pun |
| Uji prefix | Entity wajib diawali `Lab` |

Artinya `MstLabValueBound` yang diletakkan di folder Laboratorium akan dilaporkan checker
sebagai pelanggaran `QBE-MOD-002`, dengan alasan *"Entity 'MstLabValueBound' does not use
approved registry prefix 'Lab'"*. Prefix `Mst` hanya sah bila tabelnya benar-benar pindah ke
folder Master Data dan kepemilikannya diserahkan ke Master Data — yang bertentangan dengan
pernyataan blueprint sendiri bahwa kedua tabel ini milik Laboratorium.

**Preseden `MstLabRejectionReason` justru memperkuat, bukan melemahkan.** Berkasnya ada di
`Areas/HealthServices/LaboratoryManagement/Models/MstLabRejectionReason.cs` — pola yang sama
persis, dan menurut aturan yang berlaku sekarang ia **tidak konform**. Ia lolos hanya karena
checker mengecualikan legacy yang tidak disentuh. Mengikutinya untuk tabel baru berarti
menyalin cacat yang sudah ada.

**Yang perlu dirapikan apa pun jawabannya.** Blueprint saat ini memakai **dua penamaan
sekaligus** untuk aggregate yang sama:

| Artefak | Penamaan yang dipakai |
|---|---|
| `erd/`, `data-dictionary.md`, `02-backend-architecture.md` §4.4–4.5 | `MstLabValueBound`, `MstLabValueOption` |
| `contracts/api-contract.md`, `contracts/permission-audit-matrix.md` | `LabValueBound` — sebagai nama resource hak akses dan DTO |
| Dua entity anak pada aggregate yang sama | `LabValueBoundChangeRequest`, `LabValueBoundHistory` |

Induknya berprefix `Mst`, kedua anaknya berprefix `Lab`, dan hak aksesnya bernama `Lab`.
Selama ini dibiarkan, roadmap akan menerbitkan task yang kontraknya menyebut satu nama tabel
sementara ERD-nya menyebut nama lain. Menjawab butir 10 sekaligus menutup ketidaksesuaian ini.

#### Jawaban atas butir 5 — 2026-09-01

> Bagian di bawah ini menjawab **butir 5** (lokasi dokumen tata kelola), bukan pertanyaan
> prefix di atasnya. Letaknya di sini karena dari jawaban inilah `LAB-OPEN-018` dan
> `LAB-OPEN-019` diturunkan.

| Butir | Isi |
|---|---|
| **Status** | **TERJAWAB.** `LAB-OPEN-002` ditutup oleh `LAB-FACT-007` |
| **Keduanya masih berlaku?** | **Ya.** Tidak dicabut. `AGENTS.md` tetap menempatkannya pada urutan wewenang ke-2 dan ke-3, dan `QBE-MOD-002`/`QBE-MOD-003`/`QBE-NAM-004` masih dikutip aktif oleh blueprint lain |
| **Lokasi canonical** | `QuilvianEngineeringSkills/agents/rules/backend/engineering/` — sumber lintas vendor |
| **Lokasi edisi Claude** | `QuilvianEngineeringSkills/Claude/.claude/rules/backend/engineering/` — identik byte-per-byte |
| **Kenapa dulu tidak ketemu** | Kedua dokumen dipindahkan keluar dari repository backend ke repository suite Skill, dan checkout lokal saat itu tertinggal 7 commit. **Catatan 2026-09-02:** anggapan bahwa `AGENTS.md` baris 11 dan 20 masih menunjuk `docs/engineering/` sudah tidak berlaku — keduanya kini memakai jalur logis `rules/backend/engineering/` |

**Dua penghambat baru yang muncul dari jawaban ini — masih memerlukan tindakan:**

| ID | Isi | Tindakan yang diminta |
|---|---|---|
| `LAB-OPEN-018` | Rules root yang **terpasang** (`${CLAUDE_PLUGIN_ROOT}/.claude/rules/`) tertinggal jauh dari sumbernya — lihat rinciannya di bawah | Perbarui suite Skill terpasang. Selama belum, gerbang `AGENTS.md` sendiri memaksa setiap task backend berhenti dengan `BLOCKED — canonical governance unavailable` |
| `LAB-OPEN-019` | Registry mencatat `HealthServices / LaboratoryManagement / Laboratory`, prefix `Lab`, lifecycle `PLANNED`. Hak penamaan sudah ada, izin implementasi belum | Naikkan lifecycle `PLANNED` → `ACTIVE`, dengan preseden `RWI-DEC-068` untuk `InPatientManagement` |

**Rincian `LAB-OPEN-018` — pemeriksaan 2026-09-02 menemukan cakupannya jauh lebih luas.** Rules
root terpasang berisi **13 berkas**; sumber canonical berisi **29**. Yang hilang bukan hanya
subfolder `engineering/`:

| Yang hilang dari rules root terpasang | Akibatnya |
|---|---|
| `GLOBAL_RULES.md` | `AGENTS.md` memerintahkan membacanya **paling awal**, sebelum dokumen lain |
| `backend/engineering/` — kedua dokumen tata kelola | Gerbang `BLOCKED — canonical governance unavailable` |
| `rule-output/bentuk-blueprint.md` | Dikutip `plan-module-delivery` langkah 6 sebagai penentu letak roadmap |
| `backend/backend-project-profile.md`, `master-data-endpoint-standard.md`, `transaction-endpoint-standard.md`, `role-access-rules.md` | Standar endpoint dan hak akses tidak terbaca saat implementasi |
| 10 dari 11 rules frontend — termasuk `base-component-catalog.md`, `design-tokens.md`, `master-data-feature-standard.md`, `page-composition-patterns.md` | `build-module-frontend` ikut kehilangan pijakan |

Suite terpasang tercatat `0.1.0` pada `6301c62` (2026-08-24); repository skill kini sudah di
`636377c`. **Ini wewenang pemasang di mesin masing-masing, bukan pihak ketiga** — cukup
perbarui plugin dari marketplace `quilvian`.

**Dua butir "rapikan juga" pada versi terdahulu sudah tidak berlaku — dicoret 2026-09-02:**

| Butir lama | Keadaan sebenarnya di `HEAD` |
|---|---|
| ~~`AGENTS.md` baris 11 dan 20 masih menunjuk `docs/engineering/`~~ | **Sudah benar.** Keduanya memakai jalur logis `rules/backend/engineering/` |
| ~~Folder peninggalan `agents/rules/` (7 berkas) perlu dibereskan~~ | **Sudah tidak ada.** Folder `agents/` tidak ada di repository backend |

Yang menggantikan keduanya adalah satu temuan baru yang lebih serius — lihat 3.5.

---

### 3.5 Andry Zain — checker QBE tidak dapat dijalankan

| Butir | Isi |
|---|---|
| **Yang diminta** | Perbaiki path dokumen tata kelola di dalam `tooling/qbe/Invoke-QbeConformanceCheck.ps1` |
| **ID koordinasi** | `LAB-OPEN-020` — **baru 2026-09-02** |
| **Kepada** | **Andry Zain** (`andryzainhome`, `andryzain01@gmail.com`), selaku pemilik repository backend — ditetapkan pemilik modul Laboratorium 2026-09-02 |
| **Yang diblokir** | Setiap pemeriksaan konformansi QBE, termasuk gerbang CI pada setiap pull request |

**Buktinya, dijalankan langsung pada `HEAD`:**

```text
> ./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode ReportOnly
TOOL ERROR: Canonical governance missing: agents/rules/engineering/BACKEND_ENGINEERING_CONTRACT.md
Final result: TOOL ERROR
EXITCODE=2
```

**Sebabnya.** Script mencari dokumen tata kelola pada `agents/rules/engineering/` — path yang
sudah tidak ada di repository ini, dan yang oleh `AGENTS.md` sendiri dinyatakan dicabut. Ada
empat tempat di dalam script yang masih menunjuk ke sana:

| Baris | Isinya |
|---|---|
| 29–30 | Daftar `$requiredAuthority` — dua dokumen tata kelola |
| 36 | Pembacaan `BACKEND_ENGINEERING_CONTRACT.md` |
| 85 | Default `QBE_EXCEPTIONS.json` |
| 162 | Pembacaan tabel registry |

**Yang membuatnya membingungkan: dokumentasinya sendiri sudah benar.**
`tooling/qbe/README.md` menyebut `docs/engineering/`, dan menyatakan CI membaca
`docs/engineering/QBE_EXCEPTIONS.json`. Ketiga berkas itu memang ada di sana. Hanya scriptnya
yang belum ikut diperbarui.

**Akibat yang perlu diketahui pemilik repository.** Workflow `QBE Conformance / QBE Strict
GitRange` berjalan pada setiap pull request yang menyasar `QuilvianIntegrationBackend`, dan
memanggil script yang sama. Selama path ini belum diperbaiki, gerbang QBE di CI **tidak sedang
memeriksa apa pun** — ia gagal sebagai TOOL ERROR, bukan lolos.

**Kenapa ini penting bagi Laboratorium.** Butir 9 dan 10 pada akhirnya ditegakkan oleh script
ini: ia yang membaca kolom Lifecycle dan menolak entity berprefix salah. Menaikkan lifecycle ke
`ACTIVE` tanpa memperbaiki script berarti wewenangnya bertambah sementara pemeriksaannya tetap
mati. Sebaiknya keduanya berjalan bersamaan.

> Perbaikannya sepenuhnya wewenang Andry Zain selaku pemilik repository backend dan tidak
> menunggu siapa pun. Laboratorium tidak mengubah berkas ini.

---

## 4. Prioritas 3 — memblokir seluruh slice hasil pemeriksaan

Bagian ini tidak memblokir `MVP-0` sampai `MVP-4`, tetapi memblokir kelanjutannya. Diajukan
sekarang agar dapat berjalan paralel.

### 4.1 Dokter penanggung jawab laboratorium atau Komite Medis

| Butir | Isi |
|---|---|
| **Yang diminta** | Tanda tangan atas tiga keputusan keselamatan pasien |
| **ID koordinasi** | `LAB-SIGN-001` |
| **Yang diblokir** | Seluruh pengisian, validasi, rilis, nilai kritis, dan koreksi hasil |

| Keputusan | Isi | Kenapa perlu tanda tangan klinis |
|---|---|---|
| `LAB-DEC-003` | Pengisi hasil tidak boleh memvalidasi hasil yang sama, dengan jalur pengecualian bertanda permanen | Menentukan siapa yang boleh menyatakan sebuah angka hasil benar |
| `LAB-DEC-004` | Nilai kritis tetap dirilis, pelaporan wajib tercatat | Menentukan apa yang terjadi ketika pasien dalam bahaya |
| `LAB-DEC-007` | Koreksi hasil hanya oleh petugas berwenang validasi, dokter otomatis diberi tahu | Menentukan apa yang terjadi ketika hasil yang sudah dipakai ternyata salah |

Pemilik modul sudah menyetujui ketiganya dari sisi produk dan operasional lewat `LAB-DEC-011`,
sekaligus menyatakan bahwa wewenang klinis berada di pihak lain.

### 4.2 Pemilik platform — kemampuan pemberitahuan bersama

| Butir | Isi |
|---|---|
| **Yang diminta** | Kesepakatan bahwa pemberitahuan tersimpan dibangun sebagai kemampuan platform, bukan milik Laboratorium |
| **ID koordinasi** | `LAB-COORD-001` |
| **Yang diblokir** | Nilai kritis dan pemberitahuan koreksi hasil |
| **Akibat nyata bila dibangun di Laboratorium** | Ketika Farmasi dan Radiologi kelak membutuhkan hal serupa, dokter harus memeriksa tiga kotak pemberitahuan berbeda. Untuk nilai kritis, itu berbahaya |

Pemeriksaan pada `c87d9c0` menemukan platform **belum punya** sarana pemberitahuan apa pun —
tidak ada tabel notifikasi, tidak ada surel, tidak ada pesan singkat. Yang ada hanya
`Hubs/QueueHub.cs` yang khusus melayani antrean nurse station.

### 4.3 Pemilik `rekam-medis` — jenis dokumen klinis baru

| Butir | Isi |
|---|---|
| **Yang diminta** | Izin menambah satu nilai pada daftar jenis dokumen klinis, untuk hasil laboratorium |
| **ID koordinasi** | `LAB-COORD-002` |
| **Yang diblokir** | Pendaftaran hasil ke rekam medis, koreksi hasil setelah kunjungan ditutup, penautan berkas hasil eksternal |
| **Akibat nyata bila tidak ada** | Hasil laboratorium tidak terlihat sebagai bagian berkas rekam medis pasien. Saat akreditasi atau sengketa, hasil yang tidak tercatat di rekam medis sulit dipertanggungjawabkan |

**Yang tidak diminta:** menyalin isi hasil ke tabel rekam medis. Angka hasil tetap disimpan
Laboratorium. Yang didaftarkan hanya keutuhannya — siapa penulisnya, kapan ditandatangani, dan
kapan terkunci.

Mekanisme koreksi untuk dokumen terkunci **sudah tersedia** lewat addendum, sehingga tidak ada
kemampuan baru yang perlu dibangun di modul `rekam-medis`.

---

## 5. Ringkasan yang Diminta

| No | Kepada | Yang diminta | Memblokir | Keadaan |
|---:|---|---|---|---|
| 1 | Pemilik `master-data` | Satu kolom disiplin pada `MstProcedure` | `MVP-0` | ✅ **Disetujui** |
| 2 | Pemilik `master-data` | Dua data induk perujuk | `MVP-1` | ✅ **Disetujui** |
| 3 | Pemilik `registration-management` | Kolom penunjuk perujuk + kontrak pemanggilan | `MVP-1` | ✅ **Disetujui** |
| 4 | Pemilik repository backend / DBA | Jumlah baris `TrxLabSpecimen` di produksi | `MVP-2` | ⏳ Menunggu **angka** |
| 5 | Pemilik repository backend | Lokasi dua dokumen tata kelola | Seluruh implementasi | ✅ **Terjawab sendiri** — lihat 3.2 |
| 6 | Dokter PJ laboratorium / Komite Medis | Tanda tangan tiga keputusan keselamatan | Seluruh slice hasil | ⏳ Di luar wewenang pemilik repo |
| 7 | Pemilik platform | Kesepakatan pemberitahuan sebagai kemampuan platform | Nilai kritis | ✅ **Disetujui** |
| 8 | Pemilik `rekam-medis` | Satu jenis dokumen klinis baru | Hasil ke rekam medis | ✅ **Disetujui** |
| **9** | **Pemilik registry prefix** | **Lifecycle Laboratorium dari `PLANNED` ke `ACTIVE`** | **Seluruh implementasi MVP** | 🟡 **Diajukan 2026-09-02** |
| **10** | **Pemilik registry prefix** | **Prefix data induk Laboratorium — usulan `Lab`, tinggal disetujui atau ditolak** | Penamaan dua tabel + konsistensi kontrak | 🟡 **Diajukan 2026-09-02** |
| **11** | **Andry Zain** — pemilik repository backend | **Perbaiki path tata kelola di `Invoke-QbeConformanceCheck.ps1`** | Gerbang QBE di CI | 🟡 **Diajukan 2026-09-02** |

**Nomor 9 adalah penghalang terberat yang tersisa** — tetapi terhadap **implementasi**, bukan
perencanaan. Nomor 1 sampai 3 yang sudah disetujui tidak dapat dieksekusi selama registry masih
menyatakan modul ini `PLANNED`. Roadmap backend dan frontend tetap boleh terbit lebih dulu
dengan task bertanda `BLOCKED` — lihat kotak pada bagian 1.

**Nomor 10 kini berbentuk usulan, bukan pertanyaan terbuka.** Cukup dijawab ya atau tidak.
Buktinya ada di bagian 3.4: checker menurunkan pemilik dari letak folder, sehingga prefix `Mst`
untuk tabel yang duduk di folder Laboratorium akan dilaporkan sebagai pelanggaran.

**Nomor 11 tidak menunggu siapa pun.** Perbaikan path di satu berkas script, sepenuhnya di
tangan Andry Zain selaku pemilik repository backend. Tanpa itu, menaikkan lifecycle pada nomor 9
menambah wewenang tanpa menghidupkan pemeriksaannya.

**Nomor 4 masih menunggu satu angka.** Yang paling murah dijawab dari seluruh daftar.

**Nomor 6 tidak dapat ditutup pemilik repository.** Wewenangnya klinis — lihat bagian 0.3.

---

## 6. Yang Tidak Diminta

Agar tidak disalahpahami, berikut yang **bukan** bagian permintaan ini:

| Bukan yang diminta | Keterangan |
|---|---|
| Persetujuan atas rancangan Laboratorium | Sudah disetujui pemilik modulnya |
| Izin menulis ke tabel modul lain | Laboratorium **tidak akan** menulis. Ia memanggil, membaca, dan menyajikan |
| Perubahan cara kerja modul lain | Tidak ada alur modul lain yang berubah |
| Penambahan kolom operasional pada `MstProcedure` | Hanya satu kolom klasifikasi. Satuan, batas nilai, dan jenis wadah tetap di tabel Laboratorium |
| Pemindahan data tarif | Tarif tetap milik Master Data. Laboratorium hanya menyajikannya, baca saja |

---

## 7. Riwayat

| Tanggal | Perubahan | Status |
|---|---|---|
| 2026-09-01 | Permintaan disusun dari `blueprint-manifest.md` revision 9 | `dikirim` |
| 2026-09-01 | Butir 1, 2, 3, 7, 8 disetujui `andryzainhome` dan `sukmagp` | `dijawab sebagian` |
| 2026-09-01 | Butir 5 terjawab lewat penelusuran sendiri: dokumennya ada di `c9692d0`, checkout lokal 7 commit tertinggal | `ditutup` |
| 2026-09-01 | Butir 9 dan 10 ditambahkan setelah kontrak engineering dibaca | `belum diajukan` |
| 2026-09-01 | Butir 5 terjawab lewat penelusuran repository: kedua dokumen tata kelola ditemukan di `QuilvianEngineeringSkills/agents/rules/backend/engineering/` dan **masih berlaku**. `LAB-OPEN-002` ditutup oleh `LAB-FACT-007`; `LAB-OPEN-018` dan `LAB-OPEN-019` dibuka sebagai penghambat implementasi penggantinya | `draft` |
| 2026-09-02 | Pertentangan lokasi canonical antara bagian 3.2 dan 3.4 diperbaiki. Bagian 3.2 yang keliru; `AGENTS.md` baris 13 yang berlaku | `dikoreksi` |
| 2026-09-02 | Dua butir "rapikan juga" dicoret setelah diverifikasi pada `HEAD`: `AGENTS.md` baris 11 dan 20 sudah benar, folder `agents/rules/` sudah tidak ada | `dikoreksi` |
| 2026-09-02 | Cakupan `LAB-OPEN-018` diperluas setelah rules root terpasang dibandingkan dengan sumbernya — 13 berkas berbanding 29, termasuk `GLOBAL_RULES.md` dan `bentuk-blueprint.md` yang hilang | `diperbarui` |
| 2026-09-02 | Tabrakan ID diperbaiki: pertanyaan prefix dipisahkan dari persoalan rules root, diberi ID `LAB-OPEN-021` | `dikoreksi` |
| 2026-09-02 | Butir 10 diubah dari pertanyaan terbuka menjadi usulan `Lab` beserta bukti perilaku checker. Ketidaksesuaian penamaan lintas artefak didokumentasikan | `diajukan` |
| 2026-09-02 | Butir 11 dibuka (`LAB-OPEN-020`): `Invoke-QbeConformanceCheck.ps1` gagal dengan TOOL ERROR karena menunjuk path tata kelola yang sudah dicabut. Gerbang QBE di CI tidak sedang memeriksa apa pun | `diajukan` |
| 2026-09-02 | Butir 9 dan 10 diteruskan ke pemilik registry prefix | `diajukan` |
| 2026-09-02 | Butir 11 ditujukan kepada **Andry Zain** (`andryzainhome`, `andryzain01@gmail.com`) selaku pemilik repository backend, atas penetapan pemilik modul Laboratorium | `diajukan` |
| 2026-09-02 | Roadmap backend dan frontend diterbitkan di `roadmap/` beserta traceability-nya. `input_hashes` manifest dihitung ulang sebagai sha256 penuh setelah konvensinya ditemukan dan diverifikasi | `berjalan` |
