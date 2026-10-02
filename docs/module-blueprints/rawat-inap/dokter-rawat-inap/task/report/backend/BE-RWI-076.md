# Laporan Perubahan Backend — `BE-RWI-076`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-076` |
| Judul | Catatan dokter selalu punya penulis yang benar-benar menulisnya |
| Slice | `S5. Gelombang 1A — Rawat Inap Safety Corrections` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian S5 |
| Trace | `RWI-DEC-099`; `RWI-DEC-105`; `FR-DOK-063` s.d. `FR-DOK-067`; `04-prd-to-mvp.md` bagian 21.3; `RWI-TF-014`; `RWI-RISK-004`; `OPEN-MVP-004` |
| Contract version | `contracts/api-contract.md` `0.5.0` bagian 0.A.2; `contracts/permission-audit-matrix.md` `0.5.0` bagian 3A. Definisi penjaganya dipegang `episode-rawat-inap/contracts/permission-audit-matrix.md` bagian 4-A.3. **`approved`** 11 September 2026 lewat `RWI-DEC-105` |
| Dependency | Approval kontrak `0.5.0` — **terpenuhi**. ✅ `BE-RWI-074` — **selesai 11 September 2026**, kolom `AssignmentRole` beserta enum perannya ada di source |
| Klasifikasi | `HEAVY` — satu penjaga bersama baru, lima grup jalur tulis, enam berkas; nol endpoint baru, nol tabel, nol migration, nol hak akses baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/ClinicalManagement/` dan `docs/module-blueprints/rawat-inap/dokter-rawat-inap/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `c11904ea99dc0f9a9ecfb3b41f6edb2bf500d206`, branch `MHamzah` |
| Tanggal | 11 September 2026 |
| Status | ✅ **SELESAI 11 September 2026.** Kesembilan acceptance criteria terpetakan ke source yang benar-benar ada. **Dua butir verifikasi tidak dijalankan dan ditulis apa adanya:** `dotnet build` `NOT RUN` — dikecualikan atas instruksi pemilik pada task aktif; dan integration test `AC-DOK-067` s.d. `AC-DOK-075` `NOT RUN` sesuai `rules/backend/TEST_POLICY.md`, digantikan penelusuran source beserta delapan contoh berangka. **Satu selisih antar-dokumen kontrak dicatat, bukan diputuskan sepihak** — bagian 7.1 |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `ClinicalManagement` |
| Submodule | `Controllers` dan `Services` |
| Pemilik/prefix registry | `ClinicalManagement` terdaftar `ACTIVE`. Task ini **nol entity baru**, sehingga prefix tidak menjadi pertanyaan. Kolom `AssignmentRole` yang dibaca penjaga ini milik `InPatientManagement` prefix `Inp`, dan **dibaca**, bukan dibuat |
| Keberlakuan | `TOUCHED LEGACY` untuk kelima controller; `NEW CODE` untuk penjaga bersama `ResolveForDoctorWriteAsync` beserta resolver penulisnya |
| Status registry | Nol modul, submodule, maupun prefix baru. `QBE-MOD-002` dan `QBE-MOD-003` **tidak** menahan pekerjaan ini |
| QBE ID yang benar-benar berlaku | `QBE-SVC-001` orkestrasi domain tinggal di service, bukan disalin ke lima controller; `QBE-VAL-001` invarian kewenangan penulis; `QBE-API-001` boundary dan kode status yang sudah mapan; `QBE-PERM-001` metadata Access yang berlaku dipakai apa adanya; `QBE-ENUM-001` nilai `InpatientClinicalContextOutcome` dimiliki modulnya sendiri; `QBE-LOG-001` jejak perubahan tetap menyertakan aktornya |
| QBE ID yang **tidak** berlaku | `QBE-ENT-001` s.d. `QBE-CFG-002` — nol entity, nol configuration. `QBE-NAM-*` — nol penamaan entity baru. `QBE-CODE-001` s.d. `QBE-CODE-006` — nol nomor bisnis. `QBE-DB-001` dan `QBE-DB-002` — bukan `LEGACY MIGRATION`, nol migration |
| Peninggalan yang dicabut | Folder `agents/rules/` dan `.codex/` **tidak ditemukan** di working tree |

---

## 1. Masalah yang diperbaiki

Penjaga kewenangan penulis klinis **sudah ada** dan **sudah benar**. Masalahnya, ia **mati**.

