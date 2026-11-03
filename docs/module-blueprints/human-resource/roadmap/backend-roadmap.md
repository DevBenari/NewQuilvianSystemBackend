# Roadmap Delivery Backend — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 5
status: ACTIVE
status_synced_at: "2026-11-03 — tanda status task lama disalin apa adanya dari revision 2; tidak ada task yang naik atau turun status pada revision ini"
planning_updated_at: "2026-11-03 — Slice S-D1 Perencanaan Tenaga Kerja dipecah menjadi 24 task (BE-HRD-017, BE-HRD-029 s.d. BE-HRD-051) per gelombang MVP-7 s.d. MVP-10; grafik urutan dependency ditulis ulang per slice"
generated_at: "2026-10-26"
owners:
  - "Technical Owner — HR Module Lead"
approved_by:
  - "User Approved (26 Oktober 2026) — roadmap awal"
  - "Pengguna, pemilik teknis dan produk (HRD-DEC-015), 3 November 2026 — desain S-D1 yang menjadi dasar task BE-HRD-017 dan BE-HRD-029 s.d. BE-HRD-051"
blueprint_manifest_revision: 14
decision_revision: 17
decision_ids_s_d1: "HRD-DEC-058 s.d. HRD-DEC-072"
source_commits:
  backend: "26066c76e69b021f68b63b31bde2258cd9d1bb74 (branch MHamzah) — HEAD saat desain dan roadmap S-D1 ditulis"
  frontend: "66b7e5600e1707e4153ca07981ddef054856c8c9 (branch HamzahV2)"
  backend_previous: "16f6b2ad28e0e9cbd374354abe240ff3b163ace0 — dasar task BE-HRD-001 s.d. BE-HRD-028"
contract_versions:
  - "API Contract v6 — task BE-HRD-001 s.d. BE-HRD-028"
  - "API Contract v7 bagian 13 — task S-D1"
  - "State Transition Matrix v7 bagian 11 — task S-D1"
  - "Validation Matrix v7 bagian 12 — task S-D1"
  - "Permission Audit Matrix v7 bagian 11 — task S-D1"
  - "Integration Contract v7 bagian 11 — task S-D1"
  - "Data Dictionary v7 bagian 8 — task S-D1"
locked_input_hashes_s_d1:
  00-interview-decisions.md: e7696919787384e44ac5fe27ead6d0a75e8d91f4d22239cd81f036579be2fcbc
  02-backend-architecture.md: 6e97da62eb8c5ac7393827e1aa436763c34bef4bbc5786e8d54ab2f13e2919a9
  03-frontend-architecture.md: 7c2db1244ccd3fdc7ce81c3b1db6220cbfd0fee9d6cdfc70a42f18b981705bda
  04-prd-to-mvp.md: 8db8cdcd95cf70b46afa35cf3a2a8691eb78681027645aebed8625a5aea1b6ea
  data/data-dictionary.md: 814e415893808276ce3d9a85ce09ba351954dd1dd434d355aebe367ceaf7f6d8
  contracts/api-contract.md: 772901e44a9f69213e2ac63404045288bec61742eba73a816f5f43faf9a01328
  contracts/state-transition-matrix.md: 1ffaecaf13dc4d0b5aac6dd84df44d15b4b70f5cfb696c7369c0f27ef9dbf6e2
  contracts/validation-matrix.md: 27055a3971cea05d93075a527bc18acbeb911727f0fbdb3fa137a54aece5bd31
  contracts/integration-contract.md: c8d397fbae7f16108c72bdae7fbd86ff95a2f3af69162a4004f60cb92477489f
  contracts/permission-audit-matrix.md: e41eca8c8f9c53b0dd5f10a2c294892128e0c00d8eb760f67a48dcb8a5fcb182
  testing/acceptance-test-matrix.md: ee5e5bb1e29a9519f76d5995021f11cfaa8384c4888471b442d076d0ef44db9b
```

**Apa yang berubah pada revision 5.**

1. **`BE-HRD-017` tidak lagi placeholder.** Gerbang `{DEC-HRD-004}` untuk `S-D1` terpenuhi lewat
   `HRD-DEC-059`, lalu desain `S-D1` disetujui pemilik pada 3 November 2026. ID `BE-HRD-017`
   dipertahankan dan kini berisi pekerjaan nyata pertama `S-D1` (tabel, enum, migration).
   Sisanya menjadi `BE-HRD-029` s.d. `BE-HRD-051`.
2. **Grafik ditulis ulang per slice.** Grafik lama memuat 28 task dalam satu blok, garis tegaknya
   tidak lurus, dan menggambar panah dari `BE-HRD-001` ke node keputusan yang tidak pernah tercatat
   di kolom `Dependency`. Sekarang ada satu grafik ringkasan antar-slice dan delapan grafik per
   slice. Seluruh hubungan pada kolom `Dependency` lama muncul kembali sebagai garis.
3. **Satu perbaikan register.** Baris `BE-HRD-028` kehilangan tanda ✅ dan pipa penutup, padahal
   kartunya ✅ dan laporannya ada. Tandanya dikembalikan; status task tidak berubah.

**Arti tanda status pada dokumen ini:**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi (keputusan bisnis, keselamatan klinis, gap desain, atau gerbang otoritas) |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

Roadmap backend HR memuat **51 task**: 27 task lama (`BE-HRD-001` s.d. `BE-HRD-028`, tanpa
`BE-HRD-017`) dan 24 task `S-D1`. Karena jauh melewati 15 node, grafiknya dipecah: satu grafik
ringkasan antar-slice, lalu satu grafik per slice.

Cara membaca:

- `A ─> B` artinya **A harus selesai dulu, baru B boleh dimulai**. Prasyarat selalu di kiri.
- Node berlabel `[B1]` s.d. `[B8]` adalah **cermin baca-saja** dari task yang tempat aslinya di
  grafik lain pada dokumen ini. Setiap task punya tepat satu tempat asli.
- Pada Grafik B1 dan B6, node pertama blok kedua (`BE-HRD-024`, `BE-HRD-041`) adalah **sambungan**
  dari blok pertama karena barisnya tidak muat 100 karakter, bukan task kedua.
- Node dalam kurung kurawal adalah gerbang, bukan task. Artinya ada pada bagian
  [Gerbang yang masih terbuka](#gerbang-yang-masih-terbuka).

### Grafik ringkasan antar-slice

Node pada grafik ini adalah slice, bukan task.

```text
S0-A ✅ ─> S0-B ✅ ─┬─> S-A1 ✅ ─┬─> S-A2..S-A7 ✅ ─┬─> S-D1 MVP-7 ─> S-D1 MVP-8
                    │            │                  │
                    │            └──────────────────┘
                    │
                    ├─> S-B1..S-B5 ✅ ─> S-C2..S-C5 ✅
                    │
                    ├─> S-D3 ✅
                    │
                    └─> S-E ✅

S-D1 MVP-8 ─> S-D1 MVP-9 ─> S-D1 MVP-10 ─> S-D1 Verifikasi ⛔
```

Slice `S-C1`, `S-C6`, `S-D2`, `S-D4`, dan `S-D5` tidak menunggu slice lain. Penahannya adalah
gerbang keputusan pada Grafik B3. `S-D1 Verifikasi` bertanda ⛔ karena kedua task-nya menunggu
pemetaan peran `{HRD-DEC-032}`.

### Grafik B1 — Fondasi dan Administrasi Kepegawaian (`S0-A`, `S0-B`, `S-A1` s.d. `S-A7`)

```text
BE-HRD-001 ✅ ─> BE-HRD-002 ✅ ─> BE-HRD-003 ✅ ─┬─> BE-HRD-004 ✅ ─> BE-HRD-005 ✅
                                                 │
                                                 └─> BE-HRD-023 ✅ ─> BE-HRD-024 ✅

BE-HRD-024 ✅ ─┬─> BE-HRD-025 ✅
               │
               ├─> BE-HRD-026 ✅
               │
               ├─> BE-HRD-027 ✅
               │
               └─> BE-HRD-028 ✅
```

### Grafik B2 — Waktu kerja, pengembangan orang, benefit, dan ratchet (`S-B1` s.d. `S-B5`, `S-C2` s.d. `S-C5`, `S-D3`, `S-E`)

```text
BE-HRD-002 ✅ [B1] ─┬─> BE-HRD-006 ✅ ─┬─> BE-HRD-007 ✅ ─┬─> BE-HRD-010 ✅
                    │                  │                  │
                    │                  └─> BE-HRD-008 ✅ ─┘
                    │
                    ├─> BE-HRD-009 ✅ ─> BE-HRD-012 ✅ ─> BE-HRD-013 ✅
                    │
                    ├─> BE-HRD-014 ✅ ─> BE-HRD-015 ✅
                    │
                    ├─> BE-HRD-019 ✅
                    │
                    └─> BE-HRD-022 ✅
```

### Grafik B3 — Slice yang tertahan gerbang keputusan (`S-C1`, `S-C6`, `S-D2`, `S-D4`, `S-D5`)

```text
{DEC-HRD-008 ⛔} ─> BE-HRD-011 ⛔

{DEC-HRD-010 ⛔} ─> BE-HRD-016 ⛔

{DEC-HRD-004 ⛔} ─┬─> BE-HRD-018 ⛔
                  │
                  ├─> BE-HRD-020 ⛔
                  │
                  └─> BE-HRD-021
```

`{DEC-HRD-008}` = gerbang keselamatan klinis dan persetujuan Komite Medik atas SPK/RKK dokter.
`{DEC-HRD-010}` = gerbang privasi rekam kesehatan staf dan pengesahan K3RS. `{DEC-HRD-004}` =
audit isi tabel tanpa API sebelum skema diturunkan ulang. `BE-HRD-017` tidak lagi berada di bawah
`{DEC-HRD-004}`: untuk `S-D1` gerbang itu terpenuhi lewat `HRD-DEC-059`.

### Grafik B4 — `S-D1` `MVP-7`: fondasi, formasi, dan batch formasi awal

```text
BE-HRD-023 ✅ [B1] ─> BE-HRD-017 ─┬─> BE-HRD-030 ─┬─> BE-HRD-033
                                  │               │
                                  ├─> BE-HRD-032 ─┘
                                  │
                                  └─> BE-HRD-031

BE-HRD-005 ✅ [B1] ─> BE-HRD-029
```

### Grafik B5 — `S-D1` `MVP-8`: angka beban, standar kebutuhan tenaga, dan gap langsung

```text
BE-HRD-017 [B4] ─> BE-HRD-034 ─┬──┬─> BE-HRD-035 ─┬─> BE-HRD-036
                               │  │               │
BE-HRD-029 [B4] ───────────────┘  │               └──┬──┬─> BE-HRD-037
                                  │                  │  │
BE-HRD-030 [B4] ──────────────────┘                  │  │
                                                     │  │
BE-HRD-031 [B4] ─────────────────────────────────────┘  │
                                                        │
BE-HRD-032 [B4] ────────────────────────────────────────┘
```

### Grafik B6 — `S-D1` `MVP-9`: biaya, potret pengajuan, permintaan headcount, pemenuhan, dan pembatalan

```text
BE-HRD-017 [B4] ─> BE-HRD-038 ─┬──┬─> BE-HRD-040 ─> BE-HRD-041 ─> BE-HRD-042
                               │  │
BE-HRD-037 [B5] ─> BE-HRD-039 ─┘  │
                                  │
BE-HRD-030 [B4] ──────────────────┘

BE-HRD-041 ──────┬─> BE-HRD-043 ─> BE-HRD-044 ─┬─> BE-HRD-045 ⛔
                 │                             │
BE-HRD-036 [B5] ─┘                             │
                                               │
{HRD-PLAN-GAP-01 ⛔} ──────────────────────────┘
```

### Grafik B7 — `S-D1` `MVP-10`: rencana tahunan, addendum, dan potret bulanan

```text
BE-HRD-036 [B5] ─────────────────┬─> BE-HRD-048
                                 │
BE-HRD-039 [B6] ─┬───────────────┘
                 │
                 └─┬─> BE-HRD-046 ─┬─> BE-HRD-047
                   │               │
BE-HRD-038 [B6] ───┘               ├──┬─> BE-HRD-049 ⛔
                                   │  │
BE-HRD-041 [B6] ───────────────────┘  │
                                      │
{HRD-PLAN-GAP-02 ⛔} ─────────────────┘
```

### Grafik B8 — `S-D1`: data runtime dev dan verifikasi ujung-ke-ujung

```text
BE-HRD-030 [B4] ────┬─> BE-HRD-050 ⛔ ─┬──┬──┬─> BE-HRD-051 ⛔
                    │                  │  │  │
{HRD-DEC-032 ⛔} ───┘                  │  │  │
                                       │  │  │
BE-HRD-044 [B6] ───────────────────────┘  │  │
                                          │  │
BE-HRD-047 [B7] ──────────────────────────┘  │
                                             │
