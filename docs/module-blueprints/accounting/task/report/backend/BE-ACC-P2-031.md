# Laporan Perubahan Backend — `BE-ACC-P2-031`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-031` |
| Judul | Penjaga database pembalikan ganda |
| Slice | `HARDENING` — batch 14 September 2026 |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-031` (revisi 3) |
| Trace | `FR-ACC-041`; `ACC-DEC-006`; `ACC-TD-020` |
| Contract version | `ACC-API-0.10` — **tidak berubah**; `POST /journals/{id}/reverse` tetap `409` |
| Dependency | — |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 0 (2 berkas), logika bisnis 1, kontrak API 1, database 2 (perubahan index, menuntut migration), keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `AccJournalConfiguration`, `AccJournalService`, laporan ini, baris status roadmap, traceability, register `ACC-TD-020`. **Tidak** termasuk migration |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `b3ab542e` (branch `rizkiG`), perubahan belum di-commit |
| Tanggal | 14 September 2026 |
| Status | **🟡 SEBAGIAN** — 3 dari 5 acceptance terpenuhi di source; acceptance (4) dan (5) adalah langkah migration **milik Rizki** dan belum dikerjakan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `JournalManagement` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 31 (salinan suite 1.17.1 tidak memuatnya — `ACC-TD-015`) |
| Keberlakuan | `TOUCHED LEGACY` — configuration dan service yang sudah ada |
| QBE yang berlaku | `QBE-CFG-002` (configuration diperbaiki aman dalam cakupan), `QBE-TXN-001` (penyimpanan tetap dalam satu transaction), `QBE-VAL-001` (invarian "dibalik sekali"), `QBE-API-001` (pelanggaran menjadi `409`, bukan `500`) |
| Wewenang terpisah | Pembuatan dan penerapan migration — **Rizki**. Agent nol perintah `dotnet ef` |

---

## 1. Masalah yang diperbaiki

Jurnal yang sudah disahkan hanya boleh dibalik **sekali** (`ACC-DEC-006`). Sampai task ini, yang
menahan pembalikan kedua hanya **kode**: advisory lock ber-scope id jurnal asal, lalu pemeriksaan
ulang di dalam transaction. Kolom `ReversalOfJournalId` ber-index **biasa**, jadi database sendiri
tidak menolak apa pun.

Advisory lock menutup jalur aplikasi, tetapi tidak menutup jalur lain: penulisan langsung ke
database, jalur kode baru di masa depan yang lupa mengambil kunci, atau penyedia non-PostgreSQL.

**Akibat bila lolos.** Jurnal beban listrik Rp 12.000.000 dibalik dua kali. Begitu kedua jurnal
pembaliknya disahkan, buku besar mengurangi beban listrik Rp 24.000.000 — saldo akun meleset
Rp 12.000.000 **tanpa satu pun error**.

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Accounting Manager | Memilih jurnal `Posted`, mengisi alasan dan cara koreksi, menekan Balik |
| 2 | Sistem | Memeriksa "sudah pernah dibalik?" di luar transaction — bila ya, `409` langsung |
| 3 | Sistem | Membuka transaction, mengambil advisory lock `ACC_REVERSE_{id}`, memeriksa ulang |
| 4 | Sistem | Mengalokasikan nomor, menyimpan jurnal pembalik berstatus `PendingApproval` |
| 5 | **Database (baru)** | Menolak baris kedua ber-`ReversalOfJournalId` sama bila entah bagaimana lolos langkah 2–3 |
| 6 | Sistem | Menerjemahkan penolakan itu menjadi `409` dengan kalimat yang sama seperti langkah 2 |

**Jalur tidak normal — dua permintaan bersamaan.** Dua Manager menekan Balik pada jurnal yang sama
pada detik yang sama. Permintaan pertama memegang kunci; yang kedua menunggu, lalu pada pemeriksaan
ulang menemukan jurnal pembalik yang baru dibuat dan ditolak `409`. Index database tidak pernah
terpicu — ia jaring kedua, bukan penjaga utama.

**Jalur tidak normal — penjaga kode terlewati.** Database menolak dengan SQLSTATE `23505` pada index
`IX_AccJournal_ReversalOfJournalId`. Service menangkapnya **hanya bila nama index-nya cocok** —
pelanggaran check constraint baris jurnal atau deret nomor tetap dilempar sebagai kegagalan
sungguhan. Transaction dibatalkan (nomor jurnal yang sempat dialokasikan ikut batal), nomor jurnal
pembalik yang sudah tersimpan dibaca ulang, dan pengguna menerima:

> `409` — "Jurnal ini sudah pernah dibalik dengan jurnal JB/2026/09/00007."

**Yang tetap sama.** Jurnal pembalik yang sudah dihapus lunak (`IsDelete = true`) tidak dihitung —
filter index sama persis dengan pemeriksaan di service. Jurnal pembalik yang ditolak persetujuannya
tetap dihitung, sama seperti perilaku kode sejak `BE-ACC-013`; task ini tidak mengubahnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `BE-ACC-P2-031` dan `BE-ACC-P2-016`; `UTANG-TEKNIS.md` `ACC-TD-020`;
`AccJournalConfiguration.cs`; `AccJournalService.cs` (`ReverseAsync` baris 752–1065 dan pembantu
`MelanggarIndexPembalikTunggal`); `Migrations/20260902081432_AddAccountingFoundation.cs` baris 391
(nama index lama); `JournalController.cs` (satu-satunya pemanggil `ReverseAsync`);
`BillingFolioService` sebagai pola pengenalan `PostgresException`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Repositories/Configurations/Corporate/AccountingManagement/JournalManagement/AccJournalConfiguration.cs` | Index `ReversalOfJournalId` menjadi `IsUnique()` dengan `HasFilter("\"ReversalOfJournalId\" IS NOT NULL AND \"IsDelete\" = false")` dan nama dikunci `HasDatabaseName("IX_AccJournal_ReversalOfJournalId")` — nama yang sama dengan index lama |
| `Areas/Corporate/AccountingManagement/JournalManagement/Services/AccJournalService.cs` | Konstanta `NamaIndexPembalikTunggal`; blok `catch (DbUpdateException) when (MelanggarIndexPembalikTunggal(ex))` di `ReverseAsync` yang membatalkan transaction dan menjawab `409`; pembantu `private static MelanggarIndexPembalikTunggal`; `using Npgsql`; komentar advisory lock diperbarui. Advisory lock dan pemeriksaan ulang **tidak** diubah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — status dan kalimat penolakan sama dengan penjaga kode yang sudah ada |
| Database | **Model EF berubah; skema belum.** Index `IX_AccJournal_ReversalOfJournalId` pada model kini unique parsial. Migration **belum dibuat** — milik Rizki, boleh digabung dengan `BE-ACC-P2-016`. Selama belum diterapkan, penjaganya tetap advisory lock yang sudah ada — **tidak ada kemunduran** |
| Keamanan/Auth | `NOT APPLICABLE` — hak akses `Journal : Reverse` tidak berubah |

