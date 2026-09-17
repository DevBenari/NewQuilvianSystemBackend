# Roadmap Delivery Frontend V2 — Sub-modul Dokter Rawat Inap

> ## Berkas ini **baru**, dan **tidak menggantikan** `frontend-roadmap.md`
>
> | Hal | `frontend-roadmap.md` (lama) | **`frontend-roadmap-v2.md` (berkas ini)** |
> | --- | --- | --- |
> | Isinya | Task revision `0.3` s.d. `6` | **Hanya** task penyelarasan `PRD-RWI-V2-001` revision `7` |
> | Rentang task ID | `FE-RWI-039` s.d. `FE-RWI-062` | **`FE-RWI-067` s.d. `FE-RWI-080`** |
> | Statusnya | **Tetap berlaku** sebagai register task lama | Register task baru |
> | Kenapa dipisah | Permintaan pemilik 16 September 2026 | — |
>
> `FE-RWI-063` s.d. `FE-RWI-066` dipakai `episode-rawat-inap`, dan `FE-RWI-081` s.d. `FE-RWI-094`
> dipakai `keperawatan`. **Nomor task tidak pernah dipakai ulang.**
>
> Berkas lama: [`frontend-roadmap.md`](./frontend-roadmap.md) — jangan menambah task baru di sana.

## Metadata

```yaml
module_id: rawat-inap
blueprint_id: RWI-BP-001
blueprint_revision: 7
blueprint_shape: COMPOSITE
submodule: dokter-rawat-inap
blueprint_root: docs/module-blueprints/rawat-inap/dokter-rawat-inap/
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
contract_version: 0.6.0
frontend_repo: QuilvianSystemFrontendDev
frontend_branch: HamzahV2
frontend_source_sha: 1ce219b40f8e411f3c4e66975626ab33ae81616a
backend_source_sha: df3679c0d5b2f08106702153eb242d3a6cb2929b
task_id_range: FE-RWI-067..FE-RWI-080
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

## Dua kelompok layar, dua nasib berbeda

Roadmap ini memuat dua kelompok yang **tidak boleh disamakan**:

| Kelompok | Layar | Keadaannya |
| --- | --- | --- |
| **Ruang kerja** `FE-DOK-09` beserta delapan tabnya | `FE-RWI-067` s.d. `FE-RWI-076` | **Bebas sejak 2026-09-16** — `{GATE-RAJAL}` tertutup lewat `RWI-DEC-152`, pemberi persetujuan **Sukma GP** |
| **Layar berdiri sendiri** | `FE-RWI-077` s.d. `FE-RWI-080` | **Keempatnya boleh jalan** begitu backend-nya mendarat. `FE-RWI-077` lepas dari `⛔` lewat `RWI-DEC-151` |

**Diperbarui 16 September 2026 — nol task tertahan gerbang.** Dua gerbang yang dulu memisahkan kedua
kelompok ini sudah tertutup pada hari yang sama: `{GATE-YOGA}` lewat `RWI-DEC-151`, dan `{GATE-RAJAL}`
lewat `RWI-DEC-152` dengan pemberi persetujuan **Sukma GP**. Keempat belas task pada roadmap ini kini
hanya menunggu prasyarat biasa — task lain, bukan orang lain.

Pembagian dua kelompok di atas **tetap dipertahankan** karena masih menjelaskan urutan kerja: sepuluh
task ruang kerja bergantung pada kerangka `FE-RWI-067`, sedangkan empat layar berdiri sendiri tidak.

---

## Grafik Urutan Dependency

Roadmap ini memuat 14 task dengan 28 pasangan dependency, melewati batas 15 node satu grafik.
Grafiknya dipecah per kelompok layar.

### Legenda label asal

- `[BE]` = task backend pada [`backend-roadmap-v2.md`](./backend-roadmap-v2.md) sub-modul ini
- `[BE-INP]` = task backend pada `episode-rawat-inap/roadmap/backend-roadmap-v2.md`
- `[A1]`, `[A3]` = task frontend roadmap ini yang digambar penuh pada blok grafik itu
- `{...}` = gerbang yang menahan, bukan task

Node berlabel adalah **cermin baca-saja** dan boleh muncul pada lebih dari satu blok. Setiap task
milik roadmap ini digambar penuh **tepat satu kali**.

### Blok A1 — kerangka ruang kerja

```text
BE-RWI-081 [BE-INP] ─> FE-RWI-067
```

Satu pasangan. Node `{GATE-RAJAL}` dicabut 2026-09-16 setelah `RWI-DEC-152`.

### Blok A2 — tab dokumentasi

```text
FE-RWI-067 [A1] ─┬─> FE-RWI-068
                 │
BE-RWI-091 [BE] ─┘

FE-RWI-067 [A1] ─┬─> FE-RWI-070
                 │
BE-RWI-091 [BE] ─┘

BE-RWI-094 [BE] ─┐
                 │
BE-RWI-089 [BE] ─┴─┬─> FE-RWI-069
                   │
FE-RWI-067 [A1] ───┘
```

Tujuh pasangan.

### Blok A3 — tab resep dan tindakan

```text
BE-RWI-099 [BE] ─┐
                 │
BE-RWI-105 [BE] ─┴─┬─> FE-RWI-071 ─┐
                   │               │
FE-RWI-067 [A1] ───┘               │
                                   │
BE-RWI-101 [BE] ─┬─────────────────┴─> FE-RWI-072
                 │
BE-RWI-103 [BE] ─┘

BE-RWI-097 [BE] ─┐
                 │
