# Medical Fee — Roadmap Pengiriman

## 1. Identitas

```yaml
roadmap_id: MDF-ROADMAP-001
roadmap_revision: 4
roadmap_status: ACTIVE
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
  Tidak ada lagi lapisan Medical Fee yang menunggu keputusan owner.
```

## 2. Berkas anak

Berkas ini adalah **payung**. Task-nya sendiri ada di dua berkas tersendiri:

| Berkas | Isi |
|---|---|
| `01-backend-roadmap.md` | 17 task `BE-MDF-*`, urutan eksekusi, rincian per gelombang, prasyarat, DoD backend |
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
| `MF-CQ-08` — sumber `DoctorShare` | `BE-MDF-016` saja | Owner Billing | Ya — `evidence/01` |
| `MF-CQ-05` — pelaksana entri kasir | `BE-MDF-015` saja | Owner Billing | Ya — `evidence/01` |
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

**Dua permukaan tetap tidak terkunci**, dan alasannya bukan status keputusan melainkan
ketergantungan pada owner Billing:

| Permukaan | Menunggu | Bagian |
|---|---|---|
| Arah alir nilai jasa ke `BilInvoiceItem.DoctorShare` | `MF-CQ-08` — bentuk A, B, atau C belum dipilih | `integration-contract.md` §4 |
| Rujukan pelaksana pada entri `ADHOC` dan `ADHOC_CATALOG` | `MF-CQ-05` | `integration-contract.md` §5 |

Keduanya kelak **menambah permukaan baru** yang dikunci tersendiri; kedatangannya tidak
menaikkan versi ketujuh kontrak di atas.

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
| **Tertahan** | `MDF-07` | `BE-MDF-015` | `MF-CQ-05` — owner Billing |
| **Tertahan** | `MDF-11` | `BE-MDF-016` | `MF-CQ-08` — owner Billing |

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
| `BE-MDF-001` | Pendaftaran modul dan prefix `Mdf` ke registry | `MVP-0` | Siap |
| `BE-MDF-002` | Entity daftar peran | `MVP-0` | Siap |
| `BE-MDF-005` | Entity kesepakatan tarif dan baris tarif | `MVP-0` | Siap |
| `BE-MDF-007` | Entity periode, hasil jasa, rincian, koreksi, belum terhitung | `MVP-0` | Siap |
| `BE-MDF-018` | Entity penyerahan ke Finance | `MVP-0` | Siap |
| `BE-MDF-003` | Migration `AddMedicalFeeMasterData` | `MVP-0` | Siap — butuh otorisasi migration |
| `BE-MDF-008` | Migration `AddMedicalFeeCalculation` | `MVP-0` | Siap — butuh otorisasi migration |
| `BE-MDF-019` | Migration `AddMedicalFeeFinanceHandoff` | `MVP-0` | Siap — butuh otorisasi migration |
| `BE-MDF-004` | API daftar peran | `MVP-0` | Siap |
| `BE-MDF-006` | Layanan kesepakatan tarif berversi | `MVP-1` | Siap |
| `BE-MDF-009` | Adapter sumber layanan | `MVP-2` | Siap — bagian tim menunggu `MF-CQ-07` |
| `BE-MDF-010` | Mesin perhitungan periode | `MVP-2` | Siap |
| `BE-MDF-011` | Daftar layanan belum dapat dihitung | `MVP-2` | Siap |
| `BE-MDF-012` | Verifikasi, persetujuan, penutupan | `MVP-3` | Siap |
| `BE-MDF-013` | Koreksi berjenjang | `MVP-3` | Siap |
| `BE-MDF-014` | Penyerahan ke Finance — service dan API | `MVP-4` | Siap — **membuka `BE-FIN-021`** |
| `BE-MDF-017` | Pembatasan data untuk tenaga medis | `MVP-5` | Siap |
| `BE-MDF-015` | Jasa dari entri bebas kasir | — | **BLOCKED** — `MF-CQ-05` |
| `BE-MDF-016` | Alir nilai jasa ke `DoctorShare` | — | **BLOCKED** — `MF-CQ-08` |

### 6.2 Frontend — `02-frontend-roadmap.md`

