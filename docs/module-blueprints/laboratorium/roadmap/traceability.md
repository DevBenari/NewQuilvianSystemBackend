# Traceability Roadmap — Modul Laboratorium

| Field | Value |
|---|---|
| `blueprint_id` | `LAB-BP-001` |
| Roadmap revision | `13` |
| Status | `DRAFT` |
| Tanggal | 2026-09-02 |
| Manifest | `blueprint-manifest.md` revision `24` |
| Backend SHA | `c87d9c0` |
| Frontend SHA | `688daff90` |
| Contract version | `LAB-API-v1` r3, `LAB-STATE-v1` r2, `LAB-VAL-v1` r3, `LAB-INT-v1` r3, `LAB-PERM-v1` r3 — `approved` 2026-09-02 |
| Approval | Yoga Aji Pratama (`yogaaji452@gmail.com`), pemilik modul, 2026-09-02 |
| Masukan | Decisions rev `21`; capability map rev `2` |
| Input hash | `sha256:6504b18a327b9966526bd1df8f3cb878d7f6d6519dacc1f7df16b1066729ae82` (decisions), dihitung 2026-09-02 |

Dokumen ini menjawab satu pertanyaan: **untuk setiap kebutuhan, siapa yang mengerjakannya dan
apa buktinya kalau sudah benar.** Baris yang tidak punya bukti muncul sebagai *coverage gap* di
bagian 4, bukan disembunyikan.

---

## 1. Traceability per Epic

### `EPIC-LAB-01` — Penandaan cito dan batas waktunya

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-01.1` | `LAB-DEC-013`, `LAB-DEC-026` | `02-backend-architecture.md` §4.3 | `LAB-API-v1` r3 | `BE-LAB-10` | `FE-LAB-06` | `AC-18` | Direncanakan |
| `FR-01.2` | `LAB-DEC-026` | `contracts/validation-matrix.md` `VAL-03` | `LAB-VAL-v1` r3 | `BE-LAB-10` | `FE-LAB-06` | `AC-18` jalur gagal | Direncanakan |
| `FR-01.3` | `LAB-DEC-013` | `CAP-04`, `CAP-15` | `LAB-STATE-v1` r2 | `BE-LAB-10` | — | `AC-18` | Direncanakan |
| `FR-01.4` | `LAB-DEC-013` | `02-backend-architecture.md` §4.4 | `LAB-API-v1` r3 | `BE-LAB-02` | `FE-LAB-02` | Kolom penyimpanannya ada — `LabValueBound.CitoTurnaroundMinutes`, [`task/report/backend/BE-LAB-02.md`](../task/report/backend/BE-LAB-02.md) bagian 3.2 dan 5.1. `AC-17` **belum terbukti**: daftar pantau keterlambatan cito adalah cakupan `BE-LAB-14` | **`SELESAI SEBAGIAN`** — penyimpanan selesai `BE-LAB-02`; `AC-17` menunggu `BE-LAB-14` |

### `EPIC-LAB-02` — Pemisahan wadah fisik dan pemeriksaan terpesan

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-02.1` | `LAB-DEC-024` | `02-backend-architecture.md` §4.3 | `LAB-API-v1` r3 | `BE-LAB-09`, `BE-LAB-16` | `FE-LAB-07` | `AC-35` **terbukti ujung-ke-ujung** — struktur pada [`BE-LAB-09.md`](../task/report/backend/BE-LAB-09.md) bagian 6.1, dan endpointnya pada [`BE-LAB-16.md`](../task/report/backend/BE-LAB-16.md) bagian 6.1: dua pemeriksaan ditambahkan lewat `POST /by-order` lalu keduanya terbaca lewat `GET /by-specimen` dengan satu barcode yang sama. `VAL-17` .. `VAL-20` masing-masing punya ujinya | **`SELESAI`** |
| `FR-02.2` | `LAB-DEC-024` | `contracts/state-transition-matrix.md` | `LAB-STATE-v1` r2 | `BE-LAB-12`, `BE-LAB-16` | `FE-LAB-07` | `AC-36` **terbukti** — [`BE-LAB-12.md`](../task/report/backend/BE-LAB-12.md) bagian 6.1; menolak wadah menggugurkan kedua pemeriksaan yang ditopangnya. `AC-37` terbukti pada tingkat status; penerbitan faktanya menunggu `BE-LAB-13` | **`SELESAI SEBAGIAN`** — status selesai `BE-LAB-12`; fakta per pemeriksaan menunggu `BE-LAB-13` |
| `FR-02.3` | `LAB-DEC-024` | `VAL-13` | `LAB-VAL-v1` r3 | `BE-LAB-12` | `FE-LAB-07` | `AC-36` **terbukti** — `VAL-13` ditegakkan secara struktural: tidak ada satu pun jalur pengubah yang menolak sebagian pemeriksaan | **`SELESAI`** |
| `FR-02.4` | `LAB-DEC-024` | `erd/data-dictionary.md` | — | `BE-LAB-11` | — | `AC-35` | Direncanakan — penahan `LAB-OPEN-012` dicabut 2026-09-03, jumlah baris `0` pada dev pemilik |
| `FR-02.5` | `LAB-DEC-024` | `VAL-14` | `LAB-VAL-v1` r3 | `BE-LAB-12` | `FE-LAB-07` | `VAL-14` dan `VAL-15` **terbukti** — [`BE-LAB-12.md`](../task/report/backend/BE-LAB-12.md) bagian 5 | **`SELESAI`** |
| `FR-02.6` | `LAB-DEC-024` | `02-backend-architecture.md` §6 | — | `BE-LAB-11` | — | `AC-35`, `AC-38` | Direncanakan — penahan dicabut; kode sudah berhenti memakai keenam kolomnya sejak `BE-LAB-12` |

### `EPIC-LAB-03` — Batas nilai dan persetujuan klinis

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-03.1` | `LAB-DEC-006`, `LAB-DEC-018` | `02-backend-architecture.md` §4.4 | `LAB-API-v1` r3 | `BE-LAB-02`, `BE-LAB-04` | `FE-LAB-02` | `AC-24` **terbukti** — [`BE-LAB-02.md`](../task/report/backend/BE-LAB-02.md) bagian 6.1 untuk penyimpanannya, [`BE-LAB-04.md`](../task/report/backend/BE-LAB-04.md) bagian 6.1 untuk endpointnya; tiga baris Hemoglobin dibuat lewat `POST` dan terbaca lewat `GET /` | **`SELESAI`** |
| `FR-03.2` | `LAB-DEC-021` | `02-backend-architecture.md` §4.5 | `LAB-VAL-v1` r3 `VAL-22` .. `VAL-24` | `BE-LAB-02`, `BE-LAB-04` | `FE-LAB-02` | `AC-28` **terbukti** — [`BE-LAB-04.md`](../task/report/backend/BE-LAB-04.md) bagian 5; `VAL-22`, `VAL-23`, dan `VAL-24` masing-masing punya ujinya | **`SELESAI`** |
| `FR-03.3` | `LAB-DEC-023` | — | `LAB-API-v1` r3 | `BE-LAB-04` | `FE-LAB-02` | `AC-33` bagian "batas normal langsung berlaku" **terbukti** — [`BE-LAB-04.md`](../task/report/backend/BE-LAB-04.md) bagian 6.1 | **`SELESAI`** |
| `FR-03.4` | `LAB-DEC-023` | `02-backend-architecture.md` §4.6 | `LAB-API-v1` r3, `VAL-28`, `VAL-32`, `VAL-33` | `BE-LAB-03`, `BE-LAB-05` | `FE-LAB-02` | `AC-33` **terbukti seluruh jalur** — [`BE-LAB-05.md`](../task/report/backend/BE-LAB-05.md) bagian 6.1; `VAL-31` .. `VAL-35` masing-masing punya ujinya, dan `VAL-33` dibuktikan dua arah (menyetujui maupun menolak pengajuan sendiri) | **`SELESAI`** — dibangun; belum dapat dipakai sampai peran `LabCriticalBound : Approve` ditetapkan manajemen |
| `FR-03.5` | `LAB-DEC-023` | `02-backend-architecture.md` §4.7 | `LAB-STATE-v1` r2 | `BE-LAB-03`, `BE-LAB-04` | `FE-LAB-02` | `AC-34` **terbukti** — riwayat tersimpan ([`BE-LAB-03.md`](../task/report/backend/BE-LAB-03.md)) dan diterbitkan serta dibaca lewat endpoint ([`BE-LAB-04.md`](../task/report/backend/BE-LAB-04.md) bagian 6.1) | **`SELESAI`** |
| `FR-03.6` | `LAB-DEC-006` | `erd/data-dictionary.md` | — | `BE-LAB-02` | — | `AC-25` **terbukti** — [`task/report/backend/BE-LAB-02.md`](../task/report/backend/BE-LAB-02.md) bagian 6.1; nol kolom operasional laboratorium pada model EF, dan jumlah kolom `MstProcedure` tetap 35 pada keempat titik pengukuran migration, bagian 5.1 | **`SELESAI`** |

### `EPIC-LAB-04` — Daftar kerja dan pemantauan keterlambatan

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-04.1` | `LAB-DEC-013` | `LAB-FE-006` | `LAB-API-v1` r3 | `BE-LAB-14` | `FE-LAB-08` | `AC-10`, `AC-39` | Direncanakan |
| `FR-04.2` | `LAB-DEC-013` | — | `LAB-API-v1` r3 | `BE-LAB-14` | `FE-LAB-08` | `AC-17` | Direncanakan |
| `FR-04.3` | `LAB-DEC-013` | `contracts/state-transition-matrix.md` | `LAB-STATE-v1` r2 | `BE-LAB-14` | `FE-LAB-08` | `AC-17` | Direncanakan |
| `FR-04.4` | `LAB-DEC-013` | `02-backend-architecture.md` | — | `BE-LAB-14` | — | Tinjauan struktur | Direncanakan |

