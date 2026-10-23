# Laporan Perubahan Frontend — `FE-LAB-56`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-56` |
| Judul | Halaman detail peristiwa pembatalan |
| Slice | `S16b-1` Laporan Pembatalan Order & Pemeriksaan — gelombang `MVP-15d` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) — *Gelombang `MVP-15`*, bagian `FE-LAB-56` |
| Trace | `FR-20.4`, `FR-20.12`; `LAB-DEC-232` butir 3; `02-backend-architecture.md` 32.10 no. 4; `03-frontend-architecture.md` amandemen 2026-10-20 |
| Contract version | `LAB-API-v1` `r49` 44.2 `GET /{historyId}` — **`approved`** 2026-10-20 |
| Wewenang UI | Halaman detail terpisah dengan token rute (konvensi Laporan Penerimaan), bunyi `404` — **bukan** `DEV_DISCRETION`. Susunan bagian, label aksi riwayat, gaya — `DEV_DISCRETION` |
| Dependency | `FE-LAB-55` ⚠ (tautan dan token rute); `BE-LAB-99` ✅ |
| Klasifikasi | `LIGHT` — satu halaman baca-saja, satu endpoint, nol menu baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (source); laporan ini beserta tautannya pada roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `7fb1e1d3e` (branch `YogaV2`) — di atas pekerjaan `FE-LAB-54`/`55` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `4d3e4f98` (branch `yoga`) |
| Tanggal | 2026-10-21 |
| Status | ✅ **`SELESAI`** — 2026-10-21. Semula ⚠; diulang dengan **akun asli dr. Bima** — peramban 32/32 — dan `AC-329` dibuktikan pada pesanan uji lima pemeriksaan (bagian 9) |

---

## 1. Keadaan yang ditemukan di awal

| Hal | Keadaan | Bukti |
| --- | --- | --- |
| Tautan dari daftar | Sudah dipasang `FE-LAB-55` — token rute privat (scope `lab-cancellation-report`, type `cancellation-event`, id = `historyId`); halaman tujuannya belum ada | `use-lab-cancellation-report.jsx` |
| Backend `GET /{historyId}` | `200` berisi `row`, `examinations`, `specimens`, `history`; `404` *"Peristiwa pembatalan tidak ditemukan."* | Respons nyata 2026-10-21 |
| Data nyata | Peristiwa **Pesanan** `LAB-RSMMC-000018`: 1 pemeriksaan belum masuk wadah, 0 wadah, 2 riwayat. Peristiwa **Pemeriksaan** *Trombosit* `LAB-RSMMC-000001`: 5 pemeriksaan, 3 wadah, 33 riwayat dengan aksi `Order.*`, `Specimen.*`, `Examination.*`, `LabExamination.*` | Respons nyata |
| Label aksi riwayat transisi | **Tidak ada** di frontend — nol peta nama aksi | Pencarian `src/` |
| Label status | Ada: pesanan, pemeriksaan, wadah, keadaan hasil | `lab-order-constants.jsx`, `lab-specimen-constants.jsx`, `lab-result-status-constants.jsx` |

---

## 2. Proses bisnis dari sisi pengguna

1. Di Laporan Pembatalan, kepala instalasi menekan **No. order** (baris Pesanan) atau **nama pemeriksaan** (baris
   Pemeriksaan).
2. Halaman *Detail Pembatalan* terbuka. Alamatnya membawa token, bukan nomor peristiwa mentah.
3. Empat bagian dibaca berurutan:
   - **Peristiwa Pembatalan** — waktu batal (WIB), jenis, pemeriksaan (atau *n pemeriksaan*), status sebelum, sudah
     layak tagih (baris Pemeriksaan saja), dibatalkan oleh, alasan (*Alasan tidak dicatat* bila kosong).
   - **Pesanan** — No. order, No. RM, nama pasien, disiplin, unit layanan.
   - **Pemeriksaan dalam Pesanan** dan **Wadah** — seluruhnya beserta statusnya. Pemeriksaan yang dibatalkan
     peristiwa ini ditandai *Dibatalkan pada peristiwa ini*; permintaan yang belum direncanakan wadahnya tampil
     *Belum Masuk Wadah*.
   - **Riwayat Transisi Pesanan** — urut waktu, dalam kalimat petugas. Contoh nyata:

     | Waktu (WIB) | Aksi | Dari | Menjadi | Oleh |
     | --- | --- | --- | --- | --- |
     | 07/10/2026 15.01 | Pesanan diminta | - | Diminta | SuperAdmin |
     | 07/10/2026 15.01 | Pesanan dibatalkan · *Peristiwa ini* | Diminta | Dibatalkan | SuperAdmin |

     Status dibaca menurut entitasnya: *Accepted* pada wadah = *Layak Periksa*, pada pesanan = *Diterima*; hasil
     *Finalized* = *Menunggu Validasi*.
