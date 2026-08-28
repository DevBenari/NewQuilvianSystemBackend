# Klasifikasi Task

Lakukan klasifikasi sebelum menyusun rencana. Pakai faktor tertinggi yang berlaku; naikkan satu tingkat bila ada dua faktor atau lebih yang jatuh di tingkat berikutnya. Bila task masih belum pasti, klasifikasikan secara konservatif dan periksa dulu sebelum menurunkannya.

| Tingkat | Faktor penentu |
| --- | --- |
| LIGHT | Satu repository; umumnya 1–3 berkas diperiksa dan 1–2 berkas diubah; tidak ada perubahan material pada logika bisnis, API, database, keamanan/auth, maupun UI/workflow. |
| MEDIUM | Satu atau dua repository; umumnya 4–10 berkas diperiksa atau 3–6 berkas diubah; logika, UI/workflow, atau dampak ke konsumen API yang terbatas dan tidak merusak; tidak ada perancangan ulang database maupun keamanan/auth yang material. |
| HEAVY | Beberapa modul yang berkaitan, atau lebih dari 10 berkas diperiksa / 6 berkas diubah; logika bisnis yang substansial; perubahan kontrak API, pertimbangan database/schema, dampak keamanan/auth, atau scope workflow yang luas sehingga perlu review terkoordinasi. |
| EPIC | Beberapa domain yang bisa di-deploy sendiri-sendiri, workflow yang menyentuh seluruh arsitektur atau berjalan multi-fase, perancangan ulang API/database/keamanan yang luas, atau scope yang tidak dapat direview dan divalidasi dengan aman sebagai satu perubahan yang terbatas. |

## Model penilaian

Skor di bawah ini menentukan klasifikasinya. Beri skor pada setiap faktor, jumlahkan, lalu terapkan rentang klasifikasinya.

| Faktor | Skor 0 | Skor 1 | Skor 2 |
| --- | --- | --- | --- |
| Cakupan repository | Satu repository | — | Dua repository |
| Berkas diperiksa | ≤ 8 | 9–20 | > 20 |
| Berkas diubah | ≤ 3 | 4–8 | > 8 |
| Logika bisnis | Sederhana | Sedang | Kompleks |
| Kontrak API | Tidak ada | Memakai kontrak yang sudah ada | Mengubah kontrak |
| Database | Tidak ada | Hanya perilaku query/persistence yang sudah ada | Dampak schema/entity/migration |
| Keamanan/Auth | Tidak ada | Berkaitan tetapi bukan intinya | Dampak inti pada authorization/authentication/keamanan |
| UI/Workflow | Kecil/lokal | Satu halaman atau workflow terbatas | Banyak halaman atau workflow luas |

| Total skor | Klasifikasi |
| --- | --- |
| 0–3 | LIGHT |
| 4–8 | MEDIUM |
| 9–12 | HEAVY |
| 13+ | EPIC |

## Faktor yang wajib dinilai

Nilai jumlah repository; berkas yang diperiksa; berkas yang diubah; kerumitan logika bisnis; dampak terhadap kontrak API; dampak database; dampak keamanan/auth; dan cakupan UI/workflow. Aturan `AGENTS.md` yang berlaku menentukan apakah suatu faktor memang diizinkan.

## Pekerjaan module blueprint

Task murni `MODULE BLUEPRINT MODE` boleh memeriksa kedua repository aplikasi, tetapi hanya menulis dokumentasi blueprint yang dilacak. Jangan mengklasifikasikannya sebagai HEAVY semata-mata karena pemeriksaan lintas repository itu; nilai juga cakupan dokumentasinya, kerumitan arsitektur/dependency, dan risiko keputusan yang belum terselesaikan, di samping faktor normal. Penilaian untuk implementasi aplikasi tidak berubah.

Klasifikasikan pekerjaan blueprint sebagai HEAVY bila mencakup banyak modul atau dependency yang material, kontrak yang belum terselesaikan, atau keputusan berisiko tinggi di ranah keamanan, keuangan, klinis, privasi, maupun regulasi. Perlakukan perancangan ulang arsitektur yang luas atau perubahan lifecycle lintas modul sebagai EPIC: berhenti, pecah menjadi fase blueprint yang terbatas, lalu klasifikasikan ulang sebelum mulai menulis.

## Aturan eksekusi

Setiap perancangan ulang yang menyentuh seluruh arsitektur, implementasi lintas domain, atau scope yang tidak dapat direview dan divalidasi dengan aman sebagai satu perubahan terbatas adalah EPIC, berapa pun skornya.

Task EPIC tidak pernah langsung dikerjakan: `STOP → DECOMPOSE → klasifikasikan ulang setiap fase.` Pecah menjadi fase-fase yang dapat direview secara mandiri, lalu klasifikasikan dan kerjakan setiap fase secara terpisah.

## Panduan model

- **Claude Sonnet 5** adalah model bawaan.
- **Claude Opus 5** hanya untuk eskalasi pada task HEAVY yang benar-benar sulit, setelah task tersebut dibatasi scope-nya.
- Naikkan kedalaman penalaran lebih dulu sebelum menaikkan model. Eskalasi model adalah langkah terakhir, bukan langkah pertama.
