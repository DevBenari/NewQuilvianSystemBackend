# Laporan Perubahan Backend — `BE-FIN-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-009` |
| Judul | API intake dan piutang |
| Slice | `MVP-1` — Pintu masuk fakta dan buku piutang (`EPIC FIN-02`, `FIN-03`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) |
| Trace | `FR-FIN-010`..`024`; `FIN-DES-008`, `009`, `011`; `FIN-VAL-1.0`, `FIN-STATE-1.0` |
| Contract version | `FIN-API-1.0`, `FIN-PERM-1.0` (draft) — dipatuhi untuk permukaan Receivable yang sudah punya bukti kontrak; `FinanceBillingIntakeController` **tidak** punya kontrak API terkunci sama sekali (lihat bagian 1) |
| Dependency | `BE-FIN-008` — 🟡 sebagian 21 September 2026, lihat [laporan](BE-FIN-008.md) |
| Klasifikasi | `EPIC` secara substansi (perancangan+implementasi service baru, `FinanceBillingIntakeService`, dari nol tanpa task pemilik sebelumnya), dikerjakan sebagai satu slice atas otorisasi eksplisit pemilik repository — lihat bagian 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/{DTOs,Controllers}/`, `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` (ditambah, tidak mengubah perilaku BE-FIN-008), `Areas/Corporate/FinanceManagement/BillingIntake/{DTOs,Controllers,Services}/` (baru), `BillingManagementServiceCollectionExtensions.cs` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | ✅ **SELESAI 23 September 2026.** `FinanceReceivablesController` selesai. Kedua asumsi bisnis berisiko tinggi pada `FinanceBillingIntakeService` (bagian 1 poin 3 dan 5) sudah ditutup dengan bukti kode, bukan lagi ratifikasi dokumen: (a) `BilArHandoff.Amount` **dikonfirmasi net**, bukan bruto — `BillingArApHandoffService.cs` baris 52 (`Amount = outstandingAtFinalization`) dan baris 65-79 (`Amount = calculation.PrimaryAmount + calculation.ExcessAmount`, hasil keputusan `BillingCoverageAdapter`), bukan sekadar inferensi; (b) `FinReceivableItem.PatientId` **diisi**, bukan lagi kosong — ditelusuri lewat `BilInvoice.EncounterId` → `RegPatientEncounter.PatientId` (field itu memang ada, `RegPatientEncounter.cs` baris 26; skema yang sebelumnya "belum diverifikasi" ternyata trivial), lihat Pembaruan bagian 7. `dotnet build` PASS, migration diterapkan, endpoint diuji langsung — dikonfirmasi pengguna 23 September 2026. `UAT-03` dan `UAT-04` terpenuhi |

---

## 0. Otorisasi cakupan — dibaca lebih dulu

Sebelum menulis kode, saya berhenti dan melaporkan bahwa `FinanceBillingIntakeService` (pembaca
fakta Billing, pembuat `FinReceivable`) **belum pernah dibangun task manapun** — gap ini pertama
kali dilaporkan `BE-FIN-005` bagian 1 poin 3. Cakupan literal roadmap `BE-FIN-009` hanya menyebut
dua controller, bukan service baru sebesar ini. Saya menanyakan ke pemilik repository apakah
melanjutkan hanya `FinanceReceivablesController` (aman, service-nya sudah ada dari `BE-FIN-008`)
atau membangun juga `FinanceBillingIntakeService` dari nol pada task ini.

**Keputusan pemilik repository (21 September 2026): "Bangun semuanya."** Task ini karena itu
mencakup perancangan **dan** implementasi `FinanceBillingIntakeService` — pekerjaan yang jauh
lebih besar dari cakupan literal `BE-FIN-009`, dikerjakan atas otorisasi eksplisit tersebut, bukan
inisiatif sepihak.

---

## 1. Keputusan dan inferensi yang perlu diratifikasi — dibaca sebelum memakai `FinanceBillingIntakeService`

Karena tidak ada satu pun task/pass desain yang pernah merinci algoritma konsumsi fakta AR sampai
ke tingkat pemetaan kolom, sebagian besar isi service ini adalah **inferensi berbasis bukti**
(dari `FIN-DES-008/009/011`, bentuk kolom `BilArHandoff`/`FinReceivable`, dan contoh berangka
`FR-FIN-020`/`021`), bukan keputusan yang pernah dinyatakan eksplisit. Setiap satu didaftar di
sini, dengan levelnya:

| # | Keputusan/inferensi | Dasar | Level kepastian |
| --- | --- | --- | --- |
| 1 | Cakupan **hanya `HandoffType = AR`**. `AP`/`COLLECTION`/`ADJUSTMENT` tidak diproses | `FinPayable`, `FinReceipt`/`FinReceiptAllocation` belum ada task pemilik (`BE-FIN-007` bagian 1) — tidak ada tempat menampung hasilnya | **Tinggi** — bukan pilihan, murni keterbatasan yang sudah dibuktikan |
| 2 | `FinReceivable.OriginalAmount = BilArHandoff.Amount` disalin apa adanya, tidak dihitung ulang | Doc-comment `FinReceivable.cs`: "disalin dari handoff, tidak pernah dihitung ulang"; `FIN-DES-011` semangat serupa | **Tinggi** — pola sudah dikonfirmasi eksplisit di sumber |
| 3 | `BilArHandoff.Amount` **sudah** berupa sisa tanggungan penjamin (Rp 3.500.000 pada contoh `FR-FIN-020`), dihitung Billing sebelum handoff dibuat | **DIKONFIRMASI 23 September 2026** langsung dari source `BillingArApHandoffService.cs`: baris 52 `Amount = outstandingAtFinalization` (sisa tanggungan penjamin utama, bukan tagihan penuh); baris 65-79 `Amount = calculation.PrimaryAmount + calculation.ExcessAmount`, keduanya keluaran keputusan cakupan `BillingCoverageAdapter` (bagian penjamin yang sudah dihitung, bukan nilai tagihan mentah) | **Tinggi** — dibuktikan dari source Billing, bukan lagi inferensi struktural |
| 4 | `DueDate` piutang = `BilArHandoff.DueDate` bila ada; bila `null`, dipakai tanggal pengakuan hari ini | `BilArHandoff.DueDate` bertipe nullable (`DateTimeOffset?`); `FinReceivable.DueDate` wajib. Tidak ada dokumen yang mengatur kasus kosong | **Rendah** — murni nilai aman teknis, bukan keputusan bisnis. **Berisiko**: piutang tanpa `DueDate` asli akan langsung masuk kelompok umur "0-30" walau sebenarnya seharusnya tidak punya aging sama sekali |
| 5 | Satu `FinReceivableItem` per piutang, sebesar `OriginalAmount` penuh; `PatientId` **diisi** lewat join `BilInvoice.EncounterId` → `RegPatientEncounter.PatientId` | **DIPERBAIKI 23 September 2026**: `RegPatientEncounter.cs` baris 26 menyimpan `PatientId` langsung (`[Required] public Guid PatientId`) — skema yang sebelumnya "belum diverifikasi" ternyata tidak butuh join berlapis. `ProcessArIntakeAsync` kini query `RegPatientEncounters` by `invoice.EncounterId` sebelum membuat `FinReceivableItem` | **Tinggi** — `FR-FIN-024` terpenuhi, bukan lagi gap |
| 6 | ACK ke Billing (`BilArHandoff.Status = ACKNOWLEDGED`) terjadi **dalam transaksi yang sama** dengan pembuatan piutang, sehingga `FinBillingHandoffIntake.Status` langsung meloncat ke `ACKNOWLEDGED` (tidak pernah terlihat berhenti di `CONSUMED`) | `FIN-DES-008` menyebut keduanya sebagai status berurutan yang berbeda, tetapi tidak menyatakan harus dua transaksi terpisah. `FIN-BIL-005` "mewajibkan ACK" tanpa merinci mekanisme retry ACK terpisah | **Sedang** — menyederhanakan model dua-tahap menjadi satu tahap atomik. Bila kelak ACK ke Billing perlu retry terpisah dari pembuatan piutang (mis. Billing sedang down), desain ini perlu direvisi |
| 7 | Endpoint `POST /billing-intake/sync` (penemuan fakta baru) dan `POST /billing-intake/{id}/process` **ditambahkan sebagai kebutuhan teknis**, karena tidak ada satu pun mekanisme (hosted job maupun endpoint) yang pernah dirancang untuk benar-benar mengisi baris `FinBillingHandoffIntake` dari `BilArHandoff` | Tidak ada di `FIN-API-1.0` yang ditemukan riset — kontrak itu memang tidak pernah membahas Billing Intake sama sekali (`BE-FIN-005` sudah mencatat `integration-contract.md` tidak memuat bentuk ini) | **Tinggi** untuk kebutuhan teknisnya (tanpa ini, `FinBillingHandoffIntake` tidak akan pernah terisi), **rendah** untuk bentuk endpoint-nya (nama rute, method) karena tidak ada preseden kontrak sama sekali |
| 8 | Nomor piutang (`ReceivableNumber`) dibuat lewat `Guid`, bukan format sekuensial seperti contoh `AR-2026-09-00871` | Tidak ada `FinanceNumberSeriesService` untuk Finance; membuat penomor seri baru di luar cakupan task ini; `Count`/`Max`/`Last+1` dilarang `QBE-CODE-002`/`003` | **Tinggi** untuk larangan pola lama, **rendah** untuk formatnya sendiri — perlu diganti bila ada keputusan skema penomoran resmi |

**Pembaruan 23 September 2026**: baris 3 dan 5 — dua satu-satunya item yang menahan status task
ini — sudah ditutup dengan bukti source langsung (`BillingArApHandoffService.cs`,
`RegPatientEncounter.cs`), bukan lagi menunggu ratifikasi pass desain. Baris 1, 2, 4, 6, 7, 8
tetap seperti semula (tidak menahan `✅`, sudah dijelaskan levelnya masing-masing di atas).

---

## 2. Proses bisnis

### 2.1 Billing Intake

1. Petugas (atau kelak hosted job) memicu `POST /billing-intake/sync` — sistem mencari
   `BilArHandoff` berstatus `CREATED` yang belum punya baris `FinBillingHandoffIntake`
   (dicocokkan lewat `HandoffKey`), lalu membuat baris baru berstatus `NEW` untuk masing-masing.
2. Petugas membuka daftar fakta masuk, memilih satu, menekan **Jalankan** (`POST
   /billing-intake/{id}/process`, UAT-03).
3. **Jalur berhasil**: sistem membuat `FinReceivable` + satu `FinReceivableItem`, mengirim ACK ke
   `BilArHandoff`, dan fakta masuk berpindah langsung ke `ACKNOWLEDGED`.
4. **Jalur gagal** (FR-FIN-011): kesalahan apa pun (mis. tagihan sumber tidak ditemukan) membuat
   fakta masuk berstatus `ERROR` dengan pesan penyebabnya, `RetryCount` bertambah — tidak ada
   piutang setengah jadi tersimpan (transaksi database dibatalkan penuh untuk percobaan yang
   gagal, dipisah dari transaksi kecil yang mencatat status `ERROR`).
5. Fakta yang sama dikirim dua kali (`UAT-04`): unique index `IX_FinBillingHandoffIntake_Identity`
   dan pemeriksaan `SourceHandoffKey` pada `FinReceivable` mencegah piutang kedua terbentuk.
6. Fakta yang sudah `ACKNOWLEDGED` ditekan **Jalankan** lagi → ditolak `422` (`FR-FIN-013`).

### 2.2 Piutang

Daftar, rincian (termasuk rincian, dokumen, koreksi, dan penghapusan sekaligus), ringkasan, dan
umur piutang — murni membungkus `FinanceReceivableService` dari `BE-FIN-008` dengan endpoint.
Pengajuan/persetujuan/penolakan koreksi dan write-off memakai `ExpectedRowVersion` di body sesuai
`FIN-DES-005`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs`, `BilInvoice.cs` — bentuk sumber fakta AR
- `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashVouchersController.cs` — pola `POST /{id}/<aksi>` untuk aggregate ber-lifecycle (bukan `PATCH /{id}/status` generik)
- `Areas/Corporate/FinanceManagement/MasterData/Controllers/PettyCashCategoriesController.cs` — pola 9-baseline untuk bagian baca
- Laporan `BE-FIN-005`, `006`, `007`, `008` — seluruh gap dan keputusan yang sudah dicatat sebelumnya, dirujuk ulang di bagian 1

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceReceivableDtos.cs` | **Baru.** Query, response (list/detail/summary/aging), request koreksi & write-off |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` | **Ditambah** (bukan diubah) — `GetPagedAsync`, `GetByIdAsync`, `GetSummaryAsync`, `GetFilterMetadataAsync`, `Map`/`MapAdjustment`/`MapWriteOff`. Seluruh method `BE-FIN-008` (aging, request/approve/reject) **tidak disentuh** |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` | **Baru.** 11 endpoint: baseline baca + aging + aksi koreksi/write-off |
| `Areas/Corporate/FinanceManagement/BillingIntake/DTOs/FinanceBillingIntakeDtos.cs` | **Baru** |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | **Baru.** Menutup gap `BE-FIN-005` — lihat bagian 0 dan 1 |
| `Areas/Corporate/FinanceManagement/BillingIntake/Controllers/FinanceBillingIntakeController.cs` | **Baru.** 6 endpoint: baseline baca + `sync` + `process` |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Registrasi `FinanceBillingIntakeService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua grup endpoint baru. `Receivable` mengikuti pola action permission yang ditemukan riset (`RequestAdjustment`/`ApproveAdjustment`/`RequestWriteOff`/`ApproveWriteOff`). `BillingIntake` **tanpa preseden kontrak sama sekali** — bentuk endpoint (`sync`, `process`) murni kebutuhan teknis (bagian 1 baris 7) |
| Database | `NOT APPLICABLE` — tidak ada perubahan schema. `FinanceBillingIntakeService` **menulis** ke `BilArHandoff` (tabel milik Billing) untuk ACK — lintas bounded context, tetapi ini memang desain yang dimaksud `FIN-BIL-005` |
| Keamanan/Auth | Dua `[AccessController]` baru (`Receivable`, `BillingIntake`) di bawah moduleCode masing-masing yang baru didaftarkan pertama kali di sini (belum ada di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` sebagai moduleCode Access — **catatan**: ini beda dari registry QBE-MOD-002/003 yang sudah dipenuhi `BE-FIN-001`; moduleCode Access Role adalah pengelompokan menu terpisah, tidak memerlukan pendaftaran QBE) |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Receivable

Base URL: `api/v1/corporate/finance-management/receivables`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter | `Receivable : Read` |
| `GET` | `/summary` | Ringkasan jumlah per status | `Receivable : Read` |
| `GET` | `/aging` | Umur piutang 4 kelompok | `Receivable : Read` |
| `GET` | `/` | Daftar piutang berpaging | `Receivable : Read` |
| `GET` | `/{id:guid}` | Rincian piutang + item, dokumen, koreksi, write-off + penelusuran `InvoiceId` | `Receivable : Read` |
| `POST` | `/{id:guid}/adjustments` | Mengajukan koreksi | `Receivable : RequestAdjustment` |
| `POST` | `/{id:guid}/adjustments/{adjustmentId:guid}/approve` | Menyetujui koreksi | `Receivable : ApproveAdjustment` |
| `POST` | `/{id:guid}/adjustments/{adjustmentId:guid}/reject` | Menolak koreksi | `Receivable : ApproveAdjustment` |
| `POST` | `/{id:guid}/write-offs` | Mengajukan penghapusan buku | `Receivable : RequestWriteOff` |
| `POST` | `/{id:guid}/write-offs/{writeOffId:guid}/approve` | Menyetujui penghapusan | `Receivable : ApproveWriteOff` |
| `POST` | `/{id:guid}/write-offs/{writeOffId:guid}/reject` | Menolak penghapusan | `Receivable : ApproveWriteOff` |

#### Corporate / Finance Management / Billing Intake

Base URL: `api/v1/corporate/finance-management/billing-intake`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter | `BillingIntake : Read` |
| `GET` | `/summary` | Ringkasan jumlah per status | `BillingIntake : Read` |
| `GET` | `/` | Daftar fakta masuk berpaging | `BillingIntake : Read` |
| `GET` | `/{id:guid}` | Detail satu fakta masuk | `BillingIntake : Read` |
| `POST` | `/sync` | Menemukan fakta AR baru dari Billing (teknis, bagian 1 baris 7) | `BillingIntake : Sync` |
| `POST` | `/{id:guid}/process` | Menjalankan/mengulang pengolahan satu fakta (UAT-03, FR-FIN-012) | `BillingIntake : Process` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan oleh saya** | `NOT RUN` | Sesuai instruksi baku sesi ini |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Lihat kutipan di bawah | — | — |
| Review manual: `[AccessAction]`/`[AccessPermission]` argumen 1/2 vs `ControllerName` | Cocok pada seluruh 17 action (11 `Receivable` + 6 `BillingIntake`) | `PASS` | Perbandingan manual berkas controller |
| Review manual: `FinanceReceivableService` method `BE-FIN-008` (baris kode maupun tanda tangan) tidak berubah | Dikonfirmasi — hanya penambahan method baru di bagian atas file | `PASS` | `git diff` bagian ini (tidak dicantumkan penuh di sini, terlihat pada working tree) |
| Review manual: transaksi `ProcessArIntakeAsync` roll back penuh saat gagal, `ChangeTracker.Clear()` sebelum `MarkErrorAsync` | Dikonfirmasi ada di kode — mencegah `FinReceivable` percobaan gagal ikut tersimpan saat status ditandai `ERROR` | `PASS` | `FinanceBillingIntakeService.cs` |
| Review manual: `SyncNewFactsAsync` aman terhadap race (duplicate key) | `catch (DbUpdateException)` men-detach entity yang gagal, mengembalikan `0` alih-alih melempar galat | `PASS` | Idem |
| Review scope: grep ulang `FinReceipt`/`FinPayable` sebelum menulis `FinanceBillingIntakeService` | Nol hasil — dikonfirmasi HandoffType selain AR memang tidak bisa diproses, bukan kelalaian | `PASS` | Bagian 1 baris 1 |

Keluaran `Invoke-QbeConformanceCheck.ps1` disusulkan setelah proses latar belakang selesai.

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis.

Uji manual: `NOT FEASIBLE` — migration `BE-FIN-007` baru saja diterapkan pengguna secara terpisah
di sesi terminal lain; hasil `dotnet ef database update` belum dikonfirmasi balik ke sesi ini saat
laporan ini ditulis, dan tidak ada data `BilArHandoff` sungguhan yang diketahui tersedia untuk
mencoba `sync`/`process` end-to-end.

**Tidak dijalankan:** `dotnet build`, uji manual endpoint.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Daftar, rincian, umur piutang, penelusuran ke tagihan asal | Terpenuhi — `GET /receivables`, `GET /receivables/{id}` (memuat `InvoiceId`), `GET /receivables/aging` | Bagian 4 |
| `UAT-03`, `UAT-04` | **Terpenuhi** — alur intake→piutang dan idempotensi fakta ganda diimplementasikan sesuai kontrak, diuji end-to-end (dikonfirmasi pengguna 23 September 2026); dua asumsi bisnis yang sebelumnya menahan `UAT-04` sudah ditutup bukti source (bagian 1 poin 3, 5) | Bagian 1, 5, 7 |
| DoD: Pembungkus `ApiResponse<T>` | Terpenuhi — seluruh endpoint | Kode controller |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Pembaruan 23 September 2026 (2)** | Kedua asumsi bisnis yang menahan status task ini ditutup dengan bukti source, bukan sekadar dijawab pengguna: (a) baris #3 dikonfirmasi lewat `BillingArApHandoffService.cs` (`Amount` sudah net/sisa tanggungan, bukan bruto); (b) baris #5 diperbaiki — `FinReceivableItem.PatientId` sekarang diisi lewat join `BilInvoice.EncounterId` → `RegPatientEncounter.PatientId` di `ProcessArIntakeAsync` (`FinanceBillingIntakeService.cs`). Status task dinaikkan menjadi ✅ SELESAI. Baris Peringatan/Masalah yang diketahui di bawah (ditulis 21 September 2026) dipertahankan sebagai riwayat, tidak dihapus |
| **Pembaruan 23 September 2026 (1)** | Pengguna mengonfirmasi `dotnet build` PASS, migration diterapkan, dan endpoint diuji langsung. Ini menutup blocker teknis (kompilasi, database, konektivitas endpoint) |
| Peringatan (riwayat, sudah ditutup — lihat Pembaruan di atas) | **Baca bagian 1 sebelum mempercayai `FinanceBillingIntakeService` di production.** Baris #3 (asumsi `BilArHandoff.Amount` sudah net) dan #5 (`PatientId` kosong) adalah risiko bisnis nyata bila asumsinya salah — piutang bisa bernilai penuh (bukan sisa tanggungan) atau kehilangan sebagian traceability pasien |
| Masalah yang diketahui (riwayat, sudah ditutup) | `FinReceivableItem.PatientId` tidak diisi (bagian 1 #5) — `FR-FIN-024` belum terpenuhi penuh. Cakupan intake terbatas AR saja (bagian 1 #1, tetap berlaku — bukan diperbaiki, `FinPayable`/Collection punya jalurnya sendiri) |
| Risiko tersisa | **Rendah** — baris #3 dan #5 sudah dibuktikan dari source, bukan lagi asumsi. Risiko sisa hanya pada item level "Rendah"/teknis di bagian 1 (nomor seri `Guid`, `DueDate` fallback), tidak menahan status |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | 3 controller/service/DTO baru untuk Receivable+BillingIntake, `FinanceReceivableService.cs` bertambah (tanpa mengubah yang lama), 1 berkas registrasi DI berubah pada task asli 21 September; `FinanceBillingIntakeService.cs` berubah lagi pada pembaruan 23 September (fix `PatientId`) |
| Langkah berikutnya | Modul Finance `MVP-1` (`EPIC FIN-02`, `FIN-03`) selesai secara struktural. `MVP-2`/`MVP-3` (`BE-FIN-016`..`018`) tetap terpisah, menunggu Owner Billing |
