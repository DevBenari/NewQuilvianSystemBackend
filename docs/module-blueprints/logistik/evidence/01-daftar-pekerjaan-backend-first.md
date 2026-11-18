# Logistik — Desain Pekerjaan Backend-First (DRAF)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `LGS-BP-001` r2 |
| Status | **DRAF desain** — bukan roadmap resmi, bukan kontrak `approved`, dan tidak memberi wewenang implementasi. Roadmap resmi disusun lewat `plan-module-delivery` setelah keputusan pemblokir disetujui |
| Tanggal | 2026-11-17 (revisi kedua: kontrak per area dan area E) |
| SHA backend | `d946b20b` ditambah perubahan `LGS-BE-ADH-01` yang belum di-commit |
| Dasar | `00-interview-decisions.md` bagian 8–9, `01-existing-capability-map.md` r2, `evidence/LGS-BLM-INVESTIGATION.md` |

Semua nama endpoint, field, dan status di bawah adalah **usulan** untuk dibahas. Nilai yang
menunggu keputusan ditulis sebagai `⟨keputusan …⟩`.

## 0. Prinsip yang berlaku untuk semua area

1. **Satu buku saldo.** Seluruh perubahan stok obat lewat `DrugStockService` (`ApplyMutationAsync`, `ReceiveBatchAsync`, pengeluaran FEFO). Tidak ada tabel saldo kedua.
2. **Satu mutasi per sebab.** Setiap baris dokumen sumber menghasilkan paling banyak satu mutasi. Kuncinya `SourceDocumentType` + `SourceDocumentId` + nomor baris, ditambah `IdempotencyKey` pada perintah dan `ExpectedVersion` pada saldo.
3. **Transaksi database** membungkus perubahan dokumen dan mutasi saldo sekaligus.
4. **Audit.** Mutasi menyimpan `ActorUserId` dan `OccurredAt`; dokumen menyimpan riwayat status beserta alasan.
5. **Migration** dan eksekusi database adalah wewenang terpisah dan belum diberikan.
6. **Registry.** Entity baru di luar `Phm` menunggu prefix `Lgs` disetujui (`LGS-OQ-016`).

## A. Persetujuan permintaan barang unit

**Pemblokir:** `LGS-OQ-004` (siapa atasan, pengecualian cito), `LGS-OQ-015`.
**Dependency:** tidak bergantung area lain. **Risiko saldo:** tidak ada.

**Status (memakai enum yang sudah ada):**

```text
Draft → Submitted → Approved → Completed
                  ↘ Rejected
Draft/Submitted/Approved → Cancelled
```

**Kontrak usulan**

```http
POST /api/v1/health-services/pharmacy-management/stock-requests/{id}/approve
Izin: StockRequest:Approve   ⟨keputusan LGS-OQ-004: peran atasan⟩
{
  "expectedVersion": 3,
  "approvedItems": [{ "itemId": "…", "approvedQuantity": 40 }],   // ⟨keputusan: boleh mengubah jumlah?⟩
  "note": "Sisa di ruangan masih ada"
}
→ 200 StockRequestDetailResponse (status Approved, decidedAt, decisionReason)
→ 409 versi berubah / status bukan Submitted
→ 403 bukan atasan unit pemohon

POST …/stock-requests/{id}/reject
Izin: StockRequest:Approve
{ "expectedVersion": 3, "reason": "Permintaan ganda" }   // alasan wajib
→ 200 (status Rejected)
```

**Aturan turunan:** `fulfill` hanya menerima status `Approved` ⟨kecuali jalur cito, keputusan
`LGS-OQ-004`⟩. Setiap keputusan menulis `PhmStockRequestHistory` (`FromStatus`, `ToStatus`,
`Action`, `Reason`).
**Butuh migration:** mungkin kolom penyetuju dan jumlah disetujui per item; diperiksa saat desain
resmi.

## B. Stock opname dan penyesuaian

**Pemblokir:** `LGS-OQ-009`, `LGS-OQ-012`, `LGS-OQ-008` (nilai selisih), `LGS-OQ-016`.
**Referensi approved:** `PHA-DEC-030` (per lokasi/batch, cut-off, maker-checker, persetujuan
sebelum penyesuaian, investigasi selisih obat khusus).
**Dependency:** tidak bergantung area lain; jurnal menunggu area E.

**Status sesi usulan**

```text
Scheduled → Frozen (cut-off) → Counting → Verifying → Approved → Posted → Closed
                                   ↑          │
                                   └─ Recount ┘ (selisih melewati toleransi ⟨keputusan LGS-OQ-009⟩)
Scheduled/Frozen/Counting → Cancelled
```

**Kontrak usulan (ringkas)**

| Endpoint | Fungsi | Aturan |
| --- | --- | --- |
| `POST /stock-opnames` | Membuat sesi untuk satu lokasi dan jadwal | Satu sesi aktif per lokasi |
| `POST /stock-opnames/{id}/freeze` | Cut-off: memotret saldo sistem per batch | Selama Frozen, mutasi lokasi itu ditolak ⟨atau dicatat terpisah, keputusan⟩ |
| `PUT /stock-opnames/{id}/counts` | Mencatat hitung fisik per batch | Hitung buta: saldo sistem tidak dikirim ke penghitung ⟨keputusan⟩ |
| `POST /stock-opnames/{id}/verify` | Verifikasi | Pemverifikasi ≠ penghitung (maker-checker) |
| `POST /stock-opnames/{id}/approve` | Persetujuan selisih | Jenjang sesuai nilai/jenis obat ⟨keputusan LGS-OQ-009⟩; obat khusus wajib investigasi |
| `POST /stock-opnames/{id}/post` | Posting selisih lewat `ApplyMutationAsync(Adjustment)` sumber `StockOpname` | `IdempotencyKey = opname:{id}:{lineId}`; sekali per baris |
| `GET /stock-opnames/{id}/minutes` | Data berita acara | Hanya setelah Posted |

