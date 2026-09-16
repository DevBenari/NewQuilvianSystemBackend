# Roadmap Delivery Backend V2 — Sub-modul Keperawatan Rawat Inap

> ## Berkas ini **baru**, dan **tidak menggantikan** `backend-roadmap.md`
>
> | Hal | `backend-roadmap.md` (lama) | **`backend-roadmap-v2.md` (berkas ini)** |
> | --- | --- | --- |
> | Isinya | Task revision `2` s.d. `6` | **Hanya** task penyelarasan `PRD-RWI-V2-001` revision `7` |
> | Rentang task ID | `BE-RWI-054` s.d. `BE-RWI-078` | **`BE-RWI-106` s.d. `BE-RWI-126`** |
> | Statusnya | **Tetap berlaku** sebagai register task lama | Register task baru |
> | Kenapa dipisah | Permintaan pemilik 16 September 2026 | — |
>
> `BE-RWI-079` s.d. `BE-RWI-087` dipakai `episode-rawat-inap`, dan `BE-RWI-088` s.d. `BE-RWI-105`
> dipakai `dokter-rawat-inap`. **Nomor task tidak pernah dipakai ulang.**
>
> Berkas lama: [`backend-roadmap.md`](./backend-roadmap.md) — jangan menambah task baru di sana.

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
entity_prefix: Inp
blueprint_id: RWI-BP-001
blueprint_revision: 7
blueprint_shape: COMPOSITE
submodule: keperawatan
blueprint_root: docs/module-blueprints/rawat-inap/keperawatan/
roadmap_file: roadmap/backend-roadmap-v2.md
roadmap_revision: 1
status: APPROVED
roadmap_mode: DELIVERY
realignment_phase: RLN-PH-07
approval_gate: BLUEPRINT_APPROVED
approved_by: "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061)"
approved_at: "2026-09-16"
approval_decision: RWI-DEC-150
gate_closure_decision: RWI-DEC-151   # {GATE-YOGA} tertutup 2026-09-16
upstream_input: "PRD-RWI-V2-001 v2.0 — docs/Modul-RS/Rawat-Inap/04-prd-to-mvp-final.md"
input_revision_hash: sha256:2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f
contract_version: 0.5.0
decision_source: "00-interview-decisions.md revision 23; RWI-DEC terakhir 151"
backend_source_sha: df3679c0d5b2f08106702153eb242d3a6cb2929b
frontend_source_sha: 1ce219b40f8e411f3c4e66975626ab33ae81616a
task_id_range: BE-RWI-106..BE-RWI-126
task_id_next_free: BE-RWI-127
waves: [KEP-V2-0, KEP-V2-1, KEP-V2-2, KEP-V2-3, KEP-V2-4]
migration_steps: [K0, K1, K2, K3, K4, K5, K6, K7]
owned_tables_note: "Sub-modul ini TIDAK memiliki satu tabel pun — RWI-DEC-081. Enam belas tabel baru milik ClinicalManagement (11) dan PharmacyManagement (5)"
test_policy: "rules/backend/TEST_POLICY.md — backend tidak memelihara project automated test"
write_authority: "TIDAK diberikan di sini. Wewenang tulis source, migration, database, dan deployment dinyatakan terpisah per task"
```

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan Definition of Done sudah terbukti, laporan tracked-nya ada |
| 🟡 | Source-nya sudah ada tetapi kriterianya belum terbukti penuh |
| ⛔ | Prasyaratnya belum terpenuhi; nama blocker-nya disebut |
| tanpa tanda | Belum disentuh sama sekali |

---

## Peringatan kepemilikan tabel

`RWI-DEC-081` menetapkan sub-modul ini **tidak memiliki satu tabel pun**. Enam belas tabel baru pada
revision `7` seluruhnya lahir di modul lain:

| Migration | Tabel baru | Pemilik tabel |
| --- | --- | --- |
| `K1`, `K3`, `K5`, `K6` | Instrumen klinis, jawaban, Evaluasi Awal, cairan, gula darah, observasi, shift beserta revisinya; kolom pada `TrxPatientAssessment`, `TrxPatientVitalSign`, `TrxPatientAllergy` | `ClinicalManagement` |
| `K4`, `K7` | MAR, revisi dosis, jadwal, pengaturan, pelaksanaan sliding scale | `PharmacyManagement` |

Task di bawah **MUST NOT** membuat tabel tandingan di dalam `InPatientManagement`. Bila saat
implementasi terasa butuh tabel milik Rawat Inap, yang benar adalah kembali ke `grill-me`.

---

## Grafik Urutan Dependency

Roadmap ini memuat 21 task, melewati batas 15 node satu grafik. Grafiknya dipecah per gelombang,
dengan grafik ringkasan antar-gelombang di bawah ini.

### Ringkasan antar-gelombang

```text
KEP-V2-0 ─┬─> KEP-V2-1
          │
          └─> KEP-V2-2

KEP-V2-3

KEP-V2-4
```

`KEP-V2-3` dan `KEP-V2-4` tidak menunggu gelombang keperawatan mana pun. Keduanya menunggu **sub-modul
lain**: `KEP-V2-3` menunggu `dokter-rawat-inap`, `KEP-V2-4` menunggu kontrak Billing.

### Legenda label asal

- `[BE-DOK]` = task backend pada `dokter-rawat-inap/roadmap/backend-roadmap-v2.md`
- `[V0]`, `[K1]`, `[K2a]` = task roadmap ini yang digambar **penuh** pada blok grafik itu
- `[atas]` = task roadmap ini yang digambar penuh pada blok tepat di atasnya
- `{...}` = gerbang yang menahan, bukan task

Node berlabel adalah **cermin baca-saja** dan boleh muncul lebih dari sekali. Setiap task milik
roadmap ini digambar **penuh tepat satu kali**.

### `KEP-V2-0` — perbaikan keselamatan tanpa bentuk data

```text
BE-RWI-106
```

Tidak menunggu siapa pun. Nol pasangan.

### `KEP-V2-1` — konfigurasi klinis, pengkajian, Evaluasi Awal

```text
BE-RWI-106 [V0] ─> BE-RWI-107 ─┬─> BE-RWI-108
                               │
                               ├─> BE-RWI-109
                               │
                               ├─> BE-RWI-113
                               │
                               └─> BE-RWI-110 ─┬─> BE-RWI-111 ─┐
                                               │               │
                                               └───────────────┴─> BE-RWI-112
```

Delapan pasangan.

### `KEP-V2-2` blok a — MAR

```text
BE-RWI-106 [V0] ─> BE-RWI-114 ─┬─> BE-RWI-115 ─┬─> BE-RWI-116
                               │               │
                               │               └─> BE-RWI-117
                               │
                               └─> BE-RWI-118
```

Lima pasangan.

### `KEP-V2-2` blok b — Pengawasan Harian dan sliding scale

```text
BE-RWI-114 [K2a] ─> BE-RWI-119 ─┬─> BE-RWI-120
                                │
                                ├─> BE-RWI-122
                                │
                                └─> BE-RWI-123

BE-RWI-115 [K2a] ─────────────────> BE-RWI-122 [atas]

BE-RWI-103 [BE-DOK] ──────────────> BE-RWI-123 [atas]

BE-RWI-110 [K1] ─> BE-RWI-121
```

Tujuh pasangan.

### `KEP-V2-3` — permukaan yang memakai kontrak `dokter-rawat-inap`

```text
BE-RWI-094 [BE-DOK] ─> BE-RWI-124

