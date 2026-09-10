# Requirement Traceability — Modul Platform / Alokasi Nomor Bisnis

```yaml
module_id: platform
blueprint_id: PLT-BP-001
blueprint_shape: SINGLE
roadmap_revision: 1
status: DRAFT
contract_version: v1 (approved)
backend_source_sha: f0d6855
frontend_source_sha: 101ec5d3a560bd6e54d4665ae53d425f255c609f
decision_revision: 4
```

---

## 1. Kebutuhan → keputusan → task

| Kebutuhan | Keputusan | Task backend | Task frontend | Keadaan |
| --- | --- | --- | --- | --- |
| Modul punya kepemilikan dan prefix yang sah | `DEC-PLT-009`, `DEC-PLT-010` | ✅ `PLT-BE-001` | — | **Terbukti** — `Resolved: True` terhadap checker |
| Pencacah deret tersimpan dan dijaga unik | `DEC-PLT-002`, `INV-PLT-001` | ✅ `PLT-BE-002` | — | **Terbukti** ([laporan](../task/report/backend/PLT-BE-002.md)) — `AC-PLT-011` lulus di SQLite |
| Nomor dialokasikan atomik dan durabel | `DEC-PLT-007`, `DEC-PLT-008` | ✅ `PLT-BE-003` | — | **Terbukti** ([laporan](../task/report/backend/PLT-BE-003.md)). **Durabilitas belum terbukti** — `PLT-BE-004` |
| Durabilitas dan antrean terbukti | `DEC-PLT-008`, `INV-PLT-001` | 🟡 `PLT-BE-004` | — | **Uji ditulis, nol dijalankan** ([laporan](../task/report/backend/PLT-BE-004.md)). **Diperiksa ulang 10 September 2026:** 0 lulus, 6 gagal, seluruhnya `42501: permission denied to create database`. Menunggu hak `CREATEDB`, bukan lagi menunggu server |
| Kewenangan platform/modul terbagi | `DEC-PLT-005` | ✅ `PLT-BE-003` | — | **Terbukti** — 4 dari 6 parameter milik pemanggil |
| Deret baru tidak pernah diulang | `DEC-PLT-004`, `INV-PLT-004` | ✅ `PLT-BE-003` | — | **Terbukti** — pergantian tahun tidak mengulang deret |
| Keadaan deret dapat dibaca administrator | `FR-PLT-012`, `FR-PLT-013` | ✅ `PLT-BE-005` | ⛔ `PLT-FE-001` | **Terbukti** ([laporan](../task/report/backend/PLT-BE-005.md)) — 4 endpoint baca, 22 kasus uji lulus. **Belum dapat dipanggil**: migration belum dijalankan |

**Nol requirement yatim.** Ketujuh kebutuhan pada `04-prd-to-mvp.md` punya task pemilik.

---

## 2. Acceptance criteria → task → bukti

| AC | Task | Bukti yang direncanakan |
| --- | --- | --- |
| `AC-PLT-001`, `AC-PLT-002` | ✅ `PLT-BE-003` | **Terbukti** — alokasi pertama dan berurutan |
| **`AC-PLT-003`** | 🟡 `PLT-BE-004` | Uji **ada dan compile**; **belum pernah dijalankan** — dicoba 10 September 2026, ditolak `42501` sebelum database terbentuk |
| **`AC-PLT-004`**, `AC-PLT-005` | 🟡 `PLT-BE-004` | Uji **ada dan compile**; **belum pernah dijalankan** — dicoba 10 September 2026, ditolak `42501` |
| `AC-PLT-006`, `AC-PLT-007` | ✅ `PLT-BE-003` | **Terbukti** — `NEVER` tidak diulang, `DAILY` berpindah periode |
| `AC-PLT-008`, `AC-PLT-009`, `AC-PLT-010` | ✅ `PLT-BE-003` | **Terbukti** — 5 kasus penolakan parameter + `VAL-PLT-007`, nol nomor terbit |
| `AC-PLT-011` | ✅ `PLT-BE-002` | **Terbukti** — `PasanganDeretDanPeriode_KemBar_DitolakDatabase`; 10 uji lulus di `UnitTests.Sqlite` |
| **`AC-PLT-012`** | 🟡 `PLT-BE-004` | Uji **ada dan compile**; **belum pernah dijalankan** — dicoba 10 September 2026, ditolak `42501` |

**Hitungan bukti.** Dari 12 acceptance criteria, **delapan sudah terbukti** — `AC-PLT-011` lewat
`PLT-BE-002`, dan `AC-PLT-001`, `002`, `006`..`010` lewat `PLT-BE-003`, seluruhnya 9 September 2026. Empat di antara sisanya (`AC-PLT-003`, `004`, `005`, `012`)
**tidak dapat** dibuktikan tanpa database test PostgreSQL tersendiri, dan seluruhnya milik
`PLT-BE-004`. **Dikoreksi 10 September 2026:** servernya sudah ada dan terjangkau; yang belum ada
adalah hak membuat database test di sana.

**Batas yang jujur atas `AC-PLT-011`.** Index unik dan kedua check constraint terbukti di
**SQLite**, bukan PostgreSQL. Bentuknya sama, tetapi pembuktian di database sasaran menunggu
migration dijalankan.

---

## 3. Coverage gap yang diakui

