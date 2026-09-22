# Laporan Perubahan Frontend — `FE-IGD-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-017` |
| Judul | Entri susulan, koreksi, dan daftar pantau |
| Slice | `IGD-S05` · `EPIC IGD-06` |
| Roadmap | [frontend-roadmap.md](../../../roadmap/frontend-roadmap.md) kartu `FE-IGD-017` (R3.2), status pada R3.11 |
| Trace | `IGD-DEC-065`, `IGD-DEC-066`, `IGD-DEC-085`, `IGD-DEC-090`; **`IGD-DEC-137`**; `IGD-EV-123` butir 2; `BE-IGD-033`, `BE-IGD-034`, **`BE-IGD-049`** |
| Contract version | API **`0.9.0`** bagian `2.4` — `draft`; `recordedByName` dan `approvedByName` (`string?`) pada `EmergencyDepartureEventResponse` |
| Wewenang UI | `DEV_DISCRETION`. Pemilik menugaskan backlog ini 21 September 2026 dan mencabut blokirnya sesudah `BE-IGD-049` selesai dan build terverifikasi. Batas: dua nilai pada sel yang sudah ada; nol elemen baru |
| Dependency | `BE-IGD-033` ✅, `BE-IGD-034` ✅, **`BE-IGD-049` 🟡** — Implementation Complete, `dotnet build` 0 error; **runtime belum diverifikasi** |
| Klasifikasi | `LIGHT` — 1 berkas source diubah (+9/−2), satu berkas test baru; nol kontrak baru dikonsumsi selain dua ruas, nol CSS, nol backend |
| Task mode | `FRONTEND` dengan wewenang laporan lintas repository yang sempit |
| Target tulis | `QuilvianSystemFrontendDev` (source dan test); laporan ini dan tautan bukti pada roadmap/traceability di `NewQuilvianSystemBackend` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `198d56d9e` pada branch `RizkiV2` (bersih, sinkron dengan origin sebelum task). **Kemudian di-commit pemilik sebagai `c941012ac`** ("melanjutkan asigment pasien", 21 September 2026 16:01), bersama berkas `FE-IGD-034`. **Belum di-push** — `RizkiV2` `ahead 1` terhadap `origin/RizkiV2` pada 22 September 2026 (diperiksa agent) |
| Commit backend yang dijadikan rujukan | `267b56a0` pada branch `rizkiG`; source `BE-IGD-049` masih di working tree, belum di-commit |
| Tanggal | 21 September 2026 (larut malam) |
| Status | ✅ **Selesai — 22 September 2026, atas penilaian pemilik.** Kolom *Pelaku* dan *Penyetuju pembalikan* membaca nama dari backend, bukan GUID. `eslint` 0 error, 5 unit test baru lulus, seluruh suite 1415 dari 1424 lulus (9 gagal = sudah ada sebelumnya, semuanya Rawat Inap). **Uji layar pemilik 21 September 2026 (larut malam) LULUS**: kolom *Pelaku* menampilkan `SuperAdmin` pada ketiga kejadian, bukan GUID (bagian 6.1). **`npm run build` dinyatakan lulus oleh pemilik** (22 September 2026); artefak `.next` 21 September 16:03 dibangun dari commit `c941012ac` yang memuat kedua berkas task ini — diperiksa agent. UAT belum dan tidak diklaim |

**UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0** (bagian 3.3).

*Riwayat.* Task ini dihentikan pada gerbang 21 September 2026 karena respons event kepergian belum memuat nama; pemilik
menyetujui delta backend (`IGD-DEC-137`), `BE-IGD-049` dikerjakan, dan blokirnya dicabut. Rinciannya di bagian 1.

---

## 1. Keadaan yang ditemukan di awal

Kriteria 1 kartu: *"Riwayat kejadian terbaca urut beserta pelakunya."* Yang tampil adalah GUID mentah (`IGD-EV-123` butir 2),
dan dikonfirmasi lewat layar oleh pemilik: kepergian `DEP-260921064559-9B2DB7` menampilkan `0ba84a1a-…` pada ketiga kejadiannya.

