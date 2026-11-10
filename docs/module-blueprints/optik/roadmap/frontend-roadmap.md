# Frontend Roadmap — Optik

| Field | Nilai |
|---|---|
| Roadmap ID | `OPT-RM-FE-001` |
| Revision | `3` — task `FE-OPT-040`..`042` dari amendment r9 (`OPT-DEC-077`..`080`); sebelumnya `2` — dependency `BE-OPT-054` (`OPT-API-v1.1`) untuk `FE-OPT-020`/`021`; screen brief `approved` 2026-11-04 |
| Status | `approved` (Yoga Aji Pratama, 2026-11-03); **r3 `DRAFT`**: rencana task belum disetujui, dan setiap task butuh approval sendiri. Penahan persetujuan r2 dicabut 2026-11-06; `OPT-APR-014`, `OPT-OQ-020` ✅ (`OPT-EVD-003`); `FE-OPT-040`..`042` ✅ selesai 2026-11-06 |
| Blueprint | `OPT-BP-001` r1, `approved` sisi Optik (Yoga Aji Pratama, 2026-11-03); r2 `approved` sisi Optik (Yoga Aji Pratama, 2026-11-06); r3 `approved` sisi Optik (Yoga Aji Pratama, 2026-11-06) |
| Cakupan | `MVP-0` s.d. `MVP-4` — kebutuhan layar `FE-OPT-N01`..`N15` di `03-frontend-architecture.md` |
| SHA baseline | FE `b6dd6c304` (branch `YogaV2`), BE `2a5094ca` (branch `yoga`). r3: FE `811f22159`, BE `06e40fb7` |
| Kontrak masukan | `OPT-API-v1`, `OPT-PERM-v1`, `OPT-VAL-v1`, `OPT-STATE-v1` — `approved` sisi Optik, ber-hash di [`blueprint-manifest.md`](../blueprint-manifest.md). **Kerja paralel dengan backend diizinkan hanya untuk lapisan data dan aturan bersama**, karena kontraknya approved dan terkunci hash. Delta `OPT-API-v1.1` (`BE-OPT-054` ✅ 2026-11-04) belum terkunci hash; bentuk respons yang dipakai layar mengikuti [laporan `BE-OPT-054`](../task/report/backend/BE-OPT-054.md) bagian 4. **r3 memakai `OPT-API-v1.3`, `OPT-VAL-v1.2`, `OPT-PERM-v1.2`, dan brief r3, `approved` 2026-11-06 dan terkunci hash di manifest r3, sehingga kerja paralel diizinkan. Amendment r10 tidak menambah task frontend: peringatan tarif di layar sudah berbunyi "sistem akan mencoba lagi"** |
| Masukan desain | `03-frontend-architecture.md` r2, `04-prd-to-mvp.md` bagian 18 (UAT), `05-screen-brief.md` r3 |
| Owner | Yoga Aji Pratama |

> **Batas dokumen.**
> 1. Bukan izin menulis kode. Setiap task dikerjakan lewat `build-module-frontend` dengan approval task eksplisit.
> 2. **Menu, route, tab/modal, dan tata letak ditetapkan di [`05-screen-brief.md`](../05-screen-brief.md)** (`approved`, Yoga Aji Pratama, 2026-11-04). Task layar tidak lagi tertahan `FE-OPT-000`; dependency lain tetap berlaku. Warna, ikon, dan jarak adalah `DEV_DISCRETION` (`OPT-FE-004`).
> 3. Isian ukur desimal **tidak** memakai `type: "number"` mentah; pakai pola `DECIMAL_FIELD_PROPS`/`parseDecimal` (`OPT-FE-003`).
> 4. Uji unit: `node --import ./tests/helpers/register.mjs --test tests/unit/`. E2E memakai pola spec API-tiruan standalone dengan output ke scratchpad; predikat cegatan wajib memuat `/v1/`.
> 5. Folder mengikuti pola modul yang ada: `src/lib/services/health-services/optical-management/`, `src/lib/state/slice/health-services/optical-management/`, `src/lib/constants/health-services/optical-management/`, `src/utils/health-services/optical-management/`, `src/components/view/health-services/optical-management/`. Route final mengikuti screen brief.

## Grafik Dependency

