# Laporan Perubahan Frontend — `FE-LAB-46`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-46` |
| Judul | Perbaikan: layar Wadah menerjemahkan token alamat |
| Slice | Susulan `S2` (wadah) — amendment pass putaran 22 |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *Susulan putaran 22*, revisi 65 |
| Trace | Capability map revision 7 `CAP-P22-10` (`Repair`); prasyarat `AC-280`; `LAB-DEC-191`; menjaga `LAB-FE-009`/`LAB-FE-010` |
| Contract version | `LAB-API-v1` `r39` — `approved`. Tidak diamandemen |
| Wewenang UI | Nol keputusan tampilan baru. Bunyi pesan tautan tidak valid mengikuti pola detail pesanan |
| Dependency | Nol. Backend tidak berubah |
| Klasifikasi | `LIGHT` — satu berkas hook, nol endpoint, nol state Redux baru, nol komponen baru |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `use-lab-specimen-workspace.jsx`; laporan ini beserta status roadmap dan traceability di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `5427ddbe4` (branch `YogaV2`, upstream `origin/YogaV2`) |
| Commit backend yang dijadikan rujukan | `171dc314` (branch `yoga`) |
| Tanggal | 2026-10-07 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — AC (a)–(d) terbukti di peramban terhadap backend lokal dan basis data dev; AC (e) **tidak dijalankan** karena menulis ke basis data bersama tanpa izin terpisah |

---

## 1. Keadaan yang ditemukan di awal

Layar *Wadah dan Pemeriksaan* (`FE-LAB-07`) menerima segmen alamat apa adanya sebagai ID pesanan.
Padahal sejak daftar pantau dan detail pesanan memakai **token privat**, segmen itu berbentuk
`hemoglobin-82bbe6ab3485`, bukan GUID.

| Bukti | Isi |
| --- | --- |
| `app/.../lab-orders/[slug]/specimens/page.jsx` | `resolveLabOrderRouteToken` hanya memeriksa **bentuk** token, tidak menerjemahkannya |
| `use-lab-specimen-workspace.jsx` (sebelum) | `fetchLabSpecimenWorkspace(labOrderId)` dan rencana wadah memakai token mentah |
| Backend `LabSpecimenController.cs:131,157` | `by-order/{labOrderId:guid}` — rute tidak cocok untuk nilai bukan GUID → `404` |
| `use-lab-order-detail.jsx:93-111` | Detail pesanan **sudah** menerjemahkan token lewat `resolvePrivateRouteToken` — layar Wadah tidak |
| Laporan `FE-LAB-07` | Verifikasi manual tidak pernah dijalankan, sehingga cacat ini lolos |

Akibatnya, tombol *Wadah dan Pemeriksaan* pada detail pesanan membuka layar yang menulis *"Data wadah
gagal dimuat."*, dan aksi *Terima Sampling* yang direncanakan `FE-LAB-47` akan jatuh ke lubang yang sama.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** analis laboratorium (verifikasi memakai superadmin sebagai pengganti — lihat bagian 6).

1. Analis membuka *Pemeriksaan Patologi Klinik* lalu klik ganda satu baris. Detail pesanan terbuka
   dengan alamat bertoken, misalnya `/lab-orders/hemoglobin-82bbe6ab3485`.
2. Analis menekan *Wadah dan Pemeriksaan*. Alamat menjadi `/lab-orders/hemoglobin-82bbe6ab3485/specimens`.
3. **Sekarang:** layar menerjemahkan token itu menjadi ID pesanan, lalu memuat wadah dan pemeriksaannya.
   Petugas melihat kartu wadah beserta isinya dan dapat merencanakan, menerima, atau menolak wadah.