BE-RWI-101 [BE-DOK] ─┬─> BE-RWI-125
                     │
BE-RWI-097 [BE-DOK] ─┘
```

Tiga pasangan.

### `KEP-V2-4` — tagihan pasien

```text
{GATE-BILLING ⛔} ─> BE-RWI-126
```

Satu pasangan.

**Jumlah pasangan seluruh grafik: 0 + 8 + 5 + 7 + 3 + 1 = 24.** Jumlah entri kolom `Dependency`
pada tabel task: **24**. Keduanya cocok.

### Tabel gelombang eksekusi

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RWI-106` |
| 1 | `BE-RWI-094` [BE-DOK] mendarat | `BE-RWI-124` |
| 1 | `BE-RWI-101` dan `BE-RWI-097` [BE-DOK] mendarat | `BE-RWI-125` |
| 2 | `BE-RWI-106` | `BE-RWI-107`, `BE-RWI-114` — boleh paralel |
| 3 | `BE-RWI-107` | `BE-RWI-108`, `BE-RWI-109`, `BE-RWI-110`, `BE-RWI-113` — boleh paralel |
| 3 | `BE-RWI-114` | `BE-RWI-115`, `BE-RWI-118`, `BE-RWI-119` — boleh paralel |
| 4 | `BE-RWI-110` | `BE-RWI-111`, `BE-RWI-121` |
| 4 | `BE-RWI-115` | `BE-RWI-116`, `BE-RWI-117` |
| 4 | `BE-RWI-119` | `BE-RWI-120` |
| 4 | `BE-RWI-119`, `BE-RWI-115` | `BE-RWI-122` |
| 4 | `BE-RWI-119` **dan** `BE-RWI-103` [BE-DOK] | `BE-RWI-123` |
| 5 | `BE-RWI-110`, `BE-RWI-111` | `BE-RWI-112` |
| — | ⛔ menunggu `{GATE-BILLING}` | `BE-RWI-126` |

Pemetaan gelombang PRD:

| Gelombang PRD | Isinya | Task backend |
| --- | --- | --- |
| `KEP-V2-0` | `FR-KEP-038` — migration `K0` | `BE-RWI-106` |
| `KEP-V2-1` | `EPIC KEP-09` s.d. `KEP-12` — migration `K1`–`K3` | `BE-RWI-107` s.d. `BE-RWI-113` |
| `KEP-V2-2` | `EPIC KEP-13`, `14`, `15` — migration `K4`–`K7` | `BE-RWI-114` s.d. `BE-RWI-123` |
| `KEP-V2-3` | `EPIC KEP-16` | `BE-RWI-124`, `BE-RWI-125` |
| `KEP-V2-4` | `EPIC KEP-17` | `BE-RWI-126` ⛔ |

