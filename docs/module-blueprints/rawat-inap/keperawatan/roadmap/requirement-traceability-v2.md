# Requirement Traceability V2 — Sub-modul Keperawatan Rawat Inap

> Berkas ini **baru** dan hanya melacak penyelarasan `PRD-RWI-V2-001` revision `7`.
> Traceability task lama tetap di [`requirement-traceability.md`](./requirement-traceability.md).

## Metadata

```yaml
blueprint_id: RWI-BP-001
blueprint_revision: 7
submodule: keperawatan
traceability_revision: 1
contract_version: 0.5.0
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
fr_range: FR-KEP-035..FR-KEP-082
```

Label `[BE-DOK]` menandai task milik `dokter-rawat-inap`.

---

## 1. `EPIC KEP-09` — Ruang kerja V2 dengan delapan menu

| FR | Disposisi | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-KEP-035` | `EXTEND` | — | `FE-RWI-081` | `AC-KEP-130` | ✅ Frontend 17 September 2026 — navigasi 8 menu urutan tetap & tab sekunder; 71/71 test pass. [FE-RWI-081](../task/report/frontend/FE-RWI-081.md) |
| `FR-KEP-036` | `MISSING / NEW` | — | `FE-RWI-081`, `FE-RWI-090` | `INT-KEP-17`; panel Network nol permintaan | ✅ Frontend 18 September 2026 — permukaan Transfer Pasien (CAP-017), Serah Terima Klinis (DEFERRED RWI-DEC-113), Pemakaian Alat & Pemesanan Bedah terpasang dengan nol permintaan jaringan; unit test 5/5 pass, build pass. [FE-RWI-081](../task/report/frontend/FE-RWI-081.md), [FE-RWI-090](../task/report/frontend/FE-RWI-090.md) |
| `FR-KEP-037` | `EXISTING / REUSE` | — | `FE-RWI-081` | `03` 3.1 | ✅ Frontend 17 September 2026 — kegagalan konteks mematikan seluruh aksi tulis pada 8 menu. [FE-RWI-081](../task/report/frontend/FE-RWI-081.md) |
| `FR-KEP-038` | `EXTEND` — perbaikan | `BE-RWI-106` | `FE-RWI-082` | `RLN3-CAP-17`, `18`, `21`, `22`, `29` | ✅ Backend 17 September 2026 [BE-RWI-106] / ✅ Frontend 17 September 2026 [FE-RWI-082](../task/report/frontend/FE-RWI-082.md) |

## 2. `EPIC KEP-10` — Konfigurasi klinis berversi

| FR | Disposisi | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-KEP-039` | `MISSING / NEW` | `BE-RWI-107` | `FE-RWI-091` | Data 11.4–11.5 | ✅ Backend 17 September 2026 — tabel instrumen, versi, jawaban; migration `K1` diterapkan ke `QuilvianNewDevHamzah`; `dotnet build` 0 error. [BE-RWI-107](../task/report/backend/BE-RWI-107.md) / ✅ Frontend 18 September 2026 — katalog instrumen, drawer versi, editor pita skor, spectrum visual; 10/10 unit test, build pass. [FE-RWI-091](../task/report/frontend/FE-RWI-091.md) |
| `FR-KEP-040` | `MISSING / NEW` | `BE-RWI-108` | `FE-RWI-091` | `VAL-KEP-20a` | ✅ Backend 17 September 2026 — pengesah bukan pengubah terakhir; `dotnet build` 0 error. [BE-RWI-108](../task/report/backend/BE-RWI-108.md) / ✅ Frontend 18 September 2026 — tombol Sahkan disembunyikan bagi pengubah terakhir (AC-4); `canApproveInstrumentVersion()` di utils. [FE-RWI-091](../task/report/frontend/FE-RWI-091.md) |
| `FR-KEP-041` | `MISSING / NEW` | `BE-RWI-107` | `FE-RWI-091` | `VAL-KEP-19` | ✅ Backend 17 September 2026 — validasi pita tanpa tumpuk dan lubang; `dotnet build` 0 error. [BE-RWI-107](../task/report/backend/BE-RWI-107.md) / ✅ Frontend 18 September 2026 — band spectrum visual + validasi gap/overlap real-time (AC-2); `validateBands()` di utils. [FE-RWI-091](../task/report/frontend/FE-RWI-091.md) |
| `FR-KEP-042` | `MISSING / NEW` | `BE-RWI-107` | `FE-RWI-091` | `INT-KEP-16` | ✅ Backend 17 September 2026 — `resolve` versi berlaku per usia pasien; `dotnet build` 0 error. [BE-RWI-107](../task/report/backend/BE-RWI-107.md) / ✅ Frontend 18 September 2026 — Draft warning banner + status meta menjelaskan "tidak boleh di produksi" (AC-6). [FE-RWI-091](../task/report/frontend/FE-RWI-091.md) |
| `FR-KEP-043` | `EXTEND` — mencabut `RWI-FACT-036` | `BE-RWI-109`, `BE-RWI-128` | `FE-RWI-083` | `AC-KEP-054`, `061` | ✅ Backend 17 September 2026 [BE-RWI-109], diselaraskan 24 September 2026 3 skala usia & intervensi [BE-RWI-128](../task/report/backend/BE-RWI-128-resiko-jatuh-seeder-alignment.md) / ✅ Frontend 18 September 2026 — formulir risiko jatuh digambar dari instrumen berversi, 7/7 test pass. [FE-RWI-083](../task/report/frontend/FE-RWI-083.md) |

