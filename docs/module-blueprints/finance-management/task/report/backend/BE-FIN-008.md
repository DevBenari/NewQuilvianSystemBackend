# Laporan Perubahan Backend — `BE-FIN-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-008` |
| Judul | Layanan piutang: umur, koreksi, penghapusan |
| Slice | `MVP-1` — Pintu masuk fakta dan buku piutang (`EPIC FIN-02`, `FIN-03`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) |
| Trace | `FIN-DES-011`..`013`, `FR-FIN-021`..`023`; `FIN-DEC-010` (bucket aging); `FIN-VAL-011`, `014`, `020`..`025`; `state-transition-matrix.md` §2/§3 |
| Contract version | `FIN-STATE-1.0`, `FIN-VAL-1.0` (draft) — dipatuhi untuk bagian yang menjadi tanggung jawab service ini; lihat bagian 1 untuk batas cakupan yang diambil |
| Dependency | `BE-FIN-007` — 🟡 sebagian 21 September 2026 (migration dibuat, belum dijalankan), lihat [laporan](BE-FIN-007.md). Kode ini ditulis terhadap entity yang sudah dikompilasi model-nya; **belum dapat diuji end-to-end** sampai migration diterapkan |
| Klasifikasi | `HEAVY` — logika bisnis kompleks (maker-checker, invariant, aging), lintas-aggregate (`Serializable`), satu-satunya penulis kolom finansial kritis, beberapa temuan/gap kontrak yang perlu diratifikasi |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs`, `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | 🟡 **SEBAGIAN.** Seluruh tiga kapabilitas (aging, koreksi, write-off) diimplementasikan mengikuti kontrak yang ada, dengan beberapa titik yang **secara eksplisit diinferensikan** dari invariant matematis (bukan dinyatakan langsung dokumen) — dicatat rinci bagian 1. Belum dapat diuji end-to-end (migration belum jalan) |

---

## 1. Batas cakupan dan temuan kontrak — dibaca lebih dulu

### 1.1 Yang **termasuk** cakupan `FinanceReceivableService` (dan buktinya)

`02-backend-architecture.md` baris layanan menyebut tepat: *"Satu-satunya penulis
`OutstandingAmount` piutang; mengurus aging, koreksi, write-off"*. Tiga kapabilitas itulah yang
diimplementasikan. **Yang sengaja TIDAK dibuat**, dengan bukti kenapa itu bukan tanggung jawab
service ini:

| Kapabilitas | Kenapa di luar cakupan | Bukti |
| --- | --- | --- |
| Membuat `FinReceivable` baru dari fakta Billing | Deskripsi layanan `FinanceReceivableService` tidak menyebut "membuat"; yang menyebutnya adalah `FinanceBillingIntakeService` ("membaca handoff Billing, membuat piutang/utang/penerimaan") | `02-backend-architecture.md` baris service `FinanceBillingIntakeService` vs `FinanceReceivableService` |
| Alokasi penerimaan ke piutang (`FinReceiptAllocation`) | Endpoint alokasi ada di bawah `Receipt`, dilayani `FinanceReceiptService` — bukan `FinanceReceivableService`. Entity `FinReceipt`/`FinReceiptAllocation` sendiri **belum ada** (gap yang sama dilaporkan `BE-FIN-007`) | `contracts/api-contract.md` (`POST /receipts/{id}/allocations` → `FinanceReceipt : Allocate`); `BE-FIN-018` (`BLOCKED`, owner Billing) |
| Pembatalan piutang (`CANCELLED`) | `state-transition-matrix.md` §2 mendokumentasikan transisi ini, tetapi **tidak ada satu pun endpoint** untuknya di `FIN-API-1.0` maupun `permission-audit-matrix.md` (dicek ulang eksplisit) — dan deskripsi satu-baris `FinanceReceivableService` juga tidak menyebut "pembatalan". Ini kemungkinan gap kontrak, bukan alasan untuk mengarang endpoint/method yang belum diminta | Ditelusuri eksplisit sebagai bagian task ini — nol baris `Cancel`/`FinanceReceivable:Cancel` ditemukan pada kedua kontrak |

### 1.2 Bagian yang diinferensikan dari invariant matematis, bukan dinyatakan langsung

Dokumen sumber **tidak pernah** memberi contoh berangka untuk koreksi arah `DEBIT` (menambah
piutang) — seluruh contoh (`FR-FIN-021`, `UAT-10`, `UAT-11`, `FIN-VAL-024`) hanya menunjukkan arah
`CREDIT` (mengurangi). Efek `DEBIT` pada `OutstandingAmount`/`AdjustedAmount` **diturunkan secara
matematis** dari invariant yang terkunci (`OriginalAmount = Outstanding + Allocated + Adjusted +
WrittenOff`, harus tetap benar untuk kedua arah):

