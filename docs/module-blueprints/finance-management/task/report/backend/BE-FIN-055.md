# Laporan Perubahan Backend — `BE-FIN-055`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-055` |
| Judul | Saringan jenis debitur pada umur piutang pasien — `GET /receivables/aging` |
| Slice | `REV-13B` — `EPIC FIN-18` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-094`, `FIN-OQ-040` (terjawab: Kasir) |
| Contract version | `FIN-API-1.4` (perluasan `ReceivableAgingQuery`) — `approved` 1 Oktober 2026 |
| Dependency | Tidak ada — berdiri sendiri pada grafik dependency `REV-13` |
| Klasifikasi | `LIGHT` — satu repository (skor 0); 3 berkas diubah (skor 0); nol model baru, satu parameter opsional pada query existing (skor 0); nol endpoint baru, murni perluasan endpoint yang sudah ada (skor 0); database — nol migration (skor 0); keamanan/auth — nol perubahan (skor 0); UI/workflow — murni teknis, nol alur baru (skor 0). Total 0 → `LIGHT` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/**` dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — branch `Yasmina` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **Selesai** 1 Oktober 2026 — **dengan koreksi rancangan yang MUST dibaca** (lihat §3.3, bukan blocker — nilai "KASIR" yang tidak ada pada data sudah diperbaiki di source maupun dokumen). `dotnet build` **PASS**, dikonfirmasi pengguna |

---

## 1. Masalah yang diperbaiki

**Sebelum menjelaskan perbaikannya, satu koreksi penting atas rancangan task ini sendiri.**
Rancangan `03-frontend-architecture.md` §17.2 (ditulis sebelum task ini) mengasumsikan ada
"segmen Kasir" yang bisa disaring pada `GET /receivables/aging`, dengan contoh
`?segment=KASIR`. **Asumsi itu keliru dan dikoreksi di sini**: model `FinReceivable` sama sekali
tidak punya nilai `"KASIR"` — jenis debitur yang benar-benar ada hanya tiga:
`PAYER`/`PATIENT_GUARANTOR`/`EMPLOYEE_BENEFIT` (`FinReceivableDebtorTypes`). Mengarang nilai
`"KASIR"` yang tidak pernah ada di data akan membuat saringan itu **selalu kosong** bila benar-benar
dipakai.

**Apa yang sebenarnya dibutuhkan, dibuktikan dari source:** menu V1 mengelompokkan umur piutang
jadi Kasir/Parkir/Tenant. Parkir dan Tenant sudah dipisah penuh ke entity baru
(`FinNonPatientReceivable`, `BE-FIN-056`/`057`) karena memang bukan piutang pasien. Begitu
keduanya dipisah, **seluruh isi `FinReceivable` yang tersisa, tanpa kecuali, adalah yang dimaksud
"Kasir"** pada menu V1 — bukan satu nilai `DebtorType` tertentu, melainkan keseluruhan tabel ini.
Artinya `GET /receivables/aging` yang sudah ada **sudah benar** melayani kebutuhan "Umur Piutang —
Kasir" **tanpa saringan apa pun**.

---

## 2. Proses bisnis

**Pelaku:** Petugas AR Finance.

**Pemicu:** Petugas membuka "Umur Piutang — Kasir" dari grup menu "Umur Piutang (A/R Aging)".

**Langkah normal:**

1. Petugas membuka layar; sistem memanggil `GET /receivables/aging` **tanpa parameter
   `DebtorType`** — persis seperti sebelum task ini, nol perubahan perilaku bagi layar ini.
2. Sistem menampilkan empat kelompok umur (0-30, 31-60, 61-90, di atas 90 hari) atas **seluruh**
   piutang pasien aktif (`OutstandingAmount > 0`).

**Yang ditambahkan task ini, sebagai kemampuan teknis — bukan untuk "Kasir":** parameter opsional
`DebtorType` pada `ReceivableAgingQuery`, menerima salah satu dari tiga nilai nyata
(`PAYER`/`PATIENT_GUARANTOR`/`EMPLOYEE_BENEFIT`). Ini kemampuan umum yang sah untuk kebutuhan masa
depan (mis. melihat umur piutang penjamin saja), **tidak dipakai** oleh butir menu "Kasir" mana
pun pada rilis ini.