## 3. `EPIC KEP-11` — Pengkajian Pasien dan progres

| FR | Disposisi | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-KEP-044` | `MISSING / NEW` | `BE-RWI-109`, `BE-RWI-128` | `FE-RWI-083` | Data 11.6 | ✅ Backend 17 September 2026 [BE-RWI-109], pita skor baku bebas overlap/lubang 24 September 2026 [BE-RWI-128](../task/report/backend/BE-RWI-128-resiko-jatuh-seeder-alignment.md) / ✅ Frontend 18 September 2026 — skor total & pita risiko murni dari server via score-preview, tanpa kalkulasi ulang di frontend. [FE-RWI-083](../task/report/frontend/FE-RWI-083.md) |
| `FR-KEP-045` | `EXTEND` | `BE-RWI-110` | `FE-RWI-083` | `03` 10.4.3 | ✅ Backend 17 September 2026 [BE-RWI-110] / ✅ Frontend 18 September 2026 — Kajian Umum 8 seksi terstruktur RWI-DEC-141. [FE-RWI-083](../task/report/frontend/FE-RWI-083.md) |
| `FR-KEP-046` | `EXTEND` | `BE-RWI-110` | `FE-RWI-083` | `VitalSignId` | ✅ Backend 17 September 2026 [BE-RWI-110] / ✅ Frontend 18 September 2026 — Kajian Umum menunjuk VitalSignId baca-saja tanpa salin angka. [FE-RWI-083](../task/report/frontend/FE-RWI-083.md) |
| `FR-KEP-047` | `EXTEND` | `BE-RWI-111`, `BE-RWI-130`, `BE-RWI-133` | `FE-RWI-083`, `FE-RWI-130`, `FE-RWI-133` | Enum `6`–`8`, Enum `3` | ✅ Backend 17 September 2026 [BE-RWI-111], seeder Asesmen Edukasi 24 September 2026 [BE-RWI-130](../task/report/backend/BE-RWI-130-asesmen-edukasi-seeder-alignment.md), seeder Perencanaan Pulang 8 seksi 24 September 2026 [BE-RWI-133](../task/report/backend/BE-RWI-133-perencanaan-pulang-seeder-alignment.md) / ✅ Frontend 18 September 2026 [FE-RWI-083], render & clinical safety alerts 24 September 2026 [FE-RWI-130](../task/report/frontend/FE-RWI-130-asesmen-edukasi-ui-dan-evaluasi.md), [FE-RWI-133](../task/report/frontend/FE-RWI-133-perencanaan-pulang-ui-dan-safety-alerts.md). Unit test 5/5 pass. |
| `FR-KEP-048` | `MISSING / NEW` | `BE-RWI-111`, `BE-RWI-129` | `FE-RWI-083`, `FE-RWI-129` | `VAL-KEP-22`, `VAL-KEP-36d` | ✅ Backend 17 September 2026 [BE-RWI-111], seeder 3 seksi PQRST/POSS/Reassessment 60 menit 24 September 2026 [BE-RWI-129](../task/report/backend/BE-RWI-129-monitoring-nyeri-seeder-alignment.md) / ✅ Frontend 18 September 2026 [FE-RWI-083], selektor visual Wong-Baker FACES & safety alerts real-time 24 September 2026 [FE-RWI-129](../task/report/frontend/FE-RWI-129-monitoring-nyeri-ui-dan-safety-alerts.md). Unit test 16/16 pass. |
| `FR-KEP-049` | `EXTEND` | `BE-RWI-110` | `FE-RWI-084` | `VAL-KEP-22c` | ✅ Backend 17 September 2026 [BE-RWI-110] / ✅ Frontend 18 September 2026 — tanda vital rawat inap menolak isian nyeri langsung (FR-KEP-049), nyeri dibaca dari latestPain. [FE-RWI-084](../task/report/frontend/FE-RWI-084.md) |
| `FR-KEP-050` | `MISSING / NEW` | `BE-RWI-112` | `FE-RWI-082` | API 7.1 | ✅ Backend 17 September 2026 [BE-RWI-112] / ✅ Frontend 17 September 2026 — progres 5 bagian ✓/!/○ persen kelipatan 20, non-skor terpisah. [FE-RWI-082](../task/report/frontend/FE-RWI-082.md) |
| `FR-KEP-051` | `MISSING / NEW` | `BE-RWI-112` | `FE-RWI-082` | `RWI-DEC-119` (3) | ✅ Backend 17 September 2026 [BE-RWI-112] / ✅ Frontend 17 September 2026 — alert temuan berisiko di kepala konteks tanpa menurunkan progres. [FE-RWI-082](../task/report/frontend/FE-RWI-082.md) |
| `FR-KEP-052` | `MISSING / NEW` | `BE-RWI-112` | `FE-RWI-082` | `AC-KEP-072` | ✅ Backend 17 September 2026 [BE-RWI-112] / ✅ Frontend 17 September 2026 — galat eksplisit saat gagal dimuat, pantang menampilkan ○ palsu. [FE-RWI-082](../task/report/frontend/FE-RWI-082.md) |

## 4. `EPIC KEP-12` — Evaluasi Awal MPP

| FR | Disposisi | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-KEP-053` | `MISSING / NEW` | `BE-RWI-113`, `BE-RWI-132` | `FE-RWI-085`, `FE-RWI-132` | Data 11.7 | ✅ Backend 17 September 2026 [BE-RWI-113], seeder 8 seksi definitif KARS PAP 2.1 24 September 2026 [BE-RWI-132](../task/report/backend/BE-RWI-132-evaluasi-awal-mpp-seeder-alignment.md) / ✅ Frontend 18 September 2026 [FE-RWI-085], verifikasi integrasi 8 seksi accordion & wewenang MPP 24 September 2026 [FE-RWI-132](../task/report/frontend/FE-RWI-132-evaluasi-awal-mpp-verifikasi.md). Unit test 6/6 pass. |
| `FR-KEP-054` | `MISSING / NEW` | `BE-RWI-113` | `FE-RWI-085` | `VAL-KEP-23` | ✅ Backend 17 September 2026 [BE-RWI-113] / ✅ Frontend 18 September 2026 — kontrol tulis eksklusif hak MPP di unit pasien, non-MPP baca-saja, galat 403 transparan. [FE-RWI-085](../task/report/frontend/FE-RWI-085.md) |
| `FR-KEP-055` | `MISSING / NEW` — addendum bergantung `INT-KEP-12` | `BE-RWI-113` | `FE-RWI-085` | State 5.3 | ✅ Backend 17 September 2026 [BE-RWI-113] / ✅ Frontend 18 September 2026 — satu dokumen hidup per episode, jalur addendum tidak ditampilkan sebelum jenis dokumen 14 tersedia (INT-KEP-12). [FE-RWI-085](../task/report/frontend/FE-RWI-085.md) |

