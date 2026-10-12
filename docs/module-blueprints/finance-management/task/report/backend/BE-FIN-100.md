# Laporan Perubahan Backend — `BE-FIN-100`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-100` |
| Judul | Saringan jenis debitur `EMPLOYEE_BENEFIT` terpasang pada daftar piutang, kalkulasi aging AR, dan laporan rekapitulasi AR |
| Slice | `REV-18B2` — Perjanjian Cicilan, Clearance, Pelunasan & Migrasi |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `AMENDMENT ROADMAP REVISI 18` |
| Trace | `FR-FIN-219`; `FIN-DEC-176` |
| Contract version | `FIN-API-1.9` Receivable endpoints; `FIN-PERM-1.10` |
| Dependency | `BE-FIN-091` 🟡 — belum ✅ |
| Klasifikasi | `LIGHT` — perluasan parameter opsional pada 2 method service existing + wiring pada 2 controller existing, nol entity/migration baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/{Controllers,Services}/` |
| Model | Claude Sonnet 5 |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **Source selesai ditulis.** `dotnet build` **TIDAK DIJALANKAN** (instruksi eksplisit pengguna "tanpa build dan migration automatis"). Task ke-10 berturutan tanpa build pada epik ini |

---

## 0. Backend Governance Preflight

**Area/Module:** `Corporate/FinanceManagement/Receivable`, prefix `Fin`, `ACTIVE` (registry sudah ada sejak task-task sebelumnya pada epik ini). **Applicability:** `TOUCHED LEGACY` — seluruh perubahan task ini memperluas method dan endpoint **existing** (`GetSummaryAsync`, `GetReportSummaryAsync`, `GetAging` pada dua controller), bukan menulis kelas baru. Konsisten dengan disposisi `FR-FIN-219` sendiri pada `04-prd-to-mvp.md` baris 2033: **`EXTEND`**. **QBE ID:** QBE-API-001 (perluasan parameter opsional, backward compatible — perilaku bawaan tanpa argumen TIDAK berubah), QBE-PERM-001 (nol `[AccessAction]`/`[AccessPermission]` baru — seluruh endpoint yang disentuh memakai hak akses `Read`/`View` yang sudah terdaftar).

**Temuan kutipan kontrak basi** (pola yang sama berulang di `BE-FIN-091`, `BE-FIN-096`): kolom Kontrak pada baris roadmap `BE-FIN-100` merujuk `02-backend-architecture.md O.7`, tetapi O.7 pada dokumen itu berjudul "Status model dan dampak migration" — isinya sama sekali tidak membahas saringan jenis debitur pada laporan AR. Dicatat sebagai temuan, tidak diperbaiki (di luar wewenang tulis task ini atas dokumen arsitektur).

---

## 1. Interpretasi kontrak dan keputusan desain

`FR-FIN-219` dan baris roadmap menyebut cakupannya sebagai **"daftar piutang, kalkulasi aging AR, dan laporan rekapitulasi AR"**; `L.6.4` pada `acceptance-test-matrix.md` menyebutnya **"daftar piutang dan tiga laporan AR"**. Kontrak tidak merinci nama ketiga laporan itu secara eksplisit. **Keputusan yang saya tetapkan** (bukti, bukan tebakan): `FinanceReceivableService` — satu-satunya service pada kolom Reuse roadmap task ini — punya persis **tiga** method baca agregat lintas-piutang selain daftar berpaging: `GetSummaryAsync` (Ringkasan), `GetAgingSummaryAsync` (Aging), `GetReportSummaryAsync` (Report/rekapitulasi). Saya memetakan "tiga laporan AR" ke ketiganya. **MUST dikonfirmasi Finance Owner** bila yang dimaksud berbeda (misalnya salah satu dari sekian layar "Laporan Pembuatan/Piutang/Pembayaran AR" pada `03-frontend-architecture.md` §17.2 yang justru dilayani service lain seperti `FinanceReceivableInvoiceBatch`/`FinanceReceipt` — di luar wewenang task ini apa pun keputusannya, karena Reuse roadmap hanya menyebut `FinanceReceivableService`).

Ditemukan juga saat investigasi: `GetAgingSummaryAsync` **sudah** menerima parameter opsional `debtorType` sejak `BE-FIN-055`, dan `FinanceReceivablesController.GetAging` (route `api/v1/corporate/finance-management/receivables/aging`) **sudah benar** mewiring-nya. Tetapi `FinanceArController.GetAging` (route `api/finance/receivable/aging` — menurut `01-existing-capability-map.md` §21.4 inilah endpoint yang sebenarnya dipanggil layar "Buku Piutang"/"Aging AR" frontend) **mengabaikan** `request.DebtorType` sama sekali — gap legacy yang ditemukan, bukan diperkenalkan task ini. Diperbaiki pada task ini karena ini **kemampuan yang sama persis** yang sudah disetujui `BE-FIN-055`, hanya belum konsisten diterapkan ke controller kedua yang mengekspos kapabilitas identik (bukan keputusan bisnis baru).

---

## 2. Proses bisnis

Keempat permukaan baca `FinanceReceivableService` kini menerima saringan `DebtorType` opsional dengan pola yang identik di seluruhnya — null/kosong berarti seluruh `FinReceivable` tanpa saringan (perilaku bawaan sebelum task ini, **tidak berubah** bila dipanggil tanpa argumen):

1. **Daftar piutang** (`GetPagedAsync`, `GET /` pada kedua controller) — **sudah berfungsi sejak sebelum task ini** (pencocokan persis `x.DebtorType == request.DebtorType` pada `FinanceReceivableService.cs:63`, berlaku untuk nilai apa pun pada `FinReceivableDebtorTypes`, termasuk `EMPLOYEE_BENEFIT` yang ditambahkan `BE-FIN-091`). Diverifikasi baca, nol perubahan kode diperlukan di sini.
2. **Ringkasan** (`GetSummaryAsync`) — **diperluas** task ini: parameter `debtorType` opsional menyaring baik hitungan per status maupun `TotalOutstandingAmount`.
3. **Aging** (`GetAgingSummaryAsync`) — sudah punya parameter sejak `BE-FIN-055`; **diwiring** task ini pada `FinanceArController.GetAging` yang sebelumnya mengabaikannya.
4. **Rekapitulasi** (`GetReportSummaryAsync`) — **diperluas** task ini: parameter `debtorType` opsional menyaring seluruh total (`TotalOriginalAmount`, `TotalOutstandingAmount`, dst.), `StatusBreakdown`, dan diteruskan ke `GetAgingSummaryAsync` internal supaya `AgingBuckets` pada laporan tetap konsisten dengan saringan yang sama. `DebtorTypeBreakdown` tidak berubah strukturnya — tetap dictionary per jenis debitur; bila disaring, dictionary itu otomatis berisi satu kunci saja (jenis yang diminta), sehingga nol kemungkinan tercampur dengan penjamin reguler lain (`L.6.4`: "nol data tercampur").

Nilai yang valid untuk `debtorType` tetap `FinReceivableDebtorTypes` nyata — `PAYER`, `PATIENT_GUARANTOR`, `EMPLOYEE_BENEFIT` — bukan nilai dugaan lain; dicocokkan persis (case-sensitive) terhadap kolom `FinReceivable.DebtorType`, sama seperti filter daftar piutang yang sudah ada.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` baris 2033 (`FR-FIN-219`, disposisi `EXTEND`)
- `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` `L.6.4`, `L.8.2`
- `docs/module-blueprints/finance-management/01-existing-capability-map.md` §21.4 (kontrak as-is daftar piutang, dua controller memakai service yang sama), §21.5 (`FIN-CQ-11`, sudah `CLOSED`)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` — seluruh judul `O.1`..`O.11` diperiksa; `O.7` dikonfirmasi tidak relevan (lihat §0)
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` (penuh)
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceArController.cs`, `FinanceReceivablesController.cs` (penuh)
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` (konfirmasi `FinReceivableDebtorTypes.EmployeeBenefit`)
- `Program.cs` dan `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (konfirmasi `FinanceReceivableService` terdaftar DI lewat `AddBillingManagement()`, bukan gap seperti rumpun service baru `BE-FIN-091`..`099`)
- Frontend (read-only, referensi consumer): `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx` baris 826-865 (katalog menu "Laporan & Monitoring" AR, konfirmasi `FIN-CQ-09` sudah konsisten pakai action `View` pada source saat ini, tidak ada aksi perbaikan diperlukan)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` | `GetSummaryAsync` dan `GetReportSummaryAsync` menerima parameter opsional `string? debtorType = null`, diterapkan sebagai saringan `Where` tambahan |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` | `GetSummary` menerima `[FromQuery] string? debtorType` dan meneruskannya |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceArController.cs` | `GetAging` kini meneruskan `request.DebtorType` (gap diperbaiki); `GetReport` menerima `[FromQuery] string? debtorType` dan meneruskannya |

