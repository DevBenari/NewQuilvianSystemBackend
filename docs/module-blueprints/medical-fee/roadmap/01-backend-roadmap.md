# Medical Fee — Roadmap Backend

## 1. Identitas

```yaml
roadmap_id: MDF-ROADMAP-BE-001
parent_roadmap: MDF-ROADMAP-001 revisi 4
roadmap_revision: 1
roadmap_status: ACTIVE
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
```

Payung roadmap ada di `00-delivery-roadmap.md`. Pasangan frontend-nya ada di
`02-frontend-roadmap.md`.

**Berkas ini tidak menyatakan satu pun pekerjaan selesai.**

## 2. Urutan eksekusi

```text
MVP-0   BE-MDF-001 → BE-MDF-002 → BE-MDF-005 → BE-MDF-007 → BE-MDF-018
                   → BE-MDF-003 → BE-MDF-008 → BE-MDF-019 → BE-MDF-004
MVP-1   BE-MDF-006
MVP-2   BE-MDF-009 → BE-MDF-010 → BE-MDF-011
MVP-3   BE-MDF-012 → BE-MDF-013
MVP-4   BE-MDF-014                        membuka Finance BE-FIN-021
MVP-5   BE-MDF-017
—       BE-MDF-015                        BLOCKED — MF-CQ-05, owner Billing
—       BE-MDF-016                        BLOCKED — MF-CQ-08, owner Billing
```

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

