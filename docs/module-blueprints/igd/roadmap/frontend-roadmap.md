# Roadmap Delivery Frontend — Modul IGD

## Metadata

```yaml
module_id: igd
roadmap_revision: 3
wave: "Dikoreksi 2026-09-15: lima task selesai (FE-IGD-015, 016, 018, 020, 021); lima sebagian (FE-IGD-012, 013, 014, 017, 022); FE-IGD-010 belum dikerjakan. Klaim lama 'MVP-0..MVP-5 selesai' tidak akurat — lihat evidence/2026-09-15-pemeriksaan-status.md bagian 8"
status: ACTIVE
status_synced_at: "2026-09-15 — pemetaan ulang acceptance criteria pada frontend 43adae648; IGD-DEC-110, IGD-DEC-113"
planning_updated_at: "2026-09-22 — plan-module-delivery (MODULE BLUEPRINT MODE, docs saja) pada frontend c941012ac: R3.12 FE-IGD-035 sampai FE-IGD-037 ditambahkan (encounter-first, dokter jaga); IGD-DEC-139 sampai IGD-DEC-141; evidence 2026-09-22-desain-encounter-first.md; FE-IGD-027 tetap ✅. Sebelumnya 2026-09-16 (keempat) — plan-module-delivery: FE-IGD-031 dan FE-IGD-032 ditambahkan (tata letak riwayat pada ruang kerja pemeriksaan); IGD-DEC-133, IGD-DEC-134; evidence 2026-09-16-tata-letak-riwayat-pemeriksaan.md. Nol perubahan kontrak, nol perubahan backend. Revision roadmap tetap 3. Sebelumnya 2026-09-16 (kedua): FE-IGD-029 dan FE-IGD-030 (kunjungan keluar dari Arrived); IGD-DEC-127, IGD-DEC-128; evidence 2026-09-16-kunjungan-terjebak-arrived.md. Sebelumnya 2026-09-16: FE-IGD-028; 2026-09-15 (kedua): kartu susulan FE-IGD-019, FE-IGD-023 sampai FE-IGD-027, IGD-DEC-111, IGD-DEC-116 sampai IGD-DEC-121"
generated_at: "2026-08-24"
revision_3_at: "2026-08-26"
revision_3_1_at: "2026-08-27"
owners:
  - "Product/Domain Owner IGD — Rizki Gunawan (IGD-DEC-089)"
  - "Frontend authority untuk area DEV_DISCRETION (IGD-UI-004)"
approved_by:
  - "Rizki Gunawan / 2026-08-26 — IGD-DEC-094"
input_revisions:
  blueprint-manifest.md: 5
  03-frontend-architecture.md: 5
  04-prd-to-mvp.md: 5
contract_versions:
  - "State 0.4.0 — bagian 1, 1.1, 1.2 APPROVED (IGD-DEC-093)"
  - "Validation 0.4.0 — bagian 2 aturan 4-5 APPROVED (IGD-DEC-093)"
  - "API 0.4.0 — draft. TIDAK dipakai: gelombang ini nol perubahan endpoint"
artifact_hashes:
  03-frontend-architecture.md: "2b4339f9587ed1daff8444ccb68cb5415df578d76a2157dd3ec168f9a2a1fd95"
  04-prd-to-mvp.md: "7061525001d9a7e6b311424b8e3a8d85de13e35f59e545a78dcefedd600b79db"
  contracts/state-transition-matrix.md: "a41efd8d9adc87e1cf1eec2a9397b3521fdc0ebf935ccf0a19a5aa975b6c7c75"
  contracts/validation-matrix.md: "0ee98b750a29e01603db894ed3766614fe8989b2eef3573eab7d72cdc1a6b907"
source_commits:
  frontend: "96a9120111f6acc6b7c0f37973ea0c717ba41f17"
supersedes: "roadmap/archive/revision-1/frontend-roadmap.md"
```

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

> **Penandaan 15 September 2026.** Tanda dipasang setelah setiap acceptance criteria dipetakan
> ulang ke source frontend `43adae648`. `IGD-DEC-110` membuat angka test lama sah sebagai bukti
> historis, tetapi **tidak** melepas acceptance criteria yang menuntut tangkapan layar atau uji
> lewat layar. `IGD-DEC-113` menerima laporan gabungan lama. Rincian per task:
> [evidence/2026-09-15-pemeriksaan-status.md](../evidence/2026-09-15-pemeriksaan-status.md)
> bagian 8.2.

## Grafik Urutan Dependency

Roadmap ini memuat **27 task** menurut *Register status task* (22 September 2026: 24 baris + `FE-IGD-035`…`037`;
angka `22` di bawah tertinggal sejak `FE-IGD-033`/`034`), dan bersama prasyarat backend dan revision `1` jumlah node
melewati 25. Grafik dipecah: satu **grafik ringkasan** di bawah ini, lalu grafik per bagian di
bawah judulnya masing-masing — bagian 1 (`MVP-0` dan warisan revision `1`), R3.2 (pendaftaran,
pengkajian, kepergian, kebersihan), R3.4 (gelombang 27 Agustus), R3.5 (`FE-IGD-022`),
R3.7 (pemantauan observasi), R3.8 (kunjungan keluar dari `Arrived`), R3.9 (tata letak
riwayat pada ruang kerja pemeriksaan), R3.11 (pra-cek episode ganda), dan R3.12 (encounter-first dan dokter jaga).

*Jumlah task dikoreksi 16 September 2026: angka `11` tertinggal sejak revision `3` ditulis dan
tidak pernah ikut diperbarui saat `FE-IGD-019` sampai `FE-IGD-028` ditambahkan. Dinaikkan
menjadi 22 pada 16 September 2026 (keempat) bersama `FE-IGD-031` dan `FE-IGD-032`.*

Seluruh prasyarat frontend berasal dari **backend**, bukan dari task frontend lain. Karena itu
setiap task frontend berada di gelombang 1, dan kolom "Boleh mulai setelah" menyebut prasyarat
backend beserta tandanya.

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph backend["Prasyarat backend — gelombang pada backend-roadmap.md"]
        BMVP0["🟡 MVP-0<br/>Status kunjungan tidak mundur"]:::luar
        BMVP12["✅ MVP-1 dan MVP-2<br/>Pendaftaran dan episode"]:::luar
        BMVP3["🟡 MVP-3<br/>Pengkajian tanpa antrean"]:::luar
        BMVP4["🟡 MVP-4<br/>Kepergian dua rangkaian status"]:::luar
        BR37["✅ R3.7<br/>Migration dan master dipindah"]:::luar
        BR38["R3.8<br/>Perbaikan pasca-pemeriksaan"]:::luar
        BMVP5["🟡 MVP-5<br/>Serah terima dan riwayat dokter"]:::luar
        BR312["✅ R3.12<br/>BE-IGD-049, BE-IGD-050"]:::luar
        BR313["⛔ R3.13<br/>BE-IGD-053 s.d. 056 (1 siap, 3 tertahan)"]:::luar
    end

    FMVP0["🟡 Bagian 1<br/>Penolakan 409 tampil"]:::sebagian
    FWARIS["Bagian 2<br/>Detail kunjungan IGD"]:::belum
    FDAFTAR["✅ Pendaftaran<br/>FE-IGD-014"]:::selesai
    FKAJI["🟡 Pengkajian<br/>FE-IGD-013, 019, 022"]:::sebagian
    FPERGI["🟡 Kepergian<br/>FE-IGD-015 s.d. 017"]:::sebagian
    FMASTER["✅ Gelombang 27 Agustus<br/>FE-IGD-020, FE-IGD-021"]:::selesai
    FBERSIH["✅ Kebersihan<br/>FE-IGD-018"]:::selesai
    FPASCA["🟡 R3.6 Gelombang 15 September<br/>FE-IGD-023 s.d. 027"]:::sebagian
    FPASCA2["✅ R3.7 Gelombang 16 September<br/>FE-IGD-028"]:::selesai
    FARRIVED["🟡 R3.8 Gelombang 16 September (kedua)<br/>FE-IGD-029, FE-IGD-030"]:::sebagian
    FTATA["🟡 R3.9 Gelombang 16 September (keempat)<br/>FE-IGD-031, FE-IGD-032"]:::sebagian
    FPRA["✅ R3.11 Gelombang 21 September (sore)<br/>FE-IGD-034"]:::selesai
    FENC["R3.12 Gelombang 22 September<br/>FE-IGD-035 s.d. 037"]:::belum

    BMVP0 --> FMVP0
    BMVP0 --> FARRIVED
    BMVP12 --> FDAFTAR
    BMVP3 --> FKAJI
    BR37 --> FKAJI
    BMVP4 --> FPERGI
    BR37 --> FMASTER
    BR38 --> FPASCA
    BMVP5 --> FPASCA
    FPASCA --> FPASCA2
    FPASCA2 --> FTATA
    BR312 --> FPRA
    BR312 --> FPERGI
    FDAFTAR --> FPRA
    BR313 --> FENC
