# Roadmap Frontend Peta Bangsal Denah

| Metadata | Nilai |
|---|---|
| blueprint_id / owner | `RWI-BP-001` / `episode-rawat-inap` |
| planning_id / revision / tanggal | `PETA-DEL-20261020-01` / 1 / 2026-10-20 Asia/Jakarta |
| status / approval | `APPROVED_DESIGN_PLANNED`; desain disetujui `RWI-DEC-352`, palet titik `RWI-DEC-353`. Eksekusi memakai `build-module-frontend`, tepat satu task per pemanggilan |
| kontrak terkunci | `0.18.0`; [Frontend 21](../03-frontend-architecture.md), [PRD 32](../04-prd-to-mvp.md), [Acceptance 26](../testing/acceptance-test-matrix.md). API contract 14.2 tidak berubah |
| aturan keras | `RWI-DEC-325` butir 1, 2, dan 4 tetap: references `build-module-frontend` dan base component tidak wajib; komponen tinggal di folder feature denah dengan CSS module sendiri. Butir 3 digantikan `RWI-DEC-343` |
| source snapshot | Frontend `a491b05afcb6b3ed70218ccaa557007719d45ac3` (working tree bersih); backend `6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd` |
| worktree guard | Working tree frontend dapat dipakai paralel. Baca ulang source dan `git diff` sebelum edit, gabungkan perubahan pengguna secara sadar, dan jangan me-reset berkas |
| hash lock | [peta-bangsal-denah-delivery-lock.json](peta-bangsal-denah-delivery-lock.json) |
| scope / ID | 8 task `FE-RWI-272` s.d. `FE-RWI-279`; berikutnya `FE-RWI-280`. `FE-RWI-271` sengaja dilewati karena roadmap paralel prioritas member sudah menyebutnya sebagai ID berikutnya |
| roadmap pasangan | [Backend](backend-roadmap-peta-bangsal-denah.md) (nol task) · [Traceability](requirement-traceability-peta-bangsal-denah.md) · [Planning](../docs/map-bed/planning/planning-redesain-tampilan-denah-bed.md) |
| gerbang | `DEC-PETA-L3` — atasan mengesahkan contoh Lantai 3 dan Muhammad Hamzah mencatatnya sebagai keputusan baru di decision log (`RWI-DEC-349`) |

## Arti Tanda Status pada Dokumen Ini

| Tanda | Artinya |
|:---:|---|
| ✅ | Selesai; seluruh acceptance criteria dan DoD terbukti pada laporan task |
| 🟡 | Source ada, tetapi bukti acceptance belum lengkap; belum selesai |
| ⛔ | Prasyarat belum terpenuhi; task tidak boleh dimulai |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

Garis berarti prasyarat → task. Tidak ada prasyarat backend.

```text
FE-RWI-272 🟡 ─> FE-RWI-273 🟡 ─┬─> FE-RWI-274 🟡 ─┐
                                │                  │
                                └─> FE-RWI-275 🟡 ─┴─> {DEC-PETA-L3 ⛔}

{DEC-PETA-L3 ⛔} ─┬─> FE-RWI-276 ⛔ ─────┐
                  │                      │
                  ├─> FE-RWI-277 ⛔ ─┐   │
                  │                  │   │
                  └─> FE-RWI-278 ⛔ ─┴───┴─> FE-RWI-279 ⛔
```

`{DEC-PETA-L3 ⛔}` = gerbang pengesahan contoh Lantai 3 oleh atasan, dicatat Muhammad Hamzah sebagai
keputusan baru. Gerbang itu digambar dua kali sebagai simpul sambungan antara blok contoh dan blok
lima lantai lain; ia bukan task.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `FE-RWI-272` |
| 2 | `FE-RWI-272` | `FE-RWI-273` |
| 3 | `FE-RWI-273` | `FE-RWI-274`, `FE-RWI-275` — boleh paralel |
| — | ⛔ menunggu `DEC-PETA-L3`; gerbang baru dapat diputuskan setelah `FE-RWI-274` dan `FE-RWI-275` selesai | `FE-RWI-276`, `FE-RWI-277`, `FE-RWI-278` — boleh paralel sesudah gerbang |
| — | ⛔ menunggu `FE-RWI-276`, `FE-RWI-277`, dan `FE-RWI-278` | `FE-RWI-279` |

Pasangan prasyarat → task ada sembilan dan sama persis dengan kolom `Dependency`: `FE-RWI-272 →
FE-RWI-273`, `FE-RWI-273 → FE-RWI-274`, `FE-RWI-273 → FE-RWI-275`, `DEC-PETA-L3 → FE-RWI-276`,
`DEC-PETA-L3 → FE-RWI-277`, `DEC-PETA-L3 → FE-RWI-278`, `FE-RWI-276 → FE-RWI-279`, `FE-RWI-277 →
FE-RWI-279`, dan `FE-RWI-278 → FE-RWI-279`. Dua garis `FE-RWI-274 → {DEC-PETA-L3}` dan `FE-RWI-275 →
{DEC-PETA-L3}` adalah syarat gerbang, bukan pasangan task.

## Slice

