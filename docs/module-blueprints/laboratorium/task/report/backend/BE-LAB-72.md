# Laporan Perubahan Backend — `BE-LAB-72`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-72` |
| Judul | Pembaca kewenangan dari kredensial Human Resource |
| Slice | Gelombang `MVP-9a` — `EPIC-LAB-15`, fondasi `S4` validasi dan rilis hasil |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ak.3** |
| Trace | `FR-15.4`, `FR-15.5`; `LAB-DEC-142`, `LAB-DEC-143`, `LAB-DEC-148`, `LAB-DEC-150`; `02-backend-architecture.md` 20.4 dan 20.10 butir 7-8; A5.10 |
| Contract version | `LAB-INT-v1` **`r4`** `INT-07` (dan `r5` 9.1 sebagai arah); `LAB-VAL-v1` **`r12`** `VAL-128` dan 14.2 — seluruhnya **`approved` 2026-09-25** |
| Dependency | `BE-LAB-70` ✅ `SELESAI` — `LabPrivilegeKind` dan `LabPrivilegeDenial` berdiri, migration diterapkan |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 2 (>20), berkas diubah 0 (3), logika bisnis **2**, kontrak API 0, database 1 (membaca tabel yang sudah ada), keamanan/auth **2** (lapis orang kewenangan), UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Program.cs`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-67`..`71` yang belum ter-commit |
| Tanggal | 2026-09-29 |
| Status | ✅ **`SELESAI`** — resolver dan konstanta berdiri; build 0 error tanpa warning baru; **43 dari 43 skenario lolos** pada harness EF InMemory: delapan sebab penolakan dengan delapan pesan berbeda, batas tanggal WIB inklusif, tiga jalur fail-closed, nol tulisan ke Human Resource, dan snapshot penempatan `AC-239`. Belum ada endpoint yang memakainya — pemakainya `BE-LAB-73` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 28 dan 56). Tabel yang dibaca milik `Wfp` — `ACTIVE / LEGACY` (baris 12) — **dibaca saja** |
| Keberlakuan | `NEW CODE` — `LabClinicalPrivilegeResolver.cs` dan folder baru `Constants/` di submodul; `TOUCHED LEGACY` — `Program.cs` (satu registrasi) |
| QBE yang berlaku | `QBE-MOD-001` (capability di submodul pemiliknya), `QBE-SVC-001` (logika di service, nol controller), `QBE-ENUM-001` (enum `LabPrivilegeKind`/`LabPrivilegeDenial` milik modul), `QBE-VAL-001` (invarian fail-closed). **Tidak berlaku:** `QBE-ENT-*`/`QBE-CFG-*`/`QBE-NAM-*` (nol entity), `QBE-API-*`/`QBE-DTO-*`/`QBE-PERM-*` (nol endpoint), `QBE-LOG-001` (nol perubahan state) |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/README.md`, `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `REVIEW_RULES.md`, `DATABASE_RULES.md`, `REPORT_TEMPLATE.md`. Folder `.codex/` memang tidak ada — `AGENTS.md` baris 45 menyatakannya dicabut |

---

## 1. Masalah yang diperbaiki

**Belum ada satu tempat pun yang dapat menjawab apakah seorang dokter benar-benar ditunjuk
memvalidasi atau merilis hasil Patologi Klinik hari ini.**

Kewenangan validasi dan rilis berlapis dua (`LAB-DEC-142`):

| Lapis | Pertanyaannya | Yang menjawab |
| --- | --- | --- |
| Jabatan | *Boleh jabatan ini menjadi calon?* | Filter hak akses — layar Akses Role |
| **Orang** | *Apakah orang ini sungguh ditunjuk, untuk tindakan ini, dan penunjukannya berlaku hari ini?* | **Task ini** — dari kredensial Human Resource |

Tanpa lapis kedua, setiap dokter yang jabatannya diberi aksi `Validate` dapat mengesahkan hasil,
termasuk dokter yang penunjukannya sudah ditangguhkan atau habis masa berlakunya.

**Satu-satunya preseden di aplikasi justru berbahaya bila ditiru.** `OperatingRoomCredentialResolver`
(Kamar Operasi) tidak mencocokkan kode kewenangan, dan melaporkan tenaga tanpa data sebagai
`NotAvailable` yang **tetap mengizinkan**. Bagi pengesahan hasil pasien, data kosong harus berarti
*tidak boleh*.

---

## 2. Proses bisnis

**Pelaku.** Dokter yang menekan Validasi atau Rilis. Resolver dipanggil oleh `LabResultValidationService`
(`BE-LAB-73`..`75`), **bukan** oleh layar.

**Alur, berurutan — setiap kali tindakan ditekan, tanpa salinan dan tanpa cache.**

1. Akun pengguna → `WorkforceProfileId`. Kosong, atau akun tidak dikenal → **ditolak** `NoWorkforceProfile`.
2. Kode yang dicari dipilih dari **disiplin order** dan jenis tindakan lewat
   `LabClinicalPrivilegeCodes.For(...)`. Patologi Klinik: `LAB-VAL-PK` / `LAB-REL-PK` (usulan). Disiplin
   yang belum punya kode — Mikrobiologi dan Patologi Anatomi hari ini — → **ditolak** `NotAppointed`.
3. Baris `WfpClinicalPrivilege` milik orang itu dengan **kode sama persis**, `IsDelete = false`, dan
   `IsActive = true` dibaca. Nol baris → **ditolak** `NotAppointed`.
4. Dari baris itu, hanya yang masa berlakunya **mencakup hari tindakan menurut WIB** yang dinilai.
   Tanggal akhir **inklusif**.
5. Di antara baris yang berlaku, satu saja yang `Revoked`, `Suspended`, atau berpenanda
   `IsClinicalServiceBlocked` → **ditolak**, walau ada baris `Active` lain. Bila tidak ada, satu yang
   `Active` → **diterima** beserta `PrivilegeId`, yang kelak disimpan sebagai
   `ValidatedByPrivilegeId`/`ReleasedByPrivilegeId`.
6. Setiap penolakan membawa **satu sebab** dan pesannya (`VAL-128`, 14.2).

**Delapan sebab dan pesannya** — kata *validasi* menjadi *rilis* pada tindakan rilis.

| Sebab | Kapan | Pesan (Patologi Klinik, validasi) |
| --- | --- | --- |
| `NoWorkforceProfile` | Akun tidak terhubung data tenaga kerja | *"Akun Anda belum terhubung dengan data tenaga kerja, sehingga kewenangan validasi tidak dapat diperiksa. Hubungi bagian SDM."* |
| `NotAppointed` | Nol baris berkode sesuai | *"Anda belum ditunjuk sebagai pemegang kewenangan validasi Patologi Klinik."* |
| `PendingApproval` | Baris berlaku, masih menunggu persetujuan | *"Penunjukan validasi Patologi Klinik Anda masih menunggu persetujuan."* |
| `NotYetEffective` | Baris baru berlaku kemudian | *"Penunjukan validasi Patologi Klinik Anda baru berlaku mulai 1 November 2026."* |
| `Expired` | Masa berlaku lewat, atau ditandai kedaluwarsa | *"Masa berlaku penunjukan validasi Patologi Klinik Anda sudah habis pada 30 September 2026."* |
| `Suspended` | Ditangguhkan | *"Penunjukan validasi Patologi Klinik Anda sedang ditangguhkan."* |
| `Revoked` | Dicabut | *"Penunjukan validasi Patologi Klinik Anda sudah dicabut."* |
| `ClinicalServiceBlocked` | `IsClinicalServiceBlocked = true` | *"Layanan klinis Anda sedang diblokir pada data kredensial."* |

**Kenapa hari WIB, bukan jam — contoh berangka.** Human Resource menyimpan tanggal sebagai tanggal
kalender berlabel tengah malam UTC (`WfpClinicalPrivilegeController.NormalizeUtcDate`): penunjukan
yang berakhir 30 September tersimpan `2026-09-30T00:00Z`.

| Saat tindakan | Jam UTC | Bila dibandingkan sebagai jam | Aturan task ini (tanggal WIB) |
| --- | --- | --- | --- |
| 30 Sep 07.30 WIB | 30 Sep 00.30Z | **Ditolak** — sudah lewat `00.00Z` | Diterima — tanggal 30 |
| 30 Sep 23.50 WIB | 30 Sep 16.50Z | Ditolak | **Diterima** — tanggal 30, inklusif |
| 1 Okt 00.05 WIB | 30 Sep 17.05Z | Ditolak | **Ditolak** — tanggal 1, *"sudah habis pada 30 September 2026"* |

Perbandingan jam akan menutup penunjukan tujuh belas jam terlalu cepat. Kebalikannya terjadi pada
tanggal mulai: penunjukan yang berlaku 1 Oktober **diterima** pukul 00.05 WIB tanggal 1, walau
tanggal UTC-nya masih 30.

**Jalur tidak normal.**

| Keadaan | Jawaban |
| --- | --- |
| Data kewenangan **tidak dapat dibaca** (galat basis data) | `LabPrivilegeReadException` — pemanggil memetakannya `503` dan **tidak** melakukan tindakan. Pembacaan gagal tidak pernah dianggap izin |
| Permintaan dibatalkan pengguna | `OperationCanceledException` diteruskan apa adanya, tidak dibungkus menjadi galat pembacaan |
| Kode belum dimuat katalog Human Resource (`LAB-COORD-016` belum selesai) | Setiap validasi `NotAppointed` — **fail-closed**, dan itu perilaku yang benar |
| Penunjukan lama sudah habis dan penunjukan baru belum berlaku | `NotYetEffective` dengan tanggal penunjukan baru — yang lebih berguna bagi pengguna |
| Baris ditandai kedaluwarsa walau tanggal akhirnya belum tiba | `Expired` **tanpa** tanggal, sebab menyebut tanggal yang belum tiba akan menyesatkan |

**Snapshot penempatan jabatan (`AC-239`).** `ResolveActingPositionAsync` mencari penempatan pengguna
yang jabatannya **benar-benar memegang** aksi `Validate` atau `Release` pada resource
`LabExaminationResult`. Contoh: dokter dengan penempatan utama *Dokter Umum* (tanpa `Validate`) dan
penempatan kedua *Dokter Penanggung Jawab Laboratorium* (memegang `Validate`) → snapshot menunjuk
yang **kedua**. Bila dua penempatan sama-sama memegangnya, penempatan utama didahulukan, lalu nama
jabatan secara abjad. Superadmin, yang tidak punya penempatan, menghasilkan `null`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6ak.3 | Cakupan, AC, jebakan, DoD |
| `02-backend-architecture.md` 20.4, 20.9, 20.10, 21 | Aturan menilai baris, fail-closed, dan catatan bahwa resolver **nol perubahan** saat Mikrobiologi masuk |
| `contracts/integration-contract.md` 8.2, 9.1 | `INT-07` — baca saja, tanpa cache, kunci pencocokan |
| `contracts/validation-matrix.md` 14.1-14.2 | Teks `VAL-128` per sebab dan contoh tanggal inklusif |
| `testing/acceptance-test-matrix.md` amandemen 2026-09-25 | `AC-229`..`AC-233`, `AC-239`, *Data uji tambahan* |
| `Models/WfpClinicalPrivilege.cs`, `Enums/HumanResource/ClinicalPrivilegeStatus.cs` | Kolom dan status penunjukan |
| `WfpClinicalPrivilegeController.cs` baris 1198-1202 | **Cara Human Resource menyimpan tanggal** — dasar aturan tanggal |
| `OperatingRoomCredentialResolver.cs` | Pola membaca — dan keputusan yang **tidak** ditiru |
| `Services/Security/AccessPermissionService.cs` 117-176 | Syarat kelayakan penempatan yang disalin |
| `Helpers/AppDateTimeHelper.cs` | Batas tengah malam WIB yang dipakai seluruh aplikasi |
| `Services/LabConfirmingDoctorResolver.cs`, pengecualian Lab yang ada | Pola resolver dan kelas pengecualian modul ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Constants/LabClinicalPrivilegeCodes.cs` | **Baru**, beserta folder `Constants/` di submodul (20.4). `ValidationClinicalPathology = "LAB-VAL-PK"`, `ReleaseClinicalPathology = "LAB-REL-PK"` — **usulan**, final lewat `LAB-COORD-016` — dan `For(disiplin, jenis)` |
| `Services/LabClinicalPrivilegeResolver.cs` | **Baru.** `ResolveAsync(userId, discipline, kind, at)`, `ResolveActingPositionAsync(userId, kind)`, hasil `LabPrivilegeCheck`, `LabActingPosition`, dan `LabPrivilegeReadException`. Empat kueri, seluruhnya `AsNoTracking` |
| `Program.cs` | Satu registrasi `AddScoped<LabClinicalPrivilegeResolver>()` |

**Nol entity, nol migration, nol endpoint, nol seeder.**

**Selisih dari teks rancangan — dicatat, bukan disembunyikan.**

| Selisih | Alasan |
| --- | --- |
| `For(disiplin, jenis)` sudah dibuat sekarang — rancangan 21 menaruhnya pada gelombang Mikrobiologi | Rancangan yang sama menyatakan resolver **nol perubahan** saat Mikrobiologi masuk. Syarat itu hanya dapat dipenuhi bila pemilihan kode sudah lewat `For` sejak awal; gelombang Mikrobiologi kelak cukup menambah dua baris di konstanta |
| `ResolveActingPositionAsync` menerima `LabPrivilegeKind`, bukan teks nama aksi | Nama aksi `Validate`/`Release` dipetakan di satu tempat; pemanggil tidak dapat salah ketik |
| Pesan `VAL-128` disusun di resolver (`LabPrivilegeCheck.Message`) | Dua pesan membawa tanggal yang hanya resolver ketahui. Pemanggil cukup menjawab `403` dengan pesan itu |
| Urutan sebab bila beberapa keadaan sekaligus: `Revoked` → `Suspended` → `ClinicalServiceBlocked`; di luar masa berlaku, `NotYetEffective` didahulukan dari `Expired`; baris berstatus `NotApplicable` → `NotAppointed` | Rancangan menetapkan *satu sebab* tetapi tidak urutannya. Yang dipilih: sebab yang paling berat, atau yang paling berguna bagi pengguna |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **`NOT APPLICABLE`** — nol endpoint, nol DTO. Registri hak akses tetap 1572 kunci |
| Database | **Baca saja.** Membaca `AspNetUsers`, `WfpClinicalPrivilege`, `SysActionAccess`, `SysControllerAccess`, `AspNetUserOrganization`, `SysAccessPolicy`, dan `MstPosition`. **Nol tulisan**, nol migration, nol perubahan schema |
| Keamanan/Auth | **Inti task ini.** Lapis orang kewenangan validasi dan rilis. **Fail-closed** pada tiga jalur: akun tanpa data tenaga kerja, nol baris berkode sesuai, dan pembacaan gagal. Nol cache dan nol salinan penunjukan. Pesan tidak memuat data pasien |

---

## 4. Dokumentasi endpoint

**`NOT APPLICABLE`** — task ini tidak menambah maupun mengubah endpoint. Resolver dipakai
`POST /{id}/result/validate`, `/release`, dan `/return` yang lahir pada `BE-LAB-73`..`75`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (pertama) | 0 error, **231** warning — satu `CS1573` baru dari `LabClinicalPrivilegeResolver.cs` (tag `<param>` `cancellationToken` hilang) | `NEW ERROR` → **diperbaiki** | Keluaran build |
| Build ulang sesudah perbaikan | **0 error, 230 warning** — sama dengan baseline `BE-LAB-71`; **nol** warning dari berkas yang disentuh; 57 detik | `PASS` | Keluaran build disaring pada nama berkas |
| Harness perilaku EF InMemory — `ResolveAsync` dan `ResolveActingPositionAsync` | **43 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah. Nol baris ditulis ke database dev bersama |
| Tinjauan kode `AC-232` | **Nol** `Add`/`Update`/`Remove`/`Attach`/`SaveChanges`/`ExecuteUpdate`/`ExecuteDelete` pada kedua berkas baru; **nol** cache; keempat kueri `AsNoTracking` | `PASS` | Pencarian pola pada berkas |
| Kode `LAB-VAL-PK`/`LAB-REL-PK` ditulis di luar konstanta | **Nol** di seluruh `Areas/`; harness memakai `LabClinicalPrivilegeCodes.For(...)` | `PASS` | Pencarian pola |
| Kueri dijalankan terhadap PostgreSQL | Tidak dijalankan | `NOT RUN` | Lihat *Tidak dijalankan* |

**Rincian harness.** Tanggal penunjukan disimpan **persis** seperti Human Resource menyimpannya.

| Skenario | Hasil sebenarnya |
| --- | --- |
| Delapan sebab, satu per keadaan pada *Data uji tambahan* | Delapan sebab tepat; **delapan pesan berbeda** (`AC-233`) |
| Pengguna tidak dikenal | `NoWorkforceProfile` — fail-closed |
| `AC-229` — `Active` dalam masa berlaku | Diterima; `PrivilegeId` menunjuk baris itu |
| Berakhir 30 Sep: 23.50 WIB tgl 30 / 00.05 WIB tgl 1 / 07.30 WIB tgl 30 | Diterima / **ditolak** *"sudah habis pada 30 September 2026"* / diterima |
| Mulai 1 Okt: 00.05 WIB tgl 1 / 23.50 WIB tgl 30 | Diterima / ditolak *"baru berlaku mulai 1 Oktober 2026"* |
| Berakhir hari ini; tanpa tanggal akhir | Keduanya diterima |
| Saat tanpa zona waktu | Dibaca sebagai jam dinding WIB |
| Satu baris `Suspended` di samping baris `Active` yang berlaku | Ditolak `Suspended` |
| Baris `Revoked` di luar masa berlaku | Tidak dinilai — diterima dari baris `Active` |
| Baris terhapus dan nonaktif; kode lain di luar katalog | `NotAppointed` |
| `AC-217`/`AC-231` — hanya kode validasi lalu rilis; hanya kode rilis lalu validasi | `NotAppointed` dengan kata *rilis*; validasi ditolak, rilis diterima |
| Mikrobiologi dan Patologi Anatomi | `NotAppointed` menyebut disiplinnya — fail-closed berlapis |
| Status `Expired` dalam masa berlaku | `Expired` tanpa tanggal |
| Penunjukan lewat + penunjukan yang akan datang | `NotYetEffective` menyebut 1 Desember 2026 |
| Dua baris `Active` | `PrivilegeId` baris yang mulai paling akhir |
| **Pembacaan gagal** — `DbContext` sudah dibuang | `LabPrivilegeReadException`, bukan izin; sama pada `ResolveActingPositionAsync` |
| Permintaan dibatalkan | `OperationCanceledException` diteruskan |
| `AC-232` — 26 baris penunjukan sebelum dan sesudah seluruh skenario | Jumlah, status, dan `UpdateDateTime` identik; `ChangeTracker` resolver kosong sesudah setiap panggilan |
| **`AC-239`** — utama *Dokter Umum* tanpa `Validate`, kedua *Dokter Penanggung Jawab Laboratorium* memegangnya | Snapshot menunjuk **penempatan kedua** |
| Dua penempatan memegang `Validate`; sama-sama bukan utama | Utama didahulukan; lalu abjad (*Alfa* sebelum *Zeta*) |
| Penempatan batal; penempatan yang sudah berakhir; hanya memegang `Update`; tanpa penempatan; aksi belum terdaftar | Kelimanya `null` |
| Rilis | Memakai kebijakan `Release` |

**Batas harness, disebut apa adanya.**

- **InMemory, bukan PostgreSQL.** Kueri join penempatan-kebijakan disalin dari
  `AccessPermissionService`, yang sudah berjalan di PostgreSQL, ditambah pengurutan pada nama
  jabatan. Terjemahannya ke SQL baru teramati ketika `BE-LAB-73` memanggilnya lewat HTTP.
- **Galat pembacaan disimulasikan** dengan `DbContext` yang sudah dibuang, bukan koneksi PostgreSQL
  yang putus. Jalurnya sama: galat apa pun selain pembatalan dibungkus menjadi `LabPrivilegeReadException`.

Uji manual: **`NOT APPLICABLE`** — nol perilaku yang dapat dilihat pengguna sampai `BE-LAB-73`.

**Tidak dijalankan:**

- **Kueri terhadap database dev bersama.** `AGENTS.md` melarang menjalankan perintah database hanya
  untuk memvalidasi perubahan source. Lagi pula tabel `WfpClinicalPrivilege` di dev berisi **nol baris**.
- Startup aplikasi — nol endpoint dan nol aksi baru, jadi registri tetap 1572 dan tidak ada yang
  dapat dipanggil. Registrasi DI satu baris, pola yang sama dengan `LabConfirmingDoctorResolver`.
- Analyzer — build memakai `-p:RunAnalyzers=False`.
- Kontrol negatif — harness tidak dijalankan terhadap versi yang membandingkan jam UTC. Bahwa
  versi itu gagal pada kasus 07.30 WIB diketahui dari tabel di bagian 2, bukan dari eksekusi.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-229` pada tingkat resolver — `Active` dalam masa berlaku diterima beserta `PrivilegeId` | ✅ **Terpenuhi** | Harness |