---

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-106` | Lima cacat keselamatan pengkajian tertutup tanpa menyentuh bentuk data | `FR-KEP-038`; `RLN3-CAP-17`, `18`, `21`, `22`, `29` | `0.5.0` API pengkajian | Jalur pengkajian yang sudah ada | `K0` — enum frontend–backend sejajar; daftar berpaginasi; isian belum dikaji tidak terkirim sebagai normal; detail dibaca sebelum disunting; penanganan `401`/`403` | — | AC-1 s.d. AC-6 | `dotnet build`; verifikasi kontrak API; review diff dan scope | "Belum dikaji" terkirim sebagai "normal" adalah cacat keselamatan / Muhammad Hamzah | Kartu `BE-RWI-106` |
| `BE-RWI-107` | Instrumen klinis menjadi konfigurasi berversi, bukan angka di dalam kode | `FR-KEP-039`, `041`, `042`; `RWI-DEC-124`, `136` | `0.5.0` data 11.4–11.5 | — (`MISSING / NEW`) | `K1` — tabel instrumen, versi, jawaban; definisi JSON beserta hash; seeder **draft** lima instrumen dan formulir | `BE-RWI-106` | AC-1 s.d. AC-6 | `dotnet build`; verifikasi skema; verifikasi kontrak API | Seeder **tidak** membereskan batas yang bertabrakan — hanya menandainya / komite keperawatan | Kartu `BE-RWI-107` |
| `BE-RWI-108` | Versi instrumen disahkan orang yang berbeda dari pengubahnya | `FR-KEP-040`; `VAL-KEP-20a` | `0.5.0` state 5.1 | Tabel dari `BE-RWI-107` | Siklus `Draft` → `Approved` → `Retired`; pengesah bukan pengubah terakhir | `BE-RWI-107` | AC-1 s.d. AC-4 | `dotnet build`; verifikasi proses bisnis | — / komite keperawatan | Kartu `BE-RWI-108` |
| `BE-RWI-109` | Skor risiko jatuh dihitung dari instrumen berversi, bukan angka tetap | `FR-KEP-043`, `FR-KEP-044`; mencabut `RWI-FACT-036` | `0.5.0` data 11.6 | Perhitungan risiko jatuh yang sudah ada | `K2` — perhitungan rawat inap pindah ke instrumen berversi; jalur non-rawat-inap **tetap** sampai pemilik `rawat-jalan` memutuskan | `BE-RWI-107` | AC-1 s.d. AC-5 | `dotnet build`; verifikasi proses bisnis; **regresi poliklinik** | **Mengubah perilaku** — pemberitahuan pemilik `rawat-jalan` wajib; mundur wajib dicatat sebagai kejadian keselamatan / pemilik `rawat-jalan` | Kartu `BE-RWI-109` |
| `BE-RWI-110` | Kajian Umum delapan bagian tersimpan terstruktur | `FR-KEP-045`, `046`, `049`; `RWI-DEC-131` | `0.5.0` data + API | `TrxPatientAssessment`, `TrxPatientVitalSign` | `K3` — tiga kolom `TrxPatientAssessment`, satu kolom `TrxPatientVitalSign`; Kajian Umum **menunjuk** satu baris tanda vital, tidak menyalin angkanya | `BE-RWI-107` | AC-1 s.d. AC-6 | `dotnet build`; verifikasi skema; verifikasi kontrak API | Menyalin angka vital = dua sumber kebenaran / Muhammad Hamzah | Kartu `BE-RWI-110` |
| `BE-RWI-111` | Risiko jatuh, nyeri, dan edukasi menjadi dokumen tersendiri | `FR-KEP-047`, `FR-KEP-048`; `VAL-KEP-22` | `0.5.0` enum 6–8 | Jenis dokumen pengkajian | Tiga jenis dokumen baru; Monitoring Nyeri mewajibkan keadaan nyeri sebelum selesai dan menyimpan waktu kajian ulang | `BE-RWI-110` | AC-1 s.d. AC-5 | `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis | — / Muhammad Hamzah | Kartu `BE-RWI-111` |
| `BE-RWI-112` | Perawat melihat berapa bagian pengkajian yang sudah beres | `FR-KEP-050`, `051`, `052`; `RWI-DEC-119` | `0.5.0` API 7.1 | — (`MISSING / NEW`) | Progres lima bagian ✓/!/○, persen kelipatan 20; temuan berisiko menjadi **alert kepala konteks**, bukan mengubah progres | `BE-RWI-110`, `BE-RWI-111` | AC-1 s.d. AC-6 | `dotnet build`; verifikasi kontrak API | Progres gagal dimuat **tidak boleh** tampil sebagai ○ / Muhammad Hamzah | Kartu `BE-RWI-112` |
| `BE-RWI-113` | MPP mengisi Evaluasi Awal sebagai satu dokumen hidup per episode | `FR-KEP-053`, `054`, `055`; `RWI-DEC-115`, `140` | `0.5.0` data 11.7, state 5.3 | — (`MISSING / NEW`) | `K3` — dokumen Evaluasi Awal delapan bagian checklist berversi; hanya pemegang hak MPP di unit episode yang menulis | `BE-RWI-107` | AC-1 s.d. AC-5 | `dotnet build`; verifikasi skema; verifikasi proses bisnis | Addendum menunggu jenis dokumen `14` — `INT-KEP-12` / Muhammad Hamzah | Kartu `BE-RWI-113` |
| `BE-RWI-114` | Dosis obat berjadwal terbentuk sendiri dan tidak pernah ganda | `FR-KEP-064`, `FR-KEP-071`; `INT-KEP-08` | `0.5.0` data 11.13, API 7.13 | — (`MISSING / NEW`) | `K4` — tabel MAR, revisi, jadwal, pengaturan; hosted service pembentukan dosis; **idempoten** saat MAR dibuka dan saat terjadwal | `BE-RWI-106` | AC-1 s.d. AC-6 | `dotnet build`; verifikasi skema; uji idempoten dua pemanggilan | Dosis ganda = risiko pemberian obat berlebih / Muhammad Hamzah | Kartu `BE-RWI-114` |
| `BE-RWI-115` | Perawat mencatat pemberian obat beserta alasannya | `FR-KEP-065`, `067`, `068`; `VAL-KEP-30` | `0.5.0` state 5.5, API 7.11 | Tabel dari `BE-RWI-114` | Dosis `Administered`/`Held`/`Refused`/`Missed` bersyarat isian dan alasan; PRN mencatat indikasi dan evaluasi; koreksi **menyimpan revisi**, dosis tidak pernah dihapus | `BE-RWI-114` | AC-1 s.d. AC-7 | `dotnet build`; verifikasi kontrak API; uji kiriman ulang | Kiriman ulang **tidak boleh** menggandakan pencatatan / Muhammad Hamzah | Kartu `BE-RWI-115` |
| `BE-RWI-116` | Obat high-alert menunggu perawat kedua | `FR-KEP-066`; `VAL-KEP-31`; `RWI-DEC-117` | `0.5.0` state 5.5 | Jalur pencatatan dari `BE-RWI-115` | Cek ganda `NotRequired`/`Pending`/`Confirmed`/`Rejected` sebelum `Administered` | `BE-RWI-115` | AC-1 s.d. AC-5 | `dotnet build`; verifikasi proses bisnis | Perawat kedua **tidak boleh** orang yang sama / Muhammad Hamzah | Kartu `BE-RWI-116` |
| `BE-RWI-117` | Dugaan reaksi obat tercatat sebagai alergi tertaut | `FR-KEP-070`; `INT-KEP-13` | `0.5.0` data | `TrxPatientAllergy` | `K6` — dua kolom baru; dugaan reaksi dicatat dari dosis sebagai alergi `Suspected` tertaut, **tanpa mengubah MAR** | `BE-RWI-115` | AC-1 s.d. AC-4 | `dotnet build`; verifikasi skema; verifikasi proses bisnis | Mengubah MAR dari jalur alergi = riwayat pemberian obat berubah / Muhammad Hamzah | Kartu `BE-RWI-117` |
| `BE-RWI-118` | Obat yang dihentikan tidak muncul lagi di daftar perawat | `FR-KEP-069`; `INT-KEP-09`, `INT-KEP-15` | `0.5.0` integrasi | Tabel dari `BE-RWI-114` | Penghentian butir membatalkan dosis `Due` sesudahnya; penutupan episode membatalkan dosis `Due` masa depan | `BE-RWI-114` | AC-1 s.d. AC-4 | `dotnet build`; verifikasi proses bisnis; uji galat buatan | Titik temu dengan `BE-RWI-100` [BE-DOK] dan `BE-RWI-087` [BE-INP] / Muhammad Hamzah | Kartu `BE-RWI-118` |
| `BE-RWI-119` | Cairan, gula darah, dan observasi harian tersimpan terstruktur | `FR-KEP-057`, `061`, `062`; `RWI-DEC-148` | `0.5.0` data 11.8–11.10 | — (`MISSING / NEW`) | `K5` — tabel cairan, gula darah, observasi, shift beserta revisi; entri bersumber, bervolume ml, berwaktu, berpelaksana; GDS **satu tempat** dengan satuan wajib tanpa bawaan | `BE-RWI-114` | AC-1 s.d. AC-7 | `dotnet build`; verifikasi skema; verifikasi kontrak API | Satuan GDS berbawaan = salah hitung dosis insulin / Muhammad Hamzah | Kartu `BE-RWI-119` |
| `BE-RWI-120` | Balance cairan per shift dan 24 jam terbaca | `FR-KEP-059`, `FR-KEP-060`; `AC-KEP-093` | `0.5.0` API 7.5 | Tabel dari `BE-RWI-119` | Balance dihitung dari entri **aktif**; jam shift dikonfigurasi per unit atau bawaan; tanpa shift hanya 24 jam | `BE-RWI-119` | AC-1 s.d. AC-5 | `dotnet build`; verifikasi kontrak API | Jam shift **tidak** mempengaruhi kewenangan / Muhammad Hamzah | Kartu `BE-RWI-120` |
| `BE-RWI-121` | Tanda vital terbaca sebagai deret per episode | `FR-KEP-056`; `RWI-DEC-118` | `0.5.0` data 11.2 | `TrxPatientVitalSign` | Tanda vital menyimpan episode; tampil sebagai deret dan grafik per episode | `BE-RWI-110` | AC-1 s.d. AC-4 | `dotnet build`; verifikasi skema; verifikasi kontrak API | — / Muhammad Hamzah | Kartu `BE-RWI-121` |
| `BE-RWI-122` | Intake obat tertaut ke dosis yang benar-benar diberikan | `FR-KEP-058`, `FR-KEP-063`; `RWI-DEC-149`; `VAL-KEP-24d`–`g` | `0.5.0` validation | Tabel dari `BE-RWI-119`, dosis dari `BE-RWI-115` | Intake obat menunjuk **tepat satu** dosis MAR `Administered`, volume diketik termasuk pelarut; pengingat dosis tanpa entri; penanda entri yang dosisnya dikoreksi — **tanpa mewajibkan** | `BE-RWI-119`, `BE-RWI-115` | AC-1 s.d. AC-6 | `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis | Usulan `G-26`, `G-27` — `ADOPTED_AS_PROPOSED` `RWI-DEC-150` / Muhammad Hamzah | Kartu `BE-RWI-122` |
| `BE-RWI-123` | Dosis insulin dihitung dari GDS bangsal menurut order yang aktif | `FR-KEP-072` s.d. `076`; `RWI-DEC-146`, `148` | `0.5.0` state 5.6, API 7.12 | Order dari `BE-RWI-103` [BE-DOK] | `K7` — tabel pelaksanaan; ditolak tanpa order aktif; hanya dari GDS bangsal bersatuan sama; GDS, dosis MAR, dan pelaksanaan dalam **satu transaksi idempoten** | `BE-RWI-119`, `BE-RWI-103` [BE-DOK] | AC-1 s.d. AC-7 | `dotnet build`; verifikasi skema; verifikasi proses bisnis; uji galat buatan | **Paling berbahaya di roadmap ini** — salah hitung = dosis insulin salah / pemilik klinis **belum ditunjuk** | Kartu `BE-RWI-123` |
| `BE-RWI-124` | SOAP dan catatan keperawatan masuk ke CPPT yang sama | `FR-KEP-077`; `RWI-AC-204`, `205` | `0.5.0` + `0.6.0` [DOK] | Kolom `NoteKind` dari `BE-RWI-094` | SOAP dan Catatan Keperawatan disimpan sebagai CPPT berjenis `NursingSoap` dan `NursingNarrative`; menu masing-masing menyaring jenisnya | `BE-RWI-094` [BE-DOK] | AC-1 s.d. AC-4 | `dotnet build`; verifikasi kontrak API | Enum jenis dikunci `dokter-rawat-inap`, bukan di sini / Muhammad Hamzah | Kartu `BE-RWI-124` |
| `BE-RWI-125` | Perawat mencatat obat bawaan dan memesan tindakan atas instruksi dokter | `FR-KEP-078`, `FR-KEP-079`; `INT-DOK-19` | `0.6.0` [DOK] API 7.15 | Kontrak `dokter-rawat-inap` | Perawat mencatat obat bawaan dan **membaca** keputusan rekonsiliasi; perawat memesan tindakan dengan dokter pemberi instruksi | `BE-RWI-101` [BE-DOK], `BE-RWI-097` [BE-DOK] | AC-1 s.d. AC-5 | `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis | Perawat **tidak** memutuskan rekonsiliasi — hanya membaca / Muhammad Hamzah | Kartu `BE-RWI-125` |
| `BE-RWI-126` ⛔ | Ringkasan tagihan terbaca bagi pemegang hak khusus | `FR-KEP-082`; `RWI-DEC-137` | `0.5.0` API 7.14 | — (`MISSING / NEW`) | Ringkasan **baca-saja tanpa harga per item**, hanya bagi pemegang `PatientBillingSummary : Read` | `{GATE-BILLING}` | AC-1 s.d. AC-4 | `dotnet build`; verifikasi kontrak API | ⛔ **menunggu kontrak Billing**; `RWI-OQ-053` pemiliknya belum bernama | Kartu `BE-RWI-126` |