| Temuan | Bukti |
| --- | --- |
| Tab menampilkan `event.recordedByUserId` dan `event.approvedByUserId` apa adanya | `emergency-assessment-transfer-tab.jsx` baris 239–240 (sebelum task) |
| Respons backend lama hanya memuat ID, tanpa nama; kontrak tidak menjanjikan nama pada event kepergian; frontend tidak punya pencarian pengguna | Bagian *Riwayat* di atas; laporan `BE-IGD-049` |
| Kini respons memuat `recordedByName` dan `approvedByName`; ruas ID tetap dikirim | `api-contract.md` `0.9.0` bagian 2.4; `BE-IGD-049` |
| **Slice meneruskan item apa adanya** — tidak ada pemetaan yang membuang ruas baru | `buildListThunk` mengembalikan `paged.items`; `fetchTransfers` memakainya. Nama sampai ke tab tanpa perubahan lain |
| Setiap aksi (koreksi, serah terima, berangkat, tiba) memuat ulang daftar lewat `reload()` | `emergency-assessment-transfer-tab.jsx` — jadi nama pada baris baru ikut segar |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: perawat atau dokter IGD yang membuka layar Assesmen IGD, tab **Transfer**, segmen **Riwayat**.

1. Pengguna membuka riwayat kepergian pasien.
2. Setiap kepergian menampilkan **Riwayat kejadian**: *Kepergian disiapkan*, *Serah terima diajukan*, *Pasien berangkat*, dan seterusnya.
3. Pada tiap kejadian, kolom **Pelaku** menampilkan **nama** petugas yang mencatatnya, dan **Penyetuju pembalikan** menampilkan nama
   penyetuju bila kejadian itu hasil pembalikan.

*Contoh.* Sebelum: *Pelaku* `0ba84a1a-2559-49ba-a320-10fb1f399d70`. Sesudah: *Pelaku* `Ns. Ani Rahmawati`.

**Jalur tidak normal.**

| Keadaan | Yang tampil |
| --- | --- |
| Kejadian biasa, tanpa penyetuju | *Penyetuju pembalikan*: `-` |
| Pengguna pencatat sudah tidak ditemukan di backend (`recordedByName = null`) | *Pelaku*: `-` — **bukan** GUID |
| Backend belum memuat `BE-IGD-049` (nama tidak dikirim) | *Pelaku*: `-` — layar tidak jatuh ke ID |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` frontend; `rules/frontend/frontend-architecture.md`; `base-component-decision-gate.md`;
`ui-consistency-checklist.md`; `emergency-assessment-transfer-tab.jsx`; `emergency-assessment-slice.jsx` (`buildListThunk`,
`fetchTransfers`); `EmergencyDepartureDtos.cs`; `api-contract.md` bagian 2.4; pola unit test berbasis source
(`inpatient-foundation.test.mjs`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/emergency-installation-management/emergency-assessment-view/components/emergency-assessment-transfer-tab.jsx` | Dua nilai sel: `event.recordedByUserId` → `event.recordedByName`, `event.approvedByUserId` → `event.approvedByName`; fallback tetap `"-"`. Ditambah satu komentar yang menyatakan larangan jatuh ke ID (`IGD-DEC-137`) |
| `tests/unit/emergency-departure-event-actor-name.test.mjs` | **Baru** — 5 uji berbasis source sebagai pengaman regresi (bagian 6) |

**Tidak disentuh:** slice Redux, service, hook, CSS (nol berkas `.css`), `globals.css`, seluruh backend.

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi tidak berubah: view membaca data yang sudah disediakan slice; tidak ada request baru, tidak ada helper baru.
Keputusan gerbang komponen:

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Sel *Pelaku* | `<dl>`/`<dt>`/`<dd>` grid yang sudah ada (`styles.recordGrid`) | `emergency-assessment-transfer-tab.jsx`; dipakai seluruh kartu riwayat tab ini | REUSE | Hanya nilai yang berubah |
| Sel *Penyetuju pembalikan* | Sama | Sama | REUSE | Hanya nilai yang berubah |

Elemen `NEW` atau `EXTEND`: **nol**. Tidak ada keputusan yang menunggu pemilik.

