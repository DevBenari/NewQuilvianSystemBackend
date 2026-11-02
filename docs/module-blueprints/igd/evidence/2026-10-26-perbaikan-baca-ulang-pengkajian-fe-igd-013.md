# Perbaikan baca-ulang dokumen pengkajian IGD — `FE-IGD-013`, `FE-IGD-022` — 26 Oktober 2026 (petang)

| Field | Nilai |
| --- | --- |
| Dasar | `IGD-FACT-218` beserta koreksinya `IGD-FACT-229` — datanya tersimpan, tetapi layar IGD tidak pernah membacanya |
| Lingkup | Tiga berkas frontend; nol backend, nol kontrak, nol migration, nol komponen bersama |
| Bukti | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-fe-igd-013-20261026/` — `FE-IGD-013-R01-riwayat.png`, `jaringan-FE-IGD-013-R01.json`, `hasil.json`, `skrip/` |
| Verifikasi | ESLint **0 error / 4 warning**, identik baseline `HEAD` (diperiksa dengan `git stash`); nol komentar baru; `globals.css` tidak disentuh |
| Putusan | **Acceptance 1 `FE-IGD-013` terpenuhi.** Task tetap 🟡 karena acceptance 4 masih menunggu `IGD-OQ-141` |

---

## 1. Yang diperbaiki

Layar IGD hanya memanggil endpoint **daftar** pengkajian, yang DTO-nya sengaja tidak membawa ruas
dokumen lengkap. Akibatnya perawat tidak dapat melihat kembali apa yang baru saja ia simpan — dan
baris *Status lokalis* yang sudah lama terdaftar pada tab Riwayat selalu tampil kosong.

| # | Berkas | Perubahan |
| ---: | --- | --- |
| 1 | `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx` | Thunk baru `fetchPatientAssessmentDetail` memanggil `GET patient-assessments/{id}`, mengikuti pola `buildCreateThunk` yang sudah ada (`unwrap`, `normalizeError`, `isForbidden`). Reducer-nya **menggabungkan** detail ke item yang cocok pada `state.assessments.items` berdasarkan `id`; penolakan sengaja tidak menyentuh `error` bagian itu supaya kegagalan pengayaan tidak merusak daftar riwayat yang sudah tampil |
| 2 | `src/lib/hooks/.../emergency-assessment/use-emergency-assessment-detail.jsx` | Efek baru mengambil detail untuk tiap item riwayat yang belum diperkaya. Penjaganya `item.statusLokalis !== undefined` — ruas khusus DTO detail — ditambah `useRef` berisi himpunan id yang sudah diminta, sehingga tiap dokumen diambil **tepat sekali** dan tab yang dibuka berulang tidak memicu permintaan baru |
| 3 | `src/components/.../emergency-assessment-view/components/emergency-assessment-initial-tab.jsx` | Tujuh baris ruas ditambahkan ke daftar `fields` tab Riwayat: riwayat penyakit sekarang, riwayat pengobatan, catatan gizi, catatan risiko jatuh, catatan fungsional, catatan psikososial, dan catatan edukasi |

`EmergencyAssessmentRecordTab` **tidak diubah sama sekali** — ia memang sudah memetakan
`section.items` terhadap `fields`, sehingga memperkaya item sudah cukup membuat ruasnya terbit.

## 2. Pembuktian di layar

Akun perawat `dimas.kurniawan`, pasien `Pasien Uji K1-A 4287` (RM `00-00-02-71`), kunjungan
`IGD-261018123252-672191` — pengkajian yang sama yang disimpan Putaran L1 pada 22 Oktober.

**Rekaman jaringan** memuat **dua** panggilan detail per dokumen, keduanya `200`:

```
GET …/patient-assessments/425ce77f-7439-4de3-9613-14ff7394c859 → 200
GET …/patient-assessments/30934288-1874-4510-a40f-0d65f8aa4c90 → 200
```

Dua, bukan lebih — persis sejumlah dokumen pada riwayat, membuktikan penjaga sekali-ambil bekerja.

**`FE-IGD-013-R01-riwayat.png`** memperlihatkan kartu `ASM-20261022-00001` kini memuat nilai yang
sebelumnya mustahil tampil:

| Ruas di layar | Isi |
| --- | --- |
| Status lokalis | *"Status lokalis: regio thorax dada kiri tidak ada jejas deformitas"* |
| Riwayat penyakit sekarang | *"Hipertensi grade 2 terkontrol tidak teratur"* |
| Riwayat pengobatan | *"Amlodipine 10mg, Candesartan 16mg"* |
| Catatan gizi | *"Nafsu makan baik, tidak ada pantangan"* |
| Catatan fungsional | *"Pasien mandiri dengan pengawasan saat transfer"* |
| Catatan psikososial | *"Pasien cemas ringan, didampingi keluarga"* |
| Catatan edukasi | *"Edukasi pembatasan aktivitas fisik dan istirahat tirah baring"* |
| Catatan perawat | *"Pasien kooperatif, tirah baring dan pemantauan berkala"* |
| Catatan risiko jatuh | `-` |

*Catatan risiko jatuh* tampil `-` dan **itu benar**: payload uji menyatakan `hasFallRisk: false`,
sehingga server memang mengosongkannya (`IGD-FACT-229`).

Kartu `ASM-20261022-00002` menampilkan `-` pada beberapa ruas karena dokumen itu sendiri memang
tidak mengisinya — bukan gejala cacat yang sama.

### 2.1 Koreksi asersi skrip

Putaran pertama skrip melaporkan enam dari tujuh **label** *"TIDAK TAMPIL"* padahal seluruh
**nilai** tampil. Sebabnya asersi saya peka huruf besar-kecil, sedangkan label dirender huruf
kapital oleh CSS. Asersinya diperbaiki menjadi tidak peka huruf, lalu dijalankan ulang: **7 dari 7
label dan 7 dari 7 nilai TAMPIL**. Tangkapan layar pada putaran pertama pun sudah memperlihatkan
label itu — kekeliruannya di pengukur, bukan di layar.

## 3. Keadaan acceptance sesudah perbaikan

### `FE-IGD-013`

| # | Kriteria | Keadaan |
| ---: | --- | :---: |
| 1 | Pengkajian tersimpan lewat layar dan **benar-benar ada saat layar dibuka ulang** | ✅ — simpan terbukti `L1-04` (22 Okt), baca ulang terbukti di sini (26 Okt) |
| 2 | Setiap field yang tampil punya rumah di basis data | ✅ |
| 3 | Field yang belum punya rumah ditandai di laporan | ✅ |
| 4 | Tanda vital, diagnosis, tindakan, dan CPPT ikut dibuktikan | 🟡 — tanda vital, tindakan, dan CPPT ✅ (`L1-04`/`L1-05`); **diagnosis keperawatan terstruktur menunggu `IGD-OQ-141`** |

**Tetap 🟡.** Satu-satunya yang menahan kini keputusan pemilik, bukan pekerjaan kode.

### `FE-IGD-022`

Bagian keempatnya — delapan kolom pengkajian lanjutan — kini **dapat dibaca kembali dari layar**,
dan tujuh dari delapan kolom terbukti tersimpan lewat `L1-04`. Yang tersisa hanya `painNote`, yang
belum pernah diisi satu pun putaran. **Tetap 🟡**, menunggu satu putaran uji yang mengisinya.

## 4. Yang sengaja tidak dikerjakan

| Butir | Alasan |
| --- | --- |
| Memperluas DTO daftar | Tidak perlu — ruasnya memang dipisah ke DTO detail, dan jalur detail terbukti bekerja. Mengubahnya akan menyentuh pemakai lain |
| Menyentuh `EmergencyAssessmentRecordTab` | Tidak perlu — komponennya sudah generik |
| Menampilkan ruas detail pada tab SOAP dan Observasi | Di luar acceptance `FE-IGD-013`; tab Riwayat Assesmen Awal yang dipersoalkan |
| `painNote` | Belum pernah diisi putaran mana pun; menambah barisnya tanpa data uji hanya akan menampilkan `-` |
