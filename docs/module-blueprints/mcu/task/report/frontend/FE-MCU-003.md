# Laporan Perubahan Frontend — `FE-MCU-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-MCU-003` |
| Judul | Layar kelola paket |
| Slice | `MVP-0` (pasangan `BE-MCU-005` ✅) |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *`FE-MCU-003`* |
| Trace | `FR-MCU-002`, `003`; `UAT-01`, `UAT-02`; brief layar [`05-screen-brief.md`](../../../05-screen-brief.md) (`MCU-FE-002`) |
| Contract version | `MCU-API-v1` grup `Mcu Package` — `approved` 2026-10-21; `VAL-MCU-01`..`08` |
| Wewenang UI | Brief layar `FE-MCU-000` ✅ (keputusan pemilik 2026-10-21) |
| Dependency | `FE-MCU-000` ✅, `FE-MCU-002` ✅, `BE-MCU-005` ✅ |
| Klasifikasi | `LARGE` — satu repository, berkas baru ±15 (route, view, hook, utilitas, konstanta, uji), logika form komponen bertingkat, kontrak yang sudah ada, tanpa database, keamanan berkaitan (izin per aksi `McuPackage`), UI besar |
| Task mode | `FRONTEND` |
| Target tulis | Frontend: route, view, hook, utilitas, konstanta paket MCU, menu, satu resource select, uji unit + E2E. Backend: laporan ini; tanda roadmap/traceability. devYoga: data uji UAT (izin user) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `d0888bf6f` (branch `YogaV2`) |
| Commit backend yang dijadikan rujukan | `a3b63b50` (branch `yoga`) |
| Tanggal | 2026-10-23 (implementasi, uji otomatis, UAT baca) — 2026-10-27 (UAT tulis, laporan) |
| Status | Selesai — `UAT-01` dan `UAT-02` terbukti di E2E API-tiruan **dan** terhadap backend lokal dengan data sungguhan |

---

## 1. Keadaan yang ditemukan di awal

- Lapisan data `FE-MCU-002` ✅ sudah tersedia (slice `mcuPackage`, `mcuReferenceData`, service 19 endpoint).
- Belum ada route, menu, maupun layar MCU di frontend.
- Backend menolak tarif yang bukan tarif paket aktif (`VAL-MCU-03`). Daftar `GET tariffs` menerima
  `isPackageTariff`, sedangkan `GET tariffs/options` tidak.
- devYoga belum punya satu pun tarif ber-`IsPackageTariff`, maupun paket MCU.

---

## 2. Proses bisnis dari sisi pengguna

1. **Menu** *Medical Check-Up › Master Data › Paket MCU* tampil hanya bagi pemegang `McuPackage : Read`.
2. **Daftar Paket MCU** — kolom sesuai brief, penyaring status dan cari. Nonaktifkan/aktifkan langsung dari baris
   (`PATCH status`), dan baris diperbarui tanpa memuat ulang daftar. Gagal muat menampilkan pesan + *Coba lagi*;
   `403` menampilkan halaman akses ditolak.
3. **Buat Paket MCU** — kode, nama, tarif paket (harga umum), keterangan; tabel komponen (Laboratorium / Radiologi +
   modalitas / Pemeriksaan MCU), nilai pengembalian dan jasa (isian Rupiah `MCU-FE-004`: titik = pemisah ribuan),
   persyaratan per komponen. Total pengembalian berjalan tampil terhadap harga umum. Bila melebihi, layar
   memperingatkan tetapi tetap mengirim, dan **server yang memutuskan**.
4. **Simpan berhasil** → pindah ke **Ubah Paket MCU** (URL berupa slug, tanpa UUID), tempat bagian **Harga
   Perusahaan** tersedia.
5. **Simpan ditolak** → kalimat server tampil apa adanya dalam *Paket MCU belum disimpan*; semua isian tetap.
6. **Harga Perusahaan** — tambah (perusahaan penjamin + tarif paket khusus + masa berlaku) dalam modal; penolakan
   `VAL-MCU-05` tampil di modal. *Akhiri* mengirim tanggal akhir.
