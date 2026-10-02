# Laporan Perubahan Frontend — `FE-KSK-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-KSK-010` |
| Judul | Masa berlaku kartu penjamin tersimpan dan tidak dipilih ulang |
| Roadmap | `kiosk/roadmap/frontend-roadmap.md` — Amandemen 1 Oktober 2026 |
| Trace | Catatan pemilik 1 Okt 2026 (minor 3); `KSK-DEC-021`; `KSK-VAL-015` (kartu kedaluwarsa ditolak backend) |
| Contract version | Endpoint existing: `POST …/patient-insurances/kiosk` dan `POST …/patient-company-guarantors/kiosk` menerima `EffectiveEndDate`; `GET …/kiosk` mengembalikannya. Tanpa perubahan backend |
| Wewenang UI | `KSK-DEC-021` (opsi rekomendasi: tanggal "Berlaku s/d") |
| Klasifikasi | `MEDIUM` — alur pembayaran Pasien Lama dan Baru, muatan penjamin |
| Task mode | `FRONTEND` |
| Baseline | FE `fa9d5dd2`, BE `27fd8fb4` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 SEBAGIAN — AC 1–5 di source; kontrak muatan terbukti runtime; uji browser belum |

## 1. Keadaan yang ditemukan

1. Pasien Lama: masa aktif ("kurang/lebih dari 1 tahun") hanya ditulis ke teks `notes` saat menyimpan penjamin baru, tidak ke field apa pun. Saat penjamin tersimpan dipilih, `cardActivePeriodType` selalu kosong dan validasi memaksa pasien memilih ulang. Inilah keluhan pemilik.
2. Pasien Baru: pilihan masa aktif diwajibkan, tetapi **tidak dikirim sama sekali** ke backend.
3. Backend sudah punya `EffectiveStartDate`/`EffectiveEndDate` pada kedua tabel penjamin, dan route kiosk menerimanya.

## 2. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/helpers/kiosk/registration/kiosk-payer-validity.helpers.js` (baru) | Helper tanggal kalender: nilai isian, payload `YYYY-MM-DDT00:00:00` (sama dengan editor master data pasien), cek kedaluwarsa, label "Berlaku s/d 12 Sep 2027" |
| `.../old-patient/kiosk-old-patient-step-payment.jsx` | Opsi kurang/lebih 1 tahun dihapus. Penjamin baru: isian tanggal wajib (`min` = hari ini), dikirim sebagai `effectiveEndDate`. Penjamin tersimpan: tanggal read-only dari profil, tanpa pilih ulang; kedaluwarsa → tidak bisa lanjut dan diberi pesan. Daftar penjamin menampilkan "Berlaku s/d …". Data lama tanpa tanggal tetap bisa dipakai ("Belum tercatat") |
| `.../old-patient/kiosk-old-patient-step-confirm.jsx` | Ringkasan polis menampilkan "Berlaku s/d …" untuk asuransi dan perusahaan (sebelumnya membaca field `activePeriodLabel` yang tidak pernah ada) |
| `.../new-patient/kiosk-new-patient-step-payment.jsx`, `use-kiosk-new-patient-registration.jsx`, `kiosk-new-patient-payment.constants.js` | Field `cardActivePeriodType` diganti `validUntil`; validasi wajib + kedaluwarsa; konstanta periode dihapus |
| `src/lib/helpers/kiosk/registration/kiosk-new-patient-submit.helpers.jsx` | `effectiveEndDate` ikut dikirim saat membuat asuransi/penjamin perusahaan pasien baru |

## 3. Keputusan base component

| Elemen | Status | Bukti |
| --- | --- | --- |
| Isian tanggal | `REUSE` | `FieldGroup` + `<input>` di layar Pembayaran existing (pola yang sama dengan Nomor Polis) |
| Tampilan tanggal tersimpan | `REUSE` | `FieldGroup` + input read-only |

`UI GATE: REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

## 4. Validasi

| Perintah / bukti | Hasil |
| --- | --- |
| ESLint per berkas vs `HEAD` | 0 error; warning identik dengan `HEAD`; helper baru 0/0 |
| `npm run build` | `PASS` |
| Skrip node sekali pakai (scratchpad, via `tests/helpers/register.mjs`) | Helper 7/7 `PASS`: payload, baca nilai API, tanggal tidak sah (`2027-02-30`), kedaluwarsa kemarin, hari ini masih berlaku, kosong bukan kedaluwarsa, label |
| Runtime HTTP (app `bin/Release`, `QuilvianNewDevSukma`, SuperAdmin via cookie) R6 | `POST …/patient-insurances/kiosk` dengan `effectiveEndDate: "2027-09-12T00:00:00"` → `200`; `GET …/patient-insurances/kiosk?patientId=…` → `effectiveEndDate = 2027-09-12T00:00:00`; DB `12/09/2027`. Baris uji dihapus lewat `DELETE` API (soft delete, 2 baris dari dua run) |
| `MANUAL TEST` | `NOT FEASIBLE` — butuh akun perangkat Kiosk |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — dibuktikan lewat skrip sekali pakai, bukan test baru |

## 5. Acceptance criteria

| AC | Status |
| --- | --- |
| 1. Penjamin baru wajib tanggal, dikirim sebagai `effectiveEndDate` | Source + runtime kontrak (R6) |
| 2. Penjamin tersimpan read-only, tanpa pilih ulang | Source |
| 3. Kedaluwarsa ditolak | Source + helper; backend juga menolak (`KSK-VAL-015`) |
| 4. Pasien Baru menyimpan tanggal | Source (`buildPatientInsuranceRequests`/`…CompanyGuarantorRequests`) |
| 5. Konfirmasi menampilkan "Berlaku s/d …" | Source |

## 6. Risiko dan tindak lanjut

1. Penjamin yang disimpan sebelum perubahan ini tidak punya tanggal. Tampil "Belum tercatat" dan tetap boleh dipakai; tanggalnya diisi petugas lewat master data pasien.
2. IGD (`emergency-patient-payer-modal.jsx`) dan Rawat Inap (`inpatient-admission-payment-step.jsx`) masih memakai pola "kurang/lebih 1 tahun" yang sama. Di luar scope Kiosk; diusulkan sebagai task terpisah.
3. Uji alur Pembayaran di browser belum dilakukan.
