# Laporan Perubahan Frontend — `FE-HRD-R36`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R36` |
| Judul | Bangun `S-D1` MVP-9: Permintaan Headcount dan Antrean Rekrutmen |
| Slice | `S-D1` (Perencanaan Tenaga Kerja), Gelombang 3 |
| Roadmap | `roadmap/frontend-unfinished-work.md` — kartu `FE-HRD-R36`; task asal `FE-HRD-036`–`040` (sebelumnya belum dikerjakan) |
| Trace | Kartu asal `frontend-roadmap.md`; kontrak Workforce Planning |
| Contract version | Source controller backend branch `MHamzah` (dibaca saja) |
| Wewenang UI | Layar baru di atas base component |
| Dependency | `FE-HRD-R14` — 🟡 |
| Klasifikasi | `HEAVY` — 6 komponen/layar baru, 3 rute baru |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini dan tautan roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `59099bcb8` → `af4da693e`; layar R36 sudah di-commit pengguna pada `b98648b51`/`4672040fa` (branch `HamzahV2`) |
| Commit backend yang dijadikan rujukan | `7445a8b8` → `974f7208` (branch `MHamzah`) |
| Tanggal | 16 November 2026 |
| Status | 🟡 Sebagian — daftar, buat/ubah/ajukan/tarik, kursi, rencana pemenuhan, tautan pemenuhan, antrean rekrutmen, dan potret gap nyata; penanda "lewat batas" dan riwayat rencana tidak ada di backend |

---

## 1. Keadaan yang ditemukan di awal

Task `FE-HRD-036`–`040` belum punya layar maupun rute. Layar gap kebutuhan staf sudah ada tetapi tidak dapat mengajukan headcount.

## 2. Proses bisnis dari sisi pengguna

1. Kepala unit membuka **Gap Kebutuhan Staf**, menekan **Ajukan** pada baris yang kurang; formulir headcount terbuka dengan rumah sakit, unit, dan jabatan terisi dari baris itu.
2. Pada **rincian permintaan**, pengguna mengisi jenis (formasi, penggantian, melebihi formasi, sementara), tanggal butuh, dan **kursi** (FTE, sumber pemenuhan, pegawai yang digantikan bila penggantian). Sisa kursi formasi ditampilkan dengan rumus server.
3. Pengguna menyimpan draf, mengajukan, atau menarik (Draf/Diajukan/Dikembalikan). Keuangan dapat menyesuaikan biaya.
4. Setelah siap direkrut, HR mengatur **rencana pemenuhan** dan menautkan kursi ke penempatan jabatan atau SK mutasi.
5. **Antrean Rekrutmen** menampilkan kursi yang siap direkrut.
6. **Potret gap bulanan** menampilkan riwayat kekurangan staf.
7. Jalur tidak normal: tanpa hak, tombol tidak tampil; galat server tampil; formulir menolak FTE/tanggal tidak valid.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HeadcountRequestController`, `RecruitmentQueueController`, `StaffingGapController` (potret), DTO `HeadcountRequestDtos.cs`, `WfpPositionAssignmentController`, `HrdPersonnelActionOrderController`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `components/view/hr/workforce-planning/headcount-requests-view.jsx` | BARU — daftar (036) |
| `headcount-request-detail-view.jsx` | BARU — buat/ubah/ajukan/tarik/biaya/rencana (037, 038) |
| `headcount-seat-editor.jsx` | BARU — editor kursi (037) |
| `headcount-fulfilment-link-modal.jsx` | BARU — tautan pemenuhan (038/039) |
| `recruitment-queue-view.jsx` | BARU — antrean (039) |
| `staffing-gap-snapshots-panel.jsx` | BARU — potret (040) |
| `staffing-gaps-view.jsx` | Tombol Ajukan per baris dan panel potret |
| `src/app/hr/workforce-planning/headcount-requests/page.jsx`, `[id]/page.jsx`, `recruitment-queue/page.jsx` | Rute baru (`key={id}`, prefill dari query) |
| `workforce-planning-constants.js`, `workforce-planning-service.js` (+13 fungsi), `workforce-planning-display-utils.js` | Konstanta, service, util sisa kursi |
| `menu-items.jsx` | Menu "Permintaan Headcount" dan "Antrean Rekrutmen" dengan permission |

### 3.3 Kepatuhan arsitektur frontend

`page → view → service`; enum dikirim sebagai angka (backend tanpa `JsonStringEnumConverter`), filter query memakai nama enum.

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header, ringkasan, filter, tabel | `Hero`, `SummaryGrid`, `DataFilter`, `DataTable` | `REUSE` | — |
| Editor kursi | `BaseFormControl` + `DataTable` + `BaseButton` | `COMPOSE` | Baris kursi ditambah/hapus |
| Dialog | `ConfirmModal` + `HrResourceField` | `COMPOSE` | Penempatan/SK dipilih dari daftar |
| Status | `StatusBadge` / `HrStatusBadge` | `REUSE` | — |

`UI GATE: PASS — 5 elemen REUSE, 2 COMPOSE, 0 NEW`

```text
$ node .../check-css-usage.mjs <7 berkas JSX R36>
OK — 7 berkas diperiksa, 0 temuan.
$ grep Tailwind <berkas R36>
(kosong)
```

`VISUAL CHECK: NOT FEASIBLE` — backend dan frontend tidak berjalan; menyalakan backend pada basis data dev bersama di luar wewenang mode `FRONTEND`.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel/formulir |
| Kosong | "Belum ada permintaan headcount." / "Antrean rekrutmen kosong." |
| Gagal | Pesan server di `InformationAlert` |
| Tanpa hak akses | Peringatan hak; tombol tidak tampil |

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Workforce Planning / Headcount Request

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-planning/headcount-requests` | Daftar | `WorkforceHeadcountRequest : Read` |
| `GET` | `.../headcount-requests/remaining-seats` | Sisa kursi formasi | `WorkforceHeadcountRequest : Read` |
| `GET` | `.../headcount-requests/{id}` | Rincian | `WorkforceHeadcountRequest : Read` |
| `POST` | `.../headcount-requests` | Buat | `WorkforceHeadcountRequest : Create` |
| `PUT` | `.../headcount-requests/{id}` | Ubah draf | `WorkforceHeadcountRequest : Update` |
| `POST` | `.../{id}/submit`, `.../{id}/withdraw` | Ajukan / tarik | `WorkforceHeadcountRequest : Update` |
| `POST` | `.../{id}/finance-adjustment` | Penyesuaian biaya | `WorkforceHeadcountRequest : Update` |
| `POST` | `.../{id}/fulfilment-plan` | Rencana pemenuhan | `WorkforceHeadcountRequest : Update` |
| `POST`/`DELETE` | `.../{id}/seats/{seatId}/fulfilment-link` | Tautkan / lepas pemenuhan | `WorkforceHeadcountRequest : Update` / `Delete` |
| `POST` | `.../{id}/cancellations` | Pembatalan | `WorkforceHeadcountRequest : Create` |

