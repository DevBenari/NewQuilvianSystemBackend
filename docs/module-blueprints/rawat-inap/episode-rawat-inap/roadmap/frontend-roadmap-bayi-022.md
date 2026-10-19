# Roadmap Frontend — Perbaikan Admisi Bayi 022

Kontrak terkunci: [`RWI-BAYI-022-2.0.0`](../contracts/newborn-admission-repair-022.md), disetujui
pengguna 16 Oktober 2026. Source UI tetap berada di `QuilvianSystemFrontendDev`; laporan tracked
berada pada blueprint ini.

## Grafik Urutan Dependency

```text
BE-RWI-244 [BE] ─┐
BE-RWI-247 [BE] ─┼─> FE-RWI-257 ─> FE-RWI-258 ─> FE-RWI-259
BE-RWI-245 [BE] ─┘
BE-RWI-246 [BE] ────────────────────────┘
```

`[BE]` adalah dependency baca-saja dari roadmap backend.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-RWI-244`, `BE-RWI-247` | `FE-RWI-257` |
| 2 | `FE-RWI-257`, `BE-RWI-245` | `FE-RWI-258` |
| 3 | `FE-RWI-258`, `BE-RWI-246` | `FE-RWI-259` |

### `FE-RWI-257` — Stepper tujuh langkah dan pemilihan ibu

| Field | Nilai |
| --- | --- |
| Outcome | Alur bayi dimulai dari kandidat kasus persalinan dan tidak memiliki step payment/deposit |
| Trace | `PKJ-AB-06`, sebagian `PKJ-AB-07` |
| Contract | `RWI-BAYI-022-2.0.0` |
| Dependency | `BE-RWI-244`, `BE-RWI-247` |
| Wewenang UI | Komposisi layar Admisi Bayi; alur umum/transfer dan pembatalan tidak diubah |
| Klasifikasi | `MEDIUM` |
| Status | ✅ Selesai source |

Acceptance criteria:

1. Stepper bayi berisi tepat tujuh langkah dan selalu masuk langkah 1.
2. URL `payment`/`deposit` mode bayi dialihkan ke penempatan tanpa crash.
3. Langkah 1 mencari candidate endpoint dan menampilkan identitas, kasus, lokasi, penjamin, serta jumlah bayi.
4. Pilihan luar RS/rujukan, admisi ulang, dan ringkasan besar lintas langkah tidak tampil.

Definition of Done: lint, unit suite, build, laporan, roadmap, dan traceability diperbarui.

### `FE-RWI-258` — Identitas bayi sistem dan urutan otomatis

| Field | Nilai |
| --- | --- |
| Outcome | Petugas mengisi data lahir tanpa mengetik nama bebas atau urutan kembar |
| Trace | `PKJ-AB-03`, `PKJ-AB-07` |
| Contract | `RWI-BAYI-022-2.0.0` |
| Dependency | `FE-RWI-257`, `BE-RWI-245` |
| Wewenang UI | Form identitas bayi dan integrasi endpoint khusus |
| Klasifikasi | `MEDIUM` |
| Status | ✅ Selesai source |

Acceptance criteria:

1. Nama sementara ditampilkan baca-saja sebagai `By. Ny. <ibu>`.
2. Tanggal lahir, jam lahir, dan jenis kelamin wajib; input tunggal/kembar dan urutan manual hilang.
3. `Idempotency-Key` stabil dipakai untuk retry, `birthOrder` tidak dikirim.
4. Urutan final dan nomor RM dari server tampil sesudah penyimpanan.

Definition of Done: test util relevan, lint, unit suite, build, laporan, dan register selesai.

### `FE-RWI-259` — Penjamin turunan, ringkasan final, dan pemulihan bed

| Field | Nilai |
| --- | --- |
| Outcome | Encounter memakai sumber ibu; konfirmasi menjelaskan penjamin/deposit baca-saja dan kegagalan bed tidak menghapus form |
| Trace | `PKJ-AB-04`, `PKJ-AB-08`, `PKJ-AB-09`, `PKJ-AB-10` |
| Contract | `RWI-BAYI-022-2.0.0` |
| Dependency | `FE-RWI-258`, `BE-RWI-246` |
| Wewenang UI | Wiring encounter, review, error recovery, dan validasi live browser; pembatalan existing tidak diubah |
| Klasifikasi | `HEAVY` |
| Status | ✅ Selesai source; verifikasi live menunggu migration backend |

Acceptance criteria:

1. Encounter bayi mengirim referensi dan versi sumber penjamin ibu, bukan pilihan payer manual.
2. Tidak ada input nominal deposit atau pilihan penggabungan invoice.
3. Konfirmasi saja yang menampilkan identitas lengkap, urutan, DPJP, bed, penjamin, dan informasi deposit ibu.
4. Konflik service unit/bed membersihkan pilihan bed saja serta mempertahankan ibu, bayi, dan DPJP.
5. Regresi alur umum dijalankan; live browser dicatat bila runtime tersedia.

Definition of Done: lint, unit suite, build, pemeriksaan interaktif, laporan, roadmap, dan traceability diperbarui apa adanya.

## Register Status

| Task | Status | Laporan |
| --- | :---: | --- |
| `FE-RWI-257` | ✅ Selesai source | [FE-RWI-257.md](../task/report/frontend/FE-RWI-257.md) |
| `FE-RWI-258` | ✅ Selesai source | [FE-RWI-258.md](../task/report/frontend/FE-RWI-258.md) |
| `FE-RWI-259` | ⚠️ Source selesai; live pending | [FE-RWI-259.md](../task/report/frontend/FE-RWI-259.md) |