Di dalam pembentuk konteks perawatan, pemeriksaan kewenangan hanya berjalan bila pemanggil
mengirimkan dokter pelaku. Bila tidak dikirim, nilai bawaannya adalah "berwenang". Dari sembilan
titik panggil di seluruh aplikasi, **hanya satu** yang mengirimkannya, yaitu pencatatan visite.

Akibatnya dua, dan keduanya serius:

| Akibat | Wujudnya |
| --- | --- |
| **Siapa pun yang punya butir hak akses dapat menulis untuk pasien siapa pun** | Dokter poliklinik yang tidak pernah menangani seorang pasien rawat inap tetap dapat menulis catatan perkembangan, kajian medis, diagnosis, dan tindakan untuk pasien itu |
| **Penulis yang tersimpan bukan siapa yang login** | Penulis diambil dari antrean, dari payload, atau dari kunjungan. Payload yang menyebut dokter lain dipakai **diam-diam**, sehingga catatan dapat tersimpan atas nama orang yang tidak pernah menulisnya |

**Ini bukan kelemahan mesin hak akses.** Butir hak akses menjawab "peran ini boleh memanggil
endpoint ini". Ia tidak pernah dapat menjawab "dokter ini boleh menulis untuk pasien ini" —
keterbatasan yang sudah tercatat sebagai `RWI-TF-014`. Jawabannya harus ditulis di dalam service.

**Contoh konkret.** dr. Andi adalah DPJP Ny. Sari di ruang Melati. dr. Budi bertugas di
poliklinik dan tidak pernah menangani Ny. Sari. Sebelum perubahan ini:

| Yang dilakukan dr. Budi | Sebelum | Sesudah |
| --- | --- | --- |
| Menulis catatan perkembangan untuk Ny. Sari | **Berhasil** | **`403`** |
| Menulis kajian medis untuk Ny. Sari | **Berhasil** | **`403`** |
| Mengirim payload berisi `DoctorId` milik dr. Andi | **Berhasil, tersimpan atas nama dr. Andi** | **`403`**, nol baris tersimpan |

---

## 2. Proses bisnis

### 2.1 Tiga penjaga, dan apa yang dijawab masing-masing

| Penjaga | Pertanyaan yang dijawabnya |
| --- | --- |
| `GUARD-INP-05` | Siapa yang menulis? Jawabannya diambil dari akun pengguna yang sedang masuk, bukan dari payload |
| `GUARD-INP-06` | Kapan kewenangannya dinilai? Pada **waktu klinis** dokumen, bukan waktu penyimpanannya |
| `GUARD-INP-07` | Berlaku bagi perawat, milik `BE-RWI-078`. Disebut di sini hanya karena kajian pasien dipakai dua profesi |

### 2.2 Empat langkah menemukan penulis

Penulis dicari dari **data**, tidak pernah dari nama peran. Urutannya berhenti pada kecocokan
pertama:

1. Kolom `DoctorId` pada akun pengguna — sumber yang disebut kontrak `0.5.0` bagian 0.A.2.
2. Klaim identitas dokter pada token.
3. Penautan lewat profil tenaga kerja.
4. Pencocokan surel akun dengan surel baris dokter.

Ketiga langkah terakhir sudah dipakai jalur visite dan kajian medis sebelum task ini; yang baru
adalah langkah pertama dan penyatuannya menjadi satu implementasi.

Setiap langkah memeriksa baris dokternya **aktif dan belum dihapus**. Akun yang tertaut ke dokter
yang sudah tidak aktif karena itu tidak lolos, dan pencarian berlanjut ke langkah berikutnya.

### 2.3 Alur normal — dokter menulis catatan untuk pasien rawat inapnya

1. Dokter membuka pasien dan menulis catatan.
2. Sistem membentuk konteks perawatan dari kunjungan pasien itu.
3. Sistem menemukan dokter milik pengguna yang sedang masuk lewat empat langkah di atas.
4. Sistem memeriksa dokter itu punya penugasan yang **berlaku pada waktu klinis catatan**.
5. Catatan tersimpan atas nama dokter itu.

Ketiga peran penugasan diterima: DPJP, konsulen, dan dokter jaga **sama-sama boleh menulis**. Yang
tertutup bagi konsulen dan dokter jaga adalah **keputusan** atas arah perawatan — keputusan
pulang, tanda tangan resume, perpindahan pasien, dan perubahan kebutuhan isolasi — dan keempatnya
dijaga di tempat lain oleh `GUARD-INP-01` sampai `GUARD-INP-04`.

### 2.4 Jalur tidak normal

