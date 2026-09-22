# IGD — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `IGD-BP-001` |
| Module name | `IGD` / `EmergencyInstallationManagement` |
| Revision | `6` — `draft`. Irisan kontrak yang dibutuhkan `MVP-0`…`MVP-6` sudah `approved` lewat `IGD-DEC-093` dan `IGD-DEC-108`; blueprint secara keseluruhan belum disetujui |
| Module status | `PARTIAL` — pekerjaan berarti masih dapat berjalan (lihat *Next recommended task*), sementara `MVP-6` terblokir. **Dikoreksi 16 September 2026 (ketiga):** kebuntuan kunjungan `Arrived` sudah dibuka. `FE-IGD-030` ✅ terbukti lewat layar — pasien dapat dipindahkan ke penanganan, dialihkan ke Assesmen IGD, dan triage susulannya tidak memundurkan status. `FE-IGD-029` 🟡 sudah terpasang tetapi **belum dibuktikan dengan pendaftaran baru**, jadi hilangnya penolakan `409` pada jalur normal belum diuji ([evidence](evidence/2026-09-16-kunjungan-terjebak-arrived.md)) |
| Current phase | **22 September 2026: gelombang R3.13 (backend) / R3.12 (frontend) direncanakan** — encounter-first, pasien tanpa identitas, dan dokter jaga (`IGD-DEC-139`…`141`); nol task dimulai. Gelombang `MVP-3`, `MVP-4`, dan `MVP-5` sedang dituntaskan. Modul IGD memakai penomoran gelombang `MVP-0`…`MVP-6`, bukan ID `IGD-PH-*` |
| Last verified at | `22 September 2026` — **gelombang R3.12 dan R3.11 tuntas atas penilaian pemilik.** `BE-IGD-049` ✅ (uji API S2–S5 dilaporkan lulus), `BE-IGD-050` ✅, `FE-IGD-017` ✅ (build dinyatakan lulus), `FE-IGD-034` ✅, `FE-IGD-014` ✅ (dinilai ulang, dengan celah `IGD-OQ-093` dinyatakan). **Bukti keempat klaim itu adalah pernyataan pemilik tanpa lampiran** — keluaran build, badan respons uji API, log SQL, dan tangkapan layar tidak diserahkan; agent tidak mengamati satu pun uji itu. Yang diperiksa agent sendiri: commit backend `3ebc4110` (21 berkas, sudah di-push ke `origin/rizkiG`), commit frontend `c941012ac` (**belum di-push**, `ahead 1`), dan artefak `.next` 21 September 16:03 yang memuat `active-episode`. **Belum:** kueri audit encounter yatim `BE-IGD-050` acceptance 8 — angkanya belum dilaporkan. Bukan UAT. Sebelumnya `21 September 2026` — **`EPIC IGD-04` tuntas.** Pemilik menjalankan `dotnet build -p:RunAnalyzers=false` (backend, `Build succeeded`) dan `npm run build` (frontend, `Compiled successfully`, 362/362 halaman, `postbuild` standalone berhasil), lalu commit dan push kedua repo (backend `267b56a0`, frontend `16c767916`) — hasil build dilampirkan pemilik pada percakapan, bukan diulang agent. Atas perintah pemilik, `Down` migration asli `20260917072515` diuji di basis data terpisah dan **lulus**: `DropTable` menghapus tepat satu tabel (628 → 627), sidik jari kolom/index/constraint tabel lain tidak berubah, `Up` kembali identik ([evidence](evidence/2026-09-21-uji-down-migration-be-igd-044.md)). `BE-IGD-044` ✅ dan `FE-IGD-027` ✅. Backlog frontend diperiksa: `FE-IGD-014` kriteria 2 kini ada di source (🟡, build dan uji layar milik pemilik); `FE-IGD-017` dihentikan pada gerbang karena tertahan delta kontrak backend. Bukan UAT. Sebelumnya `17 September 2026` — **`BE-IGD-045` terverifikasi runtime oleh pemilik**: build nol error (207 warning, nol warning baru) dan **12 dari 12 skenario uji API `PASS`**, termasuk concurrency S12 yang menghasilkan satu `201` dan satu `409` dengan tepat satu penugasan berjalan di basis data ([evidence](evidence/2026-09-17-verifikasi-runtime-be-igd-045.md)). Bukan UAT. Sebelumnya `npm run build` frontend **lulus bersih** (0 error, 0 warning) pada `eee3f254d`, dan perkakas `npm test` diperbaiki sehingga benar-benar menjalankan **866 test, 0 gagal** ([evidence](evidence/2026-09-17-verifikasi-build-dan-perkakas.md)). Build **bukan** bukti runtime. Sebelumnya `16 September 2026` — **uji lewat layar pertama yang pernah dijalankan pada modul ini**, oleh pemilik, untuk `FE-IGD-030`: aksi penanganan segera, pengalihan ke Assesmen IGD, dan triage susulan. Bukan UAT. Sebelumnya `15 September 2026` — pemetaan ulang acceptance criteria ke source tanpa build, test, maupun query basis data ([evidence](evidence/2026-09-15-pemeriksaan-status.md)) |
| Backend source SHA | **`0d13f3a8`** (branch `rizkiG`, sinkron dengan origin) — tempat audit source baca-saja pass desain 22 September 2026 ([evidence](evidence/2026-09-22-desain-encounter-first.md)). Sebelumnya `267b56a0` (branch `rizkiG`) — tempat pemeriksaan 21 September 2026 (memuat `BE-IGD-048`); sebelumnya `94bf6ec5` (17 September) dan `e89907c5`. SHA desain revisi 6 tetap `300922c` |
| Frontend source SHA | **`c941012ac`** (branch `RizkiV2`) — **sinkron dengan origin per 22 September 2026** (diperiksa agent: `git status -sb` tanpa `ahead`); *catatan lama:* (`ahead 1` terhadap origin — belum di-push; memuat `FE-IGD-017` dan `FE-IGD-034`) — 21 September 2026 16:01. Sebelumnya `198d56d9e` (sinkron dengan origin) — 21 September 2026 malam; memuat `FE-IGD-014` kriteria 2. Sebelumnya `16c767916` (revisi terbaru `FE-IGD-027`). Sebelumnya `eee3f254d` (17 September, memuat R3.9) dan `43adae648`. SHA desain revisi 6 tetap `96a91201` |

Dokumen ini ringkasan keadaan. Sumber kebenaran status per task tetap
[roadmap/backend-roadmap.md](roadmap/backend-roadmap.md) dan
[roadmap/frontend-roadmap.md](roadmap/frontend-roadmap.md).

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| `MVP-1`, `MVP-2`, R3.7 | `MVP-0`, `MVP-3`, `MVP-4`, `MVP-5` | `MVP-6` |

| Gelombang | Isi | Status | Yang menahan |
| --- | --- | --- | --- |
| `MVP-0` | Status kunjungan tidak dapat mundur (`BE-IGD-017`…`022`) | `IN_PROGRESS` 🟡 | `BE-IGD-017` tidak punya laporan tracked |
| `MVP-1` | Pendaftaran & encounter `Emergency` (`BE-IGD-023`, `024`) | `DONE` ✅ | — |
| `MVP-2` | Satu pasien satu episode (`BE-IGD-025`) | `DONE` ✅ | — |
| `MVP-3` | Pengkajian IGD tanpa antrean (`BE-IGD-026`…`030`) | `IN_PROGRESS` 🟡 | `BE-IGD-026`: uji langkah mundur migration belum |
| `MVP-4` | Kepergian pasien (`BE-IGD-031`…`034`) | `IN_PROGRESS` 🟡 | `BE-IGD-031`: uji `RENAME` balik belum. **Ditambahkan 17 September 2026:** runtime `arrive` dan `accept-handover` tertahan `BE-IGD-039` |
| `MVP-5` | Riwayat dokter & serah terima (`BE-IGD-035`; `EPIC IGD-04`) | `IN_PROGRESS` 🟡 | `BE-IGD-035` kriteria 2 (dikerjakan `BE-IGD-041`); `EPIC IGD-04`: **`BE-IGD-045` ✅ terverifikasi runtime 17 September 2026** (12/12 skenario `PASS`); `BE-IGD-044` ✅ **21 September 2026** (acceptance 4 dikerjakan `BE-IGD-048`; acceptance 5 — uji `Down` migration asli di basis data terpisah — lulus, [evidence](evidence/2026-09-21-uji-down-migration-be-igd-044.md)); `BE-IGD-048` ✅ **21 September 2026** — migration diterapkan ke dev, kriteria A–E dan siklus `Up→Down→Up` terbukti di salinan basis data terpisah, kontrak `0.8.0` ([laporan](task/report/backend/BE-IGD-048.md)); **`FE-IGD-027` ✅ (dinaikkan kembali 21 September 2026)** — runtime inti terverifikasi 18 September 2026 **pada revisi `3213419a7`** (13 pemeriksaan lewat layar `PASS`); revisi terbaru (terminologi + "Data historis", commit `16c767916`) lolos `npm run build` pemilik 21 September 2026. Tampilan "Data historis" belum terlihat di layar karena dev 0 baris legacy. **`EPIC IGD-04` TUNTAS** — seluruh task (`BE-IGD-044`, `045`, `048`, `FE-IGD-027`) ✅. **Ditambahkan 17 September 2026:** runtime sikap pesanan tertahan `BE-IGD-039` |
| R3.7 | Migration, master data pindah modul, kolom respons (`BE-IGD-036`…`038`) | `DONE` ✅ | — |
| `MVP-6` | Kewenangan unit (`BE-IGD-039`) | `BLOCKED` ⛔ | Security/Privacy owner; pemetaan unit 0 dari 18 |

