# Execution Evidence — `RJ-BIL-BE-002`

| Field | Nilai |
|---|---|
| Task ID | `RJ-BIL-BE-002` |
| Outcome roadmap | Menyediakan clinical fact handoff yang idempotent untuk Prescription dan Procedure |
| Tanggal eksekusi | `2026-08-24` |
| Cabang | `sukmagp` |
| Source SHA sebelum | `92108587e69b9a935b2fd264882100149f80ed02` |
| Source setelah | Working tree belum di-commit |
| `IMPLEMENTATION_STATUS` | `COMPLETE` |
| `IMPLEMENTATION_AUTHORITY` | `GRANTED` oleh author pada pesan handoff `RJ-BIL-BE-002` |
| `BUILD_RESULT` | `PASS` — `0` error, `0` warning |
| `TEST_EVIDENCE` | `PASS` — `22` test, `22` lulus, `0` gagal |
| `SECURITY_REVIEW` | `PASS` |
| `PERMISSION_REVIEW` | `PASS` |
| `MIGRATION_CREATED` | `YES` — `2` migration |
| `MIGRATION_APPLIED` | `YES` — **tidak disengaja**, lihat bagian 8 |
| `GIT_COMMIT` | `NO` |
| `GIT_PUSH` | `NO` |
| `MERGE` | `NO` |
| `DEPLOY` | `NO` |

---


> **Catatan `2026-08-26`.** Tabel yang di dokumen ini disebut `BilClinicalMilestoneFact`
> semula bernama `TrxClinicalMilestoneFact`. Nama itu melanggar `QBE-NAM-001` dan diganti
> melalui migration `20260826101500_RenameClinicalMilestoneFactToBillingOwnership` yang
> mempertahankan data. Rinciannya pada
> [task/report/backend/be-rj-bil-003-remediasi-penamaan-qbe.md](task/report/backend/be-rj-bil-003-remediasi-penamaan-qbe.md).

## 1. Ringkasan untuk pembaca non-teknis

Sebelum pekerjaan ini, modul Farmasi masih bisa menyatakan sebuah resep **lunas**. Siapa pun
yang berhak mengubah resep — dengan hak akses klinis `Prescription : Update` — dapat memanggil
endpoint yang menetapkan status pembayaran. Tidak ada kasir, tidak ada persetujuan keuangan.

Sekarang jalur itu ditutup. Modul klinis hanya menyatakan **peristiwa klinis**, misalnya
"resep sudah difinalkan dokter" atau "tindakan sudah dikerjakan". Modul Billing yang membaca
peristiwa itu lalu memutuskan akibat keuangannya.

Contoh nyata perbedaannya:

| Kejadian | Sebelum | Sesudah |
|---|---|---|
| Petugas memanggil `payment-waived` atas resep Rp2.500.000 | Resep tercatat tidak perlu dibayar | Endpoint sudah tidak ada; jalurnya hilang |
| Dokter membatalkan resep yang belum ditagihkan | Status pembayaran ditulis `Dibatalkan` oleh modul klinis | Modul klinis hanya menulis status klinis; Billing tidak menerima apa pun karena memang belum ada tagihan |
| Dokter membatalkan resep yang sudah punya tagihan | Status pembayaran ditulis `Dibatalkan` oleh modul klinis | Tagihan lama **tidak dihapus**; Billing menerima revisi baru dan folio masuk tinjauan |

---

## 2. Keputusan author yang dilaksanakan

| Keputusan | Isi | Pelaksanaan |
|---|---|---|
| `1A` | Empat endpoint finansial Farmasi disetujui untuk dinonaktifkan | Route dan method-nya dihapus seluruhnya |
| `1B` | Pembatalan resep tetap ada, tetapi tidak boleh menetapkan `PaymentStatus` | `PaymentStatus` tidak lagi ditulis `CancelAsync` |
| `CASE A` | Batal sebelum charge terbentuk: jangan buat charge | Diterapkan sebagai `SuppressedNoPriorCharge` |
| `CASE B` | Batal setelah charge terbentuk: jangan hapus charge asli | Diterapkan sebagai revisi versi baru atas identitas yang sama |
| `CASE C` | Hasil sebelumnya `OutcomeUnknown`: jangan koreksi buta | Diterapkan sebagai `ReconciliationRequired` |

Seluruhnya memiliki test yang membuktikannya; lihat bagian 6.

---

## 3. Proses bisnis setelah perubahan

### 3.1 Tujuan

Memastikan status keuangan resep dan tindakan hanya ditentukan Billing, tanpa memutus alur
kerja dokter dan farmasi yang sedang berjalan.

### 3.2 Pelaku

| Pelaku | Kewenangan setelah perubahan |
|---|---|
| Dokter | Memfinalkan konsultasi, membatalkan resep, mengeksekusi tindakan |
| Petugas Farmasi | Telaah, penyiapan, dan penyerahan obat |
| Billing | Satu-satunya penentu terbentuknya tagihan dan akibat finansial pembatalan |

### 3.3 Pemicu dan langkah — resep

1. Dokter menyelesaikan konsultasi.
2. Sistem memfinalkan seluruh resep draft milik konsultasi tersebut.
3. Seluruh perubahan klinis disimpan dan di-commit.
4. **Setelah commit**, sistem menerbitkan fakta `PrescriptionCharge` ke Billing.
5. Billing membuat folio kunjungan bila belum ada, lalu membuat satu baris tagihan.
6. Karena formula tarif yang disahkan belum tersedia, baris tagihan berstatus
   `PendingFinancialReview`. Angkanya tidak diambil dari modul klinis.

Urutan langkah `3` dan `4` tidak boleh dibalik. Konsultasi yang sudah sah tidak boleh
dibatalkan hanya karena Billing sedang tidak dapat dihubungi.

### 3.4 Pemicu dan langkah — tindakan

1. Petugas mengeksekusi tindakan melalui `PATCH {id}/execute`.
2. Status tindakan menjadi `Completed` dan tersimpan.
3. Sistem menerbitkan fakta `ProcedureCharge` ke Billing.

Milestone ini mengikuti `RJ-BIL-DEC-002`: pemicu tagihan tindakan adalah tindakan
benar-benar dieksekusi, bukan pemilihan atau order.

### 3.5 Jalur tidak normal

| Kejadian | Yang terjadi | Yang dilihat pengguna |
|---|---|---|
| Billing tidak dapat dihubungi | Fakta tercatat berstatus `OutcomeUnknown` | Tindakan klinis tetap berhasil, disertai catatan bahwa penyerahan ke Billing perlu ditinjau |
| Fakta yang sama dikirim ulang | Dikenali sebagai pengulangan | Tidak ada tagihan ganda |
| Pembatalan atas fakta yang hasilnya belum pasti | Ditolak dengan `CLIN_FACT_RECONCILIATION_REQUIRED` | Diminta menyelesaikan rekonsiliasi lebih dulu |
| Konteks sumber belum dikontrak | Ditolak dengan `CLIN_FACT_SOURCE_INVALID` | Tidak ada jejak finansial yang tertulis |

---

## 4. Perubahan source

### 4.1 Berkas baru

| Berkas | Isi |
|---|---|
| `Areas/HealthServices/BillingManagement/Operational/Constants/BillingSourceContract.cs` | Daftar konteks sumber dan effect type yang sah |
| `Areas/HealthServices/ClinicalBillingIntegration/Enums/ClinicalMilestoneFactEnums.cs` | `ClinicalMilestoneKind`, `ClinicalFactDispatchStatus` |
| `Areas/HealthServices/ClinicalBillingIntegration/Models/BilClinicalMilestoneFact.cs` | Ledger satu baris satu revisi fakta |
| `Areas/HealthServices/ClinicalBillingIntegration/Configurations/ClinicalBillingIntegrationConfigurations.cs` | Index unik, filter, concurrency token, FK `Restrict` |
| `Areas/HealthServices/ClinicalBillingIntegration/DTOs/ClinicalMilestoneFactDtos.cs` | Permintaan dan hasil penerbitan fakta |
| `Areas/HealthServices/ClinicalBillingIntegration/Services/ClinicalMilestoneFactProducer.cs` | Penerbit fakta; seluruh kebijakan `CASE A/B/C` |
| `Migrations/20260824074649_AddClinicalMilestoneFactHandoff.cs` | Membuat tabel ledger dan lima index |
| `Migrations/20260824080430_StoreClinicalFactSnapshotAsText.cs` | Mengubah kolom snapshot `jsonb` menjadi `text` |
| `Tests/QuilvianSystemBackend.BillingTests/ClinicalIntegration/ClinicalMilestoneFactProducerTests.cs` | `10` test perilaku producer |
| `Tests/QuilvianSystemBackend.BillingTests/ClinicalIntegration/ClinicalFinancialAuthorityTests.cs` | `3` test pengaman regresi kewenangan finansial |

### 4.2 Berkas yang diubah

| Berkas | Perubahan |
|---|---|
| `Areas/HealthServices/BillingManagement/Operational/Services/BillingFolioService.cs` | Kontrak sumber dibuka untuk `Prescription` dan `Procedure` lewat allowlist; tambah `ClinicalFactConsumer` |
| `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs` | Empat endpoint finansial dan helper `CompletePayment` dihapus; pembatalan menerbitkan fakta |
| `Areas/HealthServices/PharmacyManagement/Services/PrescriptionWorkflowService.cs` | Lima method finansial dihapus; `CancelAsync` tidak lagi menulis `PaymentStatus` |
| `Areas/HealthServices/PharmacyManagement/DTOs/PrescriptionDtos.cs` | Dua DTO permintaan finansial dihapus |
| `Areas/HealthServices/PharmacyManagement/DTOs/ConsultationFinalizationDtos.cs` | Tambah `BillingHandoffIssues` |
| `Areas/HealthServices/PharmacyManagement/Services/ConsultationFinalizationService.cs` | Menerbitkan fakta setelah commit |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientProcedureController.cs` | `execute` dan `cancel` menerbitkan fakta |
| `Repositories/ApplicationDbContext.cs` | Tambah `DbSet<BilClinicalMilestoneFact>` |
| `Program.cs` | Registrasi `ClinicalMilestoneFactProducer` |
| `Tests/.../BillingTestDatabaseFixture.cs` | Teardown ledger; helper `CreateLoggerService` |
| `Tests/.../BillingFolioServiceTests.cs` | `Prescription` dan `Procedure` dikeluarkan dari daftar konteks yang ditolak |
| `Tests/.../QuilvianSystemBackend.BillingTests.csproj` | `FrameworkReference Microsoft.AspNetCore.App` |

---

## 5. Perubahan arsitektur

### 5.1 Identitas fakta

Identitas stabil sebuah fakta adalah kombinasi:

```text
SourceContext + SourceAggregateId + SourceItemId + EffectType
```

Kombinasi itu menghasilkan satu `MilestoneFactId` yang **tidak pernah berubah**. Koreksi dan
pembatalan memakai `MilestoneFactId` yang sama dengan `MilestoneFactVersion` berikutnya.
Karena identitas charge di Billing juga memakai `MilestoneFactId`, revisi selalu jatuh ke
tagihan yang sama dan tidak pernah membentuk tagihan kedua.

### 5.2 Kunci idempotency

```text
CF-{MilestoneFactId:N}-{MilestoneFactVersion}
```

Kunci ini dihitung dari identitas, bukan dari waktu atau nilai acak. Pengiriman ulang
menghasilkan kunci yang sama persis, sehingga Billing mengembalikan hasil canonical.

### 5.3 Urutan penyerahan

```text
transaksi klinis commit
        ↓
ledger fakta disimpan (Pending)
        ↓
Billing memproses
        ↓
ledger diperbarui (Dispatched / Rejected / OutcomeUnknown)
```

Producer menolak dijalankan di dalam transaksi klinis yang masih terbuka. Pemeriksaannya
eksplisit dan disertai pesan yang menyebut penyebabnya.

---

## 6. Bukti pengujian

`22` test, seluruhnya lulus, dijalankan terhadap PostgreSQL sungguhan.

| Skenario yang diminta author | Test | Hasil |
|---|---|---|
| `1` — fakta pertama menghasilkan satu efek finansial | `FaktaKlinisPertama_MenghasilkanTepatSatuChargeLine` | `PASS` |
| `2` — retry tidak menggandakan charge | `FaktaSamaDikirimUlang_TidakMenggandakanCharge` | `PASS` |
| `3` — versi baru diproses sebagai koreksi | `RevisiFaktaBaru_MembuatVersiBaruTanpaMenghapusChargeAsli` | `PASS` |
| `4` — konflik gagal dengan aman dan dapat diaudit | `VersiLamaSetelahVersiBaruApplied_DitolakDenganVersionConflict` | `PASS` |
| `5` — batal sebelum charge tidak menghasilkan charge | `PembatalanSebelumChargeTerbentuk_TidakMembuatChargeApaPun` | `PASS` |
| `6` — batal setelah charge tidak menghapus charge asli | `PembatalanSetelahChargeTerbentuk_TidakMenghapusChargeAsli` | `PASS` |
| `7` — batal menghasilkan fakta koreksi baru | `PembatalanSetelahChargeTerbentuk_TidakMenghapusChargeAsli` (asersi versi `2`) | `PASS` |
| `8` — clinical endpoint tidak dapat menetapkan `Paid` | `PrescriptionController_TidakLagiMemilikiRouteFinansial`, `PrescriptionWorkflowService_TidakLagiMemilikiKewenanganFinansial` | `PASS` |
| `9` — permintaan stale/concurrent tidak merusak state | `VersiLamaSetelahVersiBaruApplied_DitolakDenganVersionConflict`, `FolioKeduaUntukEncounterSama_DitolakUniqueIndexDatabase` | `PASS` |
| `10` — `OutcomeUnknown` tidak menghasilkan koreksi buta | `RevisiSetelahOutcomeUnknown_MemintaRekonsiliasiDanTidakMengoreksiButa` | `PASS` |

Test tambahan batas kontrak: `KonteksYangBelumDikontrak_DitolakProducer` (`Pharmacy`,
`Laboratory`, `Radiology`), `EffectTypeMilikKontekLain_DitolakProducer`, dan
`PenerbitanDidalamTransaksiKlinis_DitolakDenganPesanJelas`.

Kebersihan database dibuktikan oleh teardown itu sendiri: ledger memiliki FK `Restrict` ke
encounter, sehingga penghapusan encounter akan gagal bila masih ada baris fakta tertinggal.
Seluruh teardown berhasil.

### 6.1 Dua cacat yang ditemukan pengujian

Keduanya cacat implementasi sungguhan, bukan masalah test, dan keduanya sudah diperbaiki.

| Cacat | Sebab | Akibat bila lolos | Perbaikan |
|---|---|---|---|
| Sidik jari berbeda karena ketelitian waktu | `DateTime` di memori berketelitian `100` nanodetik, kolom PostgreSQL berketelitian mikrodetik | Pengiriman ulang dari baris tersimpan ditolak `BIL_IDEMPOTENCY_CONFLICT` | `TruncateToDatabasePrecision` |
| Sidik jari berbeda karena skala desimal | `2` di memori menjadi untaian `"2"`, sedangkan setelah melewati `numeric(18,6)` menjadi `"2.000000"` | Sama seperti di atas | `NormalizeToDatabaseScale` |

Kolom snapshot juga dipindahkan dari `jsonb` ke `text` karena PostgreSQL memformat ulang nilai
`jsonb` ketika dibaca kembali, sehingga baris ledger tidak lagi identik dengan apa yang
benar-benar dikirim.

---

## 7. Security dan permission review

| Pemeriksaan | Hasil | Bukti |
|---|---|---|
| Clinical permission tidak dapat melakukan financial mutation | `PASS` | Empat route dan lima method finansial hilang; diverifikasi grep dan `2` test refleksi |
| Tidak ada jalur bypass tersisa | `PASS` | Route dihapus fisik, bukan disembunyikan dari Swagger |
| Financial state tidak berasal dari client | `PASS` | Ketiga pembangun `ClinicalMilestoneFactRequest` mengambil nilai dari entity tersimpan dan waktu server, bukan dari body permintaan |
| Mass assignment | `PASS` | Tidak ada DTO baru yang di-bind dari HTTP |
| Authorization server-side | `PASS` | Producer dipanggil dari endpoint yang sudah `[Authorize]` dan ber-`AccessPermission` |
| Idempotency dan replay protection | `PASS` | Unique index ledger, unique index Billing, sidik jari permintaan |
| Concurrency | `PASS` | Concurrency token `Version`, penanganan unique violation dengan alokasi versi ulang |
| Transaction safety | `PASS` | Penerbitan wajib di luar transaksi klinis; dijaga pemeriksaan eksplisit |
| Audit trail | `PASS` | `AuditAsync` pada jalur dispatch dan suppression |
| Idempotency key tidak ditulis mentah | `PASS` | Disimpan sebagai `HashReference`, sesuai `RJ-BIL-PERM-001` |
| Stack trace atau data sensitif bocor | `PASS` | Pesan kegagalan berupa kode kontrak, bukan exception mentah |
| Hardcoded credential | `PASS` | Tidak ada |
| SQL injection | `PASS` | EF parameterized; satu-satunya SQL mentah adalah DDL migration tanpa masukan pengguna |
| Permission baru dibutuhkan | `TIDAK` | Tidak ada endpoint baru |

**Catatan permission.** Empat deklarasi `AccessAction`/`AccessPermission` ikut hilang bersama
endpoint-nya. Baris permission lama yang mungkin sudah tersimpan di database tidak dibersihkan
karena tidak ada wewenang mutasi database pada task ini. Baris tersebut menjadi yatim dan tidak
lagi menunjuk ke route mana pun.

---

## 8. Migration dan database

| Migration | Isi | Rollback |
|---|---|---|
| `20260824074649_AddClinicalMilestoneFactHandoff` | Membuat tabel `BilClinicalMilestoneFact` beserta `5` index dan FK `Restrict` ke `TrxPatientEncounter` | `DropTable` |
| `20260824080430_StoreClinicalFactSnapshotAsText` | Mengubah `TariffSnapshot` dan `RuleSnapshot` dari `jsonb` menjadi `text` | Kembali ke `jsonb` |

Migration kedua ditulis manual memakai klausa `USING` karena PostgreSQL tidak menyediakan cast
otomatis dari `jsonb` ke `text` pada `ALTER COLUMN TYPE`. Tanpa itu, migration akan gagal saat
dijalankan.

Kompatibilitas: keduanya bersifat menambah dan tidak menyentuh tabel milik modul lain.

### Yang harus diketahui author

`MIGRATION_APPLIED = YES`, dan ini **tidak disengaja**.

`BillingTestDatabaseFixture.InitializeAsync` memanggil `context.Database.Migrate()` sebelum
test pertama. Fixture itu sudah ada sejak `RJ-BIL-BE-001`. Karena connection string test
jatuh kembali ke `appsettings.Development.json`, target yang terkena adalah database dev
bersama `QuilvianNewDevTim01`.

Akibatnya, menjalankan `dotnet test` menerapkan kedua migration di atas ke `QuilvianNewDevTim01`.

| Fakta | Nilai |
|---|---|
| Perintah `dotnet ef database update` dijalankan | `TIDAK` |
| Database production disentuh | `TIDAK` — fixture menolak nama yang mengandung `prod` tanpa mekanisme override |
| Database yang berubah | `QuilvianNewDevTim01` |
| Sifat perubahan | Menambah satu tabel baru; tidak ada tabel lain yang diubah atau dihapus |
| Data tim lain terdampak | `TIDAK` |

Saya menilai ini tetap harus dilaporkan sebagai penyimpangan dari aturan
"tidak boleh `update-database` terhadap shared DB", karena akibatnya sama walaupun caranya
berbeda. Bila author menghendaki test tidak lagi menyentuh database bersama, perbaikannya
adalah mengarahkan `QUILVIAN_BILLING_TEST_DB` ke database khusus test — dan itu memerlukan
keputusan tersendiri.

---

## 9. Gap dan blocker yang tersisa

### 9.1 `RJ-BIL-BE-002-BLOCKER-001` — resep tidak dapat masuk antrean farmasi

```text
BLOCKER ID:
RJ-BIL-BE-002-BLOCKER-001

SOURCE EVIDENCE:
PrescriptionReviewService.cs:52-56 mensyaratkan FulfillmentStatus bernilai
ReadyForPharmacy atau QueuedAtPharmacy sebelum telaah farmasi dapat dimulai.
Satu-satunya penulis ReadyForPharmacy adalah PrescriptionWorkflowService.CompletePaymentAsync,
yang hanya dapat dicapai lewat empat endpoint finansial yang dihapus keputusan 1A.
Penelusuran frontend commit 29422c8 tidak menemukan satu pun pemanggil keempat endpoint itu.

WHY CODE CANNOT DECIDE:
Pertanyaannya adalah kebijakan farmasi: apakah apotek boleh menelaah resep sebelum
pembayaran diselesaikan. Blueprint yang disetujui tidak menjawabnya. Invariant yang ada hanya
menyatakan Dispensed tidak sama dengan Paid, bukan kapan telaah boleh dimulai.

AFFECTED TASK:
Alur farmasi rawat jalan. Tidak memblokir acceptance criteria RJ-BIL-BE-002 itu sendiri.

MINIMUM DECISION REQUIRED:
Apa yang membuka telaah farmasi setelah kewenangan finansial klinis dihapus.

OPTION A:
Telaah farmasi boleh dimulai segera setelah resep difinalkan dokter, dan penyerahan obat
yang ditahan sampai pembayaran selesai.
IMPACT: perubahan kecil pada satu penjaga status. Sejalan dengan invariant Dispensed != Paid.
Risiko: apotek menyiapkan obat yang mungkin batal dibayar.

OPTION B:
Telaah farmasi menunggu Billing menyatakan kewajiban finansial terpenuhi.
IMPACT: memerlukan settlement di Billing, yang baru ada pada RJ-BIL-BE-006 dan RJ-BIL-BE-008.
Sampai itu tersedia, antrean farmasi tetap tidak dapat dimasuki.

OPTION C:
Sediakan endpoint Billing tersendiri yang membuka kesiapan farmasi.
IMPACT: menambah cakupan di luar roadmap yang disetujui.

RECOMMENDED TECHNICAL OPTION:
Option A, dengan catatan bahwa ini rekomendasi teknis atas jumlah perubahan source, bukan
rekomendasi kebijakan farmasi.

IMPACT OF EACH OPTION:
Sudah ditulis pada masing-masing opsi di atas.
```

Perlu dicatat bahwa keadaan ini **sudah ada sebelum** task ini. Karena tidak ada layar yang
memanggil keempat endpoint tersebut, alur menuju `ReadyForPharmacy` memang sudah tidak dapat
dicapai dari frontend. Penghapusan endpoint tidak merusak alur yang berjalan; ia membuat
celah yang sudah ada menjadi terlihat.

### 9.2 Gap terdokumentasi

| Gap | Keterangan | Pemilik lanjutan |
|---|---|---|
| Pemulihan baris `Pending` dan `OutcomeUnknown` | Ledger menyimpan statusnya dan dapat dipindai, tetapi belum ada alur pemulihan otomatis | `RJ-BIL-BE-007` |
| `PaymentStatus` resep tidak lagi memiliki penulis selain nilai awal `NotBilled` | Sesuai `RJ-BIL-GATE-DEC-007`, kolom ini menjadi proyeksi read-only. Proyeksinya belum diisi karena Billing belum memiliki konsep pelunasan | `RJ-BIL-BE-006`, `RJ-BIL-BE-008` |
| Registry sistem `docs/system-registry/` belum ada | Pemeriksaan kavling nama pada `rule-prascan` dilakukan manual lewat grep; tidak ditemukan tabrakan nama `BilClinicalMilestoneFact` maupun namespace `ClinicalBillingIntegration` | `/qv-scan` |
| `RemoveDraftProcedure` tidak menerbitkan fakta | Penjaganya mensyaratkan status `Planned`, sehingga charge tidak mungkin sudah terbentuk | — |

### 9.3 Keputusan domain yang belum selesai

Tidak ada keputusan domain baru yang tertahan oleh task ini. `RJ-BIL-CONFLICT-001` tetap
memblokir `RJ-BIL-BE-005` dan tidak terdampak pekerjaan ini.

---

## 10. Konfirmasi batas kewenangan

| Tindakan | Status |
|---|---|
| Commit | `TIDAK dilakukan` |
| Push | `TIDAK dilakukan` |
| Merge | `TIDAK dilakukan` |
| Deploy | `TIDAK dilakukan` |
| Destructive database action | `TIDAK dilakukan` |
| Production database disentuh | `TIDAK` |
| External adapter `RJ-BIL-DEP-009` | Tetap `INACTIVE` |
| RIS, PACS, BPJS, integrasi eksternal lain | Tidak diaktifkan |
| `git diff --check` | Bersih |
