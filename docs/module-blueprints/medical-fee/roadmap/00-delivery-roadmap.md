# Medical Fee — Roadmap Pengiriman

## 1. Identitas

```yaml
roadmap_id: MDF-ROADMAP-001
roadmap_revision: 5
roadmap_status: ACTIVE
revision_note: >
  Revisi 5, 27 Oktober 2026: MDF-INTEGRATION-1.1, MDF-API-1.1, dan MDF-TEST-1.1 dikunci owner, dan
  MDF-DES-019..021 disetujui. BE-MDF-015 dan BE-MDF-016 tidak lagi tertahan keputusan dan dijadwalkan
  POST-MVP; dependency BE-MDF-016 dipindah dari BE-MDF-014 ke BE-MDF-010.
blueprint_id: MF-BP-001
blueprint_revision: 2
blueprint_status: approved
created_at: 2026-09-20T00:00:00+07:00
planned_by: /quilvian-engineering-skills:plan-module-delivery
children:
  backend: roadmap/01-backend-roadmap.md — MDF-ROADMAP-BE-001
  frontend: roadmap/02-frontend-roadmap.md — MDF-ROADMAP-FE-001

backend_commit_sha: 09101d0581695e20345a9efa8af3fce7c38b1ae4
backend_branch: Yasmina
frontend_commit_sha: 66f36b432

input_hashes:
  00-interview-decisions.md: 7d95443ad12eb56cf4a7b449604880ad62baff2be1801b56e7e00c56b9b358b5
  01-existing-capability-map.md: 30d6fce01475444c2cbae360e335d000cf0d7ebe6892c43a4cca58a80f39cee0

approval_basis: >
  DUA lapisan approved 20 September 2026, keduanya diberikan terpisah:
  (1) 17 keputusan BISNIS (MF-DEC-001..010, MF-DEC-012..018);
  (2) 18 keputusan ARSITEKTUR MDF-DES-001..018.
  (3) PENGUNCIAN tujuh kontrak turunan ke 1.0 - lihat bagian 4.
  (4) 27 Oktober 2026: PENGUNCIAN MDF-INTEGRATION-1.1, MDF-API-1.1, MDF-TEST-1.1 beserta persetujuan
      MDF-DES-019..021 dan keputusan MF-OQ-012 pilihan A, lewat pernyataan owner "saya setuju dan kunci".
  Tidak ada lagi lapisan Medical Fee yang menunggu keputusan owner.
```

## 2. Berkas anak

Berkas ini adalah **payung**. Task-nya sendiri ada di dua berkas tersendiri:

| Berkas | Isi |
|---|---|
| `01-backend-roadmap.md` | 19 task `BE-MDF-*`, grafik urutan dependency, rincian per gelombang, prasyarat, DoD backend |
| `02-frontend-roadmap.md` | 6 task `FE-MDF-*`, kemampuan `F-01`..`F-17`, yang MUST diputuskan UI brief, DoD frontend |

Yang tetap di sini: identitas, pemblokir, penguncian kontrak, gelombang, traceability lintas
keduanya, coverage gap, dan hubungan dengan roadmap Finance.

## 3. Pemblokir yang tersisa

Pemblokir utama revisi 1 — persetujuan `MDF-DES-001`..`018` — **sudah tercabut** pada
20 September 2026. Roadmap ini naik dari `FORWARD_TEST` menjadi `ACTIVE`, dan `MVP-0` sampai
`MVP-5` kini siap dimulai.

Revisi 1 berfungsi sebagaimana dimaksud: ia membuktikan keputusan arsitektur dapat diturunkan
menjadi pekerjaan yang jelas **sebelum** owner diminta menyetujuinya.

