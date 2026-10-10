# FLOW-BM-MVP-001 — Pemesanan, hunian dan pelepasan

**draft**, 10 Oktober 2026; mengikuti [manifest](../blueprint-manifest.md), last_changed_in 0.12.0. Approval amandemen belum ada. Owner produk/domain Muhammad Hamzah; penugasan/SOP nyata tetap BM-G02/03.

Pelaku dan pemicu: Admisi/perawat existing; pemicu bed yang dibaca ready. Trace: DEC-281/283/289; AC-407/408/413/419/421/426.

```mermaid
flowchart TD
 A["Baca bed/episode terotorisasi + versi"] --> B["Reserve/place dengan key stabil"]
 B --> C["Lock episode + bed, expiry server, cek receipt dan dua sumber holder"]
 C --> D{"Predicate, eligibility dan versi sah?"}
 D -- Tidak --> E["Rollback; 409/422; reload"]
 D -- Ya --> F{"Reserve atau place?"}
 F -- Reserve --> G["Reservation Active; root tetap Ready"]
 G --> H{"Dipakai sebelum batas server?"}
 H -- Tidak --> I["Cancel beralasan/expiry; tidak membuat cleaning"]
 H -- Ya --> J["Consume reservation + placement; invalidasi readiness"]
 F -- Place --> J
 J --> K["Kepergian fisik sah pada placement aktual"]
 K --> L["Atomic end placement + cycle baru WaitingCleaning + audit/receipt"]
 L --> M["Tidak bisa dipesan sampai verifikasi sah"]
```

Reservation belum dipakai tidak membuat kebutuhan pembersihan. Closure episode lama tidak memanggil release bed ulang. Error/retry tidak menambah holder atau melepas pasien berikutnya.

Kontrak authoritative: [state](../contracts/state-transition-matrix.md), [API](../contracts/api-contract.md), [permission](../contracts/permission-audit-matrix.md), [acceptance](../testing/acceptance-test-matrix.md).