### 3.4 Isi migration yang diharapkan — untuk Rizki

Satu-satunya operasi terhadap `AccJournal`:

```text
DropIndex   name: "IX_AccJournal_ReversalOfJournalId", table: "AccJournal", schema: "public"
CreateIndex name: "IX_AccJournal_ReversalOfJournalId", table: "AccJournal", schema: "public",
            column: "ReversalOfJournalId", unique: true,
            filter: "\"ReversalOfJournalId\" IS NOT NULL AND \"IsDelete\" = false"
```

Nol tabel dan nol kolom lain boleh berubah. `Down` mengembalikan index biasa tanpa filter.

**Wajib sebelum `database update`** (acceptance 5) — pastikan hasilnya **0 baris**:

```sql
SELECT "ReversalOfJournalId", COUNT(*)
FROM public."AccJournal"
WHERE "ReversalOfJournalId" IS NOT NULL AND "IsDelete" = false
GROUP BY "ReversalOfJournalId"
HAVING COUNT(*) > 1;
```

Bila ada baris, `CreateIndex` gagal dan datanya harus dibereskan owner lebih dahulu.

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Journal Management / Journal

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/accounting/journals/{id}/reverse` | Membuat jurnal pembalik atau penyesuaian. **Tidak berubah**; hanya jalur `409` yang kini juga dapat berasal dari database | `Journal : Reverse` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false` oleh Rizki, 14 Sep 2026 | `0 error`, 189 warning | `PASS` | Tangkapan layar terminal owner. Asal kenaikan dari garis dasar 145 belum dipastikan — lihat `BE-ACC-P2-033` bagian 5 |
| Pemeriksaan source — filter dan nama index | Filter persis kartu; nama sama dengan `AddAccountingFoundation` baris 391 dan dengan `NamaIndexPembalikTunggal` | `PASS` | Kedua berkas |
| Pemeriksaan source — penangkapan dipersempit | `when (MelanggarIndexPembalikTunggal(ex))` membandingkan `SqlState == 23505` **dan** `ConstraintName` persis; `catch` umum berikutnya tetap `throw` | `PASS` | `AccJournalService.cs` baris 1027 dan pembantu |
| Pemeriksaan source — advisory lock tetap | `pg_advisory_xact_lock(hashtext('ACC_REVERSE_{id}'))` di dalam transaction, sebelum pemeriksaan ulang | `PASS` | `AccJournalService.cs` baris 945–966 |
| Pemeriksaan berkas migration oleh Rizki | Migration belum dibuat | `NOT RUN` | Milik Rizki |
| Pemeriksaan data pembalik ganda | Belum dijalankan — eksekusi database di luar wewenang agent | `NOT RUN` | Query di bagian 3.4 |

