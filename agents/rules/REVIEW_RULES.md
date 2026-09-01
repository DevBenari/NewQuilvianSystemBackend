# Aturan Review Penyelesaian

Sebelum menyatakan selesai, lakukan dan catat review yang sepadan dengan bobot task-nya.

- **Kesesuaian QBE:** tentukan ID QBE yang berlaku dari `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, verifikasi kepatuhannya, dan catat setiap pengecualian terbatas yang sudah disetujui.

- **Review diff:** periksa setiap berkas yang berubah dan pastikan diff-nya hanya mengimplementasikan perilaku yang diminta.
- **Review scope:** pastikan tidak ada perubahan source, dependency, konfigurasi, workflow, migration, atau keluaran hasil generate yang tidak berkaitan.
- **Review regresi:** pertimbangkan pemanggil, route, kontrak, state, workflow, dan jalur error yang terdampak; jalankan validasi yang relevan.
- **Bukti validasi:** cantumkan perintah yang benar-benar dijalankan beserta hasil sebenarnya. Jangan pernah mengarang hasil `PASS` atau menyimpulkannya dari perintah yang tidak dijalankan.
- **Klasifikasi validasi:** laporkan setiap pemeriksaan yang relevan sebagai **PASS**, **NEW ERROR** (muncul akibat diff saat ini atau jelas disebabkan olehnya), **EXISTING / ENVIRONMENT ISSUE** (dapat direproduksi atau didukung bukti, dan tidak berkaitan dengan task), atau **NOT RUN** (memang sengaja tidak diperlukan). Jangan menyebut sebuah kegagalan sebagai sudah ada sebelumnya tanpa bukti. Task dengan `EXISTING / ENVIRONMENT ISSUE` tetap dapat direview hanya bila scope yang berubah sudah direview secara mandiri, kegagalan itu bukan disebabkan task tersebut, dan sisa risikonya dilaporkan.
- **Pemeriksaan rahasia:** pastikan tidak ada credential, token, connection string, key, atau nilai konfigurasi sensitif yang muncul pada berkas yang berubah maupun pada laporan.
- **Dampak berkas bersama:** review komponen bersama, kontrak, konfigurasi, dan konsumen lintas repository ketika hal-hal itu berubah atau terdampak.
- **Review blueprint:** perlakukan artefak blueprint sebagai perubahan dokumentasi yang dilacak. Pastikan klaim arsitektur dan status mengutip bukti, `MODULE-STATUS` tidak menandai sebuah modul `DONE` tanpa bukti verifikasi, dan sebuah fase tidak dianggap `DONE` hanya karena berkasnya sudah ada. Laporkan bukti yang basi, fase yang terblokir, dan fase yang dapat dilanjutkan secara mandiri sebagai tiga hal yang berbeda.
- **Status Git akhir:** jalankan `git status --short`, bedakan perubahan hasil task dari perubahan yang sudah ada sebelumnya, dan jangan melakukan stage, commit, atau push tanpa wewenang eksplisit.
