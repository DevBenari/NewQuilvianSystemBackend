# Bank Darah — Delivery Roadmap

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Roadmap revision | `1` |
| **Roadmap status** | **`FORWARD-TEST / DRAFT`** — desain `v1` belum di-approve owner; seluruh task gated |
| Contract version yang dipakai | `v1` (`draft`) — `02-backend-architecture.md`, `03-frontend-architecture.md`, `04-prd-to-mvp.md`, `data/`, `contracts/`, `flowcharts/` |
| Backend SHA | `db08c14dbfb9d6b704e8d0bdfb4fd05e2b52a8cb` cabang `sukmagp` |
| Frontend SHA | `afbb8ab47a6a309f24cdaf6d72024f0dc1b2c254` cabang `sukmagpV2` |
| Input hash | `design-business-module-2026-09-02` · decision revisi 4 · domain arch revisi 3 |
| `approved_by` / `approved_at` | Kosong — approval adalah tindakan manusia |

Roadmap ini **tidak** memberi wewenang implementasi. Ia memetakan pekerjaan dan menandai apa yang
`BLOCKED`. `BLOCKED` bukan cara melewati approval. Preflight QBE dan kesesuaian engineering diselesaikan
**pada waktu eksekusi** dari `AGENTS.md` backend target dan dokumen engineering canonical, bukan di sini.

---

## A. Gerbang global — tidak ada task yang boleh mulai sebelum ini tuntas

| Gate | Isi | Pemilik | Memblokir |
| --- | --- | --- | --- |
| `G1` Approval desain | Owner menyetujui blueprint `v1` (`draft`) | Pemilik proses BDRS + arsitektur backend | **Seluruh** task BE & FE |
| `G2` `BD-DEP-008` | Pendaftaran prefix `Bbk` di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Pemilik registry engineering | Seluruh task yang **membuat entity operasional `Bbk*`** dan migration-nya |
| `G3` `DEF-BD-004` | Penetapan peran jalur darurat, validator golongan darah, pencatat koreksi | Pemilik proses BDRS & klinis | **Seeding peran→hak akses** untuk `EmergencyIssue`, `Validate`, `Correct` (bukan kode endpoint-nya) |

Catatan `G2`: master `Mst*` memakai prefix `Mst` yang sudah sah, **tidak** terikat `G2`. Hanya entity
operasional `Bbk*` yang terblokir `G2`.

Catatan `G3`: endpoint & logika `emergency-issue`, `validate`, `correction` boleh **dibangun** setelah
`G2` (mereka `Bbk*`), tetapi **tidak dapat dipanggil peran mana pun** sampai seeding peran turun dari
`G3`. Karena itu `G3` membuat *task seeding* terpisah `BLOCKED`, bukan menahan seluruh task.

Karena kontrak masih `draft`, **kerja paralel BE/FE belum diizinkan**: task FE bergantung pada kontrak
BE yang sudah di-approve & terkunci hash. Selama `G1` terbuka, FE menunggu.

---

## B. ⚠️ Coverage gap — di luar blueprint `v1`

| Item diminta | Keadaan | Tindakan |
| --- | --- | --- |
| **Storage Location** | **Tidak ada** di `03-domain-architecture.md`, `02-backend-architecture.md`, maupun `data/data-dictionary.md`. Bukan konsep yang pernah diputuskan | **Tidak dibuat task implementasi.** Entity tidak boleh lahir dari nama task. Bila memang dibutuhkan (mis. lokasi simpan kantong/lemari pendingin), rutekan ke `/grill-me` lalu `/design-business-module` lebih dulu. Ditempatkan di Future scope |
| Penyaluran biaya Billing | `OPEN DECISION` `DEC-BD-016` | Future scope; tak masuk gelombang mana pun |
| Label cetak golongan darah | `OQ-BD-011` | Future scope |

---

## C. P0 — MVP prioritas ("tanggal 4")

Cakupan P0 = irisan vertikal tertipis untuk menjalankan satu kasus darah nyata: **Blood Order → PMI
Request → Blood Receipt → (Blood Group) → Allocation → Issue Patient**, plus **Cancellation**.
(*Storage Location* dikeluarkan — lihat §B.)

