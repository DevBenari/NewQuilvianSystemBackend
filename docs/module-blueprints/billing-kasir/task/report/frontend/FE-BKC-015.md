# Laporan Perubahan Frontend — `FE-BKC-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-015` |
| Judul | Badge coverage dan disclaimer pada form testing (pasien asuransi) |
| Slice | Entri manual katalog tarif + coverage per item (`BKC-DEC-059`–`062`, amendment 2 September 2026) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BKC-015` (baris 217–230) |
| Trace | `BKC-DEC-060`; `FR-BKC-005`, `006`; `UAT-03`, `04` |
| Contract version | `GET catalog-charges/coverage-preview` (`BIL-API-0.4` amendment, sudah diimplementasikan backend `BE-BKC-020`, reuse penuh, tidak ada perubahan kontrak dari task ini) |
| Wewenang UI | Tambah badge status coverage per opsi dropdown tarif (khusus pasien asuransi) dan disclaimer wajib. Untuk itu, **`FilterSelect` diperluas** dengan prop opsional `renderOption` (lihat § 3.3 — keputusan gate, disetujui pengguna eksplisit sebelum implementasi) |
| Dependency | `BE-BKC-020` (endpoint preview) — selesai, build belum diverifikasi pengguna (lihat `task/report/backend/BE-BKC-020.md`). `FE-BKC-014` (dropdown tarif dasar) — selesai pada sesi yang sama, source ada tapi verifikasi ujung-ke-ujung tertunda `BE-BKC-FIX-001` (lihat `task/report/frontend/FE-BKC-014.md`) |
| Klasifikasi | `MEDIUM` — skor 7 (repo 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 2/Kompleks, kontrak API 1, database 0, keamanan 0, UI/workflow 1) |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/features/base-features/` (perluasan `FilterSelect`/`BaseSelectField`/`BaseEditorField`, disetujui pengguna), `src/components/view/.../create-manual/`, `src/lib/hooks/.../billing-invoices/`, `src/lib/state/slice/.../billing-invoice-slice.jsx`, `src/style/` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | (working tree belum di-commit — lihat § 8 Status Git) |
| Commit backend yang dijadikan rujukan | `fec3579` (`NewQuilvianSystemBackend`) |
| Tanggal | 3 September 2026 |
| Status | Source lengkap. **Sebagian terverifikasi hidup** (login sungguhan): disclaimer tampil/tersembunyi dengan benar sesuai cara bayar kunjungan, tidak ada regresi pada dropdown kunjungan. **Badge itu sendiri belum bisa diverifikasi visual** — dropdown tarif masih kosong karena `BE-BKC-FIX-001` (bug backend yang sama dengan `FE-BKC-014`) belum di-build/restart |

---

## 1. Keadaan yang ditemukan di awal

`FE-BKC-014` (sesi yang sama) baru saja menambahkan dropdown tarif searchable, tapi tanpa indikasi
apa pun soal apakah tarif itu tercover asuransi pasien — kasir harus memilih tarif dulu, baru tahu
statusnya lewat langkah terpisah. Untuk pasien asuransi, ini berisiko: kasir bisa memilih tarif yang
ternyata tidak tercover tanpa sadar sampai tahap berikutnya.

`FilterSelect` (dipakai `BaseSelectField` di seluruh aplikasi, ~30 pemakaian) hanya merender teks
polos per opsi (`<span>{option.label}</span>`) — tidak ada mekanisme render konten custom per baris
opsi. Endpoint preview-nya sendiri (`BE-BKC-020`) sudah ada dan siap dipakai.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir/penguji, pada form "Buat Invoice Manual", khusus saat kunjungan yang dipilih
berpenjamin **asuransi** (bukan tunai).

**Pemicu**: kasir sudah memilih kunjungan pasien asuransi, lalu membuka dropdown "Tarif Layanan".

**Langkah**:

