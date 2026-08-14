# Laporan Dokumentasi Penggunaan AI Skills

| Field | Nilai |
| --- | --- |
| Tanggal | 2026-08-13 |
| Branch backend | `MHamzah` |
| Branch frontend | `HamzahV2` |
| Scope | Dokumentasi penggunaan skill |
| Source aplikasi | Tidak diubah |
| Endpoint/contract runtime | Tidak diubah |
| Database/runtime migration | Tidak ada |
| Build aplikasi | Tidak dijalankan |

## Tujuan

Menyediakan dokumentasi operasional penggunaan tujuh AI skills dari discovery awal sampai
readiness, serta panduan khusus ketika agent dijalankan dari project frontend.

## File

| File | Fungsi |
| --- | --- |
| `QuilvianBackend/.agents/PANDUAN-PENGGUNAAN-SKILLS.md` | Panduan canonical end-to-end, prompt, output, approval gate, loop perbaikan, dan DoD |
| `QuilvianFrontEnd/.agents/PANDUAN-PENGGUNAAN-SKILLS.md` | Panduan adapter frontend, handoff backend, implementasi UI, contract delta, dan hash stale |
| `QuilvianBackend/docs/hamzah/report/panduan-penggunaan-ai-skills.md` | Laporan perubahan ini |

## Keputusan dokumentasi

- Panduan lengkap disimpan satu kali di backend agar tidak terjadi drift.
- Panduan frontend tetap dapat digunakan mandiri untuk operasi frontend, tetapi
  mereferensikan panduan canonical untuk prosedur shared.
- Contoh prompt menggunakan explicit invocation `$nama-skill`.
- Workflow tidak menganggap build sukses sebagai readiness end-to-end.
- Approval bisnis, API, security, dan UI tetap menjadi tindakan manusia.
- `agency-local/` tidak diubah.

## Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| File panduan | Lulus; backend 416 baris dan frontend 221 baris |
| Code fence Markdown | Lulus; seluruh fence seimbang |
| Cakupan skill | Lulus; ketujuh explicit invocation terdokumentasi |
| Link panduan frontend ke canonical | Lulus; target relative path tersedia |
| Placeholder/trailing whitespace | Lulus; tidak ditemukan |
| Git ignore | Lulus; kedua panduan pada `.agents/` trackable |
| Build/test aplikasi | Tidak dijalankan karena source aplikasi tidak berubah |

## Batas verifikasi

Dokumentasi diperiksa secara statis. Penggunaan skill pada kasus IGD dan rawat inap tetap
perlu dilakukan sebagai forward-test terpisah sesuai roadmap setup skill.
