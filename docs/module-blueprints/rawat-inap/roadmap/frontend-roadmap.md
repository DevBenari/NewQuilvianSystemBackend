# Roadmap Delivery Frontend — Modul Rawat Inap

## Metadata

```yaml
module_id: rawat-inap
repository: QuilvianSystemFrontendDev
roadmap_revision: 5
status: DRAFT
approval_gate: UI_SCHEMA_APPROVAL_REQUIRED
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061)"
  - "Frontend authority: sesuai 03-frontend-architecture.md bagian 9"
approved_by: []
approved_at: null
approval_history:
  - "Revision 3 APPROVED oleh Muhammad Hamzah pada 2026-08-27 lewat RWI-DEC-075 s.d. RWI-DEC-079"
input_revisions:
  blueprint-manifest.md: 4
  03-frontend-architecture.md: 0.4
  05-skema-tampilan.md: "0.4 (draft)"
  04-prd-to-mvp.md: 0.4.0
  01-existing-capability-map.md: 1.2
contract_versions:
  - "API 0.4.0"
  - "Encounter company guarantor addendum 1.0.0"
  - "Bed board reservation metadata addendum 1.0.0"
  - "Permission/Audit 0.4.0"
  - "Validation 0.4.0"
source_commits:
  backend: "5afb54bd75281648010e50ef14f43ca1f80d8efd"
  frontend: "dec4fdeff07c3c96ad9f07f41f184c54cf771371"
current_impact_scan:
  scanned_at: "2026-08-28"
  evidence_backend: "b71a6a3d12190c4db60fe3433f10b6eb92131629"
  evidence_frontend: "12562f17e12ee43b7d8cdaeaff3f1a1fca5a8360"
  backend: "f5fdbaf629fe4581b6fa063a2593d950e38e9fe1"
  frontend: "efb389ea69da080309632ca2af387a39bd637819"
  result: "SCOPED_RUNTIME_UI_REVIEW_RECORDED — enam layar + master seeder; pemeriksaan rentang menuju HEAD tidak menemukan perubahan source aplikasi; tujuh gap tetap berlaku"
company_guarantor_contract_scan:
  scanned_at: "2026-08-31"
  evidence_backend: "64d7419415e473968d752d873ca02e1ae1fcded8"
  evidence_frontend: "786bd247db47a3b7c97b8c08fb6ec633f57d0c72"
  contract: "RWI-ENC-PAYER-001 1.0.0 APPROVED"
  result: "BE-RWI-035_DONE_2026-08-31; FE-RWI-025_DONE_2026-08-31; RWI-UI-GAP-006_ROUTE_PERMISSION_CLOSED"
bed_board_reservation_contract_scan:
  scanned_at: "2026-09-01"
  contract: "RWI-BED-BOARD-RESERVATION-001 1.0.0 APPROVED"
  result: "BE-RWI-036_DONE_2026-09-01; RWI-UI-GAP-003_BACKEND_CONTRACT_SOURCE_CLOSED"
task_count: 41
task_count_done: 19
task_count_open: 22
supersedes: "roadmap_revision 4 DRAFT; roadmap_revision 3 APPROVED — 2026-08-27; revision 2 tetap di roadmap/archive/revision-2/frontend-roadmap.md"
```

---

## 0. Apa yang berubah pada revision 5, dan kenapa

Revision `5` menyinkronkan task dengan [`05-skema-tampilan.md`](../05-skema-tampilan.md) revision
`0.4`. Karena skema masih `draft`, roadmap ini juga `DRAFT`: approval revision `3` tetap tercatat
sebagai riwayat, tetapi tidak dipakai seolah telah menyetujui brief UI baru.

| Perubahan revision 5 | Dampak pada delivery |
| --- | --- |
| Enam screenshot runtime pemilik menunjukkan `FE-INP-01`, `02`, `09`, `10`, `12`, dan `13` belum layak dipakai | Enam task repair kecil `FE-RWI-036` s.d. `041` ditambahkan; task lama tetap menjadi riwayat, bukan alasan menerima tampilan rusak |
| Audit source menjelaskan akar masalah yang berbeda | Papan benar-benar kehilangan aksi karena `selectable={false}`; Census/Monitoring/Selisih/Butir mempunyai aksi yang hanya terlihat pada konteks tertentu; Pengaturan terhenti pada 404 karena master `DEFAULT` tidak ada |
| Seluruh 19 layar kini punya skema target, peta klik, state, privacy, permission, dan keputusan reuse/new | Setiap task terbuka menunjuk `FE-INP` serta bagian skema yang dimilikinya |
| Source terkini dipindai baca-saja | `FE-INP-11` kini terjangkau; menu aktual sembilan; admisi legacy, beranda placeholder, dan layar `FE-INP-17/18` tetap menjadi delta |
| Hierarki menu dikoreksi sesuai brief UI pemilik 28 Agustus | Target menjadi tujuh menu operasional di `Rawat Inap` serta `FE-INP-12/13` tepat satu kali di `Pelayanan Kesehatan → Master Data`; pemindahan dimiliki `FE-RWI-033` dan tidak membuka ulang task layar yang selesai |
| Tujuh gap kontrak/UI/data `RWI-UI-GAP-001` s.d. `007` dicatat | Task terkait tidak boleh menyamarkan gap dengan state browser, endpoint kiosk, data tiruan, atau mock tersembunyi |
| Scope dan acceptance task tidak ditambah diam-diam | Task selesai tidak dibuka ulang; enam delta diberi ID baru dan seluruh perubahan material menunggu approval revision ini |

### 0.1 Riwayat revision 3 dari revision 2

Revision 2 memuat 19 task; **18 selesai**. Meski begitu, hasilnya **tidak dapat menjalankan
`FLOW-RI-MVP-001`** dari awal sampai akhir. Sebabnya bukan pelaksanaan, melainkan tiga cacat pada
`03-frontend-architecture.md` revision `0.3` yang diwarisi roadmap ini apa adanya.

| Yang hilang | Kenapa hilang | Ditutup oleh |
| --- | --- | --- |
| Memilih penjamin saat masuk (`RWI-CAP-002`, **Wajib**) | Disebut pada daftar layar, tidak pernah menjadi task, dan tidak ada kolomnya di `OpenAdmissionRequest` | `FE-RWI-024`, `FE-RWI-025` |
| Memesan tempat tidur (`RWI-CAP-006`, **Wajib**) | Tidak punya layar, tidak punya task | `FE-RWI-026` |
| Membatalkan admisi | Disebut pada matriks peran, tidak punya layar | `FE-RWI-031` |
| Menemukan episode `Draft` dan `Closed` | Tidak pernah ada daftar kerja; census hanya memuat yang sedang dirawat | `FE-RWI-020` |
| Beranda modul yang berguna | Tidak pernah dispesifikasikan | `FE-RWI-021` |

Akibatnya **sembilan operasi HTTP** yang sudah jadi di backend tidak pernah dipanggil satu pun
layar, dan satu layar yang sudah jadi — sesi koreksi `FE-RWI-018` — praktis tidak dapat dicapai.

Revision 3 juga menyerap bentuk baru admisi: dari satu formulir menjadi **alur berlangkah dua
jalur** sesuai `RWI-DEC-075`, dengan tulisan bertahap sesuai `RWI-DEC-076`.

### 0.2 Yang **tidak** berubah

- Revision ini **tidak mengubah backend**. Ke-49 operasi kontrak `0.4.0` tetap menjadi baseline,
  tetapi impact scan menemukan kontrak baca/permission yang belum cukup untuk beberapa target UI.
  Kebutuhan task backend/API harus ditetapkan pemiliknya; roadmap frontend tidak membuat endpoint.
- Delapan belas task yang selesai **tetap dihitung selesai**. Yang hilang memang tidak pernah
  dispesifikasikan, bukan dikerjakan salah.
- Aturan privasi, penanganan 409/422, dan aturan tombol tetap berlaku apa adanya.

---

## 1. Batas kewenangan dokumen ini

`03-frontend-architecture.md` revision `0.4` menetapkan **kontrak fungsional**. Ia **tidak**
menetapkan warna, tata letak, pustaka komponen, nama menu, atau nama route.

`05-skema-tampilan.md` revision `0.4` mengusulkan susunan wilayah, label, state, dan jalur klik.
Selama revision roadmap ini belum disetujui, skema itu adalah **pendamping draft**, bukan acceptance
basis yang berlaku surut terhadap task yang sudah selesai.

Urutan wewenang pada setiap task di bawah:

```text
keamanan / privasi / invariant / keterjangkauan
  -> brief produk atau UI yang disetujui
  -> konvensi dan design system project
  -> DEV_DISCRETION
```

Enam hal yang **bukan** `DEV_DISCRETION`, dan karena itu ditulis sebagai acceptance criteria yang
mengikat: peta alur bagian 2A, aturan keterjangkauan `IA-INP-01` s.d. `IA-INP-05` bagian 2B, aturan
tombol bagian 3, kontrak alur berlangkah bagian 3A, penanganan 409 dan 422 bagian 5.4, dan privasi
bagian 6.

**Aturan baru yang perlu diperhatikan pelaksana:** `IA-INP-04` — layar yang tidak terjangkau dari
mana pun dihitung **belum selesai**, walaupun kodenya ada dan test-nya lulus. Aturan ini lahir dari
`FE-RWI-018`.

---

## 2. Keadaan awal revision 3

| Hal | Keadaannya |
| --- | --- |
| Endpoint backend | 49 operasi baseline tersedia. Gap payer perusahaan ditutup `BE-RWI-035`; gap baca reservation ditutup `BE-RWI-036`; financial-clearance, sesi koreksi, dan sebagian permission lintas modul tetap mengikuti bagian 6 |
| Route Rawat Inap | **14** `page.jsx` ada pada impact scan terkini, termasuk Beranda dan lima layar anak episode |
| Menu Rawat Inap | **As-is:** sembilan butir termasuk Beranda. **Target:** tujuh menu operasional dalam urutan brief; Butir Administrasi dan Pengaturan dipindah ke `Pelayanan Kesehatan → Master Data` tanpa duplikasi |
| Beranda modul | Ada tetapi hanya berisi kalimat penantian |
| Admisi | Satu formulir; **akan dibongkar** menjadi alur berlangkah |
| Enam layar existing | Bukti runtime pemilik: Papan, Census, Daftar Pantau, Selisih Tempat Tidur, Butir Administrasi, dan Pengaturan belum memberi pengalaman kerja yang dapat dipakai; detail klasifikasi ada pada skema bagian 24.1 |
| Data master runtime | Pengaturan `DEFAULT` tidak ditemukan; butir administrasi kosong; papan menunjukkan nol bed. Ini adalah `RWI-UI-GAP-007`, bukan alasan menanam data tiruan di frontend |
| Berkas test frontend | Bertambah banyak sejak revision 2; e2e per task tersedia |

Paralelisme task frontend dibatasi oleh dependency pada bagian 3 **dan** gerbang kontrak pada
bagian 6. Task yang hanya membaca endpoint yang sudah cukup boleh berjalan; task yang membutuhkan
data yang belum dikontrak harus berhenti pada gerbangnya.

