# Laporan Perubahan Backend — `BE-FIN-086`

> **Catatan penamaan berkas.** Aturan global `rules/rule-output/lokasi-laporan-task.md` menetapkan
> nama huruf kecil beserta judul ringkas (`be-fin-086-….md`). Laporan ini mengikuti konvensi yang
> sudah berlaku pada 15 laporan backend modul ini (`BE-FIN-074.md`..`BE-FIN-085.md`: huruf besar,
> tanpa judul ringkas), karena seluruh tautan bukti pada `roadmap/00-delivery-roadmap.md` dan
> `roadmap/01-backend-roadmap.md` sudah menunjuk ke bentuk itu. Penyimpangan ini disengaja, bukan
> diam-diam — dicatat di sini, bukan menimpa 15 nama berkas yang sudah ada.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-086` |
| Judul | Ambang pembayaran langsung memakai penanda versi; kolom tanggal berlaku dibuang |
| Slice | `REV-16A` — amandemen pasca-approval revisi 16 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `REV-16A` |
| Trace | `FR-FIN-184`, `FR-FIN-185`; `FIN-DEC-145`, `146`, `153`; `FIN-DES-094` |
| Contract version | `FIN-API-1.7` G.1; `FIN-VAL-1.9` `FIN-VAL-228`; `erd/data-dictionary.md` R14.8 |
| Dependency | `has-pending-model-changes` — dinyatakan **bersih** 4 Oktober 2026 sebelum task ini dimulai |
| Klasifikasi | `MEDIUM` — 5 berkas source/configuration + 1 migration (membuang kolom pada tabel berjalan) |
| Task mode | `BACKEND` — diberikan eksplisit pengguna 4 Oktober 2026 |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/MasterData/`, `Repositories/Configurations/.../MasterData/`, `Migrations/` |
| Tanggal | 4 Oktober 2026 |
| Status | 🟡 **Source, build, penerapan migration, dan `has-pending-model-changes` retroaktif seluruhnya dilaporkan sukses oleh pengguna (4 Oktober 2026).** Sesi agent **tidak** menjalankan build maupun migrasi; hasil di bawah adalah laporan pengguna, bukan pengamatan agent. Sisa: uji manual `K.1.1`..`K.1.5`, `K.2.1`..`K.2.6` **belum dilaporkan** |

---

## 0. Keputusan yang mendasari bentuk task ini

`FIN-DES-094` menyatakan **satu** migration untuk menambah `RowVersion` dan membuang
`EffectiveFrom` sekaligus, bernama `AlterMstDirectPaymentThresholdRowVersion`. Draf roadmap
`REV-16` sebelumnya (ditulis `/plan-module-delivery`) memecahnya menjadi dua task — `BE-FIN-086`
aditif, `BE-FIN-087` membuang kolom — merujuk `02-backend-architecture.md` N.7 yang menyebut
pemecahan dua rilis sebagai salah satu opsi aman.

Ditemukan pertentangan ini sebelum menulis kode. Pemilik (Yasmin) memutuskan: **satu migration,
ikuti `FIN-DES-094`**. Akibatnya:

- `BE-FIN-086` kini mencakup **seluruh** perubahan `MstDirectPaymentThreshold` revisi 16 (tambah
  `RowVersion`, buang `EffectiveFrom`, nama pengubah, pengecualian penetapan pertama).
- `BE-FIN-087` **dicabut** — tidak lagi ada task terpisah untuk membuang kolom.
- Migration bernama persis seperti `FIN-DES-094`: `AlterMstDirectPaymentThresholdRowVersion`.

Risiko yang biasanya memotivasi pemecahan dua rilis (aplikasi versi lama membaca kolom yang
dibuang) kecil di sini: tabel ini direncanakan **nol baris** sampai pejabat menetapkan ambang
pertama (`FIN-DEC-154`), dan satu-satunya pembaca `EffectiveFrom`
(`DirectPaymentThresholdService`) ikut berubah pada rilis yang sama.

---

## 1. Perubahan

