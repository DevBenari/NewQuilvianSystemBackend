# Roadmap Backend — Integrasi Rawat Inap ↔ Billing, Finishing Rawat Inap

| Field | Nilai |
|---|---|
| Roadmap | `integrasi-billing/roadmap/backend-roadmap-finishing.md` — revision `1` |
| Blueprint | `RWI-BP-001` revision `8`, sub-modul `integrasi-billing`, kontrak **`1.1.0` `approved`** 2026-10-02 lewat `RWI-DEC-221` |
| Status roadmap | **`DRAFT`** — menunggu approval pemilik atas roadmap ini. Task belum boleh dikirim ke `build-module-backend` sebelum approval itu tercatat |
| Ditulis | 2 Oktober 2026 oleh `plan-module-delivery` |
| Masukan dan hash approval | `02-backend-architecture.md` bagian 9 (`b2913e3e…`), `contracts/api-contract.md` bagian 3 (`08a4be1b…`), `data/data-dictionary.md` bagian 6 (`58b5e101…`), `testing/acceptance-test-matrix.md` bagian 4 (`532c0b8e…`), kontrak `state-transition-matrix.md` 5, `validation-matrix.md` 2, `integration-contract.md` 4, `permission-audit-matrix.md` 5. Hash lengkap pada `../blueprint-manifest.md` bagian 2.1 |
| Keputusan | `RWI-DEC-163` s.d. `170`, `186`, `187`, `192`, `195`, `196`, `207`, `210`, `221`; gate `1.10` (seluruh slice siap) |
| Source SHA | Backend `bf5c6bde` (HEAD saat perencanaan; sejak audit `c8e99ce5` kode yang berubah hanya saringan pencarian census) |
| Deret ID | `BE-RWI-146` s.d. `BE-RWI-159`. Deret satu modul (`02-module-map.md` 1.3); ID bebas berikutnya dicatat di manifest modul |
| Roadmap pendamping | `frontend-roadmap-finishing.md`, `requirement-traceability-finishing.md`. Roadmap kontrak `1.0.0` (`backend-roadmap.md`) tetap sebagai riwayat; task yang dicabut kontrak `1.1.0` ditandai di sana |

**Pembaruan bukti 5 Oktober 2026.** Laporan dan source dicocokkan pada HEAD `e2ded614` beserta perubahan kerja yang belum di-commit. Lima task (`BE-RWI-147`, `150`, `151`, `154`, `155`) selesai pada implementasi kode dengan pengecualian build bertanggal yang tercatat di laporan; `BE-RWI-149` sebagian karena migration belum ada; delapan task lain belum dikerjakan. Header `DRAFT` tetap menyatakan status approval roadmap. Laporan mencatat instruksi pengguna 2 Oktober 2026 untuk mengerjakan prasyarat IB; pembaruan ini mencatat hasil pekerjaan itu dan tidak memberikan wewenang task baru.

`dotnet build` tetap `NOT RUN`, dikecualikan atas keputusan pengguna 2 Oktober 2026 menurut laporan task. Migration `I1`/`I2` belum dibuat maupun diterapkan dan uji API/proses bisnis dengan database belum dijalankan. Tanda selesai kode tidak membuktikan kesiapan runtime atau produksi. Sebagai contoh, source penerima sudah menangani pesan ganda, tetapi satu invoice pada database belum dibuktikan lewat pengiriman dua pesan nyata.

**Kebijakan verifikasi backend.** Mengikuti `rules/backend/TEST_POLICY.md`: bukti task backend adalah QBE preflight/conformance, review diff/scope, `dotnet build` project aplikasi (wajib pada setiap task yang menyentuh source), verifikasi API/kontrak, verifikasi proses bisnis dengan contoh berangka, dan verifikasi manual/runtime bila lingkungan tersedia. Roadmap ini **tidak** memuat task automated test, dan tidak adanya automated test bukan gap.

**Pada setiap handoff ke `build-module-backend`:** QBE preflight dan kesesuaian engineering diselesaikan pada waktu eksekusi dari `AGENTS.md` backend dan `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` beserta `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`. Wewenang menerapkan migration ke database, menjalankan putar ulang, dan deployment **tidak** diberikan roadmap ini; ketiganya dinyatakan per task oleh pemilik.

**Arti tanda status pada dokumen ini.**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi, dan task **tidak boleh dimulai** |
| tanpa tanda | Belum dikerjakan |

## Grafik Urutan Dependency

```text
BE-RWI-147 ✅ ───┬─> BE-RWI-150 ✅ ─┬─> BE-RWI-151 ✅ ─┬─> BE-RWI-154 ✅
                 │                  │                  │
BE-RWI-149 🟡 ─┬─┘                  │                  └─────────────┐
               │                    │                                │
               │                    └─> BE-RWI-155 ✅ ─┬─────────────┴─> BE-RWI-157
               │                                       │
               │                                       ├─> BE-RWI-158
               │                                       │
               │                                       └───┬─> BE-RWI-156 ─┬─> BE-RWI-159
               │                                           │               │
               │                              BE-RWI-148 ──┘               │
               │                                                           │
               │                                BE-RWI-181 [EPS] ✅ ───────┘
               │
               └───────┬─> BE-RWI-153
                       │
BE-RWI-146 ──┐         │
             │         │
BE-RWI-152 ──┴─────────┘
```

`[EPS]` = task backend sub-modul `episode-rawat-inap` pada `../../episode-rawat-inap/roadmap/backend-roadmap-finishing.md`, cermin baca-saja.

