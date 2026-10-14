# Laporan Perubahan Frontend — `FE-CRM-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-CRM-006` |
| Judul | Iuran di form tingkat |
| Slice | `MVP-1` |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/frontend-roadmap.md` § `FE-CRM-006` |
| Trace | `CRM-DEC-004`; `CRM-OQ-001`; `FR-CRM-002` (`AnnualFeeAmount` tingkat) |
| Contract version | API `v2` bagian Membership Tier, `approved` (Yoga Aji Pratama, 2026-10-13) |
| Wewenang UI | Isian baru ditaruh di grup **Benefit Membership** yang sudah memuat Minimum Belanja. Tanpa referensi visual baru |
| Dependency | `BE-CRM-003` ✅ (`annualFeeAmount` di request/response tingkat, validasi ≥ 0) |
| Klasifikasi | `SMALL`: satu isian di form yang ada, satu helper, satu berkas uji |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`: konstanta dan utils form tingkat, `tests/unit/`. Backend hanya laporan ini serta roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `f1dfd3edd` (branch `YogaV2`) |
| Tanggal | 2026-10-13 |
| Status | Selesai. E2E simpan, detail, dan buka ulang di form perbarui lulus (API tiruan) |

## 1. Keadaan yang ditemukan di awal

- Form tingkat Administrator tidak punya isian iuran, sehingga `AnnualFeeAmount` hanya bisa diisi lewat API.
- Isian uang yang sudah ada (Minimum Belanja) memakai `type: "number"`. Roadmap melarang pola itu, karena membuang pemisah desimal.
- Normalizer desimal Lab (`lab-decimal-input-rules.js`) juga tidak cocok. Ia membaca titik sebagai desimal, sehingga ketikan "2.400.000" tersimpan 2,4.

## 2. Proses bisnis dari sisi pengguna

1. Administrator membuka **Tambah** atau **Perbarui Tingkat Keanggotaan**.
2. Di grup **Benefit Membership**, di bawah Minimum Belanja, ada isian **Nominal Iuran** dengan keterangan "Iuran Rupiah per periode membership. Titik sebagai pemisah ribuan, koma untuk sen. Tingkat bernilai 0 tidak bisa didaftarkan."
3. Administrator mengetik misalnya `2.400.000`. Isian menampilkan `2400000`, dan sistem menyimpan 2.400.000.
4. Saat tingkat dibuka lagi, isian menampilkan `2400000`, dan halaman detail menampilkan **Nominal Iuran** dalam format Rupiah.
5. Ketikan yang tidak bisa dibaca (misalnya hanya `,5`) atau yang melebihi 999.999.999.999 ditolak dengan pesan di bawah isian sebelum dikirim.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend, `membership-tier-editor-config.jsx`,
`membership-tier-grouped-form.config.jsx`, `membership-tier-editor-utils.js`,
`membership-tier-detail-utils.js`, `membership-tier-display-utils.js`,
`use-master-data-membership-tier-editor.jsx` (memastikan daftar isian diambil dari konfigurasi
statis, bukan metadata backend), `base-form-control.jsx` (dukungan `normalizeValue`), dan
`lab-decimal-input-rules.js`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/administrator/master-data/membership-tiers/membership-tier-currency-input.js` | Baru. `normalizeRupiahInput` (digit + satu koma, sen maksimal dua digit, titik dibuang), `parseRupiahInput`, `toRupiahInputText` (angka API → teks isian, `2400000.5` → `2400000,5`), `validateRupiahInput` |
| `src/lib/constants/.../membership-tier-editor-config.jsx` | Isian `annualFeeAmount` ("Nominal Iuran", teks) di form buat dan ubah, bawaan `"0"` |
| `src/lib/constants/.../membership-tier-grouped-form.config.jsx` | Konfigurasi isian: `type: "text"`, `inputMode: "decimal"`, `normalizeValue: normalizeRupiahInput`, keterangan. Masuk grup `benefit`. Deskripsi grup menyebut nominal iuran |
| `src/utils/.../membership-tier-editor-utils.js` | Pemetaan detail → form memakai `toRupiahInputText`. Payload memakai `parseRupiahInput` (angka, kosong → 0). Validasi memakai `validateRupiahInput` |
| `src/utils/.../membership-tier-detail-utils.js` | Label "Nominal Iuran", urutan setelah Minimum Belanja, format Rupiah |
| `tests/unit/crm-membership-tier-annual-fee.test.mjs` | Baru, 6 uji |

Akhir baris CRLF dipertahankan di semua berkas yang berubah, dan berkas baru di folder utils juga
ditulis CRLF.

### 3.3 Kepatuhan arsitektur frontend

Memakai form grup, hook editor, dan dukungan `normalizeValue` di `BaseFormControl` yang sudah
ada. Helper ditaruh di folder utils tingkat yang sama. Tidak ada komponen atau state baru.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Validasi | Pesan "Nominal Iuran tidak valid." atau "Nominal Iuran melebihi batas maksimum yang diizinkan." di bawah isian |
| Kosong | Dikirim 0. Keterangan isian menjelaskan bahwa tingkat bernilai 0 tidak bisa didaftarkan |
| Memuat, gagal, tanpa hak akses | Tidak berubah (ditangani form tingkat yang ada) |

## 5. Endpoint yang dikonsumsi

#### Administrator / Master Data / Membership Tier

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/administrator/master-data/membership-tiers` | Simpan tingkat baru dengan `annualFeeAmount` | `MembershipTier : Create` |
| `PUT` | `/v1/administrator/master-data/membership-tiers/{id}` | Perbarui tingkat dengan `annualFeeAmount` | `MembershipTier : Update` |
| `GET` | `/v1/administrator/master-data/membership-tiers/{id}` | Memuat `annualFeeAmount` kembali ke form dan detail | `MembershipTier : Read` |

Tidak ada endpoint baru. Ketiganya sudah menerima dan mengembalikan `annualFeeAmount` sejak `BE-CRM-003`.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Uji: isian ada di form buat dan ubah, di grup benefit, berupa teks desimal | Lulus | `PASS` | `crm-membership-tier-annual-fee.test.mjs` |
| Uji: ketik `2.400.000` → tersimpan `2400000` → payload `2400000` (angka). `1500,75` → 1500,75. Kosong → 0 | Lulus | `PASS` | idem |
| Uji: nilai API 2400000 dan 2400000,5 tampil kembali di form dan bolak-balik tanpa berubah | Lulus | `PASS` | idem |
| Uji: detail menampilkan "Nominal Iuran" berformat Rupiah (`2.400.000`) | Lulus | `PASS` | idem |
| Uji: ketikan tak terbaca dan nilai melebihi batas ditolak | Lulus | `PASS` | idem |
| `npx eslint` berkas yang berubah | Bersih | `PASS` | Keluaran perintah |
| `npm run lint:errors` | Bersih | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2.787 lulus, 29 gagal | `UNRELATED EXISTING ISSUE` (29) | Sama dengan laporan `FE-CRM-001` (modul non-CRM) |
| `npm run build` | Berhasil ("Compiled successfully in 50s") | `PASS` | Log build |
| E2E: ketik `2.400.000` → isian `2400000` → `POST` membawa `annualFeeAmount: 2400000` → detail menampilkan Rp 2.400.000 → **Perbarui** menampilkan `2400000` | Lulus | `PASS` | `tests/e2e/crm-membership-registration-screen.spec.mjs` (dijalankan saat `FE-CRM-003`, API tiruan, nol tulis) |

E2E dijalankan dengan API tiruan, sehingga tidak ada data tingkat yang ditulis ke devYoga.
Penyimpanan desimal di database sudah dibuktikan di `BE-CRM-003` (`numeric(18,2)`).

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Nilai 2.400.000 tersimpan dan tampil kembali tanpa terpotong | Terpenuhi | Uji unit + E2E |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan teknis | Nominal Rupiah memakai kebiasaan Indonesia (titik = ribuan, koma = sen). Ketikan gaya Inggris `2400000.50` akan terbaca 240000050. Keterangan isian menjelaskan aturan ini |
| Masalah yang diketahui | Minimum Belanja masih `type: "number"` dan membuang pemisah. Di luar cakupan task ini, dan tidak diubah |
| Dependency backend | `BE-CRM-003` ✅ |
| Perubahan sampingan | `NONE`. Build menulis `.next/standalone` (keluaran build biasa) |
| Interupsi | `NONE` |
| Status Git | ` M` editor-config, grouped-form config, editor-utils, detail-utils. `??` `membership-tier-currency-input.js`, `tests/unit/crm-membership-tier-annual-fee.test.mjs`. Belum di-commit |
| Langkah berikutnya | `FE-CRM-003` (form pendaftaran: cari pasien, pilih tingkat beserta iuran) |