BE-RWI-098 [BE] ─┴─┬─> FE-RWI-073
                   │
FE-RWI-067 [A1] ───┘
```

Sembilan pasangan.

### Blok A4 — tab resume, visit, penunjang

```text
BE-RWI-085 [BE-INP] ─┐
                     │
BE-RWI-086 [BE-INP] ─┴─┬─> FE-RWI-074
                       │
FE-RWI-067 [A1] ───────┘

FE-RWI-067 [A1] ─┬─> FE-RWI-075
                 │
                 └─> FE-RWI-076
```

Lima pasangan.

### Blok B — layar berdiri sendiri

```text
BE-RWI-092 [BE] ─> FE-RWI-077

BE-RWI-096 [BE] ─┬─> FE-RWI-078
                 │
BE-RWI-098 [BE] ─┘

BE-RWI-102 [BE] ─> FE-RWI-079

BE-RWI-096 [BE] ─> FE-RWI-080
```

Lima pasangan.

**Jumlah pasangan seluruh grafik: 1 + 7 + 9 + 5 + 5 = 27.** Jumlah entri kolom `Dependency` pada
tabel task: **27**. Keduanya cocok. Turun satu dari 28 sejak `RWI-DEC-152` mencabut `{GATE-RAJAL}`.

### Tabel gelombang eksekusi

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-RWI-081` [BE-INP] mendarat | `FE-RWI-067` — lepas dari `⛔` lewat `RWI-DEC-152` |
| 1 | `BE-RWI-092` [BE] mendarat | `FE-RWI-077` — lepas dari `⛔` lewat `RWI-DEC-151` |
| 1 | `BE-RWI-096` dan `BE-RWI-098` [BE] mendarat | `FE-RWI-078` |
| 1 | `BE-RWI-102` [BE] mendarat | `FE-RWI-079` |
| 1 | `BE-RWI-096` [BE] mendarat | `FE-RWI-080` |
| 2 | `FE-RWI-067` dan task backend masing-masing | `FE-RWI-068`, `069`, `070`, `071`, `073`, `074`, `075`, `076` |
| 3 | `FE-RWI-071`, `BE-RWI-101`, `BE-RWI-103` [BE] | `FE-RWI-072` |

**Lima task boleh jalan hari pertama** begitu backend-nya mendarat, delapan pada gelombang 2, dan satu
pada gelombang 3. **Nol task tertahan gerbang** — sebelumnya sebelas.