## 5. `EPIC KEP-13` — Pengawasan Harian

| FR | Disposisi | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-KEP-056` | `EXTEND` | `BE-RWI-121` | `FE-RWI-084` | Data 11.2 | ✅ Backend 17 September 2026 [BE-RWI-121] / ✅ Frontend 18 September 2026 — tanda vital tampil deret dan grafik per episode, unit test 7/7 pass. [FE-RWI-084](../task/report/frontend/FE-RWI-084.md) |
| `FR-KEP-057` | `MISSING / NEW` | `BE-RWI-119` | `FE-RWI-084` | Data 11.8 | ✅ Backend 17 September 2026 [BE-RWI-119] / ✅ Frontend 18 September 2026 — entri cairan wajib arah, sumber, volume ml, waktu, pelaksana. [FE-RWI-084](../task/report/frontend/FE-RWI-084.md) |
| `FR-KEP-058` | `MISSING / NEW` | `BE-RWI-122` | `FE-RWI-084` | `VAL-KEP-24d`–`g` | ✅ Backend 17 September 2026 [BE-RWI-122] / ✅ Frontend 18 September 2026 — penautan dosis MAR pada intake obat (kategori 5). [FE-RWI-084](../task/report/frontend/FE-RWI-084.md) |
| `FR-KEP-059` | `MISSING / NEW` | `BE-RWI-120` | `FE-RWI-084` | API 7.5 | ✅ Backend 17 September 2026 [BE-RWI-120] / ✅ Frontend 18 September 2026 — total balance per shift & 24 jam murni dari server, tanpa rekalkulasi klien. [FE-RWI-084](../task/report/frontend/FE-RWI-084.md) |
| `FR-KEP-060` | `MISSING / NEW` | `BE-RWI-120` | `FE-RWI-092` | `AC-KEP-093` | ✅ Backend 17 September 2026 [BE-RWI-120] / ✅ Frontend 18 September 2026 — Master Data Jam Shift Keperawatan (FE-KEP-20) per unit atau bawaan RS, garis waktu 24 jam interaktif pencegah celah & overlap (VAL-KEP-26a), penegasan kewenangan (AC-KEP-093), unit memakai bawaan, unit test 9/9 pass, build pass. [FE-RWI-092](../task/report/frontend/FE-RWI-092.md) |
| `FR-KEP-061` | `MISSING / NEW` | `BE-RWI-119` | `FE-RWI-084` | Data 11.9 | ✅ Backend 17 September 2026 [BE-RWI-119] / ✅ Frontend 18 September 2026 — GDS bangsal terpadu, satuan wajib dipilih tanpa nilai bawaan/default (VAL-KEP-25a). [FE-RWI-084](../task/report/frontend/FE-RWI-084.md) |
| `FR-KEP-062` | `MISSING / NEW` | `BE-RWI-119` | `FE-RWI-084` | Data 11.10 | ✅ Backend 17 September 2026 [BE-RWI-119] / ✅ Frontend 18 September 2026 — observasi harian diet, mobilisasi, lingkar perut, agitasi terstruktur. [FE-RWI-084](../task/report/frontend/FE-RWI-084.md) |
| `FR-KEP-063` | `MISSING / NEW` — usulan `G-26`, `G-27` | `BE-RWI-122` | `FE-RWI-084` | `VAL-KEP-36c`, `f` | ✅ Backend 17 September 2026 [BE-RWI-122] / ✅ Frontend 18 September 2026 — banner pengingat dosis MAR tanpa intake dengan tombol pintas catat cairan. [FE-RWI-084](../task/report/frontend/FE-RWI-084.md) |

## 6. `EPIC KEP-14` — MAR

| FR | Disposisi | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-KEP-064` | `MISSING / NEW` | `BE-RWI-114` | `FE-RWI-087` | `INT-KEP-08`; uji idempoten | ✅ Backend 17 September 2026 [BE-RWI-114] / ✅ Frontend 18 September 2026 — bagan MAR satu hari memuat jadwal dosis, refresh otomatis, toleransi includeStopped. [FE-RWI-087](../task/report/frontend/FE-RWI-087.md) |
| `FR-KEP-065` | `MISSING / NEW` | `BE-RWI-115` | `FE-RWI-087` | `VAL-KEP-30` | ✅ Backend 17 September 2026 [BE-RWI-115] / ✅ Frontend 18 September 2026 — pencatatan dosis bersyarat isian dan alasan (Administered/Held/Refused/Missed), deviasi waktu > 60m wajib catatan deviasi, unit test pass. [FE-RWI-087](../task/report/frontend/FE-RWI-087.md) |
| `FR-KEP-066` | `MISSING / NEW` | `BE-RWI-116` | `FE-RWI-087`, `FE-RWI-088` | `VAL-KEP-31` | ✅ Backend 17 September 2026 [BE-RWI-116] / ✅ Frontend 18 September 2026 — dosis high-alert berstatus Pending cek ganda, tombol konfirmasi terkunci bagi pencatat awal (INV-KEP-05), antrean verifikasi per unit di FE-INP-09 (FE-KEP-22), unit test (6/6 passing). [FE-RWI-087](../task/report/frontend/FE-RWI-087.md), [FE-RWI-088](../task/report/frontend/FE-RWI-088.md) |
| `FR-KEP-067` | `MISSING / NEW` | `BE-RWI-115` | `FE-RWI-087` | API 7.11 | ✅ Backend 17 September 2026 [BE-RWI-115] / ✅ Frontend 18 September 2026 — PRN berindikasi dan berevaluasi, pemberian tanpa jadwal beralasan. [FE-RWI-087](../task/report/frontend/FE-RWI-087.md) |
| `FR-KEP-068` | `MISSING / NEW` | `BE-RWI-115` | `FE-RWI-087` | Data 11.13 | ✅ Backend 17 September 2026 [BE-RWI-115] / ✅ Frontend 18 September 2026 — koreksi dosis mewajibkan alasan min 5 karakter, nomor revisi naik, dosis tidak pernah dihapus. [FE-RWI-087](../task/report/frontend/FE-RWI-087.md) |
| `FR-KEP-069` | `MISSING / NEW` | `BE-RWI-118` | `FE-RWI-087` | `INT-KEP-09`, `15` | ✅ Backend 17 September 2026 — langkah 6 penutupan terpasang; mesin henti butir siap dipanggil `BE-RWI-100` [BE-DOK]; `dotnet build` 0 error. [BE-RWI-118](../task/report/backend/BE-RWI-118.md) |
| `FR-KEP-070` | `EXTEND` | `BE-RWI-117` | `FE-RWI-087` | `INT-KEP-13` | ✅ Backend 17 September 2026 — dugaan reaksi obat sebagai alergi `Suspected` tertaut dosis; migration `K6` diterapkan ke `QuilvianNewDevHamzah`; `dotnet build` 0 error. [BE-RWI-117](../task/report/backend/BE-RWI-117.md) |
| `FR-KEP-071` | `MISSING / NEW` | `BE-RWI-114` | `FE-RWI-093` | API 7.13 | ✅ Backend 17 September 2026 [BE-RWI-114] / ✅ Frontend 18 September 2026 — Master Data Farmasi Jadwal Pemberian Obat (FE-KEP-21) 3 tab (Jadwal per frekuensi, Pengaturan MAR, Frekuensi tanpa jadwal), validasi format jam HH:mm & duplikasi (VAL-KEP-34a), pita ketentuan non-retroaktif dosis terbentuk (AC-4), penolakan server transparan (AC-5), unit test 11/11 pass. [FE-RWI-093](../task/report/frontend/FE-RWI-093.md) |

