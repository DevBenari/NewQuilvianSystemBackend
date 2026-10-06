# IGD Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `IGD-BP-001` |
| `revision` | **`9`** — naik 6 Oktober 2026 lewat desain Ruang Kerja Dokter IGD (`EPIC IGD-14`, `IGD-DEC-220`…`229`, bagian 0n); bagian barunya `approved` lewat `IGD-DEC-230`, blueprint secara keseluruhan tetap `draft`. *Sebelumnya* **`8`** — naik 23 September 2026 lewat amendment penutupan kunjungan lewat disposisi (`IGD-DEC-163`…`169`, bagian 0i); seluruh isinya `draft`. *Sebelumnya* **`7`** — naik 22 September 2026 (penutup) lewat `IGD-DEC-157`: bagian encounter-first kelima kontrak `approved`. *Sebelumnya `6`* |
| `status` | `draft` — blueprint secara keseluruhan **belum** disetujui; irisan tertentu disetujui (lihat `approved_by`; terbaru `IGD-DEC-230`, 6 Oktober 2026 sore; sebelumnya `IGD-DEC-209`, 5 Oktober 2026 sore; sebelumnya `IGD-DEC-199`, 5 Oktober 2026). *Tulisan lama: "tidak disetujui siapa pun".* Revision `4` yang berstatus `approved sebagian` tetap berlaku sebagai baseline sampai revisi ini disetujui |
| `module` | `igd` / `EmergencyInstallationManagement`, prefix entity **`Emg`** sejak 27 Agustus 2026 (17 tabel `Emg*`; nol `Trx*`/`Mst*` milik IGD tersisa). Prefix lama `TrxEmergency`/`MstEmergency` hanya berlaku pada artefak desain yang disusun sebelum tanggal itu |
| `registry_lifecycle` | `ACTIVE` |
| `design_snapshot_at` | `2026-08-26` (revisi 6); revisi 5 pada `2026-08-24` |
| `backend_commit_sha` | `300922c` (branch `rizkiG`) — merge Hamzah/Ikbal/Yasmina. Revisi 5 disusun pada `f69e9e483052845d11c91d8b7bbdce33c4acc8d8` |
| `frontend_commit_sha` | `96a9120111f6acc6b7c0f37973ea0c717ba41f17` (branch `RizkiV2`) |
| `status_check_sha` | **6 Oktober 2026 (sore, desain)** — backend `43dab6da` (`rizkiG`), frontend `6680278a2` (`RizkiV2`), keduanya sejajar origin; impact scan capability map suplemen 3.4. *Sebelumnya* **5 Oktober 2026 (sore, sinkronisasi roadmap)** — backend `8d81d361` (`rizkiG`), frontend `553501053` (`RizkiV2`, *pull* pemilik 15.04 WIB); impact scan baca-saja bagian 0m.1 — nol berkas IGD berubah sejak `57b1d360f`. *Sebelumnya* **5 Oktober 2026** — backend `8d81d361` (`rizkiG`), frontend `57b1d360f` (`RizkiV2`); impact scan baca-saja pass desain bagian 0l — area IGD tidak berubah sejak `c1f79f79`. *Sebelumnya* **4 Oktober 2026** — backend `c1f79f79` (`rizkiG`), frontend `19ba512de` (`RizkiV2`) + working tree `FE-IGD-042`; `verify-module-readiness` `MVP-8` → `READY_WITH_CONDITIONS` ([evidence/2026-10-04-kesiapan-mvp-8.md](evidence/2026-10-04-kesiapan-mvp-8.md)). *Sebelumnya* **15 September 2026** — backend `e89907c5` (`rizkiG`), frontend `43adae648` (`RizkiV2`). SHA desain di atas **tidak diganti**; ini hanya tempat pemeriksaan status terakhir dijalankan. Lihat [evidence/2026-09-15-pemeriksaan-status.md](evidence/2026-09-15-pemeriksaan-status.md) |
| `owners` | Product/Domain: **Rizki Gunawan**, ditetapkan `IGD-DEC-089` 2026-08-24. Pemegang modul Registration Management: **ada, dalam tim Rizki Gunawan** — menyetujui titik sentuh `IGD-REQ-002` secara lisan (`IGD-DEC-181`, 3 Oktober 2026); nama tidak dicatat atas keputusan pemilik. Clinical Governance, Nursing authority, Security/Privacy: `OPEN`. Pemilik `ClinicalManagement` dan `PharmacyManagement`: **belum ditunjuk** — digantikan sementara Product/Domain Owner IGD (`IGD-DEC-107`). Pemilik `LaboratoryManagement` dan `RadiologyManagement`: **Yoga Aji Pratama** (tercatat pada blueprint masing-masing; Radiologi lewat `RAD-DEC-014`, 10 September 2026) |
| `approved_by` | Sebagian: **`IGD-DEC-230`** pada 6 Oktober 2026 sore (Ruang Kerja Dokter IGD, bagian 0n; sementara, pola `IGD-DEC-174`); **`IGD-DEC-209`** pada 5 Oktober 2026 sore (pesanan tanpa sikap dan kepergian yang dibatalkan, bagian 0m; sementara, pola `IGD-DEC-174`); **`IGD-DEC-199`** pada 5 Oktober 2026 (kewenangan unit, bagian 0l; sementara, pola `IGD-DEC-174`; keputusan masukannya `IGD-DEC-190`…`198` pada hari yang sama); **`IGD-DEC-186`** pada 3 Oktober 2026 (observasi Dieskalasi dan kunjungan yang sudah berakhir, bagian 0k; sementara, pola `IGD-DEC-174`); **`IGD-DEC-181`** pada 3 Oktober 2026 (persetujuan Registrasi, bagian 0j.4); **Rizki Gunawan** menyetujui `IGD-DEC-067`, `IGD-DEC-088`, `IGD-DEC-089`, dan `IGD-DEC-093` pada 24 Agustus 2026; dan **`IGD-DEC-157`**…**`162`** pada 22 September 2026 (bagian encounter-first API §8, validation §10, state §8, integration §5, permission/audit §7; koreksi B1/B2/B4; keterbatasan B3; `IGD-OQ-104`…`107`); **`IGD-DEC-170`** pada 23 September 2026 (penutupan lewat disposisi, bagian 0i); **`IGD-DEC-175`** pada 30 September 2026 (observasi sesudah disposisi dilaksanakan, bagian 0j; sementara, `IGD-DEC-174`). Blueprint secara keseluruhan **belum** disetujui |
| `approved_at` | Sebagian: `2026-10-06` (Ruang Kerja Dokter IGD, `IGD-DEC-230`); `2026-08-24`; `2026-09-22` (encounter-first); `2026-09-23` (penutupan lewat disposisi, `IGD-DEC-170`); `2026-09-30` (observasi sesudah disposisi dilaksanakan, `IGD-DEC-175`); `2026-10-03` (observasi Dieskalasi, `IGD-DEC-186`); `2026-10-05` (kewenangan unit, `IGD-DEC-199`; pesanan tanpa sikap, `IGD-DEC-209`) |
| `blueprint_shape` | **`SINGLE`** — `shape_decided_by: USER_CONFIRMED` (Rizki Gunawan, 22 September 2026, sesudah uji pemecahan: hanya rumpun Kepergian yang lolos 3/5; rumpun Pendaftaran & episode 2/5, Triage & penanganan 2/5, Dokter penanggung jawab 1/5). Rincian di bagian 0g |
| `struktur_berkas` | **Utang struktur dicatat**: kamus data tetap di `erd/data-dictionary.md` dan ERD lama tetap di `erd/` (kontrak output terbaru meminta `data/` dan `flowcharts/`). `flowcharts/` **ditambahkan** 22 September 2026 untuk alur encounter-first. Migrasi `erd/` → `data/` adalah pekerjaan terpisah (keputusan pemilik 22 September 2026) |
| `requirement_readiness` | **Slice encounter-first: `PARTIALLY_READY`** — sub-slice `S1`…`S6`, `S8` `READY_FOR_DOMAIN_DESIGN`; `S7` (kelayakan dokter jaga) `BUSINESS_DECISION_REQUIRED` (`IGD-OQ-102`, `103`) — [evidence/02-requirement-completeness-gate.md](evidence/02-requirement-completeness-gate.md) `0.1`, 22 September 2026. **Area IGD lain: `UNCLASSIFIED`** — lihat bagian 0 |
| `domain_architecture_revision` | **Tidak ada** — lihat bagian 0 |
| `domain_architecture_readiness` | Slice encounter-first: **`DOMAIN_ARCHITECTURE_NOT_RUN`** — slice di dalam bounded context IGD yang sudah ada; kepemilikan lintas modul sudah diputuskan pemilik (`IGD-DEC-144`, `145`, `151`, `153`); alasan lengkap di gate §10. Area lain: **`NOT_ASSESSED`** |
| `input_revisions` | `00-interview-decisions.md` **230 keputusan**, terakhir `IGD-DEC-230` (6 Oktober 2026 sore — approval desain Ruang Kerja Dokter IGD; bagian 0n); capability map suplemen **3.4**. *Sebelumnya* **209 keputusan**, terakhir `IGD-DEC-209` (5 Oktober 2026 sore — approval amandemen pesanan tanpa sikap; bagian 0m). *Sebelumnya* **208 keputusan**, terakhir `IGD-DEC-208` (5 Oktober 2026 sore, perencanaan — amendment pass `grill-me` sempit `IGD-DEC-204`…`208`, `IGD-FACT-053`…`058`, `IGD-OQ-116` dijawab; bagian 0l.3). *Sebelumnya* **203 keputusan**, terakhir `IGD-DEC-203` (5 Oktober 2026 sore — bukti uji `BE-IGD-039` `IGD-DEC-200`, `201`; kartu frontend pasangan `IGD-DEC-202`; `IGD-CONFLICT-007` → `IGD-DEC-203`; `IGD-FACT-049`…`052`; `IGD-OQ-116`; bagian 0l.2). *Sebelumnya* **199 keputusan**, terakhir `IGD-DEC-199` (5 Oktober 2026 — pass C3 `IGD-DEC-190`…`192`, `IGD-FACT-037`…`043`, `IGD-OQ-114`, `IGD-OQ-113` ditutup; pass C2 `IGD-DEC-193`…`198`, `IGD-FACT-044`…`048`, `IGD-CONFLICT-006`, `IGD-OQ-115`; approval kontrak `IGD-DEC-199`; bagian 0l). *Sebelumnya* **189 keputusan**, terakhir `IGD-DEC-189` (4 Oktober 2026 — izin Perawat IGD dan bukti uji gabungan `MVP-8` disahkan `IGD-DEC-188`; sandi SuperAdmin tidak diganti `IGD-DEC-189`; `IGD-OQ-113`; bagian 0k.2). *Sebelumnya* **187 keputusan**, terakhir `IGD-DEC-187` (3 Oktober 2026 — amendment pass observasi Dieskalasi `IGD-DEC-183`…`185`, approval kontrak `IGD-DEC-186`, layar tab Observasi `IGD-DEC-187`; fakta `IGD-FACT-032`…`036`; asumsi `IGD-ASM-003`; `IGD-OQ-112`; bagian 0k). *Sebelumnya* **182 keputusan**, terakhir `IGD-DEC-182` (3 Oktober 2026 — rute petugas loket IGD, catatan API §8.1; bagian 0j.5). *Sebelumnya* **181 keputusan**, terakhir `IGD-DEC-181` (3 Oktober 2026 — persetujuan Registrasi atas `IGD-REQ-002`, PRD §8.6 butir 8; bagian 0j.4). *Sebelumnya* **180 keputusan**, terakhir `IGD-DEC-180` (3 Oktober 2026 — jawaban `IGD-OQ-108`: kontrak berlaku, satu kunjungan berjalan per pasien; bagian 0j.4). *Sebelumnya* **179 keputusan**, terakhir `IGD-DEC-179` (1 Oktober 2026 — lima ruas kunjungan opsional pada `start-triage`, bagian 0j.3; sebelumnya `IGD-DEC-178`, 1 Oktober 2026 — batas alasan NoShow 250, bagian 0j.2; sebelumnya `IGD-DEC-177`, 30 September 2026 — keputusan perencanaan bagian 0j.1; sebelumnya `IGD-DEC-175` approval amandemen bagian 0j); masukan desainnya `IGD-DEC-171`…`174` (observasi yang diakhiri sesudah disposisi dilaksanakan; fakta `IGD-FACT-029`…`031`; `IGD-OQ-111`). *Sebelumnya* **169 keputusan**, terakhir `IGD-DEC-169` (23 September 2026 — penutupan kunjungan lewat disposisi); capability map suplemen **3.3** @ `dce1f138`. *Sebelumnya* **162 keputusan**, terakhir `IGD-DEC-162` (22 September 2026 penutup — approval kontrak encounter-first `IGD-DEC-157`, keterbatasan izin bersama `IGD-DEC-158`, jawaban `IGD-OQ-104`…`107` `IGD-DEC-159`…`162`; pertanyaan baru `IGD-OQ-108`); sebelumnya 156 sampai `IGD-DEC-156` (22 September 2026 — bentuk `SINGLE` dan susunan `erd/` dipertahankan, gerbang `design-business-module`); sebelumnya 154 sampai `IGD-DEC-154` (22 September 2026 larut — penutup impact scan `IGD-TRQ-08`…`11`: `IGD-DEC-151`…`154` `approved`; `IGD-DEC-140` U1 dan `IGD-DEC-149` `superseded`; fakta `IGD-FACT-021`…`023`); sebelumnya 150 sampai `IGD-DEC-150` (22 September 2026 sore — amendment pass `grill-me` kasus tepi: `IGD-DEC-142`…`150` `approved`; fakta `IGD-FACT-011`…`020`; asumsi `IGD-ASM-001`/`002`; `IGD-OQ-093` `superseded` sebagian; `IGD-OQ-094`/`095`/`096`/`097`/`100`/`101` `superseded`); sebelumnya 141 sampai `IGD-DEC-141` (22 September 2026 — encounter-first `IGD-DEC-139`, pasien tanpa identitas `IGD-DEC-140`, kelayakan dokter jaga `IGD-DEC-141`; ketiganya `approved` **prinsip**; `IGD-OQ-093` ditutup; `IGD-OQ-094`…`101` dibuka; bukti `IGD-EV-140`…`144`); sebelumnya 138 sampai `IGD-DEC-138` (21 September 2026); sebelumnya 134 sampai `IGD-DEC-134` (16 September 2026 keempat, tata letak riwayat pada ruang kerja pemeriksaan — murni tampilan, nol dampak kontrak); sebelumnya 132 sampai `IGD-DEC-132` (16 September 2026 ketiga, kesiapan `EPIC IGD-04`); sebelumnya 128 sampai `IGD-DEC-128` (16 September 2026 kedua, kunjungan keluar dari `Arrived`); sebelumnya 126 sampai `IGD-DEC-126` (16 September 2026, pemantauan observasi); sebelumnya 121 sampai `IGD-DEC-121` (15 September 2026, perencanaan delivery), 115 sampai `IGD-DEC-115` pada pemeriksaan status hari yang sama, dan 105 sampai `IGD-DEC-105` saat revisi 6 disusun; `01-existing-capability-map.md` revision `3` + **suplemen `3.1`** (audit terarah `EmergencyTransfer` pada `300922c`) + **suplemen `3.2`** (impact scan encounter-first pada `0d13f3a8`/`c941012ac`, 22 September 2026; area lain revisi 3 tetap stale) |
| `delivery_state` | **Per 15 September 2026, dipetakan ulang ke source:** `MVP-1`, `MVP-2`, dan R3.7 ✅; `MVP-0` 🟡 (`BE-IGD-017` tanpa laporan tracked); `MVP-3` 🟡 (`BE-IGD-026`); `MVP-4` 🟡 (`BE-IGD-031`); `MVP-5` 🟡 (`BE-IGD-035`, `EPIC IGD-04` tanpa task); `MVP-6` ⛔ (`BE-IGD-039`). Backend 18 task ✅, 4 🟡, 1 ⛔; frontend 5 ✅, 5 🟡, 1 belum dikerjakan. Seluruhnya sudah di-commit. **Sesudah perencanaan 15 September 2026 (kedua):** backend ditambah 6 task direncanakan (`BE-IGD-040`…`045`, satu ⛔ `BE-IGD-042`); frontend ditambah kartu susulan `FE-IGD-019` ✅ dan 5 task direncanakan (`FE-IGD-023`…`027`) — lihat bagian 0b.5. **Sesudah perencanaan 16 September 2026 (kedua):** frontend ditambah 2 task direncanakan, `FE-IGD-029` dan `FE-IGD-030`, yang memulihkan jalan keluar kunjungan dari `Arrived` — tanpanya modul IGD tidak dapat dipakai untuk pasien baru (`IGD-EV-131`…`IGD-EV-136`, `IGD-DEC-127`, `IGD-DEC-128`). Nol perubahan kontrak, nol task backend. Rincian di [MODULE-STATUS.md](MODULE-STATUS.md). *Keadaan lama (26 Agustus): "`MVP-0` berjalan, `BE-IGD-017`…`020` selesai, belum di-commit"* |
| `amendment` | **2026-10-06 (sore) — Ruang Kerja Dokter IGD** (`design-business-module`) — revisi **`9`**, bagian 0n `approved` (`IGD-DEC-230`); API `0.15.0`, validation `0.14.0`, state `0.10.0`, permission/audit `0.8.0`, integration `0.6.0`; nol migration; lihat bagian 0n. **2026-10-05 (sore) — pesanan tanpa sikap dan kepergian yang dibatalkan** (`design-business-module`) — revisi **tetap `8`**; validation `0.13.0`, state `0.9.0`, API §1.2 diselaraskan tanpa kenaikan versi; lihat bagian 0m. **2026-10-05 — kewenangan unit `BE-IGD-039`** (`design-business-module`) — revisi **tetap `8`**; validation `0.12.0`, permission/audit `0.7.0`; lihat bagian 0l. **2026-10-03 — observasi Dieskalasi dan kunjungan yang sudah berakhir** (`design-business-module`) — revisi **tetap `8`**; API `0.14.0`, validation `0.11.0`, state `0.8.0`; lihat bagian 0k. **2026-09-30 — observasi yang diakhiri sesudah disposisi dilaksanakan** (`design-business-module`) — revisi **tetap `8`**; API `0.13.0`, validation `0.10.0`, state `0.7.0`; lihat bagian 0j. **2026-09-23 — penutupan kunjungan lewat disposisi** (`design-business-module`) — revisi naik ke `8`; lihat bagian 0i. **2026-09-22 (penutup) — approval + `plan-module-delivery` final** — revisi naik ke `7`; lihat bagian 0h. **2026-09-22 (encounter-first)** — `IGD-DEC-139`…`141` dan task R3.13/R3.12 ditulis sebagai **rencana**; revisi blueprint **tetap `6`** karena arsitektur target baru disetujui prinsip dan kontrak belum diselaraskan (lihat `koreksi_desain_tertunda`). Bila penyelarasan kontrak dikerjakan, perubahan titik lahir `EmgVisit` dan penulisan status encounter oleh IGD adalah perubahan material yang layak menaikkan revisi. **2026-08-24 (kedua)** — `IGD-OQ-068`/`070`/`071` ditutup. **2026-08-26 (correction pass revisi 6)** — enam butir, lihat 0a.2 sampai 0a.4. **2026-09-15 (pemeriksaan status)** — `IGD-DEC-110`…`115`, `IGD-DEC-099` digantikan `IGD-DEC-111`; lihat bagian 0b |
| `contract_versions` | **Per 6 Oktober 2026 sore (Ruang Kerja Dokter IGD, bagian 0n, Rencana, `approved` `IGD-DEC-230`):** API **`0.15.0`**, validation **`0.14.0`**, state **`0.10.0`**, permission/audit **`0.8.0`**, integration **`0.6.0`**. **Per 5 Oktober 2026 sore (pesanan tanpa sikap, bagian 0m, Rencana, `approved` `IGD-DEC-209`):** API **`0.14.0`** (§1.2 diselaraskan tanpa kenaikan versi); validation **`0.13.0`** (§5, §5.1, §6, §6.1 — bukan aditif murni); state **`0.9.0`** (§6a.2, §9.2 — aditif pada matriks); permission/audit **`0.7.0`** dan integration **`0.5.0`** tidak berubah. *Sebelumnya* **Per 5 Oktober 2026 (kewenangan unit, bagian 0l, Rencana, `approved` `IGD-DEC-199`):** API **`0.14.0`** dan state **`0.8.0`** tidak berubah; validation **`0.12.0`** (§7 — bukan aditif murni); permission/audit **`0.7.0`** (catatan §3, §3.1, baris §6); integration **`0.5.0`** (satu baris §3 diselaraskan tanpa kenaikan versi). *Sebelumnya* **Per 3 Oktober 2026 (observasi Dieskalasi dan kunjungan yang sudah berakhir, bagian 0k, Rencana, `approved` `IGD-DEC-186`):** API **`0.14.0`** (§9.1 nomor 6–8, §9.2, §9.4 — bukan aditif murni), validation **`0.11.0`** (§6 aturan 2, §11.2 — bukan aditif murni), state **`0.8.0`** (§9.6 — aditif pada matriks), permission/audit **`0.6.0`** dan integration **`0.5.0`** tidak berubah. *Sebelumnya* **Per 30 September 2026 (observasi sesudah disposisi dilaksanakan, bagian 0j, Rencana):** API **`0.13.0`** (§9.1 nomor 5, §9.4 — bukan aditif murni), validation **`0.10.0`** (§11.1 — bukan aditif murni), state **`0.7.0`** (§9.5 — aditif pada matriks), permission/audit **`0.6.0`** dan integration **`0.5.0`** tidak berubah. *Sebelumnya* **Per 23 September 2026 (penutupan lewat disposisi, seluruhnya `draft` dan Rencana):** API **`0.12.0`**, validation **`0.9.0`**, state **`0.6.0`**, permission/audit **`0.6.0`**, integration **`0.5.0`** — seluruhnya aditif kecuali satu penolakan baru pada pembatalan disposisi (API bagian 9.1 nomor 3). *Sebelumnya* **Per 22 September 2026 (encounter-first, `design-business-module`, seluruhnya `draft` dan Rencana):** API **`0.11.0`** — **bukan aditif murni** (penjaga + tanpa-antrean pada `POST /patient-encounters` Emergency, penolakan `PATCH …/status`/`…/cancel` Registrasi untuk Emergency, penguncian `PUT /emergency-visits/{id}`, efek samping penutupan encounter; endpoint baru `triage-queue`, `start-triage`, `no-show`, `{id}/arrival-time`, grup `Emergency Encounter Reconciliation`); validation **`0.8.0`** — bukan aditif murni (sumber aturan episode berganti ke klausa A+B; bagian 10 baru); state **`0.5.0`** aditif (bagian 8); integration **`0.4.0`** (bagian 5; memutus perilaku bagi Registrasi); permission/audit **`0.5.0`** aditif (bagian 7: `EmergencyVisit : NoShow`, resource `EmergencyEncounterReconciliation`). Rincian di bagian 0g. *Sebelumnya — per 21 September 2026 (`BE-IGD-048`):* API **`0.8.0`** — *relaxed nullability change for legacy response*, bukan aditif murni: `assignedByUserId` pada §3.2 boleh `null` hanya untuk baris hasil pengisian data lama (`IGD-DEC-136`); **bukan** perubahan semantics penetapan/pengalihan baru. Validation, state, permission/audit, integration **tidak berubah**. *Sebelumnya — per 16 September 2026 (ketiga, kesiapan `EPIC IGD-04`):* API **`0.7.0`** aditif — bagian 3.2 baru untuk proyeksi `doctorName`/`assignedByName` (`IGD-DEC-129`) dan nama canonical `RegPatientEncounter` (`IGD-DEC-132`); validation, state, permission/audit **tidak berubah**; integration contract hanya nama tabel. *Sebelumnya (pemantauan observasi, bagian 0d):* API `0.6.0` aditif (bagian 7 baru); validation **`0.6.0`** aditif (bagian 9 baru); state, permission/audit, integration **tidak berubah**. *Per 15 September 2026 (penyelarasan teks, bagian 0c):* API `0.5.0` bukan aditif murni; validation `0.5.0` bukan aditif. *Keadaan revisi 6:* API `0.4.0` **bukan aditif**; validation `0.4.0` **bukan aditif**; state `0.4.0` aditif; permission/audit `0.4.0` aditif; integration `0.3.0` tidak berubah. Rinciannya di 0a.2. Seluruhnya `draft`. `IGD-DEC-093` **tidak diperluas**: yang `approved` tetap hanya state §1/§1.1/§1.2 dan validation §2 aturan 4–5. **Dikoreksi 15 September 2026:** kalimat ini tertinggal dari `IGD-DEC-108` (27 Agustus 2026), yang menaikkan irisan kontrak `MVP-1`…`MVP-6` menjadi `approved` — state §2, 3, 4, 6, 6a; validation §1, 1.1, 3, 4, 4.1, 5, 5.1, 6, 7; API §1.1, §2, bagian pengkajian; permission/audit §3.1; integration bagian encounter IGD — dengan wewenang sementara `IGD-DEC-107` |
| `roadmap_revision` | `3` — 2026-08-26, diperluas ke perjalanan pasien penuh: pendaftaran & triase, pengkajian, kepergian. Revision `2` (`MVP-0`) tetap di berkas yang sama; revision `1` diarsipkan ke `roadmap/archive/revision-1/`. **Penomoran gelombang bergeser**: pengkajian masuk `MVP-3`, kepergian ke `MVP-4`, serah terima `MVP-5`, kewenangan unit `MVP-6` |
| `belum_direncanakan` | **Pemakaian alat dan billing IGD.** *6 Oktober 2026:* pemesanan penunjang medis dari IGD (lima jenis) masuk desain `EPIC IGD-14` (bagian 0n); hasil penunjang dibaca sebatas yang disediakan modul pemiliknya. *Sebelumnya:* **Penunjang medis, pemakaian alat, billing IGD.** Batas lingkup ditutup `IGD-DEC-095`…`105`; masih nol epic, nol FR, nol kontrak. **Ditahan atas instruksi Product/Domain Owner** sampai correction pass revisi 6 tuntas dan `MVP-0` selesai |
| `koreksi_desain_tertunda` | **Nihil untuk slice encounter-first — diselesaikan 22 September 2026 (bagian 0g)**: (1) API §8, (2) state §8, (3) validation §10, (4) integration §5, (5) permission/audit §7, (6) `02-backend-architecture.md` §13, `03-frontend-architecture.md` §13, `flowcharts/` baru, catatan rujukan di `erd/emergency-episode.md`, (7) `04-prd-to-mvp.md` §8 (`FR-IGD-069`…`085`, `AT-IGD-166`…`185`). **Tersisa di luar slice:** `S7` kelayakan dokter jaga (ditahan `IGD-OQ-102`/`103`). *Keadaan sebelumnya:* **Ada lagi — 22 September 2026.** Keputusan `IGD-DEC-139`…`141` berlaku lebih dulu sebagai prinsip, tetapi berkas desain belum mengikutinya: (1) `contracts/api-contract.md` — `triage-queue`, `start-triage`, `eligible-doctors`, ruas `encounter` pada `active-episode` (§1.3), `eligibilityOverrideReason`, efek samping `complete`/`visit-status`; (2) `contracts/state-transition-matrix.md` — titik lahir `EmgVisit` dan encounter mengikuti status akhir kunjungan; (3) `contracts/validation-matrix.md` — rumus episode terbuka klausa A+B dan empat tanda "berakhir"; (4) `contracts/integration-contract.md` — **IGD menulis status `RegPatientEncounter`** dan Registrasi memanggil aturan IGD; (5) `contracts/permission-audit-matrix.md` — aksi baru dan jejak override; (6) `02-backend-architecture.md`, `03-frontend-architecture.md`, `erd/emergency-episode.md` — alur encounter-first; (7) `04-prd-to-mvp.md` — FR/AT untuk kebutuhan baru. Setiap task R3.13 menyelaraskan bagian yang disentuhnya (satu minor per task); sisanya pass `design-business-module`. *Keadaan sebelumnya:* **Nihil — 15 September 2026 (ketiga).** Kelima penyelarasan di bawah sudah dikerjakan pass `design-business-module`; rinciannya di bagian 0c. *Keadaan sebelumnya (15 September 2026, kedua) — lima penyelarasan teks berkas kontrak tertunda*, keputusannya sudah `approved` dan berlaku lebih dulu: (1) `contracts/api-contract.md` §3 — query `at` pada `GET /active` (`IGD-DEC-117`); (2) `erd/data-dictionary.md` §4, `erd/00-context-erd.md`, `erd/emergency-episode.md`, `02-backend-architecture.md` — nama `TrxEmergencyDoctorAssignment` → `EmgDoctorAssignment` (`IGD-DEC-116`); (3) `contracts/validation-matrix.md` §6 aturan 4 — pesan menyebut pesanan (`IGD-DEC-118`); (4) validation §1 aturan 2 — teks penolakan jenis kunjungan (`IGD-DEC-120`); (5) aturan baru batas 1000 karakter catatan status observasi (`IGD-DEC-119`). Dikerjakan pass `design-business-module` berikutnya beserta kenaikan versi dan hash. *Keadaan sebelumnya:* **Nihil.** Empat koreksi selesai pada revisi 6; enam butir correction pass 26 Agustus selesai — audit `EmergencyTransfer`, koreksi klaim aditif, penyelarasan metadata, pembentukan pesanan internal, unique constraint, kewenangan pesanan |
| `compatibility_impact` | **Empat perubahan memutus.** Lihat bagian 3 |

