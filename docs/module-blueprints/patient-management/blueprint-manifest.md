# Patient Management Blueprint Manifest

| Field | Nilai |
| --- | --- |
| `blueprint_id` | `PAT-BP-001` |
| `module_name` | Patient Management |
| `module_slug` | `patient-management` |
| `module_prefix` | `Pat` — registry `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 24) |
| `area` | `HealthServices` |
| `blueprint_shape` | `SINGLE` — `shape_decided_by: USER` (`PAT-DEC-002`) |
| `revision` | **`6`** — 19 Oktober 2026: `PAT-DEC-069`–`077` — task prasyarat `BE-PAT-MIG-003A` (guard reservasi jalur pendaftaran bayi) dan amandemen desain `BE-PAT-MIG-002` hasil impact scan read-only; baseline source `BE-PAT-MIG-003A` Integration `c6537ee4`. Riwayat `5` — 16 Oktober 2026: `PAT-DEC-068` — kunci allocator dua bagian tetap dan konfirmasi endpoint pemeriksa; baseline source `BE-PAT-MIG-003` disinkronkan ke Integration `0c6e3145`. Riwayat revision `4`: 16 Oktober 2026: amandemen pemilik sebelum build `BE-PAT-MIG-003` (`PAT-DEC-067`) — `Enabled = false` di Staging persis gagal tertutup, dan endpoint pemeriksa `PAT-GATE-005`. Riwayat revision `3`: 14 Oktober 2026: amandemen material untuk `BE-PAT-MIG-002` (keputusan, kontrak, dependency, dan gerbang baru); diamandemen 15 Oktober 2026 dengan bukti database Step 16 dan keputusan pemilik `PAT-DEC-034`–`043`; amandemen kedua 15 Oktober 2026 dengan keputusan `PAT-DEC-044`–`062` dan task prasyarat `BE-PAT-MIG-003`; **difinalkan dan disetujui 15 Oktober 2026, sesi ketiga** — keputusan `PAT-DEC-063`–`066`, `requirement-completeness-gate` `PASS`, persetujuan desain `PAT-DEC-033`. Riwayat: `2` — perubahan keputusan yang disetujui; lihat "Riwayat revisi" |
| `status` | **Keadaan 22 Oktober 2026:** `BE-PAT-MIG-003` ✅; `BE-PAT-MIG-003A` ✅ — di-*merge* (PR #294), dideploy ke Staging, `PAT-GATE-005` generasi 2 `PASS` (`PAT-FACT-092`, `093`); `PAT-RSK-042` ditutup (`PAT-FACT-094`); `PATIENT_BASELINE = 741` (`PAT-FACT-095`); F-01 `RESOLVED` (`PAT-FACT-097`). `BE-PAT-MIG-002`: `approved`, kontrak `1.3.0`, **belum dikerjakan — boleh masuk implementasi source**; `PAT-GATE-002` dan Canary-A `NOT RUN`; eksekusi massal tidak diberi wewenang. *Keadaan 20 Oktober 2026 — jejak:* `BE-PAT-MIG-003A`: **`approved`** — revision `6`, 19 Oktober 2026 (`PAT-DEC-069`); 🟡 siap finalisasi lokal, provider-real akhir `23`/`23` serial `PASS` pada `6e72ee6c`, pra-runtime. `BE-PAT-MIG-003`: ✅ — dideploy, `PAT-GATE-005` generasi 1 `PASS` (`PAT-FACT-090`). `BE-PAT-MIG-002`: ⛔ — impact scan `BLOCKED` (`PAT-FACT-091`), desain diamandemen revision `6`. `BE-PAT-MIG-001`: **`approved`** — revision `2`. `BE-PAT-MIG-002` dan task prasyarat `BE-PAT-MIG-003`: **`approved`** — revision `3` 15 Oktober 2026 (`PAT-DEC-033`), revision `4` dan `5` 16 Oktober 2026 (`PAT-DEC-067`, `068`); 27 `OWNER_APPROVED`, 3 `SUPERSEDED`, 0 `BLOCKING`. Build: `BE-PAT-MIG-003` 🟡 siap review pemilik — direkonsiliasi ke Integration `0c6e3145`, uji PostgreSQL 15.15 provider-real 10/10 PASS 17 Oktober 2026 ([`task/report/backend/BE-PAT-MIG-003.md`](task/report/backend/BE-PAT-MIG-003.md)); `BE-PAT-MIG-002` ⛔. Persetujuan desain tidak memberi wewenang *commit*, deployment, eksekusi database, pemanggilan endpoint, pembuatan QR, atau runtime Canary-A. Keduanya **bukan** persetujuan desain modul Patient Management secara utuh |
| `created_at` | `2026-10-08` |
| `updated_at` | `2026-10-22` |
| `last_verified_at` | `2026-10-22` — impact scan final read-only `BE-PAT-MIG-002` pada Integration `201d367e` (`CONDITIONAL` karena F-01, `PAT-FACT-096`), perbaikan F-01 di Integration `8bccaa03` (`PAT-FACT-097`), dan sinkronisasi dokumen dengan bukti runtime pemilik (`PAT-FACT-092`–`095`). Sebelumnya `2026-10-19` — impact scan `BE-PAT-MIG-002` read-only (`BLOCKED`, `PAT-FACT-091`) lalu build `BE-PAT-MIG-003A` pada Integration `c6537ee4` (HEAD = origin). Sesudah itu: rebase ke `98fdc69f` (commit `6ae9c251`), bukti provider-real pemilik `23`/`23` `PASS`, review final read-only, dan koreksi `F-07`/`F-13`/`F-14` (19 Oktober 2026); `2026-10-20` — rebase ke `724629ab`, kandidat `6e72ee6c`, bukti akhir pemilik provider-real `23`/`23` serial, closeout dokumen. Sebelumnya `2026-10-15` — `requirement-completeness-gate` untuk `BE-PAT-MIG-003` dan `BE-PAT-MIG-002` (`requirement-completeness-assessment.md`); HEAD backend dicocokkan dengan `a52b3c98` |
| `backend_source_sha` | **22 Oktober 2026:** impact scan final `BE-PAT-MIG-002` pada `QuilvianIntegrationBackend` @ **`201d367ed7722320069c69e266133fbabdaf0fc3`**; sinkronisasi dokumen di atas Integration **`8bccaa034db955c9acb719fc1cb476ed9c71f1c3`** (perbaikan F-01 hanya di `Tests/`). Artefak Staging **`fb2099a96806eed6e2e0e0d2f28bfff2bc04b26c`** = Integration `88806486` + manifest versi, memuat merge `BE-PAT-MIG-003A` `ce34b22b`. Source PatientManagement pada `201d367e` sama dengan artefak itu. Sebelumnya: `BE-PAT-MIG-003A`: branch `BE-PAT-MIG-003A`, kandidat **`6e72ee6c6a41c2931af6c0a36ceacf9a9ebc92ea`** di atas `QuilvianIntegrationBackend` @ **`724629ab8921eb16559c6802a64b0c45516d70b0`**; dibangun semula di atas `c6537ee4`, di-*rebase* ke `98fdc69f` (`6ae9c251`) lalu `724629ab` (delta upstream tidak menyentuh penomoran pasien). `BE-PAT-MIG-001`: `103b45ccd5f0d9e2cbacff588540d8fe3d52e706` (`QuilvianStaDeploy`). `BE-PAT-MIG-002`: **`a52b3c98e7e9b0fae42f9a889b84c6c8332f8923`** (`QuilvianIntegrationBackend`, impact scan 14 dan 15 Oktober 2026 — HEAD tidak bergeser sampai finalisasi sesi ketiga; `git fetch` tidak dijalankan. Berlaku juga untuk `BE-PAT-MIG-003`). `BE-PAT-MIG-003` build dan rekonsiliasi: **`0c6e314594766fe7e55a833a0ce88095dc665a7c`** (`QuilvianIntegrationBackend`, fast-forward 41 commit dari `a52b3c98`, 16 Oktober 2026) |
| `frontend_source_sha` | `BE-PAT-MIG-001`: tidak diperiksa (`PAT-DEC-015`). `BE-PAT-MIG-002`: **`2831ea635d83c0eca3f2cfbb0ae3be383e3ce204`** (`QuilvianSystemFrontendDev`, branch `fix/final-dev-tree-alignment`, working tree dibaca read-only 15 Oktober 2026 untuk konsumen QR; SHA dibaca dari berkas `.git` karena perintah Git ditolak oleh pemeriksaan kepemilikan folder) |
| `approved_by` | Revision `6`: pemilik (akun Git `devbenari`) — instruksi tertulis `TASK: BE-PAT-MIG-003A` 19 Oktober 2026, keputusan A–H dan amandemen 10.1–10.6. Revision `1`–`2`: pemilik/leader work item, sesi `MODULE BLUEPRINT` 8 Oktober 2026 (akun Git `devbenari`). Revision `3`–`5`: pemilik (akun Git `devbenari`) — keputusan individual `PAT-DEC-034`–`066` dalam tiga sesi 15 Oktober 2026, dan persetujuan desain `PAT-DEC-033` lewat instruksi tertulis sesi ketiga yang menyetujui finalisasi bila gate kelengkapan lulus; revision `4` lewat instruksi tertulis `TASK MODE: BACKEND` 16 Oktober 2026 bagian 0 dan 9 (`PAT-DEC-067`); revision `5` lewat instruksi tertulis *source reconciliation / review fix pass* 16 Oktober 2026 (`PAT-DEC-068`) |
| `approved_at` | 8 Oktober 2026 — revision `1`; keputusan review final 8 Oktober 2026 — revision `2`. Revision `3`: 15 Oktober 2026 (sesi ketiga). Revision `4` dan `5`: 16 Oktober 2026. Revision `6`: 19 Oktober 2026 |
| `decision_revision` | `00-interview-decisions.md` revision `6` (`approved`); riwayat `5`. Bagian 1–5 = revision `2` approved |
| `input_revision_hash` | `sha256:fcd87b5a644ab01cb7673d5729293c8999df28142f859d87210ea2d7d75a7e25` (`00-interview-decisions.md` revision `6` + bukti runtime bagian 16, 22 Oktober 2026). Riwayat: `sha256:963fe276718d29f45d5dab496ef3d9f0a0cdbc62a59d846b25ea67c9de595888` (revision `6`, 19 Oktober 2026); `sha256:601d73246c27df9d7512c496ea3e07cbc4308e01a64d0d035694d3bf836de43a` (`00-interview-decisions.md` revision `5`, 16 Oktober 2026); `requirement-completeness-assessment.md` revision `1` + catatan amandemen `PAT-DEC-067`/`068`: `sha256:94de971d1358f4788c4974b84e9db4a12ed962897f86ce33558b6b725ff8fdd2`. Revision `4`: `sha256:f7862a87…9422`. Revision `3`: `sha256:d97421ee…2076`. Amandemen kedua: `sha256:6f2bc745…517b`. Amandemen pertama: `sha256:54a4ca40…858b`. Draft 14 Oktober 2026: `sha256:624f27ad…8376`. Revision `2`: `sha256:fa0138e5…2885` |
| `contract_versions` | API **`1.3.0`** — **`approved`** 19 Oktober 2026 (`PAT-DEC-069`–`077`): guard jalur bayi B14.4, pemeriksa B14.2 dua jalur (aditif), amandemen worker B4.2/B4.3/B10/B15; `sha256:0f0ee3c525473b2ffa0c787d107164fd37ed187c213eaf7d599e112ac75928eb` (sinkronisasi bukti runtime 22 Oktober 2026, tanpa perubahan versi maupun bentuk API: nilai `PATIENT_BASELINE = 741` dan rumusnya, B9 sesuai `PAT-DEC-075`, label versi B2, contoh B4.2, traceability B13; sebelumnya ~~`b1f842b7d81a9a86bc9e32dd66d188ef31b9450ff0f2a4d589ccab647a083539`~~ — klarifikasi non-API B15: `F-14` 19 Oktober 2026 dan eksekusi serial 20 Oktober 2026; sebelumnya ~~`267ef45f8c20927abeeb470b42258d9dccddc6d59aad070ce9bb1354cef41d86`~~, ~~`dacec65928c0cc7bfed0e9c9f830df5be4696e8f1c9dd2561fae59e533f850c2`~~). Riwayat `1.2.2` — **`approved`** 16 Oktober 2026 (`PAT-DEC-068`): kunci serialisasi allocator `pg_advisory_xact_lock(1346458697, 1313821261)`, syarat endpoint pemeriksa dikonfirmasi; `sha256:d27ab079810bee29b4f83b566cb6d657d8ffbe80146f11a7cc93b95de84f1519`. `1.2.1` (`PAT-DEC-067`) dan `1.2.0` (`PAT-DEC-033`) sebelumnya. Bagian A tetap `1.1.0` (`approved` 8 Oktober 2026, `BE-PAT-MIG-001`) |
| `capability_scope` | Revision `3`: `BE-PAT-MIG-002` — RSMMC Resumable Bulk Patient Migration Worker, Canary-A saja; dan task prasyarat `BE-PAT-MIG-003` — RSMMC Normal Patient Identifier Reservation Guard (`PAT-DEC-057`). Revision `6`: task prasyarat `BE-PAT-MIG-003A` — Newborn Identifier Reservation Guard (`PAT-DEC-069`) |
| `requirement_readiness` | **`READY_FOR_DOMAIN_DESIGN`** untuk kedua slice — `requirement-completeness-gate` `PASS` 15 Oktober 2026 (`PAT-RCG-001`): `BE-PAT-MIG-003` lalu `BE-PAT-MIG-002`. 27 `OWNER_APPROVED`, 3 `SUPERSEDED`, 0 `BLOCKING`, 0 `PROPOSED`; ketiga isu P0 diputuskan. Usulan standar `PAT-GAP-002`–`016` (`NON_BLOCKING_STANDARD`) dikunci lewat `PAT-DEC-033`. *Riwayat amandemen kedua: belum terkunci, 4 `BLOCKING`; gate tidak dijalankan* |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — alasan pada `00-interview-decisions.md` bagian 6 |
| `active_dependency_ids` | `BE-PAT-MIG-002` → `BE-PAT-MIG-001` ✅, `BE-PAT-MIG-003` ✅, `BE-PAT-MIG-003A` ✅, gerbang `{PAT-GATE-005 generasi 2}` ✅ `PASS` 22 Oktober 2026 (urutan wajib pemilik, `PAT-DEC-069`) — seluruh dependency build terpenuhi. `BE-PAT-MIG-003A` → `BE-PAT-MIG-003` ✅. `BE-PAT-MIG-003` → tidak ada. Runtime: `PAT-GATE-005` generasi 2 sebelum `PAT-GATE-002`. *Revision `5`: `BE-PAT-MIG-003` 🟡, gerbang `{PAT-GATE-005}`* |
| `active_roadmap_revision` | `6` (`approved`) untuk backend; `2` (`approved`, tanpa task) untuk frontend |
| `supersedes` | `null` |

---

## Untuk apa blueprint ini ada

Blueprint ini dibuat **hanya** supaya `BE-PAT-MIG-001` punya roadmap, kontrak, dan traceability
yang sah sebelum diimplementasikan lewat `quilvian-engineering-skills:build-module-backend`.
Isinya adalah keputusan pengguna 8 Oktober 2026, dicatat apa adanya, ditambah bukti source yang
dibaca read-only pada snapshot di atas.

Blueprint ini **tidak** merancang modul Patient Management. Pendaftaran pasien, penggabungan
pasien, foto, kiosk, dan kemampuan pasien lain tidak dibahas, dan tidak boleh disimpulkan dari
berkas-berkas di sini.

**Revision `3`** menambah work item kedua, `BE-PAT-MIG-002` — RSMMC Resumable Bulk Patient
Migration Worker — atas instruksi tertulis pemilik 14 Oktober 2026, ditambah task prasyarat
`BE-PAT-MIG-003` — RSMMC Normal Patient Identifier Reservation Guard (`PAT-DEC-057`). Desain
keduanya disetujui 15 Oktober 2026 (`PAT-DEC-033`). Persetujuan itu tidak memberi wewenang
*commit*, deployment, eksekusi database, pemanggilan endpoint, atau runtime Canary-A. Seluruh
keputusan, kontrak, kartu, dan laporan `BE-PAT-MIG-001` dipertahankan.

---

## Gerbang desain

### `BE-PAT-MIG-001`

| Gerbang | Keadaan | Yang tertahan bila belum terpenuhi |
| --- | --- | --- |
| Penempatan modul dan prefix | **Terpenuhi** — `PAT-DEC-001`; registry `ACTIVE` | — |
| Task, acceptance criteria, dan batas wewenang disetujui | **Terpenuhi** 8 Oktober 2026 — `PAT-DEC-003`, `PAT-DEC-012` | — |
| Kontrak API dikunci | **Terpenuhi** — `1.1.0`, hash di bawah | — |
| Butir `PROPOSED` dikonfirmasi | **Terpenuhi** 8 Oktober 2026 — `PAT-OQ-001` sampai `PAT-OQ-004` final, ditambah `PAT-OQ-005` sampai `PAT-OQ-007`. Pertanyaan terbuka: **0** | Tidak ada. `PAT-OQ-001` tetap bersyarat pada pemeriksaan `TEST_POLICY` saat build (governance guard) |
| Otorisasi eksekusi runtime | ~~**Terbuka** — `PAT-GATE-001`~~ → **`CLOSED/PASS`**, dilaporkan pemilik 14 Oktober 2026; belum diverifikasi agent (`PAT-STALE-002`) | — |

### `BE-PAT-MIG-002` dan `BE-PAT-MIG-003` — revision `3`

| Gerbang | Keadaan | Yang tertahan bila belum terpenuhi |
| --- | --- | --- |
| Penempatan modul dan prefix | **Terpenuhi** — sama dengan `BE-PAT-MIG-001`; tidak ada entity persisted baru | — |
| Batasan pemilik dicatat | **Terpenuhi** — `PAT-DEC-017`–`PAT-DEC-032` | — |
| Keputusan pemilik 15 Oktober 2026 | **Tercatat** — sesi pertama `PAT-DEC-034`–`043`; sesi kedua `PAT-DEC-044`–`062`; sesi ketiga `PAT-DEC-063`–`066` dan `PAT-DEC-033`. 27 pertanyaan `OWNER_APPROVED`; `PAT-OQ-015`, `017`, `020` `SUPERSEDED` | — |
| Keputusan `BLOCKING` dikonfirmasi | **Terpenuhi** 15 Oktober 2026, sesi ketiga — `PAT-OQ-019` (`PAT-DEC-063`), `PAT-OQ-031` (`PAT-DEC-064`), `PAT-OQ-036` (`PAT-DEC-065`), `PAT-OQ-037` (`PAT-DEC-066`). Sisa: 0 | — |
| Isu desain P0 terselesaikan | **Terpenuhi** — `PAT-P0-001` (`PAT-DEC-052`), `PAT-P0-002` (`PAT-DEC-054`, `066`), `PAT-P0-003` (`PAT-DEC-064`, `066`) | — |
| Data read-only dari pemilik | **Terpenuhi** — `PAT-FACT-079`–`089` (`00-interview-decisions.md` bagian 7.8 dan 13.2) | — |
| Kelengkapan requirement | **`PASS`** — `requirement-completeness-assessment.md` (`PAT-RCG-001`): kedua slice `READY_FOR_DOMAIN_DESIGN` | — |
| Kontrak API dikunci | **Terpenuhi** — `1.2.0` `approved`, hash di bawah | — |
| Task, acceptance criteria, dan batas wewenang disetujui | **Terpenuhi** 15 Oktober 2026 — `PAT-DEC-033`; `PAT-MIG3-AC-01`–`14`, `PAT-MIG2-AC-01`–`51` | — |
| Otorisasi runtime Canary-A | **Belum dapat diminta — `NOT RUN`** (keadaan 22 Oktober 2026: `PAT-GATE-005` generasi 2 `PASS` dan baseline 741 sudah tersedia; source `BE-PAT-MIG-002` belum ada) — `PAT-GATE-002`; prasyaratnya kini mencakup `PAT-GATE-005` `PASS`, manifest `RSMMC-CANARY-A` v1, bukti pembekuan yang menutup ketiga jalur pendaftaran (`PAT-SRC-027`), inventaris filesystem read-only, dan bukti provider-real PostgreSQL 15 sekali pakai (`PAT-DEC-065`) | Deployment, simulasi, dan eksekusi Canary-A. Tidak menahan desain |
| Canary-B dan gelombang besar | **Dicadangkan** — `PAT-GATE-003`, `PAT-GATE-004` | Tidak menahan Canary-A |
| Task prasyarat `BE-PAT-MIG-003A` (revision `6`) | **✅ Terpenuhi** 22 Oktober 2026 — di-*merge* PR #294 (`ce34b22b`), dideploy lewat artefak `fb2099a9`, `PAT-GATE-005` generasi 2 `PASS` (`PAT-FACT-092`, `093`) ([`task/report/backend/BE-PAT-MIG-003A.md`](task/report/backend/BE-PAT-MIG-003A.md)) | — |
| Baseline `PATIENT_BASELINE` (`PAT-DEC-077`) | **Terpenuhi** 22 Oktober 2026 — 741, diukur read-only sesudah generasi 2 `PASS`; `PAT-RSK-042` 0 baris (`PAT-FACT-094`, `095`) | — |
| Project test `PatientManagementTests` dapat dikompilasi (F-01) | **Terpenuhi** — PR #300, Integration `8bccaa03` (`PAT-FACT-097`) | — |
| Task prasyarat `BE-PAT-MIG-003` | **✅ Terpenuhi** 19 Oktober 2026 — dideploy, `PAT-GATE-005` generasi 1 `PASS` (`PAT-FACT-090`). *Keadaan 17 Oktober 2026 — jejak:* **🟡 Siap review pemilik** 17 Oktober 2026 — source, build, test SQLite, QBE `Strict`, dan uji PostgreSQL 15.15 provider-real (10/10) lulus; review source leader belum ([`task/report/backend/BE-PAT-MIG-003.md`](task/report/backend/BE-PAT-MIG-003.md)). Runtime lewat `PAT-GATE-005`, yang juga prasyarat build `BE-PAT-MIG-002` | Build `BE-PAT-MIG-002`, runtime Canary-A |

---

## Hash artefak

Dihitung 8 Oktober 2026 dengan `sha256sum` terhadap berkas apa adanya di disk. Repository ini
memakai `core.autocrlf`, sehingga verifikasi wajib memakai cara yang sama atau menyebut cara lain
yang dipakai.

| Artefak | SHA-256 |
| --- | --- |
| `00-interview-decisions.md` | ~~`909bacd918dec0c202f59d19039599d4dcd0eac329da4234ea9923b6f4f08be7`~~ (revision `1`) → ~~`fa0138e5e1c7c98adaf32119564d84675452b37531b137f16b0b1cd36f332885`~~ (revision `2`) → ~~`624f27ad82f39e4b1af22155c86c2ccde75069f1d6d8bc1f1fd963c6d7b88376`~~ (revision `3`, draft 14 Oktober 2026) → ~~`54a4ca403a8244bb4c6362a213eb9ba9ff91b836ff4d1e92150dc3e33c26858b`~~ (revision `3`, draft amandemen pertama) → ~~`6f2bc745519d061ce7960381a2e79b0612c88999ca2166e4f3e4475b3ba8517b`~~ (revision `3`, draft amandemen kedua) → ~~`d97421ee2d3f69468253f6590d533ed81c9e5e3b06a542eb9ff203bc03f52076`~~ (revision `3` `approved`) → ~~`f7862a87fe8919cd3417991c30fbb6233ce1d557151a9e5a5c3752dc536f9422`~~ (revision `4`) → ~~`601d73246c27df9d7512c496ea3e07cbc4308e01a64d0d035694d3bf836de43a`~~ (revision `5` `approved` — dihitung 16 Oktober 2026) → ~~`963fe276718d29f45d5dab496ef3d9f0a0cdbc62a59d846b25ea67c9de595888`~~ (revision `6`, 19 Oktober 2026) → **`fcd87b5a644ab01cb7673d5729293c8999df28142f859d87210ea2d7d75a7e25`** (revision `6` + bukti runtime bagian 16 — dihitung 22 Oktober 2026) |
| `contracts/api-contract.md` | ~~`765a3b6ac5da8c03343ab1402baf37ff37928551c3590c03beb46b1dc40a9f39`~~ (`1.0.0`) → ~~`cb41e4d5a0f200bb8b234ad4e741413c1ba96837140fa38460a2a78dd710ad7e`~~ (`1.1.0`) → ~~`690b242904afd3dc12a31b0d98ecb8821e4bf8fb9dc926cf5062b594e97ba1ce`~~ (`1.1.0` + usulan `1.2.0` draft 14 Oktober 2026) → ~~`346a49faa5802f61b11acf10edd41effda83af37344cb42368ab2860deb47ed0`~~ (`1.1.0` + usulan `1.2.0` draft amandemen pertama) → ~~`bb29a1c8edc98fd323edf416c411557c130f1aab4435218d510bbc84cfd704f9`~~ (`1.1.0` + usulan `1.2.0` draft amandemen kedua) → ~~`339ca508120dd3909c6132ff835ea738b5250826c442fd10754cedcda669e230`~~ (`1.2.0` `approved`) → ~~`1d4ef4f1780ae6b4fd4a9ed409e3685640a7b7321a0f98a6b574b372ba4e817d`~~ (`1.2.1`) → ~~`d27ab079810bee29b4f83b566cb6d657d8ffbe80146f11a7cc93b95de84f1519`~~ (`1.2.2` `approved` — dihitung 16 Oktober 2026) → ~~`dacec65928c0cc7bfed0e9c9f830df5be4696e8f1c9dd2561fae59e533f850c2`~~ (`1.3.0`, 19 Oktober 2026) → ~~`267ef45f8c20927abeeb470b42258d9dccddc6d59aad070ce9bb1354cef41d86`~~ (`1.3.0` + klarifikasi non-API B15 — prasyarat server bukti `F-14`, 19 Oktober 2026) → ~~`b1f842b7d81a9a86bc9e32dd66d188ef31b9450ff0f2a4d589ccab647a083539`~~ (`1.3.0` + klarifikasi non-API B15 — eksekusi serial suite provider-real, 20 Oktober 2026) → **`0f0ee3c525473b2ffa0c787d107164fd37ed187c213eaf7d599e112ac75928eb`** (`1.3.0` + sinkronisasi bukti runtime — dihitung 22 Oktober 2026) |
| `requirement-completeness-assessment.md` | ~~`eb58c8add3337330f204033da89efcf6c838ae8d0f0beae46ad5d257d381ddfa`~~ (revision `1`) → ~~`8d64f1ec1605bf7f9c36a630e1945d67c872af7320040471df9dff48e359fe18`~~ (catatan `PAT-DEC-067`) → **`94de971d1358f4788c4974b84e9db4a12ed962897f86ce33558b6b725ff8fdd2`** (catatan `PAT-DEC-067`/`068` — dihitung 16 Oktober 2026) |

Hash lama dicoret, bukan dihapus, supaya jejak perubahannya tetap terbaca. Hash revision `2`
tetap sah sebagai bukti masukan yang dipakai `BE-PAT-MIG-001` saat dibangun; revision `3` hanya
menambah bagian baru dan mengubah baris kepala dokumen, tanpa mengubah isi keputusan bagian 1–5
maupun kontrak bagian 1–9.

Bila salah satunya berubah, roadmap dan traceability menjadi **stale** dan wajib ditinjau ulang
sebelum dipakai. Hash wajib dihitung ulang pada perubahan yang sama, bukan belakangan. Pada saat
`PAT-DEC-033` disetujui, kedua hash revision `3` wajib dihitung ulang bila isinya disunting — dilakukan 15 Oktober 2026, sesi ketiga.

---

## Struktur keluaran

Berkas yang dibuat pada revision `1`, beserta alasan masing-masing wajib ada:

```text
docs/module-blueprints/patient-management/
├── blueprint-manifest.md          # jangkar identitas, revision, status, SHA, dan hash
├── 00-interview-decisions.md      # keputusan berID yang dirujuk roadmap dan traceability
├── requirement-completeness-assessment.md  # hasil gate kelengkapan BE-PAT-MIG-003 dan BE-PAT-MIG-002 (revision 3)
├── contracts/
│   └── api-contract.md            # versi kontrak yang wajib dibawa setiap task
└── roadmap/
    ├── backend-roadmap.md         # kartu BE-PAT-MIG-001, BE-PAT-MIG-002, BE-PAT-MIG-003
    ├── frontend-roadmap.md        # keluaran wajib plan-module-delivery; isinya mencatat nol task
    └── requirement-traceability.md