- `CREDIT` disetujui: `OutstandingAmount -= Amount`, `AdjustedAmount += Amount`
- `DEBIT` disetujui: `OutstandingAmount += Amount`, `AdjustedAmount -= Amount` **(diinferensikan, bukan dari contoh berangka eksplisit)**

Ini dicatat sebagai delta yang **perlu diratifikasi** pemilik blueprint (Yasmin) — bukan diputuskan
sepihak sebagai kebenaran final, walau secara matematis satu-satunya cara invariant tetap konsisten.

### 1.3 Temuan yang terasa seperti kejanggalan kontrak, diimplementasikan apa adanya

`state-transition-matrix.md` §2, baris `OUTSTANDING atau PARTIAL → Penghapusan piutang disetujui →
WRITTEN_OFF`, **tidak bersyarat** pada `OutstandingAmount` mencapai nol. Digabung dengan
doc-comment `FinReceivableWriteOff.cs` sendiri yang eksplisit mendukung penghapusan **sebagian**
("sebagian/seluruh"), bacaan literalnya: **penghapusan sebagian pun langsung membuat status
piutang `WRITTEN_OFF` (status akhir/terminal)**, walau sisa `OutstandingAmount` masih di atas nol
setelahnya. Ini terasa janggal (piutang yang masih punya sisa tapi berstatus "selesai
dihapusbukukan"), tetapi **tidak ada satu pun teks pada seluruh dokumen blueprint yang
membantahnya** — diimplementasikan **apa adanya** sesuai kontrak terkunci, dengan catatan tebal
pada kode dan di sini agar pemilik blueprint dapat meninjau ulang, bukan diam-diam "diperbaiki"
sepihak oleh saya.

### 1.4 Skema penomoran `AdjustmentNumber`/`WriteOffNumber`

Berbeda dari `ReceivableNumber` (contoh "AR-2026-09-00871" tersedia), tidak ada format resmi untuk
nomor koreksi/penghapusan pada dokumen manapun. Dibuat unik lewat `Guid` (`ADJ-yyyyMMdd-<guid>`,
`WO-yyyyMMdd-<guid>`), **bukan** `Count`/`Max`/`Last+1` (dilarang `QBE-CODE-002`/`003`). Ini
teknis murni untuk memenuhi kolom wajib-unik, bukan keputusan format bisnis — pemilik blueprint
dapat menggantinya dengan skema nomor seri resmi (mis. lewat `NumberSeriesManagement`) kapan saja
tanpa mengubah perilaku bisnis lain.

---

## 2. Proses bisnis

### 2.1 Umur piutang (`GetAgingSummaryAsync`)

Empat kelompok tetap (`FIN-DEC-010`): `0-30`, `31-60`, `61-90`, `di atas 90 hari`, dihitung
`(asOfDate − DueDate).Days` saat query (bukan disimpan — `02-backend-architecture.md` §10 secara
eksplisit menolak tabel `FinAgingBucket` karena "berisiko basi"). Contoh: piutang jatuh tempo
15 Juli 2026 dilihat 20 September 2026 → 67 hari lewat → "61-90 hari". Piutang yang jatuh tempo
tepat 30 hari lalu → "0-30", bukan "31-60" (`FR-FIN-022`, batas inklusif diverifikasi lewat baris
uji `testing/acceptance-test-matrix.md`: 95 hari → "di atas 90 hari", tepat 30 hari → "0-30").
**Kasus yang tidak diatur dokumen manapun**: piutang yang belum jatuh tempo (`DueDate` di masa
depan). Diperlakukan sebagai "0-30" (hari terlambat dianggap nol) — nilai aman untuk kasus tidak
diatur, bukan keputusan bisnis baru, dicatat eksplisit pada kode.

### 2.2 Koreksi piutang

Staf mengajukan koreksi (`RequestAdjustmentAsync`) dengan arah `DEBIT`/`CREDIT`, nominal, dan
alasan wajib. Pengguna **lain** yang berwenang menyetujui (`ApproveAdjustmentAsync`) atau menolak
(`RejectAdjustmentAsync`) — pengaju tidak boleh memutuskan permohonannya sendiri (`FIN-DEC-012`,
ditegakkan service **dan** check constraint database `BE-FIN-006`). Nilai piutang **hanya**
berubah tepat saat disetujui (`UAT-11`: "sisa piutang berkurang saat itu juga, bukan sebelumnya"),
dan koreksi pengurang tidak boleh melebihi sisa piutang saat ini (`FIN-VAL-024`) — dicek ulang saat
persetujuan (bukan saat pengajuan), karena sisa piutang bisa berubah di antara keduanya akibat
koreksi/penghapusan lain yang lebih dulu disetujui.

### 2.3 Penghapusan buku piutang

Sama pola maker-checker dengan koreksi, tanpa arah (selalu mengurangi). Piutang berstatus `SETTLED`
(lunas) tidak boleh diajukan penghapusan sama sekali (`FIN-VAL-014`, dicek saat pengajuan **dan**
saat persetujuan sebagai jaga-jaga bila status berubah di antaranya). Penghapusan yang disetujui
memindahkan nilai dari `OutstandingAmount` ke `WrittenOffAmount` dan langsung mengubah status
piutang menjadi `WRITTEN_OFF` — lihat catatan kejanggalan pada bagian 1.3 untuk penghapusan
sebagian.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` — `FR-FIN-020`..`024`, `UAT-08`..`012`
- `docs/module-blueprints/finance-management/00-interview-decisions.md` — `FIN-DEC-010` (bucket aging, exact wording)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` — endpoint Receivable lengkap (dicek ulang: nol endpoint Cancel)
- `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` §3 Piutang — daftar action (Read/Update/RequestAdjustment/ApproveAdjustment/RequestWriteOff/ApproveWriteOff, nol Cancel)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` §2/§3 — `FIN-VAL-010`..`016`, `020`..`025`
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §2/§3
- `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` — baris uji batas aging
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §10 (aging tidak disimpan), tabel service §4.22
- `Areas/Corporate/FinanceManagement/Receivable/Models/*.cs` (`BE-FIN-006`) — bentuk entity persis
- `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` — pola transaksi `Serializable`, advisory lock, `RowVersion` manual, privasi log (dipakai ulang persis)
- Grep penuh `Areas/` untuk `FinReceipt`/`FinReceiptAllocation`/`FinanceReceivableService` — nol hasil selain doc-comment sendiri

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` | **Baru.** `GetAgingSummaryAsync`, `Request/Approve/RejectAdjustmentAsync`, `Request/Approve/RejectWriteOffAsync`; `ReceivableAgingBuckets`, `ReceivableAgingBucketResult`; tiga exception (`ReceivableBadRequestException`, `ReceivableValidationException`, `ReceivableConflictException`) |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Registrasi `services.AddScoped<FinanceReceivableService>()`, mengikuti titik registrasi Corporate/FinanceManagement yang sudah ada |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint pada task ini (`BE-FIN-009`) |
| Database | `NOT APPLICABLE` — tidak ada perubahan schema; hanya mutasi terhadap kolom yang sudah ada dari `BE-FIN-006` |
| Keamanan/Auth | `NOT APPLICABLE` langsung. Catatan privasi: `Reason`/`RejectionReason` sengaja **tidak** masuk log audit (bisa memuat keterangan bebas), mengikuti pola `PettyCashVoucherService` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini murni service, tanpa controller.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan oleh saya** | `NOT RUN` | Atas instruksi pengguna sejak `BE-FIN-002`; pengguna dilaporkan sedang menjalankan `dotnet ef database update` secara paralel pada sesi ini |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | `Files evaluated: 34`, `VIOLATION: 0`, `Final result: PASS` | `PASS` | Kutipan di bawah |
| Review manual: `EnsureCurrent`/`RowVersion` diregenerasi setiap mutasi pada `FinReceivableAdjustment`/`WriteOff` **dan** `FinReceivable` | Dikonfirmasi pada `DecideAdjustmentAsync`/`DecideWriteOffAsync` | `PASS` | Kode pada bagian 3.2 |
| Review manual: maker-checker (`actorUserId == RequestedBy` → tolak) diperiksa sebelum efek finansial apa pun diterapkan | Dikonfirmasi — pemeriksaan terjadi sebelum blok `if (approve)` | `PASS` | Idem |
| Review manual: transaksi `Serializable` + advisory lock per `ReceivableId` pada kedua alur keputusan | Dikonfirmasi — pola identik `PettyCashVoucherService` | `PASS` | Idem |
| Review manual: invariant `Outstanding/Adjusted` untuk `CREDIT` dicocokkan terhadap contoh berangka `FR-FIN-021` (3.500.000 → 2.000.000 sisa, 500.000 `AdjustedAmount`) | Cocok persis | `PASS` | Bagian 1.2 |
| Review manual: `FIN-VAL-014` diperiksa pada **dua** titik (pengajuan dan persetujuan write-off) | Dikonfirmasi | `PASS` | `RequestWriteOffAsync`, `DecideWriteOffAsync` |
| Review scope: grep ulang `FinReceipt`/`FinReceiptAllocation`/endpoint Cancel sebelum menulis kode | Nol hasil — dikonfirmasi tidak membangun sesuatu yang belum ada dasarnya | `PASS` | Bagian 1.1 |

Keluaran `Invoke-QbeConformanceCheck.ps1`:

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 34
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
REPORT ONLY: No enforcement/blocking performed.
Final result: PASS
```

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis. "Uji unit
invariant" pada roadmap `BE-FIN-006` sudah ditutup lewat check constraint database; task ini
menambah **logika** yang menjaga invariant itu tetap benar saat ditulis, diverifikasi lewat review
manual di atas, bukan test otomatis baru.

Uji manual: `NOT FEASIBLE` — migration `BE-FIN-007` belum diterapkan ke database manapun untuk
sesi ini (walau pengguna dilaporkan sedang menjalankannya secara paralel; hasilnya belum
dikonfirmasi balik ke sesi ini saat laporan ini ditulis).

**Tidak dijalankan:** `dotnet build`, uji manual (lihat tabel di atas).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Empat kelompok umur | Terpenuhi | `ReceivableAgingBuckets`, bagian 2.1 |
| Berkas klaim tidak menahan pengakuan piutang | `NOT APPLICABLE` untuk task ini — ini aturan penciptaan piutang (`FinanceBillingIntakeService`), bukan `FinanceReceivableService`; sudah benar secara struktural karena `ClaimStatus` tidak pernah dibaca/ditulis oleh service ini | Bagian 1.1 |
| DoD: `Serializable` | Terpenuhi — kedua alur keputusan (`Decide*Async`) berjalan `IsolationLevel.Serializable` | `BeginTransactionAsync` |
| DoD: satu penulis saja | Terpenuhi — tidak ada kode lain pada repository ini yang menulis `FinReceivable.OutstandingAmount` (digrep ulang) | Bagian 3.1 |

Kriteria yang secara literal menjadi tanggung jawab slice ini **seluruhnya terpenuhi**. Beberapa
detail (arah `DEBIT`, penghapusan sebagian) diimplementasikan lewat inferensi/bacaan literal yang
didokumentasikan eksplisit pada bagian 1, bukan diam-diam diasumsikan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) Efek `DEBIT` pada koreksi adalah **inferensi matematis**, belum pernah dicontohkan dokumen manapun — perlu ratifikasi eksplisit pemilik blueprint (bagian 1.2). (2) Penghapusan sebagian langsung membuat piutang berstatus `WRITTEN_OFF` walau sisa belum nol — kejanggalan kontrak yang diimplementasikan apa adanya (bagian 1.3), perlu ditinjau ulang. (3) Transisi `CANCELLED` didokumentasikan `state-transition-matrix.md` tetapi tidak diimplementasikan karena nol endpoint/permission untuknya di kontrak terkunci — perlu keputusan pemilik blueprint apakah kontrak API perlu ditambah atau transisi itu dicabut dari `FIN-STATE-1.0` |
| Masalah yang diketahui | Skema penomoran `AdjustmentNumber`/`WriteOffNumber` sementara (`Guid`-based), menunggu keputusan skema resmi (bagian 1.4) |
| Risiko tersisa | Sedang — logika sudah ditelaah manual terhadap kontrak, tetapi belum diverifikasi compiler (`dotnet build`) maupun database sungguhan. Tiga temuan kontrak pada bagian 1 berpotensi mengubah perilaku bila pemilik blueprint memutuskan berbeda dari bacaan literal yang dipakai di sini |
| Perubahan sampingan | `NONE` dari saya. Terlihat perubahan pada `docs/module-blueprints/pharmacy/00-interview-decisions.md` dan `docs/module-blueprints/rawat-jalan/00-interview-decisions.md` pada `git status` — **tidak berkaitan** dengan Finance, **tidak disentuh**, kemungkinan aktivitas paralel pengguna/sesi lain |
| Interupsi | `NONE` pada task ini |
| Status Git | 1 berkas baru (`FinanceReceivableService.cs`) + 1 berkas registrasi DI berubah, di atas `BE-FIN-002`..`007` yang sudah ada |
| Langkah berikutnya | (1) `dotnet build` mencakup `BE-FIN-002`..`008` (setelah migration diterapkan pengguna). (2) Ratifikasi tiga temuan bagian 1 oleh pemilik blueprint. (3) `BE-FIN-009` (API) dapat mulai — dependency-nya `BE-FIN-008`, yang sudah terpenuhi untuk cakupan yang menjadi tanggung jawabnya |
