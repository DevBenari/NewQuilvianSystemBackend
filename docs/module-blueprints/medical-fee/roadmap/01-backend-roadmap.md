# Medical Fee — Roadmap Backend

## 1. Identitas

```yaml
roadmap_id: MDF-ROADMAP-BE-001
parent_roadmap: MDF-ROADMAP-001 revisi 5
roadmap_revision: 2
roadmap_status: ACTIVE
revision_note: >
  Revisi 2, 27 Oktober 2026, plan-module-delivery: kartu BE-MDF-015 dan BE-MDF-016 ditulis ulang
  mengikuti MDF-INTEGRATION-1.1 yang dikunci owner hari yang sama; dependency BE-MDF-016 dipindah
  dari BE-MDF-014 ke BE-MDF-010; grafik urutan dependency ditulis ulang dari nol; DoD butir 15 dan 17
  diselaraskan. Task, judul, dan acceptance criteria task lain tidak berubah.
blueprint_id: MF-BP-001
blueprint_revision: 2
blueprint_status: approved
backend_commit_sha: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
contracts:
  MDF-API-1.0: locked 2026-09-20
  MDF-STATE-1.0: locked 2026-09-20
  MDF-VAL-1.0: locked 2026-09-20
  MDF-PERM-1.0: locked 2026-09-20
  MDF-INTEGRATION-1.0: locked 2026-09-20 (dua permukaan dikecualikan)
  MDF-TEST-1.0: locked 2026-09-20
  MDF-MVP-1.0: locked 2026-09-20
  MDF-INTEGRATION-1.1: locked 2026-10-27 (§4 port DoctorShare, §5 pelaksana entri kasir, §7.3)
  MDF-API-1.1: locked 2026-10-27 (satu baris bagian 9 — endpoint sharing-rules/resolve tidak dibuat)
  MDF-TEST-1.1: locked 2026-10-27 (bagian 11A, T-91..T-107)
design_decisions_added: MDF-DES-019..021 approved 2026-10-27; MF-OQ-012 diputuskan pilihan A
```

Payung roadmap ada di `00-delivery-roadmap.md`. Pasangan frontend-nya ada di
`02-frontend-roadmap.md`.

**Berkas ini tidak menyatakan satu pun pekerjaan selesai.**

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

*Ditulis ulang dari nol pada revisi 2 (27 Oktober 2026).* Rantai teks revisi 1 tidak memuat seluruh
hubungan pada kolom `Dependency`. Karena roadmap ini berisi 19 task ditambah 2 prasyarat dari Billing,
grafiknya dipecah menjadi empat blok. Node yang sudah digambar di blok lain ditulis ulang sebagai
sambungan berlabel blok asalnya.

**Blok 1a — fondasi model dan aturan tarif**

```text
BE-MDF-001 🟡 ─> BE-MDF-002 🟡 ─┬─> BE-MDF-005 🟡 ─> BE-MDF-003 🟡 ─┬─> BE-MDF-004 🟡
                                │                                   │
                                │                                   └─> BE-MDF-006 🟡
                                │
                                └─> BE-MDF-007 🟡 ─> BE-MDF-018
```

**Blok 1b — migration perhitungan dan penyerahan**

```text
BE-MDF-003 🟡 [1a] ─┬─> BE-MDF-008 🟡 ─┬─> BE-MDF-019
                    │                  │
BE-MDF-007 🟡 [1a] ─┘                  │
                                       │
BE-MDF-018 [1a] ───────────────────────┘
```

**Blok 2a — perhitungan dan integrasi Billing**

```text
BE-MDF-008 🟡 [1b] ─┬─> BE-MDF-009 🟡 ─> BE-MDF-010 🟡 ─┬─> BE-MDF-011 🟡
                    │                                   │
BE-MDF-006 🟡 [1a] ─┘                                   ├───────────────────────┬─> BE-MDF-015 🟡
                                                        │                       │
                                                        │  BE-BKC-089 ✅ [BKC] ─┘
                                                        │
                                                        └───────────────────────┬─> BE-MDF-016 🟡
                                                                                │
                                                           BE-BKC-090 ✅ [BKC] ─┘
```

**Blok 2b — persetujuan, penyerahan, keterbukaan**

```text
BE-MDF-011 🟡 [2a] ─> BE-MDF-012 🟡 ─> BE-MDF-013 🟡 ─┬─> BE-MDF-017
                                                │
                                                └─┬─> BE-MDF-014 🟡
                                                  │
BE-MDF-019 [1b] ──────────────────────────────────┘
```

`[1a]`, `[1b]`, `[2a]` = sambungan ke node yang digambar pada blok tersebut. `[BKC]` = task backend pada
`billing-kasir/roadmap/backend-roadmap.md`, cermin baca-saja; tandanya disalin dari roadmap Billing.