```text
FE-OPT-001 (aturan bersama, mandiri)
✅FE-OPT-002 (data klinis) ─┐                ✅FE-OPT-000 {screen brief}
✅FE-OPT-003 (data pesanan) ┘                       │
                                                  ▼
FE-OPT-000 + 001 + 002 + BE-OPT-005 ─> ✅FE-OPT-031 (katalog & pengaturan)
FE-OPT-000 + 002 + BE-OPT-010 ─> ✅FE-OPT-010 (menu + kunjungan) ─┬─> ✅FE-OPT-011 (pemeriksaan, + BE-OPT-011)
                                                                └─> ✅FE-OPT-012 (resep, + BE-OPT-012)
FE-OPT-012 + 003 + BE-OPT-032/034 (+054 pratinjau harga) ─> ✅FE-OPT-020 (pesanan) ─> ✅FE-OPT-021 (serah/batal) ─┬─> ✅FE-OPT-022 (penjualan langsung)
                                                                                    └─> ✅FE-OPT-030 (garansi, + BE-OPT-050)
✅FE-OPT-021 + BE-OPT-052 ─> ✅FE-OPT-032 (laporan) ─> ✅FE-OPT-033 (E2E)

✅{persetujuan r2 + kunci hash} ─┬─> ✅FE-OPT-040 (kirim kunci) ── terbit lebih dulu atau bersama ──> ✅BE-OPT-060
                                ├─> ✅FE-OPT-041 (penyaring diskon) <── ✅{OPT-APR-014, OPT-OQ-020, Yasmina}
                                └─> ✅FE-OPT-042 (E2E katalog dan penjamin) <── ✅BE-OPT-061
```

## Ringkasan Task