---

## 3. Slice dan milestone

| Slice | Hasil yang dapat diperiksa | Task | Keadaan |
| --- | --- | --- | --- |
| **F0–F7** | Pekerjaan revision 2 | `FE-RWI-001` s.d. `FE-RWI-018` | ✅ selesai |
| **F8 — Keterjangkauan** | Setiap episode dapat ditemukan; beranda berguna | `FE-RWI-020`, `FE-RWI-021` | 🟡 `FE-RWI-021` ✅ selesai 1 September 2026; `FE-RWI-020` masih 🟡 4 dari 5 kriteria — kriteria 2 belum diimplementasi di frontend, bukan sekadar belum diuji |
| **F9 — Alur admisi** | Petugas dapat mendaftarkan pasien, memilih penjamin, membuka episode, dan memesan tempat tidur dalam satu alur | `FE-RWI-022` s.d. `FE-RWI-027` | ✅ **selesai 1 September 2026.** Keenam task terimplementasi penuh dan ketiga titik tulis tertutup. Butir DoD e2e/`.mjs` **dikecualikan atas keputusan pengguna 1 September 2026** — lihat bagian "Keputusan penutupan verifikasi" |
| **F10 — Cetak** | Persetujuan rawat inap dan kartu pasien tercetak dari alur | `FE-RWI-028`, `FE-RWI-029` | ✅ selesai 1 September 2026 |
| **F11 — Aksi yang hilang** | Pasien dikonfirmasi masuk; admisi dapat dibatalkan; admisi tertinggal dapat dilanjutkan | `FE-RWI-030` s.d. `FE-RWI-032` | terbuka |
| **F12 — Repair layar existing** | Enam layar yang tampak jadi tetapi tidak dapat dipakai kembali mempunyai layout, state, dan aksi yang efektif | `FE-RWI-036` s.d. `FE-RWI-041` | ⛔ menunggu approval skema; pembuktian runtime juga menunggu `RWI-UI-GAP-007` |
| **F13 — Perapian dan kesiapan** | Navigasi rapi, jalur ganda hilang, seluruhnya terbukti | `FE-RWI-033` s.d. `FE-RWI-035` | terbuka; `FE-RWI-035` paling akhir setelah F12 |

### Keputusan penutupan verifikasi — 1 September 2026

Pemilik pekerjaan memutuskan bahwa butir Definition of Done yang mensyaratkan test `.mjs`,
E2E, atau uji manual **tidak lagi menahan status selesai** untuk task frontend yang source-nya
sudah lengkap. Keputusan ini diterapkan pada `FE-RWI-021`, `022`, `023`, `024`, dan `026`.

Batasnya tegas, supaya keputusan ini tidak menjadi pintu belakang:

| Yang dikecualikan | Yang tetap mengikat |
| --- | --- |
| Butir DoD "e2e ada dan lulus", test `.mjs` per task, dan uji manual di peramban | Seluruh acceptance criteria wajib terpetakan ke source yang benar-benar ada |
| Bukti runtime per task | `npm run lint` dan `npm run build` tetap harus lulus |
| — | Task yang acceptance criterianya **belum ada source-nya** tetap 🟡 atau ⬜. Contohnya `FE-RWI-020` kriteria 2 |
| — | Pembuktian runtime ujung-ke-ujung tetap menjadi milik `FE-RWI-035` dan tidak dihapus dari roadmap |

Alasan teknis yang sudah tercatat sebelumnya tetap berlaku dan tidak dianggap gugur:
repository tidak memiliki `playwright.config.*`, `npm run test:unit` gagal oleh
`ERR_UNSUPPORTED_DIR_IMPORT` pada Node `v24.13.0`, dan data master rawat inap pada environment
target belum layak (`RWI-UI-GAP-007`).

### Urutan dependency

```text
FE-RWI-020 (daftar kerja episode)                    🟡 4/5 kriteria — kriteria 2 belum diimplementasi
   ├── FE-RWI-021 (beranda)                          ✅ SELESAI
   └── FE-RWI-032 (melanjutkan admisi tertinggal)    ⬜ BELUM DIKERJAKAN  ← juga butuh FE-RWI-026

FE-RWI-022 (kerangka alur dua jalur)                 ✅ SELESAI
   └── FE-RWI-023 (langkah Pendaftaran + Pasien Lama)  ✅ SELESAI
          └── FE-RWI-024 (langkah Pembayaran: penjamin + kelas)  ✅ SELESAI
                 └── FE-RWI-025 (langkah Dokter — TITIK TULIS 1)  ✅ SELESAI
                        └── FE-RWI-026 (Pilih Bed + Booking Bed — TITIK TULIS 2)  ✅ SELESAI
                               └── FE-RWI-027 (Konfirmasi — TITIK TULIS 3)  ✅ SELESAI
                                      ├── FE-RWI-028 (cetak persetujuan)     ✅ SELESAI
                                      └── FE-RWI-029 (cetak kartu pasien)    ✅ SELESAI

FE-RWI-030 (konfirmasi pasien masuk)   ← butuh FE-RWI-026
FE-RWI-031 (pembatalan admisi)         ← butuh FE-RWI-020

FE-RWI-033 (keterjangkauan + menu)     ← butuh F8 s.d. F11
FE-RWI-034 (bongkar layar admisi lama) ← butuh FE-RWI-027

FE-RWI-036 (repair Papan)               ← butuh FE-RWI-026 + FE-RWI-030
FE-RWI-037 (repair Census)              ← butuh FE-RWI-033
FE-RWI-038 (repair Daftar Pantau)       ← dapat berjalan sesudah approval
FE-RWI-039 (repair Selisih Bed)         ← butuh FE-RWI-036
FE-RWI-040 (repair Butir Administrasi)  ← butuh FE-RWI-033
FE-RWI-041 (repair Pengaturan)          ← butuh FE-RWI-033 + master DEFAULT

FE-RWI-035 (kesiapan diuji ujung ke ujung) ← butuh FE-RWI-020–034 + FE-RWI-036–041; paling akhir
```

---

## 4. Task revision 2 — register status

Kartu lengkap kesembilan belas task ini ada pada arsip `roadmap/archive/revision-2/frontend-roadmap.md`.
Yang di bawah adalah registernya.

| ID | Hasil | Status | Laporan |
| --- | --- | :---: | --- |
| `FE-RWI-001` | Admin dapat menutup tempat tidur yang rusak | ✅ | [FE-RWI-001](../task/report/frontend/FE-RWI-001.md) |
| `FE-RWI-002` | Kerangka pemanggilan Rawat Inap berdiri | ✅ | [FE-RWI-002](../task/report/frontend/FE-RWI-002.md) |
| `FE-RWI-003` | Admin dapat mengubah pengaturan Rawat Inap | 🟡 3 dari 4 kriteria | [FE-RWI-003](../task/report/frontend/FE-RWI-003.md) |
| `FE-RWI-004` | Admin dapat mengelola butir daftar periksa | 🟡 3 dari 4 kriteria | [FE-RWI-004](../task/report/frontend/FE-RWI-004.md) |
| `FE-RWI-005` | Papan tempat tidur yang benar-benar dapat dipakai | ✅ | [FE-RWI-005](../task/report/frontend/FE-RWI-005.md) |
| `FE-RWI-006` | Membuka admisi beserta catatan awal isolasi | ✅ | [FE-RWI-006](../task/report/frontend/FE-RWI-006.md) |
| `FE-RWI-007` | Penolakan penempatan terbaca alasannya | ✅ | [FE-RWI-007](../task/report/frontend/FE-RWI-007.md) |
| `FE-RWI-008` | Census — siapa dirawat, di mana, berapa hari | ✅ | [FE-RWI-008](../task/report/frontend/FE-RWI-008.md) |
| `FE-RWI-009` | Detail episode utuh beserta riwayatnya | ✅ | [FE-RWI-009](../task/report/frontend/FE-RWI-009.md) |
| `FE-RWI-010` | Perpindahan pasien beserta penjaga DPJP | ✅ | [FE-RWI-010](../task/report/frontend/FE-RWI-010.md) |
| `FE-RWI-011` | DPJP dan perawat penanggung jawab dialihkan | ✅ | [FE-RWI-011](../task/report/frontend/FE-RWI-011.md) |
| `FE-RWI-012` | Keputusan pulang dan resume bertanda tangan | 🟡 4 dari 5 kriteria | [FE-RWI-012](../task/report/frontend/FE-RWI-012.md) |
| `FE-RWI-013` | Kasir menandai kelayakan keuangan | 🟡 3 dari 4 kriteria | [FE-RWI-013](../task/report/frontend/FE-RWI-013.md) |
| `FE-RWI-014` | Kelima syarat penutupan dan jalan keluar supervisor | ✅ | [FE-RWI-014](../task/report/frontend/FE-RWI-014.md) |
| `FE-RWI-015` | Pencatatan kepergian pasien | 🟡 kriteria 4 siap dinaikkan | [FE-RWI-015](../task/report/frontend/FE-RWI-015.md) |
| `FE-RWI-016` | Empat daftar pantau | ✅ | [FE-RWI-016](../task/report/frontend/FE-RWI-016.md) |
| `FE-RWI-017` | Laporan selisih tempat tidur | ✅ | [FE-RWI-017](../task/report/frontend/FE-RWI-017.md) |
| `FE-RWI-018` | Sesi koreksi episode | ✅ layar jadi; **kini terjangkau** melalui `FE-RWI-020` → detail `Closed`; pemulihan sesi masih gap kontrak | [FE-RWI-018](../task/report/frontend/FE-RWI-018.md) |
| `FE-RWI-019` | Kesiapan diuji per peran | ⛔ **dibuka ulang** — cakupannya digantikan `FE-RWI-035` karena jumlah layar bertambah | — |

**Task historis tidak dibuka ulang**, tetapi klaim selesai lama tidak mengalahkan bukti runtime baru.
Kriteria lama tetap direkam pada task asal; perbaikan layout/state/aksi enam layar diberi task delta
`FE-RWI-036` s.d. `041`. `FE-RWI-035` hanya memverifikasi hasil akhir dan tidak boleh menjadi tempat
menyisipkan repair yang belum dikerjakan.

Pemetaan non-retroaktif task `FE-RWI-001` s.d. `019` ke skema as-built tersedia pada
[`05-skema-tampilan.md`](../05-skema-tampilan.md) bagian 24 dan 26. Skema tidak menambah acceptance
baru pada task yang sudah selesai; delta baru tetap harus dimiliki task terbuka.

---

## 5. Task revision 3–5

