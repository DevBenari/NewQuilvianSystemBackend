---
description: Pindai keadaan sistem Quilvian menjadi registry as-is; wajib sebelum /qv-grill
argument-hint: [full|refresh|focus <area>]
---

Gunakan skill `scan-system-registry` dengan mode: **$ARGUMENTS**

Aturan pemanggilan:

- Pemindaian bersifat read-only. Jangan mengubah source, migration, atau konfigurasi. Perintah git yang boleh dipakai hanya `status`, `log`, `diff`, `show`, `blame`, dan `rev-parse`.
- Mode default ditentukan dari manifest bila argumen kosong: `full` bila registry belum ada, scan penuh terakhir lebih dari 30 hari, atau ada area/modul baru; selain itu `refresh`.
- Catat commit SHA backend dan frontend lebih dulu, lalu pakai SHA itu pada seluruh bukti.
- Tentukan tingkat kesiapan setiap entity `L1` sampai `L4` dengan memeriksa model, EF configuration, migration, controller/service, dan consumer nyata. Jangan menaikkan tingkat berdasarkan dugaan.
- Isi kolom pemilik data untuk setiap entity. Yang tidak jelas ditulis `Belum ditentukan` dan masuk zona konflik.
- Kumpulkan zona konflik: nama kembar, duplikasi konsep, entity tanpa pemilik, skema tidak lengkap, endpoint bentrok, enum ganda, dan prefix tidak sesuai. Tulis risikonya sebagai akibat nyata bagi pengguna atau data.
- Dilarang memuat usulan entity baru, kata `wajib`, `prioritas`, `sprint`, atau urutan implementasi. Registry hanya berisi keadaan sekarang.

Output: tujuh berkas di `docs/system-registry/` sesuai [format registry](../rules/rule-prascan/format-registry-sistem.md). Akhiri dengan ringkasan angka, lima zona konflik paling berisiko, dan batas yang tidak dapat diperiksa.