4. *Kembali ke Detail Pesanan* membuka detail dengan **token yang sama** — ID tidak pernah tampil di bilah alamat.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Token kedaluwarsa atau dibuka di sesi peramban lain | Pesan *"Link detail sudah kedaluwarsa atau dibuka dari sesi browser yang berbeda. Silakan buka ulang dari daftar data."*; **nol** permintaan ke backend |
| Alamat memakai GUID langsung (tautan lama) | Tetap berjalan seperti biasa |
| Penyimpanan sesi peramban tidak tersedia | Token memang tidak pernah dibuat; daftar memakai GUID, dan layar menerimanya |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`, `test-policy.md`, `REPORT_TEMPLATE.md`;
`use-lab-specimen-workspace.jsx`; `use-lab-order-detail.jsx` (pola acuan); `lab-specimen-workspace-view.jsx`;
`app/.../lab-orders/[slug]/specimens/page.jsx` dan `route-token.js`; `utils/security/private-route-token-utils.js`;
`lab-specimen-slice.jsx`; `lab-specimen.service.js`; `lab-examination.service.js`; backend
`LabSpecimenController.cs`, `LabExaminationController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/laboratory-management/use-lab-specimen-workspace.jsx` | (1) Token alamat diterjemahkan **sekali** di efek lewat `resolvePrivateRouteToken` (`LAB_ORDER_ROUTE_TOKEN`, `allowUuidFallback: true`) — pola persis detail pesanan. (2) **ID** hasil terjemahan dipakai pemuatan wadah dan pemeriksaan, muat ulang, dan rencana wadah. (3) Rencana wadah ditolak diam-diam bila ID belum ada. (4) Token tetap dipakai *Kembali ke Detail Pesanan*. (5) Pesan galat tautan diteruskan lewat `errorMessage` yang sudah ditampilkan view. +52 −10 baris |

### 3.3 Kepatuhan arsitektur frontend

Perubahan tinggal di lapisan hook (`src/lib/hooks`), memakai utility keamanan yang sudah ada
(`utils/security`) dan konstanta token pesanan yang sudah dipakai daftar pantau dan detail pesanan. Nol
komponen, slice, service, atau route baru; `page.jsx` tetap tipis. Penerjemahan sengaja di `useEffect`,
bukan saat render: penyimpanan sesi hanya ada di peramban, dan menerjemahkannya saat render server akan
selalu gagal lalu berbeda dari render klien.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | *"Mengambil data wadah..."* — tidak berubah |
| Kosong | *"Belum ada wadah pada pesanan ini"* — tidak berubah |
| Gagal | Pesan backend apa adanya; **baru:** pesan tautan tidak valid bila token tidak dapat diterjemahkan |
| Tanpa hak akses | `AccessDeniedGate` — tidak berubah |

---

## 5. Endpoint yang dikonsumsi

Nol endpoint baru. Yang berubah hanya **nilai** penunjuk yang dikirim: kini GUID, bukan token.

#### Health Services / Laboratory Management / Lab Specimen

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-specimens/by-order/{labOrderId}` | Kartu wadah satu pesanan | `LabSpecimen : Read` |
| `POST` | `/v1/health-services/laboratory-management/lab-specimens/by-order/{labOrderId}` | Merencanakan wadah | `LabSpecimen : Plan` |

#### Health Services / Laboratory Management / Lab Examination

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-examinations/by-order/{labOrderId}` | Isi tiap wadah | `LabExamination : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Nol error | `PASS` | Keluaran kosong |
| ESLint berkas yang diubah | 0 error, 2 warning `react-hooks/set-state-in-effect`: satu lama (baris 150, pemuat jenis specimen) dan **satu baru** (baris 93, `setOrderId` di efek) | `PASS` dengan warning | Pola acuan `use-lab-order-detail.jsx:102` memunculkan warning yang sama; dipertahankan supaya kedua layar menerjemahkan token dengan cara identik |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2475 uji: 2467 lulus, 8 gagal | `PASS` (nol kegagalan baru) | Kedelapan kegagalan sama dengan baseline 2026-10-06, seluruhnya di luar Laboratorium (privasi HD ×2, FE-HMD-01 ×2, paritas Rawat Jalan ×2, Bank Darah M0, petty cash) |
| `npm run build` | `Compiled successfully in 57s`; 470 halaman statis; route `/lab-orders/[slug]/specimens` terbentuk; standalone siap | `PASS` | Log build. Dijalankan saat nol server lokal hidup |
| (a) Daftar PK → detail → *Wadah dan Pemeriksaan* | Wadah dan pemeriksaan `LAB-RSMMC-000001` tampil (Leukosit, Hemoglobin cito, Trombosit dibatalkan; wadah kedua *Perlu Ambil Ulang*); `by-order` menjawab `200` | `PASS` | Playwright, tangkapan layar di scratchpad sesi |
| (b) Nilai yang dikirim | Alamat `…/hemoglobin-82bbe6ab3485/specimens`, permintaan memakai `5afcc717-…-94b94ccce6a1` | `PASS` | Pencatat permintaan jaringan |
| *Kembali ke Detail Pesanan* | Kembali ke token yang sama | `PASS` | Alamat sesudah klik |
| (c) Token palsu `token-palsu-abc123xyz` | Pesan tautan kedaluwarsa tampil; **nol** permintaan `/lab-specimens/**` maupun `/lab-examinations/by-order/**` | `PASS` | Diukur dari halaman Beranda yang sudah tenang. Percobaan pertama mencatat satu `GET lab-examinations/by-order/<GUID asli>` milik halaman detail sebelumnya yang masih memuat — bukan dari layar Wadah, yang tidak mungkin mengenal GUID dari token palsu |
| (d) GUID langsung | `by-order` `200`, layar terisi | `PASS` | Pencatat jaringan |
| Penjaga tulis | Setiap permintaan non-GET ke `/v1` digagalkan; **nol** yang tercatat | `PASS` | Log skrip |
| Galat JavaScript halaman | Nol | `PASS` | `pageerror` |
| (e) Rencana wadah tersimpan pada pesanan yang benar | — | `NOT RUN` | Menulis ke basis data dev bersama; tidak diizinkan tanpa persetujuan terpisah |