Uji manual: `NOT FEASIBLE` — jalur `409` dari database hanya dapat dipicu bila advisory lock
dilewati, dan index-nya belum ada di database sampai migration diterapkan.

**Tidak dijalankan:** automated test (`ACC-DEC-081`); `dotnet ef` (wewenang Rizki).

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Configuration memuat unique index parsial dengan filter persis di atas | **Terpenuhi** | `AccJournalConfiguration.cs` |
| 2 | Bila database menolak pembalik kedua, pengguna menerima `409` beserta pesan yang sama dengan penjaga kode, **bukan** `500` | **Terpenuhi** di source | `catch (DbUpdateException) when (...)` → `409` "Jurnal ini sudah pernah dibalik dengan jurnal …". Bila nomor pembalik tidak dapat dibaca ulang, kalimatnya "Jurnal ini sudah pernah dibalik." |
| 3 | Advisory lock dan pemeriksaan ulang yang sudah ada tetap dipertahankan | **Terpenuhi** | Baris 945–966 tidak berubah selain komentar |
| 4 | Migration yang dibuat Rizki hanya mengganti index itu — nol tabel dan nol kolom lain berubah | **Belum terpenuhi** | Migration belum dibuat. Isi yang diharapkan di bagian 3.4 |
| 5 | Sebelum migration diterapkan, Rizki memastikan data yang ada tidak memuat pembalik ganda | **Belum terpenuhi** | Query di bagian 3.4 belum dijalankan |

**Tiga dari lima terpenuhi.** Dua sisanya bukan kekurangan source — keduanya langkah milik Rizki.

| Butir DoD | Hasil |
| --- | --- |
| Source berubah | **Ya** |
| Laporan task tertulis | **Ya** — berkas ini |
| Migration dibuat dan diterapkan | **Belum** — milik Rizki, dinyatakan kartu di luar DoD agent |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 189 warning solution — lihat `BE-ACC-P2-033` bagian 5 |
| Masalah yang diketahui | Model EF dan skema berselisih sampai migration diterapkan; `dotnet ef migrations add` berikutnya oleh siapa pun akan membawa perubahan index ini |
| Risiko tersisa | (1) Data pembalik ganda yang sudah ada menggagalkan migration. (2) Setelah `DbUpdateException`, entitas jurnal pembalik yang gagal masih terlacak pada `DbContext` request itu; aman karena controller tidak menyimpan lagi sesudahnya, tetapi pemanggil baru yang memakai `ReverseAsync` di dalam alur penyimpanan lain wajib memperhatikannya |
| Perubahan sampingan | `NONE` |
| Interupsi | Source ditulis sesi 14 September 2026 sebelumnya; sesi ini memverifikasi diff tanpa menyunting ulang |
| Status Git | Lihat [`BE-ACC-P2-033`](BE-ACC-P2-033.md) bagian 7 — batch yang sama. Berkas milik task ini: `AccJournalConfiguration.cs`, `AccJournalService.cs`. **Nol commit, push, stage, merge, rebase, atau migration** |

### Langkah berikutnya

1. **Rizki:** jalankan query bagian 3.4; bila 0 baris, buat migration (boleh bersama `BE-ACC-P2-016`), periksa isinya sesuai bagian 3.4, lalu terapkan.
2. Sesudah diterapkan, laporan ini diperbarui dan task naik ke ✅.
