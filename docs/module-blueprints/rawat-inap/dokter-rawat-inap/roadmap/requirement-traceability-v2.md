# Requirement Traceability V2 — Sub-modul Dokter Rawat Inap

> Berkas ini **baru** dan hanya melacak penyelarasan `PRD-RWI-V2-001` revision `7`.
> Traceability task lama tetap di [`requirement-traceability.md`](./requirement-traceability.md).

## Metadata

```yaml
blueprint_id: RWI-BP-001
blueprint_revision: 7
submodule: dokter-rawat-inap
traceability_revision: 2
contract_version: 0.6.0
approval_decision: RWI-DEC-150
gate_closure_decision: RWI-DEC-151   # {GATE-YOGA} tertutup 2026-09-16
approved_by: "Muhammad Hamzah"
approved_at: "2026-09-16"
upstream_input: "PRD-RWI-V2-001 v2.0"
input_revision_hash: sha256:2b3b2f29c9e547f448f186d7ac990e33dc3bdede8043a9b4bebfad6fbe0a679f
backend_source_sha: df3679c0d5b2f08106702153eb242d3a6cb2929b
frontend_source_sha: 1ce219b40f8e411f3c4e66975626ab33ae81616a
backend_roadmap: roadmap/backend-roadmap-v2.md
frontend_roadmap: roadmap/frontend-roadmap-v2.md
fr_range: FR-DOK-069..FR-DOK-111
last_updated: "2026-09-16 — BE-RWI-092 sampai BE-RWI-096 selesai secara statis tanpa dotnet build"
```

Label `[BE-INP]`, `[FE-INP]`, `[FE-KEP]` menandai task milik sub-modul lain.

---

## 1. `EPIC DOK-10` — Ruang kerja satu halaman dan daftar pasien

| FR | Disposisi | Task BE | Task FE | Kontrak | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `FR-DOK-069` | `EXTEND` | `BE-RWI-081` [BE-INP] | `FE-RWI-067` | API 10.1 | `UAT-45` | **Bebas** — `{GATE-RAJAL}` tertutup `RWI-DEC-152` |
| `FR-DOK-070` | `EXTEND` | `BE-RWI-081` [BE-INP] | `FE-RWI-067` | API 10.1 | Acceptance 18.1 | **Bebas** — `{GATE-RAJAL}` tertutup `RWI-DEC-152` |
| `FR-DOK-071` | `EXTEND` | `BE-RWI-081` [BE-INP] | `FE-RWI-067` | API 10.1 | Acceptance 18.1 | **Bebas** — `{GATE-RAJAL}` tertutup `RWI-DEC-152` |
| `FR-DOK-072` | Frontend | — | `FE-RWI-067` | `UI-AC-DOK-001` s.d. `012` | Tangkapan layar bertopeng tiga lebar | **Bebas** — `RWI-DEC-152`; bukti tangkapan layar **tetap wajib** |
| `FR-DOK-073` | Frontend | — | `FE-RWI-067` | — | Verifikasi manual | **Bebas** — `RWI-DEC-152` |

## 2. `EPIC DOK-11` — Kewenangan penulis, registrasi, penguncian, Catatan Saya

