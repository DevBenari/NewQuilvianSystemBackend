# Roadmap Backend — Episode Rawat Inap, Workspace PPRI

| Field | Nilai |
|---|---|
| Roadmap | `episode-rawat-inap/roadmap/backend-roadmap-workspace-ppri.md` — revision `1` |
| Blueprint | `RWI-BP-001` revision `9`, sub-modul `episode-rawat-inap`, kontrak **`0.11.0` `approved`** 2026-10-08 lewat `RWI-DEC-265` |
| Status roadmap | **`APPROVED`** — Muhammad Hamzah memerintahkan eksekusi seluruh task pada 8 Oktober 2026 ("kerjakan semua task tersebut sampai dengan tuntas dan ditandai selesai"); instruksi itu dicatat sebagai approval roadmap. Hasil 8 Oktober 2026: 16 task ✅ (`BE-RWI-185` s.d. `189`, `192` s.d. `202`), 3 task tetap ⛔ (`BE-RWI-190`, `191`, `203`) |
| Ditulis | 8 Oktober 2026 oleh `plan-module-delivery` |
| Masukan dan hash approval | `02-backend-architecture.md` bagian 13 (`0a74b298…`), `data/data-dictionary.md` bagian 20 (`c81b0742…`), `contracts/api-contract.md` bagian 12 (`17119194…`), `contracts/validation-matrix.md` bagian 15 (`7c649c32…`), `contracts/state-transition-matrix.md` bagian 10 (`4336f1f5…`), `contracts/integration-contract.md` bagian 10 (`e864a12d…`), `contracts/permission-audit-matrix.md` bagian 10 (`04cc3b28…`), `testing/acceptance-test-matrix.md` bagian 21 (`0c34160f…`); urutan migration `02-module-map.md` revision `5` bagian 8.4 (`19445a0a…`). Hash lengkap pada `../blueprint-manifest.md` bagian 12.1 |
| Keputusan | `RWI-DEC-225` s.d. `266`; gerbang terbuka `RWI-OQ-128`, `RWI-OQ-129`, `DEC-INP-020`; gate `1.11` bagian 20 |
| Source SHA | Backend `fdf85a07` |
| Deret ID | `BE-RWI-185` s.d. `BE-RWI-203` |
| Roadmap pendamping | `frontend-roadmap-workspace-ppri.md`, `requirement-traceability-workspace-ppri.md` |

**Kebijakan verifikasi backend.** Mengikuti `rules/backend/TEST_POLICY.md`: bukti task backend adalah QBE preflight/conformance, review diff/scope, `dotnet restore` dan `dotnet build` project aplikasi, verifikasi kontrak API, verifikasi proses bisnis dengan contoh data samaran, dan verifikasi runtime bila lingkungan tersedia. Tidak ada task automated test, dan tidak adanya automated test bukan gap.

**Pada setiap handoff ke `build-module-backend`:** QBE preflight dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` beserta `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (prefix `Inp` `ACTIVE`). Wewenang menerapkan migration ke database dan deployment **tidak** diberikan roadmap ini. Sesuai kebiasaan pemilik, `dotnet build` dan penerapan migration dijalankan sekali di akhir satu batch task, bukan per task; kriteria `dotnet build` pada kartu dipenuhi oleh build batch itu dan dicatat apa adanya di laporan setiap task.

**Satu kalimat terpenting** (`02-backend-architecture.md` 13.0): Workspace PPRI menambah satu agregat dokumen admisi milik Rawat Inap dan membaca seluruh data modul lain lewat service pemiliknya; Rawat Inap tidak pernah menulis query ke tabel master modul lain (`INV-RWA-14`).

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

Roadmap ini memuat 19 task dan 3 gerbang keputusan, melewati batas 15 node satu grafik. Grafik dipecah per slice di bawah judul slice masing-masing; grafik di bawah ini adalah ringkasan antar-slice.

```text
Slice A Fondasi data dan bacaan cetak ─> Slice B Ruang kerja dan dokumen bertanda tangan
```

Slice B menunggu Slice A lewat dua task: `BE-RWI-193` (fondasi baca dan cetak) dan `BE-RWI-186` (butir master serah terima). Slice A tidak menunggu Slice B. Jumlah pasangan prasyarat→task pada kedua grafik slice: **25** (A: 14, B: 11), termasuk tiga pasangan gerbang→task, sama dengan isi kolom `Dependency`.

| Label | Arti |
|---|---|
| `[A]` | Task Slice A roadmap ini, digambar lengkap di grafik Slice A. Di grafik Slice B ia hanya titik sambung antar-blok (aturan grafik bagian 11), bukan task kedua |
| `{…}` | Keputusan atau persetujuan pemilik yang belum turun, bukan task |

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RWI-185`, `BE-RWI-187`, `BE-RWI-188`, `BE-RWI-189` — boleh paralel; `BE-RWI-187` dan `BE-RWI-188` tidak lagi tertahan sejak `RWI-DEC-266` |
| 2 | `BE-RWI-185` | `BE-RWI-192` |
| 3 | `BE-RWI-185`, `BE-RWI-192` | `BE-RWI-186` |
| 3 | `BE-RWI-187`, `BE-RWI-188`, `BE-RWI-189`, `BE-RWI-192` | `BE-RWI-193` |
| 4 | `BE-RWI-193` | `BE-RWI-194`, `BE-RWI-195`, `BE-RWI-196`, `BE-RWI-197`, `BE-RWI-198` — boleh paralel |
| 5 | `BE-RWI-194`, `BE-RWI-186` | `BE-RWI-199` |
| 5 | `BE-RWI-194` | `BE-RWI-200`, `BE-RWI-201` |
| 5 | `BE-RWI-194`, `BE-RWI-195` | `BE-RWI-202` |
| — | ⛔ menunggu `RWI-OQ-128` (pemilik `RegistrationManagement`) dan `BE-RWI-193` | `BE-RWI-190` |
| — | ⛔ menunggu `RWI-OQ-129` (Yasmina) dan `BE-RWI-193` | `BE-RWI-191` |
| — | ⛔ menunggu `DEC-INP-020` (Yasmina), `BE-RWI-194`, dan `BE-RWI-195` | `BE-RWI-203` |

Tiga pasangan berasal dari urutan migration atau kebutuhan data verifikasi, bukan dari pemakaian kode: `BE-RWI-185 → BE-RWI-192` (`E10` merujuk `MstInpatientClearanceItem`, maka sesudah `E9`), `BE-RWI-192 → BE-RWI-186` (aturan "jenis butir tidak dapat diubah setelah dipakai" membaca `InpAdmissionHandoverItem`), dan `BE-RWI-186 → BE-RWI-199` (verifikasi serah terima membutuhkan butir `STPB-*` dari data awal).

**Pemetaan ke gelombang `04-prd-to-mvp.md` bagian 24.20.** `RWA-MVP-0` (`EPIC-RWA-12`): `BE-RWI-185` s.d. `189`, `192` s.d. `194`. `RWA-MVP-1` (`EPIC-RWA-01`, `05`, `07`, `14`): `BE-RWI-195` s.d. `198`. `RWA-MVP-2` (`EPIC-RWA-03`, `06`, `08`, `10`, `11`): `BE-RWI-199` s.d. `202`. Di luar gelombang: `BE-RWI-203` (`EPIC-RWA-09`, `OPEN DECISION`). `BE-RWI-190` dan `191` adalah sambungan pengganti cadangan aman IPD; masing-masing ikut `RWA-MVP-1` begitu gerbangnya turun, dan IPD tetap boleh dirilis lebih dulu dengan cadangan (`RWI-AC-386`, `387`).

**Catatan jenis pertama.** Permintaan Privasi (`EPIC-RWA-06`) ikut dibangun di `BE-RWI-194` sebagai jenis dokumen pertama, supaya siklus dokumen generik dapat dibuktikan dengan satu jenis nyata yang punya slot kertas dan slot petugas. Layarnya tetap `RWA-MVP-2` (`FE-RWI-216`, yang sekaligus membangun kerangka dokumen frontend).

**Catatan migration.** Satu `ApplicationDbContext` dan satu snapshot model. Urutan yang mengikat (`02-module-map.md` 8.4): **`E9`** `MasterData` (`BE-RWI-185`) → **`E10`** `InPatientManagement` (`BE-RWI-192`) → **`E12`** data dan hak. `E12` tidak punya task tersendiri: data awal non-produksi ada di `BE-RWI-186`, butir hak akses lahir dari atribut endpoint `BE-RWI-193` lewat `AccessMenuSeeder`, sedangkan pemberian aksi per peran dan butir serah terima produksi diisi admin di layar. `E11` milik `BE-RWI-203`, di luar gelombang. Task bermigration tidak dikerjakan paralel di branch terpisah. Saringan jenis pada penutupan episode **wajib** satu task dengan `E9` (`BE-RWI-185`); butir serah terima produksi diisi admin sesudah keduanya dirilis (`02-backend-architecture.md` 13.12).

## Slice A — Fondasi data dan bacaan cetak

Hasil slice: data master dan sepuluh tabel siap, tiga modul pemilik menyediakan service baca, fondasi baca dan cetak berdiri, dan tiga cetakan tanpa tanda tangan — gelang dan label, IPD, General Consent cetak saja — terisi dari server.

### Grafik dependency slice A

```text
BE-RWI-185 ✅ ─┬──────────────────────┬─> BE-RWI-186 ✅
               │                      │
               └─> BE-RWI-192 ✅ ─┬───┘
                                  │
