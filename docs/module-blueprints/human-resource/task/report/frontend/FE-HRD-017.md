# Laporan Perubahan Frontend — `FE-HRD-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-017` |
| Judul | Verifikasi dan integrasi layar alur exit clearance pegawai |
| Slice | `S-C4` (Exit Clearance & Pengembalian Aset RS) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-014`, `FE-HRD-017` |
| Contract version | Employee Exit Clearance API v1 |
| Dependency | `BE-HRD-014` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/exit-clearance/`, laporan task, dan roadmap |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Ketika seorang pegawai memutuskan berhenti, pensiun, atau masa kontrak kerjanya berakhir di rumah sakit, pegawai tersebut wajib menjalani proses *Exit Clearance*. Proses ini memastikan seluruh kewajiban telah diselesaikan sebelum hak akhir (uang pesangon atau surat pengalaman kerja) diserahkan:
- Pengembalian aset rumah sakit (laptop, seragam, ID card, kunci ruangan).
- Penyerahan rekam medis dan serah terima tugas klinis (*handover* pasien).
- Penyelesaian tanggungan keuangan/kasbon di bagian keuangan.
- Penonaktifan akun sistem informasi manajemen rumah sakit (SIMRS) di bagian IT.

Task ini memverifikasi bahwa antarmuka alur Exit Clearance telah aktif dan berfungsi di alamat `/hr/exit-clearance`:
1. **Daftar Permohonan Exit Clearance**: Menampilkan daftar staf yang sedang dalam proses pengunduran diri.
2. **Status Checklist Lintas Departemen**: Memantau status persetujuan dari masing-masing departemen penanggung jawab (HR, Keuangan, IT, Logistik/Aset).
3. **Penyelesaian Final**: Penguncian status akhir kelulusan clearance (*Completed*) setelah seluruh departemen memberikan tanda bebas tanggungan.

---

## 1. Wewenang UI yang Dipakai

Rute exit clearance HR: `src/app/hr/exit-clearance/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Ringkasan status clearance | `SummaryCards` | `REUSE` | Total proses aktif dan selesai |
| Tabel pemantauan | `DataTable` | `REUSE` | Menampilkan progres checklist antar-divisi |
| Status verifikasi | `StatusBadge` | `REUSE` | Badge Pending / In Review / Cleared |
| Penanganan feedback | `ToastStack` | `REUSE` | Notifikasi status sistem |

---

## 3. Berkas yang Berubah

### Berkas yang Diverifikasi & Diperbarui
* `src/app/hr/exit-clearance/page.jsx`: Halaman rute aktif exit clearance terhubung ke backend.
* `src/components/view/hr/exit-clearance/`: Komponen view alur exit clearance yang mengimplementasikan checklist departemen.
* `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md`: Pembaruan status penuntasan task `FE-HRD-017`.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 syntax errors pada berkas halaman exit clearance |
| Status Checklist Verifikasi | `PASS` | Menampilkan status penyelesaian unit IT, Keuangan, Logistik, dan HR |
| Komunikasi API Backend | `PASS` | Terhubung ke controller exit clearance `BE-HRD-014` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Layar alur exit clearance pegawai tersedia dan aktif di frontend | Terpenuhi | Halaman `/hr/exit-clearance` terverifikasi aktif |
| 2. Menampilkan checklist persetujuan lintas divisi | Terpenuhi | Tersaji pada tabel progres exit clearance |
| 3. Terhubung ke controller backend `BE-HRD-014` | Terpenuhi | Endpoint exit clearance backend terintegrasi |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataTable, StatusBadge, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-017` selesai secara tuntas dan telah memvalidasi ketersediaan alur penyelesaian kewajiban akhir pegawai rumah sakit.
