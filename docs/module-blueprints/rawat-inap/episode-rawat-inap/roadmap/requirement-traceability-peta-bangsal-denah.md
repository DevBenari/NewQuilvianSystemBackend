# Requirement Traceability — Peta Bangsal Denah

| Metadata | Nilai |
|---|---|
| planning_id / revision | `PETA-DEL-20261020-01` / 1 |
| contract / approval | `0.18.0` / `RWI-DEC-352` (desain), `RWI-DEC-353` (palet titik) / `APPROVED_DESIGN_PLANNED` |
| roadmap | [Frontend](frontend-roadmap-peta-bangsal-denah.md) · [Backend](backend-roadmap-peta-bangsal-denah.md) (nol task) |
| sumber | [Frontend 21](../03-frontend-architecture.md), [PRD 32](../04-prd-to-mvp.md), [Acceptance 26](../testing/acceptance-test-matrix.md), [decision log](../../00-interview-decisions.md) revision 62–63 |
| source snapshot | Backend `6e0f2b1b58a2192a0f54f0fdff718bc77670e3bd`; frontend `a491b05afcb6b3ed70218ccaa557007719d45ac3` |
| hash lock | [peta-bangsal-denah-delivery-lock.json](peta-bangsal-denah-delivery-lock.json) |
| coverage verdict | **ZERO COVERAGE GAP** — 9 FR, 11 AC, 6 NFR, 8 UAT, dan 21 butir uji terpetakan ke task. `DEC-PETA-L3` adalah gerbang proses, bukan requirement tanpa task |

## Register Bukti Pelaksanaan

| Task | Status bukti | Laporan |
|---|---|---|
| `FE-RWI-272` | 🟡 sebagian, 20 Oktober 2026 — `FR-DNH-017`/`024` untuk Lantai 3 terpetakan ke source; `DNH-AT-918` skrip geometri 11/11 lulus; `npm run lint` 0 error; `npm run build` lulus (diulang sesudah kunci `EBUSY` lepas); dikerjakan ulang 20 Oktober 2026 atas revisi pemilik (Area Servis Lantai 3 dihapus), skrip geometri 12/12 dan build ulang lulus; uji manual oleh pengguna | [FE-RWI-272](../task/report/frontend/FE-RWI-272.md) |
| `FE-RWI-273` | 🟡 sebagian, 20 Oktober 2026 — `FR-DNH-018`–`021` dan `023` (keyboard) untuk Lantai 3 terpetakan ke source; aturan rupa, HD/LD, fokus, dan `aria-label` (`DNH-AT-914`) diuji `floor-plan-room-card.test.mjs`; `npm run lint` 0 error; `npm run build` lulus; uji manual oleh pengguna | [FE-RWI-273](../task/report/frontend/FE-RWI-273.md) |
| `FE-RWI-274` | 🟡 sebagian, 20 Oktober 2026 — `FR-DNH-022` terpetakan ke source (ringkasan, angka tab, legenda, kartu ringkas, kerangka, kompas, gerak); `DNH-AT-505`/`919`/`920`/`922` didukung uji `floor-plan-summary.test.mjs` dan pratinjau 390 px; `npm run lint` 0 error; `npm run build` lulus; uji manual oleh pengguna | [FE-RWI-274](../task/report/frontend/FE-RWI-274.md) |
| `FE-RWI-275` | 🟡 sebagian, 20 Oktober 2026 — `FR-DNH-025` terpetakan ke source (satu palet token untuk kartu, ringkasan, dan pop up); `DNH-AT-508` didukung uji `floor-plan-status-palette.test.mjs`; `npm run lint` 0 error; `npm run build` lulus; uji manual (`UAT-819`, `UAT-820`) oleh pengguna | [FE-RWI-275](../task/report/frontend/FE-RWI-275.md) |
| `FE-RWI-276` | 🟡 sebagian, 21 Oktober 2026 — `FR-DNH-017`/`018` untuk Lantai 2 terpetakan ke source; sayap utara ditampilkan sebagai blok bernama atas pilihan pemilik; `DNH-AT-918` skrip geometri lulus (40/40), kartu ringkas BAYI-1, BAYI-2, ICU VIP 2, ICU-CK muat dengan skala terkecil 0,767; `npm run lint:errors` exit 0; `npm run build` lulus; tersisa pencatatan `DEC-PETA-L3` di decision log. Riwayat: sempat ⛔ dilewati atas instruksi pemilik, dibuka kembali pada hari yang sama | [FE-RWI-276](../task/report/frontend/FE-RWI-276.md) |
| `FE-RWI-277` | 🟡 sebagian, 21 Oktober 2026 — `FR-DNH-017`/`018` untuk Lantai 4–5 terpetakan ke source; `DNH-AT-918` skrip geometri lulus (35/35 bersama uji Lantai 3), varian ringkas tepat 520–522; `npm run lint:errors` exit 0; `npm run build` lulus; tanpa area servis utara atas revisi pemilik 21 Oktober 2026; tersisa pencatatan `DEC-PETA-L3` di decision log | [FE-RWI-277](../task/report/frontend/FE-RWI-277.md) |
| `FE-RWI-278` | 🟡 sebagian, 21 Oktober 2026 — `FR-DNH-017`/`018` untuk Lantai 8–9 terpetakan ke source; `DNH-AT-918` skrip geometri lulus, kartu lebar-pendek 812, 908, 918, 919 menaruh titik bed dan badge sebaris; `npm run lint:errors` exit 0; `npm run build` lulus; tersisa pencatatan `DEC-PETA-L3` di decision log | [FE-RWI-278](../task/report/frontend/FE-RWI-278.md) |
| `FE-RWI-279` | ⛔ menunggu `FE-RWI-276` s.d. `FE-RWI-278` | — |