| Slice | Isi | Task | Status |
|---|---|---|---|
| `PETA-S1` — contoh Lantai 3 (`MVP-PETA-1`) | Lantai 3 menjadi peta bangsal di keempat pemakai: lapisan peta, kartu kamar, kepala halaman, dan satu palet titik | `FE-RWI-272` s.d. `FE-RWI-275` | 🟡 20 Oktober 2026: keempat task `FE-RWI-272` s.d. `FE-RWI-275` 🟡 sebagian — source lengkap, `npm run lint` 0 error, `npm run build` lulus; uji manual peramban dijalankan pengguna sendiri dan belum dilakukan |
| Gerbang `DEC-PETA-L3` | Atasan melihat dan mengesahkan Lantai 3; revisi dikerjakan di Lantai 3 lebih dulu | — | ⛔ menunggu `PETA-S1` (per 20 Oktober 2026 source keempat task sudah ada; tersisa uji manual pengguna), lalu pengesahan atasan yang dicatat Muhammad Hamzah sebagai keputusan baru |
| `PETA-S2` — lima lantai lain (`MVP-PETA-2`) | Data skematik Lantai 2, 4, 5, 8, dan 9 | `FE-RWI-276` s.d. `FE-RWI-278` | ⛔ menunggu `DEC-PETA-L3` |
| `PETA-S3` — penutupan | Semua lantai berskematik; gambar lama berhenti dipakai; bukti kesiapan rilis | `FE-RWI-279` | ⛔ menunggu `PETA-S2` |

## Keputusan Reuse

| Elemen | Disposisi | Bukti / alasan |
|---|---|---|
| `rooms[]` di `lantai-N.plan.js` | Reuse apa adanya | Bentuk kamar dan dasar kunci database (Frontend 21.2 no. 2) |
| `floor-plan-room-map.js` | Reuse; tidak diubah | Kunci `floorPlanKey` tetap (`RWI-DEC-343`) |
| `use-floor-plan.js`, `use-floor-plan-layout.js`, seluruh service | Reuse; tidak diubah | Data denah sudah lengkap (Frontend 21.2 no. 5–7, 9) |
| `FloorPlanView` + `react-zoom-pan-pinch` | Extend | Memilih lapisan per lantai; zoom dan geser tetap |
| `FloorPlanDrawing` | Reuse sebagai cadangan selama masa contoh | `RWI-DEC-351`; berhenti dipanggil di `FE-RWI-279`; berkasnya disimpan (`RWI-FE-012`) |
| `FloorPlanRoom` | Extend | Renderer kartu untuk lantai berskematik; renderer lama dipertahankan sampai `FE-RWI-279` |
| `statusTone` di pop up | Diekstrak ke modul bersama | Dipakai titik kartu dan pop up (`RWI-DEC-347`, `RWI-DEC-353`) |
| Pop up kamar dan pop up Master | Reuse; hanya warna titik pop up yang berubah (`FE-RWI-275`) | `RWI-DEC-343`, `RWI-DEC-353` |
| `FloorTabs`, `ClassLegend`, `ZoomToolbar`, `FloorPlanCompass`, `FloorPlanSkeleton` | Extend | Angka tab, legenda per rupa lantai, gaya kerangka dan kompas |
| Base component (`Badge`, `StatusBadge`, dan sejenisnya) | Tidak wajib | `RWI-DEC-325` butir 4 |
| `lucide-react` 0.483 | Reuse | Ikon penanda sudah tersedia (planning 5.5); tidak menambah package |

## Register Task

