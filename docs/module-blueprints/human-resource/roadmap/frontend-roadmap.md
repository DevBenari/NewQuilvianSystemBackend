# Roadmap Delivery Frontend — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 5
status: ACTIVE
status_synced_at: "2026-11-03 — PLAN-REPAIR-HRD-002: FE-HRD-026 dan FE-HRD-027 dikoreksi ke 🟡 (acceptance criteria belum terbukti, menunggu FIX-HRD-002-09); FE-HRD-022, FE-HRD-024, FE-HRD-025 dikerjakan ulang dan tetap ✅. Sebelumnya: tanda FE-HRD-011, FE-HRD-015, FE-HRD-016, FE-HRD-018 diselaraskan ke 🟡 mengikuti register"
planning_updated_at: "2026-11-03 — Slice S-D1 Perencanaan Tenaga Kerja dipecah menjadi 15 task (FE-HRD-029 s.d. FE-HRD-043) per gelombang MVP-7 s.d. MVP-10; grafik urutan dependency ditulis ulang per slice; kolom Dependency ditambahkan pada register"
generated_at: "2026-10-26"
owners:
  - "Frontend Technical Owner — HR UI Lead"
approved_by:
  - "User Approved (26 Oktober 2026) — roadmap awal"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 3 November 2026 — desain S-D1 yang menjadi dasar task FE-HRD-029 s.d. FE-HRD-043"
blueprint_manifest_revision: 14
decision_ids_s_d1: "HRD-DEC-058 s.d. HRD-DEC-072"
source_commits:
  backend: "26066c76e69b021f68b63b31bde2258cd9d1bb74 (branch MHamzah)"
  frontend: "66b7e5600e1707e4153ca07981ddef054856c8c9 (branch HamzahV2) — HEAD saat roadmap S-D1 ditulis"
contract_versions:
  - "API Contract v6 — task FE-HRD-001 s.d. FE-HRD-028"
  - "API Contract v7 bagian 13 — task S-D1"
  - "Frontend Architecture rev 3 bagian 12 — task S-D1"
  - "Permission Audit Matrix v7 bagian 11 — task S-D1"
  - "Validation Matrix v7 bagian 12 — pesan yang ditampilkan layar S-D1"
locked_input_hashes_s_d1:
  03-frontend-architecture.md: 7c2db1244ccd3fdc7ce81c3b1db6220cbfd0fee9d6cdfc70a42f18b981705bda
  04-prd-to-mvp.md: 8db8cdcd95cf70b46afa35cf3a2a8691eb78681027645aebed8625a5aea1b6ea
  contracts/api-contract.md: 772901e44a9f69213e2ac63404045288bec61742eba73a816f5f43faf9a01328
  contracts/permission-audit-matrix.md: e41eca8c8f9c53b0dd5f10a2c294892128e0c00d8eb760f67a48dcb8a5fcb182
  contracts/validation-matrix.md: 27055a3971cea05d93075a527bc18acbeb911727f0fbdb3fa137a54aece5bd31
  testing/acceptance-test-matrix.md: ee5e5bb1e29a9519f76d5995021f11cfaa8384c4888471b442d076d0ef44db9b
```

**Apa yang berubah pada revision 5.**

1. **Lima belas task `S-D1` baru**, `FE-HRD-029` s.d. `FE-HRD-043`, untuk 13 layar `FE-HRD-97` s.d.
   `FE-HRD-109` pada `03-frontend-architecture.md` bagian 12. Perhatikan dua jenis ID: `FE-HRD-0xx`
   (tiga digit) adalah **task**, sedangkan `FE-HRD-97` s.d. `FE-HRD-109` adalah **layar** pada
   dokumen arsitektur frontend.
2. **Grafik ditulis ulang per slice.** Grafik lama memakai karakter `┼` yang tidak sah, memuat 28
   task dalam satu blok, dan arah beberapa garisnya tidak dapat dibaca pasti. Hubungannya dibaca
   ulang begini dan kini tercatat di kolom `Dependency` register: `FE-HRD-003` s.d. `FE-HRD-007`
   menunggu `FE-HRD-001` dan `BE-HRD-004`; `FE-HRD-009` s.d. `FE-HRD-012` masing-masing menunggu
   satu task backend (`BE-HRD-006` s.d. `BE-HRD-009`); persilangan garis lama tidak dibaca sebagai
   sambungan.
3. **Tiga dependency disederhanakan** karena sudah tercakup lewat prasyarat lain:
   `FE-HRD-023` dan `FE-HRD-024` tidak lagi mencantumkan `BE-HRD-025` (sudah lewat `FE-HRD-022`),
   dan `FE-HRD-028` tidak lagi mencantumkan `FE-HRD-021`. Urutan kerja tidak berubah.
4. **Empat tanda diselaraskan.** Register sudah mencatat `FE-HRD-011`, `FE-HRD-015`,
   `FE-HRD-016`, dan `FE-HRD-018` sebagai 🟡 sejak `PLAN-REPAIR-HRD-001` (menunggu K-02/K-03),
   sementara kartu dan grafik lama masih ✅. Kartu dan grafik kini ikut 🟡; alasannya ditulis di
   baris status kartunya. Bukti terbaru: `FIX-HRD-001-06` dan `FIX-HRD-001-07` masih ⛔ pada
   `docs/plan-repair/plan-repair-001-layar-hr-kosong-salah-alamat-api.md`.

**Arti tanda status pada dokumen ini:**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Tampilan, alur interaksi, Redux slice/service, dan validasi form terbukti pada source serta laporan task tracked tersedia |
| 🟡 | Sebagian. Tampilan atau struktur rute sudah ada, namun integrasi API atau validasi belum penuh |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi: keputusan klinis, keselamatan medis, atau gerbang lain yang disebut namanya |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

Roadmap frontend HR memuat **43 task**: 28 task lama dan 15 task `S-D1`. Grafiknya dipecah menjadi
satu ringkasan antar-slice dan sembilan grafik per slice.

Cara membaca:

- `A ─> B` artinya **A harus selesai dulu, baru B boleh dimulai**. Prasyarat selalu di kiri.
- `[BE]` = cermin baca-saja task backend pada `backend-roadmap.md`. Tandanya disalin dari sana.
- `[F1]` s.d. `[F8]` = cermin baca-saja task frontend yang tempat aslinya di grafik lain pada
  dokumen ini. Setiap task punya tepat satu tempat asli.
- Node dalam kurung kurawal adalah gerbang keputusan, bukan task.

### Grafik ringkasan antar-slice

Node pada grafik ini adalah slice, bukan task.

```text
IN-01 ✅ ─┬─> S0-B ✅ ─┬─> S-A1 ✅
          │            │
          │            └─> S-A2..S-A7 ✅
          │
          └─> S-D1 MVP-7 ─> S-D1 MVP-8 ─> S-D1 MVP-9 ─> S-D1 MVP-10
```

`S-B1` s.d. `S-B5` 🟡, `S-C2` s.d. `S-C5` 🟡, dan `S-D3` ✅ tidak menunggu slice frontend lain;
prasyaratnya task backend. `S-C1` dan `S-C6` ⛔ tertahan gerbang keputusan.

### Grafik F1 — Fondasi (`IN-01`, `S0-B`)

```text
FE-HRD-021 ✅ ─> FE-HRD-001 ✅ ─> FE-HRD-002 ✅
```

### Grafik F2 — Administrasi Kepegawaian (`S-A1`)

```text
                    BE-HRD-025 ✅ [BE] ─┐
                                        │
FE-HRD-002 ✅ [F1] ─┬───────────────────┴─> FE-HRD-022 ✅ ─┬─> FE-HRD-023 ✅
                    │                                      │
                    │                                      ├─> FE-HRD-024 ✅
                    │                                      │
                    │                                      └─> FE-HRD-028 ✅
                    │
                    │   BE-HRD-026 ✅ [BE] ─┐
                    │                       │
                    ├───────────────────────┴─> FE-HRD-025 ✅
                    │
                    │   BE-HRD-027 ✅ [BE] ─┐
                    │                       │
                    ├───────────────────────┴─> FE-HRD-026 🟡
                    │
                    │   BE-HRD-028 ✅ [BE] ─┐
                    │                       │
                    └───────────────────────┴─> FE-HRD-027 🟡
```

### Grafik F3 — Layanan mandiri dan kotak masuk (`S-A2` s.d. `S-A7`)

```text
                    BE-HRD-004 ✅ [BE] ─┐
                                        │
FE-HRD-001 ✅ [F1] ─────────────────────┴─┬─> FE-HRD-003 ✅ ─┬──┬──┬──┬─> FE-HRD-008 ✅
                                          │                  │  │  │  │
                                          ├─> FE-HRD-004 ✅ ─┘  │  │  │
                                          │                     │  │  │
                                          ├─> FE-HRD-005 ✅ ────┘  │  │
                                          │                        │  │
                                          ├─> FE-HRD-006 ✅ ───────┘  │
                                          │                           │
                                          └─> FE-HRD-007 ✅ ──────────┘
```

### Grafik F4 — Waktu kerja, pelatihan, dan kinerja (`S-B1` s.d. `S-B5`, `S-C2`, `S-C3`)

```text
BE-HRD-006 ✅ [BE] ─> FE-HRD-009 ✅ ─┬──┬─> FE-HRD-013 ✅
                                     │  │
BE-HRD-007 ✅ [BE] ─> FE-HRD-010 ✅ ─┘  │
                                        │
BE-HRD-008 ✅ [BE] ─> FE-HRD-011 🟡 ────┘

BE-HRD-009 ✅ [BE] ─> FE-HRD-012 ✅ ─> FE-HRD-015 🟡 ─> FE-HRD-016 🟡
```

### Grafik F5 — Lifecycle, disiplin, benefit, dan slice terblokir (`S-C4`, `S-C5`, `S-D3`, `S-C1`, `S-C6`)

```text
BE-HRD-014 ✅ [BE] ─> FE-HRD-017 ✅ ─> FE-HRD-018 🟡

BE-HRD-019 ✅ [BE] ─> FE-HRD-020 ✅

{DEC-HRD-008 ⛔} ─> FE-HRD-014 ⛔

{DEC-HRD-010 ⛔} ─> FE-HRD-019 ⛔
```

### Grafik F6 — `S-D1` `MVP-7`: fondasi layar, pengaturan, formasi, batch formasi awal

```text
                       BE-HRD-032 [BE] ─┐
                                        │
FE-HRD-021 ✅ [F1] ─┬─> FE-HRD-029 ─┬───┴─> FE-HRD-030
                    │               │
BE-HRD-031 [BE] ────┘               │   BE-HRD-033 [BE] ─┐
                                    │                    │
                                    └────────────────────┴─> FE-HRD-031
```

### Grafik F7 — `S-D1` `MVP-8`: angka beban, standar, gap langsung

```text
                   BE-HRD-034 [BE] ─┐
                                    │
FE-HRD-029 [F6] ─┬──────────────────┴─> FE-HRD-032 ─┐
                 │                                  │
                 │ BE-HRD-035 [BE] ─┐               │
                 │                  │               │
                 ├──────────────────┴─> FE-HRD-033 ─┴─> FE-HRD-034
                 │
                 │ BE-HRD-037 [BE] ─┐
                 │                  │
                 └──────────────────┴─> FE-HRD-035
```

### Grafik F8 — `S-D1` `MVP-9`: permintaan headcount, antrean rekrutmen, potret gap

```text
                   BE-HRD-042 [BE] ─┐
                                    │   BE-HRD-043 [BE] ─┐
                                    │                    │
FE-HRD-029 [F6] ─┬──────────────────┴────────────────────┴─> FE-HRD-039 ─┐
                 │                                                       │
                 │ BE-HRD-040 [BE] ─┐                                    │
                 │                  │                                    │
                 └──────────────────┴─> FE-HRD-036 ─┐                    │
                                                    │                    │
FE-HRD-035 [F7] ─┬──────────────────────────────────┴─> FE-HRD-037 ──────┴──┬─> FE-HRD-038
                 │                                                          │
                 │ BE-HRD-039 [BE] ─┐                                       │
                 │                  │                                       │
                 └──────────────────┴─> FE-HRD-040                          │
                                                                            │
BE-HRD-044 [BE] ────────────────────────────────────────────────────────────┘
```

### Grafik F9 — `S-D1` `MVP-10`: rencana tahunan dan verifikasi keterjangkauan layar

```text
  BE-HRD-046 [BE] ─┐
                   │
