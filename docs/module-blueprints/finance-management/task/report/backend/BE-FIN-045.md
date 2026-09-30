# Laporan Perubahan Backend — `BE-FIN-045`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-045` |
| Judul | Accounting tahu kapan satu shift kasir benar-benar tertutup, dan kapan ia dibuka kembali |
| Slice | `POST-MVP` (`REV-6/8`) — Penanda shift kasir |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) |
| Trace | `FIN-DEC-070`, `072`, `073`, `075`; `FIN-DES-054`, `FIN-DES-059`; `FR-FIN-108`..`110` |
| Contract version | `FIN-INTEGRATION-1.6` §5.10-5.11; `FIN-VAL-1.5` `FIN-VAL-138`; `FIN-TEST-1.6` §D.3, §E.1 |
| Dependency | `BE-FIN-023` ✅, `BE-FIN-025` ✅ — keduanya selesai, gerbang terbuka |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); >8 berkas dibaca lintas kontrak/desain/source Billing (skor 1); 2 berkas diubah (skor 0); logika bisnis sedang — versioning per siklus dan empat keadaan shift yang berbeda perlakuannya (skor 2); kontrak API — satu endpoint baru memakai pola yang sudah ada (skor 1); database — **nol** (skor 0); keamanan/auth — nol resource/aksi baru (skor 0); UI/workflow — nol (skor 0). Total 4 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/BillingIntake/{Services,Controllers}`, `docs/module-blueprints/finance-management/roadmap/**` (tanda status) |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Belum di-commit — working tree `Yasmina`, HEAD `7811c048` |
| Tanggal | 29 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `dotnet build` **NOT RUN** (aturan berdiri "jangan build otomatis"). **Nol migration** — task ini tidak menyentuh skema sama sekali |

---

## 1. Masalah yang diperbaiki

Accounting tidak punya cara mengetahui kapan satu shift kasir benar-benar tertutup. Tanpa penanda
itu, penegakan `ACC-DEC-065` (tutup bulan hanya boleh berjalan bila seluruh shift periode itu sudah
tertutup) tidak punya sumber fakta, dan tutup bulan tertahan tanpa sebab yang terlihat.

Yang membuat ini tidak sepele: shift kasir boleh **dibuka kembali** sesudah ditutup. Tanpa penanda
pembalik, Accounting akan terus menganggap shift itu tertutup dan mengizinkan tutup bulan atas
periode yang sebenarnya sudah kembali terbuka.

**Contoh konkret.** Kasir Ani menutup shift 20 November 2026 dengan kas pas. Shift berhenti di
`CLOSED` — ia tidak pernah melewati `REVIEWED` karena tidak ada selisih untuk disahkan. Sebelum task
ini, **mayoritas shift ada di keadaan ini** dan nol penanda terbit untuk mereka. Sesudah task ini,
satu `PENUTUPAN-SHIFT-KASIR` bernilai `0` terbit untuk shift itu. Bila keesokan harinya supervisor
membuka kembali shift itu untuk koreksi, satu `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` bernilai `0` terbit,
dan ketika ditutup lagi penanda penutupan **siklus kedua** boleh terbit tanpa bertabrakan dengan yang
pertama.

---

## 2. Proses bisnis

**Pelaku:** Petugas Finance yang memantau fakta dari Billing (aksi `Sync` yang sudah ada).

**Langkah normal:**

1. Kasir menutup shift di modul Billing. Shift mencapai `CLOSED` (kas pas) atau — bila ada selisih
   yang kemudian disahkan — `REVIEWED`.
2. Petugas Finance menjalankan sinkronisasi penanda
   (`POST /billing-intake/cashier-shift-closure-markers/sync`).
3. Backend membaca shift kasir (**baca saja**), lalu menerbitkan satu `PENUTUPAN-SHIFT-KASIR`
   bernilai `0` untuk setiap shift tertutup yang belum punya penanda pada siklus berjalan.
4. Bila shift kemudian dibuka kembali (`REOPENED`), sinkronisasi berikutnya menerbitkan satu
   `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` bernilai `0`.
5. Ketika shift itu ditutup lagi, penanda penutupan **siklus berikutnya** terbit — nomor siklusnya
   diambil dari jumlah penanda pembalik yang sudah terbit untuk shift itu.

**Jalur yang sengaja TIDAK menerbitkan apa pun:**

- Shift `CLOSED_WITH_VARIANCE` → nol penanda. Selisihnya belum disahkan, dan shift ini memang
  **harus** tetap menahan tutup bulan.
- Shift `PERLU_TINDAK_LANJUT` → nol penanda, alasan yang sama.
- Shift `REOPENED` yang belum pernah menerbitkan penanda penutupan → nol pembalik. Tidak ada yang
  perlu dibalik.
- Sinkronisasi dijalankan dua kali → nol baris kedua. Jalur ini idempoten.

**Hasil akhir:** baris kejadian tersimpan berstatus `PENDING`. Pengirimannya ke Accounting **belum
aktif** dan memang belum boleh aktif — lihat bagian 7.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `roadmap/01-backend-roadmap.md` (kartu `BE-FIN-045`)
- `02-backend-architecture.md` §E.4 `FIN-DES-054` (pemicu, nilai, versioning, kode pembalik),
  §E.1 temuan A dan temuan 5, §E.7 (tabel perubahan class)
- `contracts/validation-matrix.md` `FIN-VAL-138`; `contracts/integration-contract.md` §5.10
- `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShift.cs` — tujuh nilai status,
  `OpenedAt`/`ClosedAt`; **dibaca saja**
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` —
  `SyncNewFactsAsync` (pola penemuan fakta) dan `ProcessCashVarianceReviewIntakeAsync` (**preseden
  langsung**: konvensi `SourceTransactionId` = `shift.Id`, `AccountingDate` = tanggal `OpenedAt`,
  `CorrelationId` = `shift.Id`)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` —
  konstanta `PenutupanShiftKasir`/`PembalikanPenutupanShiftKasir` dan daftar tertutup
  `ZeroAmountAllowedEventTypes` — **keduanya sudah ada** dari `BE-FIN-023`, tidak disalin ulang
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs`
  — `ValidateRequest` (penerimaan `Amount = 0`) dan `ResolveNextSourceVersionAsync`
- `Areas/Corporate/FinanceManagement/BillingIntake/Models/FinBillingHandoffIntake.cs` — delapan nilai
  `HandoffType` beserta check constraint-nya

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../BillingIntake/Services/FinanceBillingIntakeService.cs` | **Baru**: `SyncCashierShiftClosureMarkersAsync` + record hasil `CashierShiftClosureMarkerSyncResult`. Nol method lama diubah |
| `.../BillingIntake/Controllers/FinanceBillingIntakeController.cs` | **Baru**: `POST /cashier-shift-closure-markers/sync`, memakai aksi `Sync` yang sudah ada |
| `roadmap/01-backend-roadmap.md`, `roadmap/00-delivery-roadmap.md` | Tanda status `BE-FIN-045` → 🟡 |

### 3.3 Keputusan bentuk teknis, dan kenapa bentuk lain ditolak

Desain (`FIN-DES-054`) menetapkan pemicu, nilai, kunci, dan aturan versinya dengan tepat, tetapi
**tidak menetapkan dari mana penanda ini ditulis** — `§E.7` tidak menyebut satu pun class sebagai
penulisnya. Bentuk berikut dipilih, dan alasannya dicatat supaya dapat ditinjau:

| Bentuk | Dipakai? | Alasan |
| --- | :---: | --- |
| Kotak keluar langsung, idempotensi dari unique index, dipicu endpoint sinkronisasi | **Ya** | Memenuhi seluruh ketentuan `FIN-DES-054` tanpa menyentuh skema. Idempotensinya bukan buatan sendiri — ia jatuh dari unique index dua lapis `(SourceModule, SourceTransactionId, EventTypeCode, SourceVersion)` yang sudah ada, begitu `SourceVersion` dipatok per siklus |
| Baris `FinBillingHandoffIntake` + jenis handoff baru `CASHIER_SHIFT` | **Tidak** | Dua sebab. **(a)** Kunci dedup intake adalah `(HandoffType, SourceHandoffKey)` — satu baris per sumber **selamanya**. Padahal satu shift memang boleh ditutup dan dibuka berulang, dan setiap siklus adalah kejadian tersendiri; kunci itu justru menghalangi siklus kedua. **(b)** Jenis handoff baru menuntut perubahan check constraint `HandoffType` — yaitu migration pada tabel yang sudah berjalan, dan itu **tidak ada** pada cakupan kartu task ini maupun pada kolom risikonya (berbeda dari `BE-FIN-043` yang memang mensyaratkannya eksplisit) |
| Menulis penanda dari `CashierShiftService.CloseAsync`/`ReopenAsync` | **Tidak** | Itu service milik Billing. Finance tidak menulis ke kode maupun tabel Billing (`FIN-OOS-001`..`004`) |

**Nomor siklus diturunkan dari isi kotak keluar, bukan dari kolom status baru:**

| Keadaan shift | Yang diterbitkan | `SourceVersion` |
| --- | --- | --- |
| `CLOSED` / `REVIEWED` | `PENUTUPAN-SHIFT-KASIR` | jumlah pembalik yang sudah terbit **+ 1** |
| `REOPENED` | `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` | jumlah pembalik yang sudah terbit **+ 1** |

Untuk shift yang ditutup sekali dan tidak pernah dibuka kembali — mayoritas kasus — nol pembalik
sudah terbit, sehingga versinya `"1"`, persis seperti yang ditulis `FIN-DES-054`.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru, `POST /billing-intake/cashier-shift-closure-markers/sync`. **Delta kontrak** — `FIN-API` belum memuatnya karena kontrak tidak pernah menetapkan permukaan teknis jalur ini. Bentuknya meniru `POST /billing-intake/sync` yang sudah ada; nol aturan bisnis baru diarang |
| Database | **NOL.** Nol tabel, nol kolom, nol migration, nol check constraint. Nol tulisan ke tabel `Bil*` mana pun — shift kasir dibaca `AsNoTracking` |
| Keamanan/Auth | **Nol resource dan nol aksi baru.** Endpoint memakai `[AccessPermission("FinanceBillingIntake", "Sync")]` — resource kanonikal hasil `BE-FIN-042` dan aksi `Sync` yang sudah terdaftar. Nol `IsInRole`/nama peran/departemen/`UserType` hardcode |
| Perubahan pada kode berjalan | **Nol.** Method baru berdiri sendiri; nol baris pada `SyncNewFactsAsync`, `ProcessAsync`, maupun jalur intake mana pun yang disentuh |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Billing Intake

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/cashier-shift-closure-markers/sync` | Menerbitkan penanda penutupan shift kasir dan penanda pembaliknya untuk shift yang dibuka kembali. Idempoten | `FinanceBillingIntake : Sync` | — | `ApiResponse<CashierShiftClosureMarkerSyncResult>` | `200` |

**Contoh response:**

```json
{
  "success": true,
  "message": "3 penanda penutupan dan 1 penanda pembalik diterbitkan.",
  "data": { "closureIssued": 3, "reversalIssued": 1 }
}
```

Base URL: `api/v1/corporate/finance-management/billing-intake`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | — | `NOT RUN` | Aturan berdiri pengguna: jangan build otomatis |
| Shift `CLOSED` (kas pas) | Satu `PENUTUPAN-SHIFT-KASIR`, `Amount = 0`, versi `"1"` | `NOT RUN` (review kode) | Cabang `Closed or Reviewed` |
| Shift `REVIEWED` (selisih disahkan) | Satu penanda penutupan — **dan** `SELISIH-KAS-*` tetap terbit dari jalur `BE-FIN-025` yang terpisah | `NOT RUN` (review kode) | Dua jalur tidak saling menyentuh |
| Shift `CLOSED_WITH_VARIANCE` | **Nol** penanda | `NOT RUN` (review kode) | Penyaring status pada query |
| Shift `PERLU_TINDAK_LANJUT` | **Nol** penanda | `NOT RUN` (review kode) | Idem |
| Shift dibuka kembali (`REOPENED`) | Satu `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`, `Amount = 0` | `NOT RUN` (review kode) | Cabang `else` |
| Ditutup lagi sesudah dibuka | Penanda penutupan siklus ke-2 terbit, tidak bertabrakan dengan siklus ke-1 | `NOT RUN` (review kode) | `SourceVersion = reversalCount + 1` |
| `REOPENED` tanpa pernah ada penanda penutupan | **Nol** pembalik | `NOT RUN` (review kode) | `if (closureCount <= reversalCount) continue;` |
| Sinkronisasi dijalankan dua kali | Nol baris kedua | `NOT RUN` (review kode) | Perbandingan hitungan + unique index outbox sebagai penjaga akhir |
| `Amount = 0` diterima layanan outbox | Diterima — kode ini ada pada daftar tertutup `ZeroAmountAllowedEventTypes` | `NOT RUN` (review kode) | `ValidateRequest` (`FIN-VAL-138`, `BE-FIN-023`) |
| Nol tulisan ke tabel `Bil*` | Terpenuhi — shift dibaca `AsNoTracking`, nol `Add`/`Update` | `NOT RUN` (review diff) | Review method menyeluruh |

Uji manual/runtime: `NOT FEASIBLE` — memerlukan aplikasi berjalan beserta data shift kasir.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, seluruh uji runtime/manual, dan `Invoke-QbeConformanceCheck.ps1`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (kartu roadmap) | Status | Bukti |
| --- | --- | --- |
| Shift `CLOSED` kas pas → satu penanda bernilai `0` (skenario paling mudah terlewat) | Terpenuhi (source) | Cabang `Closed or Reviewed` |
| Shift `REVIEWED` → satu penanda **dan** satu `SELISIH-KAS-*` | Terpenuhi (source) | Jalur ini + jalur `BE-FIN-025` yang tidak disentuh |
| Shift `CLOSED_WITH_VARIANCE` → nol penanda | Terpenuhi (source) | Penyaring status |
| Shift `PERLU_TINDAK_LANJUT` → nol penanda | Terpenuhi (source) | Penyaring status |
| Shift dibuka kembali → satu pembalik bernilai `0`; penutupan berikutnya boleh terbit lagi | Terpenuhi (source) | Versioning per siklus |
| `Amount = 0` tidak diakali menjadi nilai simbolis | Terpenuhi | Nilai literal `0m`; `FIN-DEC-075` dihormati |
| Daftar kode penanda tetap satu tempat, tidak disalin | Terpenuhi | `ZeroAmountAllowedEventTypes` milik `BE-FIN-023` dipakai apa adanya |
| Nol worker pengiriman diaktifkan | Terpenuhi | Nol worker dibangun task ini |
| Selama gerbang `FIN-OQ-035` tertutup, baris tetap `PENDING`, `AttemptCount` `0`, tidak `FAILED` | Terpenuhi **secara struktural** | Nol worker yang dapat mengubahnya — lihat bagian 7 |
| `dotnet build` berhasil | **Belum** | Sengaja `NOT RUN` |
| `FIN-TEST-1.6` §D.3 dan §E.1 | **Belum dijalankan** | Memerlukan aplikasi berjalan |

Task ini **belum** dapat ditandai `✅` — dua butir validasi terakhir belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Gerbang keras yang MASIH BERLAKU | Worker pengiriman kedua kode ini **MUST NOT** diaktifkan sebelum `FIN-OQ-035` dijawab Accounting (`FIN-DES-059`). Hari ini gerbang itu terpenuhi **secara struktural**: nol worker pengiriman ada di codebase, sehingga baris tetap `PENDING` dengan `AttemptCount` `0` dan tidak mungkin ditandai `FAILED`. Ketika worker kelak dibangun, ia **MUST** memuat gerbang `FIN-VAL-132` — dan itu tanggung jawab task worker, bukan task ini |
| Keputusan yang dihormati apa adanya | `FIN-DEC-075` secara eksplisit **menolak** jalan pintas memakai nilai simbolis non-nol. Penanda ditulis bernilai `0` literal. Angka palsu di buku besar lebih berbahaya daripada baris `PENDING` yang menunggu |
| Keterbatasan yang diketahui | `BilCashierShift` **tidak menyimpan waktu pembukaan kembali**. Karena itu `EventOccurredAt` penanda pembalik memakai waktu sinkronisasi dijalankan, bukan waktu shift benar-benar dibuka kembali. `AccountingDate`-nya tetap tanggal shift, sehingga periode akuntansinya benar. Ditulis apa adanya, bukan ditebak dari kolom lain; bila ketepatan waktu itu kelak dibutuhkan, ia menuntut kolom baru di sisi Billing — permintaan lintas modul, bukan perbaikan dari Finance |
| Keterbatasan kedua | Jalur ini dipicu manual lewat endpoint, sama seperti `POST /billing-intake/sync` yang sudah ada — belum ada penjadwal. Keterbatasan ini sudah tercatat pada kode `SyncNewFactsAsync` sejak `BE-FIN-009` dan tidak diperluas task ini |
| Delta kontrak | Satu endpoint baru yang belum tercatat `FIN-API`. Kontrak tidak pernah menetapkan permukaan teknis jalur ini; bentuknya meniru endpoint sinkronisasi yang sudah ada, dan nol aturan bisnis baru diarang. Dicatat di sini untuk ditinjau pemilik kontrak |
| Risiko tersisa | Nol perubahan pada kode yang sudah berjalan — method baru berdiri sendiri, nol baris jalur intake yang disentuh |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` dua berkas (service, controller) + `??` laporan ini |
| Langkah berikutnya | (1) `dotnet build`; (2) uji runtime `FIN-TEST-1.6` §D.3 dan §E.1; (3) `BE-FIN-046` dan `BE-FIN-047` bebas dikerjakan — keduanya tidak bergantung pada task ini |