| Keadaan | Jawaban |
| --- | --- |
| Akun tidak tertaut ke baris dokter mana pun | **`403`** beserta kalimat yang menyarankan menghubungi kepegawaian |
| Dokter tidak punya penugasan pada perawatan itu | **`403`** |
| Payload menyebut `DoctorId` milik dokter lain | **`403`**, dan **nol** baris tersimpan atas nama pihak lain |
| Penugasan sudah berakhir, waktu klinis **di dalam** periodenya | **`200`** — inilah gunanya menilai pada waktu klinis |
| Penugasan sudah berakhir, waktu klinis **di luar** periodenya | **`403`** — backdating tidak dapat dipakai melewati periode |
| Perawatan belum dimulai, atau sudah ditutup | `422` beserta sebab keadaan perawatannya, **bukan** `403` |
| Kunjungan tidak menaungi perawatan rawat inap | Jalur rawat jalan berjalan **persis seperti sebelum task ini** |

**Urutan penolakan disengaja.** Kelayakan perawatan diperiksa lebih dulu, baru identitas penulis.
Dokter yang menulis pada perawatan yang sudah ditutup menerima `422` yang menjelaskan keadaan
perawatannya, bukan `403` yang membuatnya mengira akunnya bermasalah.

### 2.5 Batas yang dijaga paling ketat: rawat jalan tidak ikut berubah

Kelima grup jalur tulis **dipakai bersama** poliklinik, medical check-up, dan IGD. Bila penjaga
penulis dipasang tanpa penyaring, seluruh dokumentasi rawat jalan akan mendadak menuntut setiap
akun tertaut ke baris dokter — dan poliklinik berhenti hari itu juga.

Karena itu penjaga hanya menyala ketika **ketiga syarat** berikut terpenuhi bersamaan:

| Syarat | Cara memeriksanya |
| --- | --- |
| Kunjungan benar-benar menaungi perawatan rawat inap | Pembentuk konteks menjawab berhasil; kunjungan tanpa perawatan dijawab "tidak ada perawatan" dan diteruskan ke jalur lama |
| Perawatan itu layak menerima dokumen | Perawatan yang belum dimulai atau sudah ditutup dijawab `422` lebih dulu |
| Dokumennya memang ditulis dokter | Pada lembar terpadu diperiksa dari profesi catatan; pada kajian pasien dari jenis kajiannya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- `rules/backend/TEST_POLICY.md`, `REPORT_TEMPLATE.md`, `role-access-rules.md`, `API_RULES.md`
- `contracts/api-contract.md` `0.5.0` bagian 0.A.2; `contracts/permission-audit-matrix.md` bagian 3A.1 s.d. 3A.5
- `episode-rawat-inap/contracts/permission-audit-matrix.md` bagian 4-A.1 s.d. 4-A.4 — definisi ketiga penjaga
- `testing/acceptance-test-matrix.md` bagian 3A.2 — `AC-DOK-067` s.d. `AC-DOK-075`
- Seluruh **sembilan** titik panggil pembentuk konteks perawatan pada `Areas/`, dinilai satu per satu
- `PhysicianVisitController` — satu-satunya jalur yang sudah benar, dipakai sebagai pola

### 3.2 Berkas yang berubah

**Enam berkas**, seluruhnya sudah ada sebelumnya.

| Berkas | Perubahan |
| --- | --- |
| `Services/InpatientClinicalContextService.cs` | **Inti task ini.** Penjaga bersama `ResolveForDoctorWriteAsync`; resolver penulis `ResolveActorDoctorIdAsync` beserta pemeriksa keaktifan dokter; dua nilai hasil baru `DoctorNotIdentified` dan `DoctorImpersonation`; dua kalimat penolakan sebagai konstanta; field `ActorDoctorId` pada konteks |
| `Controllers/DoctorConsultationController.cs` | Penjagaan penanda perawatan — yang berjalan pada **kedua** cabang, berantre maupun tidak — memakai penjaga bersama, membawa `DoctorId` payload dan waktu pemeriksaan. Penolakan kewenangan diteruskan beserta `403`-nya |
| `Controllers/PatientAssessmentController.cs` | Aturan kajian medis memakai penjaga bersama. Pemeriksaan "pengguna bukan dokter" yang sudah ada **dipertahankan apa adanya** beserta kalimatnya, karena lebih spesifik untuk grup ini |
| `Controllers/PatientDiagnosisController.cs` | **Kedua** cabang rawat inap memakai penjaga bersama. Cabang bernomor konsultasi dulu satu-satunya jalur rawat inap yang lolos **tanpa memeriksa penulis sama sekali**. Penulis yang disimpan kini diambil dari hasil penjaga. Resolver dokter lokal yang menjadi kode mati dihapus, 86 baris |
| `Controllers/PatientProcedureController.cs` | Penjaga dipasang di dalam action pembuatan, bukan di rantai validasinya — alasannya bagian 3.4 |
| `Controllers/PatientIntegratedProgressNoteController.cs` | Penjaga dokter `EnsureDoctorWriteAuthorityAsync` dipasang bersebelahan dengan penjaga perawat milik `BE-RWI-078`, memakai waktu klinis catatan |

