# Laporan Perubahan Backend — `BE-FIN-096`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-096` |
| Judul | Layanan perhitungan status bebas tanggungan perorangan dan kolektif tersedia untuk konsumsi Finance dan Exit Clearance HR |
| Slice | `REV-18B2` — Perjanjian Cicilan, Clearance, Pelunasan & Migrasi |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian `AMENDMENT ROADMAP REVISI 18` |
| Trace | `FR-FIN-206`..`210`, `228`..`230`; `FIN-DEC-170`, `193`; `FIN-DES-101` |
| Contract version | `FIN-API-1.9` O.4; `FIN-INTEGRATION-1.8` §P.4; `FIN-VAL-1.10` `FIN-VAL-244`, `245` |
| Dependency | `BE-FIN-091` 🟡 — belum ✅. Tidak genap bermasalah untuk task ini: `FinReceivable.BenefitOwnerId`/`DebtorType` yang dipakai sudah ada sejak `BE-FIN-079`, bukan hasil `BE-FIN-091` |
| Klasifikasi | `LIGHT` — murni baca, nol transaksi, nol entity baru, nol migration. 2 endpoint menempel pada controller existing, nol resource hak akses baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/{Controllers,DTOs,Services}/`, `Program.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 10 Oktober 2026 |
| Status | 🟡 **Source selesai ditulis.** `dotnet build` **TIDAK DIJALANKAN**. Task paling ringan dan paling sedikit asumsi pada sesi ini — murni mengikuti kontrak `FIN-API-1.9` O.4 apa adanya, nol keputusan desain baru yang perlu diresmikan |

---

## 0. Backend Governance Preflight

**Area/Module:** `Corporate` / `FinanceManagement/Receivable`, prefix `Fin`, `ACTIVE`. **Applicability:** `NEW CODE` penuh untuk `FinanceReceivableClearanceService` dan 2 endpoint baru; `TOUCHED LEGACY` minimal pada `FinanceReceivablesController.cs` (menambah constructor parameter + 2 action, nol perubahan pada endpoint existing).

**QBE ID yang berlaku:** QBE-SVC-001 (controller tidak menyentuh `ApplicationDbContext`, seluruh query lewat service), QBE-DTO-001 (`EmployeeClearanceResponse` bukan entity, murni hasil hitung), QBE-PERM-001 (`[AccessAction]`/`[AccessPermission]` dipakai ulang persis dari endpoint `Read` existing — `ControllerName` sama, nol resource baru, sesuai `02-backend-architecture.md` O.5.9).

**Kenapa task ini tidak benar-benar tersandera `BE-FIN-091` 🟡.** Dependency roadmap menyebut `BE-FIN-091`, tetapi field yang dipakai (`FinReceivable.BenefitOwnerId`, `.DebtorType`, `FinReceivableDebtorTypes.EmployeeBenefit`, `FinReceivableStatuses.Outstanding/Partial`) semuanya sudah ada di source sejak `BE-FIN-079` (jauh sebelum sesi ini) — diverifikasi langsung ke `FinReceivable.cs`. `BE-FIN-091` hanya menambah JALUR PENGISIAN data itu dari Billing; skema dan nilainya sendiri sudah berdiri independen. Risiko compile majemuk tetap ada (lihat bagian 3), tetapi bukan karena task ini bergantung pada kebenaran `BE-FIN-091`.

---

## 1. Proses bisnis

**Pemakai:** Finance (layar piutang pegawai) dan HR Exit Clearance (`TrxExitClearance.IsFinanceCleared`, `FIN-DEC-193`).

**Alur (murni baca, nol mutasi):**
1. `GET /receivables/clearance/{benefitOwnerId}` — satu pegawai. `GET /receivables/clearance?benefitOwnerIds=...` — hingga 100 pegawai sekaligus (`FIN-VAL-245`, ditegakkan `[MaxLength(100)]` pada DTO, `400` otomatis oleh `[ApiController]` bila dilanggar).
2. Untuk setiap `BenefitOwnerId`: jumlahkan `OutstandingAmount` seluruh `FinReceivable` dengan `DebtorType = EMPLOYEE_BENEFIT`, milik `BenefitOwnerId` itu, berstatus `OUTSTANDING` atau `PARTIAL` (piutang "aktif"). Piutang `SETTLED`/`WRITTEN_OFF`/`CANCELLED` tidak ikut (`L.4.4`). Piutang `PAYER` (porsi benefit RS) tidak pernah ikut — disaring lewat `DebtorType`, bukan `BenefitOwnerId` saja (`FIN-DEC-177`).
3. `IsCleared = (OutstandingAmount == 0)` — satu-satunya definisi, dihitung setiap kali, tidak pernah dari kolom tersimpan (`FIN-DES-101`, `FIN-VAL-244`).
4. Pegawai tanpa piutang sama sekali tetap `200` dengan `IsCleared = true` — bukan `404` (`L.4.1`).
5. Perjanjian cicilan yang masih berjalan otomatis ikut terhitung **tanpa logika khusus** — karena `FinReceivable.OutstandingAmount` sendiri sudah mencerminkan sisa setelah potongan payroll (`BE-FIN-095`), jumlahnya sudah benar hanya dengan membaca kartu piutangnya (`L.4.5`).

