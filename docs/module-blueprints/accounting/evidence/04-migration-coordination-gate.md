# Migration Coordination Gate — `BE-ACC-006`

| Field | Isi |
|---|---|
| Task yang menunggu | `BE-ACC-006` — migration pertama Accounting |
| Aturan yang dijalankan | Usulan `QBE-MIG-001` dan `QBE-MIG-002`, teks di [06-shared-migration-coordination-rule.md](../06-shared-migration-coordination-rule.md) |
| Sifat | **Read-only.** Nol migration dibuat, nol `dotnet ef` dijalankan, database tidak disentuh |
| Blueprint | `ACC-BP-001` revisi 6, decision revision 1.3 |
| HEAD saat gate dijalankan | `2b152aa` pada branch `rizkiG`, working tree bersih |
| Canonical integration baseline | `origin/QuilvianIntegrationBackend@f90bcbe` |
| Tanggal | 2 September 2026 |

---

## Putusan gate

> ## ✅ TERSELESAIKAN 2 September 2026 — lihat bagian 10
>
> Putusan `❌ TIDAK LULUS` di bawah ini **tetap benar untuk baseline tempat ia dijalankan**,
> yaitu `2b152aa`. Ia **tidak** dihapus dan **tidak** disunting: ia mencatat keadaan yang memang
> nyata pada saat itu.
>
> Yang berubah adalah baselinenya. Owner modul menjalankan langkah 2 pada bagian 7 — menyegarkan
> `rizkiG` dari `origin/QuilvianIntegrationBackend@f90bcbe` — lalu membuat dan menerapkan
> migration pada `f40177a`. **`ACC-DEP-009` CLOSED.** Bukti, inventaris operasi migration, dan
> batas jujur atas apa yang diverifikasi *sesudah* alih-alih *sebelum* ada di
> **[bagian 10](#10-putusan-ulang-2-september-2026--baseline-f40177a)**.

> ## ❌ GATE TIDAK LULUS — putusan asli atas baseline `2b152aa`
>
> **`BE-ACC-006` belum boleh dijalankan.** Gate gagal pada pertanyaan 6.
>
> Sebabnya **bukan** Finance. Finance memang belum mulai dan memang bukan penghalang. Sebabnya:
> **branch `rizkiG` tertinggal lima migration dan delapan tabel di belakang canonical integration
> baseline.** Membuat migration sekarang akan menghasilkan kerusakan yang persis sama dengan
> `ACC-DEP-001` yang dulu sudah susah payah dipulihkan.

Rinciannya di bagian 3. Cara memulihkannya di bagian 7.

---

## 1. Audit dependency Accounting → Finance

### Cara memeriksanya

Tiga lapis, supaya kesimpulan "tidak ada" tidak disebabkan kelalaian mencari:

1. Seluruh `using` pada ketujuh berkas entity Accounting dibaca satu per satu.
2. Seluruh `HasOne`/`HasForeignKey` pada ketujuh configuration dipetakan.
3. Pencarian pola terlarang di seluruh kode Accounting.

### Hasil pencarian pola

| Pola dicari | Kecocokan | Keterangan |
|---|---:|---|
| `Fin[A-Z]` | **0** | — |
| `AccountsReceivable`, `AccountsPayable` | **0** | — |
| `\bAR\b`, `\bAP\b` | **0** | — |
| `Settlement` | **0** | — |
| `CashManagement` | **0** | — |
| `Payment` | **0** | — |
| `Bil[A-Z]` | 1 | **Bukan dependency.** Satu-satunya kecocokan ada di dalam komentar dokumentasi `AccNumberSeries.cs` baris 10, yang justru menyatakan tabelnya **terpisah** dari Billing (`ACC-DEC-004`). Nol `using`, nol referensi tipe |

### Matriks dependency

| Accounting Entity | Finance Dependency | Billing Dependency | Dependency nyata | Result |
|---|---|---|---|:---:|
| `AccChartOfAccount` | none | none | `MstLegalEntity`, dirinya sendiri (induk-anak) | **OK** |
| `AccJournalType` | none | none | — (berdiri sendiri) | **OK** |
| `AccAccountingPeriod` | none | none | `MstLegalEntity` | **OK** |
| `AccJournal` | none | none | `MstLegalEntity`, `AccJournalType`, `AccAccountingPeriod`, dirinya sendiri (pembalikan) | **OK** |
| `AccJournalLine` | none | none | `AccJournal`, `AccChartOfAccount`, `MstCostCenter` | **OK** |
| `AccJournalApproval` | none | none | `AccJournal` | **OK** |
| `AccNumberSeries` | none | none | — (berdiri sendiri) | **OK** |

**Nol dependency ke Finance. Nol dependency ke Billing, AR, AP, Settlement, Cash Management, atau Payment.**

### Dependency ke master existing — sesuai batas yang diizinkan

| Master | Dipakai oleh | Perilaku hapus | Disentuh? |
|---|---|---|---|
| `MstLegalEntity` | `AccChartOfAccount`, `AccAccountingPeriod`, `AccJournal` | `Restrict` | **Tidak** |
| `MstCostCenter` | `AccJournalLine` | `Restrict` | **Tidak** |

Keduanya dirujuk lewat `.WithMany()` tanpa navigasi balik, sehingga **nol berkas milik Human
Resource berubah**. Ini konsisten dengan ERD yang menandai keduanya **MUST NOT** disalin.

---

## 2. Keadaan model EF saat ini

| Ukuran | Jumlah |
|---|---:|
| Entity persisted Accounting | **7** |
| `DbSet` Accounting di `ApplicationDbContext` | **7** |
| Configuration Accounting | **7** |

Ketiganya cocok — tidak ada entity yang lupa didaftarkan maupun configuration yatim.

| Entity | `DbSet` | Configuration |
|---|---|---|
| `AccChartOfAccount` | `AccChartOfAccounts` | `AccChartOfAccountConfiguration` |
| `AccJournalType` | `AccJournalTypes` | `AccJournalTypeConfiguration` |
| `AccAccountingPeriod` | `AccAccountingPeriods` | `AccAccountingPeriodConfiguration` |
| `AccJournal` | `AccJournals` | `AccJournalConfiguration` |
| `AccJournalLine` | `AccJournalLines` | `AccJournalLineConfiguration` |
| `AccJournalApproval` | `AccJournalApprovals` | `AccJournalApprovalConfiguration` |
| `AccNumberSeries` | `AccNumberSeries` | `AccNumberSeriesConfiguration` |

Snapshot lokal memuat **530** tabel, dan **nol** di antaranya `Acc*` — ketujuhnya memang belum
pernah masuk migration.

---

## 3. Temuan yang menggagalkan gate

### `rizkiG` tertinggal 5 migration dan 8 tabel dari integration

| Ukuran | `rizkiG@2b152aa` | `origin/QuilvianIntegrationBackend@f90bcbe` |
|---|---:|---:|
| Tabel pada snapshot | **530** | **538** |
| Migration | 111 | 116 |

Lima migration yang ada di integration tetapi **belum** ada di `rizkiG`:

1. `20260828093000_AddRadiologyManagement`
2. `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix`
3. `20260831075231_AddCompanyGuarantorToPatientEncounterGuarantor`
4. `20260901041805_RenameClinicalMilestoneFactToCliPrefix`
5. `20260901094255_RepairCanonicalModelSnapshotBaseline`

Delapan tabel yang ada di snapshot integration tetapi tidak di snapshot lokal:

`CliClinicalMilestoneFact`, `MrcAccessLog`, `MrcClinicalDocumentIntegrity`,
`MrcClinicalNoteAddendum`, `MrcClinicalNoteAuthorDelegation`, `MstRadModality`,
`MstRadModalitySafetyRule`, `MstRadSafetyRequirement`, `RadAcquisitionConsumption`, `RadOrder`,
`RadStudy`, `RadStudySafetyCheck`, `RadTransitionHistory`.

### Kenapa ini berbahaya — bukan sekadar "kurang mutakhir"

Entity Framework membangun migration dengan membandingkan model sekarang terhadap snapshot.
Snapshot itu **satu berkas bersama** milik semua modul.

Bila migration Accounting dibuat dari snapshot `rizkiG` yang memuat 530 tabel, snapshot barunya
akan ditulis ulang dari model `rizkiG` — dan model itu **tidak mengenal** Radiology, Medical
Record `Mrc*`, maupun Clinical Milestone. Kedelapan tabel itu **hilang dari snapshot**.

Akibatnya berantai:

1. Migration Accounting terlihat wajar saat dibuat — hanya tujuh `CreateTable`.
2. Snapshot hasilnya kehilangan 8 tabel milik modul lain.
3. Migration **berikutnya** siapa pun, dibuat dari snapshot cacat itu, akan menyimpulkan kedelapan
   tabel "belum pernah ada" dan ikut membawanya sebagai `CreateTable` — padahal tabelnya sudah
   berdiri dan berisi data.

**Ini persis kerusakan `ACC-DEP-001`**, yang dulu tercatat sebagai 39 tabel hilang dan menuntut
dua migration perbaikan (`RepairCanonicalEfModelBaseline` dan `RepairPostCanonicalIntegration`)
untuk dipulihkan. Aturan `QBE-MIG-001` ada justru untuk mencegah pengulangannya.

### Pengamatan

Migration nomor 5 pada daftar di atas — `RepairCanonicalModelSnapshotBaseline`, 1 September —
menunjukkan kerusakan sejenis **sudah terjadi lagi** setelah `ACC-DEP-001` ditutup, dan sudah
diperbaiki lagi oleh orang lain. Ini menguatkan bahwa gate ini bukan formalitas.

---

## 4. Tujuh pertanyaan gate

Dijawab tertulis seluruhnya, sesuai `06-shared-migration-coordination-rule.md`.

| # | Pertanyaan | Jawaban | Bukti |
|---|---|---|---|
| 1 | Apakah modul paralel sudah membuat migration? | **Finance: Tidak.** Modul lain: **Ya** | `find Areas -type d -iname '*finance*'` → 0; `grep -rl 'class Fin[A-Z]'` → 0; `DbSet<Fin` → 0; `b.ToTable("Fin*"` pada snapshot integration → 0. Sebaliknya, Radiology/Medical Record/Clinical Milestone sudah membuat migration |
| 2 | Bila sudah, apa nama migration-nya? | Finance: **tidak ada**. Modul lain: lima migration pada bagian 3 | Nama berkas lengkap tercantum di bagian 3 |
| 3 | Apakah migration itu sudah commit dan push? | **Ya** — sudah ada di `origin` | `origin/QuilvianIntegrationBackend@f90bcbe` |
| 4 | Apakah sudah merge ke canonical integration baseline? | **Ya** | `f90bcbe`, *Merge pull request #73* |
| 5 | Apakah sudah diterapkan ke shared development database? | **BELUM DIKONFIRMASI** | Tidak dapat dijawab dari repository. Database pengembangan dipakai bersama, dan aturan menuntut jawabannya dikonfirmasi manusia, bukan diasumsikan |
| 6 | Apakah `ApplicationDbContextModelSnapshot` lokal berasal dari baseline terbaru? | **❌ TIDAK** | Blob lokal `6ed4fbc`, blob integration `35fb6b6`. Selisih 530 vs 538 tabel |
| 7 | SHA baseline mana yang menjadi sumber migration ini? | Belum dapat ditetapkan | Baru dapat dijawab setelah pertanyaan 6 lulus. Kandidatnya `f90bcbe` |

**Gate gagal pada pertanyaan 6, dan pertanyaan 5 belum terjawab.** Aturan menyatakan gate gagal
bila salah satu pertanyaan tidak terjawab.

---

## 5. Migration Coordination Record

### Keadaan Accounting

| Hal | Keadaan |
|---|---|
| Entity persisted siap migration | **7** |
| `ApplicationDbContext` sudah memuat entity | **Ya** — 7 `DbSet` |
| Configuration lengkap | **Ya** — 7 |
| Snapshot berubah | **Tidak** |
| Migration dibuat | **Tidak** |
| Dependency ke Finance | **Nol** |

### Keadaan Finance

| Hal | Keadaan | Bukti |
|---|---|---|
| Modul dimulai | **Belum** | 0 folder `*finance*`, 0 kelas `Fin*` |
| Entity Finance | **Tidak ada** | 0 |
| Migration Finance | **Tidak ada** | 0 |
| Perubahan Finance pada snapshot | **Tidak ada** | 0 baris `Fin*` pada snapshot integration |
| Menjadi predecessor | **Tidak** | Tidak punya schema yang harus menjadi baseline |

### Keputusan

> **Accounting menjadi migration predecessor pertama di antara Accounting dan Finance.**
>
> **Alasan:** Finance belum mempunyai schema maupun migration yang harus menjadi baseline.
> Aturan predecessor pada `QBE-MIG-001` menyatakan siapa pun boleh lebih dahulu; yang dilarang
> adalah dua modul menghasilkan migration final **paralel** dari snapshot yang sama. Karena
> Finance belum mulai, kondisi paralel itu tidak ada.

**Batas keputusan ini.** Ia hanya menetapkan urutan **antara Accounting dan Finance**. Ia
**tidak** menjadikan Accounting predecessor terhadap seluruh repository — Radiology, Medical
Record, dan Clinical Milestone sudah lebih dahulu, dan Accounting **wajib mengikuti baseline
mereka**, bukan sebaliknya.

### Kewajiban Finance ketika mulai nanti

Ketika Finance mulai, Finance **wajib**:

1. mengambil canonical integration baseline terbaru;
2. mengambil `ApplicationDbContextModelSnapshot` terbaru — yang saat itu sudah memuat tujuh tabel
   `Acc*`;
3. membuat migration Finance **dari** baseline yang sudah memuat Accounting;
4. mencatat SHA baseline sumbernya, sesuai `QBE-MIG-002`.

Finance **tidak boleh** membuat migration dari snapshot yang lebih tua daripada migration
Accounting. Bila itu terjadi, tabel `Acc*` akan hilang dari snapshot dengan pola kerusakan yang
sama seperti bagian 3.

---

## 6. Perkiraan cakupan migration `BE-ACC-006`

Ditulis sebagai perkiraan, dan **wajib diverifikasi ulang** setelah baseline disegarkan.

### Yang diharapkan

| Operasi | Jumlah | Tabel |
|---|---:|---|
| `CreateTable` | **7** | `AccChartOfAccount`, `AccJournalType`, `AccAccountingPeriod`, `AccJournal`, `AccJournalLine`, `AccJournalApproval`, `AccNumberSeries` |
| `CreateIndex` | 16 | 5 unique + 11 non-unique |
| `AddCheckConstraint` | 1 | `CK_AccJournalLine_TepatSatuSisiTerisi` |

### Yang TIDAK boleh muncul

| Operasi | Alasan |
|---|---|
| `CreateTable` untuk `Fin*` | Finance belum punya entity |
| `CreateTable` untuk `Bil*`, AR, AP, Payment | Bukan milik Accounting; kemunculannya berarti snapshot cacat |
| `CreateTable`/`DropTable` untuk `Rad*`, `Mrc*`, `Cli*` | **Penanda paling terang bahwa baseline masih basi** — bila ini muncul, hentikan dan segarkan baseline |
| `DropTable` apa pun | Tidak ada tabel yang dihapus pada task ini |
| Foreign key ke Finance atau Payment | Nol dependency, sudah diaudit bagian 1 |

### Pemeriksaan wajib sesudah migration dibuat

Sesuai `02-backend-architecture.md` bagian 8: **hitung operasinya**. Bila jumlah `CreateTable`
bukan tepat tujuh, atau ada nama tabel di luar ketujuh `Acc*`, **migration dibatalkan dan
baseline diperiksa ulang** — jangan disunting manual.

---

## 7. Yang harus dilakukan sebelum `BE-ACC-006`

Urut, dan tidak boleh dilompati.

| # | Langkah | Pemilik | Catatan |
|---:|---|---|---|
| 1 | Konfirmasi kelima migration integration sudah diterapkan ke shared development database | Lead / pemilik modul terkait | Menjawab pertanyaan gate 5. Jangan diasumsikan |
| 2 | Segarkan `rizkiG` dari `origin/QuilvianIntegrationBackend@f90bcbe` | Owner modul (Rizki) | Merge atau rebase — **keputusan Anda**, bukan saya. Ini operasi git yang mengubah riwayat branch, jadi butuh instruksi eksplisit |
| 3 | Verifikasi snapshot lokal kini 538 tabel dan blob-nya sama dengan integration | Owner modul | Pembandingan blob, bukan perasaan |
| 4 | Pastikan build dan seluruh test masih lulus setelah penyegaran | Owner modul | 7 entity Accounting bertemu 8 tabel baru modul lain |
| 5 | Jalankan ulang gate ini, isi pertanyaan 5, 6, 7 | Owner modul | Catat SHA baseline sumbernya |
| 6 | Baru `BE-ACC-006` | Owner modul | Dengan wewenang migration yang terpisah |

**Langkah 2 tidak saya kerjakan.** Merge atau rebase mengubah riwayat branch dan berdampak ke luar
task ini; ia butuh instruksi eksplisit Anda.

---

## 8. Temuan sampingan: `ACC-DEP-007` separuh selesai

Ditemukan saat memeriksa baseline integration, dan penting untuk diketahui sebelum merge.

### Yang sudah beres

**PR #72 `b19c01e` — *Restore QBE canonical governance paths*, 2 September 2026 pukul 10:57.**
Lima baris path pada `tooling/qbe/Invoke-QbeConformanceCheck.ps1` dikembalikan ke
`docs/engineering/`, persis perbaikan yang diusulkan
[evidence/03-acc-dep-007-governance-propagation.md](03-acc-dep-007-governance-propagation.md)
bagian 11.

Ketiga berkas governance ada di `docs/engineering/` pada integration. **Checker hidup kembali** —
tidak lagi `TOOL ERROR: Canonical governance missing`.

### Yang belum beres, dan justru baru menggigit sekarang

Registry yang kini dibaca checker — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` pada
integration — memuat **48 baris dan nol baris `Acc`**. Registry canonical suite skill memuat 52
baris dan `Acc` = `ACTIVE`.

Ini persis yang diperingatkan laporan `ACC-DEP-007` bagian 8: *"repairing the path is necessary
but not sufficient — the `Acc` row must also exist in whichever registry the checker reads."*

Akibatnya berubah arah, dan perlu dipahami betul:

| Sebelum PR #72 | Sesudah PR #72 |
|---|---|
| Checker mati (`TOOL ERROR`, exit 2) | Checker hidup |
| **Tidak ada** entity yang dievaluasi | **Tujuh entity `Acc*` akan dievaluasi** |
| Merge gagal karena tooling error | Merge akan gagal karena **`QBE-MOD-002 VIOLATION`** — prefix `Acc` tidak terdaftar |

**Simulasi, bukan hasil eksekusi.** Kesimpulan ini diturunkan dari membaca logika checker
(`Get-RegistryOwnershipRows`, baris 162, yang membaca registry lalu mencocokkan Area/Module/
Prefix/Lifecycle). Checker tidak dijalankan terhadap integration karena working tree `rizkiG`
belum memuat perbaikan PR #72. **Wajib diverifikasi ulang setelah langkah 2 pada bagian 7.**

### Yang perlu dilakukan

Baris berikut perlu ditambahkan ke `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` pada
branch integration, oleh pemilik registry:

```
| Corporate | AccountingManagement / Accounting | BUSINESS DOMAIN / MODULE | Acc | ACTIVE |
```

Isinya sama persis dengan baris yang sudah berlaku di registry canonical suite skill sejak
1 September 2026. **`ACC-DEP-007` tetap terbuka**, tetapi bentuknya kini lebih sempit: bukan lagi
"checker tidak dapat jalan", melainkan "registry backend tertinggal satu pendaftaran".

Ini juga menegaskan kembali usulan pada laporan `ACC-DEP-007` bagian 11 butir 1: selama tidak ada
pemeriksaan yang mengawasi selisih antara registry backend dan registry suite, cacat yang sama
akan lahir lagi setiap kali ada pendaftaran prefix baru.

---

## 9. Konfirmasi batas

| Larangan | Hasil |
|---|---|
| `dotnet ef migrations add` | **Tidak dijalankan** |
| `dotnet ef database update` | **Tidak dijalankan** |
| `dotnet ef migrations remove` | **Tidak dijalankan** |
| `Migrations/` berubah atau bertambah | **Tidak** — 0 berkas |
| `ApplicationDbContextModelSnapshot.cs` disunting | **Tidak** |
| Shared development database disentuh | **Tidak** |
| Merge, rebase, commit, push | **Tidak** |

Seluruh isi berkas ini diperoleh dari pembacaan repository dan pembandingan git, tanpa satu pun
perintah yang mengubah keadaan.

---

## 10. Putusan ulang, 2 September 2026 — baseline `f40177a`

| Field | Isi |
|---|---|
| Baseline sumber migration | **`f40177a`** pada branch `rizkiG`, working tree bersih |
| Baseline gate sebelumnya | `2b152aa` — putusan `❌ TIDAK LULUS` di bagian *Putusan gate* |
| Canonical integration baseline | `origin/QuilvianIntegrationBackend@f90bcbe` |
| Migration yang dihasilkan | `20260902081432_AddAccountingFoundation` |
| Sifat pemeriksaan ini | **Read-only.** Nol migration dibuat, nol `dotnet ef` dijalankan, database tidak disentuh |
| Tanggal | 2 September 2026 |

### Batas kejujuran atas bagian ini

Bagian ini **bukan** klaim bahwa gate dijalankan ulang sebelum migration dibuat. Urutan yang
benar-benar terjadi: owner menjalankan langkah 2 pada bagian 7, lalu langsung membuat dan
menerapkan migration. Langkah 5 — "jalankan ulang gate ini" — dilewati.

Yang dicatat di sini adalah **verifikasi sesudahnya**, dan ia memeriksa hal yang sama persis
dengan yang akan diperiksa gate: apakah baseline sumbernya sudah memuat integration, dan apakah
hasilnya merusak snapshot modul lain. Keduanya terjawab dari bukti git, bukan dari pernyataan.

### Pertanyaan 6 — penyebab kegagalan asli

| Pemeriksaan | Perintah | Hasil |
|---|---|---|
| `f90bcbe` leluhur `HEAD` | `git merge-base --is-ancestor f90bcbe HEAD` | **Ya** |
| Jumlah commit `2b152aa..f40177a` | `git rev-list --count` | 71 |
| Berkas Accounting berubah di rentang itu | `git diff --stat` atas `Areas/`, `Repositories/Configurations/`, `Tests/` | **Nol** |

**Pertanyaan 6 terjawab: `rizkiG` tidak lagi tertinggal.** Ketertinggalan lima migration dan
delapan tabel yang menggagalkan gate pada `2b152aa` sudah hilang. **`ACC-DEP-009` CLOSED.**

### Snapshot — pemeriksaan yang paling perlu dilihat

Kekhawatiran yang tercatat di bagian 3 adalah migration dari snapshot basi akan **menghapus**
tabel modul lain, pola kerusakan `ACC-DEP-001`. Itu tidak terjadi.

| Pemeriksaan | Nilai |
|---|---:|
| Perubahan snapshot pada `f40177a` (`git show --numstat`) | **751 insertion, 0 deletion** |
| Jumlah `b.ToTable(` pada snapshot | **545** |
| Blok `Acc*` | **7** |

**Nol deletion.** Snapshot murni bertambah; tidak ada satu blok entity modul lain pun yang hilang.

### `CONTAMINATION GUARD` — LULUS, putusan `CLEAN`

Inventaris operasi lengkap ada di laporan task
[`be-acc-006-migration-pertama-dan-data-master-awal.md`](../task/report/backend/be-acc-006-migration-pertama-dan-data-master-awal.md)
bagian 5. Ringkasnya:

| Pemeriksaan | Hasil |
|---|---|
| `CreateTable` | **7**, seluruhnya `Acc*`, cocok dengan tujuh tabel yang direncanakan |
| `CreateIndex` | **21**, seluruhnya menunjuk tabel `Acc*` |
| Operasi lain di `Up()` | **Nol** |
| Operasi menyentuh tabel modul lain | **Nol** |
| `Mst*` yang muncul | `MstLegalEntity`, `MstCostCenter` — **`principalTable` foreign key**, bukan operasi schema |

Perkiraan pada bagian 6 terbukti tepat: yang diharapkan tujuh `CreateTable`, yang ditemukan tujuh.

### Keadaan enam langkah bagian 7

| # | Langkah | Keadaan |
|---:|---|---|
| 1 | Konfirmasi migration integration diterapkan ke shared database | Dilakukan owner di luar sesi agent |
| 2 | Segarkan `rizkiG` dari `f90bcbe` | **Selesai** — terbukti lewat `merge-base` |
| 3 | Verifikasi snapshot | **Selesai** — 545 tabel, 0 deletion |
| 4 | Build dan test lulus setelah penyegaran | **Selesai** — 0 error; 18 test `BE-ACC-005` tetap hijau |
| 5 | Jalankan ulang gate sebelum migration | **Dilewati** — digantikan verifikasi sesudahnya, bagian ini |
| 6 | `BE-ACC-006` | **Selesai** — migration diterapkan owner; data master awal lewat seeder |