Dependency ditulis sebagai **prasyarat langsung**. Prasyarat tidak langsung terbawa lewat rantainya; contoh, `BE-RWI-151` menunggu `BE-RWI-150`, dan `BE-RWI-150` menunggu `BE-RWI-149`. Jumlah pasangan prasyarat→task: **15**, sama dengan isi kolom `Dependency`.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `BE-RWI-146`, `BE-RWI-147`, `BE-RWI-148`, `BE-RWI-149`, `BE-RWI-152` — boleh paralel |
| 2 | `BE-RWI-147`, `BE-RWI-149` | `BE-RWI-150` |
| 2 | `BE-RWI-146`, `BE-RWI-149`, `BE-RWI-152` | `BE-RWI-153` |
| 3 | `BE-RWI-150` | `BE-RWI-151`, `BE-RWI-155` — boleh paralel |
| 4 | `BE-RWI-151` | `BE-RWI-154` |
| 4 | `BE-RWI-148`, `BE-RWI-155` | `BE-RWI-156` |
| 4 | `BE-RWI-151`, `BE-RWI-155` | `BE-RWI-157` |
| 4 | `BE-RWI-155` | `BE-RWI-158` |
| 5 | `BE-RWI-156`, `BE-RWI-181` [EPS] | `BE-RWI-159` — juga menunggu `BE-RWI-181` selesai di `episode-rawat-inap` |

**Pemetaan ke gelombang `04-prd-to-mvp.md` bagian Finishing.** `MVP-0` (`RWF-W0`): `BE-RWI-146` s.d. `BE-RWI-149`. `MVP-1` (`RWF-W1`): `BE-RWI-150` s.d. `BE-RWI-156`. `MVP-2`: `BE-RWI-157` (kode putar ulang; menjalankan `I5` di produksi butuh wewenang tertulis). `MVP-3`: `BE-RWI-158`. `MVP-4` (`RWF-W7`): `BE-RWI-159`. Gelombang eksekusi di atas adalah urutan teknis; gelombang PRD adalah urutan rilis.

