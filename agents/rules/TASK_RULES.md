# Aturan Task Claude

Dokumen ini menetapkan siklus kerja yang berulang untuk pekerjaan implementasi. `AGENTS.md` tetap menjadi konstitusi repository yang berwenang; pengaman khusus repository selalu didahulukan.

## Siklus kerja standar

`CLASSIFY → INSPECT → PLAN → IMPLEMENT → VALIDATE → REVIEW → REPORT`

1. **CLASSIFY** — klasifikasikan task memakai `TASK_CLASSIFICATION.md`; pastikan task mode, target tulis, branch, dan scope-nya.
   Untuk pekerjaan aplikasi backend, lakukan preflight QBE terhadap `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` beserta registry-nya: Area, Module, owner/prefix, kelas ratchet, dan ID aturan yang berlaku.
2. **INSPECT** — periksa implementasi pembanding terdekat beserta seluruh kontrak, authorization, workflow, persistence, dan aturan yang terdampak langsung sebagaimana diwajibkan `AGENTS.md`.
3. **PLAN** — susun rencana implementasi yang singkat dan terbatas sebelum mulai menulis.
4. **IMPLEMENT** — kerjakan hanya perubahan yang diberi wewenang, mengikuti arsitektur yang ada dan pola terdekat.
5. **VALIDATE** — validasi dengan perintah yang sepadan dengan perubahannya dan dengan kebutuhan repository; catat hasil perintah yang sebenarnya.
6. **REVIEW** — review diff dan kriteria penyelesaian memakai `REVIEW_RULES.md`.
7. **REPORT** — tulis laporan task tracked memakai `REPORT_TEMPLATE.md` ke `docs/module-blueprints/<module-slug>/task/report/backend/<TASK-ID>.md` pada repository backend yang memuat blueprint.

## Efisiensi konteks

- Mulai dari berkas yang spesifik dan implementasi terdekat yang sudah ada.
- Hindari pemindaian seluruh repository kecuali task memang membutuhkannya untuk menetapkan scope atau keselamatan.
- Jangan membaca ulang modul yang tidak berkaitan setelah scope-nya dipahami.
- Hindari validasi berulang kecuali kode atau konfigurasi terkait memang berubah.
- Utamakan controller, DTO, service, akses data, validation, authorization, dan pola workflow yang sudah ada daripada membuat pola baru.
- Berhenti dan laporkan ketika syarat branch, target tulis, keamanan, database, atau kontrak yang diwajibkan belum terpenuhi.

## Pemulihan setelah interupsi

Untuk interupsi eksekusi, model, atau provider yang bersifat sesaat (misalnya galat dari hulu, batas laju, respons yang terputus, atau tool yang timeout), periksa status Git saat ini beserta diff terkait, tentukan bagian mana yang sudah selesai, lalu lanjutkan dari kondisi terakhir yang terverifikasi. Jangan mengulang audit yang sudah selesai, menduplikasi penyuntingan yang sudah ada, atau membatalkan pekerjaan valid yang sudah rampung hanya karena responsnya terputus. Laporkan kegagalan eksternal/provider yang berulang, jangan mencoba ulang tanpa batas.

## Perubahan sampingan

Perubahan yang tergenerasi oleh tool atau muncul sebagai efek samping di luar scope task yang diberi wewenang tidak boleh tertinggal pada diff akhir yang dilacak, kecuali memang diwajibkan. Sebelum memulihkan sebuah perubahan sampingan, pastikan dulu bahwa perubahan itu tidak ada saat task dimulai atau jelas tidak berkaitan, bahwa ia muncul sebagai efek samping, dan bahwa memulihkannya tidak akan menghapus pekerjaan bisnis yang diberi wewenang. Pulihkan hanya butir tersebut; jangan pernah membatalkan working tree secara menyeluruh atau membuang pekerjaan user yang tidak berkaitan.

## Batas wewenang

Alur kerja ini tidak pernah memberi wewenang untuk publikasi Git, deployment, peningkatan dependency, pembuatan maupun eksekusi migration, operasi database, atau perubahan di luar scope tulis eksplisit milik task yang sedang berjalan.
