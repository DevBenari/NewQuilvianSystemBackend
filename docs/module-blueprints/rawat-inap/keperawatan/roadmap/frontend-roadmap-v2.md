# Roadmap Delivery Frontend V2 — Sub-modul Keperawatan Rawat Inap

> ## Berkas ini **baru**, dan **tidak menggantikan** `frontend-roadmap.md`
>
> | Hal | `frontend-roadmap.md` (lama) | **`frontend-roadmap-v2.md` (berkas ini)** |
> | --- | --- | --- |
> | Isinya | Task revision `2` s.d. `6`, termasuk `FE-RWI-051` s.d. `056` yang sudah `✅` | **Hanya** task penyelarasan `PRD-RWI-V2-001` revision `7` |
> | Rentang task ID | `FE-RWI-045` s.d. `FE-RWI-062` | **`FE-RWI-081` s.d. `FE-RWI-094`** |
> | Statusnya | **Tetap berlaku** sebagai register task lama | Register task baru |
> | Kenapa dipisah | Permintaan pemilik 16 September 2026 | — |
>
> `FE-RWI-063` s.d. `FE-RWI-066` dipakai `episode-rawat-inap`, dan `FE-RWI-067` s.d. `FE-RWI-080`
> dipakai `dokter-rawat-inap`. **Nomor task tidak pernah dipakai ulang.**
>
> Berkas lama: [`frontend-roadmap.md`](./frontend-roadmap.md) — jangan menambah task baru di sana.

## Metadata

```yaml
module_id: rawat-inap
blueprint_id: RWI-BP-001
blueprint_revision: 7
blueprint_shape: COMPOSITE
submodule: keperawatan
blueprint_root: docs/module-blueprints/rawat-inap/keperawatan/
roadmap_file: roadmap/frontend-roadmap-v2.md
roadmap_revision: 1
status: APPROVED
roadmap_mode: DELIVERY
realignment_phase: RLN-PH-07
approval_gate: BLUEPRINT_APPROVED
approved_by: "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061)"
approved_at: "2026-09-16"
approval_decision: RWI-DEC-150
gate_closure_decision: RWI-DEC-151   # {GATE-YOGA} tertutup 2026-09-16
contract_version: 0.5.0
frontend_repo: QuilvianSystemFrontendDev
frontend_branch: HamzahV2
frontend_source_sha: 1ce219b40f8e411f3c4e66975626ab33ae81616a
backend_source_sha: df3679c0d5b2f08106702153eb242d3a6cb2929b
task_id_range: FE-RWI-081..FE-RWI-094
task_id_next_free: FE-RWI-095
stack: "Next.js App Router, JavaScript/JSX, Redux, Axios, design token dan base component Quilvian"
test_policy: "rules/frontend/test-policy.md — menulis test baru opsional; lint dan build wajib"
write_authority: "TIDAK diberikan di sini. Wewenang tulis frontend dinyatakan terpisah per task"
```

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan Definition of Done sudah terbukti, laporan tracked-nya ada |
| 🟡 | Source-nya sudah ada tetapi kriterianya belum terbukti penuh |
| ⛔ | Prasyaratnya belum terpenuhi; nama blocker-nya disebut |
| tanpa tanda | Belum disentuh sama sekali |

---

## Layout dipertahankan, isinya yang berubah

Berbeda dari `dokter-rawat-inap` yang ruang kerjanya dibangun ulang, **layout V2 keperawatan
dipertahankan**. `PRD-RWI-V2-001` hanya menuntut isinya mengikuti kemampuan V1 dalam delapan menu.
Karena itu roadmap ini **tidak tertahan** persetujuan ekstraksi komponen `rawat-jalan` — sepuluh dari
empat belas task-nya boleh jalan begitu backend-nya mendarat.

---

## Grafik Urutan Dependency

Roadmap ini memuat 14 task dengan 27 pasangan dependency. Grafiknya dipecah per kelompok layar.

### Legenda label asal

- `[BE]` = task backend pada [`backend-roadmap-v2.md`](./backend-roadmap-v2.md) sub-modul ini
- `[A1]`, `[A2]` = task frontend roadmap ini yang digambar **penuh** pada blok grafik itu
- `{...}` = gerbang yang menahan, bukan task

Node berlabel adalah **cermin baca-saja** dan boleh muncul lebih dari sekali. Setiap task milik
roadmap ini digambar **penuh tepat satu kali**.

### Blok A1 — kerangka ruang kerja

```text
BE-RWI-106 [BE] ─> FE-RWI-081
```

Satu pasangan.

### Blok A2 — Pengkajian Pasien dan penggambar formulir

```text
BE-RWI-112 [BE] ─┬─> FE-RWI-082
                 │
FE-RWI-081 [A1] ─┘

BE-RWI-107 [BE] ─┐
                 │
BE-RWI-109 [BE] ─┴─┬─> FE-RWI-083
                   │
FE-RWI-082 [A2] ───┘
```

Lima pasangan.

### Blok A3 — Pengawasan Harian, Evaluasi Awal, Asuhan

```text
BE-RWI-119 [BE] ─┐
                 │
BE-RWI-120 [BE] ─┴─┬─> FE-RWI-084
                   │
FE-RWI-081 [A1] ───┘

BE-RWI-113 [BE] ─┬─> FE-RWI-085
                 │
FE-RWI-081 [A1] ─┘

BE-RWI-124 [BE] ─┐
                 │
BE-RWI-121 [BE] ─┴─┬─> FE-RWI-086
                   │
FE-RWI-081 [A1] ───┘
```

