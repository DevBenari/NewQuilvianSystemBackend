# Roadmap Backend — Keperawatan Rawat Inap, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Roadmap | `keperawatan/roadmap/backend-roadmap-finishing.md` — revision `1` |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `keperawatan`, kontrak **`0.6.0` `approved`** 2026-10-02 lewat `RWI-DEC-221` |
| Status roadmap | **`DRAFT`** — menunggu approval pemilik atas roadmap ini. Task belum boleh dikirim ke `build-module-backend` sebelum approval itu tercatat |
| Ditulis | 2 Oktober 2026 oleh `plan-module-delivery` |
| Masukan dan hash approval | `02-backend-architecture.md` bagian 12 (`46259ac6…`), `contracts/api-contract.md` bagian 8 (`164b8ac8…`), `data/data-dictionary.md` bagian 12 (`9a124a09…`), `testing/acceptance-test-matrix.md` bagian Finishing (`91a92f39…`), `contracts/integration-contract.md` bagian 9. Hash lengkap pada `../blueprint-manifest.md` bagian 9 |
| Keputusan | `RWI-DEC-172`, `178`, `179`, `180`, `188`, `191`, `192`, `193`, `200`, `202`, `203`, `209`, `221`; gate `1.10` |
| Source SHA | Backend `bf5c6bde` |
| Deret ID | `BE-RWI-165` s.d. `BE-RWI-171` |
| Roadmap pendamping | `frontend-roadmap-finishing.md`, `requirement-traceability-finishing.md` |

**Kebijakan verifikasi backend.** Mengikuti `rules/backend/TEST_POLICY.md`: bukti task backend adalah QBE preflight/conformance, review diff/scope, `dotnet build` project aplikasi, verifikasi API/kontrak, verifikasi proses bisnis dengan contoh berangka, dan verifikasi runtime bila lingkungan tersedia. Tidak ada task automated test, dan tidak adanya automated test bukan gap.

**Pada setiap handoff ke `build-module-backend`:** QBE preflight dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` beserta `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Wewenang menerapkan migration ke database dan deployment tidak diberikan roadmap ini.

**Satu kalimat terpenting** (`02-backend-architecture.md` 12.0): sub-modul ini tidak memiliki satu tabel pun di `InPatientManagement`; seluruh data barunya tinggal di `ClinicalManagement`, `MasterData`, `NutritionManagement`, dan `BloodBankManagement`.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

**Otorisasi eksekusi 5 Oktober 2026.** Pengguna menyetujui implementasi BE-RWI-165–171, dependency BE-RWI-146/152/153 yang diperlukan, migration dan database update development. Build/update hanya pada tahap akhir seluruh kode. Status DRAFT pada metadata merupakan riwayat perencanaan.

**Pembaruan bukti 5 Oktober 2026 (penyelesaian).** Ketujuh task `BE-RWI-165`–`171` ✅. Sesi Codex terhenti karena batas penggunaan dan dilanjutkan Claude: error kompilasi diperbaiki, kode direview, lalu satu build akhir `dotnet build` `0 Error(s)` dan satu migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` (bagian `K9`, `K10`, `K11`, plus `InpEpisode.Version` milik `BE-RWI-153`) dibuat dan diterapkan ke database development bersama migration tertunda `20261005050735_AddNutritionAndBloodInstructionVerification`. Pengemasan satu migration menggantikan migration per task pada catatan migration di bawah. Uji API/UAT runtime `NOT RUN`, dikecualikan atas instruksi pengguna 5 Oktober 2026.

## Grafik Urutan Dependency

```text
BE-RWI-172 ✅ [EPS] ─> BE-RWI-167 ✅ ─┬─> BE-RWI-168 ✅
                                      │
BE-RWI-155 ✅ [IB] ─────┐             │
                        │             │
BE-RWI-153 ✅ [IB] ─────┴─────────────┘

BE-RWI-170 ✅ ─> BE-RWI-171 ✅

BE-RWI-165 ✅

BE-RWI-166 ✅