4. **Kembali ke Laporan Pembatalan** membawa pengguna ke daftar.

**Jalur tidak normal:** peristiwa tidak ada atau bukan pembatalan → *"Peristiwa pembatalan tidak ditemukan."* dengan
keterangan dan tombol kembali; token asing/kedaluwarsa → pesan tautan tidak valid, **nol** permintaan ke server; tanpa
izin → *Akses Ditolak*, nol identitas pasien; galat lain → pesan dengan *Coba lagi*.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Roadmap `FE-LAB-56`; `api-contract.md` 44.2; `LabCancellationReportController.cs`, `LabCancellationReportDtos.cs`,
  `LabCancellationReportService.GetDetailAsync`; daftar konstanta aksi transisi backend.
- Frontend: `lab-reception-reports/[slug]/` dan `use-lab-reception-detail.jsx` (konvensi detail), `private-route-token-utils.js`,
  peta label status Lab, `base-detail-section.jsx`, `data-table.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/laboratory-management/lab-cancellation-reports/[slug]/page.jsx` | **Baru** — route detail, judul tab *Detail Pembatalan* |
| `src/app/health-services/laboratory-management/lab-cancellation-reports/[slug]/route-token.js` | **Baru** — penjaga segmen token (pola Laporan Penerimaan) |
| `src/components/view/health-services/laboratory-management/lab-cancellation-reports/lab-cancellation-detail-view.jsx` | **Baru** — empat bagian, keadaan memuat/404/tautan tidak valid/gagal |
| `src/lib/hooks/health-services/laboratory-management/use-lab-cancellation-detail.jsx` | **Baru** — token → `historyId` lewat `useSyncExternalStore`, izin, muat detail, ganti alamat UUID ke token |
| `src/lib/hooks/health-services/laboratory-management/lab-cancellation-report-rules.js` | Ditambah aturan detail: label aksi riwayat, status per entitas, *Belum masuk wadah*, penanda peristiwa ini |
| `src/lib/services/health-services/laboratory-management/lab-cancellation-report.service.js` | `getLabCancellationReportDetail` |
| `src/lib/constants/health-services/laboratory-management/lab-cancellation-report-constants.jsx` | `LAB_CANCELLATION_DETAIL_COPY`, `LAB_CANCELLATION_DETAIL_HEADERS` |
| `src/style/health-services/laboratory-management/lab-cancellation-reports/lab-cancellation-report.module.css` | Kisi informasi dan penanda peristiwa |
| `tests/unit/lab-cancellation-report-rules.test.mjs` | +5 uji detail (total 19 di berkas ini) |

### 3.3 Kepatuhan arsitektur frontend

- Alur `src/app` (penjaga token) → `components/view` → `lib/hooks` → `lib/services` → `InstanceAxios`.
- Dipakai ulang: `BaseDetailSection`, `DataTable`, `StatusBadge`, `LaboratoryStatePanel`, `AccessDeniedGate`, `Hero`;
  `ReasonText` dan format WIB `FE-LAB-55`; `resolvePrivateRouteToken`/`registerPrivateRouteToken`; peta status Lab.
- **Dua keputusan teknis:** (1) token dibaca dari `sessionStorage` lewat `useSyncExternalStore` — tanpa `setState` di
  dalam efek, dan aman terhadap render server; (2) alamat UUID mentah diganti token dengan `history.replaceState`, bukan
  `router.replace` — navigasi rute memasang ulang segmen `[slug]` dan sempat memuat detail **dua kali** (teramati di
  peramban, diperbaiki, lalu dibuktikan satu permintaan).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | *"Mengambil detail pembatalan..."* di bawah Hero |