**Pilihan yang sengaja tidak dipakai.** Fallback `event.recordedByName || event.recordedByUserId || "-"` (nyaman saat backend lama
masih berjalan) **ditolak**: ia menampilkan GUID, dan `IGD-DEC-137` melarangnya secara eksplisit.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat, kosong, gagal, tanpa hak akses | **Tidak berubah** — ditangani `EmergencyAssessmentSection` seperti sebelumnya |
| Nama tersedia | Nama petugas |
| Nama kosong | Tanda hubung `-` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Departure

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/emergency-installation-management/emergency-departures?emergencyVisitId={id}` | Memuat riwayat kepergian dan kejadiannya; kini membawa `recordedByName` dan `approvedByName` | `EmergencyDeparture : Read` |

Request, route, dan hak akses **tidak berubah**.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/emergency-departure-event-actor-name.test.mjs` | 5 dari 5 lulus | `PASS` | Keluaran perintah |
| **Kontrol negatif** — pemeriksaan yang sama dijalankan terhadap versi `HEAD` | Versi lama: `membaca recordedByName` **false**, `menyentuh ruas ID` **true**. Versi baru: **true** dan **false** | `PASS` | Membuktikan uji itu membedakan kode lama dari kode baru |
| `npx eslint` pada berkas yang diubah | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `npm run test:unit` (seluruh suite) | 1424 test: **1415 lulus, 9 gagal** | `EXISTING / ENVIRONMENT ISSUE` | Sembilan yang gagal identik dengan sebelum task ini: delapan `FE-RWI-042`/`043`/`044` dan satu `accounting-reconciliation` (menu rute Rawat Inap). Nol rujukan ke berkas yang diubah. Total naik 1419 → 1424 = lima test baru |
| Grep anti-regresi checklist UI, 9 baris ditambahkan | Warna literal 0, typography 0, `<button` mentah 0, `<table` 0, `fw-`/`fs-` 0, `!important` 0, inline style 0; berkas CSS berubah 0 | `PASS` | Keluaran perintah |
| Tampilan nama pada layar dengan backend yang memuat `BE-IGD-049` | *Pelaku* menampilkan **`SuperAdmin`** pada ketiga kejadian `DEP-260921064559-9B2DB7`; *Penyetuju pembalikan* `-` | `PASS` | Tangkapan layar pemilik, 21 September 2026 (larut malam) — bagian 6.1 |
| `npm run build` | Dijalankan **pemilik**; dinyatakan lulus (22 September 2026) | `PASS` (pernyataan pemilik) | Keluaran build **tidak dilampirkan**. Jejak yang diperiksa agent: `.next/BUILD_ID` dan `.next/standalone/server.js` berjam 21 September 16:03, lebih baru dari commit `c941012ac` (16:01) yang memuat kedua berkas task ini |

Uji manual: `NOT FEASIBLE` oleh agent (tidak ada sesi login; uji layar berjalan pada backend milik pemilik dan menulis ke basis data
yang dilarang bagi agent); **`REQUIRED`** oleh pemilik.

### 6.1 Uji layar pemilik — 21 September 2026 (larut malam)

Pemilik membuka kembali kepergian yang sama dengan uji sebelumnya (`DEP-260921064559-9B2DB7`, tiga kejadian) sesudah backend
dibangun ulang dan dijalankan ulang serta frontend memuat perubahan ini.

| Kejadian | *Pelaku* **sebelum** (uji 21 Sep malam) | *Pelaku* **sesudah** | *Penyetuju pembalikan* |
| --- | --- | --- | --- |
| Kepergian disiapkan | `0ba84a1a-2559-…` (GUID) | **`SuperAdmin`** | `-` |
| Serah terima diajukan | `0ba84a1a-2559-…` (GUID) | **`SuperAdmin`** | `-` |
| Pasien berangkat | `0ba84a1a-2559-…` (GUID) | **`SuperAdmin`** | `-` |

**Yang dibuktikan lewat layar.** (1) Kriteria 1: pelaku tampil sebagai **nama** pada ketiga kejadian, tanpa GUID. (2) `BE-IGD-049`
berjalan pada jalur `GET` daftar kepergian yang dipakai tab ini: backend yang berjalan sudah memuat perubahan dan mengirim
`recordedByName`. (3) `Penyetuju pembalikan` tetap `-` karena tidak ada pembalikan — sesuai jalur "tanpa penyetuju".

**Yang belum dibuktikan.** Nama pada respons `amend` dan `reverse`; nama `null` untuk pengguna yang tidak ditemukan; dan bahwa
kueri nama hanya **satu** per respons (`BE-IGD-049` S2–S5). `npm run build` untuk revisi ini tidak dilaporkan pemilik.

**Yang dibuktikan unit test** (berbasis source, bukan render): kolom *Pelaku* membaca `recordedByName`; kolom *Penyetuju pembalikan*
membaca `approvedByName`; keduanya jatuh ke `"-"` bila kosong; tab **tidak** membaca `recordedByUserId` maupun `approvedByUserId`
di mana pun; kolom *Terjadi* dan *Dicatat* tidak berubah. **Tidak dibuktikan:** bahwa nama sungguhan benar-benar tampil di layar.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Riwayat kejadian terbaca urut beserta pelakunya | **Terbukti lewat layar** (21 September 2026, larut malam) | Bagian 3.2, 6.1; uji 1–4. Pelaku tampil sebagai `SuperAdmin`, bukan GUID; urutan kejadian tidak berubah |
| 2. Baris yang dikoreksi tetap terlihat, ditandai tidak berlaku | Terpenuhi (tidak berubah) | Badge "Tidak berlaku"; uji 5 menjaga kolom lain tetap |
| 3. Waktu sebenarnya di masa depan ditolak di layar | Terpenuhi (tidak berubah) | `emergency-assessment-transfer-tab.jsx` — logika koreksi tidak disentuh |

**Definition of Done.** Terpenuhi. Laporan tracked ada; roadmap, traceability, dan `MODULE-STATUS.md` diperbarui. `npm run build`
dinyatakan lulus pemilik 22 September 2026 (keluaran tidak dilampirkan), sehingga satu-satunya butir penahan ✅ tertutup.

*Riwayat penilaian sebelumnya (21 September 2026).* **Belum:** `npm run build`,
tersisa **satu butir**: `npm run build` untuk revisi ini tidak dilaporkan pemilik. Uji layar kriteria 1 sudah **lulus** (6.1). Status tetap **🟡** hanya karena butir build itu, mengikuti standar yang dipakai pada `FE-IGD-027`; bila pemilik menyatakan build lulus atau menerima ✅ tanpa build terpisah, task ini ✅.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Selama backend yang berjalan masih DLL lama, layar menampilkan `-` pada kolom *Pelaku* (bukan GUID lagi, tetapi juga belum nama). Itu **perilaku yang disengaja** sesuai `IGD-DEC-137`, bukan cacat. Bangun ulang dan restart backend dahulu |
| Masalah yang diketahui | Ruas aktor lain pada kontrak kepergian tetap ID dan tidak ditampilkan tab ini (`IGD-DEC-137` sengaja sempit) |
| Dependency backend | `BE-IGD-049` 🟡 — uji API S1–S5 milik pemilik (laporan `BE-IGD-049` bagian 5.1) |
| Perubahan sampingan | `NONE`. Berkas sementara untuk kontrol negatif (salinan `HEAD`) dihapus segera setelah dipakai |
| Interupsi | `NONE` |
| Status Git | **Diperbarui 22 September 2026:** kedua berkas sudah di-commit pemilik sebagai `c941012ac`; working tree frontend bersih. **Commit itu belum di-push** — `RizkiV2` `ahead 1` terhadap `origin/RizkiV2`. Agent tidak melakukan stage, commit, maupun push |
| Langkah berikutnya | (1) ~~Bangun ulang + restart backend, buka riwayat `DEP-260921064559-9B2DB7`~~ — **lulus** (6.1). (2) ~~Pemilik menyatakan `npm run build` lulus~~ — **dinyatakan 22 September 2026** → `FE-IGD-017` ✅. (3) ~~Uji API `BE-IGD-049` S2–S5~~ — dinyatakan lulus pemilik 22 September 2026 ([laporan `BE-IGD-049`](../backend/BE-IGD-049.md)). (4) **Pemilik: push `c941012ac`** |