| Task | Outcome ringkas | Menunggu backend | Keadaan |
|---|---|---|---|
| `FE-MDF-005` | Pengelolaan daftar peran | `BE-MDF-004` | **UI brief** |
| `FE-MDF-001` | Kesepakatan tarif dan baris tarif | `BE-MDF-006` | **UI brief** |
| `FE-MDF-002` | Periode, hasil jasa, rincian, belum terhitung | `BE-MDF-011` | **UI brief** |
| `FE-MDF-003` | Verifikasi, persetujuan, koreksi | `BE-MDF-013` | **UI brief** |
| `FE-MDF-006` | Pemantauan penyerahan ke Finance | `BE-MDF-014` | **UI brief** |
| `FE-MDF-004` | Tenaga medis melihat jasanya sendiri | `BE-MDF-017` | **UI brief** |

`FE-MDF-005` dan `FE-MDF-006` ditambahkan saat roadmap dipecah: kemampuan `F-01` dan `F-16`
pada `03-frontend-architecture.md` sebelumnya tidak punya task frontend sama sekali, padahal
backend-nya ada dan keduanya tercantum sebagai MUST.

## 7. Traceability

| Requirement | Decision | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `F-01` | `MF-DEC-016` | `MDF-DES-003`, `011` | `MDF-VAL-1.0` §1 | `BE-MDF-001`, `002`, `004` | `FE-MDF-005` | `T-01`..`T-06` | Siap |
| `F-02`..`F-04` | `MF-DEC-004`, `012` | `MDF-DES-008`..`010` | `MDF-VAL-1.0` §2–3 | `BE-MDF-005`, `006` | `FE-MDF-001` | `T-07`..`T-24` | Siap |
| `F-05`, `F-06`, `F-09`, `F-10` | `MF-DEC-006`, `013`, `014` | `MDF-DES-012`, `013`, `015` | `MDF-VAL-1.0` §5 | `BE-MDF-007`..`010` | `FE-MDF-002` | `T-25`..`T-38` | Siap — **bagian tim** menunggu `MF-CQ-07` |
| `F-07`, `F-08` | `MF-DEC-018` | `MDF-DES-014`, `016` | `MDF-API-1.0` §6 | `BE-MDF-011` | `FE-MDF-002` | `T-39`..`T-49` | Siap |
| `F-11`..`F-13` | `MF-DEC-009` | `MDF-DES-016`, `017` | `MDF-STATE-1.0` | `BE-MDF-012` | `FE-MDF-003` | `T-50`..`T-63` | Siap |
| `F-14`, `F-15` | `MF-DEC-007` | `MDF-DES-017` | `MDF-STATE-1.0` §3 | `BE-MDF-013` | `FE-MDF-003` | `T-64`..`T-70` | Siap |
| `F-16` | `MF-DEC-005`, `008` | `MDF-DES-018` | `MDF-INTEGRATION-1.0` §7 | `BE-MDF-014` | `FE-MDF-006` | `T-71`..`T-79` | Siap |
| `F-17` | `MF-DEC-010` | `MDF-DES-011` | `MDF-PERM-1.0` §5 | `BE-MDF-017` | `FE-MDF-004` | `T-80`..`T-84` | Siap |
| Entri bebas kasir | `MF-DEC-017` | — | `MDF-INTEGRATION-1.0` §5 | `BE-MDF-015` | — | — | **BLOCKED** — `MF-CQ-05` |
| Alir `DoctorShare` | `MF-DEC-001` | — | `MDF-INTEGRATION-1.0` §4 | `BE-MDF-016` | — | — | **BLOCKED** — `MF-CQ-08` |
| Jasa radiologi | `MF-DEC-015` | — | — | — | — | — | Ditunda sengaja |
| Audit akses data penghasilan | — | `MDF-PERM-1.0` §6 | — | `BE-MDF-017` | — | `T-85`..`T-90` | Siap |

## 8. Coverage gap