BE-HRD-048 [B7] ─────────────────────────────┘
```

**Legenda node gerbang `S-D1`:**

- `{HRD-PLAN-GAP-01 ⛔}` = sumber data "lowongan terbuka" untuk `VR-WFP-36` belum dirancang.
- `{HRD-PLAN-GAP-02 ⛔}` = mesin workflow belum punya kait validasi sebelum persetujuan per langkah.
- `{HRD-DEC-032 ⛔}` = pemetaan peran HR Manager, Finance / Anggaran, Direktur, dan Recruiter ke
  peran aplikasi masih `MAPPING_REQUIRED`.

**Pemeriksaan silang.** Jumlah pasangan prasyarat → task pada grafik: B1 10, B2 12, B3 5, B4 7,
B5 8, B6 12, B7 9, B8 6 — total **69**. Jumlah entri kolom `Dependency` pada register di bawah juga
**69** (27 untuk task lama, 42 untuk task `S-D1`). Grafiknya bebas siklus.

---

## Tabel Gelombang Eksekusi

Gelombang dihitung dari garis grafik saja: task tanpa prasyarat di gelombang 1, task lain di
`1 + gelombang tertinggi prasyaratnya`. Task dalam satu gelombang boleh paralel.

**Gelombang teknis bukan urutan rilis.** `04-prd-to-mvp.md` bagian 21.20 mengikat urutan **rilis
dan UAT** `MVP-7` → `MVP-8` → `MVP-9` → `MVP-10`. Gelombang di bawah hanya menunjukkan kapan sebuah
task paling cepat boleh dibangun. Contohnya `BE-HRD-034` (angka beban, `MVP-8`) boleh dibangun
bersamaan dengan `BE-HRD-030` (`MVP-7`), tetapi tetap baru dirilis dan diuji UAT bersama `MVP-8`.

| Gelombang | Boleh mulai setelah | Task Backend |
| ---: | --- | --- |
| **1** | — | `BE-HRD-001` ✅ |
| **2** | `BE-HRD-001` | `BE-HRD-002` ✅ |
| **3** | `BE-HRD-002` | `BE-HRD-003` ✅, `BE-HRD-006` ✅, `BE-HRD-009` ✅, `BE-HRD-014` ✅, `BE-HRD-019` ✅, `BE-HRD-022` ✅ |
| **4** | Prasyarat gelombang 3 masing-masing | `BE-HRD-004` ✅, `BE-HRD-007` ✅, `BE-HRD-008` ✅, `BE-HRD-012` ✅, `BE-HRD-015` ✅, `BE-HRD-023` ✅ |
| **5** | Prasyarat gelombang 4 masing-masing | `BE-HRD-005` ✅, `BE-HRD-010` ✅, `BE-HRD-013` ✅, `BE-HRD-024` ✅; **`BE-HRD-017`** (`MVP-7`, setelah `BE-HRD-023` ✅) — **boleh dikerjakan sekarang** |
| **6** | `BE-HRD-024` ✅; `BE-HRD-005` ✅; `BE-HRD-017` | `BE-HRD-025` ✅ s.d. `BE-HRD-028` ✅; **`BE-HRD-029`** (`MVP-7`, setelah `BE-HRD-005` ✅) — **boleh dikerjakan sekarang**; `BE-HRD-030`, `BE-HRD-031`, `BE-HRD-032` (`MVP-7`); `BE-HRD-034` (`MVP-8`); `BE-HRD-038` (`MVP-9`) |
| **7** | `BE-HRD-029`, `BE-HRD-030`, `BE-HRD-032`, `BE-HRD-034` | `BE-HRD-033` (`MVP-7`); `BE-HRD-035` (`MVP-8`) |
| **8** | `BE-HRD-031`, `BE-HRD-032`, `BE-HRD-035` | `BE-HRD-036`, `BE-HRD-037` (`MVP-8`) |
| **9** | `BE-HRD-037` | `BE-HRD-039` (`MVP-9`) |
| **10** | `BE-HRD-030`, `BE-HRD-036`, `BE-HRD-038`, `BE-HRD-039` | `BE-HRD-040` (`MVP-9`); `BE-HRD-046`, `BE-HRD-048` (`MVP-10`) |
| **11** | `BE-HRD-040` | `BE-HRD-041` (`MVP-9`) |
| **12** | `BE-HRD-036`, `BE-HRD-041`, `BE-HRD-046` | `BE-HRD-042`, `BE-HRD-043` (`MVP-9`); `BE-HRD-047` (`MVP-10`) |
| **13** | `BE-HRD-043` | `BE-HRD-044` (`MVP-9`) |
| **⛔** | `{HRD-PLAN-GAP-01}` dan `BE-HRD-044` | `BE-HRD-045` ⛔ (penjaga "tutup lowongan dulu") |
| **⛔** | `{HRD-PLAN-GAP-02}`, `BE-HRD-041`, `BE-HRD-046` | `BE-HRD-049` ⛔ (kait sebelum persetujuan untuk `VR-WFP-21` dan `VR-WFP-23`) |
| **⛔** | `{HRD-DEC-032}` dan `BE-HRD-030` | `BE-HRD-050` ⛔ (data runtime dev `S-D1`) |
| **⛔** | `BE-HRD-050` ⛔, `BE-HRD-044`, `BE-HRD-047`, `BE-HRD-048` | `BE-HRD-051` ⛔ (verifikasi ujung-ke-ujung `S-D1`) |
| **⛔** | `{DEC-HRD-008}` | `BE-HRD-011` ⛔ (Kredensial & SPK/RKK Klinis) |
| **⛔** | `{DEC-HRD-010}` | `BE-HRD-016` ⛔ (Kesehatan Kerja Staf & MCU K3RS) |
| **⛔** | `{DEC-HRD-004}` | `BE-HRD-018` ⛔ (Rekrutmen & Hiring), `BE-HRD-020` ⛔ (Tiket Layanan HR) |
| **DEFERRED** | Prioritas bisnis terjadwal; tetap di bawah `{DEC-HRD-004}` | `BE-HRD-021` (Perjalanan Dinas & Reimbursement) |

---

## Gerbang yang masih terbuka

| Gerbang | Apa yang belum ada | Task yang tertahan | Siapa yang menyelesaikan | Yang tetap bisa jalan |
| --- | --- | --- | --- | --- |
| `HRD-PLAN-GAP-01` | `VR-WFP-36` (`CLOSE_VACANCY_FIRST`) menolak pembatalan kursi "bila masih ada lowongan terbuka". Tetapi `data/data-dictionary.md` bagian 8 tidak punya kolom atau tabel lowongan, dan Rekrutmen (`S-D2`) masih `BLOCKED`. Decision log bagian 32 hanya menyebut "dicatat manual" tanpa bentuk datanya. Tanpa sumber data, penjaga ini tidak dapat dibangun tanpa mengarang kolom | `BE-HRD-045` | Pengguna sebagai pemilik produk dan teknis, lewat revisi desain `S-D1` (`design-business-module`). Contoh pilihan yang perlu diputuskan, **tidak** diputuskan di sini: konfirmasi manual "lowongan sudah ditutup" yang tercatat pada pengajuan pembatalan, atau menunda aturan ini sampai `S-D2` ada | Seluruh alur pembatalan `T13` lainnya di `BE-HRD-044` |
| `HRD-PLAN-GAP-02` | `WorkflowService.ApproveAsync` meng-commit langkah persetujuan lebih dulu, baru memanggil `WorkflowReferenceLifecycleService.HandleAsync` (bukti: HEAD `26066c76`, `WorkflowService.cs` sekitar baris 1137–1150). Tidak ada kait untuk menolak persetujuan **sebelum** langkahnya tersimpan. Dua aturan butuh kait itu: `VR-WFP-21` (Finance tidak boleh menyetujui bila ada baris "tarif belum tersedia" tanpa koreksi) dan `VR-WFP-23` (persetujuan terakhir `T11` ditolak `409` bila sisa kursi tidak lagi cukup). Bagian 16.4.9 hanya mengizinkan perubahan `WorkflowService` untuk penelusuran unit induk | `BE-HRD-049` | Pemilik arsitektur backend (Pengguna) lewat revisi bagian 16.4.9 | Seluruh task lain. `VR-WFP-20` (koreksi biaya hanya oleh penyetuju Finance yang aktif) tidak terkena karena bukan aksi persetujuan |
| `HRD-DEC-032` — `MAPPING_REQUIRED` | Peran HR Manager, Finance / Anggaran, Direktur, dan Recruiter belum dipetakan ke peran aplikasi. Tanpa itu, langkah `T9` s.d. `T13` tidak menemukan penyetuju dan pengajuan ditolak `400` | `BE-HRD-050`, lalu `BE-HRD-051` | Administrator | Seluruh task build `S-D1`. Pemetaan ini juga syarat **rilis** (`04-prd-to-mvp.md` bagian 21.20) |

**Wewenang yang tidak termasuk approval task mana pun.** Menjalankan migration
`AddWorkforcePlanningCore` ke database dev, dan menulis data uji ke database dev, membutuhkan
wewenang tulis database yang diberikan terpisah. Selama belum diberikan, task boleh selesai sampai
batas build dan review kode, lalu bagian uji API-nya ditandai belum terbukti (🟡) dengan menyebut
wewenang itu sebagai penghalangnya.

**Yang hanya menahan rilis, bukan pengembangan:** nilai standar klinis seperti rasio perawat ICU
(`HRD-Q-01`, pemilik kebijakan rumah sakit), nama grup dan butir menu (`HRD-FE-04`, usulan kerja
dipakai sementara), dan apakah `S-D1` ikut rilis pertama modul atau dirilis tersendiri.

---

## Register Status Task Backend

| Task ID | Slice | Ringkasan Tugas | Status | Dependency |
| --- | --- | --- | :---: | --- |
| `BE-HRD-001` | `S0-A` | Pendaftaran prefix operasional `Hrd` dan keluarga workforce `Wfp` pada registry | ✅ | — |
| `BE-HRD-002` | `S0-B` | Route canonical kebab-case dan compatibility alias pada 8 master data controller | ✅ | `BE-HRD-001` |
| `BE-HRD-003` | `S-A1` | Endpoint daftar lintas-pegawai Administrasi Kepegawaian (Workforce Core) | ✅ | `BE-HRD-002` |
| `BE-HRD-004` | `S-A2..S-A6` | Verifikasi dan pemantapan endpoint Layanan Mandiri Karyawan (ESS) | ✅ | `BE-HRD-003` |
| `BE-HRD-005` | `S-A7` | Engine SLA, eskalasi, dan query agregasi Kotak Masuk Persetujuan Terpadu | ✅ | `BE-HRD-004` |
| `BE-HRD-006` | `S-B1` | Administrasi Presensi Harian, Penutupan Periode, dan Jalur On-Behalf HR | ✅ | `BE-HRD-002` |
| `BE-HRD-007` | `S-B2` | Administrasi Cuti, Penyesuaian Saldo Bertahap, dan Rekonsiliasi Tahunan | ✅ | `BE-HRD-006` |
| `BE-HRD-008` | `S-B3` | Administrasi Lembur, Verifikasi Surat Perintah Kerja Lembur, dan Rekonsiliasi | ✅ | `BE-HRD-006` |
| `BE-HRD-009` | `S-B4` | Engine Roster Shift 24/7, Penggantian Shift Darurat, dan Penugasan Siaga | ✅ | `BE-HRD-002` |
| `BE-HRD-010` | `S-B5` | Kalkulasi Payroll HR, Pemotongan Angsuran, dan Restitusi Penggajian | ✅ | `BE-HRD-007`, `BE-HRD-008` |
| `BE-HRD-011` | `S-C1` | Kredensial Medis, SPK/RKK Dokter, dan Komite Medik | ⛔ | `{DEC-HRD-008 ⛔}` |
| `BE-HRD-012` | `S-C2` | Penugasan Pelatihan Wajib, Pelacakan Sertifikasi, dan Masa Berlaku | ✅ | `BE-HRD-009` |
| `BE-HRD-013` | `S-C3` | Siklus Evaluasi Kinerja Karyawan, Skala Penilaian KPI, dan Finalisasi | ✅ | `BE-HRD-012` |
| `BE-HRD-014` | `S-C4` | Transaksi Exit Clearance, Pengembalian Aset, dan Offboarding Karyawan | ✅ | `BE-HRD-002` |
| `BE-HRD-015` | `S-C5` | Pencatatan Kasus Hubungan Karyawan, Pembinaan Disiplin, dan Sanksi | ✅ | `BE-HRD-014` |
| `BE-HRD-016` | `S-C6` | Kesehatan dan Keselamatan Kerja Staf (K3RS) | ⛔ | `{DEC-HRD-010 ⛔}` |
| `BE-HRD-017` | `S-D1` · `MVP-7` | Fondasi data `S-D1`: 16 tabel `Hrd*`, enum, konstanta, migration `AddWorkforcePlanningCore`. **Lingkup ditetapkan 3 November 2026**; sebelumnya placeholder di bawah `{DEC-HRD-004}` | | `BE-HRD-023` ✅ |
| `BE-HRD-018` | `S-D2` | Rekrutmen, Seleksi Pelamar, dan Penawaran Kerja (Hiring) | ⛔ | `{DEC-HRD-004 ⛔}` |
| `BE-HRD-019` | `S-D3` | Pendaftaran Benefit Karyawan, Penjaminan Tanggungan, dan Mutasi Saldo | ✅ | `BE-HRD-002` |
| `BE-HRD-020` | `S-D4` | Layanan Bantuan dan Tiket Kepegawaian (HR Helpdesk) | ⛔ | `{DEC-HRD-004 ⛔}` |
| `BE-HRD-021` | `S-D5` | Pengajuan Perjalanan Dinas dan Klaim Biaya (Reimbursement) | tanpa tanda | `{DEC-HRD-004 ⛔}` |
| `BE-HRD-022` | `S-E` | Audit Ratchet Penamaan Entitas Transaksi Legacy (`Trx` ke `Hrd`) | ✅ | `BE-HRD-002` |
| `BE-HRD-023` | `S-A1` | Model, Konfigurasi EF Core, dan Migration Skema `HrdPersonnelActionOrder` & Approval Fields | ✅ | `BE-HRD-003` |
| `BE-HRD-024` | `S-A1` | Service Mutasi & Persetujuan SK Kepegawaian (`HrdPersonnelActionOrderService`) & Validasi | ✅ | `BE-HRD-023` |
| `BE-HRD-025` | `S-A1` | Controller API Manajemen Dokumen SK Kepegawaian & Unggah Pindai Berkas Fisik | ✅ | `BE-HRD-024` |
| `BE-HRD-026` | `S-A1` | Controller & Service Profil Terpadu Pegawai (Employee 360) dengan Dual-Mode Masking Keamanan | ✅ | `BE-HRD-024` |
| `BE-HRD-027` | `S-A1` | Service & Controller Inisialisasi Penempatan Baseline Pegawai Eksisting (55 Staf) | ✅ | `BE-HRD-024` |
| `BE-HRD-028` | `S-A1` | Endpoint Kuasa HR (On-Behalf) Perubahan Profil Pegawai & Penguncian Form Master Data | ✅ | `BE-HRD-024` |
| `BE-HRD-029` | `S-D1` · `MVP-7` | Penelusuran unit induk pada matriks persetujuan untuk langkah Direktur (`HRD-DEC-072`) | | `BE-HRD-005` ✅ |
| `BE-HRD-030` | `S-D1` · `MVP-7` | Lima jenis rujukan workflow dan layanan integrasi persetujuan `S-D1` | | `BE-HRD-017` |
| `BE-HRD-031` | `S-D1` · `MVP-7` | Pengaturan perencanaan per rumah sakit | | `BE-HRD-017` |
| `BE-HRD-032` | `S-D1` · `MVP-7` | Buku formasi berversi: satu-satunya penulis formasi dan API baca | | `BE-HRD-017` |
| `BE-HRD-033` | `S-D1` · `MVP-7` | Batch formasi awal sampai `T10` melahirkan formasi versi 1 | | `BE-HRD-030`, `BE-HRD-032` |
| `BE-HRD-034` | `S-D1` · `MVP-8` | Isian angka beban manual | | `BE-HRD-017` |
| `BE-HRD-035` | `S-D1` · `MVP-8` | Standar kebutuhan tenaga, rekomendasi, dan pengesahan `T9` | | `BE-HRD-029`, `BE-HRD-030`, `BE-HRD-034` |
| `BE-HRD-036` | `S-D1` · `MVP-8` | Scheduler perencanaan dan aktivasi standar pada tanggal berlaku | | `BE-HRD-035` |
| `BE-HRD-037` | `S-D1` · `MVP-8` | Gap tenaga langsung per unit × jabatan | | `BE-HRD-031`, `BE-HRD-032`, `BE-HRD-035` |
| `BE-HRD-038` | `S-D1` · `MVP-9` | Estimasi biaya dari gaji tengah grade dan penyamaran biaya | | `BE-HRD-017` |
| `BE-HRD-039` | `S-D1` · `MVP-9` | Potret gap pengajuan dan API baca potret | | `BE-HRD-037` |
| `BE-HRD-040` | `S-D1` · `MVP-9` | Permintaan headcount: draf, kursi, sisa kursi, pengajuan, tarik kembali, koreksi Finance | | `BE-HRD-030`, `BE-HRD-038`, `BE-HRD-039` |
| `BE-HRD-041` | `S-D1` · `MVP-9` | Keputusan akhir `T11`/`T12` permintaan headcount | | `BE-HRD-040` |
| `BE-HRD-042` | `S-D1` · `MVP-9` | Antrean Recruiter dan perubahan rencana pemenuhan | | `BE-HRD-041` |
| `BE-HRD-043` | `S-D1` · `MVP-9` | Tautan pemenuhan kursi dan langkah harian scheduler permintaan | | `BE-HRD-036`, `BE-HRD-041` |
| `BE-HRD-044` | `S-D1` · `MVP-9` | Pembatalan kursi `T13` | | `BE-HRD-043` |
| `BE-HRD-045` | `S-D1` · `MVP-9` | Penjaga "tutup lowongan dulu" pada pembatalan kursi (`VR-WFP-36`) | ⛔ | `BE-HRD-044`, `{HRD-PLAN-GAP-01 ⛔}` |
| `BE-HRD-046` | `S-D1` · `MVP-10` | Rencana tahunan dan addendum: draf, hitung ulang, pengajuan `T10`, koreksi Finance | | `BE-HRD-038`, `BE-HRD-039` |
| `BE-HRD-047` | `S-D1` · `MVP-10` | Keputusan akhir `T10` rencana dan addendum: formasi baru, penandaan permintaan berlebih | | `BE-HRD-041`, `BE-HRD-046` |
| `BE-HRD-048` | `S-D1` · `MVP-10` | Potret gap bulanan dan rekonstruksi | | `BE-HRD-036`, `BE-HRD-039` |
| `BE-HRD-049` | `S-D1` · `MVP-10` | Kait validasi sebelum persetujuan untuk `VR-WFP-21` dan `VR-WFP-23` | ⛔ | `BE-HRD-041`, `BE-HRD-046`, `{HRD-PLAN-GAP-02 ⛔}` |
| `BE-HRD-050` | `S-D1` · Verifikasi | Data runtime dev `S-D1`: definisi `T9` s.d. `T13`, jabatan direktur, matriks, gaji tengah grade | ⛔ | `BE-HRD-030`, `{HRD-DEC-032 ⛔}` |
| `BE-HRD-051` | `S-D1` · Verifikasi | Verifikasi ujung-ke-ujung `S-D1` terhadap `AT-HRD-D1` dan `UAT-29` s.d. `UAT-40` | ⛔ | `BE-HRD-044`, `BE-HRD-047`, `BE-HRD-048`, `BE-HRD-050` |

---

## Kartu Detail Task Backend

### `BE-HRD-001` — Pendaftaran Prefix Operasional `Hrd` dan Workforce `Wfp` pada Registry
* **Slice**: `S0-A`
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-001.md))
* **Outcome**: Dokumen `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` secara resmi memuat entri prefix `Hrd` (operasional canonical) dan `Wfp` (workforce profile HR) tanpa menandai `Wfp` sebagai legacy terhapus.
* **Acceptance Criteria**:
  1. Registry memuat baris `Corporate / SelfServices | Human Resource | BUSINESS DOMAIN | Hrd | ACTIVE`.
  2. Registry memuat baris `Corporate / SelfServices | Human Resource | BUSINESS DOMAIN | Wfp | ACTIVE`.
  3. Tidak ada entity `Trx*` yang diklaim secara borongan milik HR; pemilikan didasarkan pada domain dan bukti.
* **Definition of Done**: Berkas registry diperbarui, `Invoke-QbeConformanceCheck.ps1` lulus validasi modul.

### `BE-HRD-002` — Route Canonical Kebab-Case dan Compatibility Alias
* **Slice**: `S0-B`
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-002.md))
* **Outcome**: Delapan master data controller yang sebelumnya memakai non-hyphenated route memiliki alias kebab-case resmi tanpa merusak pemanggil lama.
* **Acceptance Criteria**:
  1. Route `work-calendars`, `work-schedules`, `shift-groups`, `shift-patterns`, `action-types`, `case-types`, `sanction-types`, `violation-types` aktif.
  2. Route lama tetap dilayani oleh action yang sama dengan header / swagger attribute deprecated.
* **Definition of Done**: `dotnet build` sukses, Swagger JSON memproyeksikan route canonical dan alias.

### `BE-HRD-003` — Endpoint Daftar Lintas-Pegawai Administrasi Kepegawaian
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-003.md))
* **Slice**: `S-A1`
* **Outcome**: HR Admin dapat menarik daftar rekapitulasi penempatan organisasi, penempatan jabatan, relasi atasan, riwayat kepegawaian, dan penetapan gaji lintas-seluruh pegawai pada satu periode kerja.
* **Acceptance Criteria**:
  1. Menyediakan endpoint paged result `GET /api/v1/corporate/human-resource/workforce-core/salary-assignments`.
  2. Menyediakan endpoint paged result untuk penempatan organisasi dan jabatan lintas pegawai.
  3. Mendukung filter berdasarkan unit kerja, status aktif, dan rentang tanggal berlaku.
* **Definition of Done**: Controller & service mengembalikan `ApiResponse<PagedResult<T>>`, authorization `[AccessAction]` terpasang.

### `BE-HRD-004` — Verifikasi dan Pemantapan Endpoint Layanan Mandiri Karyawan (ESS)
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-004.md))
* **Slice**: `S-A2..S-A6`
* **Outcome**: Endpoint pengajuan cuti, lembur, permohonan tukar shift, koreksi absensi, dan update profil mandiri diverifikasi bekerja sesuai identitas pengguna yang login (`actorWorkforceProfileId`).
* **Acceptance Criteria**:
  1. Endpoint memvalidasi bahwa permohonan hanya dibuat untuk profil pegawai yang sedang terautentikasi.
  2. Validasi kuota dan saldo cuti menolak pengajuan yang melebihi hak.
  3. Pengajuan lembur mendeteksi tumpang tindih waktu.
* **Definition of Done**: Seluruh controller ESS teruji menangani kasus normal dan boundary (saldo habis / jadwal bentrok).

### `BE-HRD-005` — Engine SLA, Eskalasi, dan Kotak Masuk Persetujuan Terpadu
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-005.md))
* **Slice**: `S-A7`
* **Outcome**: Atasan dapat menarik seluruh transaksi bawahan yang membutuhkan persetujuannya dalam satu endpoint terpadu, dan background job eskalasi mengecek batas SLA.
* **Acceptance Criteria**:
  1. `ApprovalInboxController` mengembalikan agregasi permohonan menunggu persetujuan per atasan.
  2. SLA engine mengecek `DueAt` dan memicu notifikasi peringatan.
  3. Persetujuan dan penolakan memperbarui instance workflow dan status transaksi pemilik secara atomik.
* **Definition of Done**: Service terintegrasi dengan `WorkflowManagement`, tidak ada data status yang orphan.

### `BE-HRD-006` — Administrasi Presensi Harian, Penutupan Periode, dan Jalur On-Behalf HR
* **Slice**: `S-B1`
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-006.md))
* **Outcome**: HR Admin dapat memantau presensi harian seluruh staf RS, mengunci periode presensi, dan memproses permohonan koreksi kehadiran atas nama staf (on-behalf) dengan audit trail lengkap.
* **Acceptance Criteria**:
  1. Periode kehadiran yang sudah ditutup (`Closed`) menolak pencatatan presensi retroactive tanpa pembukaan resmi.
  2. Jalur koreksi on-behalf mencatat ID HR initiator, alasan kedaruratan, dan bukti verifikasi.
* **Definition of Done**: 71 endpoint attendance diverifikasi, proteksi periode terkunci terbukti aktif.

### `BE-HRD-007` — Administrasi Cuti, Penyesuaian Saldo Bertahap, dan Rekonsiliasi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-007.md))
* **Slice**: `S-B2`
* **Outcome**: Pengelolaan saldo cuti tahunan, cuti besar, dan izin istirahat pegawai terpelihara dengan mutasi saldo yang akuntabel.
* **Acceptance Criteria**:
  1. Penyesuaian saldo manual oleh HR wajib menyertakan alasan penyesuaian dan referensi dokumen.
  2. Pembatalan cuti yang telah disetujui memulihkan saldo secara idempoten.
* **Definition of Done**: Mutasi saldo cuti tercatat di tabel log penyesuaian, audit trail tersimpan.

### `BE-HRD-008` — Administrasi Lembur dan Rekonsiliasi Penggajian
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-008.md))
* **Slice**: `S-B3`
* **Outcome**: HR memverifikasi realisasi jam lembur terhadap surat perintah kerja lembur (SPKL) sebelum diteruskan ke perhitungan payroll.
* **Acceptance Criteria**:
  1. Realisasi lembur yang belum disetujui supervisor ditandai tidak layak diperhitungkan dalam payroll run.
  2. Rekonsiliasi jam lembur menghitung faktor pengali hari kerja dan hari libur secara tepat.
* **Definition of Done**: Rekonsiliasi lembur menghasilkan status `VerifiedForPayroll`.

### `BE-HRD-009` — Penjadwalan Roster Kerja 24/7 dan Penugasan Siaga (On-Call)
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-009.md))
* **Slice**: `S-B4`
* **Outcome**: Penugasan shift harian dan jadwal jaga tenaga kesehatan per instalasi (Rawat Inap, IGD, ICU, Bedah) terkelola tanpa tabrakan jadwal.
* **Acceptance Criteria**:
  1. Validasi mendeteksi jadwal ganda pegawai pada shift yang sama atau shift berturut-turut tanpa jeda istirahat wajib.
  2. Penggantian shift mendadak (darurat) mencatat pengganti dan alasan operasional.
* **Definition of Done**: Controller roster penugasan shift terhubung ke model roster period dan unit kerja RS.

### `BE-HRD-010` — Kalkulasi Payroll HR, Pemotongan Angsuran, dan Restitusi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-010.md))
* **Slice**: `S-B5`
* **Outcome**: Proses hitung gaji kotor, potongan ketidakhadiran, potongan pinjaman/angsuran, tunjangan shift, restitusi klaim, hingga penghasilan bersih per periode penggajian.
* **Acceptance Criteria**:
  1. Perhitungan memasukkan data sah dari presensi dan lembur yang sudah berstatus final.
  2. `PayrollRestitutionController` dan `PayrollRunInstallmentDeductionController` terintegrasi pada putaran payroll.
  3. Serah terima batch payroll ke modul Finance bersifat idempoten.
* **Definition of Done**: Perhitungan payroll dapat dieksekusi, di-rollback bila ada anomali, dan dikunci sebelum serah terima.

### `BE-HRD-012` — Penugasan Pelatihan Wajib, Pelacakan Sertifikasi, dan Masa Berlaku
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-012.md))
* **Slice**: `S-C2`
* **Outcome**: Penugasan pelatihan keselamatan pasien (BHD, PPI, K3RS), pencatatan sertifikat, masa berlaku sertifikat, dan asesmen kompetensi staf rumah sakit terpantau.
* **Acceptance Criteria**:
  1. Pelatihan wajib yang belum dipenuhi terlihat per pegawai dan per unit.
  2. Sertifikat tersimpan sebagai bukti yang dapat ditelusuri.
  3. Masa berlaku sertifikat memicu peringatan sebelum kedaluwarsa.
* **Definition of Done**: Controller & model training record dan competency assessment terhubung ke profil kepegawaian.

### `BE-HRD-013` — Siklus Evaluasi Kinerja Karyawan, Skala Penilaian KPI, dan Finalisasi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-013.md))
* **Slice**: `S-C3`
* **Outcome**: Siklus evaluasi berkala KPI staf berjalan transparan, 1 review aktif per siklus, dan privasi hasil terjaga.
* **Acceptance Criteria**:
  1. Satu pegawai tidak dapat dinilai dua kali pada siklus yang sama.
  2. Penilaian yang sudah final tidak dapat diubah diam-diam; koreksi menghasilkan revisi.
  3. Pegawai melihat hasilnya sendiri, bukan hasil orang lain.
* **Definition of Done**: Controller review kinerja dan rincian KPI terhubung ke model siklus dan profil staf.

### `BE-HRD-014` — Transaksi Exit Clearance, Pengembalian Aset, dan Offboarding
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-014.md))
* **Slice**: `S-C4`
* **Outcome**: Pengelolaan proses berhenti kerja karyawan (resign/pensiun/habis kontrak), mencakup checklist penyerahan aset unit, perpustakaan, kartu akses, dan status kliring keuangan.
* **Acceptance Criteria**:
  1. `ExitClearanceController` memvalidasi status kelulusan setiap unit verifikator (IT, GA, Farmasi, Keuangan).
  2. Penutupan exit clearance menandai profil pegawai sebagai non-aktif tanpa menghapus data riwayat.
* **Definition of Done**: Controller & service exit clearance terintegrasi, laporan verifikasi tercatat.

### `BE-HRD-015` — Pencatatan Kasus Hubungan Karyawan, Pembinaan Disiplin, dan Sanksi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-015.md))
* **Slice**: `S-C5`
* **Outcome**: Penanganan kasus pelanggaran disiplin kerja staf medis dan non-medis, penerbitan surat peringatan (SP 1, SP 2, SP 3), dan riwayat pembinaan kepegawaian tercatat aman.
* **Acceptance Criteria**:
  1. Kasus kedisiplinan hanya dapat dibaca peran yang berwenang.
  2. Setiap tindakan menyimpan alasan, pelaku, dan waktu kejadian.
  3. Kasus yang sudah ditutup tidak dapat diubah diam-diam.
* **Definition of Done**: Controller & model tindakan disiplin terhubung ke master jenis pelanggaran dan sanksi.

### `BE-HRD-019` — Pendaftaran Benefit Karyawan, Tanggungan, dan Mutasi Saldo
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-019.md))
* **Slice**: `S-D3`
* **Outcome**: Pengelolaan kepesertaan asuransi kesehatan staf/tanggungan, alokasi plafon rawat jalan/inap, dan pencatatan mutasi saldo benefit pegawai rumah sakit.
* **Acceptance Criteria**:
  1. `EmployeeBenefitEnrollmentController` dan `EmployeeBenefitDependentController` mengelola eligibilitas dan plafon.
  2. Setiap mutasi saldo benefit (klaim/top-up tahunan) tercatat pada `HrdBenefitBalanceMovement`.
  3. Validasi mencegah pendaftaran tanggungan melebihi batas ketentuan paket benefit.
* **Definition of Done**: Service benefit enrollment & balance management terverifikasi, relasi ke penjamin perusahaan pasien terhubung.

### `BE-HRD-022` — Audit Ratchet Penamaan Entitas Transaksi Legacy (`Trx` ke `Hrd`)
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-022.md))
* **Slice**: `S-E`
* **Outcome**: Penegakan aturan konstitusi rekayasa bahwa penggantian prefix `Trx` ke `Hrd` hanya terjadi saat entity mengalami mutasi skema material, mencegah rename massal destruktif.
* **Acceptance Criteria**:
  1. Ratchet penamaan hanya dipicu saat entity benar-benar disentuh skemanya (`AT-HRD-E-01`).
  2. Ratchet tidak dipicu oleh pekerjaan query atau dokumentasi (`AT-HRD-E-02`).
* **Definition of Done**: Kepatuhan audit ratchet diverifikasi penuh pada seluruh entitas HR.


### `BE-HRD-023` — Model, Konfigurasi EF Core, dan Migration Skema `HrdPersonnelActionOrder` & Approval Fields
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-023.md)
* **Outcome**: Skema database modul HR diperkaya dengan entitas agregat `HrdPersonnelActionOrder`, entitas lampiran berkas `HrdPersonnelActionOrderAttachment`, entitas batch `HrdInitialPlacementBatch` & `HrdInitialPlacementBatchRow`, serta perluasan kolom `PersonnelActionOrderId` (FK nullable) dan audit approval fields pada `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`, dan `WfpSalaryAssignment` via Migration 7 (`AddPersonnelActionOrderAndAssignmentApprovalFields`).
* **Traceability**: `HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `02-backend-architecture.md` §5.6 & §8.2 Migration 7, `data/data-dictionary.md` §2.8, §2.10.
* **Contract Version**: Data Dictionary v6, Backend Architecture rev 2.
* **Kemampuan Existing yang Dipakai**: Entitas `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`, `WfpSalaryAssignment`, `MstWorkforceProfile`.
* **Cakupan yang Diharapkan**:
  1. Definisi model entity C# di `Areas/Corporate/HumanResource/Models/` atau direktori domain HR canonical.
  2. Fluent API configuration pada DbContext mencakup konfigurasi relasi, constraint `IsUnique` untuk `OrderNumber`, dan cascade delete restrict.
  3. Migration script EF Core `AddPersonnelActionOrderAndAssignmentApprovalFields`.