Jumlah pasangan prasyarat→task: **24**, sama dengan isi kolom `Dependency` pada bagian 3. Bebas siklus.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-MDF-001` |
| 2 | `BE-MDF-001` | `BE-MDF-002` |
| 3 | `BE-MDF-002` | `BE-MDF-005`, `BE-MDF-007` — boleh paralel |
| 4 | `BE-MDF-005` / `BE-MDF-007` | `BE-MDF-003`, `BE-MDF-018` — boleh paralel |
| 5 | `BE-MDF-003`, `BE-MDF-007` | `BE-MDF-004`, `BE-MDF-006`, `BE-MDF-008` — boleh paralel |
| 6 | `BE-MDF-008`, `BE-MDF-006`, `BE-MDF-018` | `BE-MDF-009`, `BE-MDF-019` — boleh paralel |
| 7 | `BE-MDF-009` | `BE-MDF-010` |
| 8 | `BE-MDF-010`; `BE-BKC-089` ✅ dan `BE-BKC-090` ✅ [BKC] | `BE-MDF-011`, `BE-MDF-015`, `BE-MDF-016` — boleh paralel. Verifikasi proses bisnis `BE-MDF-015` menunggu `BE-BKC-091` dan eksekusi migration `{OTORISASI-MIG-3}`; bukti ujung-ke-ujung `BE-MDF-016` menunggu `BE-BKC-092`. Keduanya bukti, bukan syarat mulai |
| 9 | `BE-MDF-011` | `BE-MDF-012` |
| 10 | `BE-MDF-012` | `BE-MDF-013` |
| 11 | `BE-MDF-013`, `BE-MDF-019` | `BE-MDF-014`, `BE-MDF-017` — boleh paralel |

Gelombang di atas adalah urutan teknis. Pembagian rilis `MVP-0`..`MVP-5` dan `POST-MVP` tetap mengikuti
`04-prd-to-mvp.md` bagian 4: `BE-MDF-015` dan `BE-MDF-016` boleh dikerjakan sejak gelombang 8, tetapi
termasuk `POST-MVP` karena keduanya berada di luar batas rilis pertama.

### 2.1 Kenapa seluruh entity dan migration ada di `MVP-0`

`04-prd-to-mvp.md` bagian 5 menyebut keluaran `EPIC MDF-01` adalah "9 `DbSet`, 7 `AddScoped`,
3 migration aditif". Artinya seluruh entity, configuration, dan ketiga migration memang milik
gelombang pertama; yang masuk gelombang berikutnya hanyalah service dan API-nya.

Ini juga yang menghindari cacat urutan: `BE-MDF-003` membuat tiga tabel master sekaligus
(`MstMedicalFeeRole`, `MdfSharingAgreement`, `MdfSharingRule`), jadi entity ketiganya MUST sudah
ada sebelum migration ditulis.

### 2.2 Urutan yang tidak boleh dibalik

| Aturan | Alasan |
|---|---|
| `BE-MDF-001` sebelum file model pertama | `QBE-MOD-002`/`003`/`NAM-004`; tanpa registry, entity `Mdf*` ditolak |
| `MVP-1` sebelum `MVP-2` | Tanpa baris tarif terisi, perhitungan hanya menghasilkan `RULE_MISSING` sepanjang jumlah layanan |
| `BE-MDF-011` sebelum `BE-MDF-012` | Penutupan periode memeriksa daftar belum terhitung; tanpa daftarnya, pemeriksaan itu tidak ada isinya |
| `BE-MDF-019` sebelum `BE-MDF-014` | Service penyerahan menulis ke tabel yang migration-nya belum ada |
| `BE-MDF-010` sebelum `BE-MDF-016` | `BE-MDF-016` mengekstrak pemilihan baris tarif dari `MedicalFeeCalculationService` menjadi `MedicalFeeSharingRuleMatcher` (`MDF-DES-020`). Tanpa service itu, resolver terpaksa menyalin logikanya |
| `BE-MDF-010` sebelum `BE-MDF-015` | Perbaikan pembacaan entri kasir hanya terbukti benar lewat perhitungan periode yang memakainya |

**Koreksi dependency `BE-MDF-016`, revisi 2.** Revisi 1 menulis `BE-MDF-016` menunggu `BE-MDF-014`
(penyerahan ke Finance). Dengan bentuk port (`MDF-INTEGRATION-1.1` §4), resolver hanya membaca peran,
kesepakatan, dan baris tarif. Ia tidak menyentuh `MdfFinanceHandoff` sama sekali, jadi dependency itu
tidak punya dasar dan dipindah ke `BE-MDF-010`. Akibatnya `BE-MDF-016` naik dari gelombang setelah `MVP-4`
ke gelombang teknis 8.

## 3. Task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 🟡 `BE-MDF-001` | Modul `MedicalFeeManagement` dan prefix `Mdf` terdaftar di registry | `MDF-DES-002` | — | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Satu baris modul + prefix; `Mdf` diverifikasi belum dipakai | — | Baris tercatat sebelum file model pertama | Berkas registry ter-diff | Backend Owner — **prasyarat `QBE-MOD-002`/`003`/`NAM-004`** | Registry ter-commit terpisah dari kode |
| 🟡 `BE-MDF-002` | Entity dan configuration daftar peran | `MDF-DES-003`, `011`, `MF-DEC-016` | `MDF-VAL-1.0` §1 | `OprTeamRole` (`MF-CAP-002`) | `MstMedicalFeeRole` + configuration | `BE-MDF-001` | `RoleCode` unik; satu `OprTeamRole` dipetakan paling banyak satu peran | `T-01`..`T-06` | Backend Owner | Prefix `Mst` untuk data induk |
| 🟡 `BE-MDF-005` | Entity kesepakatan tarif dan baris tarif | `MDF-DES-008`..`010`, `MF-DEC-004`, `012` | `MDF-VAL-1.0` §2–3 | `WfpContractHistory` (`MF-CAP-009`), `MstTariff` | `MdfSharingAgreement`, `MdfSharingRule` + configuration | `BE-MDF-002` | FK `Restrict` ke kontrak HR; persentase 0..100 sebagai check constraint | `T-07`..`T-13` | Backend Owner | Menunjuk kontrak HR, **tidak menyalin** |
| 🟡 `BE-MDF-007` | Entity periode, hasil jasa, rincian, koreksi, daftar belum terhitung | `MDF-DES-010`, `012`, `013`, `014` | `MDF-VAL-1.0` §4–6 | — | `MdfFeePeriod`, `MdfServiceFee`, `MdfServiceFeeDetail`, `MdfServiceFeeAdjustment`, `MdfUnresolvedService` + configuration | `BE-MDF-002` | `FinalAmount = GrossAmount + AdjustmentAmount` sebagai check constraint; satu penerima satu hasil per periode | `T-26`, `T-30` | Backend Owner | `SharingRuleId` wajib — setiap rupiah tertelusur |
| `BE-MDF-018` | Entity penyerahan ke Finance | `MDF-DES-018` | `MDF-VAL-1.0` §7 | Pola `BilArHandoff` | `MdfFinanceHandoff` + configuration | `BE-MDF-007` | `ServiceFeeId` dan `HandoffKey` masing-masing unik | `T-73`, `T-76` | Backend Owner | Empat kolom disalin dari hasil jasa agar Finance tidak perlu join |
| 🟡 `BE-MDF-003` | Migration `AddMedicalFeeMasterData` | `MDF-DES-001` | — | — | 3 tabel, aditif | `BE-MDF-005` | `Up()` membuat 3 tabel; `Down()` bersih | Migration dijalankan di lingkungan pengembangan | **Otorisasi terpisah wajib** (`AGENTS.md`) | Nol tabel modul lain tersentuh |
| 🟡 `BE-MDF-008` | Migration `AddMedicalFeeCalculation` | `MDF-DES-001` | — | — | 5 tabel, aditif | `BE-MDF-007`, `BE-MDF-003` | Urutan setelah migration 1 | Migration dijalankan | **Otorisasi terpisah wajib** | Aditif |
| `BE-MDF-019` | Migration `AddMedicalFeeFinanceHandoff` | `MDF-DES-001` | — | — | 1 tabel, aditif | `BE-MDF-018`, `BE-MDF-008` | Urutan setelah migration 2 | Migration dijalankan | **Otorisasi terpisah wajib** | Aditif |
| 🟡 `BE-MDF-004` | API daftar peran | `MF-DEC-016` | `MDF-API-1.0` §2, `MDF-PERM-1.0` | `ApiResponse<T>`, `[AccessPermission]` | `MedicalFeeRoleService`, `MedicalFeeRolesController` | `BE-MDF-003` | Peran terpakai dinonaktifkan, bukan dihapus | `T-05`, `T-06` | Backend Owner | Route `api/v1/health-services/medical-fee-management/...` |
| 🟡 `BE-MDF-006` | Layanan kesepakatan tarif berversi | `MDF-DES-009`, `MF-DEC-004` | `MDF-STATE-1.0` §1, `MDF-API-1.0` §3 | — | `MedicalFeeSharingService`, `MedicalFeeSharingAgreementsController` | `BE-MDF-003` | Mengubah tarif membuat baris baru; hasil periode lalu tidak bergeser; jumlah peran ≤ 100% | `T-14`..`T-19` | Backend Owner | Baris lama tidak pernah diubah |
| 🟡 `BE-MDF-009` | Adapter sumber layanan | `MDF-DES-011`, `MF-DEC-003`, `015` | `MDF-INTEGRATION-1.0` §2 | `OprTeamMember`, `TrxPatientProcedure`, `LabOrder` (`MF-CAP-002`, `005`, `006`) | `MedicalFeeServiceSourceAdapter` — kamar operasi (tim penuh), klinis dan laboratorium (pelaksana tunggal) | `BE-MDF-008`, `BE-MDF-006` | Operasi bertim tiga orang → tiga rincian; radiologi diabaikan sepenuhnya | `T-25`, `T-38`, `T-39` | Backend Owner. **Bagian tim di luar kamar operasi tertahan `MF-CQ-07`** | Perbedaan antar sumber terisolasi di satu berkas |
| 🟡 `BE-MDF-010` | Mesin perhitungan periode | `MDF-DES-007`, `012`, `013`, `015` | `MDF-VAL-1.0` §5 | — | `MedicalFeeCalculationService`, `MedicalFeePeriodService` | `BE-MDF-009` | Basis nilai kotor, tidak berkurang diskon; snapshot persentase; hitung ulang mengganti bukan menggandakan | `T-20`..`T-24`, `T-27`..`T-37` | Backend Owner | `Serializable` + `Idempotency-Key` |
| 🟡 `BE-MDF-011` | Daftar layanan belum dapat dihitung | `MDF-DES-014`, `016`, `MF-DEC-018` | `MDF-API-1.0` §6 | — | `MedicalFeeUnresolvedServicesController` beserta rekap per alasan | `BE-MDF-010` | Enam alasan tercatat; menahan penutupan periode; pengesampingan menuntut catatan | `T-39`..`T-49` | Backend Owner | **Tanpa** endpoint penghapus |
| 🟡 `BE-MDF-012` | Siklus verifikasi, persetujuan, penutupan | `MDF-DES-016`, `017`, `MF-DEC-009` | `MDF-STATE-1.0` §1–2, `MDF-PERM-1.0` §3 | Pola `FIN-DES-014` | Transisi status periode dan hasil jasa, pemisahan wewenang tiga lapis | `BE-MDF-011` | Pemverifikasi ditolak saat menyetujui, **termasuk lewat jalur yang melewati service** | `T-50`..`T-63` | Backend Owner | Check constraint maker-checker terpasang |
| 🟡 `BE-MDF-013` | Koreksi berjenjang | `MF-DEC-007`, `MDF-DES-017` | `MDF-STATE-1.0` §3 | — | `MedicalFeeAdjustmentService` beserta endpoint-nya | `BE-MDF-012` | `RequestedBy <> ApprovedBy`; koreksi final tidak dapat diubah; `FinalAmount` tidak negatif | `T-64`..`T-70` | Backend Owner | Pembatalan lewat koreksi berlawanan, bukan pengubahan |
| 🟡 `BE-MDF-014` | Penyerahan ke Finance — service dan API | `MDF-DES-018`, `MF-DEC-005`, `008` | `MDF-INTEGRATION-1.0` §7 | Pola `BilArHandoff` | `MedicalFeeHandoffService`, `MedicalFeeHandoffsController` | `BE-MDF-013`, `BE-MDF-019` | Ditulis di transaksi yang sama dengan persetujuan; nilai kotor; `HandoffKey` tetap saat dikirim ulang | `T-71`..`T-79` | Backend Owner. **Membuka Finance `BE-FIN-021`** | Medical Fee tidak pernah menulis ke tabel Finance |
| `BE-MDF-017` | Pembatasan data untuk tenaga medis | `MF-DEC-010`, `MDF-PERM-1.0` §5 | `MDF-PERM-1.0` | — | Penyaringan `PayeeReferenceId` di lapis service | `BE-MDF-013` | Dokter A meminta data dokter B menerima kosong; akses id langsung menghasilkan `404` bukan `403` | `T-80`..`T-84` | Backend Owner — **risiko privasi nyata**: satu izin `View` tanpa saringan membuka penghasilan ±220 orang | Pembatasan data, bukan sekadar pembatasan endpoint |
| 🟡 `BE-MDF-015` | Jasa dari entri kasir berjasa terhitung dengan pelaksana, peran, dan tanggal yang benar | `MF-DEC-017`; `MDF-DES-021`; `MF-OQ-012` pilihan A | `MDF-INTEGRATION-1.1` §5 — **locked** 27 Oktober 2026; `BIL-INTEGRATION-1.9` `BIL-INT-024` | `MedicalFeeServiceSourceAdapter.ExtractAdhocBillingServices` — **source awal sudah ada** dari `BE-MDF-009` beserta celah G1–G4 (`integration-contract.md` §5.6); kolom `BilInvoiceItem` dari `BE-BKC-089` ✅ | Memperbaiki pembacaan `ADHOC`/`ADHOC_CATALOG` di adapter: `PerformerType` dipetakan ke `MdfPayeeTypes`, kosong atau lain → `PERFORMER_MISSING` (G1); `PerformerRoleCode` dipakai, tak dikenal → `ROLE_UNMAPPED` (G2); tanggal layanan dari `SourceOccurredAt` dalam tanggal Asia/Jakarta (G3); komentar source yang mengaku §5 sudah terkunci dibetulkan (G4); entri tanpa tarif memakai baris tarif bawaan peran. Nol tabel, nol migration, nol endpoint | `BE-MDF-010`, `BE-BKC-089` ✅ [BKC] | 1. Baris `IsFeeBearing = false` tidak dihitung dan tidak dicatat `PERFORMER_MISSING`. 2. Entri berjasa `DOCTOR` menghasilkan hasil jasa ber-`PayeeType` `Doctor` dengan nomor berawalan `DOC`. 3. `PerformerRoleCode` terisi memakai peran itu; kode tak dikenal atau nonaktif → `ROLE_UNMAPPED`. 4. Layanan pukul 23.30 WIB tercatat pada tanggal WIB, bukan tanggal UTC. 5. Entri bebas tanpa tarif dihitung dengan baris tarif bawaan peran; tanpa baris itu → `RULE_MISSING` | QBE preflight; review diff; `dotnet build`; penelusuran aturan pada source dengan contoh berangka untuk skenario `T-103`..`T-107`. Verifikasi proses bisnis dengan data entri kasir nyata menunggu `BE-BKC-091` dan eksekusi migration `{OTORISASI-MIG-3}` milik Billing — bukti, bukan syarat mulai | Backend Owner. Risiko sedang: menyentuh perhitungan uang, dan adapter yang **sudah ada** hari ini menghasilkan `PayeeType` `DOCTOR` | Celah G1–G4 tertutup; laporan task |
| 🟡 `BE-MDF-016` | Billing menerima persentase jasa dari Medical Fee lewat port, dengan baris tarif yang sama dengan perhitungan periode | `MF-DEC-001`; `MDF-DES-019`, `020` | `MDF-INTEGRATION-1.1` §4 — **locked** 27 Oktober 2026; `MDF-API-1.1` bagian 9; `BIL-INTEGRATION-1.9` `BIL-INT-023` | Port `IBillingDoctorShareResolver` dan `NullBillingDoctorShareResolver` (`BE-BKC-090` ✅); pemilihan baris tarif di `MedicalFeeCalculationService` (`BE-MDF-010`) | 1. Ekstrak pemilihan baris tarif ke `MasterData/Services/MedicalFeeSharingRuleMatcher.cs` tanpa mengubah perilaku perhitungan periode. 2. `MasterData/Services/MedicalFeeDoctorShareResolver.cs` mengimplementasikan port. 3. `Program.cs`: `AddScoped<MedicalFeeSharingRuleMatcher>()` dan `services.Replace(ServiceDescriptor.Scoped<IBillingDoctorShareResolver, MedicalFeeDoctorShareResolver>())` **setelah** pemanggilan terakhir `AddBillingManagement()`. **Bukan** endpoint HTTP. Nol tabel, nol migration | `BE-MDF-010`, `BE-BKC-090` ✅ [BKC] | 1. Jawaban mengikuti `integration-contract.md` §4.4 langkah 1–7 (`Resolved` / `NoRule` / `Unavailable`). 2. Tidak ada exception yang keluar; token dibatalkan → `Unavailable`. 3. Baca saja: `AsNoTracking`, tanpa `SaveChanges`, tanpa transaksi, paling banyak tiga query. 4. Container menghasilkan `MedicalFeeDoctorShareResolver` untuk port, dan hanya satu pendaftaran tersisa. 5. Untuk layanan yang sama, baris tarif jawaban port sama dengan baris tarif perhitungan periode. 6. Log tanpa persentase, nilai, atau nama penerima | QBE preflight; review diff; `dotnet build`; penelusuran pendaftaran DI di `Program.cs`; penelusuran source untuk skenario `T-91`..`T-102`. Bukti ujung-ke-ujung dari pencatatan tagihan menunggu `BE-BKC-092` — bukti, bukan syarat mulai | Backend Owner. **Risiko tinggi pada jalur Billing**: salah pendaftaran membuat `DoctorShare` nol diam-diam; query yang tidak menghormati token membuat `DbContext` Billing gagal | `integration-contract.md` §4.5 butir 1–7 terpenuhi; laporan task |

### 3.1 Register status task

Arti tanda status ada pada legenda sebelum grafik urutan dependency.

| Task | Status | Laporan |
| --- | --- | --- |
| `BE-MDF-001` | 🟡 **SEBAGIAN 26 Oktober 2026.** Tiga dari tiga acceptance criteria terpenuhi: baris `Mdf` tercatat di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` sebelum file model pertama, prefix `Mdf` terverifikasi belum dipakai, dan berkas registry ter-diff (1 berkas, +3 baris). Satu butir DoD belum terpenuhi: "Registry ter-commit terpisah dari kode" — menunggu pemilik, karena commit tidak dilakukan agent. Checker QBE `NOT RUN` (`pwsh` tidak terpasang) dan `dotnet build` `NOT RUN` (tidak ada source berubah; pemilik meminta tanpa build otomatis). Peringatan terbuka untuk `BE-MDF-005`: sub-folder `MasterData` bentrok dengan baris `Mst` pada pencocokan pemilik | [BE-MDF-001](../task/report/backend/BE-MDF-001.md) |
| `BE-MDF-002` | 🟡 **SEBAGIAN 26 Oktober 2026.** Dua dari dua acceptance criteria terpetakan ke source: `MstMedicalFeeRole` dan `MstMedicalFeeRoleConfiguration` dengan dua unique index parsial (`RoleCode`; `OprTeamRoleMapping` non-NULL), serta satu `DbSet`. Penegakan pada basis data belum terbukti karena migration `BE-MDF-003` belum ada. Belum terpenuhi: `T-01`..`T-06` `NOT RUN` (menuntut `BE-MDF-003` dan `BE-MDF-004`), dan `dotnet build` `NOT RUN` atas permintaan pemilik — **kode belum pernah dikompilasi**. Peringatan: model EF kini punya perubahan tanpa migration (`PendingModelChangesWarning` pada `dotnet ef database update`) sampai `BE-MDF-003` dibuat. Dependency `BE-MDF-001` masih 🟡 | [BE-MDF-002](../task/report/backend/BE-MDF-002.md) |
| `BE-MDF-005` | 🟡 **SEBAGIAN 26 Oktober 2026.** Dua dari dua acceptance criteria terpetakan ke source: `MdfSharingAgreement` dan `MdfSharingRule` dengan FK `Restrict` ke `WfpContractHistory`, check constraint persentase 0..100, dua `DbSet`, serta relasi navigasi. Penegakan pada basis data belum terbukti karena migration `BE-MDF-003` belum ada. Belum terpenuhi: `T-07`..`T-13` `NOT RUN` (menuntut migration `BE-MDF-003` dan service `BE-MDF-006`), dan `dotnet build` `NOT RUN` atas permintaan pemilik — **kode belum pernah dikompilasi**. Subfolder `MasterData` diproteksi pengecualian resmi `EXP-QBE-MOD-002-MDF-001` dan `002` di `QBE_EXCEPTIONS.json`. Dependency `BE-MDF-002` masih 🟡 | [BE-MDF-005](../task/report/backend/BE-MDF-005.md) |
| `BE-MDF-003` | 🟡 **SEBAGIAN 26 Oktober 2026.** Dua dari dua acceptance criteria terpenuhi pada source migration: `Up()` menciptakan 3 tabel master data (`MstMedicalFeeRole`, `MdfSharingAgreement`, `MdfSharingRule`) lengkap dengan 5 check constraint, 6 foreign key, 9 indeks, dan presisi persentase `(5, 2)`; `Down()` menghapus ketiga tabel secara bersih dalam urutan dependensi terbalik (`MdfSharingRule` -> `MdfSharingAgreement` -> `MstMedicalFeeRole`). Satu butir DoD terpenuhi: nol tabel modul lain disentuh. Butir belum terpenuhi: eksekusi migration pada basis data belum dijalankan karena menuntut otorisasi terpisah (`AGENTS.md`) dan `dotnet build` `NOT RUN` atas instruksi eksplisit pemilik — **kode belum dikompilasi dan belum diaplikasikan ke database runtime**. Dependency `BE-MDF-005` masih 🟡 | [BE-MDF-003](../task/report/backend/BE-MDF-003.md) |
| `BE-MDF-004` | 🟡 **SEBAGIAN 26 Oktober 2026.** Dua dari dua acceptance criteria terpenuhi pada source: `MedicalFeeRolesController` dan `MedicalFeeRoleService` menyediakan 9 endpoint baseline (termasuk metadata filter, summary, list paged, options feed, single detail, create, full update, toggle activation, dan soft delete). Penolakan hapus peran yang sudah digunakan baris tarif terpasang (HTTP 422 `MDF_ROLE_IN_USE`). Izin `[AccessPermission]` dan registrasi DI di `Program.cs` terpasang. Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi skenario uji `T-01`..`T-06` belum dijalankan — **kode belum dikompilasi**. Dependency `BE-MDF-003` masih 🟡 | [BE-MDF-004](../task/report/backend/BE-MDF-004.md) |
| `BE-MDF-006` | 🟡 **SEBAGIAN 26 Oktober 2026.** Dua dari dua acceptance criteria terpenuhi pada source: `MedicalFeeSharingAgreementsController` dan `MedicalFeeSharingService` mengelola kesepakatan berversi dan aturan tarif; operasi supersede membentuk baris baru dan menutup baris lama tanpa menimpa nilai historis; validasi jumlah persentase peran `<= 100%` (termasuk nilai seperti 85% diterima sah). Validasi kecocokan kontrak HR (`WfpContractHistory`), proteksi tanggal kontrak, aktivasi, dan terminasi terpasang. Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi skenario uji `T-07`..`T-19` belum dijalankan — **kode belum dikompilasi**. Dependency `BE-MDF-003` masih 🟡 | [BE-MDF-006](../task/report/backend/BE-MDF-006.md) |
| `BE-MDF-007` | 🟡 **SEBAGIAN 26 Oktober 2026.** Tiga dari tiga acceptance criteria terpenuhi pada source: 5 model entity (`MdfFeePeriod`, `MdfServiceFee`, `MdfServiceFeeDetail`, `MdfServiceFeeAdjustment`, `MdfUnresolvedService`) dan 5 konfigurasi EF Core terpasang dengan check constraint `FinalAmount = GrossAmount + AdjustmentAmount`, partial unique index satu penerima satu hasil per periode, kolom `SharingRuleId` wajib terisi pada rincian, check constraint maker-checker, serta 5 `DbSet` terdaftar di `ApplicationDbContext`. Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi skenario uji `T-26`, `T-30` belum dijalankan — **kode belum dikompilasi**. Prasyarat `BE-MDF-008` kini terpenuhi. Dependency `BE-MDF-002` masih 🟡 | [BE-MDF-007](../task/report/backend/BE-MDF-007.md) |
| `BE-MDF-008` | 🟡 **SEBAGIAN 26 Oktober 2026.** Tiga dari tiga acceptance criteria terpenuhi pada source: berkas migrasi C# `20261026140000_AddMedicalFeeCalculation` mendefinisikan 5 tabel transaksi kalkulasi lengkap dengan 17 indeks, 17 check constraints, 6 foreign keys, dan metode `Down()` bersih dalam urutan terbalik. Urutan timestamp berada setelah migration 1; nol tabel modul lain disentuh. Butir belum terpenuhi: eksekusi migration pada basis data belum dijalankan karena menuntut otorisasi terpisah (`AGENTS.md`) dan `dotnet build` `NOT RUN` atas instruksi pemilik — **kode belum dikompilasi dan belum diaplikasikan ke database runtime**. Prasyarat `BE-MDF-009` kini terpenuhi. Dependency `BE-MDF-007` dan `BE-MDF-003` masih 🟡 | [BE-MDF-008](../task/report/backend/BE-MDF-008.md) |
| `BE-MDF-009` | 🟡 **SEBAGIAN 26 Oktober 2026.** Tiga dari tiga acceptance criteria terpenuhi pada source: `MedicalFeeServiceSourceAdapter` dan DTO `AdaptedServiceItem` mengisolasi perbedaan sumber; operasi bertim tiga orang menghasilkan tiga rincian mandiri (`T-25`); radiologi diabaikan sepenuhnya tanpa dicatat ke unresolved (`T-38`); pesanan lab tanpa pemeriksa ditandai `PERFORMER_MISSING` (`T-39`); nilai kotor diambil sebelum diskon dari Billing; dan registrasi DI di `Program.cs` terpasang. Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi skenario uji `T-25`, `T-38`, `T-39` belum dijalankan — **kode belum dikompilasi**. Prasyarat `BE-MDF-010` kini terpenuhi. Dependency `BE-MDF-008` dan `BE-MDF-006` masih 🟡 | [BE-MDF-009](../task/report/backend/BE-MDF-009.md) |
| `BE-MDF-010` | 🟡 **SEBAGIAN 26 Oktober 2026.** Seluruh kriteria penerimaan terpenuhi pada source: `MedicalFeeCalculationService` mengeksekusi kalkulasi dalam transaksi serializable ber-idempotensi; basis nilai kotor (`BaseAmount`) sebelum diskon; snapshot aturan tarif (`SharingRuleId`) dan pembulatan 2 desimal `AwayFromZero`; hitung ulang mengganti bukan menggandakan (`Calculated` membersihkan rincian lama dan unresolved status `Open`); penanganan 6 alasan unresolved (`MdfUnresolvedService`); `MedicalFeePeriodService` mengelola siklus hidup periode, Maker-Checker verifikasi/approval (`ApprovedBy != VerifiedBy`), dan proteksi penutupan periode tidak boleh ada unresolved status `Open` (`MDF_PERIOD_HAS_UNRESOLVED`); `MedicalFeePeriodsController` mengimplementasikan 11 endpoint dengan otorisasi `[AccessPermission]`; dan registrasi DI di `Program.cs` terpasang. Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi skenario uji `T-20`..`T-24`, `T-27`..`T-37` belum dijalankan — **kode belum dikompilasi**. Prasyarat `BE-MDF-011` kini terpenuhi. Dependency `BE-MDF-009` masih 🟡 | [BE-MDF-010](../task/report/backend/BE-MDF-010.md) |
| `BE-MDF-011` | 🟡 **SEBAGIAN 27 Oktober 2026.** Seluruh kriteria penerimaan terpenuhi pada source: `MedicalFeeUnresolvedServicesController` dan DTO `MedicalFeeUnresolvedServiceDtos` menyediakan 5 endpoint worklist transaksi (metadata filter, summary per alasan dan modul sumber, daftar terpaginasi dengan pencarian ILike, detail tunggal, dan waiver pengesampingan). Enam alasan unresolved resmi diakomodasi; pengesampingan menuntut catatan minimal 10 karakter (`MDF_WAIVER_NOTE_REQUIRED`) dan izin tersendiri `[AccessPermission("MedicalFeeUnresolvedServices", "Waive")]`; rekonsiliator kalkulasi ulang memperbarui layanan terselesaikan menjadi `Resolved` (`T-45`) dan mencegah duplikasi data (`T-44`); serta endpoint `DELETE` ditiadakan secara sengaja demi menjaga jejak audit (`T-49`). Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi skenario uji runtime `T-39`..`T-49` belum dijalankan — **kode belum dikompilasi**. Prasyarat `BE-MDF-012` kini terpenuhi. Dependency `BE-MDF-010` masih 🟡 | [BE-MDF-011](../task/report/backend/BE-MDF-011.md) |
| `BE-MDF-012` | 🟡 **SEBAGIAN 27 Oktober 2026.** Seluruh kriteria penerimaan terpenuhi pada source: `MedicalFeeServiceFeesController` dan DTO `MedicalFeeServiceFeeDtos` menyediakan 7 endpoint hasil jasa dan audit akuntansi per rincian tindakan (metadata filter, list terpaginasi, detail tunggal, rincian tindakan per layanan / breakdown pembentuk nominal, verifikasi per individu, pengembalian status ke Calculated, dan verifikasi massal per periode / batch verification). Aturan validasi status terpasang: Rule 4.7 (`T-55`) mencegah verifikasi periode bila masih ada hasil jasa Calculated; Rule 4.8 (`T-59`) dan check constraint database `CK_MdfFeePeriod_Maker` & `CK_MdfServiceFee_Maker` (`T-61`) menegakkan Maker-Checker tiga lapis bahwa ApprovedBy tidak boleh sama dengan VerifiedBy; Rule 4.11 (`T-52`) mengunci permanen periode Closed terhadap mutasi apa pun; serta sinkronisasi izin `[AccessPermission]` pada `MedicalFeePeriodsController` memisahkan izin Verify, Approve, dan Close. Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi skenario uji runtime `T-50`..`T-63` belum dijalankan — **kode belum dikompilasi**. Prasyarat `BE-MDF-013` kini terpenuhi. Dependency `BE-MDF-011` masih 🟡 | [BE-MDF-012](../task/report/backend/BE-MDF-012.md) |
| `BE-MDF-013` | 🟡 **SEBAGIAN 27 Oktober 2026.** Seluruh kriteria penerimaan terpenuhi pada source: `MedicalFeeAdjustmentService`, `MedicalFeeAdjustmentsController`, dan ekstensi 5 endpoint pada `MedicalFeeServiceFeesController` mengelola siklus hidup koreksi penyesuaian berjenjang (`Create`, `Approve`, `Reject`, `GetList`, `GetById`, `GetMetadata`). Prinsip imutabilitas angka historis terpenuhi: nilai kotor `GrossAmount` tidak disentuh, perubahan nominal melalui akumulasi `AdjustmentAmount` (`FinalAmount = GrossAmount + AdjustmentAmount`, Rule 6.1, `T-64`). Maker-Checker ditegakkan dua lapis: pemeriksaan service `RequestedBy <> ApprovedBy` (`MDF_MAKER_CHECKER_VIOLATION`, `T-60`) dan database check constraint `CK_MdfServiceFeeAdjustment_Maker` (`T-61`); proteksi status final `Approved`/`Rejected` permanen (`MDF_ADJUSTMENT_STATE_INVALID`, `T-68`); penolakan wajib beralasan (`T-69`); validasi nominal > 0 dan alasan wajib 10–500 karakter (`T-65`, `T-66`); proteksi nilai akhir non-negatif `FinalAmount >= 0` (`T-67`); serta perlindungan eksekusi ganda dengan `Idempotency-Key` (`T-70`). Pendaftaran DI di `Program.cs` terpasang. Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi skenario uji runtime `T-64`..`T-70` belum dijalankan — **kode belum dikompilasi**. Prasyarat `BE-MDF-014` dan `BE-MDF-017` kini terpenuhi. Dependency `BE-MDF-012` masih 🟡 | [BE-MDF-013](../task/report/backend/BE-MDF-013.md) |
| `BE-MDF-014` | 🟡 **SEBAGIAN 27 Oktober 2026.** Seluruh kriteria penerimaan terpenuhi pada source: `MdfFinanceHandoff` dan konfigurasinya mengelola integrasi penyerahan hasil jasa ke Finance; `MedicalFeeHandoffService` dan `MedicalFeeHandoffsController` menyediakan endpoint penerimaan ACK / NACK dari Finance; persetujuan periode secara otomatis membuat handoff dalam transaksi yang sama; perlindungan idempotensi penyerahan; serta registrasi DI di `Program.cs` terpasang. Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi skenario uji runtime `T-71`..`T-79` belum dijalankan — **kode belum dikompilasi**. Dependency `BE-MDF-013` masih 🟡 | [BE-MDF-014](../task/report/backend/BE-MDF-014.md) |
| `BE-MDF-015` | 🟡 **SEBAGIAN 27 Oktober 2026.** Seluruh kriteria penerimaan terpenuhi pada source: `MedicalFeeServiceSourceAdapter.ExtractAdhocBillingServices` diperbaiki menutup celah G1–G4 (`MDF-INTEGRATION-1.1` §5); pemetaan `PerformerType` ke `MdfPayeeTypes.Doctor`/`Workforce` (G1); pemanfaatan `PerformerRoleCode` (G2); normalisasi waktu layanan `SourceOccurredAt` ke jam dinding Asia/Jakarta (WIB) (G3); komentar revisi diperbarui (G4); serta entri bebas tanpa tarif dihitung dengan aturan bawaan peran (`MF-OQ-012` pilihan A, `T-107`). Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi data kasir riil menunggu `BE-BKC-091` serta migration `{OTORISASI-MIG-3}` milik Billing — **kode belum dikompilasi**. Dependency `BE-MDF-010` dan `BE-BKC-089` ✅ | [BE-MDF-015](../task/report/backend/BE-MDF-015.md) |
| `BE-MDF-016` | 🟡 **SEBAGIAN 27 Oktober 2026.** Seluruh kriteria penerimaan terpenuhi pada source: `MedicalFeeSharingRuleMatcher` mengekstrak pemilihan baris tarif berjenjang (spesifisitas 4=Tariff, 3=Category, 2=Role Default) tanpa mengubah logika kalkulasi periode (`T-102`); `MedicalFeeDoctorShareResolver` mengimplementasikan port `IBillingDoctorShareResolver` dan mematuhi 7 kewajiban teknis (`MDF-INTEGRATION-1.1` §4.5): zero-exception guarantee (`T-99`), pembatalan CancellationToken (`T-99`), read-only AsNoTracking tanpa transaksi (`T-100`), maksimal 3 kueri terindeks, privasi log tanpa nominal/persentase/nama dokter; registrasi DI di `Program.cs` menggunakan `services.Replace()` setelah `AddBillingManagement()` (`T-101`); serta penanganan skenario `Resolved`, `NoRule` (`T-93`..`T-96`), `Unavailable` (`T-97`, `T-98`). Butir belum terpenuhi: `dotnet build` `NOT RUN` atas instruksi pemilik dan verifikasi ujung-ke-ujung menunggu `BE-BKC-092` pada modul Billing — **kode belum dikompilasi**. Prasyarat terpenuhi. Dependency `BE-MDF-010` dan `BE-BKC-090` ✅ | [BE-MDF-016](../task/report/backend/BE-MDF-016.md) |