BE-RWI-187 ✅ ─┐                  │
               │                  │
BE-RWI-188 ✅ ─┴─┐                │
                 │                │
BE-RWI-189 ✅ ───┴─┐              │
                   │              │
                   └──────────────┴─> BE-RWI-193 ✅ ─┬─> BE-RWI-196 ✅
                                                     │
                                                     ├─> BE-RWI-197 ✅
                                                     │
                                                     ├─> BE-RWI-198 ✅
                                                     │
                                                     ├───────────────────┬─> BE-RWI-190 ⛔
                                                     │                   │
                                                     │  {RWI-OQ-128 ⛔} ─┘
                                                     │
                                                     └───────────────────┬─> BE-RWI-191 ⛔
                                                                         │
                                                        {RWI-OQ-129 ⛔} ─┘
```

`{…}` = persetujuan pemilik modul yang belum turun, bukan task. Pasangan: 14 (12 antar-task, 2 gerbang→task).

### Tabel task slice A

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-185` ✅ | Master butir dan pengaturan siap menampung Workspace PPRI; penutupan episode hanya membaca butir penutupan | `FR-RWA-030`, `050`, `126`; `RWI-DEC-241`, `243`, `247`; `INV-RWA-12` | Backend 13.9, 13.11, 13.12 (`E9`); data 20.15, 20.16, 20.18 | `MstInpatientClearanceItem`, `MstInpatientSetting`, `InpDischargeService.Closure` | Dua enum, 14 kolom, `CHECK`, index, migration `E9`, saringan penutupan dua titik, `InpSettingService` | — | Kartu | Kartu | Penutupan episode bisa tertahan bila saringan tertinggal / Muhammad Hamzah; `MasterData` `RWI-DEC-193` | Kartu |
| `BE-RWI-186` ✅ | Admin mengelola jenis, induk, sumber saran butir dan isian cetak; lingkungan non-produksi terisi data V1 | `FR-RWA-030`, `126`; `RWI-DEC-048`, `241`, `243`, `247`; `VAL-RWA-50` s.d. `55` | API 12.4; validation 15.7; backend 13.8.4, 13.13 | Service, controller, seeder master yang ada | DTO, saringan `checklistType`, validasi, seeder `STPB-*` dan nilai V1 | `BE-RWI-185`, `BE-RWI-192` | Kartu | Kartu | Nilai V1 hanya di seeder (`RWI-AC-368`) / Muhammad Hamzah; `MasterData` | Kartu |
| `BE-RWI-187` ✅ | Identitas pasien, isi QR, dan calon penanda tangan terbaca lewat service `PatientManagement` | `FR-RWA-003`, `021`, `052`, `082`; `RWI-DEC-257`, `259`, `264`, `266` | Backend 13.6, 13.8.4; integration `INT-RWA-01`, `02` | `MstPatient`, `MstPatientRelationship`, `MstPatientEmergencyContact`; `BuildPatientQrPayload` | `PatientProfileQueryService`, `PatientQrPayloadBuilder`, `PatientController` memakai builder | — | Kartu | Kartu | Menyentuh modul lain; disetujui `RWI-DEC-266` hanya service baca / pemilik `PatientManagement` lewat Muhammad Hamzah | Kartu |
| `BE-RWI-188` ✅ | Kop surat terbaca dari profil rumah sakit utama | `FR-RWA-126`; `RWI-DEC-247`, `264`, `266` | Backend 13.6, 13.8.4; `INT-RWA-12` | `MstHospitalSite` | `HospitalSiteProfileQueryService` | — | Kartu | Kartu | Disetujui `RWI-DEC-266` hanya service baca / pemilik HR Master Data lewat Muhammad Hamzah | Kartu |
| `BE-RWI-189` ✅ | Konteks penjamin membawa nomor kartu dan peserta; surat pengantar dan alergi aktif terbaca lewat service Clinical | `FR-RWA-003`, `051`, `070`; `RWI-DEC-253`, `254`, `262`, `264` | Backend 13.6, 13.8.4; `INT-RWA-03` s.d. `05` | `EncounterInsuranceService`, `DoctorCertificateService`, `PatientAllergyController` | Dua isian konteks; satu method surat; `PatientAllergyQueryService` | — | Kartu | Kartu | Konteks dipakai pricing; isian aditif / Muhammad Hamzah | Kartu |
| `BE-RWI-190` ⛔ | IPD mencetak dokter perujuk luar untuk pasien tanpa surat pengantar | `FR-RWA-070`; `RWI-DEC-254` (3); `RWI-AC-386` | Backend 13.8.4; `INT-RWA-06` | `RegPatientEncounter.ReferralDoctorId`, `MstReferralDoctor` | `EncounterReferralQueryService`; sambungan di `InpAdmissionSourceReader` | `BE-RWI-193`; `{RWI-OQ-128}` | Kartu | Kartu | ⛔ menunggu persetujuan pemilik `RegistrationManagement` | Kartu |
| `BE-RWI-191` ⛔ | IPD menampilkan tarif kamar per hari bagi pemegang `ViewAmount` | `FR-RWA-070`; `RWI-DEC-258`; `RWI-AC-387` | Backend 13.8.4; `INT-RWA-08`; API 12.2 `/base-data/amounts` | `ResolveRoomTariff`, `InsuranceCoverageService.ResolveTariffAsync` | `BillingCalculationService.GetDailyRoomRateAsync`; sambungan di source reader | `BE-RWI-193`; `{RWI-OQ-129}` | Kartu | Kartu | ⛔ menunggu persetujuan Yasmina | Kartu |
| `BE-RWI-192` ✅ | Sepuluh tabel dokumen admisi dan log cetak tersedia | `FR-RWA-120` s.d. `128`; `RWI-DEC-228`, `263`; `INV-RWA-01`, `04`, `09`, `10` | Backend 13.3, 13.9, 13.11, 13.12 (`E10`); data 20.2 s.d. 20.10, 20.13, 20.18 | Pola `InpAdmissionReferral`, `CliNursingIntervention`, `CliTransferHandover` | 10 model, 12 enum `Inp*`, configuration, `DbSet`, migration `E10` | `BE-RWI-185` | Kartu | Kartu | Constraint harus sama persis dengan DDL / Muhammad Hamzah | Kartu |
| `BE-RWI-193` ✅ | Fondasi baca dan cetak: penjaga tulis, pintu baca modul lain, resource hak akses, kop, log cetak | `FR-RWA-053`, `126`, `127`, `128`; `RWI-DEC-240` (7), `247`, `257`, `264`; `INV-RWA-08`, `09`, `14` | API 12.2 (`/letterhead`, `/print-logs`), 12.5; validation 15.1, 15.6; permission 10.1, 10.2; integration 10 | `InpPatientLocationQuery`, `AccessPermissionService`, pola `Idempotency-Key` | Controller dan 10 aksi, write guard, source reader, print service bagian catat | `BE-RWI-187`, `BE-RWI-188`, `BE-RWI-189`, `BE-RWI-192` | Kartu | Kartu | Pintu baca tunggal; kegagalan sumber tidak boleh `500` / Muhammad Hamzah | Kartu |
| `BE-RWI-196` ✅ | Data gelang dewasa/bayi dan label pasien tanpa data karangan | `FR-RWA-050` s.d. `053`; `RWI-DEC-243`, `253`, `259` | API 12.2 (`/identity-labels`); validation `VAL-RWA-45` | `PatientQrPayloadBuilder`, konteks penjamin | `InpWristbandRules`, endpoint, validasi jenis gelang pada catat cetak | `BE-RWI-193` | Kartu | Kartu | Aturan sapaan diverifikasi tim keselamatan pasien sebelum produksi (G-38) / Muhammad Hamzah | Kartu |
| `BE-RWI-197` ✅ | Data Dasar Rawat Inap (IPD) terangkai dengan garis kosong dan cadangan aman | `FR-RWA-070` s.d. `072`; `RWI-DEC-244`, `254`, `258`; `RWI-AC-365`, `375` | API 12.2 (`/base-data`, `/base-data/amounts`); validation `VAL-RWA-44` | Surat pengantar (`BE-RWI-189`), riwayat status, penempatan | Endpoint dua, aturan sumber, `CanPrint`, cadangan perujuk luar dan tarif kamar | `BE-RWI-193` | Kartu | Kartu | Isian tanpa sumber tidak boleh diisi tebakan / Muhammad Hamzah | Kartu |
| `BE-RWI-198` ✅ | Data cetak General Consent V1 dan Surat Persetujuan 12 butir tanpa tulis | `FR-RWA-020` s.d. `022`, `140`, `142`; `RWI-DEC-233`, `246`, `251`, `252` | API 12.2 (`/general-consent/print-data`) | Penanda isolasi dan intensif kamar/bed; relasi pasien | Endpoint baca, aturan tipe kamar, calon penanda tangan | `BE-RWI-193` | Kartu | Kartu | Tidak boleh ada jalur tulis (`RWI-DEC-230`) / Muhammad Hamzah | Kartu |

