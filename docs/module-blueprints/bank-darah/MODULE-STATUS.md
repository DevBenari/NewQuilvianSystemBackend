# Bank Darah — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Module name | `Bank Darah` |
| Module slug | `bank-darah` |
| Revision | `26` |
| Module status | `IN_PROGRESS` |
| Current phase | `BD-PH-007` |
| Last verified at | `2026-09-04` — hasil **`NOT_READY`** (modul); gelombang `MVP-0` **`READY_WITH_CONDITIONS`** |
| Backend source SHA | **`d07dcf3`** cabang `sukmagp` — naik dari `7fca34c` pada 11 September 2026 lewat `14f3778` (docs), `8e30aa9` (implementasi `BE-BD-003`), dan `d07dcf3` (docs verifikasi ulang). Impact scan terbatas: 21 berkas di luar `docs/`, seluruhnya milik Bank Darah ditambah `Program.cs` dan `ApplicationDbContext.cs` yang aditif murni serta snapshot bangkitan. **Nol berkas bukti peta kemampuan tersentuh, nol baris berpindah status**. **Riwayat:** **`7fca34c`** cabang `sukmagp` — naik dari `23fb65a` pada 10 September 2026; seluruh commit sesudahnya hanya menyentuh `docs/`, sehingga tidak ada impact scan. **Riwayat:** **`23fb65a`** cabang `sukmagp` — naik dari `95e4b8d` pada 10 September 2026 lewat dua commit: `c606baf` dokumentasi Bank Darah dan `23fb65a` milik `PLT-BE-004`. Di luar `docs/` hanya dua berkas infrastruktur uji Postgres berubah — `BillingTestDatabaseFixture.cs` dan README-nya — **nol source aplikasi**, sehingga peta kemampuan tetap `CURRENT` tanpa impact scan. **Riwayat:** **`95e4b8d`** cabang `sukmagp` — naik dari `5360286` pada 10 September 2026. Impact scan terbatas dijalankan: 8 commit, 34 berkas source, **14 milik Bank Darah sendiri** (`BE-BD-005`/`BE-BD-011` beserta migration `AddBbkBloodGroupExam` yang kini ter-commit). **Nol baris peta kemampuan berpindah status, nol memburuk** |
| Frontend source SHA | **`f79af16847c99961842081f707bc0c4ff6c2d93b`** cabang `sukmagpV2` — naik dari `101ec5d3a` pada 10 September 2026 lewat dua commit milik `FE-BD-001` dan `FE-BD-011`. Impact scan terbatas dijalankan 10 September 2026: 44 berkas berubah, **42 milik Bank Darah sendiri** (`FE-BD-001` dan `FE-BD-011`); dua sisanya `store.jsx` dan `menu-items.jsx`, yaitu titik registrasi wajib yang memang disentuh task Bank Darah. **Nol dampak asing, nol baris kemampuan berpindah status**. Pekerjaan `FE-BD-006` (satu berkas `menu-items.jsx`) **belum di-commit** |
| Decision revision | `12` — `DEC-BD-001` sampai `DEC-BD-049`. `DEC-BD-048`/`049` dinyatakan pemilik `Sukmagp` 11 September 2026 |
| Domain architecture | revisi `6` — `DOMAIN_ARCHITECTURE_READY` |
| Contract version | `v4` (**`approved`**) — `Sukmagp` / `2026-09-03` |
| Roadmap | Backend revisi **`8`** — acceptance criteria `BE-BD-012` diganti `AC-BD-098`..`102` dan `AC-BD-026/058` dipindah ke `BE-BD-013` atas keputusan `Sukmagp` 2026-09-11. **Riwayat:** revisi `7` — **`APPROVED`** oleh `Sukmagp` 2026-09-10. Frontend revisi `7` masih `FORWARD-TEST / DRAFT`. **Riwayat:** revisi `2` `APPROVED` 2026-09-03 |
| Terakhir diperbarui | **`2026-09-11`** — **`BE-BD-012` selesai**: tindakan Bank Darah tercatat dengan tarif dari data induk dan salinan beku, tanpa jalur Billing; migration `AddBbkBloodBankProcedure` diterapkan ke `QuilvianNewDevSukma` (`140/140`, nol tertunda); 600 + 231 test dan 13 uji PostgreSQL lulus; kelima kriteria terbukti ([laporan](task/report/backend/BE-BD-012.md)). Sebelumnya pada hari yang sama **roadmap revisi 8**: keputusan `DEC-BD-048`/`049` dan kriteria `AC-BD-098`..`102` dicatat; `BE-BD-012` 🟡 siap dijadwalkan kembali. Sebelumnya pada hari yang sama **`BE-BD-012` ⛔ BLOCKED** sebelum implementasi: aturan pemilihan tarif, sumber unit dan kelas, dan rumah `AC-BD-026`/`058` belum diputuskan; nol source ditulis ([laporan](task/report/backend/BE-BD-012.md)). Sebelumnya pada hari yang sama **`BE-BD-004` selesai sebagian**: permintaan PMI, penerimaan, dan kantong `Received`; migration `AddBbkProviderRequestAndBloodUnit` diterapkan ke `QuilvianNewDevSukma` (`139/139`, nol tertunda); 561 + 231 test dan 9 uji PostgreSQL lulus; 6 dari 9 kriteria. Sebelumnya pada hari yang sama: penyegaran metadata sesudah `BE-BD-003`: SHA backend `d07dcf3`, bukti build di `8e30aa9` (verifikasi ulang: build `0 Error(s)`, 498 + 231 test, 4 uji PostgreSQL), suite skill `1.18.0`. Revisi tetap `25`. Sebelumnya pada hari yang sama **`BE-BD-003` selesai**: order darah, migration `AddBbkBloodOrder` diterapkan ke `QuilvianNewDevSukma` (`138/138`, nol tertunda), 498 test dan 4 uji PostgreSQL lulus. Sebelumnya **`2026-09-10`** — **gerbang `G4` tertutup** atas pernyataan `Andry`, dan roadmap backend revisi 7 disetujui `Sukmagp`. Sebelumnya pada hari yang sama: migration diterapkan ke `QuilvianNewDevSukma` (`137/137`, nol tertunda), `FE-BD-011` selesai sebagian, dan SHA disegarkan ke `95e4b8d`. Sebelumnya `2026-09-07`: penyegaran SHA dan bukti; peta kemampuan naik ke revisi **5** |

## Keadaan sekarang — 11 September 2026: `BE-BD-012` selesai

Modul tetap **`IN_PROGRESS`**. Bank Darah kini mencatat **tindakan yang dikerjakannya** beserta
tarifnya. Contoh: uji silang serasi untuk pasien kelas VIP tercatat dengan tarif VIP Rp250.000 yang
dipilih backend dari data induk; bila tarif VIP dinaikkan bulan depan, catatan ini tetap Rp250.000.
Belum ada satu rupiah pun yang dikirim ke Billing — itu tetap milik `BE-BD-013`.

| Yang terjadi | Bukti |
| --- | --- |
| Tindakan tercatat bernomor `TND-…`, unit dan kelas dari kunjungan, tarif dipilih backend, salinan beku, penyelesaian beraudit | [laporan](task/report/backend/BE-BD-012.md) |
| Build dan test | Build `0 Error(s)` dengan `-p:RunAnalyzers=false`; 600 test `QuilvianSystemBackend.Tests` (39 tindakan), 231 test Sqlite, dan 13 uji PostgreSQL lulus. `UnitTests.InMemory` 896/905 — 9 kegagalan Billing baseline, di luar task |
| Migration | `20260911072451_AddBbkBloodBankProcedure` diterapkan ke `QuilvianNewDevSukma` — `140/140`, nol tertunda, `has-pending-model-changes` bersih |
| Delta kontrak | `VAL-BD-084` (tarif tidak tersedia, `422`), `Scope` riwayat `BloodBankProcedure`, klarifikasi urutan `DEC-BD-049` |
| Menunggu konfirmasi pemilik | Kunjungan tanpa kelas pasien ditolak `422`; "order sah" tidak dibatasi status bisnis order. Keduanya tidak menahan kriteria |
| Task yang terbuka | Backend: **nihil** sampai pemilik roadmap memutuskan penerusan tiga kriteria `BE-BD-004`. Frontend: `FE-BD-002`, `FE-BD-009`; `FE-BD-010` kehilangan penahan backend-nya |