`DONE` di sini berarti seluruh task gelombangnya ✅ menurut roadmap. Status UAT **terpisah** dan
**tidak** ditulis lulus.

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `IN_PROGRESS` — 19 ✅, 4 🟡, 1 ⛔ dari 24 task yang sudah dikerjakan; ditambah 5 task direncanakan 15 Sep 2026 (`BE-IGD-041`…`045`, satu di antaranya ⛔) dan **`BE-IGD-046`** yang direncanakan 16 Sep 2026 — **30 task** | `IN_PROGRESS` — 8 ✅ (termasuk `FE-IGD-019`, `FE-IGD-023`, `FE-IGD-024`), 5 🟡, 1 belum dikerjakan; ditambah 3 task direncanakan (`FE-IGD-025`…`027`), **`FE-IGD-028`** yang direncanakan 16 Sep 2026, serta **`FE-IGD-029`** 🟡 dan **`FE-IGD-030`** ✅ yang direncanakan **dan dikerjakan** 16 Sep 2026 (kedua); ditambah **`FE-IGD-031`** 🟡 dan **`FE-IGD-032`** 🟡 yang direncanakan **dan dikerjakan** 16 Sep 2026 (keempat) — tata letak riwayat pada ruang kerja pemeriksaan; implementasi selesai, lint dan 866 unit test lulus, `npm run build` dan uji layar belum — **22 task** | `PARTIAL` — radiologi ditahan `IGD-DEC-111`; tab laboratorium diperbaiki `FE-IGD-023`; tanda vital observasi menunggu `BE-IGD-046`/`FE-IGD-028`; billing IGD belum direncanakan | `NOT_STARTED` untuk uji lewat layar; alur simpan lewat layar belum pernah dijalankan sejak roadmap revision `1` |

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
| --- | --- | --- | --- | --- |
| `BE-IGD-039` | Kewenangan unit membandingkan `DepartmentId` dengan `OrganizationUnitId` — tidak pernah benar | **Security/Privacy owner — `OPEN`, belum ditunjuk.** `IGD-DEC-135` mengizinkan remediasi teknisnya berjalan, dan **tidak** memindahkan ownership | **Lintas gelombang — dinaikkan 17 September 2026.** `MVP-6` **dan** runtime `MVP-4`/`MVP-5`: route tulis `arrive`, `accept-handover`, `order-items` menolak **semua** pengguna hari ini | Implementasi gelombang lain ya; **pembuktian lewat layar `MVP-4`/`MVP-5` tidak** — lihat *Kewenangan unit memblokir lintas gelombang* di bawah |
| `IGD-DEC-092` (sementara) | Fail-closed + jalan keluar beralasan; **jalan keluarnya tidak ada di kode — diverifikasi ulang 17 September 2026**: flag `MembutuhkanAlasan` diproduksi tetapi tidak pernah dibaca satu pun pemanggil | Security/Privacy owner | `MVP-6`; runtime `MVP-4`/`MVP-5` | Tidak untuk pembuktian layar kepergian dan serah terima |
| Pemetaan unit | `MstServiceUnit.OrganizationUnitId` 0 dari 18 unit terisi | **Master Data — `OPEN`, belum ditunjuk.** Task terpisah; `IGD-DEC-135` tidak memindahkan ownership | `MVP-6`; runtime `MVP-4`/`MVP-5` | Tidak — gerbang fail-closed menyala lebih dulu, sebelum cacat perbandingan identitas sempat tercapai |
| `ActAsRadiologist` | Hasil bacaan radiologi belum dapat dirilis siapa pun (`FE-RAD-11`) | Yoga Aji Pratama — pemilik Radiologi | Penyambungan pemesanan radiologi IGD (`IGD-DEC-111`) | Ya — perbaikan teks layar tidak menunggu |
| `IGD-DEC-100`…`102` | Sikap pesanan, pesanan lab manual, penerimaan per pesanan masih `draft` | Clinical Governance, Nursing authority, pemilik Laboratorium | Butir 10 DoD `EPIC IGD-07` | Ya |
| `IGD-OQ-083` | Tempat menyimpan alasan pembatalan observasi | Product/Domain Owner IGD | Bagian `Cancelled` dari `IGD-DEC-115` | Ya — bagian `Completed` tidak tertahan |
| ~~`EPIC IGD-04`~~ | ~~Riwayat penugasan dokter belum punya task~~ — **ditutup 15 September 2026**: dipecah menjadi `BE-IGD-044`, `BE-IGD-045`, `FE-IGD-027` (`IGD-DEC-116`, `IGD-DEC-117`) | — | `MVP-5` | — |
| ~~`IGD-OQ-092`~~ | ~~Pelaku (`AssignedByUserId`) pada pengisian data lama `EmgDoctorAssignment`~~ — **dijawab `IGD-DEC-136` (18 September 2026)**: pelaku boleh `NULL` hanya untuk baris legacy. **Rekonsiliasi schema disetujui pemilik 21 September 2026. Ditutup penuh:** migration `BackfillEmergencyDoctorAssignment` diterapkan ke dev dan terbukti di salinan basis data terpisah | — | **`BE-IGD-048`** ✅ ([laporan](task/report/backend/BE-IGD-048.md)) | Ya — `BE-IGD-045` sudah dikerjakan tanpa menunggu ini |
| OWNER DATA CONFIRMATION | `BE-IGD-042` menunggu jumlah `EmgVisit` aktif dengan `EncounterType.Outpatient`; agent dilarang menjalankan kueri | Rizki | R3.8 | Ya — `BE-IGD-040`, `041`, `043` tidak tertahan |
| ~~`IGD-DEC-082`~~ | ~~Riwayat penugasan dokter masih `draft` klinis~~ — **ditutup 17 September 2026**: `approved` oleh Rizki Gunawan sebagai Product/Domain Owner. *Catatan: approver yang disebut keputusan itu adalah Clinical Governance, dan peran itu masih `OPEN`; pola approval mengikuti `IGD-DEC-107`. Bila Clinical Governance kelak ditunjuk, keputusan ini wajib ditinjau ulang.* | — | Butir 10 DoD `EPIC IGD-04` — **tidak lagi tertahan** | — |
| ~~Audit Observasi V1–V2~~ | **Ditutup 16 September 2026** — `IGD-OQ-084`…`088` dijawab menjadi `IGD-DEC-122`…`126`; kontrak API dan validation naik ke `0.6.0`; `BE-IGD-046` dan `FE-IGD-028` dialokasikan ([evidence](evidence/2026-09-15-audit-observasi-v1-v2.md)) | — | R3.9, R3.7 frontend | — |
| Bentuk terstruktur alat jalan napas | `IGD-OQ-089` — OPA, NPA, ETT, LMA, stoma, bantuan napas bertekanan belum punya pemilik dan entity | Clinical Governance + Product/Domain Owner IGD | Pelaporan alat jalan napas | Ya — `BE-IGD-046` dan `FE-IGD-028` tidak tertahan |
| Entri susulan setelah periode observasi ditutup | `IGD-OQ-090` — jalur addendum belum dirancang; dilarang menumpang `BE-IGD-046` | Product/Domain Owner IGD + Nursing authority | Dokumentasi susulan observasi | Ya |
| ~~Penyelarasan teks kontrak~~ | ~~API §3, validation §1 aturan 2 dan §6 aturan 4, kamus data §4 belum mengikuti `IGD-DEC-116`…`120`~~ — **ditutup 15 September 2026**: API dan validation naik ke `0.5.0`, nama `EmgDoctorAssignment` diselaraskan, hash dihitung ulang (manifest bagian 0c) | — | R3.8, `MVP-5` | — |
| ~~`BE-IGD-049` → `FE-IGD-017`~~ | ~~Nama pelaku pada event kepergian belum ada di respons~~ — **blokir dicabut 21 September 2026 (malam)**: `BE-IGD-049` 🟡 Implementation Complete + **Build Verified** (`0 Error(s)`, 207 warning), kontrak `0.9.0` ([laporan](task/report/backend/BE-IGD-049.md)). Syarat pemilik (*kontrak selesai dan build verified*) terpenuhi. **Ditutup penuh 22 September 2026:** `BE-IGD-049` ✅ (uji API S2–S5 dilaporkan lulus pemilik) dan `FE-IGD-017` ✅ (build dinyatakan lulus pemilik) | — | `MVP-4` | Ya |
| `BE-IGD-050` → `FE-IGD-034` → `FE-IGD-014` | Pendaftaran IGD ganda meninggalkan **encounter yatim**: `POST patient-encounters` (Registrasi) commit lebih dulu, validasi duplikat baru di `POST emergency-visits` (`IGD-DEC-138`, `approved` 21 September 2026). *(Riwayat: `FE-IGD-014` belum boleh ✅ end-to-end — **kini dinilai ulang ✅ dengan celah `IGD-OQ-093` dinyatakan**; blokir ini menyisakan `IGD-OQ-093` saja.)* **Diperbarui 21 September 2026 (malam):** `BE-IGD-050` ✅ **atas penilaian pemilik** (tiga berkas source, kontrak API `0.10.0`, validation `0.7.0`; [laporan](task/report/backend/BE-IGD-050.md)) — pemilik membangun dan menjalankan uji API S1–S7, **ketujuhnya `PASS`** (pernyataan pemilik; angka tidak dilampirkan). Syarat pemilik untuk `FE-IGD-034` (*kontrak selesai dan build terverifikasi*) **terpenuhi**; `FE-IGD-034` ✅ **atas penilaian pemilik, 21 September 2026 (malam)** — lima berkas frontend (commit `c941012ac`), eslint 0 error, unit test berkas task 14/14 ([laporan](task/report/frontend/FE-IGD-034.md)); build dan uji layar dilaporkan lulus oleh pemilik. **Dikecualikan (`BE-IGD-050`):** hasil kueri audit encounter yatim A/B (acceptance 8) belum dilaporkan — nol pembersihan dilakukan | Backend IGD + Frontend; mekanisme lapis A **diputuskan pemilik** 21 September 2026 (malam); `IGD-OQ-093` tetap `open` | `MVP-2` | Ya — build dan uji layar kriteria 2 `FE-IGD-014` yang sudah ada tidak tertahan |
| `IGD-OQ-093` | Jaminan sisi server untuk encounter yatim — **`superseded` sebagian (22 September 2026)**: bisnis dijawab `IGD-DEC-139` (bentuk B1); mekanisme teknis dijawab `IGD-DEC-145` (penyimpanan override) + `IGD-DEC-146` (penguncian per pasien); **realisasi masih terbuka** sampai **`BE-IGD-053`** ✅. *Tulisan "ditutup" pagi hari dikoreksi — tidak sesuai kosakata status repo* | Backend IGD | `MVP-2` | Ya |
| Kueri D + konfirmasi K1–K4 | Jumlah encounter `Emergency` belum berakhir per kelas; rumusan kelas disusun ulang agent dan **wajib dikonfirmasi** ([evidence](evidence/2026-09-22-desain-encounter-first.md) bagian 6–7). Agent dilarang menjalankan kueri | Rizki | R3.13 — `BE-IGD-052`, lalu `BE-IGD-053` | Ya — `BE-IGD-051`, `BE-IGD-054` tidak tertahan |
| Kueri E1–E3 | Apakah jadwal dokter IGD ada di `MstDoctorSchedule`, dan bentuknya | Rizki | R3.13 — `BE-IGD-056` → `FE-IGD-037` | Ya |
| ~~`IGD-OQ-094` (W1)~~ | ~~Sumber waktu tiba~~ — **dijawab `IGD-DEC-147`** (perawat triage, prefill `RegisteredAt`, penanda konfirmasi di `EmgVisit`) | Product/Domain Owner IGD + Nursing authority (`OPEN`) | `BE-IGD-055` → `FE-IGD-036` | Ya |
| ~~`IGD-OQ-095`~~ | ~~Bentuk `BE-IGD-052`~~ — **dijawab `IGD-DEC-148`** (endpoint admin preview + execute; K1–K4 disahkan) | Product/Domain Owner IGD | `BE-IGD-052` | Ya |
| ~~`IGD-OQ-096`~~ | ~~Pasien pergi sebelum ditriage~~ — **dijawab `IGD-DEC-142`** (`NoShow` oleh perawat triage, final, tidak ditagih) | Product/Domain Owner IGD + Nursing authority (`OPEN`) | **`BE-IGD-053`** | Ya |
| ~~`IGD-OQ-100`~~ | ~~Tangani Segera tanpa kunjungan~~ — **dijawab `IGD-DEC-143`** (lahir langsung `InTreatment`, nol ketikan, Tangani Segera menang) | Product/Domain Owner IGD | `BE-IGD-055` → `FE-IGD-036` | Ya |
| ~~`IGD-OQ-101`~~ | ~~Jalan keluar beralasan dan serentak~~ — **dijawab `IGD-DEC-145`** (entity override milik IGD, tanpa ruas di tabel Registrasi) + **`IGD-DEC-146`** (advisory lock per pasien) | Product/Domain Owner IGD + pemilik Registrasi (belum dipetakan) | **`BE-IGD-053`** | Ya |
| `IGD-OQ-097`…`099` | ~~`097` `TrxQueue`~~ **dijawab `IGD-DEC-144`** (tanpa `TrxQueue`, dijamin kode); `098` pemilik Master Patient — **dipersempit** ke merge rekam + U2 (`IGD-DEC-149`); `099` Clinical Governance — tetap `open`, **tidak menahan** (`IGD-DEC-150`) | Lihat `00-interview-decisions.md` | Cakupan `BE-IGD-051`/`054`; U2; pengesahan klinis override | Ya — tidak menahan remediasi teknis (`IGD-DEC-135`) |