```

| Gelombang | Boleh mulai setelah | Bagian |
| ---: | --- | --- |
| 1 | Prasyarat backend masing-masing | Seluruh bagian **kecuali R3.9** — boleh paralel. Bagian 2 menunggu task revision `1` yang seluruhnya sudah selesai; Kebersihan tanpa prasyarat. Pada R3.6, `FE-IGD-023`, `025`, dan `026` tidak menunggu backend; `FE-IGD-024` menunggu `BE-IGD-040` (R3.8 **backend**); `FE-IGD-027` menunggu `BE-IGD-045` (`MVP-5`). Pada R3.8 **frontend**, `FE-IGD-029` dan `FE-IGD-030` boleh mulai sekarang: prasyarat backend-nya `BE-IGD-018` sudah ✅ dan endpoint-nya sudah berjalan |
| 1–2 | R3.13 **backend**: `BE-IGD-054` → `FE-IGD-035`; `BE-IGD-056` (⛔ E1–E3) → `FE-IGD-037`; `BE-IGD-053` (⛔) + `BE-IGD-055` (⛔) + `FE-IGD-035` → `FE-IGD-036` (gelombang 2) | R3.12 — **belum boleh dimulai**; rincian di bagian R3.12 |
| 2 | R3.7 selesai — `FE-IGD-028` ✅ 16 September 2026 | R3.9 — `FE-IGD-031` lalu `FE-IGD-032`. Prasyaratnya **sudah** terpenuhi, jadi bagian ini boleh dimulai sekarang; nomor gelombangnya `2` semata-mata karena `FE-IGD-032` menata ulang tab Observasi yang dibangun `FE-IGD-028`. Nol prasyarat backend — gelombang ini tidak menyentuh backend sama sekali |

### Register status task

| Task | Judul | Status | Laporan |
| --- | --- | --- | --- |
| `FE-IGD-010` | Halaman detail satu kunjungan IGD | tanpa tanda — belum dikerjakan | — |
| `FE-IGD-012` | Penolakan `409` triase tampil dengan pesan backend | 🟡 kriteria 1–3 menuntut tangkapan layar | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-013` | Pengkajian tersimpan lewat layar | 🟡 uji simpan-muat ulang lewat layar belum | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-014` | Pendaftaran mengikuti `Emergency` | ✅ **Dinilai ulang 21 September 2026 (malam) — atas penilaian pemilik, sesudah `FE-IGD-034` ✅; celah `IGD-OQ-093` `open` dinyatakan** ([laporan](../task/report/frontend/FE-IGD-014.md) bagian 9): ✅ ini tidak menyatakan tidak ada encounter yatim dalam segala keadaan. *Riwayat 🟡 21 September 2026:* kriteria 2 **kini ada di source** (kotak peringatan berisi nomor kunjungan + tombol *Buka Kunjungan IGD*); eslint berkas task 0 error, 9 unit test baru lulus, **`npm run build` lulus (pemilik, 21 September 2026 malam)**. **Uji layar pemilik 21 September 2026 malam: kotak, nomor, status, dan tombol tampil benar; hasil klik tombol belum dikonfirmasi.** **Belum boleh ✅ end-to-end** (keputusan pemilik 21 September 2026, `IGD-DEC-138`): encounter yatim ditangani `BE-IGD-050` + `FE-IGD-034`, sisa celah `IGD-OQ-093` | [FE-IGD-014](../task/report/frontend/FE-IGD-014.md) |
| `FE-IGD-015` | Route `emergency-departures` | ✅ | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-016` | Dua rangkaian status kepergian | ✅ | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-017` | Entri susulan, koreksi, daftar pantau | ✅ **22 September 2026 — atas penilaian pemilik**: kolom pelaku/penyetuju membaca nama (`IGD-DEC-137`, `BE-IGD-049` ✅); eslint 0 error, 5 test baru lulus. **Uji layar pemilik LULUS** (pelaku `SuperAdmin`, bukan GUID, 21 September 2026 larut malam); **`npm run build` dinyatakan lulus pemilik** 22 September 2026 (keluaran tidak dilampirkan; artefak `.next` 16:03 dari commit `c941012ac` diperiksa agent). Kriteria 2–3 terpenuhi | [FE-IGD-017](../task/report/frontend/FE-IGD-017.md) |
| `FE-IGD-018` | Bersih-bersih sisa yang tidak dipakai | ✅ | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-020` | Route master data IGD | ✅ | [fe-igd-020-021](../task/report/frontend/fe-igd-020-021-route-master-igd-dan-kolom-kesimpulan.md) |
| `FE-IGD-021` | Kolom Kesimpulan observasi | ✅ | [fe-igd-020-021](../task/report/frontend/fe-igd-020-021-route-master-igd-dan-kolom-kesimpulan.md) |
| `FE-IGD-022` | Layar asuhan keperawatan IGD | 🟡 uji layar belum; tab lab cacat; teks radiologi usang — *tab lab dan teks radiologi ditangani `FE-IGD-023` ✅ 15 September 2026; uji layar `FE-IGD-022` tetap belum* | [fe-igd-022](../task/report/frontend/fe-igd-022-asuhan-keperawatan-pengkajian-observasi-penunjang.md) |
| `FE-IGD-019` | Assesmen Awal IGD memakai formulir bersama | ✅ — kartu susulan 15 September 2026 | [fe-igd-012-018](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| `FE-IGD-023` | Tab Penunjang Medis membaca pesanan milik pasien | ✅ 15 September 2026 — implementasi; `npm run build` lulus 17 September 2026. **Uji layar pemilik 17 September 2026 LULUS** (pernyataan pemilik) | [FE-IGD-023](../task/report/frontend/FE-IGD-023.md) |
| `FE-IGD-024` | Isian Kesimpulan saat menyelesaikan observasi | ✅ 15 September 2026 — implementasi; `npm run build` lulus 17 September 2026. **Uji layar pemilik 17 September 2026 LULUS** — satu periode observasi diselesaikan beserta isian Kesimpulan | [FE-IGD-024](../task/report/frontend/FE-IGD-024.md) |
| `FE-IGD-025` | Laporan susulan perombakan layar pengkajian dan temuan privasi | tanpa tanda — direncanakan | — |
| `FE-IGD-026` | Laporan susulan layar pendaftaran IGD | tanpa tanda — direncanakan | — |
| `FE-IGD-027` | Layar triase memakai riwayat penugasan dokter | ✅ **21 September 2026** (dinaikkan kembali sesudah build pemilik; sebelumnya 🟡 pada hari yang sama, ✅ 18 September) — Implementation Complete; eslint berkas task dan 38 test IGD `PASS`; runtime inti PASS 18 September 2026 pada revisi `3213419a7` (13 pemeriksaan lewat layar, pemilik); **`npm run build` revisi terbaru lulus — dijalankan pemilik 21 September 2026: 362/362 halaman, 0 error, commit `16c767916`**. **Belum terbukti lewat layar:** tampilan "Sumber: Data historis" untuk baris legacy — dev punya 0 baris legacy. Tanpa UAT | [FE-IGD-027](../task/report/frontend/FE-IGD-027.md) |
| `FE-IGD-028` | Pemantauan observasi dengan tanda vital tertaut | ✅ 16 September 2026 — lint, 857 unit test, dan `npm run build` lulus; **runtime terverifikasi sebagian lewat layar** (jalur pilih-existing dan ABCDE terisi belum dilalui) | [FE-IGD-028](../task/report/frontend/FE-IGD-028.md) |
| `FE-IGD-029` | Pendaftaran IGD menutup dengan status Menunggu Triage | ✅ **17 September 2026** — kedua kriteria terbukti lewat layar. Pasien `RAYYAN DHAFIR PRASETYA MAULANA` didaftarkan 17 September 2026 09.35, lahir berstatus "Menunggu Triage", lalu triage-nya tersimpan sampai berstatus "Sudah ditriage". Penolakan `409` yang memicu gelombang ini **hilang**. Tanpa UAT | [FE-IGD-029](../task/report/frontend/FE-IGD-029.md) |
| `FE-IGD-030` | Aksi Tangani Segera pada daftar triage | ✅ 16 September 2026 — kedelapan kriteria terbukti lewat layar; lint, 859 unit test, dan `npm run build` lulus; tanpa UAT | [FE-IGD-030](../task/report/frontend/FE-IGD-030.md) |
| `FE-IGD-031` | Segmen Formulir dan Riwayat pada tab pemeriksaan | ✅ **17 September 2026** — 13 kriteria terpetakan ke source; lint dan 866 unit test lulus; `npm run build` lulus; **uji layar pemilik LULUS**. Tanpa UAT | [FE-IGD-031](../task/report/frontend/FE-IGD-031.md) |
| `FE-IGD-032` | Tata letak tab Observasi dan lembar pemantauan | ✅ **17 September 2026** — 14 kriteria terpetakan ke source; lint, 866 unit test, dan `npm run build` lulus; **uji layar pemilik LULUS** — pemilih periode, primary survey satu baris, lembar pemantauan berbentuk tabel, dan segmen Lembar/Catat terbukti di layar. **Dua delta (kriteria 3 dan 4) diterima pemilik.** Tanpa UAT | [FE-IGD-032](../task/report/frontend/FE-IGD-032.md) |
| `FE-IGD-033` | Tombol Simpan Pemeriksaan berhenti aktif sesudah penilaian tersimpan | ✅ **17 September 2026** — lint, 866 unit test, dan `npm run build` lulus; **uji layar pemilik LULUS**: tombol nonaktif bertulisan "Pemeriksaan Tersimpan", **Tetapkan Dokter** bekerja sendiri, riwayat memuat tepat satu penilaian. Dirilis bersama `BE-IGD-047` ✅ | [FE-IGD-033](../task/report/frontend/FE-IGD-033.md) |
| `FE-IGD-034` | Pendaftaran IGD memeriksa episode ganda sebelum membuat encounter (`IGD-DEC-138`) | ✅ **21 September 2026 (malam) — atas penilaian pemilik.** Implementation Complete; `BE-IGD-050` ✅ (kontrak `0.10.0`). Lima berkas frontend (commit `c941012ac`); **eslint 0 error**, unit test berkas task **14/14** (suite penuh 1420/1429; 9 gagal di luar cakupan). **`npm run build` dan uji layar dijalankan pemilik, dilaporkan lulus** (pernyataan pemilik; keluaran build dan rincian skenario tidak dilampirkan; artefak `.next` 16:03 diperiksa agent). `fail-open` diputuskan pemilik. `IGD-OQ-093` `open` tidak tertutup. Tanpa UAT. [Laporan](../task/report/frontend/FE-IGD-034.md) | [R3.11](#r311-gelombang-21-september-2026-sore--pra-cek-episode-ganda-dan-nama-pelaku) |
| `FE-IGD-035` | Daftar Menunggu Triage membaca satu sumber (`triage-queue`) — `IGD-DEC-139` | tanpa tanda — direncanakan 22 September 2026; menunggu `BE-IGD-054` | [R3.12](#r312-gelombang-22-september-2026--encounter-first-dan-dokter-jaga-sisi-layar) |
| `FE-IGD-036` | Mulai Triage melahirkan kunjungan; loket berhenti membuat kunjungan — `IGD-DEC-139` | tanpa tanda — direncanakan 22 September 2026; menunggu `BE-IGD-053` ⛔, `BE-IGD-055` ⛔, `FE-IGD-035` | [R3.12](#r312-gelombang-22-september-2026--encounter-first-dan-dokter-jaga-sisi-layar) |
| `FE-IGD-037` | Pemilih Dokter Penanggung Jawab IGD menawarkan dokter jaga, dengan override beralasan — `IGD-DEC-141` | tanpa tanda — direncanakan 22 September 2026; menunggu `BE-IGD-056` ⛔ | [R3.12](#r312-gelombang-22-september-2026--encounter-first-dan-dokter-jaga-sisi-layar) |

`FE-IGD-019` sebelumnya belum punya kartu. Kartunya ditambahkan 15 September 2026 pada bagian
R3.5, tepat sebelum `FE-IGD-022`.

---

## 0. Gelombang ini nyaris tidak menyentuh frontend

`EPIC IGD-03` adalah perbaikan perilaku backend. **Nol endpoint berubah, nol bentuk response
berubah, nol layar baru.** Yang berubah bagi petugas hanyalah: beberapa perbuatan yang dulu
diam-diam berhasil kini ditolak `409` beserta alasannya.

Karena itu gelombang ini hanya punya **satu** task frontend, dan sifatnya verifikasi.

### Yang sudah benar dan tidak perlu diubah

Diperiksa pada `96a91201`:

| Yang diperiksa | Hasil |
| --- | --- |
| `emergency-triage-form-view.jsx` baris 149 | Sudah punya `errorBanner` yang menampilkan `saveError` |
| `emergency-management-triage-slice.jsx` baris 279–281 | Jalur simpan sudah `catch` dan meneruskan `normalizeErrorMessage(error, …)` |

Pesan `409` dari backend karena itu **sudah punya jalan tampil**. Task di bawah membuktikannya
benar-benar tampil, bukan membangunnya dari nol.

---

## 1. Task

Grafik bagian 1 dan warisan revision `1` (bagian 2).

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph backend["Prasyarat backend — backend-roadmap.md"]
        BEIGD019["✅ BE-IGD-019<br/>Triase memakai penjaga"]:::luar
    end

    subgraph rev1["Prasyarat revision 1 — archive/revision-1/frontend-roadmap.md"]
        FEIGD004["✅ FE-IGD-004<br/>Selesai pada revision 1"]:::luar
        FEIGD007["✅ FE-IGD-007<br/>Selesai pada revision 1"]:::luar
        FEIGD008["✅ FE-IGD-008<br/>Selesai pada revision 1"]:::luar
        FEIGD009["✅ FE-IGD-009<br/>Selesai pada revision 1"]:::luar
    end

    FEIGD012["🟡 FE-IGD-012<br/>Penolakan 409 tampil"]:::sebagian
    FEIGD010["FE-IGD-010<br/>Detail kunjungan IGD"]:::belum

    BEIGD019 --> FEIGD012
    FEIGD004 --> FEIGD010
    FEIGD007 --> FEIGD010
    FEIGD008 --> FEIGD010
    FEIGD009 --> FEIGD010
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-IGD-019` ✅ | `FE-IGD-012` |
| 1 | `FE-IGD-004`, `007`, `008`, `009` — seluruhnya ✅ pada revision `1` | `FE-IGD-010` — **dapat dikerjakan sekarang** |

### 🟡 `FE-IGD-012` — Penolakan `409` jalur triase tampil dengan pesan backend

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — ditandai 15 September 2026.** Kriteria 4 terpetakan: normalisasi galat membaca `response.data.message` (laporan). Kriteria 1–3 menuntut **tangkapan layar** kedua penolakan dan status kunjungan sesudah penilaian ulang; tangkapan layar itu **belum ada** — laporan mencatat *"Bukti screenshot dan reload terhadap backend nyata belum dibuat"*. `IGD-DEC-110` tidak melepas bukti layar. Validasi historis: `npm run build` berhasil, `npm run lint` 0 error, `npm test` 46 lulus (laporan bagian `FE-IGD-019`). DoD bagian 4 butir 5 (alur simpan lewat layar) **belum terpenuhi**. Bukti: [laporan gabungan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| **Slice** | `IGD-S01` |
| **Scope** | `emergency-triage-form-view.jsx`, `emergency-management-triage-slice.jsx`, dan tab penilaian ulang pada `emergency-assessment-view` |
| **Perubahan** | Diharapkan **nol atau nyaris nol**. Task ini memverifikasi; perubahan hanya ditulis bila verifikasi gagal |
| **Requirement** | `FR-IGD-013`, `FR-IGD-014` — sisi tampilan |
| **Kontrak** | Validation `0.3.0` bagian 2 aturan 4 dan 5 — hash `0ee98b75…` |
| **Dependency** | **`BE-IGD-019` selesai dan berjalan.** Sebelum itu penolakan yang harus ditampilkan belum ada |
| **Acceptance** | 1. Menyelesaikan triase pada kunjungan yang sudah ditutup menampilkan pesan backend apa adanya — *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."* — **bukan** pesan cadangan *"Gagal menyimpan pemeriksaan triage."* 2. Penolakan transisi menampilkan pesan yang menyebut status kunjungan saat ini. 3. Tidak ada layar yang menampilkan status kunjungan sebagai `Triaged` setelah pasien `InTreatment` dinilai ulang. 4. Bila `normalizeErrorMessage` ternyata tidak membaca `response.data.message` untuk `409`, perbaiki **di tempat yang sudah ada**, jangan membuat penangan error tandingan |
| **Test** | `AT-IGD-086` sisi tampilan; `npm run lint` dan `npm test` tetap lulus |
| **Bukti** | Tangkapan layar kedua penolakan; catatan hasil untuk keempat butir acceptance |
| **Risiko** | Rendah |
| **Kewenangan UI** | **Tidak ada layar baru, tidak ada komponen baru, tidak ada CSS baru.** Bila butir 4 menuntut perubahan, ikuti `errorBanner` dan pola gaya yang sudah dipakai layar triase |
| **Owner** | Frontend |

---

## 2. Warisan revision `1` yang belum selesai

Task berikut **bukan** bagian `MVP-0`, tetapi belum dikerjakan dan tidak boleh hilang karena
pergantian revisi roadmap.

| Task | Isi | Keadaan | Catatan |
| --- | --- | --- | --- |
| `FE-IGD-010` | Halaman detail satu kunjungan IGD | **Belum dikerjakan** — diperiksa ulang 15 September 2026: route IGD di frontend hanya `emergency-assessment` dan `emergency-triage`. Seluruh dependency-nya (`FE-IGD-004`, `007`, `008`, `009`) sudah selesai | Rincian penuh ada di `roadmap/archive/revision-1/frontend-roadmap.md` bagian `FE-IGD-010` |

`FE-IGD-010` dapat dikerjakan kapan saja dan **tidak** bergantung pada gelombang ini. Ia juga
tidak terpengaruh `IGD-DEC-091`: penggantian nama `emergency-transfers` menjadi
`emergency-departures` baru berlaku pada `MVP-3`, dan `FE-IGD-010` menampilkan data, bukan
memanggil route perpindahan.

`FE-IGD-001` sampai `FE-IGD-009` dan `FE-IGD-011` sudah selesai pada revision `1`.

---

## 3. Yang menunggu gelombang berikutnya

> **Digantikan revision `3`.** Tabel ini benar saat revision `2` ditulis. Revision `3`
> menomori ulang gelombang — kepergian pasien pindah dari `MVP-3` ke `MVP-4`, dan pengkajian
> IGD naik dari `POST-MVP` ke `MVP-3` atas dasar bukti baru. Yang berlaku adalah bagian R3.
> Tabel ini disimpan sebagai catatan keadaan saat itu.

| Pekerjaan frontend | Menunggu | Sebabnya |
| --- | --- | --- |
| Mengganti `TRANSFER_URL` menjadi `emergency-departures` | `MVP-3` | `IGD-DEC-091`. Satu baris pada `emergency-assessment-slice.jsx:16`; **jangan** diubah sebelum backend-nya berganti |
| Layar dua rangkaian status kepergian | `MVP-3` | `IGD-DEC-090` |
| Layar koreksi dan pembalikan berpersetujuan | `MVP-3` | `IGD-DEC-090` |
| Serah terima SBAR dan sikap pesanan | `MVP-4` | `EPIC IGD-07` |
| Penanda unit tanpa kewenangan | `MVP-5` | `IGD-DEC-092`, dan pengesahan Security/Privacy owner |
| Layar pengkajian IGD tersimpan sungguhan | `POST-MVP` | Pemilik `ClinicalManagement` belum ditunjuk |

---

## 4. Definition of Done gelombang `MVP-0` — sisi frontend

| No | Butir | Bukti yang diterima |
| ---: | --- | --- |
| 1 | `FE-IGD-012` keempat butir acceptance-nya terjawab | Catatan hasil beserta tangkapan layar |
| 2 | `npm run lint` lulus | Keluaran perintah |
| 3 | `npm test` lulus | Keluaran perintah |
| 4 | Nol komponen, layar, atau modul CSS baru | Diff; bila kosong, sebutkan kosong |
| 5 | Alur simpan dijalankan sungguhan lewat layar | **Belum pernah terpenuhi** — butuh kredensial petugas. Bila masih belum ada, catat sebagai belum terbukti, jangan ditandai lulus |

Butir 5 adalah utang lama yang berlaku sejak revision `1` dan **tidak** diselesaikan gelombang
ini. Ia dicatat supaya tidak hilang, bukan supaya dianggap selesai.

---

# Revision 3 — perluasan ke perjalanan pasien penuh

Ditambahkan 26 Agustus 2026. Revision `2` di atas tetap berlaku.

## R3.0 Temuan yang mengubah bentuk pekerjaan frontend

Diperiksa pada `96a91201`.

**Layar pengkajian IGD sudah dibangun.** `emergency-assessment-view` memuat sebelas komponen:
SOAP, tanda vital, observasi, disposisi, transfer, catatan terintegrasi, nosokomial, rekam,
kartu pasien, kartu formulir, dan section. Ditambah `emergency-assessment-list-view` dan
`emergency-assessment-detail-view`.

Artinya pekerjaan frontend untuk pengkajian **bukan membangun layar**, melainkan membuktikan
bahwa layar yang sudah ada benar-benar menyimpan setelah backend dibuka. Utang lama
*"alur simpan lewat layar belum pernah dijalankan sungguhan"* akhirnya dapat dilunasi —
tetapi hanya setelah `BE-IGD-027`.

| Yang diperiksa | Hasil |
| --- | --- |
| `emergency-assessment-view` | **11 komponen tab**, lengkap |
| `emergency-triage` dan `emergency-registration` | Ada, punya halaman `page.jsx` |
| `app/…/emergency-pengkajian/` | **Folder kosong, nol berkas** — rute yang tidak pernah jadi |
| `components/view/…/emergency-installation-management/test/test.txt` | Berkas sisa yang tidak dipakai |

## R3.1 Task

### 🟡 `FE-IGD-013` — Pengkajian IGD benar-benar tersimpan lewat layar

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — ditandai 15 September 2026.** Kriteria 2 dan 3 terpetakan: payload `queueId: null` lewat `patient-assessment-payload.utils.js`, dan layar hanya menampilkan field yang punya rumah pada DTO. Jalur simpan backend-nya terbukti lewat API pada `BE-IGD-036` (`ASM-20260827-00003`). **Kriteria 1 dan 4 belum terbukti:** keduanya menuntut perawat menyimpan **lewat layar** lalu membukanya ulang, dengan tangkapan layar sebelum dan sesudah; itu belum pernah dijalankan. Validasi historis: `npm run build` berhasil, `npm run lint` 0 error, `npm test` 46 lulus. Bukti: [laporan gabungan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| **Slice** | `IGD-S04` · `EPIC IGD-09` |
| **Scope** | `emergency-assessment-view` beserta sebelas tab-nya |
| **Perubahan** | Diharapkan **kecil**. Layarnya sudah ada; yang dikerjakan adalah menyambungkan ke jalur simpan yang baru terbuka, dan memperbaiki hanya bagian yang terbukti gagal |
| **Requirement** | `FR-IGD-060` … `FR-IGD-064` sisi tampilan |
| **Dependency** | **`BE-IGD-027` dan `BE-IGD-028` selesai dan berjalan.** Sebelum itu tidak ada yang dapat dibuktikan |
| **Acceptance** | 1. Perawat mengisi pengkajian pasien IGD, menekan simpan, dan datanya **benar-benar ada** saat layar dibuka ulang. 2. Setiap field yang tampil di layar punya rumah di basis data — pelajaran `review-triage-igd-alur-penyimpanan` yang menemukan 17 dari 20 field hilang diam-diam. 3. Field yang belum punya rumah **ditandai jelas di laporan**, bukan dibiarkan tampil seolah tersimpan. 4. Tanda vital, diagnosis, tindakan, dan CPPT ikut dibuktikan |
| **Bukti** | Tangkapan layar sebelum dan sesudah muat ulang, plus kueri jumlah baris per tabel |
| **Risiko** | **Menengah.** Butuh kredensial petugas — utang yang belum pernah terlunasi sejak revision `1` |
| **Kewenangan UI** | Nol layar baru. Ikuti `emergency-triage.module.css` dan komponen `form-pemeriksaan-ui` yang sudah dipakai |
| **Owner** | Frontend |

### ✅ `FE-IGD-014` — Pendaftaran IGD mengikuti `EncounterType.Emergency`

| Field | Isi |
| --- | --- |
| **Status** | ✅ **DINILAI ULANG 21 September 2026 (malam) — selesai atas penilaian pemilik, sesudah `FE-IGD-034` ✅; celah `IGD-OQ-093` `open` dinyatakan** ([laporan](../task/report/frontend/FE-IGD-014.md) bagian 9). Kriteria 2 kini bersumber pra-cek `FE-IGD-034` (heuristik `GET emergency-visits?patientId=` dihapus) dan "membukanya" termasuk dalam pernyataan pemilik bahwa uji layar lulus. ✅ ini **tidak** menyatakan tidak ada encounter yatim dalam segala keadaan; encounter yatim yang sudah ada belum diaudit. *Penilaian sebelumnya (riwayat):* 🟡 **SEBAGIAN — dinilai ulang 21 September 2026.** Kriteria 1 terpetakan: `encounterType: ENCOUNTER_TYPE.Emergency` (`emergency-registration.utils.js:1118`). Kriteria 3 terpetakan: isian `duplicateEpisodeOverrideReason` ada (`emergency-visit-step.jsx:462`), dan backend menolak pendaftaran ganda tanpa alasan. **Kriteria 2 kini terpetakan ke source (21 September 2026):** sesudah penolakan, `verification-step.jsx` menampilkan kotak peringatan berisi nomor dan status kunjungan yang berjalan beserta tombol *Buka Kunjungan IGD* (layar Triage untuk pasien belum ditriage, Assesmen IGD untuk selebihnya), memakai `GET emergency-visits?patientId=` yang sudah ada — teks galat tidak diurai. Validasi: eslint berkas task 0 error, 9 unit test baru lulus, `UI GATE` REUSE 2 / NEW 0. `npm run build` **lulus** (pemilik, 21 September 2026 malam). **Uji layar kriteria 2 (pemilik, 21 September 2026 malam): kotak peringatan menampilkan nomor `IGD-260917023643-4A7A93` dan status yang sama dengan backend, tombol tampil.** **Belum:** hasil klik *Buka Kunjungan IGD* dan penanganan encounter yatim — tangkapan layar itu sendiri membuktikan yatim terjadi; status karena itu tetap 🟡. **Keputusan pemilik 21 September 2026 (`IGD-DEC-138`):** encounter yang terlanjur dibuat sebelum penolakan **tidak boleh dibiarkan yatim**; `FE-IGD-014` **belum boleh dianggap final end-to-end** sampai perilaku itu ditangani (`BE-IGD-050` lapis A lewat `FE-IGD-034`) atau dipisahkan eksplisit sebagai backend gap (`IGD-OQ-093`). Build dan uji layar kriteria 2 yang sudah ada tetap boleh dijalankan pemilik sekarang ([laporan](../task/report/frontend/FE-IGD-014.md) bagian 8). *Sebelum 21 September:* kriteria 2 tidak ada di source (`IGD-EV-123`). Layar ini juga disentuh `40f0e6106`/`5bc96f09b` (tim Rawat Inap, 29 Agustus) dan `c8613d88c` (30 Agustus) tanpa laporan. Bukti: [laporan gabungan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| **Slice** | `IGD-S02`, `IGD-S03` · `EPIC IGD-01`, `EPIC IGD-02` |
| **Scope** | `registration-management/emergency-registration` |
| **Perubahan** | Menyesuaikan jenis kunjungan yang dikirim, dan menampilkan penolakan episode ganda beserta **nomor kunjungan yang sudah ada** sebagai jalan pintas yang dapat diklik petugas |
| **Requirement** | `FR-IGD-001` … `FR-IGD-012` sisi tampilan |
| **Dependency** | `BE-IGD-023`, `BE-IGD-025` |
| **Acceptance** | 1. Pendaftaran IGD baru terkirim sebagai `Emergency`. 2. Pendaftaran kedua untuk pasien yang sama menampilkan nomor kunjungan pertama, dan petugas dapat langsung membukanya — bukan sekadar pesan gagal. 3. Jalan keluar beralasan tersedia dan alasannya wajib diisi |
| **Risiko** | Menengah. Ini pintu masuk pasien; salah sedikit, pendaftaran berhenti |
| **Owner** | Frontend |

### ✅ `FE-IGD-015` — Route `emergency-departures`

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 26 Agustus 2026; dipetakan ulang 15 September 2026.** Kedua acceptance criteria terpetakan ke source `43adae648`: `DEPARTURE_URL` → `emergency-departures` (`emergency-assessment-slice.jsx:17`); nol rujukan `emergency-transfers`; tab tetap di dalam `emergency-assessment/[slug]` sehingga nol URL halaman berubah. Validasi historis: `npm run build` berhasil, `npm run lint` 0 error, `npm test` 46 lulus. Bukti: [laporan gabungan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) (`IGD-DEC-113`) |
| **Slice** | `IGD-S05` · `EPIC IGD-05` |
| **Scope** | `emergency-assessment-slice.jsx` baris 16 — konstanta `TRANSFER_URL`. **Satu baris** |
| **Perubahan** | `emergency-transfers` menjadi `emergency-departures`, dirilis **bersamaan** dengan `BE-IGD-031`. Tidak ada route usang di backend, sehingga mendahului atau terlambat sama-sama memutus |
| **Keputusan** | `IGD-DEC-091` |
| **Dependency** | `BE-IGD-031` — **rilis serentak, bukan berurutan** |
| **Acceptance** | 1. Tab Transfer tetap bekerja. 2. Nol URL halaman berubah — tab ini ada di dalam `emergency-assessment/[slug]`, jadi nol bookmark petugas rusak |
| **Risiko** | Rendah secara teknis; **koordinasi rilisnya** yang berisiko |
| **Owner** | Frontend, serentak dengan Backend |

### ✅ `FE-IGD-016` — Dua rangkaian status kepergian

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 26 Agustus 2026; dipetakan ulang 15 September 2026.** Ketiga acceptance criteria terpetakan ke source `43adae648`: badge keadaan fisik dan keadaan dokumen tampil berdampingan, dan `actionsFor` (`emergency-assessment-transfer-tab.jsx:60–77`) menurunkan aksi dari **kedua** status sehingga kombinasi mustahil tidak ditawarkan. Tab ini dirombak `bd1d94a8a` (31 Agustus); kedua rangkaian tetap utuh. Validasi historis: `npm run build` berhasil, `npm run lint` 0 error. Bukti: [laporan gabungan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| **Slice** | `IGD-S05` · `EPIC IGD-05` |
| **Scope** | `emergency-assessment-transfer-tab.jsx` |
| **Perubahan** | Satu status tunggal menjadi dua yang berdampingan: keadaan fisik pasien dan keadaan serah terima. Keduanya bergerak sendiri-sendiri |
| **Keputusan** | `IGD-DEC-090` |
| **Dependency** | `BE-IGD-032` |
| **Acceptance** | 1. Kedua rangkaian terbaca sekaligus tanpa perlu berpindah tab. 2. Kombinasi yang mustahil tidak dapat dipilih. 3. Petugas dapat membedakan "pasien sudah berangkat" dari "unit tujuan sudah menerima" — dua hal yang selama ini tertukar |
| **Risiko** | Menengah. Dua status berdampingan mudah membingungkan bila penamaannya tidak jelas |
| **Owner** | Frontend |

### ✅ `FE-IGD-017` — Entri susulan, koreksi, dan daftar pantau

| Field | Isi |
| --- | --- |
| **Status** | ✅ **22 September 2026 — atas penilaian pemilik**: `npm run build` dinyatakan lulus (keluaran tidak dilampirkan; artefak `.next` 21 September 16:03 dari commit `c941012ac` yang memuat kedua berkas task ini diperiksa agent), sehingga satu-satunya butir penahan tertutup. Uji layar kriteria 1 sudah lulus 21 September. `BE-IGD-049` kini ✅. *Riwayat penilaian:* 🟡 **SEBAGIAN — Implementation Complete 21 September 2026 (larut malam); blokir `BE-IGD-049` sebelumnya dicabut.** Syarat pemilik (`IGD-DEC-137`: *`BE-IGD-049` kontrak selesai **dan** build terverifikasi*) **terpenuhi**: `BE-IGD-049` 🟡 Implementation Complete, `dotnet build` 0 error ([laporan](../task/report/backend/BE-IGD-049.md)). **Dikerjakan:** dua nilai pada `emergency-assessment-transfer-tab.jsx` kini membaca `recordedByName`/`approvedByName`, tanda hubung bila kosong, **tanpa** jatuh ke ID (`IGD-DEC-137`). `eslint` 0 error; 5 unit test baru lulus (dengan kontrol negatif terhadap `HEAD`); suite 1415 dari 1424 lulus (9 gagal sudah ada sebelumnya, semuanya Rawat Inap); grep anti-regresi bersih; `UI GATE` REUSE 2 / NEW 0. **Uji layar kriteria 1 (pemilik, 21 September 2026 larut malam): LULUS** — kolom *Pelaku* menampilkan `SuperAdmin` pada ketiga kejadian `DEP-260921064559-9B2DB7`, menggantikan GUID pada uji sebelumnya. **Belum:** `npm run build` revisi ini tidak dilaporkan pemilik — satu-satunya butir yang menahan ✅ (standar yang sama dengan `FE-IGD-027`). [Laporan](../task/report/frontend/FE-IGD-017.md). Kriteria 2 dan 3 tetap terpenuhi (15 September 2026). *Riwayat: 🟡 15 September 2026; dihentikan pada gerbang 21 September 2026.* Kriteria 1 tertahan **delta kontrak backend**: respons event kepergian hanya memuat `recordedByUserId`/`approvedByUserId` tanpa nama, kontrak terkunci tidak menjanjikannya, dan frontend tidak punya pencarian pengguna — menebak payload atau memanggil endpoint pengguna satu per satu ditolak ([laporan](../task/report/frontend/FE-IGD-017.md)). Keputusan pemilik sudah ada (`IGD-DEC-137`, aditif, pola `IGD-DEC-129`) dan task backend `BE-IGD-049` sudah direncanakan; sisi frontend sesudahnya dua baris. Nol source diubah. Sebelumnya: Kriteria 2 dan 3 terpetakan: kejadian yang dikoreksi tetap tampil dengan badge "Tidak berlaku" (`emergency-assessment-transfer-tab.jsx:229`), dan waktu sebenarnya di masa depan ditolak di layar (baris 141 dan 258). **Kriteria 1 sebagian:** riwayat tampil bersama waktu terjadi dan waktu dicatat, tetapi kolom "Pelaku" dan "Penyetuju pembalikan" (baris 234–235) menampilkan **ID pengguna mentah**, bukan nama petugas (`IGD-EV-123`). Bukti: [laporan gabungan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| **Slice** | `IGD-S05` · `EPIC IGD-06` |
| **Scope** | Tab kepergian, ditambah satu daftar pantau |
| **Perubahan** | Mencatat waktu kejadian sebenarnya yang berbeda dari waktu pencatatan; menampilkan riwayat koreksi tanpa menyembunyikan yang lama; pembalikan menampilkan siapa yang menyetujui |
| **Keputusan** | `IGD-DEC-065`, `IGD-DEC-066`, `IGD-DEC-085`, `IGD-DEC-090` |
| **Dependency** | `BE-IGD-033`, `BE-IGD-034`, **`BE-IGD-049`** (ditambahkan 21 September 2026 — nama pelaku) |
| **Acceptance** | 1. Riwayat kejadian terbaca urut beserta pelakunya. 2. Baris yang sudah dikoreksi **tetap terlihat**, ditandai tidak berlaku — tidak dihapus dari layar. 3. Waktu sebenarnya di masa depan ditolak di layar, bukan hanya di backend |
| **Risiko** | Menengah |
| **Owner** | Frontend |

### ✅ `FE-IGD-018` — Bersih-bersih sisa yang tidak dipakai

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 26 Agustus 2026; dipetakan ulang 15 September 2026.** Ketiga acceptance criteria terpetakan ke source `43adae648`: folder `emergency-pengkajian` tidak ada; `test/test.txt` sudah dihapus — folder `test/` tersisa kosong dan tidak terlacak git; nol rujukan ke keduanya. Validasi historis: `npm run build` berhasil, `npm run lint` 0 error. Bukti: [laporan gabungan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) |
| **Slice** | Kebersihan. Bukan bagian epic mana pun |
| **Scope** | `app/health-services/emergency-installation-management/emergency-pengkajian/` yang **kosong**, dan `components/view/…/emergency-installation-management/test/test.txt` |
| **Perubahan** | Menghapus keduanya **setelah dipastikan tidak ada yang menunjuk ke sana**. Folder rute kosong pada App Router membingungkan: ia tampak seperti halaman yang ada padahal tidak |
| **Dependency** | Tidak ada |
| **Acceptance** | 1. Penelusuran menunjukkan nol rujukan ke keduanya. 2. `npm run build` dan `npm run lint` tetap lulus. 3. Nol rute yang sebelumnya bekerja menjadi rusak |
| **Risiko** | Rendah. **Periksa dulu, hapus kemudian** |
| **Owner** | Frontend |

## R3.2 Urutan

Pohon teks sebelumnya diganti Mermaid pada 15 September 2026; kedelapan hubungannya muncul
kembali sebagai panah. `BE-IGD-031` → `FE-IGD-015` tetap berarti **rilis serentak**, bukan
berurutan. `FE-IGD-010` ada pada grafik bagian 1.

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph backend["Prasyarat backend — backend-roadmap.md"]
        BEIGD023["✅ BE-IGD-023<br/>Encounter Emergency diterima"]:::luar
        BEIGD025["✅ BE-IGD-025<br/>Satu pasien satu episode"]:::luar
        BEIGD027["✅ BE-IGD-027<br/>Pengkajian tanpa antrean"]:::luar
        BEIGD028["✅ BE-IGD-028<br/>Konsultasi tanpa antrean"]:::luar
        BEIGD031["🟡 BE-IGD-031<br/>Transfer menjadi Departure"]:::luar
        BEIGD032["✅ BE-IGD-032<br/>Dua kolom status kepergian"]:::luar
        BEIGD033["✅ BE-IGD-033<br/>Kejadian kepergian tambah-saja"]:::luar
        BEIGD034["✅ BE-IGD-034<br/>Koreksi dan pembalikan berpersetujuan"]:::luar
        BEIGD049["✅ BE-IGD-049<br/>Nama pelaku pada event kepergian"]:::luar
    end

    FEIGD013["🟡 FE-IGD-013<br/>Pengkajian tersimpan lewat layar"]:::sebagian
    FEIGD014["✅ FE-IGD-014<br/>Pendaftaran kirim Emergency"]:::selesai
    FEIGD015["✅ FE-IGD-015<br/>Route emergency-departures"]:::selesai
    FEIGD016["✅ FE-IGD-016<br/>Dua status kepergian tampil"]:::selesai
    FEIGD017["✅ FE-IGD-017<br/>Riwayat koreksi kepergian"]:::selesai
    FEIGD018["✅ FE-IGD-018<br/>Sisa tak terpakai dihapus"]:::selesai

    BEIGD023 --> FEIGD014
    BEIGD025 --> FEIGD014
    BEIGD027 --> FEIGD013
    BEIGD028 --> FEIGD013
    BEIGD031 --> FEIGD015
    BEIGD032 --> FEIGD016
    BEIGD033 --> FEIGD017
    BEIGD034 --> FEIGD017
    BEIGD049 --> FEIGD017
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `FE-IGD-018` |
| 1 | `BE-IGD-023` ✅, `BE-IGD-025` ✅ | `FE-IGD-014` |
| 1 | `BE-IGD-027` ✅, `BE-IGD-028` ✅ | `FE-IGD-013` |
| 1 | `BE-IGD-031` 🟡 — rilis serentak | `FE-IGD-015` |
| 1 | `BE-IGD-032` ✅ | `FE-IGD-016` |
| 1 | `BE-IGD-033` ✅, `BE-IGD-034` ✅, `BE-IGD-049` ✅ (selesai 22 September 2026) | `FE-IGD-017` ✅ |

## R3.3 Yang belum dapat direncanakan

Penunjang medis, pemakaian alat, dan billing IGD **belum punya blueprint**, sehingga belum
punya task frontend. Lihat `backend-roadmap.md` bagian R3.5.

Satu hal yang sudah pasti sekarang: layar penunjang medis tidak dapat menampilkan hasil
pemeriksaan, karena `LabOrder` **tidak menyimpan hasil sama sekali** — hanya `EncounterId` dan
`ProcedureId`. Layar apa pun yang dibuat sekarang hanya akan menampilkan daftar pesanan
kosong.

---

## R3.4 Gelombang 27 Agustus 2026

Laporan lengkapnya di
`task/report/frontend/fe-igd-020-021-route-master-igd-dan-kolom-kesimpulan.md`.

| Task | Judul | Status | Laporan |
| --- | --- | --- | --- |
| `FE-IGD-020` | Route master data IGD mengikuti pemindahan modul | ✅ **SELESAI 27 Agustus 2026; dipetakan ulang 15 September 2026.** Seluruh pemanggilan master IGD memakai `…/emergency-installation-management/master-data/…`; nol rujukan route lama. Keenam endpoint dijalankan pada route baru → `200`, route lama → `404`. Validasi: `node --import ./tests/helpers/register.mjs --test tests/unit` 119 lulus, 0 gagal; `npm run lint` 0 error, 570 warning lama | [fe-igd-020-021](../task/report/frontend/fe-igd-020-021-route-master-igd-dan-kolom-kesimpulan.md) |
| `FE-IGD-021` | Kolom "Kesimpulan" pada Riwayat Observasi tidak pernah terisi | ✅ **SELESAI 27 Agustus 2026; dipetakan ulang 15 September 2026.** Layar membaca `completionSummary` (`emergency-assessment-observation-tab.jsx:493`). Catatan: kolom itu baru benar-benar terisi dari layar setelah `IGD-DEC-115` diimplementasikan di backend | [fe-igd-020-021](../task/report/frontend/fe-igd-020-021-route-master-igd-dan-kolom-kesimpulan.md) |

Grafik gelombang 27 Agustus. `FE-IGD-020` mengikuti perubahan route oleh `BE-IGD-037` dan wajib
naik bersamaan; `FE-IGD-021` tidak punya prasyarat.

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph backend["Prasyarat backend — backend-roadmap.md"]
        BEIGD037["✅ BE-IGD-037<br/>Master data IGD pindah modul"]:::luar
    end

    FEIGD020["✅ FE-IGD-020<br/>Route master data IGD"]:::selesai
    FEIGD021["✅ FE-IGD-021<br/>Kolom Kesimpulan observasi"]:::selesai

    BEIGD037 --> FEIGD020
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-IGD-037` ✅ — rilis serentak | `FE-IGD-020` |
| 1 | — | `FE-IGD-021` |

### `FE-IGD-020` — RILIS SERENTAK

`BE-IGD-037` mengubah route master data IGD:

```
sebelum : /v1/health-services/master-data/emergency-installation-management/<res>
sesudah : /v1/health-services/emergency-installation-management/master-data/<res>
```

Lima pemanggilan di empat berkas disesuaikan; nol rujukan route lama tersisa.

> Ini perubahan kontrak, sama seperti `FE-IGD-015`. Frontend baru terhadap backend lama akan
> `404` pada seluruh pilihan master IGD — cara kedatangan dan jenis kasus pada layar
> pendaftaran, level dan indikator triase, jenis tindak lanjut pada layar pengkajian.
> **Keduanya wajib naik bersamaan.**

### `FE-IGD-021` dan pemeriksaan kolom layar

Tab Observasi merujuk `conclusion`, sedangkan `EmergencyObservationResponse` menamainya
`completionSummary`. Kolomnya kosong selamanya tanpa pernah melempar galat.

Ditemukan lewat pemeriksaan menyeluruh yang membandingkan **setiap kolom yang dideklarasikan
layar pengkajian** dengan isi respons daftar sungguhan. Sembilan bagian diperiksa; tiga cacat
ditemukan — satu diperbaiki di frontend, dua di backend (`BE-IGD-038`). Sesudahnya, bagian yang
punya data menunjukkan **nol kolom hilang**.

**Pelajaran:** kolom yang dideklarasikan layar tetapi tidak dikirim endpoint tidak pernah
tampil sebagai galat, hanya kosong. Membaca kode layar saja tidak cukup, membaca DTO saja juga
tidak — keduanya terlihat wajar sendiri-sendiri.

### ~~Peringatan perkakas: `npm test` dapat lulus tanpa menjalankan test~~ — DIPERBAIKI 17 September 2026

`npm test` memakai `node --test "tests/unit/**/*.test.mjs"`. Node **v20** belum mendukung glob
pada `--test` — dukungan itu masuk pada Node 21 — sehingga perintahnya gagal menemukan berkas
dan **nol test berjalan**.

**Dua koreksi, 17 September 2026.**

1. **Exit code-nya bukan `0`, melainkan `1`.** Klaim lama ditulis dari pembacaan yang salah:
   keluaran `npm test` dipipa ke `tail`, sehingga yang terbaca exit code `tail`, bukan `npm`.
   Diukur ulang pada Node `v20.20.2`: `npm run test:unit` gagal dengan exit `1`. Jadi ini
   **bukan** hijau palsu — CI akan merah, bukan lolos diam-diam. Bahayanya jauh lebih kecil
   daripada yang tertulis sebelumnya.
2. **Sudah diperbaiki.** `package.json` skrip `test:unit` kini memakai
   `node --import ./tests/helpers/register.mjs --test tests/unit` — direktori, bukan glob, jadi
   benar pada Node 20 maupun 21. Diverifikasi 17 September 2026: `npm test` → **866 lulus,
   0 gagal, exit 0**.

Per 27 Agt (historis, lewat perintah manual): **119 lulus, 0 gagal**.

---

## R3.5 Gelombang 28 Agustus 2026 — layar asuhan keperawatan IGD

Laporan lengkapnya di
`task/report/frontend/fe-igd-022-asuhan-keperawatan-pengkajian-observasi-penunjang.md`.

| Task | Judul | Status | Laporan |
| --- | --- | --- | --- |
| `FE-IGD-022` | Pengkajian lanjutan, pemantauan observasi, dan penunjang medis | 🟡 **SEBAGIAN** — lihat baris Status pada kartu | [fe-igd-022](../task/report/frontend/fe-igd-022-asuhan-keperawatan-pengkajian-observasi-penunjang.md) |

Grafik `FE-IGD-019` dan `FE-IGD-022`.

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph backend["Prasyarat backend — backend-roadmap.md"]
        BEIGD026["🟡 BE-IGD-026<br/>QueueId pengkajian opsional"]:::luar
        BEIGD027["✅ BE-IGD-027<br/>Pengkajian tanpa antrean"]:::luar
        BEIGD036["✅ BE-IGD-036<br/>Migration diterapkan, simpan terbukti"]:::luar
    end

    FEIGD019["✅ FE-IGD-019<br/>Assesmen awal pakai formulir bersama"]:::selesai
    FEIGD022["🟡 FE-IGD-022<br/>Asuhan keperawatan IGD lengkap"]:::sebagian

    BEIGD026 --> FEIGD019
    BEIGD027 --> FEIGD019
    BEIGD026 --> FEIGD022
    BEIGD027 --> FEIGD022
    BEIGD036 --> FEIGD022
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-IGD-026` 🟡 (kolomnya sudah ada; yang belum hanya uji langkah mundur migration), `BE-IGD-027` ✅ | `FE-IGD-019` |
| 1 | `BE-IGD-026` 🟡 (kolomnya sudah ada; yang belum hanya uji langkah mundur migration), `BE-IGD-027` ✅, `BE-IGD-036` ✅ | `FE-IGD-022` |

### ✅ `FE-IGD-019` — Assesmen Awal IGD berisi pemeriksaan yang sebenarnya

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 26 Agustus 2026; kartu susulan dan pemetaan ulang 15 September 2026.** Keenam acceptance criteria terpetakan ke source `43adae648`: `emergency-assessment-initial-tab.jsx` meng-import `VitalSignTab` (baris 13) dan `AssessmentTab` (baris 12) lalu merendernya berurutan (baris 146, 154); `buildPatientAssessmentPayload` dipakai tab IGD dan `use-nurse-station-queue.js`; `queueId` bawaan `null` (`patient-assessment-payload.utils.js:42`); prop `saveError`/`canSubmit`/`disabledHint` diteruskan dengan nama yang benar (baris 140–142, dan `emergency-assessment-transfer-tab.jsx:165–167`); `toNullableText` mengubah isian kosong menjadi `null` (`patient-assessment-payload.utils.js:34`, dipakai baris 53); `tests/unit/patient-assessment-payload.test.mjs` ada. Isi tab tetap utuh setelah perombakan `bd1d94a8a`. Validasi historis: `npm run build` berhasil, `npm run lint` 0 error, `npm test` 46 lulus. Uji simpan lewat layar belum dijalankan — tercatat pada `FE-IGD-013`, bukan acceptance task ini. Bukti: [laporan gabungan](../task/report/frontend/fe-igd-012-018-penyelesaian-antarmuka.md) (`IGD-DEC-113`) |
| **Outcome** | Tab Assesmen Awal IGD memuat tanda vital, pemeriksaan pernapasan dan kesadaran, pengkajian awal, dan tujuh kolom pengkajian nyeri — memakai formulir yang sama dengan skrining perawat rawat jalan |
| **Slice** | `IGD-S04` · `EPIC IGD-09` |
| **Requirement** | `FR-IGD-060` sisi tampilan |
| **Keputusan** | Permintaan owner 26 Agustus 2026 (tercatat pada laporan); `IGD-DEC-107` untuk perubahan pada layar milik Registrasi |
| **Scope** | `emergency-assessment-initial-tab.jsx`; `emergency-assessment-transfer-tab.jsx` (prop kartu); `utils/health-services/clinical-management/patient-assessment-payload.utils.js` (baru, diekstrak dari `use-nurse-station-queue.js`); `tests/unit/patient-assessment-payload.test.mjs` |
| **Dependency** | `BE-IGD-026`, `BE-IGD-027` |
| **Acceptance** | 1. `VitalSignTab` dan `AssessmentTab` dipakai apa adanya, bukan ditiru. 2. Tanda vital beserta oksigen dan kesadaran tampil sebelum pengkajian awal dan pengkajian nyeri. 3. Satu pembentuk payload `POST /patient-assessments` dipakai tab IGD dan skrining perawat. 4. `queueId` dikirim `null`, bukan Guid nol. 5. Tombol simpan mati saat isian belum lengkap, dan galat simpan tampil — prop `canSubmit`/`saveError` benar. 6. Nilai kosong dikirim sebagai `null`, bukan string kosong |
| **Risiko** | Rendah |
| **Owner** | Frontend |

### 🟡 `FE-IGD-022`

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — ditandai 15 September 2026.** Ketiga bagian (delapan kolom pengkajian, pemantauan observasi, tab Penunjang) ada di source `43adae648`; validasi 28 Agustus: `npm run lint:errors` bersih, 119/119 unit test, `npm run build` berhasil. **Yang menahan:** (1) uji lewat layar belum pernah dijalankan (laporan bagian 13, *MANUAL TEST: NOT FEASIBLE*); (2) tab Penunjang kini **cacat** — `fetchLabOrders` memanggil `lab-orders` tanpa `encounterId`, padahal sejak 4 September 2026 backend hanya mengirim 25 pesanan terbaru dari seluruh rumah sakit (`IGD-EV-112`); (3) layar masih menyatakan *"modul Radiologi belum ada"*, bertentangan dengan `IGD-DEC-111` dan `RAD-DEC-009` (`IGD-EV-115`). Layar ini juga dirombak `bd1d94a8a` (31 Agustus) tanpa laporan, termasuk perbaikan privasi `IGD-EV-117`. Bukti: [laporan](../task/report/frontend/fe-igd-022-asuhan-keperawatan-pengkajian-observasi-penunjang.md) |
| **Slice** | `IGD-S04` · `EPIC IGD-09` |
| **Scope** | `emergency-assessment-slice.jsx`, `emergency-assessment-constant.jsx`, `emergency-assessment-initial-tab.jsx`, `emergency-assessment-observation-tab.jsx`, `emergency-assessment-detail-view.jsx`, `use-emergency-assessment-detail.jsx`, `emergency-assessment.module.css`, ditambah satu komponen baru `emergency-assessment-diagnostic-support-tab.jsx` |
| **Perubahan** | Tiga bagian: (a) delapan kolom pengkajian yang dikirim payload tetapi tidak pernah punya isian; (b) pemantauan berkala `EmergencyObservationDetail` beserta aksi status periode; (c) tab Penunjang Medis tersambung ke `laboratory-management/lab-orders` |
| **Kontrak** | **Nol perubahan.** Seluruh endpoint sudah ada sejak `BE-IGD-026`…`035` |
| **Dependency** | `BE-IGD-026`, `BE-IGD-027`, `BE-IGD-036` — seluruhnya selesai |
| **Kewenangan UI** | Nol modul CSS baru, nol komponen bersama diubah. Satu komponen tab baru mengikuti `EmergencyAssessmentFormCard`/`EmergencyAssessmentSection` yang sudah ada; satu aturan CSS `.recordItem[data-active]` |
| **Validasi** | `npm run lint:errors` lulus; 119/119 unit test lulus; `npm run build` lulus |
| **Owner** | Frontend |

### Cacat yang diperbaiki: `OBSERVATION_STATUS_OPTIONS` salah memetakan enum

Daftar status pembukaan periode observasi memetakan `3` sebagai **"Dibatalkan"**. Enum backend
`EmergencyObservationStatus` menempatkan `3` sebagai **`Escalated`** dan `4` sebagai
`Cancelled`.

Akibatnya perawat yang memilih "Dibatalkan" justru membuat periode ber-status `Escalated` —
dan `Escalated` memindahkan status kunjungan ke `InTreatment`. Kebalikan dari yang diniatkan,
dan tanpa satu pun pesan galat.

Diperbaiki dengan mempersempit daftar pembukaan menjadi `Aktif` saja. Ketiga status lain
dicapai lewat aksi pada periode yang sudah ada, mengikuti `CanTransition`.

**Pelajaran, sejalan dengan `FE-IGD-021`:** daftar pilihan berisi angka enum yang ditulis
tangan tidak pernah melempar galat ketika angkanya meleset — ia mengirim perintah yang salah
dengan sukses. Cocokkan setiap daftar semacam itu terhadap enum backend, bukan terhadap label
yang terbaca masuk akal.

### Empat celah backend yang dilaporkan, bukan ditambal

1. `PATCH .../observation-status` **membuang** `notes` untuk `Completed` dan `Cancelled` —
   cabang refleksi mencari properti `Notes` yang tidak dimiliki `EmgObservation`. Akibatnya
   `CompletionSummary` tidak punya jalan tulis dari layar sama sekali.
2. `GET /laboratory-management/lab-orders` **nol parameter** dan mengembalikan seluruh tabel;
   penyaringan per pasien terpaksa di sisi klien.
3. Sikap pesanan serah terima (`order-items`, lima route) **nol pemakai di frontend** —
   padahal `BE-IGD-035` menahan penutupan kunjungan bila ada pesanan tanpa sikap. Kunjungan
   dapat tertahan tanpa jalan keluar lewat antarmuka.
4. `EmergencyResuscitationController` **nol pemakai di frontend**.

Nomor 3 dan 4 adalah pekerjaan frontend berikutnya yang paling bernilai.

> **Diperiksa ulang 15 September 2026** ([evidence](../evidence/2026-09-15-pemeriksaan-status.md)
> `IGD-EV-109`…`112`):
>
> | Celah | Keadaan |
> | --- | --- |
> | 1. Catatan `observation-status` dibuang | **Masih terbuka.** Diputuskan `IGD-DEC-115`: `Completed` → `CompletionSummary`; alasan `Cancelled` jadi `IGD-OQ-083` |
> | 2. `lab-orders` tanpa parameter | **Tertutup di backend** 4 September 2026 (paging + `encounterId`), tetapi **berubah menjadi cacat di layar ini**: `fetchLabOrders` belum mengirim `encounterId`, sehingga pesanan pasien bisa tidak tampil |
> | 3. `order-items` nol pemakai | **Masih terbuka.** Keempat route tulisnya ikut ditolak `403` untuk setiap petugas selama `BE-IGD-039` belum beres — layar tulis sikap pesanan **belum** layak dibangun; tampilan baca lewat `GET` dapat dibangun |
> | 4. Resusitasi nol pemakai | **Masih terbuka.** Tidak terhalang kewenangan unit |
>
> Urutan nilai berubah: yang **pertama** kini perbaikan tab Penunjang (celah 2 + teks radiologi
> `IGD-DEC-111`), bukan sikap pesanan.
>
> Kalimat R3.3 *"`LabOrder` tidak menyimpan hasil sama sekali — hanya `EncounterId` dan
> `ProcedureId`"* juga sudah usang: `LabOrder` kini punya status dan spesimen; yang masih nol
> hanya kolom hasil pemeriksaan.

---

## R3.6 Gelombang 15 September 2026 — perbaikan layar, riwayat dokter, dan laporan susulan

Direncanakan `plan-module-delivery` pada 15 September 2026 dari temuan
[evidence/2026-09-15-pemeriksaan-status.md](../evidence/2026-09-15-pemeriksaan-status.md) dan
keputusan `IGD-DEC-111`, `IGD-DEC-116`…`121`. **Belum ada source yang ditulis.**

DoD baku seluruh kartu di bagian ini, kecuali disebut lain: acceptance criteria terpetakan ke
source; `npm run lint:errors` dan `node --import ./tests/helpers/register.mjs --test tests/unit`
dijalankan dan hasilnya dicatat apa adanya; perintah `npm run build` diberikan kepada Rizki;
catatan uji layar ditulis apa adanya; laporan tracked `task/report/frontend/<TASK-ID>.md`;
roadmap dan traceability diperbarui; nol komponen bersama dan CSS global diubah; tanpa UAT PASS.

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph backend["Prasyarat backend — backend-roadmap.md"]
        BEIGD040["✅ BE-IGD-040<br/>Kesimpulan observasi tersimpan"]:::luar
        BEIGD045["BE-IGD-045<br/>Dokter ditetapkan, dialihkan, dicari"]:::luar
    end

    DEC111{{"✅ IGD-DEC-111<br/>Radiologi belum disambungkan"}}:::selesai
    DEC121{{"✅ IGD-DEC-121<br/>Kesimpulan opsional"}}:::selesai
    FEIGD023["✅ FE-IGD-023<br/>Tab Penunjang baca pesanan pasien"]:::selesai
    FEIGD024["✅ FE-IGD-024<br/>Kesimpulan diisi saat Selesaikan"]:::selesai
    FEIGD025["FE-IGD-025<br/>Laporan perombakan pengkajian"]:::belum
    FEIGD026["FE-IGD-026<br/>Laporan layar pendaftaran"]:::belum
    FEIGD027["✅ FE-IGD-027<br/>Triase pakai riwayat dokter"]:::selesai

    DEC111 --> FEIGD023
    BEIGD040 --> FEIGD024
    DEC121 --> FEIGD024
    BEIGD045 --> FEIGD027
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `IGD-DEC-111` ✅ | `FE-IGD-023` — **dapat dikerjakan sekarang** |
| 1 | — | `FE-IGD-025`, `FE-IGD-026` — boleh paralel |
| 1 | `BE-IGD-040` (belum dikerjakan), `IGD-DEC-121` ✅ | `FE-IGD-024` — mulai setelah `BE-IGD-040` selesai |
| 1 | `BE-IGD-045` (belum dikerjakan) | `FE-IGD-027` — mulai setelah `BE-IGD-045` selesai |

### ✅ `FE-IGD-023` — Tab Penunjang Medis membaca pesanan milik pasien

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI (implementasi) 15 September 2026.** Delapan acceptance criteria dipetakan ke source frontend `RizkiV2` (belum di-commit, dasar `43adae648`). `npm run lint:errors` exit 0, nol error; `node --import ./tests/helpers/register.mjs --test tests/unit` **686 test, 686 lulus**; `npm run test:unit` tidak berjalan di Node 20 karena pola glob (`EXISTING / ENVIRONMENT ISSUE`). `npm run build` **tidak dijalankan agent** — perintahnya diserahkan kepada Rizki sesuai DoD. Uji layar dan tangkapan layar: `NOT FEASIBLE` — dev server dan kredensial petugas tidak tersedia bagi agent. **Runtime verified: belum.** Bukan UAT. Nol perubahan backend. Bukti: [laporan](../task/report/frontend/FE-IGD-023.md). *Keadaan sebelumnya: direncanakan 15 September 2026, belum dikerjakan* |
| **Outcome** | Perawat melihat seluruh pesanan laboratorium milik pasien yang sedang dibuka, dan layar menyatakan keadaan radiologi dengan benar |
| **Slice** | `IGD-S05` · `EPIC IGD-07` (keterbatasan penunjang dinyatakan di layar) |
| **Requirement** | `FR-IGD-046` — keterbatasan penunjang dinyatakan di layar |
| **Keputusan** | `IGD-DEC-105` (IGD hanya memesan dan membaca), `IGD-DEC-111`; bukti `IGD-EV-112`, `IGD-EV-115` |
| **Kontrak** | API Laboratorium apa adanya, milik `LaboratoryManagement`: `GET api/v1/health-services/laboratory-management/lab-orders` dengan `LabOrderPagedQuery` (`pageNumber`, `pageSize` bawaan 25, `encounterId`). Nol kontrak IGD berubah |
| **Reuse** | Helper `unwrapPaged` pada `emergency-assessment-slice.jsx`; tab Penunjang yang sudah ada |
| **Scope** | `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx` — `fetchLabOrders` baris 531–542 dan komentar baris 628–629; `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-diagnostic-support-tab.jsx` baris 189 |
| **Dependency** | `IGD-DEC-111` ✅. Dependency eksternal yang **sengaja tidak disambungkan**: pemesanan radiologi ke `POST rad-orders`, ditahan `IGD-DEC-111` butir (d) sampai `ActAsRadiologist` dapat diberikan (pemilik Radiologi) |
| **Acceptance** | 1. Request membawa `encounterId`, `pageNumber`, dan `pageSize`; penyaringan pesanan di browser dihapus. 2. **Tanpa `encounterId`, request tidak dikirim sama sekali.** 3. Pesanan pasien tetap tampil walau banyak pesanan lain dibuat sesudahnya — contoh: dari 40 pesanan sehari, pesanan urutan ke-12 milik pasien tetap tampil. 4. Bila `totalData` lebih besar dari jumlah baris yang dimuat, layar menyebut jumlah seluruhnya dan menyediakan cara memuat sisanya memakai paging backend; bentuknya `DEV_DISCRETION`. 5. Kalimat *"modul Radiologi belum ada"* **tidak muncul lagi** di layar maupun komentar. Penggantinya menyatakan bahwa pemesanan radiologi dari IGD **belum disambungkan**, dan permintaannya dicatat sebagai pesanan luar sistem pada serah terima pasien (`IGD-DEC-111`). 6. **Nol tombol pemesanan radiologi, nol integrasi Radiologi**, dan nol tombol alur di dalam laboratorium. 7. Keterangan *"hasil pemeriksaan belum dapat ditampilkan"* tetap ada selama respons laboratorium belum memuat hasil. 8. Nol perubahan backend |
| **Bukti** | DoD baku bagian ini; tangkapan layar tab Penunjang untuk pasien yang punya pesanan, bila backend dan kredensial tersedia — bila tidak, dinyatakan `NOT FEASIBLE` beserta alasannya |
| **Risiko** | Rendah |
| **Owner** | Frontend |

### ✅ `FE-IGD-024` — Isian Kesimpulan saat menyelesaikan periode observasi

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI (implementasi) 15 September 2026.** Enam acceptance criteria dipetakan ke source frontend `RizkiV2` (belum di-commit, dasar `f37e949ea`): `emergency-assessment-observation-tab.jsx` dan `emergency-assessment-constant.jsx`. `npm run lint:errors` exit 0, nol error; `node --import ./tests/helpers/register.mjs --test tests/unit` **852 test, 852 lulus**; `npm run test:unit` tidak berjalan di Node 20 karena pola glob (`EXISTING / ENVIRONMENT ISSUE`). **Build = Not Verified** — `npm run build` diserahkan kepada Rizki. Uji peramban: `NOT FEASIBLE` bagi agent. **Runtime verified: belum.** Dependency `BE-IGD-040`: source selesai, build dan runtime belum diverifikasi. Bukan UAT. Bukti: [laporan](../task/report/frontend/FE-IGD-024.md). *Keadaan sebelumnya: direncanakan 15 September 2026, menunggu `BE-IGD-040`* |
| **Outcome** | Perawat dapat menulis kesimpulan saat menyelesaikan observasi, dan kesimpulan itu langsung tampil pada riwayat |
| **Slice** | `IGD-S04` · layar observasi |
| **Requirement** | **Coverage gap:** tidak ada `FR-IGD-*`; dijejak ke keputusan (lihat `BE-IGD-040`) |
| **Keputusan** | `IGD-DEC-115`, `IGD-DEC-119`, `IGD-DEC-121`, `IGD-OQ-083` |
| **Kontrak** | `PATCH .../emergency-observations/{id}/observation-status` dengan `{ observationStatus, notes }` — bentuk tidak berubah; perilaku mengikuti `BE-IGD-040` |
| **Reuse** | Aksi status periode pada `emergency-assessment-observation-tab.jsx`; thunk `updateObservationStatus` yang sudah mengirim `notes`; kolom Kesimpulan riwayat dari `FE-IGD-021` |
| **Scope** | `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-observation-tab.jsx`; bila perlu, konstanta aksi di `emergency-assessment-constant.jsx` |
| **Dependency** | `BE-IGD-040`; `IGD-DEC-121` ✅ |
| **Acceptance** | 1. Memilih aksi **Selesaikan** menampilkan isian **Kesimpulan**, paling banyak 1000 karakter. 2. Isian **opsional**: kosong pun penyelesaian tetap dapat dikirim. 3. Setelah berhasil, riwayat observasi menampilkan kesimpulan itu tanpa memuat ulang halaman. 4. Aksi **Batalkan** tidak berubah — tanpa isian alasan (`IGD-OQ-083`). 5. Aksi eskalasi tidak ikut diubah. 6. Pesan `400` *"Catatan paling banyak 1000 karakter."* dan `409` dari backend tampil apa adanya. Bentuk isian `DEV_DISCRETION`, mengikuti pola aksi beralasan yang sudah ada di tab ini |
| **Bukti** | DoD baku bagian ini |
| **Risiko** | Rendah |
| **Owner** | Frontend |

### `FE-IGD-025` — Laporan susulan perombakan layar pengkajian (`bd1d94a8a`) dan temuan privasi

| Field | Isi |
| --- | --- |
| **Status** | **Direncanakan 15 September 2026 — belum dikerjakan.** Task laporan; **nol perubahan source** |
| **Outcome** | Perombakan 31 Agustus 2026 punya laporan tracked, dan temuan privasi `IGD-EV-117` tercatat lengkap untuk pemilik `ClinicalManagement` |
| **Slice** | `IGD-S04` · lanjutan `FE-IGD-022` |
| **Requirement** | — (laporan atas perubahan yang sudah di-commit) |
| **Keputusan** | `IGD-DEC-107` (IGD pemegang wewenang sementara `ClinicalManagement`); bukti `IGD-EV-117` |
| **Scope** | Baca-saja: 19 berkas pada `bd1d94a8a`, dan controller daftar `ClinicalManagement` di backend untuk memeriksa perilaku tanpa filter. Tulis: `task/report/frontend/FE-IGD-025.md`, status roadmap, traceability |
| **Dependency** | — |
| **Acceptance** | 1. Laporan merinci perubahan 19 berkas, termasuk alasan tab Tanda Vital IGD terpisah dihapus dan di mana isiannya kini berada. 2. Penjaga lingkup data dijelaskan dengan contoh sebelum/sesudah: sembilan thunk lewat `requiredScope` dan `fetchNosocomialInfections`. 3. Laporan mendaftar endpoint daftar klinis yang menjawab permintaan **tanpa filter** dengan data seluruh pasien, dengan bukti `path + method + baris` dari source backend. 4. Temuan itu diserahkan sebagai rekomendasi kepada pemilik `ClinicalManagement` — **tidak diperbaiki** pada task ini. 5. **Nol perubahan source** di kedua repository |
| **Bukti** | Laporan tracked; `git diff` source kosong |
| **Risiko** | Rendah untuk task-nya; **temuannya berisiko tinggi** (privasi data pasien) |
| **Owner** | Frontend |
| **DoD** | Acceptance 1–5 terpenuhi; laporan tracked ada; roadmap dan traceability diperbarui |

### `FE-IGD-026` — Laporan susulan layar pendaftaran IGD (`c8613d88c`)

| Field | Isi |
| --- | --- |
| **Status** | **Direncanakan 15 September 2026 — belum dikerjakan.** Task laporan; **nol perubahan source** |
| **Outcome** | Perubahan layar pendaftaran IGD yang di-commit tanpa laporan kini tercatat, termasuk validasi layar yang dinonaktifkan |
| **Slice** | `IGD-S02`, `IGD-S03` · `EPIC IGD-01`, `EPIC IGD-02` |
| **Requirement** | `FR-IGD-001`…`012` sisi tampilan (dibaca, tidak diubah) |
| **Keputusan** | `IGD-DEC-084`, `IGD-DEC-107` |
| **Scope** | Baca-saja: `registration-management/emergency-registration/` — `emergency-visit-step.jsx`, `payment-method-step.jsx`, `verification-step.jsx`, dan `emergency-registration.module.css` pada `c8613d88c` (30 Agustus 2026). Juga dicatat `40f0e6106`/`5bc96f09b` (tim Rawat Inap, 29 Agustus 2026) yang menyentuh `emergency-registration-stepper.jsx` dan `patient-entry-choice-step.jsx`. Tulis: `task/report/frontend/FE-IGD-026.md` |
| **Dependency** | — |
| **Acceptance** | 1. Laporan merinci perubahan ketiga commit dan siapa pembuatnya. 2. Setiap validasi layar yang dinonaktifkan dicatat lalu dibandingkan dengan validation §1 — contoh: komentar *"TEMPORARY: required dinonaktifkan"* pada alias sementara pasien, padahal §1 aturan 3 mewajibkan nama sementara. 3. Laporan menyatakan apakah backend tetap menegakkan aturan yang dinonaktifkan di layar, dengan bukti baris source. 4. **Nol perubahan source** |
| **Bukti** | Laporan tracked; `git diff` source kosong |
| **Risiko** | Rendah |
| **Owner** | Frontend |
| **DoD** | Acceptance 1–4 terpenuhi; laporan tracked ada; roadmap dan traceability diperbarui |

### ✅ `FE-IGD-027` — Layar triase memakai riwayat penugasan dokter

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI — dinaikkan kembali 21 September 2026 atas keputusan pemilik**, sesudah build revisi terbaru lulus. Ketujuh acceptance terpetakan ke source. **Per revisi:** *Implementation Complete* = ya (revisi terbaru, commit `16c767916`). *Scoped eslint* = `PASS` dan *unit test IGD* = 38/38 `PASS`. *Runtime inti* = **PASS 18 September 2026 pada revisi `3213419a7`** — 13 pemeriksaan lewat layar oleh pemilik ([evidence](../evidence/2026-09-18-verifikasi-runtime-fe-igd-027.md)). *`npm run build`* = **lulus revisi terbaru, dijalankan pemilik 21 September 2026** (`Compiled successfully`, 362/362 halaman statis, `postbuild` standalone berhasil; keluaran dilampirkan pemilik pada percakapan, bukan diulang agent). **Yang tidak terbukti lewat layar dan diterima pemilik sebagai catatan, bukan penghalang:** terminologi "Dokter Penanggung Jawab IGD" dan tampilan **"Sumber: Data historis"** untuk baris legacy `IGD-DEC-136` belum pernah dilihat di layar, sebab dev punya 0 baris legacy (`BE-IGD-048`); baru terlihat bila ada basis data berbaris legacy. **UAT belum dan tidak diklaim**. [Laporan](../task/report/frontend/FE-IGD-027.md) |
| **Outcome** | Penetapan dan pengalihan dokter IGD dilakukan lewat `Emergency Doctor Assignment`, dan petugas melihat riwayat dokter penanggung jawab, bukan hanya dokter sekarang |
| **Slice** | `IGD-S06` · `EPIC IGD-04` |
| **Requirement** | `FR-IGD-016`…`021` sisi tampilan |
| **Keputusan** | `IGD-DEC-082`, `IGD-DEC-116`, `IGD-DEC-117`; `03-frontend-architecture.md` bagian 4 |
| **Kontrak** | API §3 `Emergency Doctor Assignment` (`IGD-DEC-116`) dengan query `at` (`IGD-DEC-117`) dan **proyeksi nama §3.2** pada `0.7.0` (`IGD-DEC-129`) — dipakai persis seperti yang dibangun `BE-IGD-045`. Penanda penugasan berjalan dibaca dari `effectiveTo` yang kosong, **bukan** `isActive` (`IGD-DEC-130`) |
| **Reuse** | Layar triase IGD yang ada; mengganti pemanggilan `PATCH /patient-encounters/{id}/doctor` pada `emergency-management-triage-slice.jsx` baris 530 |
| **Scope** | `src/lib/state/slice/health-services/emergency-installation-management/emergency-management-triage-slice.jsx`; komponen layar triase yang menampilkan dokter pada `emergency-management-triage-view/` |
| **Dependency** | `BE-IGD-045` |
| **Acceptance** | 1. Layar IGD **tidak lagi bergantung** pada endpoint Registrasi `PATCH /patient-encounters/{id}/doctor` untuk penetapan dokter IGD. 2. Penetapan dokter pertama memakai `POST /`. 3. Pengalihan memakai `POST /{id}/handover` dengan alasan **wajib** diisi di layar. 4. Riwayat tampil berurutan waktu: dokter, sejak kapan, sampai kapan, alasan pengalihan; baris aktif dibedakan — bentuknya `DEV_DISCRETION`. 5. Penolakan `409` tampil beserta arahan memakai aksi pengalihan. 6. Dokter dan penugas tampil sebagai **nama**, bukan ID pengguna — pelajaran `IGD-EV-123`. Namanya dibaca dari `doctorName` dan `assignedByName` yang dikirim backend (`IGD-DEC-129`); layar **dilarang** meminta nama per baris riwayat, dan **dilarang** menampilkan GUID sebagai tampilan cadangan utama — nama kosong tampil sebagai tanda hubung. 7. Baris yang sedang berjalan dikenali dari `effectiveTo` yang kosong, **bukan** dari ruas `isActive` yang memang tidak ada (`IGD-DEC-130`) |
| **Bukti** | DoD baku bagian ini |
| **Risiko** | Menengah — layar triase dipakai setiap hari |
| **Owner** | Frontend |

### Gap yang dicatat tanpa ID task

Atas instruksi Product/Domain Owner 15 September 2026, dua gap berikut **tidak** diberi ID task
dan **tidak** memakai `FE-IGD-024` maupun `FE-IGD-025`:

| Gap | Bukti | Catatan |
| --- | --- | --- |
| Layar resusitasi IGD | `IGD-EV-111` | `EmergencyResuscitationController` nol pemakai; tidak terhalang kewenangan unit |
| Layar baca/aksi pesanan kepergian (`order-items`) | `IGD-EV-109` | Aksi tulis terhalang `BE-IGD-039`; tampilan baca tidak |

---

## R3.7 Gelombang 16 September 2026 — pemantauan observasi bertanda vital

Lahir dari audit Observasi V1 lawan V2
([evidence](../evidence/2026-09-15-audit-observasi-v1-v2.md)) dan keputusan `IGD-DEC-122`
sampai `IGD-DEC-126`. Kontrak yang mengikat: API `0.6.0` bagian 7 dan validation `0.6.0`
bagian 9; skema layar ada di `03-frontend-architecture.md` bagian 12. **Source sudah ditulis
16 September 2026** dan build-nya bersih; uji lewat layar belum dijalankan.

DoD baku gelombang ini: acceptance criteria terpetakan ke source; `npm run lint:errors` dan
`node --import ./tests/helpers/register.mjs --test tests/unit` dijalankan dan hasilnya dicatat
apa adanya; perintah `npm run build` diberikan kepada Rizki; catatan uji layar ditulis apa
adanya; laporan tracked `task/report/frontend/<TASK-ID>.md`; roadmap dan traceability
diperbarui; nol komponen bersama dan CSS global diubah; tanpa UAT PASS.

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph backend["Prasyarat backend — backend-roadmap.md"]
        BEIGD046["✅ BE-IGD-046<br/>Validasi dan proyeksi tanda vital"]:::luar
    end

    DEC122{{"✅ IGD-DEC-122<br/>Tanda vital ditautkan"}}:::selesai
    DEC123{{"✅ IGD-DEC-123<br/>ABCDE baca saja"}}:::selesai
    FEIGD024["✅ FE-IGD-024<br/>Kesimpulan saat Selesaikan"]:::selesai
    FEIGD028["✅ FE-IGD-028<br/>Pemantauan bertanda vital"]:::selesai

    BEIGD046 --> FEIGD028
    DEC122 --> FEIGD028
    DEC123 --> FEIGD028
    FEIGD024 --> FEIGD028
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-IGD-046` dinyatakan siap dipakai layar | `FE-IGD-028` ✅ |

Panah dari `FE-IGD-024` berarti urutan sentuhan berkas, bukan ketergantungan data: keduanya
menyentuh `emergency-assessment-observation-tab.jsx`, dan `FE-IGD-028` dibangun di atas layar
hasil `FE-IGD-024` tanpa mengubah alur penyelesaian periode.

### ✅ `FE-IGD-028` — Pemantauan observasi dengan tanda vital tertaut

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI (implementasi) 16 September 2026.** Seluruh 13 acceptance criteria dipetakan ke source: tab Observasi (+689/−109; +595/−15 bila indentasi diabaikan), `emergency-assessment-slice.jsx` (+28), `emergency-assessment-constant.jsx` (+44), hook detail (+5/−1), view detail (+2), util `patient-vital-sign-payload.utils.js` baru, dan satu berkas test unit baru. Nol komponen bersama, nol CSS, nol endpoint baru. `npm run lint:errors` **PASS**; `node --import ./tests/helpers/register.mjs --test tests/unit` **857/857 lulus** (852 lama + 5 baru); `npm run test:unit` tetap gagal karena glob skrip — masalah lama yang sama seperti `FE-IGD-023`/`FE-IGD-024`. **Build = Verified** — `npm run build` dijalankan Rizki 16 September 2026, berhasil tanpa error beserta tahap `postbuild`. Uji lewat layar: `NOT FEASIBLE` bagi agent. **Runtime verified: belum.** Bukan UAT. Delta tercatat pada kriteria 8: ringkasan ABCDE ternyata belum pernah dimuat workspace, sehingga satu thunk daftar triase ditambahkan memakai endpoint yang sudah ada — atas keputusan pemilik 16 September 2026. Bukti: [laporan](../task/report/frontend/FE-IGD-028.md). *Keadaan sebelumnya: direncanakan 16 September 2026, belum dikerjakan* |
| **Outcome** | Perawat mencatat satu putaran pemantauan beserta tanda vitalnya dari satu layar, dan riwayat pemantauan menampilkan angka tanda vital, GCS, kesadaran, oksigen, serta nama pencatatnya |
| **Slice** | `IGD-S04` · `EPIC IGD-09` (pemantauan observasi) |
| **Requirement** | **Coverage gap:** tidak ada `FR-IGD-*`; dijejak ke keputusan (lihat `BE-IGD-046`) |
| **Keputusan** | `IGD-DEC-122`, `IGD-DEC-123`, `IGD-DEC-124`, `IGD-DEC-125`, `IGD-DEC-126`; `IGD-DEC-056` |
| **Kontrak** | API `0.6.0` bagian 7 (termasuk 7.3 endpoint tanda vital milik `ClinicalManagement`) dan validation `0.6.0` bagian 9. Skema layar: `03-frontend-architecture.md` bagian 12 |
| **Reuse** | Tab Observasi yang sudah ada; formulir tanda vital bersama yang sudah dipakai tab Assesmen Awal; `EmergencyAssessmentFormCard`, `EmergencyAssessmentSection`, `ConfirmModal`, `InformationAlert`; thunk daftar dan simpan yang sudah ada pada `emergency-assessment-slice.jsx` |
| **Scope** | `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-observation-tab.jsx`; `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx`; bila perlu `emergency-assessment-constant.jsx`. **Nol komponen bersama diubah, nol CSS global** |
| **Dependency** | `BE-IGD-046`; `IGD-DEC-122` ✅. Tidak menunggu `BE-IGD-045` |
| **Acceptance** | 1. Bagian **Tanda Vital** tersedia di dalam *Catat Pemantauan*. 2. Petugas dapat mencatat tanda vital baru memakai kemampuan formulir tanda vital yang sudah ada, tanpa berpindah tab. 3. Petugas dapat memilih tanda vital yang sudah tercatat, dan daftarnya **hanya** memuat tanda vital pasien dan encounter kunjungan itu — penyaring dikirim ke backend, bukan disaring di browser. 4. Yang dikirim ke `emergency-observation-details` hanya `patientVitalSignId`; **nol** angka tanda vital ikut dikirim. 5. Riwayat pemantauan menampilkan angka tanda vital dari proyeksi backend, bukan dari isian formulir. 6. Riwayat menampilkan GCS E/V/M beserta total, kesadaran, dan oksigen **bila** backend mengirimnya; layar tidak menghitung total sendiri. 7. Riwayat menampilkan nama pencatat dari `recordedByName`; kosong ditampilkan sebagai tanda hubung, **bukan** GUID. 8. Ringkasan ABCDE penilaian triase terakhir tampil sebagai konteks **baca saja**, memakai data yang sudah dimuat workspace — tanpa endpoint baru. 9. Periode `Completed` dan `Cancelled` tidak menyediakan jalan masuk pencatatan pemantauan normal; bila backend tetap menolak `409`, pesannya tampil apa adanya. 10. Isian Kesimpulan dan alur Selesaikan milik `FE-IGD-024` **tetap bekerja**. 11. **Nol** isian obat, gambaran EKG, atau DC Shock ditambahkan. 12. Layar Resusitasi **tidak** disentuh. 13. Nilai kosong tampil sebagai tanda hubung; kosong berarti tidak diukur, bukan nol |
| **Bukti** | DoD baku bagian ini; tangkapan layar tab Observasi sesudah satu putaran pemantauan bertanda vital, bila backend dan kredensial tersedia — bila tidak, dinyatakan `NOT FEASIBLE` beserta alasannya |
| **Risiko** | **Menengah.** Dua permintaan berurutan (tanda vital lalu pemantauan) tidak atomik: bila permintaan kedua gagal, baris tanda vital tetap tersimpan sebagai pengukuran sah dan layar wajib menjelaskannya, bukan menghapusnya |
| **Owner** | Frontend |

---

## R3.8 Gelombang 16 September 2026 (kedua) — kunjungan keluar dari `Arrived`

Lahir dari temuan pemakaian layar oleh Product/Domain Owner
([evidence](../evidence/2026-09-16-kunjungan-terjebak-arrived.md), `IGD-EV-131`…`IGD-EV-136`)
dan keputusan `IGD-DEC-127` serta `IGD-DEC-128`.

**Gelombang ini memperbaiki modul yang tidak dapat dipakai, bukan menambah kemampuan.**
Kunjungan IGD lahir `Arrived`, dan ketiga jalan keluar yang sah menurut kontrak state bagian 1
sama-sama tidak punya pemanggil di layar. Akibatnya perawat **tidak pernah** dapat menyimpan
pemeriksaan triage: setiap penyimpanan dijawab `409` *"Status kunjungan tidak dapat berubah dari
Arrived ke Triaged."* Seluruh jalur sesudah triage — pengkajian, observasi, disposisi, kepergian
— ikut tidak terjangkau untuk pasien baru.

**Kontrak: nol perubahan.** Kedua transisi yang dipakai gelombang ini sudah sah dan sudah
`approved` sejak `IGD-DEC-093`. Tidak ada endpoint baru, tidak ada kenaikan versi kontrak, dan
`CanTransition` **tidak** boleh disentuh.

DoD baku gelombang ini: acceptance criteria terpetakan ke source; `npm run lint:errors` dan
`node --import ./tests/helpers/register.mjs --test tests/unit` dijalankan dan hasilnya dicatat
apa adanya; perintah `npm run build` diberikan kepada Rizki; catatan uji layar ditulis apa
adanya; laporan tracked `task/report/frontend/<TASK-ID>.md`; roadmap dan traceability
diperbarui; nol komponen bersama dan CSS global diubah; tanpa UAT PASS.

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph backend["Prasyarat backend — backend-roadmap.md"]
        BEIGD018["✅ BE-IGD-018<br/>Penjaga transisi status kunjungan"]:::luar
    end

    DEC127{{"✅ IGD-DEC-127<br/>Pendaftaran menutup Menunggu Triage"}}:::selesai
    DEC128{{"✅ IGD-DEC-128<br/>Penanganan cepat lewat aksi status"}}:::selesai
    FEIGD029["🟡 FE-IGD-029<br/>Pendaftaran menutup Menunggu Triage"]:::sebagian
    FEIGD030["✅ FE-IGD-030<br/>Aksi Tangani Segera"]:::selesai

    DEC127 --> FEIGD029
    BEIGD018 --> FEIGD030
    DEC128 --> FEIGD030
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | Prasyaratnya sudah selesai seluruhnya — boleh mulai sekarang, dan keduanya boleh paralel | `FE-IGD-029` 🟡, `FE-IGD-030` ✅ |

Kedua task **tidak** saling bergantung: `FE-IGD-029` membuka jalur normal, `FE-IGD-030` membuka
jalur cepat. Mengerjakan salah satunya saja sudah memberi nilai, tetapi hanya keduanya bersama
yang menutup seluruh lubang pada `IGD-EV-133` dan `IGD-EV-134`.

### 🟡 `FE-IGD-029` — Pendaftaran IGD menutup dengan status Menunggu Triage

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — 16 September 2026.** Implementasi selesai dan build bersih: default `visitStatus` payload pendaftaran kini `WaitingForTriage`, ditambah dua test unit yang mengunci perilakunya. Kriteria 1, 3, 4, 5, dan 6 **terpenuhi** — kriteria 1 **terbukti lewat layar 16 September 2026**: pasien yang baru didaftarkan langsung berstatus "Menunggu Triage". Kriteria 2 **belum terbukti**: pada uji itu pasien barunya ditekan **Tangani Segera**, bukan **Isi Triage**, sehingga transisi `WaitingForTriage` → `Triaged` belum dilalui lewat layar. Validasi: `npm run lint:errors` **PASS**; `node --import ./tests/helpers/register.mjs --test tests/unit` **859/859 lulus** (857 lama + 2 baru); `npm run build` **berhasil**. Uji lewat layar: `NOT FEASIBLE` bagi agent. Bukti: [laporan](../task/report/frontend/FE-IGD-029.md). *Keadaan sebelumnya: direncanakan 16 September 2026, belum dikerjakan* |
| **Outcome** | Pasien yang baru didaftarkan di IGD langsung berada pada antrean triage, sehingga perawat dapat menyimpan pemeriksaan triage tanpa ditolak `409` |
| **Slice** | `IGD-S01` · `EPIC IGD-01` (pendaftaran dan triage) |
| **Requirement** | `FR-IGD-013`, `FR-IGD-015` — sisi tampilan |
| **Keputusan** | `IGD-DEC-127`; `IGD-DEC-093` (kontrak state bagian 1 `approved`) |
| **Kontrak** | State `0.4.0` bagian 1 — **nol perubahan**. Transisi `WaitingForTriage` → `Triaged` sudah sah. Nol endpoint baru; payload `POST /emergency-visits` yang sudah ada hanya berganti nilai |
| **Reuse** | Konstanta `EMERGENCY_VISIT_STATUS` beserta label dan warna badge-nya yang sudah lengkap di `emergency-registration.constants.js` baris 260-300 |
| **Scope** | `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` baris 1174-1177. **Nol layar, nol komponen, nol CSS** |
| **Dependency** | `IGD-DEC-127` ✅. Tidak menunggu backend — tidak ada perubahan backend sama sekali |
| **Acceptance** | 1. Pendaftaran IGD yang tuntas menyimpan kunjungan dengan `visitStatus = 2` (`WaitingForTriage`); daftar triage menampilkannya sebagai **"Menunggu triage"**, bukan "Pasien tiba". 2. Menyimpan pemeriksaan triage pada pasien yang baru didaftarkan **berhasil**, dan status kunjungan berpindah ke `Triaged`. 3. Nilai `registrationStatus` dan `registrationCompletedAt` **tidak** berubah perilakunya. 4. Bila pemanggil menyertakan `context.visitStatus` secara eksplisit, nilai itu **tetap** dihormati — yang berubah hanya nilai default. 5. Pasien lama yang terlanjur `Arrived` **tidak** ikut berpindah sendiri; perbaikannya lewat `FE-IGD-030` atau tindakan data terpisah, dan itu dinyatakan apa adanya. 6. Nol perubahan pada backend, `CanTransition`, maupun berkas kontrak |
| **Bukti** | DoD baku bagian ini; tangkapan layar daftar triage sesudah satu pendaftaran baru, memperlihatkan badge "Menunggu triage"; tangkapan layar penyimpanan triage yang berhasil. Bila kredensial atau backend tidak tersedia, dinyatakan `NOT FEASIBLE` beserta alasannya |
| **Risiko** | **Rendah pada kode, menengah pada arti data.** Satu nilai berubah, tetapi `Arrived` praktis berhenti dihasilkan lewat layar pendaftaran — konsekuensi yang sudah diterima `IGD-DEC-127` dan dicatat sebagai `IGD-OQ-091` |
| **Owner** | Frontend |

### ✅ `FE-IGD-030` — Aksi Tangani Segera pada daftar triage

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 16 September 2026 — kedelapan acceptance criteria terpenuhi dan terbukti lewat layar.** Implementasi pada lima berkas: thunk `startEmergencyImmediateCare` beserta cabang state `immediateCare` pada slice triage, aksi pada hook daftar, dialog konfirmasi `ConfirmModal` dengan isian alasan opsional, tombol **Tangani Segera** pada kolom AKSI, dan satu kelas penataan letak `.tableActionCell`. Nol layar baru, nol komponen bersama diubah, nol CSS global. **Uji lewat layar oleh pemilik, ketiganya lulus:** (1) BAGUS SETIAWAN berpindah "Pasien tiba" → "Sedang ditangani", grant izin `EmergencyVisit` + `Update` terbukti ada; (2) pengalihan ke layar Assesmen IGD bekerja pada NABILA PUTRI MAHARANI; (3) triage susulan pada pasien `InTreatment` tersimpan dan status **tidak mundur** — `IGD-DEC-104` huruf (b) terbukti ujung ke ujung. Dua penyempurnaan atas keputusan pemilik pada hari yang sama: spanduk berhasil menyebut nama pasien, dan perawat dialihkan langsung ke Assesmen IGD memakai route token yang sama dengan daftar pengkajian — delta kriteria 3 (perubahan badge tidak lagi diamati perawat) dicatat di laporan. Validasi: `npm run lint:errors` **PASS**; `node --import ./tests/helpers/register.mjs --test tests/unit` **859/859 lulus**; `npm run build` **berhasil**; grep anti-regresi warna, tipografi, dan kelas bootstrap **nihil**. **Bukan UAT** — UAT milik tim terpisah dan belum dijalankan. Satu pilihan tampilan masih terbuka: kedua tombol pada kolom AKSI tampil identik; lihat laporan bagian 8. Bukti: [laporan](../task/report/frontend/FE-IGD-030.md). *Keadaan sebelumnya: 🟡 sebagian, uji layar belum dijalankan* |
| **Outcome** | Perawat dapat mendahulukan pasien gawat dengan satu tombol: kunjungan berpindah ke `InTreatment`, tim langsung bekerja, dan pengkajian triage-nya disusulkan tanpa memundurkan status |
| **Slice** | `IGD-S01` · `EPIC IGD-01` (pendaftaran dan triage) |
| **Requirement** | **Coverage gap:** tidak ada `FR-IGD-*` yang menuliskan jalur penanganan cepat; dijejak ke `IGD-DEC-128` |
| **Keputusan** | `IGD-DEC-128`; `IGD-DEC-104` huruf (b); `IGD-DEC-093` |
| **Kontrak** | State `0.4.0` bagian 1 — **nol perubahan**. Endpoint `PATCH /emergency-visits/{id}/visit-status` sudah ada sejak sebelum gelombang ini; bentuknya ada pada [evidence](../evidence/2026-09-16-kunjungan-terjebak-arrived.md) bagian G |
| **Reuse** | Kolom AKSI dan `styles.primaryMiniButton` yang sudah dipakai tombol "Isi Triage"; `ConfirmModal` dan pola toast/`errorBanner` yang sudah dipakai layar triage; thunk dan service triage yang sudah ada |
| **Scope** | `emergency-triage-patient-table.jsx` (kolom AKSI), `emergency-triage-patient-list-view.jsx`, service dan slice triage yang sudah ada. **Nol layar baru, nol komponen bersama diubah, nol CSS global** |
| **Dependency** | `BE-IGD-018` ✅ (penjaga transisi); `IGD-DEC-128` ✅ |
| **Acceptance** | 1. Baris pasien berstatus `Arrived` atau `WaitingForTriage` menampilkan aksi **Tangani Segera** di samping "Isi Triage"; status lain **tidak** menampilkannya. 2. Aksi itu meminta konfirmasi lebih dulu memakai pola konfirmasi yang sudah ada, bukan langsung mengirim. 3. Berhasil → kunjungan menjadi `InTreatment`, badge daftar berubah menjadi "Sedang ditangani" tanpa muat ulang halaman penuh. 4. Alasan singkat boleh diisi dan dikirim sebagai `notes`; dikosongkan tetap sah. 5. Penolakan backend tampil **apa adanya** dari `response.data.message` — termasuk `400` untuk transisi ilegal dan `403` bila izin `EmergencyVisit` + `Update` belum diberikan; **jangan** membuat penangan galat tandingan, dan **jangan** berasumsi transisi ilegal selalu `409` (`IGD-EV-136`). 6. Sesudah pasien `InTreatment`, tombol "Isi Triage" **tetap** bekerja, penilaian tersimpan, dan status **tetap** `InTreatment` — `IGD-DEC-104` huruf (b). 7. `TreatmentStartedAt` **tidak** dikirim dari layar; backend yang mengisinya. 8. Layar Resusitasi **tidak** dibangun dan **tidak** disentuh |
| **Bukti** | DoD baku bagian ini; tangkapan layar sebelum dan sesudah aksi pada satu pasien, ditambah tangkapan layar penyimpanan triage susulan yang berhasil dengan status tetap `InTreatment`. Bila kredensial atau izin tidak tersedia, dinyatakan `NOT FEASIBLE` beserta alasannya |
| **Risiko** | **Menengah.** Aksi ini memindahkan pasien ke penanganan tanpa triage — sah dan disengaja, tetapi tidak dapat dibatalkan lewat layar: kontrak tidak menyediakan jalan kembali dari `InTreatment` ke `WaitingForTriage`. Konfirmasi pada butir 2 ada khusus untuk itu. Risiko kedua: izin `EmergencyVisit` + `Update` belum terverifikasi pada peran perawat triage |
| **Owner** | Frontend |

---

## R3.9 Gelombang 16 September 2026 (keempat) — tata letak riwayat pada ruang kerja pemeriksaan

Lahir dari tinjauan tampilan layar Assesmen IGD oleh Product/Domain Owner
([evidence](../evidence/2026-09-16-tata-letak-riwayat-pemeriksaan.md)) dan keputusan
`IGD-DEC-133` serta `IGD-DEC-134`.

> **Penomoran hari itu.** Pass ketiga pada 16 September 2026 adalah penyelarasan kesiapan
> `EPIC IGD-04`, yang **tidak menambah task** dan karena itu tidak punya bagian sendiri di
> roadmap ini. Gelombang ini adalah pass keempat, tetapi bagian roadmap frontend ketiga.

**Gelombang ini memperbaiki cara data yang sudah ada dibaca, bukan menambah data.** Riwayat tiap
formulir pemeriksaan sudah tersimpan benar dan sudah dimuat ulang benar, tetapi duduk di bawah
formulir panjang sehingga praktis tidak terbaca. Lebih merugikan lagi: sesudah simpan berhasil,
posisi gulir tidak berpindah, sehingga yang terlihat perawat hanyalah formulir yang tiba-tiba
kosong — bentuk umpan balik yang sama persis dengan kegagalan simpan.

Quilvian V1 sudah menyelesaikan keduanya dengan memisahkan formulir dan riwayat menjadi dua tab.
Pola perilakunya diambil; **cara membangunnya tidak**. V2 sudah memasang satu `role="tablist"`
untuk tujuh tab utama, dan `ClinicalSegmentedNav` di repository ini ditulis justru untuk menolak
penyarangan tablist kedua. Rinciannya pada evidence bagian D.

**Kontrak: nol perubahan.** Tidak ada endpoint baru, tidak ada kenaikan versi, tidak ada ruas
data baru, dan **backend tidak disentuh sama sekali**. Yang berubah hanya susunan tampilan.

**Wewenang UI sudah terdelegasi** dan tidak menuntut approval baru:

| Hal | Wewenang | Bukti |
| --- | --- | --- |
| Bentuk tab, modal, atau drawer | `DEV_DISCRETION` | `03-frontend-architecture.md:247` |
| Bentuk pemilihan radio, tab, atau tombol | `DEV_DISCRETION`, mengikuti komponen yang sudah ada | `:328` |
| Susunan kolom riwayat dan urutannya | `DEV_DISCRETION` | `:329` |
| Tanda vital sebagai ringkasan satu baris atau tabel | `DEV_DISCRETION` | `:330` |
| Isi data yang ditampilkan dan sumber datanya | **Bukan** `DEV_DISCRETION` — dikunci | `:331` |

DoD baku gelombang ini: acceptance criteria terpetakan ke source; `npm run lint:errors` dan
`node --import ./tests/helpers/register.mjs --test tests/unit` dijalankan dan hasilnya dicatat
apa adanya; perintah `npm run build` diberikan kepada Rizki; catatan uji layar ditulis apa
adanya; laporan tracked `task/report/frontend/<TASK-ID>.md`; roadmap dan traceability
diperbarui; nol perubahan backend; nol CSS global diubah; nol palet warna baru; nol pustaka
komponen baru; tanpa UAT PASS.

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph lain["Prasyarat dari R3.7 — bagian lain roadmap ini"]
        FEIGD028["✅ FE-IGD-028<br/>Pemantauan bertanda vital"]:::luar
    end

    DEC133{{"✅ IGD-DEC-133<br/>Segmen Formulir dan Riwayat"}}:::selesai
    DEC134{{"✅ IGD-DEC-134<br/>Tata letak tab Observasi"}}:::selesai
    FEIGD031["🟡 FE-IGD-031<br/>Segmen Formulir dan Riwayat"]:::sebagian
    FEIGD032["🟡 FE-IGD-032<br/>Tata letak tab Observasi"]:::sebagian

    DEC133 --> FEIGD031
    DEC134 --> FEIGD032
    FEIGD031 --> FEIGD032
    FEIGD028 --> FEIGD032
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `IGD-DEC-133` ✅ — tidak menunggu task mana pun | `FE-IGD-031` |
| 2 | `FE-IGD-031` selesai, dan `FE-IGD-028` ✅ yang sudah selesai 16 September 2026 | `FE-IGD-032` |

`FE-IGD-032` menunggu `FE-IGD-031` karena segmen pada tab Observasi memakai **pembungkus yang
sama** yang dibuat `FE-IGD-031`. Memecahnya menjadi dua kartu disengaja: bila penataan ulang tab
Observasi tersendat, kelima tab lain tetap dapat ditutup ✅ sendiri.

### 🟡 `FE-IGD-031` — Segmen Formulir dan Riwayat pada tab pemeriksaan

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — 16 September 2026.** Implementasi selesai: satu komponen pembungkus baru (`emergency-assessment-work-panel.jsx`), satu util baru beserta 7 test unit (`emergency-assessment-summary.utils.js`), dan lima tab memakainya. Ketiga belas acceptance criteria **terpetakan ke source**. Validasi: `npm run lint:errors` **PASS**; `node --import ./tests/helpers/register.mjs --test tests/unit` **866/866 lulus** (859 lama + 7 baru). **`npm run build` belum dijalankan** — perintahnya diserahkan kepada Rizki. **Uji lewat layar belum** — `NOT FEASIBLE` bagi agent. Bukan UAT. Satu temuan lama dicatat tanpa diperbaiki: prop `disabled` pada kartu koreksi tab Transfer tidak pernah dibaca `EmergencyAssessmentFormCard` (laporan bagian 8.1). Bukti: [laporan](../task/report/frontend/FE-IGD-031.md). *Keadaan sebelumnya: direncanakan 16 September 2026 (keempat), belum dikerjakan* |
| **Outcome** | Perawat berpindah antara mengisi formulir dan membaca riwayat dalam satu tab tanpa menggulir melewati seluruh isian, dan setiap penyimpanan yang berhasil langsung terlihat hasilnya pada daftar riwayat |
| **Slice** | `IGD-S04`, `IGD-S05` · lanjutan `FE-IGD-022` (ruang kerja pemeriksaan IGD) |
| **Requirement** | **Coverage gap:** tidak ada `FR-IGD-*` yang mengatur susunan tampilan; dijejak ke `IGD-DEC-133` |
| **Keputusan** | `IGD-DEC-133`; wewenang UI `03-frontend-architecture.md` bagian 11 dan 12.5 |
| **Kontrak** | **Nol perubahan, nol kenaikan versi.** Tidak ada endpoint baru, tidak ada ruas data baru, tidak ada perubahan payload |
| **Reuse** | `ClinicalSegmentedNav` (`src/components/ui/doctor-clinical-base/ClinicalSegmentedNav.jsx`) — sudah mendukung badge angka; `EmergencyAssessmentFormCard`, `EmergencyAssessmentSection`, dan `EmergencyAssessmentRecordTab` yang sudah ada — dipindah tempat, **tidak** ditulis ulang; thunk daftar dan simpan pada `emergency-assessment-slice.jsx` yang sudah ada. Preseden pemakaian segmen di dalam satu tab klinis: `physician-workspace/tabs/medication-procedure/prescription-procedure-tab.jsx:88` |
| **Scope** | Satu komponen pembungkus baru di `emergency-assessment-view/components/`, lalu lima tab memakainya: `emergency-assessment-initial-tab.jsx`, `emergency-assessment-nosocomial-tab.jsx`, `emergency-assessment-disposition-tab.jsx`, `emergency-assessment-diagnostic-support-tab.jsx`, `emergency-assessment-transfer-tab.jsx`. Modul CSS milik layar ini boleh ditambah kelas. **Nol komponen bersama diubah, nol CSS global, nol backend** |
| **Dependency** | `IGD-DEC-133` ✅. Tidak menunggu backend — gelombang ini tidak menyentuh backend |
| **Acceptance** | 1. Kelima tab menampilkan segmen **Formulir** dan **Riwayat** memakai `ClinicalSegmentedNav`; segmen Riwayat membawa badge jumlah data. 2. Segmen dibangun sebagai **grup radio**, bukan `role="tablist"` kedua — jumlah `role="tablist"` pada layar detail tetap **satu**, dan `Tabs` react-bootstrap **tidak** dipakai. 3. Sesudah penyimpanan berhasil, layar berpindah sendiri ke segmen **Riwayat** dan daftarnya dimuat ulang. 4. Dari segmen Riwayat tersedia satu aksi kembali ke segmen Formulir. 5. Penyimpanan yang **gagal tidak** memindahkan segmen; pesan galat tetap tampil di tempatnya sekarang, apa adanya dari backend. 6. Tab **Assesmen Awal IGD** menampilkan baris ringkas **"terakhir dikaji"** — waktu, nama pencatat, dan ringkasan singkat — yang **tetap terlihat saat segmen Formulir aktif**; bila belum ada riwayat, baris itu tidak ditampilkan sama sekali, bukan ditampilkan kosong. 7. Baris itu dibentuk **dari daftar riwayat yang sudah dimuat layar**: nol pemanggilan endpoint baru, nol ruas data baru, nol sumber data baru. 8. Pada tab **Transfer Pasien**, formulir "Koreksi kejadian" menjadi aksi pada baris riwayat, bukan formulir ketiga yang berdiri sendiri. 9. Tab **SOAP**, **Catatan Terintegrasi**, dan **Resep** **tidak** diberi segmen dan **tidak** disentuh. 10. Setiap ruas data yang tampil hari ini **tetap** tampil, dari sumber yang sama — `03-frontend-architecture.md:331`. 11. Nilai kosong tetap tampil sebagai tanda hubung. 12. Nol palet warna baru; warna diambil dari modul CSS layar ini atau `emergency-triage.module.css`. 13. Nol pustaka komponen baru, nol komponen bersama diubah, nol CSS global |
| **Bukti** | DoD baku gelombang ini; tangkapan layar kedua segmen pada tab Assesmen Awal IGD — satu saat Formulir aktif yang memperlihatkan baris "terakhir dikaji", satu lagi sesudah simpan berhasil yang memperlihatkan perpindahan otomatis ke Riwayat. Bila backend atau kredensial tidak tersedia, dinyatakan `NOT FEASIBLE` beserta alasannya. Ditambah hasil grep: jumlah `role="tablist"` pada layar detail, dan nihilnya `react-bootstrap` `Tabs` pada berkas yang disentuh |
| **Risiko** | **Menengah, dan letaknya bukan pada kode.** Menyembunyikan riwayat di balik segmen memindahkan risiko pengkajian ganda, tidak menghapusnya: saat segmen Formulir aktif, perawat tidak melihat bahwa rekannya baru mengkaji pasien yang sama. Kriteria 6 dan 7 ada khusus untuk menambalnya, dan **tidak boleh dilepas** tanpa keputusan pemilik yang baru. Risiko kedua: lima tab disentuh sekaligus, sehingga satu cacat pada pembungkus bersama muncul di lima tempat — karena itu pembungkusnya wajib punya test unit sendiri |
| **Owner** | Frontend |

### 🟡 `FE-IGD-032` — Tata letak tab Observasi dan lembar pemantauan

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — 16 September 2026.** Implementasi selesai pada satu berkas tab dan satu modul CSS: pemilih periode, baris ringkas primary survey, lembar pemantauan berbentuk tabel 16 kolom, dan segmen Lembar/Catat. Empat belas acceptance criteria terpetakan ke source, **dua di antaranya dengan delta**: kriteria 4 — `ClinicalDataTable` dipakai sebagai acuan bentuk, **bukan** sebagai komponen, karena kepalanya tidak dapat dimatikan dan akan menggandakan kepala `EmergencyAssessmentSection` yang menangani memuat/galat/larangan akses; kriteria 3 — baris primary survey menghilang saat tidak ada triase, keadaan memuat dan kosongnya ikut hilang. Keduanya dijelaskan penuh pada laporan bagian 8. Kriteria 2 ternyata **sudah terpenuhi sebelum task ini** oleh effect milik `FE-IGD-028`. Validasi: `npm run lint:errors` **PASS**; unit test **866/866 lulus**. **`npm run build` belum dijalankan.** **Uji lewat layar belum**, dan untuk task ini paling menentukan karena kriteria 8 dan 9 menahan perilaku `FE-IGD-024` dan `FE-IGD-028`. Bukan UAT. Bukti: [laporan](../task/report/frontend/FE-IGD-032.md). *Keadaan sebelumnya: direncanakan 16 September 2026 (keempat), belum dikerjakan* |
| **Outcome** | Perawat membaca arah perubahan keadaan pasien dalam satu layar: periode observasi terpilih sendiri, primary survey terbaca sebagai satu baris, dan putaran pemantauan berjajar sebagai tabel sehingga tren tanda vital serta keseimbangan cairan terlihat sekaligus |
| **Slice** | `IGD-S04` · `EPIC IGD-09` (pemantauan observasi) |
| **Requirement** | **Coverage gap:** tidak ada `FR-IGD-*`; dijejak ke `IGD-DEC-134`, melanjutkan rantai `IGD-DEC-122`…`126` milik `FE-IGD-028` |
| **Keputusan** | `IGD-DEC-134`; `IGD-DEC-133` (bentuk segmennya); wewenang UI `03-frontend-architecture.md:330` |
| **Kontrak** | API `0.6.0` bagian 7 dan validation `0.6.0` bagian 9 — **nol perubahan, nol kenaikan versi**. Skema layar: `03-frontend-architecture.md` bagian 12; **aturan 12.4 tidak berubah** |
| **Reuse** | `ClinicalDataTable` (`src/components/ui/doctor-clinical-base/ClinicalDataTable.jsx`) — sudah membawa judul, badge jumlah, dan empty state; pembungkus segmen dari `FE-IGD-031`; seluruh thunk, selector, dan proyeksi data observasi yang sudah ada sejak `FE-IGD-028` |
| **Scope** | `emergency-assessment-observation-tab.jsx` dan modul CSS layar ini. **Nol perubahan pada slice, hook, service, konstanta, maupun payload** kecuali bila penataan ulang menuntut pemindahan murni tanpa perubahan perilaku — bila itu terjadi, sebutkan barisnya pada laporan. **Nol komponen bersama diubah, nol CSS global, nol backend** |
| **Dependency** | `IGD-DEC-134` ✅; `FE-IGD-031` (pembungkus segmen); `FE-IGD-028` ✅ (tab yang ditata ulang adalah hasil task itu) |
| **Acceptance** | 1. Daftar periode observasi tampil sebagai **baris pemilih ringkas di bagian atas** tab, bukan daftar kartu memanjang. 2. Periode yang **sedang berjalan** terpilih otomatis saat tab dibuka; bila tidak ada yang berjalan, periode terbaru yang terpilih; bila belum ada periode sama sekali, jalan membuka periode baru tetap terlihat tanpa klik tambahan. 3. Ringkasan **Primary Survey Terakhir** tampil sebagai satu baris ringkas, memuat ruas yang sama dengan sekarang, dan tetap **baca saja** — `IGD-DEC-123`. 4. Putaran pemantauan tampil sebagai **tabel** memakai `ClinicalDataTable`, terbaru di atas, dengan kolom waktu, tanda vital, keseimbangan cairan, pencatat, dan ringkasan keadaan/tindakan/respons. 5. Tabel itu **tidak menghitung apa pun sendiri**: angka tanda vital, GCS beserta totalnya, kesadaran, dan oksigen dibaca dari proyeksi backend — `FE-IGD-028` kriteria 5 dan 6 tetap berlaku. 6. Nama pencatat dari `recordedByName`; kosong tampil sebagai tanda hubung, **bukan** GUID. 7. Segmen pada tab ini berbunyi **"Lembar Pemantauan"** dan **"Catat Pemantauan"**, dengan **Lembar Pemantauan** sebagai bawaan; sesudah satu putaran tersimpan, layar kembali ke Lembar Pemantauan. 8. Aksi **Selesaikan** beserta isian **Kesimpulan** milik `FE-IGD-024` **tetap bekerja tanpa perubahan perilaku**. 9. Penautan tanda vital milik `FE-IGD-028` **tetap bekerja tanpa perubahan perilaku**, termasuk penyaring lingkup pasien dan encounter. 10. Periode `Completed` dan `Cancelled` tetap tidak menyediakan jalan masuk pencatatan normal; penolakan `409` tetap tampil apa adanya. 11. Tabel dapat digulir mendatar pada layar sempit **tanpa** membuat halaman ikut bergulir mendatar. 12. Nilai kosong tampil sebagai tanda hubung — kosong berarti tidak diukur, bukan nol. 13. Nol isian obat, gambaran EKG, atau DC Shock ditambahkan; layar Resusitasi tidak disentuh. 14. Nol palet warna baru, nol pustaka komponen baru, nol CSS global |
| **Bukti** | DoD baku gelombang ini; tangkapan layar tab Observasi dengan satu periode berjalan berisi **minimal tiga** putaran pemantauan, memperlihatkan ketiga bagian sekaligus dalam satu layar: pemilih periode, baris primary survey, dan tabel lembar pemantauan. Ditambah tangkapan layar segmen Catat Pemantauan. Bila backend atau kredensial tidak tersedia, dinyatakan `NOT FEASIBLE` beserta alasannya |
| **Risiko** | **Menengah ke tinggi — berkas terbesar pada modul ini (1.398 baris) dan memuat dua alur tulis yang sudah terbukti.** Penataan ulang berisiko menggeser perilaku `FE-IGD-024` (Kesimpulan) dan `FE-IGD-028` (penautan tanda vital) tanpa sengaja; kriteria 8 dan 9 ada khusus untuk menahannya, dan keduanya wajib dibuktikan ulang, bukan diasumsikan. Risiko kedua: tabel menuntut ruas yang ringkas, sehingga godaan memangkas ruas yang hari ini tampil menjadi nyata — `03-frontend-architecture.md:331` melarangnya, dan kriteria 4 menyebut ruasnya satu per satu |
| **Owner** | Frontend |

---

## R3.10 Gelombang 17 September 2026 — tombol simpan sesudah penilaian tersimpan

Lahir dari pertanyaan pemilik saat uji layar, bukan dari perencanaan.

### 🟡 `FE-IGD-033` — Tombol **Simpan Pemeriksaan** berhenti aktif sesudah penilaian tersimpan

**Status.** ✅ **SELESAI — 17 September 2026.** Lint `PASS`, unit test **866/866**,
`npm run build` lulus, dan **uji lewat layar oleh pemilik LULUS** — kelima kriteria. Tanpa UAT.
[Laporan](../task/report/frontend/FE-IGD-033.md).

**Masalah.** Sesudah penilaian tersimpan, bagian pilih dokter muncul — tetapi tombol
**Simpan Pemeriksaan** tetap menyala, tetap bertulisan sama, dan tetap mengirim permintaan
**pembuatan penilaian baru**. Perawat yang baru memilih dokter secara wajar menekannya lagi.

**Mengapa mendesak.** Sebelum `BE-IGD-047`, penekanan kedua menghasilkan `409` — membingungkan,
tetapi nol data salah. Sesudah `BE-IGD-047`, penekanan kedua **berhasil** dan menambahkan
penilaian kedua ke riwayat klinis pasien secara senyap. Memperbaiki backend saja mengubah galat
berisik menjadi catatan klinis ganda. **Kedua task ini satu paket.**

**Acceptance criteria.**

| # | Kriteria | Bukti |
| ---: | --- | --- |
| 1 | Tombol nonaktif begitu penilaian tersimpan | Uji layar langkah 1 |
| 2 | Tulisannya berubah menjadi **Pemeriksaan Tersimpan** | Uji layar langkah 1 |
| 3 | Pesan berhasil menunjuk langkah berikutnya | Uji layar langkah 1 |
| 4 | **Tetapkan Dokter** tetap bekerja tanpa menyentuh penilaian | Uji layar langkah 2 |
| 5 | Riwayat pasien memuat tepat satu penilaian sesudah alur penuh | Uji layar langkah 3 |

**Lubang yang tetap terbuka.** API masih menerima penilaian kedua pada satu kunjungan dari
pemanggil mana pun. Layar tidak lagi memancingnya, tetapi penjaga sesungguhnya harus di backend
— dan belum ada requirement maupun keputusan yang memintanya. Dicatat, bukan ditambal.

---

## R3.11 Gelombang 21 September 2026 (sore) — pra-cek episode ganda dan nama pelaku

Dua keputusan pemilik memengaruhi frontend. **`FE-IGD-017` ✅ dan `FE-IGD-034` ✅ — keduanya dinyatakan selesai atas penilaian pemilik (21–22 September 2026) sesudah build dan uji layar dilaporkan lulus.**

| Task | Nasib | Menunggu |
| --- | --- | --- |
| `FE-IGD-017` | ✅ `IGD-DEC-137` — dua baris nama pelaku. **22 September 2026: ✅ atas penilaian pemilik** (build dinyatakan lulus; uji layar lulus 21 September) | `BE-IGD-049` ✅ — selesai 22 September 2026. Kartunya di R3.2 |
| `FE-IGD-034` (baru) | ✅ `IGD-DEC-138` — layar memeriksa episode ganda **sebelum** encounter dibuat. **21 September 2026 (malam): ✅ atas penilaian pemilik** — build dan uji layar dilaporkan lulus (tanpa lampiran) | `BE-IGD-050` ✅ (kontrak selesai **dan** build terverifikasi — atas pernyataan pemilik) |
| `FE-IGD-014` | ✅ **dinilai ulang 21 September 2026 (malam)** sesudah `FE-IGD-034` ✅ — **dengan celah `IGD-OQ-093` dinyatakan**: ✅ ini tidak berarti tidak ada encounter yatim dalam segala keadaan ([laporan](../task/report/frontend/FE-IGD-014.md) bagian 9) | `FE-IGD-034` ✅ |

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph backend["Prasyarat backend — backend-roadmap.md R3.12"]
        BEIGD049["✅ BE-IGD-049<br/>Nama pelaku pada event kepergian"]:::luar
        BEIGD050["✅ BE-IGD-050<br/>Pra-cek episode ganda"]:::luar
    end

    subgraph frontend["Task frontend lain — bagian R3.2"]
        FEIGD014["✅ FE-IGD-014<br/>Pendaftaran kirim Emergency"]:::luar
    end

    FEIGD017["✅ FE-IGD-017<br/>Riwayat koreksi kepergian"]:::selesai
    FEIGD034["✅ FE-IGD-034<br/>Pra-cek sebelum encounter dibuat"]:::selesai

    BEIGD049 --> FEIGD017
    BEIGD050 --> FEIGD034
    FEIGD014 --> FEIGD034
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-IGD-049` ✅ — selesai 22 September 2026 (uji API S2–S5 dilaporkan lulus pemilik); **dikerjakan — selesai** | `FE-IGD-017` ✅ |
| 1 | `BE-IGD-050` ✅ — kontrak `0.10.0` selesai, build dan uji API dilaporkan `PASS` oleh pemilik 21 September 2026 malam; **dikerjakan — Implementation Complete**; `FE-IGD-014` kriteria 2 sudah di-commit (`198d56d9e`) | `FE-IGD-034` |

### ✅ `FE-IGD-034` — Pendaftaran IGD memeriksa episode ganda sebelum membuat encounter

| Field | Isi |
| --- | --- |
| **Status** | ✅ **21 September 2026 (malam) — atas penilaian pemilik:** `npm run build` dan uji layar dijalankan pemilik dan **dilaporkan lulus** (pernyataan pemilik; tanpa lampiran — agent memeriksa artefak `.next` 16:03 yang memuat `active-episode` dan commit `c941012ac`). Acceptance 1–6 terpenuhi; **`IGD-OQ-093` `open` tidak tertutup**. *Riwayat 🟡:* **Implementation Complete**: lima berkas frontend (`emergency-registration.service.js`, `emergency-registration.utils.js`, `use-emergency-registration.js`, `verification-step.jsx`, `emergency-registration-existing-visit.test.mjs`); `BE-IGD-050` ✅ (kontrak `0.10.0`; build dan uji API `PASS` menurut pemilik); `fail-open` diputuskan pemilik. **Validasi:** `eslint` lima berkas **0 error** (3 warning sama dengan `HEAD`); unit test berkas task **14/14 lulus**; suite penuh 1420/1429 (9 gagal di luar cakupan: `FE-RWI-*` ×8, `accounting-reconciliation` ×1); nol berkas CSS. Acceptance 5 ✅; **1–4 terpetakan ke source, runtime belum**; **6 sebagian** — `npm run build` dan uji layar U1–U6 milik pemilik. Dua hal di luar teks kartu (jalur galat memakai pra-cek yang sama; penanda `processing`) dijelaskan pada laporan bagian 3.4. `IGD-OQ-093` `open` tidak tertutup. Tanpa UAT. [Laporan](../task/report/frontend/FE-IGD-034.md) |
| **Outcome** | Petugas yang mendaftarkan pasien yang masih punya kunjungan IGD berjalan diberi tahu **di awal**, dengan nomor kunjungannya dan tombol membukanya — dan **tidak ada encounter baru** yang terbentuk |
| **Slice** | `IGD-S02` · `EPIC IGD-01` |
| **Requirement** | `FR-IGD-005`…`012` sisi tampilan |
| **Keputusan** | **`IGD-DEC-138`**; `IGD-DEC-084` |
| **Kontrak** | API bagian `1.3` — `GET emergency-visits/active-episode?patientId=` (`BE-IGD-050`). `POST patient-encounters` dan `POST emergency-visits` **tidak berubah** |
| **Dependency** | `BE-IGD-050` (kontrak selesai **dan** build terverifikasi); `FE-IGD-014` kriteria 2 (sudah di-commit pemilik sebagai `198d56d9e`; digantikan oleh task ini) |
| **Wewenang UI** | `DEV_DISCRETION` — memakai kotak peringatan dan tombol yang sudah ada pada `verification-step.jsx` (`EmergencyInlineAlert`, `BaseButton`); nol elemen `NEW` |
| **Risiko** | Menengah — pintu masuk pasien; pra-cek tidak boleh menghentikan pendaftaran darurat tanpa alasan |
| **Owner** | Frontend |

**Perilaku yang direncanakan** (urutan langkah `handleSubmitRegistration`):

1. Bila `encounterId` **belum ada**, `patientId` ada, dan alasan pendaftaran ganda **kosong** → panggil pra-cek **sebelum** `POST patient-encounters`.
2. Bila `hasActiveEpisode` benar → **berhenti**: nol permintaan `POST patient-encounters`, hasil `success: false` dengan `stage: "duplicateCheck"` dan `existingVisit` terstruktur. Kotak kuning yang sudah ada tampil.
3. Bila alasan terisi → pra-cek dilewati; alur lama (jalan keluar beralasan).
4. Percobaan ulang saat `completedEncounter` sudah ada → pra-cek **tidak** dijalankan lagi.
5. **Bila pra-cek itu sendiri gagal** (jaringan, `403`): **usulan agent — lanjutkan alur lama (`fail-open`)**, karena IGD tidak boleh berhenti menerima pasien gara-gara pemeriksaan pelengkap gagal; `POST emergency-visits` tetap menolak `409` sebagai jaring pengaman. **Ini keputusan keselamatan pasien lawan risiko encounter yatim — menunggu konfirmasi pemilik.**

**File yang akan disentuh** (`QuilvianSystemFrontendDev`):

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/registration-management/emergency-registration/use-emergency-registration.js` | Pra-cek sebelum pembuatan encounter; hasil `duplicateCheck` |
| `src/lib/services/health-services/registration-management/emergency-registration.service.js` | Fungsi pra-cek baru; `fetchEmergencyVisitsByPatient` (heuristik `FE-IGD-014`) dihapus |
| `src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js` | `pickActiveEmergencyVisit` diganti normalisasi respons terstruktur; `resolveExistingVisitDestination` tetap |
| `src/components/view/health-services/registration-management/emergency-registration/verification-step.jsx` | Kotak kuning membaca `existingVisit` dari hasil `duplicateCheck` |
| `tests/unit/emergency-registration-existing-visit.test.mjs` | Diperbarui |

**Tidak disentuh:** `globals.css`, berkas CSS mana pun, slice Redux, seluruh backend.

**Acceptance**

| # | Kriteria |
| ---: | --- |
| 1 | Pasien berepisode aktif dan alasan kosong → **nol** permintaan `POST patient-encounters` (terbukti dari panel network dan tidak ada encounter baru di basis data) |
| 2 | Kotak kuning menampilkan nomor kunjungan dan status dari respons terstruktur; tombol membuka layar Triage untuk status 1–2 dan Assesmen IGD untuk status 3–7 |
| 3 | Alasan terisi → pendaftaran ganda berhasil seperti sebelumnya |
| 4 | Pra-cek gagal → perilaku sesuai keputusan pemilik atas butir 5, dengan pesan yang jelas |
| 5 | Aturan "aktif" tidak lagi diduplikasi di frontend (heuristik `FE-IGD-014` dihapus) |
| 6 | `eslint` berkas task 0 error; unit test lulus; nol CSS baru; `npm run build` dan uji layar — milik pemilik |

**DoD.** Acceptance 1–6 terpenuhi; laporan tracked ada; roadmap dan traceability diperbarui. Sesudah itu
`FE-IGD-014` boleh ✅ end-to-end, **kecuali** celah `IGD-OQ-093` yang harus tetap dinyatakan apa adanya.

---

## R3.12 Gelombang 22 September 2026 — encounter-first dan dokter jaga, sisi layar

Tiga task layar dari keputusan pemilik yang **disetujui prinsip** 22 September 2026: `IGD-DEC-139`
(encounter-first), `IGD-DEC-140` (pasien tanpa identitas), `IGD-DEC-141` (kelayakan dokter jaga).
Pasangan backend-nya di `backend-roadmap.md` bagian **R3.13**. Bukti source:
[evidence/2026-09-22-desain-encounter-first.md](../evidence/2026-09-22-desain-encounter-first.md).

**Direncanakan saja — nol source ditulis.** Ketiganya menunggu backend; tidak ada yang boleh dimulai
hari ini.

**`FE-IGD-027` tetap ✅.** Kelayakan dokter adalah kebutuhan baru (`IGD-DEC-141` butir 6), bukan cacat
acceptance `FE-IGD-027`. Pemilik menegaskannya dua kali — kartu ini **tidak** menurunkannya.

```mermaid
flowchart LR
    classDef selesai fill:#DCFCE7,stroke:#16A34A,color:#14532D
    classDef sebagian fill:#FEF9C3,stroke:#CA8A04,color:#713F12
    classDef terblokir fill:#FEE2E2,stroke:#DC2626,color:#7F1D1D
    classDef belum fill:#F1F5F9,stroke:#64748B,color:#0F172A
    classDef luar fill:#EDE9FE,stroke:#7C3AED,color:#3B0764

    subgraph backend["Prasyarat backend — backend-roadmap.md R3.13"]
        BEIGD053["⛔ BE-IGD-053<br/>Penjaga di pintu encounter"]:::luar
        BEIGD054["BE-IGD-054<br/>Daftar Menunggu Triage terpadu"]:::luar
        BEIGD055["⛔ BE-IGD-055<br/>Kunjungan lahir lewat Mulai Triage"]:::luar
        BEIGD056["⛔ BE-IGD-056<br/>Dokter layak dan override"]:::luar
    end

    FEIGD035["FE-IGD-035<br/>Daftar triage membaca satu sumber"]:::belum
    FEIGD036["FE-IGD-036<br/>Mulai Triage; loket berhenti membuat kunjungan"]:::belum
    FEIGD037["FE-IGD-037<br/>Pemilih dokter jaga dan override"]:::belum

    BEIGD054 --> FEIGD035
    BEIGD053 --> FEIGD036
    BEIGD055 --> FEIGD036
    FEIGD035 --> FEIGD036
    BEIGD056 --> FEIGD037
```

Jumlah panah: **5**, sama dengan isi kolom `Dependency` ketiga kartu (1 + 3 + 1).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-IGD-054` (kontrak selesai **dan** build terverifikasi) | `FE-IGD-035` |
| 1 | `BE-IGD-056` (⛔ kueri E1–E3) | `FE-IGD-037` |
| 2 | `BE-IGD-053` (⛔), `BE-IGD-055` (⛔), dan `FE-IGD-035` | `FE-IGD-036` |

**Kenapa `FE-IGD-036` menunggu `BE-IGD-053`, bukan hanya `BE-IGD-055`.** Hari ini jaring pengaman
episode ganda adalah `409` dari `POST /emergency-visits`, dan pra-cek `FE-IGD-034` `fail-open` hanya
membaca **kunjungan** (klausa B). Begitu loket berhenti membuat kunjungan, pasien yang sedang *Menunggu
Triage* hanya punya encounter (klausa A) — tidak terlihat oleh pra-cek dan tidak lagi dijaga `409`.
Memutus loket dari `POST /emergency-visits` sebelum penjaga server menyala berarti **membuka pintu
pendaftaran ganda**.

*Contoh.* Pukul 09.35 RAYYAN didaftarkan (encounter saja). Pukul 09.40 petugas lain mendaftarkannya
lagi: pra-cek menjawab "tidak ada kunjungan berjalan", dan tanpa `BE-IGD-053` encounter kedua lahir.
Daftar triage kini memuat RAYYAN dua kali.

### `FE-IGD-035` — Daftar Menunggu Triage membaca satu sumber (`triage-queue`)

| Field | Isi |
| --- | --- |
| **Status** | Tanpa tanda — **direncanakan 22 September 2026**. Menunggu `BE-IGD-054` (kontrak selesai **dan** build terverifikasi) |
| **Outcome** | Perawat triage melihat pasien yang baru didaftarkan pada daftar *Menunggu Triage* walau kunjungannya belum lahir, pasien tanpa identitas tetap terlihat, dan waktu "Terdaftar" tidak pernah tampil sebagai waktu "Tiba" |
| **Slice** | `IGD-S02` · `EPIC IGD-01` |
| **Requirement** | `FR-IGD-011` sisi tampilan; daftar triage terpadu belum punya FR — coverage gap R3.13 |
| **Keputusan** | **`IGD-DEC-139`** butir 2, **`IGD-DEC-140`** U1; `IGD-DEC-128` tidak berubah untuk baris kunjungan |
| **Kontrak** | API — bagian baru dari `BE-IGD-054`: `GET emergency-visits/triage-queue`. `GET emergency-visits` tetap ada untuk layar lain |
| **Dependency** | `BE-IGD-054` |
| **Wewenang UI** | `DEV_DISCRETION` — memakai tabel daftar yang sudah ada (`emergency-triage-patient-table.jsx`, `emergency-triage-patient-list-view.jsx`); nol elemen `NEW`; nol CSS global |
| **Risiko** | Menengah — ini layar kerja utama perawat triage; salah membaca aksi membuat pasien tidak dapat diproses |
| **Owner** | Frontend |

**Perilaku yang direncanakan.**

1. Pengambilan daftar pada `fetchEmergencyTriagePatients` pindah dari `GET /emergency-visits` ke
   `GET /emergency-visits/triage-queue` (`emergency-management-triage-slice.jsx` baris 125 dan 137–160).
2. Tombol pada setiap baris diturunkan dari `availableActions` respons. Layar **tidak** menebak aksi dari
   asal baris dan **tidak** menghitung ulang "episode terbuka".
3. Baris encounter-saja menampilkan waktu `registeredAt` berlabel **"Terdaftar"**; baris kunjungan
   menampilkan `arrivalDateTime` berlabel "Tiba". Kolom waktu tidak boleh mencampur keduanya tanpa label.
4. Aksi yang tidak dikenal layar tidak dirender. Aksi `StartTriage` dipasang `FE-IGD-036`; **usulan agent:
   rilis `FE-IGD-035` bersama `FE-IGD-036`** supaya baris encounter-saja tidak tampil tanpa tombol.
   Keputusan pemilik saat go-ahead.
5. Panel pelanggaran SLA triage yang ada tidak diubah; baris encounter-saja belum punya waktu tiba,
   jadi tidak ikut dihitung SLA sampai kunjungannya lahir — dinyatakan pada laporan, bukan disembunyikan.

**Acceptance**

| # | Kriteria |
| ---: | --- |
| 1 | Daftar triage memanggil **satu** endpoint (`triage-queue`); nol permintaan `GET /emergency-visits` dari layar daftar (panel network) |
| 2 | Pasien yang baru didaftarkan tanpa kunjungan tampil sebagai *Menunggu Triage* dengan label waktu "Terdaftar" |
| 3 | Pasien tanpa identitas tampil dengan alias sementaranya, tidak diganti tebakan (`IGD-DEC-007`) |
| 4 | Tombol baris sama persis dengan `availableActions`; baris kunjungan tetap menampilkan **Isi Triage** dan **Tangani Segera** sesuai `FE-IGD-029`/`030` |
| 5 | Halaman berikutnya tidak mengulang atau melompati pasien |
| 6 | `eslint` berkas task 0 error; unit test lulus; nol CSS baru; `npm run build` dan uji layar — milik pemilik |

**DoD.** Acceptance 1–6; laporan tracked; roadmap dan traceability diperbarui.

### `FE-IGD-036` — Mulai Triage melahirkan kunjungan; loket berhenti membuat kunjungan

| Field | Isi |
| --- | --- |
| **Status** | Tanpa tanda — **direncanakan 22 September 2026**. Menunggu `BE-IGD-053` (⛔), `BE-IGD-055` (⛔), dan `FE-IGD-035`. Isi layar Mulai Triage bergantung W1 (`IGD-OQ-094`) dan aksi Tangani Segera bergantung `IGD-OQ-100` — keduanya sudah menahan `BE-IGD-055` |
| **Outcome** | Petugas loket cukup mendaftarkan pasien; perawat triage yang menekan **Mulai Triage** melahirkan kunjungan IGD dan langsung masuk ke formulir triage. Waktu tiba tidak lagi jatuh diam-diam ke jam komputer loket |
| **Slice** | `IGD-S02` · `EPIC IGD-01` |
| **Requirement** | `FR-IGD-007`…`012` sisi tampilan; titik lahir kunjungan belum punya FR — coverage gap R3.13 |
| **Keputusan** | **`IGD-DEC-139`**, **`IGD-DEC-140`** U1, `IGD-DEC-138`, `IGD-DEC-084`, `IGD-DEC-128` (lewat `IGD-OQ-100`) |
| **Kontrak** | API — `POST emergency-visits/start-triage` (`BE-IGD-055`); ruas `encounter` pada `GET emergency-visits/active-episode` (`BE-IGD-053`, aditif); penolakan `409` pada `POST patient-encounters` bertipe `Emergency` (`BE-IGD-053`) |
| **Dependency** | `BE-IGD-053`, `BE-IGD-055`, `FE-IGD-035` |
| **Wewenang UI** | `DEV_DISCRETION` untuk tata letak isian Mulai Triage; memakai komponen formulir dan konfirmasi yang sudah ada. **Isi** isiannya dikunci W1, bukan diskresi |
| **Risiko** | **Tinggi** — pintu masuk pasien dan titik nol *door-to-triage*. Urutan rilis wajib: backend penjaga menyala lebih dulu |
| **Owner** | Frontend |

**Perilaku yang direncanakan.**

1. **Loket.** `handleSubmitRegistration` (`use-emergency-registration.js`) berhenti memanggil
   `POST /emergency-visits` untuk pasien **beridentitas**. Pasien tanpa identitas tetap memakai jalur lama
   (U1 `IGD-DEC-140`).
2. **Pra-cek `FE-IGD-034` tetap berjalan** dan kini juga mengenali episode yang baru berupa encounter
   (ruas `encounter` dari `BE-IGD-053`). Kotak kuning menampilkan nomor encounter dan keterangan
   *"Menunggu Triage"* bila kunjungan belum lahir, dengan tombol ke daftar triage.
3. **Penolakan `409` dari `POST patient-encounters`** ditampilkan dengan pesan backend apa adanya — bukan
   diterjemahkan ulang.
4. **Meja triage.** Aksi **Mulai Triage** pada baris encounter-saja memanggil `start-triage`. `201` atau
   `200` (idempoten) → langsung membuka formulir triage kunjungan itu. Klik ganda tidak melahirkan dua
   kunjungan.
5. **Waktu tiba** mengikuti jawaban W1. Bila W1 disetujui: isian waktu tiba ada di Mulai Triage, diisi
   awal waktu server, boleh dikoreksi, dan isian waktu tiba di loket dilepas. Bila W1 ditolak dan waktu
   tiba tetap di loket: validasi *"Tanggal dan jam kedatangan belum valid."* yang dikomentari pada
   `emergency-visit-step.jsx` baris 201–204 **wajib dinyalakan kembali**. Dalam kedua kasus, `arrivalDateTime`
   **tidak boleh** lagi melewati `toIsoDateOrNow` (`emergency-registration.utils.js` baris 1161) — temuan
   `IGD-EV-141`.
6. **Tangani Segera** pada baris encounter-saja mengikuti jawaban `IGD-OQ-100`.

**File yang diperkirakan disentuh** (`QuilvianSystemFrontendDev`): `use-emergency-registration.js`,
`emergency-registration.utils.js`, `emergency-visit-step.jsx`, `verification-step.jsx`,
`emergency-management-triage-slice.jsx`, komponen daftar triage, dan test unit terkait. **Tidak
disentuh:** `globals.css`, berkas CSS global, seluruh backend.

**Acceptance**

| # | Kriteria |
| ---: | --- |
| 1 | Pendaftaran pasien beridentitas → tepat **satu** permintaan pembuatan (`POST patient-encounters`); nol `POST emergency-visits` (panel network) |
| 2 | Pendaftaran pasien tanpa identitas tetap melahirkan kunjungan seperti sebelumnya |
| 3 | Pendaftaran kedua untuk pasien yang sedang *Menunggu Triage* dihentikan pra-cek; bila pra-cek dilewati, `409` backend tampil dengan pesannya |
| 4 | Mulai Triage → kunjungan lahir dan formulir triage terbuka; klik kedua membuka kunjungan yang sama |
| 5 | Waktu tiba sesuai W1; nol jalur yang mengisi `arrivalDateTime` dari jam browser |
| 6 | Tangani Segera pada baris encounter-saja sesuai jawaban `IGD-OQ-100` |
| 7 | `eslint` berkas task 0 error; unit test lulus; nol CSS baru; `npm run build` dan uji layar — milik pemilik |

**DoD.** Acceptance 1–7; laporan tracked; roadmap dan traceability diperbarui. Laporan menyatakan urutan
rilis yang dipakai (backend penjaga lebih dulu).

### `FE-IGD-037` — Pemilih Dokter Penanggung Jawab IGD menawarkan dokter jaga, dengan override beralasan

| Field | Isi |
| --- | --- |
| **Status** | Tanpa tanda — **direncanakan 22 September 2026**. Menunggu `BE-IGD-056` (⛔ kueri E1–E3) |
| **Outcome** | Petugas triage ditawari dokter yang sedang jaga IGD lebih dulu; memilih dokter di luar jadwal tetap mungkin tetapi wajib beralasan; dan bila roster kosong, layar tidak pernah buntu |
| **Slice** | `IGD-S06` · `EPIC IGD-04` · `MVP-5` |
| **Requirement** | `FR-IGD-016`…`021` sisi tampilan; kelayakan belum punya FR — coverage gap R3.13 |
| **Keputusan** | **`IGD-DEC-141`**; `IGD-DEC-129` (nama, bukan ID); `IGD-DEC-136` |
| **Kontrak** | API — `GET emergency-doctor-assignments/eligible-doctors`; ruas opsional `eligibilityOverrideReason` pada `POST /` dan `POST /{id}/handover` (`BE-IGD-056`) |
| **Dependency** | `BE-IGD-056` |
| **Wewenang UI** | `DEV_DISCRETION` — memakai bagian dokter yang sudah ada (`emergency-triage-doctor-section.jsx`) dan isian alasan yang sudah dipakai untuk pengalihan; nol elemen `NEW` bila polanya mencukupi |
| **Risiko** | **Tinggi (keselamatan)** — layar tidak boleh menyembunyikan satu-satunya jalan keluar ketika roster kosong |
| **Owner** | Frontend |

**Perilaku yang direncanakan.**

1. `fetchEmergencyDoctorOptions` berhenti membaca `doctors/options?isActive=true`
   (`emergency-management-triage-slice.jsx` baris 553–576) dan membaca `eligible-doctors`.
2. Bawaannya hanya dokter layak, masing-masing dengan alasan kelayakannya (mis. *"Jadwal IGD 19.00–07.00"*).
3. Pilihan *"Tampilkan dokter di luar jadwal"* memuat dokter tidak layak (`includeIneligible=true`).
   Memilih salah satunya memunculkan isian alasan **wajib**; tanpa alasan tombol simpan tidak aktif.
4. Penolakan backend ditampilkan apa adanya — backend tetap penjaga terakhir.
5. Roster kosong → pesan yang jelas bahwa tidak ada dokter berjadwal, dengan pilihan override langsung
   terbuka.
6. Riwayat penugasan dan tampilan "Data historis" dari `FE-IGD-027` **tidak disentuh**.

**Acceptance**

| # | Kriteria |
| ---: | --- |
| 1 | Pilihan bawaan hanya dokter layak, beserta alasannya; nol permintaan `doctors/options` dari bagian dokter IGD |
| 2 | Dokter di luar jadwal dapat dipilih hanya dengan alasan terisi; alasannya terkirim sebagai `eligibilityOverrideReason` |
| 3 | Roster kosong tidak membuat layar buntu — override selalu tersedia |
| 4 | Pesan penolakan backend tampil apa adanya |
| 5 | Riwayat penugasan dan `FE-IGD-027` tidak berubah perilakunya |
| 6 | `eslint` berkas task 0 error; unit test lulus; nol CSS baru; `npm run build` dan uji layar — milik pemilik |

**DoD.** Acceptance 1–6; laporan tracked; roadmap dan traceability diperbarui.