| Task ID | Outcome | Layar | Dependency | Status |
|---|---|---|---|---|
| `FE-OPT-000` ✅ | Screen brief Optik disetujui: menu, daftar layar, route, pembagian daftar/detail, tab/modal | Semua | Pemilik (`OPT-FE-001`, `002`) | Selesai 2026-11-04 — [`05-screen-brief.md`](../05-screen-brief.md) `approved` (Yoga Aji Pratama); struktur `OPT-DEC-061`..`075`. Pratinjau harga (`FE-OPT-020`) dan dua penyaring Kepala (`FE-OPT-021`) memakai `BE-OPT-054` ✅ (`OPT-API-v1.1`) |
| `FE-OPT-001` ✅ | Aturan bersama teruji unit: input desimal bertanda minus, pratinjau transposisi, cermin aturan rentang/kelipatan | — | — | Selesai 2026-11-03 — [laporan](../task/report/frontend/FE-OPT-001.md); belum di-commit. Format rupiah/tanggal WIB dipindah ke `FE-OPT-003` (dipakai bersama status bayar) |
| `FE-OPT-002` ✅ | Lapisan data klinis: service Axios + slice Redux untuk kunjungan, pemeriksaan, resep, metadata | — | Kontrak v1 | Selesai 2026-11-04 — [laporan](../task/report/frontend/FE-OPT-002.md); 13 uji unit, lint, build `PASS` |
| `FE-OPT-003` ✅ | Lapisan data pesanan: pesanan, status bayar, keluhan, katalog, pengaturan, laporan, diskon Billing; format rupiah dan tanggal WIB (dipindah dari `FE-OPT-001`) | — | Kontrak v1 + `v1.1` | Selesai 2026-11-04 — [laporan](../task/report/frontend/FE-OPT-003.md); 14 uji unit, lint, build `PASS` |
| `FE-OPT-010` ✅ | Menu Optik + daftar kunjungan + pendaftaran + ringkasan kunjungan | `N01`..`N03` | 000 ✅, 002 ✅, `BE-OPT-010` ✅ | Selesai 2026-11-05 — [laporan](../task/report/frontend/FE-OPT-010.md); 14 uji unit, 22 E2E, lint, build `PASS`; alur daftar–detail diuji dengan backend lokal. Task layar berikutnya menyalakan `OPTICAL_SCREEN_READY` dan menambah butir menunya |
| `FE-OPT-011` ✅ | Form pemeriksaan refraksi + riwayat revisi | `N04`, `N06` | 010 ✅, 001 ✅, `BE-OPT-011` ✅ | Selesai 2026-11-05 — [laporan](../task/report/frontend/FE-OPT-011.md); 19 uji unit, 19 E2E, lint, build `PASS`. Alur tulis asli tertahan: kode profesi `OpticalManagement:Practitioner` kosong dan belum ada akun RO/Sp.M uji |
| `FE-OPT-012` ✅ | Form resep + salin resep luar + riwayat versi + cetak resep | `N05`, `N06` | 010 ✅, 001 ✅, `BE-OPT-012` ✅ | Selesai 2026-11-05 — [laporan](../task/report/frontend/FE-OPT-012.md); 19 uji unit, 20 E2E, lint, build `PASS`. Alur tulis asli tertahan: kode profesi `OpticalManagement:Practitioner` kosong dan belum ada akun RO/Sp.M uji |
| `FE-OPT-020` ✅ | Buat pesanan, katalog, tagihkan, ajukan diskon, status bayar, kirim ke pengerjaan, cetak bukti pesanan | `N07`, `N08` | 012 ✅, 003 ✅, `BE-OPT-032` ✅, `034` ✅, `054` ✅ | Selesai 2026-11-05 — [laporan](../task/report/frontend/FE-OPT-020.md); 15 uji unit, 17 E2E, lint, build `PASS`. Ajukan Diskon mengikuti Billing as-is, tanpa nominal dan tanpa persetujuan (`OPT-APR-004`). Alur tulis asli tertahan data prasyarat |
| `FE-OPT-021` ✅ | Daftar pesanan, tandai siap, serahkan, ajukan/putuskan batal, tutup tidak diambil | `N09`..`N11` | 020 ✅, `BE-OPT-040` ✅, `041` ✅, `054` ✅ | Selesai 2026-11-05 — [laporan](../task/report/frontend/FE-OPT-021.md); 11 uji unit, 15 E2E, lint, build `PASS`. Garansi menampilkan masa dari snapshot, bukan tanggal batas. Alur tulis asli tertahan data prasyarat |
| `FE-OPT-022` ✅ | Penjualan langsung | `N07` (mode langsung) | 021 ✅, `BE-OPT-042` ✅ | Selesai 2026-11-05 — [laporan](../task/report/frontend/FE-OPT-022.md); 5 uji unit, 10 E2E, lint, build `PASS`. Resep lensa kontak pada penjualan langsung tidak terkunci, mengikuti backend. Alur tulis asli tertahan data prasyarat |
| `FE-OPT-030` ✅ | Keluhan garansi: catat (alur dari pesanan), periksa ulang, putuskan | `N12` | 021 ✅, `BE-OPT-050` ✅ | Selesai 2026-11-05 — [laporan](../task/report/frontend/FE-OPT-030.md); 10 uji unit, 15 E2E, lint, build `PASS`. Tautan remake ke pesanan asal belum ada: respons tanpa `ParentOrderId`. Alur tulis asli tertahan data prasyarat dan kode profesi |
| `FE-OPT-031` ✅ | Katalog/profil barang dan pengaturan (Admin Optik) | `N13`, `N14` | 000 ✅, 001 ✅, 003 ✅, `BE-OPT-005` ✅ | Selesai 2026-11-05 — [laporan](../task/report/frontend/FE-OPT-031.md); 11 uji unit, 10 E2E, lint, build `PASS`. Aksi Daftarkan Barang di luar brief menunggu konfirmasi pemilik. devYoga belum punya barang `IsPrescribable = false`, sehingga `UAT-26` belum dapat diuji |
| `FE-OPT-032` ✅ | Empat laporan (satu halaman, 4 tab) | `N15` | 021 ✅, `BE-OPT-052` ✅ | Selesai 2026-11-05 — [laporan](../task/report/frontend/FE-OPT-032.md); 8 uji unit, 9 E2E, lint, build `PASS`. Laporan berisi data belum dapat dilihat dengan backend asli: devYoga belum punya pesanan dan pemeriksaan |
| `FE-OPT-033` ✅ | E2E per UAT `UAT-01`..`26` | Semua | 032 ✅ | Selesai 2026-11-05 — [laporan](../task/report/frontend/FE-OPT-033.md); matriks UAT, 9 skenario baru, 146 E2E `PASS` dengan API tiruan. `UAT-19` belum terpenuhi, `UAT-26` belum dapat diuji. Alur ujung-ke-ujung dengan backend dan data asli belum dijalankan |
| `FE-OPT-040` ✅ | Layar mengirim `Idempotency-Key` pada Simpan Resep (internal, luar), Simpan Draf pesanan baru, dan Catat Keluhan | `N05`, `N07`, `N12` | 012 ✅, 020 ✅, 030 ✅, `BE-OPT-060` ✅ | Selesai 2026-11-06 — [laporan](../task/report/frontend/FE-OPT-040.md); 10 uji unit, 4 E2E, lint, build `PASS`; seluruh E2E Optik 150 lulus. Belum diuji dengan backend asli karena data `B1`, `B2` kosong. Terbit lebih dulu atau bersama `BE-OPT-060` |
| `FE-OPT-041` ✅ | Modal Ajukan Diskon hanya menampilkan kebijakan Optik | `N07`, `N08` | 020 ✅ | Selesai 2026-11-06 — [laporan](../task/report/frontend/FE-OPT-041.md); seluruh E2E Optik 152 lulus. Kebijakan `OPT-DISC-10` belum dibuat di data |
| `FE-OPT-042` ✅ | E2E tambahan: penjamin pada kunjungan Optik dan Daftarkan Barang | `N02`, `N13` | 010 ✅, 031 ✅, `BE-OPT-061` ✅ | Selesai 2026-11-06 — [laporan](../task/report/frontend/FE-OPT-042.md); tanpa kode produksi |