## 3. Task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-MDF-001` | Modul `MedicalFeeManagement` dan prefix `Mdf` terdaftar di registry | `MDF-DES-002` | — | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Satu baris modul + prefix; `Mdf` diverifikasi belum dipakai | — | Baris tercatat sebelum file model pertama | Berkas registry ter-diff | Backend Owner — **prasyarat `QBE-MOD-002`/`003`/`NAM-004`** | Registry ter-commit terpisah dari kode |
| `BE-MDF-002` | Entity dan configuration daftar peran | `MDF-DES-003`, `011`, `MF-DEC-016` | `MDF-VAL-1.0` §1 | `OprTeamRole` (`MF-CAP-002`) | `MstMedicalFeeRole` + configuration | `BE-MDF-001` | `RoleCode` unik; satu `OprTeamRole` dipetakan paling banyak satu peran | `T-01`..`T-06` | Backend Owner | Prefix `Mst` untuk data induk |
| `BE-MDF-005` | Entity kesepakatan tarif dan baris tarif | `MDF-DES-008`..`010`, `MF-DEC-004`, `012` | `MDF-VAL-1.0` §2–3 | `WfpContractHistory` (`MF-CAP-009`), `MstTariff` | `MdfSharingAgreement`, `MdfSharingRule` + configuration | `BE-MDF-002` | FK `Restrict` ke kontrak HR; persentase 0..100 sebagai check constraint | `T-07`..`T-13` | Backend Owner | Menunjuk kontrak HR, **tidak menyalin** |
| `BE-MDF-007` | Entity periode, hasil jasa, rincian, koreksi, daftar belum terhitung | `MDF-DES-010`, `012`, `013`, `014` | `MDF-VAL-1.0` §4–6 | — | `MdfFeePeriod`, `MdfServiceFee`, `MdfServiceFeeDetail`, `MdfServiceFeeAdjustment`, `MdfUnresolvedService` + configuration | `BE-MDF-002` | `FinalAmount = GrossAmount + AdjustmentAmount` sebagai check constraint; satu penerima satu hasil per periode | `T-26`, `T-30` | Backend Owner | `SharingRuleId` wajib — setiap rupiah tertelusur |
| `BE-MDF-018` | Entity penyerahan ke Finance | `MDF-DES-018` | `MDF-VAL-1.0` §7 | Pola `BilArHandoff` | `MdfFinanceHandoff` + configuration | `BE-MDF-007` | `ServiceFeeId` dan `HandoffKey` masing-masing unik | `T-73`, `T-76` | Backend Owner | Empat kolom disalin dari hasil jasa agar Finance tidak perlu join |
| `BE-MDF-003` | Migration `AddMedicalFeeMasterData` | `MDF-DES-001` | — | — | 3 tabel, aditif | `BE-MDF-005` | `Up()` membuat 3 tabel; `Down()` bersih | Migration dijalankan di lingkungan pengembangan | **Otorisasi terpisah wajib** (`AGENTS.md`) | Nol tabel modul lain tersentuh |
| `BE-MDF-008` | Migration `AddMedicalFeeCalculation` | `MDF-DES-001` | — | — | 5 tabel, aditif | `BE-MDF-007`, `BE-MDF-003` | Urutan setelah migration 1 | Migration dijalankan | **Otorisasi terpisah wajib** | Aditif |
| `BE-MDF-019` | Migration `AddMedicalFeeFinanceHandoff` | `MDF-DES-001` | — | — | 1 tabel, aditif | `BE-MDF-018`, `BE-MDF-008` | Urutan setelah migration 2 | Migration dijalankan | **Otorisasi terpisah wajib** | Aditif |
| `BE-MDF-004` | API daftar peran | `MF-DEC-016` | `MDF-API-1.0` §2, `MDF-PERM-1.0` | `ApiResponse<T>`, `[AccessPermission]` | `MedicalFeeRoleService`, `MedicalFeeRolesController` | `BE-MDF-003` | Peran terpakai dinonaktifkan, bukan dihapus | `T-05`, `T-06` | Backend Owner | Route `api/v1/health-services/medical-fee-management/...` |
| `BE-MDF-006` | Layanan kesepakatan tarif berversi | `MDF-DES-009`, `MF-DEC-004` | `MDF-STATE-1.0` §1, `MDF-API-1.0` §3 | — | `MedicalFeeSharingService`, `MedicalFeeSharingAgreementsController` | `BE-MDF-003` | Mengubah tarif membuat baris baru; hasil periode lalu tidak bergeser; jumlah peran ≤ 100% | `T-14`..`T-19` | Backend Owner | Baris lama tidak pernah diubah |
| `BE-MDF-009` | Adapter sumber layanan | `MDF-DES-011`, `MF-DEC-003`, `015` | `MDF-INTEGRATION-1.0` §2 | `OprTeamMember`, `TrxPatientProcedure`, `LabOrder` (`MF-CAP-002`, `005`, `006`) | `MedicalFeeServiceSourceAdapter` — kamar operasi (tim penuh), klinis dan laboratorium (pelaksana tunggal) | `BE-MDF-008`, `BE-MDF-006` | Operasi bertim tiga orang → tiga rincian; radiologi diabaikan sepenuhnya | `T-25`, `T-38`, `T-39` | Backend Owner. **Bagian tim di luar kamar operasi tertahan `MF-CQ-07`** | Perbedaan antar sumber terisolasi di satu berkas |
| `BE-MDF-010` | Mesin perhitungan periode | `MDF-DES-007`, `012`, `013`, `015` | `MDF-VAL-1.0` §5 | — | `MedicalFeeCalculationService`, `MedicalFeePeriodService` | `BE-MDF-009` | Basis nilai kotor, tidak berkurang diskon; snapshot persentase; hitung ulang mengganti bukan menggandakan | `T-20`..`T-24`, `T-27`..`T-37` | Backend Owner | `Serializable` + `Idempotency-Key` |
| `BE-MDF-011` | Daftar layanan belum dapat dihitung | `MDF-DES-014`, `016`, `MF-DEC-018` | `MDF-API-1.0` §6 | — | `MedicalFeeUnresolvedServicesController` beserta rekap per alasan | `BE-MDF-010` | Enam alasan tercatat; menahan penutupan periode; pengesampingan menuntut catatan | `T-39`..`T-49` | Backend Owner | **Tanpa** endpoint penghapus |
| `BE-MDF-012` | Siklus verifikasi, persetujuan, penutupan | `MDF-DES-016`, `017`, `MF-DEC-009` | `MDF-STATE-1.0` §1–2, `MDF-PERM-1.0` §3 | Pola `FIN-DES-014` | Transisi status periode dan hasil jasa, pemisahan wewenang tiga lapis | `BE-MDF-011` | Pemverifikasi ditolak saat menyetujui, **termasuk lewat jalur yang melewati service** | `T-50`..`T-63` | Backend Owner | Check constraint maker-checker terpasang |
| `BE-MDF-013` | Koreksi berjenjang | `MF-DEC-007`, `MDF-DES-017` | `MDF-STATE-1.0` §3 | — | `MedicalFeeAdjustmentService` beserta endpoint-nya | `BE-MDF-012` | `RequestedBy <> ApprovedBy`; koreksi final tidak dapat diubah; `FinalAmount` tidak negatif | `T-64`..`T-70` | Backend Owner | Pembatalan lewat koreksi berlawanan, bukan pengubahan |
| `BE-MDF-014` | Penyerahan ke Finance — service dan API | `MDF-DES-018`, `MF-DEC-005`, `008` | `MDF-INTEGRATION-1.0` §7 | Pola `BilArHandoff` | `MedicalFeeHandoffService`, `MedicalFeeHandoffsController` | `BE-MDF-013`, `BE-MDF-019` | Ditulis di transaksi yang sama dengan persetujuan; nilai kotor; `HandoffKey` tetap saat dikirim ulang | `T-71`..`T-79` | Backend Owner. **Membuka Finance `BE-FIN-021`** | Medical Fee tidak pernah menulis ke tabel Finance |
| `BE-MDF-017` | Pembatasan data untuk tenaga medis | `MF-DEC-010`, `MDF-PERM-1.0` §5 | `MDF-PERM-1.0` | — | Penyaringan `PayeeReferenceId` di lapis service | `BE-MDF-013` | Dokter A meminta data dokter B menerima kosong; akses id langsung menghasilkan `404` bukan `403` | `T-80`..`T-84` | Backend Owner — **risiko privasi nyata**: satu izin `View` tanpa saringan membuka penghasilan ±220 orang | Pembatasan data, bukan sekadar pembatasan endpoint |
| `BE-MDF-015` | **BLOCKED** — jasa dari entri bebas kasir | `MF-DEC-017` | `MDF-INTEGRATION-1.0` §5 — permukaan ini **dikecualikan** dari penguncian | — | Sumber `ADHOC` dan `ADHOC_CATALOG` | `BE-MDF-010` **dan** `MF-CQ-05` | Entri kasir berjasa menunjuk pelaksananya | — | **Owner Billing** — permintaan sudah dikirim | — |
| `BE-MDF-016` | **BLOCKED** — alir nilai jasa ke `DoctorShare` | `MF-DEC-001` | `MDF-INTEGRATION-1.0` §4 — permukaan ini **dikecualikan** | `BilInvoiceItem.DoctorShare` (`MF-CAP-003`) | Bentuk A, B, atau C sesuai jawaban Billing | `BE-MDF-014` **dan** `MF-CQ-08` | Ditentukan jawaban owner Billing | — | **Owner Billing.** Pemblokir terbesar; perhitungan internal tidak tertahan olehnya | — |

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

## 5. Prasyarat eksekusi

| # | Prasyarat | Status |
|---:|---|---|
| 1 | QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi**, dari `AGENTS.md` backend target dan dokumen engineering kanonik | Berlaku terus |
| 2 | `BE-MDF-001` selesai sebelum file model pertama ditulis | **Belum** |
| 3 | Otorisasi terpisah untuk membuat **dan** menjalankan ketiga migration | **Belum** |
| 4 | Implementasi dijalankan lewat `quilvian-engineering-skills:build-module-backend` | Berlaku terus |
| 5 | Persetujuan `MDF-DES-001`..`018` | **Terpenuhi** 20 September 2026 |
| 6 | Penguncian versi kontrak | **Terpenuhi** 20 September 2026 |

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
| 15 | 90 uji pada `testing/acceptance-test-matrix.md` lulus |
| 16 | Ketiga migration aditif; nol tabel modul lain diubah |
| 17 | Nol kode untuk `BE-MDF-015`, `BE-MDF-016`, atau radiologi |

Kriteria 17 adalah kriteria **selesai**, bukan kelalaian.