| Pemblokir | Menahan | Pemilik | Sudah dikirim |
|---|---|---|:---:|
| ~~Persetujuan `MDF-DES-001`..`018`~~ | — | Yasmin | **TERCABUT 20 September 2026** |
| ~~`MF-CQ-08` — sumber `DoctorShare`~~ **DITUTUP 26 Oktober 2026** | Tidak lagi menahan `BE-MDF-016`. Permukaan §4 **dikunci 27 Oktober 2026**; tersisa dependency teknis `BE-MDF-010` | Owner Billing | Dijawab — `evidence/04` |
| ~~MF-CQ-05 — pelaksana entri kasir~~ **DITUTUP 26 Oktober 2026** | Tidak lagi menahan BE-MDF-015. Permukaan §5 **dikunci 27 Oktober 2026**; tersisa kolom Billing (BE-BKC-089 ✅, BE-BKC-091) | Owner Billing | Dijawab — evidence/04 |
| `MF-CQ-07` — pencatatan tim | Bagian tim pada `BE-MDF-009` saja | Owner Clinical + Laboratory | Ya — `evidence/03` |
| `MF-CQ-06` — sepengetahuan HR | Tidak menahan apa pun | Owner HR | Ya — `evidence/02` |

Perhitungan jasa dari kamar operasi — rumpun terbesar dan satu-satunya yang datanya sudah
lengkap — tidak tertahan satu pun dari keempatnya.

## 4. Versi kontrak — **TERKUNCI 20 September 2026**

Owner mengunci ketujuh kontrak turunan ke `1.0`.

| Kontrak | Versi | Status |
|---|---|---|
| `contracts/api-contract.md` | `MDF-API-1.0` | `locked` |
| `contracts/integration-contract.md` | `MDF-INTEGRATION-1.0` | `locked` — dua permukaan dikecualikan |
| `contracts/state-transition-matrix.md` | `MDF-STATE-1.0` | `locked` |
| `contracts/validation-matrix.md` | `MDF-VAL-1.0` | `locked` |
| `contracts/permission-audit-matrix.md` | `MDF-PERM-1.0` | `locked` |
| `04-prd-to-mvp.md` | `MDF-MVP-1.0` | `locked` |
| `testing/acceptance-test-matrix.md` | `MDF-TEST-1.0` | `locked` |

~~**Dua permukaan tetap tidak terkunci**~~ — **keduanya dikunci 27 Oktober 2026** sebagai amendment
tersendiri. Ketujuh kontrak 1.0 di atas tetap pada versinya untuk isi yang tidak tersentuh.

| Kontrak amendment | Versi | Status | Isi |
|---|---|---|---|
| `contracts/integration-contract.md` | `MDF-INTEGRATION-1.1` | `locked` 27 Oktober 2026 | §4 port `IBillingDoctorShareResolver`; §5 pelaksana entri kasir; §7.3 `BilApHandoff` bukan sumber utang jasa |
| `contracts/api-contract.md` | `MDF-API-1.1` | `locked` 27 Oktober 2026 | Satu baris bagian 9: endpoint `sharing-rules/resolve` tidak dibuat |
| `testing/acceptance-test-matrix.md` | `MDF-TEST-1.1` | `locked` 27 Oktober 2026 | Bagian 11A, `T-91`..`T-107` |

| Permukaan | Dulu menunggu | Sekarang |
|---|---|---|
| Arah alir nilai jasa ke `BilInvoiceItem.DoctorShare` | Jawaban Billing, lalu penguncian | Terkunci `MDF-INTEGRATION-1.1` §4 |
| Rujukan pelaksana pada entri `ADHOC` dan `ADHOC_CATALOG` | Jawaban Billing, lalu penguncian | Terkunci `MDF-INTEGRATION-1.1` §5 |

`MF-CQ-07` (pencatatan tim) **tidak** termasuk pengecualian ini: bentuk rincian jasa sudah
terkunci, dan yang ditunggu hanyalah modul sumber mulai mencatat timnya — bukan perubahan
kontrak.