| FR | Disposisi | Task BE | Task FE | Kontrak | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `FR-DOK-074` | `EXTEND` | `BE-RWI-091` | `FE-RWI-068`, `FE-RWI-070` | state matrix | Acceptance registrasi | Belum dikerjakan |
| `FR-DOK-075` | `REPAIR` | `BE-RWI-088` | `FE-RWI-068` | permission matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-076` | `REPAIR` | `BE-RWI-088` | `FE-RWI-070` | permission matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-077` | `REPAIR` | `BE-RWI-091` | `FE-RWI-068` | state matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-078` | `MISSING / NEW` — dipicu `episode-rawat-inap` | `BE-RWI-082` [BE-INP] | `FE-RWI-068` | `INT-INP-08` | `UAT-50` | Belum dikerjakan |
| `FR-DOK-079` | `EXTEND` + `MISSING / NEW` | `BE-RWI-092` | `FE-RWI-077` | API `my-authored` | Validasi source/QBE; runtime menunggu build mandiri | ✅ **BE selesai** — [laporan](../task/report/backend/BE-RWI-092.md); FE tetap terpisah |
| `FR-DOK-080` | `EXISTING / REUSE` | `BE-RWI-093` | `FE-RWI-077` | permission matrix | Validasi reuse/source; runtime menunggu build mandiri | ✅ **BE selesai** — [laporan](../task/report/backend/BE-RWI-093.md); FE tetap terpisah |
| `FR-DOK-081` | `REPAIR` | `BE-RWI-090` | — | validation matrix | Verifikasi proses bisnis | Belum dikerjakan |

## 3. `EPIC DOK-12` — Verifikasi CPPT dan jenis catatan

| FR | Disposisi | Task BE | Task FE | Kontrak | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `FR-DOK-082` | `REPAIR` | `BE-RWI-089` | `FE-RWI-069` | permission matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-083` | `EXTEND` | `BE-RWI-095` | `FE-RWI-069`, `FE-RWI-080` | state matrix | Validasi source/QBE; runtime menunggu build mandiri | ✅ **BE selesai** — [laporan](../task/report/backend/BE-RWI-095.md); FE tetap terpisah |
| `FR-DOK-084` | `MISSING / NEW` | `BE-RWI-096` | `FE-RWI-078`, `FE-RWI-080` | API daftar tunggu | Validasi source/QBE; runtime menunggu build mandiri | ✅ **BE selesai** — [laporan](../task/report/backend/BE-RWI-096.md); FE tetap terpisah |
| `FR-DOK-085` | `EXTEND` | `BE-RWI-094` | `FE-RWI-069` | data dictionary | Validasi source/schema statis; migration tidak dijalankan | ✅ **BE selesai** — [laporan](../task/report/backend/BE-RWI-094.md); FE tetap terpisah |

## 4. `EPIC DOK-13` — Resep Harian, penghentian butir, template, rekonsiliasi

| FR | Disposisi | Task BE | Task FE | Kontrak | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `FR-DOK-086` | `EXTEND` | `BE-RWI-099` | `FE-RWI-071` | API resep harian | Verifikasi kontrak API | Belum dikerjakan |
| `FR-DOK-087` | `MISSING / NEW` | `BE-RWI-100` | `FE-RWI-087` [FE-KEP] | `INT-KEP-09` | Verifikasi proses bisnis | Menunggu `BE-RWI-114` [BE-KEP] |
| `FR-DOK-088` | `REPAIR` | `BE-RWI-105` | `FE-RWI-071` | permission matrix | Regresi poliklinik | Belum dikerjakan |
| `FR-DOK-089` | `REPAIR` | `BE-RWI-105` | `FE-RWI-071` | permission matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-090` | `MISSING / NEW` | `BE-RWI-105` | `FE-RWI-071` | validation matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-091` | `REPAIR` | `BE-RWI-105` | `FE-RWI-071` | validation matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-092` | `MISSING / NEW` | `BE-RWI-101` | `FE-RWI-072` | data + state | Verifikasi skema dan API | Belum dikerjakan |
| `FR-DOK-093` | `MISSING / NEW` | `BE-RWI-101` | `FE-RWI-072` | state matrix | Verifikasi proses bisnis | Belum dikerjakan |

## 5. `EPIC DOK-14` — Protokol dan order sliding scale

| FR | Disposisi | Task BE | Task FE | Kontrak | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `FR-DOK-094` | `MISSING / NEW` | `BE-RWI-102` | `FE-RWI-079` | data + validation | Verifikasi skema; uji rentang tumpuk dan lubang | Belum dikerjakan |
| `FR-DOK-095` | `MISSING / NEW` | `BE-RWI-102` | `FE-RWI-079` | state matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-096` | `MISSING / NEW` | `BE-RWI-103` | `FE-RWI-072` | API order | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-097` | `MISSING / NEW` | `BE-RWI-103` | `FE-RWI-072` | state matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-098` | `MISSING / NEW` | `BE-RWI-103` | `FE-RWI-072` | validation matrix | Verifikasi kontrak API | Belum dikerjakan |
| `FR-DOK-099` | `MISSING / NEW` | `BE-RWI-103` | `FE-RWI-072` | state matrix | Verifikasi proses bisnis | Belum dikerjakan |

## 6. `EPIC DOK-15` — Pesanan tindakan dan penunjang dengan instruksi

