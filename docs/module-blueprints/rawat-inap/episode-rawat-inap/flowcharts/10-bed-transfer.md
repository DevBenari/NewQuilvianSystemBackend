# FLOW-BM-MVP-002 — Transfer bed satu langkah

**draft**, 10 Oktober 2026; mengikuti [manifest](../blueprint-manifest.md), last_changed_in 0.12.0. Approval amandemen belum ada. Owner produk/domain Muhammad Hamzah; penugasan/SOP nyata tetap BM-G02/03.

Pelaku dan pemicu: Petugas Transfer terotorisasi; pasien dengan current placement. Trace: DEC-275–277/284/291; AC-399–405/414/423.

```mermaid
flowchart TD
 A["Pilih episode authorized"] --> B["Server tampilkan asal current readonly"]
 B --> C["Pilih tujuan tersedia + kategori manual + alasan"]
 C --> D["Await refresh context dan target; simpan versions"]
 D --> E["Lock episode + kedua bed terurut; receipt, holder, versi, guard DPJP/folio"]
 E --> F{"Kelas resmi dan kategori cocok, tujuan ready?"}
 F -- Tidak --> G["409/422; source/destination/history tidak berubah"]
 F -- Ya --> H["Satu transaksi: end source + destination placement snapshot + audit/receipt/outbox"]
 H --> I["Asal WaitingCleaning; tujuan Occupied"]
 I --> J["Commit lalu handover/callback existing"]
 J --> K{"Callback gagal?"}
 K -- Ya --> L["Retry delivery existing; tidak reverse transfer"]
 K -- Tidak --> M["Refresh monitoring/history/detail"]
```

SameGrade valid disediakan manual; arah/order kelas tidak ditebak dari angka0/harga/nama. Handover bukan penerimaan dua fase dan tidak menjadi gate commit.

Kontrak authoritative: [state](../contracts/state-transition-matrix.md), [API](../contracts/api-contract.md), [permission](../contracts/permission-audit-matrix.md), [acceptance](../testing/acceptance-test-matrix.md).