### 1.1 Source

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/MasterData/Models/MstDirectPaymentThreshold.cs` | `EffectiveFrom` **dibuang**; `RowVersion` (`Guid`, `[ConcurrencyCheck]`, bawaan `Guid.NewGuid()`) **ditambah**, mengikuti pola `FinOpeningBalance`/`FinOpeningItemBatch` |
| `Areas/Corporate/FinanceManagement/MasterData/DTOs/DirectPaymentThresholdDtos.cs` | `UpdateDirectPaymentThresholdRequest`: `EffectiveFrom` dibuang, `ExpectedRowVersion` (`Guid?`, wajib kecuali penetapan pertama) ditambah. `DirectPaymentThresholdResponse`: `EffectiveFrom` dibuang; `RowVersion`, `LastChangedByName` ditambah. Exception baru `DirectPaymentThresholdConflictException` (409) |
| `Areas/Corporate/FinanceManagement/MasterData/Services/DirectPaymentThresholdService.cs` | `UpdateAsync`: pengecualian `ExpectedRowVersion` untuk penetapan pertama; pemeriksaan versi untuk baris yang sudah ada (urutan 422 → 400 → 409 dipertahankan); `RowVersion` diputar pada setiap penyimpanan; dua lapis penangkap benturan (`DbUpdateConcurrencyException` dan unique-violation pada penetapan pertama bersamaan). `MapAsync`+`GetUserNameAsync`: nama pengubah dibaca saat menyusun respons, mengikuti pola `GetUserNamesAsync` pada `FinanceOpeningBalanceService` |
| `Areas/Corporate/FinanceManagement/MasterData/Controllers/DirectPaymentThresholdController.cs` | `[ProducesResponseType(..., Status409Conflict)]` ditambah pada `Update`; `catch (DirectPaymentThresholdConflictException)` → `Conflict(...)` |
| `Repositories/Configurations/Corporate/FinanceManagement/MasterData/MstDirectPaymentThresholdConfiguration.cs` | `entity.Property(x => x.EffectiveFrom)...` dibuang; `entity.Property(x => x.RowVersion).IsConcurrencyToken();` ditambah |

### 1.2 Satu celah yang ditemukan dan ditutup di luar rencana awal

Penanda versi **tidak dapat** menangkap dua pejabat yang menetapkan ambang **pertama kali**
bersamaan — keduanya melihat baris kosong (404), keduanya lolos pemeriksaan versi (dikecualikan
untuk penetapan pertama), dan baru bertabrakan pada `IX_MstDirectPaymentThreshold_Active` saat
`SaveChangesAsync`. Tanpa penanganan, pejabat kedua menerima `500` generik.

Ditambahkan `catch (DbUpdateException exception) when (IsUniqueViolation(exception))` yang
memeriksa `SqlState == 23505` secara presisi (pola kanonik repo:
`HmdServiceSupport.IsUniqueViolation`, `EncounterIntakeService`, `BillingFolioService` — bukan
`catch (DbUpdateException)` polos yang akan menyamarkan galat database lain). Dijawab `409` yang
sama dengan `FIN-VAL-228`, supaya pejabat kedua memuat ulang dan melihat ambang yang baru
ditetapkan rekannya.

### 1.3 Migration — ditulis manual (instruksi eksplisit pengguna)

| Berkas | Isi |
| --- | --- |
| `Migrations/20261004090000_AlterMstDirectPaymentThresholdRowVersion.cs` | `Up`: (1) `AddColumn<Guid>("RowVersion", ..., defaultValueSql: "gen_random_uuid()")` — bawaan dinamis supaya baris yang sudah ada, bila ada, mendapat versi yang **berbeda** satu dari yang lain, bukan versi yang sama; (2) `ALTER COLUMN ... DROP DEFAULT` — bawaan dicabut sesudah terisi, supaya versi berikutnya datang dari aplikasi; (3) `DropColumn("EffectiveFrom")`. `Down`: mengembalikan **bentuk** kolom `EffectiveFrom` (`date NOT NULL`, bawaan `CURRENT_DATE` lalu dicabut) — nilai tanggal lama **tidak dapat** dikembalikan, lalu `DropColumn("RowVersion")` |
| `Migrations/20261004090000_AlterMstDirectPaymentThresholdRowVersion.Designer.cs` | Atribut `[DbContext]`/`[Migration]` dan `BuildTargetModel`, **diturunkan dari `ApplicationDbContextModelSnapshot` yang sudah diperbarui** (skrip penyalin terbatas pada perubahan deklarasi kelas, bukan tooling EF) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Blok entity `MstDirectPaymentThreshold` saja: `b.Property<DateOnly>("EffectiveFrom")...` dibuang; `b.Property<Guid>("RowVersion").IsConcurrencyToken()...` ditambah pada posisi alfabetis (sesudah `IsDelete`, sebelum `UpdateBy`), mengikuti rendering `RowVersion` pada entity lain di berkas yang sama |

Catatan pembuatan, supaya tidak ada klaim yang melebihi bukti:

- **Tidak** memakai `dotnet ef migrations add` dan **tidak** membangun proyek — instruksi
  eksplisit pengguna pada giliran ini. Berkas migration ditulis tangan; Designer diturunkan dari
  snapshot yang sudah disunting, dengan pergantian **hanya** pada bagian deklarasi kelas (atribut
  `[Migration]`, nama partial class, `BuildModel` → `BuildTargetModel`). Isi `BuildTargetModel`
  (seluruh 1894 blok entity) **identik** dengan `BuildModel` pada snapshot — itulah definisi
  "migration terbaru = snapshot sesudahnya" yang EF sendiri hasilkan.
- Penyuntingan snapshot **dibatasi** pada blok `MstDirectPaymentThreshold` saja lewat pencarian
  marker, supaya 126 ribu+ baris lain pada berkas itu — milik modul lain — tidak tersentuh.
- Prasyarat `FIN-DEC-138` ("migration sesuai snapshot, dihasilkan dari perubahan model") untuk
  task ini **belum dibuktikan tooling**. `dotnet ef migrations has-pending-model-changes` yang
  dilaporkan bersih 4 Oktober 2026 adalah bukti untuk migration **sebelumnya**
  (`RelaxFinCashMovementAmountForZeroOpeningBalance`) — bukan untuk migration ini, yang ditulis
  sesudahnya. Pemeriksaan itu **MUST** dijalankan ulang terhadap migration ini sebelum diterapkan.
- Designer baru ikut dikompilasi karena berada langsung di `Migrations/` (aturan
  `QuilvianSystemBackend.csproj`); `MigrationMetadata.g.cs`/`.g.props` **tidak disentuh** — jumlah
  Designer di `Migrations/History/` (199) tetap sama dengan deklarasi di `MigrationMetadata.g.cs`
  (199) dan `MigrationMetadataDeclaredCount` (199). Penjaga `ValidateMigrationHistoryCoverage`
  tidak terpengaruh. `Update-MigrationHistory.ps1` **tidak** dijalankan.
- `Down` mengembalikan **bentuk**, bukan **isi**: nilai `EffectiveFrom` lama tidak disalin ke
  mana pun sebelum `Up` membuangnya, sehingga `Down` tidak dapat mengembalikannya.

---

## 2. Validasi

| Pemeriksaan | Status | Keterangan |
| --- | :--: | --- |
| `dotnet build` | `PASS` (dilaporkan pengguna 4 Oktober 2026) | Dijalankan pengguna, bukan agent. Agent tidak membangun proyek (instruksi pengguna) |
| Migration ke database | **Diterapkan** (dilaporkan pengguna 4 Oktober 2026) | `dotnet ef database update --no-build --configuration Release` → *"Applying migration '20261004090000_AlterMstDirectPaymentThresholdRowVersion'. Done."* — nama migration pada log **cocok persis** dengan yang ditulis §1.3, bukan migration lain yang kebetulan ikut diterapkan |
| `dotnet ef migrations has-pending-model-changes` (retroaktif, untuk migration ini) | `PASS` (dilaporkan pengguna 4 Oktober 2026, `--configuration Release`) | Keluarannya *"No changes have been made to the model since the last migration."* Inilah bukti tooling bahwa snapshot yang disunting manual (§1.3) **tidak** meninggalkan selisih — disiplin penyuntingan terbatas pada blok `MstDirectPaymentThreshold` terbukti benar |
| Pembacaan statis — kurung kurawal seimbang pada 8 berkas yang disentuh | `PASS` | Model 6=6, DTO 12=12, Service 18=18, Controller 11=11, Configuration 3=3, Migration 4=4, Designer 1894=1894, Snapshot 1894=1894 |
| Pembacaan statis — Designer dan Snapshot identik pada `BuildTargetModel`/`BuildModel` | `PASS` | Designer diturunkan langsung dari snapshot sesudah disunting; selisih hanya pada deklarasi kelas |
| Pembacaan statis — konsumen lain (`FinanceReceivableService`, `FinanceSupplierPayableService`) tidak memakai `EffectiveFrom` pada entity ini | `PASS` | Keduanya hanya membaca `threshold.Amount`; pembuangan kolom tidak memecah kompilasinya |
| Pembacaan statis — pola `IsUniqueViolation` sesuai konvensi repo | `PASS` | Dibandingkan dengan `HmdServiceSupport.cs`, `EncounterIntakeService.cs`; memakai `PostgresErrorCodes.UniqueViolation` yang sama, bukan string literal |
| Uji manual (`K.1.1`..`K.1.5`, `K.2.1`..`K.2.6`) | `BELUM DILAPORKAN` | Prasyaratnya (build dan migrasi) kini terpenuhi; hasil pengamatannya belum dilaporkan pengguna |

---

## 3. Acceptance Criteria

| No | Kriteria | Status | Bukti |
|:--:|---|:--:|---|
| 1 | Penetapan ambang **pertama** diterima tanpa `ExpectedRowVersion` | Terpenuhi (source); belum dikompilasi | `UpdateAsync` cabang `entity == null` |
| 2 | Ambang yang sudah ada menolak `PUT` tanpa penanda versi atau dengan penanda basi (`409`) | Terpenuhi (source) | Cabang `else`; pesan `FIN-VAL-228` |
| 3 | Urutan pemeriksaan: alasan kosong (422) → nominal tidak sah (400) → benturan versi (409) | Terpenuhi (source) | Urutan `if` pada `UpdateAsync` tidak diubah dari urutan existing, hanya ditambah di akhir |
| 4 | `LastChangedByName` berisi nama tampilan; `null` bila tidak ditemukan | Terpenuhi (source) | `GetUserNameAsync` |
| 5 | `EffectiveFrom` yang masih dikirim klien lama diabaikan tanpa galat | Terpenuhi (source) | Ruas dibuang dari DTO; `[ApiController]` mengabaikan ruas tak dikenal secara bawaan (`System.Text.Json`) |
| 6 | Kolom `EffectiveFrom` dibuang dari database; `RowVersion` ditambah | **Terpenuhi, migration diterapkan 4 Oktober 2026** | Migration §1.3; dilaporkan pengguna |
| 7 | Dua penetapan pertama bersamaan tidak menghasilkan `500` | Terpenuhi (source, di luar rencana awal) | §1.2 |

---

## 4. Risiko dan Hal yang Harus Diketahui

1. **Migration ini membuang kolom pada tabel yang sudah berjalan, dan sudah diterapkan
   (4 Oktober 2026).** `FIN-DES-094` menerima risiko ini karena tabelnya direncanakan nol baris
   dan pembacanya tunggal — tetapi **belum diverifikasi lewat query** bahwa baris sungguh nol
   sebelum migration diterapkan; itu di luar wewenang agent. Bila ternyata ada baris, migration
   tetap aman secara skema (bawaan `gen_random_uuid()` mengisi `RowVersion`-nya), tetapi
   `EffectiveFrom` baris itu sudah hilang permanen — `Down` tidak dapat mengembalikan nilainya.
2. ~~Migration ini belum dibuktikan `has-pending-model-changes` secara retroaktif.~~ —
   ✅ **SELESAI** 4 Oktober 2026, nol perubahan tertunda. Snapshot yang disunting manual (§1.3)
   terbukti selaras dengan model.
3. **Dua lapis penangkap benturan.** Lapis pertama (pemeriksaan eksplisit `ExpectedRowVersion`)
   menjawab mayoritas kasus dengan pesan yang tepat. Lapis kedua
   (`DbUpdateConcurrencyException`) menjawab race murni antara pembacaan dan penyimpanan. Lapis
   ketiga (unique violation) khusus penetapan pertama bersamaan. Ketiganya memulangkan pesan 409
   yang serupa tetapi bukan identik — layar **MUST** menangani ketiganya sebagai kelas yang sama
   (muat ulang), bukan mencocokkan teks pesan persis.
4. **`FIN-API-1.7` G.1 MUST diberi label "Tersedia" setelah migration diterapkan dan dibangun.**
   Dokumen kontrak **tidak** disentuh giliran ini — wewenang tulis yang diberikan adalah source
   dan migration saja.
5. **Klien lama yang mengirim `EffectiveFrom`** tidak ditolak, tetapi nilainya tidak pernah
   disimpan — sesuai `FIN-DEC-153`.

---

## 5. Status Git (backend)

```text
 M Areas/Corporate/FinanceManagement/MasterData/Controllers/DirectPaymentThresholdController.cs
 M Areas/Corporate/FinanceManagement/MasterData/DTOs/DirectPaymentThresholdDtos.cs
 M Areas/Corporate/FinanceManagement/MasterData/Models/MstDirectPaymentThreshold.cs
 M Areas/Corporate/FinanceManagement/MasterData/Services/DirectPaymentThresholdService.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Repositories/Configurations/Corporate/FinanceManagement/MasterData/MstDirectPaymentThresholdConfiguration.cs