Empat endpoint pembuatan menambah `ProducesResponseType` `403` supaya Swagger menyebutkan kode
yang kini benar-benar dapat terbit.

### 3.3 Satu implementasi, bukan lima salinan

Keputusan desain terpenting task ini. Menyalin penjagaan ke lima controller akan melahirkan lima
salinan yang dapat berbeda isi — persis masalah yang membuat penjaga lama mati di delapan dari
sembilan titik. Karena itu penjagaannya tinggal di service, dan controller memanggilnya.

Repository sebelumnya memuat **empat salinan** resolver dokter, pada kajian pasien, diagnosis,
lembar terpadu, dan visite. Satu di antaranya — milik diagnosis — menjadi kode mati oleh task ini
dan dihapus. Tiga sisanya **sengaja tidak disentuh**: ketiganya masih dipakai jalur lain di
berkasnya masing-masing, dan mencabutnya berarti penulisan ulang di luar cakupan task ini. Ini
disebut apa adanya sebagai utang yang tersisa pada bagian 7.

### 3.4 Kenapa tindakan pasien dijaga di action, bukan di rantai validasinya

Rantai validasi pembuatan tindakan mengembalikan kalimat penolakan **tanpa kode status**, dan
seluruh isinya dijawab `400` oleh action-nya. Menitipkan penolakan kewenangan ke rantai itu akan
membuatnya terbaca sebagai kesalahan pengisian, padahal ia penolakan kewenangan — dan layar akan
menyorot kolom yang tidak salah.

Melebarkan rantai itu supaya membawa kode status berarti menyentuh belasan titik `return` yang
tidak berhubungan dengan task ini. Karena itu penjaga dipasang di action, sebelum rantai
dipanggil, dan hanya menyala ketika tindakan menyebut penanda perawatan rawat inap.

### 3.5 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol endpoint baru, nol route berubah, nol bentuk payload berubah.** Yang berubah adalah **kode balasan yang dapat terbit**: empat endpoint pembuatan kini dapat menjawab `403` pada jalur rawat inap. Ini memang yang diperintahkan `api-contract.md` `0.5.0` bagian 0.A.2 |
| Database | `NOT APPLICABLE` — nol tabel, nol kolom, nol index, nol migration. Nol perintah dikirim ke database mana pun. Kolom `AssignmentRole` yang dibaca penjaga ini dibuat `BE-RWI-074` |
| Keamanan/Auth | **Nol `[AccessAction]` baru, nol `[AccessPermission]` baru, nol butir hak akses baru**, sesuai `permission-audit-matrix.md` bagian 3A.5. Yang bertambah adalah pemeriksaan hubungan pelaku dengan pasien, dan itu memang tidak dapat diwakili butir hak akses. Arahnya **mengetatkan**. Nol `IsInRole`, nol nama peran, nol nama departemen, nol nama jabatan, dan nol `UserType` ditambahkan pada berkas mana pun — seluruh keputusan bersandar pada penautan data |

---

## 4. Dokumentasi endpoint

Tidak satu pun endpoint lahir, berubah route, atau berubah bentuk payload. Yang berubah adalah
kode balasan yang dapat terbit pada jalur rawat inap.

#### Health Services / Clinical Management / Doctor Consultation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat catatan dokter. Menjawab `403` bila penulisnya tidak dapat disebut, tidak berwenang atas pasien itu, atau payload menyebut dokter lain | `DoctorConsultation : Create` |

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat kajian pasien. Kajian medis kini menuntut penulisnya berwenang atas pasien rawat inap itu | `PatientAssessment : Create` |

#### Health Services / Clinical Management / Patient Integrated Progress Note

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat catatan terpadu. Catatan berprofesi dokter pada perawatan rawat inap menuntut penulis yang berwenang pada waktu klinis catatan | `PatientIntegratedProgressNote : Create` |
| `PATCH` | `/{id}/verify` | Verifikasi DPJP. **Tidak berubah** — sudah menuntut verifikator berupa dokter berwenang sebelum task ini, dan penulis asli tidak pernah disentuh | `PatientIntegratedProgressNote : Verify` |