**Catatan migration.** Seluruh migration memakai satu `ApplicationDbContext` dan satu `ApplicationDbContextModelSnapshot`. Task yang membuat migration — di sini `BE-RWI-149` (`I1`, `I2`) dan `BE-RWI-159` (`I6`) — tidak dibuat paralel di branch terpisah dengan task bermigration lain di sub-modul mana pun tanpa koordinasi snapshot. Urutan lintas sub-modul mengikuti `02-module-map.md` 7.4; `I6` wajib sesudah `E6` (`BE-RWI-181`).

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `BE-RWI-146` | Webhook izin pulang dan override pulang fisik hilang; penutupan tanpa izin kasir hanya lewat permission | `FR-RWF-001`, `005`; `RWI-DEC-166`, `167`, `186`, `187`; `UAT-RWF-02`, `12` | `1.1.0` API 3.1, 3.2 | `close-with-override` yang ada | `RWF-W0`. Hapus `InpatientDischargeClearanceController`; cabut PIN, konfirmasi password, nama peran pada `close-with-override` | — | Kartu | Kartu | FE lama masih memanggil endpoint lama — rilis bersama `FE-RWI-167`, `168` / Muhammad Hamzah | Kartu |
| `BE-RWI-147` | Tarif kamar hanya dihitung satu jalur, dan invoice rawat inap berlabel seragam `RANAP` | `FR-RWF-013`, `018`; `RWI-DEC-192` | API 3.1; backend 9.6 | `BillingCalculationService` | `RWF-W0`. Hapus `InpatientRoomChargeCalculationService` dan `occupancy-charges`; label `RANAP` | — | Kartu | Kartu | Invoice lama berlabel lain / Yasmina | Kartu |
| `BE-RWI-148` | Rupiah tagihan disaring di server berdasarkan permission | `FR-RWF-022`, `024`; `CAP-RWF-15`; `RWI-DEC-160`, `170`; `VAL-RWF-18` | API 3.7, 3.8 | `PatientBillingSummaryService` | `RWF-W0`. Ringkasan tanpa rupiah, `/amounts`, `inpatient-summary` tanpa nama peran | — | Kartu | Kartu | Pemetaan peran ke `ViewAmount` (`FIN-UNK-05`) / Muhammad Hamzah | Kartu |
| `BE-RWI-149` | Bentuk data integrasi `1.1.0` tersedia | `FR-RWF-006`, `007`, `014`, `019`; `RWI-DEC-192` | Data 6.3–6.8; backend 9.9–9.10 | Model yang ada | `RWF-W0`. Migration `I1` (Rawat Inap) dan `I2` (Billing) | — | Kartu | Kartu | Wewenang menerapkan migration terpisah / Muhammad Hamzah, Yasmina | Kartu |
| `BE-RWI-150` | Billing menerima ketukan pintu Rawat Inap dan membuka invoice `RANAP` | `FR-RWF-010`, `014`, `016`; `RWI-DEC-166`, `192`; `INT-RWF-01`; `UAT-RWF-11` | API 3.10; integrasi 4.2 | `BillingInvoiceService` | `RWF-W1`. `BillingInpatientEventReceiver`, `BilInpatientEventReceipt` | `BE-RWI-147`, `BE-RWI-149` | Kartu | Kartu | Idempotensi ganda / Yasmina | Kartu |
| `BE-RWI-151` | Outbox jujur: pesan hanya `Published` setelah tanda terima Billing | `FR-RWF-014`, `016`; `INV-RWF-05`; `RWI-DEC-161`, `166` | Integrasi 4.2; backend 9.6 | Worker yang ada | `RWF-W1`. Daftar putih, sewa pemrosesan, tanda terima, `DeadLetter` | `BE-RWI-150` | Kartu | Kartu | Pesan lama `Published` palsu / Muhammad Hamzah | Kartu |
| `BE-RWI-152` | Rawat Inap membaca status kasir hanya dari Billing lewat satu adapter | `FR-RWF-002`, `003`, `004`; `RWI-DEC-167`; `INT-RWF-02`, `03` | API 3.1, 3.3; integrasi 4.3 | `BilInpatientClearanceHandoff` | `RWF-W1`. `InpBillingClearanceAdapter`; hapus `billing-details` dan tulis `financial-clearance` | — | Kartu | Kartu | Billing tidak terbaca → gagal tertutup / Muhammad Hamzah | Kartu |
| `BE-RWI-153` | Keluar ruangan tidak ditahan kasir tetapi berjejak; penutupan menunggu izin kasir | `FR-RWF-005` s.d. `008`; `RWI-DEC-186`, `187`; `UAT-RWF-03`, `12` | API 3.2, 3.5; state 5; validasi 2 | `InpDischargeService` | `RWF-W1`. Keluar ruangan dengan peringatan, gerbang penutupan, dua daftar pantau | `BE-RWI-146`, `BE-RWI-149`, `BE-RWI-152` | Kartu | Kartu | Perubahan perilaku pulang / Muhammad Hamzah | Kartu |
| `BE-RWI-154` | Salah catat penempatan dapat dikoreksi tanpa menggandakan tarif kamar | `FR-RWF-019`; `RWI-DEC-157`, `192` (g); `RSK-RWF-02`; `UAT-RWF-25` | API 3.4; state 5 | `InpBedOccupancyService` | `RWF-W1`. `InpPlacementCorrectionService`, endpoint koreksi; transfer menerbitkan `BED_OCCUPIED` | `BE-RWI-151` | Kartu | Kartu | `IsSuperseded` dihitung ganda / Muhammad Hamzah, Yasmina | Kartu |
| `BE-RWI-155` | Layanan klinis kunjungan rawat inap masuk invoice `RANAP`; finalisasi dijaga | `FR-RWF-011`, `012`, `015`; `RWI-DEC-192`, `195`; `VAL-RWF-15` s.d. `17` | API 3.9; backend 9.6 | `BillingClinicalChargeBridgeService` | `RWF-W1`. Jembatan `RANAP`, biaya admin, finalisasi, antrean "perlu diperiksa" | `BE-RWI-150` | Kartu | Kartu | Titik tagih berbeda dari rawat jalan / Yasmina | Kartu |
| `BE-RWI-156` | Rincian Tagihan Pasien per kelompok V1 tersedia tanpa rupiah | `FR-RWF-020` s.d. `025`; `RWI-DEC-170` | API 3.7 | `PatientBillingSummaryService` | `RWF-W1`. `breakdown`, `breakdown/amounts`, pemetaan tujuh kelompok | `BE-RWI-148`, `BE-RWI-155` | Kartu | Kartu | Pemetaan kategori tarif (G-04) / Yasmina | Kartu |
| `BE-RWI-157` | Episode aktif dapat diputar ulang dengan aman saat rilis | `FR-RWF-017`; `RWI-DEC-169`; `UAT-RWF-15` | API 3.6 | Outbox | `MVP-2`. `InpIntegrationReplayService`, pemantau outbox | `BE-RWI-151`, `BE-RWI-155` | Kartu | Kartu | Putar ulang di produksi butuh wewenang tertulis / Muhammad Hamzah | Kartu |
| `BE-RWI-158` | Retur obat yang lolos pemeriksaan membatalkan tagihannya | `FR-RWF-011`; `RWI-DEC-195`, `210`; `INT-RWF-05`; `UAT-RWF-23` | Integrasi 4.5 | `DrugReturnService` | `MVP-3`. Fakta pembatalan sesudah verifikasi retur | `BE-RWI-155` | Kartu | Kartu | Modul Farmasi milik Ikbal Yulianto | Kartu |
| `BE-RWI-159` | Biaya operasi kunjungan asal tertaut ke invoice `RANAP` dan tampil di Tagihan Pasien | `RWI-DEC-207`; `RWI-AC-330` (tautan dan Tagihan Pasien); `INT-RWF-29`; `UAT-RWF-39`, `40` | API 3.11; integrasi 4.6; data 6.9 | Penerima event | `RWF-W7`. `BilInvoiceEncounterLink` (`I6`), tautan saat `ADMISSION_CONFIRMED`, baris tertaut | `BE-RWI-156`, `BE-RWI-181` [EPS] | Kartu | Kartu | Satu kwitansi butuh `BILL-INT-007` (G-27) / Yasmina | Kartu |

Seluruh kolom "Kartu" dirinci pada kartu task di bawah.

## Kartu task

### `BE-RWI-146` — Webhook izin pulang dan override pulang fisik dicabut

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Tidak ada lagi jalan masuk tanpa login atau berPIN yang mengubah status kepulangan. Penutupan tanpa izin kasir hanya bisa dilakukan pemegang `InpatientDischarge : CloseOverride` dengan alasan tertulis |
| **Requirement/decision** | `FR-RWF-001`, `FR-RWF-005`; `RWI-DEC-166`, `RWI-DEC-167`, `RWI-DEC-186`, `RWI-DEC-187`; `UAT-RWF-02`, `UAT-RWF-12` |
| **Kontrak** | `1.1.0`: API 3.1 (tiga endpoint dihapus), 3.2 (`close-with-override`); validasi 2; permission 5 |
| **Reuse** | `InpatientDischargeController.close-with-override` dan `AccessPermissionFilter` yang sudah ada |
| **Cakupan** | Hapus `InpatientDischargeClearanceController` beserta endpoint `discharge-clearance/webhook`, `supervisor-override`, dan `confirm-physical-discharge`. Pada `close-with-override`: hapus PIN, konfirmasi password, dan pemeriksaan nama peran; alasan wajib. `InpatientClearanceGateService` **belum** dihapus di sini — dihapus `BE-RWI-153` |
| **Dependency** | — |
| **Acceptance criteria** | 1. Ketiga route lama tidak terdaftar (404). 2. `close-with-override` tanpa token → 401; tanpa `InpatientDischarge : CloseOverride` → 403, apa pun nama perannya. 3. Alasan kosong atau terlalu pendek → 400 sesuai validasi 2. 4. Tidak ada sisa pembacaan PIN atau nama peran pada alur penutupan. 5. Baris registry `InpatientDischarge : CloseOverride` lahir dari atribut endpoint |
| **Verifikasi** | QBE preflight; review diff/scope; `dotnet build`; verifikasi API (atribut `[AccessPermission]`, route); verifikasi proses bisnis jalur override; percobaan Swagger untuk `UAT-RWF-02` bila lingkungan tersedia |
| **Risiko/pemilik** | Frontend lama (`FE-RWI-098`, `FE-RWI-099`) masih memanggil endpoint yang dihapus; rilis bersama `FE-RWI-167` dan `FE-RWI-168`. Pemilik: Muhammad Hamzah |
| **DoD** | Kelima kriteria terbukti; `dotnet build` tanpa error; laporan `../task/report/backend/BE-RWI-146.md`; roadmap dan `requirement-traceability-finishing.md` diperbarui |

