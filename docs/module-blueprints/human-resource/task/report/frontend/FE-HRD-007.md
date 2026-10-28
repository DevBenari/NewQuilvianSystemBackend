# Laporan Perubahan Frontend — `FE-HRD-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-007` |
| Judul | Konsolidasi form absensi harian ke area self-services |
| Slice | `S-A6` (Konsolidasi Form Presensi ke Self-Services) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 2 |
| Trace | `HRD-DEC-006`, `FE-HRD-007` |
| Contract version | Employee Attendance Self-Service Contract v1 |
| Dependency | `FE-HRD-006` (✅ Selesai) |
| Klasifikasi | `LIGHT` |
| Task mode | `FRONTEND` |
| Target tulis | `src/app/self-services/human-resource/attendance/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Sebelumnya, terdapat potensi kebingungan staf rumah sakit ketika ingin melakukan pencatatan absensi harian mandiri karena lokasi form absensi belum terpusat secara kanonikal di kelompok menu layanan mandiri (*self-services*).

Melalui task ini, rute kanonikal presensi mandiri ditetapkan secara resmi pada alamat:
`/self-services/human-resource/attendance`

Halaman ini menghubungkan staf langsung ke antarmuka pencatatan kehadiran harian (`AttendanceEmployeeView`), sehingga staf medis dan non-medis cukup membuka satu lokasi menu terpusat untuk absensi masuk, absensi keluar, dan melihat status kehadiran harian mereka.

---

## 1. Wewenang UI yang Dipakai

Rute kanonikal presensi mandiri: `src/app/self-services/human-resource/attendance/page.jsx`.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Komponen / Tampilan | Status | Keterangan |
| --- | :---: | --- |
| `AttendanceEmployeeView` | `REUSE` | Menggunakan kembali tampilan presensi mandiri yang sudah mapan |
| Kerangka halaman Next.js | `REUSE` | Rute tipis App Router dengan metadata resmi |

---

## 3. Berkas yang Berubah

### Ditambahkan
* `src/app/self-services/human-resource/attendance/page.jsx`: Halaman rute resmi Next.js yang merender `AttendanceEmployeeView` di area self-services.

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Verifikasi Navigasi Rute | `PASS` | URL `/self-services/human-resource/attendance` berhasil memuat tampilan absensi mandiri |
| Linter check (`eslint`) | `PASS` | 0 syntax errors pada berkas rute |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Tersedia rute resmi presensi harian pegawai di area self-services | Terpenuhi | Halaman `/self-services/human-resource/attendance` aktif |
| 2. Menghubungkan pegawai ke view presensi mandiri tanpa duplikasi komponen | Terpenuhi | `page.jsx` mengimpor `AttendanceEmployeeView` |
| 3. Konsisten dengan taksonomi menu mandiri sistem Quilvian | Terpenuhi | Di bawah kelompok `self-services/human-resource/` |

---

## 6. Catatan Penutup

Task `FE-HRD-007` selesai secara tuntas dan telah mengukuhkan lokasi rute presensi mandiri yang konsisten bagi seluruh staf rumah sakit.