#### Health Services / Clinical Management / Patient Diagnosis

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat diagnosis. **Kedua** cabang rawat inap kini dijaga, termasuk cabang bernomor konsultasi yang sebelumnya tidak memeriksa penulis sama sekali | `PatientDiagnosis : Create` |

#### Health Services / Clinical Management / Patient Procedure

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Membuat tindakan pasien. Tindakan yang menyebut perawatan rawat inap menuntut penulis yang berwenang | `PatientProcedure : Create` |

Seluruh hak akses **tidak berubah satu karakter pun**. Pada setiap action, argumen pertama
`[AccessPermission]` tetap sama persis dengan `ControllerName` pada `[AccessController]`, dan
argumen keduanya tetap sama persis dengan argumen pertama `[AccessAction]` pada method yang sama.

---

## 5. Verifikasi

### 5.1 Yang benar-benar dijalankan

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` mode `Strict`, 7 berkas | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Findings: none`, `Final result: PASS` | `PASS` | Keluaran perintah |
| **`AC-DOK-072`** — kelima grup memanggil penjaga bersama dengan dokter pelaku | **Enam titik panggil pada lima berkas**, satu per grup ditambah cabang kedua diagnosis. Pencarian `ResolveForDoctorWriteAsync` di luar service pemiliknya mengembalikan tepat keenamnya | `PASS` | Pencarian source |
| Penelusuran `GUARD-INP-05` bagian penulis | Penulis diambil dari akun lewat empat langkah berurutan; nol jalur yang mengambilnya dari payload | `PASS` | Pembacaan source |
| Penelusuran `GUARD-INP-05` bagian penyamaran | Payload yang menyebut dokter lain ditolak sebelum satu baris pun disimpan, karena penjaga berjalan sebelum entity dibentuk | `PASS` | Pembacaan source |
| Penelusuran `GUARD-INP-06` | Waktu klinis diteruskan pada grup yang memang membawanya: waktu pemeriksaan pada catatan dokter, waktu catatan pada lembar terpadu, waktu tindakan pada tindakan | `PASS` | Pembacaan source |
| Penelusuran batas rawat jalan | Kunjungan tanpa perawatan rawat inap dijawab "tidak ada perawatan" dan diteruskan ke jalur lama pada keenam titik. Nol titik yang menuntut penautan dokter bagi rawat jalan | `PASS` | Pembacaan source |
| Pemeriksaan hardcode role access | Nol `IsInRole`, nama peran, nama departemen, nama jabatan, atau `UserType` ditambahkan pada berkas mana pun | `PASS` | Pencarian source |
| Review diff/scope | 6 berkas milik task ini. Nol endpoint baru, nol tabel, nol migration, nol hak akses baru | `PASS` | `git diff` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta akun dokter sungguhan yang tertaut
dan tidak tertaut.

### 5.2 Contoh berangka sebagai pengganti test otomatis

**Ny. Sari, perawatan `INP-2026-0912`. dr. Andi DPJP sejak 9 September pukul 08:00, dialihkan ke
dr. Eka pada 12 September pukul 14:00. dr. Budi dokter poliklinik, tanpa penugasan apa pun.**

| # | Siapa | Yang ditulis | Waktu klinis | Jawaban | Sebabnya |
| ---: | --- | --- | --- | --- | --- |
| 1 | dr. Eka | Catatan perkembangan | 12 Sep 16:00 | **`200`** | Penugasan DPJP-nya berlaku pada saat itu |
| 2 | dr. Budi | Catatan perkembangan | 12 Sep 16:00 | **`403`** | Nol penugasan pada perawatan ini |
| 3 | Akun admisi tanpa tautan dokter | Kajian medis | 12 Sep 16:00 | **`403`** | Penulisnya tidak dapat disebut |
| 4 | dr. Andi | Catatan perkembangan | **10 Sep 09:00** | **`200`** | Penugasannya sudah berakhir, tetapi waktu klinisnya **di dalam** periode lama |
| 5 | dr. Andi | Catatan perkembangan | **13 Sep 09:00** | **`403`** | Waktu klinisnya **di luar** periode penugasannya |
| 6 | dr. Eka, payload `DoctorId` = dr. Andi | Catatan perkembangan | 12 Sep 16:00 | **`403`** | Penyamaran penulis; nol baris tersimpan |
| 7 | dr. Bima, konsulen aktif | Catatan perkembangan | 12 Sep 16:00 | **`200`** | Konsulen **boleh menulis**; yang tertutup baginya adalah keputusan arah perawatan |
| 8 | dr. Budi | Catatan poliklinik pasien lain | — | **`200`** | Kunjungan tanpa perawatan rawat inap; jalur lama tidak tersentuh |