Delapan pasangan.

### Blok A4 — Obat & Alkes, Tindakan, Transfer

```text
BE-RWI-115 [BE] ─┐
                 │
BE-RWI-123 [BE] ─┴─┐
                   │
BE-RWI-125 [BE] ───┴─┬─> FE-RWI-087
                     │
FE-RWI-081 [A1] ─────┘

BE-RWI-125 [BE] ─┬─> FE-RWI-089
                 │
FE-RWI-081 [A1] ─┘

FE-RWI-081 [A1] ─> FE-RWI-090
```

Tujuh pasangan.

### Blok B — layar berdiri sendiri dan konfigurasi

```text
BE-RWI-116 [BE] ─> FE-RWI-088

BE-RWI-107 [BE] ─┬─> FE-RWI-091
                 │
BE-RWI-108 [BE] ─┘

BE-RWI-120 [BE] ─> FE-RWI-092

BE-RWI-114 [BE] ─> FE-RWI-093

BE-RWI-126 [BE] ─> FE-RWI-094
```

Enam pasangan.

**Jumlah pasangan seluruh grafik: 1 + 5 + 8 + 7 + 6 = 27.** Jumlah entri kolom `Dependency` pada
tabel task: **27**. Keduanya cocok.

### Tabel gelombang eksekusi

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-RWI-106` [BE] mendarat | `FE-RWI-081` |
| 1 | `BE-RWI-116` [BE] mendarat | `FE-RWI-088` |
| 1 | `BE-RWI-107` dan `BE-RWI-108` [BE] mendarat | `FE-RWI-091` |
| 1 | `BE-RWI-120` [BE] mendarat | `FE-RWI-092` |
| 1 | `BE-RWI-114` [BE] mendarat | `FE-RWI-093` |
| 2 | `FE-RWI-081` dan `BE-RWI-112` [BE] | `FE-RWI-082` |
| 2 | `FE-RWI-081` dan `BE-RWI-119`, `BE-RWI-120` [BE] | `FE-RWI-084` |
| 2 | `FE-RWI-081` dan `BE-RWI-113` [BE] | `FE-RWI-085` |
| 2 | `FE-RWI-081` dan `BE-RWI-124`, `BE-RWI-121` [BE] | `FE-RWI-086` |
| 2 | `FE-RWI-081` dan `BE-RWI-115`, `123`, `125` [BE] | `FE-RWI-087` |
| 2 | `FE-RWI-081` dan `BE-RWI-125` [BE] | `FE-RWI-089` |
| 2 | `FE-RWI-081` | `FE-RWI-090` |
| 3 | `FE-RWI-082` dan `BE-RWI-107`, `BE-RWI-109` [BE] | `FE-RWI-083` |
| — | ⛔ menunggu `{GATE-BILLING}` lewat `BE-RWI-126` [BE] | `FE-RWI-094` |

---

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-081` | Perawat membuka satu ruang kerja berisi delapan menu | `FR-KEP-035`, `036`, `037`; `RWI-DEC-108`, `113` | `0.5.0` | `FE-KEP-01` — **layout dipertahankan** | `FE-KEP-07` — navigasi kiri delapan menu, tab sekunder, `?section=` dan `?tab=` menyimpan posisi | `BE-RWI-106` [BE] | AC-1 s.d. AC-6 | `npm run lint`, `npm run build`, verifikasi manual + panel Network | Menu tanpa backend **tidak boleh** memanggil jaringan / Muhammad Hamzah | Kartu `FE-RWI-081` |
| `FE-RWI-082` | Perawat melihat berapa bagian pengkajian yang sudah beres | `FR-KEP-050`, `051`, `052`; `RWI-DEC-119`, `120` | `0.5.0` API 7.1 | `FE-KEP-02`, `FE-KEP-03` | `FE-KEP-08` — progres lima bagian ✓/!/○ dan tujuh sub-menu | `FE-RWI-081`, `BE-RWI-112` [BE] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | Progres gagal dimuat **wajib** tampil galat, bukan ○ / Muhammad Hamzah | Kartu `FE-RWI-082` |
| `FE-RWI-083` | Formulir klinis digambar dari definisi versi, bukan dari kode layar | `FR-KEP-039`, `043`, `044`, `045`, `047`, `048` | `0.5.0` data 11.4–11.6 | — (layar baru) | `FE-KEP-09` — **satu penggambar** untuk Kajian Umum, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi, Perencanaan Pulang | `FE-RWI-082`, `BE-RWI-107` [BE], `BE-RWI-109` [BE] | AC-1 s.d. AC-7 | `npm run lint`, `npm run build`, verifikasi manual; **test unit direkomendasikan** | Skor dihitung server — frontend **tidak** menghitung ulang / komite keperawatan | Kartu `FE-RWI-083` |
| `FE-RWI-084` | Perawat mencatat dan membaca pengawasan satu hari dalam satu layar | `FR-KEP-056` s.d. `063`; `RWI-DEC-148`, `149` | `0.5.0` data 11.8–11.10, API 7.5 | — (layar baru) | `FE-KEP-10` — tanda vital, nyeri, intake, output, balance, GDS, diet dan mobilisasi | `FE-RWI-081`, `BE-RWI-119` [BE], `BE-RWI-120` [BE] | AC-1 s.d. AC-7 | `npm run lint`, `npm run build`, verifikasi manual | Satuan GDS **tanpa bawaan** — salah satuan = salah dosis insulin / Muhammad Hamzah | Kartu `FE-RWI-084` |
| `FE-RWI-085` | MPP mengisi Evaluasi Awal delapan bagian | `FR-KEP-053`, `054`, `055`; `RWI-DEC-115`, `140` | `0.5.0` data 11.7 | — (layar baru) | `FE-KEP-11` — delapan bagian checklist; MPP menulis, lainnya membaca | `FE-RWI-081`, `BE-RWI-113` [BE] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | Jalur addendum belum ada sampai jenis dokumen `14` tersedia / Muhammad Hamzah | Kartu `FE-RWI-085` |
| `FE-RWI-086` | Perawat menulis asuhan dan catatan dalam satu tempat | `FR-KEP-077`; `FR-KEP-056`; `RWI-DEC-113`, `114` | `0.5.0` + `0.6.0` [DOK] | `FE-KEP-04`, `FE-KEP-05` | `FE-KEP-12` — Vital Sign, SOAP, Catatan Terintegrasi, Tindakan Harian, Obat & Alkes, Catatan Keperawatan, Rencana Asuhan | `FE-RWI-081`, `BE-RWI-124` [BE], `BE-RWI-121` [BE] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | SOAP perawat masuk CPPT yang sama dengan dokter / Muhammad Hamzah | Kartu `FE-RWI-086` |
| `FE-RWI-087` | Perawat memberikan obat dari daftar yang benar | `FR-KEP-064` s.d. `076`, `078`; `RWI-DEC-116`, `117`, `145` s.d. `148` | `0.5.0` state 5.5–5.6 | — (layar baru) | `FE-KEP-13` — Pemberian Obat (MAR), Sliding Scale, Obat Bawaan, Resep Aktif, Pemakaian Alkes | `FE-RWI-081`, `BE-RWI-115` [BE], `BE-RWI-123` [BE], `BE-RWI-125` [BE] | AC-1 s.d. AC-8 | `npm run lint`, `npm run build`, verifikasi manual; **test unit direkomendasikan** | **Layar paling berbahaya di roadmap ini** — salah tampil dosis / pemilik klinis belum ditunjuk | Kartu `FE-RWI-087` |
| `FE-RWI-088` | Perawat kedua menemukan dosis yang menunggu konfirmasinya | `FR-KEP-066`; `RWI-DEC-117` | `0.5.0` API double-check | — (layar baru) | `FE-KEP-22` — daftar tunggu cek ganda per unit; kartu keenam pada `FE-INP-09` | `BE-RWI-116` [BE] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | Perawat kedua tidak boleh orang yang sama / Muhammad Hamzah | Kartu `FE-RWI-088` |
| `FE-RWI-089` | Perawat memesan tindakan dan membaca hasil penunjang | `FR-KEP-079`, `FR-KEP-080`; `RWI-DEC-113` | `0.6.0` [DOK] `INT-DOK-19` | — (layar baru) | `FE-KEP-14` Tindakan dan `FE-KEP-15` Penunjang Medis enam kartu | `FE-RWI-081`, `BE-RWI-125` [BE] | AC-1 s.d. AC-6 | `npm run lint`, `npm run build`, verifikasi manual | Pesanan Lab/Rad perawat menunggu `{GATE-LABRAD}` / pemilik Lab/Rad | Kartu `FE-RWI-089` |
| `FE-RWI-090` | Perawat memindahkan pasien dan melihat permukaan yang belum terintegrasi | `FR-KEP-081`, `FR-KEP-036`; `RWI-DEC-113` | `CAP-017` yang sudah ada | Perpindahan tempat tidur `CAP-017` | `FE-KEP-16` Transfer Pasien dan `FE-KEP-17` permukaan "Integrasi belum tersedia" | `FE-RWI-081` | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual + panel Network | Serah terima klinis **`DEFERRED`**, bukan dibuat setengah jadi / Muhammad Hamzah | Kartu `FE-RWI-090` |
| `FE-RWI-091` | Komite keperawatan mengelola instrumen tanpa mengubah kode | `FR-KEP-039`, `040`, `041`, `042`; gate `G-06` | `0.5.0` state 5.1 | — (layar baru) | `FE-KEP-19` — butir menu **baru** di grup Master Data; kelola versi, uji hitung, sahkan | `BE-RWI-107` [BE], `BE-RWI-108` [BE] | AC-1 s.d. AC-6 | `npm run lint`, `npm run build`, verifikasi manual; **test unit direkomendasikan** | Pita bertumpuk atau berlubang wajib terlihat sebelum simpan / komite keperawatan | Kartu `FE-RWI-091` |
| `FE-RWI-092` | Jam shift dikonfigurasi per unit | `FR-KEP-060`; `AC-KEP-093` | `0.5.0` API nursing-shifts | — (layar baru) | `FE-KEP-20` — butir menu **baru** di grup Master Data; tabel shift + garis waktu 24 jam | `BE-RWI-120` [BE] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | Celah dan tumpang tindih wajib terlihat sebelum simpan / Muhammad Hamzah | Kartu `FE-RWI-092` |
| `FE-RWI-093` | Farmasi mengatur jam pemberian obat per frekuensi | `FR-KEP-071`; gate `G-12` | `0.5.0` API 7.13 | — (layar baru) | `FE-KEP-21` — butir menu **baru** di grup Farmasi; Jadwal per frekuensi, Pengaturan MAR, Frekuensi tanpa jadwal | `BE-RWI-114` [BE] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | Frekuensi tanpa jadwal **wajib terlihat**, bukan disembunyikan / apoteker | Kartu `FE-RWI-093` |
| `FE-RWI-094` ⛔ | Ringkasan tagihan terbaca bagi pemegang hak khusus | `FR-KEP-082`; `RWI-DEC-137` | `0.5.0` API 7.14 | — (layar baru) | `FE-KEP-18` — ringkasan **baca-saja tanpa harga per item** | `BE-RWI-126` [BE] | AC-1 s.d. AC-4 | `npm run lint`, `npm run build`, verifikasi manual | ⛔ **menunggu `{GATE-BILLING}`**; `RWI-OQ-053` pemiliknya belum bernama | Kartu `FE-RWI-094` |

---

## Kartu task

### `FE-RWI-081` — `FE-KEP-07` Ruang Kerja Keperawatan V2

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1, menunggu `BE-RWI-106` [BE] |
| **Layar** | `FE-KEP-07` — rework `FE-KEP-01`, **layout dipertahankan** |
| **Jalan masuk** | Baris Census `FE-INP-01` dan tombol "Buka Ruang Kerja Keperawatan" pada Detail Episode `FE-INP-04` → route yang **sudah ada** `…/episodes/{id}/nursing` |

**Bisnis prosesnya.** Perawat bekerja pada satu pasien dalam waktu yang lama, berpindah antara
pengkajian, obat, pengawasan, dan asuhan. Ruang kerja mengumpulkan kedelapan menu itu di navigasi
kiri dengan urutan tetap, supaya perawat tidak mencari-cari.

Empat dari delapan menu belum punya backend. Menu itu **tetap ditampilkan** dengan konteks pasien
dan keterangan "Integrasi belum tersedia" — jujur tentang keadaannya, tanpa data tiruan, dan
**tanpa mengirim permintaan jaringan** ke modul yang belum ada.

**Acceptance criteria.**

1. Navigasi kiri memuat delapan menu dan tab sekundernya dengan **urutan tetap** — `FR-KEP-035`.
2. Menu yang backend-nya belum ada menampilkan konteks pasien dan "Integrasi belum tersedia", **tanpa data tiruan** — `FR-KEP-036`.
3. Menu itu menghasilkan **nol** permintaan jaringan ke modul terkait.
4. Kegagalan memuat konteks pasien **menonaktifkan seluruh tombol tulis** pada kedelapan menu — `FR-KEP-037`.
5. Parameter `?section=` dan `?tab=` menyimpan posisi sehingga tautannya dapat dibagikan.
6. Layout V2 yang sudah ada **tidak diubah bentuknya** — yang berubah isi menunya.

**Bukti verifikasi.** `npm run lint` dan `npm run build`; verifikasi manual; kriteria 3 dibuktikan
lewat panel Network peramban dan tangkapannya dilampirkan.

**Definition of Done.** Lint dan build hijau; bukti panel Network terlampir; laporan tracked ada di
`task/report/frontend/FE-RWI-081.md`; roadmap dan `requirement-traceability-v2.md` diperbarui.

---

### `FE-RWI-082` — `FE-KEP-08` Pengkajian Pasien dan progres

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 2 |
| **Layar** | `FE-KEP-08` — rework `FE-KEP-02` dan `FE-KEP-03` |

**Bisnis prosesnya.** Pengkajian pasien terdiri dari tujuh isi. Perawat perlu tahu sekilas mana yang
sudah beres. Progres menampilkannya sebagai lima bagian dengan tiga keadaan: ✓ selesai, ! perlu
perhatian, ○ belum.

**Dua hal yang tidak boleh tertukar.** Pertama, temuan berisiko tampil sebagai **alert pada kepala
konteks** — bukan menurunkan angka progres. Pengkajian yang lengkap tetap 100% walau hasilnya
mengkhawatirkan. Kedua, progres yang **gagal dimuat** menampilkan galat, **bukan** ○. Menampilkan ○
memberi tahu perawat bahwa bagian itu belum dikaji, padahal yang terjadi sistem tidak tahu — dan itu
bisa membuat pengkajian yang sudah ada diulang atau yang belum ada terlewat.

**Acceptance criteria.**

1. Tujuh sub-menu pengkajian tampil dengan urutan tetap.
2. Progres lima bagian tampil dengan keadaan ✓ / ! / ○ dan persen kelipatan 20 — `FR-KEP-050`.
3. Pengawasan Harian dan Evaluasi Awal tampil **tanpa dihitung** ke dalam persen.
4. Temuan berisiko tampil sebagai alert kepala konteks dan tidak mengubah progres — `FR-KEP-051`.
5. Progres yang gagal dimuat menampilkan **galat**, bukan ○ — `FR-KEP-052`.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual termasuk skenario progres
gagal dimuat.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-083` — `FE-KEP-09` Penggambar formulir berinstrumen

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 3 |
| **Layar** | `FE-KEP-09` — **satu penggambar** untuk lima formulir |

**Bisnis prosesnya.** Lima formulir — Kajian Umum, Resiko Jatuh, Monitoring Nyeri, Assesment Edukasi,
Perencanaan Pulang — tidak lagi ditulis satu per satu sebagai layar. Semuanya **digambar dari
definisi versi** yang datang dari backend. Artinya komite keperawatan mengubah instrumen tanpa
menunggu perubahan kode layar.

**Yang tidak boleh dilakukan frontend.** Menghitung skor sendiri. Skor dan pita dihitung server dan
disimpan bersama versinya. Frontend menampilkan hasilnya. Kalau frontend ikut menghitung, dua
perhitungan itu bisa berbeda, dan yang terbaca perawat belum tentu yang tersimpan.

**Acceptance criteria.**

1. Kelima formulir digambar dari definisi versi yang sama, lewat satu penggambar.
2. Skor dan pita **ditampilkan dari server**; frontend tidak menghitung ulang — `FR-KEP-044`.
3. Isian yang **belum dikaji** terkirim sebagai belum dikaji, bukan sebagai normal.
4. Kajian Umum menampilkan delapan bagian dengan isian pada bagian yang benar — `FR-KEP-045`.
5. Kajian Umum **menunjuk** satu baris tanda vital, tidak menyalin angkanya — `FR-KEP-046`.
6. Monitoring Nyeri tidak dapat diselesaikan sebelum keadaan nyeri terisi — `FR-KEP-048`.
7. Dokumen yang memakai versi lama tetap digambar dengan definisi versi **yang tersimpan padanya**, bukan versi terbaru.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual untuk ketujuh kriteria.
Karena penggambar berisi **logika murni** pemetaan definisi ke kontrol, **test unit
direkomendasikan** di `tests/unit/` sesuai tabel pada `rules/frontend/test-policy.md`.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-084` — `FE-KEP-10` Pengawasan Harian Pasien

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 2 |
| **Layar** | `FE-KEP-10` — layar baru |

**Bisnis prosesnya.** Layar ini adalah tempat perawat mencatat apa yang diukur sepanjang hari:
tanda vital, nyeri, cairan masuk dan keluar, balance, gula darah, diet, dan mobilisasi — semuanya
untuk satu hari, dalam satu tampilan.

**Yang paling menentukan keselamatan.** Satuan GDS **tidak punya nilai bawaan**. Perawat harus
memilihnya setiap kali mencatat. Bawaan yang diam-diam terpilih adalah cara paling mudah membuat
mg/dL tercatat sebagai mmol/L — dan angka itu yang nanti dipakai menghitung dosis insulin.

**Acceptance criteria.**

1. Tanda vital tampil sebagai deret dan grafik per episode — `FR-KEP-056`.
2. Entri cairan masuk dan keluar mewajibkan sumber, volume ml, waktu, dan pelaksana — `FR-KEP-057`.
3. Pembatalan dan koreksi entri mewajibkan alasan, dan nilai lamanya tetap terbaca.
4. Satuan GDS **wajib dipilih, tanpa bawaan** — `FR-KEP-061`.
5. Balance per shift dan 24 jam tampil dari server, tidak dihitung ulang frontend — `FR-KEP-059`.
6. Unit tanpa konfigurasi shift menampilkan balance 24 jam saja, tanpa shift buatan.
7. Diet, mobilisasi, lingkar perut, dan agitasi tercatat terstruktur — `FR-KEP-062`.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual termasuk mencoba menyimpan
GDS tanpa memilih satuan.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-085` — `FE-KEP-11` Evaluasi Awal (MPP)

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 2 |
| **Layar** | `FE-KEP-11` — layar baru |

**Bisnis prosesnya.** Manajer Pelayanan Pasien menilai kebutuhan pasien di luar sisi medis. Dokumen
ini **satu per episode** dan hidup sepanjang perawatan — bukan dokumen baru setiap kali dinilai
ulang.

**Acceptance criteria.**

1. Delapan bagian checklist tampil dari definisi versi, sama seperti formulir lain.
2. Hanya pemegang hak MPP **yang ditempatkan di unit episode** melihat kontrol tulis; perawat lain melihat baca-saja — `FR-KEP-054`.
3. Layar menampilkan **satu** Evaluasi Awal per episode; tidak ada tombol "buat baru" bila sudah ada.
4. Penolakan `403` dari server ditampilkan apa adanya.
5. Selama jenis dokumen `14` belum tersedia, jalur addendum **tidak ditampilkan** — bukan ditampilkan lalu gagal.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual dengan akun MPP dan akun
perawat biasa.

**Catatan.** Kriteria 5 adalah dependency yang diketahui (`INT-KEP-12`), dicatat pada laporan task
sebagai keadaan, bukan disembunyikan.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-086` — `FE-KEP-12` Asuhan Keperawatan

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 2 |
| **Layar** | `FE-KEP-12` — rework `FE-KEP-04` dan `FE-KEP-05` |

**Bisnis prosesnya.** Menu ini mengumpulkan pekerjaan harian perawat: tanda vital, SOAP, catatan
terintegrasi, tindakan harian, obat, catatan keperawatan, dan rencana asuhan. Yang berubah pada
revision `7`: SOAP dan Catatan Keperawatan **tidak lagi punya tabel sendiri** — keduanya masuk ke
CPPT yang sama dengan catatan dokter, dibedakan lewat jenis catatan.

**Acceptance criteria.**

