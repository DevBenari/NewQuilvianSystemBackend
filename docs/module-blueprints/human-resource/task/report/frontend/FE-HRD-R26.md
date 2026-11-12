# Laporan Perubahan Frontend — `FE-HRD-R26`

## Metadata

| Field | Nilai |
| --- | --- |
| Task | `FE-HRD-R26` — `S-D1` MVP-8: Angka Beban, Standar, Gap |
| Task asal | `FE-HRD-032` s.d. `FE-HRD-035` |
| Kontrak | API Contract v7 bagian 13 |
| Tanggal | 12 November 2026 |
| Status | 🟡 Source dan validasi statis selesai; uji browser/peran belum tersedia |

## Hasil

- Menambahkan rute dan layar `/hr/workforce-planning/staffing-workloads` untuk daftar, tambah, dan ubah angka beban berikut unit, jabatan, periode, nilai, dan sumber.
- Menambahkan daftar `/hr/workforce-planning/staffing-standards` dan penyusun `/staffing-standards/[id]`, termasuk baris metode, faktor cadangan, pembulatan, skill mix, rekomendasi backend, simpan, ajukan, dan batalkan.
- Menambahkan `/hr/workforce-planning/staffing-gaps` dengan formasi, lapisan tenaga, tiga gap, rangkap/Plt, alih daya, serta perlakuan `null` sebagai *data belum tersedia*.
- Menambahkan tiga butir menu dengan `requiredPermission` yang sesuai controller backend.
- Memperluas service Axios dan konstanta rute/status tanpa data contoh atau fallback sukses palsu.

## UI GATE

| Elemen | Keputusan | Dasar |
| --- | --- | --- |
| Hero, filter, tabel, pagination, badge | `REUSE` | Katalog base component |
| Form, resource selector, dialog konfirmasi | `REUSE` | Base form control, resource selector, ConfirmModal |
| Panel kepala, baris, skill mix, rekomendasi | `COMPOSE` | `HrPanel` + base component |
| Base component baru | Tidak ada | Seluruh kebutuhan tercakup komponen yang ada |

## Kontrak yang diverifikasi

- Workload: `OrganizationUnitId`, `PositionId`, `PeriodStartDate`, `PeriodEndDate`, `WorkloadValue`, `SourceNote`.
- Standard: kepala dokumen, `Lines`, `SkillMixRules`, `RowVersion`, rekomendasi, submit, dan cancel.
- Metode `WorkloadAnalysis` disaring dari pilihan; frontend tidak menghitung ulang rekomendasi.
- Gap: `TemporaryUnavailable`, `NotCompetent`, `Outsourced`, `TemporaryGap`, dan `CompetencyGap` mempertahankan `null`.

## Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint berkas R26 dan menu | Lulus, 0 error/warning |
| CSS usage checker pada 4 view | `OK — 4 berkas diperiksa, 0 temuan` |
| `npm run build` | Lulus; 808 halaman dihasilkan |
| Uji browser akun berhak/tidak berhak | `VISUAL CHECK: NOT FEASIBLE` — tidak ada alat browser pada sesi ini |

## Acceptance criteria

Kriteria source `FE-HRD-032` s.d. `035` terimplementasi. Status belum dinaikkan menjadi penuh karena tambah/ubah/submit memerlukan data dan akun peran nyata, sementara tampilan `null` dan pembatasan Kepala Unit perlu bukti browser/API terautentikasi.

Perubahan belum di-commit atau di-push.
