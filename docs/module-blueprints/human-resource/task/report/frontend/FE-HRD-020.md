# Laporan Perubahan Frontend — `FE-HRD-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-020` |
| Judul | Verifikasi dan integrasi layar pendaftaran benefit dan tanggungan staf |
| Slice | `S-D3` (Benefit Enrollment & Tanggungan Staf) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 5 |
| Trace | `HRD-DEC-018`, `FE-HRD-020` |
| Contract version | Benefit Enrollment Management API v1 |
| Dependency | `BE-HRD-019` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/benefit/enrollments/`, laporan task, dan roadmap |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Setiap pegawai rumah sakit berhak memperoleh fasilitas kompensasi dan tunjangan non-gaji (*employee benefits*), seperti keikutsertaan BPJS Kesehatan, BPJS Ketenagakerjaan, asuransi kesehatan swasta tambahan, serta fasilitas perawatan bagi anggota keluarga tanggungan (suami/istri dan anak sah).

Task ini memverifikasi bahwa antarmuka pendaftaran benefit telah aktif dan siap digunakan di alamat `/hr/benefit/enrollments`:
1. **Daftar Kepesertaan Benefit**: Menampilkan daftar staf yang telah terdaftar dalam program kompensasi kesehatan dan kesejahteraan.
2. **Rekap Anggota Tanggungan**: Memantau jumlah keluarga tanggungan terdaftar yang berhak mendapatkan plafon manfaat rumah sakit.
3. **Status Polis & Kartu**: Menampilkan nomor kepesertaan/polis, tanggal efektif berlaku kepesertaan, dan status keaktifan benefit.

---

## 1. Wewenang UI yang Dipakai

Rute kepesertaan benefit HR: `src/app/hr/benefit/enrollments/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Ringkasan kepesertaan | `SummaryCards` | `REUSE` | Total staf dan tanggungan terdaftar |
| Tabel kepesertaan | `DataTable` | `REUSE` | Menampilkan pegawai, jenis paket benefit, polis, status |
| Status kepesertaan | `StatusBadge` | `REUSE` | Badge Active / Expired / Ineligible |
| Penanganan feedback | `ToastStack` | `REUSE` | Notifikasi status sistem |

---

## 3. Berkas yang Berubah

### Berkas yang Diverifikasi & Diperbarui
* `src/app/hr/benefit/enrollments/page.jsx`: Halaman rute aktif pendaftaran benefit terhubung ke backend.
* `src/components/view/hr/benefit/enrollments/`: Komponen view kepesertaan benefit staf.
* `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md`: Pembaruan status penuntasan task `FE-HRD-020`.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 syntax errors pada berkas pendaftaran benefit |
| Data Integritas Polis | `PASS` | Menampilkan nomor kartu polis dan daftar tanggungan staf |
| Komunikasi API Backend | `PASS` | Terhubung ke controller benefit enrollment `BE-HRD-019` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Layar pendaftaran benefit dan tanggungan staf tersedia dan aktif di frontend | Terpenuhi | Halaman `/hr/benefit/enrollments` terverifikasi aktif |
| 2. Menampilkan informasi kepesertaan asuransi dan tanggungan keluarga | Terpenuhi | Tersaji pada tabel pendaftaran benefit |
| 3. Terhubung ke controller backend `BE-HRD-019` | Terpenuhi | Endpoint benefit enrollment backend terintegrasi |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataTable, StatusBadge, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-020` selesai secara tuntas dan telah memvalidasi ketersediaan antarmuka kepesertaan benefit dan tanggungan bagi staf rumah sakit.
