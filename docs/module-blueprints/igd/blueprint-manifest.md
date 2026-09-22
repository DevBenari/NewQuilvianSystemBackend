# IGD Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `IGD-BP-001` |
| `revision` | `6` |
| `status` | `draft` — **tidak disetujui siapa pun**. Revision `4` yang berstatus `approved sebagian` tetap berlaku sebagai baseline sampai revisi ini disetujui |
| `module` | `igd` / `EmergencyInstallationManagement`, prefix entity **`Emg`** sejak 27 Agustus 2026 (17 tabel `Emg*`; nol `Trx*`/`Mst*` milik IGD tersisa). Prefix lama `TrxEmergency`/`MstEmergency` hanya berlaku pada artefak desain yang disusun sebelum tanggal itu |
| `registry_lifecycle` | `ACTIVE` |
| `design_snapshot_at` | `2026-08-26` (revisi 6); revisi 5 pada `2026-08-24` |
| `backend_commit_sha` | `300922c` (branch `rizkiG`) — merge Hamzah/Ikbal/Yasmina. Revisi 5 disusun pada `f69e9e483052845d11c91d8b7bbdce33c4acc8d8` |
| `frontend_commit_sha` | `96a9120111f6acc6b7c0f37973ea0c717ba41f17` (branch `RizkiV2`) |
| `status_check_sha` | **15 September 2026** — backend `e89907c5` (`rizkiG`), frontend `43adae648` (`RizkiV2`). SHA desain di atas **tidak diganti**; ini hanya tempat pemeriksaan status terakhir dijalankan. Lihat [evidence/2026-09-15-pemeriksaan-status.md](evidence/2026-09-15-pemeriksaan-status.md) |
| `owners` | Product/Domain: **Rizki Gunawan**, ditetapkan `IGD-DEC-089` 2026-08-24. Clinical Governance, Nursing authority, Security/Privacy: `OPEN`. Pemilik `ClinicalManagement` dan `PharmacyManagement`: **belum ditunjuk** — digantikan sementara Product/Domain Owner IGD (`IGD-DEC-107`). Pemilik `LaboratoryManagement` dan `RadiologyManagement`: **Yoga Aji Pratama** (tercatat pada blueprint masing-masing; Radiologi lewat `RAD-DEC-014`, 10 September 2026) |
| `approved_by` | Sebagian: **Rizki Gunawan** menyetujui `IGD-DEC-067`, `IGD-DEC-088`, `IGD-DEC-089`, dan `IGD-DEC-093` pada 24 Agustus 2026. Blueprint secara keseluruhan **belum** disetujui |
| `approved_at` | Sebagian: `2026-08-24` |
| `blueprint_shape` | **`SINGLE`** — `shape_decided_by: USER_CONFIRMED` (Rizki Gunawan, 22 September 2026, sesudah uji pemecahan: hanya rumpun Kepergian yang lolos 3/5; rumpun Pendaftaran & episode 2/5, Triage & penanganan 2/5, Dokter penanggung jawab 1/5). Rincian di bagian 0g |
| `struktur_berkas` | **Utang struktur dicatat**: kamus data tetap di `erd/data-dictionary.md` dan ERD lama tetap di `erd/` (kontrak output terbaru meminta `data/` dan `flowcharts/`). `flowcharts/` **ditambahkan** 22 September 2026 untuk alur encounter-first. Migrasi `erd/` → `data/` adalah pekerjaan terpisah (keputusan pemilik 22 September 2026) |
| `requirement_readiness` | **Slice encounter-first: `PARTIALLY_READY`** — sub-slice `S1`…`S6`, `S8` `READY_FOR_DOMAIN_DESIGN`; `S7` (kelayakan dokter jaga) `BUSINESS_DECISION_REQUIRED` (`IGD-OQ-102`, `103`) — [evidence/02-requirement-completeness-gate.md](evidence/02-requirement-completeness-gate.md) `0.1`, 22 September 2026. **Area IGD lain: `UNCLASSIFIED`** — lihat bagian 0 |
| `domain_architecture_revision` | **Tidak ada** — lihat bagian 0 |
| `domain_architecture_readiness` | Slice encounter-first: **`DOMAIN_ARCHITECTURE_NOT_RUN`** — slice di dalam bounded context IGD yang sudah ada; kepemilikan lintas modul sudah diputuskan pemilik (`IGD-DEC-144`, `145`, `151`, `153`); alasan lengkap di gate §10. Area lain: **`NOT_ASSESSED`** |
| `input_revisions` | `00-interview-decisions.md` **156 keputusan**, terakhir `IGD-DEC-156` (22 September 2026 — bentuk `SINGLE` dan susunan `erd/` dipertahankan, gerbang `design-business-module`); sebelumnya 154 sampai `IGD-DEC-154` (22 September 2026 larut — penutup impact scan `IGD-TRQ-08`…`11`: `IGD-DEC-151`…`154` `approved`; `IGD-DEC-140` U1 dan `IGD-DEC-149` `superseded`; fakta `IGD-FACT-021`…`023`); sebelumnya 150 sampai `IGD-DEC-150` (22 September 2026 sore — amendment pass `grill-me` kasus tepi: `IGD-DEC-142`…`150` `approved`; fakta `IGD-FACT-011`…`020`; asumsi `IGD-ASM-001`/`002`; `IGD-OQ-093` `superseded` sebagian; `IGD-OQ-094`/`095`/`096`/`097`/`100`/`101` `superseded`); sebelumnya 141 sampai `IGD-DEC-141` (22 September 2026 — encounter-first `IGD-DEC-139`, pasien tanpa identitas `IGD-DEC-140`, kelayakan dokter jaga `IGD-DEC-141`; ketiganya `approved` **prinsip**; `IGD-OQ-093` ditutup; `IGD-OQ-094`…`101` dibuka; bukti `IGD-EV-140`…`144`); sebelumnya 138 sampai `IGD-DEC-138` (21 September 2026); sebelumnya 134 sampai `IGD-DEC-134` (16 September 2026 keempat, tata letak riwayat pada ruang kerja pemeriksaan — murni tampilan, nol dampak kontrak); sebelumnya 132 sampai `IGD-DEC-132` (16 September 2026 ketiga, kesiapan `EPIC IGD-04`); sebelumnya 128 sampai `IGD-DEC-128` (16 September 2026 kedua, kunjungan keluar dari `Arrived`); sebelumnya 126 sampai `IGD-DEC-126` (16 September 2026, pemantauan observasi); sebelumnya 121 sampai `IGD-DEC-121` (15 September 2026, perencanaan delivery), 115 sampai `IGD-DEC-115` pada pemeriksaan status hari yang sama, dan 105 sampai `IGD-DEC-105` saat revisi 6 disusun; `01-existing-capability-map.md` revision `3` + **suplemen `3.1`** (audit terarah `EmergencyTransfer` pada `300922c`) + **suplemen `3.2`** (impact scan encounter-first pada `0d13f3a8`/`c941012ac`, 22 September 2026; area lain revisi 3 tetap stale) |
| `delivery_state` | **Per 15 September 2026, dipetakan ulang ke source:** `MVP-1`, `MVP-2`, dan R3.7 ✅; `MVP-0` 🟡 (`BE-IGD-017` tanpa laporan tracked); `MVP-3` 🟡 (`BE-IGD-026`); `MVP-4` 🟡 (`BE-IGD-031`); `MVP-5` 🟡 (`BE-IGD-035`, `EPIC IGD-04` tanpa task); `MVP-6` ⛔ (`BE-IGD-039`). Backend 18 task ✅, 4 🟡, 1 ⛔; frontend 5 ✅, 5 🟡, 1 belum dikerjakan. Seluruhnya sudah di-commit. **Sesudah perencanaan 15 September 2026 (kedua):** backend ditambah 6 task direncanakan (`BE-IGD-040`…`045`, satu ⛔ `BE-IGD-042`); frontend ditambah kartu susulan `FE-IGD-019` ✅ dan 5 task direncanakan (`FE-IGD-023`…`027`) — lihat bagian 0b.5. **Sesudah perencanaan 16 September 2026 (kedua):** frontend ditambah 2 task direncanakan, `FE-IGD-029` dan `FE-IGD-030`, yang memulihkan jalan keluar kunjungan dari `Arrived` — tanpanya modul IGD tidak dapat dipakai untuk pasien baru (`IGD-EV-131`…`IGD-EV-136`, `IGD-DEC-127`, `IGD-DEC-128`). Nol perubahan kontrak, nol task backend. Rincian di [MODULE-STATUS.md](MODULE-STATUS.md). *Keadaan lama (26 Agustus): "`MVP-0` berjalan, `BE-IGD-017`…`020` selesai, belum di-commit"* |
| `amendment` | **2026-09-22 (encounter-first)** — `IGD-DEC-139`…`141` dan task R3.13/R3.12 ditulis sebagai **rencana**; revisi blueprint **tetap `6`** karena arsitektur target baru disetujui prinsip dan kontrak belum diselaraskan (lihat `koreksi_desain_tertunda`). Bila penyelarasan kontrak dikerjakan, perubahan titik lahir `EmgVisit` dan penulisan status encounter oleh IGD adalah perubahan material yang layak menaikkan revisi. **2026-08-24 (kedua)** — `IGD-OQ-068`/`070`/`071` ditutup. **2026-08-26 (correction pass revisi 6)** — enam butir, lihat 0a.2 sampai 0a.4. **2026-09-15 (pemeriksaan status)** — `IGD-DEC-110`…`115`, `IGD-DEC-099` digantikan `IGD-DEC-111`; lihat bagian 0b |
| `contract_versions` | **Per 22 September 2026 (encounter-first, `design-business-module`, seluruhnya `draft` dan Rencana):** API **`0.11.0`** — **bukan aditif murni** (penjaga + tanpa-antrean pada `POST /patient-encounters` Emergency, penolakan `PATCH …/status`/`…/cancel` Registrasi untuk Emergency, penguncian `PUT /emergency-visits/{id}`, efek samping penutupan encounter; endpoint baru `triage-queue`, `start-triage`, `no-show`, `{id}/arrival-time`, grup `Emergency Encounter Reconciliation`); validation **`0.8.0`** — bukan aditif murni (sumber aturan episode berganti ke klausa A+B; bagian 10 baru); state **`0.5.0`** aditif (bagian 8); integration **`0.4.0`** (bagian 5; memutus perilaku bagi Registrasi); permission/audit **`0.5.0`** aditif (bagian 7: `EmergencyVisit : NoShow`, resource `EmergencyEncounterReconciliation`). Rincian di bagian 0g. *Sebelumnya — per 21 September 2026 (`BE-IGD-048`):* API **`0.8.0`** — *relaxed nullability change for legacy response*, bukan aditif murni: `assignedByUserId` pada §3.2 boleh `null` hanya untuk baris hasil pengisian data lama (`IGD-DEC-136`); **bukan** perubahan semantics penetapan/pengalihan baru. Validation, state, permission/audit, integration **tidak berubah**. *Sebelumnya — per 16 September 2026 (ketiga, kesiapan `EPIC IGD-04`):* API **`0.7.0`** aditif — bagian 3.2 baru untuk proyeksi `doctorName`/`assignedByName` (`IGD-DEC-129`) dan nama canonical `RegPatientEncounter` (`IGD-DEC-132`); validation, state, permission/audit **tidak berubah**; integration contract hanya nama tabel. *Sebelumnya (pemantauan observasi, bagian 0d):* API `0.6.0` aditif (bagian 7 baru); validation **`0.6.0`** aditif (bagian 9 baru); state, permission/audit, integration **tidak berubah**. *Per 15 September 2026 (penyelarasan teks, bagian 0c):* API `0.5.0` bukan aditif murni; validation `0.5.0` bukan aditif. *Keadaan revisi 6:* API `0.4.0` **bukan aditif**; validation `0.4.0` **bukan aditif**; state `0.4.0` aditif; permission/audit `0.4.0` aditif; integration `0.3.0` tidak berubah. Rinciannya di 0a.2. Seluruhnya `draft`. `IGD-DEC-093` **tidak diperluas**: yang `approved` tetap hanya state §1/§1.1/§1.2 dan validation §2 aturan 4–5. **Dikoreksi 15 September 2026:** kalimat ini tertinggal dari `IGD-DEC-108` (27 Agustus 2026), yang menaikkan irisan kontrak `MVP-1`…`MVP-6` menjadi `approved` — state §2, 3, 4, 6, 6a; validation §1, 1.1, 3, 4, 4.1, 5, 5.1, 6, 7; API §1.1, §2, bagian pengkajian; permission/audit §3.1; integration bagian encounter IGD — dengan wewenang sementara `IGD-DEC-107` |
| `roadmap_revision` | `3` — 2026-08-26, diperluas ke perjalanan pasien penuh: pendaftaran & triase, pengkajian, kepergian. Revision `2` (`MVP-0`) tetap di berkas yang sama; revision `1` diarsipkan ke `roadmap/archive/revision-1/`. **Penomoran gelombang bergeser**: pengkajian masuk `MVP-3`, kepergian ke `MVP-4`, serah terima `MVP-5`, kewenangan unit `MVP-6` |
| `belum_direncanakan` | **Penunjang medis, pemakaian alat, billing IGD.** Batas lingkup ditutup `IGD-DEC-095`…`105`; masih nol epic, nol FR, nol kontrak. **Ditahan atas instruksi Product/Domain Owner** sampai correction pass revisi 6 tuntas dan `MVP-0` selesai |
| `koreksi_desain_tertunda` | **Nihil untuk slice encounter-first — diselesaikan 22 September 2026 (bagian 0g)**: (1) API §8, (2) state §8, (3) validation §10, (4) integration §5, (5) permission/audit §7, (6) `02-backend-architecture.md` §13, `03-frontend-architecture.md` §13, `flowcharts/` baru, catatan rujukan di `erd/emergency-episode.md`, (7) `04-prd-to-mvp.md` §8 (`FR-IGD-069`…`085`, `AT-IGD-166`…`185`). **Tersisa di luar slice:** `S7` kelayakan dokter jaga (ditahan `IGD-OQ-102`/`103`). *Keadaan sebelumnya:* **Ada lagi — 22 September 2026.** Keputusan `IGD-DEC-139`…`141` berlaku lebih dulu sebagai prinsip, tetapi berkas desain belum mengikutinya: (1) `contracts/api-contract.md` — `triage-queue`, `start-triage`, `eligible-doctors`, ruas `encounter` pada `active-episode` (§1.3), `eligibilityOverrideReason`, efek samping `complete`/`visit-status`; (2) `contracts/state-transition-matrix.md` — titik lahir `EmgVisit` dan encounter mengikuti status akhir kunjungan; (3) `contracts/validation-matrix.md` — rumus episode terbuka klausa A+B dan empat tanda "berakhir"; (4) `contracts/integration-contract.md` — **IGD menulis status `RegPatientEncounter`** dan Registrasi memanggil aturan IGD; (5) `contracts/permission-audit-matrix.md` — aksi baru dan jejak override; (6) `02-backend-architecture.md`, `03-frontend-architecture.md`, `erd/emergency-episode.md` — alur encounter-first; (7) `04-prd-to-mvp.md` — FR/AT untuk kebutuhan baru. Setiap task R3.13 menyelaraskan bagian yang disentuhnya (satu minor per task); sisanya pass `design-business-module`. *Keadaan sebelumnya:* **Nihil — 15 September 2026 (ketiga).** Kelima penyelarasan di bawah sudah dikerjakan pass `design-business-module`; rinciannya di bagian 0c. *Keadaan sebelumnya (15 September 2026, kedua) — lima penyelarasan teks berkas kontrak tertunda*, keputusannya sudah `approved` dan berlaku lebih dulu: (1) `contracts/api-contract.md` §3 — query `at` pada `GET /active` (`IGD-DEC-117`); (2) `erd/data-dictionary.md` §4, `erd/00-context-erd.md`, `erd/emergency-episode.md`, `02-backend-architecture.md` — nama `TrxEmergencyDoctorAssignment` → `EmgDoctorAssignment` (`IGD-DEC-116`); (3) `contracts/validation-matrix.md` §6 aturan 4 — pesan menyebut pesanan (`IGD-DEC-118`); (4) validation §1 aturan 2 — teks penolakan jenis kunjungan (`IGD-DEC-120`); (5) aturan baru batas 1000 karakter catatan status observasi (`IGD-DEC-119`). Dikerjakan pass `design-business-module` berikutnya beserta kenaikan versi dan hash. *Keadaan sebelumnya:* **Nihil.** Empat koreksi selesai pada revisi 6; enam butir correction pass 26 Agustus selesai — audit `EmergencyTransfer`, koreksi klaim aditif, penyelarasan metadata, pembentukan pesanan internal, unique constraint, kewenangan pesanan |
| `compatibility_impact` | **Empat perubahan memutus.** Lihat bagian 3 |

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

Dihitung ulang **22 September 2026** sesudah pass `design-business-module` encounter-first (bagian 0g),
SHA-256 atas byte berkas di working tree backend `0d13f3a8` (belum di-commit). Hitungan sebelumnya 16 September
2026 (tersimpan di riwayat git). *Catatan:* repository memakai `core.autocrlf = true`; hash dapat berbeda bila
berkas di-checkout ulang dengan akhir baris lain — hitung ulang dari working tree yang sama sebelum membandingkan.

| Artifact | SHA-256 | Berubah pada pass 22 Sep |
|---|---|:---:|
| `00-interview-decisions.md` | `0c1809f10676d47cdc3ce2d3269928b32c75cb85dcec6f5a079a51a7f61ff7ba` | Ya |
| `01-existing-capability-map.md` | `89322e353daf7dece993006b43e7f688fb5888f23deec839729831635b48aa13` | Ya |
| `02-backend-architecture.md` | `2b0a874c62f0a50ad72a242b0528aed55e96eb600114f7f6f0d47787a774a96f` | Ya |
| `03-frontend-architecture.md` | `d79520561b682c551471bf9052912ed8df6b8becf2a80484d084cb67a07091a1` | Ya |
| `04-prd-to-mvp.md` | `e43d5cd07461fa1b5faad36072aa41e112448285997cbd02f7af7db5bb225efa` | Ya |
| `erd/00-context-erd.md` | `361e47b20b73d25293cc0ce0d2779b86ac3f9a49d0e768d28e0f3470e48da5b1` | Tidak |
| `erd/emergency-episode.md` | `53c9df51c18bd37014ce8c44f6da4b965d197185e6402e99446dfcef1cd6867b` | Ya |
| `erd/emergency-departure.md` | `0f5445e06cc9bbd60ed15942cb9aeae34d109baee142af76995f0664909a8097` | Tidak |
| `erd/data-dictionary.md` | `54bea887bf42315e80942ae6e495ca7f1d4e2701c994c9132150f88ad11e0c59` | Ya |
| `contracts/api-contract.md` | `b670e455e18415fec1d61c1d548062a1a3cf1b6273d5b30b4fe16f63c1ef8d9e` | Ya |
| `contracts/state-transition-matrix.md` | `360568cbc9e662161e624c075e112ebfb48598bfe88e7d0faac8ce59c3963349` | Ya |
| `contracts/validation-matrix.md` | `df66bba34a29937e1a278c36e62499ae6d4ca1d13c6675f9249fb94786d44d45` | Ya |
| `contracts/integration-contract.md` | `d0aa90244d2f0f0cc12cb3993d09b6674bcc3362c6caaa97d87e6b309f705652` | Ya |
| `contracts/permission-audit-matrix.md` | `0741ef262ffb69689ad9069a2c75c9866f0c7489ea2aa38735683b82749905ac` | Ya |
| `testing/acceptance-test-matrix.md` | `b8646e60e2cdb248fb1c219d6b4d8babe0503c6f56ed6a12b5213979505acefa` | Ya |
| `flowcharts/00-alur-utama.md` | `b85d4376321523c2ce1e59a35b29c3fbb39bf9306fed9de3c2e33958b0278427` | Baru |
| `flowcharts/pendaftaran-dan-penjaga-episode.md` | `4dd3173545fb5dcc441189f20167772fb9f03ddccf00be9fed3438d46f21badc` | Baru |
| `flowcharts/kelahiran-kunjungan.md` | `66f4962ab392f628b178f117f937e31c6d95a2401e71e2e75335001f9611f11c` | Baru |
| `flowcharts/pergi-sebelum-triage.md` | `58607cb74cdf79847ec98b11664f6611a67a3a55f40d9a5dc43e76bd07cd061e` | Baru |
| `flowcharts/penutupan-encounter.md` | `320f382b58a276b1d50007b77f6fb366918c435f00c2494b989ec1fcb53c7db3` | Baru |
| `flowcharts/rekonsiliasi-encounter-historis.md` | `f9cc98de4a612877559fbd6091e2f54d9a7f49ecc5647ce204fff96a78e2e52c` | Baru |
| `evidence/02-requirement-completeness-gate.md` | `3394ea85db85e5d564d4c5898808ec0901f421d533b8a9d98e17f3c1a07bdf48` | Baru |

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