---

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-067` ✅ | Dokter membuka satu halaman berisi daftar pasiennya dan ruang kerja pasien terpilih | `FR-DOK-069` s.d. `073`; `RWI-DEC-107`, `RWI-DEC-110` | `0.6.0` API 10.1 | Komponen tata letak Dokter Rawat Jalan V2 | `FE-DOK-09` — kerangka satu halaman, daftar pasien kiri, delapan tab kanan; **nol aksi atau status antrean** | `BE-RWI-081` [BE-INP] | AC-1 s.d. AC-6 | `npm run lint` PASS, `npm run build` PASS, verifikasi manual | Disetujui Sukma GP `RWI-DEC-152` / Muhammad Hamzah | ✅ [Laporan](../task/report/frontend/FE-RWI-067.md) |
| `FE-RWI-068` ✅ | Dokter menulis SOAP dan mengoreksinya | `FR-DOK-074`, `075`, `077` | `0.6.0` state matrix | Tab SOAP `FE-DOK-03` yang sudah ada | Tab **SOAP** — Form SOAP, Riwayat SOAP, Koreksi; penulis tunggal dan penguncian | `FE-RWI-067`, `BE-RWI-091` [BE] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | Disetujui Sukma GP `RWI-DEC-152` / Muhammad Hamzah | ✅ [Laporan](../task/report/frontend/FE-RWI-068.md) |
| `FE-RWI-069` ✅ | Dokter membaca lini masa lintas profesi dan memverifikasinya | `FR-DOK-082`, `083`, `085` | `0.6.0` state matrix | Tab CPPT `FE-DOK-04` | Tab **CPPT** — saring Semua/Dokter/Perawat/Profesi Lain; tombol verifikasi hanya bagi DPJP | `FE-RWI-067`, `BE-RWI-094` [BE], `BE-RWI-089` [BE] | AC-1 s.d. AC-6 | `npm run lint`, `npm run build`, verifikasi manual | Disetujui Sukma GP `RWI-DEC-152` / Muhammad Hamzah | ✅ [Laporan](../task/report/frontend/FE-RWI-069.md) |
| `FE-RWI-070` ✅ | Dokter menulis kajian medis dan membaca pengkajian keperawatan | `FR-DOK-074`, `076` | `0.6.0` API kajian | Tab Kajian `FE-DOK-02` | Tab **Kajian Pasien** — Riwayat kajian, Kajian Baru, rujukan pengkajian keperawatan | `FE-RWI-067`, `BE-RWI-091` [BE] | AC-1 s.d. AC-4 | `npm run lint`, `npm run build`, verifikasi manual | Disetujui Sukma GP `RWI-DEC-152` / Muhammad Hamzah | ✅ [Laporan](../task/report/frontend/FE-RWI-070.md) |
| `FE-RWI-071` ✅ | Dokter meresepkan, memakai template, dan melihat resep harian | `FR-DOK-086`, `088` s.d. `091` | `0.6.0` API resep | Layar resep yang sudah ada | Tab **Resep** bagian Buat Resep, Template Resep, History Resep, Resep Harian | `FE-RWI-067`, `BE-RWI-099` [BE], `BE-RWI-105` [BE] | AC-1 s.d. AC-6 | `npm run lint`, `npm run build`, verifikasi manual | Disetujui Sukma GP `RWI-DEC-152` / Muhammad Hamzah | ✅ [Laporan](../task/report/frontend/FE-RWI-071.md) |
| `FE-RWI-072` ✅ | Dokter memutuskan obat bawaan dan membuat order sliding scale | `FR-DOK-092`, `093`, `096` s.d. `099` | `0.6.0` API rekonsiliasi + sliding scale | Tab Resep dari `FE-RWI-071` | Tab **Resep** bagian Rekonsiliasi Obat dan Sliding Scale | `FE-RWI-071`, `BE-RWI-101` [BE], `BE-RWI-103` [BE] | AC-1 s.d. AC-6 | `npm run test:unit` PASS, `npm run lint` PASS, `npm run build` PASS | Disetujui Sukma GP `RWI-DEC-152` / Muhammad Hamzah | ✅ [Laporan](../task/report/frontend/FE-RWI-072.md) |
| `FE-RWI-073` ✅ | Dokter mencatat tindakan dan memverifikasi pesanan perawat | `FR-DOK-100`, `102`, `103`, `104` | `0.6.0` API tindakan | Layar tindakan yang sudah ada | Tab **Tindakan** — Form Tindakan, Riwayat Tindakan, pesanan menunggu verifikasi instruksi | `FE-RWI-067`, `BE-RWI-097` [BE], `BE-RWI-098` [BE] | AC-1 s.d. AC-5 | `node tests/unit` PASS, `npm run lint` PASS, `npm run build` PASS | Disetujui Sukma GP `RWI-DEC-152` / Muhammad Hamzah | ✅ [Laporan](../task/report/frontend/FE-RWI-073.md) |
| `FE-RWI-074` | DPJP mengisi dan menandatangani resume dari ruang kerjanya | `FR-DOK-107`, `108`, `109` | `0.9.0` [INP] — **kontrak sama dengan `FE-INP-22`** | Formulir resume `FE-INP-06` | Tab **Resume Medis** — Resume Rawat Inap, Resume ODC "Integrasi belum tersedia", History Resume | `FE-RWI-067`, `BE-RWI-085` [BE-INP], `BE-RWI-086` [BE-INP] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | Menunggu `FE-RWI-067`; payload wajib sama dengan `FE-RWI-064` | Kartu `FE-RWI-074` |
| `FE-RWI-075` | Dokter mencatat dan membaca riwayat visite | — (dipertahankan dari revision sebelumnya) | `0.6.0` API visite | Tab Visit `FE-DOK-05` | Tab **Visit** — Riwayat visit dan Catat Visit, disambungkan ke kerangka baru | `FE-RWI-067` | AC-1 s.d. AC-3 | `npm run lint`, `npm run build`, verifikasi manual | Menunggu `FE-RWI-067`; gerbang `{GATE-RAJAL}` tertutup `RWI-DEC-152` / Sukma GP ✅ | Kartu `FE-RWI-075` |
| `FE-RWI-076` | Dokter melihat penunjang enam layanan dalam satu tempat | `FR-DOK-110`, `FR-DOK-111`; `RWI-DEC-108` | `0.6.0` API Lab/Rad | Layar penunjang `FE-DOK-07` | Tab **Penunjang Medis** — landing enam kartu; empat kartu "Integrasi belum tersedia" **tanpa permintaan jaringan** | `FE-RWI-067` | AC-1 s.d. AC-4 | `npm run lint`, `npm run build`, verifikasi manual + panel Network | Menunggu `FE-RWI-067`; gerbang `{GATE-RAJAL}` tertutup `RWI-DEC-152` / Sukma GP ✅ | Kartu `FE-RWI-076` |
| `FE-RWI-077` | Dokter menemukan dan mengoreksi catatannya sendiri | `FR-DOK-079`, `FR-DOK-080`; `RWI-DEC-127`, `142` | `0.6.0` API `my-authored` | Mesin addendum | `FE-DOK-14` **Catatan Saya** — konsep dan catatan terkunci milik dokter login; tambah addendum | `BE-RWI-092` [BE] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | ~~menunggu `{GATE-YOGA}`~~ **tertutup 2026-09-16 `RWI-DEC-151`**; layar wajib tetap tidak membuka data pasien lain / Yoga Aji Pratama | Kartu `FE-RWI-077` |
| `FE-RWI-078` | Dokter melihat semua yang menunggu tindakannya di satu tempat | `FR-DOK-084`, `FR-DOK-104` | `0.6.0` API daftar tunggu | Daftar tunggu verifikasi | `FE-DOK-15` **Perlu Review** — gabungan entri CPPT menunggu verifikasi dan pesanan perawat menunggu verifikasi instruksi | `BE-RWI-096` [BE], `BE-RWI-098` [BE] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | — / Muhammad Hamzah | Kartu `FE-RWI-078` |
| `FE-RWI-079` | Protokol sliding scale dikelola sebagai versi yang disahkan | `FR-DOK-094`, `FR-DOK-095`; `RWI-DEC-146`, `147` | `0.6.0` API protokol | — (layar baru) | `FE-DOK-16` **Protokol Sliding Scale** — butir menu baru di grup **Farmasi**; kelola versi, ubah draft, sahkan | `BE-RWI-102` [BE] | AC-1 s.d. AC-6 | `npm run lint`, `npm run build`, verifikasi manual | Salah tampil rentang = dosis insulin salah / pemilik klinis **belum ditunjuk** | Kartu `FE-RWI-079` |
| `FE-RWI-080` | Daftar pantau verifikasi ikut memuat episode yang sudah ditutup | `FR-DOK-083`, `FR-DOK-084` | `0.6.0` API daftar tunggu | `FE-DOK-08` pada `FE-INP-09` | Rework `FE-DOK-08` — memuat episode `Closed` milik DPJP terakhir | `BE-RWI-096` [BE] | AC-1 s.d. AC-4 | `npm run lint`, `npm run build`, verifikasi manual | — / Muhammad Hamzah | Kartu `FE-RWI-080` |

---

## Kartu task

### `FE-RWI-067` — `FE-DOK-09` kerangka Ruang Kerja Dokter Rawat Inap

| Field | Isi |
| --- | --- |
| **Status** | ✅ Selesai 17 September 2026. Laporan tracked: [FE-RWI-067](../task/report/frontend/FE-RWI-067.md) |
| **Gelombang** | 1 |
| **Layar** | `FE-DOK-09`, menggantikan `FE-DOK-01` |
| **`pathname`** | `/health-services/inpatient-management/doctor-inpatient` — butir menu **sudah ada** di `menu-items.jsx`, dipertahankan `RWI-DEC-107` |
| **Hak akses** | `InpatientCensus : Read` |

**Bisnis prosesnya.** Dokter rawat inap dan dokter rawat jalan mengerjakan hal yang mirip, dan
`PRD-RWI-V2-001` menuntut tata letaknya **sama persis**: daftar pasien di kiri, konteks pasien di
atas, delapan tab di kanan. Alasannya praktis — dokter yang berpindah antara poliklinik dan bangsal
tidak perlu belajar dua tata letak.

Yang membedakan hanya satu hal, dan itu penting: **tidak ada aksi maupun status antrean** di rawat
inap. Pasien bangsal tidak mengantre; mereka sudah ada di tempat tidurnya.

**Contoh.** dr. Ahmad membuka menu Dokter → Rawat Inap. Di kiri muncul dua kartu pasien miliknya
beserta Total Pasien 2 — bukan 122. Ia menekan kartu Budi, dan di kanan terbuka delapan tab dengan
konteks Budi di atasnya.

**Cakupan yang diharapkan.**

- Halaman berdiri sendiri pada `pathname` yang sudah ada.
- Daftar pasien kiri memakai census `assignedToMe=true`.
- Metrik kepala dihitung dari daftar yang sama, dan **disembunyikan** bila daftarnya gagal dimuat — bukan ditampilkan nol.
- Census `FE-INP-01` dan Detail Episode `FE-INP-04` **menautkan** ke halaman ini dengan pasien terpilih; keduanya bukan lagi induk layar ini.

**Acceptance criteria.**

1. Daftar pasien hanya memuat pasien dengan penugasan aktif dokter login — `FR-DOK-069`.
2. Total Pasien dihitung dari daftar yang sama — `FR-DOK-071`.
3. Daftar yang gagal dimuat menyembunyikan metrik kepala, tidak menampilkan angka nol — `FR-DOK-071`.
4. Tata letaknya memenuhi `UI-AC-DOK-001` s.d. `UI-AC-DOK-012`; tangkapan layar bertopeng pada tiga lebar layar menunjukkan kotak yang identik dengan Dokter Rawat Jalan — `FR-DOK-072`.
5. **Nol** aksi maupun status antrean pada layar ini — `FR-DOK-073`.
6. Tautan dari Census dan Detail Episode membuka halaman ini dengan pasien terpilih.

**Bukti verifikasi.** `npm run lint` dan `npm run build`; verifikasi manual; **tangkapan layar
bertopeng pada tiga lebar layar** dilampirkan ke laporan task sebagai bukti kriteria 4.

**~~Blocker~~ — tertutup 16 September 2026.** Layar ini dibangun dengan **mengekstraksi komponen tata
letak** milik ruang kerja Dokter Rawat Jalan V2. **Sukma GP**, pemilik blueprint `rawat-jalan`, sudah
menyetujui ekstraksi itu, tercatat `RWI-DEC-152`. Butir terbuka `04-prd-to-mvp.md` 22.20 nomor 11
ikut tertutup.

**Yang tidak ikut longgar.** Kriteria 4 tetap mengikat: tangkapan layar bertopeng pada tiga lebar
harus menunjukkan kotak yang **identik** dengan Dokter Rawat Jalan. Persetujuan membuka izin
mengekstraksi komponen; ia tidak menggantikan bukti bahwa hasilnya benar-benar sama.

**Definition of Done.** Laporan tracked merujuk `RWI-DEC-152`; lint dan build hijau; tangkapan layar
terlampir; roadmap dan traceability diperbarui.

---

### `FE-RWI-068` — Tab SOAP

| Field | Isi |
| --- | --- |
| **Status** | ✅ Selesai 17 September 2026. Laporan tracked: [FE-RWI-068](../task/report/frontend/FE-RWI-068.md) |
| **Gelombang** | 2 |
| **Layar** | `FE-DOK-03` sebagai tab di dalam `FE-DOK-09` |

**Bisnis prosesnya.** Tab SOAP berisi tiga hal: menulis SOAP baru, membaca riwayatnya, dan
mengoreksi lewat addendum. Yang berubah pada revision `7` adalah **siapa yang boleh menekan tombol
apa**: konsep hanya bisa diselesaikan penulisnya, dan catatan yang sudah terkunci hanya bisa
ditambah addendum, tidak disunting.

**Acceptance criteria.**

1. Tombol Selesaikan hanya aktif bagi penulis konsep; bagi dokter lain tombolnya tidak ada.
2. Catatan `LockedUnsigned` tampil bertanda "Tidak Ditandatangani" dan **tidak** dapat disunting.
3. Simpan otomatis tidak menggandakan konsep di layar — satu konsep tetap satu kartu.
4. Waktu klinis dan waktu tanda tangan ditampilkan sebagai dua nilai terpisah.
5. Galat `403` dari server ditampilkan apa adanya, bukan ditelan.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-069` — Tab CPPT