### C.1 Backend

| Task ID | Outcome | Req/Decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-BD-001` | Master komponen darah & alasan terkendali dapat dikelola | `DEC-BD-024`, `DEC-BD-032`, `BD-DOM-13/14` | `api-contract` v1 (Blood Component, Blood Bank Reason) | `BD-CAP-011/012/013` | `MstBloodComponent`, `MstBloodBankReason` + config + service + controller + migration + seed | `G1` | `AC-BD-055` | Master terisi (§J), CRUD berjalan | Rendah / Master Data | Endpoint master jalan, seed minimum PRC/TC/FFP + alasan per kategori |
| `BE-BD-002` | Unit pelayanan dapat dikonfigurasi berwenang memesan darah | `DEC-BD-012`, `BD-DOM-18` | `integration-contract` v1 | `BD-CAP-005` | Extend `MstServiceUnit` +`IsAvailableForBloodOrder` (default false) + migration | `G1`, pemilik Master Data | `AC-BD-013/015/016` | Unit tak dikonfigurasi ditolak; unit dikonfigurasi lolos | Rendah / Master Data | Kolom + migration tanpa downtime, bawaan menolak |
| `BE-BD-003` | Order darah dibuat (elektronik+manual), ganda tertahan, batal, pemenuhan dihitung | `DEC-BD-004/005/006`, `BD-AGG-01`, `BD-XINV-01` | `api-contract` (Blood Order), `state-transition`, `validation` v1 | `BD-CAP-002/007/009/010` | `BbkBloodOrder`+`Line`, service (deteksi ganda, fulfillment `BD-DOM-17`), controller, `BbkEncounterStatusReader` (expiry), migration | `G1`, **`G2`** | `AC-BD-001/002/003/004/010/011/017` | Order ganda ditahan; kedaluwarsa dari sinyal kunjungan | Sedang / BDRS | Semua AC lolos; riwayat append-only |
| `BE-BD-004` | Permintaan PMI dibuat & penerimaan (termasuk kelebihan) dicatat; kantong lahir | `DEC-BD-002/003/008/020/025`, `BD-AGG-02`, `BD-XINV-02/03` | `api-contract` (Provider Request), `state-transition`, `validation` v1 | `BD-CAP-007/008/010` | `BbkProviderRequest`+`Receipt`+`BbkBloodUnit` (lahir), service (sisa≥0 token `Version`, overdelivery→`IsExcess`+`PendingReview`, close-encounter), controller, migration | `G1`, **`G2`**, `BE-BD-003` | `AC-BD-005/006/009/022/023/031/032/033` | Kelebihan tak buat sisa negatif; susulan tetap dicatat | Sedang / BDRS | Semua AC lolos; stok bertambah hanya lewat penerimaan |
| `BE-BD-005` | Golongan darah pasien diperiksa & divalidasi (sumber sah gerbang klinis) | `DEC-BD-015/018/026`, `BD-AGG-04`, `BD-XINV-04` | `api-contract` (Blood Group Exam), `state-transition`, `validation` v1 | `BD-CAP-016` | `BbkBloodGroupExam`+`Sample`, service (sample→result→validate, deteksi konflik→`IsConflictHeld`), controller, `BD-DOM-21` valid view, migration | `G1`, **`G2`** · seeding `Validate`→peran **`G3`** | `AC-BD-030/034/035` | Hasil tak tervalidasi tak dipakai; konflik menahan gerbang | Tinggi / klinis | Validasi & valid-view jalan; **penyelesaian konflik = P1** |
| `BE-BD-006` | Kantong dialokasikan (satu aktif) & alokasi keliru dibatalkan | `DEC-BD-003/007/029`, `BD-AGG-03`, `ARCH-BD-POS-03` | `api-contract` (Blood Unit allocate/cancel), `state-transition`, `validation`, `concurrency` v1 | `BD-CAP-010` | `BbkBloodUnitAllocation`, service (filtered-unique aktif + token `Version`, cancel→available/PendingReview), controller | `G1`, **`G2`**, `BE-BD-004` | `AC-BD-043/044/045/046` + konkurensi `VAL-BD-018c` | Dua petugas rebut kantong → satu `409` | Tinggi / BDRS | Satu alokasi aktif terjamin; pembatalan tak menghapus |
| `BE-BD-007` | Bukti kecocokan dicatat & kantong diberikan lewat gerbang | `DEC-BD-013/027/028`, `BD-AGG-03`, `ARCH-BD-POS-01/02`, `INV-BD-019/020` | `api-contract` (evidence/issue), `state-transition`, `validation` v1 | `BD-CAP-008` | `BbkCompatibilityEvidence`, service (gerbang: bukti pasien tujuan + belum lewat masa berlaku + golongan darah sah), issue, migration | `G1`, **`G2`**, `BE-BD-005`, `BE-BD-006` | `AC-BD-018/019/038/039/040/041/042` | Pemberian ditolak tanpa bukti berlaku; gugur saat pengalihan | Tinggi / klinis & BDRS | Gerbang fail-closed; pemberian terminal |
| `BE-BD-008` | Pemberian jalur darurat oleh peran berwenang | `DEC-BD-017`, `BD-DOM-08` | `api-contract` (emergency-issue), `validation` v1 | — | `BbkEmergencyAuthorization`, service (penanda permanen, alasan wajib), controller | `G1`, **`G2`**, **`G3`** (seeding peran) | `AC-BD-020/021` | Peran tak berwenang ditolak; muncul di daftar tunggakan | Tinggi / klinis | **`BLOCKED` seeding peran sampai `G3`**; endpoint boleh dibangun, tak dapat dipanggil |

### C.2 Frontend (menunggu kontrak BE di-approve — `G1`)

| Task ID | Outcome | Layar | Kontrak | Reuse | Dependency | Acceptance | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-BD-001` | Setup master dapat dikelola petugas | `FE-BD-08/09` | `api-contract` v1 | `BD-CAP-021` | `G1`, `BE-BD-001` | Master CRUD di layar | Rendah / BDRS | `DEV_DISCRETION` rupa; sumber data terkunci |
| `FE-BD-002` | Petugas mengelola order darah + pemenuhan | `FE-BD-01/02` | `api-contract` v1 | `BD-CAP-021` | `G1`, `BE-BD-003` | Golongan darah diminta vs hasil beda jelas (`FE-BD-001`); order ganda tahan+alasan (`FE-BD-003`) | Sedang / BDRS | Empat keadaan layar digambar; menu terdaftar |
| `FE-BD-003` | Petugas mengelola permintaan PMI & penerimaan | `FE-BD-03` | `api-contract` v1 | `BD-CAP-021` | `G1`, `BE-BD-004` | Penerimaan (termasuk kelebihan) tercatat di layar | Sedang / BDRS | — |
| `FE-BD-004` | Petugas mengelola kantong: alokasi, pembatalan, daftar `PendingReview` | `FE-BD-04/05` (parsial) | `api-contract` v1 | `BD-CAP-021` | `G1`, `BE-BD-006` | Daftar `PendingReview` wajib ada (`FE-BD-002`) | Sedang / BDRS | Worklist #2 tersedia |
| `FE-BD-005` | Petugas mencatat golongan darah, bukti, memberikan; jalur darurat jelas | `FE-BD-05/06` (parsial) | `api-contract` v1 | `BD-CAP-021` | `G1`, `BE-BD-005/007/008` | Penanda konflik & bukti lewat masa berlaku wajib (`FE-BD-007/008`); jalur darurat tampak sebagai jalur tak normal (`FE-BD-005`) | Tinggi / klinis | Bagian darurat **BLOCKED** sampai `G3` |
| `FE-BD-006` | Seluruh layar Bank Darah terjangkau dari menu | — | — | `menu-items.jsx` | `G1` | Butir menu mengarah ke layar berhak akses | Rendah / BDRS | Registrasi menu jadi acceptance salah satu task |