1. Begitu kunjungan pasien asuransi dipilih, teks bantuan di bawah dropdown "Tarif Layanan"
   otomatis bertambah kalimat disclaimer: *"Badge status coverage di setiap opsi bersifat perkiraan
   — angka final dihitung ulang saat tagihan diproses di Menu Pembayaran."* — disclaimer ini
   **selalu** muncul bersamaan dengan kemungkinan munculnya badge, tidak pernah terpisah (digabung
   jadi satu kalimat description, bukan elemen terpisah yang bisa gagal muncul sendiri-sendiri).
2. Kasir mengetik kata kunci pencarian tarif. Untuk **setiap baris opsi** yang muncul, sistem
   meminta preview coverage tarif itu untuk kunjungan yang dipilih (kuantitas mengikuti field Qty
   saat itu, default 1) — ditampilkan sebagai kerangka pemuatan (skeleton) singkat, lalu badge:
   **Tercover** (hijau), **Tercover Sebagian** (kuning), **Tidak Tercover** (merah), atau
   **Menunggu Keputusan** (biru — kasus rule yang status coverage-nya sendiri belum diputuskan,
   beda dari rule Covered yang cuma butuh approval administratif).
3. Preview yang sama (tariff yang sama) tidak diminta ulang selama sesi form masih berjalan —
   membuka-tutup dropdown yang sama tidak memanggil API lagi.
4. Bila preview gagal dimuat untuk satu baris tertentu (mis. timeout jaringan), baris itu **hanya
   tidak menampilkan badge** — tetap bisa dipilih dan disubmit seperti biasa (fail-open, tidak
   pernah memblokir).
5. Untuk kunjungan pasien **tunai**, tidak ada badge maupun disclaimer sama sekali pada dropdown
   tarif — perilakunya identik dengan `FE-BKC-014` sebelum task ini.

**Jalur tidak normal**: preview API gagal (lihat langkah 4); dropdown tarif kosong sama sekali
(lihat § 8 *Dependency backend* — bukan sesuatu yang diperkenalkan task ini).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BKC-015`;
`src/components/features/base-features/{filter-select.jsx, base-form-control.jsx,
base-editor-field.jsx, base-editor-form.jsx, status-badge.jsx, summary-grid.jsx}`;
`src/style/components/features/base-features/{status-badge.css, base-data-components.module.css}`
(pola skeleton existing); `src/lib/hooks/health-services/billing-management/billing-invoices/
{use-create-manual-invoice.js, billing-invoice-constants.js}`; `src/lib/state/slice/.../
billing-invoice-slice.jsx`; `task/report/backend/BE-BKC-020.md` (bentuk response
`CatalogChargeCoveragePreviewResponse`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/features/base-features/filter-select.jsx` | Prop opsional baru `renderOption(option, {isSelected, index})` — dipakai menggantikan `<span>{option.label}</span>` bawaan HANYA bila diisi konsumen. Default (tidak diisi) = perilaku lama persis |
| `src/components/features/base-features/base-form-control.jsx` | `BaseSelectField` meneruskan `renderOption` ke `FilterSelect` |
| `src/components/features/base-features/base-editor-field.jsx` | Meneruskan `selectProps?.renderOption` (dari `optionHandlerMap`) ke `BaseSelectField` |
| `src/style/components/features/base-features/status-badge.css` | 2 kelas tone baru: `.region-status-warning` (kuning, token `--color-warning`) dan `.region-status-pending` (biru, token `--color-info`) — melengkapi `.region-status-active`/`.region-status-inactive` yang sudah ada, dipakai config badge sejak sesi sebelumnya (`BILLING_ITEM_COVERAGE_BADGE_CONFIG`) tapi belum pernah benar-benar bergaya |
| `src/lib/hooks/.../billing-invoice-constants.js` | `CATALOG_CHARGE_COVERAGE_BADGE_CONFIG` baru — 4 status (`covered`/`partiallycovered`/`notcovered`/`needapproval`) dipetakan ke label dan tone |
| `src/lib/state/slice/.../billing-invoice-slice.jsx` | Thunk baru `getCatalogChargeCoveragePreview` (`GET catalog-charges/coverage-preview`); state `coveragePreviewsByTariffId` (cache per tariffId, dikunci `action.meta.arg.tariffId`); selector `selectCatalogChargeCoveragePreview` |
| `src/components/view/.../create-manual/tariff-coverage-option.jsx` (baru) | Komponen satu baris opsi dropdown tarif — memuat preview-nya sendiri saat pertama dirender (untuk pasien asuransi), merender skeleton lalu badge, fail-open saat gagal |
| `src/style/health-services/billing-management/tariff-coverage-option.module.css` (baru) | Layout baris opsi (label + badge sejajar) |
| `src/lib/hooks/.../use-create-manual-invoice.js` | `isInsurance` (dari `paymentType` kunjungan terpilih); `selectFieldPropsMap` (rename dari `encounterSelectProps`, kini juga membawa `renderOption` untuk field `tariffId`) |
| `src/components/view/.../create-manual/create-manual-invoice-view.jsx` | Field `tariffId`: `description` bertambah kalimat disclaimer saat `isInsurance` true |