## 7. `EPIC KEP-15` — Pelaksanaan sliding scale

| FR | Disposisi | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-KEP-072` | `MISSING / NEW` | `BE-RWI-123` | `FE-RWI-087` | `VAL-KEP-27` | ✅ Backend 17 September 2026 [BE-RWI-123] / ✅ Frontend 18 September 2026 — eksekusi ditolak aman bila tidak ada order aktif, peringatan VAL-KEP-27 tampil; RWI-OQ-097 dicatat. [FE-RWI-087](../task/report/frontend/FE-RWI-087.md) |
| `FR-KEP-073` | `MISSING / NEW` | `BE-RWI-123` | `FE-RWI-087` | `VAL-KEP-28`, `29a` | ✅ Backend 17 September 2026 [BE-RWI-123] / ✅ Frontend 18 September 2026 — GDS bangsal bersatuan mg/dL atau mmol/L tanpa konversi manual. [FE-RWI-087](../task/report/frontend/FE-RWI-087.md) |
| `FR-KEP-074` | `MISSING / NEW` | `BE-RWI-123` | `FE-RWI-087` | `INT-KEP-11`; uji idempoten | ✅ Backend 17 September 2026 [BE-RWI-123] / ✅ Frontend 18 September 2026 — satu transaksi idempoten dengan Idempotency-Key, dosis MAR tampil tepat 1 kali. [FE-RWI-087](../task/report/frontend/FE-RWI-087.md) |
| `FR-KEP-075` | `MISSING / NEW` | `BE-RWI-123` | `FE-RWI-087` | API 7.12 | ✅ Backend 17 September 2026 [BE-RWI-123] / ✅ Frontend 18 September 2026 — simulasi pratinjau kalkulasi rentang dan dosis sebelum simpan, penyesuaian dosis beralasan (VAL-KEP-29d). [FE-RWI-087](../task/report/frontend/FE-RWI-087.md) |
| `FR-KEP-076` | Usulan `G-22` — **`ADOPTED_AS_PROPOSED`** `RWI-DEC-150` | `BE-RWI-123` | `FE-RWI-087` | Integrasi 8.5 | ✅ Backend 17 September 2026 [BE-RWI-123] / ✅ Frontend 18 September 2026 — rentang 0 unit dicatat berstatus Held beralasan GDS di bawah rentang. [FE-RWI-087](../task/report/frontend/FE-RWI-087.md) |

`FR-KEP-076` sebelumnya berstatus `OPEN DECISION` pada tingkat requirement. `RWI-DEC-150` bagian b
mengadopsi usulan `G-22` — rentang 0 unit mencatat dosis `Held` beralasan. `EPIC KEP-15` tetap
`MISSING / NEW` karena kedua jawaban memakai tabel, endpoint, dan layar yang sama.

## 8. `EPIC KEP-16` — Asuhan, Tindakan, Penunjang, Transfer

| FR | Disposisi | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-KEP-077` | `EXTEND` — kolom dari `dokter-rawat-inap` | `BE-RWI-124` | `FE-RWI-086` | `RWI-AC-204`, `205` | ✅ Backend 17 September 2026 [BE-RWI-124] / ✅ Frontend 18 September 2026 — SOAP perawat (`noteKind: 2`) dan catatan naratif (`noteKind: 3`) tersimpan terpadu di CPPT bersama dokter, filter perawat berfungsi, tanpa hak verifikasi (`INV-DOK-11`), TTV terhubung episode, unit test 6/6 pass, build pass. [FE-RWI-086](../task/report/frontend/FE-RWI-086.md) |
| `FR-KEP-078` | `EXTEND` — kontrak `dokter-rawat-inap` | `BE-RWI-125` | `FE-RWI-087` | API 7.15 | ✅ Backend 17 September 2026 [BE-RWI-125] / ✅ Frontend 18 September 2026 — perawat mendata obat bawaan, keputusan dokter DPJP disajikan BACA-SAJA tanpa wewenang memutuskan bagi perawat. [FE-RWI-087](../task/report/frontend/FE-RWI-087.md) |
| `FR-KEP-079` | `EXTEND` — kontrak `dokter-rawat-inap` | `BE-RWI-125` | `FE-RWI-089` | `INT-DOK-19` | ✅ Backend 17 September 2026 [BE-RWI-125] / ✅ Frontend 18 September 2026 — Form pesanan tindakan mewajibkan dokter bertugas (AC-1), penginput read-only perawat login (AC-2), history menampilkan status verifikasi (AC-3), unit test 6/6 pass, build pass. [FE-RWI-089](../task/report/frontend/FE-RWI-089.md) |
| `FR-KEP-080` | `EXTEND` | `BE-RWI-125`; pesanan perawat lewat `BE-RWI-104` [BE-DOK] | `FE-RWI-089` | `03` 10.4.8 | ✅ Backend 17 September 2026 [BE-RWI-125] / ✅ Frontend 18 September 2026 — Penunjang 6 kartu, Lab/Rad pesanan & hasil final dengan modal detail (AC-4), 4 kartu unavailable nol permintaan jaringan (AC-5), kontrol pesan disabled menunggu BE-RWI-104 (AC-6), unit test 6/6 pass, build pass. [FE-RWI-089](../task/report/frontend/FE-RWI-089.md) |
| `FR-KEP-081` | `EXISTING / REUSE` | — (`CAP-017` yang sudah ada) | `FE-RWI-090` | `RWI-DEC-113` | ✅ Frontend 18 September 2026 — Form & History mutasi tempat tidur menggunakan CAP-017 (InpatientBedBoard, PlacementFailureList, placements/transfer), Serah Terima Klinis DEFERRED (RWI-DEC-113) tanpa form tiruan, nol network request pada modul lain, unit test 5/5 pass, build pass. [FE-RWI-090](../task/report/frontend/FE-RWI-090.md) |