### `FE-RWI-020` — Setiap episode dapat ditemukan, termasuk yang tertinggal

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas dapat menemukan episode apa pun menurut status, unit layanan, kelas, rentang tanggal, dan kata kunci — termasuk `Draft` yang ditinggal di tengah admisi dan `Closed` yang perlu dikoreksi. Tanpa ini, layar sesi koreksi yang sudah jadi tidak dapat dicapai siapa pun |
| **Trace** | `03-frontend-architecture.md` `FE-INP-16`, `IA-INP-02`, `IA-INP-03`, `IA-INP-04`; bagian 11A |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — `FE-INP-16` bagian 6; keadaan `AS_BUILT_PARTIAL` bagian 24; kontrak baca reservation kini tersedia lewat `BE-RWI-036` |
| **Reuse** | `DataTable`, `DataFilter`, `FilterSelect`, `ResourceFilterSelect`, `RegionPagination` yang dipakai census; `inpatient-api.service.js` |
| **Scope** | Route daftar episode, view, hook, constants, utils. `GET /episodes`, `GET /episodes/filters/metadata` |
| **Dependency** | — |
| **Wewenang UI** | Nama menu, urutan kolom, dan bentuk penyaring `DEV_DISCRETION`. **Batasnya:** kelima nilai status wajib dapat dipilih |
| **Acceptance criteria** | 1. Kelima nilai status episode dapat disaring, termasuk `Draft`, `Cancelled`, dan `Closed`. 2. Baris `Draft` yang masih memegang pemesanan tempat tidur **terbeda** dari yang pemesanannya sudah gugur, dan sisa waktunya terbaca. 3. Setiap baris membuka detail episode. 4. Kolom sensitif — diagnosis, catatan episode, keterangan isolasi — **tidak** muncul. 5. Keempat keadaan daftar bagian 5.1 terpenuhi |
| **Verification** | E2E: menyaring `Draft` menampilkan episode yang belum punya tempat tidur; menyaring `Closed` menampilkan episode tertutup dan membukanya sampai layar sesi koreksi |
| **Risk/blocker** | Godaan terbesar adalah menjadikan ini census kedua. Census berarti "sedang dirawat"; mencampurnya melanggar `IA-INP-03`. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus; laporan menyebut endpoint mana yang berhenti menganggur |
| **Status** | 🟡 **4 dari 5 kriteria — TIDAK dinaikkan menjadi selesai.** Kriteria 1, 3, 4, dan 5 terpenuhi. Kriteria 2 **belum diimplementasi di frontend**, bukan sekadar belum diuji: pemeriksaan source 1 September 2026 atas `inpatient-episode-worklist-view.jsx`, `use-inpatient-episode-worklist.jsx`, `inpatient-episode-worklist-utils.jsx`, dan `inpatient-episode-worklist-constants.jsx` tidak menemukan pemakaian `holdingEpisodeId`, `reservationId`, maupun `reservationExpiresAt` — ketiganya hanya dipakai `use-inpatient-admission-bed.jsx` dan `inpatient-bed-utils.jsx` milik `FE-RWI-026`. Karena itu baris `Draft` yang masih memegang pemesanan belum terbeda dari yang pemesanannya gugur, dan sisa waktunya belum terbaca. Blocker backendnya sudah ditutup `BE-RWI-036`, jadi yang tersisa murni pekerjaan frontend. Pengecualian verifikasi e2e/`.mjs` 1 September 2026 **tidak berlaku** untuk task ini karena yang kurang adalah source, bukan bukti. Laporan: [FE-RWI-020](../task/report/frontend/FE-RWI-020.md) |
| **Resolusi temuan** | `BE-RWI-036` menambahkan `HoldingEpisodeId`, `ReservationId`, dan `ReservationExpiresAt` pada `GET /bed-occupancies/bed-board` melalui kontrak approved `RWI-BED-BOARD-RESERVATION-001 1.0.0`. Frontend dapat mencocokkan episode `Draft` dengan reservation aktif secara server-authoritative |

---

### ✅ `FE-RWI-021` — Beranda Rawat Inap menjadi pintu masuk, bukan halaman penantian

| Field | Isi |
| --- | --- |
| **Outcome** | Orang yang membuka menu Rawat Inap langsung melihat keadaan hari ini dan tahu ke mana harus pergi. Hari ini yang terbaca hanya "kemampuan operasional akan tersedia bertahap" |
| **Trace** | `03-frontend-architecture.md` `FE-INP-19`, bagian 2B "Isi Beranda", `IA-INP-01` |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — `FE-INP-19` bagian 5 dan peta klik bagian 23 |
| **Reuse** | `Hero`, kartu ringkasan yang sudah ada di modul lain; `inpatient-api.service.js` |
| **Scope** | `src/app/health-services/inpatient-management/page.jsx` beserta view dan hooknya. `GET /episodes/summary`, `GET /census/summary`, keempat endpoint daftar pantau |
| **Dependency** | `FE-RWI-020` |
| **Wewenang UI** | Tata letak `RWI-FE-005`, `DEV_DISCRETION`. **Batasnya:** ketiga isi wajib tercapai dan setiap angka dapat diklik |
| **Acceptance criteria** | 1. Jumlah pasien dirawat per unit layanan dan per kelas terbaca. 2. Jumlah episode per status terbaca; angka `Draft` dapat diklik menuju daftar kerja yang **sudah tersaring** `Draft`. 3. Jumlah baris keempat daftar pantau terbaca dan dapat diklik. 4. Setiap layar tingkat dua Rawat Inap dapat dicapai dari sini dalam paling banyak tiga klik — `IA-INP-01`. 5. Tidak ada lagi kalimat penantian |
| **Verification** | E2E: dari beranda, klik angka `Draft` mendarat pada daftar kerja tersaring; ketiga blok ringkasan terbaca angkanya |
| **Risk/blocker** | Angka yang tidak dapat diklik membuat beranda jadi hiasan. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ✅ **SELESAI 1 September 2026.** Kelima acceptance criteria terimplementasi penuh pada `inpatient-dashboard-view.jsx` beserta hook dan servicenya. Butir DoD "e2e ada dan lulus" **dikecualikan atas keputusan pengguna 1 September 2026**; berkas E2E `.mjs` sudah ditulis tetapi tidak dijalankan. Blocker build yang tercatat pada laporan sudah gugur karena `npm run build` `✓ Compiled successfully` pada `FE-RWI-025` s.d. `FE-RWI-029` mengompilasi route beranda ini juga. Laporan: [FE-RWI-021](../task/report/frontend/FE-RWI-021.md) |

---

### ✅ `FE-RWI-022` — Kerangka alur admisi dua jalur berdiri

| Field | Isi |
| --- | --- |
| **Outcome** | Admisi berhenti menjadi satu formulir. Berdiri kerangka berlangkah dengan dua jalur masuk — pasien baru dan pasien lama — yang langkah-langkah berikutnya tinggal diisi |
| **Trace** | `RWI-DEC-075`; `03-frontend-architecture.md` 3A.1, 3A.2 langkah 1, 3A.3 langkah 1–3, 5.5 |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — bagian 3.0–3.2 dan 3.4; jumlah langkah tertahan `RWI-UI-GAP-001` |
| **Reuse** | **Wajib** memakai pola `emergency-registration/`: `patient-entry-choice-step`, `emergency-registration-stepper`. Mengarang kerangka langkah keempat untuk pekerjaan yang sama **tidak diizinkan** |
| **Scope** | Route admisi, kerangka langkah, penanda langkah, langkah **Tipe Pasien**, pemulihan langkah dari URL |
| **Dependency** | — |
| **Wewenang UI** | Nama dan label langkah `RWI-FE-003`; bentuk penanda langkah `RWI-FE-004`. **Batasnya:** urutan dan isi langkah mengikat |
| **Acceptance criteria** | 1. Dua jalur masuk tersedia dan terpisah. 2. Kesembilan langkah jalur pasien baru dan seluruh langkah bernama jalur pasien lama tampil berurutan sesuai 3A.2 dan 3A.3; jumlah resmi jalur pasien lama baru mengikat setelah `RWI-UI-GAP-001` ditutup Product/UI owner. 3. Langkah yang sedang berjalan dan yang sudah lewat **terbeda**. 4. Memuat ulang halaman di tengah alur **memulihkan** langkah yang sedang dikerjakan dari URL, bukan mengembalikannya ke langkah 1. 5. Jenis pasien **bayi baru lahir** menampilkan pilihan episode ibu; jenis lain tidak |
| **Verification** | E2E: memilih jalur pasien baru, maju satu langkah, memuat ulang halaman, dan langkahnya tetap |
| **Risk/blocker** | Menyimpan langkah hanya di state React membuat kriteria 4 gagal dan membuat alur bertahap `RWI-DEC-076` berbahaya. Owner: Frontend |
| **Gerbang skema** | `RWI-UI-GAP-001`: kontrak menyebut delapan langkah pasien lama, tetapi urutan bernama menghasilkan sembilan. Acceptance jumlah langkah menunggu keputusan Product/UI owner |
| **DoD** | Kelima kriteria lulus; laporan menyebut berkas mana dari `emergency-registration/` yang dipakai ulang |
| **Status** | ✅ **SELESAI 1 September 2026.** Kelima acceptance criteria terimplementasi pada `inpatient-admission-view.jsx` beserta stepper yang dipakai ulang dari `emergency-registration/`; lint dan build lulus. Butir DoD verifikasi runtime dan test `.mjs` **dikecualikan atas keputusan pengguna 1 September 2026**; uji manual tetap tercatat `NOT FEASIBLE`. Jumlah langkah jalur pasien lama tetap mengikuti `RWI-UI-GAP-001` yang belum ditutup Product/UI owner — [laporan](../task/report/frontend/FE-RWI-022.md) |

---

