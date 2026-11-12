# Laporan Perubahan Backend — `BE-OPT-030`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-030` |
| Judul | Perluasan Billing untuk sumber Optik |
| Slice | `MVP-2` — `EPIC OPT-08` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-030` |
| Trace | `FR-OPT-080`, `081`; `OPT-DEC-028`, `029`, `042`, `043`; `INT-OPT-02`, `04`, `05`; `OPT-AC-025`; `UAT-18` |
| Contract version | `OPT-INT-v1` `INT-OPT-02`, `04`, `05` — `approved` sisi Optik; kontrak charge Billing `BIL-INTEGRATION-1.3` (tidak berubah) |
| Penyetuju modul lain | Billing `OPT-APR-001`, `002`, `011`, `012` ✅ — Yasmina (`OPT-DEC-051`, `OPT-EVD-002`), disampaikan Yoga Aji Pratama |
| Dependency | Tidak ada task Optik. Memakai model `OptRefractionExam`, `OptOrder`, `OptOrderItem` dari `BE-OPT-002` ✅ |
| Klasifikasi | `MEDIUM` — kebijakan dan pemetaan pada empat berkas Billing; tanpa endpoint, entity, atau migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/BillingManagement/{Operational/Constants,Billing/Services}/`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `d8fa6e40` (branch `yoga`), di atas perubahan `BE-OPT-003` yang belum di-commit |
| Tanggal | 2026-11-04 |
| Status | Selesai dan tervalidasi di harness. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement` (pemilik kebijakan, resolver, jembatan); Optik hanya dibaca |
| Registry | `Bil` / `ACTIVE`; `Opt` / `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` untuk keempat berkas Billing (hanya baris tambahan yang dinilai); `NEW CODE` untuk uji |
| QBE yang berlaku | `QBE-ENT-003` (harga tidak disimpan Optik; dibaca dari tarif), `QBE-SVC-001`, `QBE-VAL-001`, `QBE-CODE-002`/`003` (tanpa Count/Max+1) |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

Billing belum mengenal Optik sebagai sumber tagihan. Fakta dari Optik akan ditolak `BIL_SOURCE_INVALID`,
sehingga pemeriksaan dan barang tidak bisa masuk tagihan. Selain itu, kalau Optik memakai aturan sumber
lain, pasien yang melunasi di awal akan membuat tagihan terfinalisasi otomatis dan kunjungan tertutup
sebelum kacamatanya diserahkan (`OPT-DEC-043`).

---

## 2. Proses bisnis

**Contoh (alur `FLOW-OPT-MVP-001`):** Ibu Sari (samaran) diperiksa Rina (RO), lalu memesan frame merk A
Rp 450.000 dan lensa stok Rp 600.000. Pemeriksaan Rp 75.000.

1. **Pemeriksaan final → tagihan pemeriksaan.** Optik menerbitkan satu fakta `OpticalExamCharge`. Billing membaca tarif tindakan "Pemeriksaan Refraksi" (Rp 75.000) dan membuat satu baris tagihan berstatus `COMPLETED`.
2. **Pesanan ditagihkan → satu baris per barang.** Setiap baris pesanan menjadi fakta `OpticalItemCharge` berstatus `ORDERED`. Harganya dibaca dari tarif barang (`DrugId`), bukan dari Optik.
3. **Pesanan dikirim ke pengerjaan.** Fakta yang sama direvisi menjadi `IN_WORK`. Baris tagihan yang sama berubah status, tidak bertambah baris.
4. **Ibu Sari melunasi Rp 1.125.000 di awal.** Billing **tidak** memfinalisasi tagihan karena ada baris `IN_WORK` (`OrdersIncomplete`, `OPT-AC-025`, `UAT-18`). Kunjungan tetap terbuka.
5. **Kacamata diserahkan.** Baris menjadi `HANDED_OVER` dan dianggap selesai. Finalisasi tidak lagi tertahan oleh baris itu. Pemicunya dari Optik datang di `BE-OPT-040`.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Pesanan dibatalkan saat masih `ORDERED` (`INT-OPT-05`) | Baris tagihan di-void normal |
| Dibatalkan sesudah `IN_WORK`/`READY` | Tidak di-void normal; Billing mengajukan adjustment kredit yang menunggu persetujuan, sesuai aturan pembatalan klinis yang sudah ada |
| Tarif barang belum ada atau sudah kedaluwarsa | Fakta masuk antrean rekonsiliasi `TARIFF_NOT_FOUND`; tidak ada Rp 0 yang dikarang |
| Pemeriksaan belum final | Ditolak ke rekonsiliasi `SOURCE_REJECTED` |
| Baris pesanan tidak cocok dengan pesanannya | `SOURCE_NOT_FOUND` |
| Pesanan remake garansi | Tidak ditagih (`NOT_BILLABLE`) walaupun faktanya terlanjur terbit |
| `RuleSnapshot.milestone` di luar lima nilai yang sah | Rekonsiliasi `SOURCE_REJECTED`; tidak ada baris tagihan |
| Ejaan sumber lain (`Optik`, `optical`) atau jenis fakta modul lain | Tetap ditolak `BIL_SOURCE_INVALID` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`BillingSourceContract`, `ContractBillingChargeSourceAdapter` (`IsOrderComplete`, `IsNormallyVoidable`, `ValidateVoid`), `BillingSourceTariffResolver` (`FindTariffAsync`, `FindDrugTariffAsync`, `EffectiveTariffs`, `Build`), `BillingClinicalChargeBridgeService` (`BuildPlanAsync`, `BuildCancellationPlanAsync`, `MapDomain`, `MapBillableStatus`), `BillingFolioService.RecognizeMilestoneAsync`, `ClinicalMilestoneFactProducer`, `BillingFinalizationService` (`OrdersIncomplete`), `PatientBillingSummaryService.Breakdown` (pemetaan grup), `Program.cs` (DI Billing).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs` | **Diperbarui** (+28/−1). `Optical`, `OpticalExamCharge`, `OpticalItemCharge`; milestone `ExamFinalized`; daftar milestone barang yang sah; pendaftaran di gerbang kontrak |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingChargeSourceAdapter.cs` | **Diperbarui** (+19/−4). Kebijakan `OPTICAL`. Parameter opsional baru `pendingStatuses` pada kebijakan: kosong = aturan lama ("belum selesai" = "boleh void normal"), sehingga sumber lain tidak berubah |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSourceTariffResolver.cs` | **Diperbarui** (+70/−1). `ResolveOpticalAsync`. Ruas opsional `EffectType` pada `BillingTariffResolutionRequest` (bawaan `null`) |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingClinicalChargeBridgeService.cs` | **Diperbarui** (+57/−3). Sumber `Optical` masuk cakupan jembatan; domain `OPTICAL`; identitas butir (pemeriksaan per agregat, barang per baris); status dari `RuleSnapshot.milestone` (`TryReadOpticalStatus`); `EffectType` diteruskan ke resolver |
| `Tests/QuilvianSystemBackend.OpticalTests/Be030BillingOpticalTests.cs` | **Baru** (diabaikan Git; perlu `git add -f`). 21 uji: kontrak, kebijakan, regresi sumber lain, finalisasi, resolver |
| `Tests/QuilvianSystemBackend.OpticalTests/Be030BillingBridgeTests.cs` | **Baru** (diabaikan Git). 5 uji rantai penuh lewat container DI |
| `Tests/QuilvianSystemBackend.OpticalTests/Infrastructure/TestDatabase.cs` | **Diperbarui** (diabaikan Git). Tiruan bentuk `hashtext`/`pg_advisory_xact_lock` (sama dengan harness Farmasi) dan konverter `DateTimeOffset` khusus SQLite |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint baru atau berubah |
| Kontrak integrasi | `INT-OPT-02`/`04`/`05` sisi Billing terpenuhi. **Tafsiran yang perlu dikunci untuk `BE-OPT-031`/`032`:** status barang dibawa di `RuleSnapshot.milestone` dengan nilai `ORDERED`, `IN_WORK`, `READY`, `HANDED_OVER`, `NOT_COLLECTED`; pemeriksaan memakai `ExamFinalized`. Kontrak menyebut ruas `milestone` dan daftar status, tetapi tidak menautkan keduanya secara tertulis |
| Database | Tanpa migration. Billing hanya membaca tabel Optik (`OptRefractionExam`, `OptOrderItem`, `OptOrder.IsRemake`) |
| Keamanan/Auth | Tidak ada izin baru. Harga hanya dari `MstTariff` yang berlaku pada tanggal pelayanan; nilai harga dari Optik tidak pernah dibaca |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat atau mengubah endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Cek server lokal sebelum build | Tidak ada proses `dotnet`/`node` berjalan | `PASS` | `tasklist` |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` (membangun backend) | 155 lulus, 0 gagal (26 baru) | `PASS` | Keluaran `dotnet test` |
| Kriteria (1): sumber `Optical` diterima, ejaan/jenis fakta lain ditolak | Lulus | `PASS` | Uji `Kriteria_1_…` |
| Kriteria (2) / `OPT-AC-025` / `UAT-18`: baris `IN_WORK`/`READY` menahan finalisasi, `HANDED_OVER` tidak | `BIL-FIN-024` muncul tepat pada `IN_WORK`/`READY` | `PASS` | Uji `Kriteria_2_…` dengan `BillingFinalizationService` asli |
| Kriteria (3): tarif pemeriksaan dari tindakan, barang dari `DrugId` | Rp 75.000 dan Rp 450.000; jumlah dari fakta bila diisi | `PASS` | Uji resolver |
| Tarif tidak ada/kedaluwarsa, pemeriksaan belum final, baris salah pesanan, remake, jenis fakta asing | Masing-masing `TARIFF_NOT_FOUND`, `SOURCE_REJECTED`, `SOURCE_NOT_FOUND`, `NOT_BILLABLE`, ditolak | `PASS` | Uji resolver |
| Rantai penuh: fakta pemeriksaan → item invoice `OPTICAL`/`COMPLETED` Rp 75.000 | Efek `Synced` | `PASS` | `Be030BillingBridgeTests` (producer, folio, jembatan, `BillingInvoiceService` dari container DI) |
| Rantai penuh: `ORDERED` lalu `IN_WORK` | Satu baris invoice yang sama berpindah status | `PASS` | Uji rantai |
| Rantai penuh: milestone `SHIPPED` | `ReconciliationRequired` + `SOURCE_REJECTED`; tanpa item | `PASS` | Uji rantai |
| `INT-OPT-05`: batal saat `ORDERED` / sesudah `IN_WORK` | Void / tidak void (adjustment) | `PASS` | Uji rantai |
| Kriteria (4): regresi sumber lain | Kebijakan Lab, tindakan, resep tahap 1, ADHOC, BMHP, Radiologi tidak berubah | `PASS` | Uji `Kriteria_4_…` |
| Regresi `Tests/QuilvianSystemBackend.OperatingRoomTests` (memakai jembatan yang sama) | 142 lulus, 0 gagal | `PASS` | Keluaran `dotnet test` |
| Uji mutasi: `pendingStatuses` OPTICAL dikosongkan sementara | 5 uji gagal sebagaimana mestinya; berkas dipulihkan (diff kembali +19/−4) | `PASS` | Keluaran `dotnet test` |
| QBE checker `-Mode Strict -Path` atas 4 berkas Billing | `VIOLATION: 0`, `REVIEW: 0`, `PASS` | `PASS` | Keluaran checker |
| Kontrol negatif `ZzzRegistryProbe` (multi-baris) | Terkena QBE-ENT-001, QBE-CFG-001, QBE-MOD-002; probe dihapus | `PASS` | Keluaran checker. Percobaan pertama dengan probe satu baris tidak terkena, sehingga diulang dengan bentuk multi-baris |

**Tidak dijalankan / tidak dapat dijalankan:**
- `Tests/QuilvianSystemBackend.PharmacyTests` (memuat uji Billing lain) **gagal kompilasi sebelum task ini**: uji merujuk `BillingManagement…Constants` yang tidak ada, `EncounterStatus.InProgress`, `BilInvoice.PatientId`, dan `BilInvoiceItem.Amount`. Tidak satu pun terkait berkas task ini. Regresi Billing dibuktikan lewat uji Optik dan Kamar Operasi.
- Penguncian advisory PostgreSQL: harness SQLite hanya meniru bentuk fungsinya.
- Uji di basis data bersama (wewenang database tidak diberikan).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Fakta sumber `Optical` diterima; sumber tak terdaftar tetap ditolak `BIL_SOURCE_INVALID` | Terpenuhi | Uji kontrak + rantai penuh |
| (2) Item `IN_WORK` membuat `OrdersIncomplete` (`OPT-AC-025`, `UAT-18`) | Terpenuhi di harness | Uji finalisasi + uji mutasi |
| (3) Tarif barang dari `DrugId`, pemeriksaan dari tindakan | Terpenuhi di harness | Uji resolver + rantai penuh |
| (4) Regresi sumber lain tidak berubah | Terpenuhi | Uji kebijakan sumber lain; 142 uji Kamar Operasi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta cakupan | Roadmap menyebut tiga berkas Billing. Berkas keempat, `BillingClinicalChargeBridgeService`, wajib disentuh karena di sanalah sumber dipetakan ke domain invoice; tanpa itu fakta Optik berhenti sebagai `SOURCE_OUT_OF_SCOPE`. Perubahannya tetap di dalam persetujuan Billing `OPT-APR-001`/`011`/`012` |
| Untuk `BE-OPT-031`/`032` | Producer wajib mengisi `RuleSnapshot.milestone` seperti bagian 3.3. Pemeriksaan: `SourceAggregateId` = pemeriksaan, `SourceItemId` = id tindakan "Pemeriksaan Refraksi". Barang: `SourceAggregateId` = pesanan, `SourceItemId` = baris |
| Masalah yang diketahui | Grup tampilan ringkasan tagihan pasien untuk domain `OPTICAL` jatuh ke grup bawaan "Tindakan" kecuali kategori tarifnya bertanda farmasi/penunjang. Bisa diatur lewat konfigurasi `PatientBillingSummary:GroupMapping:OPTICAL` tanpa kode. Belum ada keputusan grup mana yang diinginkan |
| Risiko tersisa | Penagihan ke tagihan rawat jalan belum dicoba di PostgreSQL. Penerapan migration Optik (`BE-OPT-002`, `003`) ke devYoga tetap menunggu izin |
| Perubahan sampingan | `NONE` pada source aplikasi. Harness uji Optik diperluas (bagian 3.2). Probe QBE sementara dibuat dan dihapus |
| Status Git | `M` empat berkas `Areas/HealthServices/BillingManagement/…`, roadmap dan traceability Optik; `??` laporan ini. Berkas uji di `Tests/` diabaikan Git. Perubahan `BE-OPT-003` masih ada di working tree |
| Langkah berikutnya | `BE-OPT-033` (jenis dokumen stok Optik, penyetuju Ikbal; bagian pengembalian menunggu `OPT-OQ-015`), lalu `BE-OPT-031` (tarif pemeriksaan sekali saat final) yang kini bisa langsung memakai jalur Billing ini |