---

## Kartu task

### `BE-RWI-106` — Lima perbaikan keselamatan pengkajian (migration K0)

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1 — `KEP-V2-0` |
| **Migration** | `K0` — kode saja, nol perubahan bentuk data |

**Bisnis prosesnya.** Impact scan `RLN-PH-03` menemukan lima cacat pada jalur pengkajian yang sudah
berjalan. Yang paling berbahaya: isian yang **belum dikaji** dikirim ke server sebagai nilai
**normal**. Artinya pemeriksaan yang tidak pernah dilakukan tercatat sebagai pemeriksaan dengan hasil
baik — dan itu bisa menutupi pasien yang sebenarnya berisiko.

**Cakupan yang diharapkan.**

- Enum jenis pengkajian di frontend disejajarkan dengan backend (`RLN3-CAP-18`, `29`).
- Daftar pengkajian per episode dibaca **berpaginasi** (`RLN3-CAP-17`).
- Isian yang belum dikaji **tidak** terkirim sebagai normal (`RLN3-CAP-22`).
- Detail dibaca lebih dulu sebelum disunting (`RLN3-CAP-21`).
- Penanganan `401` dan `403` diperbaiki.

**Acceptance criteria.**

1. Nilai enum jenis pengkajian sama persis antara frontend dan backend, dan daftar sahnya satu sumber.
2. Endpoint daftar pengkajian per episode berpaginasi dan tidak mengembalikan seluruh riwayat sekaligus.
3. Isian yang belum dikaji tersimpan sebagai **belum dikaji**, bukan sebagai normal.
4. Penyuntingan menolak kiriman yang tidak didahului pembacaan detail.
5. `401` dan `403` dibedakan dan ditangani sesuai `RLN3-CAP` yang bersangkutan.
6. Regresi: pengkajian yang sudah tersimpan tetap terbaca apa adanya.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API; review diff dan scope yang menyebut
kelima cacat satu per satu.

**Definition of Done.** Laporan tracked ada di `task/report/backend/BE-RWI-106.md` dan menyebut
kelima perbaikan; roadmap dan `requirement-traceability-v2.md` diperbarui.

---

### `BE-RWI-107` — Instrumen dan formulir klinis berversi (migration K1)

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 2 — `KEP-V2-1` |
| **Migration** | `K1` — tabel instrumen, versi, jawaban; milik `ClinicalManagement` |

**Bisnis prosesnya.** Instrumen penilaian klinis — risiko jatuh, nyeri, edukasi — punya angka batas
dan pita skor yang bisa berubah mengikuti kebijakan komite keperawatan. Selama angkanya ada di dalam
kode, setiap perubahan kebijakan berarti perubahan kode. `K1` memindahkannya menjadi **konfigurasi
berversi**: definisi disimpan sebagai JSON beserta hash-nya, dan setiap dokumen menyimpan versi mana
yang dipakai.

**Yang sengaja tidak dilakukan.** Seeder membuat versi **draft** dari lima instrumen dan formulir
yang ada sekarang. Bila ada batas yang bertabrakan pada definisi lama, seeder **menandainya** dan
berhenti di situ — ia tidak membereskannya sendiri. Membereskan batas klinis adalah keputusan komite
keperawatan, bukan keputusan seeder.

**Acceptance criteria.**

1. Tabel instrumen, versi, dan jawaban ada di `ClinicalManagement`.
2. Definisi tersimpan sebagai JSON beserta hash-nya; hash berubah bila definisinya berubah.
3. Seeder membuat versi **`Draft`** untuk tiga instrumen risiko jatuh V1, formulir Kajian Umum, Edukasi, Perencanaan Pulang, dan checklist MPP.
4. Batas yang bertabrakan pada definisi lama **ditandai**, bukan dibereskan otomatis.
5. Pita skor divalidasi tidak bertumpuk dan tidak berlubang; instrumen sejenis tidak bertumpuk rentang usia — `FR-KEP-041`.
6. Versi `Draft` hanya dapat dipakai menyelesaikan dokumen di lingkungan dengan **pengaturan uji aktif** — `FR-KEP-042`.

**Bukti verifikasi.** `dotnet build`; verifikasi skema pada Postgres sekali pakai; verifikasi kontrak
API; keluaran seeder ditempel ke laporan termasuk daftar batas yang ditandai.

**Definition of Done.** Laporan tracked ada dan memuat daftar batas bertabrakan yang ditemukan
seeder; roadmap dan traceability diperbarui.

---

### `BE-RWI-108` — Pengesahan versi instrumen

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 3 — `KEP-V2-1` |

**Bisnis prosesnya.** Pemisahan pengubah dan pengesah adalah kendali empat mata. Orang yang mengetik
angka batas tidak boleh menjadi orang yang menyatakan angka itu sah dipakai pada pasien.

**Acceptance criteria.**