## Rincian Task

### `FE-OPT-000` ✅ — Screen brief

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-04 — [`05-screen-brief.md`](../05-screen-brief.md) `approved` oleh Yoga Aji Pratama; keputusan `OPT-DEC-061`..`075` |
| Outcome | `05-screen-brief.md` disetujui: menu (letak di `menu-items.jsx`, `requiredPermission`), daftar layar, route, daftar/detail, tab/modal, isi kolom daftar |
| Requirement/decision | `OPT-DEC-026`; `OPT-FE-001`, `002` |
| Kontrak | `03-frontend-architecture.md` bagian 2 |
| Dependency | Keputusan pemilik |
| Acceptance criteria | Brief berstatus `approved` dengan ruang `DEV_DISCRETION` yang tertulis |
| Risiko/pemilik | Tanpa brief, layar tidak boleh dibangun. Yoga Aji Pratama |
| DoD | Brief approved |

### `FE-OPT-001` ✅ — Aturan bersama

| Field | Isi |
|---|---|
| Outcome | Utilitas: parse/format desimal bertanda minus dengan kelipatan 0,25; pratinjau transposisi (+1,00 / +0,50 × 90 → +1,50 / −0,50 × 180); cermin `OPT-VAL-010`..`015`, `018`; label status dari metadata |
| Requirement/decision | `OPT-FE-003`; `OPT-DEC-045` |
| Kontrak | `OPT-VAL-v1` |
| Reuse | `DECIMAL_FIELD_PROPS`, `parseDecimal` |
| Dependency | — |
| Acceptance criteria | Uji unit: −1,50 tetap −1,50; −1,30 ditandai salah; transposisi sesuai contoh; AXIS wajib bila CYL ≠ 0 |
| Verifikasi | `node --import ./tests/helpers/register.mjs --test tests/unit/` |
| Risiko/pemilik | Aturan di layar hanya cermin; backend tetap penentu. Optik |
| DoD | Uji unit lulus |

### `FE-OPT-002` ✅ / `FE-OPT-003` ✅ — Lapisan data

| Field | Isi |
|---|---|
| Status | `FE-OPT-002` ✅ Selesai 2026-11-04 — [laporan](../task/report/frontend/FE-OPT-002.md). Kunci idempoten hanya di pendaftaran kunjungan (satu-satunya endpoint klinis yang membacanya); aksi lain dijaga `expectedVersion` dan penahan klik ganda. `FE-OPT-003` ✅ Selesai 2026-11-04 — [laporan](../task/report/frontend/FE-OPT-003.md): pesanan (termasuk `v1.1`), keluhan, laporan, katalog, pengaturan; diskon memakai thunk Billing yang ada; format rupiah dan tanggal WIB |
| Outcome | Service Axios per grup `[Tags]` dan slice Redux; header `Idempotency-Key` per niat aksi; `ExpectedVersion` dikirim; galat `400/403/409/503` dipetakan ke pesan backend |
| Requirement/decision | `OPT-API-v1`; `03-frontend-architecture.md` bagian 4 |
| Kontrak | `OPT-API-v1` (hash terkunci) |
| Dependency | — (paralel dengan backend) |
| Acceptance criteria | Uji unit service/slice dengan API tiruan: klik ganda memakai kunci yang sama; `409` memicu muat ulang |
| Verifikasi | Uji unit |
| Risiko/pemilik | Perubahan kontrak menaikkan versi dan memicu penyesuaian. Optik |
| DoD | Uji unit lulus |

