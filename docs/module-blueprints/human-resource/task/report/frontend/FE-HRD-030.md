# Laporan Perubahan Frontend — `FE-HRD-030`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-030` |
| Judul | Layar Formasi Jabatan baca saja beserta riwayat versi |
| Slice | `S-D1` · `MVP-7` |
| Roadmap | `roadmap/frontend-roadmap.md` kartu `FE-HRD-030`; dikerjakan lewat `FE-HRD-R12` |
| Trace | `HRD-DEC-062`, `FR-HRD-098` s.d. `100`, `AT-HRD-D1-06` |
| Contract version | API Contract v7 bagian 13 (disetujui pemilik 3 November 2026) |
| Wewenang UI | Tanpa tombol ubah (mengikat `HRD-DEC-062`); riwayat versi sebagai dialog (`DEV_DISCRETION`) |
| Dependency | `FE-HRD-029` ✅, `BE-HRD-032` ✅ [BE] |
| Klasifikasi | `MEDIUM` — layar baru dengan service, state, dan validasi |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src`; laporan dan tanda status di `docs/module-blueprints/human-resource/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `d09dc74da` (branch `HamzahV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f751f8ca` (branch `MHamzah`), hanya dibaca |
| Tanggal | 11 November 2026 |
| Status | 🟡 Sebagian — 4 dari 5 kriteria terbukti; riwayat versi belum terbukti karena belum ada data formasi |

---

## 1. Keadaan yang ditemukan di awal

Layar belum ada.

---

## 2. Proses bisnis dari sisi pengguna

HR, Kepala Unit, atau Direktur membuka Formasi Jabatan, menyaring rumah sakit, unit, jabatan, atau tanggal, lalu mengklik satu baris untuk melihat riwayat versinya. Tidak ada tombol tambah, ubah, atau hapus; formasi berubah hanya lewat dokumen yang disetujui. Di database pengembangan belum ada formasi, sehingga layar menulis "Belum ada formasi. Isi lewat Batch Formasi Awal."

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `S-D1` di `frontend-roadmap.md`, `03-frontend-architecture.md` bagian 12, controller dan DTO `WorkforcePlanning`, view referensi Batch Data Awal, `menu-items.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/hr/workforce-planning/position-formations-view.jsx` | Layar daftar dan dialog riwayat |
| `src/app/hr/workforce-planning/position-formations/page.jsx` | Rute |

### 3.3 Kepatuhan arsitektur frontend

Rute → view → service `FE-HRD-029`. Kolom dan saringan mengikuti Pola A.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx` dan `src/components/view/hr/leave/leave-admin-view.jsx`

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header halaman | `Hero` | `REUSE` | Tanpa tombol aksi |
| Saringan | `DataFilter` + 3 `ResourceFilterSelect` + `FilterDatePicker` | `REUSE` | — |
| Tabel dan paginasi | `DataTable` + `RegionPagination` | `REUSE` | — |
| Riwayat versi | `ConfirmModal` berisi `DataTable` | `COMPOSE` | Belum ada base drawer (`DEC-FER-01`) |
| Pesan galat | `InformationAlert` | `REUSE` | — |

`UI GATE: PASS — 4 elemen REUSE, 1 COMPOSE, 0 NEW`

Hasil pemeriksa dan grep anti-regresi:

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <7 berkas view S-D1 dan S-B1>
OK — 7 berkas diperiksa, 0 temuan.
$ grep -nE "className=\"[^\"]*\b(flex|grid|gap-[0-9]|p-[0-9]|px-|py-|text-(sm|xs|lg)|bg-|rounded|w-full|items-center|md:|hover:)" <7 berkas>
(kosong)
```

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/hr/workforce-planning/position-formations` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/04-formasi-jabatan.png` | Setara; tidak ada tombol ubah |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat formasi jabatan..." |
| Kosong | "Belum ada formasi. Isi lewat Batch Formasi Awal." |
| Gagal | `InformationAlert` merah |
| Tanpa hak akses | Menu tidak tampil; server menolak 403 |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Workforce Planning / Position Formation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/workforce-planning/position-formations` | Daftar formasi | `WorkforcePositionFormation : Read` |
| `GET` | `/v1/corporate/human-resource/workforce-planning/position-formations/history` | Riwayat versi | `WorkforcePositionFormation : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint <modul yang diubah> --quiet` | 0 error | `PASS` | Keluaran perintah 11 November 2026 |
| `npm run build` | `npm run build` berhasil; dijalankan tiga kali, terakhir sesudah perubahan source terakhir | `PASS` | `scratchpad build3`: keluar 0 |
| `npm run test:unit` | 4.618 lulus, 53 gagal; seluruh kegagalan ada di berkas yang tidak disentuh (billing, IGD, rawat inap, master data layanan kesehatan, dan layar HR lama yang berkasnya sudah tidak ada) | `EXISTING / ENVIRONMENT ISSUE` | Ringkasan `ℹ tests 4671, pass 4618, fail 53` |
| `check-css-usage.mjs` pada 7 berkas view baru | 0 temuan | `PASS` | Keluaran perintah |
| Uji browser (Chrome headed, backend lokal mode `Runtime__Role=Web`) | Daftar terbuka, saringan tampil, keadaan kosong benar, tanpa tombol ubah | `PASS` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/04-formasi-jabatan.png` |

Uji manual: `PASS` untuk daftar dan keadaan kosong.

**Tidak dijalankan:** Dialog riwayat versi, karena belum ada formasi di database pengembangan.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kolom unit, jabatan, formasi, berlaku mulai, dokumen sumber; saringan rumah sakit, unit, jabatan, tanggal | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/04-formasi-jabatan.png` |
| Riwayat versi menampilkan seluruh versi, jenis dan nomor dokumen, penyetuju, waktu persetujuan | Belum terbukti | Terpetakan ke source; belum ada data |
| Tidak ada tombol tambah, ubah, hapus | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/04-formasi-jabatan.png` |
| Keadaan kosong "Belum ada formasi. Isi lewat Batch Formasi Awal." | Terpenuhi | `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/04-formasi-jabatan.png` |
| Butir menu hanya untuk pemegang `WorkforcePositionFormation : Read` | Terpenuhi | `requiredPermission` dan uji akun tanpa hak |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit. Lima berkas lain ikut berubah di working tree tetapi bukan dari task ini dan tidak disentuh: `workforce-profile-dashboard.jsx`, `employee-profile-changes-self-service-view.jsx`, `employee-workforce-tabs.jsx`, `employee-workforce-detail.css`, `wfp-workforce-detail.css` |
| Langkah berikutnya | Uji ulang riwayat versi sesudah batch formasi awal pertama disetujui |
