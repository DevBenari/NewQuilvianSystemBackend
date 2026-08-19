# rule-output — Aturan Output Dokumentasi

Folder ini menyimpan aturan **bentuk dan gaya** untuk setiap dokumen yang dihasilkan skill
Quilvian. Aturan di sini bersifat canonical dan berlaku untuk backend maupun frontend.

## Isi folder

| File | Fungsi |
| --- | --- |
| [aturan-output-dokumentasi.md](aturan-output-dokumentasi.md) | Lima aturan wajib beserta penjelasan dan checklist akhir |
| [contoh-dokumentasi-modul.md](contoh-dokumentasi-modul.md) | Contoh penerapan lengkap memakai modul Allowance Type |
| [contoh-output-per-skill.md](contoh-output-per-skill.md) | Contoh bentuk keluaran setiap skill memakai studi kasus IGD |

## Lima aturan wajib

1. Bahasa harus Bahasa Indonesia.
2. Gunakan bahasa yang mudah dipahami orang umum.
3. Jelaskan secara detail beserta contoh untuk hal yang sulit dipahami.
4. Bisnis proses harus dijelaskan dengan jelas dan urut.
5. Sajikan endpoint bergaya Swagger: judul grup `[Tags(...)]` ditambah tabel API.

## Kapan aturan ini dipakai

Setiap kali skill menulis atau memperbarui:

- artefak pada `docs/module-blueprints/<module>/`;
- laporan perubahan backend atau frontend;
- readiness report dan capability map;
- ringkasan handoff yang disimpan sebagai dokumen.

Aturan ini **tidak** berlaku untuk komentar di dalam source code dan pesan commit.

## Hubungan dengan SKILL.md

`SKILL.md` menentukan **apa** yang dikerjakan, gate approval, dan batas kewenangan. Folder ini
hanya menentukan **bagaimana dokumen ditulis**. Bila keduanya bertentangan, aturan
keamanan, privasi, dan invariant pada `SKILL.md` yang menang.

## Pemeliharaan

- Ubah aturan hanya di sini, pada repository backend.
- Setelah `aturan-output-dokumentasi.md` berubah, perbarui expected SHA-256 pada adapter
  frontend `QuilvianSystemFrontendDev/.claude/rules/rule-output/aturan-output-dokumentasi.md` dalam
  perubahan yang sama.
- Jangan menyalin isi aturan ke frontend sebagai sumber kebenaran kedua.
