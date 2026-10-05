# Laporan Perubahan Frontend — `FE-KSK-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-009` |
| Judul | Kartu Jenis Kunjungan, Pembayaran, dan Poliklinik lebih ringkas |
| Roadmap | `kiosk/roadmap/frontend-roadmap.md` — Amandemen 1 Oktober 2026 |
| Trace | Catatan pemilik 1 Okt 2026 (minor 1 dan 2); `KSK-DEC-020` |
| Contract version | Tanpa API (perubahan tampilan) |
| Wewenang UI | `KSK-DEC-020`; markup dan CSS kiosk existing |
| Klasifikasi | `SMALL` — 8 berkas tampilan, tanpa perubahan alur |
| Task mode | `FRONTEND` — permintaan pemilik 1 Okt 2026 ("audit dan perbaiki … kiosk") |
| Baseline | FE `fa9d5dd2` (`sukmagpV2`), bersih sebelum task |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 SEBAGIAN — AC 1–4 terpenuhi di source dan build; uji browser belum |

## 1. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-type.jsx` | Footer `✓ …` dan data `points` dihapus |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-type.jsx` | Footer `✓ …` dan data `bullets` dihapus |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-payment.jsx` | Footer `✓ …` kartu Tunai/Asuransi dan data `points` dihapus |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-payment.jsx` | Footer `✓ …` dan `bullets` pada `PAYMENT_METHOD_META` dihapus |
| `src/components/view/kiosk/registration/old-patient/kiosk-old-patient-step-service.jsx` | `ClinicCard`: baris unit layanan \| jenis klinik dan chip estimasi menit dihapus; lokasi pindah ke baris atas, kiri chip singkatan |
| `src/components/view/kiosk/registration/new-patient/kiosk-new-patient-step-service.jsx` | Idem untuk Pasien Baru |
| `src/style/kiosk/registration/kiosk-old-patient-view.module.css`, `kiosk-new-patient-view.module.css` | Kelas baru `.serviceClinicHighlightLocation` (token `--color-text-secondary`, `--color-text-muted`, `--font-size-body`) |

Tidak diubah: ceklis di step Tujuan Layanan (`kiosk-old-patient-step-service-target.jsx:119`), karena tidak diminta.

## 2. Keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Kartu pilihan Jenis Kunjungan / Pembayaran | `REUSE` | Markup kiosk existing, hanya footer dihapus |
| Kartu poli | `REUSE` | `ClinicCard` existing; satu kelas teks lokasi ditambahkan di CSS feature kiosk |

`UI GATE: REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

## 3. Validasi

| Perintah / bukti | Hasil |
| --- | --- |
| `npx eslint` per berkas, dibandingkan dengan `HEAD` | 0 error; jumlah warning identik dengan `HEAD` di setiap berkas (mis. step-service lama 3/3, baru 4/4) |
| `npm run build` (2×, terakhir pada source final) | `PASS` — "Compiled successfully in 75s", exit 0 |
| Grep `✓ {` di `src/components/view/kiosk` | Tersisa 1, di Tujuan Layanan (di luar scope) |
| `MANUAL TEST` | `NOT FEASIBLE` — layar Kiosk mensyaratkan akun perangkat Kiosk (`isKioskUser`); kredensialnya tidak tersedia di sesi dan tidak ditebak |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — perubahan tampilan murni |

## 4. Acceptance criteria

| AC | Status | Bukti |
| --- | --- | --- |
| 1. Tanpa `✓ …` di Jenis Kunjungan dan Pembayaran | Terpenuhi di source | Grep di atas |
| 2. Nama lengkap; tanpa unit layanan dan menit | Terpenuhi di source | `ClinicCard` kedua berkas |
| 3. Lokasi di kiri chip singkatan | Terpenuhi di source | Baris atas: `<p .serviceClinicHighlightLocation>` lalu chip |
| 4. Lint tanpa warning baru, build `PASS` | `PASS` | §3 |

## 5. Risiko dan tindak lanjut

1. Tata letak belum dilihat di browser. Kartu masih memakai `grid-template-rows: auto 1fr auto` dengan dua anak, sehingga ruang kosong di bawah nama mungkin perlu dirapikan setelah dilihat. Pemilik diminta mengecek di perangkat Kiosk, atau memberikan akun perangkat Kiosk untuk uji Playwright.
2. Tafsiran "informasi di sebelah kiri samping KSK" = baris lokasi dipindah ke kiri chip singkatan poli. Bila yang dimaksud lain, cukup ubah urutan markup.