FE-HRD-029 [F6] ───┴─> FE-HRD-041 ─> FE-HRD-042 ─┬──┬──┬──┬──┬──┬─> FE-HRD-043
                                                 │  │  │  │  │  │
FE-HRD-030 [F6] ─────────────────────────────────┘  │  │  │  │  │
                                                    │  │  │  │  │
FE-HRD-031 [F6] ────────────────────────────────────┘  │  │  │  │
                                                       │  │  │  │
FE-HRD-034 [F7] ───────────────────────────────────────┘  │  │  │
                                                          │  │  │
FE-HRD-038 [F8] ──────────────────────────────────────────┘  │  │
                                                             │  │
FE-HRD-039 [F8] ─────────────────────────────────────────────┘  │
                                                                │
FE-HRD-040 [F8] ────────────────────────────────────────────────┘
```

**Pemeriksaan silang.** Jumlah pasangan prasyarat → task: F1 2, F2 11, F3 15, F4 9, F5 5, F6 6,
F7 8, F8 12, F9 10 — total **78**. Jumlah entri kolom `Dependency` pada register juga **78** (42
untuk task lama, 36 untuk task `S-D1`). Grafiknya bebas siklus.

---

## Tabel Gelombang Eksekusi

Gelombang dihitung dari prasyarat **frontend** saja. Prasyarat backend `[BE]` tidak menaikkan
nomor gelombang, tetapi ditulis di kolom kedua: task itu baru boleh dimulai setelah task backend-nya
✅. Hari ini **belum ada task `S-D1` frontend yang boleh dimulai**, karena seluruh prasyarat
backend-nya belum dikerjakan.

Urutan **rilis dan UAT** tetap `MVP-7` → `MVP-8` → `MVP-9` → `MVP-10` (`04-prd-to-mvp.md` bagian
21.20); gelombang di bawah hanya kapan sebuah layar paling cepat boleh dibangun.

| Gelombang | Boleh mulai setelah | Task Frontend |
| ---: | --- | --- |
| **1** | — dari sisi frontend; backend: `BE-HRD-006`, `BE-HRD-007`, `BE-HRD-008`, `BE-HRD-009`, `BE-HRD-014`, `BE-HRD-019` ✅ | `FE-HRD-021` ✅, `FE-HRD-009` ✅, `FE-HRD-010` ✅, `FE-HRD-011` 🟡, `FE-HRD-012` ✅, `FE-HRD-017` ✅, `FE-HRD-020` ✅ |
| **2** | `FE-HRD-021`, `FE-HRD-009` s.d. `FE-HRD-012`, `FE-HRD-017`; backend `BE-HRD-031` | `FE-HRD-001` ✅, `FE-HRD-013` ✅, `FE-HRD-015` 🟡, `FE-HRD-018` 🟡; **`FE-HRD-029`** (`MVP-7`) — menunggu `BE-HRD-031` |
| **3** | `FE-HRD-001`, `FE-HRD-015`, `FE-HRD-029`; backend `BE-HRD-004` ✅ dan task `S-D1` di kolom kanan | `FE-HRD-002` ✅, `FE-HRD-003` s.d. `FE-HRD-007` ✅, `FE-HRD-016` 🟡; `FE-HRD-030` (`BE-HRD-032`), `FE-HRD-031` (`BE-HRD-033`) — `MVP-7`; `FE-HRD-032` (`BE-HRD-034`), `FE-HRD-033` (`BE-HRD-035`), `FE-HRD-035` (`BE-HRD-037`) — `MVP-8`; `FE-HRD-036` (`BE-HRD-040`), `FE-HRD-039` (`BE-HRD-042`, `BE-HRD-043`) — `MVP-9`; `FE-HRD-041` (`BE-HRD-046`) — `MVP-10` |
| **4** | Prasyarat frontend gelombang 3 masing-masing; backend `BE-HRD-025` s.d. `BE-HRD-028` ✅, `BE-HRD-039` | `FE-HRD-008` ✅, `FE-HRD-022` ✅, `FE-HRD-025` ✅, `FE-HRD-026` 🟡, `FE-HRD-027` 🟡; `FE-HRD-034` (`MVP-8`); `FE-HRD-037`, `FE-HRD-040` (`BE-HRD-039`) — `MVP-9`; `FE-HRD-042` (`MVP-10`) |
| **5** | `FE-HRD-022`; `FE-HRD-037`, `FE-HRD-039`; backend `BE-HRD-044` | `FE-HRD-023` ✅, `FE-HRD-024` ✅, `FE-HRD-028` ✅; `FE-HRD-038` (`MVP-9`) — menunggu `BE-HRD-044` |
| **6** | `FE-HRD-030`, `FE-HRD-031`, `FE-HRD-034`, `FE-HRD-038`, `FE-HRD-039`, `FE-HRD-040`, `FE-HRD-042` | `FE-HRD-043` (verifikasi menu dan keterjangkauan layar `S-D1`) |
| **⛔** | `{DEC-HRD-008}` — Komite Medik | `FE-HRD-014` ⛔ (Layar Kredensial Medis & SPK/RKK) |
| **⛔** | `{DEC-HRD-010}` — K3RS / MCU Pegawai | `FE-HRD-019` ⛔ (Layar Kesehatan Staf K3RS) |

**Task frontend yang ikut tertahan gap backend.** Tidak ada task frontend yang langsung menunggu
`BE-HRD-045`, `BE-HRD-049`, `BE-HRD-050`, atau `BE-HRD-051` ⛔. Akibatnya pada layar: pesan
`VR-WFP-36`, `VR-WFP-21`, dan `VR-WFP-23` baru dapat diperiksa di layar setelah task backend itu
selesai; layar cukup menampilkan pesan apa pun yang dikirim backend.

---

## Keputusan UI yang masih terbuka

| Hal | Status | Perlakuan di task |
| --- | --- | --- |
| Nama grup "Perencanaan Tenaga Kerja", kesembilan butir menu, dan pengganti nama butir lama "Kebutuhan Tenaga Kerja" | `OPEN` — pemilik produk, `HRD-FE-04` | Usulan kerja dipakai apa adanya: grup "Perencanaan Tenaga Kerja", butir lama menjadi "Persyaratan Berkas Tenaga Kerja". Mengganti nama nanti cukup di satu tempat. Tidak memblokir |
| Urutan dan ikon butir menu | `DEV_DISCRETION` | Builder memilih, mengikuti grup HR tetangga |
| Penyusun standar dan rencana sebagai satu layar berbagian atau berlangkah | `DEV_DISCRETION` | Builder memilih dengan rekomendasi pada laporan task |
| Pemakaian ulang atau komponen baru per elemen | Diputuskan saat eksekusi | Mengikuti `base-component-decision-gate` pada `build-module-frontend` |
| Formasi tanpa tombol ubah; nilai `null` tampil "data belum tersedia"; biaya selalu berlabel "estimasi" | **Mengikat** — `HRD-DEC-062`, `HRD-DEC-061`, `HRD-DEC-065` | Menjadi acceptance criteria |

---

## Register Status Task Frontend

| Task ID | Slice | Ringkasan Layar & Komponen UI | Status | Dependency | Laporan Bukti |
| --- | --- | --- | :---: | --- | --- |
| `FE-HRD-001` | `S0-B` | Penyelarasan Rute Kebab-Case pada Endpoint Constants Master Data | ✅ | `FE-HRD-021` ✅ | [FE-HRD-001.md](../task/report/frontend/FE-HRD-001.md) |
| `FE-HRD-002` | `S-A1` | Layar Daftar Lintas-Pegawai Administrasi Kepegawaian (`/hr/workforce-core/*`) | ✅ | `FE-HRD-001` ✅ | [FE-HRD-002.md](../task/report/frontend/FE-HRD-002.md) |
| `FE-HRD-003` | `S-A2` | Portal Layanan Mandiri Cuti Pegawai (`/self-services/.../leave`) | ✅ | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | [FE-HRD-003.md](../task/report/frontend/FE-HRD-003.md) |
| `FE-HRD-004` | `S-A3` | Portal Layanan Mandiri Lembur Pegawai (`/self-services/.../overtime`) | ✅ | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | [FE-HRD-004.md](../task/report/frontend/FE-HRD-004.md) |
| `FE-HRD-005` | `S-A4` | Portal Layanan Mandiri Ubah Jadwal & Tukar Shift (`/self-services/.../schedule`) | ✅ | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | [FE-HRD-005.md](../task/report/frontend/FE-HRD-005.md) |
| `FE-HRD-006` | `S-A5` | Portal Layanan Mandiri Koreksi Absensi Mandiri (`/self-services/.../attendance-correction`) | ✅ | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | [FE-HRD-006.md](../task/report/frontend/FE-HRD-006.md) |
| `FE-HRD-007` | `S-A6` | Migrasi & Konsolidasi Form Absensi Harian ke Standar Self-Service Karyawan | ✅ | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | [FE-HRD-007.md](../task/report/frontend/FE-HRD-007.md) |
| `FE-HRD-008` | `S-A7` | Layar Kotak Masuk Persetujuan Terpadu Atasan (Unified Approval Inbox) | ✅ | `FE-HRD-003` ✅, `FE-HRD-004` ✅, `FE-HRD-005` ✅, `FE-HRD-006` ✅, `FE-HRD-007` ✅ | [FE-HRD-008.md](../task/report/frontend/FE-HRD-008.md) |
| `FE-HRD-009` | `S-B1` | Layar Administrasi Monitoring Presensi & Rekapitulasi Kehadiran HR | ✅ | `BE-HRD-006` ✅ [BE] | [FE-HRD-009.md](../task/report/frontend/FE-HRD-009.md) |
| `FE-HRD-010` | `S-B2` | Layar Administrasi Alokasi & Penyesuaian Saldo Cuti Karyawan | ✅ | `BE-HRD-007` ✅ [BE] | [FE-HRD-010.md](../task/report/frontend/FE-HRD-010.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-011` | `S-B3` | Layar Administrasi Verifikasi & Rekonsiliasi Jam Lembur HR | 🟡 | `BE-HRD-008` ✅ [BE] | [FE-HRD-011.md](../task/report/frontend/FE-HRD-011.md) (Menunggu K-03 / FIX-HRD-001-07) |
| `FE-HRD-012` | `S-B4` | Layar Matriks Penjadwalan Kerja & Roster Shift Instalasi 24/7 | ✅ | `BE-HRD-009` ✅ [BE] | [FE-HRD-012.md](../task/report/frontend/FE-HRD-012.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-013` | `S-B5` | Layar Penggajian Restitusi & Potongan Cicilan Piutang RS | ✅ | `FE-HRD-009` ✅, `FE-HRD-010` ✅, `FE-HRD-011` 🟡 | [FE-HRD-013.md](../task/report/frontend/FE-HRD-013.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-014` | `S-C1` | Layar Pengajuan Kredensial & Pengesahan Kewenangan Klinis Dokter | ⛔ | `{DEC-HRD-008 ⛔}` | Terblokir `{DEC-HRD-008}` |
| `FE-HRD-015` | `S-C2` | Layar Manajemen Pelatihan Wajib, Kehadiran Diklat, dan Sertifikasi | 🟡 | `FE-HRD-012` ✅ | [FE-HRD-015.md](../task/report/frontend/FE-HRD-015.md) (Menunggu K-02 / FIX-HRD-001-06) |
| `FE-HRD-016` | `S-C3` | Layar Siklus Evaluasi Kinerja, Pengisian Nilai KPI, dan Hasil Review | 🟡 | `FE-HRD-015` 🟡 | [FE-HRD-016.md](../task/report/frontend/FE-HRD-016.md) (Menunggu K-02 / FIX-HRD-001-06) |
| `FE-HRD-017` | `S-C4` | Layar Pemantauan Exit Clearance dan Kliring Aset Karyawan | ✅ | `BE-HRD-014` ✅ [BE] | [FE-HRD-017.md](../task/report/frontend/FE-HRD-017.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-018` | `S-C5` | Layar Pencatatan Hubungan Karyawan, Kasus Disiplin, dan Surat Peringatan | 🟡 | `FE-HRD-017` ✅ | [FE-HRD-018.md](../task/report/frontend/FE-HRD-018.md) (Menunggu K-02 / FIX-HRD-001-06) |
| `FE-HRD-019` | `S-C6` | Layar Rekam Medis Kerja & Pemantauan Kesehatan Karyawan K3RS | ⛔ | `{DEC-HRD-010 ⛔}` | Terblokir `{DEC-HRD-010}` |
| `FE-HRD-020` | `S-D3` | Layar Pengelolaan Kepesertaan Benefit Karyawan & Penjamin Tanggungan | ✅ | `BE-HRD-019` ✅ [BE] | [FE-HRD-020.md](../task/report/frontend/FE-HRD-020.md) |
| `FE-HRD-021` | `IN-01` | Verifikasi dan Pemeliharaan 62 Kelompok Layar Master Data HR Terpasang | ✅ | — | Terpasang Aktif |
| `FE-HRD-022` | `S-A1` | Layar Daftar Dokumen SK Kepegawaian (`/hr/workforce-core/personnel-actions`) | ✅ | `FE-HRD-002` ✅, `BE-HRD-025` ✅ [BE] | [FE-HRD-022.md](../task/report/frontend/FE-HRD-022.md) (Diperbarui PLAN-REPAIR-HRD-002) |
| `FE-HRD-023` | `S-A1` | Rute Halaman & Daftar SK Kepegawaian (`/hr/personnel-action-orders`) | ✅ | `FE-HRD-022` ✅ | [FE-HRD-023.md](../task/report/frontend/FE-HRD-023.md) |
| `FE-HRD-024` | `S-A1` | Form Penerbitan SK, Upload Pindai Fisik, & Layar Detail Approval SK | ✅ | `FE-HRD-022` ✅ | [FE-HRD-024.md](../task/report/frontend/FE-HRD-024.md) (Diperbarui PLAN-REPAIR-HRD-002) |
| `FE-HRD-025` | `S-A1` | Antarmuka Profil Terpadu Pegawai (Employee 360) Dual-View Segregation | ✅ | `FE-HRD-002` ✅, `BE-HRD-026` ✅ [BE] | [FE-HRD-025.md](../task/report/frontend/FE-HRD-025.md) (Diperbarui PLAN-REPAIR-HRD-002) |
| `FE-HRD-026` | `S-A1` | Layar Batch Inisialisasi Penempatan Awal Pegawai (55 Staf Eksisting) | 🟡 | `FE-HRD-002` ✅, `BE-HRD-027` ✅ [BE] | [FE-HRD-026.md](../task/report/frontend/FE-HRD-026.md) (Menunggu FIX-HRD-002-09) |
| `FE-HRD-027` | `S-A1` | Form Permohonan Perubahan Data Kuasa HR (On-Behalf) Berkas Kuasa | 🟡 | `FE-HRD-002` ✅, `BE-HRD-028` ✅ [BE] | [FE-HRD-027.md](../task/report/frontend/FE-HRD-027.md) (Menunggu FIX-HRD-002-09) |
| `FE-HRD-028` | `S-A1` | Penguncian Form Master Data Pegawai (Lock Unit & Jabatan Read-Only) | ✅ | `FE-HRD-022` ✅ | [FE-HRD-028.md](../task/report/frontend/FE-HRD-028.md) |
| `FE-HRD-029` | `S-D1` · `MVP-7` | Fondasi layar `S-D1`: grup menu, konstanta endpoint, util tampilan, dan layar Pengaturan Perencanaan (`FE-HRD-109`) | | `FE-HRD-021` ✅, `BE-HRD-031` [BE] | — |
| `FE-HRD-030` | `S-D1` · `MVP-7` | Layar Formasi Jabatan baca saja beserta riwayat versi (`FE-HRD-101`) | | `FE-HRD-029`, `BE-HRD-032` [BE] | — |
| `FE-HRD-031` | `S-D1` · `MVP-7` | Layar Batch Formasi Awal dan detailnya (`FE-HRD-102`, `FE-HRD-103`) | | `FE-HRD-029`, `BE-HRD-033` [BE] | — |
| `FE-HRD-032` | `S-D1` · `MVP-8` | Layar Angka Beban (`FE-HRD-100`) | | `FE-HRD-029`, `BE-HRD-034` [BE] | — |
| `FE-HRD-033` | `S-D1` · `MVP-8` | Layar Daftar Standar Kebutuhan Tenaga (`FE-HRD-98`) | | `FE-HRD-029`, `BE-HRD-035` [BE] | — |
| `FE-HRD-034` | `S-D1` · `MVP-8` | Layar Penyusun dan Detail Standar beserta rekomendasi (`FE-HRD-99`) | | `FE-HRD-032`, `FE-HRD-033` | — |
| `FE-HRD-035` | `S-D1` · `MVP-8` | Layar Pemantauan Gap Tenaga — gap langsung (`FE-HRD-97`) | | `FE-HRD-029`, `BE-HRD-037` [BE] | — |
| `FE-HRD-036` | `S-D1` · `MVP-9` | Layar Daftar Permintaan Headcount (`FE-HRD-106`) | | `FE-HRD-029`, `BE-HRD-040` [BE] | — |
| `FE-HRD-037` | `S-D1` · `MVP-9` | Form dan Detail Permintaan Headcount (`FE-HRD-107`) serta tombol "Ajukan" di Pemantauan Gap | | `FE-HRD-035`, `FE-HRD-036` | — |
| `FE-HRD-038` | `S-D1` · `MVP-9` | Aksi kursi pada detail permintaan: tautkan/lepas, ubah rencana pemenuhan, batalkan kursi | | `FE-HRD-037`, `FE-HRD-039`, `BE-HRD-044` [BE] | — |
| `FE-HRD-039` | `S-D1` · `MVP-9` | Layar Antrean Rekrutmen beserta dialog tautan pemenuhan (`FE-HRD-108`) | | `FE-HRD-029`, `BE-HRD-042` [BE], `BE-HRD-043` [BE] | — |
| `FE-HRD-040` | `S-D1` · `MVP-9` | Panel potret gap pada Pemantauan Gap Tenaga | | `FE-HRD-035`, `BE-HRD-039` [BE] | — |
| `FE-HRD-041` | `S-D1` · `MVP-10` | Layar Daftar Rencana Tahunan dan Addendum (`FE-HRD-104`) | | `FE-HRD-029`, `BE-HRD-046` [BE] | — |
| `FE-HRD-042` | `S-D1` · `MVP-10` | Layar Penyusun dan Detail Rencana (`FE-HRD-105`) | | `FE-HRD-041` | — |
| `FE-HRD-043` | `S-D1` · Verifikasi | Verifikasi menu, hak akses, dan keterjangkauan seluruh layar `S-D1` | | `FE-HRD-030`, `FE-HRD-031`, `FE-HRD-034`, `FE-HRD-038`, `FE-HRD-039`, `FE-HRD-040`, `FE-HRD-042` | — |

---

## Kartu Detail Task Frontend

### `FE-HRD-001` — Penyelarasan Rute Kebab-Case pada Endpoint Constants Master Data
* **Slice**: `S0-B`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-001.md](../task/report/frontend/FE-HRD-001.md)
* **Bukti**: 8 file konstanta dan slice diperbarui ke kebab-case (`work-calendars`, `work-schedules`, `shift-groups`, `shift-patterns`, `disciplinary-action-types`, `employee-relation-case-types`, `sanction-types`, `violation-types`).

### `FE-HRD-002` — Layar Daftar Lintas-Pegawai Administrasi Kepegawaian
* **Slice**: `S-A1`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-002.md](../task/report/frontend/FE-HRD-002.md)
* **Bukti**: 6 rute aktif di `/hr/workforce-core/` (`employee-profile-changes`, `organization-assignments`, `position-assignments`, `manager-assignments`, `employment-histories`, `salary-assignments`) terhubung ke `WorkforceCoreCrossEmployeeService`.

### `FE-HRD-003` — Portal Layanan Mandiri Cuti Pegawai (ESS)
* **Slice**: `S-A2`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-003.md](../task/report/frontend/FE-HRD-003.md)
* **Bukti**: Halaman `/self-services/human-resource/leave` dengan kartu sisa saldo cuti, tabel riwayat, dan modal interaktif permohonan cuti baru.

### `FE-HRD-004` — Portal Layanan Mandiri Lembur Pegawai (ESS)
* **Slice**: `S-A3`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-004.md](../task/report/frontend/FE-HRD-004.md)
* **Bukti**: Halaman `/self-services/human-resource/overtime` dengan fitur dual-tab (Rencana Lembur Pre-Overtime & Klaim Lembur Post-Overtime) terhubung ke API ESS.

### `FE-HRD-005` — Portal Layanan Mandiri Ubah Jadwal & Tukar Shift (ESS)
* **Slice**: `S-A4`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-005.md](../task/report/frontend/FE-HRD-005.md)
* **Bukti**: Halaman `/self-services/human-resource/schedule` dengan tabel jadwal dinas aktif dan dua modal pengajuan (Ubah Jadwal & Tukar Shift).

### `FE-HRD-006` — Portal Layanan Mandiri Koreksi Absensi Mandiri (ESS)
* **Slice**: `S-A5`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-006.md](../task/report/frontend/FE-HRD-006.md)
* **Bukti**: Halaman `/self-services/human-resource/attendance-correction` dengan modal pengajuan perbaikan jam masuk/keluar terhubung ke backend.

### `FE-HRD-007` — Migrasi & Konsolidasi Form Absensi Harian ke Standar Self-Service Karyawan
* **Slice**: `S-A6`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-007.md](../task/report/frontend/FE-HRD-007.md)
* **Bukti**: Rute kanonikal `/self-services/human-resource/attendance` aktif mengarahkan ke `AttendanceEmployeeView`.

### `FE-HRD-008` — Layar Kotak Masuk Persetujuan Terpadu Atasan (Unified Approval Inbox)
* **Slice**: `S-A7`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-008.md](../task/report/frontend/FE-HRD-008.md)
* **Bukti**: Halaman `/hr/workflow/approval-inbox` dengan aksi Approve, Reject, Revision, terdaftar di sidebar menu `menu-items.jsx` (`corporateApprovalInbox`).

### `FE-HRD-009` — Layar Administrasi Monitoring Presensi & Rekapitulasi Kehadiran HR
* **Slice**: `S-B1`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-009.md](../task/report/frontend/FE-HRD-009.md)
* **Bukti**: Halaman `/hr/attendance` dengan dasbor kehadiran, modal koreksi on-behalf, dan pengamanan penutupan periode (closed period guard).

### `FE-HRD-010` — Layar Administrasi Alokasi & Penyesuaian Saldo Cuti Karyawan
* **Slice**: `S-B2`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-010.md](../task/report/frontend/FE-HRD-010.md)
* **Bukti**: Halaman `/hr/leave/adjustments` dengan rekap kuota cuti dan modal penyesuaian saldo berdokumen audit.

### `FE-HRD-011` — Layar Administrasi Verifikasi & Rekonsiliasi Jam Lembur HR
* **Slice**: `S-B3`
* **Status**: 🟡 **SEBAGIAN** — diselaraskan 3 November 2026 dengan register yang sudah mencatat 🟡 sejak `PLAN-REPAIR-HRD-001`. Semula ✅ Selesai (28 Oktober 2026). Yang belum: kontrol golongan C/D menunggu keputusan K-03 (`FIX-HRD-001-07` ⛔).
* **Laporan**: [FE-HRD-011.md](../task/report/frontend/FE-HRD-011.md)
* **Bukti**: Halaman `/hr/overtime/reconciliation` dengan perbandingan jam rencana vs jam mesin absensi aktual.

### `FE-HRD-012` — Layar Matriks Penjadwalan Kerja & Roster Shift Instalasi 24/7
* **Slice**: `S-B4`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-012.md](../task/report/frontend/FE-HRD-012.md)
* **Bukti**: Halaman `/hr/scheduling/roster` dengan modal buat periode roster serta aksi Publish dan Lock jadwal dinas.

### `FE-HRD-013` — Layar Penggajian Restitusi & Potongan Cicilan Piutang RS
* **Slice**: `S-B5`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-013.md](../task/report/frontend/FE-HRD-013.md)
* **Bukti**: Halaman `/hr/payroll/restitution-installment` dengan dual-tab (Restitusi & Cicilan) dan format nominal mata uang IDR.

### `FE-HRD-014` — Layar Pengajuan Kredensial & Pengesahan Kewenangan Klinis Dokter
* **Slice**: `S-C1`
* **Status**: ⛔ Terblokir (`DEC-HRD-008`).
* **Catatan**: Menunggu pengesahan wewenang dari Komite Medik rumah sakit. AI dilarang mengarang hak verifikasi kewenangan klinis dokter.

### `FE-HRD-015` — Layar Manajemen Pelatihan Wajib, Kehadiran Diklat, dan Sertifikasi
* **Slice**: `S-C2`
* **Status**: 🟡 **SEBAGIAN** — diselaraskan 3 November 2026 dengan register yang sudah mencatat 🟡 sejak `PLAN-REPAIR-HRD-001`. Semula ✅ Selesai (28 Oktober 2026). Yang belum: daftar lintas-pegawai menunggu keputusan K-02 (`FIX-HRD-001-06` ⛔).
* **Laporan**: [FE-HRD-015.md](../task/report/frontend/FE-HRD-015.md)
* **Bukti**: Halaman `/hr/learning/training-records` dengan pemantauan sertifikat kompetensi (BHD, PPI, KPRS) dan modal rekam pelatihan.

### `FE-HRD-016` — Layar Siklus Evaluasi Kinerja, Pengisian Nilai KPI, dan Hasil Review
* **Slice**: `S-C3`
* **Status**: 🟡 **SEBAGIAN** — diselaraskan 3 November 2026 dengan register yang sudah mencatat 🟡 sejak `PLAN-REPAIR-HRD-001`. Semula ✅ Selesai (28 Oktober 2026). Yang belum: daftar lintas-pegawai menunggu keputusan K-02 (`FIX-HRD-001-06` ⛔).
* **Laporan**: [FE-HRD-016.md](../task/report/frontend/FE-HRD-016.md)
* **Bukti**: Halaman `/hr/performance/reviews` dengan tabel skor capaian KPI, predikat kerja, dan modal inisiasi review baru.

### `FE-HRD-017` — Layar Pemantauan Exit Clearance dan Kliring Aset Karyawan
* **Slice**: `S-C4`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-017.md](../task/report/frontend/FE-HRD-017.md)
* **Bukti**: Halaman `/hr/exit-clearance` aktif dengan alur pemantauan checklist lintas-divisi (IT, Keuangan, Logistik, HR).

### `FE-HRD-018` — Layar Pencatatan Hubungan Karyawan, Kasus Disiplin, dan Surat Peringatan
* **Slice**: `S-C5`
* **Status**: 🟡 **SEBAGIAN** — diselaraskan 3 November 2026 dengan register yang sudah mencatat 🟡 sejak `PLAN-REPAIR-HRD-001`. Semula ✅ Selesai (28 Oktober 2026). Yang belum: daftar lintas-pegawai menunggu keputusan K-02 (`FIX-HRD-001-06` ⛔).
* **Laporan**: [FE-HRD-018.md](../task/report/frontend/FE-HRD-018.md)
* **Bukti**: Halaman `/hr/employee-relations/disciplinary-actions` dengan dasbor penanganan kasus dan modal penerbitan SP baru.

### `FE-HRD-019` — Layar Rekam Medis Kerja & Pemantauan Kesehatan Karyawan K3RS
* **Slice**: `S-C6`
* **Status**: ⛔ Terblokir (`DEC-HRD-010`).
* **Catatan**: Menunggu perumusan kebijakan privasi data rekam medis staf dan wewenang tim K3RS.

### `FE-HRD-020` — Layar Pengelolaan Kepesertaan Benefit Karyawan & Penjamin Tanggungan
* **Slice**: `S-D3`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-020.md](../task/report/frontend/FE-HRD-020.md)
* **Bukti**: Halaman `/hr/benefit/enrollments` aktif dengan rekap kepesertaan asuransi, nomor polis, dan anggota keluarga tanggungan.

### `FE-HRD-021` — Pemeliharaan dan Standarisasi 62 Layar Master Data HR
* **Slice**: `IN-01`
* **Status**: ✅ Selesai (Aktif di codebase).
* **Bukti**: 62 folder halaman di `src/app/hr/master-data/*` terhubung ke controller backend master data.


### `FE-HRD-022` — Layar Daftar Dokumen SK Kepegawaian (`/hr/workforce-core/personnel-action-orders`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026). Dikerjakan ulang 3 November 2026 (`PLAN-REPAIR-HRD-002`): tampilan dibangun ulang dengan base component, saringan Status/Kategori tampil, ringkasan dari `/summary`, menu sidebar ditambahkan; build lulus, `check-css-usage` 0 temuan, screenshot terlampir.
* **Laporan**: [FE-HRD-022.md](../task/report/frontend/FE-HRD-022.md)
* **Outcome**: Petugas HR rumah sakit memiliki dasbor terpusat untuk melihat, memantau, mencari, dan menyaring dokumen SK Kepegawaian (mutasi unit, promosi/demosi jabatan, penetapan atasan, perubahan gaji) lengkap dengan statistik status dan aksi navigasi.
* **Traceability**: `FE-HRD-08`, `HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `03-frontend-architecture.md` §2.1 & §4.2a.
* **Contract Version**: API Contract v6, Frontend Architecture rev 2.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `SummaryCards` Pola A untuk kartu statistik (Total SK, Menunggu Persetujuan, Terjadwal/Aktif, Ditolak/Dibatalkan).
  - Menggunakan `DataFilter` untuk pencarian nomor SK/nama pegawai dan saringan jenis aksi (`ActionType`) serta status SK.
  - Menggunakan `DataTable` terintegrasi dengan `RegionPagination` (mematuhi pola bebas pagination ganda).
  - Menggunakan `StatusBadge` untuk membedakan status (`Draft`, `PendingApproval`, `Scheduled`, `Applied`, `Rejected`, `Cancelled`).
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/personnel-action-orders/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/personnel-action-order-list-view.jsx`.
  3. Service client `src/lib/services/hr/personnel-action-order-service.js` (metode `getPersonnelActionOrders`).
* **Dependency**: `BE-HRD-025` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Halaman menampilkan daftar SK kepegawaian dengan kolom: Nomor SK, Pegawai, Jenis Aksi, Tanggal Efektif, Tanggal Pembukuan, Lampiran SK (ikon/indikator), Status, dan Aksi.
  2. Saringan jenis aksi dan status bekerja secara reaktif memicu pemanggilan API ber-query parameter.
  3. Menggunakan satu pagination bawaan `DataTable` tanpa kontrol duplikat.
  4. Penanganan state memuat (skeleton/spinner), data kosong (empty state), dan kegagalan API (InformationAlert dengan tombol Coba Lagi) terpasang.
* **Bukti Verifikasi**:
  - Kompilasi Next.js App Router (`npm run build` / lint pass 0 error).
  - Unit test headless Next.js / component mock (`node --test`).
* **Risiko & Pemilik**: Pemilik: Frontend Technical Owner. Risiko: respons lambat diatasi dengan pagination server-side.
* **Definition of Done**: Rute aktif, linter 0 error, integrasi API teruji, dokumen bukti pengujian tersedia.

### `FE-HRD-023` — Form Penerbitan SK Kepegawaian Baru (`/hr/workforce-core/personnel-action-orders/create`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-023.md](../task/report/frontend/FE-HRD-023.md)
* **Outcome**: Form penerbitan SK kepegawaian baru yang memandu petugas HR mengisi metadata SK, memilih penempatan/jabatan tujuan, dan mewajibkan unggah berkas pindai fisik bertanda tangan sebelum draft dapat diajukan ke jenjang persetujuan.
* **Traceability**: `FE-HRD-09`, `HRD-DEC-048`, `HRD-DEC-056`, `FR-HRD-013`, `VR-WFC-04`, `03-frontend-architecture.md` §2.1 & §4.2a.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `BaseCard` untuk segmentasi form: Bagian 1 (Identitas SK), Bagian 2 (Target Pegawai & Perubahan Penempatan), Bagian 3 (Unggah Pindai Berkas SK Fisik).
  - Kontrol input menggunakan `BaseInput`, `FilterSelect` (pencarian pegawai, unit, jabatan), `BaseDatePicker`.
  - Komponen upload berkas menggunakan validasi client-side: jenis file PDF/JPG/PNG dan ukuran maksimum 10MB.
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/personnel-action-orders/create/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/personnel-action-order-form-view.jsx`.
  3. Service client `src/lib/services/hr/personnel-action-order-service.js` (metode `createOrder`, `uploadAttachment`).
* **Dependency**: `FE-HRD-022` ✅ — disederhanakan 3 November 2026: `BE-HRD-025` ✅ [BE] sudah tercakup lewat `FE-HRD-022`, yang menunggunya langsung
* **Acceptance Criteria**:
  1. Form memvalidasi seluruh isian wajib: Nomor SK unik, Jenis Aksi, Tanggal Efektif, Pegawai, dan Berkas Pindai Fisik.
  2. Tombol simpan draft (`Save as Draft`) menyimpan record tanpa mewajibkan upload berkas, namun tombol ajukan persetujuan (`Submit for Approval`) menolak jika berkas lampiran belum diunggah (`VR-WFC-04`).
  3. Pemilihan jenis aksi mutasi/rotasi memunculkan form input unit tujuan dan jabatan tujuan secara dinamis.
  4. Notifikasi umpan balik sukses/gagal menggunakan toast / alert standar sistem.
* **Bukti Verifikasi**:
  - Form validation test, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: validasi tipe mime berkas di-guard di sisi frontend sebelum payload dikirim.
* **Definition of Done**: Form terintegrasi dengan backend API, lulus audit linter dan visual tokens.

### `FE-HRD-024` — Layar Detail & Aksi Persetujuan SK (`/hr/workforce-core/personnel-action-orders/[id]`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026). Dikerjakan ulang 3 November 2026 (`PLAN-REPAIR-HRD-002`): editor dan detail standar, pilihan bercari pengganti UUID, Tolak/Batal mengirim `reason` (sebelumnya HTTP 500), lampiran dapat diunduh dan diganti; build lulus, screenshot terlampir.
* **Laporan**: [FE-HRD-024.md](../task/report/frontend/FE-HRD-024.md)
* **Outcome**: Antarmuka detail dokumen SK kepegawaian yang menyajikan perbandingan komparatif (*before vs after* penempatan), pratinjau dokumen lampiran fisik SK, riwayat persetujuan, serta tombol aksi persetujuan/penolakan dan penerbitan SK kontra/pembatalan.
* **Traceability**: `FE-HRD-09a`, `HRD-DEC-031`, `HRD-DEC-048`, `HRD-DEC-057`, `FR-HRD-020`, `VR-WFC-05`, `VR-WFC-12`, `03-frontend-architecture.md` §2.1 & §4.2a.
* **Contract Version**: API Contract v6, State Transition Matrix v6.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `BaseCard` dengan tata letak komparatif dua kolom (Kondisi Saat Ini vs Perubahan Baru).
  - Tampilan dokumen fisik terintegrasi dengan tautan unduh / preview PDF modal.
  - Dialog konfirmasi aksi persetujuan/penolakan menggunakan `BaseDialog` dengan isian alasan wajib pada penolakan/pembatalan.
  - Tombol persetujuan disembunyikan otomatis jika aktor login adalah pembuat draft SK (larangan self-approval, `VR-WFC-05`).
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/personnel-action-orders/[id]/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/personnel-action-order-detail-view.jsx`.
  3. Service client `src/lib/services/hr/personnel-action-order-service.js` (metode `getOrderById`, `approveOrder`, `rejectOrder`, `cancelOrder`).
* **Dependency**: `FE-HRD-022` ✅ — disederhanakan 3 November 2026: `BE-HRD-025` ✅ [BE] sudah tercakup lewat `FE-HRD-022`, yang menunggunya langsung
* **Acceptance Criteria**:
  1. Menampilkan detail atribut SK, riwayat audit pembuat, dan tanggal posting.
  2. Menampilkan tabel perubahan yang mencerminkan assignment unit, jabatan, manajer, atau gaji yang akan diubah.
  3. Tombol `Setujui` hanya aktif bagi pejabat yang memiliki hak dan bukan pembuat draft SK.
  4. Aksi `Batalkan SK` untuk SK yang sudah berstatus `Applied` mewajibkan penginputan nomor SK Kontra/Pembatalan resmi.
* **Bukti Verifikasi**:
  - Pengujian alur interaksi approval dan pembatalan, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: cegah double click pada tombol approval dengan state disabled loading.
* **Definition of Done**: Layar berfungsi penuh, dialog konfirmasi aktif, audit status transisi terbukti.

### `FE-HRD-025` — Antarmuka Profil Terpadu Pegawai (Employee 360) Dual-View (`/employee-360/[id]`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026). Dikerjakan ulang 3 November 2026 (`PLAN-REPAIR-HRD-002`): alamat API dibetulkan (sebelumnya 404), tampilan `BaseDetailView` + tab standar; mode HR terbukti dengan data nyata, mode atasan lewat respons dicegat.
* **Laporan**: [FE-HRD-025.md](../task/report/frontend/FE-HRD-025.md)
* **Outcome**: Layar ringkasan komprehensif 360 derajat profil staf rumah sakit yang cerdas mengenali peran login: jika dibuka oleh HR menampilkan profil lengkap (kedinasan, kompensasi, rekening, berkas keluarga, dan disiplin); jika dibuka oleh Atasan Langsung hanya menampilkan data kedinasan, riwayat jabatan, dan kompetensi bawahan aktif tanpa menampilkan data finansial/disiplin/keluarga.
* **Traceability**: `FE-HRD-09b`, `HRD-DEC-053`, `FR-HRD-018`, `VR-WFC-06`, `VR-WFC-07`, `03-frontend-architecture.md` §2.1 & §4.2b.
* **Contract Version**: API Contract v6, Frontend Architecture rev 2.
* **UI Authority & Base Component Decisions**:
  - Header profil menggunakan komponen `Hero` dengan foto, nama, NIP/ID profil, status kepegawaian, unit, dan jabatan utama.
  - Tab navigasi menggunakan `Tabs` Quilvian: Tab 1 (Informasi Umum & Kontak), Tab 2 (Penempatan & Riwayat Jabatan), Tab 3 (Atasan & Tim Kerja), Tab 4 (Kompetensi & Pelatihan), Tab 5 (Kompensasi & Rekening — khusus HR), Tab 6 (Disiplin & Catatan Khusus — khusus HR).
  - Tampilan tab finansial dan disiplin tidak di-render sama sekali pada DOM jika view mode adalah Direct Manager.
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/employee-360/[id]/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/employee-360-view.jsx`.
  3. Service client `src/lib/services/hr/employee-360-service.js` (metode `getEmployee360`).
* **Dependency**: `BE-HRD-026` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Halaman menampilkan seluruh riwayat perjalanan dinas pegawai dalam satu layar tabbed.
  2. Jika user login bertindak sebagai Atasan Langsung, tab Kompensasi dan Disiplin disembunyikan sepenuhnya dari tampilan navigasi dan DOM.
  3. Kolom NIK, tanggal lahir, dan rekening bank tidak muncul pada mode Atasan Langsung.
  4. Tersedia tombol kembali ke daftar staf atau pohon struktur organisasi.
* **Bukti Verifikasi**:
  - Verifikasi pengujian render tampilan untuk role HR vs Manager.
  - Linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: data leakage dihindari dari kontrak backend yang sudah me-masking payload.
* **Definition of Done**: Layar 360 derajat aktif dan teruji mematuhi batasan privasi data pegawai.

### `FE-HRD-026` — Layar Batch Inisialisasi Penempatan Awal Pegawai (`/initial-placement-batch`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 3 November 2026 (`PLAN-REPAIR-HRD-002`); semula ✅ Selesai (02 November 2026). Tampilan sudah standar, tetapi 1 dari 4 acceptance criteria terbukti: service masih memanggil endpoint yang tidak ada di backend. Blocker: `FIX-HRD-002-09` ⛔.
* **Laporan**: [FE-HRD-026.md](../task/report/frontend/FE-HRD-026.md)
* **Outcome**: Antarmuka khusus petugas HR untuk memverifikasi draf penempatan awal bagi 55 pegawai eksisting yang belum memiliki assignment terstruktur, melakukan validasi baris, dan memicu eksekusi batch satu kali dengan pemantauan progress.
* **Traceability**: `FE-HRD-09c`, `HRD-DEC-054`, `FR-HRD-019`, `VR-WFC-08`, `VR-WFC-09`, `03-frontend-architecture.md` §2.1.
* **Contract Version**: API Contract v6, Data Dictionary v6.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `InformationAlert` dengan tipe warning yang menjelaskan bahwa batch ini merupakan inisialisasi awal satu kali.
  - Menggunakan `DataTable` yang menampilkan status validitas per baris pegawai (Valid / Duplikat / Data Belum Lengkap).
  - Tombol `Eksekusi Batch Penempatan` dilindungi modal konfirmasi ganda (`BaseDialog`).
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/initial-placement-batch/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx`.
  3. Service client `src/lib/services/hr/initial-placement-batch-service.js` (metode `getBatchStatus`, `executeBatch`).
* **Dependency**: `BE-HRD-027` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Layar menampilkan daftar 55 pegawai eksisting beserta usulan penempatan unit, jabatan, atasan, dan gaji awal.
  2. Terdapat indikator status validasi per baris; jika terdapat baris yang tidak valid, tombol eksekusi batch dinonaktifkan.
  3. Setelah tombol eksekusi dikonfirmasi, layar menampilkan progres dan status hasil eksekusi (Sukses / Rollback).
  4. Jika batch sudah berstatus `Executed`, form beralih ke mode read-only dan menampilkan ringkasan hasil inisialisasi.
* **Bukti Verifikasi**:
  - Pengujian render tabel batch, verifikasi disabled state saat data invalid, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: eksekusi ganda dicegah dengan idempotency key dan disabled button saat mutasi berjalan.
* **Definition of Done**: Rute aktif, eksekusi batch terhubung ke backend, audit laporan tersedia.

### `FE-HRD-027` — Form Permohonan Perubahan Data Kuasa HR (On-Behalf) (`/employee-profile-changes/on-behalf`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: 🟡 **SEBAGIAN** — dikoreksi 3 November 2026 (`PLAN-REPAIR-HRD-002`); semula ✅ Selesai (02 November 2026). Tampilan dan pilihan pegawai sudah standar, tetapi 0 dari 4 acceptance criteria terpenuhi penuh: pengiriman ditolak 415 dan berkas bukti belum dapat diunggah. Blocker: `FIX-HRD-002-09` ⛔.
* **Laporan**: [FE-HRD-027.md](../task/report/frontend/FE-HRD-027.md)
* **Outcome**: Antarmuka bagi petugas HR untuk menginput permohonan perubahan data identitas/keluarga/kontak atas nama staf (on-behalf) yang berhalangan hadir atau membutuhkan asistensi administrasi, lengkap dengan pengunggahan dokumen dasar kuasa dan integrasi ke layar daftar review.
* **Traceability**: `FE-HRD-09d`, `HRD-DEC-052`, `FR-HRD-016`, `FR-HRD-017`, `VR-WFC-10`, `03-frontend-architecture.md` §2.1 & §4.2c, menutup temuan `ISS-HRD-001-T7` dan rencana perbaikan `FIX-HRD-001-09`.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **UI Authority & Base Component Decisions**:
  - Form terstruktur dengan `BaseCard` memuat: Pemilihan Pegawai Target (`FilterSelect`), Alasan Kuasa HR (`BaseInput` textarea), Field Data yang Ingin Diubah (`BaseInput`, `BaseDatePicker`), dan Unggah Berkas Kuasa / Surat Permohonan (`BaseFileUpload`).
  - Menampilkan alert peringatan bahwa permohonan kuasa ini wajib diverifikasi oleh petugas HR lain sebelum disahkan ke database profil.
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/employee-profile-changes/on-behalf/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/employee-profile-change-on-behalf-view.jsx`.
  3. Update service client `src/lib/services/hr/workforce-core-cross-employee-service.js` (metode `createOnBehalfProfileChange`).
* **Dependency**: `BE-HRD-028` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Petugas HR dapat memilih pegawai target dan mengisikan permohonan perubahan data atas nama pegawai tersebut.
  2. Field dokumen bukti kuasa wajib diunggah sebelum tombol `Kirim Permohonan` dapat diklik.
  3. Data permohonan yang dikirim muncul pada layar `/hr/workforce-core/employee-profile-changes` dengan badge `Kuasa HR (On-Behalf)`.
  4. Layar detail review menonaktifkan tombol verifikasi jika petugas yang login sama dengan pengaju on-behalf (`VR-WFC-10`).
* **Bukti Verifikasi**:
  - Verifikasi submission on-behalf, verifikasi flag audit, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: validasi segregasi verifikator ditegakkan di frontend dan backend.
* **Definition of Done**: Form terpasang, integrasi API on-behalf teruji, linter 0 error.

### `FE-HRD-028` — Penguncian Form Master Data Pegawai (Lock Unit & Jabatan Menjadi Read-Only)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-028.md](../task/report/frontend/FE-HRD-028.md)
* **Outcome**: Penyelarasan tata kelola pada form edit Master Data Pegawai (`/hr/master-data/employees/[id]`) dengan mengunci field pilihan Unit Organisasi dan Formasi Jabatan menjadi disabled / read-only, disertai banner edukasi yang mengarahkan user bahwa pemindahan unit dan penugasan jabatan wajib melalui penerbitan SK Kepegawaian resmi.
* **Traceability**: `HRD-DEC-047`, `FR-HRD-014`, `03-frontend-architecture.md` §3.3.
* **Contract Version**: Frontend Architecture rev 2, API Contract v6.
* **UI Authority & Base Component Decisions**:
  - Modifikasi `src/components/view/hr/master-data/employees/master-data-employee-view.jsx` atau sub-komponen form edit profilnya.
  - Menempatkan `InformationAlert` tipe info di atas field unit dan jabatan dengan pesan: *"Perubahan unit kerja dan jabatan tidak dapat dilakukan langsung pada form ini. Silakan gunakan menu SK Kepegawaian untuk proses mutasi, rotasi, atau promosi resmi."* disertai tombol tautan ke `/hr/workforce-core/personnel-action-orders/create`.
  - Input field unit organisasi dan jabatan diberi prop `disabled={true}` atau dirender sebagai teks statis dengan badge informasi.
* **Cakupan yang Diharapkan**:
  1. Modifikasi berkas form edit pegawai di `src/components/view/hr/master-data/employees/`.
* **Dependency**: `FE-HRD-022` ✅ — disederhanakan 3 November 2026: `FE-HRD-021` ✅ sudah tercakup lewat rantai `FE-HRD-021` → `FE-HRD-001` → `FE-HRD-002` → `FE-HRD-022`
* **Acceptance Criteria**:
  1. Pengguna tidak dapat lagi mengganti unit kerja atau jabatan secara langsung dari form master data pegawai.
  2. Banner petunjuk muncul dengan jelas mengarahkan alur ke modul SK Kepegawaian.
  3. Field data profil lainnya (alamat, kontak, data pribadi non-kedinasan) tetap dapat disunting seperti biasa.
* **Bukti Verifikasi**:
  - Pemeriksaan visual form edit master data pegawai, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: mencegah regresi pada fungsi edit master data pegawai lainnya.
* **Definition of Done**: Field unit & jabatan terkunci read-only, banner edukasi terpasang, linter pass 0 error.

---

## Kartu Detail Task Frontend — Slice `S-D1` Perencanaan Tenaga Kerja

### Catatan umum untuk seluruh kartu `S-D1`

| Hal | Ketentuan |
| --- | --- |
| Gambaran bisnis | Layar `S-D1` dipakai HR Admin, HR Manager, Kepala Unit, Finance / Anggaran, Direktur, Recruiter, dan Auditor untuk mengelola standar kebutuhan tenaga, formasi resmi, gap, permintaan tambahan tenaga, dan rencana tahunan. Contoh yang dipakai di seluruh kartu: ICU membuka 2 bed baru, Kepala ICU meminta +2 Perawat Pelaksana, satu kursi diisi pegawai baru dan satu lewat SK pindahan |
| Kontrak terkunci | API v7 bagian 13 dan Frontend Architecture rev 3 bagian 12; hash pada metadata. Layar **tidak** mengarang endpoint, kolom, atau status di luar kontrak |
| Persetujuan | Tidak ada layar persetujuan baru. Tugas `T9` s.d. `T13` muncul di Kotak Masuk Persetujuan (`FE-HRD-30`/`FE-HRD-31`, task `FE-HRD-008` ✅); layar detail dokumen `S-D1` dibuka dari tautan tugas itu |
| Letak berkas | Route `src/app/hr/workforce-planning/...`; view `src/components/view/hr/workforce-planning/...`; service di `src/lib/services/hr/`; konstanta endpoint di `src/lib/constants/hr/`; util tampilan di `src/utils/hr/workforce-planning/`. Nama berkas persisnya mengikuti tetangga terdekat (`DEV_DISCRETION`) |
| Pola layar | Daftar mengikuti Pola A (`03-frontend-architecture.md` bagian 4.1); layar dokumen bertahap mengikuti pola SK (bagian 4.2a). Pemakaian ulang atau pembuatan komponen baru diputuskan per elemen lewat `base-component-decision-gate` saat eksekusi |
| Keadaan yang mengikat (bagian 12.5) | Nilai `null` dari backend **MUST** tampil "data belum tersedia" dan **MUST NOT** diubah menjadi 0 (`UNKNOWN_IS_NOT_ZERO`). `409` → "Dokumen ini sudah diubah orang lain. Muat ulang sebelum menyimpan." tanpa membuang isian. Tombol Ajukan, Tautkan, dan Batalkan kursi nonaktif selama permintaan berjalan. Angka biaya selalu berlabel "estimasi" dan **MUST NOT** disimpan di `localStorage`, `sessionStorage`, maupun state yang dipersistenkan |
| Hak akses | Tombol dan kolom mengikuti butir hak akses `permission-audit-matrix.md` bagian 11. Menyembunyikan tombol hanyalah kenyamanan; penjaga sebenarnya di backend |
| Bukti verifikasi frontend | Mengikuti `rules/frontend/test-policy.md`: `npm run lint` (atau `npm run lint:errors`) tanpa error baru, `npm run build` berhasil, verifikasi manual setiap kontrol interaktif di peramban, dan `npm run test:unit` untuk suite yang ada. Test unit baru **direkomendasikan** untuk logika murni (format "data belum tersedia", label biaya, hitung tampilan sisa kursi), tidak diwajibkan |
| Data uji | Uji manual butuh backend `S-D1` yang berjalan dan data dev. Alur persetujuan sungguhan baru dapat diuji setelah `BE-HRD-050` (tertahan `HRD-DEC-032`); sebelum itu, layar dokumen diuji pada status yang dapat dibuat tanpa persetujuan (`Draft`, `Submitted`, `Returned`) |

### `FE-HRD-029` — Fondasi layar `S-D1` dan layar Pengaturan Perencanaan
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-031` [BE].
* **Outcome**: Menu HR punya grup baru "Perencanaan Tenaga Kerja", dan HR Manager dapat membuka layar Pengaturan Perencanaan untuk mengisi batas hari cuti panjang dan batas umur permintaan. Butir menu lama "Kebutuhan Tenaga Kerja" — yang sebenarnya membuka master persyaratan berkas — berganti nama agar tidak lagi bermakna sama.
* **Requirement/decision**: `03-frontend-architecture.md` bagian 12.1 (`FE-HRD-109`), 12.2, 12.3.5, 12.5; `HRD-TF-036`; `HRD-FE-04` (`OPEN`, usulan kerja); `HRD-DEC-061`, `HRD-DEC-071`; `VR-WFP-40`, `VR-WFP-41`.
* **Kontrak**: API v7 bagian 13 grup `Setting`; Frontend Architecture rev 3 bagian 12.2.
* **Kewenangan UI**: Nama grup dan butir memakai usulan kerja (`HRD-FE-04` `OPEN`); urutan dan ikon `DEV_DISCRETION`.
* **Reuse**: `src/utils/menu-sidebar/menu-items.jsx` (grup tingkat 1 di bawah "Sumber Daya Manusia" memakai field `subItems`); pola form HR yang sudah ada.
* **Cakupan**:
  1. Grup menu "Perencanaan Tenaga Kerja" dengan butir pertamanya, "Pengaturan Perencanaan". Butir lain ditambahkan oleh task layar masing-masing, agar tidak ada tautan mati.
  2. Nama butir lama `corporateWorkforceRequirement` (`menu-items.jsx` sekitar baris 252) beserta label pada view detail dan form-nya menjadi "Persyaratan Berkas Tenaga Kerja"; `pathname` tidak berubah.
  3. Konstanta endpoint untuk kesembilan base URL `api/v1/corporate/human-resource/workforce-planning/...`.
  4. Util tampilan bersama: nilai `null` → "data belum tersedia"; porsi kerja berformat koma (0,5); angka biaya berlabel "estimasi" atau kalimat "Biaya tidak ditampilkan untuk peran Anda." bila `costVisible = false`.
  5. Route, view, dan service layar Pengaturan Perencanaan (`/hr/workforce-planning/settings`).
* **Dependency**: `FE-HRD-021` ✅, `BE-HRD-031` [BE]
* **Acceptance criteria**:
  1. Grup "Perencanaan Tenaga Kerja" tampil di bawah "Sumber Daya Manusia" dan butir "Pengaturan Perencanaan" hanya terlihat oleh pemegang `WorkforcePlanningSetting : Read`.
  2. Tidak ada lagi dua butir menu bermakna "kebutuhan tenaga"; butir lama tetap membuka layar yang sama.
  3. Layar pengaturan menampilkan dua isian angka dan alasan. Nilai yang belum diisi tampil kosong dengan keterangan "belum diatur", bukan angka bawaan.
  4. Tombol Simpan hanya untuk pemegang `WorkforcePlanningSetting : Update`; pesan `VR-WFP-40` dan `VR-WFP-41` dari backend tampil di dekat isiannya; `409` ditangani sesuai catatan umum.
  5. Util "data belum tersedia" dan label biaya dipakai ulang oleh task `S-D1` berikutnya, bukan disalin.
* **Bukti verifikasi**: lint dan build; uji manual menu dan layar pengaturan dengan dua akun (punya dan tidak punya hak `Update`); test unit util tampilan direkomendasikan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: nama menu berganti lagi saat `HRD-FE-04` diputuskan — label dipusatkan agar cukup diubah di satu tempat.
* **Definition of Done**: Grup menu, util bersama, dan layar pengaturan aktif; lint dan build lulus; laporan tracked ada.

### `FE-HRD-030` — Layar Formasi Jabatan baca saja beserta riwayat versi
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-032` [BE].
* **Outcome**: HR, Kepala Unit, dan Direktur melihat berapa kursi resmi setiap unit × jabatan pada tanggal tertentu dan dokumen yang melahirkannya. Klik satu baris membuka riwayat versinya. Tidak ada tombol tambah, ubah, atau hapus — formasi hanya berubah lewat dokumen yang disetujui.
* **Requirement/decision**: `HRD-DEC-062`, `FR-HRD-098` s.d. `FR-HRD-100`, `AT-HRD-D1-06` (bagian layar), `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-101`).
* **Kontrak**: API v7 bagian 13 grup `Position Formation` (`GET /`, `GET /history`).
* **Kewenangan UI**: Ketiadaan tombol ubah **mengikat** (`HRD-DEC-062`); tampilan riwayat sebagai laci, dialog, atau halaman anak `DEV_DISCRETION`.
* **Reuse**: Pola A; util `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/position-formations`, view, service; butir menu "Formasi Jabatan".
* **Dependency**: `FE-HRD-029`, `BE-HRD-032` [BE]
* **Acceptance criteria**:
  1. Kolom: unit, jabatan, formasi, berlaku mulai, dokumen sumber; saringan rumah sakit, unit, jabatan, dan tanggal.
  2. Riwayat versi menampilkan seluruh versi beserta jenis dan nomor dokumen sumber, penyetuju, dan waktu persetujuan.
  3. Layar tidak punya tombol tambah, ubah, maupun hapus.
  4. Keadaan kosong: "Belum ada formasi. Isi lewat Batch Formasi Awal."; memuat dan gagal mengikuti Pola A.
  5. Butir menu hanya terlihat oleh pemegang `WorkforcePositionFormation : Read`.
* **Bukti verifikasi**: lint dan build; uji manual; tangkapan layar tanpa tombol ubah untuk `AT-HRD-D1-06`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar baca aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-031` — Layar Batch Formasi Awal dan detailnya
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-033` [BE].
* **Outcome**: HR Admin mengunduh templat, mengisi baris formasi dari SK formasi resmi, memeriksa baris yang belum valid, mengunggah scan SK, lalu mengajukan. Tombol Ajukan baru aktif bila semua baris valid dan scan sudah ada.
* **Requirement/decision**: `HRD-DEC-070`, `FR-HRD-101`, `VR-WFP-11` s.d. `VR-WFP-14`, `UAT-31`, `UAT-32`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-102`, `FE-HRD-103`).
* **Kontrak**: API v7 bagian 13 grup `Initial Formation Batch` (10 endpoint).
* **Kewenangan UI**: Susunan detail batch `DEV_DISCRETION`; tombol Ajukan nonaktif selama ada baris tidak valid atau scan belum ada **mengikat**.
* **Reuse**: Pola A untuk daftar; pola unggah berkas `FE-HRD-024` (PDF/JPG/PNG, paling besar 10 MB); pola tabel validasi baris `FE-HRD-026`.
* **Cakupan**: Route `/hr/workforce-planning/initial-formation-batches` dan `/[id]`, view, service; butir menu "Batch Formasi Awal".
* **Dependency**: `FE-HRD-029`, `BE-HRD-033` [BE]
* **Acceptance criteria**:
  1. Daftar: nomor batch, dokumen sumber, status, jumlah baris valid/tidak valid; kosong → "Belum ada batch formasi awal."
  2. Detail menampilkan status per baris beserta pesannya, termasuk `VR-WFP-11` untuk unit × jabatan yang sudah berformasi.
  3. Unggah scan memeriksa jenis dan ukuran berkas di layar dan menampilkan pesan `VR-WFP-14` dari backend.
  4. Tombol Ajukan nonaktif selama `InvalidRows > 0` atau scan belum ada, dengan keterangan penyebabnya (`UAT-32`).
  5. Simpan, Ajukan, dan Batalkan hanya tampil pada `Draft`/`Returned` untuk pemegang butir yang sesuai; batch `Applied` hanya baca.
  6. Riwayat persetujuan tampil dari detail instance workflow yang sudah ada.
* **Bukti verifikasi**: lint dan build; uji manual unggah templat, validasi, unggah scan, dan keadaan tombol Ajukan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: pengguna mengira batch langsung berlaku — status `Submitted` diberi keterangan "menunggu persetujuan".
* **Definition of Done**: Dua layar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-032` — Layar Angka Beban
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-034` [BE].
* **Outcome**: Kepala Unit atau HR mengisi dan memperbaiki angka beban langsung di layar daftar, misalnya "7 pasien ICU terisi per hari, Januari–Maret 2027, sumber: rekap sensus harian".
* **Requirement/decision**: `HRD-DEC-060`, `FR-HRD-096`, `VR-WFP-08`, `VR-WFP-09`, `VR-WFP-38`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-100`).
* **Kontrak**: API v7 bagian 13 grup `Staffing Workload`.
* **Kewenangan UI**: Isian langsung di layar ini (bagian 12.3.5); bentuk isian (baris tabel atau dialog) `DEV_DISCRETION`.
* **Reuse**: Pola A; util `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/staffing-workloads`, view, service; butir menu "Angka Beban".
* **Dependency**: `FE-HRD-029`, `BE-HRD-034` [BE]
* **Acceptance criteria**:
  1. Kolom: unit, jabatan, periode, angka, sumber; kosong → "Belum ada angka beban untuk periode ini."
  2. Tambah dan ubah hanya untuk pemegang `WorkforceStaffingWorkload : Create`/`: Update`.
  3. Pesan `VR-WFP-08` (sumber kosong) dan `VR-WFP-09` (isian ganda) tampil di dekat isiannya.
  4. Kepala Unit hanya melihat unitnya.
* **Bukti verifikasi**: lint dan build; uji manual tambah, ubah, dan isian ganda.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-033` — Layar Daftar Standar Kebutuhan Tenaga
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-035` [BE].
* **Outcome**: HR dan Kepala Unit melihat seluruh versi standar per unit beserta statusnya, dan dapat membuat versi baru.
* **Requirement/decision**: `FR-HRD-092`, `VR-WFP-01`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-98`).
* **Kontrak**: API v7 bagian 13 grup `Staffing Standard` (`GET /`, `POST /`, `GET /filters/metadata`).
* **Kewenangan UI**: Pola A.
* **Reuse**: Pola A; util `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/staffing-standards`, view, service; butir menu "Standar Kebutuhan Tenaga".
* **Dependency**: `FE-HRD-029`, `BE-HRD-035` [BE]
* **Acceptance criteria**:
  1. Kolom: unit, versi, status, berlaku mulai, berlaku sampai; kosong → "Belum ada standar untuk saringan ini."
  2. Tombol "Buat versi baru" hanya untuk pemegang `WorkforceStaffingStandard : Create`; pesan `VR-WFP-01` tampil bila unit masih punya draf.
  3. Klik baris membuka layar penyusun dan detail standar (route `[id]`, dibangun di `FE-HRD-034`).
* **Bukti verifikasi**: lint dan build; uji manual.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar daftar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-034` — Layar Penyusun dan Detail Standar beserta rekomendasi
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan.
* **Outcome**: Penyusun standar mengisi baris jabatan menurut metodenya, memilih sendiri faktor cadangan dan cara pembulatan, mengisi skill mix per shift, dan melihat angka rekomendasi beserta rinciannya. Contoh rincian yang tampil: "Perawat Pelaksana: 7 ÷ 1 × 1 = 7 per shift; 7 × 3 = 21; 21 × 1,5 = 31,5 → 32".
* **Requirement/decision**: `HRD-DEC-060`, `FR-HRD-093` s.d. `FR-HRD-095`, `VR-WFP-02` s.d. `VR-WFP-07`, `UAT-29`, `UAT-30`, `03-frontend-architecture.md` bagian 12.3.2 (`FE-HRD-99`).
* **Kontrak**: API v7 bagian 13 grup `Staffing Standard` (`GET /{id}`, `PUT /{id}`, `GET /{id}/recommendation`, `POST /{id}/submit`, `POST /{id}/cancel`, `GET /filters/metadata`).
* **Kewenangan UI**: Satu layar berbagian atau berlangkah `DEV_DISCRETION`; metode Analisis Beban Kerja **tidak** muncul sebagai pilihan (mengikat).
* **Reuse**: Pola layar dokumen bertahap SK (`FE-HRD-024`); riwayat persetujuan dari detail instance workflow.
* **Cakupan**: Route `/hr/workforce-planning/staffing-standards/[id]`, view, service.
* **Dependency**: `FE-HRD-032`, `FE-HRD-033`
* **Acceptance criteria**:
  1. Kepala: nomor, unit, versi, status, tanggal berlaku, versi yang digantikan.
  2. Isian per baris menyesuaikan metode; kolom yang tidak dipakai metode itu disembunyikan. Pilihan metode diambil dari `filters/metadata` dan tidak memuat Analisis Beban Kerja.
  3. Faktor cadangan dan pembulatan tidak punya nilai awal di layar; pesan `VR-WFP-03`/`VR-WFP-04` tampil per baris.
  4. Panel rekomendasi menampilkan angka dan rincian teks dari backend; bila angka beban belum diisi tampil "angka beban belum diisi" beserta tautan ke layar Angka Beban (`FE-HRD-032`).
  5. Simpan, Ajukan, Batalkan hanya pada `Draft`/`Returned` sesuai butir hak; versi `Active`, `Scheduled`, dan `Superseded` hanya baca.
  6. Dibuka dari tautan tugas kotak masuk, layar menampilkan dokumen yang sama.
* **Bukti verifikasi**: lint dan build; uji manual dengan data `UAT-29` dan `UAT-30`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: rincian rekomendasi dihitung ulang di layar dan berbeda dari backend — layar **hanya** menampilkan rincian dari backend.
* **Definition of Done**: Penyusun dan detail standar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-035` — Layar Pemantauan Gap Tenaga — gap langsung
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-037` [BE].
* **Outcome**: HR dan Kepala Unit melihat lapisan tenaga dan tiga gap per unit × jabatan pada tanggal acuan. Sel yang datanya belum ada tertulis miring "data belum tersedia", dan baris yang belum lengkap diberi penanda. Contoh `UAT-33`: Budi paruh waktu terhitung 0,5, Sari yang Plt tidak dihitung, kolom cuti panjang "data belum tersedia".
* **Requirement/decision**: `HRD-DEC-061` (`UNKNOWN_IS_NOT_ZERO`), `FR-HRD-102` s.d. `FR-HRD-104`, `AT-HRD-D1-05b` (bagian layar), `UAT-33`, `03-frontend-architecture.md` bagian 12.3.1 (`FE-HRD-97`).
* **Kontrak**: API v7 bagian 13 grup `Staffing Gap` (`GET /`).
* **Kewenangan UI**: Penampilan `null` sebagai "data belum tersedia" **mengikat**; gaya miring dan penanda baris `DEV_DISCRETION`.
* **Reuse**: Pola A; util `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/staffing-gaps`, view, service; butir menu "Pemantauan Gap Tenaga". Tombol "Ajukan" ditambahkan di `FE-HRD-037`; panel potret di `FE-HRD-040`.
* **Dependency**: `FE-HRD-029`, `BE-HRD-037` [BE]
* **Acceptance criteria**:
  1. Saringan rumah sakit, unit, dan tanggal acuan.
  2. Kolom: formasi, terisi, akan kosong, cuti panjang, belum kompeten, gap permanen, gap sementara, gap kompetensi, rangkap, alih daya.
  3. Setiap nilai `null` tampil "data belum tersedia", tidak pernah 0; baris dengan penanda kelengkapan `false` diberi penanda "data belum lengkap".
  4. Kosong → "Belum ada formasi untuk saringan ini. Isi formasi awal lebih dulu."; gagal → "Data gap gagal dimuat." beserta Coba lagi.
  5. Kepala Unit hanya melihat unitnya.
* **Bukti verifikasi**: lint dan build; uji manual dengan pengaturan cuti panjang kosong; tangkapan layar untuk `AT-HRD-D1-05b`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: komponen tabel bawaan menampilkan `null` sebagai 0 — diperiksa khusus pada uji manual.
* **Definition of Done**: Layar gap aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-036` — Layar Daftar Permintaan Headcount
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-040` [BE].
* **Outcome**: HR dan Kepala Unit melihat seluruh permintaan beserta jenis, status, dan jumlah kursi terisi, dan dapat menyaring permintaan yang "lewat batas" atau "melebihi formasi".
* **Requirement/decision**: `FR-HRD-115`, `FR-HRD-121`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-106`).
* **Kontrak**: API v7 bagian 13 grup `Headcount Request` (`GET /`, `GET /filters/metadata`).
* **Kewenangan UI**: Pola A.
* **Reuse**: Pola A; util `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/headcount-requests`, view, service; butir menu "Permintaan Headcount".
* **Dependency**: `FE-HRD-029`, `BE-HRD-040` [BE]
* **Acceptance criteria**:
  1. Kolom: nomor, unit, jabatan, jenis, status, kursi terisi/total; kosong → "Belum ada permintaan pada saringan ini."
  2. Saringan status, unit, jabatan, "lewat batas", dan "melebihi formasi".
  3. Tombol buat permintaan hanya untuk pemegang `WorkforceHeadcountRequest : Create`.
* **Bukti verifikasi**: lint dan build; uji manual.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar daftar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-037` — Form dan Detail Permintaan Headcount, serta tombol "Ajukan" di Pemantauan Gap
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan.
* **Outcome**: Kepala Unit menyusun permintaan per kursi — porsi kerja, sumber pemenuhan, orang yang digantikan — sambil melihat sisa kursi langsung beserta rinciannya, lalu mengajukan atau menarik kembali. Penyetuju melihat potret saat diajukan berdampingan dengan angka sekarang ("saat diajukan 3,5 — sekarang 2,5"). Finance pada langkahnya dapat mengoreksi biaya.
* **Requirement/decision**: `HRD-DEC-063`, `HRD-DEC-065`, `HRD-DEC-067`, `HRD-DEC-069` (`SUBMITTED_VS_CURRENT`); `FR-HRD-110`, `FR-HRD-113`, `FR-HRD-115`; `VR-WFP-20`, `VR-WFP-22`, `VR-WFP-24` s.d. `VR-WFP-28`, `VR-WFP-34`, `VR-WFP-37`; `UAT-34`, `UAT-37`, `UAT-38`; `AT-HRD-D1-16` (bagian layar); `03-frontend-architecture.md` bagian 12.3.4 (`FE-HRD-107`) dan 12.3.1 (tombol Ajukan).
* **Kontrak**: API v7 bagian 13 grup `Headcount Request` (`GET /{id}`, `GET /remaining-seats`, `POST /`, `PUT /{id}`, `POST /{id}/submit`, `POST /{id}/withdraw`, `POST /{id}/finance-adjustment`); `GET .../staffing-gaps` untuk angka sekarang.
* **Kewenangan UI**: Satu layar untuk form dan detail mengikuti bagian 12.3.4; susunan wilayahnya `DEV_DISCRETION`.
* **Reuse**: Pola layar dokumen bertahap; util biaya dan "data belum tersedia" `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/headcount-requests/[id]` (termasuk mode buat), view, service; tombol "Ajukan" pada setiap baris layar Pemantauan Gap yang membuka form berisi unit dan jabatan baris itu.
* **Dependency**: `FE-HRD-035`, `FE-HRD-036`
* **Acceptance criteria**:
  1. Kepala menampilkan nomor, unit, jabatan, jenis, status, penanda "melebihi formasi" dan "lewat batas".
  2. Panel sisa kursi menampilkan angka beserta rinciannya, misalnya "0,5 (formasi 30 − terisi 28,5 − permintaan lain 1)"; unit tanpa formasi menampilkan "Unit ini belum punya formasi; permintaan diproses sebagai di luar formasi."
  3. Isian kursi: porsi kerja 0,10–1,00, sumber luar/internal, dan orang yang digantikan untuk jenis pengganti; isian sementara meminta tanggal akhir dan orang yang digantikan sementara. Pesan `VR-WFP-22` (beserta angka sisa kursi), `VR-WFP-24` s.d. `VR-WFP-28` tampil di dekat isiannya (`UAT-38`).
  4. Ajukan dan Tarik kembali tampil sesuai butir hak dan status; sesudah mengajukan, sisa kursi dan gap dimuat ulang; `VR-WFP-34` tampil bila permintaan sudah siap direkrut.
  5. Potret saat diajukan dan angka sekarang tampil berdampingan pada dokumen yang sudah diajukan (`UAT-34`).
  6. Kolom biaya selalu berlabel "estimasi"; tanpa `WorkforcePlanningCost : Read` tampil "Biaya tidak ditampilkan untuk peran Anda."; isian koreksi biaya hanya tampil bagi penyetuju langkah Finance yang aktif, dan pesan `VR-WFP-20` tampil bila ditolak.
  7. Tombol "Ajukan" di Pemantauan Gap hanya untuk pemegang `WorkforceHeadcountRequest : Create`.
* **Bukti verifikasi**: lint dan build; uji manual mengikuti `UAT-37`, `UAT-38`; test unit format rincian sisa kursi direkomendasikan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: angka biaya ikut tersimpan di state yang dipersistenkan — diperiksa pada review.
* **Definition of Done**: Form dan detail permintaan aktif sesuai kontrak, tombol Ajukan di gap aktif; laporan tracked ada.

### `FE-HRD-038` — Aksi kursi pada detail permintaan: tautkan/lepas, ubah rencana pemenuhan, batalkan kursi
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-044` [BE].
* **Outcome**: Dari detail permintaan, HR menautkan kursi ke penempatan atau SK, melepasnya selama belum berlaku, HR Manager memindah sumber kursi yang belum terisi, dan Kepala Unit atau HR mengajukan pembatalan kursi yang belum terisi.
* **Requirement/decision**: `HRD-DEC-066`, `HRD-DEC-067`, `HRD-DEC-071`; `FR-HRD-117`, `FR-HRD-119`, `FR-HRD-120`; `VR-WFP-29` s.d. `VR-WFP-33`, `VR-WFP-35`, `VR-WFP-36`; `UAT-39`, `UAT-40`; `03-frontend-architecture.md` bagian 12.3.4 wilayah Kursi dan Tombol.
* **Kontrak**: API v7 bagian 13: `POST`/`DELETE /{id}/seats/{seatId}/fulfilment-link`, `POST /{id}/fulfilment-plan`, `POST /{id}/cancellations`.
* **Kewenangan UI**: Bentuk dialog `DEV_DISCRETION`; dialog tautan memakai ulang dialog `FE-HRD-039`.
* **Reuse**: Dialog tautan pemenuhan dari `FE-HRD-039`.
* **Cakupan**: Aksi per kursi dan riwayat (perubahan rencana pemenuhan, pembatalan, persetujuan) pada layar `FE-HRD-107`.
* **Dependency**: `FE-HRD-037`, `FE-HRD-039`, `BE-HRD-044` [BE]
* **Acceptance criteria**:
  1. Tautkan dan Lepas per kursi hanya untuk pemegang `WorkforceHeadcountRequest : LinkFulfilment`; pesan `VR-WFP-29` s.d. `VR-WFP-32` tampil apa adanya.
  2. Ubah rencana pemenuhan hanya untuk pemegang `: ChangeFulfilmentPlan`, meminta alasan, dan menampilkan `VR-WFP-33` bila ditolak.
  3. Batalkan kursi hanya untuk pemegang `: RequestCancellation`, untuk kursi `Open`/`Frozen`, meminta alasan; pesan `VR-WFP-35` dan `VR-WFP-36` tampil apa adanya.
  4. Kursi `Frozen` diberi keterangan "dibekukan sesudah addendum".
  5. Tombol aksi nonaktif selama permintaan berjalan; sesudah aksi, detail dimuat ulang.
  6. Riwayat perubahan rencana pemenuhan dan pembatalan tampil berurutan waktu.
* **Bukti verifikasi**: lint dan build; uji manual mengikuti `UAT-39` sejauh data dev memungkinkan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: klik ganda menautkan dua kali — tombol nonaktif selama permintaan berjalan.
* **Definition of Done**: Aksi kursi aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-039` — Layar Antrean Rekrutmen beserta dialog tautan pemenuhan
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-042` dan `BE-HRD-043` [BE].
* **Outcome**: Recruiter melihat kursi rekrut luar yang siap diisi — tanpa angka biaya — dan menautkan penempatan pegawai baru langsung dari antrean, karena Recruiter tidak punya hak membuka detail permintaan.
* **Requirement/decision**: `HRD-DEC-066` (`MANUAL_LINK_UNTIL_RECRUITMENT_EXISTS`), `FR-HRD-118`, `AT-HRD-D1-11` (Recruiter tanpa biaya), `AT-HRD-D1-14`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-108`) dan 12.4.
* **Kontrak**: API v7 bagian 13 grup `Recruitment Queue` dan `POST /{id}/seats/{seatId}/fulfilment-link`.
* **Kewenangan UI**: Ketiadaan kolom biaya **mengikat**; bentuk dialog tautan `DEV_DISCRETION`.
* **Reuse**: Pola A; pencarian penempatan memakai endpoint Workforce Core yang sudah ada.
* **Cakupan**: Route `/hr/workforce-planning/recruitment-queue`, view, service; dialog tautan pemenuhan yang dapat dipakai ulang `FE-HRD-038`; butir menu "Antrean Rekrutmen".
* **Dependency**: `FE-HRD-029`, `BE-HRD-042` [BE], `BE-HRD-043` [BE]
* **Acceptance criteria**:
  1. Kolom: nomor permintaan, unit, jabatan, kursi, tanggal dibutuhkan; **tanpa** kolom biaya; kosong → "Tidak ada kursi yang menunggu direkrut."
  2. Butir menu dan layar cukup dengan `WorkforceRecruitmentQueue : Read`.
  3. Tautkan hanya untuk pemegang `WorkforceHeadcountRequest : LinkFulfilment`; sesudah berhasil, antrean dimuat ulang.
  4. Pesan `VR-WFP-29` s.d. `VR-WFP-31` dari backend tampil apa adanya.
* **Bukti verifikasi**: lint dan build; uji manual dengan akun yang hanya memegang hak antrean dan tautan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: komponen tabel umum ikut menampilkan kolom biaya — kolom tidak diminta sama sekali.
* **Definition of Done**: Antrean dan dialog tautan aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-040` — Panel potret gap pada Pemantauan Gap Tenaga
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-039` [BE].
* **Outcome**: Dari Pemantauan Gap, pengguna membuka daftar potret — pengajuan, bulanan, dan susulan — dan detail setiap potret. Bulan tanpa potret tampil "potret tidak tersedia", dan potret susulan berlabel "direkonstruksi".
* **Requirement/decision**: `HRD-DEC-069`, `FR-HRD-105`, `AT-HRD-D1-16b` (bagian layar), `03-frontend-architecture.md` bagian 12.3.1 wilayah Potret bulanan.
* **Kontrak**: API v7 bagian 13 grup `Staffing Gap` (`GET /snapshots`, `GET /snapshots/{id}`).
* **Kewenangan UI**: Bentuk panel `DEV_DISCRETION`; potret tanpa tombol ubah maupun hapus **mengikat**.
* **Reuse**: Tabel gap `FE-HRD-035` untuk menampilkan baris potret.
* **Cakupan**: Tombol "Lihat potret bulanan", daftar, dan detail potret pada layar `FE-HRD-97`.
* **Dependency**: `FE-HRD-035`, `BE-HRD-039` [BE]
* **Acceptance criteria**:
  1. Daftar potret menampilkan jenis, tanggal acuan, dan bulan; bulan yang tidak punya potret tampil "potret tidak tersedia".
  2. Potret `Reconstructed` berlabel "direkonstruksi".
  3. Detail potret memakai aturan "data belum tersedia" yang sama dengan gap langsung.
  4. Tidak ada tombol ubah maupun hapus potret.
* **Bukti verifikasi**: lint dan build; uji manual dengan potret pengajuan yang dibuat `BE-HRD-040`.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Panel potret aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-041` — Layar Daftar Rencana Tahunan dan Addendum
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: Belum dikerjakan. Menunggu `BE-HRD-046` [BE].
* **Outcome**: HR, Finance, dan Direktur melihat rencana tahunan beserta addendumnya, statusnya, dan total estimasi biaya bagi yang berhak.
* **Requirement/decision**: `FR-HRD-106`, `FR-HRD-111`, `VR-WFP-15`, `03-frontend-architecture.md` bagian 12.3.5 (`FE-HRD-104`).
* **Kontrak**: API v7 bagian 13 grup `Manpower Plan` (`GET /`, `POST /`, `GET /filters/metadata`).
* **Kewenangan UI**: Pola A.
* **Reuse**: Pola A; util biaya `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/manpower-plans`, view, service; butir menu "Rencana Tahunan".
* **Dependency**: `FE-HRD-029`, `BE-HRD-046` [BE]
* **Acceptance criteria**:
  1. Kolom: nomor, jenis (rencana/addendum), tahun, status, total estimasi; kosong → "Belum ada rencana untuk tahun ini."
  2. Kolom total biaya hanya bagi pemegang `WorkforcePlanningCost : Read` dan berlabel "estimasi".
  3. Tombol buat rencana hanya untuk pemegang `WorkforceManpowerPlan : Create`; pesan `VR-WFP-15` tampil apa adanya.
* **Bukti verifikasi**: lint dan build; uji manual dengan akun bertanda dan tanpa hak biaya.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko kecil.
* **Definition of Done**: Layar daftar aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-042` — Layar Penyusun dan Detail Rencana
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: Belum dikerjakan.
* **Outcome**: HR Admin menyusun rencana per baris unit × jabatan, menekan "Hitung ulang" untuk mengisi formasi saat ini, rekomendasi, dan biaya, menulis alasan bila usulan berbeda dari rekomendasi, lalu mengajukan. Rencana yang sudah disetujui hanya baca dan menawarkan "Buat addendum". Finance pada langkahnya mengisi koreksi biaya beserta alasan; angka awal tetap terlihat di sampingnya.
* **Requirement/decision**: `HRD-DEC-062`, `HRD-DEC-065`, `HRD-DEC-068`, `HRD-DEC-069`; `FR-HRD-107` s.d. `FR-HRD-111`; `VR-WFP-16` s.d. `VR-WFP-21`; `UAT-35`, `UAT-36`; `03-frontend-architecture.md` bagian 12.3.3 (`FE-HRD-105`).
* **Kontrak**: API v7 bagian 13 grup `Manpower Plan` (`GET /{id}`, `PUT /{id}`, `POST /{id}/addenda`, `POST /{id}/recalculate`, `POST /{id}/submit`, `POST /{id}/cancel`, `POST /{id}/finance-adjustment`, `GET /{id}/gap-snapshots`); `GET .../staffing-gaps` untuk angka sekarang.
* **Kewenangan UI**: Satu layar berbagian atau berlangkah `DEV_DISCRETION`.
* **Reuse**: Pola layar dokumen bertahap; panel potret dan angka sekarang seperti `FE-HRD-037`; util biaya `FE-HRD-029`.
* **Cakupan**: Route `/hr/workforce-planning/manpower-plans/[id]`, view, service.
* **Dependency**: `FE-HRD-041`
* **Acceptance criteria**:
  1. Kolom baris: unit, jabatan, grade, formasi kini, rekomendasi beserta rincian, usulan, perubahan, alasan, berlaku mulai, estimasi biaya, biaya perubahan.
  2. Kolom formasi kini, rekomendasi, dan biaya hanya diisi lewat "Hitung ulang", tidak dapat diketik.
  3. Pesan `VR-WFP-18` dan `VR-WFP-19` tampil per baris saat mengajukan.
  4. Simpan, Hitung ulang, Ajukan, Batalkan hanya pada `Draft`/`Returned`; rencana `Approved` hanya baca dengan tombol "Buat addendum" bagi pemegang `WorkforceManpowerPlan : Create`; mencoba mengubah menampilkan `VR-WFP-17` (`UAT-36`).
  5. Biaya berlabel "estimasi" dengan rincian seperti "2 × gaji tengah grade III × 9 bulan"; tanpa hak → kalimat `VR-WFP-37`; tarif tidak ada → "tarif belum tersedia".
  6. Isian koreksi biaya hanya bagi penyetuju langkah Finance yang aktif; angka awal dan koreksi tampil berdampingan beserta alasan (`UAT-35`); pesan `VR-WFP-20` dan `VR-WFP-21` dari backend tampil apa adanya.
  7. Potret saat diajukan tampil berdampingan dengan angka sekarang.
* **Bukti verifikasi**: lint dan build; uji manual mengikuti `UAT-35` dan `UAT-36` sejauh data dev memungkinkan.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: angka biaya bocor ke pengguna tanpa hak lewat tooltip atau ekspor — tidak ada jalur tampilan biaya selain kolom yang dijaga `costVisible`.
* **Definition of Done**: Penyusun dan detail rencana aktif sesuai kontrak; laporan tracked ada.

### `FE-HRD-043` — Verifikasi menu, hak akses, dan keterjangkauan seluruh layar `S-D1`
* **Slice / MVP**: `S-D1` / Verifikasi
* **Status**: Belum dikerjakan.
* **Outcome**: Terbukti bahwa kesembilan butir menu `S-D1` terlihat sesuai hak akses setiap peran, ketiga belas layar dapat dicapai, butir lama tidak lagi bermakna sama, dan aturan tampilan yang mengikat dipatuhi di seluruh layar.
* **Requirement/decision**: `AT-HRD-D1-22`; bagian layar `AT-HRD-D1-05b`, `AT-HRD-D1-06`, `AT-HRD-D1-11`, `AT-HRD-D1-16`; `04-prd-to-mvp.md` bagian 21.19 butir "Seluruh layar `S-D1` dapat dicapai lewat menu"; `03-frontend-architecture.md` bagian 12.4.
* **Kontrak**: Frontend Architecture rev 3 bagian 12.2 dan 12.4; Permission v7 bagian 11.2.
* **Kewenangan UI**: Tidak menambah fitur; temuan dikembalikan ke task pemiliknya dan statusnya diturunkan.
* **Reuse**: Seluruh layar `FE-HRD-029` s.d. `FE-HRD-042`.
* **Cakupan**: Pemeriksaan manual per peran dan pencatatan tangkapan layar di laporan. Peran Finance / Anggaran, Direktur, dan Recruiter disimulasikan dengan akun uji yang diberi butir hak akses bagian 11.2; pemetaan peran produksi tetap milik Administrator (`HRD-DEC-032`).
* **Dependency**: `FE-HRD-030`, `FE-HRD-031`, `FE-HRD-034`, `FE-HRD-038`, `FE-HRD-039`, `FE-HRD-040`, `FE-HRD-042`
* **Acceptance criteria**:
  1. Untuk setiap peran pada bagian 12.4, butir menu yang terlihat sama dengan tabel butir hak akses bagian 12.2.
  2. Ketiga belas layar dapat dibuka dari menu atau dari layar induknya; layar detail dokumen juga dapat dibuka dari tautan tugas kotak masuk.
  3. Tidak ada dua butir menu bermakna "kebutuhan tenaga".
  4. Di seluruh layar, nilai `null` tampil "data belum tersedia"; Formasi Jabatan tanpa tombol ubah; Recruiter dan peran tanpa hak biaya tidak melihat angka biaya; potret tampil berdampingan dengan angka sekarang.
  5. Tidak ada angka biaya di `localStorage` maupun `sessionStorage` setelah membuka layar rencana dan permintaan.
* **Bukti verifikasi**: Tangkapan layar sidebar per peran dan per aturan di laporan; `npm run build` pada commit yang diverifikasi.
* **Risiko & pemilik**: Pemilik: Frontend HR UI Lead. Risiko: akun uji tidak mewakili peran produksi — dicatat sebagai batas bukti.
* **Definition of Done**: Kelima kriteria terbukti dengan tangkapan layar; laporan tracked ada.