* **Dependency**: `BE-HRD-003` ✅
* **Acceptance Criteria**:
  1. Entitas `HrdPersonnelActionOrder` memuat seluruh atribut wajib (OrderNumber, ActionType, WorkforceProfileId, EffectiveDate, PostingDate, Status, Reason, ReferenceDocumentNumber, Notes) sesuai kamus data.
  2. Entitas penempatan (`Wfp*Assignment`) memiliki kolom `PersonnelActionOrderId` (FK nullable) yang mereferensikan `HrdPersonnelActionOrder`.
  3. Index unik pada `OrderNumber` terdefinisi dengan benar pada EF Core metadata.
  4. Perintah migrasi database dapat dieksekusi tanpa error (`dotnet ef database update --no-build`).
* **Bukti Verifikasi**:
  - Review diff skema EF Core model snapshot.
  - Kompilasi project backend (`dotnet build`) sukses 0 error.
  - Conformance QBE (`Invoke-QbeConformanceCheck.ps1`) lulus untuk entitas ber-prefix `Hrd`.
* **Risiko & Pemilik**: Pemilik: Backend Technical Lead. Risiko: lock schema pada tabel penempatan aktif diatasi dengan nullable foreign key.
* **Definition of Done**: Migration terdaftar di snapshot EF Core, model terkompilasi 0 error, audit skema QBE lulus.

### `BE-HRD-024` — Service Mutasi & Persetujuan SK Kepegawaian (`HrdPersonnelActionOrderService`) & Validasi
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-024.md)
* **Outcome**: Layanan bisnis inti untuk siklus hidup Surat Keputusan Kepegawaian (`Draft`, `PendingApproval`, `Scheduled`, `Applied`, `Rejected`, `Cancelled`) terimplementasi lengkap dengan penegakan seluruh aturan bisnis dan validasi integritas data rumah sakit.
* **Traceability**: `HRD-DEC-031`, `HRD-DEC-048`, `HRD-DEC-049`, `HRD-DEC-050`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `FR-HRD-014`, `FR-HRD-015`, `FR-HRD-020`, `VR-WFC-01` s.d. `VR-WFC-05`, `VR-WFC-11` s.d. `VR-WFC-13`.
* **Contract Version**: State Transition Matrix v6, Validation Matrix v6.
* **Kemampuan Existing yang Dipakai**: `IWorkforceProfileService`, `IAttendanceAdministrationService` (pemeriksaan status periode presensi tertutup).
* **Cakupan yang Diharapkan**:
  1. Layanan interface `IHrdPersonnelActionOrderService` dan implementasinya di Application Layer backend.
  2. Mesin transisi status SK yang mengikat dan memvalidasi setiap fase perubahan.
  3. Integrasi pemeriksaan status periode kehadiran tertutup sebelum menyetujui mutasi berlaku surut (retroaktif).
  4. Pembuatan otomatis entitas penempatan aktif (`Wfp*Assignment`) saat status SK mencapai `Applied`.
* **Dependency**: `BE-HRD-023`
* **Acceptance Criteria**:
  1. Memvalidasi bahwa setiap pegawai hanya memiliki tepat 1 penempatan unit dan jabatan utama aktif pada satu tanggal berlaku (`VR-WFC-01`, `VR-WFC-02`).
  2. Menolak SK mutasi dengan tanggal efektif berlaku surut jika tanggal tersebut berada pada periode presensi yang sudah ditutup/closed (`VR-WFC-03`, `HRD-DEC-050`).
  3. Menolak pengajuan persetujuan SK jika belum terdapat berkas pindai fisik bertanda tangan (`VR-WFC-04`, `HRD-DEC-056`).
  4. Menolak persetujuan jika aktor penyetuju adalah pembuat draft SK (larangan self-approval, `VR-WFC-05`, `HRD-DEC-031`).
  5. Saat SK disetujui dan tanggal efektif tiba (`Applied`), sistem secara atomik memperbarui assignment aktif dan menonaktifkan assignment lama.
  6. Pembatalan SK yang sudah berstatus `Applied` hanya dapat dilakukan melalui penerbitan SK Pembatalan / Kontra resmi (`VR-WFC-12`, `HRD-DEC-057`).