## Slice B — Ruang kerja dan dokumen bertanda tangan

Hasil slice: ruang kerja menghitung kelengkapan dan memberi peringatan di Detail Episode, dan lima jenis dokumen bertanda tangan — Privasi, Serah Terima, Nilai Kepercayaan, Selisih Biaya, Pelunasan Deposit — berjalan dari konsep sampai lengkap. Estimasi Biaya menunggu keputusan.

### Grafik dependency slice B

```text
BE-RWI-193 [A] ✅ ─┬─> BE-RWI-194 ✅ ─┬────────────────────┬─> BE-RWI-199 ✅
                   │                  │                    │
                   │                  │ BE-RWI-186 [A] ✅ ─┘
                   │                  │
                   │                  ├─> BE-RWI-200 ✅
                   │                  │
                   │                  ├─> BE-RWI-201 ✅
                   │                  │
                   │                  ├─────────────────────────────────────┬─> BE-RWI-202 ✅
                   │                  │                                     │
                   │                  └─────────┬───┬─> BE-RWI-203 ⛔       │
                   │                            │   │                       │
                   │          {DEC-INP-020 ⛔} ─┘   │                       │
                   │                                │                       │
                   └─> BE-RWI-195 ✅ ─┬─────────────┘                       │
                                      │                                     │
                                      └─────────────────────────────────────┘
```

`[A]` = titik sambung dari grafik Slice A. Pasangan: 11 (10 antar-task, 1 gerbang→task).