| Field | Isi |
| --- | --- |
| **Status** | ✅ Selesai 17 September 2026. Laporan tracked: [FE-RWI-069](../task/report/frontend/FE-RWI-069.md) |
| **Gelombang** | 2 |
| **Layar** | `FE-DOK-04` sebagai tab di dalam `FE-DOK-09` |

**Bisnis prosesnya.** CPPT adalah lini masa lintas profesi. Dokter perlu menyaringnya — kadang ingin
membaca catatan perawat saja, kadang seluruhnya. Dan DPJP perlu memverifikasi catatan profesi lain
sebagai tanda ia sudah membacanya.

**Acceptance criteria.**

1. Saring Semua / Dokter / Perawat / Profesi Lain bekerja dari kolom `NoteKind`.
2. Entri lama berjenis `Unspecified` tetap tampil, dan masuk saringan "Semua".
3. Tombol Verifikasi hanya muncul bagi DPJP aktif; konsulen dan dokter jaga tidak melihatnya.
4. Pada episode `Closed`, DPJP terakhir tetap melihat tombol Verifikasi untuk entri yang tertinggal.
5. Entri yang lewat batas ditandai terlambat, beserta lamanya.
6. Menyembunyikan tombol bukan penjagaan — `403` dari server tetap ditampilkan bila terjadi.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual dengan akun DPJP dan akun
konsulen.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-070` — Tab Kajian Pasien

| Field | Isi |
| --- | --- |
| **Status** | ✅ Selesai 17 September 2026. Laporan tracked: [FE-RWI-070](../task/report/frontend/FE-RWI-070.md) |
| **Gelombang** | 2 |
| **Layar** | `FE-DOK-02` sebagai tab di dalam `FE-DOK-09` |

**Bisnis prosesnya.** Kajian medis awal adalah dokumen yang dibuat dokter saat pasien masuk. Di tab
ini dokter juga bisa **merujuk** pengkajian keperawatan — membacanya, bukan menyuntingnya, karena
dokumen itu milik perawat.

**Acceptance criteria.**

1. Riwayat kajian medis tampil per episode.
2. Kajian Baru mengikuti aturan penulis tunggal dan penugasan pada waktu klinis.
3. Rujukan pengkajian keperawatan tampil **baca-saja**, tanpa tombol sunting.
4. Konsep kajian yang terkunci tampil bertanda "Tidak Ditandatangani".

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-071` — Tab Resep: buat, template, history, resep harian