### `FE-OPT-010`..`032` — Layar

Setiap task layar memakai bentuk yang sama:

| Field | Isi |
|---|---|
| Outcome | Layar sesuai kebutuhan `FE-OPT-Nxx` dan screen brief |
| Requirement/decision | Lihat `requirement-traceability.md` |
| Kontrak | `OPT-API-v1`, `OPT-STATE-v1` (tombol per status), `OPT-PERM-v1` (`requiredPermission`) |
| Dependency | `FE-OPT-000` + task data + task backend yang disebut di ringkasan |
| Acceptance criteria | Aturan bagian 3 `03-frontend-architecture.md` terpenuhi; UAT terkait lulus di E2E API-tiruan |
| Verifikasi | Uji unit komponen bila ada logika; E2E standalone |
| Risiko/pemilik | Layar tidak boleh menghitung uang atau status sendiri. Optik |
| DoD | UAT terkait lulus; tidak ada data sensitif di `console` |

### `FE-OPT-033` ✅ — E2E

| Field | Isi |
|---|---|
| Outcome | Spec E2E untuk `UAT-01`..`26` (jalur berhasil dan gagal) |
| Dependency | 032 |
| Acceptance criteria | Seluruh UAT yang punya layar lulus; UAT backend-saja dirujuk ke laporan BE |
| Verifikasi | Laporan E2E; output di scratchpad |
| DoD | Laporan lengkap |

### `FE-OPT-040` ✅ — Kirim kunci simpan ganda

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-06 — [laporan](../task/report/frontend/FE-OPT-040.md). Uji manual dengan backend asli `NOT FEASIBLE` sampai data `B1`, `B2` terisi |
| Outcome | Petugas yang menekan Simpan lagi sesudah jaringan putus melihat data pertama, bukan data ganda. **Contoh:** jaringan putus sesudah keluhan KLM-0001 tersimpan, Dedi menekan Simpan lagi dengan isi yang sama, dan layar menampilkan KLM-0001 |
| Requirement/decision | `FR-OPT-033`, `044`, `072`; `OPT-DEC-077`; brief r3 bagian 6 |
| Kontrak | `OPT-API-v1.2`; `03-frontend-architecture.md` r2 bagian 4 |
| Reuse | `createOpticalIdempotencyKey`, `isOpticalOutcomeUnknown` (`optical-request-rules.js`), `OPTICAL_IDEMPOTENCY_HEADER`, pola niat dan sidik isi `optical-visit-slice.jsx` |
| Cakupan | `optical-clinical.service.js`, `optical-order.service.js`; `optical-prescription-slice.jsx`, `optical-order-slice.jsx`, `optical-warranty-claim-slice.jsx`; uji di `tests/`. Tanpa perubahan tata letak |
| Dependency | `FE-OPT-012` ✅, `020` ✅, `030` ✅; persetujuan r2 ✅; approval task ✅ (Yoga Aji Pratama, 2026-11-06) |
| Urutan rilis | Task ini **terbit lebih dulu atau bersama** `BE-OPT-060`. Backend lama mengabaikan header itu, sehingga aman |
| Acceptance criteria | (1) Hasil tidak diketahui (jaringan putus atau `503`) + isi sama: kunci yang sama dipakai lagi. (2) Isi diubah: kunci baru. (3) Server menjawab berhasil atau menolak `400`/`403`/`409`: kunci dibuang. (4) Jawaban `IsReplay = true` membuka data pertama seperti simpan biasa. (5) `UAT-32`, `33` lulus di E2E API tiruan. (6) Mode Ubah pesanan (`PUT`) tidak mengirim kunci baru |
| Verifikasi | `node --import ./tests/helpers/register.mjs --test tests/unit/`; E2E standalone dengan output ke scratchpad; lint; build |
| Risiko/pemilik | Empat form berbeda harus memakai satu pola, agar tidak ada yang tertinggal dan ditolak `400`. Optik |
| DoD | Kriteria (1)–(6) terbukti; laporan `task/report/frontend/FE-OPT-040.md`; traceability diperbarui |

