# Nota Permohonan Penetapan — Pemegang Wewenang Klinis Modul Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-009` |
| `tanggal` | 2026-09-17 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium (`yogaaji452@gmail.com`) |
| `rujukan` | `LAB-SIGN-001`; `LAB-REQ-004` bagian 0 dan 9.1; `blueprint-manifest.md` revision `41` — `clinical_governance: belum ditetapkan` |
| `status` | `menunggu jawaban` |
| `ditujukan kepada` | Manajemen rumah sakit — Direktur, Direktur Pelayanan Medik, atau Komite Medis, sesuai tata kelola yang berlaku |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

---

## 1. Yang dimohonkan

**Satu nama.**

Rumah sakit belum menunjuk siapa yang berwenang menyatakan aturan klinis modul Laboratorium
benar. Selama nama itu belum ada, bagian sistem yang menyentuh keselamatan pasien **tidak boleh
dan tidak akan dibangun**.

Nota ini **tidak** meminta persetujuan atas aturan apa pun. Ia meminta penetapan **siapa yang
berhak memberi persetujuan itu**.

---

## 2. Kenapa nota ini terpisah dari permintaannya sendiri

`LAB-REQ-004` — permintaan tanda tangan klinis — sudah disusun lengkap dan diajukan pada
**2026-09-09**. Sampai hari ini belum dijawab, dan sebabnya bukan kelalaian siapa pun:

> **Dokumen itu tidak punya tujuan yang sah.** Ia ditujukan kepada "pemegang wewenang Clinical
> Governance modul Laboratorium", dan jabatan itu **belum pernah diisi**.

`LAB-REQ-004` bagian 9.1 menyediakan kolom penetapannya, tetapi kolom itu hanya dapat diisi oleh
pihak yang berwenang menunjuk — yaitu manajemen rumah sakit, bukan tim pembangun sistem dan bukan
pemilik modul. Karena itu permohonannya dipisahkan menjadi nota ini.

Delapan hari berlalu tanpa gerak, dan itu bukan karena permintaannya sulit. Ia hanya tidak punya
alamat.

---

## 3. Apa yang sedang tertahan

Modul Laboratorium sudah berjalan untuk bagian yang tidak menyentuh keselamatan pasien:
pendaftaran, pemesanan, wadah dan sampel, daftar kerja, pemantauan, serta data induknya. Yang
**belum boleh dibangun** adalah tiga hal berikut.

| Yang tertahan | Artinya bagi pekerjaan sehari-hari |
|---|---|
| Pengisian dan validasi hasil pemeriksaan | Hasil masih dicatat **di luar sistem**, seperti sekarang |
| Penandaan dan pelaporan nilai kritis | Pelaporan nilai kritis masih **lisan**, tanpa catatan yang dapat ditelusuri |
| Koreksi hasil yang sudah dirilis | **Belum ada** mekanisme resmi memperbaiki hasil yang terlanjur dipakai dokter |

Ketiganya tertahan oleh satu hal yang sama, dan penahannya **bukan** teknis maupun anggaran.
Pemilik modul sendiri menyatakan — lewat keputusan `LAB-DEC-011` — bahwa tiga aturan berikut
**bukan wewenangnya**, karena ketiganya menentukan hal yang bersifat klinis:

| Keputusan | Yang ditentukannya |
|---|---|
| `LAB-DEC-003` | **Siapa yang boleh menyatakan sebuah angka hasil benar** |
| `LAB-DEC-004` | **Apa yang terjadi ketika hasil menunjukkan pasien dalam bahaya** |
| `LAB-DEC-007` | **Apa yang terjadi ketika hasil yang sudah dipakai dokter ternyata salah** |

Ketiganya sudah dirumuskan lengkap dan sudah disetujui dari sisi produk dan operasional. Yang
dibutuhkan adalah pernyataan pihak klinis bahwa rumusannya memang benar — atau koreksinya bila
keliru.

### 3.1 Dua perkara baru ikut menunggu wewenang yang sama

Sejak `LAB-REQ-004` diajukan, requirement lapangan menambahkan dua hal yang jatuh pada wewenang
yang persis sama:

| Perkara | Isinya |
|---|---|
| `LAB-OPEN-029` | Requirement menyebut hasil hanya boleh dikirim ke pasien setelah disetujui **Profesor dan Dokter Lab**. Apakah itu sama dengan tanda tangan klinis `LAB-DEC-011`, dan siapa pemegangnya, belum terjawab |
| `LAB-OPEN-030` | Requirement memakai jabatan **`Dokter Lantai`** pada pelaporan nilai kritis. Jabatan itu **nol kemunculan** di seluruh blueprint — konsep peran yang belum dikenal sistem |