**Konsekuensi penguncian:** kerja paralel backend–frontend kini **diizinkan**. Yang masih
menahan task `FE-MDF-*` hanyalah ketiadaan UI brief, dan itu keputusan Product Owner.

## 5. Gelombang

Mengikuti `04-prd-to-mvp.md` bagian 4 apa adanya.

| Gelombang | Epic | Task | Keadaan |
|---|---|---|---|
| `MVP-0` | `MDF-01`, `MDF-02` | `BE-MDF-001`, `002`, `005`, `007`, `018`, `003`, `008`, `019`, `004` | **Siap dimulai** |
| `MVP-0` | — | · `FE-MDF-005` | Sisi FE menunggu UI brief |
| `MVP-1` | `MDF-03` | `BE-MDF-006` · `FE-MDF-001` | Siap setelah `MVP-0`; sisi FE menunggu UI brief |
| `MVP-2` | `MDF-04`, `MDF-05` (sebagian), `MDF-06` | `BE-MDF-009`..`011` · `FE-MDF-002` | Siap setelah `MVP-1`; **bagian tim** menunggu `MF-CQ-07` |
| `MVP-3` | `MDF-08`, `MDF-09` | `BE-MDF-012`..`013` · `FE-MDF-003` | Siap setelah `MVP-2` |
| `MVP-4` | `MDF-10` | `BE-MDF-014` · `FE-MDF-006` | Siap setelah `MVP-3`; **membuka Finance `BE-FIN-021`** |
| `MVP-5` | `MDF-12` | `BE-MDF-017` · `FE-MDF-004` | Siap setelah `MVP-3` |
| `POST-MVP` | `MDF-07` | `BE-MDF-015` | **Siap setelah `BE-MDF-010`** (gelombang teknis 8). Verifikasi data nyata menunggu `BE-BKC-091` (⛔ `{OTORISASI-MIG-3}`) dan eksekusi migration kolom Billing (`BE-BKC-089` ✅ berkas migration ada, belum dijalankan ke database) |
| `POST-MVP` | `MDF-11` | `BE-MDF-016` | 🟡 **Sebagian 27 Oktober 2026**. Port `MedicalFeeDoctorShareResolver` dan engine matcher terpasang. Bukti ujung-ke-ujung menunggu `BE-BKC-092` |

`MDF-07` dan `MDF-11` tetap `POST-MVP` karena `04-prd-to-mvp.md` bagian 2 menaruh keduanya di luar rilis
pertama. Yang berubah hanya keadaannya: dari tertahan keputusan menjadi siap dikerjakan. Menariknya ke
rilis pertama adalah keputusan pemilik produk.

`MVP-0` MUST selesai lebih dulu: tanpa pendaftaran registry, file model pertama pun tidak boleh
ditulis (`MDF-DES-002`). `MVP-1` MUST mendahului `MVP-2`: tanpa baris tarif terisi, perhitungan
hanya menghasilkan daftar `RULE_MISSING` sepanjang jumlah layanan.

**Koreksi pembagian gelombang, revisi 2.** Revisi 1 menempatkan `BE-MDF-003` (migration tiga
tabel master) di `MVP-0` sementara `BE-MDF-005` yang menjadi syaratnya di `MVP-1` — satu task
bergantung pada task gelombang berikutnya, dan itu mustahil dikerjakan.

Perbaikannya mengikuti `04-prd-to-mvp.md` bagian 5 apa adanya: `EPIC MDF-01` memang menyebut
keluarannya "9 `DbSet`, 7 `AddScoped`, 3 migration aditif" — artinya **seluruh entity,
configuration, dan ketiga migration** memang milik `MVP-0`. Yang masuk gelombang berikutnya
hanyalah service dan API-nya. Cakupan epic tidak berubah; hanya pembagian task yang dibetulkan.

Karena itu `BE-MDF-014` kini berisi service dan controller saja, dan entity beserta
migration-nya dipisah menjadi `BE-MDF-018` dan `BE-MDF-019` di `MVP-0`.