## Matriks Keputusan

| Keputusan | Isi singkat | Task |
|---|---|---|
| `RWI-DEC-343` | Rupa boleh diubah; kamar, kunci, data, pop up, dan alur tetap | Penjaga semua task (larangan pada kartu) |
| `RWI-DEC-344` | Peta bangsal | `FE-RWI-272`, `FE-RWI-273`, `FE-RWI-276`–`278` |
| `RWI-DEC-345` | Satu peta di empat pemakai, tanpa CAD | `FE-RWI-272`, `FE-RWI-279` |
| `RWI-DEC-346` | Rupa status kembali ke `RWI-DEC-303`/`316` | `FE-RWI-273` |
| `RWI-DEC-347` | Titik bed | `FE-RWI-273` |
| `RWI-DEC-348` | Ringkasan dan angka tab; filter dan pencarian ditunda | `FE-RWI-274` |
| `RWI-DEC-349` | Contoh Lantai 3 dulu | Gerbang `DEC-PETA-L3`; `FE-RWI-276`–`278` |
| `RWI-DEC-350` | Kamar penuh tanpa penilaian tanpa label | `FE-RWI-273` |
| `RWI-DEC-351` | Tampilan campuran tidak dirilis | `FE-RWI-272` (cadangan gambar lama), `FE-RWI-279` |
| `RWI-DEC-353` | Satu palet titik | `FE-RWI-275` |
| `RWI-FE-009` / `010` / `011` / `012` | Warna kelas / lapisan peta / kartu ringkas dan gerak / berkas CAD | `FE-RWI-273` / `FE-RWI-272`, `276`–`278` / `FE-RWI-274` / `FE-RWI-279` |

## Matriks Functional Requirement

| FR | Ringkasan | Task backend | Task frontend | Bukti utama |
|---|---|---|---|---|
| `FR-DNH-017` | Peta tanpa gambar kerja | — | `FE-RWI-272` (Lantai 3), `FE-RWI-276`–`278`, `FE-RWI-279` | DNH-AT-498 |
| `FR-DNH-018` | Kartu kamar di posisi dan bentuk asli | — | `FE-RWI-272` (geometri), `FE-RWI-273`, `FE-RWI-276`–`278` | DNH-AT-499, 918 |
| `FR-DNH-019` | Rupa status kamar | — | `FE-RWI-273` | DNH-AT-501, 502 |
| `FR-DNH-020` | Label HD/LD dari server | — | `FE-RWI-273` | DNH-AT-503, 915 |
| `FR-DNH-021` | Titik bed | — | `FE-RWI-273` | DNH-AT-504 |
| `FR-DNH-022` | Ringkasan lantai dan angka tab | — | `FE-RWI-274` | DNH-AT-505 |
| `FR-DNH-023` | Satu peta di empat pemakai; keyboard | — | `FE-RWI-272`, `FE-RWI-273`, `FE-RWI-279` | DNH-AT-500, 923 |
| `FR-DNH-024` | Contoh Lantai 3 dan cadangan gambar lama | — | `FE-RWI-272`, `FE-RWI-279` | DNH-AT-507, 917 |
| `FR-DNH-025` | Satu palet titik di kartu dan pop up | — | `FE-RWI-275` | DNH-AT-508 |

## Matriks Acceptance Criteria

| Acceptance | Task pemilik | Bukti |
|---|---|---|
| `RWI-AC-498` tanpa unsur CAD | `FE-RWI-272`, `FE-RWI-276`–`279` | DNH-AT-498 |
| `RWI-AC-499` kartu di poligon asli, bentuk L, kelas dan nomor dari API | `FE-RWI-272`, `FE-RWI-273`, `FE-RWI-276`–`279` | DNH-AT-499, 918 |
| `RWI-AC-500` satu peta di empat pemakai, ruang cadangan hanya di Master | `FE-RWI-272`, `FE-RWI-273`, `FE-RWI-279` | DNH-AT-500, 921 |
| `RWI-AC-501` tepi hijau dan kelayakan pasien | `FE-RWI-273` | DNH-AT-501; UAT-815 |
| `RWI-AC-502` pesanan ungu, penuh abu tetap dapat diklik | `FE-RWI-273` | DNH-AT-502; UAT-816 |
| `RWI-AC-503` label HD/LD | `FE-RWI-273` | DNH-AT-503 |
| `RWI-AC-504` isi kartu, titik, privasi | `FE-RWI-273` | DNH-AT-504, 915 |
| `RWI-AC-505` ringkasan dan tab | `FE-RWI-274` | DNH-AT-505 |
| `RWI-AC-506` pop up dan batas perubahan | Semua task; diperiksa akhir di `FE-RWI-279` | DNH-AT-506 |
| `RWI-AC-507` contoh Lantai 3 dulu | `FE-RWI-272`–`275`, gerbang `DEC-PETA-L3`, `FE-RWI-276`–`278` | DNH-AT-507 |
| `RWI-AC-508` satu palet titik | `FE-RWI-275` | DNH-AT-508; UAT-819, 820 |

