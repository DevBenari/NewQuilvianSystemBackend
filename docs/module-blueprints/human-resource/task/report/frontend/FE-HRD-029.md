# Laporan Perubahan Frontend — `FE-HRD-029`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-029` |
| Judul | Fondasi layar `S-D1` dan layar Pengaturan Perencanaan |
| Slice | `S-D1` · `MVP-7` |
| Roadmap | `roadmap/frontend-roadmap.md` kartu `FE-HRD-029`; dikerjakan lewat `FE-HRD-R12` |
| Trace | `HRD-DEC-061`, `HRD-DEC-071`, `HRD-TF-036`, `HRD-FE-04` (`OPEN`, usulan kerja dipakai), `VR-WFP-40`, `VR-WFP-41` |
| Contract version | API Contract v7 bagian 13 (disetujui pemilik 3 November 2026) |
| Wewenang UI | Nama grup dan butir memakai usulan kerja `HRD-FE-04`; urutan dan ikon `DEV_DISCRETION` |
| Dependency | `FE-HRD-021` ✅, `BE-HRD-031` ✅ [BE] |
| Klasifikasi | `MEDIUM` — layar baru dengan service, state, dan validasi |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Tidak ada satu pun berkas Perencanaan Tenaga Kerja di frontend. Butir menu lama "Kebutuhan Tenaga Kerja" membuka master persyaratan berkas.

---

## 2. Proses bisnis dari sisi pengguna

HR Manager membuka menu Perencanaan Tenaga Kerja → Pengaturan Perencanaan, lalu memilih rumah sakit. Layar menampilkan batas hari cuti panjang dan batas umur permintaan; nilai yang belum diatur tampil kosong dengan keterangan "Belum diatur." Ia mengisi alasan perubahan, lalu menekan Simpan. Contoh di uji: Metropolitan Medical Centre Hospital menampilkan 30 hari dan 14 hari. Bila alasan kosong, muncul "Tuliskan alasan perubahan pengaturan." di bawah isian alasan. Pengguna tanpa hak tidak melihat menunya, dan tombol Simpan hanya untuk pemegang `WorkforcePlanningSetting : Update`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `S-D1` di `frontend-roadmap.md`, `03-frontend-architecture.md` bagian 12, controller dan DTO `WorkforcePlanning`, view referensi Batch Data Awal, `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/workforce-planning/workforce-planning-constants.js` | Konstanta sembilan base URL `workforce-planning`, rute, status batch |
| `src/utils/hr/workforce-planning/workforce-planning-display-utils.js` | Util "data belum tersedia", porsi kerja berkoma, label biaya "estimasi", pesan 409 |
| `src/lib/services/hr/workforce-planning-service.js` | Service grup Setting, Position Formation, dan Initial Formation Batch |
| `src/components/view/hr/workforce-planning/workforce-planning-settings-view.jsx` | Layar Pengaturan Perencanaan |
| `src/app/hr/workforce-planning/settings/page.jsx` | Rute |
| `src/utils/menu-sidebar/menu-items.jsx` | Grup "Perencanaan Tenaga Kerja" dengan tiga butir ber-`requiredPermission`; butir lama menjadi "Persyaratan Berkas Tenaga Kerja" |
| `src/components/view/hr/master-data/workforce-requirement/** dan konstanta/hook-nya` | Label "Kebutuhan Tenaga Kerja" menjadi "Persyaratan Berkas Tenaga Kerja"; `pathname` tidak berubah |

### 3.3 Kepatuhan arsitektur frontend

Alur rute → view → service → `InstanceAxios`. Util tampilan bersama di `src/utils/hr/workforce-planning/` dipakai ulang oleh `FE-HRD-030` dan `031`.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx` dan `src/components/view/hr/leave/leave-admin-view.jsx`

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header halaman | `Hero` | `REUSE` | — |
| Pilih rumah sakit | `ResourceFilterSelect` + `useSelectResource("hospitalSites")` | `REUSE` | Tanpa isian UUID |
| Dua isian angka | `BaseInputField` | `REUSE` | — |
| Alasan perubahan | `BaseTextAreaField` | `REUSE` | — |
| Tombol simpan | `BaseButton variant="primary"` | `REUSE` | — |
| Pesan galat | `InformationAlert` | `REUSE` | — |
| Notifikasi berhasil | `ToastStack` | `REUSE` | — |
| Panel isi | `src/style/hr/workforce-core/workforce-core.module.css` (`panel`) | `REUSE` | CSS Module yang sudah ada |

`UI GATE: PASS — 8 elemen REUSE, 0 COMPOSE, 0 NEW`

Hasil pemeriksa dan grep anti-regresi:

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view S-D1 dan S-B1>
OK — 7 berkas diperiksa, 0 temuan.
$ grep -nE "className=\"[^\"]*\b(flex|grid|gap-[0-9]|p-[0-9]|px-|py-|text-(sm|xs|lg)|bg-|rounded|w-full|items-center|md:|hover:)" <7 berkas>
(kosong)
```

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/hr/workforce-planning/settings` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/01-pengaturan-perencanaan-awal.png` | Setara dengan referensi |
| `/hr/workforce-planning/settings` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/02-pengaturan-perencanaan-terpilih.png` | Nilai dari server |
| `/hr/workforce-planning/settings` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/03-pengaturan-alasan-wajib.png` | Pesan alasan wajib di bawah isian |
| `/hr/attendance` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/20-menu-sidebar.png` | Grup menu baru tampil untuk SuperAdmin |
| `/hr/workforce-planning/settings` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/22-tanpa-hak-pengaturan-perencanaan.png` | Akun tanpa hak: menu tidak tampil, tombol Simpan tidak ada |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Isian nonaktif selama data dimuat |
| Kosong | Isian kosong dengan keterangan "Belum diatur." |
| Gagal | `InformationAlert` merah; 409 menjadi "Dokumen ini sudah diubah orang lain. Muat ulang sebelum menyimpan." |
| Tanpa hak akses | Menu tidak tampil; bila dibuka langsung, pilihan rumah sakit ditolak server (403) dan tombol Simpan tidak tampil |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Workforce Planning / Setting

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-planning/settings` | Membaca pengaturan rumah sakit | `WorkforcePlanningSetting : Read` |
| `PUT` | `/v1/corporate/human-resource/workforce-planning/settings` | Menyimpan pengaturan | `WorkforcePlanningSetting : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| `check-css-usage.mjs` pada 7 berkas view baru | 0 temuan | `PASS` | Keluaran perintah |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`) | Pilih rumah sakit, baca pengaturan, validasi alasan kosong, menu ber-hak, akun tanpa hak | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/01-pengaturan-perencanaan-awal.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/03-pengaturan-alasan-wajib.png`, `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/22-tanpa-hak-pengaturan-perencanaan.png` |

Uji manual: `PASS` dengan akun SuperAdmin dan akun tanpa hak.

**Tidak dijalankan:** Simpan sungguhan tidak ditekan agar tidak mengubah database pengembangan; pesan `VR-WFP-41` dari backend dipetakan ke kedua isian angka dan hanya dibuktikan lewat kode, karena isian angka sudah membatasi nilai ke 1–365.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Grup tampil di bawah Sumber Daya Manusia dan butirnya hanya untuk pemegang `WorkforcePlanningSetting : Read` | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/20-menu-sidebar.png` dan `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/22-tanpa-hak-pengaturan-perencanaan.png` |
| Tidak ada lagi dua butir bermakna "kebutuhan tenaga"; butir lama membuka layar yang sama | Terpenuhi | `menu-items.jsx`: label diganti, `pathname` tetap |
| Dua isian angka dan alasan; nilai belum diisi tampil "belum diatur" | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/02-pengaturan-perencanaan-terpilih.png` |
| Tombol Simpan hanya untuk pemegang `Update`; pesan `VR-WFP-40`/`41` di dekat isian; 409 ditangani | Terpenuhi | Tombol tidak tampil untuk akun tanpa hak; pesan alasan terbukti di layar; `VR-WFP-41` dan 409 lewat kode |
| Util "data belum tersedia" dan label biaya dipakai ulang task `S-D1` berikutnya | Terpenuhi | Dipakai `position-formations-view.jsx` dan `initial-formation-batch-detail-view.jsx` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Uji unit util tampilan direkomendasikan, belum ditulis (opsional menurut `test-policy.md`) |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | `FE-HRD-R26` (`S-D1` MVP-8) |