```

Jalur laporan task dicadangkan, tetapi **tidak dibuat** sekarang:
*(Keadaan 16 Oktober 2026: `task/report/backend/BE-PAT-MIG-001.md` dan `BE-PAT-MIG-003.md` sudah ditulis `build-module-backend`; `BE-PAT-MIG-002.md` dicadangkan.)*
`task/report/backend/BE-PAT-MIG-001.md`. Laporan itu milik `build-module-backend`, ditulis
sesudah build dan validasi. Skill perencanaan tidak menulis ke `task/report/**`, dan folder
kosong tidak dibuat untuk lapisan yang belum punya laporan.

### `PAT-GAP-001` — berkas pasti `SINGLE` yang belum dibuat

Aturan bentuk blueprint mewajibkan 14 berkas pada bentuk `SINGLE`. Revision `1` memuat 3 di
antaranya. 11 berkas berikut **sengaja tidak dibuat**, karena pengguna meminta paket minimum dan
melarang mengarang requirement di luar keputusannya:

| Berkas | Pemilik normalnya | Bagian yang sudah tertampung di revision `1` |
| --- | --- | --- |
| `01-existing-capability-map.md` | `trace-existing-capabilities` | Kolom Reuse dan Larangan pakai ulang pada kartu task |
| `02-backend-architecture.md` | `design-business-module` | Bagian Cakupan dan Risiko pada kartu task |
| `03-frontend-architecture.md` | `design-business-module` | `PAT-DEC-015` — tanpa layar |
| `04-prd-to-mvp.md` | `design-business-module` | Slice S1 dan DoD pada roadmap |
| `flowcharts/00-alur-utama.md` | `design-business-module` | `api-contract.md` bagian 4 |
| `data/data-dictionary.md` | `design-business-module` | `api-contract.md` bagian 8 dan 9 |
| `contracts/state-transition-matrix.md` | `design-business-module` | `api-contract.md` bagian 4.3 |
| `contracts/validation-matrix.md` | `design-business-module` | `api-contract.md` bagian 2.1 |
| `contracts/integration-contract.md` | `design-business-module` | `api-contract.md` bagian 5 dan 8 |
| `contracts/permission-audit-matrix.md` | `design-business-module` | `api-contract.md` bagian 7 |
| `testing/acceptance-test-matrix.md` | `design-business-module` | `AC-27` dan tabel Verifikasi pada kartu task |

Gap ini tidak menahan `BE-PAT-MIG-001`. Bila kelak modul Patient Management dirancang utuh,
berkas-berkas di atas dibuat lewat skill pemiliknya, dan revision manifest naik.

**Revision `3`.** Satu berkas baru pada finalisasi sesi ketiga: `requirement-completeness-assessment.md`, keluaran `requirement-completeness-gate`. Paket minimum diteruskan untuk `BE-PAT-MIG-002`
dan `BE-PAT-MIG-003`: keputusan pada `00-interview-decisions.md` bagian 6–14; arsitektur, alur proses, pemetaan data,
hak akses, dan log pada `contracts/api-contract.md` Bagian B; reuse, rencana berkas, acceptance
criteria, dan risiko pada kartu task. Laporan `task/report/backend/BE-PAT-MIG-001.md` kini ada
(ditulis `build-module-backend`); laporan `BE-PAT-MIG-002.md` dicadangkan dan belum dibuat.

---

## Pemicu impact scan

Blueprint ini menjadi **stale** bila salah satu berkas berikut berubah sebelum implementasi
dimulai:

| Berkas | Bagian yang terdampak |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | Route, tag, izin, logika QR, pembangkit MRN dan `PatientCode` — `api-contract.md` bagian 2, 5, 7, 9 |
| `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs` | Field yang boleh dan tidak boleh berubah — bagian 9 |
| `Models/IdentityModel.cs` | Field audit — bagian 9 |
| `Services/Security/AccessPermissionService.cs` | Contoh pola environment yang dilarang ditiru — bagian 6 |
| `Repositories/ApplicationDbContext.cs` | Pemetaan tabel `migration.rsmmc_*` — risiko `PAT-RSK-001` |
| `Tests/QuilvianSystemBackend.PharmacyTests/QuilvianSystemBackend.PharmacyTests.csproj` | Konvensi project test yang diikuti `PAT-OQ-001` |
| `.github/workflows/generate-migration-artifact.yml` | Bukti ejaan nama environment `Staging` — `PAT-OQ-002` |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderNumberService.cs`, `Areas/HealthServices/RegistrationManagement/Services/KioskPatientLookupService.cs` | Contoh mekanisme baca berparameter — `PAT-OQ-007` |

Bila commit `QuilvianStaDeploy` sudah bergeser dari `103b45cc` saat build dimulai, agent build
memeriksa ulang kutipan baris pada `api-contract.md` lebih dulu, dan mencatat selisihnya.

### Pemicu impact scan `BE-PAT-MIG-002`

Snapshot: `QuilvianIntegrationBackend` @ `a52b3c98`. Desain Bagian B menjadi **stale** bila salah
satu berkas berikut berubah sebelum build dimulai:

| Berkas | Bagian yang terdampak |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs` | Route, tag, izin, factory QR, `GetFileStoragePaths`, pembangkit normal — Bagian B2, B6, B8; `PAT-SRC-002`–`006`, `015`, `016` |
| `Areas/HealthServices/PatientManagement/MasterData/Services/RsmmcPilot*.cs` | Komponen yang dipakai ulang atau dilarang dipakai ulang — kartu task |
| `Areas/HealthServices/PatientManagement/MasterData/Services/PatientQrPayloadBuilder.cs` | Isi QR — Bagian B6 |
| `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs`, `Repositories/Configurations/HealthServices/MstPatientConfiguration.cs`, `Models/IdentityModel.cs` | Pemetaan B10, indeks unik B5, default `IsMember`/`IsActive` (`PAT-SRC-007`, `008`) |
| `Repositories/ApplicationDbContext.cs` | Efek samping `SaveChanges`, pemetaan tabel `migration.*` (`PAT-SRC-017`) |
| `Areas/Corporate/AccountingManagement/JournalManagement/Services/AccJournalService.cs` | Pola kunci database (`PAT-SRC-014`) |
| `Tests/QuilvianSystemBackend.PatientManagementTests/**` | Lokasi test wajib (`PAT-DEC-037`) |
| `Program.cs` (penyajian `/uploads`, registrasi seeder) dan `Areas/HealthServices/OperatingRoomManagement/Seeders/OperatingRoomDemoSeeder.cs` | Path QR strategi B6; gerbang `PATIENT_BASELINE` (`PAT-SRC-021`, `023`) |
| `Areas/HealthServices/PatientManagement/MasterData/DTOs/PatientDtos.cs` | Bawaan `IsMember` pendaftaran normal (`PAT-SRC-024`) |
| Frontend `kiosk-new-patient-step-print-card.jsx`, `kiosk-old-patient-registration.service.js`, `kiosk-patient-card-print-utils.jsx`, `base-patient-card.jsx` | Bukti bahwa konsumen memakai `qrCodePath` tersimpan (`PAT-SRC-020`) |
| `Attributes/AccessActionAttribute.cs`, `Attributes/AccessPermissionAttribute.cs`, policy `KioskRead` di `Program.cs`, dan atribut `CreatePatient`/`CreatePatientForAdmin` di `PatientController.cs` | Cakupan pembekuan pendaftaran dan dampak `BE-PAT-MIG-003` (`PAT-SRC-027`, kontrak B14.1) |
| Pemakai advisory lock dua kunci baru di mana pun | Keunikan key1 worker `1173286842` (`PAT-SRC-026`, `PAT-RSK-035`) |

Bila HEAD build berbeda dari `a52b3c98`, agent build memeriksa ulang kutipan baris pada
`00-interview-decisions.md` bagian 9 dan kontrak Bagian B lebih dulu, lalu mencatat selisihnya.

---

## Temuan basi (*stale*) pada impact scan 14 Oktober 2026

Basi berarti catatan di blueprint tidak lagi sama dengan keadaan sebenarnya. Temuan ini dicatat,
bukan diperbaiki diam-diam.

| ID | Temuan | Bukti | Dampak dan tindakan |
| --- | --- | --- | --- |
| `PAT-STALE-001` | Snapshot dan kutipan baris revision `2` merujuk `QuilvianStaDeploy` @ `103b45cc`, padahal source `BE-PAT-MIG-001` kini berada di Integration (`ef3adbe4`; HEAD `a52b3c98`). Contoh pergeseran: `[Route]` 42 → 43, `[Tags]` 52 → 53, `GenerateMedicalRecordNumberAsync` 1403 → 1478, `GeneratePatientCodeAsync` 2468 → 2520 | `PatientController.cs` pada `a52b3c98`; laporan `BE-PAT-MIG-001` bagian 9.5 | Tidak menahan apa pun. Kontrak Bagian A dibiarkan sebagai bukti sejarah; Bagian B memakai baris `a52b3c98` |
| `PAT-STALE-002` | Blueprint dan laporan masih mencatat `PAT-GATE-001` **terbuka** dan runtime `PENDING`, sedangkan pemilik melaporkan rekonsiliasi Pilot `CLOSED/PASS`. Bukti runtime per requirement (15 baris `NANTI` pada traceability bagian 2) belum diserahkan | Instruksi pemilik 14 Oktober 2026; `requirement-traceability.md` bagian 2; laporan `BE-PAT-MIG-001` bagian 10.2 | Status gerbang dicatat sebagai dilaporkan pemilik. Laporan `task/report/**` milik build skill, sehingga **tidak** disunting di sini. Tindakan: pemilik menyerahkan bukti runtime, lalu sesi berwenang memperbarui laporan dan kolom Runtime |
| `PAT-STALE-003` | Laporan `BE-PAT-MIG-001` bagian 10.3 menjanjikan SHA commit lokal dan hasil QBE `Strict` GitRange dicatat pada sesi berikutnya; keduanya belum tercatat. `git log` menunjukkan commit `ef3adbe4` | `git log` pada `a52b3c98` | Tidak menahan `BE-PAT-MIG-002`. Dicatat untuk sesi build berwenang berikutnya |
| `PAT-STALE-004` | `PAT-DEC-016` dan `AC-23` menyatakan 704.508 pasien di luar cakupan. Pernyataan itu tetap benar **untuk `BE-PAT-MIG-001`**, tetapi kini ada task yang menargetkan mereka | `00-interview-decisions.md` bagian 2 dan 6 | Tidak diubah, karena cakupannya per task. Pembaca diarahkan ke `BE-PAT-MIG-002` |
| `PAT-STALE-005` | `PAT-RSK-007` masih berlaku dan bertambah: `TEST_POLICY` menyebut `Tests/` sudah dihapus, padahal kini ada 4 project test di `Tests/` | `Tests/` pada `a52b3c98` | Selisih governance untuk pemilik suite Skill; tidak diubah di sini |
| `PAT-STALE-006` | Kontrak `1.1.0` bagian 1 menyebut endpoint Pilot dirancang untuk dihapus atau dinonaktifkan sesudah rekonsiliasi, dan keputusannya belum ada. Pilot kini dilaporkan selesai | `api-contract.md` bagian 1 | Di luar cakupan `BE-PAT-MIG-002`; keputusan terpisah pemilik |
| `PAT-STALE-007` | Draft revision `3` tanggal 14 Oktober 2026 menyiratkan pasien preview dengan `is_active = false` perlu ditangani, dan menyebut daftar 25 sebagai manifest beku | Bukti Step 16 (`PAT-FACT-074`); keputusan `PAT-DEC-040`, `PAT-DEC-041` | Dikoreksi pada draft yang sama dan dicatat di `00-interview-decisions.md` bagian 14 |
| `PAT-STALE-008` | Draft amandemen pertama 15 Oktober 2026 memuat: urutan `CANARY_A` naik secara angka; sidik jari himpunan; kunci `hashtext` satu kunci; gerbang `NORMAL_ALLOCATION_GUARD` yang lulus dengan pembekuan saja; serta contoh pembekuan lewat izin `Patient : Create` | Keputusan `PAT-DEC-045`, `048`, `055`; temuan `PAT-SRC-027` | Dikoreksi pada amandemen kedua dan dicatat di `00-interview-decisions.md` bagian 14 |
| `PAT-STALE-009` | Desain dan kontrak `BE-PAT-MIG-002` (impact scan `a52b3c98`) mengandalkan `PAT-SRC-008`: nilai `IsMember = false` hilang saat insert EF. Pada Integration `0c6e3145`, `MstPatientConfiguration` memakai `HasDefaultValue(true).HasSentinel(true)` dan `MstPatient.IsMember` ber-default CLR `false` (`BE-CRM-015`); `CreatePatient` kini menulis `IsMember = false` | `PAT-SRC-030`; `git diff a52b3c98 0c6e3145` | Tidak menahan `BE-PAT-MIG-003`. Desain `BE-PAT-MIG-002` — langkah `PERSIST_IS_MEMBER`, `PAT-DEC-053`, `PAT-MIG2-AC-47`, snapshot source kontrak Bagian B — wajib di-*impact scan* sebelum build-nya. `HasSentinel(true)` tidak boleh dibatalkan. **Ditutup 19 Oktober 2026:** impact scan membuktikan INSERT mengirim `IsMember = false` (`PAT-SRC-032`); `PERSIST_IS_MEMBER` dihapus (`PAT-DEC-075`), kontrak `1.3.0` |
| `PAT-STALE-010` | Status `BE-PAT-MIG-003` 🟡 "siap review pemilik" dan `PAT-GATE-005` "belum dapat diminta" pada roadmap dan manifest revision `5`; pernyataan `PAT-SRC-023` / kontrak B14.1 bahwa seluruh insert `MstPatient` bermuara pada `CreatePatient` | Impact scan `BE-PAT-MIG-002` 19 Oktober 2026; bukti runtime pemilik `PAT-FACT-090`; `PAT-SRC-031` | **Ditutup** revision `6`: `BE-PAT-MIG-003` ✅, `PAT-GATE-005` generasi 1 `PASS`; jalur bayi dicatat dan dilindungi lewat `BE-PAT-MIG-003A` |
| `PAT-STALE-011` | Sebelum 22 Oktober 2026, roadmap, manifest, kontrak, dan traceability masih mencatat `BE-PAT-MIG-003A` 🟡 pra-runtime, `PAT-GATE-005` generasi 2 "belum dapat diminta", pemeriksaan `PAT-RSK-042` dan baseline "menunggu", serta `BE-PAT-MIG-002` ⛔. Selain itu, baris B9 `public."MstPatient"` dan AC-47 masih mewajibkan UPDATE `IsMember = false`; label versi B2 masih `1.2.0`, kartu dan tabel task `BE-PAT-MIG-002` masih `1.2.2`; contoh B4.2 masih mengandaikan pembangkit normal memberi `PAT-RSMMC-00742` | Bukti runtime pemilik `PAT-FACT-092`–`095`; impact scan final `PAT-FACT-096`; perbaikan F-01 `PAT-FACT-097`; `PAT-DEC-075` | **Ditutup** 22 Oktober 2026 oleh sinkronisasi dokumen ini. Koreksinya dicatat di `00-interview-decisions.md` bagian 14; teks lama dipertahankan sebagai jejak |
| `PAT-STALE-012` | Laporan [`task/report/backend/BE-PAT-MIG-003A.md`](task/report/backend/BE-PAT-MIG-003A.md) ditulis sebelum merge. Baris Status masih 🟡 pra-runtime; `PAT-MIG3A-AC-12` dan `PAT-GATE-005` generasi 2 masih "Belum — menunggu merge dan deploy Staging"; langkah berikutnya masih menyebut merge, deploy, generasi 2, `PAT-RSK-042`, dan pengukuran baseline | `PAT-FACT-092`–`095` | **Terbuka.** Tidak menahan `BE-PAT-MIG-002`: register status roadmap dan traceability adalah sumber keadaan terkini, dan syarat ⛔ pada laporan itu sudah terpenuhi. Laporan `task/report/**` milik `build-module-backend`, sehingga **tidak** disunting di sini (sama dengan `PAT-STALE-002`). Tindakan: sesi build berwenang berikutnya — misalnya build `BE-PAT-MIG-002` — menambah adendum runtime *append-only*, seperti adendum bagian 10 laporan `BE-PAT-MIG-003` |

---

## Riwayat revisi

| Revision | Tanggal | Perubahan | Berkas yang berubah |
| ---: | --- | --- | --- |
| `1` | 8 Oktober 2026 | Paket minimum dibuat. `PAT-OQ-001`–`004` berstatus `PROPOSED` dengan default aman | Seluruh enam berkas dibuat |
| `2` | 8 Oktober 2026 | Pemilik memfinalkan `PAT-OQ-001`–`004` dan menambah `PAT-OQ-005`–`007`. Kontrak naik `1.0.0` → `1.1.0`: nama status konflik QR dikunci `CONFLICT`, pemeriksaan QR sebelum helper, pencocokan `Staging` persis, default `dryRun=true`, dan larangan EF migration dipertegas. Bentuk request dan route **tidak berubah** | `00-interview-decisions.md`, `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, `roadmap/frontend-roadmap.md` (hanya `blueprint_revision`), manifest ini |
| `3` (draft) | 14 Oktober 2026 | Usulan `BE-PAT-MIG-002` — RSMMC Resumable Bulk Patient Migration Worker, lewat `manage-module-blueprint` → `grill-me` (*amendment pass*) → `design-business-module` → `plan-module-delivery`, seluruhnya `draft`. Fakta pemilik `PAT-FACT-030`–`064`; batasan pemilik `PAT-DEC-017`–`032`; persetujuan desain `PAT-DEC-033` (draft); fakta source `PAT-SRC-001`–`018`; usulan `PAT-OQ-008`–`026`; manifest Canary-A beku 25 `legacy_pid`; gerbang `PAT-GATE-002`–`004`; kontrak usulan `1.2.0` Bagian B; kartu ⛔ dengan `PAT-MIG2-AC-01`–`42`; risiko `PAT-RSK-009`–`025`; temuan basi `PAT-STALE-001`–`006`. Status `PAT-GATE-001` dicatat `CLOSED/PASS` sebagaimana dilaporkan pemilik. Isi `BE-PAT-MIG-001` tidak berubah | `00-interview-decisions.md`, `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, `roadmap/frontend-roadmap.md`, manifest ini |
| `3` (draft, amandemen) | 15 Oktober 2026 | Bukti database Step 16 (`PAT-FACT-070`–`075`, inferensi `PAT-INF-001`) dan keputusan pemilik `PAT-DEC-034`–`043` dimasukkan. Register isu P0 `PAT-P0-001`–`003`. Bagian 10 ditulis ulang dalam bentuk seragam: `PAT-OQ-008`, `009`, `011`, `022` `OWNER_APPROVED`; `PAT-OQ-015`, `017`, `020` `SUPERSEDED`; pertanyaan baru `PAT-OQ-027`–`035`. Daftar 25 direklasifikasi menjadi CANARY-A CANDIDATE MANIFEST — DRAFT beserta 10 tabrakan. Impact scan konsumen QR dan jalur insert pasien (`PAT-SRC-019`–`025`). Koreksi: `IsActive = false` bukan kebutuhan bulk. Kontrak Bagian B: path QR strategi, langkah `PERSIST_IS_MEMBER`, gerbang `NORMAL_ALLOCATION_GUARD`, bagian B14. Kartu: AC `PAT-MIG2-AC-43`–`48`, risiko `PAT-RSK-026`–`031`, test wajib. `requirement-completeness-gate` tidak dijalankan | `00-interview-decisions.md`, `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, `roadmap/frontend-roadmap.md`, manifest ini |
| `3` (draft, amandemen kedua) | 15 Oktober 2026 | Keputusan pemilik `PAT-DEC-044`–`062` (19 pertanyaan `OWNER_APPROVED`); dua blocker baru `PAT-OQ-036` (bukti provider-real) dan `PAT-OQ-037` (cakupan environment `BE-PAT-MIG-003`); fakta pernyataan pemilik `PAT-FACT-076`–`078`; temuan source `PAT-SRC-026` (keyspace advisory lock) dan `PAT-SRC-027` (jalur kiosk tidak dijaga izin `Patient : Create`); urutan manifest dan sidik jari berurutan `c43207eb…79ad`; kunci dua bagian; kolom crosswalk dari preview; task prasyarat usulan `BE-PAT-MIG-003` dengan `PAT-MIG3-AC-01`–`12` dan gerbang `PAT-GATE-005`; `PAT-MIG2-AC-44`/`45` dipindah, `AC-49`/`50` ditambah; risiko `PAT-RSK-032`–`035`. `requirement-completeness-gate` tidak dijalankan | `00-interview-decisions.md`, `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, `roadmap/frontend-roadmap.md`, manifest ini |
| `3` (**`approved`**, sesi ketiga) | 15 Oktober 2026 | Bukti read-only final pemilik `PAT-FACT-079`–`089` (skema fisik, bukti MRN, rentang `legacy_pid`, keutuhan Canary-A, riwayat `00-00-00-27`; `PAT-INF-001` terkonfirmasi); keputusan `PAT-DEC-063` (SQL bertipe), `064` (lantai MRN 890.188), `065` (PostgreSQL 15 sekali pakai), `066` (guard hanya `Staging` persis dan `Enabled == true`); status 27 `OWNER_APPROVED` / 3 `SUPERSEDED` / 0 `BLOCKING`; temuan source `PAT-SRC-028`, `029`; `requirement-completeness-gate` `PASS` untuk kedua task dengan usulan standar `PAT-GAP-002`–`016`; persetujuan desain `PAT-DEC-033`. Kontrak `1.2.0` `approved`: gerbang `PREVIEW_SCHEMA_SIGNATURE`, B9.1 tipe SQL, B10 perlakuan NULL/enum, B14–B14.3 lantai MRN dan aturan environment/konfigurasi, B15 bukti provider-real; koreksi urutan `items[]`. Kartu dibuka: `PAT-MIG2-AC-51`, `PAT-MIG3-AC-13`, `14`; `AC-46`, `47`, `PAT-MIG3-AC-06`–`08`, `10`, `12` diperbarui; risiko `PAT-RSK-036`–`038`; grafik baru dengan gerbang `{PAT-GATE-005}`. Manifest Canary-A `RSMMC-CANARY-A` v1 berisi final. Riwayat `BE-PAT-MIG-001` dan seluruh butir `SUPERSEDED` dipertahankan | `00-interview-decisions.md`, `requirement-completeness-assessment.md` (baru), `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, `roadmap/frontend-roadmap.md`, manifest ini |
| `4` (**`approved`**) | 16 Oktober 2026 | Amandemen pemilik sebelum build `BE-PAT-MIG-003` (`PAT-DEC-067`): `Enabled = false` di Staging persis gagal tertutup seperti konfigurasi hilang/salah bentuk; endpoint pemeriksa baca-saja khusus Staging `GET /admin/migration/rsmmc-reservation-guard` dengan `Patient : Update` dan nama field instruksi pemilik. Kontrak `1.2.0` → `1.2.1`; `PAT-MIG3-AC-07`, `08`, `12`, `14`, `PAT-MIG2-AC-46`, `PAT-RSK-036` diperbarui; `PAT-GAP-002`/`008` diberi catatan amandemen. Tidak ada pertanyaan baru. Build `BE-PAT-MIG-003` 🟡 pada hari yang sama | `00-interview-decisions.md`, `requirement-completeness-assessment.md`, `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, `roadmap/frontend-roadmap.md`, manifest ini; laporan baru `task/report/backend/BE-PAT-MIG-003.md` |
| `5` (**`approved`**) | 16 Oktober 2026 | *Source reconciliation / review fix pass* `BE-PAT-MIG-003`: baseline source disinkronkan dari `a52b3c98` ke Integration `0c6e3145` (fast-forward 41 commit, dijalankan pemilik). `PAT-DEC-068`: kunci allocator dua bagian tetap `pg_advisory_xact_lock(1346458697, 1313821261)` menggantikan `hashtext`, endpoint pemeriksa dikonfirmasi dengan syarat baca-saja. Kontrak `1.2.1` → `1.2.2`. Temuan `PAT-SRC-030` dan `PAT-STALE-009` (`IsMember` `HasSentinel(true)` upstream, dampak ke `BE-PAT-MIG-002`). `.gitignore` diberi pengecualian sempit `Tests/QuilvianSystemBackend.PatientManagementTests/` | `00-interview-decisions.md`, `requirement-completeness-assessment.md`, `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, `roadmap/frontend-roadmap.md`, manifest ini, laporan `BE-PAT-MIG-003.md` |
| `6` (**`approved`**) | 19 Oktober 2026 | Impact scan read-only `BE-PAT-MIG-002` pada Integration `c6537ee4`: `BLOCKED` oleh jalur pendaftaran bayi upstream (`PAT-SRC-031`). Instruksi pemilik `TASK: BE-PAT-MIG-003A`: keputusan `PAT-DEC-069`–`077`; fakta `PAT-FACT-090`, `091`; sumber `PAT-SRC-031`–`033`. Kontrak `1.2.2` → `1.3.0` (B14.2 aditif, B14.4 baru, B4.2/B4.3/B10/B15). Kartu task `BE-PAT-MIG-003A` ditambahkan; `BE-PAT-MIG-003` ✅. `PAT-STALE-009` dan `PAT-STALE-010` ditutup. Build `BE-PAT-MIG-003A` pada sesi yang sama | `00-interview-decisions.md`, `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, manifest ini, laporan `BE-PAT-MIG-003.md` (adendum runtime), laporan baru `BE-PAT-MIG-003A.md` |
| `6` (koreksi review final) | 19 Oktober 2026 | Review final read-only pemilik atas `6ae9c251` (rebase ke Integration `98fdc69f`): temuan `F-07`, `F-13`, `F-14`. Koreksi: `AllocateReservedAsync` menolak guard buatan di luar `Staging` persis sebelum SQL; bukti provider-real pemilik `23`/`23` `PASS` dicatat; prasyarat server bukti `max_locks_per_transaction = 2048` (hanya server uji sekali pakai) dicatat di kontrak B15 tanpa perubahan versi — hash kontrak dihitung ulang. Revision blueprint tetap `6` | `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, manifest ini, laporan `BE-PAT-MIG-003A.md` |
| `6` (closeout bukti) | 20 Oktober 2026 | Kandidat `6e72ee6c` di atas Integration `724629ab`. Bukti akhir pemilik: build Release `0` error, EF tanpa perubahan, F-07 `17`/`17`, non-provider `206`/`206`, provider-real smoke `1`/`1` dan akhir `23`/`23` **serial** (6193,9 detik, 0 `pat_proof_*`). Run paralel `21`/`23` dicatat sebagai temuan operasional harness. Kontrak B15: eksekusi serial suite provider-real pada server bukti sekali pakai (klarifikasi non-API, versi tetap `1.3.0`; hash dihitung ulang). Komentar cara menjalankan di test provider-real diselaraskan; tanpa perubahan logika | `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, manifest ini, laporan `BE-PAT-MIG-003A.md` |
| `6` (sinkronisasi bukti runtime) | 22 Oktober 2026 | Sesi `MODULE BLUEPRINT` di atas Integration `8bccaa03`. Bukti runtime pemilik dicatat sebagai `PAT-FACT-092` (merge dan deploy `BE-PAT-MIG-003A`, migration Staging), `093` (`PAT-GATE-005` generasi 2 `PASS`), `094` (`PAT-RSK-042` 0 baris), `095` (`PATIENT_BASELINE = 741`); impact scan final `096`; perbaikan F-01 `097`. `BE-PAT-MIG-003A` ✅; `BE-PAT-MIG-002` tidak lagi ⛔ — boleh masuk implementasi source. Teks diselaraskan dengan `PAT-DEC-075`–`077` yang sudah `approved`: rumus `PATIENT_BASELINE` lengkap dengan nilai 741, B9 dan AC-47 tanpa UPDATE `IsMember`, label versi `1.3.0`, contoh B4.2. `PAT-RSK-042` ditutup; `PAT-STALE-011` ditutup, `PAT-STALE-012` dibuka. Tanpa keputusan baru, tanpa kenaikan revision blueprint maupun versi kontrak; hash dihitung ulang. Laporan `task/report/**` tidak disunting | `00-interview-decisions.md`, `contracts/api-contract.md`, `roadmap/backend-roadmap.md`, `roadmap/requirement-traceability.md`, `roadmap/frontend-roadmap.md` (hanya `blueprint_revision`), manifest ini |

---

## Langkah berikutnya

1. ~~Pengguna mereview paket ini lalu mengonfirmasi `PAT-OQ-001`–`004`~~ — **selesai** 8 Oktober
   2026 pada revision `2`.
2. ~~`BE-PAT-MIG-001` diimplementasikan lewat `quilvian-engineering-skills:build-module-backend`
   dalam sesi `TASK MODE: BACKEND` tersendiri.~~ — **selesai** 9 Oktober 2026 (✅ source).
3. ~~Sesudah review source, build, dan test lulus, pemilik memutuskan `PAT-GATE-001` secara
   terpisah.~~ — dilaporkan pemilik `CLOSED/PASS` 14 Oktober 2026 (`PAT-STALE-002`).
4. ~~Pemilik menjawab `PAT-OQ-008`–`PAT-OQ-026` dan menyerahkan data read-only `PAT-OQ-017` serta
   `PAT-OQ-019`.~~ — sebagian dijawab 15 Oktober 2026 (`PAT-DEC-034`–`043`, `PAT-FACT-070`–`075`).
4a. ~~Segera, tindakan operasional pemilik: bekukan pendaftaran pasien normal di Staging~~ — dinyatakan
    pemilik berlaku 15 Oktober 2026 (`PAT-DEC-055`, `PAT-FACT-078`). **Catatan agent:** pembekuan wajib
    menutup `POST /`, `POST /kiosk`, **dan** `POST /admin`; pencabutan izin `Patient : Create` saja hanya
    menutup jalur admin (`PAT-SRC-027`).
4b. ~~Pemilik memutuskan 16 butir `BLOCKING`~~ — 12 diputuskan pada amandemen kedua 15 Oktober 2026.
5. ~~Pemilik menyerahkan paket bukti read-only final `PAT-OQ-019` dan bukti MRN `PAT-OQ-031`, serta
   memutuskan `PAT-OQ-036` dan `PAT-OQ-037`.~~ — **selesai** 15 Oktober 2026, sesi ketiga.
6. ~~Agent memasukkan bukti itu dan menyiapkan `PAT-DEC-033`; `requirement-completeness-gate`, lalu
   `design-business-module` dan `plan-module-delivery`.~~ — **selesai** 15 Oktober 2026, sesi ketiga;
   gate `PASS`, `PAT-DEC-033` `approved`.
7. Urutan wajib pemilik (`00-interview-decisions.md` bagian 13.3), setiap langkah dengan wewenangnya
   sendiri:
   1. `BE-PAT-MIG-003` — sesi `TASK MODE: BACKEND` tersendiri lewat `build-module-backend`;
   2. uji PostgreSQL provider-real (PostgreSQL 15 sekali pakai);
   3. build, QBE, dan review source;
   4. deploy dan verifikasi guard di Staging — `PAT-GATE-005`;
   5. `BE-PAT-MIG-002` — sesi `TASK MODE: BACKEND` tersendiri lewat `build-module-backend`;
   6. test, build, QBE, dan review source;
   7. deploy ke Staging;
   8. inventaris filesystem read-only;
   9. simulasi Canary-A `dryRun=true`;
   10. otorisasi runtime pemilik — `PAT-GATE-002` (b);
   11. eksekusi Canary-A.

   **Disisipkan revision `6` (`PAT-DEC-069`)** sesudah langkah 4: 4a. `BE-PAT-MIG-003A` lewat
   `build-module-backend`; 4b. uji provider-real, review, dan deploy ke Staging; 4c. `PAT-GATE-005`
   generasi 2; 4d. pengukuran baseline `MstPatient` read-only (`PAT-DEC-077`). Langkah 5 baru boleh
   dimulai sesudah 4c `PASS`.

**Keadaan 22 Oktober 2026:** langkah 1–4 dan sisipan 4a–4d selesai. `BE-PAT-MIG-003A` ✅ — di-*merge*
lewat PR #294 dan dideploy lewat artefak `fb2099a9` (`PAT-FACT-092`); `PAT-GATE-005` generasi 2 `PASS`
(`PAT-FACT-093`); `PAT-RSK-042` 0 baris (`PAT-FACT-094`); baseline `MstPatient` diukur read-only = **741**
(`PAT-FACT-095`); F-01 `RESOLVED` (`PAT-FACT-097`). **Langkah 5 — implementasi source `BE-PAT-MIG-002` —
boleh dimulai** dalam sesi `TASK MODE: BACKEND` tersendiri lewat `build-module-backend`. Langkah 6–11
belum berjalan: `PAT-GATE-002` dan `RSMMC-CANARY-A` v1 `NOT RUN`. Pembekuan pembuatan pasien Staging —
normal, admin, kiosk, dan bayi — tetap berlaku sampai jendela Canary-A selesai (`PAT-DEC-073`). Eksekusi
massal **tidak** diberi wewenang. Sebelum artefak `BE-PAT-MIG-002` dideploy ke Staging, migration
Integration yang lebih baru dari artefak `fb2099a9` wajib diterapkan dengan otorisasi terpisah
(`PAT-FACT-096`), lalu pemeriksa `PAT-GATE-005` dijalankan ulang.

*Keadaan 20 Oktober 2026 — jejak:* `BE-PAT-MIG-003A` 🟡 **siap finalisasi lokal — pra-runtime**: kandidat `6e72ee6c` di atas Integration `724629ab`; uji PostgreSQL 15 provider-real **akhir `23`/`23` PASS** — dijalankan **serial** (`xUnit.ParallelizeTestCollections=false xUnit.MaxParallelThreads=1`), 0 gagal, 0 dilewati, 6193,9 detik ≈ 1 jam 43 menit; smoke `1`/`1`; 0 basis data `pat_proof_*` tersisa; kontainer `quilvian-pg-proof-mig3a` dan image `postgres:15` dihapus, tunnel SSH ditutup, variabel environment bukti dihapus; tanpa basis data DEV/STAGING/PRODUCTION (dilaporkan pemilik 20 Oktober 2026, kandidat `6e72ee6c` di atas Integration `724629ab`; server bukti `max_locks_per_transaction = 2048`, hanya server uji). Langkah 4a dan 4b selesai; 4c (`PAT-GATE-005` generasi 2) menunggu merge dan deploy Staging. Langkah 5 (`BE-PAT-MIG-002`) **belum boleh dimulai**.

*Keadaan 19 Oktober 2026 — jejak:* langkah 1–4 selesai untuk `BE-PAT-MIG-003` (✅; `PAT-GATE-005` generasi 1 `PASS`, `PAT-FACT-090`). Impact scan `BE-PAT-MIG-002` `BLOCKED`; langkah 4a dan bukti provider-real 4b selesai — `BE-PAT-MIG-003A` 🟡 siap review PR pemilik: build Release `0` error, uji PostgreSQL 15 provider-real `23`/`23` PASS — smoke `1`/`1`, 0 gagal, 0 dilewati, 4259,7 detik, 0 basis data `pat_proof_*` tersisa, kontainer dan image `postgres:15` sekali pakai dihapus (dijalankan pemilik 19 Oktober 2026 pada commit `6ae9c251`; server bukti `max_locks_per_transaction = 2048`, hanya server uji); koreksi review final `F-07`/`F-13`/`F-14` diterapkan, test SQLite `206` lulus / `0` gagal / `16` dilewati (17 kasus koreksi `F-07`), QBE `Strict` `PASS`; uji ulang 9 kasus provider-real terdampak `F-07` diminta ([laporan](task/report/backend/BE-PAT-MIG-003A.md)). Langkah 5 (`BE-PAT-MIG-002`) **belum boleh dimulai**.

*Keadaan sebelumnya — jejak:* **Keadaan 17 Oktober 2026:** langkah 1, 2, dan bagian build langkah 3 selesai di atas Integration `0c6e3145` — `BE-PAT-MIG-003` 🟡 siap review pemilik; uji PostgreSQL 15.15 provider-real 10/10 PASS ([`task/report/backend/BE-PAT-MIG-003.md`](task/report/backend/BE-PAT-MIG-003.md)). Berikutnya: review source leader, keputusan *commit*, lalu `PAT-GATE-005`. Impact scan `BE-PAT-MIG-002` wajib sebelum build-nya (`PAT-STALE-009`).

**RUNTIME PREREQUISITES PASSED — BE-PAT-MIG-002 MAY ENTER SOURCE IMPLEMENTATION**

*Penutup 20 Oktober 2026 — jejak: "BE-PAT-MIG-003A SOURCE BUILT — BE-PAT-MIG-002 REMAINS BLOCKED".*

*Penutup revision `5` — jejak: "DESIGN COMPLETE — SOURCE IMPLEMENTATION MAY BEGIN WITH BE-PAT-MIG-003".*

*Penutup lama — jejak: "WAITING FOR FINAL READ-ONLY EVIDENCE — PAT-OQ-019 / PAT-OQ-031".*