**Lingkungan uji:** backend lokal `dotnet run --no-build` (Development, https 7184) dengan biner yang
sudah memuat source `171dc314`; frontend `next dev` port 3000 diarahkan `.env.local` ke backend lokal;
basis data dev bersama `QuilvianNewDevYoga`. **Akun:** superadmin sebagai pengganti analis — sandi akun
analis tidak tersedia di sesi ini. Superadmin melewati pemeriksaan izin, sehingga verifikasi ini
membuktikan **penerjemahan token**, bukan izin wadah analis; izin itu sudah terbukti lewat data
(`CAP-P22-14`). Kedua server dimatikan sesudah uji; port 3000, 7184, 5107 bebas.

Uji manual: `PASS` — empat dari lima butir; butir (e) `NOT RUN` dengan alasan di atas.

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/` — PASS (nol kegagalan baru; 8 kegagalan baseline di luar Laboratorium). Uji unit baru tidak ditulis: perubahan berupa wiring hook ke utility yang sudah teruji, bukan logika murni (`test-policy.md`).

**Tidak dijalankan:** AC (e); uji e2e Playwright repository (`npm run test:e2e`) — tidak diminta task;
pembanding sebelum-sesudah di peramban — keadaan rusak sebelumnya dibuktikan dari kode (capability map
rev 7 `CAP-P22-10`), bukan dijalankan ulang.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (a) Dari daftar → detail pesanan → tombol Wadah: wadah dan pemeriksaan pesanan itu tampil | Terpenuhi | Bagian 6 (a) |
| (b) Permintaan jaringan memakai GUID, **bukan** token | Terpenuhi | Bagian 6 (b) |
| (c) Alamat dengan token palsu → pesan tautan tidak valid, nol permintaan `/lab-specimens/**` | Terpenuhi | Bagian 6 (c) |
| (d) Alamat dengan GUID langsung tetap berjalan | Terpenuhi | Bagian 6 (d) |
| (e) Rencana wadah dari layar ini tersimpan pada pesanan yang benar | **Belum dibuktikan** — batas verifikasi | Kode mengirim ID hasil terjemahan (`confirmPlan`), tetapi penulisan sungguhan tidak dijalankan |
| DoD: AC (a)–(d) terbukti di peramban, (e) terbukti atau dicatat sebagai batas | Terpenuhi | Di atas |
| DoD: laporan ini juga menutup butir verifikasi manual `FE-LAB-07` yang dapat dijalankan | Sebagian | Jalan Detail → Wadah dan pemuatan wadah kini terbukti. Keputusan atas wadah (`collect`, `receive`, `accept`, `reject`) menulis ke basis data bersama dan **tidak** dijalankan |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu warning lint baru `react-hooks/set-state-in-effect`, identik dengan pola acuan detail pesanan |
| Masalah yang diketahui | `openPlanDialog` mengosongkan formulir rencana menjadi `{ examinations: [], specimenDescription: "" }` tanpa ruas bahan `EMPTY_MATERIAL_FORM`, berbeda dari keadaan awal `FE-LAB-19`. Di luar cakupan; tidak diubah, perlu diperiksa saat AC (e) dijalankan |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` — Playwright dijalankan lewat skrip di scratchpad, bukan test runner, sehingga `test-results/` tidak tersentuh |
| Interupsi | `NONE` |
| Status Git | Frontend: ` M src/lib/hooks/health-services/laboratory-management/use-lab-specimen-workspace.jsx`. Backend: hanya dokumen di `docs/` |
| Langkah berikutnya | `FE-LAB-47` — *Terima Sampling* dan *Proses Pemeriksaan*; jalan ke layar Wadah kini siap dipakai |