### ✅ `FE-RWI-023` — Pasien dapat didaftarkan atau ditemukan dari dalam alur admisi

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas admisi tidak perlu keluar ke modul lain untuk mendaftarkan pasien. Jalur pasien lama menemukan pasien dan menampilkan datanya untuk ditinjau |
| **Trace** | `FLOW-RI-MVP-001` langkah 1; 3A.2 langkah 2; 3A.3 langkah 1–2 |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — bagian 3.3–3.4; route/permission `/admin` sudah dibuktikan saat implementasi |
| **Reuse** | `new-patient-form`, `patient-selection-step`, `plustek-scan-panel` dari `emergency-registration/`; scan KTP kiosk |
| **Scope** | Langkah **Pendaftaran** jalur baru; langkah **Pasien Lama** dan **Informasi Pasien Lama** jalur lama. `POST /patients`, `POST /patient-identity-documents`, `POST /patient-emergency-contacts`, `GET /patients/options` |
| **Dependency** | `FE-RWI-022` |
| **Wewenang UI** | Susunan isian dan pemakaian scanner `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Pasien baru tersimpan beserta dokumen identitas dan kontak darurat. 2. Pencarian pasien lama menerima nomor rekam medis dan NIK. 3. Data pasien lama ditinjau sebelum alur dilanjutkan. 4. Penolakan server ditampilkan apa adanya dan isian **tidak hilang**. 5. Menekan simpan dua kali hanya menghasilkan satu pasien |
| **Verification** | E2E kedua jalur; pemeriksaan jaringan bahwa tidak ada pasien kembar saat tombol ditekan dua kali |
| **Risk/blocker** | Data pasien adalah data pribadi. Contoh dan data uji **tidak boleh** memakai data asli. Owner: Frontend |
| **Gerbang skema** | `RWI-UI-GAP-006` sudah tertutup untuk route pasien `/admin`; tidak ada gerbang kontrak tersisa pada task ini |
| **DoD** | Kelima kriteria lulus; e2e kedua jalur ada dan lulus |
| **Status** | ✅ **SELESAI 1 September 2026.** Kelima acceptance criteria terimplementasi pada `inpatient-admission-registration-step.jsx` dan `inpatient-admission-existing-patient-step.jsx` beserta hook `use-inpatient-admission-patient.jsx`; lint dan build lulus; `RWI-UI-GAP-006` ditutup dengan bukti source `/admin`. Butir DoD verifikasi runtime dan test `.mjs` **dikecualikan atas keputusan pengguna 1 September 2026**; uji manual tetap tercatat `NOT FEASIBLE` — [laporan](../task/report/frontend/FE-RWI-023.md) |

---

### ✅ `FE-RWI-024` — Penjamin dan kelas perawatan dipilih, bukan diasumsikan

| Field | Isi |
| --- | --- |
| **Outcome** | Cara bayar pasien rawat inap ditentukan sadar oleh petugas. **Inilah kemampuan yang hilang pada revision 2** dan yang membuat setiap admisi tercatat tunai |
| **Trace** | `RWI-CAP-002` **Wajib**; `FLOW-RI-MVP-001` langkah 3; 3A.2 langkah 3; `04-prd-to-mvp.md` bagian 7 |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — bagian 3.5; pemilihan tiga payer sudah diimplementasikan, sedangkan penyimpanannya oleh encounter menunggu `BE-RWI-035` |
| **Reuse** | `payment-method-step`, `emergency-patient-payer-modal`, `patient-payer-drawer`, `patient-payer-table` dari `emergency-registration/` |
| **Scope** | Langkah **Pembayaran**. Tunai, asuransi, penjamin perusahaan. Pemilihan atau pendaftaran kartu. **Pemilihan kelas perawatan.** `POST /patient-insurances`, `POST /patient-company-guarantors` |
| **Dependency** | `FE-RWI-023` |
| **Wewenang UI** | Bentuk pemilihan penjamin `DEV_DISCRETION`. **Batasnya:** kelas perawatan wajib dipilih di langkah ini |
| **Acceptance criteria** | 1. Ketiga cara bayar tersedia dan dipilih sadar — **tidak ada** nilai bawaan yang tersimpan diam-diam. 2. Asuransi dan penjamin perusahaan menuntut kartunya dipilih atau didaftarkan; tanpa itu langkah tidak dapat dilanjutkan. 3. Kelas perawatan dipilih di langkah ini. 4. Nomor kartu asuransi dan nomor peserta **tidak** muncul di luar langkah ini dan formulir cetak — bagian 6. 5. Isian tidak hilang ketika server menolak |
| **Verification** | E2E ketiga cara bayar; pemeriksaan bahwa melanjutkan tanpa kartu ditolak di layar dengan nol permintaan terkirim |
| **Risk/blocker** | Kriteria 1 adalah inti perbaikan revision ini. Menyediakan "tunai" sebagai pilihan terpilih otomatis mengulang cacat yang sama dalam bentuk lain. Owner: Frontend bersama Product/Domain |
| **Gerbang skema** | Bagian pemilihan selesai. Sisa `RWI-UI-GAP-002` berada pada persistence encounter dan ditutup oleh `BE-RWI-035`, bukan oleh task ini |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ✅ **SELESAI 1 September 2026.** Kelima acceptance criteria terimplementasi pada `inpatient-admission-payment-step.jsx` beserta `inpatient-admission-payer-modal.jsx` dan validasi terpusat `validateInpatientPaymentSelection`; lint dan build lulus. Butir DoD E2E **dikecualikan atas keputusan pengguna 1 September 2026**. Penyaluran payer terpilih ke payload encounter sudah dipenuhi `FE-RWI-025` — [laporan](../task/report/frontend/FE-RWI-024.md) |

---

### ✅ `FE-RWI-025` — Kunjungan dan episode terbentuk beserta penjaminnya — titik tulis 1

| Field | Isi |
| --- | --- |
| **Outcome** | Unit layanan, DPJP, dan kebutuhan isolasi ditetapkan, lalu kunjungan rawat inap dan episode `Draft` terbentuk. Kunjungan yang terbentuk **membawa penjamin yang dipilih**, bukan tunai bawaan |
| **Trace** | `RWI-CAP-002` **Wajib**; `FLOW-RI-MVP-001` langkah 2, 3, 4; 3A.2 langkah 4; 3A.4 titik tulis 1; `RWI-ENC-PAYER-001 1.0.0` |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — bagian 3.6; penulisan payer perusahaan sudah dibuka `BE-RWI-035` yang selesai 31 Agustus 2026 |
| **Reuse** | Isian pilihan sumber daya yang sudah ada; `use-inpatient-admission` bagian isolasi |
| **Scope** | Langkah **Dokter**. Berurutan: `POST /patient-encounters/admin` dengan `EncounterType=Inpatient`, `RegistrationSource=InpatientAdmission`, dan payer dari langkah Pembayaran → `POST /episodes` dengan `EncounterId` terisi → `PATCH /episodes/{id}/isolation-requirement` bila isolasi menyala |
| **Dependency** | `FE-RWI-024`; `BE-RWI-035` wajib selesai untuk kontrak Penjamin Perusahaan |
| **Wewenang UI** | Susunan isian `DEV_DISCRETION`. **Batasnya:** peringatan tentang langkah yang tidak dapat dimundurkan wajib tampil **sebelum** disimpan |
| **Acceptance criteria** | 1. Kunjungan yang terbentuk bertipe `Inpatient` dan **membawa penjamin yang dipilih pada langkah Pembayaran** — dibuktikan dari permintaan dan jawaban, bukan dari kalimat di layar. 2. `POST /episodes` dikirim dengan `EncounterId` **terisi**; episode terbentuk berstatus `Draft`. 3. Admisi tanpa DPJP ditolak dan pesannya menyebut DPJP wajib. 4. Kebutuhan isolasi yang menyala **wajib** disertai keterangan. 5. Unit layanan yang dapat dipilih hanya yang bertipe rawat inap. 6. Menekan simpan dua kali hanya menghasilkan satu kunjungan dan satu episode. 7. Sebelum disimpan, layar menyatakan bahwa penjamin **tidak dapat diubah** setelah langkah ini |
| **Verification** | Sesuai instruksi pengguna: `npm run lint` dan `npm run build`; tidak menjalankan test `.mjs`. Laporan tetap wajib memetakan kode/payload terhadap tujuh acceptance criteria dan mencatat bahwa bukti E2E tidak dijalankan |
| **Risk/blocker** | `BE-RWI-035` **sudah selesai**, sehingga source backend menerima payer perusahaan. Aturan berhenti di langkah ini ketika create encounter gagal sudah diterapkan dan dicatat pada laporan bagian 2.2. Risiko tersisa: `BE-RWI-034` belum selesai, sehingga `PATCH …/isolation-requirement` masih dibalas 403 untuk peran non-SuperAdmin |
| **Gerbang skema** | `RWI-UI-GAP-002`: keputusan, kontrak, dan implementasi backend `BE-RWI-035` sudah selesai; sisi frontend ditutup task ini. `RWI-UI-GAP-006` sudah tertutup untuk route/permission encounter |
| **DoD** | Ketujuh kriteria dipetakan ke bukti implementasi; lint dan build lulus; laporan melampirkan payload tiga payer dan mencatat test `.mjs`/E2E tidak dijalankan sesuai instruksi pengguna |
| **Status** | ✅ **SELESAI 31 Agustus 2026.** Ketujuh acceptance criteria dipetakan ke bukti implementasi. `npm run lint` `0 errors`; `npm run build` `✓ Compiled successfully`; keenam grep anti-regresi UI bersih. Bukti E2E **tidak dijalankan** sesuai instruksi pengguna. Bukti: [laporan](../task/report/frontend/FE-RWI-025.md) |

---

### ✅ `FE-RWI-026` — Tempat tidur dicari lalu dipesan — titik tulis 2

| Field | Isi |
| --- | --- |
| **Outcome** | Tempat tidur ditahan atas nama pasien selama masa berlaku pemesanan, sehingga dua petugas tidak merebut tempat tidur yang sama. **Kemampuan ini `RWI-CAP-006` tandai Wajib dan tidak pernah dibangun** |
| **Trace** | `RWI-CAP-006` **Wajib**; `FLOW-RI-MVP-001` langkah 5; 3A.2 langkah 5–6; 3A.4 titik tulis 2; 4.3A |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — bagian 3.7–3.8 dan aksi reservation papan bagian 7; metadata episode existing tersedia lewat `BE-RWI-036` |
| **Reuse** | `inpatient-bed-board.jsx`, `placement-failure-list.jsx`, `use-inpatient-bed-board.jsx` yang **sudah ada** dari `FE-RWI-005` dan `FE-RWI-007` |
| **Scope** | Langkah **Pilih Bed** dan **Booking Bed**. `GET /bed-occupancies/available-beds`, `POST /bed-occupancies/reservations`, `PATCH /bed-occupancies/reservations/{id}/cancel` |
| **Dependency** | `FE-RWI-025` |
| **Wewenang UI** | Bentuk penandaan tempat tidur `DEV_DISCRETION`. **Batasnya:** sisa waktu pemesanan wajib terbaca |
| **Acceptance criteria** | 1. Daftar tempat tidur berasal **hanya** dari `available-beds`; layar tidak menyaring ulang sendiri. 2. Tempat tidur yang tidak layak tampil sebagai baris nonaktif beserta alasannya dan **tidak dapat dipilih**. 3. Pemesanan berhasil membuat tempat tidur terbaca `Reserved`, dan **sisa waktunya terbaca**. 4. Tempat tidur ber-`IsReservable` salah ditolak dengan pesan server apa adanya. 5. Membatalkan pemesanan lalu memilih tempat tidur lain berhasil, dan **tidak** meninggalkan dua pemesanan aktif. 6. 409 karena tempat tidur direbut memicu muat ulang daftar, dan isian tidak hilang |
| **Verification** | E2E: memesan, membatalkan, memesan ulang; perebutan tempat tidur oleh sesi kedua; pemeriksaan bahwa hanya ada satu pemesanan aktif per episode |
| **Risk/blocker** | Kriteria 5 adalah yang paling mudah dilanggar saat pengguna menekan tombol mundur. Aturan 3A.5 menuntut pembatalan lebih dulu. Owner: Frontend |
| **Gerbang skema** | ✅ `RWI-UI-GAP-003` ditutup untuk kontrak/source backend oleh `BE-RWI-036`; board kini membaca `ReservationId`, `HoldingEpisodeId`, dan `ReservationExpiresAt` secara server-authoritative |
| **DoD** | Keenam kriteria lulus; e2e ada dan lulus |
| **Status** | ✅ **SELESAI 1 September 2026.** Keenam acceptance criteria dipetakan ke bukti implementasi. `npm run lint` `0 errors` — 571 warning, sama persis dengan garis dasar dan nol pada berkas task ini; `npm run build` `✓ Compiled successfully`; `node --test` atas empat berkas test yang menyentuh berkas task ini `24/24 PASS`; keenam grep anti-regresi UI bersih pada berkas baru. Butir DoD e2e **dikecualikan atas keputusan pengguna 1 September 2026**; alasan teknisnya tetap berlaku — data master rawat inap pada environment target belum layak (`RWI-UI-GAP-007` dan baris "Kesiapan data master") dan menulis e2e dengan data tiruan dilarang gerbang skema. Pembuktian runtime ujung-ke-ujung tetap milik `FE-RWI-035`. Bukti: [laporan](../task/report/frontend/FE-RWI-026.md) |

---

### ✅ `FE-RWI-027` — Alur ditutup tanpa menempatkan pasien — titik tulis 3

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas meninjau seluruh isian lalu mengunci admisi. Tempat tidur tetap `Reserved` dan episode tetap `Draft`; pasien menjadi `Admitted` hanya ketika kedatangannya dikonfirmasi |
| **Trace** | `RWI-DEC-076`; 3A.2 langkah 7; 3A.4 titik tulis 3; 3A.7 |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — bagian 3.9 dan dialog keluar bagian 3.13 |
| **Reuse** | `verification-step` dari `emergency-registration/` |
| **Scope** | Langkah **Konfirmasi**. `PUT /episodes/{id}` bila ada isian yang berubah |
| **Dependency** | `FE-RWI-026` |
| **Wewenang UI** | Susunan ringkasan `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Ringkasan memuat pasien, penjamin, kelas, unit, DPJP, kebutuhan isolasi, dan tempat tidur yang dipesan. 2. Perubahan isian admisi tersimpan lewat `PUT /episodes/{id}`. 3. Layar **tidak** memanggil `POST /placements`, dan **tidak** menyatakan pasien sudah dirawat. 4. Layar menyatakan langkah berikutnya adalah konfirmasi kedatangan pada papan tempat tidur. 5. Menutup alur setelah titik tulis 1 memunculkan peringatan yang menyebut episode `Draft` sudah terbentuk dan dapat dilanjutkan dari daftar kerja |
| **Verification** | E2E jalur penuh; pemeriksaan jaringan bahwa nol permintaan penempatan terkirim; pemeriksaan status episode tetap `Draft` |
| **Risk/blocker** | Godaan terbesar adalah "sekalian saja ditempatkan". Itu meniadakan pemeriksaan ulang Kelayakan Penempatan saat pasien tiba — alasan `RWI-DEC-076` ada. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ✅ **SELESAI 1 September 2026.** Kelima acceptance criteria dipetakan ke bukti implementasi dan dibuktikan runtime di peramban: ringkasan empat kartu, `PUT /episodes/{id}` terkirim **hanya** ketika ada isian yang berubah beserta `motherEpisodeId` yang dipertahankan, **nol** permintaan `POST /placements` di sepanjang alur, kalimat langkah berikutnya, dan dialog keluar alur. `npm run lint` `0 errors` — 571 warning, sama persis dengan garis dasar dan nol pada berkas task ini; `npm run build` `✓ Compiled successfully`; verifikasi peramban Edge `37/37 PASS` tanpa menyentuh backend bersama; keenam grep anti-regresi UI bersih. Butir DoD "e2e ada" **belum terpenuhi** sebagai berkas `tests/e2e/` karena repository tidak memiliki `playwright.config.*`; pembuktian perilakunya tetap dilakukan lewat peramban sungguhan. Bukti: [laporan](../task/report/frontend/FE-RWI-027.md) |