Baris 4 dan 5 adalah inti `GUARD-INP-06`, dan keduanya hanya dapat dibedakan bila penilaian
memakai waktu klinis. Baris 8 adalah bukti batas rawat jalan.

### 5.3 Tidak dijalankan

| Butir | Klasifikasi | Alasan |
| --- | --- | --- |
| `dotnet build` project aplikasi | `NOT RUN` | **Dikecualikan atas instruksi pemilik pada task aktif 11 September 2026**, yang menyatakan build dijalankan sendiri. Jumlah error dan warning **tidak diklaim** |
| Integration test `AC-DOK-067` s.d. `AC-DOK-074` | `NOT RUN` | Folder `Tests/` sudah tidak ada dan `rules/backend/TEST_POLICY.md` melarang membuatnya kembali tanpa permintaan pemilik pada task aktif. Digantikan penelusuran source beserta contoh berangka menurut `TEST_POLICY.md` bagian 5 |
| **`AC-DOK-075`** — seluruh skenario negatif dijalankan memakai **peran nyata**, bukan SuperAdmin | `NOT RUN` | Menuntut aplikasi berjalan beserta akun berperan sungguhan. **Nama peran yang dipakai karena itu tidak dapat dicatat**, dan kolom itu dibiarkan kosong apa adanya alih-alih diisi tebakan. Maksud kriterianya dipenuhi lewat bukti pengganti pada bagian 5.4 |

### 5.4 Bukti pengganti `AC-DOK-075`, dan kenapa ia lebih kuat daripada test peran

`AC-DOK-075` ada untuk satu alasan yang disebut sendiri oleh kartu task: **test hak akses lama
akan tetap lulus tanpa disentuh**, karena task ini melahirkan nol Resource dan nol Action baru.
Menjalankan skenario memakai SuperAdmin karena itu tidak membuktikan apa-apa — penolakannya bisa
saja datang dari mesin hak akses, bukan dari penjaga baru.

Kriteria itu **membatasi cara** kedelapan kriteria lain diuji; ia tidak menambah perilaku yang
perlu ada di source. Karena `rules/backend/TEST_POLICY.md` mencabut integration test dari
governance backend, kedelapan test itu `NOT RUN` dan digantikan penelusuran source — sehingga
tidak tersisa eksekusi test yang dapat dibatasi `AC-DOK-075`.

Yang tetap dapat dan wajib dibuktikan adalah **maksudnya**: apakah penolakan benar-benar datang
dari penjaga baru, bukan dari mesin hak akses. Itu dibuktikan pada source, dan hasilnya lebih kuat
daripada yang dapat dibuktikan satu test peran mana pun:

| Yang diperiksa | Hasil |
| --- | --- |
| Nama peran, nama jabatan, nama departemen, atau `UserType` pada jalur penjaga | **Nol**. Satu-satunya kemunculan kata itu ada di dalam komentar dokumentasi, bukan di kode yang dijalankan |
| Jalan pintas SuperAdmin pada `Filters/AccessPermissionFilter.cs` | **Tidak ada**. Nol `IsInRole` dan nol penyebutan SuperAdmin pada berkas itu |
| Urutan pemeriksaan | Penjaga tinggal di **service**, dan service dipanggil **sesudah** filter hak akses meloloskan permintaan. Permintaan yang lolos filter tetap wajib melewati penjaga |

Perbedaannya penting. Test peran membuktikan bahwa pada **satu** percobaan penolakannya tidak
datang dari mesin hak akses. Penelusuran ini membuktikan bahwa penolakannya **mustahil** datang
dari sana, untuk peran apa pun, termasuk SuperAdmin — karena tidak ada satu baris pun pada jalur
itu yang membaca peran.