## 6. Indeks task

Rincian lengkap 11 kolom ada di berkas anak. Tabel ini hanya indeks.

### 6.1 Backend — `01-backend-roadmap.md`

| Task | Outcome ringkas | Gelombang | Keadaan |
|---|---|---|---|
| 🟡 `BE-MDF-001` | Pendaftaran modul dan prefix `Mdf` ke registry | `MVP-0` | 🟡 Sebagian 26 Oktober 2026 — baris registry tertulis, menunggu commit pemilik: [laporan](../task/report/backend/BE-MDF-001.md) |
| 🟡 `BE-MDF-002` | Entity daftar peran | `MVP-0` | 🟡 Sebagian 26 Oktober 2026 — source ada, build dan `T-01`..`T-06` belum dijalankan: [laporan](../task/report/backend/BE-MDF-002.md) |
| 🟡 `BE-MDF-005` | Entity kesepakatan tarif dan baris tarif | `MVP-0` | 🟡 Sebagian 26 Oktober 2026 — source ada, build dan `T-07`..`T-13` belum dijalankan: [laporan](../task/report/backend/BE-MDF-005.md) |
| 🟡 `BE-MDF-007` | Entity periode, hasil jasa, rincian, koreksi, belum terhitung | `MVP-0` | 🟡 Sebagian 26 Oktober 2026 — 5 entity & EF configuration terpasang, build belum dijalankan: [laporan](../task/report/backend/BE-MDF-007.md) |
| `BE-MDF-018` | Entity penyerahan ke Finance | `MVP-0` | Siap |
| 🟡 `BE-MDF-003` | Migration `AddMedicalFeeMasterData` | `MVP-0` | 🟡 Sebagian 26 Oktober 2026 — migration dibuat aditif, menunggu otorisasi eksekusi DB: [laporan](../task/report/backend/BE-MDF-003.md) |
| 🟡 `BE-MDF-008` | Migration `AddMedicalFeeCalculation` | `MVP-0` | 🟡 Sebagian 26 Oktober 2026 — migration dibuat aditif, menunggu otorisasi eksekusi DB: [laporan](../task/report/backend/BE-MDF-008.md) |
| `BE-MDF-019` | Migration `AddMedicalFeeFinanceHandoff` | `MVP-0` | Siap — butuh otorisasi migration |
| 🟡 `BE-MDF-004` | API daftar peran | `MVP-0` | 🟡 Sebagian 26 Oktober 2026 — controller & service ada, build belum dijalankan: [laporan](../task/report/backend/BE-MDF-004.md) |
| 🟡 `BE-MDF-006` | Layanan kesepakatan tarif berversi | `MVP-1` | 🟡 Sebagian 26 Oktober 2026 — controller & service ada, build belum dijalankan: [laporan](../task/report/backend/BE-MDF-006.md) |
| 🟡 `BE-MDF-009` | Adapter sumber layanan | `MVP-2` | 🟡 Sebagian 26 Oktober 2026 — adapter service & DTO terpasang, build belum dijalankan: [laporan](../task/report/backend/BE-MDF-009.md) |
| 🟡 `BE-MDF-010` | Mesin perhitungan periode | `MVP-2` | 🟡 Sebagian 26 Oktober 2026 — calculation service, period service, controller & DTO terpasang, build belum dijalankan: [laporan](../task/report/backend/BE-MDF-010.md) |
| 🟡 `BE-MDF-011` | Daftar layanan belum dapat dihitung | `MVP-2` | 🟡 Sebagian 27 Oktober 2026 — controller unresolved, waiver, summary & DTO terpasang, build belum dijalankan: [laporan](../task/report/backend/BE-MDF-011.md) |
| 🟡 `BE-MDF-012` | Verifikasi, persetujuan, penutupan | `MVP-3` | 🟡 Sebagian 27 Oktober 2026 — controller hasil jasa, rincian tindakan breakdown, maker-checker 3 lapis, service & DTO terpasang, build belum dijalankan: [laporan](../task/report/backend/BE-MDF-012.md) |
| 🟡 `BE-MDF-013` | Koreksi berjenjang | `MVP-3` | 🟡 Sebagian 27 Oktober 2026 — adjustment service, controller adjustment, endpoint sub-route service-fees, maker-checker 2 lapis, DTO & DI terpasang, build belum dijalankan: [laporan](../task/report/backend/BE-MDF-013.md) |
| 🟡 BE-MDF-014 | Penyerahan ke Finance — service dan API | MVP-4 | 🟡 Sebagian 27 Oktober 2026 — handoff service, controller, DTO, integrasi atomis pada approve periode, DbSet & DI terpasang, membuka BE-FIN-021: [laporan](../task/report/backend/BE-MDF-014.md) |
| 🟡 `BE-MDF-017` | Pembatasan data untuk tenaga medis | `MVP-5` | 🟡 Sebagian 27 Oktober 2026 — pembatasan data lapis service, actor context, access guard, T-80..T-84 lolos: [laporan](../task/report/backend/BE-MDF-017.md) |
| 🟡 BE-MDF-015 | Jasa dari entri kasir berjasa — perbaikan adapter, celah G1–G4 | POST-MVP | 🟡 Sebagian 27 Oktober 2026 — adapter ExtractAdhocBillingServices diperbaiki, celah G1–G4 tertutup, T-103..T-107 terpenuhi: [laporan](../task/report/backend/BE-MDF-015.md) |
| 🟡 BE-MDF-016 | Implementasi port IBillingDoctorShareResolver dan matcher bersama | POST-MVP | 🟡 Sebagian 27 Oktober 2026 — port resolver IBillingDoctorShareResolver, rule matcher MedicalFeeSharingRuleMatcher, DI replace terpasang, T-91..T-102 terpenuhi: [laporan](../task/report/backend/BE-MDF-016.md) |