| Tidak ditemukan (`404`) | *"Peristiwa pembatalan tidak ditemukan."* — *"Peristiwa ini tidak ada, atau bukan pembatalan pesanan/pemeriksaan. Buka ulang dari Laporan Pembatalan."* + tombol kembali |
| Tautan tidak valid | Judul sama — *"Tautan detail pembatalan tidak valid atau sudah kedaluwarsa…"*; nol permintaan |
| Kosong per bagian | *"Pesanan ini belum punya wadah."* / *"…tidak memuat pemeriksaan."* / *"Belum ada riwayat transisi…"* |
| Gagal | Pesan server atau *"Detail pembatalan gagal dimuat."* dengan *Coba lagi* |
| Tanpa hak akses | *Akses Ditolak* — *"Anda tidak memiliki izin membuka detail Laporan Pembatalan."* |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Cancellation Report

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-cancellation-reports/{historyId}` | Detail satu peristiwa | `LabCancellationReport : Read` |

Delta kontrak–source: nol. Respons riwayat tidak membawa id baris, sehingga baris *Peristiwa ini* dikenali dari aksi
batal yang sama pada waktu yang sama (`isCancellationEventEntry`).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji unit `lab-cancellation-report-rules`, `lab-order-statistics-report-rules`, `lab-menu-br137` | 45/45 | `PASS` | Keluaran perintah |
| Uji unit penuh | 3072/3101; 29 gagal — **daftar identik** dengan baseline (di luar Lab) | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar gagal |
| `npx eslint` berkas yang berubah | Nol galat, nol peringatan | `PASS` | Keluaran perintah |
| `npm run lint:errors` | 6 galat lama di 3 berkas finance/billing yang tidak disentuh | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npm run build` (server dimatikan dulu) | Berhasil; `ƒ /…/lab-cancellation-reports/[slug]` | `PASS` | Keluaran perintah |
| Peramban terhadap backend nyata (devYoga) | **32/32** — superadmin, akun hanya-izin-laporan (daftar izin dicegat), akun asli Gilang | `PASS` | Skrip Playwright; tangkapan layar di scratchpad sesi (memuat data pasien, tidak dilampirkan) |

**Rincian peramban:** dari daftar ke detail baris Pesanan dan baris Pemeriksaan; alamat bertoken; tepat satu
`GET /{historyId}`; seluruh ruas peristiwa dan pesanan sama dengan respons; ruas *Sudah Layak Tagih* hanya pada baris
Pemeriksaan; pemeriksaan pesanan lengkap (1 dan 5) dengan *Belum Masuk Wadah*, *Digugurkan*, *Layak Tagih*, *Dibatalkan*;
hanya *Trombosit* bertanda *Dibatalkan pada peristiwa ini*; wadah kosong bersebab dan 3 wadah berlabel (*Layak Periksa*,
*Perlu Ambil Ulang*); riwayat 2 dan 33 baris utuh tanpa paging, urut waktu, tepat satu *Peristiwa ini*, status per entitas,
nol nama aksi mentah; tombol kembali; muat ulang tab bertoken tetap terbuka; alamat UUID mentah → terbuka, **satu** GET,
alamat diganti token; **`404` nyata** (UUID v4 yang tidak ada) → kalimat dan keterangan, tombol kembali; token asing →
tautan tidak valid, nol permintaan detail; lebar 390 px tanpa geser halaman; storage peramban tanpa No. RM/nama;
**32.10 no. 4** — akun dengan `LabCancellationReport : Read` saja (tanpa izin daftar Lab) membuka detail; Gilang →
*Akses Ditolak*, nol identitas pasien di halaman.

**Temuan saat verifikasi:** uji `404` pertama memakai `00000000-…-0001`, yang tidak lolos pemeriksaan format UUID
sehingga yang tampil panel *tautan tidak valid* — bukan jawaban server. Diulang dengan UUID v4 sah: server `404`,
layar benar. Skrip juga sempat memeriksa terlalu dini (menunggu teks memuat *hilang* sebelum teks itu muncul); diganti
menunggu keadaan akhir.

Uji manual: `PASS` — dengan batas di bagian 8.