## 4. Rincian per gelombang

### `MVP-0` — Fondasi (`EPIC MDF-01`, `MDF-02`)

| Aspek | Isi |
|---|---|
| Tabel baru | Seluruh 9: 1 `Mst*` + 8 `Mdf*` |
| Migration | Ketiganya — `AddMedicalFeeMasterData`, `AddMedicalFeeCalculation`, `AddMedicalFeeFinanceHandoff` |
| Pendaftaran di luar modul | 9 `DbSet` di `ApplicationDbContext`, 7 `AddScoped` di `Program.cs` |
| Selesai bila | Ketiga migration berjalan; seluruh check constraint terpasang; `T-01`..`T-06` lulus |
| Yang mudah salah | Menaruh EF configuration di dalam `Areas/`. `MDF-DES-001` menempatkannya di `Repositories/Configurations/HealthServices/MedicalFeeManagement/` — perhatikan `HealthServices` **jamak** |

### `MVP-1` — Aturan tarif (`EPIC MDF-03`)

| Aspek | Isi |
|---|---|
| Selesai bila | `T-14`..`T-19` lulus, terutama `T-17`: mengganti tarif menutup baris lama dan mengisi `SupersededByRuleId` |
| Yang mudah salah | Memperbarui persentase baris yang ada. `MDF-DES-009` mewajibkan **baris baru**; menimpa akan menggeser hasil periode yang sudah dihitung |
| Catatan | `T-16` menguji jumlah persentase 85% **harus diterima** — aturannya `≤ 100`, bukan `= 100`. Sisanya porsi rumah sakit |

### `MVP-2` — Perhitungan (`EPIC MDF-04`, `MDF-05` sebagian, `MDF-06`)

| Aspek | Isi |
|---|---|
| Selesai bila | `T-20`..`T-49` lulus |
| Yang mudah salah | Mengurangkan diskon dari `BaseAmount`. `MDF-DES-012` memakai nilai **kotor**; `T-27` dan `T-28` menjaga ini |
| Yang mudah salah kedua | Melewati layanan tanpa pelaksana secara diam-diam. `MDF-DES-014` mewajibkan ia masuk `MdfUnresolvedService` — persis masalah yang modul ini ingin hentikan |
| Tertahan sebagian | Pembagian tim di luar kamar operasi menunggu `MF-CQ-07`. Perilaku pelaksana tunggal sudah dirancang dan **tidak** tertahan |

### `MVP-3` — Persetujuan dan koreksi (`EPIC MDF-08`, `MDF-09`)

| Aspek | Isi |
|---|---|
| Selesai bila | `T-50`..`T-70` lulus |
| Uji yang paling berharga | `T-61` — pengaju menyetujui koreksinya sendiri **langsung ke repository**, melewati service. Check constraint MUST tetap menolak. Inilah yang membuktikan lapis ketiga benar-benar ada |

### `MVP-4` — Penyerahan ke Finance (`EPIC MDF-10`)