1. Siklus status `Draft` → `Approved` → `Retired` ditegakkan; `Retired` tidak kembali.
2. Pengesah **bukan** pengubah terakhir versi itu — `VAL-KEP-20a`.
3. Pengesahan memensiunkan versi sah sebelumnya pada transaksi yang sama.
4. Hanya satu versi `Approved` per instrumen pada satu waktu.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis termasuk percobaan pengesahan oleh
pengubah terakhir.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-109` — Risiko jatuh memakai instrumen berversi (migration K2)

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 3 — `KEP-V2-1` |
| **Migration** | `K2` — **mengubah perilaku**; wajib diberitahukan pemilik `rawat-jalan` |

**Bisnis prosesnya.** Perhitungan risiko jatuh rawat inap berpindah dari angka di dalam kode ke
instrumen berversi dari `BE-RWI-107`. Jalur **non-rawat-inap tetap** memakai perhitungan lama sampai
pemilik `rawat-jalan` memutuskan sendiri.

**Acceptance criteria.**

1. Skor dan pita dihitung **server** dan disimpan bersama versi instrumennya — `FR-KEP-044`.
2. Skor **tidak dihitung ulang** setelah dokumen selesai.
3. Source **tidak lagi memuat** angka batas maupun skor butir risiko jatuh rawat inap, dan tidak memuat daftar isian wajib tetap — `FR-KEP-043`.
4. Jalur non-rawat-inap tetap memakai perhitungan lama, tanpa perubahan hasil.
5. Pengkajian lama yang sudah menyimpan versi instrumen tetap terbaca.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis; **regresi poliklinik** dengan
hasilnya ditempel apa adanya.

**Risiko dan kewajiban koordinasi.** `K2` adalah salah satu dari **tiga** langkah revision `7` yang
mengubah perilaku poliklinik (`R7`, `R9`, `K2`). Pemberitahuan kepada pemilik blueprint `rawat-jalan`
**wajib tercatat sebelum rilis**. Bila langkah ini harus dimundurkan, pengkajian yang sudah menyimpan
versi tetap terbaca, tetapi kembalinya ke perhitungan lama **wajib dicatat sebagai kejadian
keselamatan**.

**Definition of Done.** Pemberitahuan pemilik `rawat-jalan` tercatat; regresi poliklinik hijau;
laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-110` — Kajian Umum delapan bagian (migration K3)

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 3 — `KEP-V2-1` |
| **Migration** | `K3` — tiga kolom `TrxPatientAssessment`, satu kolom `TrxPatientVitalSign` |

**Bisnis prosesnya.** Kajian Umum ditata ulang menjadi delapan bagian, dengan tiap isian berada di
bagian yang masuk akal secara klinis: kateter di Eliminasi, alat bantu mobilitas di Ketergantungan,
psikososial di Kondisi Umum, catatan relevan di Sumber Data Pasien.

**Yang paling menentukan.** Kajian Umum **menunjuk** satu baris tanda vital lewat `VitalSignId`,
bukan menyalin angkanya. Menyalin berarti dua sumber kebenaran: kalau tanda vitalnya dikoreksi,
salinan di kajian tetap salah.

**Acceptance criteria.**

1. Tiga kolom baru ada pada `TrxPatientAssessment` dan satu pada `TrxPatientVitalSign`.
2. Kajian Umum tersusun delapan bagian sesuai `03-frontend-architecture.md` 10.4.3 — `FR-KEP-045`.
3. Kajian Umum menyimpan `VitalSignId`, **bukan** nilai tanda vitalnya — `FR-KEP-046`.
4. Koreksi pada baris tanda vital yang ditunjuk ikut terbaca dari kajian.
5. Input tanda vital rawat inap **tidak menerima** isian nyeri — `FR-KEP-049`, `VAL-KEP-22c`.
6. Migration mundur menghapus kolom selama tabel belum berisi data pasien sungguhan.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; verifikasi kontrak API.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-111` — Risiko jatuh, nyeri, dan edukasi sebagai dokumen tersendiri

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 4 — `KEP-V2-1` |

**Bisnis prosesnya.** Tiga penilaian ini punya siklus sendiri — dikaji, dinilai ulang, ditutup — dan
tidak cocok menjadi bagian dari satu dokumen kajian besar. Masing-masing menjadi jenis dokumen
tersendiri dengan enum `6`, `7`, dan `8`.

**Acceptance criteria.**

1. Tiga jenis dokumen baru terdaftar dan dapat dibuat per episode — `FR-KEP-047`.
2. Monitoring Nyeri **mewajibkan** keadaan nyeri terisi sebelum dokumen dapat diselesaikan — `VAL-KEP-22`.
3. Monitoring Nyeri menyimpan **waktu kajian ulang**.
4. Ketiganya memakai instrumen berversi dari `BE-RWI-107`, bukan angka di dalam kode.
5. Dokumen lama yang sebelumnya menjadi bagian kajian besar tetap terbaca.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-112` — Progres pengkajian lima bagian

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 5 — `KEP-V2-1` |

**Bisnis prosesnya.** Perawat perlu tahu bagian mana dari pengkajian yang sudah beres dan mana yang
belum, tanpa membuka satu per satu. Progres menghitung lima bagian dengan tiga keadaan: ✓ selesai,
! perlu perhatian, ○ belum.

**Dua hal yang sengaja dibedakan.** Pertama, **temuan berisiko** — misalnya skor risiko jatuh tinggi —
tampil sebagai **alert pada kepala konteks**, bukan mengubah angka progres. Pengkajian yang sudah
lengkap tetap 100% walau hasilnya mengkhawatirkan; keduanya informasi berbeda. Kedua, progres yang
**gagal dimuat** menampilkan **galat**, bukan ○. Menampilkan ○ berarti memberi tahu perawat bahwa
bagian itu belum dikaji, padahal yang sebenarnya terjadi adalah sistem tidak tahu.

**Acceptance criteria.**

1. Progres menghitung lima bagian dengan keadaan ✓ / ! / ○ menurut keadaan dokumen — `FR-KEP-050`.
2. Persen yang dihasilkan selalu kelipatan 20.
3. Pengawasan Harian dan Evaluasi Awal **tampil tanpa dihitung** ke dalam persen.
4. Temuan berisiko tampil sebagai alert kepala konteks dan **tidak** mengubah progres — `FR-KEP-051`.
5. Progres yang gagal dimuat menampilkan galat, **bukan** ○ — `FR-KEP-052`, `AC-KEP-072`.
6. Progres dihitung server, bukan disusun ulang oleh frontend.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API bagian 7.1.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-113` — Evaluasi Awal MPP (migration K3)

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 3 — `KEP-V2-1` |
| **Migration** | `K3` — dokumen Evaluasi Awal, milik `ClinicalManagement` |

**Bisnis prosesnya.** Evaluasi Awal adalah dokumen Manajer Pelayanan Pasien: menilai kebutuhan
pasien di luar sisi medis — sosial, pembiayaan, rencana pulang. `RWI-DEC-150` bagian b mengadopsi
usulan `G-09`: **satu dokumen hidup per episode**, dikoreksi lewat addendum — bukan dokumen baru
setiap kali dinilai ulang.

**Acceptance criteria.**

1. Dokumen Evaluasi Awal ada sebagai jenis tersendiri milik `ClinicalManagement`, delapan bagian checklist berversi — `FR-KEP-053`.
2. Hanya pemegang hak MPP yang **ditempatkan di unit episode** yang dapat menulis; perawat lain membaca — `FR-KEP-054`, `VAL-KEP-23`.
3. Satu Evaluasi Awal hidup per episode; unique parsial per episode menegakkannya — `FR-KEP-055`.
4. Status `Draft` → `Completed` → `Cancelled` sesuai state 5.3.
5. Koreksi lewat addendum **menunggu** jenis dokumen `14` tersedia — `INT-KEP-12`; selama belum ada, jalur addendum-nya belum dibuka dan itu dicatat apa adanya.

**Bukti verifikasi.** `dotnet build`; verifikasi skema termasuk unique parsial; verifikasi proses
bisnis.

**Catatan.** Kriteria 5 adalah **dependency yang diketahui dan sengaja dibiarkan terbuka**, bukan
cacat. Ia dicatat pada laporan task sebagai keadaan, bukan disembunyikan.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-114` — MAR dan pembentukan dosis (migration K4)

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 2 — `KEP-V2-2` |
| **Migration** | `K4` — tabel MAR, revisi, jadwal, pengaturan; milik `PharmacyManagement` |

