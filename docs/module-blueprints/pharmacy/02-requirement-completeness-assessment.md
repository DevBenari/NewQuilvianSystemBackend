# Farmasi — Penilaian Kelengkapan Requirement

| Field | Value |
| --- | --- |
| Blueprint | `PHA-BP-001` revision `2` |
| Assessment ID | `PHA-RCG-001` |
| Tanggal | 20 Agustus 2026 |
| Backend snapshot | `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Frontend snapshot | `400104f2a0f3239c14c40f5905b419977a538450` |
| Kesiapan keseluruhan | `PARTIALLY_READY` |

## Scope dan bukti

Penilaian dibatasi pada tiga slice: menentukan satu Depo dari encounter, mereservasi stok setelah pembayaran/jaminan valid, dan menyerahkan obat lalu mengurangi stok fisik. Pengadaan serta operasional Gudang Utama tidak termasuk.

Bukti berasal dari `00-interview-decisions.md` revision `2`, `PHA-DEPOT-ROUTING-v1`, audit kemampuan existing, daftar prerequisite, serta source snapshot yang tercatat di atas. Baseline domain rumah sakit Indonesia tidak dipakai, sehingga tidak ada observasi `REFERENCE_ONLY` yang dianggap sebagai kebijakan rumah sakit.

## Kesiapan per slice

| Slice | Kesiapan | Alasan |
| --- | --- | --- |
| Routing Depo | `READY_FOR_DOMAIN_DESIGN` | Tujuan, data encounter, kandidat lokasi, prioritas, larangan, hasil, serta exception nol/lebih dari satu kandidat sudah dikonfirmasi |
| Reservasi setelah payment gate | `BUSINESS_DECISION_REQUIRED` | Belum diputuskan penanganan pembayaran berhasil tetapi reservasi gagal, callback ganda, koreksi pembayaran, dan owner authoritative Billing |
| Penyerahan dan pengurangan stok | `BUSINESS_DECISION_REQUIRED` | Kebijakan partial dispensing, checker kedua, dan obat dibayar tetapi tidak diambil belum lengkap |

## Penilaian 18 dimensi

| Dimensi | Routing Depo | Reservasi | Penyerahan |
| --- | --- | --- | --- |
| Tujuan | `CONFIRMED` | `CONFIRMED` | `CONFIRMED` |
| Aktor | Sistem/Farmasi `CONFIRMED` | Farmasi dikonfirmasi; owner callback Billing `MISSING` | Farmasi dikonfirmasi; checker kedua `MISSING` |
| Pemicu/prasyarat | Encounter valid `CONFIRMED` | Payment/jaminan valid dan mulai diproses `CONFIRMED` | Resep dan obat siap `CONFIRMED` |
| Alur utama | `CONFIRMED` | Urutan umum `CONFIRMED`; kegagalan lintas transaksi `MISSING` | Penyerahan lalu pengurangan stok `CONFIRMED` |
| Exception | Nol/lebih dari satu kandidat `CONFIRMED` | Reservasi gagal setelah dibayar `MISSING` | Tidak diambil dan partial dispense `MISSING` |
| Data minimum | Encounter dan atribut lokasi `CONFIRMED` | Depo, obat, jumlah, payment reference, actor, waktu `PROPOSED` | Penerima, item, batch, jumlah aktual, actor, waktu `PROPOSED` |
| Aturan/validation | Kandidat dan tepat satu hasil `CONFIRMED` | Stok negatif dilarang dan reservasi atomik `CONFIRMED` | Stok berkurang saat handover berhasil `CONFIRMED` |
| Status/lifecycle | Ditemukan atau ditolak `CONFIRMED` | Gagal, retry, dan release `MISSING` | Partial, tidak diambil, reversal `MISSING` |
| Authorization | Resolver sistem `CONFIRMED` | Petugas Farmasi `CONFIRMED`; permission rinci `MISSING` | Pelaku umum `CONFIRMED`; matriks rinci `MISSING` |
| Dependency antarmodul | Encounter dan Master Data `CONFIRMED` | Billing dan Inventory `CONFIRMED`; kontrak belum siap | Billing dan Inventory `CONFIRMED`; checker policy belum siap |
| Integrasi | Internal V2 `CONFIRMED` | Billing authoritative `CONFLICT` dengan generic `Prescription.Update` | Mutasi dispense/ledger `MISSING` |
| Hasil akhir | Satu Depo atau penolakan `CONFIRMED` | Reserved naik, on-hand tetap `CONFIRMED` | On-hand turun dan handover tercatat `CONFIRMED` |
| Pembatalan/koreksi | Validasi ulang sebelum reservasi `CONFIRMED` | Release/reversal tepat sekali `PROPOSED` | Retur diverifikasi `CONFIRMED`; tidak-diambil `MISSING` |
| Audit/histori | Kandidat dan alasan penolakan `PROPOSED` | Actor, waktu, nilai, payment reference `CONFIRMED` secara prinsip | Penerima, actor, batch, jumlah, waktu `CONFIRMED` secara prinsip |
| Notifikasi | Tidak material untuk resolver | Kegagalan setelah pembayaran `MISSING` | Siap diambil dan lewat 24 jam `MISSING` |
| Billing/charge | Tidak mengubah tagihan `CONFIRMED` | Failure compensation `MISSING` | Sinkronisasi rawat jalan masih `CONFLICT` |
| Keselamatan klinis | Salah Depo dicegah `CONFIRMED` | Stok tidak cukup ditolak `CONFIRMED` | Checker dan partial supply `MISSING` |
| Pelaporan/traceability | Audit hasil routing `PROPOSED` | Ledger/reservasi di implementasi `MISSING` | Batch-ke-pasien `CONFIRMED` sebagai requirement, implementasi `MISSING` |

## Gap dan Decision Log

| ID | Bukti | Dampak | Pertanyaan pemblokir | Owner |
| --- | --- | --- | --- | --- |
| `PHA-OQ-014` | `MISSING` | `BLOCKING` | Apa hasil resmi bila pembayaran valid tetapi reservasi atomik gagal karena stok habis? | Product, Billing, Pharmacy |
| `PHA-OQ-015` | `CONFLICT` | `BLOCKING` | Siapa sumber authoritative paid/approved/reversal/refund dan bagaimana callback ganda diproses tepat sekali? | Billing, Finance, Security |
| `PHA-OQ-009` | `MISSING` | `BLOCKING` | Setelah 24 jam obat tidak diambil, bagaimana status racikan, release stok, refund, dan notifikasi? | Product, Billing, Pharmacy |
| `PHA-OQ-016` | `MISSING` | `BLOCKING` | Apakah partial dispensing diizinkan, untuk apa, oleh siapa, dan bagaimana sisa serta tagihannya? | Pharmacy, Clinical, Billing |
| `PHA-OQ-017` | `MISSING` | `BLOCKING` | Obat apa yang wajib checker kedua dan peran apa yang boleh menjadi checker? | Pharmacy, Clinical, Security |
| `PHA-GAP-ROUTE-001` | `PROPOSED` | `NON_BLOCKING_STANDARD` | Simpan jejak kandidat dan alasan resolver menolak konfigurasi | Engineering saat desain |

**Contoh `PHA-OQ-014`:** pasien membayar 10 tablet. Sebelum Farmasi memproses, resep lain mengambil stok terakhir. Sistem harus memiliki satu status penyelesaian resmi dan tidak boleh membiarkan pembayaran berhasil tanpa tindak lanjut yang terlacak.

**Contoh `PHA-OQ-016`:** resep meminta 10 tablet tetapi hanya 6 dapat diserahkan. Belum diputuskan apakah 6 boleh diserahkan, apakah sisa 4 tetap aktif, dan kapan tagihan untuk sisa tersebut dikoreksi.

## Boleh berjalan

Routing Depo boleh masuk `hospital-domain-architect`, terbatas untuk menentukan satu lokasi. Slice ini tidak boleh membuat reservasi, mengubah stok, atau mengubah pembayaran. Dependency `PHA-DEP-004` tetap `REUSE WITH ADAPTER`.

## Harus berhenti

Domain design reservasi, callback Billing, dispense, partial dispense, release setelah 24 jam, dan checker kedua menunggu `PHA-OQ-009` serta `PHA-OQ-014` sampai `PHA-OQ-017`. Task `PHA-BE-001` belum siap dieksekusi.

## Handoff

- Routing Depo: `hospital-domain-architect`.
- Keputusan terbuka: `grill-me`, dibatasi pada lima Decision ID di atas.