## Riwayat — 11 September 2026: `BE-BD-004` selesai sebagian

Modul tetap **`IN_PROGRESS`**. Bank Darah kini dapat mencatat **dari mana darahnya datang**:
permintaan ke PMI, setiap kiriman yang diterima fisik — termasuk yang berlebih — dan kantong yang
lahir berstatus `Received`.

| Yang terjadi | Bukti |
| --- | --- |
| Permintaan PMI, penerimaan termasuk kelebihan per komponen, kantong `Received`, pembatalan beralasan | [laporan](../task/report/backend/BE-BD-004.md) |
| Build dan test | Build solution `0 Error(s)`, `210 Warning(s)` sama dengan baseline; 561 test `QuilvianSystemBackend.Tests` (63 permintaan PMI), 231 test Sqlite, dan 9 uji PostgreSQL lulus |
| Migration | `20260911032311_AddBbkProviderRequestAndBloodUnit` diterapkan ke `QuilvianNewDevSukma` — `139/139`, nol tertunda, `has-pending-model-changes` bersih |
| Kenapa 🟡, bukan ✅ | `AC-BD-023`/`032` menuntut kantong `PendingReview` yang lahir sesudah penyimpanan (`BE-BD-015`), dan `AC-BD-033` menuntut alokasi (`BE-BD-006`). Nol pekerjaan tersisa di scope `BE-BD-004` |
| Task yang terbuka | Backend: **`BE-BD-012`** sejak roadmap revisi 8. **Riwayat:** backend nihil sejak `BE-BD-012` ⛔ pada hari yang sama. Frontend: `FE-BD-002`. `BE-BD-015` terbuka begitu pemilik roadmap meneruskan ketiga kriteria di atas. **Riwayat:** `BE-BD-012` dan `FE-BD-002` |

## Riwayat — 11 September 2026: `BE-BD-003` selesai

Modul tetap **`IN_PROGRESS`**. Order darah — pintu masuk seluruh alur Bank Darah — kini ada, terbukti, dan tabelnya sudah ada di database pengembangan personal.

| Yang terjadi | Bukti |
| --- | --- |
| Order darah elektronik dan manual, deteksi ganda, pembatalan dua peran | [laporan](../task/report/backend/BE-BD-003.md) |
| Build dan test | Build solution `0 Error(s)`, `210 Warning(s)` sama dengan baseline; 498 test `QuilvianSystemBackend.Tests` lulus, 79 di antaranya order darah; 4 uji PostgreSQL lulus |
| Migration | `20260910153119_AddBbkBloodOrder` diterapkan ke `QuilvianNewDevSukma` — `138/138`, nol tertunda, `has-pending-model-changes` bersih |
| Task yang terbuka | `BE-BD-004`, `BE-BD-012`, dan `FE-BD-002` |

## Riwayat — 10 September 2026: `G4` tertutup

Modul tetap **`IN_PROGRESS`**. Yang berubah hari ini adalah satu gerbang: **`G4`** — mesin pemberi
nomor bisnis yang dapat dipakai Bank Darah — **tertutup**. Gerbang itu menahan sembilan task backend
dan delapan task frontend sejak revisi 3 roadmap.

| Yang terjadi | Bukti |
| --- | --- |
| Mesin pemberi nomor bersama berdiri | `PLT-BE-003` — `NumberSeriesAllocator`, 9 September 2026 |
| Mesin itu terbukti andal di PostgreSQL | `PLT-BE-004` — 6 dari 6 uji lulus di `QuilvianNewDevSukma`, commit `23fb65a`. Dijalankan di database personal atas keputusan `RJ-BIL-DEC-019` |
| Pemilik gerbang menyatakan tertutup | `Andry`, disampaikan `Sukmagp` pada 10 September 2026 |
| Roadmap backend revisi 7 disetujui | `Sukmagp`, 10 September 2026 |

**Contoh supaya jelas.** Setiap order darah wajib punya nomor yang tidak pernah kembar. Sebelum hari
ini Bank Darah belum punya mesin pemberi nomor yang sah. Sekarang punya, dan sudah terbukti dua hal:
dua puluh permintaan nomor serentak menghasilkan dua puluh nomor berbeda, dan nomor dari order yang
batal tidak diterbitkan lagi.

Status task yang berpindah:

| Task | Sebelum | Sesudah |
| --- | --- | --- |
| `BE-BD-003` order darah | ⛔ tertahan `G4` | 🟡 **siap dijadwalkan** |
| `BE-BD-004`, `BE-BD-012` | ⛔ tertahan `G4` dan `BE-BD-003` | ⛔ tertahan `BE-BD-003` saja |
| `BE-BD-015`, `006`, `007`, `008`, `009`, `010` | ⛔ lewat rantai dependency | ⛔ lewat rantai dependency — tidak berubah |
| Delapan task frontend bertanda ⛔ | ⛔ | ⛔ — kini menunggu pasangan backend-nya, bukan gerbang |

**Task backend berikutnya diverifikasi ulang 10 September 2026: `BE-BD-003`.** Verifikasi ini
memeriksa bukti, bukan hanya membaca roadmap:

| Pemeriksaan | Hasil |
| --- | --- |
| Dependency `G1`, `G2b`, `G4` | Ketiganya ✅ tertutup |
| Dependency `BE-BD-001`, `BE-BD-002` | Laporan keduanya berstatus `SELESAI` |
| Provider nomor di source | `NumberSeriesAllocator.AllocateAsync` ada di `Areas/Platform/NumberSeriesManagement/Services/` |
| Task sudah dimulai? | **Belum** — nol class `BbkBloodOrder` maupun `BbkBloodOrderLine` di source |
| Kandidat lain | `BE-BD-005` dan `BE-BD-011` sudah ✅ sejak 9 September 2026. Sisa `BE-BD-016` hanya dapat lahir bersama controller pemakainya. Tujuh task backend lain masih ⛔ lewat rantai yang berawal dari `BE-BD-003` |

**Batas yang jujur.**

- Persetujuan `Andry` dicatat berdasarkan keterangan `Sukmagp`. Tidak ada dokumen persetujuan tertulis
  yang dilampirkan.
- Keandalan mesin nomor dibuktikan di **satu** database. Tabel `NumNumberSeries` ada di
  `QuilvianNewDevSukma`; `QuilvianNewDevTim01`, staging, dan production belum.
- Approval roadmap hari ini hanya menjangkau roadmap **backend**. Roadmap frontend revisi 7 masih
  `FORWARD-TEST / DRAFT`.
- Revisi blueprint naik ke 25 karena satu dependency — `BD-DEP-017` — berpindah status. Set kontrak
  **tetap `v4` `approved`** dan tidak tersentuh.

---

## Catatan historis — 7 September 2026

> Blok di bawah benar pada tanggalnya dan dipertahankan sebagai rekaman. Keadaan terkini ada di atas.

Modul tetap berstatus **`IN_PROGRESS`**, dan **tidak ada satu pun status yang berubah** dibanding
4 September 2026. Yang dikerjakan hari ini adalah penyegaran bukti, bukan perubahan rencana.

**Pemicunya.** Backend bergerak dari `ba75a05` ke **`5360286`** lewat 38 commit yang membawa
**121 berkas source aplikasi** — berbeda dari catatan sebelumnya yang menyebut pergerakan sesudah
`5f7acaf` sebagai *docs-only*, yaitu hanya menyentuh dokumen. Karena itu impact scan terbatas
dijalankan ulang.

### Hasil impact scan 7 September 2026

| Pemeriksaan | Hasil |
| --- | --- |
| Baris kemampuan berpindah status | **0** |
| Baris kemampuan memburuk | **0** |
| Berkas source Bank Darah tersentuh | **0** dari 205 berkas yang berubah |
| Rujukan bukti yang perlu diperbarui | **2** — `BD-CAP-008` dan `BD-CAP-009` |
| Bukti frontend | **Tetap sahih** tanpa scan; SHA frontend tidak bergerak |

**Bukti `MVP-0` selamat penuh.** Ketujuh berkas source dan keempat migration hasil gelombang `MVP-0`
utuh di `5360286`. Dibuktikan langsung, bukan disimpulkan:

> **Diperbarui 10 September 2026.** Migration Bank Darah kini **lima**, bukan empat —
> `AddBbkBloodGroupExam` lahir 9 September 2026 dari `BE-BD-005`/`BE-BD-011`. Kelimanya
> **sudah diterapkan** di `QuilvianNewDevSukma`; lihat bagian *Migration* di bawah.

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build QuilvianSystemBackend.sln` | **`0 Error(s)`**, 210 peringatan |
| `dotnet test` penyaring Bank Darah | **`Failed: 0, Passed: 101`** |
| `git status --porcelain` | Bersih |

### Migration — diterapkan 10 September 2026

Perintah `dotnet ef database update` dijalankan atas wewenang eksplisit pemilik pekerjaan dengan
target yang disebut namanya: **`QuilvianNewDevSukma`**. Hasilnya **`137/137` migration diterapkan,
nol tertunda**.

| Sebelum | Sesudah |
| ---: | ---: |
| 126 diterapkan, 11 tertunda | **137 diterapkan, 0 tertunda** |

**Empat dari lima migration Bank Darah ternyata sudah lebih dulu ada di sana** — termasuk
`AddMstBloodStorageLocation`, sehingga layar `FE-BD-011` sebenarnya sudah dapat dipakai sebelum
perintah ini dijalankan. Yang benar-benar baru diterapkan dari sisi Bank Darah hanya
`AddBbkBloodGroupExam`.

**Koordinasi lintas modul yang diramalkan 7 September 2026 memang terjadi.** Sebelas migration
milik lima modul ikut diterapkan sekaligus, dan salah satunya **merusak**:

| Modul | Migration | Catatan |
| --- | --- | --- |
| Billing | `DropTableMstBillingCategory` | ⚠️ **`MstBillingItemCategory` dihapus** beserta isinya |
| Billing | `AddTariffIdToBilInvoiceItem` | Kolom baru |
| Insurance | `FixTariffCategoryInsuranceCoverageDefault` | Perbaikan default |
| Laboratorium | `AddLabExamination`, `RenameLaboratoryTrxTablesToLabPrefix`, `SplitLabSpecimenIntoExamination`, `AddLabExaminationIdToLabTransitionHistory`, `AddLabDisciplineAndReferralMasterData` | ⚠️ Dua di antaranya mengganti nama dan memecah tabel |
| Registration | `AddReferralPointerToPatientEncounter` | Kolom baru |
| **Bank Darah** | `AddBbkBloodGroupExam` | Tabel pemeriksaan golongan darah |
| Platform | `AddNumNumberSeries` | Tabel pencacah deret nomor |

**Batas yang jujur.** Baru **satu** database yang diterapkan. `QuilvianNewDevTim01`, staging, dan
production **belum**, dan masing-masing menuntut wewenang tersendiri. Pernyataan "migration sudah
dijalankan" tanpa menyebut nama database adalah pernyataan yang menyesatkan.

### Tiga hal yang berubah artinya, tanpa mengubah status

**1. Dua rujukan bukti menunjuk berkas yang sudah berganti nama.** Modul Laboratorium mengganti nama
dan memecah dua entity yang dipinjam peta kemampuan **sebagai pola**, bukan dipakai bersama:

| Kemampuan | Rujukan lama | Rujukan baru |
| --- | --- | --- |
| `BD-CAP-009` | `TrxLabTransitionHistory.cs` | `LabTransitionHistory.cs` — ganti nama saja, kesembilan field utuh |
| `BD-CAP-008` | `TrxLabSpecimen.cs` | `LabSpecimen.cs` + `LabExamination.cs` — dipecah dua tingkat |

Keduanya **tetap `Reuse with adapter`**, dan rancangan Bank Darah tidak perlu diubah. Rujukannya sudah
diperbarui di peta kemampuan revisi 5.

**2. Eksekusi migration kini lintas modul.** Keempat migration Bank Darah bukan lagi migration
terakhir, dan `20260903071535_AddLabExamination` milik Laboratorium **menyelip di tengahnya**. Karena
Entity Framework menerapkan migration berurutan dan tidak boleh dilangkahi, menjalankan migration
Bank Darah otomatis ikut menerapkan migration Laboratorium, Billing, dan Registration. **Syaratnya
tidak bertambah, tetapi pemiliknya bertambah** — perlu disepakati dengan ketiga pemilik modul itu.

**3. Batas modul kini dijaga pengujian otomatis.** Berkas `LabScopeBoundaryTests.cs` milik Laboratorium
memuat tiga pengujian `AC-42` yang menegakkan bahwa tidak ada tipe, tabel, maupun endpoint Laboratorium
yang melayani Bank Darah. Ketiganya lulus. Batas `BD-CTX-09` yang sebelumnya hanya berupa keterangan
di kode kini **dijaga mesin**, dan itu menguatkan `DEC-BD-015` serta `DEC-BD-018`.

### Satu regresi merge yang sempat memblokir modul ini — sudah tertutup

Merge `b70b735` sempat membuat dua baris kepemilikan pada registry sama-sama mencocokkan folder
`Areas/HealthServices/MasterData` dengan prefix `Mst`. Akibatnya pemeriksa kepemilikan memulangkan
*"ambiguous"*, dan **seluruh** entity `Mst*` baru terblokir `QBE-MOD-002` — terlihat persis pada
ketiga master Bank Darah. Commit `5360286`, yaitu HEAD saat ini, menghapus baris duplikatnya.
Diperiksa ulang: baris `Bbk` **`ACTIVE`**, baris `Mst` **`ACTIVE`**, tanpa duplikat. **`G2a` dan `G2b`
tetap tertutup**, dan build hijau membuktikannya di luar catatan changelog.

> ⚠️ **Build hijau bukan bukti kesiapan modul.** Ia hanya mencabut kekhawatiran bahwa merge terakhir
> merusak baseline. Putusan `NOT_READY` di bawah tetap berdiri karena **cakupan**, dan itu tidak
> tersentuh sama sekali oleh hasil build ini.

---

## Catatan historis — 4 September 2026

> Blok di bawah benar pada tanggalnya dan dipertahankan sebagai rekaman. Keadaan terkini ada di atas.

Modul berstatus **`IN_PROGRESS`**. Gelombang `MVP-0` **tuntas secara kode dan sudah terbukti**: build
hijau dan 101 pengujian Bank Darah lulus di `5f7acaf`. Yang menahan kesiapan sekarang bukan lagi
kerusakan, melainkan **cakupan** — baru satu dari lima gelombang yang ada.

| Cakupan | Putusan kesiapan | Syarat tersisa |
| --- | --- | --- |
| Modul Bank Darah | **`NOT_READY`** | Gelombang `MVP-1`..`MVP-4` beserta 12 task frontend. Nol dari 15 entity `Bbk*` operasional ada |
| Gelombang `MVP-0` | **`READY_WITH_CONDITIONS`** — terpenuhi di satu database | **Satu syarat:** seluruh migration Bank Darah dijalankan — **lima**, bukan empat. **Terpenuhi 10 September 2026 di `QuilvianNewDevSukma`**; belum di `QuilvianNewDevTim01`, staging, maupun production |

Seluruh fase perancangan sudah menghasilkan artefaknya, **tidak ada satu pun keputusan bisnis yang
masih memblokir**, dan tidak ada satu pun fase yang `BLOCKED`.

✅ **Kedua penanda `STALE` sudah dicabut.** Impact scan terbatas dijalankan 4 September 2026 atas
rentang `4205d18..5f7acaf` (backend) dan `afbb8ab..101ec5d3` (frontend). Peta kemampuan naik ke revisi
**4** dan berstatus `CURRENT`. Dua baris berpindah status dan **keduanya membaik**.

---

## Catatan historis — 3 September 2026

> Blok di bawah benar pada tanggalnya dan dipertahankan sebagai rekaman. Keadaan terkini ada di atas.

Modul naik dari `PARTIAL` ke `READY` pada 3 September 2026. Seluruh fase perancangan sudah
menghasilkan artefaknya, tidak ada satu pun keputusan bisnis yang masih memblokir, dan sejak hari itu
tidak ada satu pun fase yang `BLOCKED`.

**`BD-DEP-008` sudah ditutup** pada 3 September 2026 lewat commit `ed7fba8`: prefix `Bbk` terdaftar di
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, **persis seperti yang diajukan blueprint**.
Risiko "prefix berbeda → seluruh nama `Bbk*` berganti sebagai satu paket" yang tercatat sejak `v1`
**tidak terjadi**; seluruh nama pada kontrak `v4` tetap berlaku apa adanya.

**`G2b` juga sudah tertutup** pada 3 September 2026 lewat commit `8075784`: Lifecycle registri naik
dari `PLANNED` ke **`ACTIVE`**, yang menurut changelog registry "membuka wewenang implementasi entity
operasional `Bbk*` sesuai `QBE-MOD-002`".

**`G1` approval desain juga sudah tertutup** pada 3 September 2026. Owner menyatakan approval turun atas
nama **`Sukmagp`** bertanggal **`2026-09-03`**, dan keterangan itu sudah dicatat pada
`blueprint-manifest.md` revisi 20 beserta seluruh artefak set kontrak `v4`. Pertentangan pencatatan yang
sempat dicatat — changelog registry menyebut approval sudah ada sementara blueprint masih `draft` —
**selesai**: changelog registry ternyata benar, dan yang tertinggal memang pencatatan di sisi blueprint.

Dengan itu **ketiga gerbang global tertutup**: `G1` approval, `G2a` penamaan, dan `G2b` aktivasi modul.
Tidak ada lagi yang menahan penjadwalan task. Yang tetap berlaku adalah batas wewenang biasa: approval
membuka penjadwalan task lewat `build-module-backend`, sementara migration, eksekusi database di luar dev
pemilik, deployment, dan publikasi Git tetap wewenang terpisah yang diminta per tindakan.

---

## Pemeriksaan status dan impact scan 3 September 2026 — **selesai, blueprint tidak berubah**

**Pemicunya.** Backend bergerak dari `a9bc9fd` ke **`4205d18`** lewat merge `QuilvianIntegrationBackend`
ke `sukmagp`. Berbeda dengan seluruh pergerakan SHA sebelumnya pada modul ini, merge ini **membawa
perubahan source aplikasi yang nyata** — bukan hanya dokumen blueprint. Seluruh commit dokumen Bank
Darah tetap docs-only sebagaimana tercatat; yang berubah adalah keadaan sesudahnya.

`02-existing-capability-map.md` karena itu sempat ditandai `STALE`. **Impact scan terbatas sudah
dijalankan pada hari yang sama, dan penandanya dicabut.** Rincian buktinya ada di
`02-existing-capability-map.md` §Impact scan terbatas; ringkasannya di bawah.

### Cara batas scan dipertanggungjawabkan

Membatasi scan pada beberapa baris hanya sah bila baris lain memang tidak tersentuh. Itu diperiksa,
bukan diasumsikan: seluruh nama berkas `.cs` yang dikutip peta diadu dengan daftar berkas yang berubah.

| Pemeriksaan | Hasil |
| --- | --- |
| Berkas `.cs` yang dikutip peta kemampuan | 24 |
| Berkas `.cs` yang berubah karena merge | 28 |
| **Irisan keduanya** | **1 — `LabOrder.cs`**, dan perubahannya aditif |

### Hasil per area

| Area yang berubah | Kemampuan yang bergantung | Putusan |
| --- | --- | --- |
| LaboratoryManagement | `BD-CAP-014` pola API · `BD-CAP-007` pola pesanan · `BD-CAP-010` token konkurensi | **Tetap sahih.** `LabOrderController.cs` **tidak berubah**; route, `[Tags]`, `[AccessController]`, dan pembungkus `ApiResponse<T>` identik. `LabOrder.cs` bertambah satu kolom `Discipline`; seluruh field yang dikutip utuh, `Version` tidak tersentuh |
| InPatientManagement | `BD-CAP-003` sinyal penutupan kunjungan | **Tetap sahih.** `InpEpisode.cs` dan `EncounterStatus.cs` **tidak berubah**; kelima field `DEC-BD-014` utuh. Yang berubah hanya controller, yang tidak dipanggil Bank Darah |
| Migrations + snapshot | Rencana migration `02-backend-architecture.md` §I | **Tetap sahih.** Nol entity Bank Darah di snapshot, sesuai harapan. Yang bergeser hanya basis migration, kini `20260902042242_AddLabOrderDiscipline` |
| MasterData | `BD-CAP-006` | **Tetap sahih.** Yang berubah `BedController` dan `InpatientClearanceItem*`; keempat master yang dipakai Bank Darah tidak tersentuh |
| BillingManagement | `BD-CAP-015` | **Tetap `Extend`.** `BillingSourceContract.cs` **tidak berubah**; Bank Darah tetap belum ada di daftar sumber, sehingga `DEC-BD-016` tetap dibutuhkan |

**Nol baris kemampuan berpindah status. Blueprint Bank Darah tidak perlu diubah.**

### Dua temuan yang justru menguatkan blueprint

1. **Laboratory menyatakan Bank Darah di luar scope-nya, dengan kata-katanya sendiri.** Enum
   `LabDiscipline` yang baru memuat keterangan bahwa Bank Darah "sengaja tidak ada di sini karena tetap
   berada di luar scope modul". Ini menguatkan `DEC-BD-015`, `DEC-BD-018`, dan batas `BD-CTX-09` —
   batas itu kini berbukti **dua arah**, bukan hanya dari sisi Bank Darah.
2. **Pemecahan butir hak akses ternyata pola rumah.** Tim InPatient memecah `AccessAction` menjadi
   butir tersendiri (`Sign`, `SetIsolation`, `Reopen`, `MarkFinancialClearance`, `ReadFinancialClearance`)
   dengan alasan yang dinyatakan di kode: agar kasir dapat menandai tanpa ikut memperoleh akses baca
   resume pulang. Itu persis alasan `DEC-BD-043` dan `DEC-BD-044`. Rancangan hak akses `v4` terbukti
   mengikuti konvensi yang sedang berlaku.

### Satu catatan untuk task migration, bukan cacat blueprint

`MstServiceUnit` memasangkan ketiga penanda `IsAvailableFor*` yang sudah ada dengan satu index
gabungan. `BE-BD-002` menambahkan `IsAvailableForBloodOrder` tanpa menyebut index. Itu **bukan**
kekeliruan — jalur akses utamanya pemeriksaan satu unit berdasarkan `Id`, yang tidak menuntut index.
Relevan hanya bila kelak ada layar yang menyaring daftar unit berdasarkan penanda ini.

---

## Fase modul

| Fase | Nama | Status | Keterangan |
| --- | --- | --- | --- |
| `BD-PH-001` | Discovery dan Requirement | `DONE` | Sepuluh pass wawancara: scope, closure, architecture gap closure, architecture gap final closure, Storage Location, Storage Location decision, gerbang pemberian, role & authority, role residue, OQ residue. `SCOPE-BD-001`, `DEC-BD-001`..`DEC-BD-044`, `INV-BD-011`..`INV-BD-035`, `AC-BD-001`..`AC-BD-097`. |
| `BD-PH-002` | Audit kemampuan existing | `DONE` | 24 baris kemampuan pada `02-existing-capability-map.md` revisi **5**, status `CURRENT`. Audit penuh di `9522caa`; impact scan terbatas di `4205d18` (3 Sep), `5f7acaf` (4 Sep), dan **`5360286` (7 Sep)**. Dua baris membaik pada 4 Sep: `BD-CAP-005` dan `BD-CAP-018` menjadi `Ready to reuse`; scan 7 Sep **nol baris berpindah**. **Catatan:** dua master baru belum punya baris `BD-CAP-*`, dan pola yang dipinjam `BD-CAP-008` kini terpecah dua tingkat; audit penuh disarankan sebelum `MVP-2`. |
| `BD-PH-003` | Gerbang kelengkapan requirement | `DONE` | `02-requirement-completeness-assessment.md` revisi 2. Delapan slice `READY_FOR_DOMAIN_DESIGN`, dua `PARTIALLY_READY`. **Catatan:** `BR-BD-020` (Storage Location) belum punya rumah slice resmi; sementara diperlakukan sebagai perluasan `BD-SLICE-03/04/10`. |
| `BD-PH-004` | Arsitektur domain rumah sakit (opsional) | `DONE` | Revisi 6, `DOMAIN_ARCHITECTURE_READY`. Sepuluh bounded context, dua puluh lima konsep domain, lima aggregate, empat invariant lintas aggregate, tujuh posisi arsitektur. Sepuluh gap arsitektur seluruhnya tertutup; nol gap terbuka. |
| `BD-PH-005` | Penyusunan blueprint target | `DONE` | Set kontrak naik empat kali: `v1` → `v2` (Storage Location) → `v3` (role & authority) → **`v4`** (role residue). **Bukti penerimaan:** set kontrak `v4` disetujui `Sukmagp` pada `2026-09-03` (`G1`), tercatat di manifest revisi 20 dan di kepala setiap artefak kontrak. |
| `BD-PH-006` | Perencanaan delivery | `DONE` | Roadmap **revisi 2** menggantikan revisi 1 yang `STALE`, dan statusnya naik dari `FORWARD-TEST / DRAFT` menjadi **`APPROVED`** ketika `G1` turun. Ketiga gerbangnya tertutup: `G1` approval, `G2a` penamaan (`ed7fba8`), `G2b` aktivasi (`8075784`); `G3` revisi 1 dihapus karena `DEF-BD-004` tertutup. |
| `BD-PH-007` | Implementasi backend | **`IN_PROGRESS`** | **Per 11 September 2026 sesudah `BE-BD-004`:** delapan task backend berlaporan — `BE-BD-001`, `002`, `003`, `005`, `011`, `014` ✅; `BE-BD-016` 🟡 25 dari 39 butir; `BE-BD-004` 🟡 6 dari 9 kriteria. `BE-BD-012` **siap dijadwalkan**. **Riwayat per 11 September 2026 sebelum `BE-BD-004`:** tujuh task backend berlaporan — `BE-BD-001`, `002`, `003`, `005`, `011`, `014` ✅ dan `BE-BD-016` 🟡 20 dari 39 butir; `BE-BD-004` dan `BE-BD-012` **siap dijadwalkan**. **Riwayat per 10 September 2026:** enam task backend berlaporan — `BE-BD-001`, `002`, `005`, `011`, `014` ✅ dan `BE-BD-016` 🟡 17 dari 39 butir. `G4` tertutup, sehingga `BE-BD-003` (`MVP-1`) **siap dijadwalkan**. **Riwayat:** Gelombang `MVP-0` **tuntas secara kode dan terbukti**. `BE-BD-001`, `BE-BD-002`, `BE-BD-014` **selesai**; `BE-BD-016` **selesai sebagian** (12 dari 39 butir; sisanya arsitektural). Keempatnya meninggalkan laporan tracked. Build hijau dan **101 pengujian Bank Darah lulus** di `5f7acaf`. `MVP-1` (`BE-BD-003`) **kini aman dijadwalkan**. |
| `BD-PH-008` | Implementasi frontend | **`IN_PROGRESS`** | **Per 10 September 2026:** tiga task frontend berlaporan — `FE-BD-001` ✅, `FE-BD-011` 🟡 1 dari 2 kriteria, `FE-BD-006` 🟡 1 dari 2 kriteria; `FE-BD-009` siap dijadwalkan. **Riwayat (`NOT_STARTED`):** Kontrak API **sudah** `approved` dan terkunci pada `v4`, sehingga gerbangnya tidak lagi menahan. Yang menahan tinggal urutan biasa: tidak ada task FE yang mendahului task BE pasangannya, dan belum ada satu pun task BE yang dijalankan. |
| `BD-PH-009` | Verifikasi kesiapan | **`IN_PROGRESS`** | Dijalankan dua kali pada 4 September 2026. Pass `f940ae3`: `NOT_READY`, dua blocker kritis. Pass `5f7acaf`: **`NOT_READY` karena cakupan**, kedua blocker kritis tertutup; gelombang `MVP-0` sendiri **`READY_WITH_CONDITIONS`**. Belum dapat ditutup `DONE`. |

### Ringkasan fase

| Fase selesai | Fase siap dimulai | Fase terblokir |
| --- | --- | --- |
| `BD-PH-001` sampai `BD-PH-006` | `BD-PH-007` berjalan · `BD-PH-008` berjalan · `BD-PH-009` berjalan | **Nihil** |

---

## Keadaan delivery

| Backend | Frontend | Integrasi | Verifikasi |
| --- | --- | --- | --- |
| **`IN_PROGRESS`** | **`IN_PROGRESS`** | `NOT_STARTED` | **`IN_PROGRESS`** |

Pembaginya 15 task backend (`BE-BD-001`..`012`, `014`, `015`, `016`; `BE-BD-013` berada di future
scope) dan 12 task frontend (`FE-BD-001`..`012`) — **27 task**. Angka di bawah dihitung dari
keberadaan laporan `task/report/**`, bukan diperkirakan.

| Task | Status | Yang menahan penyelesaiannya |
| --- | --- | --- |
| `BE-BD-001` | ✅ **`SELESAI`** | —. `MstBloodBankReason` diselesaikan `7d00647`; **naik dari `SELESAI SEBAGIAN`** |
| `BE-BD-002` | ✅ **`SELESAI`** | — |
| `BE-BD-014` | ✅ **`SELESAI`** | — |
| `BE-BD-005` | ✅ **`SELESAI`** 9 September 2026 | — |
| `BE-BD-011` | ✅ **`SELESAI`** 9 September 2026 | — |
| `BE-BD-016` | `SELESAI SEBAGIAN` | **28** dari 39 butir hak akses terdaftar per 11 September 2026 (naik dari 25 lewat `BE-BD-012`, dari 20 lewat `BE-BD-004`, dan dari 17 lewat `BE-BD-003`); sisanya lahir bersama controller pemakainya, kecuali `BloodOrder : Update` yang tidak punya endpoint kontrak `v4` |
| `FE-BD-001` | ✅ **`SELESAI`** 7 September 2026 | — |
| `FE-BD-011` | `SELESAI SEBAGIAN` 10 September 2026 | 1 dari 2 kriteria; jumlah kantong tertahan menunggu `BE-BD-015` |
| `FE-BD-006` | `SELESAI SEBAGIAN` 10 September 2026 | 1 dari 2 kriteria; penyaringan menu menurut hak akses belum ada di frontend |
| `BE-BD-003` | ✅ **`SELESAI`** 11 September 2026 | — |
| `BE-BD-004` | 🟡 `SELESAI SEBAGIAN` 11 September 2026 | 6 dari 9 kriteria; `AC-BD-023`/`032` menunggu `BE-BD-015`, `AC-BD-033` menunggu `BE-BD-006` — butuh keputusan penerusan pemilik roadmap |
| `BE-BD-012` | ✅ **`SELESAI`** 11 September 2026 | —. Kelima kriteria `AC-BD-098`..`102` terbukti; dua tafsiran menunggu konfirmasi pemilik tanpa menahan kriteria ([laporan](task/report/backend/BE-BD-012.md) bagian 7). **Riwayat:** 🟡 siap dijadwalkan sejak roadmap revisi 8; ⛔ `BLOCKED` 11 September 2026 menunggu tiga keputusan, seluruhnya diputuskan pemilik pada hari yang sama |
| `FE-BD-002` | 🟡 penahan backend-nya hilang 11 September 2026 | Roadmap frontend masih `FORWARD-TEST / DRAFT` |
| `FE-BD-009` | 🟡 **siap dijadwalkan** sejak 9 September 2026 | — |
| 13 task lainnya | ⛔ `BLOCKED` | 6 task backend dan 7 task frontend, seluruhnya lewat rantai dependency sesudah `BE-BD-003` |

**Kemajuan delivery per 11 September 2026 sesudah `BE-BD-012`: 8 selesai penuh dan 4 selesai sebagian, dari 27 task** — `BE-BD-012` menambah satu yang penuh. **Riwayat:** **7 selesai penuh dan 4 selesai sebagian** sesudah `BE-BD-004`, yang menambah satu yang sebagian. **Riwayat:** **7 selesai penuh dan 3 selesai sebagian** sebelum `BE-BD-004`, `BE-BD-003` menambah satu. **Riwayat:** **kemajuan per 10 September 2026: 6 selesai penuh dan 3 selesai sebagian, dari 27 task.**
Angka ini dihitung dari sembilan laporan yang benar-benar ada di `task/report/**`. **Riwayat:** 3
selesai penuh dan 1 selesai sebagian per 4 September 2026.

**Riwayat 4 September 2026 — tidak berlaku lagi sejak 10 September 2026, lihat bagian *Migration* di atas:**
**Empat** migration sudah dibuat dan **belum satu pun dijalankan** — `AddMstBloodComponent`,
`AddServiceUnitBloodOrderFlag`, `AddMstBloodStorageLocation`, dan `AddMstBloodBankReason`. Selama
keempatnya belum dijalankan, keempat task di atas belum dapat dipakai di lingkungan mana pun.
Eksekusi database adalah wewenang terpisah, dan inilah **satu-satunya syarat** yang memisahkan
gelombang `MVP-0` dari selesai penuh.

⚠️ **Sejak 7 September 2026 syaratnya berubah sifat, bukan berubah isi.** Keempat migration itu
**bukan lagi migration terakhir**, dan `20260903071535_AddLabExamination` milik Laboratorium
**menyelip di antara** `AddServiceUnitBloodOrderFlag` dan `AddMstBloodStorageLocation`.

**Contoh supaya jelas.** Bila petugas menjalankan `dotnet ef database update` sampai
`AddMstBloodBankReason`, maka migration Laboratorium itu **ikut terpasang**, karena Entity Framework
menerapkan migration berurutan dan tidak boleh melangkahi satu pun. Tidak ada cara memasang migration
Bank Darah yang keempat tanpa melewati migration Laboratorium yang ketiga.

Enam migration modul lain juga berdiri **sesudah** keempatnya — empat milik Laboratorium, satu Billing,
satu Registration. Karena itu tindakan ini **perlu disepakati dengan pemilik Laboratorium, Billing, dan
Registration** lebih dulu. Rinciannya beserta urutan lengkapnya ada di `02-existing-capability-map.md`
§Dampak migration.

**Bukti pengujian sudah terverifikasi.** Pada `5f7acaf`, `dotnet build` memulangkan `0 Error(s)` dan
`dotnet test` memulangkan **`Failed: 0, Passed: 101`** untuk pengujian Bank Darah serta
**`Failed: 0, Passed: 212`** untuk seluruh project unit test. Angka 101 persis sama dengan yang
diklaim keempat laporan task (26 + 8 + 25 + 30 + 12).

---

## Blocker yang masih terbuka

| Blocker ID | Ringkasan | Pemilik | Terdampak | Kelanjutan yang tetap aman |
| --- | --- | --- | --- | --- |
| `DEC-BD-016` | Persetujuan pemilik Billing atas konteks sumber biaya Bank Darah | Pemilik BillingManagement | Penyerahan biaya ke Billing | Pencatatan tindakan tetap dirancang penuh tanpa penyaluran biaya |
| `OQ-BD-011` | Mekanik label golongan darah | Pemilik proses klinis | Slice label | Pemeriksaan dan validasi golongan darah tetap dirancang penuh |
| `DEF-BD-003` | Apakah semua komponen darah menuntut bukti kecocokan yang sama | Pemilik proses klinis | `IMPLEMENTATION` aturan per komponen | Titik pemeriksaan kecocokan tetap dirancang |
| `OQ-BD-010` | Apakah PMI menerima pengembalian kantong | Pemilik proses BDRS | Kegunaan `RETURNED_TO_PROVIDER` | Rancangannya tetap dibuat |
| `OQ-BD-012` | Berapa jam masa berlaku bukti kecocokan per komponen | Pemilik proses klinis | `IMPLEMENTATION` gerbang pemberian | Nilainya dari konfigurasi katalog; selama kosong gerbang menolak |
| `OQ-BD-014` | Keadaan kantong yang tercatat keliru setelah dikoreksi | Pemilik proses BDRS | `IMPLEMENTATION` jalur koreksi | Konsep catatan koreksi tetap dirancang penuh |
| `OQ-BD-016` | Apakah bukti pendukung koreksi menuntut lampiran berkas | Pemilik proses BDRS | Bentuk kolom bukti pendukung | Dirancang sebagai teks; lampiran kemampuan tersendiri |
| `BD-DEP-009` | Tiga berkas bukti kebutuhan yang dirujuk BRD tidak ada di repository | Pemilik kebutuhan | Penelusuran bukti ke kebutuhan | Perancangan tetap jalan |

**Tidak ada satu pun baris di atas yang memblokir gerbang atau fase.** Seluruhnya menyangkut scope di
luar rilis pertama (penyaluran biaya Billing), detail implementasi yang nilainya datang dari konfigurasi
master, atau satu baris seeder. Daftar ini dipertahankan supaya tidak hilang, bukan sebagai penahan.

**`DEF-BD-004` sudah tertutup seluruhnya** — keenam wewenangnya dipetakan `DEC-BD-039` sampai
`DEC-BD-044`. Ia tidak lagi menjadi blocker, dan gerbang `G3` pada roadmap revisi 1 dihapus.

⚠️ **Satu catatan pencatatan yang bukan blocker.** Approval `G1` menutup **blueprint dan set kontrak
`v4`**, sesuai bunyi gerbangnya di roadmap §B. Ia **tidak** otomatis menaikkan status register keputusan:
`DEC-BD-001` sampai `DEC-BD-044` pada `00-interview-decisions.md` tetap `draft` dengan `approved_by`
kosong. Menaikkannya menuntut pernyataan owner tersendiri. Ini **tidak menahan task mana pun** — builder
membaca kontrak, bukan register keputusan — tetapi dicatat supaya tidak dikira sudah ikut naik.

---

## Blocker yang sudah ditutup

| Blocker | Ditutup oleh |
| --- | --- |
| Sinyal penutupan kunjungan berbeda antar jenis kunjungan | `DEC-BD-014` |
| Bukti kecocokan sebelum pemberian darah | `DEC-BD-013`, `DEC-BD-017` |
| Sumber sah golongan darah | `DEC-BD-015` |
| Pengembalian dan pemakaian ulang kantong (`DEF-BD-001`) | `DEC-BD-019` |
| Penutupan administratif permintaan PMI (`DEF-BD-002`) | `DEC-BD-020` |
| Tindakan Bank Darah dan dasar biayanya | `DEC-BD-021` |
| Sampling dan batas dengan Laboratorium | `DEC-BD-018`, `DEC-BD-015` |
| Kedudukan HCLAB · laporan · setup | `DEC-BD-022`, `DEC-BD-023`, `DEC-BD-024` |
| `ARCH-BD-GAP-01`..`06` | `DEC-BD-025` sampai `DEC-BD-030` |
| `ARCH-BD-GAP-07`, `08`, `09` · `OQ-BD-013` | `DEC-BD-031` sampai `DEC-BD-034` |
| Coverage gap Storage Location | `DEC-BD-035`, `DEC-BD-036` |
| `ARCH-BD-GAP-10` nasib kantong di lokasi nonaktif | `DEC-BD-037` |
| `OQ-BD-015` gerbang pemberian dari lokasi nonaktif | `DEC-BD-038` |
| `DEF-BD-004` — validator, jalur darurat, koreksi | `DEC-BD-039`, `DEC-BD-040`, `DEC-BD-041` |
| `DEF-BD-004` — bukti kecocokan, penyelesaian, pembatalan order | `DEC-BD-042`, `DEC-BD-043`, `DEC-BD-044` |
| `BD-DEP-008` — prefix entity belum terdaftar di registry | Pendaftaran `Bbk` pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, commit `ed7fba8` 3 September 2026 |
| `BD-DEP-016` — modul belum diaktifkan (`PLANNED`) | Kenaikan Lifecycle ke `ACTIVE`, commit `8075784` 3 September 2026 |
| `OQ-BD-017` — nama peran pemegang `BloodUnit : ResolveNotUsable` | `DEC-BD-045` — kewenangan operasional BDRS, peran yang sama dengan `ResolveReturn`; ketiga butir tetap terpisah |
| `OQ-BD-018` — apakah hasil bukti kecocokan menggerbang | `DEC-BD-046` — hasil `Incompatible` menahan pemberian jalur normal; `VAL-BD-079` ditegaskan |
| `CONF-BD-006` — baris peran BDRS umum memuat `BloodUnit : Compatibility`, bertentangan dengan `DEC-BD-042`, `VAL-BD-078`, dan `AC-BD-090` | `DEC-BD-047` — butir dicabut dari baris peran umum. Ditemukan `BE-BD-016`, diserap `permission-audit-matrix.md` pada hari yang sama |
| `G1` — approval blueprint dan set kontrak `v4` | Approval owner `Sukmagp` bertanggal `2026-09-03`, dicatat pada manifest revisi 20 dan seluruh artefak set kontrak |
| Pencatatan `G1` yang bertentangan antara registry dan blueprint | Keterangan owner 3 September 2026; changelog registry terbukti benar, pencatatan blueprint yang tertinggal dan kini sudah selaras |
| Build backend rusak — `HEAD` `f940ae3` gagal dikompilasi, 217 error `CS0246` Xunit | Commit `5f7acaf` 4 September 2026. Kelima berkas test Bank Darah dipindahkan dari folder yatim `QuilvianSystemBackend.Tests/` ke `Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/MasterData/`. `dotnet build` kini `0 Error(s)` |
| Bukti 101 pengujian Bank Darah tidak berada di project mana pun | Commit `5f7acaf`. Git mencatat kelimanya sebagai rename `R099` — isinya utuh, bukan dihapus. `dotnet test` memulangkan `Failed: 0, Passed: 101` |
| `G4` / `BD-DEP-017` — provider number-series yang dapat dipakai Bank Darah | `PLT-BE-003` `NumberSeriesAllocator` pada 9 September 2026, lalu `PLT-BE-004` 6 dari 6 lulus di PostgreSQL `QuilvianNewDevSukma` pada 10 September 2026 (commit `23fb65a`). Dinyatakan tertutup pemiliknya, `Andry`, lewat keterangan `Sukmagp` 10 September 2026 |

---

## Bukti yang sudah usang

| Artefak | SHA tercatat | SHA saat ini | Tinjauan dampak yang diperlukan |
| --- | --- | --- | --- |
| `02-existing-capability-map.md` | audit penuh `9522caa` · impact scan **`5360286`** | **`d07dcf3`** | ✅ **Tetap sahih sampai `d07dcf3`.** Impact scan terbatas 11 September 2026 atas rentang `7fca34c..d07dcf3`: 21 berkas di luar `docs/`, seluruhnya milik Bank Darah (`BE-BD-003`) ditambah dua titik registrasi aditif dan snapshot bangkitan — **nol berkas bukti tersentuh, nol baris berpindah status**. Rentang `5360286..7fca34c` sudah dinilai pada 10 September 2026 (lihat `backend_source_sha_note` pada manifest). **Riwayat:** ✅ **Sudah disegarkan dan tetap sahih.** Impact scan 7 September 2026 atas rentang `ba75a05..5360286`: dari 37 nama berkas bukti unik, 6 tersentuh dan seluruhnya tetap menopang barisnya. **Nol baris berpindah status.** Dua rujukan bukti diperbarui karena Laboratorium mengganti nama entity |
| `BUSINESS REQUIREMENTS DOCUMENT (BRD).md` | `8b298bb` | `d07dcf3` | Terbatas pada konfigurasi Laboratorium. Dampaknya menyempit sejak `DEC-BD-018` memisahkan sampel Bank Darah dari sampel Laboratorium |
| `PRODUCT REQUIREMENTS DOCUMENT (PRD).md` | `8b298bb` | `d07dcf3` | Sama seperti di atas. PRD §3 yang menganjurkan memakai model sampel Laboratorium sudah digantikan `DEC-BD-018` |

✅ **Frontend `afbb8ab` → `101ec5d3` sudah ikut discan.** Kesepuluh komponen dasar yang dikutip
`BD-CAP-021` / `BD-DEP-014` **tidak berubah**. Enam berkas `base-features/` lain memang berubah, tetapi
bukan yang dikutip peta — `base-editor-view.jsx` berbeda dari `base-editor-form.jsx`, dan
`resource-filter-select.jsx` berbeda dari `filter-select.jsx`. Bukti frontend **tetap sahih**.

---

## Artefak yang sudah ada

| Artefak | Keadaan |
| --- | --- |
| `00-interview-decisions.md` | Revisi **11** — `DEC-BD-001`..`047`, `INV-BD-011`..`035`, `AC-BD-001`..`097` |
| `02-existing-capability-map.md` | Revisi **5** — 24 kemampuan, `CURRENT`. Impact scan **`5360286`** 7 September 2026; tetap sahih sampai **`d07dcf3`** menurut impact scan terbatas 11 September 2026 |
| `02-requirement-completeness-assessment.md` | Revisi 2 — `BR-BD-020` belum punya rumah slice |
| `01-prerequisite-readiness.md` | Revisi **4** — `BD-DEP-001`..`015`. Disegarkan 7 September 2026; nol dependency berubah status |
| `03-domain-architecture.md` | Revisi 6 — `DOMAIN_ARCHITECTURE_READY`, nol gap terbuka |
| `02-backend-architecture.md` | Kontrak `v4` (`approved`) — 15 tabel `Bbk*`, 3 master `Mst*`, 11 enum |
| `03-frontend-architecture.md` | Kontrak `v4` (`approved`) — 10 layar, peta menu, 21 kewajiban layar |
| `04-prd-to-mvp.md` | Kontrak `v4` (`approved`) — 12 epic, `UAT-01`..`21`, gelombang `MVP-0`..`MVP-4` |
| `data/data-dictionary.md` | Kontrak `v4` (`approved`) |
| `contracts/` (5 berkas) | `v4` (`approved`), kecuali `integration-contract.md` yang `last_changed_in: v2` karena isinya tidak bergerak — ia tetap ikut disetujui sebagai bagian set `v4` |
| `flowcharts/` (7 berkas) | Termasuk `penyimpanan-kantong.md` yang baru pada `v2` |
| `testing/acceptance-test-matrix.md` | Kontrak `v4` (`approved`) — `AC-BD-001`..`097` |
| `roadmap/00-delivery-plan.md` | Revisi 2 — **`APPROVED`** |
| `task/report/**` | **Sebelas laporan** per 11 September 2026 — backend `BE-BD-001`, `002`, `003`, `004`, `005`, `011`, `014`, `016`; frontend `FE-BD-001`, `006`, `011`. Bukti `BE-BD-003` diverifikasi ulang di `8e30aa9` pada 11 September 2026. **Riwayat:** empat laporan backend pertama terverifikasi lulus 4 September 2026 |

---

## Task berikutnya yang disarankan

### Per 11 September 2026 — sesudah `BE-BD-012`

| Urutan | Tindakan | Pemilik | Sifat |
| --- | --- | --- | --- |
| 1 | **Putuskan penerusan tiga kriteria `BE-BD-004`** — `AC-BD-023`/`032` (bagian `PendingReview`) ke `BE-BD-015`, `AC-BD-033` ke `BE-BD-006` | Pemilik roadmap, lewat `plan-module-delivery` | Satu-satunya penahan backend yang tersisa. Sesudahnya `BE-BD-004` sah ✅ dan `BE-BD-015` terbuka |
| 2 | `BE-BD-015` — penyimpanan kantong | `build-module-backend` | Jalur kritis `BE-BD-015` → `BE-BD-006` → `BE-BD-007` |
| 3 | Konfirmasi dua tafsiran `BE-BD-012`: kunjungan tanpa kelas pasien, dan arti "order sah" untuk tindakan | Pemilik proses BDRS | [Laporan](task/report/backend/BE-BD-012.md) bagian 7 nomor 4 dan 5. Tidak menahan task mana pun |
| 4 | Jawab dua pertanyaan terbuka `BE-BD-004`: kantong sesudah `Fulfilled`, dan kategori alasan pembatalan permintaan PMI | Pemilik proses BDRS | Lihat laporan `BE-BD-004` bagian 8 |
| 5 | Jadwalkan `FE-BD-010` — daftar dan pencatatan tindakan | Pemilik roadmap frontend | Penahan backend-nya hilang; roadmap frontend masih `DRAFT` |
| 6 | Terapkan migration Bank Darah ke `QuilvianNewDevTim01`, staging, dan production | Pemilik database | Wewenang terpisah per database; sejauh ini baru `QuilvianNewDevSukma` |

### Riwayat — per 11 September 2026 sesudah `BE-BD-004`

| Urutan | Tindakan | Pemilik | Sifat |
| --- | --- | --- | --- |
| 1 | **Putuskan penerusan tiga kriteria `BE-BD-004`** — `AC-BD-023`/`032` (bagian `PendingReview`) ke `BE-BD-015`, `AC-BD-033` ke `BE-BD-006` | Pemilik roadmap, lewat `plan-module-delivery` | Preseden `BE-BD-002` → `BE-BD-003` dan `BE-BD-014` → `BE-BD-015`. Sesudahnya `BE-BD-004` sah ✅ dan `BE-BD-015` terbuka |
| 2 | `BE-BD-015` — penyimpanan kantong | `build-module-backend` | Jalur kritis `BE-BD-015` → `BE-BD-006` → `BE-BD-007`. Kantong sudah ada sejak `BE-BD-004` |
| 3 | `BE-BD-012` — tindakan Bank Darah | `build-module-backend` | **🟡 siap dijadwalkan sejak roadmap revisi 8.** Kriteria `AC-BD-098`..`102`; aturan tarif `DEC-BD-049`, sumber unit/kelas `DEC-BD-048`. **Riwayat:** ⛔ sejak 11 September 2026. Butuh tiga keputusan pada [laporan](task/report/backend/BE-BD-012.md) bagian 6.2 sebelum dapat dikerjakan. **Riwayat:** siap dijadwalkan; tidak di jalur kritis |
| 4 | Jawab dua pertanyaan terbuka `BE-BD-004`: kantong sesudah `Fulfilled`, dan kategori alasan pembatalan permintaan PMI | Pemilik proses BDRS | Lihat laporan `BE-BD-004` bagian 8 |
| 5 | Terapkan migration Bank Darah ke `QuilvianNewDevTim01`, staging, dan production | Pemilik database | Wewenang terpisah per database; sejauh ini baru `QuilvianNewDevSukma` |

### Riwayat — per 11 September 2026 sebelum `BE-BD-004`

| Urutan | Tindakan | Pemilik | Sifat |
| --- | --- | --- | --- |
| 1 | **Jadwalkan `BE-BD-004`** — permintaan PMI, penerimaan, kantong lahir | `build-module-backend` | Jalur kritis `BE-BD-004` → `BE-BD-015` → `BE-BD-006` → `BE-BD-007`. `RequestNumber` dari `NumberSeriesAllocator`. Satu task satu wewenang |
| 2 | `BE-BD-012` — tindakan Bank Darah | `build-module-backend` | Terbuka bersamaan; tidak di jalur kritis |
| 3 | `FE-BD-002` — layar order darah | `build-module-frontend` | Penahan backend-nya hilang; roadmap frontend masih draft |
| 4 | Putuskan `BloodOrder : Update` | Pemilik kontrak | Buat endpoint suntingan beserta aturannya, atau cabut butirnya dari matriks |
| 5 | Konfirmasi tafsiran "kunjungan sah" dan jadwalkan pemicu kedaluwarsa otomatis | Pemilik proses BDRS | Lihat laporan `BE-BD-003` bagian 7 dan 8 |
| 6 | Terapkan migration Bank Darah ke `QuilvianNewDevTim01`, staging, dan production | Pemilik database | Wewenang terpisah per database; sejauh ini baru `QuilvianNewDevSukma` |

### Riwayat — 10 September 2026

| Urutan | Tindakan | Pemilik | Sifat |
| --- | --- | --- | --- |
| 1 | **Jadwalkan `BE-BD-003`** — order darah | `build-module-backend` | Satu task satu wewenang. Seluruh dependency-nya tertutup: `G1`, `G2b`, `BE-BD-001`, `BE-BD-002`, dan `G4`. `OrderNumber` dialokasikan lewat `NumberSeriesAllocator`; `Count+1`/`Max+1` tetap dilarang (`QBE-CODE-002/003`). `BD-CAP-009` merujuk `LabTransitionHistory.cs`. Pembuatan dan penerapan migration-nya masing-masing wewenang tersendiri |
| 2 | `FE-BD-009` — penyelesaian konflik di layar pemeriksaan | `build-module-frontend` | Siap sejak 9 September 2026; tidak bergantung pada `BE-BD-003`, sehingga dapat berjalan paralel |
| 3 | Putuskan approval roadmap frontend revisi 7 | `Sukmagp` | Masih `FORWARD-TEST / DRAFT`; approval 10 September 2026 hanya menjangkau roadmap backend |
| 4 | Audit penuh peta kemampuan | `trace-existing-capabilities` | Disarankan sebelum `MVP-2`: dua master belum punya baris `BD-CAP-*`, dan pola Laboratorium yang dipinjam sudah bergeser |
| 5 | Jalankan ulang `verify-module-readiness` | Skill | Setelah `MVP-1` tuntas |
| 6 | Terapkan migration ke `QuilvianNewDevTim01`, staging, dan production | Pemilik database bersama pemilik modul terkait | Wewenang terpisah per database; sejauh ini baru `QuilvianNewDevSukma` |

### Riwayat — 7 September 2026

| Urutan | Tindakan | Pemilik | Sifat |
| --- | --- | --- | --- |
| 1 | **Sepakati lalu jalankan migration `MVP-0`** di dev pemilik | Pemilik database **bersama** pemilik Laboratorium, Billing, dan Registration | **Wewenang terpisah, dan sejak 7 September 2026 lintas modul.** Tetap satu-satunya syarat yang memisahkan `MVP-0` dari selesai penuh, tetapi tidak lagi dapat dikerjakan sendirian: migration modul lain menyelip di antara migration Bank Darah dan ikut terpasang |
| 2 | ~~Impact scan terbatas~~ | Skill | ✅ **Selesai lagi** 7 September 2026 di `5360286`. Peta naik ke revisi **5**, `CURRENT`. **Audit penuh** disarankan sebelum `MVP-2` — dua alasan: `MstBloodStorageLocation` dan `MstBloodBankReason` belum punya baris `BD-CAP-*`, dan pola Laboratorium yang dipinjam `BD-CAP-008` kini terpecah dua tingkat |
| 3 | **Jadwalkan `MVP-1` mulai `BE-BD-003`** (order darah) | Skill | `build-module-backend`, satu task satu wewenang. **Aman** — baseline diverifikasi ulang di `5360286`: build `0 Error(s)`, 101 pengujian Bank Darah lulus. Dependency `BE-BD-001` dan `BE-BD-002` keduanya sudah selesai. Catatan untuk builder: `BD-CAP-009` yang dipakai task ini kini merujuk `LabTransitionHistory.cs`, bukan `TrxLabTransitionHistory.cs` |
| 4 | Setelah pasangan BE-nya ada: mulai task frontend dari `FE-BD-001` | Skill | `build-module-frontend`. Kontrak `v4` sudah terkunci dan `approved` |
| 5 | Jalankan ulang `verify-module-readiness` setelah `MVP-1` tuntas | Skill | Verifikasi 4 September 2026 berlaku sampai gelombang berikutnya selesai. Penyegaran 7 September 2026 **bukan** verifikasi kesiapan baru |

**Migration, eksekusi database di luar dev pemilik, deployment, dan publikasi Git tetap wewenang
terpisah.** Approval `G1` tidak menyentuh keempatnya.

`grill-me` untuk keputusan bisnis **tidak** diperlukan pada scope yang dinilai — tidak ada keputusan
bisnis yang masih memblokir.

---

## Kontrak status

`DRAFT` berarti identitas modul sudah ada tetapi pengumpulan kebutuhan belum lengkap. `DISCOVERY`
berarti sedang mengumpulkan keputusan dan bukti. `READY` berarti fase yang direncanakan boleh
dimulai. `PARTIAL` berarti minimal satu fase siap sementara fase lain terblokir atau belum
diketahui. `BLOCKED` berarti tidak ada satu pun fase berarti yang dapat berjalan dengan aman.
`IN_PROGRESS` berarti ada pekerjaan aktif yang sudah diberi wewenang. `VERIFYING` berarti menunggu
bukti kesiapan. `DONE` menuntut bukti verifikasi yang memadai. `SUPERSEDED` mencatat blueprint
penggantinya.

Status fase memakai `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, dan `SUPERSEDED`.
Sebuah fase menjadi `DONE` hanya bila bukti penerimaannya tercatat. Keberadaan file saja tidak cukup.
