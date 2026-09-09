# Rencana Sinkronisasi Baseline — prasyarat `BE-ACC-006`

| Field | Isi |
|---|---|
| Tujuan | Menyiapkan pemulihan baseline `rizkiG` agar `BE-ACC-006` dapat dijalankan dengan aman |
| Sifat | **Read-only audit.** Nol `git merge`, `git rebase`, `commit`, `push`, `dotnet ef`, atau sentuhan database |
| Menutup | `ACC-DEP-009` |
| Tanggal | 2 September 2026 |
| Putusan gate sebelumnya | `evidence/04-migration-coordination-gate.md` — **TIDAK LULUS** |

> **Angka pada gate sebelumnya sudah berubah.** Saat gate dijalankan, selisihnya 5 migration.
> Sekarang **6 migration**, karena integration bergerak lagi. Ini menegaskan satu hal: selisih
> ini bertambah setiap jam, jadi menundanya membuat pekerjaannya makin besar.

---

## 1. Branch saat ini

| Field | Nilai |
|---|---|
| Branch | `rizkiG` |
| HEAD | `2c9cca8bf29764b326a5b2678dee1472c9908fde` |
| Commit terakhir | `2c9cca8` — *update*, Rizki Gunawan, 2026-09-02 12:39 |
| Working tree | **Bersih** — 0 berkas berubah |
| Snapshot | blob `6ed4fbc`, **530 tabel** |
| Terhadap `origin/rizkiG` | 74 **ahead**, 0 behind |

Enam commit yang belum ada di integration, seluruhnya pekerjaan Accounting:

| Commit | Isi |
|---|---|
| `2c9cca8` | Migration Coordination Gate record |
| `2b152aa` | `BE-ACC-005` — entity jurnal |
| `a4df550` | `BE-ACC-004` — entity periode |
| `e1ee173` | `BE-ACC-001`..`003` — enum + entity master |
| `ca6b7e0` | Blueprint Accounting |
| `aa837d7` | *merge dengan branch integration* — **commit merge** |

---

## 2. Target integration

| Field | Nilai |
|---|---|
| Branch | `origin/QuilvianIntegrationBackend` |
| SHA | `1fbda31cf6c92d4ab1311713e72c2a5557566a58` |
| Commit terakhir | `1fbda31` — *Merge pull request #75 from DevBenari/yoga*, Yoga Aji, 2026-09-02 12:36 |
| Migration terakhir | `20260902042242_AddLabOrderDiscipline` |
| Snapshot | blob `774b24c`, **538 tabel**, terakhir diubah `9312950` *updates BE modul lab* |
| Merge-base dengan `rizkiG` | `d3e7715` |

**`rizkiG` 6 ahead, 65 behind.**

---

## 3. Enam migration yang tertinggal

| # | Migration | Tanggal | Operasi | Dampak tabel |
|---:|---|---|---|---|
| 1 | `20260828093000_AddRadiologyManagement` | 28 Agu | 8 `CreateTable`, 8 `DropTable`, 26 `CreateIndex` | **+8 tabel Radiology**: `MstRadModality`, `MstRadModalitySafetyRule`, `MstRadSafetyRequirement`, `RadAcquisitionConsumption`, `RadOrder`, `RadStudy`, `RadStudySafetyCheck`, `RadTransitionHistory` |
| 2 | `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix` | 31 Agu | 4 `Sql()` | **Rename 4 tabel** `Trx*` → `Mrc*` |
| 3 | `20260831075231_AddCompanyGuarantorToPatientEncounterGuarantor` | 31 Agu | 5 `AddColumn`, 5 `DropColumn`, 2 `AddForeignKey`, 2 `CreateIndex` | Kolom pada penjamin: `CompanyGuarantorId`, `PatientCompanyGuarantorId`, tiga kolom snapshot |
| 4 | `20260901041805_RenameClinicalMilestoneFactToCliPrefix` | 1 Sep | 2 `RenameTable`, 10 `RenameIndex`, 4 `Sql()` | **Rename** `TrxClinicalMilestoneFact` → `CliClinicalMilestoneFact` |
| 5 | `20260901082243_AddBilTenderKwitansiNumber` | 1 Sep | 4 `Sql()` | Kolom `KwitansiNumber` pada `BilTender` |
| 6 | `20260902042242_AddLabOrderDiscipline` | 2 Sep | 1 `AddColumn`, 1 `DropColumn`, 1 `CreateIndex`, 1 `DropIndex` | Kolom disiplin pada order laboratorium |