| Aspek | Isi |
|---|---|
| Selesai bila | `T-71`..`T-79` lulus, terutama `T-78`: handoff dan persetujuan gagal bersama saat transaksi dibatalkan |
| Dampak lintas modul | **Membuka `BE-FIN-021`** di Finance. Selama task ini belum ada, `FIN-CAP-021` tetap `Missing` |

### `MVP-5` — Keterbukaan (`EPIC MDF-12`)

| Aspek | Isi |
|---|---|
| Selesai bila | `T-80`..`T-84` lulus |
| Yang mudah salah | Mengembalikan `403` untuk id milik orang lain. `T-82` menuntut `404` — `403` atas id tertentu membocorkan bahwa hasil jasa dengan id itu ada |

### `POST-MVP` — Integrasi Billing (`EPIC MDF-07`, `MDF-11`)

Kedua epic berada di luar batas rilis pertama (`04-prd-to-mvp.md` bagian 2). Sejak `MDF-INTEGRATION-1.1`
dikunci 27 Oktober 2026 keduanya **tidak lagi tertahan keputusan**; secara teknis boleh dikerjakan sejak
gelombang 8, paralel dengan `BE-MDF-011`. Memasukkannya ke rilis pertama adalah keputusan pemilik produk,
bukan keputusan roadmap ini.