| FR | Disposisi | Task BE | Task FE | Kontrak | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `FR-DOK-100` | `EXTEND` | `BE-RWI-097` | `FE-RWI-073`, `FE-RWI-089` [FE-KEP] | data + API | Verifikasi skema dan API | Belum dikerjakan |
| `FR-DOK-101` | `EXISTING / REUSE` | `BE-RWI-097` | — | validation matrix | **Regresi poliklinik** | Belum dikerjakan |
| `FR-DOK-102` | `REPAIR` | `BE-RWI-097` | `FE-RWI-073` | permission matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-103` | `EXISTING / REUSE` | `BE-RWI-098` | `FE-RWI-073` | permission matrix | Verifikasi proses bisnis | Belum dikerjakan |
| `FR-DOK-104` | `MISSING / NEW` | `BE-RWI-098` | `FE-RWI-073`, `FE-RWI-078` | state matrix | Verifikasi kontrak API | Belum dikerjakan |
| `FR-DOK-105` | `MISSING / NEW` — dipicu `episode-rawat-inap` | `BE-RWI-083` [BE-INP] | `FE-RWI-066` [FE-INP] | `INT-INP-09` | `UAT-50` | Belum dikerjakan |
| `FR-DOK-106` | `EXTEND` | `BE-RWI-104` | `FE-RWI-076` | integrasi Lab/Rad | Verifikasi skema; **regresi Lab/Rad** | **Bebas** — `{GATE-LABRAD}` tertutup `RWI-DEC-153` |

## 7. `EPIC DOK-16` — Tab Resume Medis

| FR | Disposisi | Task BE | Task FE | Kontrak | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `FR-DOK-107` | `EXTEND` — dirancang `episode-rawat-inap` | `BE-RWI-085` [BE-INP] | `FE-RWI-074` | `0.9.0` data 18.2–18.3 | Acceptance 18.3 | **Bebas** — `RWI-DEC-152` |
| `FR-DOK-108` | `MISSING / NEW` | `BE-RWI-086` [BE-INP] | `FE-RWI-074` | `0.9.0` API 10.3 | Acceptance 18.3 | **Bebas** — `RWI-DEC-152` |
| `FR-DOK-109` | Frontend | — | `FE-RWI-074` | — | Verifikasi manual | **Bebas** — `RWI-DEC-152` |

## 8. `EPIC DOK-17` — Penunjang enam layanan

| FR | Disposisi | Task BE | Task FE | Kontrak | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `FR-DOK-110` | `EXISTING / REUSE` | — (nol backend baru) | `FE-RWI-076` | API Lab/Rad yang sudah ada | Verifikasi manual | **Bebas** — `RWI-DEC-152` |
| `FR-DOK-111` | Frontend | — | `FE-RWI-076` | — | Panel Network menunjukkan nol permintaan | **Bebas** — `RWI-DEC-152` |

---

## 9. Ringkasan cakupan

| Hal | Angka |
| --- | ---: |
| FR revision `7` sub-modul ini | **43** (`FR-DOK-069` s.d. `FR-DOK-111`) |
| FR yang punya task backend di roadmap ini | 30 |
| FR yang task backend-nya milik sub-modul lain | 4 (`FR-DOK-078`, `105`, `107`, `108`) |
| FR berdisposisi **Frontend** — memang nol backend | 5 (`FR-DOK-072`, `073`, `109`, `111`, dan `110` yang reuse penuh) |
| FR tanpa task sama sekali | **0** |
| FR tanpa bukti acceptance | **0** |

**Gap keterkaitan requirement ke bukti verifikasi: nol.** FR berdisposisi Frontend tidak punya task
backend, dan itu **bukan** gap — buktinya berupa verifikasi manual dan tangkapan layar, sesuai
`rules/backend/TEST_POLICY.md` yang menyatakan tidak adanya automated test backend bukan coverage gap.

---

## 10. Dependency lintas sub-modul

| Arah | Task | Menunggu | Kenapa |
| --- | --- | --- | --- |
| Masuk | `BE-RWI-091`, `094`, `097` | `BE-RWI-079` [BE-INP] | Penjaga penulis klinis membaca tujuan penugasan — `02-module-map.md` 3.4.1 V1 |
| Masuk | `BE-RWI-100` | `BE-RWI-114` [BE-KEP] | Pembatalan dosis butuh tabel MAR |
| Masuk | `FE-RWI-067` | `BE-RWI-081` [BE-INP] | Daftar pasien memakai census `assignedToMe` |
| Masuk | `FE-RWI-074` | `BE-RWI-085`, `086` [BE-INP] | Tab Resume memakai kontrak `0.9.0` |
| Keluar | `BE-RWI-091` | dibutuhkan `BE-RWI-082` [BE-INP] | Penguncian konsep saat penutupan |
| Keluar | `BE-RWI-097` | dibutuhkan `BE-RWI-083` [BE-INP] | Pembatalan pesanan saat penutupan |
| Keluar | `BE-RWI-094` | dibutuhkan `BE-RWI-124` [BE-KEP] | SOAP keperawatan disimpan sebagai CPPT berjenis |
| Keluar | `BE-RWI-101` | dibutuhkan `BE-RWI-125` [BE-KEP] | Perawat membaca keputusan rekonsiliasi |
| Keluar | `BE-RWI-097` | dibutuhkan `BE-RWI-125` [BE-KEP] | Perawat memesan tindakan dengan pemberi instruksi |

---

## 11. Gerbang yang belum tertutup

| Gerbang | Jenis | Menahan | Pemilik |
| --- | --- | --- | --- |
| ~~`{GATE-RAJAL}` ekstraksi komponen~~ | **TERTUTUP 2026-09-16** `RWI-DEC-152` | ~~`FE-RWI-067` dan sembilan tab~~ — seluruhnya bebas | **Sukma GP** ✅ |
| ~~`{GATE-YOGA}` `my-authored`~~ | **TERTUTUP 2026-09-16** `RWI-DEC-151` | ~~`BE-RWI-092`, `FE-RWI-077`~~ — keduanya bebas | Yoga Aji Pratama ✅ |
| ~~`{GATE-LABRAD}` kolom instruksi~~ | **TERTUTUP 2026-09-16** `RWI-DEC-153` | ~~`BE-RWI-104`~~ — bebas; **regresi Lab/Rad tetap wajib** | **Yoga Aji** ✅ |
| ~~Pemberitahuan `rawat-jalan` atas `R7` dan `R9`~~ | **TERTUTUP 2026-09-16** `RWI-DEC-152` | ~~Rilis `BE-RWI-097` dan `BE-RWI-105`~~ — **regresi poliklinik tetap wajib** | **Sukma GP** ✅ |
| Pengesah isi protokol sliding scale — **`RWI-OQ-097`** | Gerbang produksi, **sebagian tertutup** | Kewenangan memakai **sudah** ada `RWI-DEC-155`; **nama** pengesah belum, sehingga nol versi dapat `Approved` | Manajemen rumah sakit |

---

## 12. Usulan non-blocking yang diadopsi saat approval

`RWI-DEC-150` bagian b pada `../blueprint-manifest.md` 0-B.7 mencatat dua usulan sub-modul ini
sebagai `ADOPTED_AS_PROPOSED`:

| Sumber | Usulan | Gate | Task yang menanggungnya |
| --- | --- | --- | --- |
| 22.20 nomor 7 | Dosis sliding scale terjadwal; rentang 0 unit menjadi `Held` beralasan | `G-22` | `BE-RWI-103`, `BE-RWI-123` [BE-KEP] |
| 22.20 nomor 8 | Satuan GDS disimpan eksplisit; satuan berbeda **ditolak**, tanpa konversi | `G-25` | `BE-RWI-102`, `BE-RWI-119` [BE-KEP] |

Bila salah satu ternyata tidak dimaksudkan pemilik, baris itu diralat lewat `grill-me` **sebelum**
task yang menanggungnya dikerjakan.

---

## 13. Catatan revision

| Revision | Tanggal | Isi |
| ---: | --- | --- |
| `1` | 2026-09-16 | Dibuat `plan-module-delivery` fase `RLN-PH-07` sesudah `RWI-DEC-150`. Empat puluh tiga FR dipetakan ke 18 task backend dan 14 task frontend, ditambah enam task milik sub-modul lain. Ditulis sebagai berkas terpisah atas permintaan pemilik |