### `EPIC-LAB-05` — Fakta kelayakan tagih per pemeriksaan

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-05.1` | `LAB-INH-013` | `CAP-11` | `LAB-INT-v1` r3 `INT-01` | `BE-LAB-13` | — | `AC-37` | Direncanakan |
| `FR-05.2` | `LAB-INH-013` | `CAP-11` | `LAB-INT-v1` r3 | `BE-LAB-13` | — | `AC-37` | Direncanakan |
| `FR-05.3` | `LAB-INH-013` | `CAP-11` `Ready to reuse` | `LAB-INT-v1` r3 | `BE-LAB-13` | — | `AC-12` | Direncanakan |
| `FR-05.4` | `LAB-DEC-011` | `CAP-12` | — | `BE-LAB-13` | — | `AC-13` | Direncanakan |

### `EPIC-LAB-06` — Pengelolaan alasan penolakan

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-06.1` | `LAB-DEC-019` | `CAP-05` | `LAB-API-v1` r3 | `BE-LAB-06` | `FE-LAB-03` | `AC-26` **terbukti** — [`BE-LAB-06.md`](../task/report/backend/BE-LAB-06.md) bagian 6.1; alasan "Sampel tidak diberi label" ditambahkan lewat `POST /` lalu langsung terbaca petugas, dan penonaktifannya terbukti lewat `PUT /{id}/activation` | **`SELESAI`** |
| `FR-06.2` | `LAB-DEC-019` | `LAB-FE-012`, `VAL-37` | `LAB-PERM-v1` r3 | `BE-LAB-06` | `FE-LAB-03` | `AC-26` jalur gagal **terbukti** — [`BE-LAB-06.md`](../task/report/backend/BE-LAB-06.md) bagian 5; `VAL-37` diuji tiga arah: penanda kesalahan internal, penanda wajib catatan, dan sifat menyeluruh penolakannya. Hak akses `LabRejectionReason : SystemFlag` berdiri terpisah dari `: Update` | **`SELESAI`** — dibangun; penanda biaya belum dapat disetel sampai pemegang `SystemFlag` ditetapkan manajemen |
| `FR-06.3` | `LAB-DEC-019` | `CAP-05` | — | `BE-LAB-06` | — | Data awal terisi **terbukti** — [`BE-LAB-06.md`](../task/report/backend/BE-LAB-06.md) bagian 5; `LabRejectionReasonSeeder` mengisi sepuluh alasan baseline, tidak menimpa keputusan pengguna, dan memakai identitas yang sama dengan migration `20260824091610` | **`SELESAI`** |

### `EPIC-LAB-08` — Pendaftaran pasien datang langsung dan rujukan luar

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-08.1` | `LAB-DEC-032` | `CAP-09` | `LAB-API-v1` r3 | `BE-LAB-08` | `FE-LAB-05` | `AC-44` | Direncanakan |
| `FR-08.2` | `LAB-DEC-032` | `LAB-INT-v1` r3 `INT-05` | `LAB-INT-v1` r3 | `BE-LAB-08`, `BE-EXT-03` | `FE-LAB-05` | `AC-45` | Menunggu eksternal |
| `FR-08.3` | `LAB-DEC-035` | `erd/data-dictionary.md` §9b | — | `BE-EXT-02`, `BE-EXT-03` | `FE-LAB-05` | `AC-46`, `AC-50` | Menunggu eksternal |
| `FR-08.4` | `LAB-DEC-032` | `INT-05` idempotensi | `LAB-INT-v1` r3 | `BE-EXT-03`, `BE-LAB-08` | — | `AC-44` | Menunggu eksternal |
| `FR-08.5` | `LAB-DEC-032` | `INT-05` perilaku tolak | `LAB-INT-v1` r3 | `BE-LAB-08` | `FE-LAB-05` | `AC-45` | Direncanakan |

### `EPIC-LAB-09` — Katalog, harga, dan cakupan penjamin

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-09.1` | `LAB-DEC-036` | `CAP-06` | `LAB-API-v1` r3 | `BE-LAB-07`, `BE-EXT-01` | `FE-LAB-04` | `AC-43` | Menunggu eksternal |
| `FR-09.2` | `LAB-DEC-033` | `CAP-10` | `LAB-API-v1` r3 | `BE-LAB-07` | `FE-LAB-04` | `AC-43` | Direncanakan |
| `FR-09.3` | `LAB-DEC-033` | `MstInsuranceTariff` | `LAB-INT-v1` r3 `INT-06` | `BE-LAB-07` | `FE-LAB-04` | `AC-43` | Direncanakan |
| `FR-09.4` | `LAB-DEC-033` | `VAL-50` | `LAB-API-v1` r3 | `BE-LAB-07` | `FE-LAB-04` | `AC-47`, `AC-48` | Direncanakan |
| `FR-09.5` | `LAB-DEC-036` | `INV-22`, `VAL-46` | `LAB-VAL-v1` r3 | `BE-LAB-07` | — | `AC-51` | Menunggu eksternal |

### `EPIC-LAB-10` — Monitoring per disiplin

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-10.1` | `LAB-DEC-025` | `03-domain-architecture.md` `S15` | `LAB-API-v1` r3 | `BE-LAB-15` | `FE-LAB-09` | `AC-41` | Direncanakan |
| `FR-10.2` | `LAB-DEC-025` | — | `LAB-API-v1` r3 | `BE-LAB-15` | `FE-LAB-09` | `AC-41` | Direncanakan |
| `FR-10.3` | `LAB-DEC-025` | `erd/data-dictionary.md` | `LAB-API-v1` r3 | `BE-LAB-01` | — | `AC-11` **terbukti**, `AC-41` terbukti sebagian — [`task/report/backend/BE-LAB-01.md`](../task/report/backend/BE-LAB-01.md) bagian 6; migration terbukti dua arah pada `QuilvianNewDevYoga`, bagian 5.1 | **`SELESAI`** |

### `EPIC-LAB-07` — Layar Laboratorium

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-07.1` | `LAB-DEC-010` | `03-frontend-architecture.md` §3.1 | `LAB-API-v1` r3 | — | `FE-LAB-06` | `AC-18`, `AC-43` | Direncanakan |
| `FR-07.2` | `LAB-DEC-010` | §3.2, `LAB-FE-009`, `LAB-FE-010` | `LAB-API-v1` r3 | — | `FE-LAB-07` | `AC-36` | Direncanakan |
| `FR-07.3` | `LAB-DEC-010` | §3.3, `LAB-FE-006` | `LAB-API-v1` r3 | — | `FE-LAB-08` | `AC-10`, `AC-17` | Direncanakan |
| `FR-07.4` | `LAB-DEC-010` | §3.4, `LAB-FE-011`, `LAB-FE-013` | `LAB-API-v1` r3 | — | `FE-LAB-02` | `AC-33` | Direncanakan |
| `FR-07.5` | `LAB-DEC-010` | §3.5, `LAB-FE-012` | `LAB-API-v1` r3 | — | `FE-LAB-03` | `AC-26` | Direncanakan |

---

## 2. Rekapitulasi Cakupan

| Yang diperiksa | Jumlah | Tercakup | Keterangan |
|---|---:|---:|---|
| Functional requirement `FR-*` | 45 | **45** | Seluruhnya punya task backend, task frontend, atau keduanya |
| Acceptance criteria pada `testing/acceptance-test-matrix.md` | 30 | **30** | Naik dari 28 setelah `AC-11` dan `AC-19` ditambahkan 2026-09-02. Seluruhnya dirujuk minimal satu task |
| Epic dalam scope | 10 | **10** | `EPIC-LAB-01` .. `EPIC-LAB-10` |
| Slice dalam scope | 10 | **10** | `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |
| Endpoint To-Be pada `contracts/api-contract.md` | 37 | **37** | Diaudit 2026-09-02; empat endpoint Lab Examination semula tanpa pemilik, ditutup `BE-LAB-16` |
| Aturan validasi `VAL-*` | 50 | **50** | Seluruhnya berpemilik per bagian matriks. `VAL-09` — empat mata pada tingkat wadah — semula tidak dikutip task mana pun; kini tegas di `BE-LAB-12` |
| Entity pada `02-backend-architecture.md` §4 | 9 | **9** | Delapan berpemilik task; `TrxLabTransitionHistory` dipakai apa adanya tanpa pekerjaan struktur |
| Pasangan kewenangan `resource : action` | 29 | **29** | 16 baru dan berpemilik task; 13 sisanya sudah terdaftar pada `c87d9c0` |
| Kontrak integrasi `INT-*` | 6 | **6** | `INT-01`, `INT-05`, `INT-06` berpemilik task; `INT-02` sampai `INT-04` sudah berjalan dan tidak disentuh Rilis 1 |
| Task backend | 19 | — | 16 milik Laboratorium, 3 dependency eksternal |
| Task frontend | 9 | — | Seluruhnya milik Laboratorium |

---

## 3. Kemampuan Existing yang Dipakai Ulang

Delapan belas task di atas bersandar pada kemampuan yang **sudah ada dan terbukti**, bukan
dibangun ulang:

| Capability | Status | Dipakai oleh |
|---|---|---|
| `CAP-01` pesanan lab | `Extend` | `BE-LAB-01`, `BE-LAB-15` |
| `CAP-02` siklus hidup sampel | `Extend` | `BE-LAB-09`, `BE-LAB-12` |
| `CAP-04` riwayat perpindahan status | `Ready to reuse` | `BE-LAB-03`, `BE-LAB-10` |
| `CAP-05` alasan penolakan | `Reuse with adapter` | `BE-LAB-06` |
| `CAP-06` katalog `MstProcedure` | `Reuse with adapter` | `BE-LAB-07` |
| `CAP-08` kunjungan pasien | `Ready to reuse` | `BE-LAB-08`, `BE-LAB-15` |
| `CAP-09` identitas pasien dan dokter | `Ready to reuse` | `BE-LAB-08` |
| `CAP-10` tarif dan salinannya | `Ready to reuse` | `BE-LAB-07`, `BE-LAB-11` |
| `CAP-11` fakta kelayakan tagih | `Ready to reuse` | `BE-LAB-13` |
| `CAP-12` batas kewenangan finansial | `Ready to reuse` | `BE-LAB-13` |
| `CAP-13`, `CAP-14` kewenangan per aksi | `Ready to reuse` | `BE-LAB-04`, `BE-LAB-05`, `BE-LAB-06` |
| `CAP-15` identitas pelaku | `Ready to reuse` | `BE-LAB-05`, `BE-LAB-10` |
| `CAP-17` perlindungan konkurensi | `Ready to reuse` | `BE-LAB-03`, `BE-LAB-12` |
| `CAP-22` pola tujuh lapis frontend | `Ready to reuse` | `FE-LAB-01` |
| `CAP-23` `axiosInstance` dan Redux | `Ready to reuse` | `FE-LAB-01` |

**Satu kemampuan yang sengaja tidak dipakai ulang.** `CAP-16` — penegakan prinsip empat mata —
berstatus `Missing`, dan sistem permission yang ada **tidak dapat** menggantikannya:
`AccessPermissionService.HasAccessAsync` hanya menjawab boleh atau tidak, tidak pernah
membandingkan pelaku sebelumnya. `BE-LAB-05` wajib menegakkannya di dalam service.

---

## 4. Coverage Gap

### 4.1 Dua acceptance criteria dalam scope — ✅ **ditutup 2026-09-02**

| AC | Isi | Gap yang ditemukan | Yang dilakukan |
|---|---|---|---|
| `AC-11` | Pesanan lab dapat dibuat dari kunjungan Rawat Jalan, Rawat Inap, maupun IGD dengan alur kerja yang sama | Tercatat di decision log (`LAB-DEC-009`) tetapi tidak punya baris di `testing/acceptance-test-matrix.md`. `CAP-08` menyatakan kemampuannya sudah ada, tetapi `BE-LAB-01` menambah kolom pada `LabOrder` sehingga jalur pembuatan pesanan tersentuh | Bagian **1b. Alur Pemesanan Lintas Unit** ditambahkan ke matriks dengan empat skenario, termasuk pembuktian bahwa penambahan kolom disiplin tidak melahirkan cabang khusus per jenis kunjungan. `AC-11` menjadi bukti verifikasi `BE-LAB-01` |
| `AC-19` | Tidak ada satu pun tabel atau endpoint Laboratorium yang menyimpan stok, pembelian, atau pemakaian reagen | Penjaga batas scope sejenis `AC-42` Bank Darah, yang sudah punya baris uji. `AC-19` tidak punya | Ditambahkan sebagai uji unit penelusuran pada bagian 7e matriks, sejajar `AC-42`, dan menjadi bukti verifikasi `BE-LAB-15` |

Dengan keduanya ditutup, **seluruh acceptance criteria dalam scope kini punya baris uji.**

### 4.2 Dua puluh satu acceptance criteria di luar scope — bukan gap

`AC-01` .. `AC-09`, `AC-14` .. `AC-16`, `AC-20` .. `AC-23`, `AC-27`, `AC-29` .. `AC-32` juga
tidak punya baris uji, tetapi seluruhnya milik slice yang **memang di luar scope**: pengisian
dan validasi hasil (`S4`), nilai kritis (`S5`), koreksi hasil (`S6`), pemberitahuan (`S8`), dan
penyuntingan pesanan oleh dokter (`S1b`). Ketiadaan barisnya wajar dan tidak perlu ditutup
sekarang.

### 4.3 Dua peran yang belum ditetapkan

Keduanya bukan penahan pembangunan, melainkan penahan pernyataan siap pakai. Task dapat
dibangun dan diuji dengan peran contoh, tetapi belum dapat dinyatakan siap dipakai sebelum
manajemen rumah sakit menetapkan pemegangnya.

| Hak akses | Dibangun oleh | Akibat selama pemegangnya belum ada |
|---|---|---|
| `LabCriticalBound : Approve` | `BE-LAB-05` | Tidak ada akun yang dapat menyetujui pengajuan, sehingga batas kritis tetap tidak dapat diubah lewat aplikasi |
| `LabRejectionReason : SystemFlag` | `BE-LAB-06` — **ditemukan 2026-09-03** | Tidak ada akun yang dapat menyetel penanda kesalahan internal, sehingga setiap alasan penolakan baru selalu bernilai "bukan kesalahan internal". Akibat nyatanya: pengambilan ulang untuk alasan-alasan baru itu **dapat ditagihkan kepada pasien** sampai peran ini ditetapkan. Perlu diketahui Billing |

### 4.4 Utang pembukuan — ✅ **keduanya ditutup 2026-09-02**

| Butir | Keadaan |
|---|---|
| ~~`input_hashes` pada manifest masih milik revisi lama~~ | ✅ **Ditutup.** Konvensinya ditemukan dari `pharmacy/blueprint-manifest.md` dan `billing-kasir/blueprint-manifest.md`: **sha256 penuh atas isi ber-line-ending LF**, bukan 16 digit. Metodenya diverifikasi cocok dengan keempat `artifact_hashes` pharmacy sebelum dipakai. Keempat hash Laboratorium dihitung ulang, dan nilai 16 digit lama dinyatakan keliru — tidak cocok dengan isi berkas mana pun |
| ~~`Riwayat Revisi` pada `00-interview-decisions.md` memuat baris revision 19 dua kali~~ | ✅ **Ditutup 2026-09-02.** Kedua salinan digabungkan; keduanya sempat bertentangan soal lokasi canonical dokumen tata kelola dan soal arti `LAB-OPEN-018`. Dicatat sebagai decisions revision 21 |

---

## 5. Penahan yang Masih Terbuka

| ID | Yang tertahan | Pencabut |
|---|---|---|
| ~~`LAB-OPEN-018`~~ | ~~Eksekusi seluruh task backend~~ | ✅ Ditutup 2026-09-02 — rules root runtime 32 berkas |
| `LAB-OPEN-018b` | Tidak menahan apa pun; `/plugin update` berikutnya mengembalikan rules root ke 13 berkas | Pendaftaran ulang marketplace ke `DevBenari/QuilvianEngineeringSkills` |
| ~~`LAB-OPEN-019`~~ | ~~Entity `Lab*` dan migration~~ | ✅ Ditutup 2026-09-02 — registry kini `ACTIVE` |
| ~~`LAB-OPEN-020`~~ | ~~Pemeriksaan konformansi QBE~~ | ✅ Ditutup 2026-09-02 — checker `PASS`, exit 0 |
| ~~`LAB-OPEN-021`~~ | ~~Penamaan dua tabel batas nilai~~ | ✅ Ditutup 2026-09-02 — ditetapkan `Lab` |
| `LAB-OPEN-012` | `BE-LAB-11`, `FR-02.4`, `FR-02.6` | Pemilik repository backend atau DBA |
| `LAB-SIGN-001` | Slice `S4`, `S4b`, `S4c`, `S5`, `S6` — **di luar scope roadmap ini** | Dokter PJ laboratorium atau Komite Medis |
| `LAB-AMD-001` | Slice `S1b` — **di luar scope roadmap ini** | Pemilik blueprint `rawat-jalan` |

---

## 6. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-02 | Traceability pertama. 45 FR, 28 AC, 10 epic, dan 10 slice dipetakan ke 18 task backend dan 9 task frontend. Dua coverage gap dalam scope ditemukan dan dicatat | `DRAFT` |
| 2 | 2026-09-02 | Kedua coverage gap ditutup: `AC-11` alur pemesanan lintas unit dan `AC-19` batas reagen ditambahkan ke matriks uji. Cakupan acceptance criteria naik menjadi 30 dari 30. Utang pembukuan riwayat revisi decisions ikut ditutup | `DRAFT` |
| 3 | 2026-09-02 | `input_hashes` dihitung ulang sebagai sha256 penuh setelah konvensinya ditemukan dari pharmacy dan billing-kasir dan diverifikasi. `LAB-OPEN-020` ditetapkan menjadi wewenang Andry Zain. Seluruh utang pembukuan tertutup | `DRAFT` |
| 4 | 2026-09-02 | Audit cakupan endpoint ditambahkan sebagai dimensi ketiga di samping FR dan AC. Empat endpoint Lab Examination ternyata tanpa pemilik task; `BE-LAB-16` ditambahkan pada roadmap backend. Total task backend menjadi 19 | `DRAFT` |
| 5 | 2026-09-02 | Audit cakupan diperluas ke aturan validasi, entity, kewenangan, dan integrasi. Seluruhnya berpemilik. Temuan terpenting: `VAL-09`, aturan empat mata pada tingkat wadah, semula tidak dikutip task mana pun — kini dibebankan ke `BE-LAB-12`. Tujuh dimensi cakupan kini terperiksa | `DRAFT` |
| 13 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-02.2`, `FR-02.3`, dan `FR-02.5` diperbarui oleh `BE-LAB-12`: `AC-36` terbukti — menolak wadah menggugurkan seluruh pemeriksaan yang ditopangnya — dan `VAL-13` ditegakkan secara struktural. `VAL-05` sampai `VAL-15` masing-masing punya ujinya, termasuk `VAL-09` empat mata yang ditulis sebagai kode di dalam service. `FR-02.4` dan `FR-02.6` keluar dari `BLOCKED` setelah `LAB-OPEN-012` dijawab dengan angka `0`. `AC-37` baru terbukti pada tingkat status; penerbitan fakta per pemeriksaan tetap milik `BE-LAB-13` | `DRAFT` |
| 12 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-02.1` naik dari `SELESAI SEBAGIAN` menjadi **`SELESAI`**: `BE-LAB-16` melengkapi entity dari `BE-LAB-09` dengan empat endpoint, sehingga `AC-35` kini terbukti ujung-ke-ujung — dua pemeriksaan ditambahkan lewat `POST /by-order` lalu terbaca lewat `GET /by-specimen` dengan satu barcode yang sama. `VAL-17` .. `VAL-20` masing-masing punya ujinya, dan batas terpenting task itu terbukti: membatalkan satu pemeriksaan tidak mengubah pemeriksaan lain maupun status wadahnya | `DRAFT` |
| 11 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-02.1` berpindah dari `Direncanakan` menjadi **`SELESAI SEBAGIAN`**: entity `LabExamination` tuntas pada `BE-LAB-09` berikut pembuatan dan eksekusi migration dua arah, sementara endpoint beserta penolakannya menunggu `BE-LAB-16`. `AC-35` dan `AC-40` terbukti pada tingkat struktur. Satu risiko dicatat: keenam kolom yang harus pindah dari `TrxLabSpecimen` belum dipindahkan karena `BE-LAB-11` masih `BLOCKED` oleh `LAB-OPEN-012`, sehingga salinan tarif untuk sementara ada di dua tempat. Satu pertentangan dokumen dicatat: kamus data bagian 4 menuntut kolom baru pada `TrxLabTransitionHistory`, sedangkan roadmap bagian 8.3 menyatakan tabel itu tanpa pekerjaan struktur | `DRAFT` |
| 10 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** Ketiga baris `EPIC-LAB-06` berpindah dari `Direncanakan` menjadi **`SELESAI`**: `BE-LAB-06` tuntas pada tingkat source dan test, tanpa menyentuh schema sehingga tanpa migration. `AC-26` terbukti seluruh jalur — menambah, menonaktifkan, penolakan `VAL-37` dua arah, dan penyetelan sah oleh administrator sistem. `FR-06.3` terbukti lewat `LabRejectionReasonSeeder` yang mengisi sepuluh alasan baseline tanpa menimpa keputusan pengguna. Satu risiko organisasi dicatat pada `FR-06.2`: pemegang `LabRejectionReason : SystemFlag` belum ditetapkan, sehingga alasan baru selalu bernilai "bukan kesalahan internal" dan pengambilan ulangnya dapat ditagihkan kepada pasien sampai peran itu ada | `DRAFT` |
| 9 | 2026-09-02 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-10.3` berpindah dari `Direncanakan` menjadi **`SELESAI`**: `BE-LAB-01` tuntas pada tingkat source, test, pembuatan migration, dan eksekusi migration ke `QuilvianNewDevYoga` beserta pembuktian jalur `Down`. `AC-11` terbukti lewat tiga skenario kunjungan; `AC-41` terbukti separuh karena daftar pantau per disiplin adalah cakupan `BE-LAB-15`. Satu temuan lintas modul dibuka sebagai `LAB-REQ-003` — penyimpangan status `FINAL`/`CLOSED` pada Billing yang mematikan koreksi AR. Catatan pembukuan: revision 6 sampai 8 tidak pernah tercatat pada tabel ini walaupun kepala dokumen sudah menyebut revision 8 — selisih itu peninggalan sebelum task ini dan menjadi utang pemilik blueprint | `DRAFT` |
