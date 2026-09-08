# BE-ACC-009 — API periode akuntansi

- **TASK ID:** `BE-ACC-009` — API periode akuntansi
- **TASK TYPE:** Implementasi backend, controller + service + DTO
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** **7** — repository 0, berkas diperiksa 2 (>20), berkas diubah 1 (5 berkas), logika bisnis **2** (matriks perpindahan status, matriks jenis jurnal per status, pembangkitan tahun kabisat), kontrak API 1, database 1, keamanan/auth 0, UI/workflow 0
- **MODEL:** Claude Opus 5
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend` — `Areas/Corporate/AccountingManagement/`, `Program.cs`, `docs/module-blueprints/accounting/`
- **VISUAL REFERENCE:** `NOT REQUIRED`
- **BLUEPRINT STATUS/EVIDENCE:** `NOT APPLICABLE`
- **STALE EVIDENCE / BLOCKED PHASES:** `NOT APPLICABLE`
- **INTERRUPTIONS:** `NONE`
- **WARNINGS:** 203 warning build, seluruhnya pre-existing dan milik modul lain
- **Tanggal:** 2 September 2026
- **Baseline:** `ACC-BP-001` revisi 9 `APPROVED`, `decision_revision` 1.6
- **HEAD saat mulai:** `d9a9111` pada branch `rizkiG`

> ## ✅ STATUS: `DONE`
>
> Kelima acceptance terbukti **36 test**, seluruhnya lulus. Butir (3) — yang paling mudah salah
> di seluruh task ini — dibuktikan dua arah sekaligus.

## Validasi baseline

| Yang diperiksa | Tercatat | Nyata | Hasil |
|---|---|---|---|
| Blueprint revision | `9` | `9` | Cocok |
| `decision_revision` | `1.6` | `1.6` | Cocok |
| `ACC-API` / `ACC-STATE` | `0.2` / `0.1` | `0.2` / `0.1` | Cocok |
| 17 hash artefak canonical | manifest | dihitung ulang | **17/17 cocok** |

`BE-ACC-008` belum di-commit saat task ini dimulai, sehingga keduanya menumpuk di working tree.
Dicatat apa adanya; tidak mengubah hasil, tetapi membuat pemisahan pekerjaan lebih repot bila ada
yang perlu dibatalkan.

## Backend Governance Preflight

| Field | Isi |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | `AccountingPeriod` |
| Pemilik / prefix registry | Rizki / **`Acc`** — terdaftar, lifecycle **`ACTIVE`** |
| Applicability | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-002`/`003` tidak terpicu — nol model persisted baru. Alur `Controller → Module Service → DbContext` ditegakkan |
| `AGENTS.md` | Terbaca |
| Registry canonical | Terbaca dari `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. FILE YANG DIBUAT

| Berkas | Baris |
|---|---:|
| `Areas/.../AccountingPeriod/DTOs/AccountingPeriodDtos.cs` | 94 |
| `Areas/.../AccountingPeriod/Services/AccAccountingPeriodService.cs` | 433 |
| `Areas/.../AccountingPeriod/Controllers/AccountingPeriodController.cs` | 135 |
| `Tests/.../AccountingManagement/AccountingPeriodServiceTests.cs` | 502 |

## 2. FILE YANG DIUBAH

| Berkas | Perubahan |
|---|---|
| `Program.cs` | **+2 baris** — 1 `using`, 1 `AddScoped<AccAccountingPeriodService>()` |

---

## 3. ENDPOINT YANG DIBUAT

Lima, persis `ACC-API-0.2` grup Accounting Period. Base URL `api/v1/corporate/accounting/periods`.

| Method | Path | Hak akses |
|---|---|---|
| `GET` | `/` | `AccountingPeriod : Read` |
| `GET` | `/current` | `AccountingPeriod : Read` |
| `POST` | `/generate` | `AccountingPeriod : Create` |
| `POST` | `/{id}/close` | `AccountingPeriod : Close` |
| `POST` | `/{id}/reopen` | `AccountingPeriod : Reopen` |

**Nol delta kontrak.** Berbeda dari `BE-ACC-008`, task ini tidak menambah endpoint apa pun.

### `Close` dan `Reopen` memakai hak akses tersendiri

Bukan `Update`. `ACC-DEC-026` membatasi keduanya pada Manajer Akuntansi, dan memisahkannya membuat
pembatasan itu ditegakkan matriks hak akses — bukan pemeriksaan tambahan di dalam kode yang mudah
terlewat saat endpoint berikutnya ditulis.

Inilah yang memenuhi **acceptance (5)**: hanya pemegang `AccountingPeriod : Close` yang dapat
menutup, dan penegakannya berada di lapisan yang sama dengan seluruh modul lain.

## 4. ATURAN YANG DITEGAKKAN

### Perpindahan status, `ACC-STATE-0.1` bagian 2.1

| Dari | Tindakan | Ke |
|---|---|---|
| — | Bangkitkan setahun | `Open` |
| `Open` | Tutup sementara | `SoftClosed` |
| `Open` | Tutup permanen | `Closed` |
| `SoftClosed` | Tutup permanen | `Closed` |
| `SoftClosed` | Buka kembali | `Open` |
| `Closed` | Buka kembali | **`SoftClosed`** |

Satu perpindahan sengaja **ditolak** walau tampak masuk akal: `Closed` → `SoftClosed` lewat
endpoint tutup. Itu pembukaan kembali yang menyamar, dan pembukaan kembali mewajibkan alasan
tertulis. Membiarkannya berarti menyediakan jalan memutar untuk kewajiban `ACC-DEC-027`.

### Jenis jurnal per status, bagian 2.2

| Status | `JU` | `JP` | `JB` | `SA` |
|---|:---:|:---:|:---:|:---:|
| `Open` | ✅ | ✅ | ✅ | ✅ |
| `SoftClosed` | ❌ | ✅ | ✅ | ❌ |
| `Closed` | ❌ | ❌ | ❌ | ❌ |

Ditegakkan `AlasanPenolakanJenisJurnalAsync`, dibuat **`public static`** menerima
`ApplicationDbContext` — persis yang diminta cakupan roadmap, supaya `BE-ACC-010` dan
`BE-ACC-011` memakainya tanpa registrasi DI baru.

Pesan penolakannya menyebut **nama periode** yang dibaca pengguna, bukan istilah teknis:
*"Periode September 2026 sudah ditutup sementara. Hanya jurnal penyesuaian dan pembalikan yang
masih dapat disahkan."*

`AccountingPeriodResponse` juga membawa `AcceptedJournalTypeCodes`, sehingga layar tidak perlu
menyalin tabel di atas sendiri dan tidak akan menyimpang darinya.

## 5. BUILD RESULT

```
dotnet build ./QuilvianSystemBackend.sln
Build succeeded.
    0 Error(s)
