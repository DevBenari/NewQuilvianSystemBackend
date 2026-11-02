# Laporan Perubahan Backend — `BE-FIN-120`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-120` |
| Judul | Pembayaran supplier hanya melunasi utang milik supplier penerimanya, dan hanya penyetuju yang sesuai jenjang nominal yang dapat menyetujui pembayaran |
| Slice | `REV-22` Slice A — Finance AP mengikuti BLM |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22" |
| Trace | `AP-UAT-14`, `AP-UAT-15` (dokumen proses bisnis AP BLM, 26 Oktober 2026); `FIN-DEC-022`, `FIN-DEC-052`; `FIN-VAL-052`, `FIN-VAL-096` |
| Contract version | `FIN-VAL-1.13` (approved) tidak diedit. `FIN-VAL-052` dan `FIN-VAL-096` dipenuhi; satu aturan baru (supplier alokasi sama dengan penerima, `422`) dicatat sebagai delta pada roadmap `REV-22` |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 7: repository 0; berkas diperiksa 1 (sekitar 12); berkas diubah 1 (2 source + 3 test + 1 helper); logika bisnis 1; kontrak API 1 (memakai kontrak yang ada, menambah kode `403`); database 1 (hanya perilaku query yang ada); keamanan/auth 2 (inti: penegakan jenjang persetujuan); UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`: `Areas/Corporate/FinanceManagement/Payable/`, `Tests/QuilvianSystemBackend.PharmacyTests/`, `docs/module-blueprints/finance-management/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `48011b09` (branch `Yasmina`). Working tree sudah memuat perubahan Billing milik pengguna yang tidak disentuh task ini |
| Tanggal | 2026-10-26 |
| Status | 🟡 **SEBAGIAN** — seluruh kriteria terpetakan ke source dan test; `dotnet build` dan test otomatis **NOT RUN** |

---

## 1. Masalah yang diperbaiki

**Masalah 1 — utang supplier lain dapat dilunasi lewat pembayaran ke supplier yang salah.** Saat menyusun atau mengubah draft pembayaran,
`FinancePaymentService` memeriksa status dan sisa utang, tetapi tidak pernah membandingkan supplier pemilik utang dengan penerima pembayaran
(`PayeeReferenceId`). Kata `SupplierId` bahkan tidak muncul di seluruh berkas service itu.

Contoh: faktur `INV-A-1` milik Supplier A bernilai Rp 1.000.000. Petugas menyusun pembayaran dengan penerima Supplier B dan mengalokasikannya
ke utang itu. Sebelum perbaikan, draft tersimpan; setelah disetujui dan ditandai lunas, pembayaran tercatat atas nama Supplier B sementara
utang Supplier A berkurang.

**Masalah 2 — jenjang persetujuan nominal tidak ditegakkan.** Kontrak `FIN-VAL-052` mewajibkan penyetuju pembayaran sesuai `ApprovalTier`
(`403`). `ApproveAsync` hanya menolak pengaju yang menyetujui dirinya sendiri, sehingga pembayaran Rp 60.000.000 (`TIER_2`) dapat disetujui oleh
siapa pun yang memegang permission `Approve`, termasuk yang tidak berperan Manajer Finance. Purchase Order dan Purchasing Invoice sudah
menegakkannya; pembayaran, yang justru mengeluarkan uang, belum.

**Masalah 3 — jenis pembayaran tidak dicocokkan dengan jenis utang.** `FIN-VAL-096` mewajibkan `400` bila `PaymentType = MEDICAL_SERVICE`
tetapi alokasinya menunjuk utang supplier. Kode tidak memeriksanya.

---

## 2. Proses bisnis

**Pelaku:** petugas AP (menyusun), penyetuju Finance (menyetujui), Treasury (menandai lunas).

**Alur sesudah perbaikan:**

1. Petugas AP menyusun draft pembayaran: jenis `SUPPLIER`, penerima = supplier, rekening sumber, dan alokasi ke satu atau beberapa utang.
2. Untuk setiap alokasi utang supplier, sistem kini memeriksa berurutan: jenis pembayaran harus `SUPPLIER` (`400`), utang harus milik
   penerima (`422`), utang belum `PAID`/`CANCELLED`, dan nilai alokasi tidak melebihi sisa utang.
3. Pemeriksaan yang sama berlaku saat draft diubah (`PUT`). Bila gagal, alokasi lama tidak berubah.
4. Petugas mengajukan draft; sistem menetapkan `ApprovalTier` dari total (di bawah Rp 50.000.000 = `TIER_1`, selebihnya `TIER_2`).
5. Penyetuju menyetujui. Urutan pemeriksaan: status harus `SUBMITTED`; pengaju tidak boleh menyetujui sendiri (`422`); lalu penyetuju harus
   berwenang untuk jenjang itu (`403`).
6. Treasury menandai lunas (tidak berubah).

**Contoh berangka:**

| Skenario | Hasil |
| --- | --- |
| Utang Supplier A Rp 1.000.000, penerima Supplier B | Ditolak `422` "Utang ... milik supplier lain dan tidak dapat dibayarkan kepada penerima pembayaran ini." |
| `MEDICAL_SERVICE` dengan alokasi utang supplier | Ditolak `400` "Jenis pembayaran tidak cocok dengan utang yang dipilih." |
| Pembayaran Rp 60.000.000 (`TIER_2`), penyetuju Supervisor Finance | Ditolak `403`, status tetap `SUBMITTED` |
| Pembayaran Rp 60.000.000 (`TIER_2`), penyetuju Manajer Finance | Disetujui |
| Pembayaran Rp 10.000.000 (`TIER_1`), penyetuju Supervisor Finance | Disetujui |
| Pembayaran Rp 10.000.000 (`TIER_1`), penyetuju tanpa role Finance | Ditolak `403` |
| Pengaju yang berperan Manajer Finance menyetujui pembayarannya sendiri | Ditolak `422` (self-approval diperiksa sebelum jenjang) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` (Create, Update, Submit, Approve, AddReturnDeposit, validator)
- `Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs`
- `Areas/Corporate/FinanceManagement/Payable/Services/FinanceApprovalAuthorizationService.cs` dan `FinanceApprovalTierResolver.cs`
- `Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchasingInvoiceService.cs` (pola penegakan jenjang) dan `FinanceSupplierReturnService.cs` (`ReserveAsync`, pencocokan supplier deposit)
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` dan `state-transition-matrix.md`
- `QuilvianSystemFrontendDev/src/components/view/finance/payable/payment/finance-payment-ap-view.jsx` (konsumen; referensi read-only)
- `Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/TestDatabase.cs` dan test Finance/Billing sebagai pola

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` | (1) `FinanceApprovalAuthorizationService` disuntikkan sebagai parameter konstruktor terakhir. (2) `CreateDraftAsync` dan `UpdateDraftAsync` memanggil `EnsureSupplierAllocationMatchesPayee` untuk setiap alokasi utang supplier. (3) `ApproveAsync` memanggil `CanApproveAsync` setelah pemeriksaan self-approval dan melempar `PaymentForbiddenException` bila tidak berwenang; bila `ApprovalTier` kosong (kolom nullable, baris lama), jenjang dihitung ulang dari `TotalAmount` lewat `FinanceApprovalTierResolver`. (4) Helper `EnsureSupplierAllocationMatchesPayee` baru. (5) Kelas `PaymentForbiddenException` baru |
| `Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs` | `PaymentForbiddenException` dipetakan ke `403` pada `Failure` dan `IsHandled`; `Approve` mendapat `ProducesResponseType` untuk `200`, `403`, `422` |
| `Tests/QuilvianSystemBackend.PharmacyTests/FinancePaymentSupplierConsistencyTests.cs` | Baru: 4 test supplier alokasi dan jenis pembayaran |
| `Tests/QuilvianSystemBackend.PharmacyTests/FinancePaymentApprovalTierTests.cs` | Baru: 5 test jenjang persetujuan |
| `Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/FinanceApSeed.cs` | Baru: helper data awal bersama (dipakai juga `BE-FIN-121`) |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Bagian "AMENDMENT ROADMAP REVISI 22" (dipakai bersama `BE-FIN-121`) |

Tidak ada perubahan pada registrasi DI: `FinanceApprovalAuthorizationService` sudah terdaftar (`AddScoped`), dan `FinancePaymentService` tidak
pernah dibentuk manual di source mana pun, sehingga penambahan parameter konstruktor aman.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint, DTO, atau route baru. `POST /payments/{id}/approve` kini dapat menjawab `403` (aditif). `POST /payments` dan `PUT /payments/{id}` kini dapat menjawab `422` karena supplier berbeda, dan `400` karena jenis pembayaran tidak cocok. Konsumen frontend `finance-payment-ap-view.jsx` mengirim `paymentType: "SUPPLIER"` dengan `payeeReferenceId = supplierId` utang yang dipilih, sehingga tidak terdampak |
| Database | `NOT APPLICABLE` — nol entity, tabel, kolom, dan migration. Hanya membaca `FinSupplierPayable.SupplierId` yang sudah ada |
| Keamanan/Auth | Menambah penegakan di backend. Metadata `[AccessAction]`/`[AccessPermission]` pada `Approve` tidak berubah (`FinancePayment`/`Approve`, cocok dengan `ControllerName`). Kewenangan jenjang memakai `FinanceApprovalAuthorizationService` yang sudah dipakai PO dan Purchasing Invoice. **Efek operasional:** pembayaran `TIER_2` tidak dapat disetujui bila belum ada pengguna ber-role `Manajer Finance` |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Payment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/finance-management/payments` | Menyusun draft pembayaran; kini menolak utang milik supplier lain (`422`) dan jenis pembayaran yang tidak cocok (`400`) | `FinancePayment : Create` |
| `PUT` | `/api/v1/corporate/finance-management/payments/{id}` | Mengubah draft; pemeriksaan supplier yang sama | `FinancePayment : Update` |
| `POST` | `/api/v1/corporate/finance-management/payments/{id}/approve` | Menyetujui pembayaran; kini menolak penyetuju yang tidak sesuai jenjang nominal (`403`) | `FinancePayment : Approve` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik: build dijalankan mandiri oleh pemilik, bukan otomatis oleh agent |
| QBE preflight | `TOUCHED LEGACY`, nol entity/migration/endpoint baru; `QBE-VAL-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001` terpenuhi | `PASS` | Penelusuran source; `AuditAsync` pada `ApproveAsync` tidak berubah |
| Review diff dan scope | Dua berkas source berubah pada modul Payable (`FinancePaymentService.cs` +32/-1, `FinancePaymentsController.cs` +5/-0), tiga berkas test baru, satu helper, dan roadmap. Tidak ada perubahan dependency, konfigurasi, atau migration | `PASS` | `git diff --stat` dan `git status --short` (bagian 7) |
| Pencocokan metadata akses | `Approve`: `[AccessAction("Approve", ...)]` dan `[AccessPermission("FinancePayment", "Approve")]` cocok dengan `ControllerName = "FinancePayment"` | `PASS` | `FinancePaymentsController.cs` baris 104-110 |
| Kompatibilitas konsumen frontend | `paymentType: "SUPPLIER"`, `payeeReferenceId: selectedPayable.supplierId`, `payableType: "SUPPLIER"` | `PASS` | `finance-payment-ap-view.jsx` baris 206-213 |
| Penelusuran jalur normal dan tidak normal pada source | Urutan pemeriksaan Create, Update, dan Approve sesuai bagian 2 | `PASS` | Pembacaan `FinancePaymentService.cs` setelah edit |
| Test otomatis (9 test) | Ditulis, tidak dijalankan | `NOT RUN` | Lihat baris status di bawah |
| Konfirmasi ada pengguna ber-role `Manajer Finance` pada database lingkungan | Tidak diperiksa | `NOT RUN` | Tidak ada akses database; wewenang pemilik |

`AUTOMATED TEST: NOT RUN — 9 test ditulis atas persetujuan pemilik pada rencana kerja 26 Oktober 2026 (lihat roadmap REV-22), tidak dijalankan karena build dan test dijalankan mandiri oleh pemilik.`

Uji manual: `NOT FEASIBLE` — server backend tidak dijalankan pada sesi ini.

**Tidak dijalankan:** `dotnet build`, `dotnet test`, uji manual Swagger, dan pemeriksaan data role pada database. Semuanya menunggu pemilik.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Draft SUPPLIER ke Supplier B dengan alokasi utang Supplier A ditolak `422`, tidak ada pembayaran tersimpan | Terpetakan ke source; belum dibuktikan build/test | `EnsureSupplierAllocationMatchesPayee`, dipanggil dari `CreateDraftAsync`; test `CreateDraft_RejectsPayableOwnedByAnotherSupplier` |
| 2. Mengubah draft ke utang supplier lain ditolak `422`, alokasi lama utuh | Terpetakan ke source; belum dibuktikan build/test | Pemanggilan yang sama pada `UpdateDraftAsync` sebelum `SaveChangesAsync`; test `UpdateDraft_RejectsPayableOwnedByAnotherSupplier_AndKeepsOriginalAllocation` |
| 3. `MEDICAL_SERVICE` dengan alokasi utang supplier ditolak `400` | Terpetakan ke source; belum dibuktikan build/test | Cabang pertama helper; test `CreateDraft_RejectsMedicalServicePaymentAllocatedToSupplierPayable` |
| 4. Draft ke supplier pemilik utang tetap berhasil | Terpetakan ke source; belum dibuktikan build/test | Test `CreateDraft_AcceptsPayableOwnedByThePayee`; kompatibilitas UI (bagian 5) |
| 5. Jenjang persetujuan: `TIER_2` hanya Manajer; `TIER_1` Supervisor atau Manajer; tanpa role ditolak `403` | Terpetakan ke source; belum dibuktikan build/test | `ApproveAsync` memanggil `CanApproveAsync`; empat test pada `FinancePaymentApprovalTierTests` |
| 6. Persetujuan oleh pengaju tetap ditolak `422` sebelum pemeriksaan jenjang | Terpetakan ke source; belum dibuktikan build/test | Urutan pemeriksaan pada `ApproveAsync`; test `Approve_ByRequester_IsStillRejectedAsSelfApproval_EvenForManager` |
| DoD: preflight selesai | Terpenuhi | Bagian Metadata dan bagian 5 |
| DoD: `dotnet build` berhasil | **Belum terpenuhi** | `NOT RUN` |
| DoD: laporan tracked ada | Terpenuhi | Berkas ini |

Task ini **belum** dapat ditandai ✅ sampai pemilik mengonfirmasi `dotnet build` dan test, sesuai `status-task-roadmap.md`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) `FinanceApprovalAuthorizationService` menentukan kewenangan dari nama role teks ("Supervisor Finance", "Manajer Finance"). Itu keputusan `FIN-DEC-052` yang sudah ada dan dipakai PO serta Purchasing Invoice; task ini memakainya apa adanya dan **mencatatnya sebagai temuan** (`role-access-rules.md` bagian 5), bukan mengubahnya. (2) Aksi `Reject` memakai permission `Approve` yang sama dan tidak memeriksa jenjang, sama seperti PO dan Purchasing Invoice; tidak diubah. (3) Pelanggaran serialisasi basis data tidak dipetakan menjadi `409`; sama dengan service Finance lain |
| Masalah yang diketahui | Tiga aturan baru (supplier alokasi = penerima; supplier TF = supplier PO/GR; GR `CANCELLED` tidak dapat dirujuk TF) belum bernomor `FIN-VAL` di kontrak. Dicatat pada roadmap `REV-22` bagian "Delta Kontrak" menunggu revisi kontrak |
| Risiko tersisa | Bila belum ada pengguna ber-role `Manajer Finance`, pembayaran >= Rp 50.000.000 tidak dapat disetujui siapa pun. Kebenaran runtime belum dibuktikan karena build dan test belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat blok di bawah |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` dan test `FinancePaymentSupplierConsistencyTests` serta `FinancePaymentApprovalTierTests`, memastikan ada pengguna ber-role `Manajer Finance`, lalu `BE-FIN-120` dinaikkan ke ✅ pada roadmap |

```text
M Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs
M Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs
?? Tests/QuilvianSystemBackend.PharmacyTests/FinancePaymentApprovalTierTests.cs
?? Tests/QuilvianSystemBackend.PharmacyTests/FinancePaymentSupplierConsistencyTests.cs
?? Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/FinanceApSeed.cs
M docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md
?? docs/module-blueprints/finance-management/task/report/backend/BE-FIN-120.md
```

Berkas lain pada `git status --short` adalah milik task lain: Billing dan test Pharmacy (pekerjaan pengguna yang sudah ada sebelum task ini, tidak disentuh), serta berkas task `REV-22` lainnya (`BE-FIN-121`..`126`).
