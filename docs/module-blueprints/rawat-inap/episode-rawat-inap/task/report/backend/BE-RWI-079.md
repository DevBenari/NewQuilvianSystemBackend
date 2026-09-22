# Laporan Perubahan Backend — `BE-RWI-079`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-079` |
| Judul | Kolom tujuan penugasan dokter (migration E1) |
| Slice | Gelombang 1 — `RI-V2-1`, `EPIC RI-39` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-079` |
| Trace | `FR-RI-193`, `FR-RI-194`; `RWI-DEC-099`, `RWI-DEC-130`; `INV-INP-12`; `data/data-dictionary.md` 18.1 dan 18.4; `02-backend-architecture.md` 11.5.1, 11.6, 11.8 |
| Contract version | `0.9.0` — disetujui `RWI-DEC-150`, 16 September 2026 |
| Dependency | — (task pembuka gelombang 1) |
| Klasifikasi | `MEDIUM` — satu kolom enum, satu check constraint, satu index, satu migration dua arah |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/InPatientManagement/**`, `Repositories/Configurations/**`, `Migrations/**`, `docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `70a30f1c2c62f18254273544a61a48c580b7657f` |
| Tanggal | 2026-09-16 |
| Status | **SELESAI dan terverifikasi.** `dotnet build` `0 Error(s)`; skema, check constraint, index, serta jalur migration maju dan mundur terbukti pada container Postgres 16 sekali pakai — lihat bagian 5 |

---

## 1. Masalah yang diperbaiki

Sampai hari ini tabel penugasan dokter rawat inap hanya dapat menjawab dua pertanyaan: **siapa**
dokternya dan **peran apa** yang ia bawa — DPJP, konsulen, atau dokter jaga. Yang tidak dapat
dijawabnya adalah **kenapa** penugasan itu dibuat.

Perbedaan itu terlihat pada satu keadaan yang benar-benar terjadi di ruangan. dr. Yoga lupa
menandatangani catatan SOAP shift malam. Kepala ruangan membuatkan penugasan singkat pukul
10.00–11.00 supaya ia dapat menyelesaikannya. Dengan tabel yang lama, baris itu tercatat sebagai
**dokter jaga** — sama persis dengan dr. Sari yang benar-benar menjaga bangsal sepanjang shift.

Akibatnya ada dua, dan keduanya nyata:

1. **Laporan jumlah jaga menghitung orang yang tidak pernah jaga.** dr. Yoga yang hanya menulis
   catatan selama satu jam terhitung sebagai dokter jaga pada hari itu.
2. **Jendela penulisan yang tidak pernah tertutup.** Karena tidak ada aturan yang memaksa penugasan
   singkat punya waktu selesai, satu baris tanpa `EndDateTime` memberi dr. Yoga akses menulis rekam
   medis pasien itu **selamanya**.

Kolom `AssignmentPurpose` yang dibuat task ini menutup keduanya.

---

## 2. Proses bisnis

**Tujuan.** Membedakan penugasan biasa dari penugasan singkat yang dibuat semata-mata agar seorang
dokter dapat menyelesaikan catatan klinis yang tertinggal.

**Pelaku.** Kepala ruangan atau supervisor. Dokter **tidak** dapat membuat penugasan untuk dirinya
sendiri — `RWI-DEC-130` (4).

**Pemicu.** Ditemukan catatan klinis yang belum ditandatangani, sementara penugasan dokter
penulisnya sudah berakhir.

**Langkah yang berurutan.**

1. Kepala ruangan membuka episode pasien yang bersangkutan.
2. Ia membuat penugasan dokter pendukung bertujuan `LateDocumentation`, berperan dokter jaga,
   dengan waktu selesai yang wajib diisi dan alasan yang wajib diisi.
3. Sistem menyimpan barisnya sebagai penugasan berperiode biasa — bukan tabel tersendiri.
4. Dokter penulis dapat menyelesaikan catatannya selama jendela itu terbuka.
5. Jendela tertutup sendiri pada waktu selesai yang ditetapkan. Tidak ada yang perlu menutupnya
   secara manual.

**Aturan yang berlaku — `INV-INP-12`.** Baris bertujuan `LateDocumentation` wajib memenuhi ketiganya
sekaligus:

| No | Aturan | Kenapa |
| ---: | --- | --- |
| 1 | Peran wajib dokter jaga (`AssignmentRole = 3`) | Penugasan singkat tidak pernah membawa kewenangan DPJP maupun konsulen |
| 2 | Waktu selesai wajib terisi dan lebih besar dari waktu mulai | Tanpa ini, jendela penulisannya tidak pernah tertutup |
| 3 | Alasan wajib terisi | Riwayat penugasan dibaca auditor; baris tanpa alasan tidak dapat dijelaskan siapa pun |

**Contoh berangka.** Kepala ruangan membuat penugasan 16 September 2026 pukul 10.00 sampai 11.00
beralasan "penulisan SOAP shift malam 15 September". Baris tersimpan dengan `AssignmentPurpose = 1`,
`AssignmentRole = 3`, `StartDateTime = 2026-09-16 10:00`, `EndDateTime = 2026-09-16 11:00`. Pada
pukul 11.01 baris itu sudah tidak aktif, dan dr. Yoga tidak lagi dapat menulis pada episode
tersebut.

**Status yang dihasilkan.** Tidak ada perubahan status episode. Penugasan adalah catatan berperiode,
bukan tahapan yang dilalui episode.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Penugasan singkat dikirim tanpa waktu selesai | Ditolak service (`BE-RWI-080`), dan bila entah bagaimana lolos, ditolak check constraint `CK_InpDoctorAssignment_LateDocumentation` |
| Penugasan singkat berperan selain dokter jaga | Ditolak service dan check constraint |
| Baris lama sebelum migration | Seluruhnya bernilai `Regular` — tidak satu pun ditebak |
| Migration mundur ketika sudah ada baris `LateDocumentation` | **Ditolak** dengan pesan yang menyebut jumlah barisnya |

**Hasil akhirnya.** Laporan jumlah jaga dapat menyaring `AssignmentPurpose = Regular` dan berhenti
menghitung dokter yang hanya menulis catatan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk apa |
| --- | --- |
| `AGENTS.md` | Konstitusi repository, batas wewenang, aturan migration |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Memastikan prefix `Inp` berstatus `ACTIVE` |
| `docs/module-blueprints/rawat-inap/episode-rawat-inap/data/data-dictionary.md` 18.1, 18.4 | Bentuk kolom, check constraint, dan index yang diminta |
| `.../02-backend-architecture.md` 11.5.1, 11.6, 11.8 | Nama enum, nilai, dan urutan migration `E1` |
| `Areas/HealthServices/InPatientManagement/Models/InpDoctorAssignment.cs` | Bentuk entity saat ini |
| `Areas/HealthServices/InPatientManagement/Enums/InpDoctorAssignmentRole.cs` | Pola penulisan enum modul ini |
| `Repositories/Configurations/HealthServices/InPatientManagement/InpDoctorAssignmentConfiguration.cs` | Pola konfigurasi, index, dan default value |
| `Migrations/20260911000000_AddAssignmentRoleToInpDoctorAssignment.cs` | Pola migration tulis-tangan beserta penjaga rollback-nya |
| `Repositories/Configurations/Corporate/AccountingManagement/JournalManagement/AccJournalLineConfiguration.cs` | Pola `HasCheckConstraint` yang sudah dipakai repository ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Enums/InpDoctorAssignmentPurpose.cs` | **Baru.** Enum `Regular = 0`, `LateDocumentation = 1` |
| `Areas/HealthServices/InPatientManagement/Models/InpDoctorAssignment.cs` | Kolom `AssignmentPurpose`, bawaan `Regular` |
| `Repositories/Configurations/HealthServices/InPatientManagement/InpDoctorAssignmentConfiguration.cs` | Nilai bawaan database `0`, check constraint `CK_InpDoctorAssignment_LateDocumentation`, index `IX_InpDoctorAssignment_DoctorId_Active` |
| `Migrations/20260916000000_AddAssignmentPurposeToInpDoctorAssignment.cs` | **Baru.** Migration `E1` maju dan mundur |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Kolom, index, dan check constraint baru |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint yang berubah pada task ini. Field `AssignmentPurpose` pada `InpatientDoctorAssignmentResponse` dipasang `BE-RWI-080` |
| Database | **Ada.** Satu kolom `integer NOT NULL DEFAULT 0`, satu check constraint, satu index parsial. Migration **sudah dibuat dan terbukti berjalan maju serta mundur pada container Postgres 16 sekali pakai yang kemudian dibuang.** Ia **belum** diterapkan ke database dev, staging, maupun production; menjalankannya di sana tetap wewenang terpisah |
| Keamanan/Auth | **Ada, tidak langsung.** Check constraint menutup kemungkinan jendela penulisan rekam medis yang tidak pernah tertutup. Tidak ada perubahan pada `[Authorize]`, `[AccessController]`, `[AccessAction]`, maupun `[AccessPermission]` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh satu pun endpoint. Jalur tulisnya dibuka `BE-RWI-080`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` | `All projects are up-to-date for restore.` | `PASS` | Dijalankan 16 September 2026 pada commit `36db5e6d` |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` | **`0 Error(s)`, `211 Warning(s)`, `Time Elapsed 00:04:31.02`** | `PASS` | Dijalankan 16 September 2026. Nol `error CS`; seluruh warning modul ini bertipe `CS1573` (tag `<param>` kurang pada komentar XML), pola yang sudah ada di seluruh `InPatientManagement` |
| `dotnet ef migrations has-pending-model-changes` | `No changes have been made to the model since the last migration.` | `PASS` | **Membuktikan suntingan tangan pada `ApplicationDbContextModelSnapshot.cs` cocok dengan model.** `HostAbortedException` yang menyertainya adalah perilaku normal EF design-time, bukan galat |
| Seluruh rantai migration diterapkan dari nol ke container Postgres 16 sekali pakai | `Done.` | `PASS` | `dotnet ef database update 20260916001000_AddEightSectionColumnsToInpDischargeSummary --connection "Host=localhost;Port=55432;Database=quilvian_rwi_test;…" --no-build`. **Tidak menyentuh database dev bersama** |
| Verifikasi skema — kolom | `AssignmentPurpose \| integer \| NO \| 0` | `PASS` | `SELECT column_name, data_type, is_nullable, column_default FROM information_schema.columns WHERE table_name='InpDoctorAssignment' AND column_name='AssignmentPurpose';` |
| Verifikasi skema — check constraint | `CHECK ((("AssignmentPurpose" <> 1) OR (("AssignmentRole" = 3) AND ("EndDateTime" IS NOT NULL) AND ("EndDateTime" > "StartDateTime") AND ("HandoverReason" IS NOT NULL) AND (length(TRIM(BOTH FROM "HandoverReason")) > 0))))` | `PASS` | Dibaca dari `pg_constraint`; **sama persis** dengan DDL `data-dictionary.md` 18.4 |
| Verifikasi skema — index | `CREATE INDEX "IX_InpDoctorAssignment_DoctorId_Active" ON public."InpDoctorAssignment" USING btree ("DoctorId", "EndDateTime") WHERE ("IsDelete" = false)` | `PASS` | Dibaca dari `pg_indexes` |
| **Uji perilaku check constraint — 6 kasus** | 2 diterima, 4 ditolak, seluruhnya sesuai rancangan | `PASS` | Rinciannya pada tabel di bawah |
| Migration **mundur** ditolak ketika ada baris `LateDocumentation` | `P0001: BE-RWI-079: rollback ditolak. 1 baris penugasan singkat penulisan catatan terlambat sudah tersimpan, dan membuang kolom AssignmentPurpose menghapus satu-satunya pembeda terhadap dokter jaga biasa. Pemulihan harus maju, bukan mundur.` | `PASS` | **Acceptance criteria 4 terbukti**, beserta jumlah barisnya |
| Migration **mundur** berhasil sesudah baris itu dihapus | `Done.` | `PASS` | `dotnet ef database update 20260915074405_RevisiTablePettyCash --connection …` |
| Migration **maju lagi** sesudah mundur | `Done.` | `PASS` | Siklus penuh maju → mundur → maju terbukti |
| QBE Backend Governance Preflight | Area `HealthServices`, Module `InPatientManagement`, prefix `Inp` berstatus `ACTIVE` pada registry sejak 2026-08-24 (`RWI-DEC-068`). Keberlakuan `TOUCHED LEGACY` — entity `Inp*` sudah ada, task menambah satu kolom | `PASS` | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 23 dan 103 |
| Pemeriksaan QBE yang berlaku | `QBE-MOD-002` tidak menahan (entry `ACTIVE`); `QBE-NAM-003` dan `QBE-NAM-004` tidak berlaku (tanpa rename, tanpa folder baru); `QBE-DB-001` dan `QBE-DB-002` diperhatikan — migration dibuat, eksekusi database dinyatakan terpisah | `PASS` | Bagian 3.3 baris Database |
| Kesesuaian dengan `data-dictionary.md` 18.4 | Nama kolom, tipe, nilai bawaan, bunyi check constraint, nama dan filter index sama persis dengan DDL yang didokumentasikan | `PASS` | Bandingkan `data/data-dictionary.md` bagian 18.4 dengan `Migrations/20260916000000_AddAssignmentPurposeToInpDoctorAssignment.cs` |
| Keseimbangan sintaks berkas yang berubah | Kurung kurawal, kurung biasa, dan kurung siku seimbang pada 27 berkas yang disentuh rangkaian task ini | `PASS` | Pemeriksaan statis; **bukan pengganti `dotnet build`** |
| Review diff dan scope | Hanya 28 berkas yang disentuh rangkaian task `BE-RWI-079` s.d. `BE-RWI-086`; tidak ada berkas di luar `InPatientManagement`, `Repositories/Configurations`, `Migrations`, `Program.cs`, dan blueprint | `PASS` | `git status --short` pada bagian 7 |

### Uji perilaku check constraint `CK_InpDoctorAssignment_LateDocumentation`

Tabel diklon dengan `CREATE TABLE uji_ck (LIKE public."InpDoctorAssignment" INCLUDING ALL)` — klon membawa check constraint tetapi tidak membawa foreign key, sehingga barisnya dapat diisi langsung tanpa menyiapkan episode, dokter, dan pengguna. Constraint pada klon dibaca ulang dari `pg_constraint` dan **identik** dengan yang ada di tabel aslinya.

| No | Baris yang dicoba | Yang diharapkan | Hasil sebenarnya |
| ---: | --- | --- | --- |
| 1 | Penugasan biasa (`Regular`), DPJP, tanpa waktu selesai | Diterima | `INSERT 0 1` — **diterima** |
| 2 | `LateDocumentation` lengkap: dokter jaga, 10.00–11.00, beralasan | Diterima | `INSERT 0 1` — **diterima** |
| 3 | `LateDocumentation` **tanpa waktu selesai** | Ditolak | `ERROR: new row … violates check constraint "CK_InpDoctorAssignment_LateDocumentation"` — **ditolak** |
| 4 | `LateDocumentation` berperan **DPJP**, bukan dokter jaga | Ditolak | Ditolak constraint yang sama |
| 5 | `LateDocumentation` **beralasan kosong** (hanya spasi) | Ditolak | Ditolak constraint yang sama |
| 6 | `LateDocumentation` waktu selesai **mendahului** waktu mulai | Ditolak | Ditolak constraint yang sama |