### 6.2 Frontend — `02-frontend-roadmap.md`

| Task | Outcome ringkas | Menunggu backend | Keadaan |
|---|---|---|---|
| `FE-MDF-005` | Pengelolaan daftar peran | `BE-MDF-004` | ✅ **Selesai** ([laporan](../task/report/frontend/FE-MDF-005.md)) |
| `FE-MDF-001` | Kesepakatan tarif dan baris tarif | `BE-MDF-006` | ✅ **Selesai** ([laporan](../task/report/frontend/FE-MDF-001.md)) |
| `FE-MDF-002` | Periode, hasil jasa, rincian, belum terhitung | `BE-MDF-011` | ✅ **Selesai** ([laporan](../task/report/frontend/FE-MDF-002.md)) |
| `FE-MDF-003` | Verifikasi, persetujuan, koreksi | `BE-MDF-013` | ✅ **Selesai** ([laporan](../task/report/frontend/FE-MDF-003.md)) |
| `FE-MDF-006` | Pemantauan penyerahan ke Finance | `BE-MDF-014` | ✅ **Selesai** ([laporan](../task/report/frontend/FE-MDF-006.md)) |
| `FE-MDF-004` | Tenaga medis melihat jasanya sendiri | 🟡 `BE-MDF-017` | ✅ **Selesai** ([laporan](../task/report/frontend/FE-MDF-004.md)) |

`FE-MDF-005` dan `FE-MDF-006` ditambahkan saat roadmap dipecah: kemampuan `F-01` dan `F-16`
pada `03-frontend-architecture.md` sebelumnya tidak punya task frontend sama sekali, padahal
backend-nya ada dan keduanya tercantum sebagai MUST.

## 7. Traceability

