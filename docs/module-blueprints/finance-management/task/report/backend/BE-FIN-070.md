# Laporan Task Backend — BE-FIN-070

**Tanggal Selesai:** 2 Oktober 2026
**Branch:** `Yasmina`
**Penulis Laporan:** Antigravity (build-module-backend)

---

## 1. Identitas Task

| Kolom | Isi |
|---|---|
| **Task ID** | `BE-FIN-070` |
| **Gelombang** | `REV-14C` — EPIC FIN-22, jalur pengiriman |
| **Outcome** | Accounting dapat mengetahui shift mana yang belum selesai, sehingga penegakan tutup bulan tidak punya celah |
| **Requirement** | `FR-FIN-157`; `FIN-DEC-115`, `FIN-DEC-121`; `FIN-DES-084` |
| **Kontrak** | `FIN-INTEGRATION-1.7` §5.12.3; `FIN-STATE-1.6` F.3 |
| **Dependency** | `BE-FIN-062` ✅ |

---

## 2. Backend Governance Preflight

| Dimensi | Nilai |
|---|---|
| **Area** | Corporate |
| **Module** | FinanceManagement |
| **Submodule** | BillingIntake |
| **Prefix** | `Fin` — `ACTIVE` (catatan registry 21 September 2026) |
| **Berlakunya** | `TOUCHED LEGACY` — menyentuh `FinanceBillingIntakeService.SyncCashierShiftClosureMarkersAsync` yang sudah berjalan sejak `BE-FIN-045` |
| **File model baru** | **Nol** |
| **Migration** | **Nol** (nol perubahan skema database) |
| **QBE berlaku** | QBE-NAM-001, QBE-NAM-004 |
| **Gerbang** | `FIN-OQ-047` hanya menahan **pengiriman** oleh worker; pembangunan tidak tertahan |

---

## 3. Deskripsi Perubahan

### 3.1 Konteks As-Is

Sebelum task ini, `SyncCashierShiftClosureMarkersAsync` hanya memproses **3 status** shift:
- `CLOSED` — menerbitkan `PENUTUPAN-SHIFT-KASIR`
- `REVIEWED` — menerbitkan `PENUTUPAN-SHIFT-KASIR`
- `REOPENED` — menerbitkan `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`

Status `OPEN`, `HANDED_OVER`, `CLOSED_WITH_VARIANCE`, dan `PERLU_TINDAK_LANJUT` tidak terlihat oleh Accounting, sehingga tutup bulan dapat dilakukan meski ada shift yang tertinggal terbuka.

### 3.2 Keputusan Desain yang Diterapkan

**`FIN-DEC-115` dan `FIN-DEC-121`** menambahkan kode `PEMBUKAAN-SHIFT-KASIR`:
- Diterbitkan saat Finance pertama kali melihat shift dengan status **belum final**
- "Belum final" = `OPEN`, `HANDED_OVER`, `REOPENED`, `CLOSED_WITH_VARIANCE`, `PERLU_TINDAK_LANJUT`
- Accounting menahan tutup bulan selama ada `PEMBUKAAN-` tanpa pasangan `PENUTUPAN-` pada siklus yang sama

**`FIN-DES-084`** menetapkan pola siklus yang sama persis dengan penanda penutupan:
- `SourceVersion` = nomor siklus = jumlah `PEMBALIKAN-PENUTUPAN` yang sudah terbit + 1
- Idempotensi dijaga dengan `HashSet` kunci `"<shiftId>:<SourceVersion>"`

### 3.3 Perubahan Siklus Lengkap (REOPENED)

Saat shift ber-status `REOPENED`, urutan penerbitan:
1. **Terbitkan `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR`** untuk siklus lama (perilaku lama, dipertahankan)
2. **Terbitkan `PEMBUKAAN-SHIFT-KASIR`** untuk siklus baru (perilaku baru BE-FIN-070)

---

## 4. Berkas yang Diubah

| Berkas | Perubahan |
|---|---|
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` | Menambahkan konstanta `PembukaanShiftKasir = "PEMBUKAAN-SHIFT-KASIR"` dan mendaftarkannya ke `ZeroAmountAllowedEventTypes` |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | Memperluas query dari 3 ke 7 status; menambahkan blok penerbitan `PEMBUKAAN-SHIFT-KASIR`; memperbarui `CashierShiftClosureMarkerSyncResult` dengan parameter `OpeningIssued` |

---

## 5. Validasi Acceptance Criteria

| Acceptance Criteria | Status | Bukti |
|---|---|---|
| Shift `CLOSED_WITH_VARIANCE` yang belum pernah terlihat menerbitkan penanda pembukaan | **Terpenuhi** | Query diperluas mencakup `ClosedWithVariance`; blok `else` menangani status belum final |
| Shift `CLOSED` tidak menerbitkan penanda pembukaan | **Terpenuhi** | Blok `if (Closed or Reviewed)` hanya menerbitkan `PENUTUPAN-SHIFT-KASIR` |
| Shift `REOPENED` menerbitkan pembalik siklus lama sebelum pembukaan siklus baru | **Terpenuhi** | Urutan dalam blok `else if (Reopened)`: pembalikan diterbitkan dahulu, pembukaan sesudahnya |
| Sinkronisasi berulang tidak menggandakan penanda | **Terpenuhi** | `openingMarkerKeys` menggunakan composite key `"<shiftId>:<SourceVersion>"`; unique index outbox sebagai lapisan kedua |
| Kode `PEMBUKAAN-SHIFT-KASIR` terdaftar di `ZeroAmountAllowedEventTypes` | **Terpenuhi** | `FinAccountingEventOutbox.cs`: `ZeroAmountAllowedEventTypes` memuat `PembukaanShiftKasir` |

---

## 6. Status Migration dan Database

- **Nol migration baru** — task ini tidak mengubah skema database.
- Penanda disimpan pada tabel `FinAccountingEventOutbox` yang sudah ada.

---

## 7. Keterbatasan yang Dicatat

| # | Keterbatasan | Keputusan |
|---|---|---|
| 1 | `BilCashierShift` tidak menyimpan `ReopenedAt` | `EventOccurredAt` pada siklus baru diambil dari `DateTimeOffset.UtcNow`, sama dengan perilaku `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` dari `BE-FIN-045` |
| 2 | Gerbang `FIN-OQ-047` masih terbuka | Worker `BE-FIN-071` melewati `PEMBUKAAN-SHIFT-KASIR` sampai ratifikasi turun |

---

## 8. Task Berikutnya

- **`BE-FIN-073`** — Hosted service penjadwal penanda shift (bergantung pada `BE-FIN-070`)
- **`BE-FIN-071`** — Worker pengiriman outbox Finance ke Accounting
- Menutup `FIN-OQ-047` bersama Accounting untuk mengaktifkan pengiriman penanda pembukaan

---

## 9. Definition of Done

- [x] Kode `PEMBUKAAN-SHIFT-KASIR` ditambahkan ke katalog `FinAccountingEventTypeCodes`
- [x] Kode terdaftar di `ZeroAmountAllowedEventTypes`
- [x] Query shift diperluas dari 3 menjadi 7 status
- [x] Blok penerbitan `PEMBUKAAN-SHIFT-KASIR` untuk shift belum final diimplementasikan
- [x] Idempotensi dijaga dengan composite key `shiftId:SourceVersion`
- [x] Urutan penerbitan `REOPENED`: pembalik dulu, pembukaan sesudahnya
- [x] Nol migration baru
- [x] Nol tulisan ke tabel `Bil*`
- [x] Kompilasi manual diserahkan ke pengguna
- [x] Laporan tracked ini ditulis