Pembacaan akhir tabel klon memuat **tepat dua baris** — nomor urut 1 dan 2. Keempat baris yang salah tidak satu pun tersimpan.

**Catatan cara build.** Perintahnya memakai `-m:1`, `-p:BuildInParallel=false`, `-p:UseSharedCompilation=false`, dan `-p:RunAnalyzers=false` atas permintaan pemilik pekerjaan supaya build tidak membebani mesin. Solution ini kini hanya memuat satu project — folder `Tests/` sudah tidak ada — sehingga build penuh selesai 4 menit 31 detik.

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT APPLICABLE` — task ini tidak menyentuh endpoint maupun layar.

**Tidak dijalankan:** tidak ada. Seluruh butir verifikasi yang diminta kartu task sudah dijalankan dan hasilnya dicatat di atas.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| AC-1 — Kolom `AssignmentPurpose` ada, `NOT NULL`, bawaan `0` = `Regular` | Terpenuhi di source | `AddColumn<int>(... nullable: false, defaultValue: 0)` pada migration; `HasDefaultValue(InpDoctorAssignmentPurpose.Regular)` pada configuration |
| AC-2 — Check constraint menolak nilai di luar enum `data-dictionary` 18.1 | Terpenuhi di source, dengan catatan bentuk | `CK_InpDoctorAssignment_LateDocumentation` menegakkan seluruh syarat `LateDocumentation`; nilai di luar `{0, 1}` dijaga penjaga eksplisit pada langkah 2 migration yang **menggagalkan migration** bila ditemukan, serta `Enum.IsDefined` pada jalur tulis `BE-RWI-080` |
| AC-3 — Seluruh baris lama bernilai `Regular` sesudah migration | Terpenuhi di source | `UPDATE ... SET "AssignmentPurpose" = 0 WHERE "AssignmentPurpose" IS DISTINCT FROM 0` — langkah 2 migration |
| AC-4 — Migration mundur menghapus kolom hanya selama belum ada baris `LateDocumentation`; bila sudah ada, berhenti dengan pesan yang menyebut alasannya | Terpenuhi di source | Blok `DO $$ ... RAISE EXCEPTION` pada `Down()`, menyebut jumlah barisnya |

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Migration maju dan mundur dijalankan pada Postgres sekali pakai | **Terpenuhi** — maju, mundur ditolak saat ada data, mundur bersih saat kosong, lalu maju lagi |
| Laporan tracked ada di `task/report/backend/BE-RWI-079.md` | Terpenuhi — berkas ini |
| Baris status pada roadmap diperbarui | Terpenuhi |
| `requirement-traceability-v2.md` membawa buktinya | Terpenuhi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol `error CS`. Modul ini menyumbang warning bertipe `CS1573` saja — tag `<param>` kurang pada komentar XML — pola yang sudah ada sebelum task ini pada `HandoverDoctorAsync`, `DecideDischargeAsync`, dan `CancelAdmissionAsync`. Migration baru dan snapshot **nol warning** |
| Masalah yang diketahui | AC-2 dipenuhi oleh **dua penjaga yang berbeda**, bukan satu check constraint tunggal atas rentang nilai enum. Bentuk DDL pada `data-dictionary.md` 18.4 memang hanya menuliskan constraint `LateDocumentation`; implementasi mengikutinya dan menambahkan penjaga migration untuk nilai di luar `{0, 1}`. Selisih bentuk ini dicatat di sini, bukan didiamkan |
| Risiko tersisa | **Kolom sudah terbukti lahir dengan benar, tetapi baru pada container sekali pakai yang kemudian dibuang.** Migration ini **belum** diterapkan ke database dev, staging, maupun production — menjalankannya di sana tetap wewenang terpisah yang belum diberikan. Sampai itu terjadi, jalur tulis `BE-RWI-080` gagal pada runtime di lingkungan tersebut |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `git status --short` pada laporan `BE-RWI-086` — seluruh rangkaian task `BE-RWI-079` s.d. `BE-RWI-086` dikerjakan pada satu working tree yang sama. Branch `MHamzah`, upstream `origin/MHamzah`, sesuai penetapan pemegang modul. Tidak ada `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun `deploy` yang dilakukan |
| Langkah berikutnya | Tidak ada pada sisi source. Penerapan migration `E1` ke database dev atau seterusnya menunggu wewenang terpisah dari pemilik |