### Tabel task slice B

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-194` ✅ | Siklus dokumen generik dengan tanda tangan, salinan beku, versi, batal, dan cetak; Permintaan Privasi jenis pertama | `FR-RWA-060` s.d. `062`, `120` s.d. `125`, `128`; `RWI-DEC-237` s.d. `240`, `263`; `INV-RWA-01` s.d. `06`, `10` | API 12.2, 12.3; state 10.1; validation 15.1, 15.3, 15.4, 15.5, 15.6; data 20.2 s.d. 20.7 | Fondasi `BE-RWI-193` | Document, signature, snapshot, prefill dasar, print service bagian dokumen, handler Privasi | `BE-RWI-193` | Kartu | Kartu | Task terbesar; seluruh dokumen bergantung padanya / Muhammad Hamzah | Kartu |
| `BE-RWI-195` ✅ | Ringkasan ruang kerja, kelengkapan, rupiah header, dan peringatan Detail Episode | `FR-RWA-001` s.d. `008`; `RWI-DEC-234`, `245`, `250`, `256`, `258`; `RWI-AC-345`, `346`, `378`, `379` | API 12.1, 12.2 (`/summary`, `/summary/amounts`); state 10.2 | `InpEpisodeService`, `BillingDepositService`, `OperatingRoomCaseService` | `InpAdmissionCompletenessEvaluator`, dua endpoint, peringatan detail episode | `BE-RWI-193` | Kartu | Kartu | Detail episode tidak boleh gagal karena sumber lain / Muhammad Hamzah | Kartu |
| `BE-RWI-199` ✅ | Serah Terima Pasien Baru dengan butir beku, saran sistem, dan tiga tanda tangan berbeda | `FR-RWA-030` s.d. `034`; `RWI-DEC-239`, `241`, `255`, `262`; `INV-RWA-04`, `11` | Validation 15.4 (`20`, `21`), 15.5; data 20.5 | Butir master `STPB-*`, log cetak, surat pengantar | Handler serah terima, saran, syarat bed | `BE-RWI-194`, `BE-RWI-186` | Kartu | Kartu | Saran tidak boleh memilih otomatis / Muhammad Hamzah | Kartu |
| `BE-RWI-200` ✅ | Nilai Kepercayaan per episode dengan isian episode lalu; ringkasan hak pasien | `FR-RWA-110` s.d. `113`; `RWI-DEC-242`; `RWI-AC-362` | API 12.2 (`/patient-rights`); validation `VAL-RWA-16`, `23` | Fondasi dokumen | Handler, prefill episode lalu, endpoint ringkasan | `BE-RWI-194` | Kartu | Kartu | Data keyakinan sensitif (G-35) / Muhammad Hamzah | Kartu |
| `BE-RWI-201` ✅ | Selisih Biaya untuk penjamin asuransi/perusahaan dengan subjek berkode | `FR-RWA-100` s.d. `103`; `RWI-DEC-234`, `256`; `RWI-AC-377` | Validation `VAL-RWA-10`, `14`, `24`; data 20.4, 20.9 | Konteks penjamin | Handler, prefill "diri saya sendiri" | `BE-RWI-194` | Kartu | Kartu | Nomor identitas deklarer sensitif (G-35) / Muhammad Hamzah | Kartu |
| `BE-RWI-202` ✅ | Pelunasan Deposit dari angka Billing, jatuh tempo berbatas, angka beku, peringatan jatuh tempo | `FR-RWA-080` s.d. `085`; `RWI-DEC-231`, `248`, `252`, `258`, `260`, `261`, `263` | Validation `VAL-RWA-11`, `12`, `19`, `25`; data 20.10; API `/documents/{id}/amounts`, `/amount-print` | `BillingDepositService` | Handler, `InpDepositDueDateCalculator`, rupiah dokumen, peringatan terlewati | `BE-RWI-194`, `BE-RWI-195` | Kartu | Kartu | Angka uang; tidak pernah diketik / Muhammad Hamzah, Yasmina | Kartu |
| `BE-RWI-203` ⛔ | Estimasi Biaya Rekap dari tarif, penanda rencana tindakan, aturan wajib | `FR-RWA-090` s.d. `093`; `RWI-DEC-232`, `250`, `258` | Backend 13.12 (`E11`); data 20.11, 20.12, 20.14; API `/procedure-plan-mark` | `InsuranceCoverageService`, `AdministrationFeePolicyService`, `OperatingRoomCaseService` | `E11`, handler, penanda, aturan wajib Estimasi di evaluator | `BE-RWI-194`, `BE-RWI-195`; `{DEC-INP-020}` | Kartu | Kartu | ⛔ `OPEN DECISION` / Yasmina | Kartu |

## Kartu task

### ✅ `BE-RWI-185` — Bentuk data master Workspace PPRI dan saringan penutupan episode (`E9`)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Migration `E9` tergabung bersama `E10` dalam `20261008055604_AddWorkspacePpriAdmissionDocuments` atas instruksi pengguna (migration sekali). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-185.md) |
| **Outcome** | `MstInpatientClearanceItem` membedakan butir penutupan dan butir serah terima beserta induk dan sumber saran; `MstInpatientSetting` memuat sebelas isian cetak; daftar periksa penutupan episode hanya membaca butir jenis penutupan |
| **Requirement/decision** | `FR-RWA-030`, `050`, `126`; `RWI-DEC-241` butir 3, `243`, `247`; `INV-RWA-12`; `RWI-AC-361`; QBE `TOUCHED LEGACY` (`SortOrder`) |
| **Kontrak** | Backend 13.9 (`MstClearanceChecklistType`, `MstHandoverSuggestionSource`), 13.11, 13.12 (`E9`); data 20.15, 20.16, 20.18; API 12.1 (perilaku penutupan) |
| **Reuse** | Model dan configuration `MstInpatientClearanceItem`, `MstInpatientSetting`; `InpDischargeService.Closure.cs:88-101`, `:162-177`; `InpSettingService` |
| **Cakupan** | Dua enum `MasterData/Enums/`; tiga kolom `MstInpatientClearanceItem` (bawaan `1`, kosong, `0`), FK diri sendiri `Restrict`, `CK_MstInpatientClearanceItem_Type`, index (`ChecklistType`, `IsActive`) dan (`ParentItemId`); sebelas kolom `MstInpatientSetting` (`InfantWristbandMaxAgeYears` bawaan `5`); satu migration `E9` dengan `Down()`; saringan `ChecklistType = EpisodeClosure` pada daftar dan penandaan butir penutupan; `InpatientSettingValues` membawa nilai baru. Tanpa perubahan DTO endpoint master (milik `BE-RWI-186`) |
| **Dependency** | — |
| **Acceptance criteria** | 1. Satu migration `E9` memuat seluruh kolom, bawaan, nullability, FK, `CHECK`, dan index sesuai kamus data 20.15, 20.16, 20.18, dan punya `Down()`. 2. Butir lama terbaca `EpisodeClosure`; daftar periksa penutupan episode uji sama persis sebelum dan sesudah migration (butir, wajib, penahan). 3. Butir `NewPatientHandover` yang disisipkan di basis data uji tidak pernah muncul di `GET …/discharges/{episodeId}/clearance`, dan `POST …/clearance/{itemId}/mark` untuknya mengembalikan `404` "Butir administrasi tidak ditemukan". 4. Baris pengaturan lama mendapat `InfantWristbandMaxAgeYears = 5` dan kolom baru lain kosong. 5. Tidak ada kolom `SortOrder` baru |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet restore`, `dotnet build`; pemeriksaan skrip migration terhadap DDL 20.18; verifikasi API daftar dan penandaan penutupan pada data samaran; regresi alur penutupan episode |
| **Risiko/pemilik** | **Tinggi**: tanpa saringan, butir serah terima wajib menahan penutupan setiap episode. Langkah mundur: nonaktifkan butir `STPB-*` sebelum mundur kode (backend 13.12). Pemilik: Muhammad Hamzah; `MasterData` milik seluruh tim (`RWI-DEC-193`) |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-185.md` mencatat status penerapan migration apa adanya; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-186` — Endpoint master butir serah terima dan pengaturan cetak, data awal non-produksi

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Pemanggil seeder master Rawat Inap dipulihkan; hasil seeder runtime belum dilaporkan. Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-186.md) |
| **Outcome** | Admin mengelola jenis, induk sub-butir, dan sumber saran butir administrasi serta sebelas isian pengaturan cetak; lingkungan pengembangan dan UAT terisi 15 + 3 butir `STPB-*` dan nilai cetak V1 |
| **Requirement/decision** | `FR-RWA-030`, `126`; `RWI-DEC-048`, `241`, `243`, `247`; `VAL-RWA-50` s.d. `55`; `RWI-AC-360`, `368` |
| **Kontrak** | API 12.4; validation 15.7; backend 13.8.4 (`InpatientClearanceItemService`, `InpatientSettingService`, `InpatientMasterDataSeeder`), 13.13 |
| **Reuse** | `InpatientClearanceItemService`/`Controller`, `InpatientSettingService`/`Controller`, `InpatientMasterDataSeeder` |
| **Cakupan** | DTO request/response; query `checklistType` pada daftar, opsi, dan ringkasan; validasi `MST-ICI-001` s.d. `004` (jenis tidak dapat diubah bila sudah dipakai `InpClearanceMark` atau `InpAdmissionHandoverItem`) dan `MST-IST-001`, `002`; seeder: 18 butir `STPB-01` s.d. `STPB-15`, `STPB-02A` s.d. `02C` dengan induk, sumber saran, dan `SortOrder` sesuai backend 13.13, serta nilai pengaturan V1 bila kosong — **menolak lingkungan produksi** |
| **Dependency** | `BE-RWI-185`, `BE-RWI-192` |
| **Acceptance criteria** | 1. Daftar `checklistType = NewPatientHandover` hanya memuat butir serah terima, lengkap dengan nama induk. 2. Induk berjenis lain, butir penutupan bersumber saran, ubah jenis butir terpakai, dan sumber saran ganda ditolak dengan `MST-ICI-001` s.d. `004`. 3. `PUT` pengaturan menolak batas umur gelang bayi di luar 0–16 dan panjang isian berlebih; `GET` memuat sebelas isian. 4. Seeder di lingkungan `Development` menyisipkan 18 butir dan nilai V1 hanya bila kosong; di `Production` menolak. 5. Permintaan lama tanpa isian baru tetap berhasil dan membuat butir `EpisodeClosure` |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API pada basis data uji; seeder dijalankan di lingkungan pengembangan bila diizinkan, selain itu review kode |
| **Risiko/pemilik** | Nilai V1 (kode formulir, kota) hanya boleh ada di seeder, bukan di service, controller, DTO, atau komponen cetak (`RWI-AC-368`). Butir produksi diisi admin. Pemilik: Muhammad Hamzah; `MasterData` |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-186.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-187` — Service baca pasien di `PatientManagement`

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Gerbang `RWI-OQ-126` ditutup `RWI-DEC-266` (8 Oktober 2026). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-187.md) |
| **Outcome** | Workspace PPRI membaca identitas, isi QR No. RM, relasi, dan kontak darurat lewat service `PatientManagement`, tanpa query ke tabel master dari Rawat Inap |
| **Requirement/decision** | `FR-RWA-003`, `021`, `052`, `082`; `RWI-DEC-252`, `257`, `259`, `264`, `266`; `INV-RWA-14` |
| **Kontrak** | Backend 13.6, 13.8.4; integration `INT-RWA-01`, `02` |
| **Reuse** | `MstPatient`, `MstPatientRelationship`, `MstPatientEmergencyContact`; logika `PatientController.BuildPatientQrPayload` (`PatientController.cs:1386-1400`) |
| **Cakupan** | Folder baru `Areas/HealthServices/PatientManagement/MasterData/Services/`; `PatientProfileQueryService` (`GetIdentityAsync`, `GetPartyCandidatesAsync`); `PatientQrPayloadBuilder` hasil ekstraksi; `PatientController` memanggil builder; registrasi `AddScoped` |
| **Dependency** | — |
| **Acceptance criteria** | 1. `GetIdentityAsync` mengembalikan No. RM terformat, nama, nama panggilan, tanggal lahir, jenis kelamin, agama, status nikah, jenis dan nomor identitas, telepon, email, alamat beserta nama wilayah, penanda bayi baru lahir, nama ibu, dan isi QR; pasien tidak ada atau terhapus → kosong. 2. Isi QR builder sama persis dengan keluaran method lama untuk sampel No. RM. 3. `GetPartyCandidatesAsync` hanya memuat relasi dan kontak darurat aktif pasien itu: relasi dengan jenis terstruktur, kontak dengan teks hubungan apa adanya. 4. Tidak ada tabel maupun endpoint `PatientManagement` yang berubah; respons endpoint pasien yang memuat QR sama sebelum dan sesudah |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; perbandingan keluaran QR; regresi endpoint pasien |
| **Risiko/pemilik** | Menyentuh modul lain; persetujuan hanya untuk service baca dan ekstraksi tanpa perubahan perilaku. Pemilik: pemilik `PatientManagement` lewat Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-187.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-188` — Service baca profil rumah sakit (HR Master Data)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Gerbang `RWI-OQ-127` ditutup `RWI-DEC-266` (8 Oktober 2026). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-188.md) |
| **Outcome** | Kop surat seluruh cetakan Workspace PPRI dibaca dari profil situs rumah sakit utama |
| **Requirement/decision** | `FR-RWA-126`; `RWI-DEC-247`, `264`, `266`; `NFR-RWA-11` |
| **Kontrak** | Backend 13.6, 13.8.4; `INT-RWA-12` |
| **Reuse** | `MstHospitalSite` beserta wilayah |
| **Cakupan** | Folder baru `Areas/Corporate/HumanResource/MasterData/Organization/Services/`; `HospitalSiteProfileQueryService.GetMainSiteProfileAsync`; registrasi `AddScoped`. `HospitalSiteController` tidak disentuh |
| **Dependency** | — |
| **Acceptance criteria** | 1. Mengembalikan nama, kode, baris alamat beserta nama wilayah, telepon, email, dan zona waktu situs aktif bertanda `IsMainSite`. 2. Tidak ada situs utama aktif, atau lebih dari satu → `IsAvailable = false` beserta alasannya, tanpa menebak. 3. Tidak ada tabel maupun endpoint HR yang berubah |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; pembacaan pada data situs uji |
| **Risiko/pemilik** | Pemilik: pemilik HR Master Data lewat Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-188.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-189` — Bacaan Clinical: konteks penjamin, surat pengantar, alergi aktif

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-189.md) |
| **Outcome** | Konteks penjamin membawa nomor kartu dan nomor peserta; surat pengantar rawat inap terbaru dan alergi aktif terbaca lewat service Clinical |
| **Requirement/decision** | `FR-RWA-003`, `051`, `070`; `RWI-DEC-253`, `254`, `262`, `264`; `RWI-FACT-066`, `067` |
| **Kontrak** | Backend 13.6, 13.8.4; integration `INT-RWA-03` s.d. `05` |
| **Reuse** | `EncounterInsuranceService.GetContextAsync`, `DoctorCertificateService`, `PatientAllergyController.GetActiveAlerts` |
| **Cakupan** | `EncounterInsuranceContext` + `CardNumber`, `MemberNumber` dari snapshot sumber pembayaran; `DoctorCertificateService.GetLatestIssuedInpatientReferralAsync(encounterId)` (nomor, dokter, tanggal terbit, diagnosis, alasan); `PatientAllergyQueryService.GetActiveAlertsAsync`, dan `PatientAllergyController` memanggilnya |
| **Dependency** | — |
| **Acceptance criteria** | 1. Konteks penjamin asuransi dan perusahaan memuat nomor kartu dan peserta; tunai kosong; pemakai konteks yang ada (perkiraan harga) tidak berubah keluarannya. 2. Surat yang dipilih adalah `InpatientReferral` berstatus `Issued` terbaru pada kunjungan; surat `Cancelled` diabaikan; tanpa surat → kosong; respons tidak memuat data pasien maupun gambar tanda tangan dokter. 3. Respons `GET patient-allergies/active-alerts` sama sebelum dan sesudah untuk pasien uji. 4. Tidak ada endpoint baru |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API dan pembacaan pada data samaran |
| **Risiko/pemilik** | `EncounterInsuranceContext` dipakai perhitungan harga; isian hanya ditambah. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-189.md`; roadmap dan traceability diperbarui |