---

## 2. Perubahan yang dikerjakan

### 2.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/api-contract.md` O.4 (dibaca sebelumnya, dikonfirmasi ulang)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` `FIN-VAL-244`, `245`
- `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` `M.3.1`..`M.3.2`, `L.4.1`..`L.4.9`
- `docs/module-blueprints/finance-management/02-backend-architecture.md` O.5.8-O.5.9 (catatan desain service + alasan menempel pada controller existing)
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` (pola `[AccessAction]`/`[AccessPermission]` existing, dipakai ulang persis)
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` (konfirmasi `BenefitOwnerId`/`DebtorType` sudah ada sejak `BE-FIN-079`)

### 2.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceReceivableClearanceDtos.cs` | **Baru.** `ClearanceBatchQuery`, `EmployeeClearanceResponse` |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableClearanceService.cs` | **Baru.** `GetAsync`, `GetBatchAsync` — murni baca, nol transaksi |
| `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` | Constructor menerima `FinanceReceivableClearanceService`; 2 action baru (`GetClearance`, `GetClearanceBatch`), hak akses dipakai ulang dari endpoint `Read` existing |
| `Program.cs` | `AddScoped<FinanceReceivableClearanceService>()` |

### 2.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 2 endpoint baru pada controller existing, persis `FIN-API-1.9` O.4 |
| Database | `NOT APPLICABLE` — nol entity, nol migration, nol kolom baru |
| Keamanan/Auth | **Nol resource hak akses baru** (`L.4.9`) — memakai `FinanceReceivable : Read` yang sudah terdaftar. Respons tidak memuat rincian layanan/diagnosis (`FIN-DEC-172`), hanya agregat nominal |

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pengguna |
| Review diff manual terhadap `L.4.1`..`L.4.9` satu per satu | Seluruh skenario dipetakan ke source (lihat bagian 4) | `PASS` (review manual, bukan compiler) | §1, §4 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).`

**Risiko compile** tetap ada sebagai bagian dari 6 task berturutan (`BE-FIN-091`..`096`) yang belum dikompilasi — tetapi task ini sendiri adalah yang paling sederhana dan paling kecil kemungkinan menyumbang error (nol entity baru, nol query kompleks, pola yang disalin persis dari endpoint existing di file yang sama).

Uji manual: `NOT FEASIBLE` — butuh build sukses dan data piutang nyata.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `M.3.1`/`L.4.2` Pegawai berpiutang aktif → tidak bebas | **Terpenuhi struktural** | `GetBatchAsync` |
| `M.3.2`/`L.4.1` Pegawai tanpa piutang → `200` bebas, bukan `404` | **Terpenuhi struktural** — `ids.Select(...)` selalu menghasilkan satu baris per id diminta, terlepas ada tidaknya baris aktif | `GetBatchAsync` |
| `L.4.3` Piutang keluarga (SPOUSE/SELF) ikut terhitung pada pemilik yang sama | **Terpenuhi struktural** — disaring murni dari `BenefitOwnerId`, bukan hubungan keluarga | `GetBatchAsync` |
| `L.4.4` Lunas/dihapus-buku/dibatalkan tidak menahan | **Terpenuhi struktural** — filter eksplisit `Outstanding`/`Partial` saja | `GetBatchAsync` |
| `L.4.5` Cicilan berjalan menahan status | **Terpenuhi struktural**, otomatis lewat `OutstandingAmount` kartu piutang | §1 butir 5 |
| `L.4.6` Pegawai berhenti kerja, sisa tidak terhapus | **Terpenuhi by design** — task ini tidak pernah menulis apa pun | — |
| `L.4.7` Status tidak dapat diisi tangan | **Terpenuhi** — nol field request yang memengaruhi `IsCleared`/`OutstandingAmount` | DTO `ClearanceBatchQuery` hanya memuat `BenefitOwnerIds` |
| `L.4.8` Maks 100, 101 ditolak `400` | **Terpenuhi struktural** — `[MaxLength(100)]` + `[ApiController]` otomatis | `ClearanceBatchQuery` |
| `L.4.9` Nol resource baru, `403` tanpa hak | **Terpenuhi struktural** — `AccessPermission("FinanceReceivable", "Read")` dipakai ulang | Controller |

**DoD roadmap** ("Build PASS, unit test PASS, laporan task tracked"): **belum terpenuhi** — `dotnet build` sengaja tidak dijalankan.

---

## 5. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Source belum pernah dikompilasi (bagian dari 6 task berturutan) |
| Masalah yang diketahui | `NONE` spesifik task ini |
| Risiko tersisa | Risiko compile majemuk 6 task, bukan risiko baru dari task ini sendiri |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short`: 2 file baru (DTO, Service), `FinanceReceivablesController.cs` diperluas, `Program.cs` dimodifikasi |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` untuk memvalidasi `BE-FIN-091`..`096` sekaligus. Task ini tidak punya langkah lanjutan khusus di luar itu — murni baca, siap dipakai begitu build bersih dan migration `BE-FIN-092` diterapkan |