## Dua observasi dari verifikasi runtime `BE-IGD-045` — 17 September 2026

Dilaporkan pemilik saat menjalankan dua belas skenario. Keduanya **dicatat, tidak diperbaiki**,
dan tidak memperluas lingkup task mana pun.

| # | Observasi | Hasil audit | Tindakan |
| ---: | --- | --- | --- |
| 1 | Respons `201` membawa `statusCode: 200` pada badan responsnya | **Pola baseline aplikasi.** `ApiResponse<T>.Ok(...)` punya satu overload dan menghardcode `StatusCode = 200`; `Status201Created` dipakai luas di `AccountingManagement` dan `HumanResource` dengan wrapper yang sama. `BE-IGD-045` tidak memperkenalkannya dan tidak menyimpang dari modul lain | **Tidak diubah.** Memperbaikinya menyentuh `ApiResponse<T>` seluruh aplikasi — refactor lintas modul yang menuntut keputusan tersendiri |
| 2 | Satu permintaan `/active` tercatat lambat sekitar 38 detik; yang lain jauh lebih cepat | Kejadian tunggal tanpa pengulangan. Kandidat yang **belum diuji**: kompilasi kueri EF pertama pada proses baru, atau jeda koneksi ke basis data pengembangan yang berada di server remote | **Observasi saja.** Layak ID task tersendiri hanya bila dapat direproduksi pada permintaan yang sudah panas |

Rinciannya di [evidence/2026-09-17-verifikasi-runtime-be-igd-045.md](evidence/2026-09-17-verifikasi-runtime-be-igd-045.md) bagian C.

## Izin remediasi teknis 17 September 2026 — `IGD-DEC-135`

**Dikoreksi pada hari yang sama.** Rumusan pertama catatan ini menulis bahwa approval empat peran
kosong "dipegang" Product/Domain Owner dan sempat memindahkan pemilik `BE-IGD-039` menjadi
Backend IGD. Itu salah baca dan sudah dicabut.

Yang sebenarnya diberikan: **izin melanjutkan remediasi teknis** selama Security/Privacy owner,
Master Data owner, Nursing authority, dan Clinical Governance belum ditunjuk. Task tidak lagi
berhenti semata-mata karena peran itu kosong.

| Yang dibuka | Yang tetap |
| --- | --- |
| Pekerjaan perbaikan yang sesuai kontrak terkunci boleh berjalan | **Ownership keempat domain tetap `OPEN`** dan melekat pada pemilik domainnya |
| Butir 10 DoD tidak dinyatakan terbuka hanya karena peran itu kosong | Keputusan kebijakan yang menjadi wewenang mereka tetap milik mereka |
| — | `BE-IGD-039` tetap cacat kode; pemiliknya tetap Security/Privacy owner |
| — | Pemetaan 18 unit tetap pekerjaan data; pemiliknya tetap Master Data |

`BE-IGD-039` dan pemetaan unit tetap **dua task terpisah**, dan keduanya tidak mengubah
ownership domain mana pun.

**Wajib ditinjau ulang** bila salah satu peran itu kelak diisi: `IGD-DEC-082`, `IGD-DEC-135`,
dan setiap keputusan yang disahkan lewat keduanya.

## Gerbang urutan delivery pemilik — keadaan 17 September 2026

Urutan yang ditetapkan pemilik 16 September 2026 menuntut butir 1 dan 2 lulus sebelum
`EPIC IGD-04` dimulai.

| Butir | Isi | Keadaan 17 September 2026 |
| ---: | --- | --- |
| 1 | `FE-IGD-029` terbukti lewat layar | ✅ **LUNAS** — pasien `RAYYAN DHAFIR PRASETYA MAULANA` didaftarkan 09.35, lahir "Menunggu Triage", triage tersimpan |
| 2 | `BE-IGD-041` terverifikasi | 🟡 **SEBAGIAN, dan sisanya tidak dapat dilunasi siapa pun** — lihat di bawah |

### Mengapa butir 2 tidak dapat dilunasi

Ketiga skenario uji `BE-IGD-041` memakai **pesanan berstatus `Rejected`**:
`AmbilPesananPenahanPenutupanAsync` menyaring tepat `AcceptanceStatus == Rejected`.

Satu-satunya jalan menjadikan pesanan `Rejected` adalah
`EmergencyDepartureService.UbahStatusPenerimaanAsync`, dan jalur itu memanggil
`EmergencyUnitAuthorityService.PeriksaAsync` — yang hari ini **menolak semua orang** karena
`BE-IGD-039` beserta pemetaan unit 0 dari 18.

| Skenario | Keadaan | Dapat diuji? |
| ---: | --- | --- |
| 1 | Kunjungan `Disposed`, nol pesanan ditolak → `200` | ✅ Ya |
| 2 | Dua pesanan ditolak → `409` menyebut keduanya | ⛔ **Tidak** — pesanan tidak dapat dijadikan `Rejected` |
| 3 | Tujuh pesanan ditolak → lima uraian + "dan 2 lainnya" | ⛔ **Tidak** — sebab yang sama |

Menyuntik baris `Rejected` langsung lewat SQL **dilarang** — itu memaksa lulus tanpa jalur
aplikasi yang sah.

**Kesimpulan yang menentukan langkah berikutnya.** Butir 2 tertahan `BE-IGD-039`, bukan tertahan
pekerjaan tim. Menunggunya berarti menunggu penunjukan Security/Privacy owner yang belum ada.
`EPIC IGD-04` karena itu **boleh dimulai sekarang** — dan memang tidak bergantung pada butir 2
sama sekali.

## Uji layar pemilik 17 September 2026 — empat task ditutup, satu cacat alur ditemukan

Uji lewat layar **kedua** yang pernah dijalankan pada modul ini, oleh Product/Domain Owner.
Bukan UAT.

### Yang lulus

| Task | Yang dibuktikan |
| --- | --- |
| `FE-IGD-031` ✅ | Segmen Formulir/Riwayat pada tab pemeriksaan |
| `FE-IGD-032` ✅ | Pemilih periode, primary survey diringkas satu baris, lembar pemantauan berbentuk tabel berjajar, segmen Lembar/Catat. **Dua delta bagian 8 diterima pemilik** |
| `FE-IGD-023` ✅ | Tab Penunjang Medis |
| `FE-IGD-024` ✅ | Satu periode observasi diselesaikan beserta isian Kesimpulan |
| `BE-IGD-046` / `FE-IGD-028` | Satu putaran pemantauan bertanda vital tercatat; penanda *Tanda Vital Kritis* menyala |

### Cacat alur yang ditemukan — `FE-IGD-033`

Sesudah penilaian triage tersimpan, bagian pilih dokter muncul, tetapi tombol **Simpan
Pemeriksaan** tetap menyala dan tetap mengirim permintaan **penilaian baru**. Pemilik
menekannya lagi sesudah memilih dokter — kesimpulan yang wajar — dan menerima `409`.

**Konsekuensi yang menentukan urutan rilis.** Sesudah `BE-IGD-047`, penekanan kedua itu tidak
lagi gagal: ia **berhasil**, dan menambahkan penilaian kedua ke riwayat klinis pasien secara
senyap. Memperbaiki backend saja mengubah galat berisik menjadi catatan klinis ganda.
**`BE-IGD-047` dan `FE-IGD-033` wajib dirilis bersama.**

## Cacat nomor urut penilaian triage — ditemukan uji layar 17 September 2026

**Setiap kunjungan IGD hanya pernah bisa menyimpan satu penilaian triage.** Penilaian kedua
selalu ditolak `409`. Ditemukan Product/Domain Owner saat menjalankan butir 2 urutan delivery
(uji layar `FE-IGD-029`), dan ditangani sebagai `BE-IGD-047` gelombang R3.10
([laporan](task/report/backend/BE-IGD-047.md)).

`CreateEmergencyTriageRequest.Sequence` berbawaan `1`, sementara controller memakai
`request.Sequence > 0 ? request.Sequence : hitung()`. Penghitungan nomor urut di server karena
itu **tidak pernah dijalankan sekali pun**, dan setiap penilaian memakai nomor urut `1` —
menabrak `IX_EmgTriage_EmergencyVisitId_Sequence` mulai penilaian kedua.

Cacat ini ada sejak endpoint-nya dibuat dan **bukan** akibat gelombang mana pun yang lalu. Ia
tidak terlihat karena uji lewat layar pada modul ini baru dimulai 16 September 2026 — bukti
langsung bahwa ✅ berbasis pemetaan source tidak menggantikan uji runtime.

Source sudah diperbaiki 17 September 2026; `dotnet build` dan uji API tiga skenario **belum
dijalankan** dan menjadi syarat ✅ task itu.

## Kewenangan unit memblokir lintas gelombang — dinaikkan 17 September 2026

Dicatat saat review modul atas permintaan pemilik. **Nol source diubah, nol keputusan baru.**
Yang berubah hanya klasifikasi dampak `BE-IGD-039`: sebelumnya ditulis hanya menahan `MVP-6`.

### Apa yang dibaca di source

`EmergencyUnitAuthorityService.PeriksaAsync` punya dua lapis. Lapis pertama adalah gerbang
fail-closed `IGD-DEC-092` untuk unit yang belum dipetakan; lapis kedua barulah perbandingan
`ApplicationUserOrganization.DepartmentId` dengan `MstServiceUnit.OrganizationUnitId` yang
memang salah domain identitas.

Karena `MstServiceUnit.OrganizationUnitId` terisi **0 dari 18**, setiap panggilan berhenti di
lapis pertama dan **lapis kedua tidak pernah tercapai**. Artinya cacat perbandingan identitas
belum pernah benar-benar dieksekusi — memperbaikinya saja tidak membuka apa pun selama
pemetaan unit masih kosong. Keduanya harus selesai bersamaan.

### Jalan keluar beralasan `IGD-DEC-092` tidak ada pemakainya

Lapis pertama mengembalikan `Hasil(Berwenang: false, MembutuhkanAlasan: true, …)`. Flag
`MembutuhkanAlasan` itulah jalan keluar beralasan yang dijanjikan `IGD-DEC-092`.

**Tidak satu pun pemanggil membacanya.** Keempat titik panggil pada
`EmergencyDepartureService` — `arrive`, `accept-handover`, sikap pesanan, dan pemeriksaan unit
asal — hanya memeriksa `!authority.Berwenang` lalu mengembalikan `403`. Tidak ada jalur yang
menawarkan pengisian alasan, dan tidak ada tempat alasan itu disimpan.

### Akibat yang perlu dibaca pemilik

| Yang tertahan | Requirement | Sebelumnya ditulis |
| --- | --- | --- |
| `arrive` — kedatangan memindahkan pemilik klinis | `FR-IGD-023`, `024`, `025`, `027` | `MVP-4` 🟡 "uji `RENAME` balik belum" |
| `accept-handover` — peninjauan dokumen serah terima | `EPIC IGD-05` | sama |
| Sikap pesanan pada penutupan kunjungan | `FR-IGD-045`, `047`…`051` | `MVP-5` 🟡 |
| Seluruh `EPIC IGD-08` | `FR-IGD-053`…`059` | `MVP-6` ⛔ — **ini yang sudah benar** |

`MVP-4` dan `MVP-5` **tetap** 🟡, bukan ⛔: implementasinya ada dan boleh dilanjutkan. Yang
tidak dapat dilakukan adalah **membuktikannya lewat layar** sampai pemetaan unit terisi dan
jalan keluar beralasan dibangun. Karena itu keduanya tidak akan pernah mencapai ✅ lewat jalur
uji layar selama blocker ini terbuka.

### Yang diminta dari luar tim IGD

Satu permintaan, dua penerima, dan keduanya harus turun bersamaan:

1. **Penunjukan Security/Privacy owner** — untuk mengesahkan bentuk jalan keluar beralasan
   (siapa boleh memakainya, apa yang disimpan, bagaimana diaudit).
2. **Master Data mengisi `MstServiceUnit.OrganizationUnitId`** untuk 18 unit.

Tanpa keduanya, `BE-IGD-039` tidak dapat diselesaikan, dan `MVP-4`, `MVP-5`, serta `MVP-6`
sama-sama berhenti di pembuktian.

## Stale evidence

| Artifact/evidence | Recorded SHA | Current SHA | Required impact review |
| --- | --- | --- | --- |
| `01-existing-capability-map.md` revision `3` + suplemen `3.1` | `f69e9e48` / `300922c` | `0d13f3a8` (671 commit) | **Impact scan terbatas dikerjakan 22 September 2026 → suplemen `3.2`** untuk klaster encounter-first (`IGD-DEC-139`…`150`): 13 kemampuan lama dinilai ulang, 15 kemampuan baru `IGD-CAP-51`…`65`, conflict `IGD-CONF-06`…`08`, unknown `IGD-UNK-06`…`11`, pertanyaan penutup `IGD-TRQ-08`…`11`. **Area lain revisi 3 tetap stale** (pengkajian, penunjang, obat, kepergian, kewenangan unit) |
| ~~`blueprint-manifest.md` `artifact_hashes`~~ | 24 Agustus 2026 | 15 September 2026 | **Ditutup** — dihitung ulang pada pass penyelarasan teks kontrak (manifest bagian 0c) |
| Angka test pada laporan `BE-IGD-018`…`038` | `761 total, 759 lulus` (27 Agt) | Proyek test dihapus 11 Sep | Tidak dapat diulang; sah sebagai bukti historis (`IGD-DEC-110`) |
| ~~Penerapan migration `20260910031500` (rename encounter/resep, milik tim Registrasi)~~ | — | — | **Tidak lagi usang — dikoreksi 15 September 2026.** Migration **sudah diterapkan** pada `QuilvianNewDevRizki`. Bukti dari owner: Rizki menjalankan `dotnet ef migrations list --no-build` pada 15 September 2026, dan tidak ada migration berlabel `Pending`. Agent tidak menjalankan perintah basis data apa pun |

## Next recommended task

> **Diperbarui 22 September 2026 (akhir) — desain encounter-first selesai sebagai `draft` (`design-business-module`).**
> Gate slice: `PARTIALLY_READY` ([evidence](evidence/02-requirement-completeness-gate.md)); bentuk `SINGLE` (`IGD-DEC-155`); `erd/`
> dipertahankan + `flowcharts/` baru (`IGD-DEC-156`). Kontrak baru (semua `draft`, Rencana): API `0.11.0` (**bukan aditif murni**),
> validation `0.8.0`, state `0.5.0`, integration `0.4.0`, permission/audit `0.5.0`. Arsitektur §13 backend/frontend, kamus data §6,
> PRD §8 (`EPIC IGD-11`: `FR-IGD-069`…`085`, gelombang `MVP-7`; `EPIC IGD-12` kelayakan dokter `OPEN DECISION`), uji
> `AT-IGD-166`…`185`. Ditahan: `S7` (`IGD-OQ-102`/`103`). Tinjauan desain: `IGD-OQ-104`…`107` (tidak memblokir).
> **Langkah berikutnya:** pemilik menyetujui desain + kontrak, lalu `plan-module-delivery` final menyelaraskan kartu R3.13/R3.12.
>
> **Diperbarui 22 September 2026 (larut) — pertanyaan penutup impact scan dijawab: `IGD-DEC-151`…`154` (`approved`).**
> Pasien tanpa identitas memakai rekam pengganti lewat alur pasien baru yang ada (U1 `IGD-DEC-140` dan `IGD-DEC-149`
> `superseded`); koreksi waktu tiba tidak boleh sesudah peristiwa klinis pertama; `PATCH …/status` Registrasi menolak
> encounter Emergency, `…/cancel` hanya sebelum kunjungan lahir; `PatientId`/`EncounterId` kunjungan terkunci pada `PUT`.
> **Nol conflict terbuka** pada klaster encounter-first. Berikutnya `design-business-module`; kartu R3.13/R3.12 masih menunggu
> `plan-module-delivery` final. Yang tersisa hanya data (kueri D, E1–E3, A/B) dan pemilik yang belum dipetakan.
>
> **Diperbarui 22 September 2026 (malam) — impact scan `trace-existing-capabilities` selesai (capability map suplemen 3.2).**
> Temuan yang menahan desain: **`IGD-CONF-06`** — layar pendaftaran selalu menuntut pasien dan selalu membuat encounter,
> jadi jalur U1 `IGD-DEC-140` tidak punya penghasil di frontend. **Koreksi fakta:** waktu tiba **bukan** dasar SLA triage
> (`ResponseDueAt` dari mulai triage) — isi `IGD-DEC-147` tetap, alasannya dikoreksi. **Fakta baru:** pintu encounter sudah
> antre lewat kunci penomoran global (`PatientEncounterNumberService`). Empat pertanyaan penutup `IGD-TRQ-08`…`11` untuk
> pass `grill-me` singkat sebelum `design-business-module`.
>
> **Diperbarui 22 September 2026 (sore) — amendment pass `grill-me` kasus tepi encounter-first.**
> Pemilik menjawab butir A–I: `IGD-DEC-142`…`150` (`approved`), koreksi `IGD-DEC-139` (tanda kelima `NoShowAt`),
> `IGD-OQ-094`/`095`/`096`/`097`/`100`/`101` `superseded`, `IGD-OQ-093` `superseded` sebagian (realisasi terbuka di
> `BE-IGD-053`). **Kartu R3.13/R3.12 di bawah BELUM diselaraskan** — atas keputusan pemilik, urutannya:
> `trace-existing-capabilities` (impact scan) → `design-business-module` → `plan-module-delivery` final → implementasi.
> Kolom "Yang dibutuhkan dari pemilik" pada tabel di bawah karena itu **sebagian usang**: yang tersisa hanya angka
> kueri D, E1–E3, dan A/B.
>
> **Diperbarui 22 September 2026 — desain encounter-first ditulis ke repo (docs saja, nol source).**
> Tiga keputusan disetujui **prinsip**: `IGD-DEC-139` (encounter-first + rumus episode terbuka),
> `IGD-DEC-140` (pasien tanpa identitas: U1 transisi `approved`, U2 `candidate`), `IGD-DEC-141` (kelayakan
> dokter jaga + override). `IGD-OQ-093` **`superseded` sebagian** (bentuk B1; realisasi `BE-IGD-053`). `FE-IGD-027` **tetap ✅**.
> Task baru: backend R3.13 `BE-IGD-051`…`056`, frontend R3.12 `FE-IGD-035`…`037`
> ([evidence](evidence/2026-09-22-desain-encounter-first.md)).
>
> | Urutan | Task | Keadaan | Yang dibutuhkan dari pemilik |
> | ---: | --- | --- | --- |
> | 1 | `BE-IGD-051` — kunjungan berakhir menutup encounter | **Siap** (prasyarat ✅) | Go-ahead + jawaban TK-1 (hapus lunak) dan TK-2 (encounter `Outpatient` lama) |
> | 1 | `BE-IGD-054` — daftar Menunggu Triage terpadu | **Siap** (prasyarat ✅) | Go-ahead + terima/tunda risiko baris K3 lama tampil sampai `BE-IGD-052` |
> | 2 | `BE-IGD-052` — rekonsiliasi encounter historis | ⛔ | Konfirmasi K1–K4, **angka** kueri D, `IGD-OQ-095` |
> | 3 | `BE-IGD-053` — penjaga di pintu encounter | ⛔ | `IGD-OQ-096`, `IGD-OQ-101` (sesudah `051` dan `052`) |
> | — | `BE-IGD-055` — Mulai Triage | ⛔ | W1 (`IGD-OQ-094`), `IGD-OQ-100`, status awal kunjungan |
> | — | `BE-IGD-056` — dokter layak + override | ⛔ | **Angka** kueri E1–E3 |
> | sesudah backend | `FE-IGD-035`, `FE-IGD-036`, `FE-IGD-037` | Menunggu backend | `FE-IGD-036` **wajib** menunggu `BE-IGD-053` — memutus loket dari `POST emergency-visits` lebih dulu membuka pendaftaran ganda |
>
> **Masih terbuka dari gelombang sebelumnya:** angka kueri audit A/B `BE-IGD-050` acceptance 8. Teks
> `IGD-DEC-138` kolom status masih menulis "menunggu konfirmasi pemilik atas laporan rencana" padahal
> mekanismenya sudah diputuskan 21 September 2026 malam — housekeeping, tidak disentuh pass ini.
>
> **Diperbarui 21 September 2026 (sore) — `EPIC IGD-04` tuntas; backlog kecil diperiksa.** Butir 4 di bawah
> (`BE-IGD-044` → `BE-IGD-045` → `FE-IGD-027`) **selesai**. Sesuai urutan pemilik, backlog kecil (butir 7)
> dikerjakan sesudahnya, dengan hasil:
>
> | Task | Hasil | Yang dibutuhkan berikutnya |
> | --- | --- | --- |
> | `FE-IGD-014` kriteria 2 | ✅ **Dinilai ulang 21 September 2026 (malam) — atas penilaian pemilik, sesudah `FE-IGD-034` ✅; celah `IGD-OQ-093` `open` dinyatakan** ([laporan](task/report/frontend/FE-IGD-014.md) bagian 9). ✅ ini **tidak** menyatakan tidak ada encounter yatim dalam segala keadaan, dan encounter yatim yang sudah ada belum diaudit. *Riwayat 🟡:* **Sudah di source** (kotak peringatan + tombol *Buka Kunjungan IGD*, memakai endpoint yang sudah ada). `npm run build` **lulus**; **uji layar (pemilik, 21 September 2026 malam): kotak, nomor, dan status tampil benar**, hasil klik tombol belum dikonfirmasi ([laporan](task/report/frontend/FE-IGD-014.md) bagian 6.1). Uji itu membuktikan **encounter yatim terjadi** (peringatan *Encounter sudah terbentuk*). **Belum boleh ✅ end-to-end** (`IGD-DEC-138`) | Pemilik: konfirmasi hasil klik *Buka Kunjungan IGD*. Encounter yatim ditangani `BE-IGD-050` → `FE-IGD-034`; sisa celah `IGD-OQ-093` |
> | `FE-IGD-017` kriteria 1 | ✅ **22 September 2026 — atas penilaian pemilik** (`npm run build` dinyatakan lulus; keluaran tidak dilampirkan). *Riwayat 🟡 21 September 2026 (larut malam):* kolom pelaku/penyetuju membaca `recordedByName`/`approvedByName` (kontrak `0.9.0`, `BE-IGD-049`); eslint 0 error, 5 unit test baru lulus, suite 1415/1424 (9 gagal = Rawat Inap, sudah ada sebelumnya). **Uji layar pemilik LULUS (21 September 2026, larut malam): kolom *Pelaku* menampilkan `SuperAdmin` pada ketiga kejadian `DEP-260921064559-9B2DB7`, menggantikan GUID** ([laporan](task/report/frontend/FE-IGD-017.md) 6.1) — sekaligus membuktikan `BE-IGD-049` berjalan pada jalur `GET` daftar | ~~Pemilik: nyatakan `npm run build` lulus~~ — **dinyatakan 22 September 2026 → `FE-IGD-017` ✅**. ~~Uji API `BE-IGD-049` S2–S5~~ — **dilaporkan lulus 22 September 2026 → `BE-IGD-049` ✅** |
>
> **R3.12 / R3.11 (21 September 2026) — `BE-IGD-049` sudah diimplementasikan (malam, build lulus); `BE-IGD-050` ✅ sudah diimplementasikan dan diuji pemilik (malam, tiga berkas source, kontrak `0.10.0`; S1–S7 `PASS` menurut pemilik); `FE-IGD-034` ✅ sudah dikerjakan sesudahnya dan dinyatakan ✅ atas penilaian pemilik (malam, lima berkas frontend, commit `c941012ac`).** Task baru: `BE-IGD-049`
> (nama pelaku pada event kepergian), `BE-IGD-050` (pra-cek episode ganda sebelum encounter dibuat — korektif),
> `FE-IGD-034` (layar memanggil pra-cek). Rantai: `BE-IGD-033/034 ✅ → BE-IGD-049 → FE-IGD-017` dan
> `BE-IGD-023/025 ✅ → BE-IGD-050 → FE-IGD-034 ← FE-IGD-014`. Keputusan: `IGD-DEC-137`, `IGD-DEC-138`
> (`approved`), `IGD-OQ-093` (`open`). **Diputuskan pemilik 21 September 2026 (malam):** mekanisme `BE-IGD-050` = lapis A
> (pra-cek) dengan `IGD-OQ-093` sebagai backend gap eksplisit; hak akses `EmergencyVisit : Create`; `FE-IGD-034` `fail-open`
> bila pra-cek gagal. **Go-ahead implementasi `BE-IGD-050` diberikan pemilik dan dikerjakan 21 September 2026 (malam)** —
> laporan [BE-IGD-050](task/report/backend/BE-IGD-050.md). **Pemilik membangun dan menjalankan uji API S1–S7 — semuanya `PASS`
> (pernyataan pemilik); `BE-IGD-050` ✅ atas penilaian pemilik.** Sisa milik pemilik: kueri audit A/B (acceptance 8, dikecualikan
> dari ✅). **`FE-IGD-034` ✅ atas penilaian pemilik (21 September 2026 malam)** — build dan uji layar dilaporkan lulus (pernyataan pemilik; keluaran dan rincian skenario tidak dilampirkan; artefak `.next` 16:03 dan commit `c941012ac` diperiksa agent). **`FE-IGD-014` dinilai ulang → ✅ dengan celah `IGD-OQ-093` dinyatakan.** `IGD-OQ-093` dicatat sebagai **backend gap eksplisit** pada laporan (pendaftaran serentak dan
> klien tanpa pra-cek tidak tertutup).
>
> Sisa yang **tidak** menunggu pihak luar: laporan susulan (`BE-IGD-017`, `BE-IGD-043`, `FE-IGD-025`, `FE-IGD-026`)
> dan uji `Down` migration `20260826090500` (membuka `BE-IGD-026` dan `BE-IGD-031`; migration itu berada di
> belakang sekitar 75 migration modul lain, jadi butuh rancangan uji tersendiri). Sisanya menunggu pihak luar:
> `BE-IGD-039` beserta pemetaan 18 unit, `BE-IGD-042`.

> **Urutan delivery ditetapkan Product/Domain Owner 16 September 2026.** Butir 1 dan 2 wajib lulus
> lebih dulu; sesudahnya pekerjaan besar berikutnya adalah `EPIC IGD-04`. Butir lain **tidak boleh**
> didahulukan, dan agent **tidak memulai task lain secara otomatis**.