1. SOAP perawat tersimpan sebagai CPPT berjenis `NursingSoap`, dan menunya menyaring jenis itu — `FR-KEP-077`.
2. Catatan Keperawatan tersimpan sebagai `NursingNarrative`, dan menunya menyaring jenis itu.
3. Catatan Terintegrasi menampilkan lini masa lintas profesi, tanpa tombol verifikasi bagi perawat.
4. Vital Sign menampilkan deret per episode dari `BE-RWI-121`.
5. Rencana Asuhan dan Tindakan Harian tetap bekerja seperti sebelumnya.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual; kriteria 1 dan 2
dibuktikan dengan memeriksa bahwa catatan yang disimpan muncul juga di tab CPPT dokter.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-087` — `FE-KEP-13` Obat & Alkes

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 2 |
| **Layar** | `FE-KEP-13` — layar baru, **paling berisiko pada roadmap ini** |

**Bisnis prosesnya.** Di layar inilah perawat melihat dosis apa yang harus diberikan, dan mencatat
apa yang sudah diberikan. Kesalahan tampilan di sini berujung pada obat yang salah masuk ke pasien.

Lima bagian: Pemberian Obat (MAR), Sliding Scale, Obat Bawaan, Resep Aktif, dan Pemakaian Alkes.

**Contoh sliding scale.** Perawat mencatat GDS 280 mg/dL. Layar menampilkan pratinjau: rentang
250–299 → 6 unit, **disesuaikan separuh** karena "pasien sensitif insulin" → **3 unit**. Angka dan
alasannya tampil bersama, sehingga tidak ada yang mengira 3 unit itu dosis standar. Setelah
disimpan, dosis itu muncul **sekali** di MAR.

**Acceptance criteria.**

1. Daftar dosis menampilkan status `Due`, `Administered`, `Held`, `Refused`, `Missed`, `Cancelled` dengan pembeda yang jelas.
2. Pencatatan dosis mewajibkan isian dan alasan sesuai statusnya — `FR-KEP-065`.
3. Dosis high-alert menampilkan penanda "Menunggu cek ganda (n)" dan **tidak dapat** ditandai `Administered` sebelum konfirmasi masuk — `FR-KEP-066`.
4. Koreksi dosis mewajibkan alasan; dosis **tidak pernah hilang** dari daftar — `FR-KEP-068`.
5. Sliding Scale menampilkan pratinjau rentang dan dosis **sebelum** menyimpan, beserta alasan penyesuaian bila ada — `FR-KEP-075`.
6. Sliding Scale menolak disimpan bila pasien tidak punya order aktif — `FR-KEP-072`.
7. Dosis hasil sliding scale muncul **sekali** di MAR, tidak dua kali — `FR-KEP-074`.
8. Obat Bawaan menampilkan keputusan rekonsiliasi **baca-saja**; perawat tidak melihat kontrol untuk mengubahnya — `FR-KEP-078`.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual untuk kedelapan kriteria.
Karena bagian sliding scale menampilkan angka hasil perhitungan, **test unit direkomendasikan**
untuk utility pemetaan rentang ke dosis bila ada di `src/utils`.

**Gerbang produksi.** Sliding scale **tidak boleh dipakai pada pasien sungguhan** sebelum pemilik
klinis pengesah isi protokol ditunjuk. Gerbang itu dicatat pada laporan task, dan bukan milik task
ini untuk ditutup.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; gerbang produksi dicatat;
laporan tracked ada; roadmap dan traceability diperbarui.

---

### `FE-RWI-088` — `FE-KEP-22` Daftar Tunggu Cek Ganda

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1, menunggu `BE-RWI-116` [BE] |
| **Layar** | `FE-KEP-22` — layar baru |
| **Jalan masuk** | Penanda "Menunggu cek ganda (n)" pada `FE-KEP-13`, dan **kartu keenam** pada Daftar Pantau `FE-INP-09` |

**Bisnis prosesnya.** Dosis high-alert menunggu perawat kedua. Kalau perawat kedua tidak tahu ada
yang menunggunya, dosisnya tertunda dan pasien terlambat mendapat obat. Daftar ini memunculkannya
per unit layanan.

**Acceptance criteria.**

1. Daftar memuat dosis yang menunggu cek ganda **di unit pengguna**, bukan seluruh rumah sakit.
2. Setiap baris menampilkan pasien, bed, obat, dosis, pencatat, dan waktu.
3. Pengguna **tidak melihat** tombol konfirmasi pada dosis yang ia sendiri catat.
4. Letak kartu pada `FE-INP-09` mengikuti urutan yang ditetapkan 12 September 2026 — kelompok `keperawatan` paling akhir, dan `FE-KEP-22` di dalamnya.
5. Daftar kosong menampilkan keadaan kosong yang wajar, bukan galat.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual dengan dua akun perawat
berbeda.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-089` — `FE-KEP-14` Tindakan dan `FE-KEP-15` Penunjang Medis

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 2 |
| **Layar** | `FE-KEP-14` dan `FE-KEP-15` — dua layar baru, satu task |

**Bisnis prosesnya.** Perawat memesan tindakan **atas instruksi dokter** — nama dokternya wajib
dipilih, dan dokter itu harus sedang bertugas. Untuk penunjang, perawat melihat pesanan dan hasil
final Laboratorium dan Radiologi; empat layanan lain berupa permukaan "Integrasi belum tersedia".

**Acceptance criteria.**

