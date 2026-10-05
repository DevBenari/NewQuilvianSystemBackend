# Roadmap Backend — Episode Rawat Inap, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Roadmap | `episode-rawat-inap/roadmap/backend-roadmap-finishing.md` — revision `1` |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `episode-rawat-inap`, kontrak **`0.10.0` `approved`** 2026-10-02 lewat `RWI-DEC-221` |
| Status roadmap | **`DRAFT`** — menunggu approval pemilik atas roadmap ini. Task belum boleh dikirim ke `build-module-backend` sebelum approval itu tercatat |
| Ditulis | 2 Oktober 2026 oleh `plan-module-delivery` |
| Masukan dan hash approval | `02-backend-architecture.md` bagian 12 (`b02d92c0…`), `contracts/api-contract.md` bagian 11 (`958fcd2c…`), `data/data-dictionary.md` bagian 19 (`87f201ce…`), `testing/acceptance-test-matrix.md` bagian Finishing (`8e1e2b5d…`); kolom `MstTariff` dan `MstMedicalEquipment` dari `keperawatan` `data/data-dictionary.md` 12.14 (`9a124a09…`). Hash lengkap pada `../blueprint-manifest.md` bagian 11 |
| Keputusan | `RWI-DEC-173` s.d. `177`, `182`, `189`, `193`, `196`, `197`, `199`, `201`, `204`, `205`, `207`, `208`, `217`, `220`, `221`; gate `1.10` |
| Source SHA | Backend `bf5c6bde` |
| Deret ID | `BE-RWI-172` s.d. `BE-RWI-184` |
| Roadmap pendamping | `frontend-roadmap-finishing.md`, `requirement-traceability-finishing.md` |

**Pembaruan bukti 5 Oktober 2026.** Build project saat `dotnet ef database update` **PASS** menurut output pengguna yang diterima 5 Oktober 2026 (`Build succeeded.`); migration `20261005033044_AddRawatInapFinishing` diterapkan sampai `Done.`. Uji API, regresi, alur klinis, dan rollback `Down()` tetap `NOT RUN`. Nama database dan lingkungan tidak tercantum pada output. Catatan pengecualian 2 Oktober 2026 adalah riwayat sesi implementasi, bukan status build/migration terkini. `BE-RWI-172` selesai pada tingkat source dan penerapan skema; API/regresi tetap menunggu. Source migration dicocokkan pada HEAD `f32b2308291c8d02b083319dac4210d3431f899e` beserta migration/snapshot yang belum di-commit. SHA metadata tetap snapshot perencanaan; approval roadmap tetap `DRAFT`. [Bukti lengkap](../task/report/backend/BE-RWI-172.md#51-pembaruan-bukti-5-oktober-2026).

Migration `K8` + `E4`, kedua bagian `E5`, `E6`, dan `E7` sudah tercakup dalam `20261005033044_AddRawatInapFinishing` yang berhasil diterapkan pengguna. Kriteria keberadaan migration dan `Down()` pada `BE-RWI-172` sudah terpenuhi di source. Build terintegrasi berhasil; eksekusi seeder, verifikasi API/alur bisnis, dan rollback belum dibuktikan. Catatan build `NOT RUN` pada kartu task lain merujuk sesi 2 Oktober 2026; bukti build project terbaru ada pada laporan `BE-RWI-172` bagian 5.1. Status ini belum menyatakan siap produksi.

**Kebijakan verifikasi backend.** Mengikuti `rules/backend/TEST_POLICY.md`: bukti task backend adalah QBE preflight/conformance, review diff/scope, `dotnet build` project aplikasi, verifikasi API/kontrak, verifikasi proses bisnis dengan contoh, dan verifikasi runtime bila lingkungan tersedia. Tidak ada task automated test, dan tidak adanya automated test bukan gap.

**Pada setiap handoff ke `build-module-backend`:** QBE preflight dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` beserta `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Wewenang menerapkan migration ke database dan deployment tidak diberikan roadmap ini.

**Satu kalimat terpenting** (`02-backend-architecture.md` 12.0): hampir seluruh data baru milik Kamar Operasi dan Clinical; Rawat Inap hanya memperoleh `InpAdmissionReferral` dan dua kolom pengaturan.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

```text
BE-RWI-172 ✅ ─┬─> BE-RWI-173 ✅ ───┬─> BE-RWI-176 ✅
               │                    │
               │   BE-RWI-174 ✅ ─┬─┘
               │                  │
               │                  ├─> BE-RWI-175 ✅
               │                  │
               │                  └─> BE-RWI-181 ✅ ─┬─> BE-RWI-182 ✅
               │                                     │
               │       BE-RWI-177 ✅ ──────┐         │
               │                           │         │
               ├───────────────────────────┴─────────┘
               │
               └───────────────────────────┬─> BE-RWI-179 ✅
                                           │
                    BE-RWI-178 ✅ ─────┐   │
                                       │   │
                   BE-RWI-155 [IB] ✅ ─┴───┘

BE-RWI-180 ✅

BE-RWI-154 [IB] ✅ ─┬─> BE-RWI-183 ✅
                    │
                    └─> BE-RWI-184 ✅
```

`[IB]` = task backend sub-modul `integrasi-billing` pada `../../integrasi-billing/roadmap/backend-roadmap-finishing.md`, cermin baca-saja.

Jumlah pasangan prasyarat→task: **13**, sama dengan isi kolom `Dependency`. Dua pasangan berasal dari urutan migration yang disetujui `02-module-map.md` 7.4, bukan dari pemakaian kode: `BE-RWI-174 → BE-RWI-181` (`E6` berjalan sesudah `E5`) dan `BE-RWI-154 → BE-RWI-183` (`RWF-W5` bersyarat `RWF-W1`; keduanya juga mengubah alur transfer yang sama).

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RWI-172`, `BE-RWI-174`, `BE-RWI-177`, `BE-RWI-178`, `BE-RWI-180` — boleh paralel, kecuali catatan migration |
| 1 | `BE-RWI-154` [IB] | `BE-RWI-183`, `BE-RWI-184` |
| 2 | `BE-RWI-172` | `BE-RWI-173` |
| 2 | `BE-RWI-174` | `BE-RWI-175`, `BE-RWI-181` |
| 2 | `BE-RWI-172`, `BE-RWI-178`, `BE-RWI-155` [IB] | `BE-RWI-179` |
| 3 | `BE-RWI-173`, `BE-RWI-174` | `BE-RWI-176` |
| 3 | `BE-RWI-172`, `BE-RWI-177`, `BE-RWI-181` | `BE-RWI-182` |

**Pemetaan ke gelombang `04-prd-to-mvp.md` bagian 23.20.** `MVP-1` (`RWF-W3`): `BE-RWI-172` s.d. `BE-RWI-179`. `MVP-2` (`RWF-W7`): `BE-RWI-180`, `BE-RWI-181`, `BE-RWI-182`. `POST-MVP` (`P2`, `RWF-W5`/`RWF-W7`): `BE-RWI-183`, `BE-RWI-184`. Catatan: endpoint tolak order ikut `BE-RWI-174` di `MVP-1` karena kolom dan nilai status `Rejected` memang bagian migration `E5`; dengan persetujuan OK (`RWI-DEC-208`) dan prioritas `P1` (`RWI-DEC-217`), status Ditolak boleh dirilis lebih awal dari `MVP-2`. Pemilik dapat menahannya saat approval.

**Catatan migration.** Satu `ApplicationDbContext` dan satu snapshot model. Urutan yang mengikat (`02-module-map.md` 7.4): `K8` + `E4` **satu** migration `MasterData` (`BE-RWI-172`) → `E5` → `E6` → `I6` (`BE-RWI-159`, `integrasi-billing`). `E5` dipecah dua: kolom `OprCase` di `BE-RWI-174`; tabel pra-operasi di `BE-RWI-176`, sesudah `E4` karena merujuk `MstSurgicalPreparationItem`. `E7` di `BE-RWI-183`. Data awal `E8` dibagi: butir persiapan di `BE-RWI-173`, salinan hak serah terima di `BE-RWI-177`; tarif komponen operasi diisi pemilik tarif. Task bermigration tidak dikerjakan paralel di branch terpisah.

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-172` | Bentuk data `MasterData` Finishing tersedia dalam satu migration, berikut isian baru endpoint tarif dan pengaturan | `FR-RWF-045`, `047`, `060`, `061`, `088`; `RWI-DEC-173`, `179`, `193`, `196` | `0.10.0` backend 12.10, 12.11 (`E4`); API 11.9; `keperawatan` kamus 12.14 | Model `MstTariff`, `MstInpatientSetting` | `K8` + `E4`; DTO tarif empat isian; pengaturan dua isian | — | Kartu | Kartu | Tabel tarif dibaca Billing / Muhammad Hamzah, Yasmina | Kartu |
| `BE-RWI-173` | Master butir persiapan bedah dapat dikelola dan terisi data awal | `FR-RWF-045`; `RWI-DEC-173` (3) | API 11.4; backend 12.12 | Model dari `BE-RWI-172` | Service, controller, data awal `E8` | `BE-RWI-172` | Kartu | Kartu | Isi awal disahkan klinis / Muhammad Hamzah | Kartu |
| `BE-RWI-174` | Kasus OK menyimpan jenis layanan bedah, rencana anestesi, dan dapat ditolak | `FR-RWF-043`, `044`, `086`; `RWI-DEC-175`, `204`, `208`; `INV-RWF-31` | API 11.5.1; backend 12.7, 12.8 (`E5` bagian kasus) | `OperatingRoomCaseService` | Lima kolom, status `Rejected`, endpoint tolak, respons ditambah, laporan memisahkan Ditolak | — | Kartu | Kartu | Modul OK milik Ikbal Yulianto | Kartu |
| `BE-RWI-175` | Bangsal memesan ruang bedah dengan tepat satu order tindakan | `FR-RWF-040` s.d. `043`; `RWI-DEC-176`; `INV-RWF-25`; `UAT-RWF-32`, `42` | API 11.2 | `OperatingRoomCaseService.CreateAsync` | `InpSurgeryBookingAdapter` dan controller | `BE-RWI-174` | Kartu | Kartu | `POST cases` OK tidak dipersempit / Muhammad Hamzah | Kartu |
| `BE-RWI-176` | Pra-operasi bangsal berversi menjadi syarat "Siap" | `FR-RWF-045`, `048`, `090`; `RWI-DEC-173`, `174`, `199`; `INV-RWF-26`, `27`; `UAT-RWF-21` | API 11.3; backend 12.7 (`E5` bagian pra-operasi) | `OperatingRoomPreparationService`, tanda vital, instrumen nyeri | Tiga tabel, service, endpoint, gerbang Siap, penundaan | `BE-RWI-173`, `BE-RWI-174` | Kartu | Kartu | Modul OK milik Ikbal Yulianto | Kartu |
| `BE-RWI-177` | Serah terima pasca operasi hanya diterima penerima sah di bed unit tujuan | `FR-RWF-046`, `049`, `088`; `RWI-DEC-177`, `189`, `220` (3); `INV-RWF-28`; `UAT-RWF-13` | API 11.5.2, 11.5.3 | `OperatingRoomRecoveryService` | Permission kirim/terima dipisah; aturan penerima; daftar serah terima; salinan hak | — | Kartu | Kartu | Perubahan permission pada peran OK / Muhammad Hamzah | Kartu |
| `BE-RWI-178` | Penyelesaian order tindakan dapat dipakai OK tanpa memanggil HTTP | `FR-RWF-047`; `RWI-DEC-196` | Backend 12.7 | `PatientProcedureController.execute` | Ekstraksi `PatientProcedureExecutionService` | — | Kartu | Kartu | Perubahan struktur endpoint yang dipakai luas / Muhammad Hamzah | Kartu |
| `BE-RWI-179` | Biaya operasi masuk invoice tepat sekali saat kasus selesai | `FR-RWF-047`; `RWI-DEC-196`; `INV-RWF-29`, `30`; `UAT-RWF-05` | Backend 12.5, 12.7; API 11.9 | `StageChargeDeliveryAsync`, jembatan folio Billing | Efek kasus selesai; tujuan Billing dibuka; sumber `OPERATING_ROOM` | `BE-RWI-172`, `BE-RWI-178`, `BE-RWI-155` [IB] | Kartu | Kartu | Menyentuh OK dan Billing / Muhammad Hamzah, Yasmina | Kartu |
| `BE-RWI-180` | Ringkasan operasi baca-saja untuk bangsal | `FR-RWF-081`, `082`; `RWI-DEC-197`; `UAT-RWF-17` | API 11.5.1 `post-operative-summary` | Empat bacaan OK yang ada | Query gabungan dan endpoint | — | Kartu | Kartu | — / Muhammad Hamzah | Kartu |
| `BE-RWI-181` | Permintaan admisi dari kamar pulih tanpa admisi otomatis | `FR-RWF-080`, `089`; `RWI-DEC-201`, `207`, `208`, `220` (1); `INV-RWF-32`; `UAT-RWF-16`, `33`, `41` | API 11.6; backend 12.7 (`E6`) | `InpEpisodeService`, keputusan kamar pulih | Tabel `InpAdmissionReferral`, service, controller, `AdmissionReferralId` pada admisi | `BE-RWI-174` | Kartu | Kartu | Target FK `BilInvoiceEncounterLink` / Muhammad Hamzah | Kartu |
| `BE-RWI-182` | Dua daftar pantau tertunda dengan ambang yang dapat diatur | `FR-RWF-088`; `RWI-DEC-201`, `216`, `220` (6) | API 11.9 | Daftar serah terima (`BE-RWI-177`), permintaan admisi (`BE-RWI-181`) | Dua endpoint pemantauan | `BE-RWI-172`, `BE-RWI-177`, `BE-RWI-181` | Kartu | Kartu | — / Muhammad Hamzah | Kartu |
| `BE-RWI-183` | Serah terima klinis saat transfer antarunit tanpa menahan transfer (`P2`) | `FR-RWF-071`; `RWI-DEC-182`, `189`; `INV-RWF-33`; `UAT-RWF-14` | API 11.8; backend 12.7 (`E7`) | `InpBedOccupancyService` | Model, service, controller Clinical; panggilan sesudah transfer | `BE-RWI-154` [IB] | Kartu | Kartu | `P2` / Muhammad Hamzah | Kartu |
| `BE-RWI-184` | Laporan transfer ruangan per periode (`P2`) | `FR-RWF-087`; `RWI-DEC-205`, `214`; `INV-RWF-34`; `UAT-RWF-20` | API 11.7 | Linimasa `InpBedPlacement` | Service dan controller laporan, ekspor | `BE-RWI-154` [IB] | Kartu | Kartu | `P2` / Muhammad Hamzah | Kartu |

## Kartu task

### ✅ `BE-RWI-172` — Bentuk data `MasterData` Finishing (`K8` + `E4`)

| Field | Isi |
|---|---|
| **Status** | ✅ **Selesai pada tingkat source dan penerapan skema maju**; bukti diperbarui 5 Oktober 2026. `K8` + `E4` tercakup dalam `20261005033044_AddRawatInapFinishing` yang berhasil diterapkan pengguna. Build project terintegrasi `PASS`; API, regresi, alur bisnis, dan rollback `NOT RUN`. Riwayat pengecualian 2 Oktober 2026 tetap pada laporan. Bukti: [laporan](../task/report/backend/BE-RWI-172.md#51-pembaruan-bukti-5-oktober-2026) |
| **Outcome** | Seluruh perubahan `MasterData` Finishing tersedia dalam **satu** migration: master jenis alat, empat kolom baru `MstTariff`, master butir persiapan bedah, dan dua kolom ambang pengaturan Rawat Inap — beserta isian barunya pada endpoint tarif dan pengaturan |
| **Requirement/decision** | `FR-RWF-045`, `047`, `060`, `061`, `088`; `RWI-DEC-173`, `179`, `193`, `196`; `02-module-map.md` 7.4 (satu-satunya migration bersama yang wajib digabung) |
| **Kontrak** | Backend 12.8, 12.10, 12.11 (`E4`); `keperawatan` backend 12.7, 12.8, 12.10, 12.11 (`K8`) dan kamus data 12.14; API 11.9 (tarif tiga isian komponen operasi, pengaturan dua isian); `keperawatan` API 8.2 (tarif `MedicalEquipmentId`) |
| **Reuse** | Model dan configuration `MstTariff`, `MstInpatientSetting`; controller tarif dan pengaturan yang sudah ada |
| **Cakupan** | Model `MstMedicalEquipment` dan enum `MstEquipmentChargeUnit`, `MstEquipmentRoundingRule`; kolom `MstTariff.MedicalEquipmentId`, `SurgeryComponentType`, `ChargeBasis`, `ChargeRounding` dan enum `MstSurgeryComponentType`, `MstTariffChargeBasis`; model `MstSurgicalPreparationItem`; kolom `MstInpatientSetting.PendingSurgicalHandoverAlertMinutes` (bawaan 60) dan `PendingAdmissionReferralAlertMinutes` (bawaan 30); configuration; satu migration; DTO dan validasi endpoint tarif (empat isian opsional) dan pengaturan (1–1440). Tanpa service baru |
| **Dependency** | — |
| **Acceptance criteria** | 1. Satu migration memuat seluruh perubahan di atas dan punya `Down()`. 2. Nama, tipe, nullability, bawaan, index, dan FK (`Restrict`) sama dengan kamus data. 3. Tarif tindakan dan obat lama tidak berubah nilainya; endpoint tarif lama tetap menerima permintaan tanpa isian baru (regresi `RWI-DEC-193`). 4. Pengaturan menolak ambang di luar 1–1440. 5. Tidak ada perubahan perilaku Billing pada task ini |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; pemeriksaan skrip migration terhadap kedua kamus data; verifikasi API tarif dan pengaturan; verifikasi regresi tarif |
| **Risiko/pemilik** | `MstTariff` dibaca Billing; kolom nullable dan bawaan aman. Pemilik: Muhammad Hamzah; Yasmina untuk dampak Billing; `MasterData` milik seluruh tim (`RWI-DEC-193`) |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-172.md` mencatat status penerapan migration apa adanya; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-173` — Master butir persiapan bedah

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 2 Oktober 2026.** Service, controller sembilan endpoint, dan seeder 14 butir empat kelompok (`SurgicalPreparationItemSeeder`, menolak berjalan di Production); kriteria terpetakan ke source. `dotnet build` NOT RUN dan seeder belum dijalankan di lingkungan uji (kriteria 3) — dikecualikan atas keputusan pengguna 2 Oktober 2026. Isi awal wajib disahkan pemilik klinis sebelum produksi. Bukti: [laporan](../task/report/backend/BE-RWI-173.md) |
| **Outcome** | Admin Master Data mengelola butir checklist persiapan bedah per kelompok, dan master terisi data awal sehingga pra-operasi dapat dipakai |
| **Requirement/decision** | `FR-RWF-045`; `RWI-DEC-173` butir 3 |
| **Kontrak** | API 11.4 (lima endpoint `master-data/surgical-preparation-items`, `SurgicalPreparationItem : Read/Create/Update`); backend 12.7, 12.12 |
| **Reuse** | Model dari `BE-RWI-172` |
| **Cakupan** | DTO, service, `SurgicalPreparationItemController`; butir nonaktif tidak muncul di versi baru; data awal `E8` empat kelompok — verifikasi pasien, persiapan fisik, hasil pemeriksaan, persiapan lain — dengan butir V1 |
| **Dependency** | `BE-RWI-172` |
| **Acceptance criteria** | 1. Tambah, ubah, dan nonaktif berfungsi dengan `RowVersion`. 2. Saringan kelompok dan aktif berfungsi. 3. Data awal empat kelompok tersedia di lingkungan uji. 4. Baris registry `SurgicalPreparationItem : Read/Create/Update` lahir dari atribut endpoint |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; pemeriksaan data awal |
| **Risiko/pemilik** | Isi awal wajib disahkan pemilik klinis sebelum produksi (gerbang produksi). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-174` — Jenis layanan bedah, rencana anestesi, dan status Ditolak (`E5` bagian kasus)

| Field | Isi |
|---|---|
| **Status** | ✅ **Selesai pada tingkat source dan penerapan skema maju**; bukti diperbarui 5 Oktober 2026. `E5` bagian kasus tercakup dalam `20261005033044_AddRawatInapFinishing` yang berhasil diterapkan pengguna. Build project terintegrasi `PASS`; API, regresi, alur bisnis, dan rollback `NOT RUN`. Riwayat pengecualian 2 Oktober 2026 tetap pada laporan. Bukti: [laporan](../task/report/backend/BE-RWI-174.md#51-pembaruan-bukti-5-oktober-2026) |
| **Outcome** | Kasus OK menyimpan jenis layanan bedah dan rencana anestesi saat dipesan. Petugas OK dapat menolak order berstatus Diminta dengan alasan; status Ditolak final dan terlihat beserta penolak dan waktunya |
| **Requirement/decision** | `FR-RWF-043`, `044`, `086`; `RWI-DEC-175`, `RWI-DEC-204`, `RWI-DEC-208`; `INV-RWF-30`, `31`; `AC-RWF-085`, `099`; `UAT-RWF-24` |
| **Kontrak** | API 11.5.1 (`POST cases` dua isian, respons ditambah, `PATCH cases/{id}/reject`, kode `OPR-CASE-REJ-001`/`002`), 11.9 (`RejectedCount`); backend 12.7, 12.8, 12.10 |
| **Reuse** | `OperatingRoomCaseService`, `OperatingRoomCaseController`, `OperatingRoomCommandSupport`, `OperatingRoomReportService` |
| **Cakupan** | Kolom `SurgicalServiceType` (bawaan `General`), `PlannedAnesthesiaType`, `RejectedAt`, `RejectedByUserId`, `RejectionReason`; enum `OprSurgicalServiceType`, `OprPlannedAnesthesiaType`; nilai `OprCaseStatus.Rejected = 8`; configuration dan migration `E5` bagian kasus; `RejectAsync` dan endpoint dengan `OperatingRoomCase : Reject`; aksi `Reject` pada `Requested`; respons kasus bertambah `RejectedAt`, `RejectedByName`, `RejectionReason`, `LastStatusReason`, `WardPreOpStatus`, `HandoverStatus`; laporan memisahkan `RejectedCount` dari `CancelledCount` |
| **Dependency** | — |
| **Acceptance criteria** | 1. Kasus lama terbaca `General` tanpa rencana anestesi. 2. Tolak tanpa alasan atau kurang dari 10 karakter → 400; dengan alasan dari `Requested` → `Rejected`. 3. Tolak dari status lain → 422 `OPR-CASE-REJ-001`. 4. Mengubah, menjadwalkan, menunda, membatalkan, atau memulai kasus `Rejected` → 422 `OPR-CASE-REJ-002`. 5. Laporan operasi menampilkan Ditolak terpisah dari Dibatalkan. 6. `POST cases` tetap menerima banyak tindakan bagi petugas OK. 7. Alur OK lama tidak berubah (regresi testing bagian 20) |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration; verifikasi API; verifikasi proses bisnis `UAT-RWF-24` |
| **Risiko/pemilik** | Nilai `OprPlannedAnesthesiaType` masih usulan, disahkan pemilik OK saat implementasi. Modul OK milik Ikbal Yulianto (persetujuan `RWI-DEC-208`) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-175` — Pemesanan ruang bedah dari bangsal

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 2 Oktober 2026.** `InpSurgeryBookingAdapter` dan `InpatientSurgeryBookingController` (`INP-SRG-001`/`002`, `Idempotency-Key`); lima kriteria terpetakan ke source. Permission memakai alias `OperatingRoomCase : Create` tanpa `[AccessAction]` baru (delta dicatat di laporan). `dotnet build` NOT RUN — dikecualikan atas keputusan pengguna 2 Oktober 2026. Verifikasi dengan order nyata belum dijalankan. Bukti: [laporan](../task/report/backend/BE-RWI-175.md) |
| **Outcome** | Perawat atau dokter bangsal memesan ruang bedah dari tab Bedah Operasi atau Bedah Obgyn dengan merujuk tepat satu order tindakan operasi aktif; kasus OK terbentuk berstatus Diminta dengan konteks pasien terisi |
| **Requirement/decision** | `FR-RWF-040` s.d. `043`; `RWI-DEC-175`, `RWI-DEC-176`; `INV-RWF-25`; `AC-RWF-040`, `048`; `UAT-RWF-32`, `UAT-RWF-42` |
| **Kontrak** | API 11.2 (`POST inpatient-management/episodes/{episodeId}/surgery-bookings`, `SurgeryBookingRequest`, `Idempotency-Key`, kode `INP-SRG-001`/`002`) |
| **Reuse** | `OperatingRoomCaseService.CreateAsync` (dipanggil dalam proses yang sama; transaksi milik OK) |
| **Cakupan** | `InpSurgeryBookingAdapter` dan `InpatientSurgeryBookingController`; episode wajib `Admitted`; order wajib aktif dan milik kunjungan episode; tab Obgyn memaksa `Obstetric`; dokter operator dari order; penginput dari akun login |
| **Dependency** | `BE-RWI-174` |
| **Acceptance criteria** | 1. Contoh Budi S. dengan order "Appendektomi" → 201, kasus `Requested`. 2. Tanpa order aktif → 422 `INP-SRG-001` (`UAT-RWF-32`). 3. Episode `DischargePending` → 422 `INP-SRG-002` (`UAT-RWF-42`). 4. Tab Obgyn → `SurgicalServiceType = Obstetric`, tidak dapat diubah dari tab itu. 5. Kunci idempotensi sama dengan isi berbeda → 409; isi sama → tidak dobel |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; verifikasi proses bisnis dengan order tindakan nyata di lingkungan uji |
| **Risiko/pemilik** | Aturan satu order dijaga adapter, bukan dengan mempersempit `POST cases` OK. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-176` — Pra-operasi bangsal berversi dan syarat "Siap" (`E5` bagian pra-operasi)

| Field | Isi |
|---|---|
| **Status** | ✅ **Selesai pada tingkat source dan penerapan skema maju**; bukti diperbarui 5 Oktober 2026. `E5` bagian pra-operasi tercakup dalam `20261005033044_AddRawatInapFinishing` yang berhasil diterapkan pengguna. Build project terintegrasi `PASS`; API, regresi, alur bisnis, dan rollback `NOT RUN`. Riwayat pengecualian 2 Oktober 2026 tetap pada laporan. Bukti: [laporan](../task/report/backend/BE-RWI-176.md#51-pembaruan-bukti-5-oktober-2026) |
| **Outcome** | Perawat bangsal mengirim Catatan Pra-Operasi berisi potret tanda vital dan nyeri, checklist, dan penandaan area operasi; perawat OK mengonfirmasinya dari akun berbeda. Kasus hanya dapat "Siap" dengan versi terbaru terkonfirmasi kedua sisi; penundaan membuat versi itu "perlu diperbarui" |
| **Requirement/decision** | `FR-RWF-045`, `048`, `090`; `RWI-DEC-173`, `174`, `199`; `INV-RWF-26`, `27`; `AC-RWF-042`, `046`, `093`, `094`; `UAT-RWF-21` |
| **Kontrak** | API 11.3 (lima endpoint `ward-pre-op` dan kode `Blockers[]` baru, kode `OPR-WPO-001` s.d. `005`); backend 12.5, 12.7, 12.8 |
| **Reuse** | `OperatingRoomPreparationService` (gerbang), `OperatingRoomSchedulingService.PostponeAsync`, `TrxPatientVitalSign`, respons instrumen `PainScale` |
| **Cakupan** | Model `OprWardPreOpNote`, `OprWardPreOpItem`, `OprWardPreOpSiteMark`, enum `OprWardPreOpStatus`, configuration, migration `E5` bagian pra-operasi; `OprWardPreOpService`; endpoint pada `OperatingRoomPreparationController` dengan `OperatingRoomWardPreOp : Read/Send/Confirm`; syarat keempat gerbang Siap; penundaan menandai `NeedsUpdate` dalam transaksi yang sama. Foto tubuh tidak disimpan |
| **Dependency** | `BE-RWI-173`, `BE-RWI-174` |
| **Acceptance criteria** | 1. Kirim tanpa tanda vital tercatat → 422 `OPR-WPO-005`; kirim membekukan potret dari pencatatan terbaru. 2. Butir wajib belum dikonfirmasi pengirim → 422 `OPR-WPO-003`. 3. Sisi penandaan berbeda dari sisi pesanan → 422 `OPR-WPO-001`. 4. Konfirmasi dari akun yang sama → 422 `OPR-WPO-002`. 5. Kasus tidak dapat Siap sebelum versi terbaru terkonfirmasi; `Blockers[]` memuat kode baru. 6. Kasus ditunda → versi lama `NeedsUpdate`; versi baru menyalin butir lama dan memuat TD terbaru (`UAT-RWF-21`). 7. Kasus `Rejected`/`Cancelled`/`InProgress`/`Completed` → 422 `OPR-WPO-004`. 8. Jalur bypass darurat yang ada tetap berlaku dengan alasan |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration; verifikasi API; verifikasi proses bisnis dengan dua akun |
| **Risiko/pemilik** | Modul OK milik Ikbal Yulianto (persetujuan `RWI-DEC-208`) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-177` — Penerima sah serah terima pasca operasi

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 2 Oktober 2026.** Permission `Send`/`Receive` dipisah, `OPR-HO-001`/`002`, `InpPatientLocationQuery`, daftar serah terima, seeder salinan hak `Update` → `Send` (sekali jalan; `Receive` tidak disalin); enam kriteria terpetakan ke source. `dotnet build` NOT RUN — dikecualikan atas keputusan pengguna 2 Oktober 2026; seeder belum dijalankan. Verifikasi pemindahan ke ICU belum dijalankan. Bukti: [laporan](../task/report/backend/BE-RWI-177.md) |
| **Outcome** | Serah terima pasca operasi hanya dapat diterima pemegang permission terima yang bukan pengirimnya, dan hanya bila pasien sudah menempati bed aktif di unit tujuan. Bangsal dan Daftar Pantau dapat membaca daftar serah terima per unit |
| **Requirement/decision** | `FR-RWF-046`, `049`, `088`; `RWI-DEC-177`, `RWI-DEC-189`, `RWI-DEC-220` butir 3; `INV-RWF-28`; `AC-RWF-043`, `047`, `049`, `087`; `UAT-RWF-13` |
| **Kontrak** | API 11.5.2 (`POST handovers` → `OperatingRoomHandover : Send`, `PATCH handovers/{id}/accept` → `: Receive`, kode `OPR-HO-001`/`002`), 11.5.3 (`GET operating-room-management/handovers`); backend 12.7, 12.12 |
| **Reuse** | `OperatingRoomRecoveryService.AcceptHandoverAsync`; penolakan beralasan yang sudah ada |
| **Cakupan** | Permission kirim dan terima dipisah; `InpPatientLocationQuery.IsPatientInUnitAsync`; aturan penerima ≠ pengirim; `OperatingRoomHandoverQueryController` dengan `OverdueOnly` memakai ambang pengaturan; data awal `E8`: salin pemberian hak `OperatingRoomHandover : Update` menjadi `: Send` pada peran yang sama. Hak `: Receive` **tidak** disalin otomatis |
| **Dependency** | — |
| **Acceptance criteria** | 1. Serah terima ke ICU saat pasien masih di bangsal → 422 `OPR-HO-001` dengan pesan "Pindahkan pasien …" (`UAT-RWF-13`). 2. Pengirim mencoba menerima sendiri → 422 `OPR-HO-002`. 3. Tanpa `: Receive` → 403. 4. Menerima tidak memindahkan bed; tarif kamar bed asal tetap berjalan selama di OK. 5. Daftar serah terima menampilkan `PatientInDestinationUnit`, `WaitingMinutes`, `IsOverdue`. 6. Peran yang dulu memegang `Update` kini memegang `Send` |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; verifikasi proses bisnis pemindahan pasien ke ICU |
| **Risiko/pemilik** | Perubahan permission pada peran yang sudah ada; pemberian `: Receive` dikonfigurasi admin hak akses (`02-backend-architecture.md` 12.12). Task ini dan `BE-RWI-179` sama-sama menyentuh `OperatingRoomRecoveryService`; bila paralel, gabungkan berurutan. Pemilik: Muhammad Hamzah; modul OK Ikbal Yulianto |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-178` — Ekstraksi `PatientProcedureExecutionService`

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 2 Oktober 2026.** `ExecuteAsync` (perilaku lama) dan `ExecuteFromOperatingRoomAsync` (idempoten); controller mendelegasikan; tiga kriteria terpetakan ke source. `dotnet build` NOT RUN — dikecualikan atas keputusan pengguna 2 Oktober 2026. Regresi penyelesaian tindakan rawat jalan/rawat inap belum dijalankan runtime. Bukti: [laporan](../task/report/backend/BE-RWI-178.md) |
| **Outcome** | Logika penyelesaian order tindakan berpindah dari controller ke service, sehingga Kamar Operasi dapat menyelesaikan order tanpa memanggil HTTP. Perilaku endpoint `execute` tidak berubah |
| **Requirement/decision** | `FR-RWF-047`; `RWI-DEC-196` |
| **Kontrak** | Backend 12.7 (`PatientProcedureExecutionService`, `PatientProcedureController`) |
| **Reuse** | `PATCH patient-procedures/{id}/execute` (`PatientProcedureController.cs:1199`, tagih `:1322-1324`) |
| **Cakupan** | `ExecuteAsync` (perilaku lama) dan `ExecuteFromOperatingRoomAsync` (pelaksana dokter operator, waktu = kasus selesai, tanpa dokumen tindakan baru); order yang sudah `Completed` dilewati; controller memanggil service |
| **Dependency** | — |
| **Acceptance criteria** | 1. `execute` dari layar tindakan menghasilkan status, dokumen, dan fakta tagih yang sama dengan sebelum ekstraksi. 2. `ExecuteFromOperatingRoomAsync` pada order `Completed` tidak membuat apa pun (idempoten). 3. Fakta tagih tetap diterbitkan sesudah commit |
| **Verifikasi** | QBE preflight; review diff (perubahan struktur, bukan perilaku); `dotnet build`; verifikasi proses bisnis regresi penyelesaian tindakan rawat jalan dan rawat inap |
| **Risiko/pemilik** | Endpoint dipakai luas; regresi wajib. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-179` — Biaya operasi saat kasus selesai

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 2 Oktober 2026.** `OperatingRoomCompletionEffects`, komponen `ANESTHESIA`/`OR_RENT`/`MATERIAL-`, Billing mengenal `OPERATING_ROOM` dan tarif komponen; staging tindakan lama dicabut; enam kriteria terpetakan ke source. `dotnet build` NOT RUN — dikecualikan atas keputusan pengguna 2 Oktober 2026. Verifikasi dengan Billing sungguhan dan contoh berangka belum dijalankan; tarif komponen diisi pemilik tarif. Bukti: [laporan](../task/report/backend/BE-RWI-179.md) |
| **Outcome** | Saat kasus OK `Completed`, tindakan operasi tertagih sekali lewat order tindakan yang dirujuk, sedangkan anestesi, sewa kamar operasi, dan bahan dikirim OK ke Billing per komponen — tepat sekali, dan nol untuk kasus batal atau ditolak |
| **Requirement/decision** | `FR-RWF-047`; `RWI-DEC-192`, `RWI-DEC-196`; `INV-RWF-29`, `30`; `AC-RWF-044`, `092`, `099`; `UAT-RWF-05` |
| **Kontrak** | Backend 12.5, 12.7 (`OperatingRoomCompletionEffects`, `OperatingRoomIntegrationService`); API 11.9; `keperawatan` kamus 12.14 (kolom komponen tarif) |
| **Reuse** | `StageChargeDeliveryAsync` (sudah berkunci `case:charge:component:revision`); `BillingFolioService` dan jembatan `RANAP` (`BE-RWI-155`); `PatientProcedureExecutionService` (`BE-RWI-178`) |
| **Cakupan** | `OperatingRoomCompletionEffects.ApplyAsync` sesudah commit penerimaan serah terima; komponen `ANESTHESIA` (bila catatan anestesi final), `OR_RENT` (durasi menit), `MATERIAL-{usageId}` (`OprMaterialUsage` `Used`); `BlockedDestinations` tidak lagi memuat Billing; Billing mengenal `SourceContext = OPERATING_ROOM` dan menentukan tarif dari `MstTariff` (`SurgeryComponentType`, `ChargeBasis`, `ChargeRounding`; bahan per `DrugId`) dengan "tarif belum ada" bila tidak ditemukan. Biaya OK tetap dikirim dengan `EncounterId` kasus OK (`RWI-DEC-207`) |
| **Dependency** | `BE-RWI-172`, `BE-RWI-178`, `BE-RWI-155` [IB] |
| **Acceptance criteria** | 1. Kasus `Completed` → order tindakan `Completed` dan satu baris tindakan di invoice; komponen anestesi, sewa kamar, dan bahan masing-masing satu baris. 2. Efek dijalankan ulang → tidak ada baris ganda (`INV-RWF-29`). 3. Kasus `Cancelled` atau `Rejected` → nol biaya (`INV-RWF-30`). 4. Tarif komponen tidak ada → "tarif belum ada" dan invoice tidak dapat difinalkan. 5. Billing gagal → dicatat dan dicoba ulang lewat delivery OK yang sudah ada, tanpa membatalkan penyelesaian kasus. 6. Contoh Bedah Obgyn `UAT-RWF-05`: biaya muncul setelah `Completed`, tidak saat dipesan |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi proses bisnis dengan Billing sungguhan dan contoh berangka |
| **Risiko/pemilik** | Menyentuh OK (Ikbal Yulianto) dan Billing (Yasmina). Tarif komponen operasi diisi pemilik tarif (`02-backend-architecture.md` 12.12). Pemilik: Muhammad Hamzah, Yasmina |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-180` — Ringkasan operasi baca-saja

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 2 Oktober 2026.** `OperatingRoomPostOperativeSummaryQuery` dan `GET cases/{id}/post-operative-summary`; empat kriteria terpetakan ke source. `dotnet build` NOT RUN — dikecualikan atas keputusan pengguna 2 Oktober 2026. Verifikasi API kasus final/draft belum dijalankan. Bukti: [laporan](../task/report/backend/BE-RWI-180.md) |
| **Outcome** | Bangsal dan dokter membaca ringkasan operasi pasien — diagnosis pasca bedah, temuan, komplikasi, perdarahan, drain dan implan, rencana, anestesi, kamar pulih, serah terima — lewat satu bacaan dengan satu permission |
| **Requirement/decision** | `FR-RWF-081`, `FR-RWF-082`; `RWI-DEC-197`; `AC-RWF-081`, `082`; `UAT-RWF-17` |
| **Kontrak** | API 11.5.1 (`GET cases/{id}/post-operative-summary`, `OperatingRoomCase : Read`) |
| **Reuse** | `OprExecutionRecord`, catatan anestesi, kamar pulih, serah terima — dibaca saja |
| **Cakupan** | `OperatingRoomPostOperativeSummaryQuery.GetAsync` dan endpoint pada `OperatingRoomCaseController`; laporan draft → `ReportFinal = false` dan isi klinis kosong |
| **Dependency** | — |
| **Acceptance criteria** | 1. Laporan final → seluruh isian terisi dari empat sumber. 2. Laporan draft → `ReportFinal = false` tanpa isi klinis. 3. Tanpa `OperatingRoomCase : Read` → 403. 4. Tidak ada field rupiah dan tidak ada penulisan |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API dengan kasus final dan draft |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-181` — Permintaan admisi dari kamar pulih (`E6`)

| Field | Isi |
|---|---|
| **Status** | ✅ **Selesai pada tingkat source dan penerapan skema maju**; bukti diperbarui 5 Oktober 2026. `E6` tercakup dalam `20261005033044_AddRawatInapFinishing` yang berhasil diterapkan pengguna. Build project terintegrasi `PASS`; API, regresi, alur bisnis, dan rollback `NOT RUN`. Riwayat pengecualian 2 Oktober 2026 tetap pada laporan. Bukti: [laporan](../task/report/backend/BE-RWI-181.md#51-pembaruan-bukti-5-oktober-2026) |
| **Outcome** | Bila kamar pulih memutuskan rawat inap atau ICU untuk pasien tanpa episode hadir, permintaan admisi lahir dan tampil bagi petugas admisi. Tidak ada admisi otomatis; admisi dari permintaan menyelesaikan permintaan dalam transaksi yang sama |
| **Requirement/decision** | `FR-RWF-080`, `FR-RWF-089`; `RWI-DEC-201`, `207`, `208`, `220` butir 1; `INV-RWF-32`; `AC-RWF-080`, `088`, `089`; `UAT-RWF-16`, `33`, `41` |
| **Kontrak** | API 11.6 (dua endpoint `admission-referrals`, `InpatientAdmissionReferral : Read`; `POST episodes` + `AdmissionReferralId`; kode `INP-ADM-REF-001`/`002`), 11.5.2 (`PUT …/execution/recovery` perilaku baru); backend 12.5, 12.7; kamus data 19.8 |
| **Reuse** | `InpEpisodeService`, `InpatientEpisodeController`; index `IX_InpEpisode_PatientId_Present`; `OperatingRoomRecoveryService` |
| **Cakupan** | Model `InpAdmissionReferral`, enum `InpAdmissionReferralStatus`, configuration, migration `E6` (dua unique parsial); `InpAdmissionReferralService` (`CreateFromRecoveryAsync`, `CancelFromRecoveryAsync`, penyelesaian saat admisi); `InpatientAdmissionReferralController`; panggilan dari simpan kamar pulih; `OpenAdmissionRequest.AdmissionReferralId` |
| **Dependency** | `BE-RWI-174` |
| **Acceptance criteria** | 1. Pasien poliklinik dengan keputusan kamar pulih `Inpatient` → satu permintaan `Pending` (`UAT-RWF-16`). 2. Pasien yang sudah punya episode hadir → tidak ada permintaan. 3. Keputusan berubah dari `Inpatient`/`Icu` → permintaan dibatalkan. 4. Admisi biasa untuk pasien berpermintaan `Pending` → 409 `INP-ADM-REF-001`; admisi dari permintaan berhasil dan permintaan `Completed` (`UAT-RWF-33`, `41`). 5. Permintaan selesai atau batal → 422 `INP-ADM-REF-002`. 6. Tidak ada episode yang dibuat otomatis. 7. Pesan `ADMISSION_CONFIRMED` tetap daftar putih |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration; verifikasi API; verifikasi proses bisnis ujung ke ujung dengan kasus OK |
| **Risiko/pemilik** | Tabel ini menjadi target FK `BilInvoiceEncounterLink.SourceReferralId` (`BE-RWI-159`, `integrasi-billing`); `E6` wajib sesudah `E5` dan sebelum `I6`. Pemilik: Muhammad Hamzah; panggilan dari OK disetujui Ikbal Yulianto (`RWI-DEC-208`) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-182` — Dua daftar pantau tertunda

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 2 Oktober 2026.** `GET monitoring/pending-surgical-handovers` dan `GET monitoring/pending-admission-referrals` dengan ambang dari pengaturan; empat kriteria terpetakan ke source. `dotnet build` NOT RUN — dikecualikan atas keputusan pengguna 2 Oktober 2026. Verifikasi dengan data melewati ambang belum dijalankan. Bukti: [laporan](../task/report/backend/BE-RWI-182.md) |
| **Outcome** | Daftar Pantau menampilkan serah terima pasca operasi yang tertunda dan permintaan admisi yang tertunda melewati ambang yang dapat diatur |
| **Requirement/decision** | `FR-RWF-088`; `RWI-DEC-201`, `216`, `220` butir 6; gate G-18 |
| **Kontrak** | API 11.9 (`GET monitoring/pending-surgical-handovers`, `GET monitoring/pending-admission-referrals`, `InpatientMonitoring : Read`) |
| **Reuse** | Daftar serah terima (`BE-RWI-177`), permintaan admisi (`BE-RWI-181`), ambang pengaturan (`BE-RWI-172`) |
| **Cakupan** | Dua endpoint pemantauan yang membaca 11.5.3 dan 11.6 dengan `OverdueOnly = true` |
| **Dependency** | `BE-RWI-172`, `BE-RWI-177`, `BE-RWI-181` |
| **Acceptance criteria** | 1. Serah terima yang menunggu lebih dari 60 menit (bawaan) muncul dengan tanda "pasien belum di unit tujuan" bila relevan. 2. Permintaan admisi yang menunggu lebih dari 30 menit (bawaan) muncul. 3. Mengubah ambang di pengaturan mengubah isi daftar. 4. Tanpa `InpatientMonitoring : Read` → 403 |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API dengan data uji melewati ambang |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-183` — Serah terima klinis saat transfer antarunit (`E7`, `P2`)

| Field | Isi |
|---|---|
| **Status** | ✅ **Selesai pada tingkat source dan penerapan skema maju**; bukti diperbarui 5 Oktober 2026. `E7` tercakup dalam `20261005033044_AddRawatInapFinishing` yang berhasil diterapkan pengguna. Build project terintegrasi `PASS`; API, regresi, alur bisnis, dan rollback `NOT RUN`. Riwayat pengecualian 2 Oktober 2026 tetap pada laporan. Bukti: [laporan](../task/report/backend/BE-RWI-183.md#51-pembaruan-bukti-5-oktober-2026) |
| **Outcome** | Setiap perpindahan bed ke unit lain otomatis membuat satu dokumen serah terima sembilan bagian V1 berstatus "Belum dikirim", tanpa pernah menahan transfer |
| **Requirement/decision** | `FR-RWF-071`; `RWI-DEC-182`, `RWI-DEC-189`; `INV-RWF-33`; `AC-RWF-071`, `072`; `UAT-RWF-14` |
| **Kontrak** | API 11.8 (lima endpoint `transfer-handovers`, `TransferHandover : Read/Send/Receive`); backend 12.5, 12.7; kamus data 19.9 |
| **Reuse** | `InpBedOccupancyService` (transfer); pencatatan klinis terakhir untuk potret |
| **Cakupan** | Model `CliTransferHandover`, enum `CliTransferHandoverStatus`, configuration, migration `E7`; `CliTransferHandoverService.CreateForTransferAsync` dipanggil sesudah commit transfer antarunit; `TransferHandoverController`; dokumen yang gagal dibuat dibuat ulang oleh pengecekan Daftar Pantau |
| **Dependency** | `BE-RWI-154` [IB] |
| **Acceptance criteria** | 1. Pindah bangsal → ICU membuat satu dokumen; pindah bed di unit yang sama tidak (`UAT-RWF-14`). 2. Pembuatan dokumen gagal → transfer tetap sah. 3. Kirim membekukan potret klinis. 4. Terima/tolak beralasan oleh pemegang `: Receive`. 5. Koreksi salah catat penempatan tidak membuat dokumen |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration; verifikasi API; verifikasi proses bisnis transfer |
| **Risiko/pemilik** | `P2`. Mengubah alur transfer yang juga diubah `BE-RWI-154`. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-184` — Laporan transfer ruangan (`P2`)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 2 Oktober 2026.** `InpRoomTransferReportService`, `InpatientReportController` (laporan dan ekspor `.xlsx` tanpa paket baru, ekspor dicatat logger, `VAL-RWF-90`); empat kriteria terpetakan ke source. `dotnet build` NOT RUN — dikecualikan atas keputusan pengguna 2 Oktober 2026. Verifikasi API dan pembukaan berkas ekspor belum dijalankan. Bukti: [laporan](../task/report/backend/BE-RWI-184.md) |
| **Outcome** | Kepala ruangan dan manajemen membaca laporan transfer ruangan per periode dari linimasa penempatan bed, dengan koreksi salah catat dibedakan dari transfer, dan dapat mengekspornya |
| **Requirement/decision** | `FR-RWF-087`; `RWI-DEC-205`, `RWI-DEC-214`, `RWI-DEC-220` butir 4; `INV-RWF-34`; `AC-RWF-086`, `100`; `UAT-RWF-20` |
| **Kontrak** | API 11.7 (`GET reports/room-transfers`, `GET reports/room-transfers/export`; `InpatientReport : ReadRoomTransfer`, `: ExportRoomTransfer`; `VAL-RWF-90` periode ≤ 31 hari) |
| **Reuse** | `InpBedPlacement` (alasan transfer, pencatat, penanda supersede, `CorrectsPlacementId` dari `integrasi-billing` `I1`) |
| **Cakupan** | `InpRoomTransferReportService`, `InpatientReportController`; saringan periode wajib, unit asal, unit tujuan, kelas, termasuk koreksi; ekspor Excel dicatat logger |
| **Dependency** | `BE-RWI-154` [IB] |
| **Acceptance criteria** | 1. Laporan satu minggu memuat waktu, No. RM, pasien, asal, tujuan, alasan, pencatat, dan jenis (Transfer/Koreksi) (`UAT-RWF-20`). 2. Periode lebih dari 31 hari → pesan `VAL-RWF-90`. 3. Tanpa permission → 403. 4. Ekspor hanya bagi `ExportRoomTransfer` dan tercatat |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API dengan data transfer dan koreksi |
| **Risiko/pemilik** | `P2`. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |
