# FLOW-BM-MVP-004 — Penutupan administratif dan pembukaan

**draft**, 10 Oktober 2026; mengikuti [manifest](../blueprint-manifest.md), last_changed_in 0.12.0. Approval amandemen belum ada. Owner produk/domain Muhammad Hamzah; penugasan/SOP nyata tetap BM-G02/03.

Pelaku dan pemicu: Admin MasterData berwenang; seluruh write path. Trace: DEC-285; AC-412/419/424.

```mermaid
flowchart TD
 A["Close/nonactive/nonreservable/delete atau hierarchy availability change"] --> B["Reason + expected version; shared lock coordinator"]
 B --> C{"Ada holder aktif setelah expiry server?"}
 C -- Ya --> D["409; tidak menutup, holder tetap terlihat"]
 C -- Tidak --> E["Invalidasi verification/cycle; interrupt attempt aktif; dirty tidak hilang"]
 E --> F["Master closed; Unavailable + audit"]
 F --> G["Reopen beralasan"]
 G --> H{"Masih butuh cleaning?"}
 H -- Ya --> I["WaitingCleaning; flow cleaning"]
 H -- Tidak --> J["Unverified; verifikasi sesuai SOP"]
 I --> K["Ready hanya dari verifier current"]
 J --> K
```

PUT/status/availability/create/delete tidak dapat menjadi bypass. Pada konflik data legacy, pasien/reservation aktif tetap terlihat dan pemesanan dilarang, bukan dianggap kosong.

Kontrak authoritative: [state](../contracts/state-transition-matrix.md), [API](../contracts/api-contract.md), [permission](../contracts/permission-audit-matrix.md), [acceptance](../testing/acceptance-test-matrix.md).
