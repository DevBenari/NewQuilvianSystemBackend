# FLOW-BM-MVP-006 — Hasil tidak pasti dan pemulihan koneksi

**draft**, 10 Oktober 2026; mengikuti [manifest](../blueprint-manifest.md), last_changed_in 0.12.0. Approval amandemen belum ada. Owner produk/domain Muhammad Hamzah; penugasan/SOP nyata tetap BM-G02/03.

Pelaku dan pemicu: Petugas actor permintaan asli; pemicu timeout/network. Trace: DEC-289/290; AC-421/422.

```mermaid
flowchart TD
 A["Mutation terkirim dengan key/payload stabil"] --> B{"Respons diketahui?"}
 B -- Sukses --> C["Refresh data server"]
 B -- Rejected rollback --> D["Tampilkan error; perbaiki input untuk intent baru"]
 B -- Timeout --> E["Jangan tampilkan sukses; cek own operation receipt"]
 E --> F{"Committed receipt ditemukan?"}
 F -- Ya --> C
 F -- Belum/unknown --> G["Tidak menganggap batal; cek lagi atau retry same key/payload"]
 G --> H{"Koneksi pulih?"}
 H -- Tidak --> I["Tahan aksi; ikuti SOP downtime yang sah"]
 H -- Ya --> E
 I --> J["Rekonsiliasi authorized+audit sesuai SOP; tanpa cache overwrite"]
 J --> E
```

NotFound saat request masih in-flight bukan bukti pasti gagal. Tidak membuat offline queue, key baru otomatis, sukses fiktif atau reverse transfer karena callback. SOP rumah sakit tetap BM-G02.

Kontrak authoritative: [state](../contracts/state-transition-matrix.md), [API](../contracts/api-contract.md), [permission](../contracts/permission-audit-matrix.md), [acceptance](../testing/acceptance-test-matrix.md).