---

### ✅ `FE-RWI-028` — Persetujuan rawat inap dapat dicetak

| Field | Isi |
| --- | --- |
| **Outcome** | Formulir persetujuan umum tercetak berisi data yang sudah ada di sistem, sehingga petugas tidak menulis ulang dengan tangan. Tanda tangan tetap di atas kertas |
| **Trace** | `RWI-DEC-077`; `03-frontend-architecture.md` `FE-INP-18` dan 3A.8; `RWI-DEC-035` isi minimal |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — langkah cetak bagian 3.10 dan layar standalone `FE-INP-18` bagian 22 |
| **Reuse** | Pola halaman cetak kiosk |
| **Scope** | Halaman cetak per episode. Tidak ada endpoint baru; data dibaca dari detail episode dan kunjungan |
| **Dependency** | `FE-RWI-027` |
| **Wewenang UI** | Tata letak formulir `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Formulir memuat identitas pasien, penjamin, unit layanan, kelas, DPJP, nomor episode, dan tanggal. 2. Ketiga isi minimal `RWI-DEC-035` tercetak. 3. Layar **tidak** menyatakan persetujuan tersimpan atau tertanda tangan — sistem tidak menyimpan apa pun. 4. Halaman cetak tidak dapat dibuka tanpa hak akses. 5. Dapat dicapai dari alur admisi **dan** dari detail episode |
| **Verification** | E2E membuka halaman cetak dari kedua jalur; pemeriksaan bahwa peran tanpa hak ditolak |
| **Risk/blocker** | Menyatakan "tersimpan" akan membuat petugas mengira kertasnya tidak perlu disimpan. `RWI-CAP-031` dan `DEC-INP-003` **tetap terbuka**. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; laporan menegaskan nol penyimpanan |
| **Status** | ✅ **SELESAI 1 September 2026.** Kelima acceptance criteria dipetakan ke bukti implementasi dan dibuktikan runtime: formulir memuat identitas, penjamin, unit, kelas, DPJP, nomor episode, dan tanggal; ketiga isi minimal `RWI-DEC-035` tercetak; **nol** operasi tulis dan **nol** salinan di peramban; 403 dari server mengganti seluruh isi halaman dengan Akses Ditolak; dicapai dari alur admisi **dan** Detail Episode. `npm run lint` `0 errors`; `npm run build` `✓ Compiled successfully` dengan route `/episodes/[id]/consent-print` terdaftar; verifikasi peramban Edge `37/37 PASS`; keenam grep anti-regresi UI bersih. Bukti: [laporan](../task/report/frontend/FE-RWI-028.md) |

---

### ✅ `FE-RWI-029` — Kartu pasien tercetak pada jalur pasien baru

| Field | Isi |
| --- | --- |
| **Outcome** | Pasien baru pulang dari meja admisi membawa kartunya, tanpa petugas berpindah ke aplikasi kiosk |
| **Trace** | 3A.2 langkah 9; 3A.3 catatan "Kartu Pasien tidak ada pada jalur pasien lama" |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — bagian 3.11 |
| **Reuse** | `src/components/view/kiosk/registration/patient-card/print/` yang **sudah ada** |
| **Scope** | Langkah **Kartu Pasien** jalur pasien baru |
| **Dependency** | `FE-RWI-027` |
| **Wewenang UI** | `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Kartu tercetak berisi data pasien yang baru didaftarkan. 2. Langkah ini **tidak** ada pada jalur pasien lama. 3. Melewatinya tidak membatalkan admisi yang sudah terbentuk |
| **Verification** | E2E jalur pasien baru sampai langkah terakhir; e2e jalur pasien lama membuktikan langkah ini tidak muncul |
| **Risk/blocker** | Menyalin komponen cetak alih-alih memakainya ulang akan melahirkan dua bentuk kartu. Owner: Frontend |
| **DoD** | Ketiga kriteria lulus |
| **Status** | ✅ **SELESAI 1 September 2026.** Ketiga acceptance criteria dipetakan ke bukti implementasi dan dibuktikan runtime: kartu memuat data pasien yang baru didaftarkan, langkah ini terbukti **tidak ada** pada jalur pasien lama, dan melewatinya tidak membatalkan admisi. `BasePatientCard` dipakai ulang apa adanya sehingga tidak lahir bentuk kartu kedua. `npm run lint` `0 errors`; `npm run build` `✓ Compiled successfully`; verifikasi peramban Edge `37/37 PASS`; keenam grep anti-regresi UI bersih. Bukti: [laporan](../task/report/frontend/FE-RWI-029.md) |

---

### `FE-RWI-030` — Pasien dikonfirmasi masuk saat benar-benar tiba

| Field | Isi |
| --- | --- |
| **Outcome** | Episode menjadi `Admitted` dan tempat tidur menjadi `Occupied` pada saat pasien benar-benar sampai di kamar, dengan Kelayakan Penempatan diperiksa **ulang** di detik itu |
| **Trace** | `RWI-DEC-076`; `FLOW-RI-MVP-001` langkah 6; `FE-INP-02`; 3.2; 4.3A |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — `FE-INP-02` bagian 7; metadata reservation tersedia lewat `BE-RWI-036` |
| **Reuse** | Papan tempat tidur `FE-RWI-005`; penanganan penolakan `FE-RWI-007` |
| **Scope** | Aksi konfirmasi masuk pada papan tempat tidur. `POST /bed-occupancies/placements` |
| **Dependency** | `FE-RWI-026` |
| **Wewenang UI** | Penempatan tombol `DEV_DISCRETION`. **Batasnya:** konfirmasi wajib menyebut nama pasien dan tempat tidur |
| **Acceptance criteria** | 1. Aksi hanya dirender bagi **petugas admisi** dan **supervisor** — bagian 3.2. Peran lain tidak melihatnya. 2. Tempat tidur `Reserved` menampilkan episode yang memegangnya beserta sisa waktunya pada layar yang berhak. 3. Penolakan 422 karena Kelayakan Penempatan berubah ditampilkan apa adanya dan terbaca sebagai **keadaan yang berubah**, bukan kesalahan petugas. 4. Papan dimuat ulang tepat sebelum dialog konfirmasi tampil. 5. Setelah berhasil, episode terbaca `Admitted` dan pasien muncul pada census |
| **Verification** | E2E per peran; e2e penolakan dengan tempat tidur yang sengaja dibuat tidak layak setelah dipesan |
| **Risk/blocker** | Kontrak hak akses **tidak** memberi `InpatientBedOccupancy : Create` kepada perawat maupun kepala ruangan. Merender tombol bagi mereka menghasilkan tombol yang pasti ditolak server. Butir terbuka `RWI-OQ-045`. Owner: Frontend bersama Product/Domain |
| **Gerbang skema** | ✅ `RWI-UI-GAP-003` ditutup untuk kontrak/source backend oleh `BE-RWI-036`; papan menerima episode, pasien, `ReservationId`, dan batas waktu reservation aktif |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-031` — Admisi yang keliru dapat dibatalkan

| Field | Isi |
| --- | --- |
| **Outcome** | Admisi yang salah — penjamin keliru, DPJP keliru, atau pasien batal dirawat — dapat dibatalkan beserta pemesanan dan penempatannya dalam satu tindakan. Tanpa ini, satu-satunya jalan keluar dari kesalahan adalah membiarkannya |
| **Trace** | `FE-INP-17`; matriks peran bagian 3; 3A.5 |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — `FE-INP-17` bagian 21, dipicu dari bagian 6 dan 9 |
| **Reuse** | `confirm-modal.jsx`; detail episode |
| **Scope** | Aksi pembatalan pada detail episode dan daftar kerja. `PATCH /episodes/{id}/cancel` |
| **Dependency** | `FE-RWI-020` |
| **Wewenang UI** | Penempatan `DEV_DISCRETION`. **Batasnya:** konfirmasi wajib menyebut bahwa pemesanan dan penempatan ikut dilepas |
| **Acceptance criteria** | 1. Pembatalan `Draft` tersedia bagi petugas admisi dan supervisor. 2. Pembatalan `Admitted` tersedia bagi kepala ruangan dan supervisor, **tidak** bagi petugas admisi. 3. Pembatalan wajib beralasan. 4. Konfirmasi menyebut bahwa tempat tidur akan dilepas. 5. Setelah dibatalkan, episode terbaca `Cancelled` dan tempat tidurnya terbaca bebas pada papan |
| **Verification** | E2E per peran untuk kedua status; pemeriksaan papan sesudah pembatalan |
| **Risk/blocker** | Kewenangannya **berbeda** menurut status episode — pola yang sama dengan tombol isolasi dan sama mudahnya salah. Owner: Frontend |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-032` — Admisi yang ditinggal dapat dilanjutkan

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas yang terputus di tengah alur — browser tertutup, giliran kerja berganti, pasien pergi sebentar — dapat melanjutkan admisi yang sama, bukan memulai dari nol dan meninggalkan episode yatim |
| **Trace** | `RWI-DEC-076`; 3A.6; `IA-INP-02` |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — `FE-INP-16` bagian 6 → alur bagian 3; kontrak baca reservation tersedia lewat `BE-RWI-036` |
| **Reuse** | Daftar kerja `FE-RWI-020`; kerangka alur `FE-RWI-022` |
| **Scope** | Jalur dari daftar kerja menuju alur admisi pada langkah yang tepat. `GET /episodes/{id}` |
| **Dependency** | `FE-RWI-020`, `FE-RWI-026` |
| **Wewenang UI** | Bentuk tautan `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Episode `Draft` tanpa pemesanan dilanjutkan ke langkah **Pilih Bed**. 2. Episode `Draft` dengan pemesanan aktif dilanjutkan ke langkah **Konfirmasi**, dan sisa waktu pemesanannya terbaca. 3. Episode `Draft` yang pemesanannya sudah gugur dilanjutkan ke langkah **Pilih Bed** disertai keterangan bahwa pemesanan sebelumnya gugur. 4. Langkah yang sudah lewat **tidak** meminta pengguna mengetik ulang data yang sudah tersimpan. 5. Episode selain `Draft` **tidak** menawarkan pelanjutan |
| **Verification** | E2E ketiga keadaan `Draft`; pemeriksaan bahwa data pasien, penjamin, dan DPJP terbaca dari server, bukan kosong |
| **Risk/blocker** | Kriteria 3 menuntut layar membedakan pemesanan gugur dari tidak pernah ada. Sumbernya jawaban server, bukan hitungan waktu di sisi layar. Owner: Frontend |
| **Gerbang skema** | ✅ `RWI-UI-GAP-003` ditutup untuk kontrak/source backend oleh `BE-RWI-036`; implementasi pemulihan frontend belum dikerjakan |
| **DoD** | Kelima kriteria lulus; e2e ada dan lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-033` — Tidak ada lagi layar yang tidak dapat dicapai