?? Migrations/20261004090000_AlterMstDirectPaymentThresholdRowVersion.cs
?? Migrations/20261004090000_AlterMstDirectPaymentThresholdRowVersion.Designer.cs
```

(Perubahan lain yang belum di-commit berasal dari task sebelumnya — blueprint docs REV-16.)
Tidak ada stage, commit, push, pull, merge, rebase, atau deploy.

---

## 6. Dampak ke Frontend

`FE-FIN-033` (REV-16 frontend, belum dikerjakan) menunggu rilis ini: ia **MUST** mengirim
`ExpectedRowVersion` pada setiap `PUT` kecuali penetapan pertama, **MUST** menghapus kolom
tanggal berlaku dari form, dan **MUST** menangani `409` dengan memuat ulang.

Konsumen existing yang **tidak** berubah: `FE-FIN-030`/`FE-FIN-031` (layar pembayaran langsung)
hanya membaca `threshold.Amount` lewat endpoint lain; tidak tersentuh perubahan ini.

---

## 7. Yang Belum Dikerjakan — Menunggu Wewenang Lebih Lanjut

| Hal | Keadaan |
| --- | --- |
| `dotnet build` | ✅ **PASS**, dilaporkan pengguna 4 Oktober 2026 |
| Penerapan migration ke database | ✅ **Diterapkan**, dilaporkan pengguna 4 Oktober 2026 (log menyebut nama migration persis) |
| `dotnet ef migrations has-pending-model-changes` untuk migration ini (retroaktif) | ✅ **PASS**, dilaporkan pengguna 4 Oktober 2026 — nol perubahan tertunda |
| Uji manual `K.1.1`..`K.1.5`, `K.2.1`..`K.2.6` | **BELUM DILAPORKAN** — satu-satunya yang tersisa |
| `contracts/api-contract.md` G.1 | Label **"Tersedia"** — **MUST** diperbarui; belum disentuh giliran ini (wewenang tulis hanya source dan migration) |
| `FE-FIN-033` | **Tidak lagi tertahan backend.** Boleh mulai |

### Langkah yang masih perlu pengguna jalankan

1. ~~`dotnet build`~~ — ✅ **SELESAI** 4 Oktober 2026.
2. ~~Terapkan migration~~ — ✅ **SELESAI** 4 Oktober 2026.
3. ~~`dotnet ef migrations has-pending-model-changes`~~ — ✅ **SELESAI** 4 Oktober 2026, nol
   perubahan tertunda.
4. Uji manual `K.1.1`..`K.1.5`, `K.2.1`..`K.2.6` (lihat `testing/acceptance-test-matrix.md`
   bagian K.1–K.2) — **satu-satunya langkah yang tersisa**.
5. Setelah langkah 4 dilaporkan, task dapat ditandai ✅ pada roadmap dan kontrak `FIN-API-1.7`
   G.1 diberi label "Tersedia".