---

## D. P1 — setelah P0 core & approval

### D.1 Backend

| Task ID | Outcome | Req/Decision | Kontrak | Cakupan | Dependency | Acceptance | Risiko/pemilik |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-BD-009` | Kantong `PendingReview` diselesaikan (alih/kembali/tidak layak) | `DEC-BD-019/028`, `BD-AGG-03` | `api-contract`, `state-transition` v1 | reallocate (bukti gugur), return, not-usable | `G1`, `G2`, `BE-BD-006/007` | `AC-BD-007/008/024/025/029` | Sedang / BDRS |
| `BE-BD-010` | Koreksi pencatatan pemberian (append-only) | `DEC-BD-030/034`, `BD-DOM-23`, `INV-BD-021/024` | `api-contract`, `validation` v1 | `BbkIssuanceCorrection`, hitung ulang pemenuhan | `G1`, `G2`, `G3` (seeding `Correct`) | `AC-BD-047/048/049/050` | Tinggi / klinis |
| `BE-BD-011` | Konflik golongan darah diselesaikan lewat pemeriksaan ulang | `DEC-BD-026/031`, `BD-DOM-22`, `INV-BD-022` | `api-contract`, `validation` v1 | `BbkBloodGroupConflictResolution` (wajib `ResolvingExamId`) | `G1`, `G2`, `G3` (seeding `Validate`) | `AC-BD-036/037/051/053/054` | Tinggi / klinis |
| `BE-BD-012` | Tindakan Bank Darah dicatat (tanpa charge) | `DEC-BD-021`, `BD-AGG-05` | `api-contract` v1 | `BbkBloodBankProcedure` (snapshot tarif), **tanpa** penyaluran Billing | `G1`, `G2` | `AC-BD-026` (bagian pencatatan) | Sedang / BDRS |

### D.2 Frontend

| Task ID | Outcome | Layar | Dependency |
| --- | --- | --- | --- |
| `FE-BD-007` | Penyelesaian kantong `PendingReview` di layar | `FE-BD-05` | `G1`, `BE-BD-009` |
| `FE-BD-008` | Koreksi pemberian + daftar tunggakan bukti darurat (#3) | `FE-BD-05`, `FE-BD-04` | `G1`, `BE-BD-010` |
| `FE-BD-009` | Penyelesaian konflik di dalam layar pemeriksaan (bukan daftar keempat) | `FE-BD-06` | `G1`, `BE-BD-011` |
| `FE-BD-010` | Daftar & pencatatan tindakan Bank Darah | `FE-BD-07` | `G1`, `BE-BD-012` |

---

## E. Future scope — di luar rilis pertama

| Item | Alasan | Prasyarat sebelum dapat direncanakan |
| --- | --- | --- |
| `BE-BD-013` Penyaluran biaya ke Billing | `OPEN DECISION` `DEC-BD-016` — kontrak `BillingSourceContract` belum disetujui | Persetujuan pemilik BillingManagement, lalu `/design-business-module` untuk membekukan kontrak charge |
| **Storage Location kantong darah** | **Tidak ada di blueprint `v1`** (coverage gap §B) | `/grill-me` (aturan bisnis lokasi simpan) → `/design-business-module` |
| Label cetak golongan darah | `OQ-BD-011` | `/grill-me` mekanik label → design |
| Integrasi API PMI | `DEC-BD-002` MVP manual | Kebutuhan + kontrak PMI |
| Integrasi HCLAB | `DEC-BD-022`, tak ada kontrak | Bukti integrasi dari luar repo |
| Mesin crossmatch / manajemen donor | `INV-BD-013`, BRD §9 | Di luar batas modul |

---

## F. Traceability requirement → task

| Requirement/Decision | Desain | Contract v1 | Task BE | Task FE | Bukti | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `DEC-BD-004/005/006` | `BD-AGG-01` | api/state/validation | `BE-BD-003` | `FE-BD-002` | `AC-BD-001..004,010,011,017`, `UAT-01` | `BLOCKED` `G1,G2` |
| `DEC-BD-002/003/008/020/025` | `BD-AGG-02` | api/state/validation | `BE-BD-004` | `FE-BD-003` | `AC-BD-005,006,009,022,023,031..033`, `UAT-04` | `BLOCKED` `G1,G2` |
| `DEC-BD-015/018/026` | `BD-AGG-04`, `BD-DOM-21` | api/state/validation | `BE-BD-005` | `FE-BD-005` | `AC-BD-030,034,035` | `BLOCKED` `G1,G2`; validate `G3` |
| `DEC-BD-003/007/029` | `BD-AGG-03`, `BD-DOM-06` | api/state/validation/concurrency | `BE-BD-006` | `FE-BD-004` | `AC-BD-043..046`, `UAT-02` | `BLOCKED` `G1,G2` |
| `DEC-BD-013/017/027/028` | `BD-AGG-03`, `BD-DOM-07/08` | api/state/validation | `BE-BD-007`, `BE-BD-008` | `FE-BD-005` | `AC-BD-018..021,038..042`, `UAT-03` | `BLOCKED` `G1,G2`; darurat `G3` |
| `DEC-BD-019/028` | `BD-AGG-03` | api/state | `BE-BD-009` | `FE-BD-007` | `AC-BD-007,008,024,025,029` | `BLOCKED` `G1,G2` (P1) |
| `DEC-BD-030/034` | `BD-DOM-23` | api/validation | `BE-BD-010` | `FE-BD-008` | `AC-BD-047..050`, `UAT-06` | `BLOCKED` `G1,G2`; correct `G3` (P1) |
| `DEC-BD-026/031` | `BD-DOM-22` | api/validation | `BE-BD-011` | `FE-BD-009` | `AC-BD-036,037,051,053,054`, `UAT-05` | `BLOCKED` `G1,G2`; validate `G3` (P1) |
| `DEC-BD-021` | `BD-AGG-05` | api | `BE-BD-012` | `FE-BD-010` | `AC-BD-026` | `BLOCKED` `G1,G2` (P1) |
| `DEC-BD-024/032/012` | `BD-DOM-13/14/18` | api/integration | `BE-BD-001`, `BE-BD-002` | `FE-BD-001` | `AC-BD-013,015,016,055` | `BLOCKED` `G1` (`BE-BD-001` tak kena `G2`) |
| `DEC-BD-016` | `BD-DOM-19` | — (belum ada) | `BE-BD-013` | — | `AC-BD-026/027` (tertunda) | Future / `OPEN DECISION` |
| **Storage Location** | **—** | **—** | **—** | **—** | **—** | **Coverage gap — belum didesain** |

### Coverage gap yang tercatat

1. **Storage Location** — diminta pada P0, tetapi tak ada requirement/desain/kontrak. Tidak dibuatkan
   task; wajib lewat `/grill-me` + `/design-business-module` dulu.
2. **Penyaluran biaya Billing** — `AC-BD-026/027` belum dapat diuji sampai `DEC-BD-016` turun.
3. **Nilai jam masa berlaku bukti** (`OQ-BD-012`) — tidak menahan task; nilai dari konfigurasi master
   saat eksekusi.

---

## G. Langkah berikutnya

1. **`G1`** owner menyetujui desain `v1` → membuka seluruh task.
2. **`G2`** daftarkan prefix `Bbk` (registry engineering) → membuka task entity operasional.
3. **`G3`** tetapkan peran `DEF-BD-004` → membuka seeding peran jalur darurat/validator/koreksi.
4. Mulai P0 gelombang: `BE-BD-001/002` (fondasi) → `BE-BD-003/004` → `BE-BD-005/006` → `BE-BD-007/008`,
   lalu FE mengikuti kontrak yang di-approve.
5. Setiap handoff ke `/build-module-backend` menyelesaikan preflight QBE & kesesuaian engineering pada
   waktu eksekusi dari `AGENTS.md` backend target.

Roadmap `FORWARD-TEST/DRAFT`. Tidak ada task yang boleh dijalankan sampai `G1` (dan `G2`/`G3` sesuai
task) tuntas. Approval manusia belum diklaim.
