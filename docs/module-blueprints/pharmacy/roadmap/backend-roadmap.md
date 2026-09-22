# Farmasi — Backend Roadmap

| Field | Nilai |
| --- | --- |
| Blueprint | `PHA-BP-001` revisi `4` · status `approved` |
| Bentuk blueprint | `SINGLE` |
| Backend SHA | `6782ae652ca53299f7469c49b2edb64d23e77b60` |
| Frontend SHA | `1b138b9aac7a50524fd751a47c9a76e0a55f8803` |

Berkas ini lahir 21 September 2026 bersama slice Financial Clearance. Task Routing Depo
(`PHA-BE-001`–`003`) tetap hidup pada `README.md` roadmap dan **tidak dipindahkan** ke sini —
memindahkannya berarti menyentuh rencana yang approval-nya berdiri sendiri.

---

# Gelombang Financial Clearance

| Field | Nilai |
| --- | --- |
| Masukan | `PHA-DEC-063`–`071`, `PHA-DES-001`–`006` — seluruhnya `approved` 21 September 2026 |
| Gerbang requirement | `PHA-RCG-002` — `READY_FOR_DOMAIN_DESIGN` |
| Contract version berlaku | `PHA-API-CLEARANCE-v1`, `PHA-STATE-CLEARANCE-v1`, `PHA-VAL-CLEARANCE-v1`, `PHA-INT-CLEARANCE-v1`, `PHA-PERM-CLEARANCE-v1`, `PHA-TEST-CLEARANCE-v1` — seluruhnya `approved` |
| Arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` untuk slice ini, dengan alasan tercatat di manifest |

## Ketergantungan yang tidak dapat dicabut approval siapa pun

Seluruh task di bawah **menunggu sisi penerbit Billing berdiri**. Ini bukan urutan yang dipilih,
melainkan kenyataan: tanpa surat yang terbit, slice ini tidak punya masukan apa pun.

| Prasyarat | Milik | Yang menunggu |
| --- | --- | --- |
| `BE-BKC-067` — surat clearance terbit | `billing-kasir` | `PHA-BE-004` |
| `BE-BKC-068` — permukaan pemeriksaan ulang | `billing-kasir` | `PHA-BE-005` |

## Grafik Urutan Dependency

```text
[BIL] BE-BKC-067 ─> PHA-BE-004 ─┬─> PHA-BE-006 ─> [FE] PHA-FE-002
                                │
                                └─┬─> PHA-BE-005
