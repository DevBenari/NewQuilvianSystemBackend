# Pemberitahuan Medical Fee kepada Owner HR

| Field | Nilai |
|---|---|
| Dari | Yasmin, owner modul Medical Fee |
| Untuk | Owner modul Human Resource |
| Tanggal | 20 September 2026 |
| Sifat | **Pemberitahuan dan tiga pertanyaan.** Tidak meminta HR mengubah apa pun sekarang |
| Menutup | `MF-CQ-06` |
| Dasar | `MF-DEC-012`, `approved` 20 September 2026 |
| Yang memblokir | Tidak ada. Berkas ini perlu **sepengetahuan** HR, bukan persetujuan atas bentuk internal Medical Fee |

Berkas ini berdiri sendiri, dapat dibaca tanpa membuka blueprint Medical Fee.

---

## 1. Inti pemberitahuan dalam satu paragraf

Modul Medical Fee yang sedang dibangun akan menghitung jasa tenaga medis berdasarkan tarif
sharing yang disepakati pada PKS/SK masing-masing. Tarif itu akan disimpan sebagai data Medical
Fee, tetapi **menunjuk baris kontrak yang sudah Anda kelola** di `WfpContractHistory` — bukan
menyalin atau membuat catatan kontrak sendiri.

Konsekuensinya bagi HR: data kontrak kerja kini menjadi rujukan perhitungan penghasilan, dan
perubahan status kontrak berdampak langsung pada jasa yang dihitung.

---

## 2. Mengapa menunjuk data HR, bukan membuat sendiri

Keputusan awal Medical Fee sebenarnya menyimpan data PKS sendiri, dengan alasan "HR belum punya
manajemen kontrak dan belum terjadwal". Audit terhadap source pada commit `09101d05`
membuktikan alasan itu **keliru**, dan keputusannya dicabut pada hari yang sama.

Yang ditemukan sudah berjalan di HR:

| Entity | Isi yang relevan |
|---|---|
| `WfpContractHistory` | Nomor kontrak, jenis kontrak, jenis kepegawaian, tanggal mulai dan berakhir, tanggal tanda tangan, status, penanda kontrak yang sedang berlaku, urutan perpanjangan, path dokumen |
| `MstContractType` | Master jenis kontrak |
| `TrxContractNonRenewal` | Kontrak yang tidak diperpanjang |
| `MstDoctor` | `ContractTypeId`, `ContractStartDate`, `EmploymentTypeId`, `PracticeType` dengan bawaan `FullTime` |

Membuat catatan kontrak kedua di Medical Fee akan melahirkan dua sumber kebenaran untuk hal yang
sama — dan yang lebih berbahaya, tarif sharing tidak akan tahu ketika kontrak seseorang berakhir
di HR.

---

## 3. Yang Medical Fee tambahkan, dan yang tidak

| Medical Fee menambahkan | Medical Fee TIDAK menyentuh |
|---|---|
| Kesepakatan tarif sharing yang menunjuk satu baris `WfpContractHistory` | Isi kontrak kerja itu sendiri |
| Persentase jasa per layanan per peran | Jenis kontrak, jenis kepegawaian, status |
| Masa berlaku tarif, bila berbeda dari masa kontrak | Masa berlaku kontrak |
| — | Dokumen kontrak dan path-nya |

Medical Fee **tidak pernah menulis** ke tabel HR mana pun. Hubungannya satu arah: membaca dan
menunjuk.

---

## 4. Konsekuensi yang perlu HR ketahui

Inilah bagian terpenting berkas ini.

| Kejadian di HR | Akibat di Medical Fee |
|---|---|
| Kontrak seseorang berakhir | Tarif sharing yang menunjuk kontrak itu **ikut berhenti berlaku**. Layanan setelah tanggal itu tidak menghasilkan jasa sampai ada kontrak baru beserta tarifnya |
| Kontrak diperpanjang sebagai baris baru | Tarif sharing **tidak ikut berpindah sendiri**. Perlu kesepakatan tarif baru yang menunjuk kontrak penerus |
| Status kontrak diubah | Berdampak pada kelayakan perhitungan jasa periode berjalan |
| Baris kontrak dikoreksi | Jasa yang sudah dihitung pada periode yang sudah ditutup **tidak berubah**; koreksi berlaku ke depan |

Baris kedua yang paling mudah terlewat: perpanjangan kontrak tidak otomatis membawa tarif
sharing-nya. Itu disengaja — tarif adalah kesepakatan tersendiri yang bisa saja berubah saat
kontrak diperpanjang.

---

## 5. Tiga pertanyaan untuk HR

| # | Pertanyaan | Mengapa penting |
|---:|---|---|
| 1 | Apakah HR keberatan datanya menjadi rujukan perhitungan penghasilan seperti dijelaskan di atas? | Menaikkan tingkat kepentingan data kontrak: kekeliruan di sana kini berdampak pada uang |
| 2 | Apakah `WfpContractHistory` juga dipakai untuk **dokter tamu dan dokter paruh waktu**, atau hanya untuk karyawan tetap? | Transcript meeting menyebut tarif sharing berbeda untuk Full Time, Part Time, dan Dokter Tamu. Bila dokter tamu tidak punya baris kontrak di HR, Medical Fee perlu jalan lain untuk kelompok itu |
| 3 | Apakah ada rencana HR menambahkan sisi **tarif atau bagi hasil** ke dalam manajemen kontrak? | Bila ya, sebaiknya Medical Fee menunggu atau menyesuaikan, alih-alih membangun yang serupa |

Pertanyaan kedua yang paling mendesak. Bila dokter tamu tidak tercatat di `WfpContractHistory`,
justru kelompok itulah yang tarif sharing-nya paling sering berbeda-beda — dan paling butuh
tercatat.

---

## 6. Rujukan

| Berkas | Isi |
|---|---|
| `docs/module-blueprints/medical-fee/00-interview-decisions.md` | `MF-DEC-012` beserta alasan dan keputusan yang digantikannya |
| `docs/module-blueprints/medical-fee/01-existing-capability-map.md` bagian 5 | Temuan lengkap tentang `WfpContractHistory` beserta buktinya |