> **Sisipan 16 September 2026 (keempat).** Pemilik menugaskan gelombang **R3.9 — tata letak
> riwayat pada ruang kerja pemeriksaan**: `FE-IGD-031` lalu `FE-IGD-032`
> ([evidence](evidence/2026-09-16-tata-letak-riwayat-pemeriksaan.md), `IGD-DEC-133` dan
> `IGD-DEC-134`). Letaknya **sesudah butir 1** dan **sebelum butir 4** (`EPIC IGD-04`). Keduanya
> nol backend, nol kontrak, dan tidak menunggu siapa pun selain `FE-IGD-028` ✅ yang sudah
> selesai. Butir 1 tetap didahulukan karena tinggal satu klik.
>
> **Diperbarui 16 September 2026 (keempat):** implementasi kedua task **sudah selesai** dan
> berstatus 🟡. Yang menahan keduanya sekarang hanya dua hal, dan keduanya milik Rizki:
> menjalankan `npm run build` di `QuilvianSystemFrontendDev`, lalu membuka layar Assesmen IGD
> untuk mencoba segmen Formulir/Riwayat pada tab Assesmen Awal, dan — untuk `FE-IGD-032` —
> menyelesaikan satu periode observasi beserta isian Kesimpulan serta mencatat satu putaran
> pemantauan bertanda vital. Dua delta pada `FE-IGD-032` menunggu penilaian pemilik; lihat
> [laporan](task/report/frontend/FE-IGD-032.md) bagian 8.

Seluruh task di bawah **sudah punya kartu roadmap**. Urutan yang tidak menunggu pihak lain:

1. **`FE-IGD-029`** 🟡 — satu-satunya sisa gelombang R3.8, dan tinggal **satu klik**. Kriteria 1
   sudah terbukti lewat layar 16 September 2026: pasien yang baru didaftarkan langsung berstatus
   "Menunggu Triage". Kriteria 2 belum — tekan **Isi Triage** pada pasien baru itu lalu simpan
   sampai berhasil; itu membuktikan penolakan `409` yang memicu gelombang ini benar-benar hilang.
   Pasangannya **`FE-IGD-030`** ✅ sudah terbukti penuh.
2. **`BE-IGD-046`** → **`FE-IGD-028`** — pemantauan observasi bertanda vital (`IGD-DEC-122`…`126`,
   kontrak `0.6.0`). Keputusan dan kontraknya sudah terkunci 16 September 2026; implementasinya
   sudah ditulis dan tinggal uji lewat layar.
3. ~~**`BE-IGD-041`**~~ 🟡 — **dikerjakan 16 September 2026.** Pesan penutupan kini menyebut
   pesanan yang menahannya, dan kriteria 2 `BE-IGD-035` terpenuhi pada source. Tersisa
   `dotnet build` dan uji API tiga skenario ([laporan](task/report/backend/BE-IGD-041.md)).
4. **`BE-IGD-044`** → **`BE-IGD-045`** → **`FE-IGD-027`** — riwayat penugasan dokter
   (`EPIC IGD-04`). **Pekerjaan besar berikutnya menurut keputusan owner 16 September 2026**,
   dimulai setelah butir 1 dan 2 lulus. Pada `BE-IGD-044` agent menyiapkan model, konfigurasi EF,
   kebutuhan service, dan kebutuhan migration; **agent tidak menjalankan migration** — Rizki
   menjalankannya sendiri setelah review. Kartu ketiganya sudah ditinjau ulang 16 September 2026
   terhadap `IGD-DEC-116`/`117` dan kontrak terbaru; hasilnya di bagian *Tinjauan kesiapan
   `EPIC IGD-04`* di bawah.
5. **Laporan susulan:** `BE-IGD-017`, `BE-IGD-043` (`f76ebaab`), `FE-IGD-025` (`bd1d94a8a`,
   termasuk temuan privasi `IGD-EV-117`), `FE-IGD-026` (`c8613d88c`).
6. **Uji langkah mundur migration** `20260826090500` di basis data terpisah — membuka
   `BE-IGD-026` dan `BE-IGD-031`. Butuh basis data terpisah milik Rizki.
7. **Backlog kecil, sengaja tidak didahulukan** (keputusan owner 16 September 2026):
   `FE-IGD-017` (pelaku kepergian tampil sebagai UUID) dan `FE-IGD-014` kriteria 2 — keduanya
   `IGD-EV-123`. Dikerjakan setelah `EPIC IGD-04`, bukan sebelum.

**Diperbarui 17 September 2026.** `npm run build` frontend sudah dijalankan dan **lulus bersih**
([evidence](evidence/2026-09-17-verifikasi-build-dan-perkakas.md)), jadi penahan build pada
`FE-IGD-023`, `FE-IGD-024`, `FE-IGD-031`, dan `FE-IGD-032` **lunas**. Keempatnya kini menyisakan
**uji lewat layar** saja — ditambah penilaian pemilik atas dua delta `FE-IGD-032` bagian 8.

Yang masih menunggu **build backend milik Rizki**, lalu uji API: `BE-IGD-040` dan `BE-IGD-041`.
Perintahnya `dotnet build -p:RunAnalyzers=false` di dalam `NewQuilvianSystemBackend`.

Yang **menunggu owner**: `BE-IGD-042` — jumlah `EmgVisit` aktif dengan `EncounterType.Outpatient`
dari kueri yang dijalankan Rizki sendiri.

Gap yang **tidak** diberi ID atas instruksi owner: layar resusitasi IGD (`IGD-EV-111`) dan layar
baca/aksi `order-items` (`IGD-EV-109`).

**Ditegaskan ulang 16 September 2026** sesudah `BE-IGD-041` membuat penolakan penutupan menyebut
pesanan yang menahannya: gap aksi `order-items` **tetap tidak dibuatkan task implementasi**. Ia
dicatat sebagai terblokir dengan dependency `BE-IGD-039`. Layar baca-saja **juga tidak**
diprioritaskan, karena menampilkan daftar pesanan tanpa aksi belum memberi petugas jalan
penyelesaian — hanya memindahkan kebuntuan dari pesan galat ke layar.

## Tinjauan kesiapan `EPIC IGD-04` — 16 September 2026

Dijalankan atas perintah owner sebelum coding dimulai. Ketiga kartu `BE-IGD-044`, `BE-IGD-045`,
dan `FE-IGD-027` ditinjau terhadap `IGD-DEC-116`, `IGD-DEC-117`, dan kontrak `0.5.0`.
**Nol requirement didesain ulang.**

### Ketujuh pertanyaan audit owner terjawab kontrak yang sudah terkunci

| # | Pertanyaan | Dijawab oleh | Keadaan |
| ---: | --- | --- | :-: |
| 1 | Siapa dokter penanggung jawab saat ini | `GET /active` tanpa `at` (API §3); baris dengan `EffectiveTo IS NULL`, dijaga unique bersyarat | ✅ |
| 2 | Sejak kapan | Kolom `EffectiveFrom` (kamus data §4) | ✅ |
| 3 | Siapa dokter sebelumnya | `GET /` riwayat urut waktu (`BE-IGD-045` acceptance 8); baris lama tidak pernah ditimpa | ✅ |
| 4 | Kapan pengalihan terjadi | `EffectiveTo` baris lama ditutup bersamaan dengan `EffectiveFrom` baris baru dalam satu transaksi (acceptance 3) | ✅ |
| 5 | Siapa yang mengalihkan | `AssignedByUserId` pada baris baru, diisi sistem dari pengguna aktif | ✅ |
| 6 | Alasan pengalihan | `AssignmentReason` (500), wajib saat pengalihan (acceptance 4) | ✅ |
| 7 | Dokter pada waktu tertentu | `GET /active?at={datetime}` (`IGD-DEC-117`, API §3.1, acceptance 9) | ✅ |

### Empat hal yang perlu diputuskan — **SELURUHNYA DIPUTUSKAN OWNER 16 September 2026**