Nol berkas DTO baru — `ReceivableAgingQuery.DebtorType` sudah ada (`BE-FIN-055`); `GetSummary`/`GetReport` memakai parameter `[FromQuery]` langsung tanpa DTO pembungkus, mengikuti pola `GetReport([FromQuery] DateOnly? asOfDate, ...)` yang sudah ada di controller yang sama.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 3 endpoint existing mendapat parameter query opsional baru (`debtorType` pada `GET summary`, `GET report`; wiring pada `GET aging`). **Backward compatible** — tanpa parameter, perilaku identik dengan sebelum task ini. Nol endpoint baru, nol breaking change |
| Database | `NOT APPLICABLE` — nol entity, nol migration |
| Keamanan/Auth | **Nol** `[AccessAction]`/`[AccessPermission]` baru. Ketiga endpoint yang disentuh sudah memakai hak baca (`Read`/`View`) yang terdaftar sebelumnya |

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna |
| Review manual: seluruh caller `GetSummaryAsync`/`GetReportSummaryAsync`/`GetAgingSummaryAsync` di luar `FinanceReceivableService.cs` | Hanya 2 controller yang memanggil, keduanya sudah diperbarui; parameter baru bersifat opsional (default `null`) sehingga tidak ada pemanggil lain yang rusak | `PASS` (grep lintas repo) | `grep -rn "GetSummaryAsync\|GetReportSummaryAsync\|GetAgingSummaryAsync"` |
| Review manual: `FinanceReceivableService` terdaftar DI | Terdaftar lewat `BillingManagementServiceCollectionExtensions.AddBillingManagement()`, dipanggil `Program.cs` — bukan gap seperti rumpun service baru pada task sebelumnya | `PASS` | `Program.cs:1033,1067`; `BillingManagementServiceCollectionExtensions.cs:116` |
| Review manual: `git diff` tiga berkas yang diubah | Hanya perubahan yang disengaja task ini; nol perubahan sampingan pada berkas lain | `PASS` | `git diff -- <3 berkas>` |
| Verifikasi fungsional: filter mengembalikan data `EMPLOYEE_BENEFIT` non-kosong (`L.6.4`) | **TIDAK DAPAT dijalankan** — memerlukan aplikasi berjalan dan data `FinReceivable.DebtorType = 'EMPLOYEE_BENEFIT'` nyata di database, keduanya bergantung pada `BE-FIN-091` yang belum pernah di-build/dijalankan | `NOT FEASIBLE` | — |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).`

---

## 5. Acceptance criteria dan Definition of Done

| Kriteria | Status |
| --- | --- |
| `L.6.4` — Saring daftar piutang dan tiga laporan AR dengan jenis manfaat karyawan, hasil berisi bukan nol | **Terpenuhi struktural** untuk keempat permukaan (daftar, ringkasan, aging, rekapitulasi). Verifikasi runtime non-kosong `NOT FEASIBLE` tanpa build dan data nyata (lihat §4) |

**DoD roadmap**: **belum terpenuhi** — `dotnet build` sengaja tidak dijalankan, dan verifikasi fungsional non-kosong belum dapat dibuktikan.

---

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Source belum pernah dikompilasi. Ini task ke-10 berturutan pada epik `REV-18` tanpa satu pun build — risiko compile majemuk (`BE-FIN-091`..`100`) kini mencakup seluruh rentang task epik ini |
| Masalah yang diketahui | Interpretasi "tiga laporan AR" (§1) — **MUST dikonfirmasi Finance Owner** |
| Risiko tersisa | Fungsionalitas filter EMPLOYEE_BENEFIT bergantung penuh pada `BE-FIN-091` (intake yang benar-benar menulis `DebtorType = EMPLOYEE_BENEFIT`) sudah dibangun dan berjalan — task ini hanya menambah **jalur baca**, bukan sumber datanya |
| Perubahan sampingan | `NONE` — perbaikan wiring `GetAging` pada `FinanceArController` adalah bagian sah dari cakupan task ini (kemampuan identik, bukan fitur baru), bukan perubahan di luar cakupan |
| Interupsi | `NONE` |
| Status Git | 3 berkas diubah: `FinanceReceivableService.cs`, `FinanceReceivablesController.cs`, `FinanceArController.cs`. Catatan: `git status --short` repository ini juga menunjukkan perubahan terakumulasi dari `BE-FIN-091`..`099` yang belum pernah di-build maupun di-commit — bukan bagian task ini |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` untuk seluruh rentang `BE-FIN-091`..`100`; ini adalah **task terakhir** pada rentang `BE-FIN-091`..`100` epik `REV-18` "Piutang Manfaat Karyawan" |