### ⛔ `BE-RWI-190` — Service baca dokter perujuk luar (Registration)

| Field | Isi |
|---|---|
| **Status** | ⛔ **Terblokir** — menunggu `RWI-OQ-128`, persetujuan pemilik `RegistrationManagement` (belum tercatat namanya; ditanyakan lewat Muhammad Hamzah). Selama itu IPD mencetak garis kosong (`RWI-AC-386`) |
| **Outcome** | IPD pasien tanpa surat pengantar mencetak dokter perujuk luar beserta institusinya |
| **Requirement/decision** | `FR-RWA-070`; `RWI-DEC-254` butir 3; `RWI-AC-386`; `RWI-OQ-128` |
| **Kontrak** | Backend 13.8.4; `INT-RWA-06` |
| **Reuse** | `RegPatientEncounter.ReferralDoctorId`, `MstReferralDoctor`, `MstReferralInstitution` |
| **Cakupan** | `EncounterReferralQueryService.GetExternalReferralAsync(encounterId)` di `RegistrationManagement/Services/`; adapter "belum tersedia" di `InpAdmissionSourceReader` diganti panggilan nyata |
| **Dependency** | `BE-RWI-193`; `{RWI-OQ-128}` |
| **Acceptance criteria** | 1. Kunjungan dengan dokter perujuk luar → nama dokter dan institusinya; tanpa → kosong. 2. IPD pasien tanpa surat pengantar memakai hasil ini; pasien dengan surat tetap memakai surat. 3. Tidak ada tabel maupun endpoint Registration yang berubah |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi `GET …/base-data` |
| **Risiko/pemilik** | Pemilik: pemilik `RegistrationManagement` |
| **DoD** | Kriteria terbukti; laporan `../task/report/backend/BE-RWI-190.md`; roadmap dan traceability diperbarui |

### ⛔ `BE-RWI-191` — Method baca tarif kamar harian (Billing)