* **Bukti Verifikasi**:
  - Pemeriksaan kelengkapan guard clauses pada service method.
  - Verifikasi log audit transaksi perpindahan penempatan.
  - Kompilasi project backend (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: kegagalan konsistensi atomik saat aktivasi multi-assignment diatasi dengan DbContext transaction scope.
* **Definition of Done**: Service terdaftar di ServiceCollection DI, mematuhi matrix transisi status v6 dan matriks validasi v6.

### `BE-HRD-025` — Controller API Manajemen Dokumen SK Kepegawaian & Unggah Pindai Berkas Fisik
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-025.md)
* **Outcome**: Controller RESTful resmi ber-tag Swagger `[Tags("Corporate / Human Resource / Workforce Core / Personnel Action Order")]` aktif melayani query paged list SK, detail SK, aksi siklus hidup (create draft, submit, approve, reject, cancel), serta endpoint unggah dan unduh berkas pindai fisik SK.
* **Traceability**: `HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `contracts/api-contract.md` §PersonnelActionOrder.
* **Contract Version**: API Contract v6.
* **Kemampuan Existing yang Dipakai**: `BaseApiController`, `IAccessActionAuthorizer`, modul manajemen berkas upload internal.
* **Cakupan yang Diharapkan**:
  1. Controller `HrdPersonnelActionOrderController` pada route `api/v1/corporate/human-resource/workforce-core/personnel-action-orders`.
  2. DTO request dan response: `PersonnelActionOrderListItemDto`, `PersonnelActionOrderDetailDto`, `CreatePersonnelActionOrderDto`, `UpdatePersonnelActionOrderDto`, `SubmitPersonnelActionOrderDto`, `ApprovePersonnelActionOrderDto`, `RejectPersonnelActionOrderDto`, `CancelPersonnelActionOrderDto`.
  3. Endpoint unggah berkas `POST .../{id}/attachments` yang menyimpan metadata dan berkas PDF/JPG/PNG max 10MB.
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. `GET /api/v1/corporate/human-resource/workforce-core/personnel-action-orders` mengembalikan `ApiResponse<PagedResult<PersonnelActionOrderListItemDto>>` dengan filter `actionType`, `status`, `organizationUnitId`, `startDate`, `endDate`.
  2. `GET .../{id}` mengembalikan detail lengkap SK beserta daftar penempatan terdampak dan berkas lampiran fisik.
  3. `POST .../create` membuat record SK berstatus `Draft`.
  4. `POST .../{id}/submit`, `approve`, `reject`, `cancel` menjalankan aksi transisi status sesuai wewenang.
  5. Seluruh endpoint dilindungi atribut authorisasi `[AccessAction("Corporate.HumanResource.WorkforceCore.PersonnelActionOrder.*")]`.
* **Bukti Verifikasi**:
  - Swagger UI memproyeksikan tag grup dan skema DTO secara tepat.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend Technical Lead. Risiko: sanitasi nama berkas dan MIME-type pada attachment handler.
* **Definition of Done**: Seluruh endpoint lolos audit Swagger, DTO mematuhi API Contract v6, authorisasi RBAC aktif.

### `BE-HRD-026` — Controller & Service Profil Terpadu Pegawai (Employee 360) dengan Dual-Mode Masking Keamanan
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-026.md)
* **Outcome**: Controller RESTful resmi ber-tag Swagger `[Tags("Corporate / Human Resource / Workforce Core / Employee 360")]` aktif melayani data profil komprehensif pegawai (identitas, penempatan, jabatan, atasan, riwayat pendidikan/pelatihan, dan riwayat mutasi SK) dengan filter otorisasi dan masking keamanan dinamis berbasis relasi aktor login.
* **Traceability**: `HRD-DEC-053`, `FR-HRD-018`, `VR-WFC-06`, `VR-WFC-07`, `contracts/api-contract.md` §Employee360.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **Kemampuan Existing yang Dipakai**: `IWorkforceProfileService`, `WfpManagerAssignment` (relasi hierarki manajerial langsung).
* **Cakupan yang Diharapkan**:
  1. Controller `HrdEmployee360Controller` pada route `api/v1/corporate/human-resource/workforce-core/employee-360/{workforceProfileId}`.
  2. Service `IHrdEmployee360Service` yang mengagregasikan sub-domain data profil dari database.
  3. Mekanisme evaluasi peran: mendeteksi apakah pemanggil adalah HR Officer berwenang, Atasan Langsung aktif pegawai bersangkutan, atau pihak tidak berhak.
  4. Logika sanitasi/masking data personal sensitif dan kompensasi gaji.
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. Jika pemanggil memiliki hak HR (`Corporate.HumanResource.WorkforceCore.Employee360.ViewAll`), kembalikan seluruh data termasuk kompensasi, rekening, NIK, keluarga, dan catatan disiplin (`VR-WFC-06`).
  2. Jika pemanggil adalah Atasan Langsung (`DirectManager`), validasi bahwa target adalah bawahan langsung aktif (`VR-WFC-07`).
  3. Untuk Atasan Langsung, respons wajib secara mutlak menyembunyikan gaji pokok, tunjangan, nomor rekening bank, NIK, susunan keluarga, dan sanksi disiplin (`HRD-DEC-053`).
  4. Jika pemanggil bukan HR dan bukan atasan langsung dari target profil, tolak permintaan dengan HTTP 403 Forbidden.
* **Bukti Verifikasi**:
  - Verifikasi payload keluaran untuk simulasi user HR vs user Direct Manager.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend Security & Domain Lead. Risiko: kebocoran data gaji pada manajer diatasi dengan DTO terpisah atau masking ketat sebelum serialisasi.
* **Definition of Done**: Endpoint Employee 360 aktif dan mematuhi batas privasi klinis/keuangan HRD-DEC-053.

### `BE-HRD-027` — Service & Controller Inisialisasi Penempatan Baseline Pegawai Eksisting (55 Staf)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-027.md)
* **Outcome**: Modul API dan service batch satu kali untuk memasukkan dan memvalidasi penempatan organisasi, jabatan, atasan, dan gaji bagi 55 staf eksisting agar memiliki rekam data penempatan resmi yang terstruktur tanpa merusak integritas historis.
* **Traceability**: `HRD-DEC-054`, `FR-HRD-019`, `VR-WFC-08`, `VR-WFC-09`, `contracts/api-contract.md` §InitialPlacementBatch.
* **Contract Version**: API Contract v6, Data Dictionary v6.
* **Kemampuan Existing yang Dipakai**: `MstWorkforceProfile`, `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`, `WfpSalaryAssignment`.
* **Cakupan yang Diharapkan**:
  1. Controller `HrdInitialPlacementBatchController` pada route `api/v1/corporate/human-resource/workforce-core/initial-placement-batches`.
  2. Engine pemrosesan batch yang membaca daftar baris penempatan awal, memverifikasi ketiadaan duplikasi, dan mengeksekusi penulisan dalam satu transaksi database.
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. Menyediakan endpoint pembuatan draf batch `POST .../initial-placement-batches` dan query detail status batch `GET .../{id}`.
  2. Menyediakan endpoint eksekusi batch `POST .../{id}/execute`.
  3. Validasi batch memastikan tidak ada profil ganda dalam satu batch dan setiap profil belum memiliki penempatan aktif yang bertentangan (`VR-WFC-08`, `VR-WFC-09`).
  4. Eksekusi batch bersifat atomik: jika 1 baris gagal validasi, seluruh batch di-rollback dengan status `Failed` beserta daftar detail baris error (`HRD-DEC-054`).
  5. Setelah sukses dieksekusi, status batch berubah menjadi `Executed` dan tidak dapat dijalankan ulang.
* **Bukti Verifikasi**:
  - Pemeriksaan transaksi rollback saat baris invalid disimulasikan.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Database & Backend Lead. Risiko: duplikasi penempatan awal dicegah dengan flag status executed dan cek penempatan eksisting.
* **Definition of Done**: Service batch teruji, endpoint lolos validasi authorisasi dan kontrak API.

### `BE-HRD-028` — Endpoint Kuasa HR (On-Behalf) Perubahan Profil Pegawai & Penguncian Form Master Data
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-028.md)
* **Outcome**: Jalur pengajuan perubahan data profil pegawai atas nama pegawai oleh petugas HR (on-behalf) aktif dengan audit trail lengkap dan segregasi tugas (pembuat on-behalf dilarang memverifikasi permohonannya sendiri), serta penyesuaian backend master data pegawai untuk menolak pemindahan unit kerja dan jabatan secara langsung.
* **Traceability**: `HRD-DEC-047`, `HRD-DEC-052`, `FR-HRD-016`, `FR-HRD-017`, `VR-WFC-10`, `contracts/api-contract.md` §EmployeeProfileChange.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **Kemampuan Existing yang Dipakai**: `EmployeeProfileChangeController`, `WorkforceProfileController`.
* **Cakupan yang Diharapkan**:
  1. Penambahan action `POST /api/v1/corporate/human-resource/workforce-core/employee-profile-changes/on-behalf` pada `EmployeeProfileChangeController`.
  2. Pencatatan flag `IsSubmittedOnBehalf = true`, `SubmittedByWorkforceProfileId = actorId`, dan field alasan kuasa `OnBehalfReason`.
  3. Penegakan aturan: verifikator persetujuan wajib merupakan petugas HR yang berbeda dari aktor yang mengajukan kuasa (`VR-WFC-10`, `HRD-DEC-052`).
  4. Modifikasi `WorkforceProfileController` pada method Update agar mengabaikan atau menolak perubahan langsung pada kolom organisasi dan jabatan, mengharuskan pemakai menggunakan alur SK Kepegawaian (`HRD-DEC-047`).
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. Endpoint `POST .../employee-profile-changes/on-behalf` menerima payload perubahan data mandiri dan mencatat identitas petugas pembuat kuasa.
  2. Sistem menolak persetujuan permohonan on-behalf jika verifikator memiliki ID yang sama dengan pembuat kuasa (`VR-WFC-10`).
  3. Request `PUT /api/v1/corporate/human-resource/master-data/employees/{id}` menolak perubahan unit organisasi atau jabatan dengan pesan penolakan yang mengarahkan ke menu SK Kepegawaian.
* **Bukti Verifikasi**:
  - Verifikasi penolakan persetujuan on-behalf oleh pembuatnya sendiri.
  - Verifikasi proteksi field unit/jabatan pada endpoint master data pegawai.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend HR Domain Lead. Risiko: kompatibilitas seeder master data existing diantisipasi dengan bypass khusus flag initialization.
* **Definition of Done**: Endpoint on-behalf aktif, guard clause segregasi tugas terpasang, form master data terlindungi.

---

## Kartu Detail Task Backend — Slice `S-D1` Perencanaan Tenaga Kerja

### Catatan umum untuk seluruh kartu `S-D1`

Bacalah bagian ini sekali; setiap kartu di bawah menganggapnya berlaku.

| Hal | Ketentuan |
| --- | --- |
| Gambaran bisnis | `S-D1` menutup rantai "standar kebutuhan tenaga → formasi resmi → gap → permintaan tambahan tenaga → kursi terisi orang yang benar-benar bertugas". Contoh yang dipakai di seluruh kartu: ICU membuka 2 bed baru, Kepala ICU meminta +2 Perawat Pelaksana di luar formasi, Direktur Pelayanan Medis menyetujui, satu kursi diisi pegawai baru (Wati) dan satu lewat SK pindahan (Andi) |
| Kontrak terkunci | API v7 bagian 13, State Transition v7 bagian 11, Validation v7 bagian 12, Permission v7 bagian 11, Integration v7 bagian 11, Data Dictionary v7 bagian 8, Backend Architecture rev 3 bagian 16. Hash-nya pada metadata. Perubahan kontrak di tengah jalan = revisi blueprint baru, bukan keputusan builder |
| Persetujuan | Tidak ada endpoint setuju/tolak/kembalikan di controller domain. Keputusan `T9` s.d. `T13` diambil lewat kotak masuk persetujuan yang sudah ada (`HRD-DEC-018`) dan diteruskan ke domain oleh `HrdWorkforcePlanningWorkflowIntegrationService` (`BE-HRD-030`). Setiap langkah `AllowSelfApproval = false` |
| Pola endpoint | Pola API bagian 0.1–0.3: `ApiResponse<T>`, `PagedResult<T>`, `{id:guid}`, `[Authorize]`, `[AccessController]` di kelas, `[AccessPermission("Resource","Action")]` di setiap endpoint. Controller daftar menyediakan `GET /filters/metadata` sesuai bagian 0.2. Controller **MUST NOT** mengakses `ApplicationDbContext` langsung (`QBE-SVC-001`) |
| Konkurensi dan nomor | Request ubah, ajukan, batal, dan tarik kembali membawa `rowVersion`; versi usang → `409` (`NFR-012`). Nomor dokumen dialokasikan `NumberSeriesAllocator` di luar transaksi pemanggil, mengikuti `EmployeeBenefitEnrollmentService.cs:323-330` |
| Cakupan unit | Kepala Unit hanya melihat dan mengajukan untuk unit di cakupan organisasinya lewat `OrganizationAuthorizationProjectionService`. Di luar cakupan: tidak muncul di daftar, detail `404` (`VR-WFP-38`) |
| Data sensitif | Angka biaya dan teks alasan **MUST NOT** masuk logger dan respons tanpa hak (`NFR-014`, `data-dictionary.md` bagian 8.5). Payload logger hanya `EntityId`, controller, action, status |
| Tanggal | Tanggal acuan memakai tanggal operasional rumah sakit; waktu kejadian disimpan UTC (`NFR-015`) |
| Bukti verifikasi backend | Mengikuti `rules/backend/TEST_POLICY.md`: QBE preflight dan conformance, review diff dan cakupan, `dotnet restore` dan `dotnet build` project aplikasi, verifikasi kontrak API lewat Swagger, verifikasi proses bisnis lewat uji API terhadap database dev, dan uji runtime bila relevan. **Tidak ada** task maupun syarat automated test backend |
| Wewenang database | Membuat berkas migration adalah source. **Menjalankan** migration ke database dev dan **menulis data uji** butuh wewenang tulis database terpisah. Tanpa wewenang itu, bagian uji API ditandai belum terbukti dan task berhenti di 🟡 |
| Handoff | QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi** dari `AGENTS.md` backend target dan dokumen engineering canonical (`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Larangan lintas task | Tidak menyentuh sepuluh model kandidat `WorkforcePlanning` maupun `MstWorkforceRequirement`; tidak membuat endpoint ubah/hapus formasi maupun potret; tidak membaca gaji perorangan; tidak membaca data pasien; tidak mencatat penugasan orang per shift |

### `BE-HRD-017` — Fondasi data `S-D1`: 16 tabel `Hrd*`, enum, konstanta, dan migration `AddWorkforcePlanningCore`
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Belum dikerjakan. **Lingkup ditetapkan 3 November 2026.** Sebelumnya placeholder "Perencanaan Kebutuhan Tenaga Kerja" yang tertahan `{DEC-HRD-004}`; gerbang itu terpenuhi lewat `HRD-DEC-059`, desain `S-D1` disetujui, dan placeholder dipecah menjadi task ini serta `BE-HRD-029` s.d. `BE-HRD-051`.
* **Outcome**: Database punya 16 tabel baru yang masih kosong untuk seluruh data perencanaan tenaga — standar, angka beban, formasi, batch formasi awal, rencana, permintaan beserta kursinya, pembatalan, perubahan rencana pemenuhan, potret gap, dan pengaturan — tanpa satu pun tabel lama yang berubah.
* **Requirement/decision**: `HRD-DEC-059`, `HRD-DEC-019` (prefix `Hrd`), `HRD-TQ-13` (registry), `NFR-012`; `02-backend-architecture.md` bagian 16.4–16.7.
* **Kontrak**: Data Dictionary v7 bagian 8.2–8.4; Backend Architecture rev 3 bagian 16.4.11, 16.5, 16.7.
* **Reuse**: Pola satu berkas model memuat kelas barisnya dari `HrdInitialPlacementBatch.cs`; `RowVersion` + `IsConcurrencyToken()` dari `TrxEmployeeBenefitEnrollmentConfiguration`; FK ke master dan Workforce Core yang sudah ada.
* **Cakupan**:
  1. Sepuluh berkas model di `Areas/Corporate/HumanResource/WorkforcePlanning/Models/` yang memuat 16 kelas (bagian 16.5).
  2. Empat belas enum di `.../WorkforcePlanning/Enums/` dengan nilai persis bagian 16.4.11. `WorkloadAnalysis = 4` ada sebagai nilai cadangan tanpa perilaku.
  3. `Constants/WorkforcePlanningValueConstants.cs`: lima jenis rujukan workflow, lima kunci number series, dan pola kunci advisory lock `HRD_WFP_FORMATION_<unit>_<jabatan>`.
  4. Enam belas `Hrd*Configuration.cs` di `Repositories/Configurations/Corporate/HumanResource/WorkforcePlanning/`.
  5. Enam belas `DbSet` jamak di `ApplicationDbContext`.
  6. Migration `AddWorkforcePlanningCore`.
* **Dependency**: `BE-HRD-023` ✅ (FK kursi internal ke `HrdPersonnelActionOrder`).
* **Acceptance criteria**:
  1. Ke-16 tabel beserta kolom, tipe, nullability, dan panjangnya cocok dengan `data-dictionary.md` bagian 8.2 dan DDL bagian 8.4.
  2. Empat tabel dokumen — `HrdStaffingStandard`, `HrdInitialFormationBatch`, `HrdManpowerPlan`, `HrdHeadcountRequest` — memakai `RowVersion` sebagai concurrency token.
  3. Seluruh index unik DDL bagian 8.4 terpasang, termasuk yang terfilter: satu `LinkedPositionAssignmentId` atau `LinkedPersonnelActionOrderId` hanya untuk satu kursi yang tidak `Cancelled`; satu rencana tahunan per rumah sakit per tahun di luar status `Rejected`/`Cancelled`; satu pengaturan per rumah sakit; satu angka beban per unit × jabatan × tanggal mulai periode.
  4. Perilaku hapus mengikuti DDL: baris anak `CASCADE` ke induknya, rujukan ke master dan Workforce Core `RESTRICT`.
  5. `Up()` hanya berisi pembuatan tabel, index, dan FK untuk ke-16 tabel baru. **Tidak ada** `ALTER` atau `DROP` pada tabel lain, termasuk sepuluh tabel kandidat `WorkforcePlanning` (`HRD-DEC-059`). `Down()` hanya membuang ke-16 tabel itu.
  6. Sepuluh model kandidat dan `MstWorkforceRequirement` tidak dipindah, tidak di-rename, dan tidak dihapus.
* **Bukti verifikasi**: review diff model snapshot EF; `dotnet restore` dan `dotnet build` 0 error; `tooling/qbe/Invoke-QbeConformanceCheck.ps1` memetakan model baru ke baris registry `Hrd`; keluaran `dotnet ef migrations script` untuk migration ini dilampirkan di laporan.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead. Risiko: nama kolom meleset dari kamus data membuat kontrak API ikut meleset — dicegah dengan mencocokkan DDL baris per baris.
* **Definition of Done**: Model, enum, konstanta, konfigurasi, `DbSet`, dan migration terkompilasi; diff hanya menambah; laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-029` — Penelusuran unit induk pada matriks persetujuan untuk langkah Direktur
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Belum dikerjakan. Prasyaratnya sudah ✅ — **boleh dikerjakan sekarang**.
* **Outcome**: Langkah "Direktur yang membawahi unit" pada `T9` dan `T12` menemukan orangnya sendiri. Contoh: Kepala ICU mengajukan standar; sistem menelusuri Unit ICU → Departemen Keperawatan → Direktorat Pelayanan Medis, menemukan baris matriks "Direktorat Pelayanan Medis → jabatan Direktur Pelayanan Medis", dan menugaskan pemegang jabatan itu. Bila jabatan itu kosong, tugas naik ke Direktur Utama; bila keduanya kosong, pengajuan ditolak.
* **Requirement/decision**: `HRD-DEC-072` (`DIRECTOR_IS_POSITION_HOLDER`, `DIRECTOR_VIA_APPROVAL_MATRIX`, `MATRIX_FOLLOWS_UNIT_ANCESTOR`, `DIRECTOR_FALLBACK_TO_PRESIDENT_DIRECTOR`), `HRD-DEC-064` (`UNRESOLVED_STEP_FAILS_CLOSED`), `FR-HRD-116`, `AT-HRD-D1-20`, `INT-HR-18`.
* **Kontrak**: Backend Architecture rev 3 bagian 16.4.9 (`WorkflowService` diperbarui); Integration v7 bagian 11.2.
* **Reuse**: Pencocokan `MstApprovalMatrix` di `WorkflowService.cs` (sekitar baris 1621 dan 1669); sumber penyetuju `Position` dan baris cadangan `IsFallback` (`WorkflowService.cs:1791-1798`); penolakan langkah tanpa penyetuju dan pembuangan pembuat (`WorkflowService.cs:534-556`); `MstOrganizationUnit.ParentOrganizationUnitId`.
* **Cakupan**: Perubahan pada fungsi pencocokan matriks saja. Tanpa perubahan skema, tanpa kolom "kepala unit" baru.
* **Dependency**: `BE-HRD-005` ✅
* **Acceptance criteria**:
  1. Baris matriks untuk sebuah direktorat berlaku bagi seluruh unit di bawahnya; penelusuran naik lewat unit induk sampai baris yang cocok ditemukan.
  2. Baris untuk unit yang lebih dekat menang atas baris unit induknya.
  3. Penyetuju adalah pemegang penempatan jabatan direktur yang berlaku pada tanggal pengajuan; pergantian direktur lewat SK langsung mengganti penyetuju tanpa mengubah data matriks.
  4. Jabatan direktur kosong → langkah naik ke pemegang jabatan Direktur Utama lewat baris `IsFallback`.
  5. Keduanya kosong → pembuatan instance ditolak `400`; langkah **tidak** dilewati.
  6. Rantai `T1` s.d. `T8` dan definisi workflow lain berperilaku sama seperti sebelumnya.
* **Bukti verifikasi**: review diff yang hanya menyentuh pencocokan matriks; `dotnet build` 0 error; uji pembuatan instance di database dev memakai baris matriks dan penempatan jabatan uji untuk kriteria 1–5 (butuh wewenang tulis data dev).
* **Risiko & pemilik**: Pemilik: Backend Workflow Engineer. Risiko: perubahan di mesin persetujuan bersama memengaruhi seluruh modul — dibatasi karena matriks hari ini kosong dan perilaku tanpa baris matriks tidak berubah.
* **Definition of Done**: Penelusuran unit induk aktif, regresi rantai lama tidak ditemukan pada review, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-030` — Lima jenis rujukan workflow dan layanan integrasi persetujuan `S-D1`
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Belum dikerjakan.
* **Outcome**: Dokumen `S-D1` dapat memulai persetujuan di kotak masuk yang sudah ada, dan keputusan akhirnya kembali ke dokumen asal tepat satu kali — walaupun keputusan itu terkirim dua kali.
* **Requirement/decision**: `HRD-DEC-018`, `HRD-DEC-064`, `INT-HR-18` (idempotensi), `NFR-013`, API v7 bagian 13.1.
* **Kontrak**: API v7 bagian 13.1; Integration v7 bagian 11.2; Backend Architecture rev 3 bagian 16.4.9.
* **Reuse**: `WorkflowReferenceLifecycleService.HandleAsync`/`SynchronizeAsync` yang sudah membagi keputusan per jenis rujukan (pola `IsEmployeeProfileChangeReference`); pembuatan instance di `WorkflowService`.
* **Cakupan**:
  1. Mendaftarkan lima jenis rujukan: `WorkforceStaffingStandard`, `WorkforceInitialFormationBatch`, `WorkforceManpowerPlan`, `WorkforceHeadcountRequest`, `WorkforceHeadcountCancellation`.
  2. `HrdWorkforcePlanningWorkflowIntegrationService`: memulai instance untuk dokumen `S-D1` dan menerima keputusan akhir.
  3. Kontrak penerap per jenis rujukan (setujui akhir, tolak, kembalikan) yang diisi task domain berikutnya.
  4. Registrasi `AddScoped` di `Program.cs` dekat registrasi HR lain.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. Kelima jenis rujukan dikenali dan diteruskan ke layanan integrasi; jenis rujukan modul lain tidak terpengaruh.
  2. Keputusan untuk jenis rujukan yang belum punya penerap dicatat sebagai galat dan dokumen tidak berubah — bukan crash.
  3. Keputusan "setujui" yang terkirim ulang untuk dokumen yang sudah final (`Approved`, `Applied`, `ReadyForRecruitment`) diabaikan dan dicatat, tidak menghasilkan apa pun untuk kedua kalinya.
  4. Definisi langkah `T9` s.d. `T13` tidak ditulis di source; definisinya data (`BE-HRD-050`).
  5. Payload logger tanpa angka biaya dan tanpa teks alasan.
* **Bukti verifikasi**: review diff; `dotnet build` 0 error; telusur jalur kode `ApproveAsync` → `HandleAsync` → layanan integrasi dicatat di laporan.
* **Risiko & pemilik**: Pemilik: Backend Workflow Engineer. Risiko: penerapan keputusan berjalan **sesudah** langkah workflow di-commit (`HRD-PLAN-GAP-02`), sehingga bila penerapan domain gagal instance tetap `Approved` — dimitigasi dengan `SynchronizeAsync` yang dapat diulang dan pemeriksaan status idempoten.
* **Definition of Done**: Lima jenis rujukan terdaftar, layanan integrasi terdaftar di DI, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-031` — Pengaturan perencanaan per rumah sakit
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Belum dikerjakan.
* **Outcome**: HR Manager mengisi dua angka kebijakan yang sengaja tidak ditetapkan sistem: berapa hari cuti yang dianggap "tidak tersedia sementara" (misalnya 30 hari), dan berapa hari sebuah permintaan boleh menunggu sebelum ditandai "lewat batas" (misalnya 90 hari).
* **Requirement/decision**: `HRD-DEC-061`, `HRD-DEC-071`, `VR-WFP-40`, `VR-WFP-41`.
* **Kontrak**: API v7 bagian 13 grup `Setting`; Permission v7 bagian 11 `WorkforcePlanningSetting`.
* **Reuse**: Pola controller HR dan custom logger yang sudah ada.
* **Cakupan**: `HrdWorkforcePlanningSettingService`, `HrdWorkforcePlanningSettingController`, `WorkforcePlanningSettingDtos.cs`.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. `GET /?hospitalSiteId=` mengembalikan pengaturan; rumah sakit yang belum punya baris mendapat kedua nilai kosong (`null`), **bukan** angka bawaan.
  2. `PUT /` hanya untuk pemegang `WorkforcePlanningSetting : Update`; peran lain `403`.
  3. Alasan kosong → `400` dengan pesan `VR-WFP-40`.
  4. Angka di bawah 1 atau di atas 365 → `400` dengan pesan `VR-WFP-41`.
  5. `rowVersion` usang → `409`.
  6. Perubahan meninggalkan jejak sesuai `permission-audit-matrix.md` bagian 11.4 tanpa memuat kolom bertanda sensitif di logger.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger grup `Workforce Planning / Setting`; uji API dev untuk kriteria 1–5.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko kecil.
* **Definition of Done**: Dua endpoint aktif sesuai kontrak, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-032` — Buku formasi berversi: satu-satunya penulis formasi dan API baca
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Belum dikerjakan.
* **Outcome**: Formasi jabatan — berapa kursi resmi ICU untuk Perawat Pelaksana pada tanggal tertentu — dapat dibaca beserta riwayat versinya, tetapi tidak dapat diubah lewat endpoint mana pun. Perubahan hanya lahir dari dokumen yang disetujui, lewat satu service.
* **Requirement/decision**: `HRD-DEC-062` (`NO_DIRECT_FORMATION_EDIT`, `FORMATION_CARRIES_FORWARD`), `FR-HRD-098`, `FR-HRD-099`, `FR-HRD-100`, `VR-WFP-10`, `NFR-010`, `NFR-011`, `AT-HRD-D1-06` (bagian API), `AT-HRD-D1-07`.
* **Kontrak**: API v7 bagian 13 grup `Position Formation`; State Transition v7 bagian 11.7.
* **Reuse**: Advisory lock `pg_advisory_xact_lock(hashtext(...))` dari `EmployeeBenefitEnrollmentService.cs:726`.
* **Cakupan**: `HrdPositionFormationService` (buat versi di dalam transaksi pemanggil, tutup versi lama, baca formasi berlaku, baca riwayat); `HrdPositionFormationController` baca saja; `PositionFormationDtos.cs`.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. `GET /?date=2028-01-01` tanpa versi baru untuk 2028 mengembalikan versi terakhir 2027 (`AT-HRD-D1-07`).
  2. `GET /history` mengembalikan seluruh versi satu unit × jabatan beserta jenis dan nomor dokumen sumber, penyetuju, dan waktu persetujuan.
  3. `POST`, `PUT`, `PATCH`, dan `DELETE` ke base URL menjawab `405` dengan pesan `VR-WFP-10` (`AT-HRD-D1-06`).
  4. Service menulis versi baru hanya di dalam transaksi pemanggil, di bawah advisory lock unit × jabatan; versi lama ditutup sehari sebelum tanggal mulai versi baru; kolom selain `EffectiveEndDate` tidak pernah diubah.
  5. Service tidak membuka transaksi sendiri dan tidak dipanggil controller untuk menulis.
  6. Kepala Unit hanya melihat unit dalam cakupannya (`VR-WFP-38`).
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev untuk kriteria 1–3 dan 6; review kode untuk kriteria 4–5 (jalur tulisnya dipakai pertama kali oleh `BE-HRD-033`).
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: penutupan versi lama yang salah tanggal membuat dua versi berlaku bersamaan — dijaga index unik unit × jabatan × tanggal mulai.
* **Definition of Done**: Dua endpoint baca aktif, method tulis `405`, service penulis tunggal terdaftar, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-033` — Batch formasi awal sampai `T10` melahirkan formasi versi 1
* **Slice / MVP**: `S-D1` / `MVP-7`
* **Status**: Belum dikerjakan.
* **Outcome**: HR Admin mengisi formasi resmi pertama dari SK formasi rumah sakit: unduh templat, isi, periksa per baris, unggah scan SK, lalu ajukan. Setelah HR Manager, Finance, dan Direktur Utama menyetujui — semuanya orang lain, bukan pengaju — formasi versi 1 lahir untuk setiap baris sekaligus. Contoh: batch dari "SK Direktur 12/2026" melahirkan formasi ICU versi 1 = 28.
* **Requirement/decision**: `HRD-DEC-070`, `HRD-DEC-064` (`T10`), `FR-HRD-101`, `VR-WFP-11` s.d. `VR-WFP-14`, `AT-HRD-D1-17`, `UAT-31`, `UAT-32`, `NFR-011`, `NFR-012`. Sengaja **tidak** meniru cacat `HRD-TF-033` dan `HRD-TF-034` pada batch penempatan `S-A1`.
* **Kontrak**: API v7 bagian 13 grup `Initial Formation Batch`; State Transition v7 bagian 11.2.
* **Reuse**: Pola unggah berkas `BE-HRD-025` (PDF/JPG/PNG, paling besar 10 MB); `NumberSeriesAllocator`; layanan integrasi `BE-HRD-030`; `HrdPositionFormationService` `BE-HRD-032`; pustaka xlsx yang sudah ada di project (menambah dependency baru butuh approval terpisah).
* **Cakupan**: `HrdInitialFormationBatchService`, `HrdInitialFormationBatchController` (10 endpoint), `InitialFormationBatchDtos.cs`, penerap keputusan `T10` untuk `WorkforceInitialFormationBatch`.
* **Dependency**: `BE-HRD-030`, `BE-HRD-032`
* **Acceptance criteria**:
  1. `GET /template` mengunduh xlsx berisi kolom unit, jabatan, jumlah formasi, dan tanggal berlaku.
  2. `POST /validate` mengembalikan status dan pesan per baris. Unit × jabatan yang sudah punya formasi, sudah ada di batch lain yang belum ditolak/dibatalkan, atau muncul dua kali di batch yang sama → ditolak `VR-WFP-11` (`409` saat simpan atau ajukan).
  3. Scan bukan PDF/JPG/PNG atau lebih dari 10 MB → `400` dengan pesan `VR-WFP-14`.
  4. Ajukan selagi `InvalidRows > 0` → `422` `VR-WFP-12`; ajukan tanpa scan → `422` `VR-WFP-13`.
  5. Ajukan yang sah → `Submitted` dan instance `T10` dimulai; pengaju tidak muncul sebagai penyetuju.
  6. Persetujuan terakhir → `Applied` dan satu formasi versi 1 per baris lewat `HrdPositionFormationService` dalam satu transaksi; satu baris gagal membatalkan seluruhnya; keputusan terkirim ulang tidak membuat versi kedua.
  7. Kembalikan/tolak → `Returned`/`Rejected` beserta alasan; batal hanya dari `Draft`/`Returned` oleh pembuat; `Applied` tidak dapat diubah apa pun.
  8. `rowVersion` usang → `409`.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger grup `Initial Formation Batch`; uji API dev untuk kriteria 1–5, 7, 8; kriteria 6 lewat review jalur penerap, lalu dibuktikan ujung-ke-ujung di `BE-HRD-051`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: batch besar melambat saat persetujuan — seluruh baris tetap satu transaksi demi `NFR-011`.
* **Definition of Done**: Sepuluh endpoint aktif sesuai kontrak, penerap `T10` batch terdaftar, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-034` — Isian angka beban manual
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan.
* **Outcome**: Kepala Unit atau HR mengisi angka beban per unit × jabatan × periode beserta sumbernya. Contoh: "rata-rata 7 pasien ICU terisi per hari, Januari–Maret 2027, sumber: rekap sensus harian". Angka ini dipakai menghitung rekomendasi metode rasio.
* **Requirement/decision**: `HRD-DEC-060` (`MANUAL_WORKLOAD_INPUT`), `FR-HRD-096`, `VR-WFP-08`, `VR-WFP-09`, `VR-WFP-38`. `INT-HR-12` (tarik otomatis dari data pasien) sengaja ditunda.
* **Kontrak**: API v7 bagian 13 grup `Staffing Workload`.
* **Reuse**: `OrganizationAuthorizationProjectionService`.
* **Cakupan**: `HrdStaffingWorkloadService`, `HrdStaffingWorkloadController` (`GET`, `POST`, `PUT`), `StaffingWorkloadDtos.cs`.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. `POST /` yang lengkap → `201`.
  2. Sumber kosong → `400` `VR-WFP-08`.
  3. Isian kedua untuk unit, jabatan, dan tanggal mulai periode yang sama → `409` `VR-WFP-09`.
  4. Kepala Unit di luar cakupan unitnya: tidak muncul di daftar; `POST`/`PUT` → `404` (`VR-WFP-38`).
  5. Isian tidak melewati persetujuan dan tidak menunjuk baris standar, sehingga tetap berlaku saat standar naik versi.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger; uji API dev.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko kecil.
* **Definition of Done**: Tiga endpoint aktif sesuai kontrak, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-035` — Standar kebutuhan tenaga, angka rekomendasi, dan pengesahan `T9`
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan.
* **Outcome**: HR Admin atau Kepala Unit menyusun standar kebutuhan tenaga per unit sebagai versi bertanggal berlaku, melihat angka rekomendasi beserta rincian hitungnya, lalu mengajukannya. Setelah HR Manager dan Direktur yang membawahi unit menyetujui, standar berlaku pada tanggalnya. Penjadwalan dapat membaca standar yang berlaku untuk satu unit pada satu tanggal.
* **Requirement/decision**: `HRD-DEC-058` (`WFP_OWNS_STANDARD`, `SCHEDULING_READS_STANDARD`), `HRD-DEC-060`, `HRD-DEC-064` (`T9`), `HRD-DEC-072`; `FR-HRD-092` s.d. `FR-HRD-095`, `FR-HRD-097`; `VR-WFP-01` s.d. `VR-WFP-07`; `AT-HRD-D1-02`, `AT-HRD-D1-02b`, `AT-HRD-D1-19`; `UAT-29`, `UAT-30`; `INT-HR-13`.
* **Kontrak**: API v7 bagian 13 grup `Staffing Standard`; State Transition v7 bagian 11.1; Backend Architecture rev 3 bagian 16.9 (rumus).
* **Reuse**: Layanan integrasi `BE-HRD-030`; penelusuran Direktur `BE-HRD-029`; angka beban `BE-HRD-034`; `NumberSeriesAllocator`.
* **Cakupan**: `HrdStaffingStandardService`, `HrdStaffingRecommendationService`, `HrdStaffingStandardController` (8 endpoint kontrak + `GET /filters/metadata`), `StaffingStandardDtos.cs`, penerap keputusan `T9`.
* **Dependency**: `BE-HRD-029`, `BE-HRD-030`, `BE-HRD-034`
* **Acceptance criteria**:
  1. `POST /` membuat draf versi baru untuk satu unit; bila ada versi aktif, isinya disalin sebagai titik awal. Draf kedua untuk unit yang masih punya draf/`Returned` → `409` `VR-WFP-01`.
  2. `PUT /{id}` menyimpan baris jabatan dan skill mix. Metode Analisis Beban Kerja → `422` `VR-WFP-02`; faktor cadangan di bawah 1,00 → `400` `VR-WFP-05`; jabatan ganda → `409` `VR-WFP-07`.
  3. `POST /{id}/submit` menolak `422` bila isian per metode kurang (`VR-WFP-03`), faktor cadangan atau pembulatan belum dipilih (`VR-WFP-04` — sistem tidak memberi nilai bawaan), atau tanggal berlaku di masa lalu (`VR-WFP-06`); menolak `400` bila penyetuju langkah Direktur tidak ditemukan.
  4. `GET /{id}/recommendation?periodStartDate=` menghitung per baris dengan rumus bagian 16.9. Contoh yang wajib cocok persis: ICU beban 7, rasio 1:1, 3 shift, faktor 1,5, pembulatan ke atas → `32` dengan rincian "7 ÷ 1 × 1 = 7 per shift; 7 × 3 = 21; 21 × 1,5 = 31,5 → 32"; Farmasi jumlah tetap 4 → `4`; IGD cakupan layanan 3 shift × 2 orang × 1,5 → `9`. Angka beban belum diisi → rekomendasi `null` dan `missingInput` "angka beban belum diisi", **bukan** 0.
  5. Persetujuan terakhir `T9`: tanggal berlaku hari ini → `Active` dan versi aktif sebelumnya `Superseded`; tanggal berlaku di masa depan → `Scheduled`. Kembalikan/tolak wajib beralasan. Versi `Active`, `Scheduled`, `Superseded` tidak dapat diubah isinya.
  6. `GET /effective?organizationUnitId=&date=` mengembalikan versi `Active` pada tanggal itu, atau `404` bila belum ada. Versi `Draft`/`Submitted` tidak pernah dikembalikan.
  7. Tidak ada endpoint grup ini yang menerima atau mengembalikan penugasan orang per shift (`AT-HRD-D1-19`).
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger grup `Staffing Standard`; uji API dev untuk kriteria 1–4, 6, 7; kriteria 5 lewat review jalur penerap, lalu ujung-ke-ujung di `BE-HRD-051`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: pembulatan desimal berbeda dari contoh — rincian teks wajib persis contoh `AT-HRD-D1-02`.
* **Definition of Done**: Endpoint standar aktif, rumus terverifikasi dengan tiga contoh, penerap `T9` terdaftar, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-036` — Scheduler perencanaan dan aktivasi standar pada tanggal berlaku
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan.
* **Outcome**: Setiap hari sistem menjalankan pekerjaan rutin perencanaan. Langkah pertamanya: standar `Scheduled` yang tanggal berlakunya tiba menjadi `Active`, dan versi aktif sebelumnya menjadi `Superseded` dengan tanggal akhir sehari sebelumnya. Task berikutnya menambah langkah ke scheduler yang sama.
* **Requirement/decision**: State Transition v7 bagian 11.1 (`Scheduled` → `Active`), `NFR-013`, Integration v7 bagian 11.2 (scheduler idempoten), `AT-HRD-D1-04`.
* **Kontrak**: Backend Architecture rev 3 bagian 16.4.9 (`HrdWorkforcePlanningSchedulerService`, `HrdWorkforcePlanningSchedulerHostedService`).
* **Reuse**: Pola `BackgroundService` yang didaftarkan `AddHostedService` di `Program.cs` (sekitar baris 1271–1287).
* **Cakupan**: `HrdWorkforcePlanningSchedulerService` dengan langkah-langkah terpisah dan transaksi per langkah; `HrdWorkforcePlanningSchedulerHostedService`; langkah aktivasi standar.
* **Dependency**: `BE-HRD-035`
* **Acceptance criteria**:
  1. Standar `Scheduled` dengan tanggal berlaku = hari ini menjadi `Active`; versi aktif sebelumnya `Superseded` dengan `EffectiveEndDate` = kemarin.
  2. Menjalankan scheduler dua kali pada hari yang sama tidak mengubah apa pun pada jalan kedua.
  3. Kegagalan satu langkah dicatat log dan tidak menghentikan langkah lain.
  4. Struktur langkah memungkinkan task lain menambah langkah tanpa mengubah langkah yang ada.
* **Bukti verifikasi**: `dotnet build` 0 error; uji runtime di dev dengan standar `Scheduled` bertanggal hari ini, lalu periksa status kedua versi sesudah scheduler berjalan dua kali.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: zona waktu membuat aktivasi terlambat sehari — `NFR-015` mengikat tanggal operasional rumah sakit.
* **Definition of Done**: Hosted service terdaftar, langkah aktivasi idempoten, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-037` — Gap tenaga langsung per unit × jabatan
* **Slice / MVP**: `S-D1` / `MVP-8`
* **Status**: Belum dikerjakan.
* **Outcome**: HR dan Kepala Unit melihat, untuk setiap unit × jabatan pada tanggal acuan, berapa formasinya, berapa yang terisi, siapa yang akan kosong, siapa yang tidak tersedia sementara, siapa yang belum kompeten, dan tiga angka gap. Data yang belum ada tampil kosong, **tidak pernah** 0. Contoh: ICU formasi 32, terisi 29,5 (Budi paruh waktu dihitung 0,5; Sari yang Plt di ICU tidak dihitung), akan kosong 1 → gap permanen 3,5.
* **Requirement/decision**: `HRD-DEC-061` (`UNKNOWN_IS_NOT_ZERO`), `FR-HRD-102` s.d. `FR-HRD-104`, `AT-HRD-D1-03`, `AT-HRD-D1-05`, `AT-HRD-D1-05b` (bagian API), `UAT-33`, `INT-HR-15`, `INT-HR-16`.
* **Kontrak**: API v7 bagian 13 grup `Staffing Gap` — `GET /` saja; potret di `BE-HRD-039`. Backend Architecture rev 3 bagian 16.9 (lapisan dan rumus).
* **Reuse**: `WfpOrganizationAssignment`, `WfpPositionAssignment`, cuti yang disetujui, `WfpCompetencyAssessment`, pengunduran diri yang disetujui, SK pindah keluar `Scheduled`; formasi `BE-HRD-032`; standar berlaku `BE-HRD-035`; pengaturan `BE-HRD-031`.
* **Cakupan**: `HrdStaffingGapCalculationService` (tanpa transaksi); `HrdStaffingGapController` endpoint `GET /`; `StaffingGapDtos.cs`.
* **Dependency**: `BE-HRD-031`, `BE-HRD-032`, `BE-HRD-035`
* **Acceptance criteria**:
  1. Terisi dihitung dari penempatan organisasi dan jabatan **utama** yang aktif pada tanggal acuan, dalam porsi kerja; porsi kerja yang belum ada dihitung 1 sampai kolomnya tersedia di Workforce Core. Penempatan bukan utama (rangkap/Plt) hanya tampil sebagai informasi dan tidak menambah terisi.
  2. Lapisan "tidak tersedia sementara" bernilai `null` selama `LongLeaveThresholdDays` belum diisi; lapisan alih daya `null` sampai master jenis tenaga punya kategori alih daya; lapisan "belum kompeten" `null` bila standar berlaku tidak punya skill mix wajib. Baris dengan lapisan `null` membawa penanda kelengkapan `false`.
  3. Gap permanen = formasi − (terisi − akan kosong), ditambah alih daya hanya bila `CountsOutsourced = true`; gap sementara = tidak tersedia sementara; gap kompetensi = belum kompeten.
  4. Hanya standar `Active` yang dipakai; standar versi 2 yang masih `Submitted` tidak menggantikan versi 1 (`AT-HRD-D1-03`), dan respons menyebut versi standar yang dipakai.
  5. Kepala Unit hanya menerima baris unit dalam cakupannya.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev dengan susunan data `UAT-33` (butuh wewenang tulis data dev).
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: penempatan lama yang tanggalnya salah membuat angka terisi salah (`HRD-TF-013`) — bukan cacat task ini, dicatat di laporan bila ditemukan.
* **Definition of Done**: Gap langsung sesuai kontrak dan contoh `UAT-33`, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-038` — Estimasi biaya dari gaji tengah grade dan penyamaran biaya
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan.
* **Outcome**: Setiap permintaan dan baris rencana punya estimasi biaya yang dihitung dari gaji tengah grade, bukan dari gaji orang. Contoh: +2 kursi grade III selama 9 bulan dengan gaji tengah Rp 6.500.000 → 2 × Rp 6.500.000 × 9 = Rp 117.000.000, berlabel "estimasi". Orang yang tidak berhak melihat biaya menerima kolom biaya kosong.
* **Requirement/decision**: `HRD-DEC-065`, `FR-HRD-109`, `FR-HRD-111`, `VR-WFP-37`, `AT-HRD-D1-11` (bagian perhitungan dan penyamaran).
* **Kontrak**: API v7 bagian 13 aturan "Kolom biaya"; Permission v7 bagian 11.1 `WorkforcePlanningCost`; Data Dictionary v7 bagian 8.5.
* **Reuse**: `MstSalaryGrade.MidpointSalary`.
* **Cakupan**: `HrdWorkforcePlanningCostService` (tarif pada tanggal, hitung estimasi, status `RateUnavailable`, penyamaran). Tidak ada controller.
* **Dependency**: `BE-HRD-017`
* **Acceptance criteria**:
  1. Estimasi = porsi kerja × gaji tengah grade × jumlah bulan; contoh di atas menghasilkan Rp 117.000.000.
  2. Grade tanpa gaji tengah → status `RateUnavailable` dan biaya `null`, **bukan** 0.
  3. Pemanggil tanpa `WorkforcePlanningCost : Read` menerima seluruh kolom biaya `null` dan `costVisible = false`.
  4. Tidak membaca gaji perorangan maupun tabel penetapan gaji; membaca biaya tidak termasuk audit pembacaan gaji sensitif `HRD-DEC-039`.
  5. Angka biaya tidak masuk logger.