BE-RWI-169 ✅
```

| Label | Asal |
|---|---|
| `[EPS]` | Task backend sub-modul `episode-rawat-inap` pada `../../episode-rawat-inap/roadmap/backend-roadmap-finishing.md` |
| `[IB]` | Task backend sub-modul `integrasi-billing` pada `../../integrasi-billing/roadmap/backend-roadmap-finishing.md` |

Keduanya cermin baca-saja. Jumlah pasangan prasyarat→task: **5**, sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RWI-165`, `BE-RWI-166`, `BE-RWI-169`, `BE-RWI-170` — boleh paralel, kecuali catatan migration |
| 1 | `BE-RWI-172` [EPS] | `BE-RWI-167` |
| 2 | `BE-RWI-167`, `BE-RWI-153` [IB], `BE-RWI-155` [IB] | `BE-RWI-168` |
| 2 | `BE-RWI-170` | `BE-RWI-171` |

**Pemetaan ke gelombang `04-prd-to-mvp.md` bagian 23.20.** `MVP-1` (`RWF-W2`): `BE-RWI-165`, `BE-RWI-166`. `MVP-2` (`RWF-W4`): `BE-RWI-167`, `BE-RWI-168` — bentuk data `K8` sudah maju ke `RWF-W3` lewat `BE-RWI-172`. `MVP-3` (`RWF-W7`): `BE-RWI-169`, `BE-RWI-170`. `MVP-4`: `BE-RWI-171`. Catatan perencanaan: pemilihan kantong (`GET selectable-units`) ikut `BE-RWI-170` di `MVP-3`, maju dari `MVP-4`, karena monitoring tidak dapat dimulai tanpa kantong (`INV-RWF-17`) dan persetujuan Bank Darah sudah ada (`RWI-DEC-209`). Pemilik dapat menolak pemindahan ini saat approval.

