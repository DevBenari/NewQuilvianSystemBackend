# Laporan Perubahan Backend — `BE-FIN-084`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-084` |
| Judul | Saldo awal cutover menyampaikan nama penyetuju, membuang `Notes` mati, dan penguncian `KAS-KASIR` selalu menerbitkan mutasi `SALDO-AWAL` |
| Slice | `REV-14F` (`EPIC FIN-21`) — amandemen pasca-`FE-FIN-028` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian REV-14F |
| Trace | `FR-FIN-146`..`148`; `FIN-DEC-128`; keputusan pemilik 3 Oktober 2026 (belum bernomor `FIN-DEC`); `FIN-DES-088` |
| Contract version | `FIN-API-1.5` F.1; `FIN-STATE-1.6` F.1; `FIN-VAL-1.7` `FIN-VAL-168` |
| Dependency | `BE-FIN-066`, `BE-FIN-062` |
| Klasifikasi | `MEDIUM` — 3 berkas source + 1 configuration + 1 migration (menyentuh tabel yang sudah berjalan) |
| Task mode | `BACKEND` — diberikan eksplisit pengguna 3 Oktober 2026 (backend source dan artefak kontrak) |
| Target tulis | `NewQuilvianSystemBackend` |
| Tanggal | 3 Oktober 2026 |
| Status | 🟡 **Source selesai; build dan migrasi dilaporkan sukses oleh pengguna (3 Oktober 2026).** `has-pending-model-changes` dinyatakan **BERSIH** 4 Oktober 2026. Sisa: **hanya** uji manual penguncian `KAS-KASIR` bernominal 0. Sesi agent **tidak** menjalankan build maupun migrasi; hasil di bawah adalah laporan pengguna, bukan pengamatan agent |

---

## 1. Masalah yang Diperbaiki

Saat `FE-FIN-028` dibangun, empat selisih antara kontrak dan `BE-FIN-066` ditemukan. Pemilik memutuskan penyelesaiannya (3 Oktober 2026):

1. **`Notes` mati.** `ApproveOpeningBalanceRequest` dan `LockOpeningBalanceRequest` menerima `Notes`, tetapi entitas `FinOpeningBalance` tidak punya ruas untuk menyimpannya. Ruas itu dibuang.
2. **Penyetuju tanpa nama.** `OpeningBalanceResponse.ApprovedBy` hanya `Guid`. Ditambah `ApprovedByName`.
3. **Mutasi kas tidak terbit untuk saldo awal kas nol.** `LockAsync` hanya menerbitkan mutasi `SALDO-AWAL` bila `Amount > 0`, sedangkan kontrak menyatakan satu mutasi per penguncian. Diputuskan: **selalu terbit**. Itu bertabrakan dengan `FIN-VAL-168` (nominal mutasi harus > 0) dan constraint DB `CK_FinCashMovement_Amount`, sehingga dibuat **satu pengecualian sempit** untuk jenis `SALDO-AWAL`.
4. **Label kontrak usang.** `api-contract.md` F.1 menyebut endpoint yang sudah ada sebagai "Rencana (belum tersedia)".

---

## 2. Perubahan

### 2.1 Source

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/SubledgerOpeningBalanceDtos.cs` | Hapus `Notes` dari `ApproveOpeningBalanceRequest` dan `LockOpeningBalanceRequest`; tambah `ApprovedByName` (`string?`) pada `OpeningBalanceResponse` |
| `.../AccountingIntegration/Services/FinanceOpeningBalanceService.cs` | `GetUserNamesAsync` (`DisplayName ?? UserName ?? Email ?? UserCode`, pola `PettyCashBudgetService`); `MapToResponseAsync`; `GetAllAsync` mengambil nama satu kali untuk seluruh baris; syarat `&& entity.Amount > 0m` pada penerbitan mutasi dihapus |
| `.../AccountingIntegration/Services/FinanceSubledgerMovementService.cs` | `RecordCashMovementAsync`: `amount < 0` ditolak untuk semua jenis; `amount == 0` ditolak kecuali `movementType == SaldoAwal` |
| `Repositories/Configurations/Corporate/FinanceManagement/CashManagement/FinCashMovementConfiguration.cs` | `CK_FinCashMovement_Amount`: `"Amount" > 0 OR ("MovementType" = 'SALDO-AWAL' AND "Amount" = 0)` |

Controller tidak berubah (meneruskan request apa adanya).

### 2.2 Migration — ditulis manual

| Berkas | Isi |
| --- | --- |
| `Migrations/20261003090000_RelaxFinCashMovementAmountForZeroOpeningBalance.cs` | `Up`: `DropCheckConstraint` lalu `AddCheckConstraint` dengan ekspresi baru pada `public."FinCashMovement"`. `Down`: kembali ke `"Amount" > 0` |
| `Migrations/20261003090000_RelaxFinCashMovementAmountForZeroOpeningBalance.Designer.cs` | Atribut `[DbContext]`/`[Migration]` dan `BuildTargetModel`, **diturunkan dari `ApplicationDbContextModelSnapshot` yang sudah diperbarui** (skrip penyalin, bukan tooling EF) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | **Satu baris** berubah: ekspresi `CK_FinCashMovement_Amount` |

Catatan pembuatan, supaya tidak ada klaim yang melebihi bukti:

- **Tidak** memakai `dotnet ef` dan **tidak** membangun proyek (instruksi pengguna). Berkas ditulis tangan dan Designer-nya disalin dari snapshot.
- `FIN-DEC-138` mensyaratkan migration "sesuai snapshot, dihasilkan dari perubahan model". Kesesuaiannya di sini diupayakan lewat disiplin satu baris, dan **sudah dibuktikan** oleh EF: `dotnet ef migrations has-pending-model-changes --configuration Release` menyatakan nol perubahan tertunda (dilaporkan pengguna 4 Oktober 2026). Syarat `FIN-DEC-138` terpenuhi dengan bukti tooling, bukan hanya pembacaan berkas.
- Designer baru ikut dikompilasi karena berada di `Migrations/` (aturan `QuilvianSystemBackend.csproj`); `MigrationMetadata.g.cs`/`.g.props` **tidak disentuh**, sehingga penjaga `ValidateMigrationHistoryCoverage` tidak terpengaruh. Pemangkasan Designer lama lewat `Update-MigrationHistory.ps1` **tidak** dijalankan.
- `Down` **gagal** bila sudah ada mutasi `SALDO-AWAL` bernilai nol; sengaja tidak menghapus baris apa pun karena buku mutasi bersifat append-only.

### 2.3 Dokumen kontrak dan ERD

| Berkas | Perubahan |
| --- | --- |
| `contracts/api-contract.md` F.1 | Sepuluh endpoint `subledger-setup` (pemetaan dan saldo awal) berlabel **Tersedia**; bentuk body saldo awal didokumentasikan; `Notes` dinyatakan dihapus |
| `contracts/validation-matrix.md` | `FIN-VAL-168` memuat pengecualian `SALDO-AWAL` nol |
| `contracts/state-transition-matrix.md` F.1 | Penguncian `KAS-KASIR` menerbitkan mutasi "termasuk bernilai nol" |
| `erd/data-dictionary.md` | DDL `CK_FinCashMovement_Amount` diselaraskan |

---

## 3. Validasi

| Pemeriksaan | Status | Keterangan |
| --- | :--: | --- |
| `dotnet build` | `PASS` (dilaporkan pengguna 3 Oktober 2026) | Dijalankan pengguna, bukan agent. Agent tidak membangun proyek (instruksi pengguna) |
| `dotnet ef migrations has-pending-model-changes` | `PASS` (dilaporkan pengguna 4 Oktober 2026, `--configuration Release`) | Keluarannya *"No changes have been made to the model since the last migration."* Inilah yang membuktikan migration **tulisan tangan** ini sesuai snapshot — disiplin satu baris itu terbukti benar, bukan hanya diupayakan |
| Migration ke database | **Diterapkan** (dilaporkan pengguna 3 Oktober 2026: sukses) | Dijalankan Yasmin, bukan agent. Constraint `CK_FinCashMovement_Amount` baru berlaku |
| Pembacaan statis | `PASS` | Snapshot berbeda tepat satu baris dari sebelumnya; Designer baru memuat satu kemunculan constraint baru; kode memakai `FinCashMovementTypes.SaldoAwal` yang sudah ada |
| Uji manual penguncian `KAS-KASIR` bernominal 0 | `BELUM DILAPORKAN` | Prasyaratnya (build dan migrasi) kini terpenuhi; hasil pengamatannya belum dilaporkan pengguna |

---

## 4. Acceptance Criteria

| No | Kriteria | Status | Bukti |
|:--:|---|:--:|---|
| 1 | `Approve`/`Lock` tidak lagi memuat `Notes` | Terpenuhi (source); belum dikompilasi | DTO |
| 2 | Setiap respons saldo awal memuat `ApprovedByName` | Terpenuhi (source); belum dikompilasi | `MapToResponseAsync` dan `GetAllAsync` |
| 3 | Mengunci `KAS-KASIR` bernominal 0 menerbitkan satu mutasi `SALDO-AWAL` bernilai 0 | Terpenuhi (source); **bergantung migration diterapkan** | `LockAsync`; pengecualian di `RecordCashMovementAsync` |
| 4 | Mutasi negatif ditolak untuk semua jenis; mutasi nol selain `SALDO-AWAL` ditolak `400` | Terpenuhi (source) | Kondisi `amount < 0m \|\| (amount == 0m && !allowsZeroAmount)`; constraint DB |
| 5 | Constraint DB selaras dengan aturan service | Terpenuhi (berkas); **belum diterapkan** | Migration dan configuration memuat ekspresi yang sama |
| 6 | Kontrak diselaraskan | Terpenuhi | §2.3 |

---

## 5. Risiko dan Hal yang Harus Diketahui

1. **Penguncian `KAS-KASIR` bernominal 0 ditolak database sampai migration diterapkan.** Itu bukan kegagalan yang senyap (constraint menolak), tetapi tombol *Kunci* di layar akan gagal pada kondisi itu.
2. **Pengecualian harus tetap sempit.** Ia dijaga di dua tempat (service dan constraint DB). Menambah jenis mutasi lain ke pengecualian berarti mengubah invariant buku kas dan **MUST** lewat keputusan pemilik.
3. **Migration menyentuh tabel yang sudah berjalan** (berbeda dari migration REV-14 lain yang murni tabel baru). Operasinya hanya mengganti satu check constraint; baris yang ada tidak berubah.
4. **Keputusan belum tercatat.** Keputusan 1–3 belum bernomor `FIN-DEC`; pencatatannya milik `qv-grill`.
5. **Klien lama yang mengirim `Notes`** tidak ditolak (ruas tak dikenal diabaikan), tetapi nilainya tidak pernah disimpan — seperti sebelumnya.

---

## 6. Langkah yang Perlu Pengguna Jalankan

1. `dotnet build` — pastikan perubahan source dan Designer baru terkompilasi.
2. ~~`dotnet ef migrations has-pending-model-changes`~~ — ✅ **SELESAI** 4 Oktober 2026, nol perubahan tertunda.
3. Terapkan migration `RelaxFinCashMovementAmountForZeroOpeningBalance` ke database (milik Yasmin).
4. Uji manual: kunci saldo awal `KAS-KASIR` bernominal 0 → muncul tepat satu mutasi `SALDO-AWAL` bernilai 0 pada buku kas; coba mutasi kas nol jenis lain → ditolak `400`.
5. Setelah itu task dapat ditandai ✅ pada roadmap.
