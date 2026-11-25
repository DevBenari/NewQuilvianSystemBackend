# PAY-FE-FIX-027 — Popup Tambah Tunjangan Pegawai: semua jenis dapat dipilih, panel dinamis per jalur (frontend)

Tanggal: 24 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `b05275ec` (working tree memuat PAY-FE-FIX-025/026 yang belum di-commit). Pasangan: PAY-BE-FIX-027 (terapkan bersamaan).

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang). Status tugas UI: paling tinggi sebagian.

## File
| File | Perubahan |
| --- | --- |
| `src/utils/payroll/payroll-allowance-policy-assignment-utils.js` | Normalisasi metadata jalur; `buildTypeSelectOptions` tanpa `disabled` dari `assignable` (hanya `selectable=false`); keterangan netral dari `hint`; `buildTypeGuidance`, `typeUsesPolicy`; opsi kebijakan selalu dapat dipilih; `pickAutoPolicy`; label metode/nilai per sumber; payload memuat `policySource`; `canSubmit` mensyaratkan `preview.canSave` |
| `src/lib/hooks/payroll/use-payroll-workforce-allowance-policy-assignment.js` | Kebijakan hanya dimuat untuk jenis berkebijakan; `policySource` ke pratinjau dan simpan; `selectedType/selectedPolicy/usesPolicy/guidance` |
| `src/components/view/payroll/compensation/payroll-workforce-allowance-policy-modal.jsx` | Hilang teks "Tidak dapat dipilih" dan penguncian pilihan; dropdown kebijakan dan kolom hasil hanya untuk jenis berkebijakan; panduan dan syarat sebagai `InformationAlert` info; penyebab tidak dapat disimpan dari Backend |
| `src/lib/services/payroll/payroll-workforce-allowance-service.js` | `preview(..., policySource, signal)` |
| `src/lib/constants/payroll/payroll-workforce-allowance-constants.js` | 4 teks baru |
| `tests/unit/payroll-allowance-multi-source.test.mjs` (baru) + 4 berkas tes diperbarui | 12 tes baru; asersi lama yang mengunci perilaku lama dibalik sesuai kontrak baru |

## Perilaku
1. Semua jenis aktif dapat diklik; keterangan di bawah nama = `hint` netral ("Memerlukan data lembur", dst.), tidak ada warna oranye/kuning.
2. Setelah memilih: reset kebijakan, preview, galat; jenis berkebijakan memuat kebijakan (Grade Gaji, Struktur Gaji, dan cakupan lain diputuskan Backend), memilih otomatis bila hanya satu.
3. Jenis Lembur/Variabel/manual: tidak ada dropdown kebijakan; panel info menyebut data yang dibutuhkan dan jalur pembayarannya. Shift/Siaga/Risiko: kebijakan dapat ditinjau beserta syaratnya; penyebab tidak dapat disimpan ditampilkan.
4. Simpan hanya aktif bila Backend menyatakan `canSave` untuk pratinjau pilihan terkini; payload hanya `{ allowanceTypeId, allowancePolicyId, policySource }`.
5. Batal tetap menutup modal tanpa permintaan simpan; Simpan memakai `confirmProps` (tidak mengunci Batal).

## UI GATE
REUSE: `BaseSelectField`, `BaseTextField`, `ConfirmModal`, `InformationAlert variant="info"`, kelas `optionRow/optionMeta` yang sudah ada. Tanpa komponen atau CSS baru.

## Validasi
`AUTOMATED TEST`: `payroll-*.test.mjs` dari root workspace: 535 tes, 532 lulus, 3 gagal lama (module-contract x2, run-screens x1; tidak dibandingkan dengan HEAD bersih). ESLint 0 error; `check-css-usage.mjs` 0 temuan.

## Matriks pemeriksaan statis (15 skenario)
| # | Skenario | Status |
| --- | --- | --- |
| 1-5 | Lembur, Transport, Siaga, Risiko, Shift dapat dipilih; muncul kebutuhan/kebijakan yang sesuai | VERIFIED STATIC (tes utils + modal); NEEDS RUNTIME UAT |
| 6 | Jenis manual dapat dipilih, menampilkan data yang dibutuhkan | VERIFIED STATIC; penyimpanan BLOCKED |
| 7 | Jenis tanpa kebijakan tidak menghasilkan penetapan fiktif | VERIFIED STATIC (tidak ada jalur simpan; canSave) |
| 8 | Kebijakan grade/struktur lain tidak dipakai | VERIFIED STATIC (aturan cakupan Backend, diuji statis) |
| 9 | Aktivitas belum terverifikasi tidak masuk pembayaran | VERIFIED STATIC (aktivitas `CanSave=false`, create ditolak) |
| 10 | Penetapan tidak disetujui tidak ikut hitung | VERIFIED STATIC (tidak diubah; transport lama ditolak bila perlu persetujuan) |
| 11 | Tanpa pembayaran ganda | VERIFIED STATIC (pagar dua arah, indeks unik) |
| 12 | ViewAmount melindungi nominal | VERIFIED STATIC (nominal null; "Tidak ditampilkan") |
| 13 | Payroll final tidak berubah | VERIFIED STATIC (tidak ada tulis ke tabel payroll) |
| 14 | API menolak manipulasi ID/nominal/periode/referensi | VERIFIED STATIC (identitas saja; sumber dan cakupan divalidasi); NEEDS RUNTIME UAT |
| 15 | Dropdown mouse/keyboard, loading, kosong, gagal, ganti cepat | VERIFIED STATIC (stage + AbortController tetap); NEEDS RUNTIME UAT |

## Ketergantungan
Backend PAY-BE-FIX-027 harus diterapkan bersamaan. Dengan backend lama, `canSave` pratinjau diasumsikan true dan jenis lama tetap berfungsi, tetapi jenis non-rutin tidak punya panduan.