7. **Tanpa `McuPackage : Update`** halaman Ubah bersifat baca-saja (tanpa simpan, tambah, Akhiri); tanpa
   `McuPackage : Create` halaman Buat menampilkan akses ditolak.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah (frontend)

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/medical-checkup-management/master-data/packages/page.jsx`, `packages-client.jsx`, `create/page.jsx`, `[packageId]/page.jsx` (baru) | Route daftar, buat, ubah (slug → id) |
| `src/components/view/…/medical-checkup-management/master-data/mcu-packages/master-data-mcu-package-view.jsx` (baru) | Daftar paket |
| `…/mcu-packages/add/mcu-package-form-view.jsx`, `mcu-package-component-table.jsx`, `mcu-package-company-price-section.jsx` (baru) | Form paket, tabel komponen, bagian harga perusahaan |
| `src/lib/hooks/…/mcu-packages/use-master-data-mcu-package.jsx`, `…-editor.jsx`, `…-company-prices.jsx` (baru) | Logika daftar, editor, harga perusahaan |
| `src/utils/…/mcu-packages/mcu-package-utils.js` (baru) | Pemetaan detail ↔ form ↔ payload, total, validasi data paket, keadaan harga |
| `src/lib/constants/…/mcu-packages/mcu-package-constants.js` (baru) | Kolom, jenis komponen, pesan |
| `src/lib/hooks/select/health-service/health-service-select-resources.js` | Resource `packageTariffs` (`GET tariffs` dengan `isPackageTariff`, `isActive`) |
| `src/utils/menu-sidebar/menu-items.jsx` | Grup *Medical Check-Up › Master Data › Paket MCU* (`McuPackage : Read`) |
| `tests/unit/mcu-package-screen-rules.test.mjs` (baru) | 7 uji aturan layar |
| `tests/e2e/mcu-package-screen.spec.mjs` (baru) | 12 skenario API-tiruan |

### 3.2 Kepatuhan arsitektur frontend

- Alur `view → hook → slice (FE-MCU-002) → service → InstanceAxios`; tidak ada Axios di view/page.
- Pilihan memakai `BaseSelectField`/`FilterSelect`/`ResourceFilterSelect` yang sudah ada. Perusahaan penjamin
  semula memakai select `administrator` (`/company-guarantors/options`); **direvisi 2026-10-27** ke
  `/company-guarantors/admin/options` — lihat §8 *Revisi*.
- Tidak ada dependency atau pola state baru.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Penanda muat daftar, detail, dan tindakan; tombol simpan terkunci saat mengirim (klik ganda tidak mengirim dua kali) |
| Kosong | *Data paket MCU tidak ditemukan…* |
| Gagal muat | Pesan + *Coba lagi* |
| Ditolak server | Kalimat server apa adanya; isian tetap |
| Tanpa hak akses | Halaman akses ditolak (`403`/tanpa `Create`); Ubah baca-saja tanpa `Update` |

---

## 5. Endpoint yang dikonsumsi

Base URL: `/v1/health-services/medical-checkup-management` — `GET/POST /packages`, `GET/PUT /packages/{id}`,
`PATCH /packages/{id}/status`, `POST /packages/{id}/company-prices`, `PATCH /packages/{id}/company-prices/{priceId}/end`
(rincian di [laporan `FE-MCU-002`](FE-MCU-002.md) §5). Pilihan: `GET /examination-types`,
`GET /v1/health-services/master-data/tariffs` (`isPackageTariff=true&isActive=true`),
`GET /v1/health-services/master-data/procedures/options`, modalitas Radiologi,
`GET /v1/administrator/master-data/company-guarantors/admin/options` (`CompanyGuarantor : Read`; semula `/options`).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji unit MCU (`mcu-package-screen-rules`, `mcu-master-data-slices`, `mcu-shared-rules`) | 25/25 lulus (diulang 2026-10-27) | `PASS` | `# pass 25`, `# fail 0` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 3.614 uji: 3.568 lulus, 46 gagal — daftar nama kegagalan **identik** dengan baseline (semua di luar MCU) | `EXISTING / ENVIRONMENT ISSUE` | `diff` daftar kegagalan kosong |
| `npm run lint:errors` | Exit 0 | `PASS` | Keluaran perintah |
| `npm run build` | Exit 0: *Compiled successfully in 59s*, 519 halaman statis | `PASS` | Keluaran build |
| E2E API-tiruan `tests/e2e/mcu-package-screen.spec.mjs` | 12/12 lulus (menu ×2, daftar ×3, `UAT-01`, `UAT-02`, buat ×2, ubah ×2, harga perusahaan) | `PASS` | `12 passed (20.7s)` |
| UAT baca, backend lokal + devYoga | Daftar, pilihan tarif paket/Lab/Rad/modalitas/jenis Pemeriksaan MCU termuat; nol tulis; nol GET gagal | `PASS` | Log skrip UAT |
| **`UAT-02` sungguhan** — Hemoglobin Rp 240.000 + Pemeriksaan Fisik Rp 150.000 pada tarif Rp 350.000 | Layar memperingatkan *Rp 390.000 dari Rp 350.000*; `POST` → `400` `VAL-MCU-04` *"Total nilai pengembalian Rp 390.000 melebihi harga paket Rp 350.000."* tampil apa adanya; isian tetap (`UJI-MCU-BASIC`, 240.000, 150.000); tetap di Buat Paket; paket tidak tercipta | `PASS` | Log skrip + tangkapan layar |
| **`UAT-01` sungguhan** — paket 4 komponen | `POST` → `200`; body: Hemoglobin 60.000; Glukosa Darah Sewaktu 40.000 + persyaratan konfirmasi; THORAX PA + LATERAL / Digital Radiography 150.000 + jasa 25.000; Pemeriksaan Fisik 40.000 + jasa 50.000 (total 290.000 dari 350.000). Halaman Ubah terbuka dengan URL slug tanpa UUID; `GET` detail mengembalikan keempat komponen persis | `PASS` | Log skrip + tangkapan layar |
| Harga perusahaan sungguhan | `POST company-prices` → `200`; baris *PT PLN Persero · UJI Paket MCU Basic · Rp 350.000 · 27-10-2026 – tanpa batas · Berlaku* | `PASS` | Log skrip + tangkapan layar |
| Pagar tulis UAT | Hanya tiga tulis lewat layar (`POST packages` 400, `POST packages` 200, `POST company-prices` 200) + satu `POST tariffs` lewat API; GET non-200 nihil | `PASS` | Log skrip |

