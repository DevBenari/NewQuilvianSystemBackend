# Roadmap Delivery — Billing dan Kasir

Blueprint `BIL-CASH-001 revision 0.4` telah disetujui pada 20 Agustus 2026; revision `0.5` (amendment `BKC-DEC-059`–`062`, form "Buat Invoice Manual (Testing)" berbasis katalog tarif + coverage per item) disetujui 2 September 2026. **Revision `0.8` (mencakup amendment 3 dan 4 September 2026, sampai `BKC-DES-025`) disetujui 4 September 2026** — Product/Domain Owner mengunci keenam dokumen kontrak turunannya. Hanya revision `0.9` (`BKC-DES-026`/`027`, perluasan perutean jalur `NotCovered`) yang masih `draft`.

Roadmap ini berada pada **revision `2`** (4 September 2026) dan berstatus `DRAFT_FORWARD_TEST`: urutan dan task sudah dapat ditinjau, tetapi **belum memberi wewenang menulis source**. Setiap builder hanya boleh menjalankan satu task yang kemudian disetujui secara eksplisit. Revision `1` memuat `BKC-PH-001` sampai `BKC-PH-008`; revision `2` menambahkan `BKC-PH-009` sampai `BKC-PH-015`.

## Fase

| Phase | Outcome | Backend | Frontend | Dependency | Status |
| --- | --- | --- | --- | --- | --- |
| `BKC-PH-001` | Fondasi dapat diuji | `BE-BKC-001` | — | Blueprint approved | `READY_FOR_TASK_APPROVAL` |
| `BKC-PH-002` | Policy finansial dapat dikelola | `BE-BKC-002`–`004` | `FE-BKC-002` | PH-001 | `PLANNED` |
| `BKC-PH-003` | Charge menjadi running invoice yang benar | `BE-BKC-005`–`008` | `FE-BKC-001`,`003`,`004` | PH-001/002 | `PLANNED` |
| `BKC-PH-004` | Deposit dan split payment berjalan | `BE-BKC-009`–`011` | `FE-BKC-005`,`006` | PH-003 | `PLANNED` |
| `BKC-PH-005` | Shift dan exception finansial terkontrol | `BE-BKC-012`–`014` | `FE-BKC-007`,`008` | PH-004 | `PLANNED` |
| `BKC-PH-006` | Finalisasi menghasilkan AR/AP idempotent | `BE-BKC-015`,`016` | `FE-BKC-009` | PH-003–005 | `PLANNED` |
| `BKC-PH-007` | Bukti lintas-slice dan hardening lengkap | `BE-BKC-017` | `FE-BKC-010` | Semua slice | `PLANNED` |
| `BKC-PH-008` | Entri manual katalog tarif + coverage per item (form "Buat Invoice Manual (Testing)") — `BKC-DEC-059`–`062` | `BE-BKC-018`–`021` | `FE-BKC-014`–`016` | Blueprint `0.5 approved` (2 Sep 2026) | `READY_FOR_TASK_APPROVAL` |
| `BKC-PH-009` | Rupiah tanggungan penjamin per baris biaya (`MVP-4`, `EPIC BKC-04`) | `BE-BKC-022` | — | Tidak ada | **`READY_FOR_TASK_APPROVAL`** |
| `BKC-PH-010` | Lembar "Invoice Asuransi" — endpoint dan tab (`MVP-5`/`MVP-6`, `EPIC BKC-05`) | `BE-BKC-023` | `FE-BKC-018` | `BKC-PH-009`; `BKC-GATE-03` | `BLOCKED` |
| `BKC-PH-011` | Tanggungan tanpa nominal menggantung dan anomali data penjamin (`MVP-7`, `EPIC BKC-06`/`BKC-07`) | `BE-BKC-024`, `BE-BKC-025` | — | Tidak ada | **`READY_FOR_TASK_APPROVAL`** |
| `BKC-PH-012` | Verifikasi gerbang PPN rawat inap versus rawat jalan (`MVP-8`, `EPIC BKC-08`) | `BE-BKC-026` | — | Tidak ada | **`READY`** |
| `BKC-PH-013` | Menu Pembayaran menjumlah dan menampilkan anomali (`MVP-9`) | — | `FE-BKC-019`, `FE-BKC-020` | `BKC-PH-011`, `BKC-PH-014` terverifikasi hidup | `BLOCKED` |
| `BKC-PH-014` | Penanggungan selisih yang tidak dapat ditagihkan (`MVP-11`/`MVP-12`, `EPIC BKC-09`) | `BE-BKC-027`–`029` | `FE-BKC-021` | `BKC-PH-011` **selesai lebih dulu** (sequencing, bukan gerbang) | `BLOCKED` |
| `BKC-PH-015` | Perluasan perutean jalur `NotCovered`, data induk PPN, dan regresi penutup | `BE-BKC-030`–`032` | — | `BKC-PH-014`; `BKC-GATE-06` | `BLOCKED`, kecuali `BE-BKC-031` yang **`READY`** |