## 9. `EPIC KEP-17` — Tagihan Pasien

| FR | Disposisi | Task BE | Task FE | Bukti acceptance | Status |
| --- | --- | --- | --- | --- | --- |
| `FR-KEP-082` | `MISSING / NEW` | `BE-RWI-126` | `FE-RWI-094` | API 7.14 | ✅ Backend 17 September 2026 ([BE-RWI-126](../task/report/backend/BE-RWI-126.md)); ✅ Frontend 18 September 2026 — ringkasan tagihan baca-saja tanpa harga per item, 4 kartu metrik agregat, penyaringan menu PatientBillingSummary:Read, nol kontrol tulis, unit test 7/7 passing, build pass ([FE-RWI-094](../task/report/frontend/FE-RWI-094.md)). |

---

## 10. Ringkasan cakupan

| Hal | Angka |
| --- | ---: |
| FR revision `7` sub-modul ini | **48** (`FR-KEP-035` s.d. `FR-KEP-082`) |
| FR yang punya task backend di roadmap ini | 41 |
| FR berdisposisi Frontend atau `EXISTING / REUSE` — memang nol backend baru | 4 (`FR-KEP-035`, `036`, `037`, `081`) |
| FR yang task backend-nya bergantung `dokter-rawat-inap` | 4 (`FR-KEP-077` s.d. `080`) |
| FR tanpa task sama sekali | **0** |
| FR tanpa bukti acceptance | **0** |
| FR yang tertahan gerbang | 2 (`FR-KEP-080` sebagian, `FR-KEP-082` penuh) |