### ✅ `BE-RWI-147` — Tarif kamar satu jalur dan label `RANAP` seragam

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI KODE 2 Oktober 2026.** Empat kriteria terpetakan ke source: endpoint `occupancy-charges` dan registrasi DI dicabut, pembaca invoice memakai `RANAP`, hitungan kamar lewat satu jalur. Tiga berkas service/interface/DTO lama masih ada tetapi tidak dipanggil; penghapusan tertunda sebagaimana laporan. `dotnet build` `NOT RUN` — dikecualikan atas keputusan pengguna 2 Oktober 2026. Hitungan tiga hari dengan data belum diuji runtime. Bukti: [laporan](../task/report/backend/BE-RWI-147.md) |
| **Outcome** | Tarif kamar hanya dihitung `BillingCalculationService` dari penempatan bed; tagihan susulan sesudah izin `CLEARED` masuk invoice yang sama karena labelnya seragam |
| **Requirement/decision** | `FR-RWF-013`, `FR-RWF-018`; `RWI-DEC-192` butir (b) dan (e) |
| **Kontrak** | `1.1.0`: API 3.1 (`occupancy-charges` dihapus); backend 9.6 |
| **Reuse** | `BillingCalculationService`, `MstRoomChargePolicy` |
| **Cakupan** | Hapus `InpatientRoomChargeCalculationService`, interface-nya, dan endpoint `occupancy-charges` pada `InpatientClearanceController`. Ganti label `"INPATIENT"` menjadi `"RANAP"` pada `InpatientClearanceService` dan `BilConsumerHandoffService` |
| **Dependency** | — |
| **Acceptance criteria** | 1. `POST …/invoices/occupancy-charges` tidak terdaftar. 2. Tidak ada pemanggil service lama. 3. Seluruh pembaca invoice rawat inap memakai `RANAP`. 4. Contoh berangka: kamar kelas 2 tiga hari hanya muncul satu kali pada invoice |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; verifikasi proses bisnis tarif kamar dengan contoh berangka |
| **Risiko/pemilik** | Invoice lama berlabel `INPATIENT` di lingkungan uji: dilaporkan sebagai temuan, **tidak** dimigrasi tanpa wewenang. Pemilik: Yasmina |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### `BE-RWI-148` — Rupiah tagihan disaring berdasarkan permission

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Pengguna tanpa izin rupiah tidak pernah menerima field rupiah dari API; pemegang `PatientBillingSummary : ViewAmount` menerimanya lewat endpoint tersendiri |
| **Requirement/decision** | `FR-RWF-022`, `FR-RWF-024`; `CAP-RWF-15`; `RWI-DEC-160`, `RWI-DEC-170`; `VAL-RWF-18` |
| **Kontrak** | `1.1.0`: API 3.7 (`GET /episodes/{id}` tanpa rupiah, `GET /episodes/{id}/amounts`), 3.8 (`inpatient-summary`) |
| **Reuse** | `PatientBillingSummaryService`, `PatientBillingSummaryController` |
| **Cakupan** | Pindahkan field rupiah dari ringkasan ke `/amounts` (`PatientBillingSummary : ViewAmount`); ganti penyaringan `Contains("Admin")` pada `inpatient-summary` dengan permission |
| **Dependency** | — |
| **Acceptance criteria** | 1. `GET …/episodes/{id}` tidak memuat field rupiah. 2. `GET …/amounts` tanpa `ViewAmount` → 403 (`AC-RWF-020`, `021`). 3. Penyaringan `Contains("Admin")`, `"Cashier"`, `"Finance"` pada `inpatient-summary` hilang; akun berperan "Cashier" tanpa `BillingInpatient : Read` → 403 (`AC-RWF-022`). 4. Baris registry `PatientBillingSummary : ViewAmount` lahir dari atribut endpoint |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API dan bentuk respons; penelusuran tidak ada field rupiah pada DTO ringkasan |
| **Risiko/pemilik** | Pemetaan peran ke `ViewAmount` adalah konfigurasi hak akses (`FIN-UNK-05`). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### 🟡 `BE-RWI-149` — Bentuk data integrasi `1.1.0` (`I1`, `I2`)