| Field | Isi |
|---|---|
| **Status** | ⛔ **Terblokir** — menunggu `RWI-OQ-129`, persetujuan Yasmina (Billing). Selama itu "Rencana @ Kamar (Rp)" tertulis "lihat kasir" (`RWI-AC-387`) |
| **Outcome** | Pemegang `ViewAmount` melihat tarif kamar per hari menurut unit, kelas, dan penjamin pada IPD, dengan aturan yang sama dengan tagihan |
| **Requirement/decision** | `FR-RWA-070`; `RWI-DEC-258`; `RWI-AC-387`; `RWI-OQ-129` |
| **Kontrak** | Backend 13.8.4; `INT-RWA-08`; API 12.2 `/base-data/amounts` |
| **Reuse** | `BillingCalculationService.ResolveRoomTariff` (`:797`), `InsuranceCoverageService.ResolveTariffAsync` |
| **Cakupan** | `BillingCalculationService.GetDailyRoomRateAsync(serviceUnitId, patientClassId, momentUtc)` memakai aturan yang ada; adapter source reader diganti panggilan nyata |
| **Dependency** | `BE-RWI-193`; `{RWI-OQ-129}` |
| **Acceptance criteria** | 1. Tarif yang dipilih sama dengan yang dipakai perhitungan kamar tagihan untuk unit, kelas, dan waktu yang sama. 2. `GET …/base-data/amounts` mengembalikan harga menurut penjamin; tanpa tarif → `TariffMissing`. 3. Perilaku `CalculateRoomChargeAsync` tidak berubah |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; perbandingan dengan pratinjau tagihan pada data uji |
| **Risiko/pemilik** | Menyentuh Billing. Pemilik: Yasmina |
| **DoD** | Kriteria terbukti; laporan `../task/report/backend/BE-RWI-191.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-192` — Tabel dokumen admisi dan log cetak (`E10`)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Migration `E10` tergabung bersama `E9` dalam `20261008055604_AddWorkspacePpriAdmissionDocuments`; isi ditinjau sesuai DDL 20.18. Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-192.md) |
| **Outcome** | Sepuluh tabel `InpAdmission*` gelombang MVP tersedia dengan kunci, constraint, dan index yang menegakkan invariant di basis data |
| **Requirement/decision** | `FR-RWA-120` s.d. `128`; `RWI-DEC-228`, `229`, `263`; `INV-RWA-01`, `03`, `04`, `09`, `10` |
| **Kontrak** | Backend 13.3, 13.9, 13.11, 13.12 (`E10`); data 20.2 s.d. 20.10, 20.13, 20.18 |
| **Reuse** | Pola `InpAdmissionReferral` (`Guid RowVersion`, `CHECK`, unique bersaring), `CliNursingIntervention` (`IdempotencyKey`), `CliTransferHandover` (`jsonb`) |
| **Cakupan** | Model `InpAdmissionDocument`, `…Signature`, `…Party`, `InpAdmissionHandoverItem`, `InpAdmissionPrivacyRequest`, `…PrivacyEntry`, `InpAdmissionBeliefItem`, `InpAdmissionCostDifferenceStatement`, `InpAdmissionDepositStatement`, `InpAdmissionPrintLog`; 12 enum di `InPatientManagement/Enums/` (seluruh enum `Inp*` pada 13.9 kecuali `InpCostEstimateLineType` dan `InpCostEstimatePriceSource`; dua enum `MasterData` milik `BE-RWI-185`); configuration; `DbSet` jamak; satu migration `E10` dengan `Down()` |
| **Dependency** | `BE-RWI-185` |
| **Acceptance criteria** | 1. Kolom, tipe, nullability, bawaan, FK `Restrict`, dan `CHECK` sama dengan kamus data. 2. Index unik bersaring `UX_InpAdmissionDocument_Episode_Type_Active`, `UX_…_PreviousVersionId`, `UX_InpAdmissionDocumentSignature_Document_Slot`, `UX_…_Document_Signer`, dan unique `IdempotencyKey` ada. 3. `Down()` menghapus bersih. 4. Tidak ada `Trx*` maupun `SortOrder` baru |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; pemeriksaan skrip migration terhadap DDL 20.18 |
| **Risiko/pemilik** | Constraint yang meleset membuat invariant hanya dijaga kode. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-192.md` mencatat status penerapan migration apa adanya; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-193` — Fondasi baca dan cetak

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-193.md) |
| **Outcome** | Satu controller dengan sepuluh aksi hak akses, penjaga tulis episode, satu pintu baca data modul lain, kop surat, dan log cetak beralasan siap dipakai seluruh layar |
| **Requirement/decision** | `FR-RWA-053`, `126`, `127`, `128`; `RWI-DEC-240` butir 7, `247`, `257`, `264`; `INV-RWA-08`, `09`, `14`; G-33 |
| **Kontrak** | API 12.2 (`GET /letterhead`, `GET`/`POST /print-logs`), 12.5; validation `VAL-RWA-01`, `02`, `09`, `40`, `41`, `46`; permission 10.1, 10.2; integration `INT-RWA-01` s.d. `12`; data 20.13.1 |
| **Reuse** | `InpPatientLocationQuery`, `AccessPermissionService`, pola `Idempotency-Key` `InpatientSurgeryBookingController` |
| **Cakupan** | `InpatientAdmissionDocumentController` (`ControllerName = "InpatientAdmissionDocument"`, `[Tags("Health Services / Inpatient Management / Inpatient Admission Workspace")]`) dengan sepuluh `[AccessAction]` dan pengelompokan `AccessType` (backend 13.9); `InpAdmissionWriteGuard`; `InpAdmissionSourceReader` tersambung ke `BE-RWI-187`, `188`, `189`, Billing deposit, dan kasus OK, dengan adapter "belum tersedia" untuk dokter perujuk luar dan tarif kamar; `InpPatientLocationQuery.HasActivePlacementAsync`; `InpAdmissionWorkspaceQueryService` bagian kop dan riwayat cetak; bagian catat dari `InpAdmissionPrintService` |
| **Dependency** | `BE-RWI-187`, `BE-RWI-188`, `BE-RWI-189`, `BE-RWI-192` |
| **Acceptance criteria** | 1. Sesudah aplikasi dinyalakan, registry hak akses memuat `InpatientAdmissionDocument` dengan sepuluh aksi. 2. `GET /letterhead` mengembalikan profil situs utama untuk episode status apa pun; pencarian source berkas Workspace PPRI atas identitas rumah sakit client, kota bawaan V1, dan pola kode formulir = nol hasil. 3. `POST /print-logs`: cetak pertama tanpa alasan; cetak berikutnya untuk kunci yang sama `422 INP-ADM-PRT-001` tanpa alasan; alasan `Other` wajib keterangan; episode `Closed`/`Cancelled` selalu wajib alasan; `Idempotency-Key` sama → baris sama. 4. `GET /print-logs` menghitung "cetakan ke-*n*" dari urutan. 5. Sumber yang gagal atau belum tersedia menghasilkan keadaan `Failed`/`NotYetAvailable`, tidak pernah `500` |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API; pencarian source |
| **Risiko/pemilik** | Seluruh bacaan modul lain lewat class ini (`INV-RWA-14`). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-193.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-194` — Siklus dokumen admisi, dengan Permintaan Privasi sebagai jenis pertama

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-194.md) |
| **Outcome** | Setiap jenis dokumen admisi berperilaku sama: konsep, kunci dengan salinan beku, tanda tangan kertas dan atestasi per slot, versi koreksi, buang konsep, batal, cetak per status. Permintaan Privasi berjalan penuh sebagai jenis pertama |
| **Requirement/decision** | `FR-RWA-060` s.d. `062`, `120` s.d. `125`, `128`; `RWI-DEC-237` s.d. `240`, `263`; `INV-RWA-01` s.d. `06`, `10`; `RWI-AC-351`, `352`, `355` s.d. `358`, `384` |
| **Kontrak** | API 12.2 (`/prefill/{documentType}`, `/documents`, `/documents/{id}`, `lock`, `unlock`, `discard`, `revisions`, `cancel`, lima `signatures/*`, `/documents/{id}/print`, plumbing `/amounts` dan `/amount-print`), 12.3; state 10.1; validation 15.1, 15.3, 15.4 (`22`, `27`), 15.5, 15.6 (`42`, `43`); data 20.2 s.d. 20.7; flowchart `05` |
| **Reuse** | Fondasi `BE-RWI-193`; pola konkurensi `RowVersion` |
| **Cakupan** | `InpAdmissionDocumentService` (perintah siklus dan pendaftaran handler per jenis), `InpAdmissionSignatureService`, `InpAdmissionSnapshotBuilder`, `InpAdmissionPrefillService` (bagian umum dan Privasi), bagian dokumen `InpAdmissionPrintService` (penanda `Draft`, lembar tanda tangan bernomor versi, final, `Superseded`, `Cancelled`, "ADMISI DIBATALKAN"); handler Privasi |
| **Dependency** | `BE-RWI-193` |
| **Acceptance criteria** | 1. Privasi berjalan `Draft` → kunci → catatan kertas → atestasi Kepala Ruangan → `Completed` dengan dua akun; cetakan per status memakai penanda yang benar dan cetakan `AwaitingSignature` dibentuk dari salinan beku. 2. Alamat pasien diubah sesudah dikunci → cetakan tetap lama, `SourceChangedSinceLock = true`. 3. Buka kunci hanya tanpa tanda tangan (`409 INP-ADM-DOC-006`). 4. Versi koreksi hanya dari `Completed`, alasan 10–500; versi lama `Superseded`, versi baru `Draft` `VersionNo` 2 dalam satu transaksi tanpa pelanggaran index. 5. Batal oleh pemegang `Cancel` beralasan ≥ 10; buang konsep hanya pembuatnya (`422 INP-ADM-DOC-008`). 6. Dokumen aktif kedua `409 INP-ADM-DOC-003`; dua simpan bersamaan → satu baris. 7. `RowVersion` basi `409 INP-ADM-DOC-004`. 8. Slot bukan wajib `422 INP-ADM-DOC-031`; slot terisi `409 INP-ADM-DOC-032`; tanda tangan pada `Draft` `409 INP-ADM-DOC-030`; tanpa `SignAsHeadNurse` `403`. 9. Catatan kertas ditolak bila waktunya sebelum kunci atau di masa depan. 10. Tulis pada episode `Closed`/`Cancelled` `409 INP-ADM-DOC-001`, pada `Draft` `409 INP-ADM-DOC-002`. 11. Tidak ada endpoint `DELETE`. 12. Kerabat dan permintaan khusus tersimpan per baris, maksimal 3. 13. `Idempotency-Key` sama → hasil sama. 14. Payload logger tidak memuat kolom sensitif |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API proses bisnis dengan dua akun pada data samaran; pemeriksaan logger |
| **Risiko/pemilik** | Task terbesar; seluruh dokumen bergantung padanya. Aturan satu akun satu slot dibuktikan penuh pada `BE-RWI-199`. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-194.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-195` — Ringkasan ruang kerja, kelengkapan, dan peringatan Detail Episode

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-195.md) |
| **Outcome** | Header, sembilan menu (delapan tampil pada MVP), kelengkapan *x* dari *y*, rupiah bagi pemegang `ViewAmount`, dan peringatan Detail Episode tanpa rupiah |
| **Requirement/decision** | `FR-RWA-001` s.d. `008`; `RWI-DEC-226`, `234`, `245`, `250`, `256`, `257`, `258`; `RWI-AC-344` s.d. `346`, `378`, `379`; G-30 |
| **Kontrak** | API 12.1 (`GET episodes/{id}` peringatan), 12.2 (`/summary`, `/summary/amounts`), 12.3; state 10.2, 10.6; integration `INT-RWA-07`, `11`, `13` |
| **Reuse** | `InpEpisodeService`, `BillingDepositService`, `OperatingRoomCaseService` (lewat source reader) |
| **Cakupan** | `InpAdmissionCompletenessEvaluator` (aturan wajib tanpa Estimasi sampai `BE-RWI-203`); `InpAdmissionWorkspaceQueryService` bagian ringkasan dan ringkasan rupiah; dua endpoint; `InpEpisodeService` menambah peringatan untuk `Admitted`/`DischargePending`, diteruskan `InpatientEpisodeController` |
| **Dependency** | `BE-RWI-193` |
| **Acceptance criteria** | 1. Pasien asuransi dengan kekurangan deposit → wajib 6; tunai dengan deposit cukup → wajib 4; General Consent, Privasi, Label tidak dihitung; Estimasi tidak dihitung sebelum `EPIC-RWA-09`. 2. Menu urutan V1 tanpa Assessment Edukasi dan MP Benefit, lencana sesuai state 10.2. 3. `Availability` `NotYetAdmitted` untuk episode `Draft`, `ReadOnly` untuk `Closed`/`Cancelled`. 4. `/summary` tanpa rupiah; `/summary/amounts` `403` tanpa `ViewAmount`; Billing gagal → `Unavailable` dan butir deposit "?". 5. Detail episode memuat "Dokumen admisi belum lengkap: *n* (…)" tanpa rupiah; sumber gagal → "Kelengkapan dokumen admisi tidak dapat dihitung" dan detail tetap `200` |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API dengan dua pasien samaran; uji kegagalan sumber |
| **Risiko/pemilik** | Detail episode adalah layar paling sering dibuka; panggilan sumber harus ringan dan gagal aman (`NFR-RWA-16`). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-195.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-196` — Data gelang dan label pasien

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-196.md) |
| **Outcome** | Gelang dewasa atau bayi dengan sapaan yang benar dan label pasien tanpa nomor karangan, dengan QR No. RM saja |
| **Requirement/decision** | `FR-RWA-050` s.d. `053`; `RWI-DEC-243`, `253`, `259`; `RWI-AC-363`, `364`, `374`, `380` |
| **Kontrak** | API 12.2 (`/identity-labels`), 12.3 `IdentityLabelResponse`; validation `VAL-RWA-45`; flowchart `07` bagian 1 |
| **Reuse** | `PatientQrPayloadBuilder` (`BE-RWI-187`), konteks penjamin (`BE-RWI-189`), pengaturan |
| **Cakupan** | `InpWristbandRules` (jenis gelang, sapaan, teks umur); `InpAdmissionWorkspaceQueryService` bagian gelang dan label; endpoint; validasi jenis gelang pada `POST /print-logs` |
| **Dependency** | `BE-RWI-193` |
| **Acceptance criteria** | 1. Pria 45 th → Gelang Dewasa "*NAMA*, Tn."; bayi `IsNewborn` → Gelang Bayi "BY. NY. *nama ibu*" dan dua label kecil; anak 4 th → Gelang Bayi "An."; wanita dewasa status nikah `Unknown` → tanpa sapaan. 2. QR sama persis dengan isi QR pasien; pasien tanpa berkas `QrCodePath` tetap terlayani. 3. Label: kode RS dari pengaturan, kosong → `SiteCode`; No. Kartu hanya `CardNumberSnapshot`, kosong tetap kosong. 4. Catat cetak gelang yang jenisnya berbeda dari hitungan → `422 INP-ADM-PRT-005` |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API pada empat pasien samaran |
| **Risiko/pemilik** | Aturan sapaan dan batas umur menunggu verifikasi tim keselamatan pasien sebelum produksi (G-38). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-196.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-197` — Data Dasar Rawat Inap (IPD)

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Dokter perujuk luar dan tarif kamar memakai cadangan aman sampai `BE-RWI-190`/`191` ⛔. Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-197.md) |
| **Outcome** | IPD terangkai dari sumber resmi; isian tanpa sumber dicetak garis kosong; dokter perujuk luar dan tarif kamar memakai cadangan aman sampai `BE-RWI-190`, `191` |
| **Requirement/decision** | `FR-RWA-070` s.d. `072`; `RWI-DEC-244`, `253`, `254`, `258`; `RWI-AC-365`, `375`, `386`, `387` |
| **Kontrak** | API 12.2 (`/base-data`, `/base-data/amounts`), 12.3; validation `VAL-RWA-44`; flowchart `07` bagian 2 |
| **Reuse** | Surat pengantar (`BE-RWI-189`), `InpStatusHistory` (petugas yang mengonfirmasi admisi), `InpBedPlacement`, penugasan DPJP dan perawat |
| **Cakupan** | `InpAdmissionWorkspaceQueryService` bagian IPD; dua endpoint; aturan sumber per isian PRD Lampiran A.6 (API 12.3 `InpatientBaseDataResponse`); `BlankFields`; penanggung jawab dari relasi atau kontak darurat bertanda penanggung jawab; isian Nilai Kepercayaan dan Privasi dari dokumen `Completed`; `CanPrint`; validasi cetak IPD pada `POST /print-logs` |
| **Dependency** | `BE-RWI-193` |
| **Acceptance criteria** | 1. Pekerjaan, kewarganegaraan, RT/RW, kelurahan, alamat domisili, alamat kantor, no. mutasi, persetujuan direktur, perhatian khusus, dan kasir masuk `BlankFields`. 2. Surat `Issued` terbaru → diagnosis, rencana, dokter penerbit; surat `Cancelled` diabaikan; tanpa surat → garis kosong (sampai `BE-RWI-190`). 3. `/base-data/amounts` → `NotYetAvailable` sampai `BE-RWI-191`; tanpa `ViewAmount` → `403`. 4. Identitas atau episode gagal → `CanPrint = false`; catat cetak IPD `422 INP-ADM-PRT-004`. 5. Isian penerima informasi kosong selama General Consent *fail-closed* |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API pada kunjungan dengan surat `Issued`, hanya surat `Cancelled`, dan tanpa surat; isian dokumen `Completed` diuji dengan baris uji atau sesudah `BE-RWI-194`/`200` tersedia |
| **Risiko/pemilik** | Tidak boleh mengisi tebakan, misalnya keluhan utama sebagai diagnosis masuk. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-197.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-198` — Data cetak General Consent dan Surat Persetujuan

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-198.md) |
| **Outcome** | Formulir General Consent V1 dan Surat Persetujuan 12 butir terisi dari server tanpa satu pun jalur tulis |
| **Requirement/decision** | `FR-RWA-020` s.d. `022` (bagian cetak), `140`, `142`; `RWI-DEC-230`, `233`, `246`, `251`, `252`; `RWI-AC-347`, `348`, `372`, `373` |
| **Kontrak** | API 12.2 (`/general-consent/print-data`), 12.3 `GeneralConsentPrintDataResponse` |
| **Reuse** | Penanda `MstRoom`/`MstBed` dan `InpEpisode.RequiresIsolation`; calon penanda tangan (`BE-RWI-187`); pengaturan kode formulir dan kota |
| **Cakupan** | `InpAdmissionWorkspaceQueryService` bagian data cetak General Consent; endpoint baca; aturan tipe kamar; pemetaan hubungan V1 ke relasi terstruktur; isian surat 12 butir setara yang hari ini dibaca `use-inpatient-consent-print.js:58-82` |
| **Dependency** | `BE-RWI-193` |
| **Acceptance criteria** | 1. Bed `IsIntensiveCareBed` atau episode `RequiresIsolation` → "Khusus"; kamar bernama "Melati Khusus" tanpa penanda dan kelas `IsForIntensiveCare` di bed biasa → "Umum". 2. Calon penanda tangan: relasi `Spouse` untuk Istri/Suami, `Child`, `Mother`/`Father`; lebih dari satu dikembalikan semua; kontak darurat hanya sebagai daftar dengan teks hubungannya. 3. Tidak ada endpoint tulis General Consent; `patient-consents` tidak dipanggil. 4. Kode formulir dan kota dari pengaturan |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API pada pasien samaran dengan relasi dan kontak darurat |
| **Risiko/pemilik** | *Fail-closed* privasi (`RWI-DEC-230`). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-198.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-199` — Serah Terima Pasien Baru

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-199.md) |
| **Outcome** | Serah terima memakai butir master yang dibekukan, menampilkan saran sistem, menolak kunci yang belum lengkap, dan selesai dengan tiga tanda tangan petugas berbeda sesudah pasien menempati bed |
| **Requirement/decision** | `FR-RWA-030` s.d. `034`; `RWI-DEC-239`, `241`, `255`, `262`; `INV-RWA-04`, `11`, `12`; `RWI-AC-353`, `354`, `359`, `360`, `376`, `383` |
| **Kontrak** | Validation `VAL-RWA-20`, `21`, `30` s.d. `34`; data 20.5; backend 13.9 (slot wajib); flowchart `06` |
| **Reuse** | Siklus `BE-RWI-194`; butir `STPB-*` (`BE-RWI-186`); log cetak (`BE-RWI-193`); surat pengantar (`BE-RWI-189`); `HasActivePlacementAsync` |
| **Cakupan** | Handler serah terima: pembekuan butir aktif jenis serah terima menurut `SortOrder` dengan nomor induk dan sub-butir; saran per `HandoverSuggestionSource` (`ReferralLetter`, `DepositStatementCompleted`, `BaseDataPrinted`, `WristbandPrinted`; `CostEstimateCompleted` diam sampai `BE-RWI-203`; `LabelAndGeneralConsent` tanpa saran selama *fail-closed*); validasi kunci; tiga slot; syarat bed slot Perawat |
| **Dependency** | `BE-RWI-194`, `BE-RWI-186` |
| **Acceptance criteria** | 1. Butir 5 belum dipilih dan butir 11 Belum tanpa keterangan → satu penolakan `422` berisi dua pesan bernomor. 2. Nama butir master diubah sesudah dokumen dibuat → dokumen tetap nama lama. 3. Saran butir 1 hanya bila surat `Issued` ada (menyebut dokter dan tanggal); butir 9 sesudah IPD dicetak; butir 13 sesudah gelang dicetak; butir 12 tanpa saran. 4. Akun yang sama mengisi dua slot → `422 INP-ADM-DOC-033`; dua permintaan bersamaan ditolak unique index. 5. Slot Perawat saat bed masih dipesan → `422 INP-ADM-DOC-034`, lalu berhasil sesudah penempatan; slot CRO tidak terpengaruh. 6. Sebelum kunci, tanda tangan CRO `409 INP-ADM-DOC-030` |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API proses bisnis dengan tiga akun dan penempatan bed pada data samaran |
| **Risiko/pemilik** | Saran tidak boleh memilih otomatis; petugas tetap memilih. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-199.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-200` — Nilai Kepercayaan dan ringkasan hak pasien

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-200.md) |
| **Outcome** | Nilai Kepercayaan per episode dengan butir dari dokumen lengkap terakhir pasien sebagai konsep; modul lain membaca ringkasan nilai kepercayaan dan privasi |
| **Requirement/decision** | `FR-RWA-110` s.d. `113`; `RWI-DEC-242`; `RWI-AC-362`; G-41 |
| **Kontrak** | API 12.2 (`/patient-rights`); validation `VAL-RWA-16`, `23`; data 20.4, 20.8 |
| **Reuse** | Siklus `BE-RWI-194` |
| **Cakupan** | Handler Nilai Kepercayaan; prefill dari dokumen `Completed` terakhir pasien pada episode lain; `InpAdmissionWorkspaceQueryService` bagian ringkasan hak pasien; `GET /patient-rights` dijaga `InpatientEpisode : Read` |
| **Dependency** | `BE-RWI-194` |
| **Acceptance criteria** | 1. Episode baru menampilkan butir episode lalu sebagai konsep, belum lengkap sampai ditandatangani ulang. 2. Butir ke-6 `400` "Maksimal 5 butir"; kunci tanpa butir `422`. 3. Nama, jenis kelamin, hubungan, alamat penanda tangan wajib saat kunci. 4. `/patient-rights` hanya memuat dokumen `Completed`, tidak versi `Superseded` |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API dengan dua episode pasien samaran |
| **Risiko/pemilik** | Isi keyakinan sensitif; tinjauan privasi gerbang produksi G-35. Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-200.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-201` — Selisih Biaya

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-201.md) |
| **Outcome** | Surat pernyataan selisih biaya tersimpan untuk penjamin asuransi atau perusahaan, dengan subjek berkode dan data pasien dari master |
| **Requirement/decision** | `FR-RWA-100` s.d. `103`; `RWI-DEC-234`, `256`; `RWI-AC-377`; G-43, G-44 |
| **Kontrak** | Validation `VAL-RWA-10`, `14`, `24`; data 20.4, 20.9 |
| **Reuse** | Siklus `BE-RWI-194`; konteks penjamin |
| **Cakupan** | Handler Selisih Biaya; prefill "diri saya sendiri" dari identitas pasien; normalisasi telepon |
| **Dependency** | `BE-RWI-194` |
| **Acceptance criteria** | 1. Pasien tunai: buat dan kunci `422 INP-ADM-DOC-010`. 2. Penjamin bertanda tidak mengizinkan selisih dibebankan tetap dapat membuat surat. 3. Subjek tersimpan sebagai kode; "saudara kandung lainnya" wajib keterangan. 4. Nama, alamat, tipe ID, No. ID deklarer wajib saat kunci; HP "0812-3456-7890" tersimpan "081234567890", 14 digit `400` |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API pada pasien asuransi dan tunai samaran |
| **Risiko/pemilik** | Nomor identitas deklarer sensitif (G-35). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-201.md`; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-202` — Pelunasan Deposit dan peringatan jatuh tempo

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI 8 Oktober 2026.** Kode selesai; `dotnet build` pengguna 0 error; migration `20261008055604_AddWorkspacePpriAdmissionDocuments` diterapkan (`Done.`). Butir DoD verifikasi API/runtime **dikecualikan atas keputusan pengguna 8 Oktober 2026** (pengguna menguji mandiri). Bukti: [laporan](../task/report/backend/BE-RWI-202.md) |
| **Outcome** | Surat pelunasan memakai angka Billing, jatuh tempo bawaan dan batasnya menurut keputusan, angka beku saat dikunci, dan peringatan bila jatuh tempo terlewati |
| **Requirement/decision** | `FR-RWA-080` s.d. `085`; `RWI-DEC-231`, `248`, `252`, `258`, `260`, `261`, `263`; `RWI-AC-366`, `367`, `381`, `382`, `385`; G-45 |
| **Kontrak** | Validation `VAL-RWA-11`, `12`, `14`, `19`, `25`; data 20.10; API `/documents/{id}/amounts`, `/amount-print`; flowchart `08` |
| **Reuse** | Siklus `BE-RWI-194`; evaluator `BE-RWI-195`; `BillingDepositService` |
| **Cakupan** | Handler Pelunasan Deposit; `InpDepositDueDateCalculator`; pembekuan angka saat kunci; rupiah dokumen dan cetak berupiah; peringatan terlewati di evaluator (berupiah di `/summary/amounts`, tanpa rupiah di detail episode) |
| **Dependency** | `BE-RWI-194`, `BE-RWI-195` |
| **Acceptance criteria** | 1. Minimum Rp 5.000.000, diterima Rp 2.000.000 → kekurangan Rp 3.000.000 dengan teks perhitungan. 2. Kekurangan 0 → buat `422 INP-ADM-DOC-011`; Billing gagal → `422 INP-ADM-DOC-012`. 3. Surat Jumat 9 Oktober 2026: interval 3 → bawaan Senin 12 Oktober 11.00 WIB, 13 Oktober `422 INP-ADM-DOC-019`; interval 1 → Sabtu 10 Oktober; Kamis 8 Oktober interval 3 → Jumat 9 Oktober; interval 0 → tanggal surat. 4. Dikunci saat kurang Rp 3.000.000, deposit bertambah Rp 1.000.000, tanda tangan dicatat → tersimpan dan tercetak Rp 3.000.000; ringkasan Rp 2.000.000. 5. Lewat jatuh tempo dengan kekurangan masih ada → peringatan; detail episode tanpa rupiah. 6. Cetak berupiah butuh `ViewAmount` dan `Print`. 7. Pihak bersumber relasi dibaca ulang dari service pemilik |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi proses bisnis dengan Billing pada data samaran dan jam dipalsukan untuk jatuh tempo |
| **Risiko/pemilik** | Angka uang tidak pernah diketik. Pemilik: Muhammad Hamzah; Yasmina untuk angka Billing |
| **DoD** | Kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-202.md`; roadmap dan traceability diperbarui |

### ⛔ `BE-RWI-203` — Estimasi Biaya Rekap

| Field | Isi |
|---|---|
| **Status** | ⛔ **Terblokir** — menunggu `DEC-INP-020` (`RWI-OQ-122`): tarif visit dokter dan sumber catatan cito, lebih dari 4 jam, *standby*, anestesi. Pemilik jawaban: Yasmina, lewat Muhammad Hamzah. `EPIC-RWA-09` `OPEN DECISION`, di luar gelombang |
| **Outcome** | Penjelasan prakiraan biaya tersimpan bertanda tangan dengan harga dari tarif, baris manual beralasan, catatan dari kebijakan Billing, penanda rencana tindakan, dan aturan wajib Estimasi pada kelengkapan |
| **Requirement/decision** | `FR-RWA-090` s.d. `093`; `RWI-DEC-232`, `250`, `258`; `RWI-AC-370`, `371` |
| **Kontrak** | Backend 13.12 (`E11`); data 20.11, 20.12, 20.14; API `/procedure-plan-mark`, jenis `CostEstimate`; validation `VAL-RWA-26` |
| **Reuse** | `InsuranceCoverageService.ResolveProcedureAsync`, `AdministrationFeePolicyService`, `OperatingRoomCaseService`; `BE-RWI-191` untuk baris kamar bila sudah ada |
| **Cakupan** | Migration `E11`; dua enum; handler; `InpAdmissionProcedurePlanService`; aturan wajib Estimasi di evaluator. Rincian baris visit dan catatan biaya bedah ditetapkan ulang sesudah `DEC-INP-020` |
| **Dependency** | `BE-RWI-194`, `BE-RWI-195`; `{DEC-INP-020}` |
| **Acceptance criteria** | Ditetapkan final saat keputusan turun; minimal `RWI-AC-370` (baris tarif tidak ditemukan menahan kunci) dan `RWI-AC-371` (aturan wajib dari kasus OK dan penanda) |
| **Verifikasi** | QBE preflight; `dotnet build`; verifikasi API dengan tarif uji |
| **Risiko/pemilik** | Pemilik: Yasmina untuk aturan biaya; Muhammad Hamzah untuk dokumen |
| **DoD** | Ditetapkan ulang sesudah keputusan; laporan `../task/report/backend/BE-RWI-203.md` |

## Yang sengaja tidak direncanakan

| Tidak ada task | Sebab |
|---|---|
| Penyimpanan General Consent, tanda tangan digital (`EPIC-RWA-02`, `13`) | `DEC-INP-003` belum turun; desain belum memuat model datanya |
| Automated test backend | `rules/backend/TEST_POLICY.md` |
| Penerapan migration ke database, deployment | Wewenang terpisah dari roadmap |
| Isi butir serah terima dan kode formulir produksi | Diisi admin di layar (`RWI-DEC-048`); bukan task kode |
| Pemberian sepuluh aksi `InpatientAdmissionDocument` kepada peran | Konfigurasi Akses Role oleh admin hak akses; data peran CRO, supervisor admisi, dan kepala ruangan menunggu `RWI-OQ-124` (memblokir UAT, bukan pembangunan) |
| Perbaikan cetakan Final lain yang menanam identitas rumah sakit | Issue terpisah G-RWA-02 (`RWI-DEC-247`) |