1. Form pesanan tindakan mewajibkan **dokter pemberi instruksi** yang sedang bertugas — `FR-KEP-079`.
2. Penginput diambil dari akun login perawat, tidak dapat diketik.
3. History Tindakan menampilkan status verifikasi instruksi setiap pesanan.
4. Penunjang menampilkan enam kartu; Laboratorium dan Radiologi berisi pesanan dan hasil final — `FR-KEP-080`.
5. Empat kartu lain menampilkan konteks pasien dan "Integrasi belum tersedia", **tanpa permintaan jaringan**.
6. Kontrol **memesan** Lab/Rad dari sisi perawat **tidak ditampilkan** selama `{GATE-LABRAD}` belum ditutup.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual; kriteria 5 dibuktikan lewat
panel Network.

**Catatan.** Kriteria 6 adalah pembatasan yang sengaja: pemesanan Lab/Rad oleh perawat menunggu
persetujuan pemilik `LaboratoryManagement` dan `RadiologyManagement` lewat `BE-RWI-104` [BE-DOK].

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-090` — `FE-KEP-16` Transfer Pasien dan `FE-KEP-17` permukaan

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 2 |
| **Layar** | `FE-KEP-16` dan `FE-KEP-17` — dua layar, satu task |

**Bisnis prosesnya.** Transfer Pasien memakai perpindahan tempat tidur yang **sudah ada** (`CAP-017`)
— tidak ada mesin baru. Serah Terima Klinis, yang biasanya menyertai transfer, berstatus `DEFERRED`
pada revision `7`: ia ditampilkan sebagai "belum tersedia", **bukan** dibuat setengah jadi.

**Acceptance criteria.**

1. Form dan History perpindahan tempat tidur memakai `CAP-017` yang sudah ada — `FR-KEP-081`.
2. Serah Terima Klinis tampil "belum tersedia" tanpa form dan tanpa data tiruan.
3. `FE-KEP-17` Pemakaian Alat dan Pemesanan Ruangan Bedah tampil sebagai konteks pasien + "Integrasi belum tersedia".
4. Keempat permukaan itu menghasilkan **nol** permintaan jaringan ke modul terkait.
5. Nol tabel baru dan nol endpoint baru dari task ini.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual + panel Network.

**Definition of Done.** Lint dan build hijau; bukti panel Network terlampir; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-091` — `FE-KEP-19` Instrumen & Formulir Klinis

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1, menunggu `BE-RWI-107` dan `BE-RWI-108` [BE] |
| **Layar** | `FE-KEP-19` |
| **Butir menu** | **Baru** — Pelayanan Kesehatan → **Master Data** → Instrumen & Formulir Klinis, `pathname` `/health-services/clinical-management/clinical-instruments` |
| **Hak akses** | `ClinicalInstrumentConfiguration : Read` |

**Bisnis prosesnya.** Inilah layar yang membuat angka batas klinis bisa diubah tanpa mengubah kode.
Komite keperawatan menyusun versi, mengujinya, lalu mengesahkannya.

**Yang paling menentukan.** Pita skor yang **bertumpuk atau berlubang** harus terlihat **sebelum
disimpan**. Pita bertumpuk berarti satu skor menghasilkan dua tingkat risiko; pita berlubang berarti
ada skor yang tidak menghasilkan tingkat apa pun.

**Acceptance criteria.**

1. Daftar instrumen beserta versinya, lengkap dengan status `Draft`, `Approved`, `Retired`.
2. Editor pita skor menandai tumpang tindih dan lubang **secara visual sebelum simpan** — `FR-KEP-041`.
3. Instrumen sejenis dengan rentang usia bertumpuk ditandai sebelum simpan.
4. Tombol Sahkan **tidak tersedia** bagi pengguna yang merupakan pengubah terakhir versi itu — `FR-KEP-040`.
5. "Uji hitung" menampilkan hasil dari **server**, bukan perhitungan frontend.
6. Versi `Draft` ditandai jelas sebagai belum boleh dipakai di luar lingkungan uji — `FR-KEP-042`.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual termasuk mencoba pita
bertumpuk. Karena pemeriksa pita adalah **logika murni**, **test unit direkomendasikan** di
`tests/unit/`.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-092` — `FE-KEP-20` Jam Shift Keperawatan

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1, menunggu `BE-RWI-120` [BE] |
| **Layar** | `FE-KEP-20` |
| **Butir menu** | **Baru** — Pelayanan Kesehatan → **Master Data** → Jam Shift Keperawatan, `pathname` `/health-services/clinical-management/nursing-shifts` |
| **Hak akses** | `NursingShift : Read` / `Update` |

**Bisnis prosesnya.** Jam shift menentukan bagaimana balance cairan dipotong per periode. Unit
berbeda bisa punya jam berbeda. Yang harus terlihat sebelum simpan: **celah** — jam yang tidak
masuk shift mana pun — dan **tumpang tindih**.

**Acceptance criteria.**

1. Pilihan unit atau "Bawaan", lalu tabel shift untuk pilihan itu.
2. Garis waktu 24 jam menandai **celah dan tumpang tindih** sebelum simpan.
3. Pita keterangan menjelaskan bahwa jam shift **tidak mempengaruhi kewenangan** siapa pun.
4. Unit tanpa konfigurasi sendiri jelas terbaca sebagai memakai bawaan.
5. Penolakan dari server ditampilkan apa adanya.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual termasuk membuat celah
sengaja dan memastikan terlihat.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-093` — `FE-KEP-21` Jadwal Pemberian Obat

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1, menunggu `BE-RWI-114` [BE] |
| **Layar** | `FE-KEP-21` |
| **Butir menu** | **Baru** — Pelayanan Kesehatan → **Farmasi** → Jadwal Pemberian Obat, `pathname` `/health-services/pharmacy-management/medication-schedule-settings` |
| **Hak akses** | `MedicationScheduleSetting : Read` / `Update` |