| Field | Isi |
| --- | --- |
| **Status** | ✅ Selesai 17 September 2026. Laporan tracked: [FE-RWI-071](../task/report/frontend/FE-RWI-071.md) |
| **Gelombang** | 2 |
| **Layar** | `FE-DOK-10`, tab tersendiri hasil pemecahan `FE-DOK-06` |

**Bisnis prosesnya.** Resep dipisahkan dari Tindakan menjadi tab sendiri karena isinya bertambah
banyak: selain membuat resep, ada template pribadi dokter, riwayat, dan daftar resep harian.

**Acceptance criteria.**

1. Buat Resep bekerja dan tersimpan lewat kontrak `0.6.0`.
2. Daftar template hanya memuat template **milik dokter login** — `FR-DOK-089`.
3. Memakai template menandai butir yang bentrok alergi atau obatnya tidak tersedia, **tanpa** menggagalkan butir lain — `FR-DOK-090`.
4. Draft yang masih membawa butir bertanda ditolak saat disimpan, dan alasannya ditampilkan.
5. Resep Harian menyaring Hari Ini, Minggu Ini, Bulan Ini, dan Rentang, termasuk racikan dan obat pulang — `FR-DOK-086`.
6. Template kosong ditolak dengan pesan yang jelas — `FR-DOK-091`.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual termasuk skenario butir
bentrok alergi.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-072` — Tab Resep: rekonsiliasi obat dan sliding scale

| Field | Isi |
| --- | --- |
| **Status** | ✅ Selesai 17 September 2026. Laporan tracked: [FE-RWI-072](../task/report/frontend/FE-RWI-072.md) |
| **Gelombang** | 3 |
| **Layar** | `FE-DOK-10`, dua bagian tambahan |

**Bisnis prosesnya.** Dua bagian ini adalah bagian paling berbahaya di seluruh tab Resep.
Rekonsiliasi memutuskan nasib obat yang sudah diminum pasien di rumah; sliding scale menentukan
dosis insulin dari hasil gula darah. Keduanya harus menampilkan **apa yang sebenarnya tersimpan**,
bukan ringkasan yang enak dibaca.

**Contoh.** dr. Ahmad membuka Sliding Scale Budi dan melihat pratinjau: "GDS 280 → 3 unit
(disesuaikan separuh: pasien sensitif insulin)". Angka 3 dan alasannya tampil bersama, sehingga
tidak ada yang mengira itu dosis standar.

**Acceptance criteria.**

1. Setiap obat bawaan menampilkan pilihan Lanjut Sama, Lanjut Ubah, dan Hentikan.
2. Keputusan yang sudah diambil menampilkan riwayat penggantiannya, bukan hanya nilai terakhir.
3. Setelah resep aktif, layar menampilkan `409` dari server sebagai penolakan yang jelas, bukan galat umum.
4. Order sliding scale menampilkan **rentang yang tersalin ke order itu**, bukan rentang template terbaru.
5. Penyesuaian dosis mewajibkan alasan sebelum tombol simpan aktif.
6. Pratinjau rentang dan dosis tampil **sebelum** menyimpan — `FR-DOK-096`.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual untuk keenam kriteria.
Karena bagian ini menyentuh perhitungan dosis, **test unit direkomendasikan** bila ada utility
perhitungan atau normalisasi baru di `src/utils` — mengikuti tabel pada `rules/frontend/test-policy.md`.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-073` — Tab Tindakan

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan — menunggu `FE-RWI-067`. ~~⛔ `{GATE-RAJAL}`~~ tertutup 2026-09-16 `RWI-DEC-152` |
| **Gelombang** | 2 |
| **Layar** | `FE-DOK-11`, tab tersendiri hasil pemecahan `FE-DOK-06` |