| Task ID / Status | Outcome | Requirement / keputusan | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko / pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| 🟡 `FE-RWI-272` — [laporan](../task/report/frontend/FE-RWI-272.md) | Lantai 3 di keempat pemakai tampil di atas lapisan peta bangsal tanpa garis CAD; lantai lain tetap gambar lama | FR-DNH-017, 024; DEC-344, 345, 349, 351 | 0.18.0; Frontend 21.5, 21.8, 21.9 | `rooms[]`, `floors.js`, `FloorPlanView`, `FloorPlanDrawing` | Fungsi geometri, data skematik Lantai 3, `FloorPlanSchematic`, pemilihan lapisan per lantai, tiga pemakai | — | AC-498, 500 (Lantai 3); DNH-AT-498, 500, 917, 918, 922 | `V-FE`; Lantai 3 di empat pemakai; lantai lain tidak berubah | Rawat Inap FE; presisi koordinat | `D-FE` |
| 🟡 `FE-RWI-273` — [laporan](../task/report/frontend/FE-RWI-273.md) | Kamar Lantai 3 tampil sebagai kartu dengan rupa status, label HD/LD, titik bed, dan fokus keyboard yang benar | FR-DNH-018–021, 023; DEC-344, 346, 347, 350 | 0.18.0; Frontend 21.5–21.7, 21.9 | Data kamar `use-floor-plan`, `describeRoom`, `statusTone`, fungsi geometri `FE-RWI-272` | Renderer kartu, aturan rupa 1–7, label HD/LD, titik bed, varian ringkas, fokus dan `aria-label`, modul status bed bersama | `FE-RWI-272` | AC-499, 501–504 (Lantai 3); DNH-AT-499, 501–504, 914–916, 921, 923 | `V-FE`; data uji 328/329/330; akun Housekeeping; keyboard | Rawat Inap FE; urutan aturan rupa | `D-FE` |
| 🟡 `FE-RWI-274` — [laporan](../task/report/frontend/FE-RWI-274.md) | Kepala halaman menyebut ringkasan bed per status dan bed kosong per tab; legenda, kartu ringkas, kerangka, kompas, dan gerak selesai | FR-DNH-022; DEC-348; FE-011 | 0.18.0; Frontend 21.5, 21.9 | `floors[].availableBedCount`, `FloorTabs`, `ClassLegend`, `FloorPlanSkeleton`, `FloorPlanCompass` | `floor-plan-summary.jsx`, `floor-plan-room-hovercard.jsx`, angka tab, legenda per rupa, kerangka, kompas, animasi | `FE-RWI-273` | AC-505; DNH-AT-505, 919, 920, 922 | `V-FE`; jejak jaringan; gerak dikurangi; lebar 390 px | Rawat Inap FE; selisih ringkasan dan badge | `D-FE` |
| 🟡 `FE-RWI-275` — [laporan](../task/report/frontend/FE-RWI-275.md) | Titik status di kartu dan pop up memakai satu palet | FR-DNH-025; DEC-353 | 0.18.0; Frontend 21.7, 21.12 | Modul status bed bersama, CSS titik pop up | Token palet bersama; titik pop up dan kartu memakai token yang sama | `FE-RWI-273` | AC-508; DNH-AT-508; UAT-819, 820 | `V-FE`; kartu dan pop up 329, 325 | Rawat Inap FE; perubahan warna terlihat di keempat pemakai | `D-FE` |
| ⛔ `FE-RWI-276` | Lantai 2 tampil sebagai peta bangsal | FR-DNH-017, 018; DEC-344; FE-010 | 0.18.0; Frontend 21.5, 21.8; planning 5.9 | Fondasi `FE-RWI-272`–`275` | Data skematik Lantai 2, zona unit lain, penanda | `{DEC-PETA-L3}` | AC-498, 499 (Lantai 2); DNH-AT-498, 499, 918 | `V-FE`; bandingkan dengan `lantai-2.png` | Rawat Inap FE; banyak zona non-rawat-inap | `D-FE` |
| ⛔ `FE-RWI-277` | Lantai 4 dan 5 tampil sebagai peta bangsal | FR-DNH-017, 018; DEC-344; FE-010 | 0.18.0; Frontend 21.5, 21.8; planning 5.9 | Fondasi `FE-RWI-272`–`275` | Data skematik Lantai 4 dan 5 | `{DEC-PETA-L3}` | AC-498, 499 (Lantai 4–5); DNH-AT-498, 499, 918 | `V-FE`; bandingkan dengan `lantai-4.png`, `lantai-5.png` | Rawat Inap FE; kamar L 401, 418, 506 | `D-FE` |
| ⛔ `FE-RWI-278` | Lantai 8 dan 9 tampil sebagai peta bangsal | FR-DNH-017, 018; DEC-344; FE-010 | 0.18.0; Frontend 21.5, 21.8; planning 5.9 | Fondasi `FE-RWI-272`–`275` | Data skematik Lantai 8 dan 9 | `{DEC-PETA-L3}` | AC-498, 499 (Lantai 8–9); DNH-AT-498, 499, 918 | `V-FE`; bandingkan dengan `lantai-8.png`, `lantai-9.png` | Rawat Inap FE; kamar L 908, 909 | `D-FE` |
| ⛔ `FE-RWI-279` | Keenam lantai di keempat pemakai memakai peta bangsal; gambar lama berhenti dipakai; bukti kesiapan rilis lengkap | FR-DNH-023, 024; DEC-345, 351; FE-012 | 0.18.0; Frontend 21.8–21.10 | Seluruh hasil `FE-RWI-272`–`278` | Hapus jalur gambar lama dan renderer lama dari pemakaian, hapus gaya CAD tak terpakai, verifikasi penuh | `FE-RWI-276`, `FE-RWI-277`, `FE-RWI-278` | AC-498–500, 506 (semua lantai); DNH-AT-498–500, 506, 920, 922 | `V-FE`; 6 lantai × 4 pemakai | Rawat Inap FE; regresi pemakai | `D-FE` |

Singkatan pada tabel: `FR` = `FR-DNH-0xx`, `DEC` = `RWI-DEC-3xx`, `AC` = `RWI-AC-xxx`, `FE-0xx` = `RWI-FE-0xx`.

## Batas Handoff dan Definition of Done

### V-FE — bukti wajib setiap task frontend

1. Baca ulang source dan `git diff` sebelum mengedit; jangan menimpa perubahan pengguna.
2. Jalankan `npm run lint:errors`; garis dasar exit `0`.
3. Jalankan `npm run test:unit` untuk suite yang ada dan bandingkan dengan garis dasar terbaru;
   kegagalan lama bukan regresi task. Menulis test baru **opsional**; dianjurkan untuk fungsi murni
   (`schematic-geometry.js` dan aturan rupa status) dengan pola `node:test` di `tests/unit/`. Jest
   dilarang.
4. Jalankan `npm run build`. Bila dilewati, tulis `NOT RUN` sebagai butir yang dikecualikan atas
   keputusan pemilik 10 September 2026 (build dijalankan pengguna sendiri); jangan diklaim lulus.