| Field | Isi |
| --- | --- |
| **Outcome** | Setiap layar Rawat Inap punya jalan masuk yang jelas dan ditempatkan pada kelompok yang benar. Impact scan 28 Agustus membuktikan sesi koreksi kini terjangkau melalui daftar kerja dan detail; task ini tetap menutup keterjangkauan seluruh 19 layar, koreksi hierarki tujuh operasional + dua master/configuration, metadata filter census, dan kepemilikan operasi yang lain |
| **Trace** | `IA-INP-01` s.d. `IA-INP-05`; bagian 11A |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — register bagian 2 dan peta navigasi seluruh layar bagian 23 |
| **Reuse** | Menu, route, page, hook, dan permission yang sudah ada; daftar kerja `FE-RWI-020`. `FE-INP-12/13` di-re-parent, bukan dibuat ulang |
| **Scope** | Menu sidebar dan tautan antar layar; memindahkan **Butir Administrasi Rawat Inap** serta **Pengaturan Rawat Inap** ke `Pelayanan Kesehatan → Master Data`; menghapus duplikatnya dari submenu `Rawat Inap`; mempertahankan route existing; `GET /census/filters/metadata` yang masih menganggur |
| **Dependency** | `FE-RWI-020` s.d. `FE-RWI-032` |
| **Wewenang UI** | Brief UI pemilik mengunci induk dan urutan menu operasional. Ikon, warna, jarak, dan bentuk expand/collapse tetap `DEV_DISCRETION`. **Batasnya:** kelima aturan `IA-INP` |
| **Acceptance criteria** | 1. Setiap layar bagian 2 dapat dicapai dari beranda dalam paling banyak tiga klik. 2. Layar sesi koreksi dapat dicapai dari daftar kerja tersaring `Closed`. 3. Submenu `Rawat Inap` berisi tepat tujuh butir dan berurutan: **Beranda Rawat Inap, Admisi Rawat Inap, Papan Tempat Tidur, Daftar Kerja Episode, Pasien Sedang Dirawat, Daftar Pantau, Selisih Tempat Tidur**. 4. **Butir Administrasi Rawat Inap** dan **Pengaturan Rawat Inap** masing-masing tampil tepat satu kali di `Pelayanan Kesehatan → Master Data`, tidak lagi di submenu `Rawat Inap`, serta tetap memakai route dan permission existing. 5. Layar per-episode tidak mendapat butir menu. 6. Tidak ada operasi pada api contract yang tidak dimiliki satu layar, kecuali yang dinyatakan sengaja tidak dipakai pada bagian 11A. 7. Penyaring census memakai `filters/metadata`, bukan daftar yang ditanam di kode |
| **Verification** | Penelusuran manual seluruh 19 layar dari beranda dan kedua jalur master data, dilampirkan sebagai daftar jalur; pemeriksaan bahwa kedua item tidak terduplikasi dan direct URL existing tetap bekerja; e2e menuju sesi koreksi lewat daftar kerja |
| **Risk/blocker** | Re-parent harus memindahkan definisi menu, bukan menyalinnya. Normalisasi URL tidak termasuk scope; bila dipaksakan tanpa compatibility redirect, deep link dan tautan internal dapat putus. Kriteria 6 menuntut pemeriksaan endpoint satu per satu terhadap bagian 11A. Owner: Frontend |
| **DoD** | Ketujuh kriteria lulus; laporan memuat tabel jalur untuk seluruh 19 layar dan bukti hierarki tujuh operasional + dua master/configuration tanpa duplikasi |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-034` — Layar admisi lama dibongkar, jalur gandanya hilang

| Field | Isi |
| --- | --- |
| **Outcome** | Hanya ada **satu** jalan menuju admisi. Membiarkan formulir lama berdampingan dengan alur baru menghasilkan dua jalur menuju hal yang sama, dan salah satunya pasti lupa diperbarui |
| **Trace** | `RWI-DEC-079`; 2C "satu kemampuan, satu tempat" |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — alur target bagian 3 dan klasifikasi conflict/replace bagian 24 |
| **Reuse** | Bagian yang masih terpakai dari `use-inpatient-admission` dipindahkan, bukan disalin |
| **Scope** | `inpatient-admission-view.jsx`, `use-inpatient-admission.jsx`, `inpatient-admission-utils.jsx`, `inpatient-admission-constants.jsx`, dan test yang menyertainya |
| **Dependency** | `FE-RWI-027` |
| **Wewenang UI** | Tidak ada |
| **Acceptance criteria** | 1. Tidak ada lagi route, komponen, atau menu yang membuka formulir admisi tunggal. 2. Tidak ada berkas yatim yang tidak diacu siapa pun. 3. Test lama yang menguji formulir tunggal dihapus atau diarahkan ke alur baru — **tidak** dibiarkan dilewati. 4. `lint`, `test:unit`, dan `build` lulus |
| **Verification** | Pencarian menyeluruh atas nama berkas lama; keluaran ketiga perintah dilampirkan apa adanya |
| **Risk/blocker** | Menandai test lama sebagai dilewati alih-alih menghapusnya menyembunyikan penurunan cakupan. Owner: Frontend |
| **DoD** | Keempat kriteria lulus |
| **Status** | ⬜ belum dikerjakan |

---

### `FE-RWI-036` — Papan Tempat Tidur kembali menjadi layar kerja

| Field | Isi |
| --- | --- |
| **Outcome** | Papan tidak lagi berhenti sebagai daftar pasif. Petugas yang berhak dapat membaca keadaan bed, menindaklanjuti reservation, dan memahami mengapa sebuah bed tidak dapat dipakai |
| **Trace** | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-02`; skema §7 dan §24.1; `RWI-DEC-076` |
| **Kontrak** | API `0.4.0`: `GET /bed-occupancies/board`, `GET /available-beds`, `POST /bed-occupancies/placements`, `PATCH /bed-occupancies/reservations/{id}/cancel`; permission/audit `0.4.0` |
| **Reuse** | `InpatientBedBoard`, `useInpatientBedBoard`, filter resource, penanganan 409/422, serta hasil `FE-RWI-026/030`. Logic kelayakan server tidak ditulis ulang |
| **Scope** | Layout/status bed, reload/retry, empty state, countdown reservation, serta integrasi **Konfirmasi Masuk** dan **Batalkan Pesanan** pada standalone board. Menghapus keadaan `selectable={false}` yang menjadikan layar tanpa aksi efektif |
| **Dependency** | `FE-RWI-026`, `FE-RWI-030`; metadata board backend `BE-RWI-036` sudah selesai. Data runtime untuk pembuktian menunggu `RWI-UI-GAP-007` |
| **Wewenang UI** | Susunan wilayah dan label aksi mengikuti skema §7. Warna, ukuran kartu, dan ikon tetap `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Ringkasan dan kartu bed mengikuti keadaan server. 2. Bed `Reserved` menampilkan pemegang, sisa waktu, dan aksi yang diizinkan. 3. **Konfirmasi Masuk** serta **Batalkan Pesanan** tidak muncul bagi peran tanpa hak. 4. Empty state membedakan “master bed belum tersedia” dari “tidak cocok dengan filter” dan memberi jalan ke Master Data bagi admin yang berhak. 5. Gagal baca menyediakan **Coba Lagi** tanpa kehilangan filter. 6. Tidak ada aturan kelayakan yang dihitung ulang di browser |
| **Verification** | Pada saat task dieksekusi: penelusuran manual keadaan Available/Reserved/Occupied/Unavailable, bukti per peran, dan e2e aksi reservation/placement. Tidak dijalankan pada penyusunan roadmap ini |
| **Risk/blocker** | Metadata reservation sudah tersedia lewat `BE-RWI-036`; data bed runtime masih gap 007. Owner tersisa: Frontend dan Admin Master Data |
| **DoD** | Keenam kriteria lulus; laporan membuktikan layar tidak lagi pasif dan tidak memakai mock tersembunyi |
| **Status** | ⛔ `BLOCKED` — menunggu approval skema/roadmap; pembuktian penuh juga menunggu gap 007. Gap 003 sudah ditutup `BE-RWI-036` |

---

### `FE-RWI-037` — Census mempunyai jalan kerja saat berisi maupun kosong

| Field | Isi |
| --- | --- |
| **Outcome** | Petugas dapat membuka Detail Episode dari setiap pasien yang sedang dirawat; saat census kosong, halaman tetap menjelaskan tindakan berikutnya, bukan menjadi tabel mati |
| **Trace** | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-01`; skema §8 dan §24.1; `EPIC RI-24` |
| **Kontrak** | API `0.4.0`: `GET /census`, `GET /census/filters/metadata`, route Detail Episode; permission `InpatientCensus : Read` dan `InpatientEpisode : Read` |
| **Reuse** | `InpatientCensusView`, `useInpatientCensus`, `DataTable`, `ResourceFilterSelect`, dan route detail existing |
| **Scope** | Hierarki visual, filter metadata, action column, empty/error state, serta CTA permission-aware ke **Admisi Rawat Inap** dan **Daftar Kerja Episode** ketika tidak ada pasien dirawat |
| **Dependency** | `FE-RWI-033` untuk metadata dan navigasi; data runtime untuk pembuktian menunggu `RWI-UI-GAP-007` |
| **Wewenang UI** | Susunan wilayah, kolom, dan tujuan aksi mengikuti skema §8; gaya visual tetap `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Kolom Episode, Pasien, Lokasi, DPJP, Perawat, Hari Rawat, Status, dan Aksi terbaca pada desktop maupun sempit. 2. **Detail Episode** tampil pada setiap baris hanya bila berhak. 3. Empty state menyediakan **Buka Admisi** dan/atau **Buka Daftar Kerja Episode** sesuai permission. 4. Filter berasal dari `filters/metadata`. 5. Gagal baca menyediakan **Coba Lagi** dan tidak ditampilkan sebagai nol data. 6. Informasi klinis sensitif di luar skema tidak ditampilkan |
| **Verification** | Pada saat task dieksekusi: manual state berisi/kosong/gagal/peran dan e2e menuju Detail Episode. Tidak dijalankan pada penyusunan roadmap ini |
| **Risk/blocker** | Tanpa episode `Admitted`/`DischargePending`, aksi baris tidak dapat dibuktikan pada environment target. Owner: Frontend; data: Admin Master Data/tim penyiap environment |
| **DoD** | Keenam kriteria lulus dan laporan memuat bukti state berisi serta kosong |
| **Status** | ⛔ `BLOCKED` — menunggu approval skema/roadmap; bukti runtime penuh menunggu gap 007 |

---

### `FE-RWI-038` — Daftar Pantau menunjukkan tindak lanjut yang nyata

| Field | Isi |
| --- | --- |
| **Outcome** | Empat daftar pantau mudah dibedakan dan setiap baris mengarahkan petugas ke layar pemilik tindakan; keadaan kosong tetap memberi konteks operasional |
| **Trace** | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-09`; skema §14 dan §24.1; `FR-RI-135` s.d. `138`, `FR-RI-161` |
| **Kontrak** | API monitoring `0.4.0` untuk penutupan tertunda, override, tanpa perawat, dan ketidakcocokan isolasi; permission `InpatientMonitoring : Read` |
| **Reuse** | `InpatientMonitoringView`, empat normalizer/list config, route Detail/Penutupan/Perpindahan, dan filter existing |
| **Scope** | Hierarki tab dan count, action column, empty/error state, serta jalur tindak lanjut. Halaman tetap read-only dan tidak memanggil endpoint tulis |
| **Dependency** | Endpoint monitoring existing; data runtime untuk pembuktian menunggu `RWI-UI-GAP-007` |
| **Wewenang UI** | Label empat daftar dan pemilik tindak lanjut mengikuti skema §14; visual tab tetap `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Keempat tab menampilkan count dan mempertahankan tab aktif saat filter/retry. 2. Penutupan tertunda menawarkan Detail dan Penutupan Episode. 3. Ketidakcocokan isolasi menawarkan jalan ke Detail/Perpindahan. 4. Daftar lain menawarkan Detail Episode. 5. Empty state menyatakan tidak ada tindak lanjut dan menyediakan jalan ke Daftar Kerja Episode. 6. Satu tab gagal tidak menutup tab lain yang berhasil. 7. Tidak ada request tulis dari halaman daftar pantau |
| **Verification** | Pada saat task dieksekusi: manual empat tab/state dan pemeriksaan request network; e2e setiap tujuan tautan. Tidak dijalankan pada penyusunan roadmap ini |
| **Risk/blocker** | Data kosong dapat menyamarkan action column. Owner: Frontend; data pembuktian: penyiap environment |
| **DoD** | Ketujuh kriteria lulus; laporan menunjukkan tujuan setiap tindak lanjut |
| **Status** | ⛔ `BLOCKED` — menunggu approval skema/roadmap; bukti runtime penuh menunggu gap 007 |

---

### `FE-RWI-039` — Selisih Tempat Tidur menjadi laporan diagnostik yang terbaca

| Field | Isi |
| --- | --- |
| **Outcome** | Supervisor memahami perbedaan status dan dapat membuka Papan Tempat Tidur dengan konteks yang sama; layar tidak berpura-pura memperbaiki data tanpa kontrak |
| **Trace** | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-10`; skema §15 dan §24.1; `FR-RI-135` s.d. `138` |
| **Kontrak** | API monitoring bed drift `0.4.0`; route `FE-INP-02`; permission `InpatientMonitoring : Read` |
| **Reuse** | `InpatientBedDriftView`, `DataTable`, status badge, filter unit, dan route papan existing |
| **Scope** | Hierarki visual laporan, penjelasan dua nilai status, empty/error state, serta action **Buka Papan Tempat Tidur** yang membawa konteks unit/bed bila tersedia. Tidak menambah aksi rekonsiliasi |
| **Dependency** | `FE-RWI-036`; data runtime untuk pembuktian menunggu `RWI-UI-GAP-007` |
| **Wewenang UI** | Batas read-only dan tujuan navigasi dikunci skema §15; gaya visual tetap `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Setiap baris memperlihatkan bed, lokasi, status salinan, status seharusnya, selisih, dan episode pemegang. 2. **Buka Papan Tempat Tidur** terlihat pada state berisi maupun kosong. 3. Navigasi mempertahankan konteks unit/bed bila tersedia. 4. Empty state dinyatakan sebagai keadaan positif, bukan error. 5. Gagal baca menyediakan **Coba Lagi**. 6. Tidak ada tombol “Perbaiki” atau request tulis |
| **Verification** | Pada saat task dieksekusi: manual state mismatch/kosong/gagal, inspeksi query navigasi, dan pemeriksaan tidak ada request tulis. Tidak dijalankan pada penyusunan roadmap ini |
| **Risk/blocker** | Menambah tombol koreksi akan menciptakan kemampuan yang tidak dikontrak. Owner: Frontend |
| **DoD** | Keenam kriteria lulus; laporan membuktikan sifat read-only dan navigasi kontekstual |
| **Status** | ⛔ `BLOCKED` — menunggu approval skema/roadmap; bukti runtime penuh menunggu gap 007 |

---

### `FE-RWI-040` — Butir Administrasi dapat dikelola dari keadaan kosong

| Field | Isi |
| --- | --- |
| **Outcome** | Admin dapat menambah butir pertama saat daftar kosong dan mengelola butir existing tanpa kehilangan aksi akibat layout atau permission yang salah |
| **Trace** | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-13`; skema §18 dan §24.1; `FR-RI-142` s.d. `144` |
| **Kontrak** | API master data `0.4.0`: GET list/detail, POST, PUT, PATCH status, DELETE; permission `InpatientClearanceItem : Read/Create/Update/Delete` |
| **Reuse** | Hook CRUD, modal/editor, confirmation, toast, `HealthServicesMasterData` base components, dan service existing |
| **Scope** | Layout target, empty-state **Tambah**, action row Detail/Ubah/Aktifkan/Nonaktifkan/Hapus, retry, permission gating, serta integrasi induk menu hasil `FE-RWI-033` |
| **Dependency** | `BE-RWI-005`, `FE-RWI-033`; seed tiga butir untuk bukti awal berada pada `RWI-UI-GAP-007`, tetapi admin berhak tetap harus dapat menambah dari keadaan kosong |
| **Wewenang UI** | Label, wilayah, dan jenis aksi mengikuti skema §18. Bentuk modal/drawer tetap `DEV_DISCRETION` |
| **Acceptance criteria** | 1. **Tambah Butir** tetap terlihat dan bekerja saat tabel kosong bagi peran Create. 2. Aksi row hanya muncul sesuai permission masing-masing. 3. Form mempertahankan isian ketika server menolak. 4. Status/hapus memakai konfirmasi yang menjelaskan dampak pada checklist berikutnya. 5. Error list mempunyai **Coba Lagi**. 6. Layar muncul tepat sekali di `Pelayanan Kesehatan → Master Data`. 7. Tidak ada data awal yang ditanam di frontend |
| **Verification** | Pada saat task dieksekusi: manual/e2e create-detail-update-status-delete per permission dan state kosong. Tidak dijalankan pada penyusunan roadmap ini |
| **Risk/blocker** | Source memuat handler, tetapi laporan runtime menyatakan aksi tidak dapat dipakai; penyebab permission/runtime harus dibuktikan, bukan ditebak. Owner: Frontend bersama Admin Master Data |
| **DoD** | Ketujuh kriteria lulus; laporan membuktikan aksi dari keadaan kosong dan tidak ada duplikasi menu |
| **Status** | ⛔ `BLOCKED` — menunggu approval skema/roadmap; validasi data awal menunggu gap 007 |