**Bisnis prosesnya.** Selain mencatat tindakan yang ia kerjakan sendiri, dokter juga perlu melihat
pesanan yang dibuat perawat atas instruksinya, dan mengkonfirmasinya.

**Acceptance criteria.**

1. Form Tindakan dan Riwayat Tindakan bekerja lewat kontrak `0.6.0`.
2. Daftar "menunggu verifikasi instruksi" hanya memuat pesanan yang **pemberi instruksinya adalah dokter login**.
3. Verifikasi tidak mengubah penginput maupun isi pesanan yang ditampilkan.
4. Pembatalan pesanan mewajibkan alasan sebelum tombol aktif.
5. Dokter yang bukan pemberi instruksi tidak melihat tombol verifikasi pada pesanan itu.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-074` — Tab Resume Medis

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan — menunggu `FE-RWI-067`. ~~⛔ `{GATE-RAJAL}`~~ tertutup 2026-09-16 `RWI-DEC-152` |
| **Gelombang** | 2 |
| **Layar** | `FE-DOK-12` — permukaan `CAP-026` |

**Bisnis prosesnya.** Resume selama ini hanya bisa diisi dari layar Detail Episode, yang bukan
tempat kerja dokter. Tab ini membawanya ke ruang kerja dokter, memakai **kontrak yang sama persis**.

**Acceptance criteria.**

1. Tab membaca, menyimpan, dan menandatangani resume lewat kontrak `0.9.0` yang sama dengan `FE-INP-22`.
2. Tiga isian baru — Pemeriksaan Penting, Kondisi Saat Pulang, Edukasi — tampil dan tersimpan.
3. Tombol usulan bersumber bekerja sama seperti pada `FE-RWI-064`, beserta label sumbernya.
4. Resume ODC tampil "Integrasi belum tersedia" **tanpa form** — `FR-DOK-109`.
5. History Resume menampilkan revisi beserta penandatangannya.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual; **perbandingan payload**
dengan `FE-RWI-064` dicatat pada laporan sebagai bukti kriteria 1.

**Risiko.** Dua permukaan memakai satu kontrak. Bila salah satunya perlu berubah, perubahannya lewat
kontrak `0.9.0` milik `episode-rawat-inap` — **bukan** lewat salah satu layar.

**Definition of Done.** Lint dan build hijau; perbandingan payload tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-075` — Tab Visit

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan — menunggu `FE-RWI-067`. ~~⛔ `{GATE-RAJAL}`~~ tertutup 2026-09-16 `RWI-DEC-152` |
| **Gelombang** | 2 |
| **Layar** | `FE-DOK-05`, dipertahankan apa adanya |

**Bisnis prosesnya.** Tab ini **tidak berubah isinya**. Yang berubah hanya tempatnya: ia berpindah
ke dalam kerangka `FE-DOK-09`.

**Acceptance criteria.**

1. Riwayat visit dan Catat Visit bekerja sama seperti sebelum pemindahan.
2. Pembatalan visit yang salah tetap tersedia.
3. Tidak ada perubahan kontrak API pada tab ini.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual sebagai **regresi** — yang
dibuktikan adalah "tidak ada yang rusak akibat pemindahan".

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-076` — Tab Penunjang Medis

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan — menunggu `FE-RWI-067`. ~~⛔ `{GATE-RAJAL}`~~ tertutup 2026-09-16 `RWI-DEC-152` |
| **Gelombang** | 2 |
| **Layar** | `FE-DOK-13`, menggantikan `FE-DOK-07` |

**Bisnis prosesnya.** Penunjang medis punya enam layanan, tetapi hanya dua yang backend-nya
tersedia: Laboratorium dan Radiologi. Empat sisanya ditampilkan sebagai kartu berketerangan
"Integrasi belum tersedia" — jujur, dan **tanpa mengirim permintaan jaringan** ke modul yang belum
ada, karena permintaan yang pasti gagal hanya menghasilkan galat palsu di log.

**Acceptance criteria.**

1. Landing menampilkan enam kartu.
2. Kartu Laboratorium dan Radiologi menampilkan jumlah pesanan, hasil final baru, dan detail hasil — `FR-DOK-110`.
3. Empat kartu lain menampilkan konteks pasien dan "Integrasi belum tersedia" — `FR-DOK-111`.
4. Membuka keempat kartu itu menghasilkan **nol** permintaan jaringan ke modul terkait.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual; kriteria 4 dibuktikan lewat
panel Network peramban, dan tangkapannya dilampirkan.

**Definition of Done.** Lint dan build hijau; bukti panel Network terlampir; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-077` — `FE-DOK-14` Catatan Saya

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan. ~~⛔ menunggu `{GATE-YOGA}`~~ — **gerbang tertutup 2026-09-16 lewat `RWI-DEC-151`**; kini hanya menunggu `BE-RWI-092` |
| **Gelombang** | 1 |
| **Layar** | `FE-DOK-14`, layar berdiri sendiri — **bukan** tab di dalam `FE-DOK-09` |