### Selisih tabel snapshot: 530 → 538

| Arah | Jumlah | Tabel |
|---|---:|---|
| Ada di integration, belum di `rizkiG` | **13** | `CliClinicalMilestoneFact`, `MrcAccessLog`, `MrcClinicalDocumentIntegrity`, `MrcClinicalNoteAddendum`, `MrcClinicalNoteAuthorDelegation`, `MstRadModality`, `MstRadModalitySafetyRule`, `MstRadSafetyRequirement`, `RadAcquisitionConsumption`, `RadOrder`, `RadStudy`, `RadStudySafetyCheck`, `RadTransitionHistory` |
| Ada di `rizkiG`, tidak di integration | **5** | `TrxClinicalDocumentIntegrity`, `TrxClinicalMilestoneFact`, `TrxClinicalNoteAddendum`, `TrxClinicalNoteAuthorDelegation`, `TrxMedicalRecordAccessLog` |

Kelima tabel pada baris kedua adalah **nama lama sebelum rename** pada migration 2 dan 4.
Artinya `rizkiG` **murni tertinggal, bukan divergen** — tidak ada satu pun tabel yang hanya ada di
`rizkiG` karena pekerjaan sendiri. `530 − 5 + 13 = 538`.

Ini kabar baik: tidak ada skema yang harus didamaikan, hanya perlu disusul.

---

## 4. Konfirmasi keberadaan di origin

| Migration | `.cs` di `origin/QuilvianIntegrationBackend` | `Designer.cs` |
|---|:---:|:---:|
| `AddRadiologyManagement` | **Ada** | Tidak ada |
| `RenameMedicalRecordTrxTablesToMrcPrefix` | **Ada** | Tidak ada |
| `AddCompanyGuarantorToPatientEncounterGuarantor` | **Ada** | Ada |
| `RenameClinicalMilestoneFactToCliPrefix` | **Ada** | Ada |
| `AddBilTenderKwitansiNumber` | **Ada** | Ada |
| `AddLabOrderDiscipline` | **Ada** | Ada |

**Keenamnya sudah ada di `origin`.** Tidak ada yang masih menggantung di branch pribadi.

**Catatan, bukan blocker:** dua migration tidak punya `Designer.cs`. Ini pola yang sudah ada di
repo — dari 117 migration, 112 punya Designer, jadi 5 memang tidak. Keduanya migration
berbasis `Sql()`/rename yang ditulis tangan. `ApplicationDbContextModelSnapshot.cs` tetap menjadi
sumber model yang dipakai EF, dan ia lengkap. Dicatat untuk lead sebagai pengamatan.

---

## 5. Apakah sinkronisasi aman — dan cara mana

### Simulasi merge: **bersih, nol konflik**

Dijalankan dengan `git merge-tree --write-tree`, yang menghitung hasil merge **tanpa menyentuh
working tree, index, maupun branch mana pun**:

```bash
git merge-tree --write-tree HEAD origin/QuilvianIntegrationBackend
# exit 0 → tidak ada konflik
# tree hasil: 7ba45e8e305316d24bec7494c5601bf337cb627a
```

Isi tree hasilnya diperiksa langsung:

| Yang diperiksa | Hasil |
|---|---|
| Snapshot | blob `774b24c`, **538 tabel** — identik dengan integration |
| Ketujuh entity Accounting | **Selamat semua** |
| `DbSet` Accounting | **7**, selamat |
| Total `DbSet` | **538** = 531 milik integration + 7 milik Accounting. Gabungan bersih |
| `DbSet` modul lain | `Rad*` 5, `Mrc*` 4, `Cli*` 1, `Bil*` 28 — selamat semua |
| Checker QBE | Membaca `docs/engineering/` — **perbaikan PR #72 ikut terbawa** |

`Repositories/ApplicationDbContext.cs` disunting **kedua sisi**, dan itu berkas paling berisiko.
Simulasi membuktikan git menggabungkannya bersih: Accounting menambah region sendiri, modul lain
menambah region mereka, tidak ada baris yang bertabrakan.

### Rekomendasi: **merge, bukan rebase**

| Pertimbangan | Merge | Rebase |
|---|---|---|
| Hasil sudah terbukti | **Ya** — disimulasikan, nol konflik | Belum, dan tidak dapat disimulasikan semudah itu |
| Jumlah titik resolusi | **1** | **6** — tiap commit diputar ulang di atas 65 commit baru |
| Commit merge `aa837d7` | Dipertahankan | **Dibuang** — `git rebase` membuang commit merge kecuali `--rebase-merges` |
| Konvensi tim | **Sesuai** — 20 dari 20 commit terakhir di integration adalah merge | Menyimpang |
| Riwayat `rizkiG` | Utuh | Ditulis ulang |

**Keamanan penulisan ulang riwayat.** Keenam commit `rizkiG` terbukti **belum dipublikasikan** —
tidak satu pun ada di remote branch mana pun, dan lokal 74 ahead / 0 behind terhadap
`origin/rizkiG`. Jadi rebase pun **tidak akan merusak pekerjaan orang lain**. Yang membuat merge
tetap lebih baik bukan soal keamanan itu, melainkan tiga hal di tabel: hasilnya sudah terbukti,
titik resolusinya satu, dan commit merge `aa837d7` tidak hilang.

**Jawaban atas pertanyaan Anda:** rebase **aman** dalam arti tidak merusak riwayat bersama, tetapi
**bukan langkah yang saya rekomendasikan**. Merge lebih murah, sudah terbukti bersih, dan sejalan
dengan cara tim ini bekerja.

### Perintah yang diusulkan — **belum dijalankan**

```bash
git merge origin/QuilvianIntegrationBackend
```

Tanpa opsi apa pun. Bila git meminta pesan merge, terima bawaannya.

---

## 6. Status `ACC-DEP-007` — audit ulang terhadap `1fbda31`

### Yang sudah beres