| Gap | Keadaan | Akibat |
| --- | --- | --- |
| ~~Baris registry `Platform`/`Num`~~ (`P1` / `OQ-PLT-014`) | ✅ **Tertutup 9 September 2026** — dicatat dan `ACTIVE`, terbukti diterima checker | Gap tertutup. `PLT-BE-002` terbuka |
| **Tiga salinan registry tidak sinkron** — `FACT-PLT-012` | Yang **ditegakkan** ada di `NewQuilvianSystemBackend/docs/engineering/`; salinan suite skill tertinggal satu entri changelog; plugin cache terpasang tertinggal perbaikan `Mst` 2026-09-04 | **Tidak menahan `PLT-SLICE-01`.** Berisiko menyesatkan agent berikutnya yang membaca salinan basi. Sinkronisasi milik pemilik registry |
| **Pembuktian menuntut PostgreSQL** | `AC-PLT-003`, `004`, `005`, `012` mustahil dibuktikan provider InMemory | Uji di InMemory akan **lulus tanpa membuktikan apa pun** — karena itu `PLT-BE-004` dipisah sebagai task tersendiri, bukan digabung |
| **Role tanpa hak `CREATEDB`** — ditemukan 10 September 2026 | Server PostgreSQL terjangkau, target `QuilvianNumberSeriesTest` lolos ketiga penjagaan fixture, tetapi `CREATE DATABASE` ditolak `42501` | Menahan keempat `AC` di atas. Penutupnya satu tindakan pemilik server: `ALTER ROLE <role> CREATEDB;` atau DBA membuatkan database-nya |
| **`AddDbContextFactory` belum diverifikasi** | Berdampingan dengan `AddDbContext` yang sudah melayani seluruh aplikasi | Risiko implementasi `PLT-BE-003`; **tidak** boleh dianggap pasti aman |
| Perilaku deret hampir habis | `OQ-PLT-005` terbuka | Sementara ditahan tegas oleh `VAL-PLT-007` |
| Kode fasilitas dalam awalan | `OQ-PLT-006` terbuka | `LATER SLICE` |
| Nomor kembar yang mungkin sudah terbit | `OQ-PLT-009` terbuka | `PLT-SLICE-04`; risiko lama dibiarkan apa adanya |
| ~~Perlu-tidaknya layar pemantauan~~ | Backend-nya **sudah berdiri** — `PLT-BE-005` selesai 10 Sep 2026. Brief navigasi masih belum ada | Tidak lagi menjadi gap backend. `PLT-FE-001` tetap menunggu brief navigasi; mencabutnya kini berarti membuang endpoint yang sudah terbukti, bukan sekadar membatalkan rencana |
| **Migration `NumNumberSeries` belum dijalankan** | Tabelnya belum ada di lingkungan mana pun | Menahan **pemakaian nyata** `PLT-BE-003` dan keempat endpoint `PLT-BE-005`; nol pengaruh pada pembuktian kodenya. Wewenang terpisah |

---

## 4. Keputusan yang masih terbuka

| ID | Ringkasan | Memblokir | Pemilik |
| --- | --- | --- | --- |
| ~~`OQ-PLT-014`~~ | ~~Baris registry dicatat dan `ACTIVE`~~ ✅ **Tertutup 9 September 2026** | Tidak lagi memblokir | — |
| `DEC-PLT-006` | Penerimaan pelanggaran `INV-PLT-001` selama peralihan | `PLT-SLICE-02` | Pemilik platform |
| `OQ-PLT-005` | Panjang nomor dan deret hampir habis | `PLT-SLICE-03` | Pemilik platform |
| `OQ-PLT-006` | Kode fasilitas di dalam awalan | `LATER SLICE` | Pemilik platform |
| `OQ-PLT-009` | Penelusuran nomor kembar produksi | `PLT-SLICE-04` | Pemilik platform |

---

## 5. Dampak ke modul lain

| Modul | Yang menunggu | Terbuka ketika |
| --- | --- | --- |
| **Bank Darah** (`BD-BP-001`) | Gerbang `G4` — 9 task backend, 8 task frontend | ✅ **`PLT-BE-003` selesai** 9 Sep 2026 — `G4` tertutup secara kemampuan. **Disarankan menunggu `PLT-BE-004` lulus** sebelum dijadwalkan |
| Billing & Kasir | Tidak menunggu apa pun | Empat deret produksinya **tidak disentuh** slice ini |
| Rawat Inap, Rawat Jalan | Direncanakan memakai (`FACT-PLT-006`) | Belum dijadwalkan |

---

## 6. Rekap angka

| Butir | Jumlah |
| --- | ---: |
| Task backend | 5 |
| — ✅ selesai | **4** — `PLT-BE-001`, `PLT-BE-002`, `PLT-BE-003`, `PLT-BE-005` |
| — 🟡 selesai sebagian | **1** — `PLT-BE-004`, nol dari 4 AC terbukti; tertahan hak `CREATEDB` |
| — 🟡 pending, siap dijadwalkan | **0** — nol tersisa |
| — ⛔ blocked | 0 |
| Task frontend | 1 |
| — ⛔ blocked | 1 |
| **Total task** | **6** |
| Acceptance criteria | 12 |
| — terbukti | **8** dari 12 |
| — menuntut database test PostgreSQL | 4 — **uji-nya sudah ditulis**; tertahan hak `CREATEDB`, bukan tertahan server |
| Keputusan approved | `DEC-PLT-001`..`005`, `007`..`010`, `INV-PLT-001`..`004` |
| Gerbang tertutup | `P0`, **`P1`** |
| Gerbang terbuka | **nol** |
