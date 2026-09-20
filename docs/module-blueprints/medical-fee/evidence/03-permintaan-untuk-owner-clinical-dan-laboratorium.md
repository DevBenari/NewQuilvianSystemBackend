# Permintaan Medical Fee kepada Owner Clinical Management dan Laboratory Management

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner modul Medical Fee |
| Untuk | Owner modul Clinical Management dan owner modul Laboratory Management |
| Tembusan | Owner modul Radiology Management — hanya untuk bagian 5 |
| Tanggal | 20 September 2026 |
| Sifat | **Permintaan perubahan pencatatan beserta alasannya.** Belum meminta menulis kode sekarang |
| Menutup | `MF-CQ-07` |
| Dasar | `MF-DEC-003`, `MF-DEC-014`, `MF-DEC-016`, `MF-DEC-018`, `approved` 20 September 2026 |
| Yang memblokir | Hanya rumpun pembagian tim di luar kamar operasi |

Berkas ini berdiri sendiri, dapat dibaca tanpa membuka blueprint Medical Fee.

---

## 1. Mengapa Medical Fee menghubungi Anda

Modul Medical Fee akan menghitung jasa tenaga medis dari layanan yang sudah diberikan. Untuk itu
ia perlu tahu **siapa yang mengerjakan** setiap layanan — dan itu data yang hanya modul layanan
yang tahu.

Audit terhadap source pada commit `09101d05` menemukan keadaan yang sangat tidak merata:

| Sumber layanan | Pelaksana tercatat | Mendukung tim | Ada peran |
|---|---|:---:|:---:|
| Kamar operasi | `OprTeamMember` — `WorkforceId`, `Role`, `IsLead` | **Ya** | **Ya** |
| Tindakan klinis | `TrxPatientProcedure.DoctorId` (wajib), `PerformedByUserId` (opsional) | Tidak | Tidak |
| Laboratorium | `LabOrder.ExaminerDoctorId` (boleh kosong) | Tidak | Tidak |
| Radiologi | **Tidak ada sama sekali** | Tidak | Tidak |

Kamar operasi sudah siap. Dua modul Anda menyimpan satu orang saja, tanpa peran. Inilah yang
diminta diperluas.

---

## 2. Mengapa satu orang tidak cukup

Transcript meeting RS MMC 15 Juli 2026 menyebut kelompok Laboratorium, Radiologi, Anestesi,
Digestive, dan Urologi masih dihitung manual lewat Excel "satelit", karena sistem belum bisa
menangani pembagian tim — satu tindakan dikerjakan beberapa dokter.

Audit kami menjelaskan sebabnya dengan tepat: **Laboratorium hanya menyimpan satu dokter, dan
Radiologi tidak menyimpan siapa pun.** Anestesi justru sudah tertangani — tetapi hanya untuk
kasus yang lewat kamar operasi.

Jadi keluhan itu bukan soal perhitungan yang rumit. Datanya memang tidak ada.

---

## 3. Yang diminta

Pencatatan tim beserta perannya, mengikuti pola `OprTeamMember` yang sudah berjalan di kamar
operasi. Isi minimumnya:

| Bidang | Tipe | Wajib | Kegunaan bagi Medical Fee |
|---|---|:---:|---|
| Rujukan tenaga medis | `Guid` | Ya | Siapa yang berhak atas jasa |
| Peran | `string` atau rujukan | Ya | Menentukan porsi; lihat bagian 4 |
| Penanda pelaksana utama | `bool` | Opsional | Membedakan operator utama dari pendamping |
| Rujukan layanan induk | `Guid` | Ya | Menaut ke tindakan atau pemeriksaan yang dikerjakan |

Bentuk tabel, nama bidang, dan apakah dijadikan tabel terpisah atau kolom tambahan — seluruhnya
terserah modul Anda. Medical Fee hanya menetapkan isi minimum yang dibutuhkan.

**Untuk Laboratorium ada satu permintaan tambahan:** `LabOrder.ExaminerDoctorId` saat ini boleh
kosong. Bila pemeriksa tidak diisi, jasanya tidak dapat dihitung — lihat bagian 6 untuk
perlakuannya.

---

## 4. Daftar peran yang seragam

Porsi jasa dibagi menurut **persentase per peran** yang ditetapkan pada aturan tarif sharing
(`MF-DEC-014`). Contoh: operator 60%, asisten 20%, anestesi 20% dari jasa satu tindakan.

Agar porsi itu berarti sama di seluruh modul, Medical Fee menetapkan **satu daftar peran** yang
berlaku untuk seluruh sumber layanan (`MF-DEC-016`), lalu memetakan peran kamar operasi yang
sudah ada ke dalamnya.