**Bisnis prosesnya.** Inilah jawaban `RWI-DEC-127` atas pertanyaan "bagaimana dokter mengoreksi
catatannya sendiri setelah penugasannya berakhir". Layar ini **hanya** membuka catatan buatan dokter
login. Ia bukan pintu belakang ke rekam medis pasien.

**Contoh.** dr. Yoga membuka Catatan Saya, menemukan SOAP Budi Selasa 02.00, dan menambah addendum
"koreksi dosis: 500 mg, bukan 5000 mg". Dari layar ini ia **tidak** bisa membuka CPPT Budi, resep
Budi, atau data Budi yang lain, dan tidak bisa menulis catatan baru.

**Acceptance criteria.**

1. Daftar hanya memuat catatan yang penulisnya adalah pengguna login.
2. Setiap baris menampilkan identitas pasien **seminimum mungkin** — cukup untuk mengenali.
3. Addendum dapat ditambahkan pada catatan final maupun terkunci milik sendiri, termasuk pada episode yang sudah ditutup.
4. Layar ini **tidak** menyediakan tautan apa pun ke CPPT, resep, atau data pasien lain.
5. Layar ini **tidak** menyediakan jalan membuat catatan baru.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual; kriteria 4 dan 5 dibuktikan
dengan menelusuri seluruh kontrol pada layar dan mencatat bahwa tautan itu memang tidak ada.

**~~Blocker~~ — tertutup 16 September 2026.** Bagian **Terkunci** pada layar ini bergantung pada
`my-authored` dan `serviceContext` milik Yoga Aji Pratama, dan ia sudah menyetujuinya lewat
`RWI-DEC-151`. Yang tersisa hanya menunggu `BE-RWI-092` mendarat — dependency biasa, bukan gerbang.