| Aspek | Isi |
|---|---|
| Selesai bila | `T-91`..`T-107` terbukti lewat bukti verifikasi backend (`rules/backend/TEST_POLICY.md`) |
| Yang mudah salah | Mendaftarkan resolver dengan `TryAddScoped`. Aplikasi tetap menyala, tetapi bawaan Billing yang dipakai dan `DoctorShare` tetap nol tanpa tanda apa pun. `T-101` menjaga ini |
| Yang mudah salah kedua | Menyalin pemilihan baris tarif ke resolver alih-alih memakai `MedicalFeeSharingRuleMatcher`. Jawaban ke Billing lalu menyimpang dari hasil jasa bulanan. `T-102` menjaga ini |
| Contoh | dr. Rina, Konsultasi Dokter Spesialis Rp 500.000, baris tarif peran utama 40%. Billing menerima `Resolved` 40.00 dan menulis `DoctorShare` Rp 200.000; periode Oktober menghitung rincian dr. Rina Rp 200.000 dari baris tarif yang sama |
| Dampak lintas modul | Billing menukar implementasi bawaan dengan milik Medical Fee. Billing **tidak** perlu dirilis ulang; pengisian `DoctorShare` sendiri baru berjalan setelah `BE-BKC-092` |

## 5. Prasyarat eksekusi