---

## 0n. Amendment 6 Oktober 2026 (sore) — Ruang Kerja Dokter IGD (`design-business-module`)

Revisi blueprint naik ke **`9`** — epic baru dengan layar baru (`EPIC IGD-14`, gelombang `MVP-9`). Masukannya *amendment
pass* `grill-me` 6 Oktober 2026 (sore) — `IGD-DEC-220`…`229`, fakta `IGD-FACT-080`…`084` — dan capability map
*Suplemen revision 3.4*. Seluruh isi bagian ini **`approved`** (`IGD-DEC-230`, Rizki Gunawan, 6 Oktober 2026).

| Butir | Isi |
| --- | --- |
| Masalah yang ditutup | Dokter IGD tidak punya layar untuk kajian medis, catatan dokter, diagnosis, resep, tindakan, dan pesanan penunjang selain laboratorium; layar pemeriksaan IGD dipakai perawat |
| Isi desain | Layar dokter terpisah mengikuti Ruang Kerja Dokter Rawat Inap (enam tab); penunjang lima jenis; tindak lanjut dibuat dan dikonfirmasi dokter, dijalankan perawat; tindakan klinis umum untuk dokter dan perawat; penjaga diagnosis saat konfirmasi; status awal tindak lanjut Draft; fakta jasa konsultasi IGD tidak dikirim |
| Perluasan di modul lain | ClinicalManagement: timeline catatan dokter per encounter, kajian medis untuk kunjungan IGD, endpoint tindakan keperawatan IGD. Laboratorium dan Radiologi: aturan instruksi untuk pasien IGD (`IGD-OQ-117`). Farmasi: penyelesaian catatan dokter tanpa fakta jasa konsultasi untuk encounter `Emergency` |
| Perubahan di IGD | Daftar dan detail kunjungan membawa DPJP aktif dan saringan `doctorId`/`ongoing`; tindak lanjut lahir hanya Draft; konfirmasi dijaga diagnosis |
| Snapshot source | Backend `rizkiG` `43dab6da`, frontend `RizkiV2` `6680278a2` — keduanya sejajar origin; fakta diverifikasi pada capability map suplemen 3.4 |
| Gerbang masuk | `requirement-completeness-gate` **tidak** dijalankan untuk slice ini, mengikuti preseden amandemen 0i–0m; `hospital-domain-architect` tidak dipakai (`DOMAIN_ARCHITECTURE_NOT_RUN`) — seluruh data klinis tetap milik modul pemiliknya dan tidak ada bounded context baru. Keputusan pemilik lengkap; blocker hanya sebagian (`IGD-OQ-117`, `119`). **Dicatat sebagai gerbang yang dilanggar sebagian**, bukan dianggap terpenuhi |
| Schema | **Nol** tabel, nol kolom, nol migration |
| Endpoint | Dua baru (milik ClinicalManagement): `GET …/doctor-consultations/encounters/{encounterId}/soap-timeline`, `POST …/patient-procedures/emergency-nursing-actions`. Perilaku berubah pada tujuh endpoint (API §10.1) — tiga di antaranya **memutus** (status awal tindak lanjut, penjaga diagnosis, instruksi lab/radiologi untuk perawat IGD) |
| Hak akses | **Nol butir baru**; anjuran pemberian per peran pada permission/audit §9.1 |
| Turunan | `02-backend-architecture.md` §15; `03-frontend-architecture.md` §15; `04-prd-to-mvp.md` §10 (`FR-IGD-096`…`105`, `AT-IGD-200`…`213`, DoD 1–10); `erd/data-dictionary.md` §8; `testing/acceptance-test-matrix.md` bagian Ruang Kerja Dokter IGD; `flowcharts/ruang-kerja-dokter.md` (baru) |
| Yang tidak berubah | `flowcharts/00-alur-utama.md` dan flowchart lain; seluruh bagian lama kontrak |
| Dampak frontend | Satu butir menu baru dengan dua anak (`SCR-IGD-D01`…`D03`); layar perawat berubah pada tab Tindak Lanjut, Penunjang, Tindakan |
| Pertanyaan terbuka | `IGD-OQ-117` (memblokir kaki perawat atas instruksi), `IGD-OQ-118` (tidak memblokir — ditunda), `IGD-OQ-119` (memblokir jenis gizi); `IGD-UNK-12`…`17` dijawab saat uji atau konfigurasi |
| Pilihan desain | Delapan butir pada `02-backend-architecture.md` §15.11 — disetujui seluruhnya (`IGD-DEC-230`) |
| Status | **`approved`** — `IGD-DEC-230`, Rizki Gunawan, 6 Oktober 2026 (sore), lewat pilihan interaktif *"Setujui seluruhnya"*; sementara menurut pola `IGD-DEC-174`. `plan-module-delivery` boleh menerbitkan kartu `MVP-9`; kartu yang bergantung pada `IGD-OQ-117`/`119` ditandai ⛔ |

**Versi kontrak sesudah pass ini:** API **`0.15.0`**, validation **`0.14.0`**, state **`0.10.0`**, permission/audit
**`0.8.0`**, integration **`0.6.0`** — seluruh bagian barunya `approved` (`IGD-DEC-230`).

**Hash sesudah pass ini** — dihitung **sesudah** baris approval `IGD-DEC-230` ditulis. SHA-256 atas isi berakhir-baris LF
(konvensi 0i–0m); akhiran baris setiap berkas tidak diubah. Manifest ini, `MODULE-STATUS.md`, `roadmap/`, `evidence/`,
dan `task/` tidak di-hash. Tabel ini **menggantikan** tabel 0m.

| Artifact | SHA-256 | Keterangan |
|---|---|---|
| `00-interview-decisions.md` | `2458597f609b598bdefcfe8841fec79e0aba77869d084faf03f73b504204086f` | berubah — `IGD-DEC-210`…`230` (bukti uji 6 Oktober, Ruang Kerja Dokter IGD, approval) |
| `01-existing-capability-map.md` | `aeb0b2a45ae96185db1eb5fdfd62509a84571e7c0004ec841b7058bdfe14867b` | berubah — suplemen revision 3.4 |
| `02-backend-architecture.md` | `af126c072c4cc4d11fa24bc60c97b85c1c1919af9c79b9e1727371ada854e306` | berubah — §15 |
| `03-frontend-architecture.md` | `293889268ae18790c3cf0927d18cd33a52bc904fc59b5295a58c32f59d59b573` | berubah — §15 |
| `04-prd-to-mvp.md` | `450266b8e33c3d6e38f6f2c5d5c73655ab1c125b32b45d5761ece64db1a54dda` | berubah — §10 (`EPIC IGD-14`) |
| `contracts/api-contract.md` | `0562246fcd74a372d5d921128010e05300143f1de6516ca2621609dca8504176` | `0.15.0` — §10 |
| `contracts/validation-matrix.md` | `cabd4aa3a2c935a654cc669fdd17a1489963aa361919ea106b7d64b67e809064` | `0.14.0` — §12 |
| `contracts/state-transition-matrix.md` | `8fdc61fd8e2bfd2dd24ee9574d99992bce3b6f588277a50e53448999e6f35d77` | `0.10.0` — §10 |
| `contracts/permission-audit-matrix.md` | `867d5936bb8ba4d8a692e226ad75408bd6a956b27bcf284b49ca21b070773f20` | `0.8.0` — §9 |
| `contracts/integration-contract.md` | `6f7ad86789ac269e26bc110b4cbbcddced903646f02c7e1441688b4c3f38c436` | `0.6.0` — §7 |
| `erd/data-dictionary.md` | `bf510df09758a7ac5264e05c89a94713c11d95cc558f0c97bfa4cd218e1fc5f9` | berubah — §8 |
| `flowcharts/penutupan-lewat-disposisi.md` | `671637a73ac844dff5ca1ea789ad57fbcb9661a476ad8d12aa48b17860162a83` | tidak berubah (sama dengan 0m) |
| `flowcharts/ruang-kerja-dokter.md` | `197e33ccefc563f847be58d846690e4ac2828bee330ab5e3055af51b6df877df` | **baru** — `approved` |
| `testing/acceptance-test-matrix.md` | `e1bf15047519f34094bba9229d4b61887a9577adcca1cfdf39cb5d2784d22f66` | **baru di-hash** — bagian Ruang Kerja Dokter IGD |