Yang diminta dari modul Anda: memakai daftar peran itu saat menambahkan pencatatan tim — bukan
membuat daftar sendiri. Tanpa itu, "operator" di tindakan klinis dan "operator" di kamar operasi
bisa berarti dua hal berbeda dan mendapat porsi berbeda.

Daftar perannya akan disusun bersama, dan masukan dari kedua modul Anda justru dibutuhkan:
peran apa saja yang nyata ada di tindakan poli dan di pemeriksaan laboratorium.

---

## 5. Untuk owner Radiologi

Radiologi **tidak diminta berubah sekarang.** Owner Medical Fee memutuskan menunda radiologi
dari rilis pertama (`MF-DEC-015`), justru karena datanya paling tidak siap — tidak ada satu pun
field yang mencatat siapa mengerjakan pemeriksaan atau siapa membaca hasilnya.

Selama ditunda, jasa radiologi tetap dihitung manual seperti sekarang. Bila kelak modul
Radiologi menambahkan pencatatan pelaksana, keputusan menunda itu dapat ditinjau ulang.

Berkas ini dikirimkan sebagai tembusan supaya rencana itu diketahui, bukan sebagai permintaan.

---

## 6. Perlakuan bila pelaksana tidak tercatat

Supaya jelas apa yang terjadi selama masa peralihan (`MF-DEC-018`):

| Keadaan | Yang dilakukan Medical Fee |
|---|---|
| Pelaksana tercatat lengkap | Jasa dihitung dan terbit normal |
| Pelaksana tidak tercatat | Layanan itu dicatat sebagai **belum dapat dihitung** beserta alasannya, dan terlihat di layar sebagai pekerjaan yang perlu dilengkapi |
| Pelaksana dilengkapi kemudian | Jasa terbit, tanpa perlu mengulang proses dari awal |

Yang **tidak** dilakukan: melewati layanan itu diam-diam, atau memberikan jasanya kepada dokter
penanggung jawab kunjungan. Keduanya pernah dipertimbangkan dan ditolak — yang pertama
mengulang persis masalah sekarang (jasa terlewat tanpa ketahuan), yang kedua berisiko memberi
jasa kepada orang yang tidak mengerjakan.

Konsekuensinya bagi modul Anda: selama pencatatan pelaksana belum ada, layanan dari modul itu
akan muncul sebagai daftar "belum dapat dihitung" yang panjang. Itu disengaja — supaya
terlihat, bukan supaya menyalahkan.

---

## 7. Pertanyaan untuk kedua modul

| # | Pertanyaan | Untuk |
|---:|---|---|
| 1 | Apakah bersedia menambahkan pencatatan tim beserta peran sesuai bagian 3? | Clinical + Laboratory |
| 2 | Peran apa saja yang nyata ada pada tindakan poli dan pemeriksaan laboratorium? | Clinical + Laboratory |
| 3 | Apakah `LabOrder.ExaminerDoctorId` dapat diwajibkan pengisiannya, atau ada alasan operasional mengapa ia boleh kosong? | Laboratory |
| 4 | Apakah satu tindakan klinis di poli memang bisa dikerjakan lebih dari satu orang, atau pada praktiknya selalu satu? | Clinical |

Pertanyaan keempat menentukan apakah perubahan di Clinical benar-benar perlu tabel tim, atau
cukup menambahkan peran pada satu pelaksana yang sudah ada.

---

## 8. Dampak bila permintaan ini belum turun

| Yang tertahan | Yang tetap jalan |
|---|---|
| Pembagian jasa untuk tim di luar kamar operasi | Jasa dari kamar operasi — datanya sudah lengkap |
| Jasa laboratorium untuk pemeriksaan yang pemeriksanya kosong | Jasa laboratorium yang pemeriksanya terisi, sebagai satu pelaksana |
| — | Aturan tarif sharing, perhitungan, verifikasi, persetujuan, periode, penyerahan ke Finance |

Rumpun yang tertahan sudah ditandai `OPEN DECISION` pada rencana Medical Fee dan dikeluarkan
dari gelombang pengerjaan, sehingga tidak ada pekerjaan yang dimulai lalu dibongkar.

---

## 9. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/medical-fee/01-existing-capability-map.md` bagian 6 | Tabel kesiapan data pelaksana per sumber layanan beserta buktinya |
| `docs/module-blueprints/medical-fee/00-interview-decisions.md` | `MF-DEC-003`, `014`, `016`, `018` beserta alasannya |
| `Referensi_Meeting_FIN-OQ_dan_Medical_Fee.pdf` | Rangkuman transcript meeting RS MMC |
