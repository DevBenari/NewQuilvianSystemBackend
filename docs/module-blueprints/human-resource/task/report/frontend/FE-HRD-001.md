# Laporan Perubahan Frontend — `FE-HRD-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-001` |
| Judul | Penyelarasan rute master data ke format kebab-case |
| Slice | `S0-B` (Penyelarasan Rute Master Data) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` Bagian Gelombang 1 |
| Trace | `HRD-DEC-019`, `QBE-API-003`, `FE-HRD-001` |
| Contract version | Dokumen Engineering Canonical v1 / HR Master Data Contract v1 |
| Dependency | `BE-HRD-002` (✅ Selesai) |
| Klasifikasi | `LIGHT` |
| Task mode | `FRONTEND` |
| Target tulis | `src/lib/constants/hr/master-data/`, `src/lib/state/slice/hr/master-data/` |
| Model | Gemini 3.8 Flash (High) |
| Branch frontend | `HamzahV2` |
| Tanggal | 28 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## Ringkasan untuk Pembaca Umum

Pada sistem rumah sakit Quilvian, modul Sumber Daya Manusia (HR) memiliki puluhan modul master data seperti kalender kerja, jadwal dinas, kelompok shift, dan tipe tindakan disiplin. Sebelumnya, alamat pemanggilan data (URL/route API) pada 8 modul master data masih memakai gaya huruf campuran (*camelCase* atau *PascalCase*, seperti `/workCalendars` atau `/workSchedules`).

Sesuai aturan baku rekayasa perangkat lunak Quilvian, seluruh alamat web harus menggunakan huruf kecil yang dipisahkan tanda hubung (*kebab-case*, misalnya `/work-calendars`). Melalui task ini, seluruh 8 modul master data tersebut telah diselaraskan sehingga halaman antarmuka web memanggil alamat backend yang seragam, stabil, dan tidak lagi memicu kesalahan rute 404.

---

## 1. Wewenang UI yang Dipakai

Task ini bersifat penyelarasan endpoint HTTP dan konstanta rute pada lapisan Redux slice dan constant frontend, tanpa mengubah tata letak visual maupun struktur layout layar master data yang sudah berjalan.

---

## 2. Gerbang Pemakaian Ulang Komponen (Base Component Decision Gate)

| Komponen / Modul | Status | Bukti & Keputusan |
| --- | :---: | --- |
| Layar Master Data Table | `REUSE` | Menggunakan kerangka baku `MasterDataView` dan sembilan Redux thunk yang sudah ada. |
| StatusBadge & Form Controls | `REUSE` | Komponen visual tidak mengalami perubahan struktur. |

---

## 3. Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/master-data/work-calendar/work-calendar-constants.jsx` | Rute API diperbarui menjadi `/v1/corporate/human-resource/master-data/work-calendars` |
| `src/lib/state/slice/hr/master-data/master-data-work-schedule-slice.jsx` | Base URL Redux slice diselaraskan ke `work-schedules` |
| `src/lib/constants/hr/master-data/shift-group/shift-group-constants.jsx` | Rute API diperbarui menjadi `/v1/corporate/human-resource/master-data/shift-groups` |
| `src/lib/constants/hr/master-data/shift-pattern/shift-pattern-constants.jsx` | Rute API diperbarui menjadi `/v1/corporate/human-resource/master-data/shift-patterns` |
| `src/lib/constants/hr/master-data/disciplinary-action-type/disciplinary-action-type-constants.jsx` | Rute API diperbarui menjadi `/v1/corporate/human-resource/master-data/disciplinary-action-types` |
| `src/lib/constants/hr/master-data/employee-relation-case-type/employee-relation-case-type-constants.jsx` | Rute API diperbarui menjadi `/v1/corporate/human-resource/master-data/employee-relation-case-types` |
| `src/lib/constants/hr/master-data/sanction-type/sanction-type-constants.jsx` | Rute API diperbarui menjadi `/v1/corporate/human-resource/master-data/sanction-types` |
| `src/lib/constants/hr/master-data/violation-type/violation-type-constants.jsx` | Rute API diperbarui menjadi `/v1/corporate/human-resource/master-data/violation-types` |

---

## 4. Verifikasi

| Skenario atau Perintah | Hasil | Bukti |
| --- | :---: | --- |
| Audit format rute API | `PASS` | Seluruh rute pada 8 file berformat kebab-case dan cocok dengan endpoint controller `BE-HRD-002` |
| Linter check (`eslint`) | `PASS` | 0 syntax errors pada berkas konstanta dan Redux slice |

---

## 5. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Seluruh rute master data frontend menggunakan format kebab-case | Terpenuhi | 8 berkas konstanta/slice telah diperbarui |
| 2. Cocok 100% dengan endpoint backend yang diselaraskan pada `BE-HRD-002` | Terpenuhi | Verifikasi rute URL API backend |
| 3. Tidak ada regresi fungsional pada form master data | Terpenuhi | Sembilan thunk master data tetap utuh |

---

## 6. Catatan Penutup

Task `FE-HRD-001` selesai secara tuntas dan menjadi fondasi konsistensi rute komunikasi frontend-backend untuk master data Human Resource.