| Field | Isi |
|---|---|
| **Status** | 🟡 **SEBAGIAN, diperbarui 5 Oktober 2026.** Empat dari lima kriteria terpetakan ke source: bentuk model/configuration, FK koreksi `Restrict`, unique `IdempotencyKey`, dan tidak ada perubahan perilaku endpoint pada task ini. Kriteria 4 belum terpenuhi: migration `I1`/`I2` beserta `Down()` belum dibuat. Laporan mencatat kode selesai 2 Oktober 2026 serta build dan migration diambil alih pengguna; pengecualian build tetap berlaku (`NOT RUN`), sedangkan ketiadaan berkas migration tetap dicatat sebagai kekurangan source. Bukti: [laporan](../task/report/backend/BE-RWI-149.md) |
| **Outcome** | Kolom pengamatan status kasir, rantai koreksi penempatan, pemrosesan outbox, tanda "perlu diperiksa", dan tabel tanda terima tersedia tanpa mengubah perilaku |
| **Requirement/decision** | `FR-RWF-006`, `007`, `014`, `019`; `RWI-DEC-166`, `192` |
| **Kontrak** | Data 6.3 (`InpEpisode`), 6.4 (`InpBedPlacement`), 6.5 (`InpIntegrationOutboxes`), 6.6 (`BilInvoice`), 6.7 (`BilInpatientEventReceipt`), 6.8 DDL; backend 9.7, 9.9, 9.10 |
| **Reuse** | Model dan configuration yang sudah ada |
| **Cakupan** | Model, configuration, enum `InpClearanceObservation`, migration `I1` di `InPatientManagement`, dan migration `I2` di `BillingManagement`. Kolom yang dipensiunkan tidak dihapus |
| **Dependency** | — |
| **Acceptance criteria** | 1. Nama, tipe, nullability, bawaan, index, dan FK sama dengan kamus data 6.3–6.7. 2. FK rantai koreksi ke diri sendiri `Restrict`. 3. Unique `IdempotencyKey` pada tanda terima. 4. Kedua migration punya `Down()`. 5. Tidak ada perubahan perilaku endpoint |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; pemeriksaan skrip migration terhadap kamus data; penerapan migration hanya bila wewenang database diberikan |
| **Risiko/pemilik** | Wewenang menerapkan migration ke database terpisah. Pemilik: Muhammad Hamzah (Rawat Inap), Yasmina (Billing) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked mencatat status penerapan migration apa adanya; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-150` — Penerima ketukan pintu Billing dan invoice `RANAP` otomatis

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI KODE 2 Oktober 2026.** Empat kriteria terpetakan ke `BillingInpatientEventReceiver.ReceiveAsync`: invoice `RANAP`, tanda terima, penanganan `DUPLICATE`, dan rollback Billing terpisah dari transaksi admisi. `dotnet build` `NOT RUN` — dikecualikan atas keputusan pengguna 2 Oktober 2026. Empat jenis event dan gangguan Billing belum diuji runtime; tabel tanda terima menunggu migration `I2` pada `BE-RWI-149` 🟡. Bukti: [laporan](../task/report/backend/BE-RWI-150.md) |
| **Outcome** | Begitu episode `Admitted`, Billing membuka tepat satu invoice `RANAP` dan menjawab dengan tanda terima; pesan ganda tidak berefek dua kali |
| **Requirement/decision** | `FR-RWF-010`, `FR-RWF-014`, `FR-RWF-016`; `RWI-DEC-166`, `RWI-DEC-192`; `INT-RWF-01`; `UAT-RWF-11` |
| **Kontrak** | API 3.10 (penerima di dalam aplikasi); integrasi 4.2; data 6.7 |
| **Reuse** | `BillingInvoiceService`, `BillingCalculationService` |
| **Cakupan** | `BillingInpatientEventReceiver`: `ADMISSION_CONFIRMED` membuka invoice `RANAP` bila belum ada; `BED_OCCUPIED`, `OCCUPANCY_CORRECTED`, `BED_RELEASED` memicu hitung ulang tarif kamar. `BilInpatientEventReceipt` sebagai kunci idempotensi |
| **Dependency** | `BE-RWI-147`, `BE-RWI-149` |
| **Acceptance criteria** | 1. Pesan pertama → invoice `RANAP` terbuka, tanda terima `INVOICE_OPENED`, `Accepted = true`. 2. Pesan sama dikirim ulang → `DUPLICATE`, invoice tetap satu. 3. Galat Billing → tidak ada tanda terima tersimpan dan transaksi Billing batal. 4. Contoh `UAT-RWF-11`: Billing gangguan saat admisi, admisi tetap tersimpan |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi proses bisnis empat jenis event dengan contoh; verifikasi runtime bila lingkungan tersedia |
| **Risiko/pemilik** | Pemilik: Yasmina (Billing) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-151` — Outbox jujur

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI KODE 2 Oktober 2026.** Lima kriteria terpetakan ke source: daftar putih, `Published` hanya dengan tanda terima, backoff, `DeadLetter` setelah 10 kegagalan, dan pengambilan ulang sewa `Processing` yang habis. `dotnet build` `NOT RUN` — dikecualikan atas keputusan pengguna 2 Oktober 2026. Worker belum diuji dengan Billing hidup/mati; kolom pemrosesan menunggu migration `I1` pada `BE-RWI-149` 🟡. Bukti: [laporan](../task/report/backend/BE-RWI-151.md) |
| **Outcome** | Pesan hanya berisi field daftar putih dan hanya `Published` setelah Billing benar-benar menerima |
| **Requirement/decision** | `FR-RWF-014`, `FR-RWF-016`; `INV-RWF-05`; `RWI-DEC-161`, `RWI-DEC-166` |
| **Kontrak** | Integrasi 4.2 (daftar putih, kunci idempotensi, kegagalan); backend 9.6, 9.11 (`ProcessingLeaseSeconds = 300`, `BatchSize = 50`, `MaxRetry = 10`) |
| **Reuse** | `InpatientIntegrationOutboxWorker`, `InpIntegrationOutboxService` |
| **Cakupan** | Penolakan payload di luar daftar putih saat pendaftaran; sewa pemrosesan; worker memanggil `BillingInpatientEventReceiver` dalam proses yang sama; `Published` hanya bila `Accepted = true`; backoff; `DeadLetter` setelah 10 kali |
| **Dependency** | `BE-RWI-150` |
| **Acceptance criteria** | 1. Field di luar daftar putih ditolak. 2. Tanpa tanda terima, pesan tidak pernah `Published`. 3. Billing mati → `Failed`, dicoba ulang dengan backoff. 4. Gagal 10 kali → `DeadLetter`. 5. Pesan `Processing` yang sewanya habis diambil ulang |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi proses bisnis kelima kriteria; runtime bila tersedia |
| **Risiko/pemilik** | Pesan lama yang dulu ditandai `Published` tanpa terkirim ditangani putar ulang (`BE-RWI-157`). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### `BE-RWI-152` — Satu pembaca status kasir

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Status kasir di Rawat Inap selalu dibaca langsung dari Billing, tanpa salinan dan tanpa rupiah |
| **Requirement/decision** | `FR-RWF-002`, `FR-RWF-003`, `FR-RWF-004`; `RWI-DEC-167`; `INT-RWF-02`, `INT-RWF-03` |
| **Kontrak** | API 3.1 (`billing-details`, tulis `financial-clearance` dihapus), 3.3 (`billing-status`); integrasi 4.3 |
| **Reuse** | `BilInpatientClearanceHandoff` milik Billing; `InpatientBillingQueryService` |
| **Cakupan** | `IInpBillingClearanceAdapter` dan implementasinya; `billing-status` lewat adapter; hapus `GetFinancialDetailsAsync`, `GET billing-details`, dan `POST financial-clearance`; `GET financial-clearance` tetap baca saja |
| **Dependency** | — |
| **Acceptance criteria** | 1. `billing-status` memuat status dan daftar kendala tanpa rupiah dari Billing. 2. Billing tidak terbaca → status "tidak dapat dibaca", tidak pernah dianggap `CLEARED`. 3. `billing-details` dan `POST financial-clearance` tidak terdaftar. 4. Riwayat `financial-clearance` tetap terbaca |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; verifikasi proses bisnis jalur gagal-tertutup |
| **Risiko/pemilik** | Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### `BE-RWI-153` — Keluar ruangan berjejak dan gerbang penutupan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Perawat dapat mencatat pasien meninggalkan ruangan tanpa ditahan kasir, dengan peringatan dan jejak status kasir; episode hanya ditutup normal bila izin kasir `CLEARED` |
| **Requirement/decision** | `FR-RWF-005` s.d. `FR-RWF-008`; `RWI-DEC-186`, `RWI-DEC-187`; `UAT-RWF-03`, `UAT-RWF-12` |
| **Kontrak** | API 3.2 (`record-departure`, `closure-readiness`, `close`, `close-with-override`), 3.5 (dua daftar pantau); state 5; validasi 2 (`INP-DEP-001`) |
| **Reuse** | `InpDischargeService` partial `Closure`; `InpatientMonitoringController` |
| **Cakupan** | `record-departure`: lepas bed seketika, baca status lewat adapter, pengakuan peringatan wajib bila bukan `CLEARED` (409 `INP-DEP-001`), simpan kolom pengamatan, antrekan `BED_RELEASED`. `close` wajib `CLEARED`; `close-with-override` menyimpan status saat penutupan. Hapus `InpatientClearanceGateService`. Daftar `departures-before-clearance` dan `closures-without-financial-clearance` |
| **Dependency** | `BE-RWI-146`, `BE-RWI-149`, `BE-RWI-152` |
| **Acceptance criteria** | 1. Status `PENDING` + tanpa pengakuan → 409; dengan pengakuan → bed kosong dan status saat keluar tersimpan. 2. `close` dengan status selain `CLEARED` → ditolak. 3. Izin dicabut sesudah disetujui → tombol penutupan kembali terkunci (`UAT-RWF-03`). 4. Override tercatat dengan alasan dan status saat itu, dan muncul di laporan (`UAT-RWF-12`). 5. Daftar "pulang sebelum izin kasir" memuat status saat keluar dan status sekarang |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; verifikasi proses bisnis dengan contoh `UAT-RWF-03`; runtime bila tersedia |
| **Risiko/pemilik** | Perubahan perilaku pulang yang terasa bagi perawat dan kasir; penutupan pemakaian alat saat keluar ruangan ditambahkan `BE-RWI-168` (`keperawatan`). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-154` — Koreksi salah catat penempatan

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI KODE 2 Oktober 2026.** Empat kriteria terpetakan ke source: gerbang invoice, baris koreksi berantai, pemisahan transfer/koreksi, dan saringan Billing `SupersededByCorrectionId`. `dotnet build` `NOT RUN` — dikecualikan atas keputusan pengguna 2 Oktober 2026. Skenario `RSK-RWF-02` belum diuji dengan database; menunggu migration `I1`. Adapter status kasir sudah ada untuk koreksi, tetapi cakupan penuh `BE-RWI-152` tetap belum dikerjakan. Bukti: [laporan](../task/report/backend/BE-RWI-154.md) |
| **Outcome** | Kepala ruangan atau admisi berwenang mengoreksi kamar, bed, kelas, atau waktu selama invoice `OPEN`; versi lama tetap tersimpan dan tarif kamar dihitung ulang tanpa dobel |
| **Requirement/decision** | `FR-RWF-019`; `RWI-DEC-157`, `RWI-DEC-192` butir (g); `RSK-RWF-02`; `UAT-RWF-25` |
| **Kontrak** | API 3.4 (`POST placements/{placementId}/corrections`, `InpatientBedOccupancy : Correct`; `placements/by-episode` bertambah tiga field; transfer menerbitkan `BED_OCCUPIED`); state 5 |
| **Reuse** | `InpBedOccupancyService`, linimasa penempatan |
| **Cakupan** | `InpPlacementCorrectionService` dan endpoint-nya; event `OCCUPANCY_CORRECTED` hanya untuk koreksi; transfer biasa menerbitkan `BED_OCCUPIED` |
| **Dependency** | `BE-RWI-151` |
| **Acceptance criteria** | 1. Koreksi saat invoice `FINAL` → 422 "tagihan sudah difinalkan". 2. Koreksi membuat baris baru dan menandai baris lama, alasan wajib. 3. Transfer dan koreksi dapat dibedakan pada riwayat. 4. Skenario `RSK-RWF-02`: pindah kamar lalu koreksi salah catat — tarif kamar per hari tidak dobel dan tidak kurang (contoh berangka di laporan) |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; verifikasi proses bisnis skenario `IsSuperseded`; runtime bila tersedia |
| **Risiko/pemilik** | Penanda `IsSuperseded` dipakai transfer dan koreksi. Pemilik: Muhammad Hamzah, Yasmina |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### ✅ `BE-RWI-155` — Jembatan layanan klinis `RANAP`, biaya admin, dan finalisasi

| Field | Isi |
|---|---|
| **Status** | ✅ **SELESAI KODE 2 Oktober 2026.** Enam kriteria terpetakan ke source: jembatan `Inpatient`, obat saat penyerahan, biaya admin termasuk tarif kamar, penjaga `BIL-FIN-020`/`021`, penyelesaian `BIL-REV-001`, dan perluasan kondisi rawat jalan tanpa perubahan titik tagih. `dotnet build` `NOT RUN` — dikecualikan atas keputusan pengguna 2 Oktober 2026. Contoh berangka per layanan dan regresi rawat jalan belum diuji runtime; kolom pemeriksaan invoice menunggu migration `I2`. Bukti: [laporan](../task/report/backend/BE-RWI-155.md) |
| **Outcome** | Tindakan, lab, radiologi, obat, dan konsultasi kunjungan rawat inap masuk invoice `RANAP` tanpa input kasir; invoice tidak dapat difinalkan selama "perlu diperiksa" atau ada tarif yang belum diatur |
| **Requirement/decision** | `FR-RWF-011`, `FR-RWF-012`, `FR-RWF-015`; `RWI-DEC-192`, `RWI-DEC-195`; `VAL-RWF-15` s.d. `17` |
| **Kontrak** | API 3.9 (`review-queue`, `review-resolution`); backend 9.6 |
| **Reuse** | `BillingClinicalChargeBridgeService`, `BillingSourceTariffResolver`, `BillingCalculationService`, `BillingFinalizationService` |
| **Cakupan** | Jembatan menerima kunjungan `Inpatient` dengan titik tagih seperti rawat jalan; biaya admin untuk invoice bertarif kamar; finalisasi menolak `RequiresReview` dan "tarif belum ada"; antrean dan penyelesaian "perlu diperiksa" |
| **Dependency** | `BE-RWI-150` |
| **Acceptance criteria** | 1. Tindakan "Pasang infus" `Completed` dan lab "Darah Lengkap" pasien rawat inap masuk invoice `RANAP` dengan harga master tarif (`AC-RWF-011`). 2. Obat masuk saat diserahkan; MAR bukan sumber tagihan (`AC-RWF-090`, `091`). 3. Biaya admin dihitung untuk invoice bertarif kamar (`AC-RWF-013`). 4. Finalisasi ditolak bila `RequiresReview` (`BIL-FIN-020`) atau ada "tarif belum ada" (`BIL-FIN-021`). 5. Penyelesaian pemeriksaan ditolak bila biaya kamar manual dan otomatis sama-sama aktif (`BIL-REV-001`). 6. Regresi rawat jalan: tindakan, lab, dan obat pasien rawat jalan menghasilkan invoice yang sama dengan sebelum perubahan |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; verifikasi proses bisnis per jenis layanan dengan contoh berangka |
| **Risiko/pemilik** | Pemilik: Yasmina |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### `BE-RWI-156` — Rincian Tagihan Pasien per kelompok

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Bangsal membaca rincian tagihan per kelompok V1 tanpa rupiah; pemegang izin rupiah mendapat subtotal per kelompok dan total berjalan, tidak pernah harga per item |
| **Requirement/decision** | `FR-RWF-020` s.d. `FR-RWF-025`; `RWI-DEC-170`; gap G-04 (`CONFIGURABLE_DEFAULT`) |
| **Kontrak** | API 3.7 (`breakdown`, `breakdown/amounts`); backend 9.11 (`PatientBillingSummary:GroupMapping`) |
| **Reuse** | Hitungan invoice Billing |
| **Cakupan** | Dua endpoint, pemetaan tujuh kelompok dari penanda kategori tarif, keadaan `NOT_FORMED`, penanda "tarif belum ada" |
| **Dependency** | `BE-RWI-148`, `BE-RWI-155` |
| **Acceptance criteria** | 1. Urutan kelompok: Kamar, Tindakan, Penunjang, Obat & Alkes, Pemakaian Alat, Operasi, Biaya Administrasi; kelompok tanpa data tidak tampil. 2. Baris tanpa rupiah: nama, periode atau tanggal, jumlah unit. 3. `breakdown/amounts` hanya dengan `ViewAmount`; tanpa harga per item. 4. Invoice belum ada → `NOT_FORMED`, tidak pernah "Rp 0" |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API dan bentuk respons; contoh berangka |
| **Risiko/pemilik** | Pemetaan kategori tarif ke kelompok (G-04). Pemilik: Yasmina |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### `BE-RWI-157` — Putar ulang terkontrol dan pemantauan outbox

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Episode yang aktif saat rilis dapat dimasukkan ke alur otomatis lewat satu kali putar ulang yang aman, dimulai dengan uji tanpa efek |
| **Requirement/decision** | `FR-RWF-017`; `RWI-DEC-169`; `UAT-RWF-15` |
| **Kontrak** | API 3.6 (`GET integration-outbox`, `POST integration-outbox/replay`, `InpatientIntegrationOutbox : Read`, `: Replay`) |
| **Reuse** | Outbox jujur (`BE-RWI-151`), invoice `RANAP` (`BE-RWI-155`) |
| **Cakupan** | `InpIntegrationReplayService` (`DryRun`, kunci idempotensi asli, `ReplayBatchId`); `InpatientIntegrationOutboxController`. Episode berbiaya kamar manual ditandai "perlu diperiksa" |
| **Dependency** | `BE-RWI-151`, `BE-RWI-155` |
| **Acceptance criteria** | 1. `DryRun = true` menghasilkan daftar tanpa efek. 2. Putar ulang kedua tidak mengubah apa pun. 3. Episode berbiaya kamar manual menjadi "perlu diperiksa". 4. Tanpa `InpatientIntegrationOutbox : Replay` → 403 |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API; verifikasi proses bisnis di lingkungan uji (`UAT-RWF-15`) bila tersedia |
| **Risiko/pemilik** | Menjalankan putar ulang di produksi (`I5`) **bukan** bagian task ini; butuh wewenang tertulis tersendiri (`RWI-DEC-169` butir 5). Pemilik: Muhammad Hamzah |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### `BE-RWI-158` — Retur obat membatalkan tagihan

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Retur yang lolos pemeriksaan Farmasi membatalkan tagihan obat sebanyak jumlah yang kembali, dengan rujukan nomor retur, tanpa menghapus baris asli |
| **Requirement/decision** | `FR-RWF-011`; `RWI-DEC-195` butir 3, `RWI-DEC-210`; `INT-RWF-05`; `UAT-RWF-23`; `AC-RWF-012` |
| **Kontrak** | Integrasi 4.5 |
| **Reuse** | `DrugReturnService.VerifyAsync`, jembatan klinis Billing |
| **Cakupan** | Fakta pembatalan klinis sesudah verifikasi retur commit; Billing membatalkan baris; retur bahan OK mengikuti aturan yang sama begitu bahan OK tertagih |
| **Dependency** | `BE-RWI-155` |
| **Acceptance criteria** | 1. Serah 3 vial, retur 1 layak → invoice 2 vial dengan pembatalan merujuk nomor retur. 2. Retur dinilai rusak → tagihan tidak berubah. 3. Retur sesudah invoice final tidak otomatis mengubah invoice (jalur adjustment Billing) |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi proses bisnis dengan contoh berangka; runtime disaksikan petugas Farmasi bila tersedia |
| **Risiko/pemilik** | Perubahan di modul Farmasi milik Ikbal Yulianto (disetujui `RWI-DEC-210`) |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

### `BE-RWI-159` — Tautan biaya operasi kunjungan asal ke invoice `RANAP`

| Field | Isi |
|---|---|
| **Status** | Belum dikerjakan |
| **Outcome** | Pasien poli atau ODC yang dirawat sesudah operasi punya invoice kunjungan asal yang tertaut ke invoice `RANAP`; baris operasinya tampil di Tagihan Pasien bangsal tanpa satu baris pun berpindah invoice |
| **Requirement/decision** | `RWI-DEC-207`; `RWI-AC-330` bagian tautan dan Tagihan Pasien; `INT-RWF-29`; `UAT-RWF-39`, `UAT-RWF-40` |
| **Kontrak** | API 3.11 (field `LinkedEncounter`, `LinkedEncounters`, `IncludesLinkedEncounter`); integrasi 4.6; data 6.9; backend 9.14 |
| **Reuse** | Penerima event (`BE-RWI-150`), rincian Tagihan Pasien (`BE-RWI-156`) |
| **Cakupan** | `BilInvoiceEncounterLink` dan migration `I6` (sesudah `E6`); penerima membaca `InpAdmissionReferral` dengan `CompletedEpisodeId`; `breakdown` dan `amounts` menyertakan baris operasi kunjungan tertaut |
| **Dependency** | `BE-RWI-156`, `BE-RWI-181` [EPS] |
| **Acceptance criteria** | 1. Admisi dari permintaan kamar pulih → tepat satu tautan. 2. Tidak ada baris yang berpindah invoice. 3. Pesan ganda dan putar ulang tidak menambah tautan. 4. Admisi tanpa permintaan → tanpa tautan. 5. Isi pesan `ADMISSION_CONFIRMED` tetap daftar putih |
| **Verifikasi** | QBE preflight; review diff; `dotnet build`; verifikasi API dan bentuk respons; verifikasi proses bisnis dengan contoh pasien Poli Bedah |
| **Risiko/pemilik** | "Satu transaksi, satu kwitansi" pada `RWI-AC-330` **tidak** dicakup task ini; bergantung `BILL-INT-007` milik `billing-kasir` (gap G-27). Pemilik: Yasmina |
| **DoD** | Kriteria terbukti; build tanpa error; laporan tracked; roadmap dan traceability diperbarui |

## Langkah operasional di luar task

| Langkah | Kapan | Gerbang |
|---|---|---|
| `I5` — putar ulang episode aktif di produksi, diawali `DryRun` | Sesudah `BE-RWI-157` terbukti di lingkungan uji | ⛔ menunggu **wewenang tertulis tersendiri** dari pemilik (`RWI-DEC-169` butir 5). Bukan pekerjaan builder |
| Penghapusan kolom yang dipensiunkan pada `InpEpisode` | `POST-MVP`, sesudah dua siklus rilis tanpa pembaca | Tidak direncanakan di roadmap ini |

## Dependency luar dan gerbang

| Butir | Menahan | Pemilik |
|---|---|---|
| `BE-RWI-181` [EPS] — tabel dan service `InpAdmissionReferral` | `BE-RWI-159` | Muhammad Hamzah |
| G-27 — penyelesaian multi-invoice `BKC-DEC-118` / `BILL-INT-007` | Bagian "satu transaksi" `RWI-AC-330`; bukan task di roadmap ini | Yasmina (`billing-kasir`) |
| Approval roadmap ini | Seluruh task | Muhammad Hamzah; Yasmina untuk task Billing |