**Butuh migration:** ya — tabel sesi dan baris opname.

## C. Distribusi dan serah terima

**Pemblokir:** `LGS-OQ-003` (saldo di unit), `LGS-OQ-005` (sisa pemenuhan), `LGS-OQ-006`
(konfirmasi unit). **Dependency:** sebaiknya setelah A.

**Lifecycle usulan bila unit punya saldo (`LGS-OQ-003` = ya)**

```text
Permintaan Approved
  → Penyiapan (picking FEFO, belum memotong saldo)
  → Pengiriman = transfer gudang → lokasi unit, status InTransit (saldo gudang berkurang SEKALI di sini)
  → Penerimaan aktual oleh unit = transfer Completed (saldo unit bertambah sesuai jumlah diterima)
  → Outstanding = diminta − diterima ⟨keputusan LGS-OQ-005: ditutup atau menunggu⟩
```

**Kontrak usulan:** `POST /stock-requests/{id}/dispatch` (baru, usulan) membuat `PhmStockTransfer`
bertaut ke permintaan. Pengiriman dan penerimaan memakai endpoint transfer yang **sudah ada**:
`POST /stock-transfers/{id}/issue` (`StockTransfer:Issue`) dan `POST /stock-transfers/{id}/receive`
(`StockTransfer:Receive`), dengan alasan wajib bila jumlah diterima berbeda.

**Larangan saldo ganda:** begitu C berlaku, `fulfill` lama **tidak boleh** lagi memotong saldo
gudang. Pemotongan hanya terjadi saat transfer dikirim.
**Butuh migration:** mungkin tautan permintaan ↔ transfer.

## D. Penerimaan barang dari pengadaan

**Pemblokir:** `LGS-OQ-017`, `LGS-OQ-018` (verifikasi BLM), `LGS-OQ-015`, `LGS-OQ-016`.
**Dependency:** pengolah kiriman BLM milik Finance (P0 `FIN-DEC-271`) bila dokumen datang dari BLM.

**Kontrak data usulan**

| Bagian | Isi |
| --- | --- |
| Pemetaan produk | `ExternalSystem`, `ExternalProductId`, `ExternalProductCode` → `DrugId` ⟨atau katalog non-medis⟩, `BaseUomCode` → satuan stok obat; unik per (`ExternalSystem`, `ExternalProductId`) |
| Penerimaan gudang | Merujuk satu baris `FinGoodsReceiptItem`; lokasi tujuan, nomor batch, kedaluwarsa; jumlah = `AcceptedBaseQuantity` |
| Status barang | `ConditionCode` sah → `Available`; rusak → `Damaged`; menunggu periksa → `Quarantine` |
| Posting | `ReceiveBatchAsync` sumber `GoodsReceipt` (konstanta sudah ada); `IdempotencyKey = gr:{goodsReceiptItemId}` |
| Pembatalan GR | Mutasi koreksi lewat `mutations/correct`, tidak menghapus mutasi |

**Larangan saldo ganda:** satu baris GR hanya boleh diposting sekali; sinkron ulang dari BLM
tidak boleh memposting ulang; tidak boleh berjalan bersamaan dengan saldo gudang BLM bila
`LGS-OQ-018` menyatakan BLM memegang saldo.
**Butuh migration:** ya — tabel pemetaan produk dan penerimaan gudang.

## E. Valuasi dan jurnal persediaan

**Pemblokir:** `LGS-OQ-008` (metode valuasi), `LGS-OQ-012` (titik jurnal), pemilik Akuntansi.
**Dependency:** D (harga dari dokumen pengadaan) dan B (nilai selisih opname).

**Kebutuhan integrasi Finance/Akuntansi yang harus dijawab, tanpa menetapkan metode:**

| No | Kebutuhan | Pertanyaan untuk Akuntansi |
| --- | --- | --- |
| E1 | Sumber harga per batch atau per mutasi | Harga diambil dari PO, GR, atau faktur pembelian? |
| E2 | Metode valuasi | Rata-rata tertimbang atau FIFO? |
| E3 | Peristiwa yang dijurnal | Penerimaan, pengeluaran ke unit, penyesuaian opname, pemusnahan — mana saja? |
| E4 | Pusat biaya | Pengeluaran ke unit dibebankan ke pusat biaya unit? |
| E5 | Rekonsiliasi | Akun persediaan mana yang dicocokkan, dan kapan (bulanan)? |
| E6 | Mekanisme | Lewat outbox/inbox Akuntansi yang sudah ada (`AddAccountingEventInbox`) atau mekanisme lain? |

**Butuh migration:** kemungkinan besar (kolom harga atau tabel nilai persediaan). Tidak ada
jurnal otomatis yang boleh dibuat sebelum E2 dan E3 diputuskan.

## Urutan yang disarankan setelah keputusan tersedia

| Urutan | Area | Alasan |
| --- | --- | --- |
| 1 | A — Persetujuan | Keputusan paling sedikit, tanpa risiko saldo, memakai status yang sudah ada |
| 2 | B — Stock opname | Pola sudah `approved` (`PHA-DEC-030`); memakai `adjust` yang sudah ada |
| 3 | C — Distribusi | Menyelesaikan alur permintaan → serah terima; bergantung `LGS-OQ-003` |
| 4 | D — Penerimaan | Bergantung keputusan lintas modul dan verifikasi BLM |
| 5 | E — Valuasi | Bergantung D dan keputusan Akuntansi |
