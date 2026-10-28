# Laporan Perubahan Frontend — `FE-HRD-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-012` |
| Judul | Penyediaan rute dan layar matriks visual roster shift dan status siaga on-call 24/7 |
| Slice | `S-B4` (Matriks Roster Shift & On-Call 24/7) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-005`, `FE-HRD-012` |
| Contract version | Shift Roster & On-Call Scheduling API v1 |
| Dependency | `BE-HRD-009` (✅ Selesai) |
| Klasifikasi | `MEDIUM` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/hr/scheduling/roster/`, `src/components/view/hr/scheduling/`, `src/lib/services/hr/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Rumah sakit beroperasi tanpa henti 24 jam sehari, 7 hari seminggu. Penjadwalan kerja perawat, bidan, dokter jaga IGD/ICU, dan staf laboratorium memerlukan penyusunan roster shift dinas bulanan/mingguan serta pengaturan giliran siaga panggilan darurat (*on-call duty*). Kepala unit perlu melihat matriks pembagian jadwal ini, mempublikasikan jadwal dinas ke staf, atau menguncinya agar tidak berubah sembarangan.

Task ini menyediakan antarmuka di alamat `/hr/scheduling/roster`:
1. **Dasbor Roster Shift**: Metrik ringkasan jumlah periode aktif, unit yang tercover, dan status siaga on-call.
2. **Tabel Periode Penjadwalan Roster**: Menampilkan daftar periode roster (nama periode, unit kerja, tanggal mulai s/d tanggal selesai, jumlah staf bertugas, dan status draf/terbit/terkunci).
3. **Modal Penyusunan Periode Roster Baru**: Form dialog untuk membuat periode jadwal kerja baru untuk suatu unit kerja.
4. **Aksi Publikasi & Penguncian**: Aksi cepat bagi kepala ruangan atau HR untuk mempublikasikan roster (*Publish*) dan mengunci roster (*Lock*) secara terverifikasi.

---

## 1. Wewenang UI yang Dipakai

Rute penjadwalan roster shift HR: `src/app/hr/scheduling/roster/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Kebutuhan Antarmuka | Komponen Base yang Dipakai | Status | Keterangan |
| --- | --- | :---: | --- |
| Header halaman | `Hero` | `REUSE` | Standar header Quilvian |
| Ringkasan periode jadwal | `SummaryCards` | `REUSE` | Metrik periode aktif dan on-call |
| Filter & pencarian unit | `DataFilter` | `REUSE` | Filter nama unit dan status periode |
| Tabel periode roster | `DataTable` | `REUSE` | Kolom ID, Periode, Unit, Rentang Tanggal, Staf, Status, Aksi |
| Status periode | `StatusBadge` | `REUSE` | Badge Draft / Published / Locked |
| Tombol aksi jadwal | `BaseButton` | `REUSE` | Tombol Publikasikan dan Kunci Jadwal |
| Modal buat periode baru | `ConfirmModal` | `REUSE` | Form input periode roster dinas baru |
| Notifikasi umpan balik | `ToastStack` | `REUSE` | Notifikasi status eksekusi |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/lib/services/hr/roster-scheduling-service.js`: Layanan pemanggilan API manajemen periode roster, publikasi, dan penguncian jadwal.
* `src/components/view/hr/scheduling/roster-scheduling-view.jsx`: Tampilan dasbor roster shift HR terintegrasi modal dan aksi publikasi/kunci.
* `src/app/hr/scheduling/roster/page.jsx`: Rute Next.js untuk halaman Matriks Roster Shift & On-Call.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Linter check (`eslint`) | `PASS` | 0 error pada berkas view, page, dan service roster |
| Aksi Publikasi & Kunci | `PASS` | Mengubah status jadwal menjadi Published / Locked dengan modal konfirmasi |
| Komunikasi API Backend | `PASS` | Terhubung ke controller roster shift scheduling `BE-HRD-009` di backend |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia layar pengelolaan roster shift dinas dan siaga on-call | Terpenuhi | Halaman `/hr/scheduling/roster` aktif |
| 2. Kepala ruangan/HR dapat membuat periode roster baru | Terpenuhi | Modal ConfirmModal terhubung ke `createRosterPeriod` |
| 3. Mendukung aksi publikasi dan penguncian periode jadwal dinas | Terpenuhi | Terhubung ke `publishRosterPeriod` dan `lockRosterPeriod` |
| 4. Menggunakan base component baku Quilvian | Terpenuhi | Hero, SummaryCards, DataFilter, DataTable, StatusBadge, ConfirmModal, ToastStack |

---

## 6. Catatan Penutup

Task `FE-HRD-012` selesai secara tuntas dan telah menyediakan instrumen penjadwalan dinas rotasi 24/7 bagi seluruh instalasi rumah sakit.
