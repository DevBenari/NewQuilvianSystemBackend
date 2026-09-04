# Bank Darah — Delivery Roadmap

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` |
| Roadmap revision | `2` — menggantikan revisi 1 yang ditandai `STALE`; gerbang direkonsiliasi 3 September 2026 |
| **Roadmap status** | **`APPROVED`** — disusun sebagai forward-test di atas set kontrak `v4`, lalu ikut disetujui ketika `G1` turun pada 3 September 2026 |
| Contract version yang dipakai | **`v4`** (**`approved`**) — `02-backend-architecture.md`, `03-frontend-architecture.md`, `04-prd-to-mvp.md`, `data/`, `contracts/`, `flowcharts/`, `testing/` |
| Backend SHA | `5f7acaf` cabang `sukmagp` — semula `ec2bcac`; disegarkan 4 September 2026 |
| Frontend SHA | `101ec5d3a560bd6e54d4665ae53d425f255c609f` cabang `sukmagpV2` — semula `afbb8ab`; disegarkan 4 September 2026 |
| Input hash | `design-business-module-role-residue-2026-09-03` · decision revisi **11** · domain arch revisi **6** (`DOMAIN_ARCHITECTURE_READY`) |
| `approved_by` / `approved_at` | `Sukmagp` / `2026-09-03` |

Roadmap ini **tidak** memberi wewenang implementasi, bahkan setelah disetujui. Approval `G1` membuka
**penjadwalan** task; wewenang menulis source diberikan terpisah, satu task satu wewenang, lewat
`build-module-backend`. Migration, eksekusi database di luar dev pemilik, deployment, dan publikasi Git
tetap wewenang tersendiri yang diminta per tindakan. Preflight QBE dan
kesesuaian engineering diselesaikan **pada waktu eksekusi** dari `AGENTS.md` backend target dan dokumen
engineering canonical, bukan di sini.

---

## A. Kenapa roadmap revisi 1 diganti, bukan ditambal

Revisi 1 disusun di atas kontrak `v1`. Sejak itu empat rangkaian keputusan turun dan set kontrak naik
tiga kali. Tiga hal membuat revisi 1 tidak dapat sekadar ditambal:

| Perubahan | Akibat pada rencana |
| --- | --- |
| Storage Location masuk MVP (`DEC-BD-035`..`037`) | Revisi 1 menaruhnya di **Future scope** sebagai coverage gap. Kini ia P0, dan **mendahului** alokasi karena kantong tak dapat dialokasikan sebelum tersimpan |
| Gerbang pemberian diperluas (`DEC-BD-038`) | Task pemberian bertambah syarat yang dinilai ulang saat pemberian, bukan diwarisi dari alokasi |
| `DEF-BD-004` tertutup penuh (`DEC-BD-039`..`044`) | **Gerbang `G3` pada revisi 1 dihapus.** Ia dulu menahan seeding peran; peran kini sudah dipetakan |

Revisi 1 juga menyatakan Storage Location "tidak boleh dibuatkan task karena entity tidak lahir dari
nama task". Penilaian itu **benar pada saat itu** dan tidak dicabut — yang berubah adalah buktinya:
Storage Location kini punya keputusan, konsep domain (`BD-DOM-24`, `BD-DOM-25`), dan kontrak.

---

## B. Gerbang global

| Gate | Isi | Pemilik | Memblokir |
| --- | --- | --- | --- |
| ~~`G1` Approval desain~~ | Owner menyetujui blueprint & set kontrak `v4` | Pemilik proses BDRS + arsitektur backend | ✅ **TERTUTUP** 3 September 2026 oleh `Sukmagp`. Set kontrak `v4` naik dari `draft` ke `approved`; tercatat pada `blueprint-manifest.md` revisi 20 |
| ~~`G2a` `BD-DEP-008`~~ | Pendaftaran prefix `Bbk` di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Pemilik registry engineering | ✅ **TERTUTUP** 3 September 2026, commit `ed7fba8`. Prefix yang disahkan **persis `Bbk`** |
| ~~`G2b` `BD-DEP-016`~~ | Keputusan aktivasi modul: Lifecycle registri `PLANNED` → `ACTIVE` | Pemilik registry engineering | ✅ **TERTUTUP** 3 September 2026, commit `8075784`. Membuka wewenang entity operasional `Bbk*` dan migration modul |

**`G3` revisi 1 dihapus.** Ia menahan seeding peran sampai `DEF-BD-004` turun. `DEC-BD-039` sampai
`DEC-BD-044` sudah memetakan keenam wewenangnya, sehingga tidak ada lagi task yang menunggunya.

**`G2` dipecah menjadi `G2a` dan `G2b` pada rekonsiliasi 3 September 2026.** Registry kini memuat baris
Bank Darah, sehingga **penamaan tidak lagi menahan**: seluruh nama `Bbk*` pada kontrak `v4` berlaku apa
adanya, dan skenario penggantian nama sebagai satu paket tidak terjadi.

Yang menggantikannya waktu itu adalah `G2b`. Kepala registry menyatakan sendiri bahwa persetujuan
pendaftaran "hanya memberi wewenang penamaan dan kepemilikan" dan **tidak** memberi wewenang
implementasi, migration, pekerjaan database, deployment, maupun aktivasi modul berstatus `PLANNED` —
dengan `InsuranceManagement`/`Ins`/`PLANNED` sebagai contoh yang disebut langsung. Baris Bank Darah
memang sempat berstatus `PLANNED`, sehingga entity operasional menunggu keputusan aktivasi.

**Keputusan itu sudah turun.** Commit `8075784` menaikkan Lifecycle Bank Darah ke **`ACTIVE`** pada
3 September 2026, dan sejak itu baris registry berbunyi
`HealthServices | BloodBankManagement / Blood Bank | BUSINESS DOMAIN / MODULE | Bbk | ACTIVE`. Uraian
`PLANNED` di paragraf sebelumnya adalah rekaman keadaan sebelum aktivasi, bukan keadaan sekarang.

Satu catatan dari pemeriksaan waktu itu tetap layak dibaca siapa pun yang bekerja di modul ini.
Pemeriksaan `tooling/qbe/Invoke-QbeConformanceCheck.ps1` menunjukkan checker membaca registry untuk
kepemilikan prefix tetapi **tidak** tampak menegakkan Lifecycle. Artinya mesin tidak akan menghentikan
siapa pun; yang menahan adalah teks governance-nya. *Checker lolos* tidak sama dengan *diberi wewenang*
— dan itu berlaku sama untuk batas yang masih hidup, yaitu eksekusi database di luar dev pemilik dan
deployment.

**Catatan `G2b` yang mengubah urutan kerja, dan tetap berlaku setelah gerbangnya tertutup.** Prefix
`Mst` berstatus **`ACTIVE`** di registry — terverifikasi langsung, bukan inferensi. Karena
`MstBloodStorageLocation`, `MstBloodComponent`, dan `MstBloodBankReason` seluruhnya master `Mst*`,
ketiganya tidak pernah terblokir `G2b`. Itulah sebabnya gelombang `MVP-0` tetap dijadwalkan lebih dulu:
bukan lagi karena menyiasati gerbang, melainkan karena fondasi master memang harus ada sebelum alur
operasional berdiri di atasnya. Pada revisi 1 keuntungan ini tidak terlihat karena Storage Location
belum ada.

**Ketiga gerbang global kini tertutup.** Tidak ada task yang tertahan gerbang, dan kolom `Dependency`
pada tabel task di bawah tetap menyebut `G1`/`G2b` sebagai **rekaman prasyarat yang sudah terpenuhi**,
bukan penahan yang masih hidup.

Kontrak sudah `approved` dan terkunci pada `v4`, sehingga penghalang gerbang bagi frontend hilang. Yang
tetap berlaku hanyalah urutan biasa: **tidak ada task FE yang mendahului task BE pasangannya**, karena FE
menempel pada endpoint yang harus lebih dulu ada.

---

## C. Pemisahan yang diminta

### C.1 Task yang **tidak pernah** terblokir registry

| Task | Sebabnya bebas `G2b` sejak awal |
| --- | --- |
| `BE-BD-001` Master komponen darah & alasan terkendali | Prefix `Mst`, pemilik Master Data |
| `BE-BD-002` Titipan flag kewenangan unit pada `MstServiceUnit` | Kolom pada tabel milik Master Data yang sudah ada |
| `BE-BD-014` **Master lokasi penyimpanan darah** | Prefix `Mst`, pemilik Master Data (`DEC-BD-035`) |
| `BE-BD-016` Seeder resource & action hak akses | Mendaftarkan butir hak akses, bukan membuat entity |

### C.2 Task yang dulu terblokir aktivasi modul — **kini terbuka**

Seluruhnya membuat entity operasional `Bbk*` (`QBE-MOD-002`, `QBE-MOD-003`). Sejak `G2b` tertutup
(commit `8075784`) pembuatannya sudah berwenang; pemisahan ini dipertahankan sebagai rekaman urutan, dan
karena `MVP-0` tetap dijalankan lebih dulu:

`BE-BD-003` Blood Order · `BE-BD-004` PMI Request + Receipt + kelahiran kantong · `BE-BD-015`
Penempatan kantong · `BE-BD-005` Pemeriksaan golongan darah · `BE-BD-006` Alokasi ·
`BE-BD-007` Bukti kecocokan + pemberian · `BE-BD-008` Jalur darurat · `BE-BD-009` Penyelesaian
`PendingReview` · `BE-BD-010` Koreksi dua tahap · `BE-BD-011` Penyelesaian konflik ·
`BE-BD-012` Tindakan Bank Darah.

### C.3 Future scope — tidak masuk gelombang mana pun

Lihat bagian G.

---

## D. P0 — irisan vertikal terkecil yang menjalankan satu kasus darah nyata

Alurnya: **Order → Permintaan PMI → Penerimaan → Penyimpanan → Alokasi → Bukti kecocokan → Pemberian**,
ditambah **pembatalan** dan **jalur darurat**. Sesuai prioritas yang diminta.

### D.1 Backend — fondasi master (sejak awal bebas `G2b`)

| Task ID | Outcome | Req/Decision | Kontrak `v4` | Reuse | Cakupan | Dependency | Acceptance | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-BD-001` **`SELESAI`** — [laporan](../task/report/backend/BE-BD-001.md) | Katalog komponen darah & daftar alasan terkendali dapat dikelola | `DEC-BD-024`, `DEC-BD-032`, `DEC-BD-044`, `BD-DOM-13/14` | api-contract (Blood Component, Blood Bank Reason) | `BD-CAP-011/012/013` | `MstBloodComponent`, `MstBloodBankReason` (+kategori `OrderCancellationClinical`/`Operational`, `CorrectionRejection`) + config + service + controller + migration + seed | `G1` | `AC-BD-055/056` | CRUD berjalan; seed minimum terisi | Rendah / Master Data | **Kedua master selesai** 3 September 2026. `MstBloodComponent`: 9 endpoint, seeder PRC/TC/FFP, 26 test. `MstBloodBankReason`: 9 endpoint, seeder satu alasan untuk **setiap** dari sepuluh kategori, 30 test. Dua migration dibuat, **belum dijalankan**. Acceptance "seluruh kategori alasan terseed" **terpenuhi** |
| `BE-BD-002` **`SELESAI`** — [laporan](../task/report/backend/BE-BD-002.md) | Unit pelayanan dapat dikonfigurasi berwenang memesan darah | `DEC-BD-012`, `BD-DOM-18` | integration-contract | `BD-CAP-005` | Extend `MstServiceUnit` +`IsAvailableForBloodOrder` (bawaan `false`) + migration | `G1`, pemilik Master Data | `AC-BD-013/015/016` | Unit tak dikonfigurasi ditolak | Rendah / Master Data | **Terpenuhi** 3 September 2026: satu `AddColumn` `defaultValue: false`, nol index dibuat maupun diubah, 8 test lulus. `AC-BD-015`/`AC-BD-016` terbukti; `AC-BD-013` diteruskan ke `BE-BD-003` karena penegakannya ada di jalur order darah |
| `BE-BD-014` **`SELESAI`** — [laporan](../task/report/backend/BE-BD-014.md) | **Lokasi penyimpanan darah dapat dikelola, termasuk dinonaktifkan** | `DEC-BD-035`, `DEC-BD-037`, `BD-DOM-24` | api-contract (Blood Storage Location), validation | `BD-CAP-011/012/013` | `MstBloodStorageLocation` + config + service + controller + `GET /options` (hanya aktif) + `PATCH /status` + migration + seed | `G1` | `AC-BD-062/065/067`, `VAL-BD-067/068` | Lokasi nonaktif hilang dari pilihan; penonaktifan **tidak** memindahkan kantong | Sedang / BDRS | **Terpenuhi** 3 September 2026: 9 endpoint, migration dibuat (belum dijalankan), seeder 2 lokasi aktif, 25 test lulus. `MstDrugStorageLocation` **nol berkas disentuh**. `AC-BD-064` terbukti; `AC-BD-062/065/066/067` diteruskan ke `BE-BD-015` karena menuntut penempatan kantong. **Tiga gap dicatat** di laporan bagian 8, termasuk konflik `SortOrder` antara kamus data dan kontrak engineering |