* **Bukti verifikasi**: `dotnet build` 0 error; review kode; perhitungan dibuktikan lewat endpoint `BE-HRD-040` dan `BE-HRD-046`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: pembulatan rupiah — mengikuti tipe `decimal` kamus data.
* **Definition of Done**: Service terdaftar di DI, aturan penyamaran terpusat di satu tempat, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-039` — Potret gap pengajuan dan API baca potret
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan.
* **Outcome**: Saat permintaan atau rencana diajukan, angka gap hari itu dikunci sebagai potret yang tidak dapat diubah. Penyetuju kemudian dapat membandingkan "saat diajukan 3,5 — sekarang 2,5".
* **Requirement/decision**: `HRD-DEC-069` (`SNAPSHOT_IMMUTABLE`, `SUBMITTED_VS_CURRENT`), `FR-HRD-105` (bagian potret pengajuan), `VR-WFP-39`, `AT-HRD-D1-16` (bagian API).
* **Kontrak**: API v7 bagian 13 grup `Staffing Gap` — `GET /snapshots`, `GET /snapshots/{id}`; State Transition v7 bagian 11.7.
* **Reuse**: `HrdStaffingGapCalculationService` `BE-HRD-037`.
* **Cakupan**: `HrdStaffingGapSnapshotService` (potret `Submission`, ikut transaksi pemanggil); dua endpoint baca potret pada `HrdStaffingGapController`.
* **Dependency**: `BE-HRD-037`
* **Acceptance criteria**:
  1. Service menulis potret `Submission` beserta baris dan penanda kelengkapannya di dalam transaksi pemanggil; nilai `null` tetap `null`.
  2. `GET /snapshots` dan `GET /snapshots/{id}` mengembalikan potret beserta jenisnya.
  3. Tidak ada endpoint ubah maupun hapus potret; method tulis menjawab `405` (`VR-WFP-39`).
  4. Data penempatan yang berubah sesudah potret diambil tidak mengubah isi potret.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev untuk kriteria 2–3; kriteria 1 dan 4 dibuktikan saat `BE-HRD-040` mengajukan permintaan.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko kecil.
* **Definition of Done**: Potret pengajuan dapat dipanggil service lain, dua endpoint baca aktif, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-040` — Permintaan headcount: draf, kursi, sisa kursi, pengajuan, tarik kembali, koreksi Finance
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan.
* **Outcome**: Kepala Unit mengajukan permintaan tenaga untuk satu unit × jabatan yang dipecah menjadi kursi. Setiap kursi punya porsi kerja 0,10–1,00, sumber pemenuhan (rekrut luar atau pindahan internal), dan orang yang digantikan bila pengganti. Sisa kursi diperiksa saat pengajuan sehingga dua permintaan yang diajukan bersamaan tidak dapat mengklaim kursi yang sama. Contoh: sisa kursi ICU 0,5; permintaan 1 kursi penuh ditolak, tetapi 1 kursi berporsi 0,5 diterima.
* **Requirement/decision**: `HRD-DEC-063` (`REMAINING_SEATS`, `NO_DOUBLE_CLAIM_ON_SEATS`), `HRD-DEC-064` (`T11`/`T12`), `HRD-DEC-065` (`FINANCE_ADJUSTMENT`), `HRD-DEC-067`; `FR-HRD-113` (pemeriksaan saat pengajuan), `FR-HRD-114`, `FR-HRD-115`, `FR-HRD-110` (untuk permintaan); `VR-WFP-20` (untuk permintaan), `VR-WFP-22`, `VR-WFP-24` s.d. `VR-WFP-28`, `VR-WFP-34`, `VR-WFP-37`, `VR-WFP-38`; `AT-HRD-D1-08`, `AT-HRD-D1-08b`, `AT-HRD-D1-09`, `AT-HRD-D1-10` (bagian pengajuan), `AT-HRD-D1-11`; `UAT-38`; `NFR-010`.
* **Kontrak**: API v7 bagian 13 grup `Headcount Request` — daftar, detail, sisa kursi, buat, ubah, ajukan, tarik kembali, koreksi Finance; State Transition v7 bagian 11.4 baris pengajuan dan tarik kembali.
* **Reuse**: Advisory lock unit × jabatan yang sama dengan `BE-HRD-032`; biaya `BE-HRD-038`; potret `BE-HRD-039`; layanan integrasi `BE-HRD-030`; `NumberSeriesAllocator`.
* **Cakupan**: `HrdHeadcountRequestService` (bagian draf sampai pengajuan); `HrdHeadcountRequestController` delapan endpoint di atas + `GET /filters/metadata`; `HeadcountRequestDtos.cs`.
* **Dependency**: `BE-HRD-030`, `BE-HRD-038`, `BE-HRD-039`
* **Acceptance criteria**:
  1. Sisa kursi = formasi berlaku − (terisi − akan kosong) − jumlah porsi kursi `Open`/`Linked` permintaan lain unit × jabatan yang sama yang berstatus `Submitted`, `ReadyForRecruitment`, atau `PartiallyFulfilled`. `GET /remaining-seats` mengembalikan angka beserta rinciannya; unit tanpa formasi diberi keterangan bahwa permintaan diproses sebagai di luar formasi.
  2. Pengajuan jenis dalam formasi atau pengganti yang melebihi sisa kursi → `422` `VR-WFP-22` dengan angka sisa kursi (`AT-HRD-D1-08`); 1 kursi berporsi 0,5 pada sisa 0,5 → diterima (`AT-HRD-D1-08b`).
  3. Dua pengajuan bersamaan atas sisa kursi 1 → tepat satu lolos (`AT-HRD-D1-09`), karena pemeriksaan dan penulisan berada di bawah advisory lock unit × jabatan.
  4. Pengajuan menolak `422` bila: kursi pengganti tanpa orang yang digantikan atau orangnya belum berstatus akan keluar/sudah keluar (`VR-WFP-24`); jenis sementara tanpa orang yang digantikan sementara atau tanggal akhir (`VR-WFP-25`); jenis di luar formasi tanpa alasan (`VR-WFP-26`); jumlah kursi luar + internal ≠ jumlah kursi (`VR-WFP-27`). Porsi kursi di luar 0,10–1,00 → `400` `VR-WFP-28`.
  5. Pengajuan yang sah dalam satu transaksi: status `Submitted`, potret gap `Submission`, dan instance `T11` (pengganti, dalam formasi, sementara) atau `T12` (di luar formasi). Penyetuju tidak ditemukan → `400` (`AT-HRD-D1-10`, peran Finance belum dipetakan).
  6. Tarik kembali hanya sebelum persetujuan terakhir; sesudah `ReadyForRecruitment` → `409` `VR-WFP-34`.
  7. Koreksi biaya `POST /{id}/finance-adjustment` hanya oleh penyetuju langkah Finance `T12` yang sedang aktif; lainnya `403` `VR-WFP-20`. Angka awal tetap tersimpan berdampingan dengan koreksi.
  8. Detail tanpa `WorkforcePlanningCost : Read` → kolom biaya `null` dan `costVisible = false`; Kepala Unit unit lain → `404`.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger; uji API dev untuk kriteria 1–4, 6, 8; kriteria 3 dengan dua permintaan yang dikirim serentak dan hitungan kursi `Submitted` sesudahnya; kriteria 5 dan 7 sejauh data workflow dev tersedia, lalu ujung-ke-ujung di `BE-HRD-051`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: kunci advisory yang tidak sama dengan penulis formasi membuka celah klaim ganda — nama kuncinya wajib dari konstanta `BE-HRD-017`.
* **Definition of Done**: Delapan endpoint aktif sesuai kontrak, klaim ganda terbukti tertolak, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-041` — Keputusan akhir `T11`/`T12` permintaan headcount
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan.
* **Outcome**: Permintaan yang disetujui sampai langkah terakhir menjadi "siap direkrut". Untuk permintaan di luar formasi, persetujuan Direktur sekaligus menaikkan formasi. Contoh: +2 Perawat Pelaksana ICU disetujui Direktur Pelayanan Medis → formasi ICU naik dari 30 ke 32 pada transaksi yang sama.
* **Requirement/decision**: `HRD-DEC-063`, `HRD-DEC-064`, `FR-HRD-115`, `UAT-37`, `NFR-011`, `NFR-013`.
* **Kontrak**: State Transition v7 bagian 11.4 (persetujuan terakhir, kembalikan, tolak) dan 11.5 (`Frozen` → `Open` saat `T12` baru menambah formasi).
* **Reuse**: `HrdPositionFormationService` `BE-HRD-032`; layanan integrasi `BE-HRD-030`.
* **Cakupan**: Penerap keputusan untuk jenis rujukan `WorkforceHeadcountRequest`.
* **Dependency**: `BE-HRD-040`
* **Acceptance criteria**:
  1. Persetujuan terakhir `T11` (HR Manager) → `ReadyForRecruitment`.
  2. Persetujuan terakhir `T12` (Direktur yang membawahi unit) → `ReadyForRecruitment` dan versi formasi baru bersumber `HeadcountRequest`, dalam satu transaksi; gagal → seluruhnya batal.
  3. Persetujuan `T12` yang menambah formasi membuka kembali kursi `Frozen` pada unit × jabatan itu bila kini tercakup formasi.
  4. Kembalikan → `Returned`; tolak → `Rejected`; keduanya beralasan.
  5. Keputusan terkirim ulang tidak membuat versi formasi kedua.
  6. Pemeriksaan ulang sisa kursi pada persetujuan terakhir `T11` (`VR-WFP-23`) **tidak** termasuk task ini; ia butuh kait sebelum persetujuan dan dipegang `BE-HRD-049`.
* **Bukti verifikasi**: `dotnet build` 0 error; review jalur penerap; ujung-ke-ujung di `BE-HRD-051` (`UAT-37`, `UAT-39`).
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: penerapan sesudah commit workflow (lihat `BE-HRD-030`).
* **Definition of Done**: Penerap `T11`/`T12` terdaftar dan idempoten, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-042` — Antrean Recruiter dan perubahan rencana pemenuhan
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan.
* **Outcome**: Recruiter melihat daftar kursi rekrut luar yang siap diisi, tanpa angka biaya. HR Manager yang bukan pembuat permintaan dapat memindah sumber kursi yang belum terisi, beserta alasannya. Contoh: permintaan +6 dengan 4 luar + 2 internal → antrean memuat 4; satu kursi internal dipindah ke luar → antrean memuat 5.
* **Requirement/decision**: `HRD-DEC-066` (`MANUAL_LINK_UNTIL_RECRUITMENT_EXISTS`), `HRD-DEC-067`, `FR-HRD-118` (bagian antrean), `FR-HRD-119`, `VR-WFP-33`, `AT-HRD-D1-14`, `INT-HR-17`.
* **Kontrak**: API v7 bagian 13 grup `Recruitment Queue` dan `POST /{id}/fulfilment-plan` pada grup `Headcount Request`.
* **Reuse**: `HrdHeadcountRequestService`.
* **Cakupan**: `HrdRecruitmentQueueController` tersendiri; perubahan rencana pemenuhan yang menulis `HrdFulfilmentPlanChange`.
* **Dependency**: `BE-HRD-041`
* **Acceptance criteria**:
  1. Antrean memuat kursi luar `Open` dari permintaan `ReadyForRecruitment`/`PartiallyFulfilled`; kursi `Frozen` tidak muncul; respons **tanpa** kolom biaya.
  2. Antrean cukup dengan `WorkforceRecruitmentQueue : Read`; Recruiter tidak perlu `WorkforceHeadcountRequest : Read`.
  3. Perubahan rencana pemenuhan hanya oleh HR Manager yang bukan pembuat permintaan, untuk kursi yang belum terisi, dengan alasan; pelanggaran → `403`/`422` `VR-WFP-33`.
  4. Setiap perubahan tercatat satu baris `HrdFulfilmentPlanChange` berisi sumber lama, sumber baru, alasan, pelaku, dan waktu.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger dua grup; uji API dev mengikuti `AT-HRD-D1-14`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko kecil.
* **Definition of Done**: Antrean dan perubahan rencana pemenuhan aktif sesuai kontrak, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-043` — Tautan pemenuhan kursi dan langkah harian scheduler permintaan
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan.
* **Outcome**: HR Admin atau Recruiter menautkan kursi ke penempatan pegawai baru (kursi luar) atau ke SK pindahan (kursi internal). Kursi baru dianggap terisi pada tanggal penempatannya mulai berlaku, bukan saat ditautkan. Contoh: penempatan Wati berlaku 2 Mei; sampai 1 Mei kursinya `Linked`, sesudah scheduler 2 Mei menjadi `Fulfilled` dan permintaannya `PartiallyFulfilled`.
* **Requirement/decision**: `HRD-DEC-066`, `HRD-DEC-071` (`NO_AUTO_EXPIRY`), `FR-HRD-117`, `FR-HRD-118` (kursi internal lewat SK), `FR-HRD-121`, `VR-WFP-29` s.d. `VR-WFP-32`, `AT-HRD-D1-12`, `AT-HRD-D1-13`, `UAT-39` (bagian tautan), `INT-HR-15`.
* **Kontrak**: API v7 bagian 13 `POST`/`DELETE /{id}/seats/{seatId}/fulfilment-link`; State Transition v7 bagian 11.4 (status sistem), 11.5, 11.8 (`IsOverdue`).
* **Reuse**: `WfpPositionAssignment`, `HrdPersonnelActionOrder`; scheduler `BE-HRD-036`; pengaturan `BE-HRD-031`.
* **Cakupan**: `HrdHeadcountFulfilmentService`; dua endpoint tautan; tiga langkah scheduler: `Linked` → `Fulfilled`, kursi kembali `Open` bila SK tertautnya dibatalkan, dan penanda `IsOverdue`.
* **Dependency**: `BE-HRD-036`, `BE-HRD-041`
* **Acceptance criteria**:
  1. Tautan hanya untuk permintaan `ReadyForRecruitment`/`PartiallyFulfilled`; unit atau jabatan penempatan berbeda, atau penempatan bukan utama → `422` `VR-WFP-29` (`AT-HRD-D1-13`).
  2. Penempatan atau SK yang sudah menautkan kursi lain yang tidak dibatalkan → `409` `VR-WFP-30`.
  3. Kursi internal hanya ke SK; kursi luar hanya ke penempatan pegawai baru; selain itu `422` `VR-WFP-31`.
  4. Lepas tautan hanya selama penempatan belum berlaku; kursi `Fulfilled` → `409` `VR-WFP-32`.
  5. Scheduler mengubah kursi `Linked` menjadi `Fulfilled` pada tanggal penempatannya aktif, lalu menghitung ulang status permintaan: ada kursi terisi dan masih ada yang `Open`/`Linked`/`Frozen` → `PartiallyFulfilled`; seluruh kursi `Fulfilled` → `Fulfilled`; seluruhnya `Fulfilled` atau `Cancelled` dengan sekurangnya satu `Cancelled` → `Closed`.
  6. SK tertaut yang berstatus `Cancelled` mengembalikan kursinya ke `Open`.
  7. Permintaan `ReadyForRecruitment`/`PartiallyFulfilled` yang melewati `OverdueRequestThresholdDays` ditandai `IsOverdue` tanpa membatalkan apa pun; selama pengaturan kosong, penanda tidak pernah menyala.
  8. Menjalankan langkah-langkah itu dua kali pada hari yang sama tidak mengubah apa pun pada jalan kedua.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger; uji API dev untuk kriteria 1–4; uji runtime scheduler untuk kriteria 5–8 dengan penempatan bertanggal hari ini.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: penempatan yang sendirinya salah unit tetap lolos (`permission-audit-matrix.md` bagian 11.3) — di luar kendali task ini.
* **Definition of Done**: Tautan dan tiga langkah scheduler aktif dan idempoten, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-044` — Pembatalan kursi `T13`
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: Belum dikerjakan.
* **Outcome**: Kursi yang belum terisi dapat dibatalkan lewat persetujuan satu langkah HR Manager. Bila kursi itu lahir dari permintaan di luar formasi, formasi ikut turun. Contoh: manajemen membatalkan bed tambahan kedua; pembatalan 1 kursi disetujui → formasi ICU 32 menjadi 31.
* **Requirement/decision**: `HRD-DEC-071` (`ONLY_UNFILLED_SEATS`, `T12_CANCEL_REDUCES_FORMATION`), `FR-HRD-120` (kecuali syarat lowongan), `VR-WFP-35`, `AT-HRD-D1-18` (kecuali kaki `VR-WFP-36`), `UAT-39` (bagian pembatalan).
* **Kontrak**: API v7 bagian 13 `POST /{id}/cancellations` dan bagian 13.1 (`T13`); State Transition v7 bagian 11.5 dan 11.6.
* **Reuse**: Layanan integrasi `BE-HRD-030`; `HrdPositionFormationService` `BE-HRD-032`; hitung ulang status permintaan dari `BE-HRD-043`.
* **Cakupan**: Pengajuan pembatalan; penerap keputusan `WorkforceHeadcountCancellation`.
* **Dependency**: `BE-HRD-043`
* **Acceptance criteria**:
  1. Pengajuan hanya untuk kursi `Open` atau `Frozen`, dengan alasan; kursi `Fulfilled` atau `Linked` → `422` `VR-WFP-35`.
  2. Pengajuan yang sah → `HrdHeadcountRequestCancellation` berstatus `Submitted` dan instance `T13`; pengaju tidak dapat menyetujui sendiri.
  3. Persetujuan → kursi `Cancelled`; bila permintaan berasal dari `T12`, versi formasi turun sebesar porsi kursi yang dibatalkan, dalam satu transaksi; status permintaan dihitung ulang (bisa menjadi `Closed`).
  4. Penolakan beralasan → kursi kembali tanpa tanda pembatalan.
  5. Keputusan terkirim ulang tidak menurunkan formasi dua kali.
  6. Penjaga "tutup lowongan dulu" (`VR-WFP-36`) **tidak** termasuk task ini; dipegang `BE-HRD-045`.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev untuk kriteria 1–2; review jalur penerap untuk 3–5; ujung-ke-ujung di `BE-HRD-051`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: tanpa `BE-HRD-045`, kursi dengan kandidat yang sedang diproses di luar sistem dapat terbatalkan — dicatat sebagai risiko rilis selama `HRD-PLAN-GAP-01` terbuka.