## 0m. Amendment 5 Oktober 2026 (sore) — pesanan tanpa sikap dan kepergian yang dibatalkan (`design-business-module`)

Revisi blueprint **tetap `8`**. Amandemen sempit: nol status, nol tabel, nol kolom, nol endpoint, nol hak akses baru.
Masukannya amendment pass `grill-me` 5 Oktober 2026 (sore) — `IGD-DEC-203` (`IGD-CONFLICT-007`) dan `IGD-DEC-205`, fakta
`IGD-FACT-051`, `054`, `055`, `057`. Hanya `IGD-DEC-205` yang mengubah isi kontrak; `IGD-DEC-203` menyelaraskan kode dengan
kontrak yang sudah `approved`.

| Butir | Isi |
| --- | --- |
| Masalah yang ditutup | Blocker kartu `BE-IGD-041` (manifest 0l.3). Validation §5.1 dan §6 aturan 4 tidak menyebut status kepergian, sehingga pesanan milik kepergian yang dibatalkan — yang barisnya tidak disentuh saat pembatalan (`IGD-FACT-055`) — akan menahan penutupan selamanya, dan perawat dipaksa memberi sikap atas resep yang tidak pernah *pergi* |
| Aturan baru | Validation §5 aturan 12 dan §6 aturan 4: pengecualian kepergian yang fisiknya `Cancelled`, untuk pesanan tanpa sikap maupun ditolak (`IGD-DEC-205`). §5.1 mengunci arti *tanpa sikap* (baris berlaku tanpa `Continue`/`Handover`/`Cancel` — arti yang sama dengan aturan 1), *ditolak*, yang tidak menahan, dan **batas cakupan**: pesanan yang belum pernah menjadi baris pesanan kepergian tidak dihitung. §6.1 (d) diselaraskan ke `AmbilPesananPenahanPenutupanAsync`; (e) isi daftar; (f) berlaku ke depan (pola `IGD-DEC-167`). State §6a.2: pengecualian yang sama; §9.2 butir 4 mencakup sikap pertama atas pesanan tanpa sikap |
| Penyelarasan rujukan basi | API §1.2 butir 2 (*observasi `Active`* → `Active` atau `Escalated`, validation `0.11.0`) dan butir 4 (*"pada kepergian yang dokumennya sudah diajukan"* → bunyi validation §5.1/§6 aturan 4) — **tanpa** kenaikan versi (preseden 0j.3, 0l) |
| Snapshot source | Backend `rizkiG` `8d81d361` + working tree `EmergencyUnitAuthorityService.cs` (`BE-IGD-039`); fakta diverifikasi pada `EmergencyDepartureService` `:169`–`:191`, `:574`–`:587`, `:654`–`:689`, `:712`–`:752`, `EmergencyDispositionService` `:108`–`:157`, `EmergencyVisitService` `:703`, `:791`, `EmergencyVisitController` `:714`. Frontend `RizkiV2` `57b1d360f` |
| Gerbang masuk | `requirement-completeness-gate` **tidak** dijalankan: area kepergian tetap `UNCLASSIFIED` (bagian 0), mengikuti preseden amandemen 0i–0l. `hospital-domain-architect` tidak dipakai (`DOMAIN_ARCHITECTURE_NOT_RUN`) — perubahan di dalam satu bounded context IGD. Keputusan pemilik lengkap dan nol blocker. **Dicatat sebagai gerbang yang dilanggar sebagian**, bukan dianggap terpenuhi |
| Schema | **Nol.** Nol migration |
| Endpoint | **Nol endpoint baru.** Perilaku berubah pada `PATCH /emergency-visits/{id}/complete`, penutupan susulan, dan isi `awaitingClosureReason` pada `GET /emergency-visits` dan `GET /{id}` — lewat satu penjaga (`IGD-FACT-057`) |
| Hak akses | **Nol perubahan** |
| Turunan | `02-backend-architecture.md` §11.1 (catatan baris yang terbentuk saat pengajuan ditolak) dan §14 (baris `EmergencyDepartureService`, class diagram, pohon folder); `04-prd-to-mvp.md` §9 `AT-IGD-198`, `199`, butir DoD 12; `flowcharts/penutupan-lewat-disposisi.md` langkah 3 (tetap `draft`) |
| Yang tidak berubah | Permission/audit `0.7.0`, integration `0.5.0`, `03-frontend-architecture.md`, `erd/data-dictionary.md`, `01-existing-capability-map.md`. Kartu frontend `FE-IGD-044` dan `IGD-DEC-206` memakai kontrak API §2.3 yang sudah `approved` |
| Dampak frontend | Nol pada pass ini. Layar sikap pesanan (`FE-IGD-044`) tidak menawarkan aksi pada kepergian yang dibatalkan (`IGD-DEC-205`, sudah tertulis di kartunya) |
| Pertanyaan terbuka | Nol baru. *Batas cakupan* §5.1 menjadi *coverage gap* `FR-IGD-051` yang tercatat di traceability; menutupnya butuh keputusan bisnis tersendiri |
| Status | **`approved`** — `IGD-DEC-209`, Rizki Gunawan, 5 Oktober 2026 (sore), lewat pilihan interaktif *"Setujui seluruhnya"*; sementara menurut pola `IGD-DEC-174`, Nursing authority belum ditunjuk. `plan-module-delivery` boleh membuka kartu `BE-IGD-041` |

**Versi kontrak sesudah pass ini:** API **`0.14.0`** (§1.2 diselaraskan tanpa kenaikan versi), validation **`0.13.0`**
(§5, §5.1, §6, §6.1 — bukan aditif murni), state **`0.9.0`** (§6a.2, §9.2 — aditif pada matriks), permission/audit
**`0.7.0`**, integration **`0.5.0`** — keduanya tidak berubah.

**Hash sesudah pass ini** — dihitung **sesudah** baris approval `IGD-DEC-209` ditulis. SHA-256 atas isi berakhir-baris LF
(konvensi 0i–0l); akhiran baris setiap berkas tidak diubah. Manifest ini, `MODULE-STATUS.md`, `roadmap/`, dan `evidence/`
tidak di-hash. Tabel ini **menggantikan** tabel 0l beserta baris pengganti 0l.2 dan 0l.3.

| Artifact | SHA-256 | Keterangan |
|---|---|---|
| `00-interview-decisions.md` | `e441203d24e2a1ba258c351d0061bce0b30a815ab32a126af2d08a20249822cc` | berubah — pass `grill-me` 5 Oktober sore (`IGD-DEC-204`…`208`) dan approval `IGD-DEC-209` |
| `01-existing-capability-map.md` | `c1097d63363407dd79658dba09ebacf20241d1da0fb56cd7058b4440803189ec` | tidak berubah (sama dengan 0l) |
| `02-backend-architecture.md` | `95af14bd2f3234e0e7da6350a9a996b0a44b86b39e712170da366c754d75f2bd` | berubah — §11.1 catatan, §14 |
| `03-frontend-architecture.md` | `d3bfbe8af698fbfea4525c678a9470b969759b8e45d5ec873f65ea9d88da4960` | tidak berubah (sama dengan 0l) |
| `04-prd-to-mvp.md` | `22c7ce53fefa195926b52c8722333d6fbfe21c7beb4d95104746fd242d2143cb` | berubah — §9 `AT-IGD-198`, `199`, DoD 12 |
| `contracts/api-contract.md` | `aef6ac68575da413a59c0a980684f3592625b33a787f95cf2d40091b832ecc8d` | `0.14.0` — §1.2 diselaraskan tanpa kenaikan versi |
| `contracts/validation-matrix.md` | `890ec43a978bc53378b7ad0352bb40bae97783f48fddda53b2265e3d07ca7cd8` | `0.13.0` |
| `contracts/state-transition-matrix.md` | `dba5ddfb885080f36248328100636bb85bd27fd0a8914ff0720893e27099a488` | `0.9.0` |
| `contracts/permission-audit-matrix.md` | `6849f9d208e9d954e87e669d413148cc55cd2ddaa27a2a23e0980feb590cfcdb` | `0.7.0` — tidak berubah (sama dengan 0l) |
| `contracts/integration-contract.md` | `b569ebcc1ffa278173ee00dbcc0690fc025d5f1f849f83e73a605af75e0fc5cb` | `0.5.0` — tidak berubah (sama dengan 0l) |
| `erd/data-dictionary.md` | `6edcedb0642bda2b20127da19029b868e7698707ba478137a2174982a0cb5552` | tidak berubah (sama dengan 0l) |
| `flowcharts/penutupan-lewat-disposisi.md` | `671637a73ac844dff5ca1ea789ad57fbcb9661a476ad8d12aa48b17860162a83` | berubah — langkah 3; tetap `draft` |

### 0m.1 Sinkronisasi roadmap sesudah amandemen — 5 Oktober 2026 (sore) (`plan-module-delivery`)

Nol perubahan kontrak, nol perubahan arsitektur, **revisi tetap `8`**. Pass ini hanya menurunkan approval `IGD-DEC-209` ke
roadmap dan traceability, sesuai urutan `IGD-DEC-208` langkah 1. Tabel hash 0m **tetap berlaku**: tidak satu pun artefak
yang di-hash disentuh, karena `roadmap/`, `MODULE-STATUS.md`, dan manifest ini memang tidak di-hash.