**Bisnis prosesnya.** MAR — *Medication Administration Record* — adalah daftar dosis obat yang harus
diberikan perawat, lengkap dengan jamnya. Dosis `Due` dibentuk dari butir resep aktif yang berjadwal.
Pembentukannya harus **idempoten**: kalau dibentuk dua kali, yang muncul tetap satu dosis. Dosis ganda
berarti perawat melihat dua perintah pemberian obat untuk satu jadwal yang sama.

**Contoh.** Ceftriaxone 2 kali sehari, jam 08.00 dan 20.00. MAR dibuka pukul 07.00 → terbentuk dosis
08.00 dan 20.00. MAR dibuka lagi pukul 07.05 → **tidak ada** dosis tambahan.

**Acceptance criteria.**

1. Tabel MAR, revisi dosis, jadwal, dan pengaturan ada di `PharmacyManagement`.
2. Dosis `Due` terbentuk dari butir resep aktif **berjadwal** — `FR-KEP-064`.
3. Pembentukan **idempoten**: dua pemanggilan berturut-turut menghasilkan himpunan dosis yang sama.
4. Pembentukan berjalan saat MAR dibuka **dan** secara terjadwal lewat hosted service.
5. Jadwal jam per frekuensi dan pengaturan MAR dikonfigurasi Farmasi — `FR-KEP-071`.
6. Frekuensi yang **tidak punya jadwal terkonfigurasi** tetap terlihat, tidak disembunyikan.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; **uji idempoten** dua pemanggilan dengan
keluarannya ditempel apa adanya.

**Catatan lintas sub-modul.** Task ini adalah prasyarat `BE-RWI-100` [BE-DOK] dan `BE-RWI-087`
[BE-INP]. Keduanya tidak boleh dimulai sebelum task ini mendarat.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-115` — Pencatatan pemberian obat

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 3 — `KEP-V2-2` |

**Bisnis prosesnya.** Perawat mencatat apa yang terjadi pada setiap dosis: diberikan, ditahan,
ditolak pasien, atau terlewat. Yang tidak boleh terjadi: dosis **dihapus**. Rekam pemberian obat
adalah catatan hukum; koreksi dilakukan dengan menyimpan revisi, bukan menimpa.

**Acceptance criteria.**

1. Dosis dapat dicatat `Administered`, `Held`, `Refused`, atau `Missed` dengan syarat isian dan alasan sesuai `VAL-KEP-30`.
2. Pelaksana diambil dari **akun login**, bukan dari kiriman.
3. Kiriman ulang **tidak menggandakan** pencatatan — `FR-KEP-065`.
4. PRN mencatat indikasi dan evaluasi; butir tanpa jadwal terkonfigurasi dicatat beralasan — `FR-KEP-067`.
5. Koreksi dosis wajib beralasan dan **menyimpan revisi** — `FR-KEP-068`.
6. Dosis **tidak pernah dihapus** dari tabel.
7. Catatan penyimpangan tersimpan ketika pencatatan berbeda dari jadwal.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API bagian 7.11; uji kiriman ulang dengan
keluaran ditempel.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-116` — Cek ganda obat high-alert

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 4 — `KEP-V2-2` |

**Bisnis prosesnya.** Obat high-alert — insulin, antikoagulan, elektrolit pekat — adalah obat yang
kesalahannya berakibat serius. Standar keselamatan menuntut **perawat kedua** memeriksa sebelum obat
diberikan. Sistem menegakkannya: dosis high-alert tidak bisa berstatus `Administered` sebelum
konfirmasi perawat kedua masuk.

**Acceptance criteria.**

1. Dosis obat high-alert berstatus cek ganda `Pending` dan **tidak dapat** langsung `Administered` — `VAL-KEP-31`.
2. Perawat kedua **tidak boleh** orang yang sama dengan pencatatnya.
3. Cek ganda dapat `Confirmed` atau `Rejected`; `Rejected` menghalangi pemberian.
4. Obat yang bukan high-alert berstatus `NotRequired` dan tidak terpengaruh.
5. Daftar tunggu cek ganda dapat dibaca per unit layanan.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis termasuk percobaan konfirmasi oleh
orang yang sama.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-117` — Dugaan reaksi obat sebagai alergi tertaut (migration K6)

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 4 — `KEP-V2-2` |
| **Migration** | `K6` — dua kolom `TrxPatientAllergy` |

**Bisnis prosesnya.** Perawat yang melihat reaksi setelah pemberian obat perlu mencatatnya di tempat
yang akan terbaca saat obat itu diresepkan lagi — yaitu daftar alergi pasien. Catatan itu berstatus
`Suspected`, bukan `Confirmed`, karena yang mengkonfirmasi alergi adalah dokter.

**Acceptance criteria.**

1. Dua kolom baru ada pada `TrxPatientAllergy`.
2. Dugaan reaksi dicatat **dari dosis**, dan alergi yang lahir tertaut ke dosis itu — `FR-KEP-070`.
3. Pencatatan ini **tidak mengubah** status maupun isi dosis MAR-nya.
4. Alergi yang lahir berstatus `Suspected`.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; verifikasi proses bisnis yang membuktikan
MAR tidak berubah.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-118` — Penghentian butir dan penutupan episode membatalkan dosis

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 3 — `KEP-V2-2` |

**Bisnis prosesnya.** Dua kejadian membuat dosis yang akan datang tidak boleh lagi diberikan:
dokter menghentikan obatnya, dan episode ditutup. Keduanya membatalkan dosis `Due` **sesudah** waktu
kejadian, dan tidak pernah menyentuh dosis yang sudah tercatat.

**Acceptance criteria.**

1. Penghentian butir resep membatalkan dosis `Due` sesudahnya — `INT-KEP-09`.
2. Penutupan episode membatalkan dosis `Due` masa depan — `INT-KEP-15`.
3. Dosis `Administered`, `Held`, `Refused`, `Missed` tidak disentuh pada kedua jalur.
4. Keduanya berjalan **di dalam transaksi** pemicunya; galat buatan → nol perubahan.

**Bukti verifikasi.** `dotnet build`; verifikasi proses bisnis; uji galat buatan.

