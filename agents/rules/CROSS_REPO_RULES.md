# Aturan Lintas Repository

`AGENTS.md` adalah pemegang wewenang. Task mode dan target tulis yang eksplisit menentukan akses ke repository; mode atau target tulis yang hilang maupun ambigu otomatis diperlakukan sebagai **AUDIT MODE**.

| Mode | Frontend | Backend |
| --- | --- | --- |
| AUDIT | Hanya baca | Hanya baca |
| MODULE BLUEPRINT | Sumber bukti, hanya baca | Hanya `docs/module-blueprints/**` |
| FRONTEND | Target tulis | Sumber kebenaran kontrak dan perilaku bisnis, hanya baca secara ketat |
| BACKEND | Rujukan, hanya baca | Target tulis |
| CROSS-REPO | Hanya bila disebut eksplisit sebagai target tulis | Hanya bila disebut eksplisit sebagai target tulis |

- Periksa backend lebih dulu sebelum menebak kontrak API frontend.
- Kode frontend adalah rujukan konsumen dan tidak menimpa aturan bisnis maupun keamanan backend.
- Jangan pernah diam-diam mengubah repository seberang ketika menemukan cacat di sana; laporkan cacat itu, kecuali task yang sedang berjalan memang memberi wewenang atas target tulis tersebut.
- Jangan memakai task lintas repository untuk memperluas scope source, konfigurasi, migration, Git, atau deployment.
- Perubahan governance/dokumentasi hanya diizinkan bila task mode dan target tulis eksplisit sama-sama mengizinkannya.
- Pada `MODULE BLUEPRINT MODE`, sebuah blueprint boleh mengutip bukti dengan format `repository/path#symbol@source-SHA`. Bukti dari frontend tidak pernah memberi wewenang tulis di frontend, dan bukti dari backend tidak pernah memberi wewenang tulis pada source aplikasi backend.
- Source SHA backend atau frontend yang berubah membuat bukti blueprint yang bergantung padanya menjadi basi. Lakukan impact review yang dibatasi scope-nya sebelum memperlakukan bukti itu sebagai terkini atau memakainya ulang untuk desain, perencanaan, maupun penilaian kesiapan.
