# Laporan Perubahan Backend — `BE-HRD-038`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-038` |
| Judul | Estimasi Biaya dari Gaji Tengah Grade dan Penyamaran Biaya |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-9` (Biaya, Potret Pengajuan, Permintaan Headcount, Pemenuhan, dan Pembatalan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-9 |
| Traceability | `HRD-DEC-065`, `FR-HRD-109`, `FR-HRD-111`, `VR-WFP-37`, `AT-HRD-D1-11`, `Permission v7 §11.1 WorkforcePlanningCost`, `Data Dictionary v7 §8.5` |
| Dependency | `BE-HRD-017` ✅ |
| Klasifikasi | `MEDIUM` |
| Task Mode | `BACKEND` |
| Target Tulis | `CostEstimationResult.cs`, `IHrdWorkforcePlanningCostService.cs`, `HrdWorkforcePlanningCostService.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, privasi gaji terjaga & seluruh 5 acceptance criteria terpenuhi |

---

## 1. Masalah yang Diselesaikan

Dalam penyusunan rencana kebutuhan tenaga kerja (*Manpower Plan*) maupun permohonan penambahan pegawai (*Headcount Request*), pihak manajemen rumah sakit dan bagian Keuangan (Finance) membutuhkan estimasi implikasi biaya penggajian tahunan.

Aturan bisnis dan privasi:
1. **Penghitungan Estimasi Berbasis Midpoint Grade (`HRD-DEC-065`, Kriteria 1)**: Estimasi biaya dihitung menggunakan nilai tengah rentang gaji (*MidpointSalary*) dari kelas jabatan (*MstSalaryGrade*), bukan berdasarkan riwayat gaji riil orang perorangan. Formula: `Porsi Kerja (FTE) × MidpointSalary × Jumlah Bulan`.
2. **Penanganan Tarif Tidak Tersedia (`RateUnavailable`, Kriteria 2)**: Jika kelas jabatan belum memiliki tarif nilai tengah atau grade tidak dipilih, sistem memberikan status `RateUnavailable` dan nilai biaya `null` ("data belum tersedia"), bukan angka `0`.
3. **Penyamaran Kolom Biaya bagi Non-Finance (`VR-WFP-37`, Kriteria 3)**: Pengguna yang tidak memiliki wewenang `WorkforcePlanningCost:Read` menerima nilai biaya `null` dan indikator `CostVisible = false` untuk menjaga kerahasiaan proyeksi anggaran.
4. **Isolasi Privasi Gaji Individu (Kriteria 4)**: Layanan ini tidak membaca tabel penetapan gaji atau profil kompensasi pegawai, sehingga tidak memicu audit akses data sensitif `HRD-DEC-039`.
5. **Keamanan Logging (Kriteria 5)**: Angka nominal rupiah biaya tidak pernah dicatat ke dalam log server (*logger*).

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Estimasi = porsi kerja × gaji tengah grade × jumlah bulan (contoh +2 kursi grade III selama 9 bulan dengan gaji Rp 6.500.000 menghasilkan Rp 117.000.000). | Diimplementasikan di `HrdWorkforcePlanningCostService.CalculateEstimatedCostAsync`: `workloadPortion * midpoint * months`. | ✅ Terbukti |
| 2 | Grade tanpa gaji tengah → status `RateUnavailable` dan biaya `null`, bukan 0. | Jika `MidpointSalary <= 0`, mengembalikan `RateStatus = RateUnavailable` dan `EstimatedCost = null`. | ✅ Terbukti |
| 3 | Pemanggil tanpa `WorkforcePlanningCost:Read` menerima seluruh kolom biaya `null` dan `costVisible = false`. | Logika penyamaran `HasCostReadPermission` dan `MaskCost` diterapkan secara terpusat. | ✅ Terbukti |
| 4 | Tidak membaca gaji perorangan maupun tabel penetapan gaji; membaca biaya tidak termasuk audit pembacaan gaji sensitif `HRD-DEC-039`. | Sumber data murni dari `MstSalaryGrade.MidpointSalary`. | ✅ Terbukti |
| 5 | Angka biaya tidak masuk logger. | Logger hanya mencatat status tanpa mencetak nominal rupiah estimasi. | ✅ Terbukti |

---

## 3. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/CostEstimationResult.cs`: DTO hasil perhitungan biaya dan enum `CostRateStatus`.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdWorkforcePlanningCostService.cs`: Antarmuka layanan perhitungan biaya dan penyamaran.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningCostService.cs`: Implementasi perhitungan estimasi berbasis `MstSalaryGrade.MidpointSalary`.

### Berkas Diubah:
1. `Program.cs`: Mendaftarkan scoped service `IHrdWorkforcePlanningCostService` dan `HrdWorkforcePlanningCostService`.