### 3.3 Kepatuhan arsitektur frontend

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Render badge per baris opsi tarif | `EXTEND` (`FilterSelect`) | Tidak ada mekanisme render-custom-per-opsi sebelumnya di base component manapun. Prop `renderOption` baru bersifat **opt-in murni** — ~30 pemakaian `FilterSelect` lain di seluruh aplikasi tidak memanggilnya, sehingga perilaku default tidak berubah sama sekali |
| Visual badge (Tercover/Sebagian/Tidak Tercover/Menunggu) | `REUSE` | `StatusBadge` + `config` map — pola identik `BILLING_ITEM_COVERAGE_BADGE_CONFIG` yang sudah ada. 2 tone CSS baru (`warning`/`pending`) melengkapi tone yang SUDAH DIRUJUK config lama tapi belum pernah bergaya — bukan menambah bahasa visual baru |
| Skeleton loading per opsi | `REUSE` | `baseStyles.skeletonLineSmall` dari `base-data-components.module.css` — mekanisme shimmer yang sudah ada (dipakai `summary-grid.jsx`), diimpor ulang, tidak menulis animasi baru |
| Baris opsi tarif (label + badge) | `COMPOSE` | Komponen feature-scoped baru (`tariff-coverage-option.jsx`) merangkai `StatusBadge` + logika fetch-nya sendiri per opsi — bukan base component baru, hidup di folder fitur (`create-manual/`), sama seperti `encounter-context-card.jsx` yang sudah ada di folder yang sama |

**`UI GATE`**: satu elemen berstatus `EXTEND` (`renderOption` pada `FilterSelect`) — **ditanyakan
eksplisit ke pengguna sebelum implementasi dimulai**, dengan 2 opsi bernomor (extend `FilterSelect`
vs. teks suffix tanpa badge visual), rekomendasi dan konsekuensinya dijelaskan. Pengguna memilih
opsi extend. Elemen lain seluruhnya `REUSE`/`COMPOSE`, tidak menunggu keputusan.