| Gap | Akibat | Pemilik |
|---|---|---|
| Daftar peran nyata pada tindakan poli dan laboratorium belum diketahui | `BE-MDF-002` tidak dapat mengisi data master awal | Owner Clinical + Laboratory (`MF-CQ-07` pertanyaan 2) |
| Dokter tamu dan paruh waktu mungkin tanpa `WfpContractHistory` | Kelompok yang tarifnya paling beragam tidak dapat punya kesepakatan sah | Owner HR (`MF-CQ-06` pertanyaan 2) |
| Perilaku bila Medical Fee belum punya tarif saat item Billing dicatat | `BE-MDF-016` tidak dapat mengunci bentuknya | Owner Billing (`MF-CQ-08` pertanyaan 3) |
| Pengisian kesepakatan tarif ±220 orang | Tanpa ini modul aktif tetapi tidak menghasilkan apa pun | Yasmin — pekerjaan data, MUST dijadwalkan terpisah dari `MVP-1` |

Gap terakhir adalah yang paling mudah terlewat karena bukan pekerjaan kode: perangkat lunaknya
bisa selesai seluruhnya dan tetap tidak berguna sampai kesepakatan tarif terisi.

## 9. Prasyarat eksekusi

| # | Prasyarat |
|---:|---|
| 1 | ~~Persetujuan owner atas `MDF-DES-001`..`018`~~ — **terpenuhi 20 September 2026** |
| 2 | QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi**, dari `AGENTS.md` backend target dan dokumen engineering kanonik |
| 3 | `BE-MDF-001` selesai sebelum file model pertama ditulis (`QBE-MOD-002`) |
| 4 | Setiap migration meminta otorisasi terpisah untuk dibuat **dan** untuk dijalankan |
| 5 | Implementasi dijalankan lewat `quilvian-engineering-skills:build-module-backend` |
| 6 | Tidak ada kode untuk `MDF-07`, `MDF-11`, atau radiologi — ini kriteria selesai, bukan kelalaian |

## 10. Hubungan dengan roadmap Finance

| Arah | Isi |
|---|---|
| Medical Fee → Finance | `BE-MDF-014` (`MdfFinanceHandoff`) **membuka** `BE-FIN-021` (`FinMedicalServicePayable`). Selama `BE-MDF-014` belum ada, `FIN-CAP-021` tetap `Missing` dan `EPIC FIN-08` tidak dapat dimulai |
| Finance → Medical Fee | **Tidak ada.** Finance sudah menyesuaikan diri lebih dulu lewat `FIN-DES-025`..`028`; tidak ada yang ditunggu Medical Fee dari Finance |
| Bersama | Keduanya menunggu owner Billing, tetapi untuk hal yang **berbeda**: Finance menunggu `BilCollectionHandoff`, Medical Fee menunggu `DoctorShare` dan pelaksana entri kasir. Kedua permintaan sudah dikirim terpisah |

Kedua modul kini sama-sama bebas pemblokir pada gelombang awalnya, dan kontrak keduanya
terkunci `1.0` sehingga sisi frontend masing-masing boleh berjalan paralel.

**Keduanya dimiliki orang yang sama.** Yasmin adalah Product/Domain Owner Finance Management
sekaligus Medical Fee. Yang hilang karenanya hanyalah kebutuhan negosiasi antar orang — bukan
disiplin batas modulnya. Medical Fee tetap berhenti pada jasa **kotor**, tetap menyerahkan
lewat `MdfFinanceHandoff`, dan tetap **tidak pernah** menulis ke tabel Finance. Menggabungkan
keduanya karena owner-nya satu akan membatalkan `MF-DEC-005` dan `MDF-DES-018` sekaligus.

Urutan yang masuk akal bila dikerjakan berurutan: Finance `MVP-0`/`MVP-1` lebih dulu, karena
`BE-FIN-021` pada akhirnya menunggu `BE-MDF-014` dan bukan sebaliknya.

## 11. Yang roadmap ini tidak lakukan

- Tidak menandai satu pun task selesai.
- Tidak menaikkan versi kontrak apa pun, dan tidak menetapkan approval sendiri: approval `MDF-DES-001`..`018` diberikan owner secara langsung dan dicatat apa adanya.
- Tidak menyembunyikan dependency: empat pemilik eksternal disebut namanya.
- Tidak menjalankan builder mana pun.
- Tidak memperlakukan `BLOCKED` sebagai jalan pintas melewati approval.