```

Nol warning berasal dari keempat berkas baru.

## 6. VALIDATION

| Perintah / pemeriksaan | Hasil | Klasifikasi |
|---|---|---|
| `dotnet build ./QuilvianSystemBackend.sln` | 0 error | **PASS** |
| `dotnet test --filter AccountingPeriodServiceTests` | **36 lulus**, 0 gagal | **PASS** |
| `dotnet test --filter AccountingManagement` | **98 lulus**, 0 gagal | **PASS** |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | **274 lulus**, 0 gagal | **PASS** — nol regresi |
| Verifikasi 17 hash canonical | 17/17 cocok | **PASS** |

## 7. ACCEPTANCE CRITERIA `BE-ACC-009`

| # | Kriteria | Hasil | Test |
|---|---|:---:|---|
| 1 | `POST /generate` menghasilkan tepat 12 periode, tahun kabisat benar | ✅ | `Generate_MenghasilkanTepatDuaBelasPeriode`, `Generate_TahunKabisatBenarPadaFebruari` (4 kasus), `Generate_SetiapPeriodeBerakhirDiHariTerakhirBulannya` |
| 2 | Membangkitkan tahun yang sama dua kali ditolak `409` | ✅ | `Generate_TahunYangSamaDuaKali_Ditolak409` |
| 3 | **Membuka kembali `Closed` menghasilkan `SoftClosed`, bukan `Open`** | ✅ | `Reopen_DariClosed_MenghasilkanSoftClosed_BukanOpen` |
| 4 | Membuka kembali tanpa alasan ditolak `400` | ✅ | `Reopen_TanpaAlasan_Ditolak400` (2 kasus) |
| 5 | Hanya pemegang `AccountingPeriod : Close` yang dapat menutup | ✅ | `[AccessPermission("AccountingPeriod", "Close")]` sesuai `ACC-PERMISSION-0.3` bagian 7 |

### Butir (3) dibuktikan dua arah

Roadmap menandainya sebagai yang paling mudah salah, dan memang begitu: satu test yang hanya
memeriksa "statusnya berubah" akan lulus walau hasilnya `Open`.

Karena itu diuji dua arah sekaligus — `Assert.Equal(SoftClosed)` **dan**
`Assert.NotEqual(Open)` — ditambah pemeriksaan bahwa periode itu kemudian hanya menerima `JP` dan
`JB`. Pasangannya, `Reopen_DariSoftClosed_MenghasilkanOpen`, memastikan aturannya tidak
kebablasan ke arah sebaliknya.

### Tahun kabisat diuji sampai perangkapnya

Empat kasus: 2027 (28 hari), 2028 (29), **2100 (28)**, dan **2000 (29)**.

Dua terakhir itu yang penting. 2100 habis dibagi empat tetapi **bukan** kabisat, sedangkan 2000
habis dibagi 400 dan **kabisat**. Implementasi yang memakai `tahun % 4 == 0` akan lulus dua kasus
pertama dan gagal dua terakhir. `DateTime.DaysInMonth` menanganinya, dan test ini yang
membuktikannya — bukan asumsi.

## 8. DEFINITION OF DONE

| Butir DoD roadmap | Hasil |
|---|:---:|
| Acceptance terbukti test | ✅ 36 test |
| Alasan tercatat di jejak audit | ✅ `LastReasonNote`, `ReopenedBy`, `ReopenedAt` tersimpan; `LoggerService` mencatat alasannya. Dibuktikan `Reopen_AlasanTercatatDiJejakAudit` |
| Laporan task tersedia | ✅ Berkas ini |

## API CONTRACT IMPACT

Mewujudkan `ACC-API-0.2` grup Accounting Period, lima endpoint, **tanpa delta**.

## DATABASE IMPACT

`NONE` sebagai perubahan schema. Migration tetap **119**, snapshot tetap **545 tabel**,
`git diff -- Migrations/` kosong.

## SECURITY IMPACT

Memakai mekanisme hak akses yang sudah ada. Dua hak akses baru didaftarkan lewat atribut —
`AccountingPeriod : Close` dan `AccountingPeriod : Reopen` — keduanya sudah tercantum
`ACC-PERMISSION-0.3` dan bukan penambahan sepihak.

Penjaga `ACC-DEC-043` dipanggil di kelima jalur service.

## MANUAL TEST

`NOT APPLICABLE` — kelima acceptance tertutup test otomatis.

## INCIDENTAL CHANGES

`NONE`.

## GIT STATUS

Menumpuk bersama `BE-ACC-008` yang belum di-commit. Tidak ada stage, commit, push, pull, merge,
rebase, maupun deploy.

## NEXT RECOMMENDED STEP

`BE-ACC-010` — jurnal draft beserta penomorannya. **Ini task paling berisiko di seluruh modul**,
dan dua hal perlu disiapkan sebelum memulainya:

1. **Penomoran wajib aman saat bersamaan.** Roadmap mengunci polanya:
   `pg_advisory_xact_lock(hashtext(key))` di dalam transaction ditambah penambahan `CurrentValue`
   pada `AccNumberSeries`. `Count+1` dan `Max+1` dilarang `QBE-CODE-003`, dan application-level
   lock dilarang karena tidak melindungi saat aplikasi berjalan lebih dari satu instance.
2. **`ACC-TD-001` akan menggigit di sana.** `BE-ACC-010` banyak menyisipkan `AccJournalLine`, dan
   check constraint `CK_AccJournalLine_TepatSatuSisiTerisi` mustahil dipenuhi lewat EF di SQLite.
   Siasatnya sudah ada di `ChartOfAccountServiceTests`, tetapi sebaiknya diangkat menjadi
   pembantu bersama supaya tidak disalin-tempel.

**Menunggu instruksi eksplisit owner.**