Alur dependensi: `TariffCoverageOption` (baru) → thunk `getCatalogChargeCoveragePreview` →
`InstanceAxios`, dan dirender lewat `optionHandlerMap.tariffId.renderOption` → `BaseSelectField` →
`FilterSelect` (perluasan). Tidak ada state/HTTP paralel — cache preview memakai Redux slice yang
sudah menampung state `billingInvoice` lain, bukan store baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Skeleton shimmer kecil menggantikan posisi badge, per baris opsi, selama preview tarif itu belum selesai dimuat |
| Kosong | `NOT APPLICABLE` untuk badge itu sendiri — bila tarifnya tidak tercover, itu direpresentasikan sebagai badge "Tidak Tercover", bukan keadaan kosong |
| Gagal | Baris opsi tetap tampil normal tanpa badge (fail-open) — tidak ada pesan error per baris, supaya tidak mengganggu kasir memilih tarif lain yang preview-nya berhasil |
| Tanpa hak akses | `NOT APPLICABLE` — mengandalkan penanganan 401/403 global yang sama dengan `FE-BKC-014` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/catalog-charges/coverage-preview` | Preview coverage satu tarif untuk badge per opsi dropdown (khusus pasien asuransi) | `BillingInvoice : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` (9 berkas berubah + 2 berkas baru) | Berhasil tanpa error | `PASS` | Keluaran perintah kosong |
| Render halaman langsung, pilih kunjungan **asuransi** (login sungguhan) | Halaman render sempurna; kartu konteks kunjungan benar (AdMedika, Asuransi); deskripsi field "Tarif Layanan" **bertambah kalimat disclaimer** persis seperti yang dirancang | `PASS` | Pemeriksaan DOM langsung: `text=Badge status coverage` ditemukan setelah memilih kunjungan asuransi |
| Ganti ke kunjungan **tunai** (regresi) | Disclaimer **hilang** kembali; dropdown kunjungan tetap berfungsi seperti sebelum task ini | `PASS` | `text=Badge status coverage` count = 0 setelah memilih kunjungan tunai |
| Buka dropdown tarif untuk pasien asuransi, cari "a" | Dropdown tetap kosong (0 opsi) | `EXISTING / ENVIRONMENT ISSUE` | Sama persis dengan temuan `FE-BKC-014` — `BE-BKC-FIX-001` belum di-build/restart, bukan regresi baru dari task ini |
| Badge (Tercover/Sebagian/Tidak Tercover/Menunggu) tampil sungguhan per opsi | **Belum bisa diverifikasi visual** — terhalang dropdown tarif kosong (baris di atas) | `NOT RUN` | Lihat § 8 *Dependency backend* |
| Fail-open saat API preview error | **Belum bisa diverifikasi** — sama alasannya, tidak ada opsi tarif untuk memicu preview sama sekali | `NOT RUN` | Lihat § 8 |

Uji manual: `PARTIAL` — regresi dan logika kondisional disclaimer (inti dari perubahan hook/view)
sudah dibuktikan langsung lewat klik-coba nyata. Bagian yang butuh opsi tarif sungguhan (badge
visual, skeleton, fail-open) terhalang bug backend yang sama dengan `FE-BKC-014`, belum bisa
diklik-coba sampai `BE-BKC-FIX-001` di-build/restart.

**Tidak dijalankan:** `npm run build`/`next build` penuh; component test (sesuai instruksi
eksplisit pengguna — **tanpa file test** untuk task ini, konsisten juga dengan `test-policy.md`
yang menyatakan test otomatis opsional dan repo ini tidak memakai Jest).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-03`, `04` — kasir melihat status coverage per tarif sebelum memilih, dengan disclaimer perkiraan | Source terpenuhi; **disclaimer terverifikasi hidup**, badge visual **belum** (terhalang backend) | Lihat § 6 |
| Badge tersembunyi total untuk pasien tunai | Terpenuhi, **terverifikasi hidup** | `isInsurance=false` → `TariffCoverageOption` tidak merender apa pun selain label; dibuktikan lewat regresi kunjungan tunai di § 6 |
| Preview tidak dipanggil per keystroke | Terpenuhi (source) — preview dipicu oleh MOUNT baris opsi (hasil pencarian yang sudah di-debounce `useSelectResource`, 350ms), bukan oleh event keystroke itu sendiri | Kode `TariffCoverageOption` — `useEffect` bergantung pada `tariffId`/`preview`, bukan search term |
| DoD: Disclaimer tampil setiap kali badge tampil | Terpenuhi by design — keduanya dikendalikan variabel `isInsurance` yang sama, digabung jadi satu `description` (tidak mungkin salah satu tampil tanpa yang lain) | Kode `create-manual-invoice-view.jsx` § 3.2 |
| tests/lint/build lulus | Lint: terpenuhi. Test: sengaja tidak dibuat (instruksi eksplisit pengguna). Build (`next build`) penuh: **belum dijalankan** | Lihat § 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Perluasan `FilterSelect` (`renderOption`) menyentuh komponen yang dipakai luas (~30 lokasi lain) — sudah dirancang opt-in murni dan lint bersih di seluruh berkas terkait, tapi **verifikasi visual langsung terhadap konsumen `FilterSelect` LAIN (di luar layar ini) belum dilakukan** — risikonya rendah (default behavior dijaga eksplisit), tapi disebutkan di sini apa adanya |
| Masalah yang diketahui | 1) Cache preview per-tariffId tidak bereaksi terhadap perubahan Qty setelah preview pertama diminta (preview memakai kuantitas saat baris itu pertama dirender) — simplifikasi yang disengaja, konsisten dengan sifat "perkiraan" yang memang diberitahu lewat disclaimer, bukan bug. 2) Status `needapproval` (badge "Menunggu Keputusan") adalah tambahan teknis di luar 3 status yang disebut eksplisit di scope roadmap (Tercover/Sebagian/Tidak Tercover) — ditambahkan karena `CoverageStatus="NeedApproval"` adalah state nyata yang bisa dikembalikan `BE-BKC-020` (rule yang status coverage-nya sendiri belum diputuskan, beda dari `IsNeedApproval` administratif yang sudah dihitung "Covered" sejak `BE-BKC-021`) — tanpa ini, state itu akan tampil sebagai badge kosong tanpa penjelasan |
| Dependency backend | **`BE-BKC-FIX-001`** (perbaikan filter scope tarif, sama dengan `FE-BKC-014`) — source sudah ditulis, backend belum di-build ulang/restart. Ini SATU-SATUNYA penghalang tersisa untuk verifikasi visual badge/skeleton/fail-open task ini |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat sama sekali (instruksi eksplisit pengguna) — verifikasi hidup dijalankan lewat skrip Playwright yang dieksekusi langsung dari `stdin` (`node --input-type=module`), **tanpa satu pun file ditulis ke working tree** — dikonfirmasi `git status --short` sebelum dan sesudah identik kecuali source task ini |
| Interupsi | Sebelum implementasi, sesi ini berhenti dan mengajukan pertanyaan konfirmasi eksplisit ke pengguna soal pendekatan render-badge-per-opsi (`extend FilterSelect` vs. teks suffix) — pengguna memilih extend. Dicatat di § 3.3 |
| Status Git | `working tree QuilvianSystemFrontendDev`. Modified: `src/components/features/base-features/{base-editor-field.jsx, base-form-control.jsx, filter-select.jsx}`, `src/components/view/.../create-manual/create-manual-invoice-view.jsx`, `src/lib/hooks/.../{billing-invoice-constants.js, use-create-manual-invoice.js}`, `src/lib/hooks/select/health-service/health-service-select-resources.js`, `src/lib/state/slice/.../billing-invoice-slice.jsx`, `src/style/components/features/base-features/status-badge.css`. Untracked (baru): `src/components/view/.../create-manual/tariff-coverage-option.jsx`, `src/style/health-services/billing-management/tariff-coverage-option.module.css`. Belum staged/commit |
| Langkah berikutnya | 1) **Build ulang dan restart backend** (`BE-BKC-FIX-001` — satu langkah ini membuka verifikasi visual penuh `FE-BKC-014` DAN `FE-BKC-015` sekaligus). 2) Setelah itu: klik-coba badge sungguhan (3+1 status, warna, label), skeleton loading, dan fail-open (mis. matikan sebentar backend saat dropdown terbuka). 3) Pertimbangkan verifikasi visual cepat pada 2-3 layar lain yang memakai `FilterSelect` untuk memastikan perluasan `renderOption` benar-benar tidak mengubah apa pun di sana (lihat *Peringatan* di atas). 4) Lanjut `FE-BKC-016` (subtotal Mandiri/Asuransi terpisah di Menu Pembayaran) — tidak bergantung apa pun, boleh dikerjakan kapan saja |