**Jalur tidak normal:** `DebtorType` yang diisi nilai di luar tiga itu tidak menghasilkan error
eksplisit — query-nya hanya tidak akan mencocokkan baris mana pun (perilaku `Where` biasa). Tidak
divalidasi ketat karena task ini `LIGHT` dan parameter belum dipakai layar mana pun; divalidasi
nanti bila ada konsumen nyata.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/03-frontend-architecture.md` §17.2, §18.1 — **dua**
  baris "Umur Piutang — Kasir" ditemukan memuat asumsi `?segment=KASIR` yang keliru, dikoreksi
  sebagai bagian task ini (lihat §3.3)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` — dicek, **tidak** memuat
  rincian field `ReceivableAgingQuery` (hanya baris ringkas endpoint dari kontrak dasar
  `FIN-API-1.0`), sehingga tidak ada field kontrak yang perlu dikoreksi di sana
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` — **bukti utama temuan
  §1**: `FinReceivableDebtorTypes` hanya `Payer`/`PatientGuarantor`/`EmployeeBenefit`
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` —
  `GetAgingSummaryAsync` dibaca penuh; dikonfirmasi dipanggil dari dua tempat
  (`FinanceReceivablesController.GetAging` dan `GetSummaryAsync` baris 783) — perubahan signature
  **wajib** backward-compatible untuk caller kedua

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Receivable/Dtos/FinanceReceivableDtos.cs` | `ReceivableAgingQuery` bertambah satu field opsional `DebtorType` (`string?`), berdokumentasi eksplisit bahwa ini **bukan** "segmen Kasir" |
| `Areas/.../Receivable/Services/FinanceReceivableService.cs` | `GetAgingSummaryAsync` bertambah parameter opsional `string? debtorType = null` (bawaan `null` — caller `GetSummaryAsync` **tidak perlu diubah**, perilakunya identik dengan sebelum task ini); satu `Where` tambahan hanya aktif bila diisi |
| `Areas/.../Receivable/Controllers/FinanceReceivablesController.cs` | `GetAging` meneruskan `request.DebtorType` ke service; doc comment kelas diperbarui dengan koreksi "bukan segmen Kasir" |
| `docs/module-blueprints/finance-management/03-frontend-architecture.md` | **Dua** baris "Umur Piutang — Kasir" (§17.2, §18.1) dikoreksi: `pathname` dari `?segment=KASIR` menjadi tanpa parameter; keterangan dikoreksi menunjuk laporan ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif dan backward-compatible.** Parameter baru opsional; pemanggilan tanpa parameter ini **menghasilkan hasil identik** dengan sebelum task ini — dibuktikan lewat default `null` pada signature dan DTO |
| Database | **Nol.** Murni query filter tambahan. Nol migration |
| Keamanan/Auth | **Nol** perubahan. Endpoint dan resource hak akses (`FinanceReceivable : Read`) sama persis |
| **Koreksi rancangan** | `03-frontend-architecture.md` dikoreksi di dua tempat (bukan hanya source backend) karena rancangannya sendiri yang keliru berasumsi nilai `"KASIR"` ada pada data — **MUST** dibaca sebagai bagian penting task ini, bukan sekadar catatan sampingan |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Receivable

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/aging` | Umur piutang (**diperluas**: parameter `DebtorType` opsional, nilai `PAYER`/`PATIENT_GUARANTOR`/`EMPLOYEE_BENEFIT`) | `FinanceReceivable : Read` |

Endpoint ini **sudah ada** sejak kontrak dasar (`FIN-API-1.0`); task ini hanya memperluas query-nya.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil tanpa error | `PASS` | Dikonfirmasi pengguna, 1 Oktober 2026 |
| Review diff/scope | 3 berkas source + 1 berkas dokumen, persis sesuai koreksi §3.3 | `PASS` | `git status --short` Bagian 7 |
| Review kontrak API — nol regresi | `debtorType` bawaan `null`; `GetSummaryAsync` (caller lain) tidak diubah sama sekali; perilaku tanpa parameter identik dengan sebelum task ini | `PASS` | `git diff` menunjukkan `GetSummaryAsync` nol baris berubah |
| Review proses bisnis — tidak ada nilai "KASIR" yang dikarang | `FinReceivableDebtorTypes` dibaca langsung dari model, tiga nilai nyata dipakai | `PASS` | `Models/FinReceivable.cs` baris 80-85 |
| QBE preflight | Area `Corporate/Finance`, Module `FinanceManagement`, Submodule `Receivable`, prefix `Fin` `ACTIVE` | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

Uji manual: `NOT FEASIBLE` — server tidak dijalankan pada task ini.

**AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).**

**Tidak dijalankan:** uji manual/runtime; perbandingan angka sebelum-sesudah secara runtime
(server tidak dijalankan — tetapi secara source terbukti nol regresi karena parameter bawaan
`null` tidak mengubah query sama sekali).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Saringan dikirim ke backend dan memengaruhi angka | Terpenuhi — **untuk nilai `DebtorType` yang nyata**, bukan "KASIR" yang tidak ada | `GetAgingSummaryAsync` menerapkan `Where` saat diisi |
| Tanpa saringan, hasil sama persis dengan hari ini (nol regresi) | Terpenuhi | Parameter bawaan `null`; `GetSummaryAsync` tidak tersentuh |
| Build PASS | Terpenuhi | `dotnet build` PASS, dikonfirmasi pengguna 1 Oktober 2026 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Rancangan task ini sendiri keliru berasumsi nilai `"KASIR"` ada pada `FinReceivable.DebtorType`.** Dikoreksi di source maupun di `03-frontend-architecture.md` (dua lokasi). "Umur Piutang — Kasir" pada menu V1 **MUST** dipahami sebagai "seluruh `FinReceivable`, tanpa saringan apa pun" — bukan satu nilai `DebtorType` tertentu |
| Masalah yang diketahui | Tidak ada temuan lain |
| Risiko tersisa | **Rendah.** Build sudah dibuktikan PASS. Risiko regresi pada `GetSummaryAsync` sangat rendah karena signature baru memakai default parameter |
| Perubahan sampingan | `docs/module-blueprints/finance-management/03-frontend-architecture.md` dikoreksi (dua baris) — **disengaja**, bagian dari task ini, bukan di luar cakupan, karena rancangan itu sendiri yang menjadi sumber kekeliruan yang diperbaiki |
| Interupsi | `NONE` |
| Status Git | `git status --short` mencakup tiga berkas source task ini ditambah satu berkas dokumen blueprint |
| Langkah berikutnya | Tidak ada — task ini selesai |
