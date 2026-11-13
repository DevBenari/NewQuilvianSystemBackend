# Laporan Perubahan Frontend — `FE-HRD-R25`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-R25` |
| Judul | Onboarding dan Kontrak: migrasi |
| Slice | `S-C4-ONB` |
| Task asal | `FE-HRD-059` s.d. `FE-HRD-073` |
| Kontrak | API Contract v9 bagian 15 dan source backend `MHamzah` |
| Task mode | `FRONTEND` — source backend hanya dibaca |
| Branch frontend | `HamzahV2` (`origin/HamzahV2`) |
| Tanggal | 12 November 2026 |
| Status | 🟡 Sebagian — implementasi, lint, build, dan gerbang CSS lulus; uji browser belum tersedia dan unggah spreadsheet batch belum didukung backend |

## 1. Masalah dan hasil perubahan

Layar lifecycle masih mencampur data contoh, UUID cadangan, modal buatan sendiri, inline style, dan nama field yang tidak cocok dengan DTO. Lima belas view sekarang memakai pola layar HR bersama, hak akses ketat, pesan galat API yang aman, serta data server. Perubahan terakhir menyelesaikan daftar/rincian masa percobaan, preseptorship, batch kontrak, permintaan akses, dan pengaturan per rumah sakit.

Batch kontrak mengikuti kemampuan backend saat ini: draf dibuat per rumah sakit dan baris dimasukkan satu per satu. Endpoint unggah spreadsheet dan unduh templat tidak ada, sehingga fungsi itu tidak ditampilkan sebagai aksi palsu.

## 2. Berkas utama

| Area | Berkas |
| --- | --- |
| Service dan taxonomy | `src/lib/services/hr/lifecycle-service.js`, `src/lib/constants/hr/lifecycle/lifecycle-constants.js` |
| Template, kasus, kontrak, worklist | `src/components/view/hr/lifecycle/onboarding-*`, `employment-contract*`, `lifecycle-worklist-view.jsx` |
| Masa percobaan | `probations-list-view.jsx`, `probation-detail-view.jsx` |
| Preseptorship | `clinical-preceptorships-list-view.jsx`, `clinical-preceptorship-detail-view.jsx` |
| Batch kontrak | `initial-contract-batches-list-view.jsx`, `initial-contract-batch-detail-view.jsx` |
| Akses dan pengaturan | `onboarding-access-requests-view.jsx`, `lifecycle-settings-view.jsx` |

## 3. Kontrak dan keadaan layar

Kontrak diverifikasi langsung terhadap controller dan DTO lifecycle backend. Contoh koreksi penting: `PreceptorshipStatus`, `NewPreceptorWorkforceProfileId`, `IndependentOn`, `AssessmentNote`, `EvidenceFile`, `BatchStatus`, `RequestType`, `RequestStatus`, `CompletionNote`, dan empat field `HrdEmploymentLifecycleSettingResponse`. Semua daftar menangani loading, empty, error, filter, pagination, unauthorized, dan retry melalui pola bersama. Aksi tulis hanya tampil jika pasangan permission yang sesuai tersedia.

### 3.1 Keputusan base component

| Elemen | Keputusan | Implementasi |
| --- | --- | --- |
| Shell, hero, tabel, filter, pagination | `REUSE` | `Hero`, `DataFilter`, `DataTable`, `FilterSelect`, `RegionPagination` |
| Tombol, status, alert, toast | `REUSE` | `BaseButton`, `StatusBadge` lewat `HrStatusBadge`, `InformationAlert`, `ToastStack` |
| Dialog dan field | `REUSE` | `ConfirmModal`, keluarga `BaseFormControl`, `ResourceFilterSelect` |
| Panel informasi | `COMPOSE` | `HrPanel` dan `HrInfoGrid` menyusun style bersama yang sudah ada |
| Unggah bukti | `WRAP` | `HrFileField` membungkus `BaseFormControl`; menunggu base upload pada `FE-HRD-R49` |

`UI GATE: REUSE/COMPOSE/WRAP — tidak ada base component baru atau perubahan default base component.`

## 4. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint src/components/view/hr/lifecycle ...` | PASS — 0 error |
| `npm run lint:errors` | PASS — exit 0, tidak ada error |
| `node .../check-css-usage.mjs <8 view yang berubah>` | PASS — 8 berkas, 0 temuan |
| `npm run build` | PASS — verifikasi akhir lintas R25–R29 menghasilkan 813 halaman dan standalone runtime siap |
| `npm run test:unit` | FAIL — issue existing lintas modul dan test yang merujuk file lama/hilang; tidak menunjuk view R25–R29 |
| Pemeriksaan data contoh/inline style pada view lifecycle | PASS untuk view yang dimigrasikan pada R25 |
| Uji browser dan screenshot | `VISUAL CHECK: NOT FEASIBLE` — tool kontrol browser tidak tersedia pada sesi ini |
| Test otomatis baru | Tidak ditambahkan; suite unit existing telah dijalankan dan hasil non-hijau dicatat di atas |

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-L` 1 — tidak ada data contoh | Terpenuhi pada view R25 | Daftar dan detail membaca service; fallback contoh dihapus |
| `AC-L` 2 — galat ramah pengguna | Terpenuhi | `useHrRemoteData` dan `useHrAction` |
| `AC-L` 3 — base component | Terpenuhi | Tabel keputusan dan UI gate |
| `AC-L` 4 — CSS checker 0 temuan | Terpenuhi | 8 berkas, 0 temuan |
| `AC-L` 5 — tombol sesuai hak akses | Terpenuhi pada source | `useStrictPermissions` per aksi |
| Tombol Batal berbeda dari Simpan | Terpenuhi pada source | `ConfirmModal` memakai aksi cancel dan confirm terpisah |
| Unggah template batch dan validasi per baris | Sebagian | Validasi per baris tampil; endpoint unggah/template tidak tersedia |
| Uji manual akun berhak/tidak berhak dan screenshot | Belum | Kontrol browser tidak tersedia |

## 6. Risiko dan langkah berikutnya

R25 belum dapat menjadi ✅ sampai endpoint unggah/template batch tersedia atau acceptance criteria disesuaikan, serta uji browser per peran dilakukan. Source backend tidak diubah. Working tree belum di-commit atau di-push.