**Catatan lintas sub-modul.** Task ini adalah sisi keperawatan dari `BE-RWI-100` [BE-DOK] dan
`BE-RWI-087` [BE-INP]. Ketiganya menyentuh mesin yang sama; urutan pengerjaannya dikoordinasikan
supaya tidak saling menimpa.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-119` — Cairan, gula darah, dan observasi harian (migration K5)

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 3 — `KEP-V2-2` |
| **Migration** | `K5` — tabel cairan, gula darah, observasi, shift beserta revisi; FK ke MAR |

**Bisnis prosesnya.** Pengawasan Harian mengumpulkan apa yang diukur perawat sepanjang hari: cairan
masuk dan keluar, gula darah, diet, mobilisasi, lingkar perut, agitasi. Setiap entri punya sumber,
volume, waktu, dan pelaksana — supaya balance cairannya bisa dipertanggungjawabkan.

**Yang paling menentukan keselamatan.** GDS bangsal disimpan di **satu tempat** dengan **satuan
wajib tanpa nilai bawaan**. `RWI-DEC-150` bagian b mengadopsi usulan `G-25`: satuan disimpan
eksplisit, dan pencocokan **menolak** satuan yang berbeda — tanpa konversi diam-diam. Alasannya
langsung: dosis insulin dihitung dari angka ini, dan mg/dL keliru dibaca sebagai mmol/L adalah
kesalahan berlipat delapan belas.

**Acceptance criteria.**

1. Tabel cairan, gula darah, observasi, dan shift ada di `ClinicalManagement`, beserta tabel revisinya.
2. Entri cairan masuk dan keluar bersumber, bervolume **ml**, berwaktu, dan berpelaksana — `FR-KEP-057`.
3. Pembatalan dan koreksi entri wajib beralasan dan **menyimpan nilai lama**.
4. GDS bangsal tersimpan **satu tempat** dengan satuan **wajib tanpa bawaan** — `FR-KEP-061`.
5. Diet, mobilisasi, lingkar perut, dan agitasi tercatat terstruktur — `FR-KEP-062`.
6. Entri berstatus `Active` atau `Cancelled` beserta nomor revisi — state 5.4.
7. Migration mundur dilarang tanpa ekspor bila tabel sudah berisi data pasien sungguhan.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; verifikasi kontrak API.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-120` — Balance cairan per shift dan 24 jam

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 4 — `KEP-V2-2` |

**Bisnis prosesnya.** Balance cairan adalah selisih masuk dan keluar. Ia dibaca per shift dan per 24
jam. Jam shift berbeda antar unit, sehingga dikonfigurasi — bawaan rumah sakit, dan boleh ditimpa per
unit.

**Acceptance criteria.**

1. Balance dihitung dari entri **aktif** saja; entri `Cancelled` tidak ikut — `FR-KEP-059`.
2. Balance tersedia per shift dan per 24 jam.
3. Unit **tanpa** konfigurasi shift hanya mendapat balance 24 jam, bukan shift buatan.
4. Jam shift dikonfigurasi per unit atau memakai bawaan — `FR-KEP-060`.
5. Jam shift **tidak mempengaruhi kewenangan** siapa pun — `AC-KEP-093`.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API bagian 7.5.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-121` — Tanda vital per episode

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 4 — `KEP-V2-2` |

**Bisnis prosesnya.** Tanda vital selama ini tidak menyimpan episode, sehingga tidak bisa ditampilkan
sebagai deret perawatan. Menyimpan episode membuat perawat dan dokter bisa melihat perkembangannya
sepanjang rawat inap, bukan sebagai angka lepas.

**Acceptance criteria.**

1. `TrxPatientVitalSign` menyimpan episode rawat inap — `FR-KEP-056`.
2. Tanda vital dapat dibaca sebagai deret per episode, terurut waktu.
3. Data grafik tersedia dari kontrak yang sama, tidak dihitung ulang frontend.
4. Tanda vital lama tanpa episode tetap terbaca pada jalurnya yang lama.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; verifikasi kontrak API.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-122` — Intake obat tertaut dosis MAR

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 4 — `KEP-V2-2` |

**Bisnis prosesnya.** Obat yang masuk lewat infus ikut menambah cairan masuk pasien. Supaya tidak
dihitung dua kali dan tidak hilang, entri intake obat **menunjuk tepat satu dosis MAR yang berstatus
`Administered`** — dosis yang benar-benar diberikan, bukan yang dijadwalkan. `RWI-DEC-149`
menetapkan volumenya **diketik** perawat sebagai volume aktual, termasuk pelarutnya.

**Dua usulan yang diadopsi `RWI-DEC-150`.** `G-26`: entri yang dosisnya kemudian dikoreksi
**ditandai, tidak diubah**. `G-27`: dosis yang sudah `Administered` tetapi belum punya entri intake
memunculkan **pengingat**, bukan kewajiban — karena tidak semua obat masuk lewat cairan.

**Acceptance criteria.**

1. Entri intake obat wajib menunjuk **tepat satu** dosis MAR berstatus `Administered` — `VAL-KEP-24d`–`g`.
2. Volume diketik perawat sebagai volume aktual, termasuk pelarut — `FR-KEP-058`.
3. Satu dosis tidak dapat ditunjuk oleh dua entri intake.
4. Dosis `Administered` tanpa entri intake memunculkan **pengingat**, tanpa mewajibkan — `FR-KEP-063`.
5. Entri yang dosisnya dikoreksi **ditandai**, nilainya tidak diubah otomatis.
6. Entri intake obat ikut dihitung pada balance cairan `BE-RWI-120`.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis untuk keenam
kriteria.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-123` — Pelaksanaan sliding scale (migration K7)

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 4 — `KEP-V2-2` |
| **Migration** | `K7` — tabel pelaksanaan; milik `PharmacyManagement`; **setelah** `R6` `dokter-rawat-inap` |

**Bisnis prosesnya.** Ini adalah **task paling berbahaya pada roadmap ini**. Perawat mengukur gula
darah, sistem mencari rentang pada order pasien, dan hasilnya adalah dosis insulin. Salah pada
langkah mana pun berarti dosis insulin yang salah.

Karena itu tiga penjagaan ditegakkan sekaligus: pelaksanaan **ditolak** tanpa order aktif; dosis
**hanya** dihitung dari GDS bangsal yang **satuannya sama** dengan protokol; dan GDS, dosis MAR,
serta catatan pelaksanaan tersimpan dalam **satu transaksi idempoten** sehingga tidak mungkin ada
dosis tanpa GDS atau GDS tanpa dosis.

**Contoh.** Perawat mencatat GDS 280 mg/dL pukul 11.00. Order Budi menyebut rentang 250–299 → 6
unit, disesuaikan separuh karena "pasien sensitif insulin" → **3 unit**. Layar menampilkan pratinjau
itu sebelum disimpan. Setelah disimpan, dosis 3 unit muncul **sekali** di MAR.

**Acceptance criteria.**

1. Pelaksanaan ditolak bila pasien tidak punya order sliding scale aktif — `VAL-KEP-27`.
2. Dosis hanya dihitung dari **GDS bangsal**, dan hanya bila satuannya sama dengan satuan protokol — `VAL-KEP-28`, `29a`.
3. Satuan berbeda **ditolak**, tanpa konversi diam-diam.
4. GDS, dosis MAR, dan catatan pelaksanaan tersimpan dalam **satu transaksi**; dosisnya tampil **sekali** di MAR — `INT-KEP-11`.
5. Transaksi **idempoten**: kiriman ulang tidak menggandakan dosis.
6. Pratinjau rentang dan dosis tersedia **sebelum** menyimpan; pengecualian dosis wajib beralasan — `FR-KEP-075`.
7. Hasil yang jatuh pada rentang **0 unit** mencatat dosis `Held` beralasan — `FR-KEP-076`, usulan `G-22` yang diadopsi `RWI-DEC-150`.

**Bukti verifikasi.** `dotnet build`; verifikasi skema; verifikasi proses bisnis untuk ketujuh
kriteria; uji galat buatan pada setiap langkah transaksi.

**Gerbang produksi.** Sliding scale **tidak boleh dipakai pada pasien sungguhan** sebelum pemilik
klinis pengesah isi protokol ditunjuk — `04-prd-to-mvp.md` 22.20 nomor 13 pada `dokter-rawat-inap`.
Gerbang itu bukan milik task ini untuk ditutup, tetapi wajib dicatat pada laporannya.

**Definition of Done.** Laporan tracked ada dan menyebut gerbang produksi yang masih terbuka;
roadmap dan traceability diperbarui.

---

### `BE-RWI-124` — SOAP dan catatan keperawatan sebagai CPPT berjenis

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1 — `KEP-V2-3`, menunggu `BE-RWI-094` [BE-DOK] |

**Bisnis prosesnya.** SOAP keperawatan dan Catatan Keperawatan bukan tabel baru — keduanya masuk ke
CPPT yang sama dengan catatan dokter, dibedakan lewat kolom `NoteKind` yang dibuat `BE-RWI-094`.
Itulah arti "catatan perkembangan pasien **terintegrasi**": satu lini masa, banyak profesi.

**Acceptance criteria.**

1. SOAP keperawatan tersimpan sebagai CPPT berjenis `NursingSoap` — `RWI-AC-204`.
2. Catatan Keperawatan tersimpan sebagai CPPT berjenis `NursingNarrative` — `RWI-AC-205`.
3. Menu SOAP dan menu Catatan Keperawatan masing-masing **menyaring jenisnya sendiri**.
4. Jenis yang dikirim diperiksa terhadap profesi penulis — perawat tidak dapat menyimpan jenis milik dokter.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API.

**Catatan.** Enum jenis catatan dikunci pada `BE-RWI-094` milik `dokter-rawat-inap`. Task ini
**memakainya**, tidak menambah nilai enum sendiri.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-125` — Obat bawaan dan pesanan tindakan dari sisi perawat

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1 — `KEP-V2-3`, menunggu `BE-RWI-101` dan `BE-RWI-097` [BE-DOK] |