**Gap keterkaitan requirement ke bukti verifikasi: nol.** Sesuai `rules/backend/TEST_POLICY.md`,
tidak adanya automated test backend **bukan** coverage gap; bukti yang dipetakan adalah verifikasi
kontrak API, verifikasi proses bisnis, verifikasi skema, dan `dotnet build`.

---

## 11. Migration dan task yang menanggungnya

| Migration | Isi | Task | Tanpa mematikan layanan |
| --- | --- | --- | :---: |
| `K0` | Lima perbaikan keselamatan tanpa bentuk data | `BE-RWI-106` | Ya |
| `K1` | Tabel instrumen, jawaban, enum; seeder draft | `BE-RWI-107` | Ya |
| `K2` | Risiko jatuh rawat inap ke instrumen berversi | `BE-RWI-109` | **Tidak sepenuhnya** — perilaku poliklinik |
| `K3` | Kolom `TrxPatientAssessment`, `TrxPatientVitalSign`, dokumen Evaluasi Awal | `BE-RWI-110`, `BE-RWI-113` | Ya |
| `K4` | Tabel MAR, revisi, jadwal, pengaturan; hosted service | `BE-RWI-114` | Ya |
| `K5` | Tabel cairan, gula darah, observasi, shift beserta revisi | `BE-RWI-119` | Ya |
| `K6` | Dua kolom `TrxPatientAllergy` | `BE-RWI-117` | Ya |
| `K7` | Tabel pelaksanaan sliding scale | `BE-RWI-123` | Ya — **setelah** `R6` `dokter-rawat-inap` |

