# Laporan Perubahan Backend — `BE-HRD-037`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-037` |
| Judul | Gap Tenaga Langsung per Unit × Jabatan |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-8` (Angka Beban, Standar Kebutuhan Tenaga, dan Gap Langsung) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-8 |
| Traceability | `HRD-DEC-061` (`UNKNOWN_IS_NOT_ZERO`), `FR-HRD-102` s.d. `FR-HRD-104`, `AT-HRD-D1-03`, `AT-HRD-D1-05`, `AT-HRD-D1-05b`, `UAT-33`, `INT-HR-15`, `INT-HR-16` |
| Dependency | `BE-HRD-031` ✅, `BE-HRD-032` ✅, `BE-HRD-035` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `StaffingGapDtos.cs`, `IHrdStaffingGapCalculationService.cs`, `HrdStaffingGapCalculationService.cs`, `HrdStaffingGapController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, read-only & seluruh 5 acceptance criteria terpenuhi |

---

## 1. Masalah yang Diselesaikan

Dalam perencanaan kebutuhan pegawai rumah sakit, manajer HR dan kepala ruangan/kepala unit kerja perlu melihat posisi riil keterisian formasi (*headcount gap*) secara langsung (*live calculation*) per tanggal acuan tertentu.

Perhitungan gap harus transparan dan akurat membedakan:
1. **Penempatan Utama vs Rangkap/Plt (`FR-HRD-102`, Kriteria 1)**: Pegawai yang dihitung memenuhi formasi hanyalah pegawai dengan penempatan organisasi dan jabatan utama aktif (`IsPrimary = true`). Pegawai yang merangkap jabatan atau menjabat sebagai Pelaksana Tugas (Plt) tidak menambah angka keterisian formasi unit, melainkan disajikan sebagai catatan informasi penempatan tambahan (*SecondaryAssignments*).
2. **Prinsip UNKNOWN_IS_NOT_ZERO (`HRD-DEC-061`, `FR-HRD-104`, Kriteria 2)**: Jika suatu lapisan data belum dikonfigurasi (misal ambang batas hari cuti panjang `LongLeaveThresholdDays` belum diset, atau master alih daya belum tersedia), nilai lapisan tersebut adalah `null` ("data belum tersedia"), bukan angka `0`. Baris data dengan nilai `null` ditandai dengan flag kelengkapan `IsComplete = false`.
3. **Tiga Angka Kesenjangan Kebutuhan Tenaga (`FR-HRD-103`, Kriteria 3)**:
   - **Gap Permanen**: `Formasi - (Terisi - Akan Kosong)` [+ Alih Daya bila `CountsOutsourced = true`].
   - **Gap Sementara**: Jumlah pegawai yang tidak tersedia sementara (cuti panjang, dsb).
   - **Gap Kompetensi**: Jumlah pegawai yang belum kompeten terhadap aturan komposisi kualifikasi (*skill mix*) wajib.
4. **Hanya Memakai Standar Aktif (`AT-HRD-D1-03`, Kriteria 4)**: Draf standar yang masih dalam proses pengajuan (`Submitted`) tidak menggantikan standar aktif versi sebelumnya. Versi standar yang digunakan wajib dicantumkan dalam respons.
5. **Isolasi Wewenang Kepala Unit (`VR-WFP-38`, Kriteria 5)**: Pengguna dengan wewenang Kepala Unit hanya dapat melihat data unit dalam lingkup organisasinya dan sub-unit di bawahnya.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Terisi dihitung dari penempatan organisasi dan jabatan utama yang aktif pada tanggal acuan, dalam porsi kerja. Penempatan bukan utama (rangkap/Plt) hanya tampil sebagai informasi dan tidak menambah terisi. | Diimplementasikan di `HrdStaffingGapCalculationService.CalculateStaffingGapsAsync`. Hanya penempatan dengan `IsPrimary == true` pada `WfpOrganizationAssignments` dan `WfpPositionAssignments` yang dijumlahkan ke `Filled`. Penempatan bukan utama dipetakan ke koleksi `SecondaryAssignments`. | ✅ Terbukti |
| 2 | Lapisan "tidak tersedia sementara" bernilai `null` selama `LongLeaveThresholdDays` belum diisi; lapisan alih daya `null` sampai master jenis tenaga punya kategori alih daya; lapisan "belum kompeten" `null` bila standar berlaku tidak punya skill mix wajib. Baris dengan lapisan `null` membawa penanda kelengkapan `false`. | Diimplementasikan logika penanganan `null` sesuai aturan `UNKNOWN_IS_NOT_ZERO`. Properti `IsComplete` bernilai `false` jika ada lapisan yang tidak memiliki nilai. | ✅ Terbukti |
| 3 | Gap permanen = formasi − (terisi − akan kosong), ditambah alih daya hanya bila `CountsOutsourced = true`; gap sementara = tidak tersedia sementara; gap kompetensi = belum kompeten. | Formula dihitung secara deterministik dan disajikan dalam field `PermanentGap`, `TemporaryGap`, dan `CompetencyGap`. | ✅ Terbukti |
| 4 | Hanya standar `Active` yang dipakai; standar versi 2 yang masih `Submitted` tidak menggantikan versi 1 (`AT-HRD-D1-03`), dan respons menyebut versi standar yang dipakai. | Query menyaring `StandardStatus == StaffingStandardStatus.Active` yang tanggalnya mencakup `asOfDate`. `StandardNumber` dan `StandardVersionNumber` disertakan dalam setiap baris respons. | ✅ Terbukti |
| 5 | Kepala Unit hanya menerima baris unit dalam cakupannya. | Pengecekan dilakukan menggunakan metode `GetScopedOrganizationUnitIdsAsync` berbasis penugasan manajerial dan hierarki unit anak. | ✅ Terbukti |

---

## 3. Spesifikasi Endpoint Controller

Base URL: `/api/v1/corporate/human-resource/workforce-planning/staffing-gaps`
Tags: `Corporate / Human Resource / Workforce Planning / Staffing Gap`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses |
| :--- | :--- | :--- | :--- | :---: |
| `GET` | `/` | Ambil kalkulasi langsung kesenjangan tenaga kerja pada tanggal acuan per unit dan jabatan | `WorkforceStaffingGap:Read` | `200 OK` (`StaffingGapResponse`) |

---

## 4. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/StaffingGapDtos.cs`: DTO query dan respons baris kesenjangan, penempatan sekunder, dan ringkasan.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdStaffingGapCalculationService.cs`: Antarmuka layanan kalkulasi langsung kesenjangan tenaga kerja.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdStaffingGapCalculationService.cs`: Implementasi mesin kalkulasi gap multi-lapisan tanpa transaksi.
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdStaffingGapController.cs`: Controller ASP.NET Core endpoint `GET /`.

### Berkas Diubah:
1. `Program.cs`: Mendaftarkan scoped service `IHrdStaffingGapCalculationService` dan `HrdStaffingGapCalculationService`.