**Bisnis prosesnya.** Perawatlah yang biasanya mendata obat bawaan pasien saat masuk, dan perawat
pula yang memasukkan pesanan tindakan atas instruksi dokter. Yang perawat **tidak** lakukan adalah
memutuskan nasib obat bawaan itu — keputusan Lanjut atau Hentikan tetap milik dokter.

**Acceptance criteria.**

1. Perawat dapat mencatat obat bawaan pasien — `FR-KEP-078`.
2. Perawat **membaca** keputusan rekonsiliasi per obat, dan tidak dapat mengubahnya (`403`).
3. Perawat membuat pesanan tindakan dengan dokter pemberi instruksi yang sedang bertugas — `FR-KEP-079`, `INT-DOK-19`.
4. Penginput pesanan diambil dari akun login perawat.
5. Pesanan yang dibuat perawat muncul pada daftar verifikasi instruksi dokter yang bersangkutan.

**Bukti verifikasi.** `dotnet build`; verifikasi kontrak API; verifikasi proses bisnis termasuk
percobaan perawat mengubah keputusan rekonsiliasi.

**Definition of Done.** Laporan tracked ada; roadmap dan traceability diperbarui.

---

### `BE-RWI-126` ⛔ — Ringkasan tagihan pasien baca-saja

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR** — menunggu kontrak Billing; `RWI-OQ-053` pemilik `BillingManagement` **belum bernama** |
| **Gelombang** | Tidak diberi nomor gelombang selama `{GATE-BILLING}` terbuka |

**Bisnis prosesnya.** Perawat atau petugas tertentu kadang perlu tahu gambaran tagihan pasien —
misalnya untuk mengarahkan keluarga ke bagian keuangan. Yang **tidak** boleh terbuka adalah harga per
item, karena itu bukan informasi yang tepat disampaikan dari sisi tempat tidur.

**Acceptance criteria.**

1. Ringkasan tagihan **baca-saja**, tanpa harga per item — `FR-KEP-082`.
2. Hanya pemegang hak `PatientBillingSummary : Read` yang dapat membacanya.
3. Selama kontrak Billing belum ada, permukaannya menampilkan "belum tersedia" **tanpa data tiruan**.
4. Tidak ada jalur tulis apa pun ke data tagihan dari sub-modul ini.

**Blocker.** `{GATE-BILLING}` — kontrak Billing belum disetujui, dan `RWI-OQ-053` mencatat bahwa
**pemilik `BillingManagement` belum bernama**. Task ini tidak boleh dimulai sebelum keduanya beres.

**Definition of Done.** Kontrak Billing disetujui dan pemiliknya bernama; laporan tracked ada;
roadmap dan traceability diperbarui.

---

## Kebijakan verifikasi backend

Mengikuti `rules/backend/TEST_POLICY.md`:

- Backend **tidak memelihara project automated test**. Tidak adanya automated test **bukan** coverage gap dan tidak pernah menahan `✅`.
- Roadmap ini karena itu **tidak memuat** satu pun task "write unit tests", "integration tests", maupun pembuatan test project.
- Kata "regresi" berarti **verifikasi proses bisnis yang dijalankan sungguhan**, bukan automated test suite.
- Bukti yang dipetakan: QBE preflight dan conformance, review diff dan scope, `dotnet restore` serta `dotnet build`, verifikasi kontrak API, verifikasi proses bisnis, dan verifikasi runtime pada Postgres sekali pakai untuk sepuluh task bermigration.
- Kesesuaian QBE dan aturan engineering diselesaikan **pada waktu eksekusi** dari `AGENTS.md` repository backend target.

---

## Gerbang yang masih terbuka

| Gerbang | Jenis | Menahan | Siapa yang membukanya |
| --- | --- | --- | --- |
| `{GATE-BILLING}` — kontrak Billing | Kontrak eksternal | **Mulai** `BE-RWI-126` | Pemilik `BillingManagement` — `RWI-OQ-053`, **belum bernama** |
| Pemberitahuan pemilik `rawat-jalan` atas `K2` | Koordinasi lintas modul | **Rilis** `BE-RWI-109` | Pemilik blueprint `rawat-jalan` |
| Pengesah isi protokol sliding scale | **Gerbang produksi** | Pemakaian `BE-RWI-123` pada pasien sungguhan | Manajemen rumah sakit |
| Jenis dokumen `14` untuk addendum Evaluasi Awal — `INT-KEP-12` | Dependency yang diketahui | Jalur addendum `BE-RWI-113`; **tidak** menahan task-nya | Pemilik `MedicalRecordManagement` |
| `BE-RWI-094`, `097`, `101`, `103` [BE-DOK] | Dependency lintas sub-modul | `BE-RWI-123`, `124`, `125` | Roadmap `dokter-rawat-inap` |
| Handover shift dan transfusi | `DEFERRED` | Tidak ada task pada revision `7` | Muhammad Hamzah — gate `1.6` bagian 15 |

---

## Traceability

Tabel penuh ada di [`requirement-traceability-v2.md`](./requirement-traceability-v2.md).
Empat puluh delapan FR revision `7` (`FR-KEP-035` s.d. `FR-KEP-082`) dipetakan di sana.