**Tidak dijalankan:** spec e2e API tiruan (digantikan putaran peramban backend nyata); `AC-329`/`T-S16B-05` dengan pesanan
**lima** pemeriksaan yang dibatalkan sekaligus — devYoga hanya punya pembatalan pesanan satu pemeriksaan (menunggu data uji
`MVP-15e` butir 3). Bahwa layar menampilkan **seluruh** pemeriksaan terbukti pada pesanan lima pemeriksaan milik peristiwa
Pemeriksaan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Klik nama pemeriksaan atau No. order membuka detail: informasi pemeriksaan dan order induk, pemeriksaan dan wadah beserta status, riwayat transisi | Terpenuhi | Peramban kedua jenis baris |
| Tanpa izin daftar Laboratorium (32.10 no. 4) | Terpenuhi | Peramban akun hanya-izin-laporan |
| `AC-329` — detail order memuat kelima pemeriksaan dan wadahnya | Terpenuhi | Detail pembatalan `LAB-RSMMC-000029` memuat kelima pemeriksaan (*Belum Masuk Wadah* — pesanan dibatalkan sebelum wadah direncanakan) — bagian 9 |
| `404` terbaca *"Peristiwa pembatalan tidak ditemukan."* | Terpenuhi | Peramban dengan `404` nyata |
| Detail tidak muncul bagi akun tanpa `LabCancellationReport : Read` | Terpenuhi | Peramban Gilang |
| Jebakan: `historyId` mentah di alamat | Dihindari | Token rute; alamat UUID diganti token |
| DoD: halaman detail berjalan; laporan `FE-LAB-56.md` | Terpenuhi | Bagian 6; berkas ini |
| Verifikasi dengan akun pemegang izin asli (dr. Bima) | Terpenuhi | 2026-10-21 — bagian 9 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Label aksi riwayat adalah peta di layar; aksi baru dari backend tampil apa adanya sampai petanya ditambah |
| Dependency backend | `NONE`. Data uji pesanan banyak pemeriksaan menunggu `MVP-15e` butir 3 |
| Perubahan sampingan | `NONE` — `.next/` hasil build tidak di-track |
| Interupsi | `NONE` |
| Status Git | Berkas `FE-LAB-56` (baru, di samping berkas `FE-LAB-54`/`55` yang sudah dilaporkan): `src/app/.../lab-cancellation-reports/[slug]/`, `lab-cancellation-detail-view.jsx`, `use-lab-cancellation-detail.jsx`; diubah: `lab-cancellation-report-rules.js`, `lab-cancellation-report.service.js`, `lab-cancellation-report-constants.jsx`, `lab-cancellation-report.module.css`, `tests/unit/lab-cancellation-report-rules.test.mjs` — seluruhnya belum di-track/di-commit |
| Langkah berikutnya | `NONE` — gelombang `MVP-15` frontend selesai. Izin jabatan manajemen (`MVP-15e` butir 2) ditunda atas keputusan pemilik modul 2026-10-21: belum ada jabatan manajemen pelayanan di master jabatan |

---

## 9. Verifikasi ulang dengan akun asli dr. Bima (2026-10-21)

| Hal | Isi |
| --- | --- |
| Akun | dr. Bima (Kepala Instalasi Laboratorium) — pemegang `LabCancellationReport : Read`; sandi tidak dicatat |
| Data uji | Data uji `MVP-15e` langkah 3 dibuat 2026-10-21 lewat API (izin tulis pemilik modul): kunjungan Rawat Inap (unit Rawat Inap), IGD (unit Kamar Bersalin), IGD (unit IGD); pesanan `LAB-RSMMC-000025`..`000030`, termasuk satu prosedur katalog uji tanpa disiplin (kini nonaktif), pesanan lima pemeriksaan yang dibatalkan (`000029`), dan satu pembatalan yang alasannya dikosongkan lewat satu `UPDATE` SQL untuk meniru data lama (`000030`) |
| Skrip peramban (bagian 6), pemegang izin = dr. Bima | **32/32** `PASS` |
| `AC-329` / `T-S16B-05` | Dari baris `000029` (*Pesanan · 5 pemeriksaan*) → detail memuat **kelima** pemeriksaan: Hemoglobin, Leukosit, Trombosit, Ureum, Kreatinin — `PASS` |
| Akun asli Gilang | *Akses Ditolak*, nol identitas pasien di halaman — `PASS` |