## Roadmap revision `2` — gelombang `MVP-4` sampai `MVP-12`

Revision `2` (4 September 2026) menambahkan `BKC-PH-009` sampai `BKC-PH-015` di atas, yaitu
sebelas task backend (`BE-BKC-022`–`032`) dan empat task frontend (`FE-BKC-018`–`021`). Isinya
menurunkan amendment blueprint 3 dan 4 September 2026 — dokumen "Invoice Asuransi", pembagian
tanggungan penjamin, anomali data pendaftaran, gerbang PPN, dan penanggungan selisih yang tidak
dapat ditagihkan.

**Diperbarui 4 September 2026 — kontrak dikunci.** Semula sepuluh dari sebelas task backend dan
keempat task frontend berstatus `BLOCKED`. Urutan yang terjadi: `/qv-trace` dijalankan terhadap
`HEAD` `fd4a605` dan menemukan sebagian besar pekerjaan sudah selesai lewat task ad-hoc di luar
roadmap; pemilik menjawab tiga pertanyaan penutup soal PPN rawat inap dan limit bulanan; lalu
**Product/Domain Owner (wewenang ganda Finance/AR) mengunci keenam dokumen kontrak**, menutup
`BKC-GATE-01`. Hasilnya: **empat task backend kini `READY_FOR_TASK_APPROVAL` tanpa satu gerbang
pun** (`BE-BKC-022`, `024`, `026`, `031`); lima task lain hanya menunggu task pendahulunya selesai
(sequencing, bukan gerbang governance).

Yang masih tertahan gerbang sungguhan tinggal dua: `BKC-GATE-03` (Security, hanya
`BE-BKC-023`/`FE-BKC-018`) dan `BKC-GATE-06` (revisi `0.9`, hanya `BE-BKC-030`). `BKC-GATE-05`
(MCU/telemedicine/OTC) turun menjadi syarat aktivasi, bukan syarat roadmap.

**Kontrak dikunci bukan wewenang tulis.** Setiap task tetap menunggu approval task tersendiri dan
konfirmasi `TASK MODE: BACKEND`/`FRONTEND` beserta cabang kerja (`BKC-GATE-09`) sebelum satu baris
source pun ditulis — sesuai § Aturan eksekusi di bawah.

**Riwayat koreksi.** Tiga putaran koreksi berturut-turut pada roadmap ini — penutupan
`BKC-GATE-07` (working tree ternyata sudah ter-commit), penutupan `BKC-GATE-02` (`/qv-trace`
dijalankan), jawaban pemilik yang menutup `BKC-GATE-04`/menurunkan `BKC-GATE-05`, dan akhirnya
penutupan `BKC-GATE-01` (kontrak dikunci) — ada di [backend-roadmap.md](./backend-roadmap.md)
§ 4 dan § 5, serta [01-existing-capability-map.md](../01-existing-capability-map.md) § 17.

Rincian gerbang, urutan gelombang, dan alasan tiap dependency ada di
[backend-roadmap.md](./backend-roadmap.md) § Amendment 4 September 2026 dan
[requirement-traceability.md](./requirement-traceability.md) § Amendment 4 September 2026.

## Aturan eksekusi

1. Task backend dan frontend tetap terpisah.
2. Migration **boleh digenerasikan** hanya bila disebut dalam scope task backend yang disetujui. Migration tidak boleh dijalankan ke database tanpa otorisasi terpisah.
3. Setiap backend task menjalankan QBE preflight dari `AGENTS.md`, engineering contract, registry prefix, dan aturan `.codex` yang berlaku saat eksekusi.
4. Frontend task menunggu governance frontend tersedia; pilihan visual yang tidak mengubah kontrak tetap `DEV_DISCRETION`.
5. Task berstatus `DONE` hanya setelah bukti acceptance yang ditetapkan benar-benar tersedia.

Dokumen: [backend](./backend-roadmap.md), [frontend](./frontend-roadmap.md), dan [traceability](./requirement-traceability.md).