5. Bukti visual per lantai dan mode (screenshot peramban Edge) dianjurkan dan dilampirkan di laporan.
   Uji manual peramban tidak menahan `✅` atas keputusan pemilik 1 September 2026, kecuali contoh
   Lantai 3 yang harus dapat ditunjukkan kepada atasan untuk gerbang `DEC-PETA-L3`.
6. Data pasien pada bukti visual memakai data uji samaran.

### D-FE — selesai berarti

- Setiap acceptance criteria task terpetakan ke source yang benar-benar ada, dan langkah `V-FE` 1–4
  dicatat apa adanya.
- Tidak ada perubahan pada hook data, service, kontrak API, route, menu, permission, package, backend,
  maupun database.
- Laporan tracked ada pada `task/report/frontend/<TASK-ID>.md`, dan keenam titik status pada roadmap
  ini serta traceability ikut diperbarui.

## Kartu Task

### 🟡 `FE-RWI-272` — Fondasi peta bangsal dan lapisan peta Lantai 3

| Field | Isi |
|---|---|
| Status | 🟡 **SEBAGIAN 20 Oktober 2026.** Kelima acceptance criteria (a)–(e) terpetakan ke source: `schematic-geometry.js`, `lantai-3.schematic.js`, `floor-plan-schematic.jsx`, `floors.js` (hanya Lantai 3 berskematik), `FloorPlanView` memilih lapisan per lantai, dan ketiga berkas pemakai meneruskan `schematic`. Skrip geometri `tests/unit/floor-plan-schematic-geometry.test.mjs` 11/11 lulus. `npm run lint` `0 errors` — 1083 warning, sama persis dengan garis dasar. `npm run test:unit` 3265/3300 lulus; 35 kegagalan berada di berkas uji di luar denah, dan 112/112 uji yang membaca berkas task ini lulus. `npm run build` percobaan pertama berhenti `EBUSY` karena folder `.next\standalone` dikunci proses `node .next/standalone/server.js`; diulang 20 Oktober 2026 sesudah proses itu berhenti: `✓ Compiled successfully in 69s`, 514 halaman statis, exit 0. **Dikerjakan ulang 20 Oktober 2026 atas revisi pemilik:** Area Servis utara Lantai 3 beserta penanda Lift, Toilet, dan Spoel hoek di dalamnya dihapus, lempeng lantai menjadi blok bangsal saja, dan `viewBox` dipangkas ke `y 386, 1406 × 551`; uji denah 134/134 lulus, `npm run build` `✓ Compiled successfully in 60s` exit 0, `npm run lint` `0 errors` — 1083 warning. Uji manual peramban di keempat pemakai **belum dilakukan** — dijalankan pengguna sendiri atas keputusan pengguna 20 Oktober 2026 — sehingga task ditandai kuning. Bukti: [laporan](../task/report/frontend/FE-RWI-272.md) |
| Outcome | Lantai 3 di Monitoring Bed, Pilih Bed, transfer, dan Master Bed tampil di atas lempeng lantai, koridor, blok zona, dan penanda berikon — tanpa dinding, pintu, tangga, garis sumbu, angka ukuran, label teknis, maupun judul "3rd Floor". Lantai lain tampil persis seperti sebelumnya |
| Trace | FR-DNH-017, FR-DNH-024; RWI-DEC-344, 345, 349, 351; RWI-AC-498 dan 500 untuk Lantai 3, RWI-AC-507 bagian contoh; DNH-AT-498, 500, 917, 918, 922; NFR-808, NFR-813 |
| Scope source | Baru: `src/lib/constants/health-services/inpatient-management/floor-plan/schematic/schematic-geometry.js`, `.../schematic/lantai-3.schematic.js`, `src/components/features/health-services/inpatient-management/floor-plan/floor-plan-schematic.jsx`. Diperbarui: `floors.js`, `floor-plan-view.jsx`, `floor-plan-monitoring.jsx`, `floor-plan-selection.jsx`, `src/components/view/health-services/master-data/bed/master-data-bed-floor-plan-view.jsx`, `floor-plan.module.css` |
| Cakupan | (1) Fungsi geometri murni `insetOrthogonal`, `roundedOrthogonalPath`, `largestRect`. (2) Data skematik Lantai 3: `viewBox` dipangkas ke area gedung, `footprint`, koridor (y 697,5–764), zona, dan penanda lift, tangga, NS3A, NS3B, pantry, toilet, spoel hoek, gudang linen — koordinat dicocokkan dengan `denah-map-ruangan-rs-mmc/lantai-3.png`. (3) `floors.js` memberi properti `schematic` hanya pada Lantai 3. (4) `FloorPlanView` menggambar `FloorPlanSchematic` dan `viewBox` skematik bila lantai punya `schematic`, tanpa judul lembar; bila tidak, gambar lama apa adanya. (5) Ketiga pemakai meneruskan data skematik lantai aktif dengan perubahan sekecil mungkin. (6) Skrip pemeriksa geometri Lantai 3 — boleh berupa test `node:test` — membuktikan 29 poligon terdaftar, hasil inset berada di dalam poligonnya, serta koridor dan penanda tidak menimpa kamar. Overlay kamar pada task ini masih renderer lama; kartu dikerjakan `FE-RWI-273` |
| Larangan | Tidak mengubah `rooms[]`, `floorPlanKey`, hook, service, atau kontrak; tidak menambah package; tidak menghapus berkas CAD; tidak mengubah rupa Lantai 2, 4, 5, 8, dan 9 |
| Dependency | — |
| Acceptance | (a) Lantai 3 di keempat pemakai tanpa unsur CAD. (b) Lantai 2, 4, 5, 8, dan 9 tetap gambar lama tanpa galat. (c) Satu panggilan `GET .../floor-plan` per lantai. (d) Skrip geometri Lantai 3 lulus. (e) Zoom, geser, Ctrl + Scroll, layar penuh, dan kompas tetap bekerja |
| Verifikasi wajib | `V-FE`; buka Lantai 3 di Monitoring, Pilih Bed admisi, transfer (detail episode dan Workspace Keperawatan), dan Master; bandingkan dengan `lantai-3.png`; buka satu lantai lain di setiap pemakai |
| Risiko / owner | Tim frontend Rawat Inap. Koordinat tidak presisi → cocokkan ulang dengan gambar sumber; berkas pemakai mungkin sedang diubah pekerjaan paralel |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-272.md` |

### 🟡 `FE-RWI-273` — Kartu kamar, rupa status, titik bed, dan fokus keyboard

| Field | Isi |
|---|---|
| Status | 🟡 **SEBAGIAN 20 Oktober 2026.** Keenam acceptance criteria (a)–(f) terpetakan ke source: renderer kartu `CardRoom` dan bentuk tanpa data `GhostRoom` di `floor-plan-room.jsx` (renderer lama tetap untuk lantai gambar lama), modul status bersama `floor-plan-bed-status.js` (rupa urutan 1–7, HD/LD dari `dependencySummary`, fokus, `aria-label`), pop up hanya mengimpor `bedStatusTone`. Uji `floor-plan-room-card.test.mjs` 10 uji baru; seluruh uji denah 122/122 lulus. `npm run lint` `0 errors` — 1083 warning, sama dengan garis dasar. `npm run test:unit` 3275/3310 lulus; 35 kegagalan identik dengan sebelum task dan berada di luar denah. `npm run build` sempat tertahan `EBUSY` (proses `node .next/standalone/server.js`); diulang 20 Oktober 2026 sesudah proses itu berhenti: `✓ Compiled successfully in 69s`, 514 halaman statis, exit 0. Uji manual peramban (data uji 328/329/330, akun Housekeeping, keyboard, mode Pilih pasien isolasi) **belum dilakukan** — dijalankan pengguna sendiri atas keputusan pengguna 20 Oktober 2026 — sehingga task ditandai kuning. Bukti: [laporan](../task/report/frontend/FE-RWI-273.md) |
| Outcome | Pada lantai berskematik, setiap kamar tampil sebagai kartu bersudut bulat di poligon aslinya dengan kelas, nomor, badge, dan titik bed. Rupa status mengikuti `RWI-DEC-346` dan `RWI-DEC-350`. Kamar penuh di Monitoring dan semua kamar berdata di Master dapat dibuka dengan keyboard |
| Trace | FR-DNH-018, 019, 020, 021, 023 (keyboard); RWI-DEC-344, 346, 347, 350; RWI-AC-499, 501, 502, 503, 504 untuk Lantai 3; DNH-AT-499, 501–504, 914, 915, 916, 921, 923; NFR-809, NFR-810; RWI-FE-009 |
| Scope source | Diperbarui: `floor-plan-room.jsx` (renderer kartu untuk lantai berskematik; renderer lama tetap untuk lantai gambar lama), `floor-plan-view.jsx` (meneruskan penanda lantai berskematik bila perlu), `floor-plan.module.css` (token status, kartu, varian ringkas), `floor-plan-room-popup.jsx` (hanya mengimpor pemetaan status bersama; perilaku tidak berubah). Baru: `floor-plan-bed-status.js` (pemetaan status bed ke nada, diekstrak dari `statusTone`) |
| Cakupan | (1) Kartu dari poligon `rooms[]`: inset 3 unit, sudut bulat, teks di kotak terbesar; varian ringkas bila lebar atau tinggi < 56 unit (BAYI-1, BAYI-2, ICU-CK, 520, 521, 522). (2) Rupa urutan 1–7 Frontend 21.6. (3) Label HD/LD hanya pada kamar penuh, dari `dependencySummary`; tanpa label bila semua belum dinilai atau ringkasan tidak dikirim. (4) Titik bed untuk kamar sampai enam bed, berurutan menurut label, warnanya dari modul status bersama. (5) Strip dan teks warna kelompok kelas; kelompok khusus tidak hijau. (6) Fokus: Monitoring dan Master semua kartu berdata dapat difokus; Pilih: kartu tidak layak `tabIndex = -1` dan `aria-disabled`. (7) `aria-label` menyebut penuh, HD, dan pesanan. (8) Master: ruang cadangan dan kamar belum dipetakan putus-putus berlabel |
| Larangan | Tidak menghitung HD dengan suara terbanyak; tidak menampilkan nama pasien, No. RM, jenis kelamin, tier, atau Guid; tidak mengubah isi maupun aksi pop up; tidak mengubah rupa lantai gambar lama |
| Dependency | `FE-RWI-272` |
| Acceptance | (a) 328 Terisi/Dipesan/Tersedia → tepi hijau, ikon ungu, `3/2`. (b) 329 penuh 1 HD + 2 LD → abu, label "HD", `3/3`. (c) 330 penuh, semua belum dinilai → tanpa label. (d) Akun Housekeeping → tanpa label dan ikon pesanan. (e) Kamar tanpa data API → putus-putus, tidak dapat diklik. (f) Tab dan Enter membuka kamar penuh di Monitoring dan kamar di Master |
| Verifikasi wajib | `V-FE`; data uji samaran 328, 329, 330; akun Housekeeping; keyboard di Monitoring dan Master; mode Pilih untuk pasien isolasi. Test unit untuk fungsi aturan rupa dianjurkan |
| Risiko / owner | Tim frontend Rawat Inap. Urutan aturan rupa salah → uji tiap urutan; renderer lama tidak boleh ikut berubah |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-273.md` |

### 🟡 `FE-RWI-274` — Kepala halaman, kartu ringkas, kerangka, kompas, dan gerak

| Field | Isi |
|---|---|
| Status | 🟡 **SEBAGIAN 20 Oktober 2026.** Keenam acceptance criteria (a)–(f) terpetakan ke source: `floor-plan-summary.jsx` (ringkasan lima kelompok status yang selalu berjumlah sama dengan total bed), angka `availableBedCount` di tab (Master dan respons tanpa `floors` tanpa angka), legenda per rupa lantai, `floor-plan-room-hovercard.jsx` (tanpa identitas pasien, dibuka kursor atau fokus), kerangka berbentuk kartu, kompas lebih kecil, animasi masuk yang mati saat `prefers-reduced-motion`, tanpa panggilan API baru. Uji `floor-plan-summary.test.mjs` 6 uji baru; seluruh uji denah 128/128 lulus. Pratinjau statis 390 px tanpa geser horizontal. `npm run lint` `0 errors` — 1083 warning, sama dengan garis dasar. `npm run test:unit` 3281/3316 lulus; 35 kegagalan identik dengan sebelum task dan berada di luar denah. `npm run build` sempat tertahan `EBUSY` (proses `node .next/standalone/server.js`); diulang 20 Oktober 2026 sesudah proses itu berhenti: `✓ Compiled successfully in 69s`, 514 halaman statis, exit 0. Uji manual peramban (jejak jaringan, gerak dikurangi, lebar 390 px) **belum dilakukan** — dijalankan pengguna sendiri atas keputusan pengguna 20 Oktober 2026 — sehingga task ditandai kuning. Bukti: [laporan](../task/report/frontend/FE-RWI-274.md) |
| Outcome | Kepala halaman menyebut jumlah bed lantai aktif per status dan setiap tab lantai menyebut bed kosong. Legenda mengikuti rupa lantai aktif, kartu ringkas muncul saat kursor atau fokus, kerangka memuat berbentuk kartu, dan animasi menghormati pilihan gerak dikurangi |
| Trace | FR-DNH-022; RWI-DEC-348; RWI-AC-505; DNH-AT-505, 919, 920, 922; NFR-808, 811, 812; RWI-FE-011 |
| Scope source | Baru: `floor-plan-summary.jsx`, `floor-plan-room-hovercard.jsx`. Diperbarui: `floor-plan-view.jsx`, `floor-plan-skeleton.jsx`, `floor-plan-compass.jsx`, `floor-plan.module.css` |
| Cakupan | (1) Ringkasan: total, tersedia, terisi, dipesan, persiapan (`WaitingCleaning` + `Cleaning`), dan tidak tersedia; jumlahnya sama dengan total bed lantai; Master dihitung dari data susunan. (2) Tab: angka dari `availableBedCount` bila respons membawa `floors`; Master dan respons tanpa `floors` tanpa angka. (3) Legenda: lantai berskematik memakai legenda baru (Pilih ditambah Bed terpilih; Master ditambah Ruang belum aktif); lantai gambar lama memakai legenda lama. (4) Kartu ringkas tanpa identitas pasien dan dapat dibuka lewat fokus. (5) Kerangka berbentuk kartu, kompas lebih kecil, animasi masuk mati bila `prefers-reduced-motion`. (6) Halaman 390 px tidak bergeser horizontal |
| Larangan | Filter kelas, pilihan "hanya kamar tersedia", dan pencarian nomor kamar tidak dibuat (`RWI-DEC-348`); tidak menambah panggilan API |
| Dependency | `FE-RWI-273` |
| Acceptance | (a) Ringkasan Lantai 3 pada data uji berjumlah sama dengan total bed lantai. (b) Tab Lantai 3 menampilkan `availableBedCount` dari server. (c) Tab Master tanpa angka. (d) Gerak dikurangi → tanpa animasi. (e) 390 px tanpa geser horizontal. (f) Satu panggilan API per lantai |
| Verifikasi wajib | `V-FE`; jejak jaringan per lantai; emulasi gerak dikurangi; lebar 390 px |
| Risiko / owner | Tim frontend Rawat Inap. Status bed yang tidak dikenal → dihitung tidak tersedia supaya jumlah tetap cocok |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-274.md` |

### 🟡 `FE-RWI-275` — Satu palet titik status di kartu dan pop up

| Field | Isi |
|---|---|
| Status | 🟡 **SEBAGIAN 20 Oktober 2026.** Ketiga acceptance criteria (a)–(c) terpetakan ke source: token palet `--fp-bed-*` didefinisikan sekali di `floor-plan.module.css` (hijau Tersedia, ungu Dipesan, oranye `#ea580c` terisi HD, biru terisi LD, abu-biru netral terisi belum dinilai/Housekeeping, kuning Menunggu Pembersihan, sian Dalam Pembersihan, abu berongga Tidak tersedia); titik kartu, ringkasan, titik dan tepi baris bed pop up, serta tag ketergantungan memakai token yang sama; isi, teks, tata letak, dan aksi pop up tidak berubah. Uji `floor-plan-status-palette.test.mjs` 5 uji baru; seluruh uji denah 133/133 lulus. `npm run lint` `0 errors` — 1083 warning, sama dengan garis dasar. `npm run test:unit` 3284/3321 lulus; 35 kegagalan lama dan 2 kegagalan dari pekerjaan paralel resep/tindakan, tidak satu pun di uji denah. `npm run build` `✓ Compiled successfully in 69s`, 514 halaman statis, exit 0. Uji manual peramban (kartu dan pop up 329 dan 325 di Monitoring dan Pilih Bed, akun Housekeeping) **belum dilakukan** — dijalankan pengguna sendiri atas keputusan pengguna 20 Oktober 2026 — sehingga task ditandai kuning. Bukti: [laporan](../task/report/frontend/FE-RWI-275.md) |
| Outcome | Titik status bed di kartu dan di baris bed pop up memakai satu palet: hijau hanya Tersedia, ungu hanya Dipesan, oranye hanya terisi High Dependency; status lain memakai warna yang tidak bertabrakan |
| Trace | FR-DNH-025 (`EPIC DNH-08`); RWI-DEC-353; RWI-AC-508; DNH-AT-508; UAT-819, UAT-820 |
| Scope source | Diperbarui: `floor-plan.module.css` (token palet bersama; warna `.bedStatusDot` pop up pada `HEAD` baris 1908–1934), `floor-plan-room.jsx` dan `floor-plan-bed-status.js` bila titik kartu perlu menunjuk token yang sama |
| Cakupan | Token palet status bed didefinisikan sekali; titik kartu dan titik pop up memakainya. Menunggu Pembersihan tidak oranye; terisi belum dinilai dan profil Housekeeping memakai warna "terisi" netral. Pilihan warna di luar tiga warna kunci adalah `DEV_DISCRETION` |
| Larangan | Isi, teks, tata letak, dan aksi pop up tidak berubah; warna kunci tidak dipakai untuk status lain |
| Dependency | `FE-RWI-273` |
| Acceptance | (a) 329-A terisi LD, 329-B dipesan, 329-C tersedia berwarna sama di kartu dan pop up; hijau hanya 329-C, ungu hanya 329-B. (b) 325-A Menunggu Pembersihan tidak oranye. (c) Akun Housekeeping: 329-A memakai warna "terisi" netral di kartu dan pop up |
| Verifikasi wajib | `V-FE`; bandingkan kartu dan pop up kamar 329 dan 325 di Monitoring dan Pilih Bed |
| Risiko / owner | Tim frontend Rawat Inap. Pop up dipakai keempat pemakai, sehingga perubahan warna terlihat di semuanya — sesuai `RWI-DEC-353` |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-275.md` |

### ⛔ `FE-RWI-276` — Peta bangsal Lantai 2

| Field | Isi |
|---|---|
| Status | ⛔ Menunggu gerbang `DEC-PETA-L3` (pengesahan atasan atas contoh Lantai 3; dicatat Muhammad Hamzah) |
| Outcome | Lantai 2 di keempat pemakai tampil sebagai peta bangsal: enam kamar Perinatologi dan ICU menjadi kartu, dan unit lain menjadi blok zona bernama |
| Trace | FR-DNH-017, 018 untuk Lantai 2; RWI-DEC-344; RWI-AC-498, 499 untuk Lantai 2; DNH-AT-498, 499, 918; RWI-FE-010 |
| Scope source | Baru: `schematic/lantai-2.schematic.js`. Diperbarui: `floors.js` |
| Cakupan | Zona OK dan OK 1–3, Hemodialisa, Persiapan Endoscopy, Cath Lab, CSSD, Farmasi, Kafetaria, Ruang Tunggu, dan Taman (blok polos); penanda Nurse Station, R. NS Bayi, lift, tangga. Kartu ringkas untuk BAYI-1, BAYI-2, dan ICU-CK. Skrip geometri untuk enam kamar |
| Larangan | Tidak menggambar detail ruang unit lain; label teknis tidak ditampilkan |
| Dependency | `{DEC-PETA-L3}` |
| Acceptance | DNH-AT-498, 499, dan 918 lulus untuk Lantai 2; keenam kamar dapat dibuka di keempat pemakai |
| Verifikasi wajib | `V-FE`; bandingkan dengan `denah-map-ruangan-rs-mmc/lantai-2.png` |
| Risiko / owner | Tim frontend Rawat Inap. Lantai 2 didominasi unit non-rawat-inap; zona dibuat sederhana |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-276.md` |

### ⛔ `FE-RWI-277` — Peta bangsal Lantai 4 dan Lantai 5

| Field | Isi |
|---|---|
| Status | ⛔ Menunggu gerbang `DEC-PETA-L3` |
| Outcome | Lantai 4 (27 kamar) dan Lantai 5 (11 kamar) di keempat pemakai tampil sebagai peta bangsal |
| Trace | FR-DNH-017, 018 untuk Lantai 4–5; RWI-DEC-344; RWI-AC-498, 499; DNH-AT-498, 499, 918; RWI-FE-010 |
| Scope source | Baru: `schematic/lantai-4.schematic.js`, `schematic/lantai-5.schematic.js`. Diperbarui: `floors.js` |
| Cakupan | Lantai 4: koridor y 679,5–747, NS A dan NS B, blok "Ruang staf" (meeting, print, IT, store), kamar berbentuk L 401 dan 418. Lantai 5: koridor y 643,5–706, blok "Rawat jalan & administrasi" (Poli 22/23/24/26, R. Medik, Dep. MD, KPR), penanda NS, pantry, toilet; kamar L 506; kartu ringkas 520–522 |
| Larangan | Sama dengan `FE-RWI-276` |
| Dependency | `{DEC-PETA-L3}` |
| Acceptance | DNH-AT-498, 499, dan 918 lulus untuk Lantai 4 dan 5 |
| Verifikasi wajib | `V-FE`; bandingkan dengan `lantai-4.png` dan `lantai-5.png` |
| Risiko / owner | Tim frontend Rawat Inap. Kamar 520–522 kecil dan berdempetan |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-277.md` |

### ⛔ `FE-RWI-278` — Peta bangsal Lantai 8 dan Lantai 9

| Field | Isi |
|---|---|
| Status | ⛔ Menunggu gerbang `DEC-PETA-L3` |
| Outcome | Lantai 8 (13 kamar) dan Lantai 9 (20 kamar) di keempat pemakai tampil sebagai peta bangsal |
| Trace | FR-DNH-017, 018 untuk Lantai 8–9; RWI-DEC-344; RWI-AC-498, 499; DNH-AT-498, 499, 918; RWI-FE-010 |
| Scope source | Baru: `schematic/lantai-8.schematic.js`, `schematic/lantai-9.schematic.js`. Diperbarui: `floors.js` |
| Cakupan | Lantai 8: koridor y 472–616,5, VVIP 801–810, Luxury 811, Suite 812–813, penanda lobby lift service dan tangga. Lantai 9: koridor y 472–614,5, kolom barat 918–920 dan timur 908–909 (bentuk L), penanda Day Room, Lobby Room, R. Doctor |
| Larangan | Sama dengan `FE-RWI-276` |
| Dependency | `{DEC-PETA-L3}` |
| Acceptance | DNH-AT-498, 499, dan 918 lulus untuk Lantai 8 dan 9 |
| Verifikasi wajib | `V-FE`; bandingkan dengan `lantai-8.png` dan `lantai-9.png` |
| Risiko / owner | Tim frontend Rawat Inap. Koridor Lantai 8 dan 9 lebar; penanda harus tetap terbaca |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-278.md` |

### ⛔ `FE-RWI-279` — Penutupan: semua lantai berskematik

| Field | Isi |
|---|---|
| Status | ⛔ Menunggu `FE-RWI-276`, `FE-RWI-277`, dan `FE-RWI-278` |
| Outcome | Keenam lantai di keempat pemakai memakai peta bangsal. `FloorPlanDrawing` dan renderer kamar lama tidak dipanggil lagi, gaya CSS CAD yang tidak terpakai dihapus, dan bukti kesiapan rilis lengkap |
| Trace | FR-DNH-023, FR-DNH-024 (penutupan); RWI-DEC-345, 351; RWI-AC-498, 499, 500, 506 untuk semua lantai; DNH-AT-498–500, 506, 920, 922; NFR-808, 812; RWI-FE-012 |
| Scope source | Diperbarui: `floor-plan-view.jsx`, `floor-plan-room.jsx`, `floor-plan-monitoring.jsx`, `floor-plan-selection.jsx`, `master-data-bed-floor-plan-view.jsx`, `floor-plan.module.css`. Disimpan tanpa dipanggil: `floor-plan-drawing.jsx`, `layers`/`labels` di `lantai-N.plan.js` |
| Cakupan | Hapus jalur gambar lama dan renderer kamar lama dari pemakaian; hapus gaya CAD yang tidak terpakai; verifikasi 6 lantai × Monitoring, Pilih Bed, transfer, dan Master; daftar periksa kesiapan rilis `RWI-DEC-351` di laporan |
| Larangan | Tidak menghapus `lantai-N.plan.js` maupun `rooms[]`; tidak melakukan deploy — rilis dijalankan pengguna |
| Dependency | `FE-RWI-276`, `FE-RWI-277`, `FE-RWI-278` |
| Acceptance | DNH-AT-498, 499, 500, 506, 920, dan 922 lulus untuk keenam lantai; tidak ada pemakai yang masih memanggil gambar lama |
| Verifikasi wajib | `V-FE`; screenshot 6 lantai × 4 pemakai; pencarian source membuktikan `FloorPlanDrawing` tidak dirender |
| Risiko / owner | Tim frontend Rawat Inap. Penghapusan jalur lama dapat memutus pemakai yang terlupa → cari semua pemakai `drawing` sebelum menghapus |
| DoD | `D-FE`; laporan `task/report/frontend/FE-RWI-279.md` |