| # | Prasyarat | Status |
|---:|---|---|
| 1 | QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi**, dari `AGENTS.md` backend target dan dokumen engineering kanonik | Berlaku terus |
| 2 | `BE-MDF-001` selesai sebelum file model pertama ditulis | 🟡 **Sebagian, 26 Oktober 2026.** Baris `Mdf` sudah ada di registry dan nol file model ditulis; tinggal commit terpisah oleh pemilik — [laporan](../task/report/backend/BE-MDF-001.md) |
| 3 | Otorisasi terpisah untuk membuat **dan** menjalankan ketiga migration | **Belum** |
| 4 | Implementasi dijalankan lewat `quilvian-engineering-skills:build-module-backend` | Berlaku terus |
| 5 | Persetujuan `MDF-DES-001`..`018` | **Terpenuhi** 20 September 2026 |
| 6 | Penguncian versi kontrak | **Terpenuhi** 20 September 2026 |
| 7 | Penguncian `MDF-INTEGRATION-1.1`, `MDF-API-1.1`, `MDF-TEST-1.1` dan persetujuan `MDF-DES-019`..`021` untuk `BE-MDF-015`/`016` | **Terpenuhi** 27 Oktober 2026 |
| 8 | Eksekusi migration kolom Billing (`{OTORISASI-MIG-3}`) dan `BE-BKC-091`, untuk verifikasi data nyata `BE-MDF-015` | **Belum** — milik Billing |

