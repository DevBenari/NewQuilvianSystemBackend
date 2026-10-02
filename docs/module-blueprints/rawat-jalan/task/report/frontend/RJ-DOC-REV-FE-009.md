# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-009` |
| Judul | EWS diastolik dan kategori BMI di Skrining Perawat dan Dokter |
| Roadmap | `rawat-jalan/roadmap/doctor-consultation-roadmap.md` bagian `10` |
| Trace | Catatan pemilik 1 Okt 2026, Prioritas Screening butir 1–2; `RJ-DOC-DEC-010` (a), (b) |
| Kontrak | Tanpa API baru; `BirthDate` antrean perawat/dokter existing (`NurseStationQueueDtos`, `DoctorQueueDtos`) |
| Task mode | `FRONTEND` |
| Baseline | FE `fa9d5dd2` (`sukmagpV2`) |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 SEBAGIAN — logika, lint, dan build terbukti; uji klik layar belum |

## 1. Keputusan klinis yang dipakai

1. Pemilik menetapkan NEWS/MEWS sebagai acuan. NEWS2 maupun MEWS **tidak** memberi skor pada diastolik. Karena itu diastolik ditampilkan di tabel EWS sebagai informasi ("Tidak diskor"), dan skor total tidak berubah. Interpretasinya memakai ambang yang sudah ada di backend (`PatientVitalSignCalculation`): < 60 rendah, > 110 tinggi, ≥ 120 sangat tinggi (kritis), selebihnya normal. Skor EWS tersimpan di backend tidak diubah; kalkulator itu juga dipakai Rawat Inap.
2. Kategori IMT dewasa Kemenkes RI: < 18,5 berat badan kurang; 18,5–25,0 normal; > 25,0–27,0 berat badan lebih; > 27,0 obesitas. IMT dibulatkan satu desimal seperti tampilan sebelum dikategorikan. Pasien < 18 tahun mendapat catatan "gunakan grafik IMT/U". Bila tanggal lahir tidak ada, kategori dewasa tetap ditampilkan.

## 2. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/utils/health-services/registration-management/{doctor-queue,nurse-station-management}/ews.utils.js` | `getDiastolicInterpretation`; baris `diastolic` (`scored: false`) di `calculateEwsPreview`; total hanya dari baris yang diskor |
| `…/{doctor-queue,nurse-station-management}/vital-preview.utils.js` | `getAgeInYears`, `getBmiCategory`, `BMI_ADULT_MIN_AGE_YEARS` |
| `src/components/features/health-services/{doctor-queue-features,nurse-station-management}/EwsPreviewPanel.jsx` | Kolom skor menampilkan "Tidak diskor" untuk baris informasi; teks penjelasan menyebut NEWS2/MEWS |
| `src/lib/hooks/.../doctor-queue/useDoctorScreeningForm.js`, `doctor-queue-features/DoctorScreeningForm.jsx` | `bmiCategory` dari tanggal lahir pasien; deskripsi kartu BMI "Kategori (Kemenkes): …"; `form.diastolic` masuk dependency EWS |
| `nurse-station-management/VitalSignTab.jsx`, `nurse-station-queue-view.jsx` | Prop `patientBirthDate`; deskripsi kartu BMI; `diastolic` masuk dependency EWS |

## 3. Keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Kartu BMI | `REUSE` | `VitalPreviewCard` existing, isi `description` |
| Tabel EWS | `REUSE` | `EwsPreviewPanel` existing, satu baris tambahan |

`UI GATE: REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

## 4. Validasi

| Perintah / bukti | Hasil |
| --- | --- |
| ESLint per berkas vs `HEAD` | 0 error; warning identik dengan `HEAD` (dua warning `exhaustive-deps` baru sempat muncul dan diperbaiki) |
| `npm run build` pada source final | `PASS` |
| Skrip node sekali pakai (scratchpad, `tests/helpers/register.mjs`), kedua salinan util | `30/30 PASS` untuk BMI dan EWS (ditambah 7 helper `FE-KSK-010`; total `37/37`). Run pertama `33/37`: IMT tepat 25,0 dan 27,0 masuk kategori di atasnya karena galat floating point (1,7² = 2,8899…). Diperbaiki dengan pembulatan satu desimal, lalu `37/37` |
| Uji browser (Playwright, SuperAdmin, semua request non-GET diblokir) | Login `PASS`; layar Skrining Pasien terbuka, tetapi antrean "0 pasien sesuai cluster hari ini" untuk SuperAdmin, sehingga form tidak dapat dibuka. Tidak ada tulisan ke server |
| `MANUAL TEST` | `NOT FEASIBLE` — butuh akun perawat dengan cluster poli dan antrean hari ini |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — dibuktikan lewat skrip sekali pakai |

## 5. Acceptance criteria

| AC | Status |
| --- | --- |
| 1. Baris diastolik "Tidak diskor", skor total tetap | Logika terbukti (skrip); tampilan belum dilihat |
| 2. Kategori Kemenkes, batas dibulatkan | Logika terbukti (skrip, batas 18,5/25,0/27,0/27,5) |
| 3. Anak < 18 tahun tidak dikategorikan dewasa | Logika terbukti (skrip) |

## 6. Risiko dan tindak lanjut

1. Bila RS memakai form EWS lokal yang **memberi skor** pada diastolik, kirim tabel ambangnya. Perubahan di FE kecil; skor tersimpan di backend perlu task BE terpisah karena berdampak ke Rawat Inap.
2. Uji klik di layar Perawat dan Dokter oleh pemilik/UAT.