---

### `FE-RWI-041` — Pengaturan Rawat Inap mempunyai shell dan form yang operasional

| Field | Isi |
| --- | --- |
| **Outcome** | Admin melihat halaman master data yang utuh dan dapat menyimpan pengaturan ketika baris `DEFAULT` tersedia; bila environment belum siap, halaman menjelaskan blocker tanpa menjadi ruang kosong besar |
| **Trace** | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-12`; skema §17 dan §24.1; `FR-RI-142` s.d. `144`; `RWI-DEC-063` |
| **Kontrak** | API master data `0.4.0`: `GET /inpatient-settings`, `PUT /inpatient-settings/{id}`; **tidak ada POST**; permission `InpatientSetting : Read/Update` |
| **Reuse** | `HealthServicesMasterDataEditorView`, `BaseEditorForm`, hook/service/validator setting existing |
| **Scope** | Shell halaman target, form/audit, save/error state, serta keadaan 404 yang ringkas dan actionable. Tidak membuat endpoint atau nilai `DEFAULT` dari browser |
| **Dependency** | `BE-RWI-002` harus benar-benar mengisi master `DEFAULT` pada environment target; `BE-RWI-005`; `FE-RWI-033`; `RWI-UI-GAP-007` |
| **Wewenang UI** | Urutan field, satuan, dan audit mengikuti skema §17. Gaya form tetap `DEV_DISCRETION` |
| **Acceptance criteria** | 1. Dengan baris `DEFAULT`, seluruh field kontrak dan audit tampil. 2. **Simpan Pengaturan** hanya aktif ketika form valid dan berubah. 3. Error simpan mempertahankan isian. 4. Pada 404, shell tetap utuh dan menyebut master environment belum diisi, menyediakan **Muat Ulang** serta navigasi kembali ke Master Data. 5. Layar tidak menawarkan Create dan tidak mengirim POST. 6. Layar muncul tepat sekali di `Pelayanan Kesehatan → Master Data`. 7. Tidak ada nilai bawaan yang ditanam di frontend |
| **Verification** | Pada saat task dieksekusi: manual/e2e GET sukses, PUT sukses/422/403, dan 404; inspeksi bahwa tidak ada POST. Tidak dijalankan pada penyusunan roadmap ini |
| **Risk/blocker** | Frontend tidak dapat menyelesaikan 404 tanpa data `DEFAULT`; memperkenalkan tombol Create akan melanggar kontrak satu baris. Owner data: Admin Master Data/Tim Master Data; owner UI: Frontend |
| **DoD** | Ketujuh kriteria lulus; bukti data `DEFAULT` environment dilampirkan tanpa mengekspos data sensitif |
| **Status** | ⛔ `BLOCKED` — menunggu approval skema/roadmap dan penutupan `RWI-UI-GAP-007` |

---

### `FE-RWI-035` — Alur bisnis utama terbukti berjalan ujung ke ujung

| Field | Isi |
| --- | --- |
| **Outcome** | `FLOW-RI-MVP-001` terbukti dapat dijalankan dari pasien datang sampai episode ditutup, dan setiap layar terbukti hanya dijangkau peran yang berhak. Menggantikan cakupan `FE-RWI-019` yang disusun ketika layarnya masih lima belas |
| **Trace** | `03-frontend-architecture.md` bagian 10; `RWI-DEC-051`; `GUARD-INP-01` s.d. `GUARD-INP-04` |
| **Skema tampilan** | [`05-skema-tampilan.md`](../05-skema-tampilan.md) — seluruh `FE-INP-01` s.d. `19`, state bagian 4, navigasi bagian 23, dan gerbang bagian 25 |
| **Reuse** | `tests/e2e/route-smoke.spec.mjs` dan seluruh e2e yang sudah ada — menambah kasus, bukan membuat kerangka baru |
| **Scope** | Rangkaian e2e; penyelesaian empat kriteria yang tertahan pada `FE-RWI-003`, `004`, `012`, dan `013`; pembuktian hasil repair `FE-RWI-036` s.d. `041` |
| **Dependency** | Seluruh task `FE-RWI-020` s.d. `FE-RWI-034` dan `FE-RWI-036` s.d. `FE-RWI-041` |
| **Wewenang UI** | Tidak ada |
| **Acceptance criteria** | 1. Satu e2e menjalankan `FLOW-RI-MVP-001` jalur pasien baru dari langkah 1 sampai episode `Closed`. 2. Satu e2e menjalankan jalur pasien lama sampai tempat tidur `Reserved`. 3. Kunjungan yang terbentuk terbukti membawa penjamin yang dipilih, bukan tunai bawaan. 4. Alur yang ditinggal setelah titik tulis 1 terbukti dapat ditemukan kembali dan dilanjutkan. 5. Setiap layar dari kesembilan belas terbukti tertutup bagi peran yang tidak berhak. 6. Keempat aturan penjaga `GUARD-INP-01` s.d. `GUARD-INP-04` terbukti terlihat di layar. 7. Empat kriteria yang tertahan sejak revision 2 diselesaikan atau dinyatakan tertahan beserta alasannya yang masih berlaku. 8. Keenam layar bukti runtime mempunyai state berisi/kosong/gagal dan aksi sesuai `FE-RWI-036` s.d. `041`; tidak ada lagi layar pasif yang diterima hanya karena route-nya terbuka |
| **Verification** | Jalankan rangkaian e2e penuh; lampirkan keluarannya apa adanya; tidak ada kasus yang ditandai dilewati |
| **Risk/blocker** | Kriteria 1 adalah e2e terpanjang pada modul ini dan menyentuh tiga bounded context. Menyiapkan data masternya lebih dulu — unit layanan bertipe rawat inap, kamar, tempat tidur, kelas, penjamin — adalah prasyarat, bukan bagian dari test. Owner: Frontend bersama penanggung jawab data master |
| **Gerbang skema** | `RWI-UI-GAP-001` s.d. `007` harus tertutup atau dinyatakan tertahan dengan owner dan bukti yang masih berlaku; e2e tidak boleh menyamarkannya dengan mock |
| **DoD** | Kedelapan kriteria lulus; keluaran rangkaian e2e dan bukti enam layar terlampir |
| **Status** | ⬜ belum dikerjakan |

---

## 6. Gerbang yang masih terbuka

| Gerbang | Keadaannya | Menahan |
| --- | --- | --- |
| `RWI-UI-GAP-001` jumlah langkah pasien lama | Revision `3` menulis delapan, tetapi urutan bernama menghasilkan sembilan bila Cetak Persetujuan dihitung | `FE-RWI-022`, `035` |
| ~~`RWI-UI-GAP-002` penjamin perusahaan~~ | ✅ **Tertutup 31 Agustus 2026.** `BE-RWI-035` menutup sisi backend, `FE-RWI-025` menutup sisi frontend. Kunjungan admisi kini membawa payer perusahaan beserta referensi dan snapshot-nya | Tidak lagi menahan; bukti runtime ujung-ke-ujung tetap milik `FE-RWI-035` |
| ~~`RWI-UI-GAP-003` pemesanan tidak terbaca~~ | ✅ **Tertutup untuk kontrak/source backend 1 September 2026 oleh `BE-RWI-036`.** `GET /bed-occupancies/bed-board` mengembalikan `HoldingEpisodeId`, `HoldingEpisodeNumber`, `PatientName`, `ReservationId`, dan `ReservationExpiresAt` untuk reservation aktif; expired/occupied/tanpa pemegang dijaga test | Tidak lagi memblokir `FE-RWI-020`, `026`, `030`, `032`, atau `036`; implementasi frontend masing-masing tetap mengikuti status task-nya |
| `RWI-UI-GAP-004` baca kelayakan keuangan | Tidak ada GET nilai/riwayat dan permission baca kasir belum terbukti | Delta `FE-RWI-013`; `FE-RWI-035` |
| `RWI-UI-GAP-005` baca sesi koreksi | Tidak ada GET sesi; refresh tidak memulihkan sesi terbuka | Delta `FE-RWI-018`; `FE-RWI-035` |
| `RWI-UI-GAP-006` route dan permission pasien/encounter | ✅ **Tertutup pada level kontrak/source.** `FE-RWI-023` membuktikan route pasien `/admin`; `FE-RWI-024` membuktikan route payer `/admin/options` dan `/admin`; source backend `64d7419…` membuktikan `POST /patient-encounters/admin` dijaga `PatientEncounter : Create` | Tidak lagi menahan `FE-RWI-025`; kesiapan payer perusahaan tetap milik gap 002 |
| `RWI-UI-GAP-007` data master/runtime belum layak | Screenshot pemilik menunjukkan pengaturan `DEFAULT` tidak ditemukan, butir administrasi kosong, papan nol bed, dan tidak ada episode untuk membuktikan aksi berbasis baris. Seeder ada di source, tetapi keterisiannya pada environment target belum terbukti | `FE-RWI-036`–`041`; **memblokir penuh `FE-RWI-041`** dan bukti runtime `FE-RWI-035` |
| Approval blueprint revision 3 | ✅ **Tertutup.** `RWI-DEC-075` s.d. `RWI-DEC-079` disetujui Muhammad Hamzah pada 27 Agustus 2026 | Riwayat; tidak menyetujui revision 4 maupun 5 |
| Approval skema/roadmap revision 5 | **Terbuka.** `05-skema-tampilan.md` `0.4` dan roadmap ini tetap `DRAFT` | Menahan pemakaian skema dan enam task repair sebagai brief UI mengikat |
| Kecukupan kontrak backend | **Sebagian.** Ke-49 operasi baseline ada, tetapi gap 002–006 membuktikan tidak semua target layar mempunyai baca/tulis/permission yang cukup | Task sesuai baris gap; roadmap frontend tidak membuat backend |
| Kesiapan data master | `RWI-DEC-063`. Unit layanan bertipe rawat inap, kamar, tempat tidur, kelas, penjamin, satu pengaturan `DEFAULT`, dan butir administrasi awal | `FE-RWI-026` ke atas tidak dapat dibuktikan dengan data nyata; hard blocker `FE-RWI-041` |
| `IsQueueRequired` unit rawat inap | Harus bernilai salah agar admisi tidak membuat antrean semu — 3A.7. **Ini properti master data, bukan sesuatu yang dapat dipaksakan layar:** `FE-RWI-025` mengirim `serviceUnitId` apa adanya dan backend yang membaca benderanya. Perlu dibuktikan Tim Master Data pada environment target | Tidak menahan `FE-RWI-025` yang sudah selesai; menahan bukti runtime bebas antrean semu |
| `RWI-OQ-045` hak akses konfirmasi masuk | Kepala ruangan belum punya `InpatientBedOccupancy : Create` | Tidak menahan; `FE-RWI-030` berjalan dengan peran yang kontraknya izinkan |
| `RWI-OQ-046` jalur admisi tanpa `EncounterId` | Masih terbuka di backend | Tidak menahan; tidak ada layar yang menempuhnya |
| Security/privacy owner | Belum ditunjuk | Tidak menahan; aturan privasi tetap berlaku dan tetap diuji |

---

## 7. Yang sengaja tidak ada di roadmap ini

| Yang tidak dikerjakan | Alasan |
| --- | --- |
| Layar pengkajian, catatan dokter, CPPT, dan resep | Slice di luar scope MVP — `DEC-INP-001` |
| Layar serah terima IGD | Di luar scope — `DEC-INP-002` |
| **Penyimpanan** persetujuan umum rawat inap | `RWI-DEC-077` memilih cetak tanpa menyimpan. `RWI-CAP-031` dan `DEC-INP-003` tetap terbuka |
| Daftar pantau kepatuhan pengkajian dan CPPT | Bergantung pada slice yang di luar scope — `DEC-INP-001` |
| Perubahan hak akses agar perawat dapat mengonfirmasi masuk | Wewenang kontrak, bukan wewenang roadmap frontend — `RWI-OQ-045` |
| Penutupan jalur admisi tanpa `EncounterId` di backend | Wewenang Backend/API — `RWI-OQ-046` |
| Mengisi baris `DEFAULT`, tiga butir administrasi awal, kamar, atau tempat tidur dari browser | Wewenang Admin Master Data/Tim Master Data dan seeder `BE-RWI-002`; dicatat sebagai `RWI-UI-GAP-007` |
| Tombol rekonsiliasi pada Selisih Tempat Tidur | Tidak ada endpoint tulis yang dikontrak; layar hanya menavigasi ke Papan Tempat Tidur |
| Menyalin ruang kerja antrean dokter | Pasien rawat inap tidak punya antrean |
| Menyaring ulang tempat tidur di sisi layar | Aturan Kelayakan Penempatan hanya boleh ada **satu**, dan tempatnya di server |