**Yang tidak ikut longgar.** Kriteria 4 dan 5 tetap mengikat: layar ini **tidak** menyediakan tautan
ke CPPT, resep, atau data pasien lain, dan **tidak** menyediakan jalan membuat catatan baru.
Persetujuan Yoga Aji Pratama membuka mesinnya, bukan melebarkan cakupan layarnya.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-078` — `FE-DOK-15` Perlu Review

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan — **boleh jalan** begitu `BE-RWI-096` dan `BE-RWI-098` mendarat |
| **Gelombang** | 1 |
| **Layar** | `FE-DOK-15`, layar berdiri sendiri |

**Bisnis prosesnya.** Seorang dokter punya dua antrean yang selama ini terpisah: entri CPPT yang
menunggu verifikasinya sebagai DPJP, dan pesanan perawat yang menunggu konfirmasi instruksinya.
Layar ini menggabungkan keduanya supaya tidak ada yang terlewat karena tersembunyi di layar lain.

**Acceptance criteria.**

1. Daftar memuat entri CPPT yang menunggu verifikasi **dokter login sebagai DPJP**.
2. Daftar memuat pesanan perawat yang **pemberi instruksinya dokter login**.
3. Kedua jenis dibedakan dengan jelas, bukan dicampur tanpa penanda.
4. Entri yang lewat batas ditandai terlambat.
5. Setiap baris membuka tempat aslinya — entri CPPT membuka tab CPPT pasien itu.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual.

**Catatan.** Kriteria 5 menautkan ke tab CPPT `FE-RWI-069`, yang kini **tidak lagi tertahan gerbang**
tetapi berada di gelombang 2 sedangkan task ini gelombang 1. Selama tab itu belum ada, tautannya
mengarah ke layar CPPT yang berlaku saat itu, dan pilihan itu dicatat sebagai `DEV_DISCRETION` pada
laporan task.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; pilihan tautan dicatat;
laporan tracked ada; roadmap dan traceability diperbarui.

---

### `FE-RWI-079` — `FE-DOK-16` Protokol Sliding Scale

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan — **boleh jalan** begitu `BE-RWI-102` mendarat |
| **Gelombang** | 1 |
| **Layar** | `FE-DOK-16` |
| **Butir menu** | **Baru** — Pelayanan Kesehatan → **Farmasi** → Protokol Sliding Scale, `pathname` `/health-services/pharmacy-management/sliding-scale-templates` |
| **Hak akses** | `SlidingScaleTemplate : Read` |

**Bisnis prosesnya.** Protokol sliding scale bukan data pasien, melainkan **konfigurasi klinis**.
Karena itu letaknya di grup Farmasi, bukan di ruang kerja dokter. Yang mengelolanya adalah pengubah
dan pengesah konfigurasi farmasi-klinis.

**Yang paling menentukan.** Layar ini harus membuat rentang yang **bertumpuk atau berlubang terlihat
sebelum disimpan**. Menyerahkan seluruh pemeriksaan itu ke server berarti pengguna baru tahu
salahnya setelah menekan simpan, dan pada data dosis insulin itu terlalu terlambat.

**Acceptance criteria.**

1. Daftar template beserta versinya, lengkap dengan status `Draft`, `Approved`, `Retired`.
2. Editor rentang menandai tumpang tindih dan lubang **secara visual sebelum simpan**.
3. Tombol Sahkan tidak tersedia bagi pengguna yang merupakan **pengubah terakhir** versi itu — `FR-DOK-095`.
4. Pengesahan menampilkan konfirmasi yang menyebut versi mana yang akan dipensiunkan.
5. Versi `Draft` ditandai jelas sebagai belum boleh dipakai membuat order.
6. Penolakan dari server ditampilkan apa adanya, termasuk alasan rentang yang ditolak.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual termasuk mencoba rentang
200–260 bersama 250–299 dan memastikan tumpang tindihnya terlihat sebelum simpan. Karena editor
rentang berisi **logika murni**, test unit di `tests/unit/` **direkomendasikan** untuk fungsi
pemeriksa tumpang tindih dan lubang.

**Gerbang produksi — diperbarui 16 September 2026.** Manajemen rumah sakit sudah menyetujui
**pemakaian** sliding scale lewat `RWI-DEC-155`. Yang **belum** ada adalah **nama** pengesah isi
protokol, dicatat `RWI-OQ-097`. Akibatnya pada layar ini: tombol Sahkan boleh dibangun dan diuji,
tetapi selama nama itu kosong **nol versi protokol dapat dinaikkan menjadi `Approved`** untuk pasien
sungguhan, karena kriteria 3 menuntut pengesah yang berbeda dari pengubah terakhir.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; `RWI-OQ-097` dicatat sebagai
masih terbuka pada laporan; laporan tracked ada; roadmap dan traceability diperbarui.

---

### `FE-RWI-080` — `FE-DOK-08` Daftar Pantau Verifikasi memuat episode tertutup

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan — **boleh jalan** begitu `BE-RWI-096` mendarat |
| **Gelombang** | 1 |
| **Layar** | `FE-DOK-08`, tetap berada pada Daftar Pantau `FE-INP-09` |

**Bisnis prosesnya.** `RWI-DEC-126` membuat DPJP terakhir tetap boleh memverifikasi entri yang
tertinggal pada episode yang sudah ditutup. Daftar pantau inilah tempat ia menemukannya.

**Acceptance criteria.**

1. Daftar memuat episode `Closed` yang DPJP terakhirnya adalah pengguna login.
2. Episode tertutup dibedakan secara visual dari episode aktif.
3. Episode hilang dari daftar setelah seluruh entrinya terverifikasi.
4. Letak daftar ini pada `FE-INP-09` mengikuti urutan yang ditetapkan 12 September 2026 — kelompok `dokter-rawat-inap` berada sesudah kelompok `episode-rawat-inap`.

**Bukti verifikasi.** `npm run lint`, `npm run build`, verifikasi manual.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

## Pilihan UI yang belum disetujui

| Butir | Keadaan | Ditangani di |
| --- | --- | --- |
| Tautan baris "Perlu Review" selama tab CPPT baru belum ada | `DEV_DISCRETION` | `FE-RWI-078` |
| Bentuk visual penanda rentang bertumpuk pada editor sliding scale | `DEV_DISCRETION` — bentuknya bebas, **kewajibannya tidak**: tumpang tindih wajib terlihat sebelum simpan | `FE-RWI-079` |

Pilihan `DEV_DISCRETION` dicatat pada laporan task dan **tidak** diam-diam dijadikan keputusan
produk. Bila pemilik kemudian menetapkan bentuknya, penetapan itu masuk lewat `grill-me`.

---

## Kebijakan verifikasi frontend

Mengikuti `rules/frontend/test-policy.md`:

- **Menulis test baru bersifat opsional.** Tidak ada task di atas yang tertahan karena tidak menambah test.
- Validasi minimum tetap `npm run lint` dan `npm run build`, dijalankan sungguhan dan keluarannya ditempel.
- Test unit **direkomendasikan** pada dua tempat yang berisi logika murni: pemeriksa rentang sliding scale (`FE-RWI-079`) dan perhitungan dosis yang ditampilkan (`FE-RWI-072`). Polanya mengikuti berkas tetangga di `tests/unit/<fitur>.test.mjs`.
- `npm run test:e2e` **tidak** dijalankan kecuali task memintanya dan environment mendukung.
- Baris `AUTOMATED TEST:` dan `MANUAL TEST:` ditulis terpisah pada laporan task.

---

## Gerbang yang masih terbuka

| Gerbang | Menahan | Siapa yang membukanya |
| --- | --- | --- |
| ~~`{GATE-RAJAL}` — ekstraksi komponen tata letak~~ | **TERTUTUP 2026-09-16** lewat `RWI-DEC-152` — `FE-RWI-067` beserta sembilan tab bebas | **Sukma GP** ✅ |
| ~~`{GATE-YOGA}` — `my-authored` dan `serviceContext`~~ | **TERTUTUP 2026-09-16** lewat `RWI-DEC-151` — `FE-RWI-077` bebas | Yoga Aji Pratama ✅ |
| Pengesah isi protokol sliding scale — **`RWI-OQ-097`** | Gerbang produksi bagi `FE-RWI-079`, **sebagian tertutup**: kewenangan memakai sudah ada `RWI-DEC-155`, **nama pengesah belum** | Manajemen rumah sakit |

---

## Traceability

Tabel penuh ada di [`requirement-traceability-v2.md`](./requirement-traceability-v2.md).