| Butir | Isi |
| --- | --- |
| Backend | `BE-IGD-041` ⛔ → 🟡 (roadmap backend R3.8). Blocker amandemen tercabut; **7 dari 17** acceptance terpetakan pada source (1–7, 16 September 2026 — build dan uji API belum dijalankan), **10 belum** (8–15 belum ada source-nya; 16 diff dan 17 build menunggu pengerjaan ulang). Tanda 🟡, bukan tanpa tanda, karena source dan laporan tracked kartu ini sudah ada (`status-task-roadmap` §2). Kolom Kontrak dan Dependency menunjuk validation `0.13.0` dan state `0.9.0`; perubahan b dan d menunjuk ketentuan barunya. Isi acceptance 1–17 **tidak berubah** |
| Frontend | `FE-IGD-044` ⛔ → tanpa tanda, **gelombang 2** R3.13.2: yang tersisa hanya dependency task biasa (`FE-IGD-043`, `BE-IGD-041`), bukan keputusan atau gerbang (`grafik-dependency-roadmap` §5). Kolom Kontrak menunjuk validation `0.13.0`. `FE-IGD-043` tetap siap, gelombang 1 |
| Skenario uji | Rujukan *"pola `AT-IGD-187`"* diganti `AT-IGD-198` (langkah 3 dan 4) pada `BE-IGD-041` acceptance 8, 10 dan `FE-IGD-044` acceptance 11; `BE-IGD-041` acceptance 12, 13 dan `FE-IGD-044` acceptance 7 dijejak ke `AT-IGD-199`. Keduanya ditulis pada amandemen 0m (PRD §9) |
| Grafik dependency | Backend R3.8: node amandemen ⛔ → ✅, node `BE-IGD-041` ⛔ → 🟡; tetap **11 node, 7 panah** = kolom `Dependency` 2 + 3 + 2 + 0. Frontend R3.13.2: cermin `BE-IGD-041` ⛔ → 🟡, node `FE-IGD-044` ⛔ → tanpa tanda; tetap **4 node, 3 panah** = 1 + 2. Ringkasan frontend: label node luar R3.8 dan node R3.13.2 diselaraskan; panah tetap 19. Ringkasan backend tidak berubah (R3.8 tetap 🟡; 15 node, 15 panah). Seluruh grafik bebas siklus |
| Traceability | Baris `BE-IGD-041` (R3.4.1 dan bagian *Pesanan tanpa sikap*) dan `FE-IGD-044` diselaraskan; kontrak menunjuk validation `0.13.0`, state `0.9.0`, API §1.2; `FR-IGD-051` dan `FR-IGD-087`/`088` memakai `AT-IGD-198`, `199`; baris keputusan `IGD-DEC-209` ditambahkan. Coverage gap tidak berubah |
| Register | Backend **39 ✅ dari 48**, frontend **25 ✅ dari 34**, gabungan **64 dari 82** — tidak berubah (task yang dibuka tidak naik ke ✅) |
| Impact scan | Frontend bergerak `57b1d360f` → **`553501053`** lewat *pull fast-forward* pemilik pukul 15.04 WIB hari ini (merge PR #66–#70: Finance, antrean dokter, `DataTable`, `store.jsx`). Snapshot 0l.3 dan 0m yang menyebut `57b1d360f` ditulis pada sore yang sama tanpa memeriksa ulang HEAD frontend, sehingga sebagian atau seluruhnya mungkin sudah tertinggal saat ditulis — dicatat di sini, tidak ditimpa di sana. **Nol** berkas IGD, `ConfirmModal`, `InformationAlert`, atau slice IGD berubah; `DataTable` kini menerima alias prop lama tanpa mengubah perilaku pemakai nama kanonik dan tidak dipakai tab Transfer; `store.jsx` hanya menambah reducer Finance. Working tree frontend bersih. Backend tidak bergerak (`8d81d361` + working tree yang sama). Dampak pada kartu `FE-IGD-043`, `FE-IGD-044`, dan kontrak: **nihil** |
| Urutan berikutnya | `IGD-DEC-208` langkah 2: keempat kartu (`FE-IGD-043`, `BE-IGD-041`, `BE-IGD-064`, `FE-IGD-044`) dibangun atas izin `build-module-*` per task dari Rizki; build backend milik Rizki; lalu satu panduan uji Antigravity untuk keempatnya. Rilis `BE-IGD-041` hanya bersama `FE-IGD-044` (`IGD-DEC-204`) |
| Snapshot | Backend `rizkiG` `8d81d361` + working tree `EmergencyUnitAuthorityService.cs` dan dokumen 5 Oktober (belum di-commit); frontend `RizkiV2` `553501053`, bersih |

## 0l. Amendment 5 Oktober 2026 — kewenangan unit `BE-IGD-039` (`design-business-module`)

Revisi blueprint **tetap `8`**. Ini amandemen sempit: nol status, nol tabel, nol kolom, nol endpoint, dan nol hak akses
baru. Yang naik adalah versi dua kontrak. Masukannya dua amendment pass `grill-me` 5 Oktober 2026: pass C3 (konfigurasi
peran untuk UAT, `IGD-DEC-190`…`192`, `IGD-FACT-037`…`043`, `IGD-OQ-114`) dan pass C2 (kewenangan unit,
`IGD-DEC-193`…`198`, `IGD-FACT-044`…`048`, `IGD-CONFLICT-006`, `IGD-OQ-115`). Hanya pass C2 yang diturunkan ke kontrak.
Keputusan pass C3 adalah pemberian hak ke peran dan naskah UAT, bukan isi kontrak.

| Butir | Isi |
| --- | --- |
| Masalah yang ditutup | Syarat C2 [kesiapan `MVP-8`](evidence/2026-10-04-kesiapan-mvp-8.md). Penjaga kewenangan unit membandingkan departemen penempatan pengguna dengan simpul organisasi unit, sehingga **setiap** pengguna ditolak `403`. Pesan untuk unit yang belum dipetakan juga menjanjikan jalan keluar beralasan yang tidak pernah dibangun. Akibatnya serah terima dan sikap pesanan tidak dapat dijalankan siapa pun, dan `BE-IGD-061` acceptance 2–3 tidak dapat dibuktikan |
| Aturan baru | Validation §7: aturan 1 ditegaskan — jembatan lewat simpul organisasi penugasan HR (`SourceAssignmentId` → `WfpOrganizationAssignment.OrganizationUnitId`), simpul sama persis (`IGD-DEC-193`). Aturan 3 diputuskan — fail-closed dengan kalimat `IGD-DEC-195`; jalan keluar beralasan ditunda. Aturan 6–9 baru: simpul induk tidak mencakup turunan, penempatan warisan tanpa sumber ditolak, penempatan sekunder berlaku, dan cakupan lima tindakan (`IGD-DEC-194`, `197`). Permission/audit: catatan di bawah tabel §3 (lima tindakan yang benar-benar dijaga; `IGD-CONFLICT-006` ditunda), §3.1 *"Yang belum terjawab"* diganti, dan baris §6 diselaraskan |
| Snapshot source | Backend `rizkiG` **`8d81d361`** — merge integration (PR #233–#235) sesudah `0078a0dc`. `git diff 0078a0dc 8d81d361` **kosong** untuk `Areas/HealthServices/EmergencyInstallationManagement` dan `docs/module-blueprints/igd`, juga untuk berkas dasar fakta C2 (`ApplicationUserOrganization.cs`, `MstOrganizationUnit.cs`, `WfpOrganizationAssignment.cs`, `MstServiceUnit.cs`, `OrganizationAuthorizationProjectionService.cs`). Source IGD identik dengan `c1f79f79`. Frontend `RizkiV2` `57b1d360f` |
| Gerbang masuk | `requirement-completeness-gate` **tidak** dijalankan: area kewenangan unit tetap `UNCLASSIFIED` (bagian 0), mengikuti preseden amandemen 0i–0k. `hospital-domain-architect` tidak dipakai (`DOMAIN_ARCHITECTURE_NOT_RUN`), karena perubahannya di dalam satu bounded context IGD; HR, Master Data, dan platform-authorization hanya dibaca. Keputusan pemilik lengkap dan nol blocker. **Dicatat sebagai gerbang yang dilanggar sebagian**, bukan dianggap terpenuhi |
| Schema | **Nol.** Nol migration |
| Endpoint | **Nol endpoint baru.** Perilaku berubah pada lima tindakan kepergian (`arrive`, `accept-handover`, `reject-handover`, `order-items/{itemId}/accept`/`reject`/`action`, `POST {id}/order-items`): pengguna yang penugasan HR-nya cocok kini lolos, dan pesan `403` untuk unit yang belum dipetakan berganti |
| Hak akses | **Nol resource dan nol aksi baru.** Konfigurasi peran 5 Oktober 2026 (`IGD-DEC-190`, `191`, `198`) adalah pemberian hak di basis data (permission/audit §7.1), bukan perubahan kontrak |
| Data | `MstServiceUnit.OrganizationUnitId` diisi pemilik lewat basis data per lingkungan; agent hanya menyiapkan kueri baca. Pembukaan kolomnya di API diminta ke pemilik Master Data (`IGD-DEC-196`) |
| Penyelarasan rujukan basi | Integration `0.5.0` §3 baris *"Simpul organisasi unit belum dipetakan"*; `04-prd-to-mvp.md` tabel gelombang `MVP-5` dan baris `IGD-OQ-071`. Ketiganya masih menulis `IGD-OQ-071` *"belum diputuskan"* sejak Agustus. Diselaraskan **tanpa** kenaikan versi (preseden 0j.3) |
| Yang tidak berubah | API `0.14.0`, state `0.8.0`, `02-backend-architecture.md` (baris `EmergencyUnitAuthorityService` sudah menulis penelusuran *"profil pegawai dan penugasan organisasi yang sedang berlaku"*), `03-frontend-architecture.md`, `erd/data-dictionary.md`, `flowcharts/` |
| Dampak frontend | **Nol.** `BE-IGD-039` tetap kartu backend saja (`IGD-DEC-195`); layar menampilkan pesan `403` dari server apa adanya |
| Pertanyaan terbuka | `IGD-OQ-114` (pasien tanpa observasi tidak dapat ditindaklanjuti dari layar) dan `IGD-OQ-115` (aksi unit asal yang belum dijaga) — keduanya **tidak** memblokir |
| Status | **`approved`** — `IGD-DEC-199`, Rizki Gunawan, 5 Oktober 2026, lewat pilihan interaktif *"Setujui + selaraskan 3 rujukan"*; sementara menurut pola `IGD-DEC-174`, dengan approver akhir Security/Privacy owner yang belum ditunjuk. `plan-module-delivery` boleh memperbarui dan membuka kartu `BE-IGD-039` |

**Versi kontrak sesudah pass ini:** API **`0.14.0`**, validation **`0.12.0`** (§7 — bukan aditif murni), state **`0.8.0`**,
permission/audit **`0.7.0`** (§3 catatan, §3.1, §6 baris), integration **`0.5.0`** (satu baris diselaraskan tanpa kenaikan
versi).

**Hash sesudah pass ini** — dihitung **sesudah** baris approval `IGD-DEC-199` ditulis. SHA-256 atas isi berakhir-baris LF
(konvensi 0i–0k). Working tree `docs/` checkout bercampur akhiran baris per berkas: permission/audit dan integration ber-LF,
sisanya CRLF; byte setiap berkas dinormalkan dulu, dan akhiran barisnya tidak diubah. Manifest ini, `MODULE-STATUS.md`, dan
`evidence/` tidak di-hash. Tabel ini **menggantikan** tabel 0k beserta baris pengganti 0k.2.

| Artifact | SHA-256 | Keterangan |
|---|---|---|
| `00-interview-decisions.md` | `f4a61d615b163e86627eed37163ac468947d58b90ec6305612b1570377867a0c` | berubah — pass C3 dan C2 5 Oktober 2026, `IGD-DEC-190`…`199`. ***Usang 5 Oktober 2026 (sore)*** — digantikan bagian 0l.2 |
| `01-existing-capability-map.md` | `c1097d63363407dd79658dba09ebacf20241d1da0fb56cd7058b4440803189ec` | tidak berubah (sama dengan 0k) |
| `02-backend-architecture.md` | `bd5e7d527895a261c6bea92a406b07686c57569524c24e694b5bec3bd2f314f9` | tidak berubah (sama dengan 0k) |
| `03-frontend-architecture.md` | `d3bfbe8af698fbfea4525c678a9470b969759b8e45d5ec873f65ea9d88da4960` | tidak berubah (sama dengan 0k) |
| `04-prd-to-mvp.md` | `d8d00faf5648ed5a1c97e11510491a5f49799a4900fdd9fc0a778b6fa46ed25d` | berubah — dua baris `IGD-OQ-071` diselaraskan |
| `contracts/api-contract.md` | `9b9b29beb910d7ca238646c122ca252daf12f64bb47f74f36728bc920cbd3c10` | `0.14.0` — tidak berubah |
| `contracts/validation-matrix.md` | `5f2fbe98186ad97fa35a326454f6912bf534c21886805a2c71e23335a5aa6757` | `0.12.0` |
| `contracts/state-transition-matrix.md` | `02d6c649fea1a34eecfc8d7347f731ae1a21c1a51fdd6ddaf3dd0a36bee37440` | `0.8.0` — tidak berubah |
| `contracts/permission-audit-matrix.md` | `6849f9d208e9d954e87e669d413148cc55cd2ddaa27a2a23e0980feb590cfcdb` | `0.7.0` |
| `contracts/integration-contract.md` | `b569ebcc1ffa278173ee00dbcc0690fc025d5f1f849f83e73a605af75e0fc5cb` | `0.5.0` — satu baris §3 diselaraskan |
| `erd/data-dictionary.md` | `6edcedb0642bda2b20127da19029b868e7698707ba478137a2174982a0cb5552` | tidak berubah (sama dengan 0k) |
| `flowcharts/penutupan-lewat-disposisi.md` | `e08e936a0d45c2e1a0329f2c2a9eb304858701569aa2b9e845b752be4226cf17` | tidak berubah (sama dengan 0k); tetap `draft` |

### 0l.1 Perencanaan delivery 5 Oktober 2026 (`plan-module-delivery`)

Kartu `BE-IGD-039` dibuka atas `IGD-DEC-199`. Nol keputusan perencanaan baru: acceptance 11 dan 12 diturunkan dari
validation §7 aturan 4 dan 5 yang sudah ada, supaya `FR-IGD-056` dan `FR-IGD-059` punya bukti.

| Butir | Isi |
| --- | --- |
| Backend | `BE-IGD-039` **ditulis ulang di tempatnya** (roadmap backend bagian R3.7; ID tetap): ⛔ → tanpa tanda, **siap dikerjakan**. Cakupan satu berkas (`EmergencyUnitAuthorityService.cs`), nol migration, nol `Program.cs`, nol endpoint. Acceptance 1–13, persiapan data P1–P5 milik pemilik, dan uji ulang `BE-IGD-061` `S2`, `S3`, `S4`, `S12`, kaki `reject-handover` `S11` pada putaran yang sama. Kartu `BE-IGD-061` mendapat baris *Syarat penyelesaian* (acceptance 2–3 menunggu `BE-IGD-039`), tanpa panah baru |
| Frontend | **Nol task** (`IGD-DEC-195`). Tombol *Terima Dokumen*, *Tolak Dokumen*, *Catat Tiba* sudah ada di tab Transfer; sikap pesanan diuji lewat API. Roadmap frontend tidak berubah — catatannya *"terhalang `BE-IGD-039`"* tetap benar sampai kartu itu ✅ |
| Grafik dependency | Ringkasan: node blocker *Security/Privacy owner* dan *Pemetaan unit terisi* beserta dua panahnya dihapus (16 → 14 node, 17 → 15 panah); `MVP-6` tanpa tanda. R3.7: node blocker dilepas (1 → 0 panah), `BE-IGD-039` gelombang 1. R3.14.2 tidak berubah (5 panah). Jumlah panah = isi kolom `Dependency` pada ketiga grafik |
| Traceability | Bagian baru *`EPIC IGD-08` — kewenangan unit `BE-IGD-039`*: `FR-IGD-053`…`059` tertelusuri penuh ke acceptance 1–13; `IGD-DEC-190`…`199` ditelusuri; coverage gap dicatat — tindakan unit asal yang belum dijaga (`IGD-CONFLICT-006`), sikap pesanan tanpa layar, acceptance 5 boleh `NOT RUN` |
| Register | Tetap: backend 37 ✅ dari 47, frontend 25 ✅ dari 32, gabungan 62 dari 79 — `BE-IGD-039` berpindah dari ⛔ ke tanpa tanda |
| Snapshot | Backend `rizkiG` `8d81d361` (area IGD identik dengan `c1f79f79`); frontend `RizkiV2` `57b1d360f` |

### 0l.2 Penyelarasan 5 Oktober 2026 (sore) — bukti uji `BE-IGD-039` dan penutupan `MVP-8` (`manage-module-blueprint`)

Hanya status, hash, dan tautan bukti — nol perubahan kontrak, nol perubahan arsitektur, **revisi tetap `8`**.

| Butir | Isi |
| --- | --- |
| Keputusan baru | `IGD-DEC-200` (bukti uji 5 Oktober diterima dengan penyimpangan tercatat), `IGD-DEC-201` (`BE-IGD-039` ✅ dengan acceptance 3, 4, 5, 6, 11 dikecualikan), `IGD-DEC-202` (pesan galat aksi kepergian harus terbaca — kartu frontend pasangan), `IGD-DEC-203` (`IGD-CONFLICT-007` diperbaiki lewat pengerjaan ulang `BE-IGD-041`); fakta `IGD-FACT-049`…`052`; `IGD-OQ-116` (kartu pasien Ruang Kerja, `open`). Seluruhnya keputusan operasional dan perencanaan; tidak menyentuh kontrak |
| Hash `00-interview-decisions.md` | `2e1fe1925874b6aa33b37bc5f645354dda0b75ab0f607004214a43914c26b459` — **menggantikan** baris decision log pada tabel 0l (`f4a61d61…`). Artefak lain pada tabel 0l dihitung ulang: **identik**. ***Usang 5 Oktober 2026 (sore, perencanaan)*** — digantikan bagian 0l.3 |
| Status delivery | `BE-IGD-039` ✅ (`IGD-DEC-201`); `BE-IGD-061` ✅ — `MVP-8` **6 dari 6** task ✅; `MVP-6` ✅ |
| Register | Backend **39 ✅ dari 47** (sebelumnya 37); frontend 25 ✅ dari 32; gabungan **64 dari 79** |
| Bukti | [Panduan](testing/2026-10-05-panduan-uji-be-igd-039.md) dan [laporan uji](testing/2026-10-05-laporan-uji-be-igd-039.md) bagian 7; laporan task [`BE-IGD-039`](task/report/backend/BE-IGD-039.md), [`BE-IGD-061`](task/report/backend/BE-IGD-061.md) |
| Perencanaan tertunda | Kartu frontend pasangan `BE-IGD-039` (`IGD-DEC-202`) dan perluasan `BE-IGD-041` (`IGD-DEC-203`) — milik `plan-module-delivery`. ***Dikerjakan 5 Oktober 2026 (sore)*** — bagian 0l.3 |
| Snapshot | Backend `rizkiG` `8d81d361` + working tree `EmergencyUnitAuthorityService.cs`; frontend `RizkiV2` `57b1d360f`, hasil build 5 Oktober 2026 12.00.44 |

### 0l.3 Perencanaan delivery 5 Oktober 2026 (sore) — galat aksi kepergian dan pesanan tanpa sikap (`plan-module-delivery`, `grill-me`)

Nol perubahan kontrak, nol perubahan arsitektur, **revisi tetap `8`**. Perencanaan `IGD-DEC-202` dan `IGD-DEC-203` menemukan
dua aturan yang belum diputuskan, lalu satu aturan layar yang lahir dari jawaban pertama. Ketiganya dijawab lewat amendment
pass `grill-me` sempit (decision log, bagian *Pesanan tanpa sikap dan jalan penyelesaiannya dari layar*).

| Butir | Isi |
| --- | --- |
| Keputusan baru | `IGD-DEC-204` (layar sikap pesanan dibangun; `BE-IGD-041` dirilis bersamanya; gap `IGD-EV-109` dicabut sebagian), `IGD-DEC-205` (pesanan pada kepergian yang fisiknya dibatalkan tidak menahan penutupan — tanpa sikap maupun ditolak; sementara, pola `IGD-DEC-174`), `IGD-DEC-206` (unit penerima *Handover* di layar = unit tujuan kepergian), `IGD-DEC-207` (`IGD-OQ-116` dijawab — kartu backend `BE-IGD-064`), `IGD-DEC-208` (urutan: amandemen dulu, satu putaran uji untuk empat kartu); fakta `IGD-FACT-053`…`058`. Approver Rizki Gunawan, 5 Oktober 2026, pilihan interaktif — kelimanya opsi yang direkomendasikan agent. `IGD-DEC-207` dan `208` lahir dari perluasan scope oleh pemilik sesudah perencanaan |
| Backend | `BE-IGD-041` **diperluas di tempatnya** (roadmap backend R3.8; ID tetap): 🟡 → ⛔. Penjaga penutupan ikut menghitung pesanan tanpa sikap (definisi `SubmitHandoverAsync`); pesanan pada kepergian yang dibatalkan tidak dihitung; kalimat §6 aturan 4 tetap. Cakupan `EmergencyDepartureService.cs`; nol `Program.cs`, controller, DTO, migration, endpoint. Acceptance 8–17, termasuk ulang `PROBE-1` dan regresi `BE-IGD-061` acceptance 3. Rilis hanya bersama `FE-IGD-044`. **`BE-IGD-064` baru** (gelombang R3.15): `GET /emergency-visits/{id}` memuat `Patient`, `ServiceUnit`, `ArrivalMode`, `CaseType`; satu berkas `EmergencyVisitController.cs`; nol kontrak (API §1), nol frontend, nol migration; siap dikerjakan; acceptance 1–6 |
| Frontend | Bagian **R3.13.2** baru. `FE-IGD-043` — galat aksi kepergian tampil di dalam modal konfirmasi; satu berkas tab Transfer; **siap dikerjakan**; acceptance 1–9 termasuk uji ulang `039-U1`, `039-U2`. `FE-IGD-044` — daftar pesanan per kepergian dan penetapan sikap; ⛔ menunggu `BE-IGD-041` dan `FE-IGD-043`; acceptance 1–13 |
| Blocker | **Amandemen validation §5.1 dan §6 aturan 4** untuk `IGD-DEC-205` — pemilik agent `design-business-module`, approval Rizki. Menahan `BE-IGD-041` dan, lewat kartu itu, `FE-IGD-044`. **Tidak** menahan `FE-IGD-043` dan `BE-IGD-064`. Urutan kerja: amandemen lebih dulu, lalu keempat kartu dibangun dan diuji dalam satu putaran (`IGD-DEC-208`) |
| Grafik dependency | Backend R3.8: node `IGD-DEC-203` dan blocker amandemen ditambahkan (9 → 11 node, 5 → 7 panah = kolom `Dependency` 2 + 3 + 2 + 0). Ringkasan backend: node `R3.15` tanpa panah (14 → 15 node, panah tetap 15). Backend R3.15 baru: 1 node, 0 panah. Frontend ringkasan: node R3.13.2 ditambahkan beserta `BR37 --> FPESAN`, `BR38 --> FPESAN` (17 → 19 panah); tanda node luar R3.8 diselaraskan menjadi 🟡. Frontend R3.13.2 baru: 4 node, 3 panah = kolom `Dependency` 1 + 2. Ketiga grafik bebas siklus |
| Traceability | Bagian baru *Pesanan tanpa sikap*: `FR-IGD-045`…`051`, `087`, `088`, `091`, `053`, `054` ke task dan uji; `BE-IGD-064` tanpa FR, dijejak ke API §1; `IGD-DEC-200`…`208` ditelusuri; `IGD-OQ-116` dijawab. Coverage gap: pesanan internal yang belum pernah dibentuk tidak menahan; `FR-IGD-049` tanpa layar; syarat klinisi `Cancel`; nama pelaku sikap; `BE-IGD-064` tanpa FR |
| Register | Backend **39 ✅ dari 48** (`BE-IGD-041` 🟡 → ⛔; `BE-IGD-064` baru). Frontend **25 ✅ dari 34** (sebelumnya dari 32). Gabungan **64 dari 82** |
| Hash `00-interview-decisions.md` | `aada7c30026f79d6a15839bb588078ae9b9520007a43fcaff7a8a8db54da9412` — **menggantikan** baris 0l.2 (`2e1fe192…`). ***Usang 5 Oktober 2026 (sore, amandemen)*** — digantikan tabel bagian 0m. SHA-256 atas isi berakhir-baris LF (konvensi 0i–0l); berkasnya tetap CRLF. Sebelas artefak lain pada tabel 0l dihitung ulang: **identik** |
| Snapshot | Backend `rizkiG` `8d81d361` + working tree `EmergencyUnitAuthorityService.cs` dan dokumen 5 Oktober (belum di-commit); frontend `RizkiV2` `57b1d360f`, bersih |

## 0k. Amendment 3 Oktober 2026 — observasi Dieskalasi dan kunjungan yang sudah berakhir (`design-business-module`)

Revisi blueprint **tetap `8`**: amandemen sempit di dalam slice R3.14 yang sama — nol status, nol tabel, nol kolom,
nol endpoint, nol hak akses baru. Yang naik adalah versi tiga kontrak. Masukan `IGD-DEC-183`…`185`, fakta
`IGD-FACT-032`…`036`, asumsi `IGD-ASM-003`, dan `IGD-OQ-112` (`00-interview-decisions.md`, amendment pass
3 Oktober 2026).

| Butir | Isi |
| --- | --- |
| Masalah yang ditutup | Temuan uji `BE-IGD-061` S6: observasi yang dieskalasi sebelum disposisi dilaksanakan tidak menahan penutupan, sehingga kunjungan tertutup sementara observasinya masih Dieskalasi — dan sesudahnya observasi itu tidak dapat diselesaikan lagi (`409` teknis penjaga transisi) |
| Aturan baru | Penjaga penutupan menghitung observasi `Active` **dan** `Escalated` (`IGD-DEC-183`); pada kunjungan `Completed`/`Cancelled`, aksi observasi selain *batalkan* ditolak `409` dengan kalimat sendiri (`IGD-DEC-184`). Pemantauan baru pada kunjungan berakhir ikut ditolak (validation §11.2 aturan 21 — usulan agent, disetujui `IGD-DEC-186`) |
| Snapshot source | Backend `rizkiG` `2a63a3bb` (`ahead 11`, seluruh commit sesudah `5af6ef3b` hanya dokumen — `git diff 5af6ef3b HEAD -- Areas/HealthServices/EmergencyInstallationManagement` kosong). Fakta diverifikasi pada `EmergencyDispositionService.ValidateVisitClosureAsync` `:108`, `EmergencyObservationController.UpdateObservationStatus` `:271`–`:407`, `EmergencyObservationService.CanTransition` dan `ValidateDetailScopeAsync`, `EmergencyVisitService.SaringMenungguPenutupan`. Frontend `RizkiV2` `521b18a9a` + working tree |
| Schema | **Nol.** Nol migration |
| Endpoint | **Nol endpoint baru.** Perilaku berubah pada `PATCH /emergency-visits/{id}/complete`, `PATCH /emergency-observations/{id}/observation-status`, saringan `GET /emergency-visits?awaitingClosure=true` (isi, bukan bentuk), dan `POST /emergency-observation-details` (API §9.1 nomor 6–8) |
| Hak akses | **Nol perubahan** |
| Gelombang | `MVP-8`, `EPIC IGD-13`; `FR-IGD-094`, `095`; `AT-IGD-194`…`197`; DoD butir 11. Dikerjakan sebagai pengerjaan ulang `BE-IGD-061` (`IGD-DEC-185`) |
| Pertanyaan terbuka | `IGD-OQ-112` (jumlah observasi Dieskalasi tertinggal per lingkungan) — **tidak memblokir** |
| Dampak frontend — dicatat untuk `plan-module-delivery` | Tab Observasi (`OBSERVATION_STATUS_ACTIONS`, `resolveObservationStatusActions`) **tidak** membedakan kunjungan yang sudah berakhir: observasi Dieskalasi lama tetap menampilkan *Selesaikan* beserta kalimat konfirmasi *"Kunjungan pasien berpindah ke Menunggu Tindak Lanjut"*, yang sesudah amandemen ini selalu dijawab `409`. Alasan penahan pada daftar Pengkajian (`FE-IGD-041`) ditampilkan apa adanya dari server — nol perubahan di sana. **Diputuskan `IGD-DEC-187`:** pada kunjungan berakhir, *Selesaikan* dan *Eskalasi* tampil nonaktif beserta keterangan kalimat aturan 18, *Batalkan* tetap aktif — pengerjaan ulang `FE-IGD-042`. Pass ini tidak mengubah layar |
| Catatan, bukan keputusan | Ruang kerja pemeriksaan tidak menjadi baca-saja pada kunjungan yang sudah berakhir; ini perilaku lama di luar amandemen |
| Status | **`approved`** — `IGD-DEC-186`, Rizki Gunawan, 3 Oktober 2026, lewat pilihan interaktif *"Setujui seluruhnya"*; sementara menurut pola `IGD-DEC-174`. Kontrak terkunci; `plan-module-delivery` boleh memperluas `BE-IGD-061` (`IGD-DEC-185`) dan `FE-IGD-042` (`IGD-DEC-187`). `flowcharts/penutupan-lewat-disposisi.md` tetap `draft` |

**Versi kontrak sesudah pass ini** (Rencana): API **`0.14.0`** (§9.1 nomor 6–8, catatan §9.2, perluasan §9.4),
validation **`0.11.0`** (§6 aturan 2, §11.2 aturan 16–21, penanda pada §11 aturan 2 dan 15), state **`0.8.0`**
(§9.6, satu baris §9.4, penanda pada §9.5), permission/audit **`0.6.0`** dan integration **`0.5.0`** tidak berubah.
Berkas turunan yang disentuh: `02-backend-architecture.md` §14 (paragraf status, §14.3, §14.4), `04-prd-to-mvp.md` §9,
`flowcharts/penutupan-lewat-disposisi.md` (langkah 3, dua jalur pengecualian; tetap `draft`).

**Hash sesudah pass ini** — dihitung **sesudah** baris status approval `IGD-DEC-186`/`187` ditulis. SHA-256 atas isi
berakhir-baris LF (konvensi tabel 0i/0j; working tree checkout ber-CRLF karena `core.autocrlf = true`, sehingga
byte-nya dinormalkan dulu). HEAD lokal `2a63a3bb`, belum di-commit. Manifest ini sendiri tidak di-hash. Tabel ini
**menggantikan** tabel 0j beserta baris pengganti 0j.1–0j.5.

| Artifact | SHA-256 | Keterangan |
|---|---|---|
| `00-interview-decisions.md` | `cac57c061cba23bc0e596e56303f4143dd71c1e83faa6f49cfc8c87b488021dd` | berubah — `IGD-DEC-183`…`187` |
| `01-existing-capability-map.md` | `c1097d63363407dd79658dba09ebacf20241d1da0fb56cd7058b4440803189ec` | tidak berubah (sama dengan 0j) |
| `02-backend-architecture.md` | `bd5e7d527895a261c6bea92a406b07686c57569524c24e694b5bec3bd2f314f9` | berubah — §14 paragraf status, §14.3, §14.4 |
| `03-frontend-architecture.md` | `d3bfbe8af698fbfea4525c678a9470b969759b8e45d5ec873f65ea9d88da4960` | tidak berubah (sama dengan 0j) |
| `04-prd-to-mvp.md` | `5837324b76415181ffe4fdd89e19e04d62a18a22fdf3cb907bf3ddd34bb94d06` | berubah — §9 |
| `contracts/api-contract.md` | `9b9b29beb910d7ca238646c122ca252daf12f64bb47f74f36728bc920cbd3c10` | `0.14.0` |
| `contracts/validation-matrix.md` | `37761c9019d05f9e1965e85fce65d156c9d0f5bb3b27fdb5cb3e6857b0c9589c` | `0.11.0` |
| `contracts/state-transition-matrix.md` | `02d6c649fea1a34eecfc8d7347f731ae1a21c1a51fdd6ddaf3dd0a36bee37440` | `0.8.0` |
| `contracts/permission-audit-matrix.md` | `24234cb2dab7c73d1787a8ac24accd34539a7cc9cf580445dbb0f5840132e444` | `0.6.0` — tidak berubah (sama dengan 0j) |
| `contracts/integration-contract.md` | `0f0a544f107040f140f6abb117b67e1241da196ec9514e3ae747b3eba111bca0` | `0.5.0` — tidak berubah (sama dengan 0j) |
| `erd/data-dictionary.md` | `6edcedb0642bda2b20127da19029b868e7698707ba478137a2174982a0cb5552` | tidak berubah (sama dengan 0j) |
| `flowcharts/penutupan-lewat-disposisi.md` | `e08e936a0d45c2e1a0329f2c2a9eb304858701569aa2b9e845b752be4226cf17` | berubah — langkah 3, dua jalur pengecualian; tetap `draft` |

### 0k.1 Perencanaan delivery 3 Oktober 2026 (`plan-module-delivery`)

Kartu R3.14/R3.13.1 diperluas atas `IGD-DEC-186`. Nol keputusan perencanaan baru — tempat pemeriksaan aturan 21
ditetapkan dari urutan §9.1 yang mengikat, bukan dipilih.

| Butir | Isi |
| --- | --- |
| Backend | `BE-IGD-061` diperluas sebagai pengerjaan ulang (`IGD-DEC-185`): acceptance 13–19; berkas tambahan `EmergencyDispositionService.cs` dan `EmergencyObservationService.cs` (`ValidateDetailScopeAsync` — satu-satunya tempat yang menjaga urutan §9.1). Tetap 🟡. Nol kartu baru, nol migration |
| Frontend | `FE-IGD-042` **dibuka ulang** (`IGD-DEC-187`): acceptance 10–13; status **turun dari ✅ ke 🟡** karena kartu bertambah acceptance. Tombol tambah pemantauan tidak termasuk (di luar `IGD-DEC-187`) |
| Grafik dependency | Struktur tidak berubah — nol task baru, nol panah baru; hanya tanda `FE-IGD-042` pada grafik Mermaid R3.13.1.1 (dibiarkan Mermaid) |
| Traceability | `FR-IGD-094`, `095` → `BE-IGD-061` + `FE-IGD-042`; `IGD-DEC-183`…`187` ditelusuri; `IGD-OQ-112` dicatat tidak menahan; coverage gap nihil |
| Register | Frontend 23 ✅ dari 32 (sebelumnya 24); backend tetap 37 ✅ dari 47 |
| Snapshot | Backend `rizkiG` `2a63a3bb` (source IGD identik dengan `5af6ef3b`); frontend `RizkiV2` `521b18a9a` + working tree |

### 0k.2 Penyelarasan 4 Oktober 2026 — uji gabungan dan kesiapan `MVP-8` (`manage-module-blueprint`)

Syarat C6 [kesiapan `MVP-8`](evidence/2026-10-04-kesiapan-mvp-8.md). Hanya status, hash, dan tautan bukti — nol
perubahan kontrak, nol perubahan arsitektur, **revisi tetap `8`**.

| Butir | Isi |
| --- | --- |
| Keputusan baru | `IGD-DEC-188` (izin Perawat IGD yang ditambahkan agen penguji disahkan; bukti uji gabungan `MVP-8` diterima dengan penyimpangan tercatat), `IGD-DEC-189` (sandi SuperAdmin dev tidak diganti; uji tanpa SuperAdmin), `IGD-OQ-113` (`EmergencyDeparture : Approve` pada Perawat IGD, `open`, tidak memblokir). Ketiganya keputusan operasional; tidak menyentuh kontrak |
| Hash `00-interview-decisions.md` | `d44df2b653903a910320b5440ffdd9bbd3ad622c2b4c0aa50c26c697e7ba0509` — **menggantikan** baris decision log pada tabel 0k (`cac57c06…`). SHA-256 atas isi berakhir-baris LF, konvensi yang sama dengan 0k. ***Usang 5 Oktober 2026*** — decision log bertambah `IGD-DEC-190`…`199`; hash terkini ada di tabel bagian 0l |
| Artefak lain pada tabel 0k | Dihitung ulang 4 Oktober 2026: **identik** — `01`, `02`, `03`, `04`, kelima kontrak, `erd/data-dictionary.md`, `flowcharts/penutupan-lewat-disposisi.md` |
| Status delivery | `MVP-8`: 5 dari 6 task ✅ — `BE-IGD-060`, `062`, `063`, `FE-IGD-041`, `FE-IGD-042`; `BE-IGD-061` 🟡 (acceptance 13–19 terbukti 4 Oktober 2026; acceptance 2–3 tertahan `BE-IGD-039`) |
| Kesiapan | `READY_WITH_CONDITIONS` — siap UAT untuk alur tanpa serah terima dan sikap pesanan; belum siap produksi; DoD §9.4 10 dari 11, butir 1 sebagian; syarat C1–C6 |
| Register | Frontend **25 ✅ dari 32** (sebelumnya 23); backend tetap 37 ✅ dari 47; gabungan 62 dari 79 |
| Dokumen yang diselaraskan | Paragraf pengantar R3.13.1 `roadmap/frontend-roadmap.md` (versi kontrak `0.14.0`/`0.11.0`/`0.8.0`, `IGD-DEC-186`, `187`); baris *Snapshot source* R3.14 pada `roadmap/backend-roadmap.md` dan `roadmap/requirement-traceability.md` |
| Snapshot | Backend `rizkiG` `c1f79f79`; frontend `RizkiV2` `19ba512de` + working tree (`emergency-assessment-constant.jsx`, `emergency-assessment-status-action.utils.js`) |

## 0j. Amendment 30 September 2026 — observasi yang diakhiri sesudah disposisi dilaksanakan (`design-business-module`)

Revisi blueprint **tetap `8`**: amandemen sempit di dalam slice R3.14 yang sama — nol status, nol tabel, nol kolom,
nol endpoint, nol hak akses baru. Yang naik adalah versi tiga kontrak. Masukan `IGD-DEC-171`…`174`, fakta
`IGD-FACT-029`…`031`, dan `IGD-OQ-111` (`00-interview-decisions.md`, amendment pass 30 September 2026).

| Butir | Isi |
| --- | --- |
| Masalah yang ditutup | Temuan uji `BE-IGD-060` S3: sesudah disposisi dilaksanakan, observasi yang masih berjalan hanya dapat dibatalkan — *selesaikan* dan *eskalasi* sama-sama ditolak `409` oleh penjaga transisi, karena aksi observasi mencoba memindahkan kunjungan `Disposed` ke `AwaitingDisposition` atau `InTreatment` |
| Aturan baru | Pada kunjungan `Disposed`: *selesaikan observasi* diterima tanpa memindahkan status kunjungan, lalu penutupan susulan dicoba (`IGD-DEC-171`); *eskalasi* ditolak `409` dengan kalimat mengikat, observasi tetap aktif dan tetap menahan penutupan (`IGD-DEC-172`) |
| Snapshot source | Backend `rizkiG` `327ccad3` + working tree `BE-IGD-060` (belum di-commit). `EmergencyObservationController` dan `EmergencyObservationService` nol selisih terhadap `dce1f138` — `git diff --stat` kosong, diperiksa ulang pada pass ini. Fakta `IGD-FACT-029`…`031` cocok dengan source (`UpdateObservationStatus` `:287`–`:320`; `EmergencyVisitService.CanTransition` `:477`; `ValidateVisitClosureAsync` `:115`–`:124`). Frontend `RizkiV2` `2c2190858` |
| Schema | **Nol.** Nol migration |
| Endpoint | **Nol endpoint baru.** Satu perilaku berubah pada `PATCH /emergency-observations/{id}/observation-status` (API §9.1 nomor 5) |
| Hak akses | **Nol perubahan** — tetap `EmergencyObservation : Update` |
| Gelombang | `MVP-8`, `EPIC IGD-13`; `FR-IGD-092`, `093`; `AT-IGD-192`, `193`; DoD butir 10. Dikerjakan di `BE-IGD-061` (`IGD-DEC-173`) |
| Pertanyaan terbuka | `IGD-OQ-111` (pencatatan pasien yang memburuk sesudah disposisi dilaksanakan) — **tidak memblokir** |
| Dampak frontend — dicatat untuk `plan-module-delivery` | Kalimat konfirmasi di `OBSERVATION_STATUS_ACTIONS` (`src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx`) menyebut *"Kunjungan pasien berpindah ke Menunggu Tindak Lanjut"* untuk Selesaikan dan *"Kunjungan kembali ke status Sedang Ditangani"* untuk Eskalasi. Keduanya **tidak lagi benar** pada kunjungan `Disposed`. Pass ini tidak mengubah layar |
| Catatan, bukan keputusan | Observasi `Escalated` tidak menahan penutupan (`IGD-FACT-031`), sehingga dapat tertinggal pada kunjungan `Completed` dan hanya dapat dibatalkan — di luar `IGD-DEC-171` (state §9.5) |
| Penyelarasan status yang tertinggal dari `IGD-DEC-170` | Baris *"Status bagian ini: `draft`"* pada state §9, validation §11, integration §6 — **dan** juga API §9, `02-backend-architecture.md` §14, `04-prd-to-mvp.md` §9 yang sama-sama disetujui `IGD-DEC-170` — diperbarui menjadi `approved`. Baris `approved_by` kelima kontrak ditambah `IGD-DEC-170` |
| Status | **`approved`** — `IGD-DEC-175`, Rizki Gunawan, 30 September 2026, lewat pilihan interaktif *"Setujui seluruhnya"*; sementara menurut `IGD-DEC-174`. Kontrak terkunci; `plan-module-delivery` boleh memperluas `BE-IGD-061`. `flowcharts/penutupan-lewat-disposisi.md` tetap `draft` |

**Versi kontrak sesudah pass ini** (Rencana): API **`0.13.0`** (§9.1 nomor 5, §9.4 baris observasi), validation
**`0.10.0`** (§11.1 aturan 12–15), state **`0.7.0`** (§9.4 satu baris, §9.5), permission/audit **`0.6.0`** tidak
berubah (hanya baris `approved_by`), integration **`0.5.0`** tidak berubah (hanya baris status §6 dan `approved_by`).
Berkas turunan yang disentuh: `02-backend-architecture.md` §14.3–14.4, `04-prd-to-mvp.md` §9,
`flowcharts/penutupan-lewat-disposisi.md` (langkah 5 dan satu jalur pengecualian).

**Mengapa hash bagian 0i basi.** Hash 0i dihitung sebelum baris approval `IGD-DEC-170` ditulis ke kelima kontrak
dan ke decision log. Dihitung ulang 30 September 2026 sebelum pass ini menyunting apa pun: kelima kontrak dan
`00-interview-decisions.md` **berbeda** dari tabel 0i; `01`, `02`, `03`, `04`, `erd/data-dictionary.md`, dan
`flowcharts/penutupan-lewat-disposisi.md` **sama**. Tabel di bawah menggantikan tabel 0i.

**Hash sesudah pass ini** — dihitung **sesudah** baris status approval `IGD-DEC-175` ditulis. SHA-256 atas byte
working tree backend, belum di-commit (HEAD lokal `327ccad3`, `core.autocrlf = true`, berkas ber-LF). Manifest ini
sendiri tidak di-hash.

| Artifact | SHA-256 | Keterangan |
|---|---|---|
| `00-interview-decisions.md` | `96bba2e15b035336bbd89d5dcb743951694b551f1969ac1deb2ca0daa946748d` | berubah — `IGD-DEC-175` |
| `01-existing-capability-map.md` | `c1097d63363407dd79658dba09ebacf20241d1da0fb56cd7058b4440803189ec` | tidak berubah |
| `02-backend-architecture.md` | `6a22cf17880e394cb5d1cf5ff59e87b3d383cd5fb61283b982ac249092da4649` | berubah — §14 status, §14.3, §14.4 |
| `03-frontend-architecture.md` | `d3bfbe8af698fbfea4525c678a9470b969759b8e45d5ec873f65ea9d88da4960` | tidak berubah |
| `04-prd-to-mvp.md` | `650e4641200d1699ad2164dc0aae75f6b9a27831bb6285fffa5ab49b893b2e16` | berubah — §9 |
| `contracts/api-contract.md` | `e6b704d46ed2d0cf3c9ec4519dbc3f3fd716f4d84728c8ad7b58c1773b7f7c71` | `0.13.0` |
| `contracts/validation-matrix.md` | `4c3567b5a667ca2df5fa29bbba3c32cb9cbe6eb3c6b2559ff7d6f7eb086485fa` | `0.10.0` |
| `contracts/state-transition-matrix.md` | `20f0004cc202a7e98778832c98d0ae7b9f67a304c4947badb749ecbf997b5fdb` | `0.7.0` |
| `contracts/permission-audit-matrix.md` | `24234cb2dab7c73d1787a8ac24accd34539a7cc9cf580445dbb0f5840132e444` | `0.6.0` — baris `approved_by` saja |
| `contracts/integration-contract.md` | `0f0a544f107040f140f6abb117b67e1241da196ec9514e3ae747b3eba111bca0` | `0.5.0` — baris status §6 dan `approved_by` saja |
| `erd/data-dictionary.md` | `6edcedb0642bda2b20127da19029b868e7698707ba478137a2174982a0cb5552` | tidak berubah |
| `flowcharts/penutupan-lewat-disposisi.md` | `bddaab8a9328c3ff20fa3525423ec972a96ee9e3fee331554ce20d1ea55f0557` | berubah — langkah 5, satu jalur pengecualian; tetap `draft` |


### 0j.1 Perencanaan delivery 30 September 2026 (`plan-module-delivery`)

Kartu R3.14 diperluas atas `IGD-DEC-175`. Dua keputusan perencanaan diambil pemilik lewat pilihan interaktif:
`IGD-DEC-176` (`BE-IGD-062` sesuai kontrak; `Executed` tetap final; koreksi fakta API §9.1 nomor 3 — perilaku target
tidak berubah) dan `IGD-DEC-177` (tombol Eskalasi nonaktif beserta keterangan pada kunjungan `Disposed`).

| Butir | Isi |
| --- | --- |
| Backend | `BE-IGD-061` diperluas (pemetaan observasi pada kunjungan `Disposed`, acceptance 8–12); `BE-IGD-062` diperiksa ulang dan ditulis ulang; `BE-IGD-061`…`063` **siap** sesudah `BE-IGD-060` ✅. Nol kartu backend baru, nol migration |
| Frontend | `FE-IGD-042` **baru** — pasangan layar `BE-IGD-061`; ikut membereskan kalimat konfirmasi *Jalankan* yang keliru sejak `BE-IGD-060`. Grafik R3.13.1.1 baru; node `FE-IGD-041` dipindah dari grafik R3.12.1 (tergambar tanpa panah) |
| Traceability | `FR-IGD-092`, `093` → `BE-IGD-061` + `FE-IGD-042`; `IGD-DEC-171`…`177` ditelusuri; coverage gap nihil |
| Snapshot | Backend `rizkiG` `327ccad3` + working tree `BE-IGD-060`; frontend `RizkiV2` `2c2190858` |

**Hash yang menggantikan baris tabel 0j** — dua berkas berubah sesudah tabel 0j dihitung (keputusan
`IGD-DEC-176`/`177` dan koreksi API §9.1 nomor 3). Baris lain pada tabel 0j tetap berlaku.

| Artifact | SHA-256 |
|---|---|
| `00-interview-decisions.md` | `ef016ca57b99f29b3bdcc138a883469904da224694b333eda65c64f4870edd8c` |
| `contracts/api-contract.md` | `0cddfba16bcf676c37c232c1b90cf96f9bc46a659006f03bd3d7f55bc89dbac3` |


### 0j.2 Koreksi fakta kontrak saat pelaksanaan `BE-IGD-057` — 1 Oktober 2026

`IGD-DEC-178` (Rizki Gunawan, pilihan interaktif): batas alasan NoShow **250** karakter, bukan 500, karena kolom
`RegPatientEncounter.NoShowReason` bertipe `character varying(250)` dan task itu nol schema. Dua baris kontrak
dikoreksi — API §8.3.3 baris `reason`, validation §10.3 aturan 2 beserta pesannya. Nomor versi kontrak **tidak**
dinaikkan: bagian encounter-first tetap `approved`, dan koreksinya disetujui pemilik.

**Hash yang menggantikan baris tabel 0j dan 0j.1** untuk tiga berkas yang berubah:

| Artifact | SHA-256 |
|---|---|
| `00-interview-decisions.md` | `df8d9ae47c7bd63d1670cc5ff220b173e8ea102dd074fb430e36082957d7cbac` |
| `contracts/api-contract.md` | `23bf4c3c3a40ce804e5d84cc7716a1ff4286971f6bf020dea480e0005ef017e2` |
| `contracts/validation-matrix.md` | `e96dd407a3c6b2966a3308152ceb0d0b93b29f0e45a0854c05362a7a70f54be1` |


### 0j.3 Tambahan kontrak atas perintah pemilik — lima ruas kunjungan pada `start-triage` — 1 Oktober 2026

`IGD-DEC-179` (Rizki Gunawan, perintah tertulis pada percakapan): lokasi kedatangan, lokasi ditemukan, lokasi
trauma, waktu trauma, dan catatan kunjungan dipulihkan sebagai ruas **opsional** `POST /start-triage` dan isian
dialog Mulai Triage. Nol schema — kelima kolomnya sudah ada pada `EmgVisit`. Yang berubah pada kontrak: API
§8.3.2 (lima baris ruas dan satu kalimat), validation §10.2 (aturan 14 dan 15). Nomor versi kontrak **tidak**
dinaikkan: tambahannya aditif (permintaan lama tetap sah), bagian encounter-first tetap `approved`, dan
tambahannya diperintahkan pemilik. Aturan 15 (waktu trauma di masa depan ditolak) adalah usulan agent yang
boleh ditolak pemilik. Dikerjakan sebagai pengerjaan ulang `BE-IGD-055` dan `FE-IGD-036`.

**Hash yang menggantikan baris tabel 0j, 0j.1, dan 0j.2** untuk tiga berkas yang berubah:

| Artifact | SHA-256 |
|---|---|
| `00-interview-decisions.md` | `ce5c379f9820c76c61fe9b706031074563926fc544713b6f7e048069a7d7a93d` |
| `contracts/api-contract.md` | `653d1518c8e2766e40f60406e1cd9334cda08c020c16a430c00bf43fe02a3e32` |
| `contracts/validation-matrix.md` | `bc247e4763cd8eb3a431090e0b3d5ef9abd88303c95ec5bfbc55c094503bd48c` |

### 0j.5 Catatan rute petugas pada API §8.1 — 3 Oktober 2026

`IGD-DEC-182` (Rizki Gunawan, perintah tertulis pada percakapan): layar loket IGD membuat encounter lewat rute petugas
`POST /patient-encounters/admin`; rute tanpa akhiran adalah rute kiosk (policy `KioskRead`) yang menolak petugas loket
`403` (uji `C4-01`). API §8.1 diberi catatan dua rute pembuat encounter beserta otorisasinya. **Nomor versi kontrak
tidak dinaikkan** dan nol perubahan backend — pola `IGD-DEC-178`/`179` (0j.2, 0j.3). Perbaikan layar dikerjakan sebagai
pengerjaan ulang `FE-IGD-036`.

**Hash yang menggantikan baris tabel 0j sampai 0j.4** untuk dua berkas yang berubah (SHA-256 atas isi berakhir-baris
LF; HEAD lokal `a23e5e21`, belum di-commit):

| Artifact | SHA-256 |
|---|---|
| `00-interview-decisions.md` | `d65a1b682451d497047dffe620bf4c6ad9534b7857f02ca055cf607a1339ac12` |
| `contracts/api-contract.md` | `faf2f06d0b0f0f0a98a43117e5f3b3fa49511a96f3f599d1d2319a263f129c22` |

### 0j.4 Jawaban `IGD-OQ-108` — 3 Oktober 2026

`IGD-DEC-180` (Rizki Gunawan, pilihan interaktif pada review status modul): kontrak berlaku apa adanya — encounter
kedua hasil pendaftaran ganda beralasan tidak dapat dimulai selama kunjungan IGD lama pasien belum berakhir
(validation §10.2 aturan 7). Nol perubahan kontrak, source, dan schema; nomor versi kontrak **tidak** dinaikkan.
Keterbatasan yang diterima — pasien yang kembali saat kunjungan lamanya tertahan `BE-IGD-039` — dicatat pada
keputusan itu. Yang berubah hanya decision log.

**Hash yang menggantikan baris `00-interview-decisions.md` pada tabel 0j, 0j.1, 0j.2, dan 0j.3** (SHA-256 atas isi
berakhir-baris LF — konvensi tabel 0i/0j; working tree checkout ber-CRLF karena `core.autocrlf = true`, sehingga
byte-nya dinormalkan dulu. HEAD lokal `5af6ef3b`, belum di-commit):

| Artifact | SHA-256 |
|---|---|
| `00-interview-decisions.md` | `07c5c0728869759744fd28931321a7f78db4a6b112fe55bae87c846d19bfd62c` |

*Diperbarui 3 Oktober 2026:* nilai di atas sudah mencakup penyelarasan status `IGD-OQ-093` (syarat C6) dan
keputusan `IGD-DEC-181` (persetujuan Registrasi atas `IGD-REQ-002`, PRD §8.6 butir 8). Keduanya tidak mengubah kontrak.

*Dikoreksi 3 Oktober 2026 (audit kesiapan `MVP-7`):* hash pertama yang ditulis pada bagian ini dihitung atas byte
CRLF sehingga tidak sebanding dengan tabel lain. Kelima kontrak diperiksa ulang dengan cara yang sama: isinya
**identik** dengan hash tabel 0j/0j.3 (`653d1518…`, `bc247e47…`, `20f0004c…`, `24234cb2…`, `0f0a544f…`).

---

## 0i. Amendment 23 September 2026 — penutupan kunjungan lewat disposisi (`design-business-module`)

Revisi naik ke **`8`**. Slice sempit: satu aturan baru, satu kolom baru, nol tabel baru, nol endpoint baru.
Masukan `IGD-DEC-163`…`169` (`00-interview-decisions.md`, amendment pass 23 September 2026, seluruhnya `approved`
oleh Rizki Gunawan) dan capability map suplemen revision **3.3** @ backend `dce1f138` / frontend `c941012ac`.

| Butir | Isi |
| --- | --- |
| Masalah yang ditutup | Encounter IGD tertinggal terbuka karena penutupan kunjungan bergantung pada ingatan petugas. Diverifikasi dari source: disposisi nol menyentuh encounter, admisi ranap membuat encounter `Inpatient` sendiri, penutup kiosk tidak menyentuh IGD |
| Aturan baru | Disposisi berstatus dilaksanakan menutup kunjungan; bila penjaga menolak, kunjungan menunggu penutupan dan tertutup otomatis saat penahan terakhir dibereskan, atas nama petugas yang membereskannya |
| Schema | **Satu kolom** `EmgVisit.ClosedByDispositionId` (`uuid?`, FK `EmgDisposition`, `Restrict`). Satu migration `AddEmergencyVisitClosureSource`, aditif, `Down()` berpenjaga |
| Endpoint | **Nol endpoint baru.** Satu saringan dan dua ruas response pada `GET /emergency-visits`; satu penolakan baru pada `PATCH /emergency-dispositions/{id}/disposition-status` |
| Hak akses | **Nol resource dan nol aksi baru** — penutupan menumpang izin aksi pemicunya |
| Gelombang | `MVP-8`, `EPIC IGD-13`, `FR-IGD-086`…`091`, `AT-IGD-186`…`191` |
| Keterbatasan yang diterima | `IGD-DEC-169` — sesudah kunjungan tertutup, Bank Darah menolak order darah baru dan alokasi kantong, Laboratorium menolak pemesanan baru. Nol perubahan pada kedua modul itu |
| Tidak berlaku surut | `IGD-DEC-167` — `BE-IGD-052` tidak diubah; kelas K2 lama tetap ditangani petugas |
| Pertanyaan terbuka | `IGD-OQ-110` (jumlah encounter nonaktif tanpa tanda berakhir pada data lama) — **tidak memblokir** |
| Status | **`approved`** — `IGD-DEC-170`, Rizki Gunawan, 23 September 2026. Kontrak terkunci; `plan-module-delivery` boleh menyusun kartu |

**Versi kontrak sesudah pass ini** (seluruhnya `draft`, Rencana): API **`0.12.0`** (bagian 9), validation
**`0.9.0`** (bagian 11), state **`0.6.0`** (bagian 9), permission/audit **`0.6.0`** (bagian 8), integration
**`0.5.0`** (bagian 6). Bagian encounter-first yang `approved` lewat `IGD-DEC-157` **tidak disentuh**, sehingga
kunci hash lima kontrak pada bagian 2 tetap berlaku untuk bagian-bagian itu — yang berubah adalah berkasnya secara
keseluruhan, jadi hash berkas di bawah menggantikan hash lama.

**Hash sesudah pass ini** — SHA-256 atas byte working tree backend, belum di-commit (HEAD lokal `dce1f138`,
`core.autocrlf = true`).

| Artifact | SHA-256 |
|---|---|
| `00-interview-decisions.md` | `00e06680bdccb93b4f86f45b63be3e073b4d035566073e2e0051c076e2b8f287` |
| `01-existing-capability-map.md` | `c1097d63363407dd79658dba09ebacf20241d1da0fb56cd7058b4440803189ec` |
| `02-backend-architecture.md` | `7a1a079cf330b1b1cb3d4d4d2e92e323c8cf12d118152106dd7d1e5deb5150e2` |
| `03-frontend-architecture.md` | `d3bfbe8af698fbfea4525c678a9470b969759b8e45d5ec873f65ea9d88da4960` |
| `04-prd-to-mvp.md` | `e576f3e9d9741993686078a5e7a1dcce9f78b28ac7ff6f5cc6788dfcf9bbbfad` |
| `contracts/api-contract.md` | `6ec241785c9cdbcce1810d20185fad0d236a3744c2a5b1672fa40cfd9f40c236` |
| `contracts/validation-matrix.md` | `de9f11bf1942fa729efaa694a246d9ac457b0f09d533bc3fb22b1522e83ad16b` |
| `contracts/state-transition-matrix.md` | `c26282fd577eb415ca7172a9e17042a9311284c7d7af768e2e271654d29bacdd` |
| `contracts/permission-audit-matrix.md` | `314d5448c18078c871c7ffcac05182e3293156a93ddb690b0735798c951069c6` |
| `contracts/integration-contract.md` | `ceb595a8c968af028b78d2a4bfd5cadeaf8ffbcccc9d3beb10b6b71895a9a4f9` |
| `erd/data-dictionary.md` | `6edcedb0642bda2b20127da19029b868e7698707ba478137a2174982a0cb5552` |
| `flowcharts/penutupan-lewat-disposisi.md` (**baru**) | `ad27f0705efdb0509d52eb5ccd4adbc7ef5ce36ec6a272148d7600a1a9846db8` |

**Basi sejak 23 September 2026** untuk kelima kontrak dan `00-interview-decisions.md` — dihitung sebelum baris
approval `IGD-DEC-170` ditulis. Digantikan tabel hash bagian 0j.

---

## 0h. Approval dan `plan-module-delivery` final — 22 September 2026 (penutup)

**Revisi blueprint naik menjadi `7`.** Pemilik menyetujui ringkasan persetujuan yang diajukan agent — jawabannya
dikutip apa adanya di `00-interview-decisions.md` bagian *Approval desain encounter-first*.

| Hal | Isi |
| --- | --- |
| Disetujui | Bagian encounter-first: API `0.11.0` §8, validation `0.8.0` §10, state `0.5.0` §8, integration `0.4.0` §5, permission/audit `0.5.0` §7 (`IGD-DEC-157`) |
| Koreksi yang ikut disetujui | **B1** integration §5.2 menyebut `EmergencyEpisodeRule`; **B2** pesan validation §10.4 aturan 4 (teks usulan agent — pemilik menjawab dengan placeholder `"..."`); **B4** kalimat Swagger (hanya Development, `Program.cs:1330`) di decision log dan PRD §8.3 |
| Keterbatasan yang diterima | **B3** — izin `EmergencyVisit : Create` dipakai bersama pra-cek loket dan Mulai Triage (`IGD-DEC-158`) |
| Butir tinjauan desain | `IGD-OQ-104`…`107` → `IGD-DEC-159`…`162`, disahkan apa adanya |
| Tidak diajukan, tetap `draft` | `02` §13, `03` §13, kamus data §6, `flowcharts/`, PRD §8, `AT-IGD-166`…`185` — turunan kontrak; catatan OQ di dalamnya diperbarui |
| Delivery | Backend R3.13 `BE-IGD-051`…`059` (baru `057`, `058`, `059`); frontend R3.12 `FE-IGD-035`…`040` (baru `038`, `039`, `040`); kandidat tautkan U1 dibatalkan; `BE-IGD-056`/`FE-IGD-037` ⛔ `S7`, isi kartu dibekukan |
| Pertanyaan baru | `IGD-OQ-108` — encounter kedua hasil override dan Mulai Triage (tidak menahan) |
| Snapshot | Source backend `0d13f3a8`; dokumen sebelumnya di-commit pemilik sebagai `69953e98` (lokal, belum di-push); frontend `c941012ac`. Perubahan pass ini **belum** di-commit |
| `koreksi_desain_tertunda` | Nihil untuk slice encounter-first. Celah desain layar dicatat di traceability R3.13.5 (tempat melengkapi ruas kunjungan sesudah Tangani Segera) |

---

## 0g. Amendment 22 September 2026 — desain encounter-first (`design-business-module`)

**Revisi blueprint tetap `6` (`draft`).** Pass ini menyelaraskan desain dan kontrak dengan keputusan
`IGD-DEC-139`…`154` (`IGD-DEC-140` U1 dan `IGD-DEC-149` `superseded`). Perubahannya material — titik lahir
kunjungan pindah, IGD menulis status encounter Registrasi, tiga tabel baru — sehingga **layak menaikkan revisi
menjadi `7` saat pemilik menyetujuinya**; agent tidak menaikkan revisi sendiri karena revisi mengikat approval.

| Hal | Isi |
| --- | --- |
| Gerbang masuk | `requirement-completeness-gate` slice encounter-first → `PARTIALLY_READY` ([evidence](evidence/02-requirement-completeness-gate.md)); `hospital-domain-architect` tidak dipakai (`DOMAIN_ARCHITECTURE_NOT_RUN`, alasan gate §10) |
| Bentuk blueprint | `SINGLE`, `USER_CONFIRMED` |
| Snapshot | backend `0d13f3a8`, frontend `c941012ac` — capability map suplemen 3.2 |
| Kontrak | API `0.11.0` §8, validation `0.8.0` §10, state `0.5.0` §8, integration `0.4.0` §5, permission/audit `0.5.0` §7 — semuanya `draft`, **Rencana (belum tersedia)** |
| Arsitektur | `02-backend-architecture.md` §13 (3 tabel baru `EmgDuplicateEpisodeOverride`, `EmgEncounterReconciliationRun`, `EmgEncounterReconciliationItem`; `EmgVisit` +3 kolom; nol kolom baru `RegPatientEncounter`; nol baris `Program.cs`; 3 migration dijalankan Rizki); `03-frontend-architecture.md` §13 (nol butir menu baru) |
| Kamus data | `erd/data-dictionary.md` §6 |
| Flowchart | `flowcharts/` baru — alur utama + 5 proses |
| PRD | `04-prd-to-mvp.md` §8 — `EPIC IGD-11` (`FR-IGD-069`…`085`, gelombang `MVP-7`), `EPIC IGD-12` `OPEN DECISION` |
| Uji | `testing/acceptance-test-matrix.md` — `AT-IGD-166`…`185` |
| Ditahan | `S7` kelayakan dokter jaga — `IGD-OQ-102`, `IGD-OQ-103` |
| Butir tinjauan desain | `IGD-OQ-104`…`107` — tidak memblokir desain |
| **Tidak** disentuh | Kartu roadmap R3.13/R3.12 (milik `plan-module-delivery` final), laporan task, source aplikasi |

### 0g.1 Kompatibilitas

Empat perilaku yang **memutus** bila dirilis: penolakan pendaftaran Emergency ganda di pintu encounter;
penolakan `PATCH /patient-encounters/{id}/status` untuk Emergency; penolakan `PATCH …/cancel` Emergency yang sudah
punya kunjungan; penguncian `PUT /emergency-visits/{id}`. Urutan rilis wajib: penjaga backend sebelum loket berhenti
membuat kunjungan (API §8.1).

---

## 0b. Amendment 15 September 2026 — pemeriksaan status terhadap source

Dokumen IGD tidak diperbarui sejak 28 Agustus 2026 (`eb18ac6b`). Pemeriksaan status dijalankan
pada backend `e89907c5` dan frontend `43adae648`; buktinya di
[evidence/2026-09-15-pemeriksaan-status.md](evidence/2026-09-15-pemeriksaan-status.md).

**Revisi blueprint tetap `6`.** Arsitektur target, kontrak, dan lingkup task tidak berubah. Yang
berubah adalah keputusan, status, dan bukti.

### 0b.1 Keputusan yang dicatat

| Keputusan | Isi singkat |
| --- | --- |
| `IGD-DEC-110` | Automated test bukan acceptance criterion; angka test lama sah sebagai bukti historis |
| `IGD-DEC-111` | Menggantikan `IGD-DEC-099`: radiologi dipesan lewat `POST rad-orders`; penyambungan ditahan sampai hasil bacaan radiologi dapat dirilis |
| `IGD-DEC-112` | Pengaturan IGD tersirat (`f76ebaab`) disahkan, termasuk akibat sampingnya pada disposisi |
| `IGD-DEC-113` | Laporan task gabungan lama diterima sebagai bukti |
| `IGD-DEC-114` | `MVP-5` sebagian; `EPIC IGD-04` dijadwalkan |
| `IGD-DEC-115` | Catatan penutupan observasi `Completed` → `CompletionSummary`; `IGD-OQ-083` dibuka untuk `Cancelled` |

### 0b.2 Perubahan dari luar IGD yang menyentuh modul ini

| Commit | Siapa | Dampak pada IGD |
| --- | --- | --- |
| `58c61a5b` (10 Sep) | Yasmina | Rename `TrxPatientEncounter` → `RegPatientEncounter` dan `TrxPrescription` → `PhmPrescription` pada empat berkas IGD. Konsisten di kode. Migration `20260910031500` **sudah diterapkan** pada `QuilvianNewDevRizki` — bukti dari owner: Rizki menjalankan `dotnet ef migrations list --no-build` pada 15 September 2026, dan tidak ada migration berlabel `Pending`. *(Dikoreksi 15 September 2026; sebelumnya tertulis "belum diketahui".)* |
| `50ccf615` (10 Sep) | Rivenjxv | Komentar `EmergencyOrderKind` saja |
| `259d53ce`, `a517cdbd` (4 Sep) | Rivenjxv | `GET lab-orders` berhalaman + `encounterId`. Tab Penunjang IGD belum menyesuaikan — cacat baru |
| `cefd927d`, `b3ab542e` (11 Sep) | lead, Rizki | Seluruh proyek test backend dihapus, termasuk test IGD |

### 0b.3 Bukti yang `STALE`

| Artefak/bukti | SHA tercatat | SHA terkini | Tinjauan dampak |
| --- | --- | --- | --- |
| `01-existing-capability-map.md` revision `3` + suplemen `3.1` | `f69e9e48` / `300922c` | `e89907c5` | **Wajib** sebelum gelombang berikutnya menyentuh `ClinicalManagement`, `RegistrationManagement`, `PharmacyManagement`, `LaboratoryManagement`, atau `RadiologyManagement` |
| Nomor baris pada kartu roadmap revision `3` | `300922c` | `e89907c5` | Sudah ditinjau untuk acceptance criteria `BE-IGD-017`…`039` dan `FE-IGD-010`…`022` pada 15 September 2026; letak source terkini ada pada baris `Status` kartu |
| ~~`artifact_hashes` bagian 2~~ | 24 Agustus 2026 | 15 September 2026 | **Ditutup 15 September 2026** — dihitung ulang pada pass penyelarasan teks (bagian 0c) |

### 0b.4 Temuan terbuka yang lahir dari pemeriksaan ini

| Bukti | Temuan | Pemilik tindak lanjut |
| --- | --- | --- |
| `IGD-EV-112` | Tab Penunjang memanggil `lab-orders` tanpa `encounterId` | IGD (frontend) |
| `IGD-EV-115` | Layar masih menyatakan modul Radiologi belum ada | IGD (frontend), `IGD-DEC-111` |
| `IGD-EV-117` | Endpoint daftar klinis menjawab permintaan tanpa filter dengan data semua pasien | Pemilik `ClinicalManagement` (sementara: IGD, `IGD-DEC-107`) |
| `IGD-EV-121` | `Outpatient` belum dicabut walau syarat `IGD-DEC-109` terpenuhi | IGD (backend) |
| `IGD-EV-122` | Penolakan penutupan kunjungan tidak menyebut pesanan mana | IGD (backend) |
| `IGD-EV-123` | `FE-IGD-014` kriteria 2 tidak ada; `FE-IGD-017` pelaku tampil sebagai ID | IGD (frontend) |

### 0b.5 Perencanaan delivery 15 September 2026 (kedua)

`plan-module-delivery` menambahkan task pada roadmap revision `3` **tanpa** mengubah task lama.
Revision blueprint tetap `6`.

| Keputusan | Isi singkat |
| --- | --- |
| `IGD-DEC-116` | API §3 `Emergency Doctor Assignment` jadi target kontrak; nama tabel `EmgDoctorAssignment` |
| `IGD-DEC-117` | Dokter aktif pada waktu tertentu lewat query `at` pada `GET /active`, tanpa endpoint baru |
| `IGD-DEC-118` | Validation §6 aturan 4 menyebut hingga 5 pesanan + *"dan N lainnya"*, memakai `ValidatePesananSebelumPenutupanAsync` |
| `IGD-DEC-119` | Catatan status observasi paling banyak 1000 karakter; lebih → `400`; tanpa migration, tanpa pemotongan |
| `IGD-DEC-120` | Teks penolakan `Outpatient` memakai teks source; `BE-IGD-042` terblokir sampai owner menyerahkan jumlah baris |
| `IGD-DEC-121` | Kesimpulan observasi opsional |

| Task baru | Keadaan |
| --- | --- |
| `BE-IGD-040`, `041`, `043`; `FE-IGD-023`, `025`, `026` | Direncanakan; tidak menunggu pihak lain |
| `BE-IGD-044` → `BE-IGD-045` → `FE-IGD-027` (`EPIC IGD-04`) | Direncanakan; migration `BE-IGD-044` dikerjakan Rizki |
| `BE-IGD-040` → `FE-IGD-024` | Direncanakan |
| `BE-IGD-042` | ⛔ menunggu OWNER DATA CONFIRMATION |
| `BE-IGD-017` | Dibuka ulang untuk laporan tracked susulan |
| `FE-IGD-019` | Kartu susulan, ✅ |

Rentang requirement `EPIC IGD-04` dikoreksi menjadi `FR-IGD-016`…`021` (`FR-IGD-022` milik
`EPIC IGD-05`).

### 0c. Penyelarasan teks berkas kontrak — 15 September 2026 (ketiga)

Pass `design-business-module` menyelaraskan teks berkas kontrak dengan `IGD-DEC-116`…`121` yang
sudah `approved`. **Makna keputusan tidak diubah**; yang berubah hanya teks yang tertinggal.
**Revisi blueprint tetap `6`** — arsitektur target, tabel, endpoint, dan lingkup task tidak
bertambah. Status seluruh berkas tetap `draft`; tidak ada bagian yang ditandai `approved` oleh
pass ini.

| # | Berkas dan bagian | Perubahan | Keputusan |
| ---: | --- | --- | --- |
| 1 | `contracts/api-contract.md` §3, §3.1 baru | `GET /active` menerima query `at`; tanpa `at` = sekarang; tidak ada dokter pada waktu itu → `404`; dilarang endpoint terpisah. Judul §3 diberi label *Rencana (belum tersedia)* | `IGD-DEC-117` |
| 2 | `erd/data-dictionary.md` §4 dan §5.3; `erd/00-context-erd.md`; `erd/emergency-episode.md`; `02-backend-architecture.md` §2.1, §3.1, §3.7, §4, §5, §11.2 | `TrxEmergencyDoctorAssignment` → `EmgDoctorAssignment` (model, tabel, configuration). Nama service, controller, DTO, resource izin, dan isi kolom **tidak** berubah | `IGD-DEC-116` |
| 3 | `contracts/validation-matrix.md` §6 aturan 4, §6.1 baru | Pesan menyebut hingga 5 pesanan + *"dan N lainnya"*; kode `409` dan kondisi tetap | `IGD-DEC-118` |
| 4 | `contracts/validation-matrix.md` §1 aturan 2 | Teks penolakan jenis kunjungan memakai teks source | `IGD-DEC-120` |
| 5 | `contracts/validation-matrix.md` §8 baru; `contracts/api-contract.md` §5 | Catatan `Completed`/`Escalated` > 1000 karakter → `400`, dilarang dipotong; catatan kosong pada `Completed` tetap diterima | `IGD-DEC-119`, `IGD-DEC-121` |

**Versi:** API `0.4.0 → 0.5.0`, validation `0.4.0 → 0.5.0`. Hash bagian 2 dihitung ulang.

**Sisa yang sengaja tidak disunting pass ini:**

| Sisa | Alasan |
| --- | --- |
| Judul `contracts/state-transition-matrix.md` §6 masih `TrxEmergencyDoctorAssignment` | Tidak termasuk daftar koreksi `IGD-DEC-116`, dan §6 berstatus `approved` (`IGD-DEC-108`). Dibaca sebagai `EmgDoctorAssignment` sesuai `IGD-DEC-116`; diselaraskan bila state contract dibuka lagi |
| Kalimat *"Teks berkas kontrak … belum diselaraskan"* pada `00-interview-decisions.md` bagian keputusan 15 September 2026 (kedua) | Decision log keluaran wawancara, tidak disunting pass desain (pola bagian 4.0). Bagian 0c ini yang mencatat penutupannya |
| Nama rancangan lama `TrxEmergency*` untuk entity IGD lain pada ERD dan arsitektur backend | Berlaku aturan kolom `module`: prefix lama hanya berlaku pada artefak yang disusun sebelum 27 Agustus 2026. Bukan bagian keputusan mana pun |
| Sebutan nama lama pada `roadmap/backend-roadmap.md` dan `roadmap/requirement-traceability.md` | Keluaran `plan-module-delivery`; teksnya menjelaskan penggantian nama, bukan memakai nama lama sebagai target |


### 0d. Pemantauan observasi bertanda vital — 16 September 2026

Pass `design-business-module` menutup audit Observasi
([evidence](evidence/2026-09-15-audit-observasi-v1-v2.md)) menjadi keputusan, kontrak, dan dua
kartu task. **Revisi blueprint tetap `6`**: tidak ada tabel baru, tidak ada kolom baru, dan
tidak ada migration. Status seluruh berkas tetap `draft`.

| Keputusan | Isi singkat |
| --- | --- |
| `IGD-DEC-122` | Tanda vital ditautkan lewat `PatientVitalSignId`, tidak disalin; pilihan terbatas pada pasien dan encounter yang sama |
| `IGD-DEC-123` | ABCDE terakhir dibaca saja; evaluasi ditulis pada `ClinicalConditionSummary`; tanpa kolom ABCDE baru |
| `IGD-DEC-124` | Alat bantu jalan napas belum terstruktur; ditulis pada `InterventionSummary`; kepemilikan tetap terbuka |
| `IGD-DEC-125` | Jenis oksigen memakai enum `ClinicalManagement` apa adanya; `Other` + catatan untuk yang belum ada |
| `IGD-DEC-126` | Periode `Completed`/`Cancelled` menolak pemantauan baru dengan `409`; bukan larangan permanen atas dokumentasi susulan |

| Artefak | Perubahan |
| --- | --- |
| `contracts/api-contract.md` | `0.5.0` → **`0.6.0`**, aditif. Bagian 7 baru; bagian 5 tidak lagi memuat `Emergency Observation Detail` |
| `contracts/validation-matrix.md` | `0.5.0` → **`0.6.0`**, aditif. Bagian 9 baru beserta urutan pemeriksaan 9.1 |
| `02-backend-architecture.md` | Bagian 12 baru |
| `03-frontend-architecture.md` | Bagian 12 baru |
| `roadmap/backend-roadmap.md` | Gelombang R3.9 dan kartu `BE-IGD-046` |
| `roadmap/frontend-roadmap.md` | Gelombang R3.7 dan kartu `FE-IGD-028` |
| `roadmap/requirement-traceability.md` | Bagian R3.5 |

**Pertanyaan yang tetap terbuka:** `IGD-OQ-089` (bentuk terstruktur alat jalan napas) dan
`IGD-OQ-090` (entri susulan setelah periode observasi ditutup). Keduanya **tidak** menahan
`BE-IGD-046` maupun `FE-IGD-028`.

**Yang sengaja tidak disentuh pass ini:** `04-prd-to-mvp.md` — pemantauan observasi memakai
kapabilitas `IGD-CAP-26` dan `IGD-CAP-21` yang sudah tercatat `EXISTING / REUSE`, dan tidak ada
epic maupun functional requirement baru yang lahir; coverage gap-nya dicatat pada
`requirement-traceability.md` bagian R3.5.3. `erd/` juga tidak berubah karena tidak ada kolom
baru.

---

## 0. Gerbang kemampuan rumah sakit — BELUM TERPENUHI

Modul IGD **tidak memiliki** dua artefak hulu yang diwajibkan untuk kemampuan bisnis rumah
sakit:

| Artefak | IGD | Rawat Inap sebagai pembanding |
|---|---|---|
| `evidence/02-requirement-completeness-gate.md` | **Tidak ada** | Ada |
| `evidence/03-hospital-domain-architecture.md` | **Tidak ada** | Ada |

Akibatnya modul IGD tidak punya klasifikasi kesiapan requirement per slice. Substansinya
tersebar pada 88 keputusan, bukan pada berkas hulu tersendiri.

**Diperbarui 22 September 2026:** gate dijalankan **untuk slice encounter-first saja** — hasil `PARTIALLY_READY` ([evidence/02-requirement-completeness-gate.md](evidence/02-requirement-completeness-gate.md)). Area IGD lain tetap tanpa klasifikasi.

Gerbang ini **tidak** ditandai terpenuhi. Bila Product/Domain Owner menghendaki kesetaraan
dengan Rawat Inap, `/requirement-completeness-gate` dan `hospital-domain-architect` perlu
dijalankan lebih dulu dan revisi ini ditinjau ulang terhadap hasilnya.

---

## 0a. Yang berubah pada revision 6 — 26 Agustus 2026

Revisi sempit. **Empat koreksi** yang diminta Product/Domain Owner setelah Scope Pass ketiga,
ditambah satu penegasan arti. Tidak ada tabel baru, tidak ada endpoint yang dibuang.

| # | Koreksi | Keputusan | Berkas |
| ---: | --- | --- | --- |
| 1 | `EmergencyOrderAction` menjadi **`Continue`=1, `Handover`=2, `Cancel`=9** | `IGD-DEC-100` | `02-…` §3.4, `erd/emergency-departure.md` |
| 2 | `EmergencyOrderKind` menambah **`RadiologyOrder`=4** | `IGD-DEC-099` | Sama |
| 3 | **`EmergencyOrderSource`** memisahkan pesanan internal dari luar sistem; `OrderReferenceId` menjadi nullable, `ExternalReference` dan `OrderDescription` ditambahkan | `IGD-DEC-103` | Sama, + `contracts/api-contract.md` §2.3 |
| 4 | **`EmergencyOrderAcceptanceStatus`** — lifecycle penerimaan **per pesanan**, terpisah dari `EmergencyHandoverStatus` | `IGD-DEC-102` | Sama, + `contracts/state-transition-matrix.md` §6a |
| 5 | Arti validation §2 aturan 5 dipertegas per status kunjungan | `IGD-DEC-104` | `contracts/validation-matrix.md` §2.1 |

### 0a.1 Mengapa `Completed` bukan sekadar salah nama

Daftar sikap **hanya memuat pesanan yang belum selesai**, sehingga `Completed` adalah nilai
yang tidak pernah terpakai. Yang justru tidak punya nilai adalah keadaan sebenarnya: pesanan
yang **masih berjalan** dan akan diproses sampai hasil final meski pasien sudah pergi.

### 0a.2 Koreksi — revisi 6 **bukan** murni aditif

> **Klaim yang diperbaiki.** Terbitan pertama revisi 6 menyatakan kenaikan `0.3.0 → 0.4.0`
> bersifat *"aditif — nol bagian lama diubah"*. **Klaim itu salah.** Endpoint lama diganti, dan
> beberapa bagian validation berubah teksnya. Diperbaiki pada correction pass 26 Agustus 2026.

#### Yang benar-benar berubah, bukan sekadar bertambah

| # | Perubahan | Sifat |
| ---: | --- | --- |
| 1 | `GET /{id}/pending-orders` dan `POST /{id}/order-actions` **dihapus** dari tabel endpoint, diganti lima route keluarga `order-items` | **Memutus** secara kontrak — meski nol pemakai nyata, karena keduanya belum pernah diimplementasikan |
| 2 | Validation §5 aturan 2: *"Sikap `Cancelled` wajib beralasan"* → *"Sikap **`Cancel`** wajib beralasan"* | Teks berubah mengikuti `IGD-DEC-100` |
| 3 | Validation §5 aturan 4: *"Pemeriksaan penunjang tidak ikut dihitung"* → *"…tidak ikut dihitung **otomatis**"* | Teks berubah — artinya menyempit, bukan sekadar bertambah |
| 4 | `EmergencyOrderAction` dan `EmergencyOrderKind` **diganti nilainya**, bukan ditambah | Entitas `New` yang belum diimplementasikan; nol data terdampak |
| 5 | `OrderReferenceId` berubah dari wajib menjadi **nullable** | Sama seperti nomor 4 |
| 6 | Rumusan unique constraint diganti seluruhnya | Correction pass; rumusan lama **tidak dapat ditegakkan** — lihat `02-backend-architecture.md` §11.2 |

#### Satu baris `approved` yang ikut tersentuh

| Bagian | Yang berubah | Yang **tidak** berubah |
| --- | --- | --- |
| Validation §2 **aturan 5** | Kolom *Keputusan* bertambah *"; artinya dipertegas `IGD-DEC-104`"* | Kolom **Aturan, Kode, dan Pesan identik** — isi normatifnya utuh |
| Validation §2 aturan 4 | — | **Seluruhnya identik** |
| State §1, §1.1, §1.2 | — | **Seluruhnya identik.** `§6a` adalah bagian baru yang berdiri sendiri |

Perubahan pada aturan 5 hanyalah rujukan ke keputusan yang menafsirkannya, ditetapkan
**approver yang sama** lewat `IGD-DEC-104`. Aturan yang ditegakkan kode **tidak berubah**, dan
`BE-IGD-019` yang berjalan di atasnya tetap sahih.

Meski begitu, menyebutnya "teks identik" **tidak akurat** dan sudah diperbaiki di sini.

#### Akibat pada penomoran versi

Karena bukan murni aditif, kenaikan `0.3.0 → 0.4.0` pada API dan validation adalah
**perubahan yang memutus pada tingkat kontrak**, bukan penambahan. State `0.4.0` tetap aditif —
`§6a` murni bagian baru.

### 0a.3 Gerbang yang dilanggar sebagian, dan alasannya dicatat

| Gerbang `/qv-design` | Keadaan |
| --- | --- |
| Decision log `approved` | **Tidak terpenuhi.** `IGD-DEC-099`…`104` seluruhnya `draft`. Sama seperti revisi 5, yang juga disusun di atas keputusan `draft`; keluaran desain pun `draft` |
| Capability map terbaru | **Stale** — revision `3` dihitung pada `f69e9e48`, `HEAD` kini `300922c`. **Tidak berdampak pada revisi ini**: kelima entitas yang dikoreksi — `EmergencyOrderAction`, `EmergencyOrderKind`, `TrxEmergencyHandoverOrderItem`, `TrxEmergencyDeparture`, `EmergencyPhysicalStatus` — **nol berkas di source**. Tidak ada perilaku existing yang dapat salah dibaca |

Capability map tetap **perlu** diperbarui sebelum gelombang yang menyentuh kode yang sudah ada.

---

### 0a.4 Revisi 6 tetap `draft` — approval yang diajukan

**Tidak satu pun bagian revisi 6 ditandai `approved`.** Correction pass 26 Agustus 2026
menegaskannya kembali atas permintaan Product/Domain Owner.

`IGD-DEC-093` yang lama **tetap berlaku apa adanya** dan **tidak diperluas** oleh revisi ini:
ia menyetujui state §1/§1.1/§1.2 dan validation §2 aturan 4–5, dan hanya itu. `BE-IGD-018`,
`019`, dan `020` berjalan di atas irisan itu — sah, dan tidak terpengaruh revisi 6.

#### Yang membutuhkan approval, dan dari siapa

| Bagian | Approver | Kenapa mereka |
| --- | --- | --- |
| `IGD-DEC-100` — tiga sikap pesanan, larangan pembatalan otomatis | **Clinical Governance** | Menentukan pesanan klinis mana yang boleh dihentikan saat pasien pergi |
| `IGD-DEC-101` — sikap pesanan lab ditetapkan manual klinisi | **Clinical Governance** + **pemilik `LaboratoryManagement`** | Menetapkan sikap tanpa data status adalah penilaian klinis; dan lab yang menanggung akibatnya |
| `IGD-DEC-102` — penerimaan per pesanan, penolakan tidak membatalkan penerimaan pasien | **Nursing authority** | Serah terima antar-unit adalah pekerjaan keperawatan |
| Permission §3.1 — kewenangan `accept`/`reject` atas unit tujuan | **Nursing authority** + Security/Privacy owner | Menentukan siapa berhak menyatakan penerimaan |
| Validation §5 aturan 5 — larangan menampilkan sikap lab seolah dari `LabOrder` | **Pemilik `LaboratoryManagement`** | Melindungi lab dari klaim yang tidak mereka buat |
| `02-backend-architecture.md` §11.1 — pembentukan baris `Medication` dan `Procedure` | Pemilik `PharmacyManagement` dan `ClinicalManagement` | Membaca tabel milik mereka |

#### Tiga peran approver itu **belum ditunjuk**

Clinical Governance, Nursing authority, dan pemilik `LaboratoryManagement` seluruhnya masih
kosong. Permintaan penunjukannya sudah disiapkan di
`approval-requests/2026-08-24-permintaan-penunjukan-pemilik-modul.md`, dan `IGD-OQ-081`
mencatat langkah termurahnya: menanyakan `andryzainhome` yang membuat fondasi
`LaboratoryManagement` lewat commit `1a8a9ce`.

**Akibatnya butir 10 Definition of Done tidak dapat dijawab "ya"** untuk `EPIC IGD-07` maupun
gelombang mana pun yang memakai bagian di atas. Ini dicatat terbuka, bukan dilewati.

---

## 1. Yang berubah pada revision 5

| Area | Perubahan |
|---|---|
| Capability map | Revision `2` **dibuang seluruhnya**, diganti revision `3`. Bukti lama menunjuk nama repository dan path yang tidak ada lagi |
| Keputusan | Dua puluh dua keputusan baru `IGD-DEC-067` sampai `IGD-DEC-088`; `IGD-DEC-081` `superseded` sebagian |
| Jenis kunjungan | Kunjungan IGD menjadi `EncounterType.Emergency` |
| Pencatatan klinis | Pembatas antrean dan konsultasi dilonggarkan untuk kunjungan `Emergency` |
| Kepergian pasien | `TrxEmergencyTransfer` menjadi `TrxEmergencyDeparture` dengan dua rangkaian status |
| Tempat tidur | Seluruh urusan tempat tidur pindah ke Rawat Inap |
| Kepemilikan pasien | Matriks lengkap tanpa satu pun keadaan tanpa pemilik |
| Audit klinis | Koreksi bersifat tambah-saja |
| Kewenangan unit | Jembatan `MstServiceUnit` ke simpul organisasi, bukan tabel penugasan baru |
| Dokumen baru | `04-prd-to-mvp.md`, `erd/emergency-departure.md` |
| Arsip | Revision `4` disimpan di `archive/`, tidak dihapus |

---

## 2. Artifact hashes

Dihitung ulang **22 September 2026 (penutup)** sesudah approval kontrak dan `plan-module-delivery` final (bagian 0h),
SHA-256 atas byte berkas di working tree backend (belum di-commit; HEAD lokal `69953e98`). Hitungan sebelumnya pada
pass `design-business-module` hari yang sama (tersimpan di commit `69953e98`). *Catatan:* repository memakai
`core.autocrlf = true`; hash dapat berbeda bila berkas di-checkout ulang dengan akhir baris lain — hitung ulang dari
working tree yang sama sebelum membandingkan. **Lima hash kontrak adalah kunci `IGD-DEC-157`**: task R3.13/R3.12 yang
menemukan hash berbeda wajib berhenti dan melaporkan.

| Artifact | SHA-256 | Berubah pada pass penutup |
|---|---|:---:|
| `00-interview-decisions.md` | `d6d4ca32972967046a7267b521cab9de42f9ae0e32ccb75087dfcfb2c478c040` | Ya |
| `01-existing-capability-map.md` | `89322e353daf7dece993006b43e7f688fb5888f23deec839729831635b48aa13` | Tidak |
| `02-backend-architecture.md` | `267a5ec8037c8fe0eeab31d63cba23fad76999b5abc8c2cb0defb361e905a111` | Ya |
| `03-frontend-architecture.md` | `92d8ddc1505b378f3cc5f53aef5a88217a7b4c01c4b9743147d8d02367f2afe3` | Ya |
| `04-prd-to-mvp.md` | `56b54ef133ced76d925fbcbad80c790b6099b7a5b63c9b68c805c54697a70d25` | Ya |
| `erd/00-context-erd.md` | `361e47b20b73d25293cc0ce0d2779b86ac3f9a49d0e768d28e0f3470e48da5b1` | Tidak |
| `erd/emergency-episode.md` | `53c9df51c18bd37014ce8c44f6da4b965d197185e6402e99446dfcef1cd6867b` | Tidak |
| `erd/emergency-departure.md` | `0f5445e06cc9bbd60ed15942cb9aeae34d109baee142af76995f0664909a8097` | Tidak |
| `erd/data-dictionary.md` | `54bea887bf42315e80942ae6e495ca7f1d4e2701c994c9132150f88ad11e0c59` | Tidak |
| `contracts/api-contract.md` | `c0bcea54125879e328a1b4f28cfcc6adaa5356a7272316cb4dd56a6a8b9fcedd` | Ya — **kunci** |
| `contracts/state-transition-matrix.md` | `0661fdf4326f927425828afa42ce71c69ed8a030c599d17eb9063d44d2e6a793` | Ya — **kunci** |
| `contracts/validation-matrix.md` | `580832c37db3f3a1b73b03b6a366d5d5108ee745e4c408ece53b01034d98b504` | Ya — **kunci** |
| `contracts/integration-contract.md` | `46c869830a078bc2288005cae9b16b44970ccd9bfa6b38c80b01ced249683d53` | Ya — **kunci** |
| `contracts/permission-audit-matrix.md` | `597b8f5ef0e8cbed6963c98374508a80726d652ac4dfa3c2bb99eccef4dec662` | Ya — **kunci** |
| `testing/acceptance-test-matrix.md` | `b8646e60e2cdb248fb1c219d6b4d8babe0503c6f56ed6a12b5213979505acefa` | Tidak |
| `roadmap/backend-roadmap.md` | `d60df773d659290e4bbfa8e78d923fb0ecb243a353344e96ac4287cacac3bc5b` | Ya |
| `roadmap/frontend-roadmap.md` | `6052b3127f5d1feb03fef6efd1de60338e343a14e5542ab9ecb179505bc60dbf` | Ya |
| `roadmap/requirement-traceability.md` | `96fd8d44f91e93a903396637b5b8bc985b73a61e1bdd6952a259c37da3e4d155` | Ya |
| `flowcharts/00-alur-utama.md` | `b85d4376321523c2ce1e59a35b29c3fbb39bf9306fed9de3c2e33958b0278427` | Tidak |
| `flowcharts/pendaftaran-dan-penjaga-episode.md` | `4dd3173545fb5dcc441189f20167772fb9f03ddccf00be9fed3438d46f21badc` | Tidak |
| `flowcharts/kelahiran-kunjungan.md` | `66f4962ab392f628b178f117f937e31c6d95a2401e71e2e75335001f9611f11c` | Tidak |
| `flowcharts/pergi-sebelum-triage.md` | `58607cb74cdf79847ec98b11664f6611a67a3a55f40d9a5dc43e76bd07cd061e` | Tidak |
| `flowcharts/penutupan-encounter.md` | `320f382b58a276b1d50007b77f6fb366918c435f00c2494b989ec1fcb53c7db3` | Tidak |
| `flowcharts/rekonsiliasi-encounter-historis.md` | `f9cc98de4a612877559fbd6091e2f54d9a7f49ecc5647ce204fff96a78e2e52c` | Tidak |
| `evidence/02-requirement-completeness-gate.md` | `3394ea85db85e5d564d4c5898808ec0901f421d533b8a9d98e17f3c1a07bdf48` | Tidak |

Manifest tidak menghitung hash dirinya sendiri.

`roadmap/` **tidak** diperbarui pada revisi ini. Isinya masih roadmap revision `1` yang
seluruh task-nya sudah dikerjakan. Roadmap baru adalah keluaran `/qv-plan`, bukan `/qv-design`.

---

## 3. Dampak kompatibilitas

### 3.1 Empat perubahan yang memutus

| Perubahan | Siapa yang terdampak |
|---|---|
| Kunjungan IGD wajib `EncounterType.Emergency` | Setiap pemanggil yang mengirim `Outpatient`, termasuk test `FE-IGD-001 K1` yang **akan gagal** |
| Grup `emergency-transfers` menjadi `emergency-departures` | Seluruh pemanggil route lama |
| `TransferStatus` dipecah menjadi dua kolom | Pembaca satu kolom status |
| Empat field tempat tidur dan ruangan dihapus | Pengirim field tersebut |

### 3.2 Perubahan pada tabel milik modul lain

**Sembilan tabel** yang bukan milik IGD terdampak: `TrxPatientEncounter`, `MstServiceUnit`,
`TrxPatientAssessment`, `TrxDoctorConsultation`, `TrxPatientDiagnosis`, `TrxPatientProcedure`,
`TrxPatientVitalSign`, `TrxPatientIntegratedProgressNote`, `TrxPrescription`.

Janji "nol perubahan kolom pada tabel modul lain" yang dipegang blueprint Rawat Inap **tidak
dapat dipertahankan** untuk IGD, dan `IGD-DEC-075` juga membatalkannya bagi Rawat Inap.

### 3.3 Yang bersifat aditif

Empat tabel baru, sebelas endpoint baru, lima enum baru, tiga service baru, dua controller
baru. Tidak satu pun memutus pemakai lama.

---

## 4. Gerbang sebelum produksi

| Gerbang | Menunggu | Memblokir |
|---|---|---|
| Penunjukan pemilik `ClinicalManagement` dan `PharmacyManagement` | Organisasi | **`EPIC IGD-09`** — pengkajian, diagnosis, tindakan, resep IGD |
| Persetujuan Muhammad Hamzah atas revisi `RWI-RULE-026` dan `compatibility_impact` | Product/Domain Owner Rawat Inap | `EPIC IGD-09` |
| Persetujuan pemilik IGD atas `RWI-OQ-034` / `DEC-INP-002` | **Pemilik IGD, nama belum tercatat** | Slice `INP-S09` milik Rawat Inap |
| ~~`IGD-OQ-068` penafsiran kolom status dan tabel kejadian~~ | **Terjawab** `IGD-DEC-090` 2026-08-24 | — |
| ~~`IGD-OQ-070` penggantian nama tabel dan route~~ | **Terjawab** `IGD-DEC-091` 2026-08-24 | — |
| ~~`IGD-OQ-071` perilaku unit tanpa pemetaan organisasi~~ | **Terjawab sementara** `IGD-DEC-092` 2026-08-24. Pengesahan Security/Privacy owner masih ditunggu | Penyalaan penjagaan di produksi, **bukan** implementasi |
| Pengisian pemetaan unit selesai **sebelum** penjagaan dinyalakan | Pemilik Master Data | Syarat melekat `IGD-DEC-092`; `MVP-5` |
| `IGD-UNK-01` … `IGD-UNK-07` | Kueri ke basis data bersama | `MVP-1`, `MVP-3`, `MVP-5` |
| Otorisasi menjalankan migration | Pemilik basis data pengembangan | Seluruh gelombang |
| Data master kelas pasien IGD | Penanggung jawab data master | `MVP-1` |
| Pemetaan unit ke simpul organisasi | Master Data + Corporate/HR | `MVP-5` |
| SOP triase dan SOP pengkajian ulang | Clinical governance MMC | Nilai batas waktu; **tidak** memblokir kode |
| Break-glass dan pemisahan SuperAdmin | Security/Privacy owner | Produksi |

Gerbang yang belum terpenuhi berarti menolak tindakan privileged, integrasi, atau finansial
yang terdampak. Gerbang **tidak pernah** memblokir pelayanan klinis darurat.

### 4.0 Amendment 2026-08-24 (kedua) — larangan `/qv-plan` dicabut

`04-prd-to-mvp.md` bagian 7 berbunyi: dokumen ini *"**tidak boleh** diteruskan ke `/qv-plan`
sebelum `IGD-OQ-068`, `IGD-OQ-070`, dan `IGD-OQ-071` dijawab"*. **Ketiganya sudah dijawab**
pada 24 Agustus 2026 lewat `IGD-DEC-090`, `IGD-DEC-091`, dan `IGD-DEC-092`. Larangan itu
**tidak lagi berlaku**, dan `/qv-plan` boleh berjalan untuk seluruh gelombang.

`04-prd-to-mvp.md` **sengaja tidak disunting** oleh Amendment Pass: menyunting keluaran
`/qv-design` dari dalam pass wawancara melanggar batas peran, dan hash-nya akan melenceng
tanpa perhitungan ulang seluruh artefak. Manifest inilah yang berwenang atas keadaan gerbang.
Bagian 7 dokumen tersebut akan diselaraskan pada pass `/qv-design` berikutnya.

`IGD-DEC-092` bersifat **sementara** — ia membuka implementasi `EPIC IGD-08`, tetapi
**tidak** membuka penyalaan penjagaan di produksi. Dua hal itu berbeda.

### 4.1 Satu gelombang yang tidak terhalang apa pun

**`MVP-0` tidak bergantung pada satu pun gerbang di atas.** Isinya: pengisian master kelas
pasien IGD, pemetaan unit ke simpul organisasi, dan `EPIC IGD-03` — perbaikan status kunjungan
yang dapat mundur. `EPIC IGD-03` adalah perbaikan cacat murni yang tidak memerlukan keputusan
siapa pun.

---

## 5. Yang tidak dikerjakan revisi ini

| Yang tidak dikerjakan | Alasan |
|---|---|
| Menandai desain `approved` | Approval adalah tindakan manusia |
| Source code, migration, endpoint, atau UI | Di luar wewenang tahap desain |
| Roadmap dan task | Keluaran `/qv-plan` |
| Pemetaan proses ke ClickUp | Tidak diminta |
| Menjalankan kueri ke basis data | Basis data dipakai bersama satu tim |
| Memperbarui `blueprint-manifest.md` milik Rawat Inap | Bukan wewenang IGD; diusulkan, bukan diberlakukan |

---

## 6. Impact scan 2026-08-24

| Repository | SHA capability map revision 3 | SHA saat desain disusun | Hasil |
|---|---|---|---|
| Backend | `f69e9e48` | `f69e9e48` | **Sama.** Nol berkas `.cs` berubah |
| Frontend | `96a91201` | `96a91201` | **Sama.** Working tree bersih |

Bukti source pada capability map revision `3` karena itu **sahih** dan tidak perlu diaudit
ulang sebelum implementasi dimulai.