**Bisnis prosesnya.** "2 kali sehari" harus diterjemahkan menjadi jam tertentu supaya MAR bisa
membentuk dosis. Terjemahan itu kebijakan Farmasi, bukan angka di dalam kode.

**Yang harus terlihat.** Frekuensi yang **belum punya jadwal** wajib tampil pada tab tersendiri.
Menyembunyikannya berarti resep dengan frekuensi itu diam-diam tidak membentuk dosis, dan tidak ada
yang tahu sampai perawat bertanya kenapa obatnya tidak muncul.

**Acceptance criteria.**

1. Tab Jadwal per frekuensi menampilkan jam standar dan dapat disunting.
2. Tab Pengaturan MAR menampilkan pengaturan pembentukan dosis.
3. Tab "Frekuensi tanpa jadwal" menampilkan frekuensi yang belum dipetakan — `FR-KEP-071`.
4. Pita keterangan menjelaskan bahwa perubahan jadwal **tidak mengubah** dosis yang sudah terbentuk.
5. Penolakan dari server ditampilkan apa adanya.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-094` ⛔ — `FE-KEP-18` Tagihan Pasien

| Field | Isi |
| --- | --- |
| **Status** | ⛔ **TERBLOKIR** — menunggu `BE-RWI-126`, yang sendirinya menunggu `{GATE-BILLING}` |
| **Layar** | `FE-KEP-18` |
| **Hak akses** | `PatientBillingSummary : Read` |

**Bisnis prosesnya.** Ringkasan tagihan **baca-saja tanpa harga per item**, hanya bagi pemegang hak
khusus. Selama kontrak Billing belum ada, menunya tetap tampil dengan keterangan "belum tersedia" —
itu sudah dikerjakan `FE-RWI-081` kriteria 2.

**Acceptance criteria.**

1. Ringkasan tampil baca-saja, **tanpa harga per item** — `FR-KEP-082`.
2. Pengguna tanpa hak `PatientBillingSummary : Read` tidak melihat menunya sama sekali.
3. Tidak ada satu pun kontrol tulis pada layar ini.
4. Selama kontrak Billing belum ada, layar tetap menampilkan "belum tersedia" tanpa data tiruan.

**Blocker.** `{GATE-BILLING}` — kontrak Billing belum disetujui, dan `RWI-OQ-053` mencatat pemilik
`BillingManagement` **belum bernama**.

**Definition of Done.** Kontrak Billing disetujui; lint dan build hijau; verifikasi manual tercatat;
laporan tracked ada; roadmap dan traceability diperbarui.

---

## Pilihan UI yang belum disetujui

| Butir | Keadaan | Ditangani di |
| --- | --- | --- |
| Bentuk visual penanda pita bertumpuk dan berlubang | `DEV_DISCRETION` — bentuknya bebas, **kewajibannya tidak**: wajib terlihat sebelum simpan | `FE-RWI-091` |
| Bentuk visual penanda celah dan tumpang tindih garis waktu shift | `DEV_DISCRETION` dengan kewajiban yang sama | `FE-RWI-092` |
| Susunan lima bagian pada `FE-KEP-13` di layar sempit | `DEV_DISCRETION` | `FE-RWI-087` |

Pilihan `DEV_DISCRETION` dicatat pada laporan task dan **tidak** diam-diam dijadikan keputusan
produk.

---

## Kebijakan verifikasi frontend

Mengikuti `rules/frontend/test-policy.md`:

- **Menulis test baru bersifat opsional.** Tidak ada task di atas yang tertahan karena tidak menambah test.
- Validasi minimum tetap `npm run lint` dan `npm run build`, dijalankan sungguhan dan keluarannya ditempel.
- Test unit **direkomendasikan** pada tiga tempat yang berisi logika murni: penggambar formulir dari definisi (`FE-RWI-083`), pemeriksa pita skor (`FE-RWI-091`), dan pemetaan rentang ke dosis bila ada utility-nya (`FE-RWI-087`).
- `npm run test:e2e` **tidak** dijalankan kecuali task memintanya dan environment mendukung.
- Baris `AUTOMATED TEST:` dan `MANUAL TEST:` ditulis terpisah pada laporan task.

---

## Gerbang yang masih terbuka

| Gerbang | Menahan | Siapa yang membukanya |
| --- | --- | --- |
| `{GATE-BILLING}` — kontrak Billing | `FE-RWI-094` lewat `BE-RWI-126` | Pemilik `BillingManagement` — `RWI-OQ-053`, **belum bernama** |
| `{GATE-LABRAD}` — kolom instruksi Lab/Rad | Kontrol pemesanan Lab/Rad pada `FE-RWI-089` kriteria 6 | Pemilik `LaboratoryManagement` dan `RadiologyManagement` |
| Jenis dokumen `14` — `INT-KEP-12` | Jalur addendum pada `FE-RWI-085`; **tidak** menahan task-nya | Pemilik `MedicalRecordManagement` |
| Pengesah isi protokol sliding scale | **Gerbang produksi** bagi `FE-RWI-087` | Manajemen rumah sakit |
| Serah Terima Klinis | `DEFERRED` — tidak ada task pada revision `7` | Muhammad Hamzah |

---

## Traceability

Tabel penuh ada di [`requirement-traceability-v2.md`](./requirement-traceability-v2.md).