### `FE-OPT-041` ✅ — Penyaring kebijakan diskon Optik

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-06 — [laporan](../task/report/frontend/FE-OPT-041.md). `OPT-APR-014` dan `OPT-OQ-020` ✅ (`OPT-DEC-084`): penanda kode berawalan `OPT-`. Kebijakan datanya dibuat pemilik Billing |
| Outcome | Petugas hanya melihat kebijakan Optik. **Contoh:** Diskon Optik 10% tampil, promo 50% milik unit lain tidak tampil; memilih 10% pada frame Rp 450.000 mengurangi tagihan Rp 45.000 |
| Requirement/decision | `FR-OPT-082`, `083`; `OPT-DEC-079`; risiko `R4` |
| Kontrak | `OPT-INT-v1.1` `INT-OPT-11`; `OPT-PERM-v1.1` (`DiscountPolicy : Read`); brief r3 §5.8 |
| Reuse | `optical-order-action-modals.jsx` (penyaring `promoPolicies`), hook Billing `use-apply-billing-discount.js` tanpa perubahan |
| Cakupan | Penyaring di modal Optik dan konstantanya; uji. **Jangan** mengubah hook Billing, karena dipakai layar Billing lain |
| Dependency | `FE-OPT-020` ✅; persetujuan r2 ✅; `OPT-APR-014` ✅; `OPT-OQ-020` ✅; approval task ✅. Data: kebijakan Diskon Optik dibuat pemilik Billing |
| Acceptance criteria | (1) `OPT-AC-037`: hanya kebijakan berpenanda Optik di pilihan. (2) Tidak ada kebijakan Optik: tampil "Belum ada kebijakan diskon aktif." (3) Tanpa `DiscountPolicy : Read`: pesan server tampil apa adanya. (4) `UAT-28`, `29` lulus di E2E API tiruan. (5) Layar Billing tetap menampilkan seluruh kebijakannya |
| Verifikasi | Uji unit penyaring; E2E standalone; lint; build. Dengan backend asli sesudah kebijakan dibuat |
| Risiko/pemilik | Hook memuat paling banyak 100 kebijakan aktif, sehingga kebijakan Optik bisa terpotong bila kebijakan aktif lebih dari 100. Periksa saat pelaksanaan dan catat di laporan. Penyaringan hanya di layar (risiko diterima `OPT-DEC-079`). Optik |
| DoD | Kriteria (1)–(5) terbukti; laporan `task/report/frontend/FE-OPT-041.md`; traceability diperbarui |

### `FE-OPT-042` ✅ — E2E penjamin dan Daftarkan Barang

| Field | Isi |
|---|---|
| Status | ✅ Selesai 2026-11-06 — [laporan](../task/report/frontend/FE-OPT-042.md). Hanya berkas uji; `UAT-34`, `35` terbukti dengan API tiruan sampai `B2` terisi |
| Outcome | Bukti bahwa layar yang sudah ada menangani penjamin dan Daftarkan Barang sesuai brief r3, tanpa mengubah alur |
| Requirement/decision | `FR-OPT-012`, `112`; `OPT-DEC-078`, `080`; risiko `R6` |
| Kontrak | `OPT-VAL-v1.1` bagian 8; brief r3 §5.2, §5.12 |
| Reuse | Spec E2E `FE-OPT-010` dan `FE-OPT-031` |
| Cakupan | Spec E2E baru. Perubahan kode layar hanya bila uji menemukan selisih |
| Dependency | `FE-OPT-010` ✅, `031` ✅; `BE-OPT-061` ✅ |
| Acceptance criteria | (1) `UAT-30`: Penjamin Perusahaan terkirim dan kunjungan tampil. (2) `UAT-31`: pesan penolakan server tampil apa adanya. (3) `UAT-34`: barang terdaftar tampil di katalog. (4) `UAT-35`: pencarian terkirim dengan `isPrescribable=false`. (5) `OPT-AC-038`: keterangan penimpaan tampil |
| Verifikasi | E2E standalone dengan output ke scratchpad; lint |
| Risiko/pemilik | Tanpa barang `IsPrescribable = false` di devYoga (`B2`), `UAT-34`, `35` hanya terbukti dengan API tiruan. Optik |
| DoD | Kriteria (1)–(5) terbukti atau sisa dicatat; laporan `task/report/frontend/FE-OPT-042.md`; traceability diperbarui |