## Matriks Kebutuhan Non-Fungsional

| NFR | Task | Bukti |
|---|---|---|
| `NFR-808` satu panggilan API per lantai | `FE-RWI-272`, `FE-RWI-274`, `FE-RWI-279` | DNH-AT-922 |
| `NFR-809` nomor kamar terbaca, kontras ≥ 4,5 : 1 | `FE-RWI-273` | Screenshot; DNH-AT-499 |
| `NFR-810` status tidak hanya warna | `FE-RWI-273` | DNH-AT-914 |
| `NFR-811` gerak dikurangi | `FE-RWI-274` | DNH-AT-919 |
| `NFR-812` 390 px tanpa geser horizontal | `FE-RWI-274`, `FE-RWI-279` | DNH-AT-920 |
| `NFR-813` tanpa pustaka baru | Semua task | `package.json` tidak berubah pada laporan |

## Matriks UAT

| UAT | Jalur | Task |
|---|---|---|
| `UAT-813` | Berhasil — Monitoring Lantai 3 | `FE-RWI-272`, `FE-RWI-273`, `FE-RWI-274` |
| `UAT-814` | Gagal — Housekeeping, jaringan putus | `FE-RWI-273`, `FE-RWI-274` |
| `UAT-815` | Berhasil — Pilih Bed pasien isolasi | `FE-RWI-273` |
| `UAT-816` | Gagal — dua petugas berebut bed | `FE-RWI-273` |
| `UAT-817` | Berhasil — Master Bed dengan keyboard | `FE-RWI-272`, `FE-RWI-273` |
| `UAT-818` | Gagal — lantai tanpa data skematik pada masa contoh | `FE-RWI-272`, `FE-RWI-279` |
| `UAT-819` | Berhasil — satu palet titik | `FE-RWI-275` |
| `UAT-820` | Gagal — Housekeeping tanpa tingkat ketergantungan | `FE-RWI-275` |

## Matriks Butir Uji Desain

| Butir uji | Task |
|---|---|
| `DNH-AT-914` status tidak hanya warna | `FE-RWI-273` |
| `DNH-AT-915` privasi Housekeeping | `FE-RWI-273` |
| `DNH-AT-916` bentuk tanpa data API | `FE-RWI-273` |
| `DNH-AT-917` masa contoh | `FE-RWI-272` |
| `DNH-AT-918` skrip geometri | `FE-RWI-272`, `FE-RWI-276`–`278` |
| `DNH-AT-919` gerak dikurangi | `FE-RWI-274` |
| `DNH-AT-920` layar sempit | `FE-RWI-274`, `FE-RWI-279` |
| `DNH-AT-921` mode Master | `FE-RWI-273` |
| `DNH-AT-922` beban jaringan | `FE-RWI-272`, `FE-RWI-274`, `FE-RWI-279` |
| `DNH-AT-923` keyboard | `FE-RWI-273` |

## Gerbang dan Batas

| Butir | Sifat | Dampak |
|---|---|---|
| `DEC-PETA-L3` | Gerbang proses: atasan mengesahkan contoh Lantai 3; Muhammad Hamzah mencatatnya sebagai keputusan baru | 21 Oktober 2026: pemilik menyatakan Lantai 3 disetujui dan memerintahkan `FE-RWI-276`, `FE-RWI-277`, dan `FE-RWI-278` dikerjakan (ketiganya 🟡; `FE-RWI-276` sempat dilewati lalu dibuka kembali pada hari yang sama); gerbang tetap terbuka sampai pengesahan dicatat sebagai keputusan baru di decision log. `FE-RWI-279` menunggu `FE-RWI-276` s.d. `FE-RWI-278`. Riwayat: sebelumnya `FE-RWI-276` s.d. `FE-RWI-279` belum boleh dimulai |
| `DNH-G04` | Gerbang produksi lama: kriteria klinis HD/LD dan persetujuan privasi | Penayangan HD/LD di produksi; bukan dependency coding |
| `npm run build` | Boleh `NOT RUN` atas keputusan pemilik 10 September 2026 | Ditulis apa adanya di laporan; bukan gap |
| Backend | Nol task (`RWI-DEC-343`) | Tidak ada pasangan task backend; bukan gap |