#### Corporate / Human Resource / Workforce Planning / Recruitment Queue

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-planning/recruitment-queue` | Antrean | `WorkforceRecruitmentQueue : Read` |

#### Corporate / Human Resource / Workforce Planning / Staffing Gap

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-planning/staffing-gaps/snapshots` | Daftar potret | `WorkforceStaffingGap : Read` |
| `GET` | `.../staffing-gaps/snapshots/{id}` | Rincian potret | `WorkforceStaffingGap : Read` |

Pendukung: `GET /v1/corporate/human-resource/workforce-core/position-assignments`, `GET /v1/corporate/human-resource/personnel-action-orders`.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` 14 berkas R36 | 0 error, 0 warning | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` 7 JSX | 0 temuan | `PASS` | 3.4 |
| `npm run build` BUILD-2 | 832/832 halaman (termasuk 3 rute baru), `BUILD_EXIT=0` | `PASS` | Log build |
| Uji browser | Tidak dijalankan | `NOT RUN` | 3.4 |

Uji manual: `NOT FEASIBLE`.

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `FE-HRD-036` daftar + filter | Terpenuhi sebagian | Penanda/filter "lewat batas" tidak ada di DTO maupun query |
| `FE-HRD-037` buat/ubah dengan kursi | Terpenuhi sebagian | Kolom "kursi terisi" tidak ada di DTO daftar |
| `FE-HRD-038` rencana pemenuhan | Terpenuhi sebagian | Riwayat perubahan rencana & pembatalan tidak ada di DTO (AC 038.6) |
| `FE-HRD-039` antrean & tautan pemenuhan | Terpenuhi | Bagian 5 |
| `FE-HRD-040` potret gap | Terpenuhi sebagian | Potret saat pengajuan tidak dipasangkan otomatis per kursi |
| Screenshot | Belum terpenuhi | `VISUAL CHECK: NOT FEASIBLE` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nama hak di kartu (LinkFulfilment, ChangeFulfilmentPlan, RequestCancellation) tidak ada di backend; frontend mengikuti backend (Update/Delete/Create) |
| Masalah yang diketahui | "Melebihi formasi" ditampilkan dari jenis `AdditionalBeyondFormation` |
| Dependency backend | Flag lewat batas, jumlah kursi terisi, riwayat rencana pemenuhan |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna meng-commit sebagian layar R36 di tengah sesi |
| Status Git | Layar R36 sudah di-commit pengguna; sisa Gelombang 3 masih 81 entri belum di-commit |
| Langkah berikutnya | Backend menambah field di atas; uji browser |