| `AC-230` pada tingkat resolver — `Suspended`, `Revoked`, `Expired`, di luar masa berlaku ditolak | ✅ **Terpenuhi** | Harness. Bahwa `Version` dan kolom `LabExamination` tidak berubah dibuktikan `BE-LAB-73` |
| `AC-233` pada tingkat resolver — delapan sebab, delapan pesan berbeda | ✅ **Terpenuhi** | Harness, teks 14.2 persis |
| `AC-232` — nol tulisan ke Human Resource | ✅ **Terpenuhi** | Harness dan tinjauan kode |
| `VAL-128` — tanggal inklusif | ✅ **Terpenuhi** | 23.50 WIB diterima, 00.05 WIB ditolak |
| `VAL-128` — pembacaan gagal | ✅ **Terpenuhi** pada resolver | Pengecualian tersendiri; pemetaan `503` milik pemanggil `BE-LAB-73` |
| `VAL-128` — kode belum ada di katalog | ✅ **Terpenuhi** | `NotAppointed` |
| `AC-239` dua penempatan | ✅ **Terpenuhi** | Harness |
| DoD — delapan sebab teruji | ✅ **Terpenuhi** | — |
| DoD — fail-closed pada tiga jalur: akun tanpa tenaga kerja, nol baris, pembacaan gagal | ✅ **Terpenuhi** | Ketiganya ditolak atau melempar; tidak satu pun menjadi izin |
| DoD — nol tulisan ke Human Resource | ✅ **Terpenuhi** | — |
| DoD — laporan `BE-LAB-72.md` | ✅ **Terpenuhi** | Berkas ini |
| Verifikasi — kode pada test memakai konstanta | ✅ **Terpenuhi** | Nol kode tertulis ulang |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh. Satu `CS1573` sempat muncul dari berkas baru dan sudah diperbaiki |
| Masalah yang diketahui | **1. Nilai kode belum final** — `LAB-COORD-016`. Sampai katalog Human Resource memuat kode yang sama persis, setiap validasi ditolak `NotAppointed`; yang berubah kelak hanya konstanta. **2. Human Resource menghitung "hari ini" menurut tanggal UTC** (`WfpClinicalPrivilegeController` baris 82, 151, 709: `DateTime.UtcNow.Date`). Antara pukul 00.00 dan 07.00 WIB, layar kredensial Human Resource dapat menampilkan penunjukan masih berlaku, sementara Laboratorium sudah menolaknya — atau sebaliknya pada tanggal mulai. **Laboratorium yang benar menurut 20.10 butir 8**; temuan ini untuk pemilik Human Resource, tidak ditambal. **3. Arti `IsClinicalServiceBlocked`** dibaca apa adanya sampai pemilik Human Resource menyatakan lain (20.10 butir 8) |
| Risiko tersisa | **Sedang.** (a) Aturan tanggal bersandar pada cara Human Resource menyimpan tanggal hari ini — tanggal kalender berlabel tengah malam UTC. Bila kelak jalur tulisnya berubah menjadi tengah malam WIB yang dikonversi (`17.00Z` hari sebelumnya), setiap penunjukan bergeser satu hari. (b) Syarat kelayakan penempatan **disalin** dari `AccessPermissionService`; bila platform mengubahnya, snapshot dapat menunjuk penempatan yang berbeda dari yang memberi izin. (c) Terjemahan SQL belum teramati di PostgreSQL |
| Perubahan sampingan | `NONE` di repository. Harness di scratchpad sesi, di luar repository |
| Interupsi | **Ada.** Percakapan terputus sesudah inspeksi, sebelum berkas pertama ditulis. Dilanjutkan dari keadaan terverifikasi: `git status` memastikan nol berkas `BE-LAB-72` ada, lalu implementasi dimulai. Nol suntingan ganda |
| Status Git | Berkas `BE-LAB-72`: `??` `Constants/LabClinicalPrivilegeCodes.cs`, `Services/LabClinicalPrivilegeResolver.cs`, laporan ini; ` M` `Program.cs` (satu baris dari task ini; dua baris lain milik `BE-LAB-71`), `roadmap/backend-roadmap.md`, `roadmap/traceability.md`. Perubahan `BE-LAB-67`..`71` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-73` (validasi) kini tidak punya penahan kode: `BE-LAB-71` dan `BE-LAB-72` berdiri. Ia wajib memetakan `LabPrivilegeReadException` menjadi `503`, memakai `LabPrivilegeCheck.Message` untuk `403`, dan memeriksa lapis orang **sesudah** keadaan hasil (20.4). **2.** Minta pemilik Human Resource menanggapi masalah nomor 2 dan 3, dan menutup `LAB-COORD-016` |