[BIL] BE-BKC-068 ────────────────┘
```

Legenda: `[BIL]` adalah cermin baca-saja milik `billing-kasir/roadmap/backend-roadmap.md`;
`[FE]` cermin baca-saja milik `frontend-roadmap.md` modul ini. Keduanya dihitung dan dijadwalkan
di roadmap asalnya, bukan di sini. Tidak ada node `{DEC-...}` — seluruh keputusan sudah turun.

| Gelombang eksekusi | Task | Dapat berjalan paralel? |
| --- | --- | --- |
| 1 | `PHA-BE-004` | Tidak — menunggu `BE-BKC-067` |
| 2 | `PHA-BE-005`, `PHA-BE-006` | **Ya**; `PHA-BE-005` juga menunggu `BE-BKC-068` |

Jumlah pasangan prasyarat→task pada grafik: **lima**, sama persis dengan isi kolom `Dependency`
pada tabel task di bawah.

## Task

### `PHA-BE-004` — Resep terlepas dari keadaan menunggu pembayaran

| Field | Isi |
| --- | --- |
| Outcome | Resep yang tagihannya sudah beres secara finansial masuk ke antrean apoteker tanpa petugas melakukan apa pun — menutup kemacetan yang berjalan sejak 24 Agustus 2026 |
| Jejak | `PHA-DEC-063`, `PHA-DEC-064`, `PHA-DEC-065`, `PHA-DES-001`, `PHA-DES-002`, `PHA-DES-004` |
| Contract | `PHA-INT-CLEARANCE-v1`, `PHA-STATE-CLEARANCE-v1` |
| Kemampuan existing yang dipakai | `PhmPrescription`, `PrescriptionWorkflowService`, enum keadaan pemenuhan dan pembayaran yang sudah terpasang |
| Cakupan yang diharapkan | Satu tabel salinan beserta configuration dan migration; satu service konsumsi; pemindahan keadaan resep; penulisan salinan kenyamanan pada kolom pembayaran yang sudah ada |
| Dependency | `[BIL] BE-BKC-067` |
| Acceptance criteria | Surat boleh dikerjakan memindahkan resep dari menunggu pembayaran ke antrean; surat bernomor versi lebih rendah **tidak** mengubah salinan dan **tidak** menimbulkan galat; dua surat untuk resep yang sama diproses bersamaan menghasilkan tepat satu baris salinan dengan versi tertinggi yang menang; hasil penjaminan tercatat sebagai disetujui penjamin, bukan lunas tunai; kolom pembayaran pada resep hanya ditulis oleh service ini |
| Bukti verifikasi | QBE preflight dan conformance; review diff dan scope; `dotnet restore` dan `dotnet build` berhasil; verifikasi proses bisnis atas `PHA-AT-CLR-01`, `02`, `03`, `11`; pemeriksaan runtime bahwa resep benar-benar muncul di antrean |
| Risiko | Bila penolakan versi basi salah arah, resep yang sudah dicabut akan kembali terbaca boleh dikerjakan — dan obat keluar tanpa dasar |
| Pemilik | Pharmacy Backend |
| Definition of Done | Tabel berdiri beserta index uniknya; resep terlepas dari kemacetan; **nol** jalur yang memungkinkan Farmasi menyimpulkan keadaan finansial sendiri; **nol** baris tabel Billing tersentuh |

> **Wewenang terpisah.** Pembuatan dan eksekusi migration `AddPrescriptionFinancialProjection`
> **MUST** diminta tersendiri saat eksekusi. `PHA-DEC-071` menyetujui desainnya, bukan
> menjalankannya.

### `PHA-BE-005` — Gerbang penahanan pada empat titik

| Field | Isi |
| --- | --- |
| Outcome | Pekerjaan Farmasi berhenti di tempat ketika izin dicabut, tanpa membuang pekerjaan yang sudah terlanjur dikerjakan, dan dilanjutkan dari titik terakhir ketika izin dipulihkan |
| Jejak | `PHA-DEC-067`, `PHA-DEC-069`, `PHA-DES-003`, `PHA-DES-005`, `PHA-DES-006` |
| Contract | `PHA-STATE-CLEARANCE-v1`, `PHA-VAL-CLEARANCE-v1` |
| Kemampuan existing yang dipakai | `PrescriptionReviewService`, `PrescriptionPreparationService`, `PrescriptionFinalCheckService`, dan gerbang penyerahan yang **sudah ada** dan sudah menanti tiga keadaan yang sama |
| Cakupan yang diharapkan | Penanda tahan finansial yang **dihitung**, bukan disimpan; pemasangan gerbang pada empat titik; pemakaian permukaan pemeriksaan ulang saat salinan bermasalah |
| Dependency | `PHA-BE-004`, `[BIL] BE-BKC-068` |
| Acceptance criteria | Pencabutan saat resep sedang disiapkan **tidak** menurunkan keadaan pemenuhan dan **tidak** mengembalikan racikan menjadi bahan; pemulihan melanjutkan dari titik terakhir tanpa mengulang antrean, telaah, maupun penyiapan; keadaan belum diketahui dan tertinggal menolak seluruh gerbang; **tidak ada** permukaan override bagi peran mana pun termasuk Kepala Farmasi; pencabutan setelah obat diserahkan tidak mengubah apa pun |
| Bukti verifikasi | QBE preflight dan conformance; review diff dan scope; build berhasil; verifikasi proses bisnis atas `PHA-AT-CLR-04`–`09` dan `12`, termasuk jalur gagal `PHA-AT-CLR-04-F` dan `07-F`; pemeriksaan runtime bahwa **nol** baris stok bergerak saat penahanan |
| Risiko | Menarik keadaan pemenuhan mundur akan membuat catatan berbohong tentang keadaan fisik obat, dan apoteker berisiko meracik untuk kedua kalinya saat izin pulih |
| Pemilik | Pharmacy Backend |
| Definition of Done | Keempat gerbang menolak pada keadaan yang benar; racikan tidak pernah direstock; **nol** kolom penanda baru ditambahkan pada tabel resep |

### `PHA-BE-006` — Keadaan finansial terbaca pada layar kerja

| Field | Isi |
| --- | --- |
| Outcome | Petugas mengetahui, di tempat ia bekerja, mengapa sebuah resep tidak dapat dilanjutkan |
| Jejak | `PHA-DEC-069`, `PHA-API-CLEARANCE-v1` |
| Contract | `PHA-API-CLEARANCE-v1` |
| Kemampuan existing yang dipakai | Response layar kerja resep dan response satu resep yang sudah ada |
| Cakupan yang diharapkan | Penambahan field keadaan finansial, alasan penahanan, dan kesegaran salinan pada response yang sudah ada — **aditif**, tanpa menghapus maupun mengubah arti field mana pun |
| Dependency | `PHA-BE-004` |
| Acceptance criteria | Setiap baris resep membawa keadaan finansialnya; resep yang keadaannya belum diketahui tetap dikembalikan dengan penanda belum diketahui, **bukan** galat dan bukan `404`; hak akses **tidak berubah**; nol endpoint baru |
| Bukti verifikasi | QBE preflight; review diff dan scope; build berhasil; verifikasi kontrak API terhadap `PHA-API-CLEARANCE-v1`; verifikasi proses bisnis atas `PHA-AT-CLR-10` |
| Risiko | Menambah endpoint baru untuk keperluan ini akan memecah tempat petugas mencari keterangan |
| Pemilik | Pharmacy Backend |
| Definition of Done | Field tersedia pada kedua response; nol endpoint baru; nol butir hak akses baru |

## Catatan kebijakan verifikasi

Mengikuti kebijakan test backend yang berlaku di repository ini, gelombang ini **tidak**
memunculkan task penulisan automated test, dan acceptance criteria maupun Definition of Done di
atas **tidak** menuntutnya. Skenario `PHA-AT-CLR-01`–`12` pada `testing/acceptance-test-matrix.md`
dipakai sebagai **daftar skenario yang diverifikasi**. Bila pemilik menghendaki automated test,
itu permintaan eksplisit tersendiri.

QBE preflight dan kesesuaian engineering diselesaikan **pada waktu eksekusi**, dari `AGENTS.md`
backend target beserta dokumen engineering canonical.

## Traceability requirement ke bukti verifikasi

| Requirement | Task | Bukti verifikasi |
| --- | --- | --- |
| `FR-PHA-CLR-01`, `02` | `PHA-BE-004` | `PHA-AT-CLR-01`, `02`, `11` |
| `FR-PHA-CLR-03`, `04` | `PHA-BE-005` | `PHA-AT-CLR-07`, `07-F`, `08` |
| `FR-PHA-CLR-05` | `PHA-BE-004` | `PHA-AT-CLR-01` |
| `FR-PHA-CLR-06` | `PHA-BE-005` | `PHA-AT-CLR-04`, `05`, `09` |
| `FR-PHA-CLR-07` | `PHA-BE-005` | `PHA-AT-CLR-03`, `12` |
| `FR-PHA-CLR-08` | `PHA-BE-005` | `PHA-AT-CLR-06` |
| `FR-PHA-CLR-09` | `PHA-BE-006` | `PHA-AT-CLR-10` |
| `FR-PHA-CLR-10` | `[FE] PHA-FE-002` | Lihat `frontend-roadmap.md` |

**Nol requirement tanpa bukti verifikasi.** Seluruh sepuluh functional requirement pada
`04-prd-to-mvp.md` terpetakan.

## Wewenang yang tetap terpisah

| Wewenang | Catatan |
| --- | --- |
| Menulis source | Diminta per task saat handoff |
| Membuat dan menjalankan migration | `PHA-BE-004` saja; diminta terpisah sesudah backup |
| Eksekusi database langsung | **Tidak dibutuhkan** gelombang ini |