`MANUAL TEST: PASS` — UAT sungguhan lewat peramban (Playwright) terhadap backend lokal.
`AUTOMATED TEST: npm run test:unit — FAIL (46 kegagalan existing di luar MCU, identik dengan baseline; 25/25 uji MCU PASS)`.

### Data uji yang ditulis ke devYoga (izin user)

| Data | Nilai |
| --- | --- |
| Tarif paket | `TF-RSMMC-00452` *UJI Paket MCU Basic* Rp 350.000, kategori `OTHER`, `IsPackageTariff`, id `2a295d4b-dd8e-470f-95f5-38cc473b9648` |
| Paket MCU | `UJI-MCU-BASIC` *UJI Paket MCU Basic (UAT FE-MCU-003)*, aktif, 4 komponen, id `1c6626ed-0dc1-4557-87b0-bd30ac60c346` |
| Harga perusahaan | PT PLN Persero, tarif `TF-RSMMC-00452`, mulai 2026-10-27 tanpa batas, id `431c66fc-dd6b-4c7e-b3a5-b51a20763c60` |

Data ini dapat dipakai ulang oleh uji booking/order MCU berikutnya (`FE-MCU-008`, `009`).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `UAT-01` — admin membuat paket 4 komponen | Terpenuhi | §6 (E2E + sungguhan) |
| `UAT-02` — Rp 390.000 pada paket Rp 350.000 → pesan server apa adanya, isian tidak hilang | Terpenuhi | §6 (E2E + sungguhan) |
| DoD: kriteria terbukti | Terpenuhi | §6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tangkapan layar halaman penuh memperlihatkan footer tetap menimpa judul tabel komponen; itu efek tangkapan halaman penuh, bukan tata letak saat digulir |
| Masalah yang diketahui | Resource select bersama `companyGuarantors` (`health-service-select-resources.js`, bawaan lama, di luar task ini) menunjuk `/health-services/master-data/company-guarantors/options` yang tidak ada di backend. Layar paket tidak memakainya (memakai select `administrator`), jadi tidak terdampak; perlu dicek pemiliknya bila ada layar lain yang bergantung padanya |
| Dependency backend | Tidak ada; `BE-MCU-005` ✅ |
| Perubahan sampingan | `NONE` di source. devYoga: tiga baris data uji di atas |
| Revisi 2026-10-27 (saat `FE-MCU-005`) | `/company-guarantors/options` dijaga policy `KioskRead`, yang hanya meloloskan SuperAdmin/Administrator/akun kiosk. Admin MCU biasa akan menerima `403` saat menambah harga perusahaan; UAT memakai superadmin sehingga hal ini tertutup. Atas keputusan pemilik, pilihan perusahaan pindah ke `/company-guarantors/admin/options` (`CompanyGuarantor : Read`) lewat `MCU_COMPANY_GUARANTOR_SELECT_RESOURCE`. Spec E2E kini hanya meniru path admin; 12/12 lulus. UAT baca: `200` dan lima perusahaan termuat. **Jabatan admin MCU perlu diberi `CompanyGuarantor : Read`** di Role Access |
| Interupsi | Sesi 2026-10-23 terhenti di pertanyaan izin tulis data uji; dilanjutkan 2026-10-27 sesudah izin diberikan |
| Status Git (frontend) | ` M menu-items.jsx`, ` M health-service-select-resources.js`, ` M store.jsx` (`FE-MCU-002`); `??` folder route/view/hook/konstanta/utilitas MCU, uji unit dan E2E MCU. Berkas Lab/Radiologi lain yang ` M` bukan bagian task ini |
| Langkah berikutnya | `FE-MCU-004` (jenis Pemeriksaan MCU dan kategori kelayakan) atau `FE-MCU-005` (jadwal, kuota, dokter jaga) — keduanya siap |