## 6. Definition of Done backend

| # | Kriteria |
|---:|---|
| 1 | `MedicalFeeManagement` dan prefix `Mdf` terdaftar **sebelum** file model pertama |
| 2 | Seluruh 9 entity mewarisi `IdentityModel`; nol hard delete |
| 3 | Data induk memakai prefix `Mst`, entity transaksi memakai `Mdf` (`MDF-DES-003`) |
| 4 | Status sebagai `string` + `static class ...Statuses` + check constraint |
| 5 | `Guid RowVersion` pada aggregate root; perintah pengubah memeriksanya |
| 6 | Perintah pengubah nilai uang menerima `Idempotency-Key` |
| 7 | Perhitungan periode berjalan `IsolationLevel.Serializable` |
| 8 | Maker-checker ditegakkan di service **dan** check constraint pada tiga pasangan di `contracts/permission-audit-matrix.md` bagian 3 |
| 9 | Seluruh partial unique index memakai `WHERE "IsDelete" = false` |
| 10 | Kolom uang `HasPrecision(18, 2)`; persentase `HasPrecision(5, 2)` |
| 11 | `MdfServiceFeeDetail.SharingRuleId` wajib — setiap rupiah tertelusur |
| 12 | EF configuration di `Repositories/Configurations/HealthServices/MedicalFeeManagement/` |
| 13 | Seluruh endpoint memakai `ApiResponse<T>`, route hyphenated, `[AccessPermission]` |
| 14 | Penyaringan data untuk tenaga medis diuji, bukan hanya izin endpoint |
| 15 | 107 skenario pada `testing/acceptance-test-matrix.md` terbukti lewat bukti verifikasi backend menurut `rules/backend/TEST_POLICY.md` — bukan project test otomatis |
| 16 | Ketiga migration aditif; nol tabel modul lain diubah |
| 17 | Nol kode untuk radiologi (`MF-DEC-015`). Kode `BE-MDF-015` dan `BE-MDF-016` hanya sesuai `MDF-INTEGRATION-1.1`: tanpa endpoint `sharing-rules/resolve`, tanpa menulis tabel Billing |

Kriteria 17 adalah kriteria **selesai**, bukan kelalaian.

**Perubahan kriteria 15 dan 17, revisi 2.** Kriteria 17 lama berbunyi "nol kode untuk `BE-MDF-015`,
`BE-MDF-016`". Bunyi itu sudah tidak benar sebelum revisi ini: `BE-MDF-009` menulis pembacaan entri kasir
berjasa lebih dulu (`integration-contract.md` §5.6, celah G1–G4). Setelah kedua permukaan dikunci, larangan
itu juga kehilangan dasarnya. Kriteria 15 menyesuaikan jumlah skenario (90 → 107) dan kebijakan test backend.