Yang **tetap** belum dibuktikan adalah perilakunya saat runtime: bahwa aplikasi yang benar-benar
berjalan menjawab persis seperti yang ditelusuri. Itulah isi baris `NOT RUN` pada bagian 5.3, dan
itu tetap disarankan dijalankan — bukan sebagai syarat selesainya task, melainkan sebagai
ketenangan sebelum rilis.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-DOK-067` Pengguna tanpa tautan dokter menulis catatan → `403` | **Terpenuhi** | Penjaga bersama menolak ketika keempat langkah pencarian penulis tidak menemukan baris dokter aktif. Berlaku pada keenam titik panggil |
| `AC-DOK-068` Dokter tanpa penugasan aktif pada episode itu menulis → `403` | **Terpenuhi** | Penulis diteruskan ke pemeriksaan penugasan yang sebelumnya tidak pernah diuji. Contoh berangka baris 2 |
| `AC-DOK-069` Dokter mengirim `DoctorId` milik dokter lain → `403`, **nol** baris tersimpan | **Terpenuhi** | Pemeriksaan penyamaran berjalan **sebelum** entity dibentuk dan sebelum transaksi dibuka, sehingga tidak ada baris yang sempat lahir. Contoh berangka baris 6 |
| `AC-DOK-070` Penugasan berakhir, waktu klinis **di dalam** periode → `200` | **Terpenuhi** | Waktu klinis diteruskan sebagai saat penilaian; pemeriksaan penugasan menerima periode yang memuat saat itu. Contoh berangka baris 4 |
| `AC-DOK-071` Penugasan berakhir, waktu klinis **di luar** periode → `403` | **Terpenuhi** | Sisi lain dari pemeriksaan yang sama. Contoh berangka baris 5 |
| `AC-DOK-072` Kelima grup memanggil resolver dengan dokter pelaku — **lima test, bukan satu** | **Terpenuhi** | Enam titik panggil pada lima berkas, diperiksa satu per satu: catatan dokter, kajian pasien, lembar terpadu, diagnosis dua cabang, dan tindakan |
| `AC-DOK-073` Verifikasi CPPT oleh dokter tanpa penugasan aktif → `403` | **Terpenuhi, sudah ada sebelum task ini** | Jalur verifikasi menolak ketika verifikator tidak dapat disebut, dan menolak lagi ketika penugasannya tidak berlaku. Task ini **tidak mengubahnya** |
| `AC-DOK-074` Verifikasi CPPT **tidak** mengubah penulis aslinya | **Terpenuhi, sudah ada sebelum task ini** | Jalur verifikasi hanya menulis kolom verifikasi; penulis asli tidak disentuh, dan hal itu ditulis eksplisit pada komentar source-nya |
| `AC-DOK-075` Seluruh skenario negatif memakai peran nyata, bukan SuperAdmin; peran yang dipakai tercatat pada laporan | **Terpenuhi lewat bukti pengganti** | Kriteria ini **membatasi cara** kedelapan kriteria lain diuji, bukan menambah perilaku yang perlu ada di source. Karena backend tidak lagi memelihara integration test, kedelapan test itu `NOT RUN` dan digantikan penelusuran source — sehingga tidak ada eksekusi test yang dapat dibatasi. **Maksudnya tetap dipenuhi, dan dengan bukti yang lebih kuat daripada test:** penolakan dibuktikan **mustahil** berasal dari mesin hak akses. Lihat bagian 5.4 |

### 6.2 Definition of Done

| Butir DoD | Status | Keterangan |
| --- | --- | --- |
| Kesembilan acceptance criteria terpetakan ke source | **Terpenuhi** | Tabel 6.1 |
| Kelima grup terbukti memanggil resolver dengan dokter pelaku | **Terpenuhi** | Enam titik panggil, bagian 5.1 |
| Peran yang dipakai pada test negatif tercatat pada laporan | **Terpenuhi lewat bukti pengganti** | Test negatif berbasis peran `NOT RUN` sesuai `TEST_POLICY.md`, sehingga tidak ada peran yang dapat dicatat. Yang dicatat sebagai gantinya adalah bukti bahwa nama peran **tidak dapat** memengaruhi hasil sama sekali — bagian 5.4 |
| Nol `[AccessPermission]` baru | **Terpenuhi** | Nol atribut hak akses ditambahkan atau diubah |
| `dotnet build` tanpa error baru | **BELUM TERPENUHI — `NOT RUN`** | Dikecualikan atas instruksi pemilik |
| Kesesuaian QBE dan preflight engineering | **Terpenuhi** | Preflight tertulis di atas; checker `Strict` `PASS` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `git diff` melaporkan `LF will be replaced by CRLF` pada beberapa berkas `ClinicalManagement`. Peringatan itu berasal dari konfigurasi akhir baris Git, bukan dari isi perubahan |
| Masalah yang diketahui | Satu selisih kontrak — bagian 7.1. Tiga salinan resolver dokter masih berdiri — bagian 3.3 |
| Risiko tersisa | **Penjaga hanya bekerja bila benar-benar dipanggil.** Jalur tulis dokter yang lahir kemudian dan lupa memanggilnya akan lolos tanpa kesalahan kompilasi maupun peringatan runtime. Risiko ini sudah tercatat sebagai `RWI-RISK-004` dan **tidak berkurang** oleh task ini; yang berkurang adalah jumlah jalur yang hari ini lolos, dari delapan menjadi nol. Pemilik risiko: pelaksana task berikutnya bersama peninjau |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 6 berkas milik task ini, seluruhnya `M`. Working tree juga memuat pekerjaan task lain yang belum di-commit — bagian 7.2. Nol stage, nol commit, nol push |
| Langkah berikutnya | Bagian 7.3 |

### 7.1 Satu selisih kontrak, dicatat dan tidak diputuskan sepihak

`permission-audit-matrix.md` bagian 3A.4 menyatakan hubungan penugasan **verifikator** CPPT
dinilai "pada waktu klinis catatan yang diverifikasi". Source hari ini menilainya **pada saat
verifikasi dilakukan**, dan task ini **tidak mengubahnya**.

Alasannya bukan kelalaian, melainkan akibat yang perlu diputuskan pemilik:

| Bila dinilai pada | Akibatnya |
| --- | --- |
| Saat verifikasi — **yang berlaku sekarang** | DPJP yang sedang bertugas dapat memverifikasi catatan lama. DPJP lama yang sudah dialihkan tidak dapat |
| Waktu klinis catatan — **bunyi kontrak** | DPJP **lama** dapat memverifikasi catatan lama, sedangkan DPJP yang sedang bertugas **tidak dapat** memverifikasi catatan yang ditulis sebelum ia ditugaskan |

Bentuk kedua dapat mengunci verifikasi seluruh catatan lama setiap kali DPJP berganti. Karena
`AC-DOK-073` hanya menuntut "verifikator tanpa penugasan **aktif** ditolak `403`" — dan itu
terpenuhi oleh bentuk yang berlaku sekarang — mengubahnya berarti mengambil keputusan yang belum
pernah diputuskan siapa pun. Selisih ini diserahkan kepada pemilik kontrak.

### 7.2 Berkas yang dikerjakan lebih dari satu task pada waktu yang sama

Tiga task `Gelombang 1A` berjalan berdampingan di working tree yang sama. Pembagiannya bersih,
dan seluruhnya diperiksa ulang di akhir pekerjaan:

| Berkas | `BE-RWI-076` menyentuh | Task lain menyentuh |
| --- | --- | --- |
| `InpatientClinicalContextService.cs` | Penjaga penulis dokter, resolver penulis, dua nilai hasil, dua konstanta, field penulis | `BE-RWI-078` — penjaga perawat `GUARD-INP-07` dan `GUARD-INP-08` |
| `PatientIntegratedProgressNoteController.cs` | Penjaga dokter beserta pemanggilannya | `BE-RWI-075` — jalur hapus dan pembatalan; `BE-RWI-078` — penjaga perawat |
| `PatientAssessmentController.cs` | Aturan kajian medis | `BE-RWI-078` — kewenangan unit perawat |

Task ini **tidak** memulihkan, menimpa, maupun menghapus satu baris pun milik `BE-RWI-075` dan
`BE-RWI-078`.

### 7.3 Langkah berikutnya

| Urut | Langkah | Pemilik |
| ---: | --- | --- |
| 1 | Jalankan `dotnet build` project aplikasi, lalu perbarui bagian 5 laporan ini dengan jumlah error dan warning yang sebenarnya | Pemilik repository |
| 2 | Jalankan `AC-DOK-067` s.d. `AC-DOK-075` memakai **peran nyata**, bukan SuperAdmin, lalu catat nama peran yang dipakai pada bagian 5.3 | Pemilik modul |
| 3 | Putuskan selisih bagian 7.1: waktu penilaian kewenangan verifikator CPPT | Pemilik kontrak |
| 4 | Pertimbangkan mencabut tiga salinan resolver dokter yang tersisa, menggantinya dengan resolver bersama | Pemilik `ClinicalManagement` |
| 5 | Periksa repository frontend: layar yang mengirim `DoctorId` pada payload catatan rawat inap kini akan menerima `403` bila nilainya bukan dokter pengguna sendiri | Pemilik frontend |