Keduanya diajukan lewat `LAB-REQ-007` pada 2026-09-16, dan keduanya menunggu nama yang sama.

---

## 4. Jabatan yang lazim memegangnya

Ditulis sebagai bahan pertimbangan, **bukan** usulan yang mengikat. Penetapannya sepenuhnya
wewenang manajemen rumah sakit.

| Calon | Pertimbangan |
|---|---|
| **Dokter penanggung jawab laboratorium** | Paling dekat dengan pekerjaan sehari-harinya; keputusan dapat diambil cepat oleh satu orang. Risikonya: bila berhalangan, tidak ada penggantinya |
| **Komite Medis sebagai lembaga** | Keputusannya lebih kuat secara tata kelola dan tidak bergantung pada satu orang. Risikonya: perlu waktu rapat |
| **Keduanya berlapis** | Dokter penanggung jawab menandatangani, Komite Medis mengetahui. Lebih lambat sedikit, tetapi tidak menggantung bila salah satu berhalangan |

Yang dibutuhkan sistem hanya **satu hal**: ada nama yang tercatat, dan nama itu berwenang.
Bentuknya perorangan atau lembaga sama-sama dapat dijalankan.

---

## 5. Yang terjadi setelah nama ditetapkan

Berurutan, supaya jelas nota ini membuka apa.

| # | Langkah | Pelaksana |
|---:|---|---|
| 1 | `LAB-REQ-004` dikirim kepada nama yang ditetapkan | Pemilik modul |
| 2 | Tiga keputusan ditandatangani, diubah, atau ditolak | Pemegang wewenang klinis |
| 3 | `LAB-REQ-007` bagian 3 dan 5 ikut terjawab | Pemegang wewenang klinis |
| 4 | Arsitektur domain untuk pengisian hasil disusun | Tim pembangun sistem |
| 5 | Kontrak diamandemen, pekerjaan dijadwalkan | Tim pembangun sistem |

**Langkah 4 pantas diketahui sejak awal supaya harapannya tepat.** Bagian pengisian hasil belum
pernah dirancang sama sekali — ia sengaja dikecualikan dari dokumen arsitektur karena aturannya
belum sah. Tanda tangan **tidak** membuat fiturnya langsung ada; ia membuat perancangannya boleh
dimulai.

---

## 6. Yang **tidak** dimohonkan

Agar tidak salah baca:

| Hal | Keadaan |
|---|---|
| Persetujuan atas ketiga aturan klinis | **Tidak di sini.** Itu isi `LAB-REQ-004`, dan hanya dapat dijawab setelah nota ini |
| Anggaran, perangkat, atau tambahan tenaga | Tidak diminta |
| Perubahan alur kerja laboratorium hari ini | Tidak ada. Selama tertahan, hasil tetap dicatat di luar sistem seperti sekarang |
| Penilaian atas kinerja siapa pun | Tidak. Penahan ini murni ketiadaan penetapan, bukan kelalaian |

---

## 7. Halaman penetapan

| Field | Isian |
|---|---|
| Nama pemegang wewenang Clinical Governance modul Laboratorium | |
| Jabatan | |
| Bentuk penetapan — perorangan / lembaga / berlapis | |
| Ditetapkan oleh | |
| Tanggal penetapan | |
| Berlaku sampai — bila dibatasi | |

**Pengganti bila berhalangan**, diisi bila penetapannya perorangan:

| Field | Isian |
|---|---|
| Nama pengganti | |
| Jabatan | |

### Pengesahan

| | Nama | Jabatan | Tanda tangan | Tanggal |
|---|---|---|---|---|
| Ditetapkan oleh | | | | |
| Diketahui | | | | |

---

## 8. Setelah diisi

Cukup kembalikan nota ini kepada pengaju. Nama yang tercatat akan dimasukkan ke
`blueprint-manifest.md` pada ruas `owners.clinical_governance`, dan `LAB-REQ-004` akan dikirimkan
kepada nama itu pada hari yang sama.

Bila manajemen menilai penetapan ini sebaiknya ditempuh dengan cara lain — surat keputusan
tersendiri, misalnya — jawaban itu juga cukup. Yang dibutuhkan modul ini adalah **nama yang sah**,
bukan bentuk dokumennya.