* **Definition of Done**: Pembatalan `T13` aktif dan idempoten, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ⛔ `BE-HRD-045` — Penjaga "tutup lowongan dulu" pada pembatalan kursi
* **Slice / MVP**: `S-D1` / `MVP-9`
* **Status**: ⛔ **TERBLOKIR** — tertahan `HRD-PLAN-GAP-01`. Pemilik penyelesaian: Pengguna (pemilik produk dan teknis) lewat revisi desain `S-D1`. Task tidak boleh dimulai sebelum sumber data lowongan dirancang dan disetujui.
* **Outcome**: Pembatalan kursi ditolak selama masih ada lowongan terbuka untuk kursi itu, agar kandidat yang sedang diproses tidak ditinggalkan diam-diam.
* **Requirement/decision**: `HRD-DEC-071` (`CLOSE_VACANCY_FIRST`), `VR-WFP-36`, `AT-HRD-D1-18` (kaki gagal pertama), `UAT-40`.
* **Kontrak**: Validation v7 bagian 12.4 `VR-WFP-36`; **sumber data belum ada** di Data Dictionary v7 bagian 8.
* **Reuse**: Ditentukan setelah gap ditutup.
* **Cakupan**: Pemeriksaan pada pengajuan pembatalan `BE-HRD-044`, memakai sumber data yang ditetapkan revisi desain.
* **Dependency**: `BE-HRD-044`, `{HRD-PLAN-GAP-01 ⛔}`
* **Acceptance criteria**:
  1. Pengajuan pembatalan untuk kursi yang lowongannya masih terbuka → `409` dengan pesan `VR-WFP-36`.
  2. Sesudah lowongan ditutup, pembatalan yang sama diterima (`AT-HRD-D1-18` kaki berhasil).
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev `UAT-40`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: membangun task ini sebelum gap ditutup berarti mengarang kolom data.
* **Definition of Done**: Penjaga aktif sesuai kontrak hasil revisi, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-046` — Rencana tahunan dan addendum: draf, hitung ulang, pengajuan `T10`, koreksi Finance
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: Belum dikerjakan.
* **Outcome**: HR Admin menyusun Rencana Kebutuhan Tenaga 2027 dari usulan unit: sistem mengisi formasi saat ini, angka rekomendasi beserta rinciannya, dan estimasi biaya; usulan yang berbeda dari rekomendasi wajib beralasan. Di tengah tahun, perubahan dibuat sebagai addendum dari rencana yang sudah disetujui. Finance dapat mengoreksi total biaya pada langkahnya, dan angka awal tetap tersimpan.
* **Requirement/decision**: `HRD-DEC-062`, `HRD-DEC-065`, `HRD-DEC-068` (`APPROVED_PLAN_LOCKED`); `FR-HRD-106` s.d. `FR-HRD-110`; `VR-WFP-15` s.d. `VR-WFP-20`; `AT-HRD-D1-15` (kaki gagal), `AT-HRD-D1-21` (kaki berhasil dan kaki `VR-WFP-20`); `UAT-35` (bagian penyusunan), `UAT-36`.
* **Kontrak**: API v7 bagian 13 grup `Manpower Plan` (10 endpoint); State Transition v7 bagian 11.3.
* **Reuse**: Rekomendasi dari `BE-HRD-035`; biaya `BE-HRD-038`; potret `BE-HRD-039`; formasi berlaku `BE-HRD-032`; layanan integrasi `BE-HRD-030`; `NumberSeriesAllocator`.
* **Cakupan**: `HrdManpowerPlanService` (bagian draf sampai pengajuan dan koreksi Finance); `HrdManpowerPlanController` + `GET /filters/metadata`; `ManpowerPlanDtos.cs`.
* **Dependency**: `BE-HRD-038`, `BE-HRD-039`
* **Acceptance criteria**:
  1. Rencana tahunan kedua untuk rumah sakit dan tahun yang sama yang belum ditolak/dibatalkan → `409` `VR-WFP-15`.
  2. Addendum hanya dari rencana `Approved` → selain itu `422` `VR-WFP-16`; addendum hanya memuat baris yang berubah dan sekurangnya satu baris.
  3. Mengubah rencana `Approved` → `409` `VR-WFP-17` dengan arahan membuat addendum (`UAT-36`).
  4. `POST /{id}/recalculate` mengisi formasi saat ini, rekomendasi beserta rincian teksnya, dan biaya; kolom itu tidak diterima dari request.
  5. Pengajuan menolak `422` bila baris tanpa grade (`VR-WFP-18`) atau usulan berbeda dari rekomendasi tanpa alasan (`VR-WFP-19`); pengajuan yang sah menyimpan potret gap `Submission` dan memulai `T10` dalam satu transaksi.
  6. Koreksi Finance hanya oleh penyetuju langkah Finance `T10` yang aktif → lainnya `403` `VR-WFP-20`; angka awal dan koreksi tersimpan berdampingan beserta alasan, pelaku, dan waktunya.
  7. `GET /{id}/gap-snapshots` mengembalikan potret yang menempel pada dokumen.
  8. Kolom biaya mengikuti aturan penyamaran `BE-HRD-038`.
* **Bukti verifikasi**: `dotnet build` 0 error; Swagger; uji API dev untuk kriteria 1–5, 7, 8; kriteria 6 sejauh data workflow dev tersedia, lalu ujung-ke-ujung di `BE-HRD-051`.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: rencana dengan banyak baris lambat dihitung ulang — hitung ulang hanya pada draf.
* **Definition of Done**: Sepuluh endpoint aktif sesuai kontrak, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-047` — Keputusan akhir `T10` rencana dan addendum: formasi baru dan penandaan permintaan berlebih
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: Belum dikerjakan.
* **Outcome**: Saat Direktur Utama menyetujui rencana atau addendum, formasi baru lahir untuk setiap baris pada tanggal berlakunya, dan permintaan yang sedang berjalan tetapi kini melebihi formasi ditandai. Contoh: addendum ICU −2 disetujui → formasi ICU turun pada tanggal berlakunya, dan kursi permintaan berjalan yang berlebih dibekukan dengan keterangan "dibekukan sesudah addendum".
* **Requirement/decision**: `HRD-DEC-062`, `HRD-DEC-068`, `FR-HRD-112`, `AT-HRD-D1-15` (kaki berhasil), `UAT-35` (formasi 30 mulai 1 Januari 2027), `NFR-011`, `NFR-013`.
* **Kontrak**: State Transition v7 bagian 11.3 (persetujuan terakhir), 11.5 (`Open`/`Linked` → `Frozen`), 11.8 (`IsOverFormationAfterAddendum`).
* **Reuse**: `HrdPositionFormationService` `BE-HRD-032`; permintaan dan kursi dari `BE-HRD-041`.
* **Cakupan**: Penerap keputusan untuk jenis rujukan `WorkforceManpowerPlan`.
* **Dependency**: `BE-HRD-041`, `BE-HRD-046`
* **Acceptance criteria**:
  1. Persetujuan terakhir → `Approved`, versi formasi baru per baris bersumber `ManpowerPlan` atau `PlanAddendum`, dalam satu transaksi; satu baris gagal membatalkan seluruhnya.
  2. Permintaan berjalan yang kini melebihi formasi diberi `IsOverFormationAfterAddendum = true`, dan kursi berlebihnya menjadi `Frozen` sehingga hilang dari antrean Recruiter.
  3. Penanda padam bila kursi berlebih dibatalkan atau `T12` baru disetujui.
  4. Kembalikan/tolak beralasan; `Approved` tidak dapat dibatalkan maupun dibuka kembali.
  5. Keputusan terkirim ulang tidak membuat versi formasi kedua.
* **Bukti verifikasi**: `dotnet build` 0 error; review jalur penerap; ujung-ke-ujung di `BE-HRD-051` (`UAT-35`, `AT-HRD-D1-15`).
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: pembekuan kursi yang sudah `Linked` membingungkan Recruiter — keterangan pembekuan wajib ikut di respons.
* **Definition of Done**: Penerap `T10` rencana terdaftar dan idempoten, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### `BE-HRD-048` — Potret gap bulanan dan rekonstruksi
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: Belum dikerjakan.
* **Outcome**: Setiap akhir bulan sistem menyimpan potret gap seluruh unit. Bulan yang terlewat karena scheduler mati tidak diisi diam-diam: bulan itu tampil "potret tidak tersedia", dan pengisian susulan diberi label "direkonstruksi".
* **Requirement/decision**: `HRD-DEC-069` (`MISSED_SNAPSHOT_NOT_FAKED`), `FR-HRD-105` (bagian bulanan), `AT-HRD-D1-16b`, `INT-HR-14` (kolom `ShortageSummaryCount` tetap kosong sampai Penjadwalan mengirimnya).
* **Kontrak**: State Transition v7 bagian 11.7; Data Dictionary v7 bagian 8.2 `HrdStaffingGapSnapshot`.
* **Reuse**: Scheduler `BE-HRD-036`; `HrdStaffingGapSnapshotService` `BE-HRD-039`.
* **Cakupan**: Langkah akhir bulan pada scheduler; penulisan potret `Monthly`; cara membuat potret `Reconstructed` untuk bulan lampau.
* **Dependency**: `BE-HRD-036`, `BE-HRD-039`
* **Acceptance criteria**:
  1. Pada akhir bulan, satu potret `Monthly` per rumah sakit tersimpan; jalan kedua di hari yang sama tidak membuat potret kedua.
  2. Potret `Monthly` tidak pernah ditulis untuk bulan lampau; potret susulan wajib berjenis `Reconstructed` dan mengisi `ReconstructedAt`.
  3. `ShortageSummaryCount` dibiarkan kosong (`null`).
* **Bukti verifikasi**: `dotnet build` 0 error; uji runtime dengan tanggal akhir bulan di dev; skenario `AT-HRD-D1-16b` dengan scheduler dimatikan sepanjang satu bulan.
* **Risiko & pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: "akhir bulan" pada zona waktu salah — `NFR-015`.
* **Definition of Done**: Potret bulanan idempoten dan tidak memalsukan bulan terlewat, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ⛔ `BE-HRD-049` — Kait validasi sebelum persetujuan untuk `VR-WFP-21` dan `VR-WFP-23`
* **Slice / MVP**: `S-D1` / `MVP-10`
* **Status**: ⛔ **TERBLOKIR** — tertahan `HRD-PLAN-GAP-02`. Pemilik penyelesaian: pemilik arsitektur backend (Pengguna) lewat revisi `02-backend-architecture.md` bagian 16.4.9. Task tidak boleh dimulai sebelum revisi itu disetujui, karena menyentuh mesin persetujuan bersama di luar izin desain saat ini.
* **Outcome**: Dua persetujuan ditolak sebelum tersimpan bila aturannya dilanggar: Finance tidak dapat menyetujui rencana yang masih punya baris "tarif belum tersedia" tanpa koreksi, dan HR Manager tidak dapat menyetujui terakhir permintaan `T11` yang sisa kursinya sudah habis dipakai permintaan lain — permintaan itu dikembalikan ke Kepala Unit.
* **Requirement/decision**: `HRD-DEC-065` (`FINANCE_ADJUSTMENT`), `HRD-DEC-063` (`RECHECK_ON_FINAL_APPROVAL`), `FR-HRD-113` (pemeriksaan ulang), `VR-WFP-21`, `VR-WFP-23`, `AT-HRD-D1-21` (kaki gagal kedua).
* **Kontrak**: Validation v7 bagian 12.3 dan 12.4; State Transition v7 bagian 11.3 dan 11.4; bentuk kaitnya menunggu revisi bagian 16.4.9.
* **Reuse**: Ditentukan revisi desain.
* **Cakupan**: Ditentukan revisi desain.
* **Dependency**: `BE-HRD-041`, `BE-HRD-046`, `{HRD-PLAN-GAP-02 ⛔}`
* **Acceptance criteria**:
  1. Finance menyetujui rencana yang masih punya baris "tarif belum tersedia" tanpa koreksi → `422` `VR-WFP-21`; langkah tidak tersimpan.
  2. Persetujuan terakhir `T11` saat sisa kursi tidak lagi cukup → `409` `VR-WFP-23`, dan permintaan menjadi `Returned`.
* **Bukti verifikasi**: `dotnet build` 0 error; uji API dev dua skenario di atas.
* **Risiko & pemilik**: Pemilik: Backend Workflow Engineer. Risiko: kait baru di mesin bersama berdampak pada semua modul.
* **Definition of Done**: Kedua aturan ditegakkan sebelum commit langkah, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ⛔ `BE-HRD-050` — Data runtime dev `S-D1`
* **Slice / MVP**: `S-D1` / Verifikasi
* **Status**: ⛔ **TERBLOKIR** — menunggu pemetaan peran `HRD-DEC-032` (`MAPPING_REQUIRED`) oleh Administrator, dan wewenang tulis data dev. Bukan pekerjaan source.
* **Outcome**: Database dev punya isian minimum agar rantai persetujuan `S-D1` dapat dijalankan sungguhan: definisi `T9` s.d. `T13`, jabatan dan pemegang jabatan direktur, baris matriks persetujuan beserta cadangannya, gaji tengah grade, dan satu baris pengaturan perencanaan.
* **Requirement/decision**: `02-backend-architecture.md` bagian 16.8, `HRD-DEC-064`, `HRD-DEC-071`, `HRD-DEC-072`, `HRD-DEC-032`; `04-prd-to-mvp.md` bagian 21.19 butir "Data master awal terisi".
* **Kontrak**: Backend Architecture rev 3 bagian 16.8.
* **Reuse**: Layar dan endpoint master yang sudah ada: definisi workflow, matriks persetujuan, master jabatan, batch penempatan atau SK (`S-A1`), master grade gaji.
* **Cakupan**: Pengisian data lewat endpoint/layar yang sudah ada, dicatat langkah demi langkah di laporan. Tanpa perubahan source, tanpa seed di kode.
* **Dependency**: `BE-HRD-030`, `{HRD-DEC-032 ⛔}`
* **Acceptance criteria**:
  1. Lima definisi alur `T9` s.d. `T13` dengan langkah sesuai `HRD-DEC-064` dan `HRD-DEC-071`; setiap langkah `AllowSelfApproval = false`.
  2. Jabatan "Direktur Utama" dan sekurangnya satu jabatan direktur per direktorat yang membawahi unit uji, masing-masing dengan satu penempatan utama aktif.
  3. Baris matriks langkah Direktur untuk `T9` dan `T12` per direktorat dengan sumber `Position`, ditambah satu baris `IsFallback` ke Direktur Utama.
  4. Gaji tengah terisi untuk grade yang dipakai data uji.
  5. Nilai standar klinis yang dipakai uji diberi label data uji; nilai produksi tetap milik pemilik kebijakan (`HRD-Q-01`).
* **Bukti verifikasi**: Tangkapan isi definisi, matriks, dan penempatan dari endpoint baca yang sudah ada, dilampirkan di laporan.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead bersama Administrator. Risiko: data uji terbawa ke produksi — semua isian diberi penanda data uji.
* **Definition of Done**: Kelima kriteria terbukti di dev, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### ⛔ `BE-HRD-051` — Verifikasi ujung-ke-ujung `S-D1`
* **Slice / MVP**: `S-D1` / Verifikasi
* **Status**: ⛔ **TERBLOKIR** — menunggu `BE-HRD-050`, yang tertahan `HRD-DEC-032`.
* **Outcome**: Alur bisnis `FLOW-HRD-MVP-002` dibuktikan utuh lewat API di dev: formasi awal disetujui orang lain, standar ICU disahkan, rencana 2027 disetujui dengan koreksi Finance, permintaan +2 ICU di luar formasi sampai Direktur Pelayanan Medis, satu kursi diisi pegawai baru dan satu lewat SK, satu kursi dibatalkan dengan formasi ikut turun, dan potret bulanan tersimpan.
* **Requirement/decision**: `04-prd-to-mvp.md` bagian 21.5 (titik akhir rilis), 21.9, 21.18, 21.19; `AT-HRD-D1-04`, `AT-HRD-D1-10`, `AT-HRD-D1-12`, `AT-HRD-D1-14`, `AT-HRD-D1-15`, `AT-HRD-D1-17`, `AT-HRD-D1-18` (kecuali kaki `VR-WFP-36`), `AT-HRD-D1-19`, `AT-HRD-D1-20`, `AT-HRD-D1-21` (kecuali kaki `VR-WFP-21`); `UAT-29` s.d. `UAT-39`.
* **Kontrak**: Seluruh kontrak v7 bagian `S-D1`.
* **Reuse**: Kotak masuk persetujuan yang sudah ada.
* **Cakupan**: Eksekusi skenario lewat API, pencatatan request/response per skenario. Tanpa perubahan source; cacat yang ditemukan dikembalikan ke task pemiliknya dan statusnya diturunkan.
* **Dependency**: `BE-HRD-044`, `BE-HRD-047`, `BE-HRD-048`, `BE-HRD-050`
* **Acceptance criteria**:
  1. Setiap baris `AT-HRD-D1` dan `UAT` di atas punya bukti request/response di laporan, dengan hasil yang diharapkan tercapai.
  2. Penyetuju setiap langkah bukan pembuat dokumen.
  3. Kaki `VR-WFP-21`, `VR-WFP-23`, dan `VR-WFP-36` dicatat "tidak diuji — tertahan `HRD-PLAN-GAP-01`/`HRD-PLAN-GAP-02`", bukan dihilangkan.
* **Bukti verifikasi**: Laporan skenario beserta potongan respons; Swagger grup `Workforce Planning` untuk `AT-HRD-D1-19`.
* **Risiko & pemilik**: Pemilik: Backend Technical Lead. Risiko: data dev bercampur data uji lain — memakai unit uji khusus.
* **Definition of Done**: Seluruh skenario yang tidak tertahan gap terbukti, laporan tracked ada.
* **Handoff**: QBE preflight dan kesesuaian engineering diselesaikan saat eksekusi (catatan umum `S-D1`).

### Requirement `S-D1` yang sengaja tidak punya task backend

| Requirement | Alasan | Kapan menjadi task |
| --- | --- | --- |
| `AT-HRD-D1-01`, `AT-HRD-D1-01b` — pengaman tabel kosong pada migration pembersihan | Migration `RetireWorkforcePlanningCandidateTables` **tidak dijadwalkan** pada `S-D1` (bagian 16.7): lima tabel kandidat masih dirujuk `S-D2` dan Penjadwalan | Setelah desain `S-D2` dan Penjadwalan menetapkan nasib FK-nya |
| `INT-HR-12` — angka beban otomatis dari data pasien | `HRD-DEP-005` `MISSING`; ditunda `HRD-DEC-060` | Setelah kontrak privasi data pasien ada |
| `INT-HR-14` — ringkasan kekurangan tenaga dari Penjadwalan | Bentuknya ditetapkan desain Penjadwalan; `S-D1` hanya menyediakan kolom | Desain Penjadwalan |
| Metode Analisis Beban Kerja | Ditunda `HRD-DEC-060` `ABK_LATER_SLICE`; nilai enum dicadangkan | Slice berikutnya |
| Kolom porsi kerja (modul 02) dan kategori alih daya (modul 01) | Milik modul lain; selama belum ada, lapisan terkait tampil "data belum tersedia" | Desain Workforce Core dan Master Data HR |