| Yang diperiksa | Hasil |
|---|---|
| Path yang dibaca checker di integration | `docs/engineering/` pada baris 28, 29, 36, 85, 162 |
| Ketiga berkas governance ada di sana | **Ada** — `BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `QBE_EXCEPTIONS.json` |
| Checker dapat berjalan | **Ya** — tidak lagi `TOOL ERROR` |

Perbaikan PR #72 bertahan di `1fbda31`. **Bagian path dari `ACC-DEP-007` selesai.**

### Yang belum beres — masalahnya memang propagasi registry

| Registry | Baris tabel | Baris `Acc` |
|---|---:|:---:|
| `origin/QuilvianIntegrationBackend:docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | **49** | **NOL** |
| `QuilvianEngineeringSkills:agents/rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | **52** | **Ada, `ACTIVE`** |

Baris yang ada di registry suite tetapi belum sampai ke registry backend:

```
| HealthServices | LaboratoryManagement / Laboratory | BUSINESS DOMAIN / MODULE | Lab | PLANNED |
| Corporate | AccountingManagement / Accounting | BUSINESS DOMAIN / MODULE | Acc | ACTIVE |
| Acc | Accounting |
| 2026-09-01 | AccountingManagement / Accounting / `Acc` | ... pendaftaran prefix ...
| 2026-09-01 | AccountingManagement / Accounting / `Acc` | `PLANNED` → `ACTIVE` | ...
```

**Ya, masalahnya masih registry propagation** — dan sekarang terbukti bukan hanya soal Accounting:
baris `Lab` juga belum sampai. Registry backend tertinggal **dua pendaftaran**, bukan satu.

Ini menguatkan usulan pada `evidence/03-acc-dep-007-governance-propagation.md` bagian 11 butir 1:
selama tidak ada pemeriksaan yang mengawasi selisih kedua registry, cacat ini akan berulang tiap
kali ada pendaftaran prefix baru. Ia sudah berulang sekali.

### Status

| Field | Nilai |
|---|---|
| `ACC-DEP-007` | **Masih terbuka**, bentuknya menyempit |
| Dulu | Checker mati — `TOOL ERROR`, nol entity dievaluasi |
| Sekarang | Checker hidup, dan diperkirakan menolak `QBE-MOD-002` atas ketujuh entity `Acc*` |
| Pemilik | Pemilik registry / lead |
| Yang dibutuhkan | Satu baris ditambahkan ke registry backend di integration |

**Registry tidak diubah dari sini**, sesuai instruksi. Baris yang perlu ditambahkan:

```
| Corporate | AccountingManagement / Accounting | BUSINESS DOMAIN / MODULE | Acc | ACTIVE |
```

**Catatan waktu.** Ini tidak menahan sinkronisasi baseline maupun `BE-ACC-006` — keduanya
pekerjaan lokal. Ia menahan **merge Accounting ke integration**. Jadi urutannya boleh paralel:
sinkronisasi jalan sekarang, pendaftaran registry diminta ke pemilik registry bersamaan.

---

## 7. Langkah sebelum `BE-ACC-006`

| # | Langkah | Pemilik | Status |
|---:|---|---|:---:|
| 1 | Konfirmasi keenam migration sudah diterapkan ke shared development database | Lead / pemilik modul | **Belum** — menjawab pertanyaan gate 5, tidak boleh diasumsikan |
| 2 | `git merge origin/QuilvianIntegrationBackend` pada `rizkiG` | Owner modul | **Menunggu approval Anda** |
| 3 | Verifikasi snapshot menjadi 538 tabel dan blob-nya `774b24c` | Owner modul | Sesudah 2 |
| 4 | Verifikasi ketujuh entity Accounting dan 7 `DbSet` selamat | Owner modul | Sesudah 2 |
| 5 | `dotnet build` + seluruh test | Owner modul | Sesudah 2 — 965 test harus tetap lulus |
| 6 | Jalankan ulang Migration Coordination Gate, isi pertanyaan 5, 6, 7 | Owner modul | Sesudah 5 |
| 7 | Minta pemilik registry menambahkan baris `Acc` (paralel, tidak menahan 2–6) | Lead | Menahan merge ke integration saja |
| 8 | `BE-ACC-006` | Owner modul | Sesudah 6 lulus |

Langkah 3 dan 4 sudah **diprediksi berhasil** lewat simulasi pada bagian 5, tetapi tetap wajib
diverifikasi terhadap hasil nyata — simulasi bukan pengganti pemeriksaan.

---

## 8. Konfirmasi batas

| Larangan | Hasil |
|---|---|
| `git merge` | **Tidak dijalankan** |
| `git rebase` | **Tidak dijalankan** |
| `commit` / `push` | **Tidak dijalankan** |
| `dotnet ef migrations add` / `database update` | **Tidak dijalankan** |
| `ApplicationDbContextModelSnapshot.cs` disunting | **Tidak** |
| Registry diubah | **Tidak** |
| Shared database disentuh | **Tidak** |

`git merge-tree --write-tree` pada bagian 5 adalah perintah **read-only**: ia menghitung hasil
merge dan menuliskannya sebagai object lepas di database git, **tanpa** menggeser branch, HEAD,
index, maupun working tree. Diverifikasi: working tree tetap 0 berkas berubah sesudahnya.