> `BE-BD-014` **mendahului** seluruh task kantong. Master lokasi yang kosong menghentikan modul total:
> tanpa satu pun lokasi aktif, tidak ada kantong yang dapat melewati `Stored`, sehingga tidak ada yang
> dapat dialokasikan maupun diberikan (`INV-BD-025`). Ini *fail-closed* yang disengaja.

### D.2 Backend — alur inti (dulu terblokir `G2b`, kini terbuka)

| Task ID | Outcome | Req/Decision | Kontrak `v4` | Reuse | Cakupan | Dependency | Acceptance | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-BD-003` | Order darah dibuat (elektronik+manual), ganda tertahan, dibatalkan dua peran, pemenuhan dihitung | `DEC-BD-004/005/006/044`, `BD-AGG-01`, `BD-XINV-01`, `INV-BD-035` | api-contract (Blood Order), state-transition, validation | `BD-CAP-002/007/009/010` | `BbkBloodOrder`+`Line`, service (deteksi ganda, `BD-DOM-17`), `BloodOrder : Cancel` terpisah dari `Update`, `BbkEncounterStatusReader`, migration | `G1`, **`G2b`**, `BE-BD-001/002` | `AC-BD-001/002/003/004/010/011/017/095/096/097` | Order ganda tertahan; pembatalan wajib beralasan berkategori sesuai peran | Sedang / BDRS | Semua AC lolos; tidak ada pembatalan tanpa jejak |
| `BE-BD-004` | Permintaan PMI dibuat; penerimaan (termasuk kelebihan) dicatat; kantong lahir `Received` | `DEC-BD-002/003/008/020/025/036`, `BD-AGG-02`, `BD-XINV-02/03` | api-contract (Provider Request), state-transition, validation | `BD-CAP-007/008/010` | `BbkProviderRequest`+`Receipt`+`BbkBloodUnit` (lahir **`Received`**, `CurrentPlacementId` null), sisa≥0 token `Version`, kelebihan→`IsExcess`, migration | `G1`, **`G2b`**, `BE-BD-003` | `AC-BD-005/006/009/022/023/031/032/033/059` | Kelebihan tak buat sisa negatif; kantong lahir belum dapat dialokasikan | Sedang / BDRS | Stok bertambah hanya lewat penerimaan; status awal `Received` |
| `BE-BD-015` | **Kantong disimpan pada lokasi, dipindahkan, dan riwayatnya tak pernah ditimpa** | `DEC-BD-036/037`, `BD-DOM-25`, `INV-BD-025/026/027/028`, `ARCH-BD-POS-04/05/06` | api-contract (storage-location, placements), state-transition, validation | `BD-CAP-009/010` | `BbkBloodUnitPlacement` + filtered-unique `IsCurrent` + `BbkBloodUnit.CurrentPlacementId` (satu transaksi) + `POST`/`PUT /{id}/storage-location` + `GET /{id}/placements` + migration | `G1`, **`G2b`**, `BE-BD-004`, `BE-BD-014` | `AC-BD-060/061/063/066/067/068/069/070` | Kantong `Received` ditolak dialokasikan; perpindahan tak ubah status; penonaktifan lokasi **tak** memindahkan kantong | Sedang / BDRS | Riwayat append-only; nol background job; nol batch update |
| `BE-BD-005` | Golongan darah pasien diperiksa & divalidasi | `DEC-BD-015/018/026/039`, `BD-AGG-04`, `BD-XINV-04` | api-contract (Blood Group Exam), state-transition, validation | `BD-CAP-016` | `BbkBloodGroupExam`+`Sample`, sample→result→**validate rutin**, deteksi konflik→`IsConflictHeld`, `BD-DOM-21`, migration | `G1`, **`G2b`** | `AC-BD-030/034/035/077/078` | Hasil tak tervalidasi tak dipakai klinis; konflik menahan gerbang | Tinggi / klinis | Butir `Validate` terpisah dari `ResolveConflict`; penyelesaian konflik = P1 |
| `BE-BD-006` | Kantong dialokasikan (satu aktif) & alokasi keliru dibatalkan | `DEC-BD-003/007/029/036/037`, `BD-AGG-03`, `ARCH-BD-POS-03/06` | api-contract (allocate/cancel-allocation), state-transition, validation, concurrency | `BD-CAP-010` | `BbkBloodUnitAllocation`, `EvaluateAllocationGate` (sudah `Stored` **dan** lokasi terakhir aktif), filtered-unique aktif + token `Version`, pembatalan→`Available`/`PendingReview` | `G1`, **`G2b`**, `BE-BD-015` | `AC-BD-043/044/045/046/060/068/071` + konkurensi `VAL-BD-018c` | Dua petugas rebut kantong → satu `409`; kantong di lokasi nonaktif ditolak | Tinggi / BDRS | Satu alokasi aktif terjamin; keaktifan lokasi **tidak** disalin ke kantong |
| `BE-BD-007` | Bukti kecocokan dicatat beserta hasilnya; kantong diberikan lewat gerbang tiga syarat | `DEC-BD-013/027/028/038/042`, `BD-AGG-03`, `ARCH-BD-POS-01/02/07`, `INV-BD-019/020/029` | api-contract (compatibility-evidence, issue), state-transition, validation | `BD-CAP-008` | `BbkCompatibilityEvidence` (+`EvidenceResult`, `ValidatedByUserId`), `EvaluateIssuanceGate` (gerbang alokasi + bukti berlaku **dan hasilnya cocok**, dinilai ulang), migration | `G1`, **`G2b`**, `BE-BD-005`, `BE-BD-006` | `AC-BD-018/019/038/039/040/041/042/072/073/089/090/091` | Pemberian ditolak tanpa bukti berlaku, dari lokasi nonaktif, atau bila hasilnya tidak cocok | **Tinggi / klinis & BDRS** | Gerbang *fail-closed* dan dinilai ulang; pemberian terminal |
| `BE-BD-008` | Pemberian jalur darurat oleh Dokter BDRS atau DPJP, tercatat penuh | `DEC-BD-017/038/040`, `BD-DOM-08`, `INV-BD-030/032` | api-contract (emergency-issue), validation | — | `BbkEmergencyAuthorization` (+`AuthorizerRole`, `EmergencyConditionNote`, `BypassScope`), penanda permanen, alasan wajib | `G1`, **`G2b`**, `BE-BD-007` | `AC-BD-020/021/074/075/081/082/083/084/085` | Peran tak berwenang ditolak; keterangan gerbang & kondisi wajib; muncul di daftar tunggakan | **Tinggi / klinis** | Penanda menyebut gerbang yang dilewati; enum menutup keadaan tak sah |

### D.3 Frontend P0 — kontrak BE sudah di-approve (`G1` tertutup); menunggu task BE pasangannya

| Task ID | Outcome | Layar | Kontrak | Reuse | Dependency | Acceptance | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-BD-001` | Setup master dapat dikelola petugas | `FE-BD-08/09` | api-contract `v4` | `BD-CAP-021` | `G1`, `BE-BD-001` | Master CRUD di layar | Rendah / BDRS | Rupa `DEV_DISCRETION`; sumber data terkunci |
| `FE-BD-011` | **Lokasi penyimpanan darah dikelola; akibat penonaktifan terbaca** | `FE-BD-10` | api-contract `v4` | `BD-CAP-021` | `G1`, `BE-BD-014` | `FE-BD-014` keadaan kosong menyatakan modul berhenti; `FE-BD-015` konfirmasi menyebut jumlah kantong tertahan | Sedang / BDRS | Tombol hapus **tidak ada**; hanya nonaktif |
| `FE-BD-002` | Petugas mengelola order darah + pemenuhan + pembatalan | `FE-BD-01/02` | api-contract `v4` | `BD-CAP-021` | `G1`, `BE-BD-003` | Order ganda tertahan + alasan (`FE-BD-003`); kategori alasan pembatalan sesuai peran | Sedang / BDRS | Empat keadaan layar digambar |
| `FE-BD-003` | Petugas mengelola permintaan PMI & penerimaan | `FE-BD-03` | api-contract `v4` | `BD-CAP-021` | `G1`, `BE-BD-004` | Penerimaan termasuk kelebihan tercatat | Sedang / BDRS | — |
| `FE-BD-012` | **Penyimpanan & perpindahan lokasi kantong; saringan kantong tertahan** | `FE-BD-04/05` (parsial) | api-contract `v4` | `BD-CAP-021` | `G1`, `BE-BD-015` | `FE-BD-010` saringan `Received` & lokasi nonaktif wajib; `FE-BD-011` kolom lokasi + penanda | Sedang / BDRS | Bukan daftar kerja keempat — saringan pada daftar yang ada |
| `FE-BD-004` | Petugas mengalokasikan kantong & membatalkan alokasi | `FE-BD-05` (parsial) | api-contract `v4` | `BD-CAP-021` | `G1`, `BE-BD-006` | Daftar `PendingReview` wajib ada (`FE-BD-002`) | Sedang / BDRS | Worklist #2 tersedia |
| `FE-BD-005` | Petugas mencatat golongan darah, bukti + hasilnya, memberikan; jalur darurat jelas | `FE-BD-05/06` (parsial) | api-contract `v4` | `BD-CAP-021` | `G1`, `BE-BD-005/007/008` | `FE-BD-021` hasil tidak cocok menutup tombol Berikan dengan pesan yang benar; `FE-BD-018` peran penerbit dipilih sendiri; `FE-BD-013` gerbang tidak bebas dipilih | **Tinggi / klinis** | Jalur darurat tampak sebagai jalur tak normal (`FE-BD-005`) |
| `FE-BD-006` | Seluruh layar Bank Darah terjangkau dari menu | — | — | `menu-items.jsx` | `G1` | Butir menu mengarah ke layar berhak akses | Rendah / BDRS | Registrasi menu jadi acceptance salah satu task |

---

## E. P1 — setelah P0 inti

### E.1 Backend

| Task ID | Outcome | Req/Decision | Kontrak `v4` | Cakupan | Dependency | Acceptance | Risiko/pemilik |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-BD-009` | Kantong `PendingReview` diselesaikan lewat **tiga wewenang terpisah** | `DEC-BD-019/028/043`, `INV-BD-034` | api-contract, state-transition, validation | `reallocate` (bukti gugur, gerbang lokasi), `return-to-provider`, `mark-not-usable` — tiga butir hak akses berbeda | `G1`, `G2b`, `BE-BD-006/007` | `AC-BD-007/008/024/025/029/092/093/094` | Sedang / BDRS |
| `BE-BD-010` | Koreksi pencatatan pemberian **dua tahap** | `DEC-BD-030/034/041`, `BD-DOM-23`, `INV-BD-021/024/033` | api-contract, validation | `BbkIssuanceCorrection` + lifecycle `Requested`→`Approved`/`Rejected`, peminta ≠ pemutus, pemenuhan hanya hitung `Approved` | `G1`, `G2b`, `BE-BD-007` | `AC-BD-047/048/049/050/086/087/088` | **Tinggi / klinis** |
| `BE-BD-011` | Konflik golongan darah diselesaikan validator klinis lewat pemeriksaan ulang | `DEC-BD-026/031/039`, `BD-DOM-22`, `INV-BD-022/031` | api-contract, validation | `BbkBloodGroupConflictResolution` (wajib `ResolvingExamId`), butir `ResolveConflict` terpisah | `G1`, `G2b`, `BE-BD-005` | `AC-BD-036/037/051/053/054/079/080` | **Tinggi / klinis** |
| `BE-BD-012` | Tindakan Bank Darah dicatat (tanpa charge) | `DEC-BD-021/034`, `BD-AGG-05` | api-contract | `BbkBloodBankProcedure` (snapshot tarif), **tanpa** penyaluran Billing | `G1`, `G2b`, `BE-BD-003` | `AC-BD-026/058` | Sedang / BDRS |
| `BE-BD-016` **`SELESAI SEBAGIAN`** — [laporan](../task/report/backend/BE-BD-016.md) | Seluruh resource & action hak akses Bank Darah terdaftar | `DEC-BD-039`..`044` | permission-audit-matrix | Seeder resource + action; `BloodUnit : Resolve` lama **MUST NOT** didaftarkan | `G1` (bebas `G2b`) | `AC-BD-078/090/093` | Sedang / keamanan platform. **12 dari 39 butir terdaftar** 3 September 2026 — 8, lalu naik setelah `MstBloodBankReason` selesai; 12 test kontrak lulus. **Temuan arsitektural:** tidak ada seeder permission untuk ditulis — `AccessMenuSeeder` mendaftarkan butir lewat refleksi atas controller yang benar-benar ada, sehingga 31 butir sisanya lahir bersama task pembuat controller-nya. Deskripsi task ini perlu diubah pemilik roadmap; lihat laporan §8.1 |

### E.2 Frontend

| Task ID | Outcome | Layar | Dependency |
| --- | --- | --- | --- |
| `FE-BD-007` | Penyelesaian `PendingReview` — tiga tombol, tiga penjaga (`FE-BD-020`) | `FE-BD-05` | `G1`, `BE-BD-009` |
| `FE-BD-008` | Koreksi **dua langkah** + daftar tunggakan bukti darurat (#3) | `FE-BD-05`, `FE-BD-04` | `G1`, `BE-BD-010` |
| `FE-BD-009` | Penyelesaian konflik di dalam layar pemeriksaan (bukan daftar keempat) | `FE-BD-06` | `G1`, `BE-BD-011` |
| `FE-BD-010` | Daftar & pencatatan tindakan Bank Darah | `FE-BD-07` | `G1`, `BE-BD-012` |

---

## F. Urutan gelombang

| Gelombang | Isi | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | `BE-BD-001`, `BE-BD-002`, `BE-BD-014`, `BE-BD-016` — seluruh master + seeder hak akses | `G1`. **Tidak menunggu `G2b`**. **Kemajuan:** `BE-BD-001`, `BE-BD-002`, dan `BE-BD-014` **selesai** 3 September 2026. `BE-BD-016` **selesai sebagian** — 12 dari 39 butir hak akses terdaftar, sisanya terikat task pembuat controller. **`MVP-0` tuntas dari sisi source**; empat migration menunggu dijalankan |
| `MVP-1` | `BE-BD-003`, `BE-BD-004` — order dan permintaan PMI | `G1` + `G2b` + `MVP-0` |
| `MVP-1b` | `BE-BD-015` — penyimpanan dan perpindahan kantong | `MVP-1`. **Wajib mendahului `MVP-3`** |
| `MVP-2` | `BE-BD-005` — pemeriksaan golongan darah | `MVP-1` |
| `MVP-3` | `BE-BD-006`, `BE-BD-007`, `BE-BD-008` — alokasi, bukti+pemberian, jalur darurat | `MVP-1b` **dan** `MVP-2` |
| `MVP-4` | `BE-BD-009`..`BE-BD-012` — penyelesaian, koreksi, konflik, tindakan | `MVP-3` |
| `POST-MVP` | Lihat bagian G | Di luar rilis pertama |

Frontend mengikuti gelombang backend yang kontraknya sudah di-approve; tidak ada FE yang mendahului
task BE pasangannya.

---

## G. Future scope — di luar rilis pertama

| Item | Alasan | Prasyarat sebelum dapat direncanakan |
| --- | --- | --- |
| `BE-BD-013` Penyaluran biaya ke Billing | `OPEN DECISION` `DEC-BD-016` — kontrak sumber biaya belum disetujui pemilik Billing | Persetujuan pemilik BillingManagement, lalu `/design-business-module` |
| Label cetak golongan darah | `OQ-BD-011` — mekanik label belum ditetapkan | `/grill-me` mekanik label → design |
| Lampiran berkas pada bukti pendukung koreksi | `OQ-BD-016` — dirancang sebagai teks; lampiran adalah kemampuan penyimpanan berkas tersendiri | Keputusan pemilik BDRS + scope penyimpanan berkas |
| Generalisasi `MstStorageLocation` lintas domain | `DEC-BD-035` menundanya POST-MVP | Kebutuhan lintas domain nyata + penetapan pemilik master gabungan |
| Pemantauan suhu / kapasitas / IoT lokasi penyimpanan | Dikeluarkan `DEC-BD-035` dari MVP | Kebutuhan + kontrak perangkat |
| Integrasi API PMI | `DEC-BD-002` — MVP manual | Kebutuhan + kontrak PMI |
| Integrasi HCLAB | `DEC-BD-022` — tak ada kontrak/protokol | Bukti integrasi dari luar repository |
| Mesin crossmatch / manajemen donor | `INV-BD-013`, BRD §9 | Di luar batas modul |

---

## H. Traceability requirement → task

| Requirement/Decision | Desain | Kontrak `v4` | Task BE | Task FE | Bukti | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `DEC-BD-004/005/006` | `BD-AGG-01` | api/state/validation | `BE-BD-003` | `FE-BD-002` | `AC-BD-001..004,010,011,017` · `UAT-01` | `READY` — gerbang tertutup |
| `DEC-BD-044` | `BD-AGG-01`, `BD-DOM-14` | api/state/validation | `BE-BD-003`, `BE-BD-001` | `FE-BD-002` | `AC-BD-095..097` · `UAT-21` | `READY` — gerbang tertutup |
| `DEC-BD-002/003/008/020/025` | `BD-AGG-02` | api/state/validation | `BE-BD-004` | `FE-BD-003` | `AC-BD-005,006,009,022,023,031..033` · `UAT-04` | `READY` — gerbang tertutup |
| `DEC-BD-035` | `BD-DOM-24` | api/validation | `BE-BD-014` | `FE-BD-011` | `AC-BD-062,064,065,067` | **`SEBAGIAN`** — `AC-BD-064` terbukti ([BE-BD-014](../task/report/backend/BE-BD-014.md)); `AC-BD-062/065/067` diteruskan ke `BE-BD-015` karena menuntut penempatan kantong |
| `DEC-BD-036` | `BD-DOM-25`, `BD-AGG-03` | api/state/validation | `BE-BD-015` | `FE-BD-012` | `AC-BD-059..063,066` · `UAT-11/12` | `READY` — gerbang tertutup |
| `DEC-BD-037` | `BD-DOM-24`, `BD-AGG-03` | api/state/validation | `BE-BD-014`, `BE-BD-015`, `BE-BD-006` | `FE-BD-011`, `FE-BD-012` | `AC-BD-067..071` · `UAT-13` | `READY` — gerbang tertutup |
| `DEC-BD-015/018/026` | `BD-AGG-04`, `BD-DOM-21` | api/state/validation | `BE-BD-005` | `FE-BD-005` | `AC-BD-030,034,035` | `READY` — gerbang tertutup |
| `DEC-BD-039` | `BD-AGG-04` | api/permission | `BE-BD-005`, `BE-BD-011`, `BE-BD-016` | `FE-BD-005`, `FE-BD-009` | `AC-BD-077..080` · `UAT-16` | `READY` — gerbang tertutup |
| `DEC-BD-003/007/029` | `BD-AGG-03`, `BD-DOM-06` | api/state/validation/concurrency | `BE-BD-006` | `FE-BD-004` | `AC-BD-043..046` · `UAT-02` | `READY` — gerbang tertutup |
| `DEC-BD-013/027/028` | `BD-DOM-07` | api/state/validation | `BE-BD-007` | `FE-BD-005` | `AC-BD-018,019,038..042` · `UAT-03` | `READY` — gerbang tertutup |
| `DEC-BD-038` | `BD-AGG-03`, `ARCH-BD-POS-07` | api/state/validation | `BE-BD-007`, `BE-BD-008` | `FE-BD-005` | `AC-BD-072..076` · `UAT-14/15` | `READY` — gerbang tertutup |
| `DEC-BD-042` | `BD-DOM-07` | api/state/validation | `BE-BD-007` | `FE-BD-005` | `AC-BD-089..091` · `UAT-19` | `READY` — gerbang tertutup |
| `DEC-BD-017/040` | `BD-DOM-08` | api/validation | `BE-BD-008` | `FE-BD-005` | `AC-BD-020,021,081..085` · `UAT-17` | `READY` — gerbang tertutup |
| `DEC-BD-019/028/043` | `BD-AGG-03` | api/state/permission | `BE-BD-009`, `BE-BD-016` | `FE-BD-007` | `AC-BD-007,008,024,025,029,092..094` · `UAT-20` | `READY` — gerbang tertutup (P1) |
| `DEC-BD-030/034/041` | `BD-DOM-23` | api/validation | `BE-BD-010` | `FE-BD-008` | `AC-BD-047..050,086..088` · `UAT-06/18` | `READY` — gerbang tertutup (P1) |
| `DEC-BD-026/031` | `BD-DOM-22` | api/validation | `BE-BD-011` | `FE-BD-009` | `AC-BD-036,037,051,053,054` · `UAT-05` | `READY` — gerbang tertutup (P1) |
| `DEC-BD-021` | `BD-AGG-05` | api | `BE-BD-012` | `FE-BD-010` | `AC-BD-026,058` | `READY` — gerbang tertutup (P1) |
| `DEC-BD-024/032/012` | `BD-DOM-13/14/18` | api/integration | `BE-BD-001`, `BE-BD-002` | `FE-BD-001` | `AC-BD-013,015,016,055,056` | **`SEBAGIAN`** — `AC-BD-055`/`AC-BD-056` terbukti ([BE-BD-001](../task/report/backend/BE-BD-001.md)); `AC-BD-015`/`AC-BD-016` terbukti ([BE-BD-002](../task/report/backend/BE-BD-002.md)); `AC-BD-013` diteruskan ke `BE-BD-003`, dan kategori alasan **sudah ada** lewat `MstBloodBankReason`. Hanya `AC-BD-013` yang tersisa, diteruskan ke `BE-BD-003` |
| `DEC-BD-016` | `BD-DOM-19` | — belum ada | `BE-BD-013` | — | `AC-BD-027` (tertunda) | Future / `OPEN DECISION` |

### H.1 Coverage gap yang tercatat

| Gap | Keadaan | Akibat pada rencana |
| --- | --- | --- |
| Penyaluran biaya Billing | `DEC-BD-016` `OPEN DECISION` | `AC-BD-027` belum dapat diuji; tidak masuk gelombang mana pun |
| Nilai jam masa berlaku bukti per komponen | `OQ-BD-012` | **Tidak** menahan task; nilainya dari konfigurasi master saat eksekusi. Selama kosong, gerbang menolak |
| ~~Nama peran pemegang `BloodUnit : ResolveNotUsable`~~ | `OQ-BD-017` | ✅ **TERTUTUP** `DEC-BD-045` — kewenangan operasional BDRS. Baris seeder `BE-BD-016` sudah ada isinya; butirnya tetap terpisah dari `ResolveReturn` |
| ~~Penegasan gerbang hasil bukti kecocokan~~ | `OQ-BD-018` | ✅ **TERTUTUP** `DEC-BD-046` — hasil `Incompatible` menahan pemberian jalur normal. `VAL-BD-079` tetap berlaku apa adanya; rancangan *fail-closed* `v4` ditegaskan, bukan diubah |
| Keadaan kantong setelah koreksi | `OQ-BD-014` | Menahan detail implementasi `BE-BD-010`, bukan bentuknya |
| Rumah slice resmi `BR-BD-020` | Penilaian kelengkapan requirement masih revisi 2 | Tidak menahan task; `BR-BD-020` diperlakukan sebagai perluasan `BD-SLICE-03/04/10` |

**Storage Location tidak lagi menjadi coverage gap.** Pada revisi 1 ia tercatat sebagai gap karena
belum ada requirement, desain, maupun kontrak. Ketiganya kini ada, dan ia menjadi `BE-BD-014` +
`BE-BD-015`.

---

## I. Langkah berikutnya

Ketiga gerbang global — `G1` approval, `G2a` penamaan, `G2b` aktivasi — **seluruhnya tertutup pada
3 September 2026**. Urutan di bawah karena itu bukan lagi daftar tunggu, melainkan urutan kerja.

1. Jalankan `MVP-0` lewat `/build-module-backend`, satu task satu wewenang tulis:
   `BE-BD-001` → `BE-BD-002` → `BE-BD-014` → `BE-BD-016`. Seluruhnya master `Mst*` dan seeder.
2. Lanjutkan `MVP-1` → `MVP-1b` → `MVP-2` → `MVP-3` → `MVP-4` sesuai bagian F. `MVP-1b` **wajib
   mendahului** `MVP-3` karena kantong tidak dapat dialokasikan sebelum tersimpan.
3. FE mengikuti gelombang BE: tidak ada task FE yang mendahului task BE pasangannya, walaupun
   kontraknya sudah `approved` dan terkunci pada `v4`.
4. ~~Dua pertanyaan terbuka `OQ-BD-017` dan `OQ-BD-018`.~~ ✅ **Sudah ditutup** 3 September 2026 oleh
   `DEC-BD-045` dan `DEC-BD-046`, sebelum `BE-BD-016` dijalankan sebagaimana disarankan. Register
   keputusan kini revisi 10; set kontrak tetap `v4` `approved` tanpa perubahan.
5. Setiap handoff ke `/build-module-backend` menyelesaikan preflight QBE dan kesesuaian engineering
   **pada waktu eksekusi** dari `AGENTS.md` backend target dan dokumen engineering canonical.
6. Setelah satu gelombang selesai, jalankan `/verify-module-readiness` atas bukti `task/report/**`
   sebelum gelombang berikutnya dinyatakan aman.

Roadmap `APPROVED`, disetujui `Sukmagp` pada `2026-09-03`. **Approval ini membuka penjadwalan task, bukan
izin menulis source.** Wewenang tulis backend, migration, eksekusi database di luar dev pemilik,
deployment, dan publikasi Git tetap diminta terpisah untuk setiap tindakan.
