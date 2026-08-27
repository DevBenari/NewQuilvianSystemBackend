# `BE-15` — Pemberitahuan kepada penulis CPPT tentang kolom Catatan Pribadi

| | |
|---|---|
| Task | `BE-15` |
| Sumber kewajiban | `RM-DEC-022` |
| Sifat pekerjaan | **Komunikasi, bukan kode** |
| Status | **Belum dijalankan.** Perlu dijalankan pemilik modul, bukan oleh pengembang |
| Penerima | Seluruh tenaga klinis yang menulis CPPT: dokter, perawat, dan profesi lain yang mengisi catatan terintegrasi |

---

## 1. Kenapa dokumen ini ada

Roadmap `BE-15` menyebut satu risiko yang **tidak dapat ditutup dengan kode**:

> Penulis CPPT selama ini menganggap kolom itu sepenuhnya pribadi. `RM-DEC-022` mewajibkan
> mereka diberi tahu bahwa tidak demikian. Ini pekerjaan komunikasi, bukan kode.

Definition of Done `BE-15` menuntut dua hal: endpoint berjalan, **dan** penulis CPPT sudah
diberi tahu. Butir pertama sudah selesai. Butir kedua belum, dan tidak boleh dianggap selesai
hanya karena kodenya jalan.

**Dokumen ini adalah bahan siap pakai untuk menjalankan butir kedua.** Isinya perlu ditinjau
pemilik modul dan bagian komunikasi rumah sakit sebelum disebarkan.

## 2. Apa yang sebenarnya berubah

Yang berubah **bukan** kolomnya. Kolom `PrivateNote` pada CPPT sudah ada sejak lama, dan isinya
tidak diubah, tidak dihapus, dan tidak dipindahkan.

Yang berubah adalah **siapa yang dapat membukanya, dan bagaimana caranya**.

| Sebelumnya | Sekarang |
|---|---|
| Tidak ada layar yang menampilkannya, sehingga terasa sepenuhnya pribadi | Dapat dibuka lewat satu jalur resmi yang tercatat |
| Tidak ada catatan siapa pernah membukanya | Setiap pembukaan dicatat: siapa, kapan, dan atas keperluan apa |
| Tidak ada izin khusus | Ada izin tersendiri yang harus diberikan secara sengaja |

Perlu ditegaskan supaya tidak menimbulkan salah paham: **kolom itu tidak pernah benar-benar
rahasia.** Ia tersimpan di basis data rumah sakit sejak awal, dan siapa pun yang memiliki akses
basis data dapat membacanya tanpa jejak apa pun. Perubahan ini justru **memperketat**, bukan
memperlonggar: pembukaan yang dahulu tidak terlihat sekarang menjadi tercatat dan dapat
ditelaah.

## 3. Pengaman yang berlaku

Tiga hal yang membedakan catatan pribadi dari isi rekam medis lainnya:

1. **Izin terpisah.** Hak membaca berkas rekam medis **tidak** memberi hak membuka catatan
   pribadi. Izinnya harus diberikan tersendiri kepada orang yang memang perlu.
2. **Keperluan akses selalu wajib.** Bahkan dokter yang sedang merawat pasien itu tetap harus
   memilih keperluan sebelum dapat membukanya. Untuk isi rekam medis lain, dokter yang sedang
   merawat tidak diminta alasan.
3. **Selalu ditandai untuk ditelaah.** Setiap pembukaan catatan pribadi masuk ke antrean
   tinjauan unit rekam medis, dan dihitung terpisah pada rekap bulanan.

## 4. Yang perlu disampaikan kepada penulis CPPT

Bahan berikut dapat dipakai apa adanya atau disesuaikan bahasanya.

> **Pemberitahuan: kolom Catatan Pribadi pada CPPT**
>
> Selama ini kolom **Catatan Pribadi** pada CPPT tidak pernah ditampilkan di layar mana pun,
> sehingga banyak di antara kita menulisnya dengan anggapan kolom itu sepenuhnya pribadi.
>
> Perlu kami sampaikan dengan terus terang: **kolom itu tidak pernah bersifat rahasia.** Isinya
> tersimpan di basis data rumah sakit sejak awal.
>
> Mulai berlakunya modul Rekam Medis, kolom itu **dapat dibuka lewat satu jalur resmi**, dengan
> pengaman berikut:
>
> - hanya oleh orang yang diberi izin khusus, terpisah dari izin membaca rekam medis;
> - selalu dengan memilih keperluan akses terlebih dahulu, tanpa kecuali;
> - dan setiap pembukaan tercatat serta ditelaah unit rekam medis.
>
> **Apa artinya bagi Anda saat menulis.** Perlakukan kolom Catatan Pribadi sebagai bagian dari
> rekam medis, bukan sebagai catatan pribadi Anda sendiri. Tuliskan hal yang memang perlu
> tercatat secara klinis, dengan bahasa yang siap dibaca rekan sejawat maupun penelaah.
>
> **Kolom ini tetap berguna.** Ia tempat yang tepat untuk keterangan klinis yang belum pantas
> ditampilkan pada ringkasan umum — misalnya dugaan yang belum terkonfirmasi, atau keterangan
> yang tidak boleh terbaca pengantar pasien. Yang berubah hanya satu: ia sekarang dapat
> dipertanggungjawabkan.

## 5. Langkah menjalankan

| Langkah | Penanggung jawab | Catatan |
|---|---|---|
| 1. Meninjau isi pemberitahuan | Pemilik modul dan clinical governance | Pastikan bahasanya sesuai kebiasaan setempat |
| 2. Menyepakati siapa yang berhak diberi izin `ReadPrivateNote` | Pemilik modul dan security/privacy | **Jangan** disamakan dengan hak baca rekam medis |
| 3. Menyampaikan pemberitahuan | Bagian komunikasi atau kepala unit | Sebelum modul dipakai, bukan sesudah |
| 4. Mencatat tanggal penyampaian | Pemilik modul | Isikan ke tabel di bawah sebagai bukti DoD |

## 6. Bukti penyampaian

Diisi setelah langkah 3 dijalankan. **Selama tabel ini kosong, Definition of Done `BE-15` belum
terpenuhi seluruhnya.**

| Kelompok penerima | Tanggal disampaikan | Cara | Penanggung jawab |
|---|---|---|---|
| | | | |

## 7. Hal yang masih perlu diputuskan

| Pertanyaan | Kenapa perlu dijawab |
|---|---|
| Siapa saja yang berhak diberi izin `MedicalRecord : ReadPrivateNote`? | Bila diberikan seluas hak baca rekam medis, seluruh pengaman ini kehilangan artinya |
| Apakah penulis perlu diberi tahu saat catatannya dibuka orang lain? | Tidak ada pada rilis pertama. Perlu diputuskan apakah menjadi kebutuhan berikutnya |
| Bagaimana perlakuan catatan pribadi yang sudah terlanjur ditulis sebagai catatan pribadi sungguhan? | Tidak ada pembersihan otomatis. Bila diperlukan, itu pekerjaan tersendiri dengan persetujuan clinical governance |