| Requirement | Decision | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `F-01` | `MF-DEC-016` | `MDF-DES-003`, `011` | `MDF-VAL-1.0` §1 | 🟡 `BE-MDF-001`, `BE-MDF-002`, `BE-MDF-004` | ✅ `FE-MDF-005` | `T-01`..`T-06` | 🟡 BE Sebagian, ✅ FE Selesai 27 Oktober 2026 ([laporan BE](../task/report/backend/BE-MDF-004.md), [laporan FE](../task/report/frontend/FE-MDF-005.md)) |
| `F-02`..`F-04` | `MF-DEC-004`, `012` | `MDF-DES-008`..`010` | `MDF-VAL-1.0` §2–3 | 🟡 `BE-MDF-005`, 🟡 `BE-MDF-006` | ✅ `FE-MDF-001` | `T-07`..`T-24` | 🟡 BE Sebagian, ✅ FE Selesai 27 Oktober 2026 ([laporan BE](../task/report/backend/BE-MDF-006.md), [laporan FE](../task/report/frontend/FE-MDF-001.md)) |
| `F-05`, `F-06`, `F-09`, `F-10` | `MF-DEC-006`, `013`, `014` | `MDF-DES-012`, `013`, `015` | `MDF-VAL-1.0` §5 | 🟡 `BE-MDF-007`..`010` | ✅ `FE-MDF-002` | `T-25`..`T-38` | 🟡 BE Sebagian, ✅ FE Selesai 27 Oktober 2026 ([laporan BE](../task/report/backend/BE-MDF-010.md), [laporan FE](../task/report/frontend/FE-MDF-002.md)) |
| `F-07`, `F-08` | `MF-DEC-018` | `MDF-DES-014`, `016` | `MDF-API-1.0` §6 | 🟡 `BE-MDF-011` | ✅ `FE-MDF-002` | `T-39`..`T-49` | 🟡 BE Sebagian, ✅ FE Selesai 27 Oktober 2026 ([laporan BE](../task/report/backend/BE-MDF-011.md), [laporan FE](../task/report/frontend/FE-MDF-002.md)) |
| `F-11`..`F-13` | `MF-DEC-009` | `MDF-DES-016`, `017` | `MDF-STATE-1.0` | 🟡 `BE-MDF-012` | ✅ `FE-MDF-003` | `T-50`..`T-63` | 🟡 BE Sebagian, ✅ FE Selesai 27 Oktober 2026 ([laporan BE](../task/report/backend/BE-MDF-012.md), [laporan FE](../task/report/frontend/FE-MDF-003.md)) |
| `F-14`, `F-15` | `MF-DEC-007` | `MDF-DES-017` | `MDF-STATE-1.0` §3 | 🟡 `BE-MDF-013` | ✅ `FE-MDF-003` | `T-64`..`T-70` | 🟡 BE Sebagian, ✅ FE Selesai 27 Oktober 2026 ([laporan BE](../task/report/backend/BE-MDF-013.md), [laporan FE](../task/report/frontend/FE-MDF-003.md)) |
| `F-16` | `MF-DEC-005`, `008` | `MDF-DES-018` | `MDF-INTEGRATION-1.0` §7 | 🟡 `BE-MDF-014` | ✅ `FE-MDF-006` | `T-71`..`T-79` | 🟡 BE Sebagian, ✅ FE Selesai 27 Oktober 2026 ([laporan BE](../task/report/backend/BE-MDF-014.md), [laporan FE](../task/report/frontend/FE-MDF-006.md)) |
| `F-17` | `MF-DEC-010` | `MDF-DES-011` | `MDF-PERM-1.0` §5 | 🟡 `BE-MDF-017` | ✅ `FE-MDF-004` | `T-80`..`T-84` | 🟡 BE Sebagian 27 Oktober 2026 ([laporan BE](../task/report/backend/BE-MDF-017.md)), ✅ FE Selesai 27 Oktober 2026 ([laporan FE](../task/report/frontend/FE-MDF-004.md)) |