**Catatan migration.** Satu `ApplicationDbContext` dan satu snapshot model. `K9` dipecah per task: WSD (`BE-RWI-165`), pemakaian alat (`BE-RWI-168`), surveilans (`BE-RWI-169`), monitoring transfusi (`BE-RWI-170`); ditambah `K10` (`BE-RWI-166`, boleh digabung dengan `R10` milik `BE-RWI-161`) dan `K11` (`BE-RWI-171`). Task bermigration dikerjakan berurutan, tidak paralel di branch terpisah. `K8` bukan milik roadmap ini; ia lahir di `BE-RWI-172` sebagai satu migration `MasterData` bersama `E4` (`02-module-map.md` 7.4).

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-165` | Observasi WSD per selang dengan volume dihitung server dan satu data cairan | `FR-RWF-054`, `058`; `RWI-DEC-200`; `INV-RWF-10` s.d. `12`; `UAT-RWF-09`, `22` | `0.6.0` API 8.4; backend 12.6.2 | `CliFluidBalanceEntry` dan mesin revisinya | Dua model, migration `K9` bagian WSD, service, controller | — | Kartu | Kartu | Resource `FluidBalance` (G-15) / Muhammad Hamzah | Kartu |
| `BE-RWI-166` | Diet Medis atas instruksi dokter tersimpan di Gizi dan diverifikasi dokter penetap | `FR-RWF-055`; `RWI-DEC-178`, `188`, `191`; `INV-RWF-19`; `UAT-RWF-26`; `RWI-AC-303` | API 8.8, 8.9; integrasi 9.5 | `NutritionDietService`, `IsDoctorAssignedAsync` | Tiga kolom, migration `K10`, verifikasi, adapter dan controller Rawat Inap | — | Kartu | Kartu | Modul Gizi milik Ikbal Yulianto | Kartu |
| `BE-RWI-167` | Master jenis alat medis dapat dikelola; tarif alat lewat master tarif yang sama | `FR-RWF-060`, `061`, `068`; `RWI-DEC-179`, `180`, `193` | API 8.2 | Model, kolom tarif, dan isian tarif dari `BE-RWI-172` | Service, controller, DTO jenis alat | `BE-RWI-172` [EPS] | Kartu | Kartu | Satuan dan pembulatan disahkan pemilik tarif / Muhammad Hamzah | Kartu |
| `BE-RWI-168` | Pemakaian alat tercatat, tertagih ke invoice `RANAP`, dan tertutup otomatis saat keluar ruangan | `FR-RWF-062` s.d. `066`, `069`; `RWI-DEC-179`, `192` (4); `INV-RWF-13`, `14`; `INT-RWF-06` s.d. `08`; `UAT-RWF-06`, `27` | API 8.3; integrasi 9.2 | `ClinicalMilestoneFactProducer`, jembatan folio Billing, `record-departure` | Dua model, migration `K9` bagian pemakaian alat, service, controller, sumber `EQUIPMENT_USAGE` di Billing | `BE-RWI-167`, `BE-RWI-153` [IB], `BE-RWI-155` [IB] | Kartu | Kartu | Menyentuh Billing dan alur keluar ruangan / Muhammad Hamzah, Yasmina | Kartu |
| `BE-RWI-169` | Surveilans infeksi luka operasi terbentuk, diisi, berhenti, dan menandai dicurigai lewat register nosokomial | `FR-RWF-083`, `084`, `091`, `092`; `RWI-DEC-202`; `INV-RWF-15`, `16`; `UAT-RWF-18`, `28` | API 8.5; integrasi 9.3 | `TrxNosocomialInfection`, instrumen klinis berversi, tanda vital | Tiga model, migration `K9` bagian surveilans, worker, controller, data awal `K12` | — | Kartu | Kartu | Gerbang produksi G-21 / Muhammad Hamzah | Kartu |
| `BE-RWI-170` | Monitoring transfusi per kantong yang sudah diserahkan Bank Darah | `FR-RWF-085`, `093`; `RWI-DEC-203`, `209`; `INV-RWF-17`, `18`; `UAT-RWF-19`, `29` | API 8.6; integrasi 9.4 | `BbkBloodUnit` (baca saja) | Tiga model, migration `K9` bagian transfusi, service, controller | — | Kartu | Kartu | Gerbang produksi G-21, G-22 / Muhammad Hamzah | Kartu |
| `BE-RWI-171` | Reaksi transfusi sampai di kotak masuk Bank Darah dengan pengiriman ulang | `FR-RWF-085`; `RWI-DEC-203`, `209`; `INT-RWF-12`; `UAT-RWF-19`; `RWI-AC-303` | API 8.7; integrasi 9.4 | Reaksi dari `BE-RWI-170` | Model, migration `K11`, service, controller Bank Darah, worker Clinical | `BE-RWI-170` | Kartu | Kartu | Modul Bank Darah milik Sukma Giri Pratama | Kartu |

## Kartu task

### ✅ `BE-RWI-165` — Observasi WSD per selang (`K9` bagian WSD)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Ketujuh acceptance criteria terpetakan ke source. `dotnet build` build akhir `0 Error(s)`, 233 warning (nol di berkas baru); migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` dibuat dan diterapkan ke database development (`Done.`, nol `Pending`). Uji API/UAT runtime `NOT RUN`; butir DoD itu **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Bukti: [laporan](../task/report/backend/BE-RWI-165.md) |
| **Outcome** | Perawat mendaftarkan selang WSD dan mencatat pembacaan per shift per selang. Server menghitung volume yang bertambah dan membuat tepat satu entri cairan per pembacaan, sehingga Spooling Cairan, Pengawasan Harian, dan WSD membaca data yang sama |
| **Requirement/decision** | `FR-RWF-054`, `FR-RWF-058`; `RWI-DEC-200`; `INV-RWF-10`, `11`, `12`; `AC-RWF-052`, `057`, `058`; `UAT-RWF-09`, `UAT-RWF-22` |
| **Kontrak** | `0.6.0`: API 8.4 (delapan endpoint `wsd-drains`); kamus data `CliWsdDrain`, `CliWsdReading`; backend 12.6.2, 12.7, 12.8 |
| **Reuse** | `CliFluidBalanceEntry` (`SourceCategory = DrainOrWsd = 14`), `CliFluidBalanceEntryRevision`, enum `ClinicalMeasurementStatus` |
| **Cakupan** | Model `CliWsdDrain`, `CliWsdReading` beserta configuration dan migration `K9` bagian WSD; enum `CliWsdDrainStatus`; `CliWsdObservationService` (`RegisterDrainAsync`, `RemoveDrainAsync`, `RecordReadingAsync`, `CorrectLatestReadingAsync`, `CancelLatestReadingAsync`); `WsdObservationController` dengan Resource `FluidBalance` |
| **Dependency** | — |
| **Acceptance criteria** | 1. Satu selang: sisa lalu 200 ml, dibuang 300 ml, sisa sekarang 150 ml → bertambah 250 ml dan satu entri cairan `DrainOrWsd` 250 ml (`UAT-RWF-09`). 2. Dua selang dihitung terpisah per selang (`UAT-RWF-22`). 3. Hasil negatif → 422 `CLI-WSD-001`. 4. Pembacaan sesudah selang dilepas → 422 `CLI-WSD-002`. 5. Koreksi selain pembacaan terakhir → 422 `CLI-WSD-003`; koreksi pembacaan terakhir merevisi entri cairan lewat mesin revisi yang ada. 6. Pembacaan pertama memakai sisa awal saat pemasangan, atau 0 ml bila tidak diisi. 7. Tidak ada tabel volume WSD kedua (`INV-RWF-10`) |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; pemeriksaan skrip migration terhadap kamus data; verifikasi API; verifikasi proses bisnis dengan contoh berangka di atas |
| **Risiko/pemilik** | Permission memakai Resource `FluidBalance` yang sudah ada (gate G-15). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-165.md` mencatat status penerapan migration apa adanya; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-166` — Diet Medis atas instruksi dan verifikasi dokter (`K10`)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Kedelapan acceptance criteria terpetakan ke source. `dotnet build` build akhir `0 Error(s)`, 233 warning (nol di berkas baru); migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` dibuat dan diterapkan ke database development (`Done.`, nol `Pending`). Uji API/UAT runtime `NOT RUN`; butir DoD itu **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Catatan untuk pemilik: penghentian diet atas instruksi menimpa penetap (lihat laporan). Bukti: [laporan](../task/report/backend/BE-RWI-166.md) |
| **Outcome** | Perawat menetapkan, mengganti, atau menghentikan diet pasien lewat Rawat Inap atas instruksi dokter berpenugasan. Diet tersimpan di modul Gizi berstatus menunggu verifikasi, lalu dokter penetap memverifikasinya |
| **Requirement/decision** | `FR-RWF-055`; `RWI-DEC-178`, `RWI-DEC-188`, `RWI-DEC-191`; `INV-RWF-19`; `AC-RWF-055`; `UAT-RWF-26`; `RWI-AC-303` |
| **Kontrak** | API 8.8 (`POST episodes/{episodeId}/diets`, `POST …/{dietId}/stop`), 8.9 (`POST nutrition-management/diets` diperluas, worklist, `verify-instruction`); integrasi 9.5 (`INT-RWF-13`); backend 12.7 |
| **Reuse** | `NutritionDietService.PrescribeAsync`/`StopAsync`; `InpatientClinicalContextService.IsDoctorAssignedAsync`; `MstDoctor.WorkforceProfileId` |
| **Cakupan** | Kolom `InstructionVerificationStatus`, `InstructionVerifiedAt`, `InstructionVerifiedByUserId` pada `GziPatientDiet`; configuration; migration `K10`; enum `GziInstructionVerificationStatus` (bila belum dibuat `BE-RWI-161`); `PrescribeAsync` menerima status verifikasi; worklist dan verifikasi dengan `NutritionPatientDiet : VerifyInstruction`; `InpDietOrderAdapter` dan `InpatientDietController` |
| **Dependency** | — |
| **Acceptance criteria** | 1. Perawat tanpa dokter → 400 "Dokter pemberi instruksi wajib dipilih" (`UAT-RWF-26`). 2. Dengan dokter berpenugasan → diet tersimpan di Gizi berstatus `Pending`, `PrescribedByWorkforceId` = profil dokter itu. 3. Dokter tidak berpenugasan → 403 dan tidak ada yang tersimpan. 4. Dokter lain memverifikasi → 403 `GIZ-VER-001`; dokter penetap → `Verified`; verifikasi kedua → 409 `GIZ-VER-002`. 5. Dokter atau ahli gizi menulis sendiri → `NotRequired`. 6. Ganti diet aktif tanpa alasan → `GIZ010` tetap berlaku. 7. Alur diet Gizi yang sudah ada tidak berubah (`RWI-AC-303`). 8. `IdempotencyKey` sama tidak membuat diet ganda |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration; verifikasi API; verifikasi proses bisnis termasuk regresi diet poliklinik |
| **Risiko/pemilik** | Enum dipakai bersama `BE-RWI-161`; `K10` boleh satu migration dengan `R10`. Modul Gizi milik Ikbal Yulianto (persetujuan `RWI-DEC-191`) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-167` — Master jenis alat medis

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Kelima acceptance criteria terpetakan ke source. `dotnet build` build akhir `0 Error(s)`, 233 warning (nol di berkas baru); tanpa perubahan skema (tabel dan kolom tarif dari `BE-RWI-172`). Uji API/UAT runtime `NOT RUN`; butir DoD itu **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Isi `filters/metadata` masih minimal terhadap standar master data (dicatat di laporan). Bukti: [laporan](../task/report/backend/BE-RWI-167.md) |
| **Outcome** | Admin Master Data mengelola jenis alat medis besar beserta satuan tagih dan pembulatannya; tarif alat per kelas memakai master tarif yang sama dengan tindakan dan obat |
| **Requirement/decision** | `FR-RWF-060`, `FR-RWF-061`, `FR-RWF-068`; `RWI-DEC-179`, `RWI-DEC-180`, `RWI-DEC-193`; `AC-RWF-060` |
| **Kontrak** | API 8.2 (`master-data/medical-equipments`, lima endpoint; tarif alat lewat master tarif dengan field `MedicalEquipmentId`); backend 12.7 |
| **Reuse** | Model `MstMedicalEquipment`, enum satuan dan pembulatan, kolom `MstTariff.MedicalEquipmentId`, dan isian `MedicalEquipmentId` pada endpoint master tarif — seluruhnya dibuat `BE-RWI-172` |
| **Cakupan** | `MedicalEquipmentDtos`, `MedicalEquipmentService`, `MedicalEquipmentController` (`MedicalEquipment : Read`, `: Create`, `: Update`); kode `MST-EQP-001`; endpoint master tarif menolak `MedicalEquipmentId` yang merujuk alat nonaktif. Isian tarif sendiri lahir di `BE-RWI-172` supaya satu endpoint tarif tidak diubah tiga task. Pemeriksaan `MST-EQP-002` dikerjakan `BE-RWI-168`, karena tabel pemakaian lahir di sana |
| **Dependency** | `BE-RWI-172` [EPS] |
| **Acceptance criteria** | 1. "Ventilator" per hari dengan pembulatan ke atas tersimpan. 2. Kode ganda → 409 `MST-EQP-001`. 3. Alat nonaktif tidak muncul pada pilihan alat aktif dan tidak dapat dirujuk tarif baru. 4. Tarif per kelas untuk alat aktif dapat dibuat lewat endpoint master tarif; tarif tindakan dan obat lama tidak berubah (regresi `RWI-DEC-193`). 5. Baris registry `MedicalEquipment : Read/Create/Update` lahir dari atribut endpoint (Resource tanpa awalan `Mst`) |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; verifikasi regresi endpoint tarif |
| **Risiko/pemilik** | Satuan dan pembulatan tiap jenis alat disahkan pemilik tarif (`02-backend-architecture.md` 12.12). Pemilik: Muhammad Hamzah; `MasterData` milik seluruh tim (`RWI-DEC-193`) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-168` — Pemakaian alat dan tagihannya (`K9` bagian pemakaian alat)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Kesembilan acceptance criteria terpetakan ke source. `dotnet build` build akhir `0 Error(s)`, 233 warning (nol di berkas baru); migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` dibuat dan diterapkan ke database development (`Done.`, nol `Pending`). Verifikasi dengan Billing sungguhan dan UAT `NOT RUN`; butir DoD itu **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Gap integrasi 9.2: harga kontrak penjamin (`MstInsuranceTariff`) belum diterapkan resolver. Bukti: [laporan](../task/report/backend/BE-RWI-168.md) |
| **Outcome** | Perawat mencatat mulai dan selesai pemakaian alat. Server menghitung unit dan menerbitkan tagihan ke invoice `RANAP` lewat jalur klinis yang sama dengan tindakan. Pemakaian yang masih berjalan ditutup otomatis saat pasien keluar ruangan |
| **Requirement/decision** | `FR-RWF-062` s.d. `066`, `FR-RWF-069`; `RWI-DEC-179`, `RWI-DEC-192` butir 4; `INV-RWF-13`, `14`; `INT-RWF-06`, `07`, `08`; `AC-RWF-061` s.d. `064`; `UAT-RWF-06`, `UAT-RWF-27` |
| **Kontrak** | API 8.3 (lima endpoint `equipment-usages`); integrasi 9.2 (`INT-RWF-07`), `INT-RWF-06`, `INT-RWF-08`; backend 12.5, 12.7 |
| **Reuse** | `ClinicalMilestoneFactProducer`; jembatan folio Billing dan `BillingSourceTariffResolver` (diperluas `BE-RWI-155`); `InpatientClearanceService.GetLatestStatusAsync`; `IsDoctorAssignedAsync`; `record-departure` (`BE-RWI-153`) |
| **Cakupan** | Model `CliEquipmentUsage`, `CliEquipmentUsageRevision`, configuration, migration `K9` bagian pemakaian alat; enum `CliEquipmentUsageStatus`; `CliEquipmentUsageService` (`StartAsync`, `FinishAsync`, `CancelAsync`, `CorrectTimeAsync`, `CloseRunningForDepartureAsync`); `EquipmentUsageController`; fakta `EQUIPMENT_USAGE` berkunci per pemakaian dan revisi; Billing mengenal `SourceContext = EQUIPMENT_USAGE` (kelompok Pemakaian Alat) dan mencari tarif lewat `MedicalEquipmentId`, kelas, lalu `MstInsuranceTariff`; panggilan `INT-RWF-06` dari `record-departure` sesudah commit; pemeriksaan `MST-EQP-002` |
| **Dependency** | `BE-RWI-167`, `BE-RWI-153` [IB], `BE-RWI-155` [IB] |
| **Acceptance criteria** | 1. Ventilator per hari, pembulatan ke atas, 1 Okt 08.00 s.d. 3 Okt 11.00 → 3 unit dan satu baris invoice `RANAP` 3 × tarif kelas (`UAT-RWF-06`). 2. Tarif tidak ada → `TARIFF_NOT_FOUND` dan invoice tidak dapat difinalkan. 3. Batal saat invoice `FINAL` → 422 `CLI-EQP-002` (`UAT-RWF-27`). 4. Pembatalan hanya membatalkan charge pemakaian itu; charge alkes Farmasi tidak tersentuh (`AC-RWF-061`). 5. Koreksi waktu menyimpan revisi lama dan memperbarui tagihan tanpa dobel. 6. Dokter penanggung jawab tidak berpenugasan → 403. 7. Pasien keluar ruangan saat pemakaian `Running` → ditutup pada waktu keluar dan ditandai "perlu diperiksa perawat"; kegagalan penutupan tidak membatalkan keluar ruangan. 8. Respons tanpa rupiah. 9. Mengubah satuan tagih alat yang sedang dipakai → 422 `MST-EQP-002` |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration; verifikasi API; verifikasi proses bisnis dengan Billing sungguhan dan contoh berangka |
| **Risiko/pemilik** | Menyentuh jembatan folio Billing (Yasmina) dan alur keluar ruangan. Pemilik: Muhammad Hamzah, Yasmina |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-169` — Surveilans infeksi luka operasi (`K9` bagian surveilans, `K12`)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Kesembilan acceptance criteria terpetakan ke source. `dotnet build` build akhir `0 Error(s)`, 233 warning (nol di berkas baru); migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` dibuat dan diterapkan ke database development (`Done.`, nol `Pending`). Verifikasi dengan kasus OK di lingkungan uji `NOT RUN`; **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Gerbang produksi G-21 tetap terbuka. Bukti: [laporan](../task/report/backend/BE-RWI-169.md) |
| **Outcome** | Formulir surveilans per kasus operasi terbentuk otomatis sesudah operasi selesai — bila versi formulir sudah disahkan — lalu diisi harian dengan suhu dibaca dari tanda vital, berhenti saat pasien keluar ruangan, dan dapat ditandai "dicurigai" oleh tim PPI lewat register infeksi nosokomial |
| **Requirement/decision** | `FR-RWF-083`, `084`, `091`, `092`; `RWI-DEC-202`; `INV-RWF-15`, `16`; `INT-RWF-09`, `10`; `AC-RWF-083`, `095`, `096`; `UAT-RWF-18`, `UAT-RWF-28` |
| **Kontrak** | API 8.5; integrasi 9.3; backend 12.6.3, 12.7, 12.12 |
| **Reuse** | `TrxNosocomialInfection` beserta aturan `NosocomialInfectionController` dan layanan nomornya; `CliClinicalInstrument`/`Version`; `TrxPatientVitalSign`; `InpatientClinicalContextService` (episode aktif dan `PhysicallyLeftAt`) |
| **Cakupan** | Model `CliSurgicalSiteSurveillance`, `…Entry`, `…EntryRevision`, configuration, migration `K9` bagian surveilans; enum `CliSurveillanceStatus`; `ClinicalInstrumentKind` ditambah `SurgicalSiteSurveillanceForm = 7`; `CliSurgicalSiteSurveillanceService`; `CliSurgicalSiteSurveillanceWorker` tiap 5 menit; `SurgicalSiteSurveillanceController` (`SurgicalSiteSurveillance : Read`, `: Update`, `: Review`); data awal `K12` berupa instrumen versi `Draft`; konfigurasi `MaxDay = 15`, `WorkerIntervalMinutes = 5`; peringatan Daftar Pantau bila belum ada versi `Approved` |
| **Dependency** | — |
| **Acceptance criteria** | 1. Operasi selesai 1 Okt → hari ke-1 jatuh pada 2 Okt. 2. Versi formulir masih `Draft` → formulir tidak dibentuk dan peringatan tampil di daftar PPI (`UAT-RWF-28`). 3. Suhu 38,5 °C hari ke-2 terbaca dari tanda vital dan tidak dapat diketik. 4. Tim PPI menandai → satu `TrxNosocomialInfection` `SurgicalSiteInfection` berstatus `Suspected`; tanda kedua → 409 `CLI-SSI-004`. 5. Pasien keluar hari ke-5 → `StoppedOnDeparture`; isian baru → 422 `CLI-SSI-001` (`UAT-RWF-18`). 6. Hari di luar 1–15 → 422 `CLI-SSI-002`. 7. Isian tidak sesuai definisi versi → 422 `CLI-SSI-003`. 8. Satu formulir per kasus operasi. 9. Tidak ada kode modul Kamar Operasi yang diubah |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration; verifikasi API; verifikasi proses bisnis dengan kasus OK `Completed` di lingkungan uji |
| **Risiko/pemilik** | Gerbang produksi G-21: formulir wajib disahkan (versi `Approved` beserta nama pengesah) sebelum dipakai pasien sungguhan. Pemilik: Muhammad Hamzah; pengesahan oleh pemilik klinis atau komite PPI |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-170` — Monitoring transfusi per kantong (`K9` bagian transfusi)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Kedelapan acceptance criteria terpetakan ke source. `dotnet build` build akhir `0 Error(s)`, 233 warning (nol di berkas baru); migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` dibuat dan diterapkan ke database development (`Done.`, nol `Pending`). Verifikasi dengan kantong di lingkungan uji `NOT RUN`; **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Gerbang produksi G-21 dan G-22 tetap terbuka. Bukti: [laporan](../task/report/backend/BE-RWI-170.md) |
| **Outcome** | Perawat memantau transfusi per kantong yang sudah diserahkan Bank Darah kepada pasien itu: empat titik ukur dengan tanda terlambat, hentikan, selesai, batal, dan catat reaksi |
| **Requirement/decision** | `FR-RWF-085`, `FR-RWF-093`; `RWI-DEC-203`, `RWI-DEC-209`; `INV-RWF-17`, `18`; `INT-RWF-11`; `AC-RWF-084`, `097`, `098`; `UAT-RWF-19` (bagian Clinical), `UAT-RWF-29` |
| **Kontrak** | API 8.6 (delapan endpoint `transfusion-monitorings`); integrasi 9.4; backend 12.6.4, 12.7, 12.12 (`LateToleranceMinutes = 10`) |
| **Reuse** | `BbkBloodUnit` (`IssuedToPatientId`, `IssuedAt`, `PmiBagNumber`, `BloodComponentId`), dibaca saja |
| **Cakupan** | Model `CliTransfusionMonitoring`, `…Point`, `…Reaction`, configuration, migration `K9` bagian transfusi; enum `CliTransfusionMonitoringStatus`, `CliTransfusionPointType`, `CliReactionNoticeDelivery`; `CliTransfusionMonitoringService`; `TransfusionMonitoringController`; konfigurasi toleransi. Reaksi disimpan berstatus pemberitahuan `Pending`; pengirimannya ke Bank Darah dikerjakan `BE-RWI-171` |
| **Dependency** | — |
| **Acceptance criteria** | 1. Hanya kantong ber-`IssuedToPatientId` pasien itu, diserahkan sejak admisi, dan belum dipantau yang tampil; kantong pasien lain tidak tampil dan pemanggilan langsung → 422 `CLI-TRF-001` (`UAT-RWF-29`). 2. Kantong yang sama dua kali → 409 `CLI-TRF-002`. 3. Titik 1 jam terlewat → bertanda terlambat; isian tanpa keterangan → 422 `CLI-TRF-003`. 4. Titik sesudah transfusi dihentikan → 422 `CLI-TRF-004`. 5. Batal sesudah ada titik ukur → 422 `CLI-TRF-005`. 6. Reaksi tersimpan dengan status pemberitahuan `Pending`. 7. Nomor kantong tidak dapat diketik. 8. Volume darah tidak disalin ke cairan |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration; verifikasi API; verifikasi proses bisnis dengan kantong yang sudah diserahkan di lingkungan uji |
| **Risiko/pemilik** | Gerbang produksi G-21 (titik ukur) dan G-22 (toleransi) disahkan pemilik klinis. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-171` — Pemberitahuan reaksi transfusi ke Bank Darah (`K11`)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 5 Oktober 2026.** Kelima acceptance criteria terpetakan ke source. `dotnet build` build akhir `0 Error(s)`, 233 warning (nol di berkas baru); migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` dibuat dan diterapkan ke database development (`Done.`, nol `Pending`). Verifikasi disaksikan petugas Bank Darah `NOT RUN`; **dikecualikan atas instruksi pengguna 5 Oktober 2026**. Bukti: [laporan](../task/report/backend/BE-RWI-171.md) |
| **Outcome** | Reaksi transfusi yang dicatat bangsal sampai di kotak masuk Bank Darah. Pengiriman yang gagal dicoba ulang tiap menit tanpa menghilangkan catatan klinisnya, dan petugas Bank Darah menyatakan sudah menindaklanjuti |
| **Requirement/decision** | `FR-RWF-085` (reaksi); `RWI-DEC-203`, `RWI-DEC-209`; `INT-RWF-12`; `UAT-RWF-19` (bagian Bank Darah); `RWI-AC-303` |
| **Kontrak** | API 8.7 (tiga endpoint `transfusion-reaction-notices`); integrasi 9.4; backend 12.5, 12.7 |
| **Reuse** | `CliTransfusionReaction` dari `BE-RWI-170` |
| **Cakupan** | Model `BbkTransfusionReactionNotice`, enum `BbkReactionNoticeStatus`, configuration, migration `K11`; `BbkTransfusionReactionNoticeService` (`ReceiveAsync` idempoten pada `ClinicalReactionId`, `AcknowledgeAsync`); `BbkTransfusionReactionNoticeController` (`TransfusionReactionNotice : Read`, `: Acknowledge`); `CliTransfusionReactionNoticeWorker` tiap 1 menit; `RecordReactionAsync` memanggil `ReceiveAsync` sesudah commit |
| **Dependency** | `BE-RWI-170` |
| **Acceptance criteria** | 1. Reaksi "menggigil" tersimpan → pemberitahuan muncul di kotak masuk Bank Darah dengan pasien, kantong, reaksi, waktu, dan unit (`UAT-RWF-19`). 2. Bank Darah tidak terjangkau → `Failed`, worker mengirim ulang, reaksi klinis tetap tersimpan. 3. Pengiriman ganda tidak membuat pemberitahuan dua kali. 4. Tindak lanjut menyimpan catatan dan pelaku. 5. Alur Bank Darah lain tidak berubah (`RWI-AC-303`) |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration; verifikasi API; verifikasi proses bisnis, disaksikan petugas Bank Darah bila lingkungan tersedia |
| **Risiko/pemilik** | Komunikasi darurat di luar sistem tetap prosedur klinis (G-23); notifikasi seketika `DEFERRED`. Modul Bank Darah milik Sukma Giri Pratama (persetujuan `RWI-DEC-209`) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |
