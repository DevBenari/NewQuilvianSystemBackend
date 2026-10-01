# Laporan Perubahan Backend — `BE-RJE-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RJE-003` |
| Judul | Jembatan folio → invoice untuk tindakan, Lab, Radiologi |
| Slice | `MVP-1` — `EPIC RJE-02` Jembatan folio ke invoice |
| Roadmap | [roadmap/e2e-backend-roadmap.md](../../../roadmap/e2e-backend-roadmap.md), kartu `BE-RJE-003` |
| Trace | `FR-RJE-010`..`013`; `RJ-E2E-DEC-002`, `003`, `006`, `016`, `019`; `02-backend-architecture.md` V2.7.1–V2.7.4; `contracts/state-transition-matrix.md` V2-A; `contracts/validation-matrix.md` V2 |
| Contract version | `RJ-E2E-CONTRACT-001@1.0.2` — `approved` |
| Dependency | `BE-RJE-002` ✅ (28 September 2026) |
| Klasifikasi | `MEDIUM` — 2 service baru, 2 berkas diubah, integrasi lintas tiga transaksi, tanpa model/migration/endpoint baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` source dan dokumen blueprint `rawat-jalan`; runtime terhadap `QuilvianNewDevSukma` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `5f68db49` (`sukmagp`), working tree belum di-commit |
| Tanggal | 28 September 2026 |
| Status | ✅ **SELESAI** — kedelapan acceptance criteria terbukti di runtime; sumber Lab dan Radiologi sebagian sintetis (bagian 5.1) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` (Billing, Operational) |
| Registry | `Bil` `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk dua service baru; perluasan `BillingFolioService` dan registrasi DI |
| QBE yang berlaku | `QBE-SVC-001` (logika di service; tanpa controller), `QBE-VAL-001`, `QBE-TXN-001` (batas transaksi terpisah per mata rantai), `QBE-LOG-001`/`QBE-AUD-001` (audit penerusan lewat `LoggerService.AuditAsync`, terpisah dari tabel), `QBE-DTO-001` (entity tidak diekspos) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-MOD-*` — tanpa model; `QBE-PERM-001`/`QBE-API-001` — tanpa endpoint baru |
| Wewenang | `RJ-E2E-DEC-019` — source dan runtime ke `QuilvianNewDevSukma`; tanpa migration, commit, atau deployment |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, pelayanan yang tercatat di folio tidak pernah sampai ke invoice yang dibaca kasir.
Tindakan *USG Regio Cruris* yang sudah dikerjakan dokter tidak muncul di Menu Pembayaran, dan kasir
harus mengetik ulang harganya.

Sesudah task ini, begitu tindakan dikerjakan, item `PROCEDURE` Rp1.246.000 langsung muncul di invoice
kunjungan. Harganya dari katalog tarif, tanpa input kasir.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Satu pelayanan menghasilkan tepat satu item invoice berharga katalog, atau tercatat jelas mengapa tidak |
| Pelaku | Sistem; petugas Billing untuk item yang masuk antrean (layar menyusul di `FE-RJE-003`) |
| Pemicu | Folio mengakui efek pelayanan (tindakan dikerjakan, spesimen Lab diterima, mutu study Radiologi diterima) |
| Langkah | 1. Folio membuat efek dan menandainya `Pending` bila konteksnya calon, **di dalam** transaksi folio. 2. Setelah commit, folio memanggil jembatan. 3. Jembatan (scope sendiri) menilai kelayakan: konteks, tipe kunjungan, pembatalan, domain yang sudah didukung, keadaan invoice. 4. Resolver mencari tarif pada tanggal pelayanan. 5. Jembatan memanggil `UpsertChargeAsync` dengan kunci idempotency deterministik. 6. Hasil ditulis ke kolom sinkron efek, dijaga `InvoiceSyncVersion`, dan diaudit |
| Aturan | Harga dari `MstTariff`, bukan snapshot klinis; tidak pernah Rp0 karena tarif hilang; kunci selalu sama untuk satu (fakta, versi); kegagalan jembatan tidak pernah membatalkan folio maupun klinis |
| Status | `Pending` → `Synced` / `NotApplicable` / `ReconciliationRequired` / `Failed` (lihat state matrix V2-A) |
| Jalur tidak normal | Tarif tidak ada → `ReconciliationRequired` `TARIFF_NOT_FOUND`. Kunjungan bukan Rawat Jalan → `NotApplicable`. Pengulangan Radiologi karena kesalahan rumah sakit, tindakan gratis → `NotApplicable`. Invoice sudah final → `ReconciliationRequired` `INVOICE_NOT_OPEN` (penyesuaian otomatis di `BE-RJE-005`). Gangguan sementara → `Failed`, dijadwalkan ulang dari `MstBillingSyncPolicy`; habis batas → `RETRY_EXHAUSTED` |
| Sengaja ditunda | Efek pembatalan (`BE-RJE-009`), konsultasi (`BE-RJE-007`), dan resep (`BE-RJE-008`) **tetap `Pending`** dengan kode `CANCELLATION_PENDING_SUPPORT` / `SOURCE_PENDING_SUPPORT` — tidak dibuang, diproses begitu dukungannya tersedia |
| Hasil akhir | Satu invoice per kunjungan berisi item dari seluruh domain yang didukung |

**Contoh jadwal kirim ulang** dengan kebijakan seed (5 / 60 / 3600): gagal ke-1 → 60 detik, ke-2 → 120,
ke-3 → 240, ke-4 → 480; gagal ke-5 → `ReconciliationRequired` `RETRY_EXHAUSTED`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingFolioService.cs` (struktur `RecognizeMilestoneAsync`, titik commit, `ChangeTracker.Clear`),
`BillingInvoiceService.cs` (`UpsertChargeAsync`: transaksi serializable, advisory lock, receipt,
`ValidateRequest`, `ComputePayloadHash`), `BillingInvoiceDtos.cs`, `BilInvoice.cs`,
`ClinicalMilestoneFactProducer.cs`, `TrxPatientProcedure.cs`, `LabExamination.cs`, `RadStudy.cs`,
`RadOrder.cs`, `MstTariff.cs`, `RegPatientEncounter.cs`, `LoggerService.cs`, `Program.cs`,
`BillingManagementServiceCollectionExtensions.cs`, `InpatientIntegrationOutboxWorker.cs` (pola scope),
serta controller Lab/tindakan untuk validasi.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | **Baru.** `TrySyncEffectAsync` (tidak pernah melempar), penilaian kelayakan V2.7.1, pemetaan domain/status V2.7.2, kunci deterministik (SHA-256 → Guid v5), panggilan `UpsertChargeAsync` pada scope DI sendiri, pencatatan hasil dengan token `InvoiceSyncVersion`, jadwal kirim ulang, audit `BillingBridge.*`. Konstanta `BillingBridgeSourceDomains` dan `BillingBridgeCodes` |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSourceTariffResolver.cs` | **Baru.** Tarif untuk `PROCEDURE`, `LABORATORY`, `RADIOLOGY`: tarif langsung dari sumber, lalu pencarian per tindakan dengan urutan paling spesifik (klinik + kelas pasien); tindakan gratis dan pengulangan kesalahan internal → tidak ditagihkan |
| `Areas/HealthServices/BillingManagement/Operational/Services/BillingFolioService.cs` | Logika lama dipindah ke `RecognizeMilestoneCoreAsync`; `RecognizeMilestoneAsync` memanggilnya lalu memanggil jembatan saat sukses; `CreateProcessingEffect` menandai efek calon `Pending` |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Registrasi `BillingSourceTariffResolver` dan `BillingClinicalChargeBridgeService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru atau bentuk respons yang berubah. Perilaku baru: endpoint yang menerbitkan fakta klinis (eksekusi tindakan, penerimaan spesimen, penerimaan study, endpoint folio internal) kini ikut membentuk item invoice |
| Database | Tanpa migration. Kolom yang dipakai dibuat `BE-RJE-001`. Penulisan baru: `BilInvoice`/`BilInvoiceItem`/`BilChargeReceipt` (lewat `UpsertChargeAsync`) dan kolom sinkron `BilProcessingEffect` |
| Keamanan/Auth | Harga tidak lagi dapat berasal dari modul klinis maupun browser pada jalur ini. Tidak ada perubahan atribut hak akses. Audit tanpa nama pasien maupun isi klinis |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru atau berubah bentuk. Endpoint pemicu yang dipakai validasi:

#### Health Services / Billing Management / Billing Folio

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/internal/milestones/recognize` | Pengakuan milestone internal; kini juga memicu penerusan ke invoice | `BillingMilestone : RecognizeInternal` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false --no-incremental -o <scratchpad>/out-rje003` | `0 Error(s)`, `230 Warning(s)`, 2 menit 35 detik; **nol** warning dari berkas task | `PASS` | Sama dengan baseline |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | — |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 4 berkas, `VIOLATION 0` / `REVIEW 0` / `INFO 0`, `PASS` | `PASS` | — |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test; folder `Tests/` dilarang dibuat |
| Validasi runtime R0–R8 | Seluruhnya `PASS` (bagian 5.1) | `PASS` | — |

### 5.1 Hasil validasi runtime — 28 September 2026

Aplikasi hasil build dijalankan dari scratchpad pada `http://localhost:5219` terhadap
**`QuilvianNewDevSukma`**, sesi `superadmin` dari login seed (kredensial tidak dicetak).

**Batas bukti yang disengaja.** Tindakan diuji lewat alur API sungguhan. **Lab:** order dan spesimen
dibuat lewat API sungguhan sampai `receive`, tetapi `accept` ditolak `403` "Petugas yang mengambil
sampel tidak boleh menyatakan kelayakannya". Aturan pemisahan tugas itu benar, dan hanya satu akun
yang tersedia. Karena itu fakta Lab disisipkan sintetis dengan menunjuk **pemeriksaan Lab nyata**
hasil API. **Radiologi:** acquisition lewat API ditolak gerbang keselamatan fail-closed (9 alat
belum punya aturan berlaku), sehingga order/study dan fakta disisipkan sintetis bertanda
`TEST-RJE003`. Pada kedua kasus, **folio, jembatan, resolver, dan `UpsertChargeAsync` berjalan
sungguhan** lewat endpoint folio internal.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R0 | Login; tanpa cookie | `200`; tanpa cookie `401` | — | `PASS` |
| R1 | Tindakan tunai (*USG Regio Cruris*): buat → approve → execute (API sungguhan) | Efek `Synced`; satu item `PROCEDURE`/`PERFORMED`, qty 1, Rp1.246.000, `TariffId` = tarif hasil perhitungan SQL pembanding, kontrak `BIL-INTEGRATION-1.3`; invoice `OPEN` | 1 | `PASS` |
| R8 | Tindakan pada kunjungan **asuransi** (*GU5 Fissure Sealent*) | Item Rp1.210.000 = tarif katalog, **bukan** harga pada tindakan (Rp1.573.000). `calculation-preview` `200`: gross Rp1.210.000; bagian penjamin Rp0 karena aturan coverage penjamin itu belum dikonfigurasi di data dev — di luar jembatan | 1, 8 | `PASS` |
| R2 | Lab: order + spesimen nyata; fakta sintetis menunjuk pemeriksaan nyata *D6 IGA* | Efek `Synced`; item `LABORATORY`/`ACCEPTED`, qty 1, Rp1.251.000 = tarif pemeriksaan | 2 | `PASS` |
| R3 | Radiologi: study normal (sintetis) | Efek `Synced`; item `RADIOLOGY`/`PERFORMED`, Rp198.000 = tarif yang diharapkan (*Film Xray 8x10 4 Film*) | 3 | `PASS` |
| R3b | Radiologi: pengulangan `InternalHospitalError` | Efek `NotApplicable` `REPEAT_INTERNAL_ERROR`; nol item | 3 | `PASS` |
| R4 | Efek R3 dikembalikan ke `Pending` (simulasi status hilang setelah invoice berhasil), lalu endpoint folio dipanggil ulang dengan body sama → replay folio memicu jembatan lagi | Replay folio `200`; item tetap **1**, receipt tetap **1**, efek kembali `Synced` | 4 | `PASS` |
| R5 | Pemeriksaan tanpa tarif (sintetis) | Efek `ReconciliationRequired` `TARIFF_NOT_FOUND`; nol item — tidak ada Rp0 | 5 | `PASS` |
| R5b | Tindakan tanpa tarif lewat API | Ditolak modul klinis `400` "Tarif rumah sakit untuk tindakan belum dikonfigurasi" sebelum fakta terbit — penjaga hulu yang sudah ada | 5 | `PASS` |
| R6 | Efek *Procedure* pada kunjungan **IGD** (`EncounterType 2`) | `NotApplicable` `NOT_OUTPATIENT` | 6 | `PASS` |
| R7 | Invoice kunjungan R1 | Hanya domain `PROCEDURE` (nol `REGISTRATION`); `AdministrationFeeAmount` berasal dari kalkulasi (Rp0 — policy `ADM-RAJAL-DRAFT` seed tidak aktif) | 7 | `PASS` |
| R9 | Ringkasan kunjungan uji R1/R2/R3 | Tepat **1** invoice berisi `LABORATORY` 1, `PROCEDURE` 1, `RADIOLOGY` 1 | 1 (`AC-RJ-001`) | `PASS` |

**Catatan AC 6.** Kartu menyebut kunjungan **rawat inap**. Database dev hanya memiliki kunjungan tipe
`1` (Rawat Jalan) dan `2` (IGD), sehingga cabang "bukan Rawat Jalan" dibuktikan dengan kunjungan IGD.
Cabangnya satu dan sama (`EncounterType != Outpatient`).

**Percobaan yang tidak dipakai sebagai bukti.** (a) Menyusun ulang payload `from-source` dari kolom item
untuk menguji replay menghasilkan `409` karena hash payload peka skala desimal dan presisi waktu —
kesalahan rekonstruksi skrip uji, bukan cacat. Item tetap 1. Digantikan R4. (b) Skrip sempat berhenti
karena variabel uji menimpa URL dasar; dilanjutkan dengan ID yang sama tanpa membuat data ulang.

Uji manual: `PASS` — dijalankan agent lewat HTTP sungguhan.

**Tidak dijalankan:** jalur `Failed` → kirim ulang terjadwal (butuh gangguan database terkendali;
dibuktikan saat `BE-RJE-010`); `INVOICE_NOT_OPEN` (tidak ada invoice final di data dev; cabangnya
diganti adjustment pada `BE-RJE-005`); konkurensi dua pemroses bersamaan (`BE-RJE-010`).

### 5.2 Keadaan database sesudah run

| Data | Keadaan akhir |
| --- | --- |
| Invoice `6810a4d2-605c-48f0-9c29-34b1a6cb80b1` (kunjungan uji R1) | `OPEN`, 3 item: `PROCEDURE`, `LABORATORY`, `RADIOLOGY` |
| Invoice `df6ef641-e220-4ef2-8b65-08e4430c06db` (kunjungan asuransi R8) | `OPEN`, 1 item `PROCEDURE` |
| Tindakan uji | 2 tindakan dikerjakan (catatan `TEST-RJE003`) |
| Lab | Order `3b7d2c62-bb3a-4e9d-80ef-90f0d289d4e5` dengan satu spesimen berstatus *diterima* (belum dinyatakan layak); satu fakta sintetis Lab (`e2997573-…`) |
| Radiologi | 3 `RadOrder` + 3 `RadStudy` sintetis bernomor `TEST-RJE003-*` (`9671ece6-…`, `f03007d2-…`, `b4df288c-…`) dan 3 fakta sintetis berstatus penyerahan `Pending` |
| Efek IGD uji | 1 efek `NotApplicable` pada folio kunjungan IGD |

**Dampak ke pengujian berikutnya:** fakta sintetis berstatus `Pending` **akan terbaca** oleh pekerja kirim
ulang fakta (`BE-RJE-011`). Saat task itu divalidasi, fakta ini perlu diperhitungkan atau ditandai
selesai lewat antrean rekonsiliasi. Data dibiarkan dan dicatat, sama dengan perlakuan data uji Bank
Darah.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tindakan dieksekusi → satu item `PROCEDURE` dengan harga `MstTariff` (bukan snapshot) | Terpenuhi | R1, R8 |
| 2. Lab diterima → item `LABORATORY` per pemeriksaan | Terpenuhi (fakta sintetis, pemeriksaan nyata) | R2 |
| 3. Study diterima → item `RADIOLOGY`; pengulangan `InternalHospitalError` → `NotApplicable` | Terpenuhi (sumber sintetis) | R3, R3b |
| 4. Baris yang sama diproses dua kali → satu item | Terpenuhi | R4 |
| 5. Tarif tidak ada → `ReconciliationRequired` `TARIFF_NOT_FOUND`, tanpa item Rp0 | Terpenuhi | R5, R5b |
| 6. Kunjungan bukan Rawat Jalan → `NotApplicable` | Terpenuhi (IGD; tidak ada data rawat inap) | R6 |
| 7. Tanpa item `REGISTRATION`; biaya admin di `AdministrationFeeAmount` | Terpenuhi | R7 |
| 8. Kunjungan berasuransi → invoice terbentuk dan dihitung Billing | Terpenuhi | R8 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Log aplikasi: aturan keselamatan radiologi fail-closed dan HTTPS redirect port — perilaku yang sudah ada |
| Masalah yang diketahui | (a) Efek pembatalan, konsultasi, dan resep berstatus `Pending` tanpa jadwal; pekerja `BE-RJE-010` **harus** melewati kode `CANCELLATION_PENDING_SUPPORT` dan `SOURCE_PENDING_SUPPORT` sampai `BE-RJE-007`/`008`/`009` selesai, supaya tidak memprosesnya berulang tanpa hasil. (b) Efek dari sesi `BE-RJE-002` terbentuk sebelum penanda `Pending` ada, sehingga tetap `NotApplicable` |
| Risiko tersisa | Permintaan klinis kini ikut menunggu penerusan ke invoice (satu upsert tambahan) — dijaga jembatan yang tidak pernah melempar |
| Perubahan sampingan | `NONE` di repository. Data uji di database: bagian 5.2 |
| Interupsi | `NONE` |
| Status Git | 2 berkas `M`, 2 berkas `??` source; dokumen blueprint (laporan ini, roadmap, traceability, decision log). Belum di-stage atau di-commit |
| Langkah berikutnya | Gelombang 4 terbuka: `BE-RJE-005`, `BE-RJE-007`, `BE-RJE-010`, `BE-RJE-014`. Gelombang 1 yang tersisa: `BE-RJE-004`, `BE-RJE-006` |