| No | Temuan | Keputusan |
| ---: | --- | --- |
| 1 | **`BE-IGD-045` tidak menjanjikan nama.** `FE-IGD-027` acceptance 6 menuntut dokter dan penugas tampil sebagai **nama**, bukan ID (`IGD-EV-123`), tetapi acceptance `BE-IGD-045` tidak menyebut proyeksi nama sama sekali. Bila backend hanya mengirim `DoctorId` dan `AssignedByUserId`, frontend tidak dapat memenuhi kriterianya tanpa panggilan tambahan | **`IGD-DEC-129` — DISETUJUI.** Proyeksi aditif `doctorName` dan `assignedByName` pada `GET /` dan `GET /active`; `doctorId` dan `assignedByUserId` tetap dikirim. Satu kueri berproyeksi, nol `N+1`, nol kolom baru. API naik `0.7.0` bagian 3.2; acceptance `BE-IGD-045` bertambah butir 12 |
| 2 | **`IsActive` dan `EffectiveTo` sama-sama menyatakan "aktif".** Kamus data §4 memuat keduanya, sedangkan unique bersyarat hanya menjaga `EffectiveTo IS NULL` | **`IGD-DEC-130` — DIPUTUSKAN.** `IsActive` **dihapus dari rancangan sebelum model dan migration dibuat**. Penugasan berjalan = `EffectiveTo IS NULL`; pada waktu `T` = `EffectiveFrom <= T AND (EffectiveTo IS NULL OR T < EffectiveTo)`. Kamus data §4 sudah diselaraskan; acceptance `BE-IGD-044` butir 2 dan 3 serta `BE-IGD-045` butir 13 mengikuti |
| 3 | **`BE-IGD-045` menuntut baris baru di `Program.cs`.** Service dan controller `EmergencyDoctorAssignment` sepenuhnya baru, jadi pendaftaran DI tidak terhindarkan. Kartunya sudah mensyaratkan agent berhenti dan meminta persetujuan owner | **`IGD-DEC-131` — DISETUJUI TERBATAS.** Hanya untuk pendaftaran DI service Emergency Doctor Assignment yang memang baru; dilarang cleanup atau refactor DI lain; baris pendaftarannya wajib dicantumkan pada laporan task |
| 4 | **Kontrak menyebut nama tabel yang sudah diganti.** API §3 masih menulis *"memperbarui `TrxPatientEncounter.DoctorId`"*, padahal tabel itu sudah menjadi `RegPatientEncounter` sejak `58c61a5b` (10 September 2026) | **`IGD-DEC-132` — DISETUJUI.** Kontrak memakai `RegPatientEncounter`. Sudah diterapkan pada API §3 dan integration contract. ERD dan kamus data §5.3 **masih** memakai nama lama dan berada di luar lingkup keputusan ini |

### Yang sudah siap dan tidak perlu dipertanyakan lagi

Pola reuse `BE-IGD-044` terbukti ada: `InPatientManagement/Models/InpDoctorAssignment.cs` beserta
`Repositories/Configurations/HealthServices/InPatientManagement/InpDoctorAssignmentConfiguration.cs`.
Hak akses `EmergencyDoctorAssignment` sudah terdaftar pada permission matrix baris 41 dengan
`Read`, `Create`, dan `Update`. Nama tabel, kolom, index, dan unique bersyarat sudah terkunci
`IGD-DEC-116` dan kamus data §4.

### Gerbang yang tetap terbuka

~~`IGD-DEC-082` masih `draft` menunggu Clinical Governance.~~ **Ditutup 17 September 2026** —
`approved` oleh Product/Domain Owner, jadi butir 10 Definition of Done `BE-IGD-045` tidak lagi
tertahan. Peran Clinical Governance tetap `OPEN` dan wajib meninjau ulang bila kelak ditunjuk.

Yang **masih** terbuka: `BE-IGD-044` acceptance 5 menuntut langkah mundur migration diuji di
**basis data terpisah** — milik Rizki, dan menjadi syarat ✅ task itu.

## Backlog lintas modul — `ClinicalManagement`

Ditemukan pada uji layar `FE-IGD-028` 16 September 2026, dan **atas keputusan owner tidak masuk
lingkup `EPIC IGD-04`**. Keduanya milik formulir tanda vital bersama, bukan modul IGD.

| Temuan | Isi | Bukti |
| --- | --- | --- |
| Jenis oksigen bertentangan dengan laju alir | Baris tersimpan dengan penanda memakai oksigen bernilai benar, jenis "tidak menggunakan", dan laju `50 L/menit`. Riwayat menampilkan *"Oksigen: Tidak menggunakan · 50 L/menit"* — tampilannya benar, isiannya yang longgar | Laporan `FE-IGD-028` butir (4) |
| Nilai tanda vital ekstrem diterima tanpa peringatan | Nadi `250 x/menit` dan frekuensi napas `80 x/menit` tersimpan. Penanda *"Tanda Vital Kritis"* menyala, tetapi nol batas kewajaran yang menahan salah ketik | Laporan `FE-IGD-028` butir (5) |

## Optional deterministic delivery progress

| Lapisan | Rumus | Hasil |
| --- | --- | --- |
| Backend | task ✅ / seluruh task roadmap | **26 / 40 = 65%** (22 September 2026: penyebut naik karena `BE-IGD-051`…`056` direncanakan — bukan karena ada yang mundur; **dan dikoreksi**: register memuat 34 baris sebelum penambahan, bukan 35, jadi angka lama seharusnya 26 / 34 = 76%). *Sebelumnya tertulis* **26 / 35 = 74%** (`BE-IGD-049` ✅ 22 September 2026 atas penilaian pemilik sesudah uji API S2–S5; sebelumnya 25 / 35 = 71% dan 24 / 35 = 69%). *Catatan lama:* **24 / 35 = 69%** (penyebut naik dari 33 karena `BE-IGD-049` dan `BE-IGD-050` direncanakan 21 September 2026 sore — bukan karena ada yang mundur; sebelumnya **24 / 33 = 73%**: `BE-IGD-044` ✅ 21 September 2026 sesudah uji `Down` asli lulus; sebelumnya 23 / 33 = 70% sesudah `BE-IGD-048` ✅, dan 22 / 33 = 67% sesudah `BE-IGD-045` ✅ 17 September 2026). Dihitung dari *Register status task* `backend-roadmap.md`. Tidak dikecualikan: `BE-IGD-039` dan `BE-IGD-042` (⛔) tetap dihitung di penyebut. *Sebelumnya tertulis 18 / 29 = 62% — usang sejak `BE-IGD-040` dan `BE-IGD-046` ✅ dan `BE-IGD-046` masuk register* |
| Frontend | task ✅ / seluruh task roadmap | **18 / 27 = 67%** (22 September 2026: penyebut naik karena `FE-IGD-035`…`037` direncanakan; **dan dikoreksi**: register memuat 24 baris sebelum penambahan, bukan 25, jadi angka lama seharusnya 18 / 24 = 75%). *Sebelumnya tertulis* **18 / 25 = 72%** (`FE-IGD-017` ✅ 22 September 2026 atas penilaian pemilik; sebelumnya 17 / 25 = 68% sesudah `FE-IGD-034` ✅ dan `FE-IGD-014` ✅, dan 15 / 25 = 60%). *Catatan lama:* **15 / 25 = 60%** (penyebut naik dari 24 karena `FE-IGD-034` direncanakan 21 September 2026 sore; sebelumnya **15 / 24 = 63%**: `FE-IGD-027` ✅ kembali 21 September 2026 sesudah `npm run build` pemilik lulus; sempat 14 / 24 = 58% pada hari yang sama saat dinilai ulang). `FE-IGD-014` 🟡 dan `FE-IGD-017` 🟡 tidak dihitung (dihitung ulang 17 September 2026 sesudah uji layar pemilik). ✅: `FE-IGD-015`, `016`, `018`, `019`, `020`, `021`, `023`, `024`, `027`, `028`, **`029`**, `030`, **`031`**, **`032`**, **`033`**. *Sebelumnya tertulis 6 / 17 = 35% — usang sejak `FE-IGD-023`, `024`, `028`, `030` ✅ dan `FE-IGD-029`…`032` masuk register* |

Persentase turun dari 78%/45% bukan karena ada yang mundur, melainkan karena penyebutnya
bertambah task baru. Tiga area R3.5 (penunjang medis, pemakaian alat, billing IGD) **tidak**
masuk penyebut karena belum punya task. Persentase ini bukan ukuran kesiapan produksi.

## Status contract

`DRAFT` has identity but incomplete intake. `DISCOVERY` is collecting decisions/evidence. `READY` means planned phases may start. `PARTIAL` means at least one phase is ready while another is blocked or unknown. `BLOCKED` means no material phase can safely proceed. `IN_PROGRESS` has authorized active work. `VERIFYING` awaits readiness evidence. `DONE` requires appropriate verification evidence. `SUPERSEDED` records the successor blueprint.

Phase statuses are `NOT_STARTED`, `READY`, `IN_PROGRESS`, `BLOCKED`, `DONE`, and `SUPERSEDED`. A phase becomes `DONE` only when its acceptance/readiness evidence is recorded; file existence is insufficient.