---

## 12. Dependency lintas sub-modul

| Arah | Task | Terkait | Kenapa |
| --- | --- | --- | --- |
| Masuk | `BE-RWI-123` | `BE-RWI-103` [BE-DOK] | Pelaksanaan butuh order sliding scale |
| Masuk | `BE-RWI-124` | `BE-RWI-094` [BE-DOK] | SOAP perawat butuh kolom `NoteKind` |
| Masuk | `BE-RWI-125` | `BE-RWI-101`, `BE-RWI-097` [BE-DOK] | Obat bawaan dan pesanan tindakan memakai kontrak dokter |
| Keluar | `BE-RWI-114` | dibutuhkan `BE-RWI-100` [BE-DOK] | Penghentian butir membatalkan dosis MAR |
| Keluar | `BE-RWI-114` | dibutuhkan `BE-RWI-087` [BE-INP] | Penutupan episode membatalkan dosis MAR |
| Keluar | `BE-RWI-118` | bersinggungan `BE-RWI-100` [BE-DOK] dan `BE-RWI-087` [BE-INP] | Tiga task menyentuh mesin pembatalan dosis yang sama |

**Catatan koordinasi.** `BE-RWI-118`, `BE-RWI-100` [BE-DOK], dan `BE-RWI-087` [BE-INP] menyentuh
mesin pembatalan dosis yang sama dari tiga arah. Urutan pengerjaannya dikoordinasikan supaya tidak
saling menimpa; `BE-RWI-118` dikerjakan lebih dulu sebagai pemilik mesinnya.

---

## 13. Gerbang yang belum tertutup

| Gerbang | Jenis | Menahan | Pemilik |
| --- | --- | --- | --- |
| ~~`{GATE-BILLING}`~~ | **TERTUTUP 2026-09-16** `RWI-DEC-154` | ~~`BE-RWI-126`, `FE-RWI-094`~~ — keduanya bebas; `BE-RWI-126` ✅ | **Yasmina** ✅; `RWI-OQ-053` tertutup |
| `BE-BKC-040` kelayakan keuangan | `P0 — external dependency`, **tetap terbuka** | Gerbang kesiapan produksi; nol task di roadmap ini | Yasmina — `RWI-DEC-102` |
| ~~`{GATE-LABRAD}`~~ | **TERTUTUP 2026-09-16** `RWI-DEC-153` | ~~Bagian pesanan Lab/Rad pada `FR-KEP-080`~~ — bebas | **Yoga Aji** ✅ |
| ~~Pemberitahuan `rawat-jalan` atas `K2`~~ | **TERTUTUP 2026-09-16** `RWI-DEC-152` | ~~Rilis `BE-RWI-109`~~ ✅ — **regresi poliklinik tetap wajib** | **Sukma GP** ✅ |
| Jenis dokumen `14` — `INT-KEP-12` | Dependency yang diketahui | Jalur addendum `FR-KEP-055`; **tidak** menahan task-nya | Pemilik `MedicalRecordManagement` |
| Pengesah isi protokol sliding scale — **`RWI-OQ-097`** | Gerbang produksi, **sebagian tertutup** | Kewenangan memakai **sudah** ada `RWI-DEC-155`; **nama** pengesah belum | Manajemen rumah sakit |
| Handover shift dan transfusi | `DEFERRED` | Nol task pada revision `7` | Muhammad Hamzah — gate `1.6` bagian 15 |

---

## 14. Usulan non-blocking yang diadopsi saat approval

`RWI-DEC-150` bagian b pada `../blueprint-manifest.md` 0-B.7 mencatat empat usulan sub-modul ini
sebagai `ADOPTED_AS_PROPOSED`:

| Sumber | Usulan | Gate | Task yang menanggungnya |
| --- | --- | --- | --- |
| 22.20 nomor 5 | Evaluasi Awal berupa **satu dokumen hidup per episode** + addendum | `G-09` | `BE-RWI-113`, `FE-RWI-085` |
| 22.20 nomor 6 | Rentang 0 unit → dosis `Held` beralasan | `G-22` | `BE-RWI-123`, `FE-RWI-087` |
| 22.20 nomor 8 | Entri intake yang dosisnya dikoreksi **ditandai, tidak diubah**; pengingat tanpa kewajiban | `G-26`, `G-27` | `BE-RWI-122`, `FE-RWI-084` |
| 22.20 nomor 9 | Dosis `Due` terlewat saat penutupan **tetap `Due` hanya-baca** | — | `BE-RWI-118`, `FE-RWI-087` |

Butir 22.20 nomor 7 — satuan GDS disimpan eksplisit tanpa konversi (`G-25`) — sama dengan
`dokter-rawat-inap` 22.20 nomor 8, dan ditanggung `BE-RWI-119` beserta `FE-RWI-084`.

Bila salah satu ternyata tidak dimaksudkan pemilik, baris itu diralat lewat `grill-me` **sebelum**
task yang menanggungnya dikerjakan.

---

## 15. Catatan revision

| Revision | Tanggal | Isi |
| ---: | --- | --- |
| `1` | 2026-09-16 | Dibuat `plan-module-delivery` fase `RLN-PH-07` sesudah `RWI-DEC-150`. Empat puluh delapan FR dipetakan ke 21 task backend dan 14 task frontend. Ditulis sebagai berkas terpisah atas permintaan pemilik agar berkas lama tetap terbaca |
