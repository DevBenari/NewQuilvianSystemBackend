# PLAN-REPAIR-016 — Perjelas admisi bayi, kelayakan penempatan, dan penagihan ibu–bayi

~~~yaml
plan_id: PLAN-REPAIR-EPS-016
issue: ../issue/issue-016-alur-admisi-episode-bayi.md
status_rencana: DISETUJUI
tanggal_rencana: "2026-10-14"
tanggal_pembaruan_skema: "2026-10-14"
basis_kebutuhan_tambahan: "Rekap meeting, issue bagian 1.3, klarifikasi dua invoice, dan instruksi pengguna memperbarui stepper"
diputuskan_oleh: "Pengguna - instruksi kerjakan sampai tuntas, pasien baru dan pasien lama"
tanggal_keputusan: "2026-10-14"
basis_source_backend: "9f8d82666e8d67dde203d64ffab640305efc90ab (MHamzah), perubahan lokal eksternal dicatat pada issue"
basis_source_frontend: "88cb19f0c8edd998fb7a19701f0f52ac9aab839b (HamzahV2), working tree existing; sidik berkas pada issue"
wewenang_saat_ini: "Implementasi source admisi bayi BE/FE untuk pasien baru dan pasien lama; bukan penerapan DB, deployment atau publikasi Git"
~~~

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| FIX-EPS-016-01 | ISS-01, ISS-03 | Kunci cabang layanan dan amendemen desain admisi bayi | Dokumen/desain | 1 | BE-RWI-231 / FE-RWI-245 | ✅ SELESAI - amendemen cabang bayi disetujui | [kontrak](../../contracts/newborn-admission-repair-016.md) |
| FIX-EPS-016-02 | ISS-02 | Mode form bayi dan payload master pasien yang benar | FE | 1 | FE-RWI-245 | ✅ SELESAI SOURCE - build/penerapan berikutnya oleh pemilik, 2026-10-15 | [laporan FE](../../task/report/frontend/FE-RWI-245.md) |
| FIX-EPS-016-03 | T1 | Konsistensi pasien ibu, episode ibu, dan hubungan historis | BE | 1 | BE-RWI-231 | ✅ SELESAI pada source/build; ibu tidak dikenal tetap di luar kontrak | [laporan BE](../../task/report/backend/BE-RWI-231.md) |
| FIX-EPS-016-04 | ISS-04 | Kelayakan neonatal di seluruh transaksi penempatan | BE | 1 | BE-RWI-231 | ✅ SELESAI pada source/build; penerapan DB terpisah | [laporan BE](../../task/report/backend/BE-RWI-231.md) |
| FIX-EPS-016-05 | ISS-04 | Daftar tempat sesuai konteks bayi di layar admisi | FE | 1 | FE-RWI-245 | ✅ SELESAI SOURCE - build/penerapan berikutnya oleh pemilik, 2026-10-15 | [laporan FE](../../task/report/frontend/FE-RWI-245.md) |
| FIX-EPS-016-06 | ISS-01, ISS-03 | Stepper kondisional dengan keluaran tiap tahap | FE | 2 | FE-RWI-245 | ✅ SELESAI SOURCE - build/penerapan berikutnya oleh pemilik, 2026-10-15 | [laporan FE](../../task/report/frontend/FE-RWI-245.md) |
| FIX-EPS-016-07 | T2 | Bedakan daftar ibu memuat, kosong, gagal, dan pilihan tersimpan | FE | 2 | FE-RWI-245 | ✅ SELESAI SOURCE - build/penerapan berikutnya oleh pemilik, 2026-10-15 | [laporan FE](../../task/report/frontend/FE-RWI-245.md) |
| FIX-EPS-016-08 | T3 | Jalur bayi yang sudah mempunyai RM dan rawat ulang | FE | 2 | FE-RWI-245 | ✅ SELESAI SOURCE - build/penerapan berikutnya oleh pemilik, 2026-10-15 | [laporan FE](../../task/report/frontend/FE-RWI-245.md) |
| FIX-EPS-016-09 | ISS-05 | Handoff kontrak dua invoice dalam satu kelompok pembayaran | Billing + integrasi, desain | 2 | BE-RWI-231 / FE-RWI-245 | 🟡 SEBAGIAN - preferensi tersimpan; settlement lintas invoice belum diimplementasikan | [kontrak](../../contracts/newborn-admission-repair-016.md) |

**Pembaruan implementasi 14 Oktober 2026:** pengguna menyetujui source admisi bayi untuk pasien baru dan pasien lama. Delapan perbaikan mempunyai desain/source terpasang; preferensi dua invoice juga tersimpan. BE-RWI-231 selesai pada source/build; FE-RWI-245 sebagian karena pengemasan standalone belum selesai. Lima skenario browser API tiruan dan lint final lulus. Suite 2.869 uji: 2.842 lulus, 27 gagal yang juga ada di HEAD; nol kegagalan baru. Detail bukti dan batas ada pada laporan task.

Tidak menganggap payment engine lintas invoice, identitas ibu tidak dikenal, target response time, atau penerapan database selesai. Migration disiapkan untuk diterapkan pemilik sesuai AGENTS.md. Dokumen ini tetap mempertahankan uraian diagnosis historis; kontrak implementasi scope baru/lama dikunci pada [RWI-BAYI-016-1.0](../../contracts/newborn-admission-repair-016.md).

## 2. Solusi terpilih per temuan

Solusi terpilih berarti rekomendasi analisis. Kebijakan rumah sakit yang belum diputuskan tetap mengikuti tabel keputusan, bukan dianggap disetujui.

| Temuan | Opsi A | Opsi B | Solusi terpilih dan alasan |
| --- | --- | --- | --- |
| ISS-01 | Semua bayi wajib memilih episode ibu aktif | Cabang asal kelahiran dan perawatan, dengan hubungan pasien ibu terpisah | B; mencakup rujukan dan rawat ulang tanpa membuat episode ibu fiktif |
| ISS-02 | Tambahkan petunjuk di form umum | Form yang sama diberi mode bayi dengan field dan payload sesuai | B; memperbaiki makna data dan mempertahankan reuse komponen |
| ISS-03 | Pertahankan 12 langkah, ganti beberapa judul | Tahap bayi kondisional, reservasi digabung dalam tampilan penempatan, cetak menjadi aksi akhir | B; mengurangi keputusan semu; transaksi booking dan placement tetap terpisah |
| ISS-04 | Filter semua bayi ke RoomType BabyRoom | Pilih sumber daya bayi menurut rawat gabung/neonatal/isolasi | B; boks di kamar ibu tetap didukung dan server menjaga kesesuaian |
| ISS-05 | Hapus dua invoice dan pindahkan semua item ke satu invoice | Kelompok penagihan/pembayaran yang menunjuk dua invoice asal | B; sesuai jawaban pengguna tentang dua tagihan dan pembayaran bersama; jejak subjek biaya tetap utuh |
| T1 | Cukup memeriksa episode ibu ada | Cocokkan hubungan pasien, konteks perawatan, dan penggunaan referensi historis | B; mencegah salah taut dan hambatan setelah ibu pulang |
| T2 | Kegagalan API dianggap daftar kosong | Keadaan gagal dengan coba lagi; pemulihan pilihan dan pagination | B; petugas mengetahui tindakan pemulihan yang benar |
| T3 | Semua bayi selalu dibuat sebagai pasien baru | Gunakan profil bayi yang ada, lalu episode baru sesuai konteks | B; mencegah RM ganda dan tidak bergantung pada pilihan kategori sementara |

Opsi tetap 12 langkah masih memungkinkan bila pemilik ingin perubahan navigasi lebih kecil. Namun setiap tahap tetap harus mempunyai mode bayi dan label keluaran yang jelas; mengganti teks saja tidak menyelesaikan builder pasien atau kelayakan bed.

## 3. Skema tampilan sebelum → sesudah

Bagian ini diperinci atas instruksi pengguna 14 Oktober 2026 setelah analisis rekap meeting. Seluruh tampilan SESUDAH adalah **usulan skema**, bukan screenshot aplikasi yang sudah berubah. Dasar meeting dan batas penafsirannya ada pada [issue bagian 1.3](../issue/issue-016-alur-admisi-episode-bayi.md#13-bukti-tambahan-dari-rekap-meeting-admisi). Status rencana dan keputusan K-01–K-07 tetap dibaca terpisah.

### 3.1 SEBELUM — satu alur umum 12 langkah

~~~text
ADMISI RAWAT INAP
1 Jenis Kunjungan → 2 Tipe Pasien → 3 Pendaftaran
→ 4 Pembayaran → 5 Deposit → 6 Dokter
→ 7 Pilih Bed → 8 Booking Bed → 9 Konfirmasi
→ 10 Form Persetujuan → 11 Cetak Persetujuan → 12 Kartu Pasien

LANGKAH 2 — TIPE PASIEN
( ) Pasien Umum    (•) Bayi Baru Lahir    ( ) Pegawai RS
Episode Ibu *
[ Pilih episode ibu yang sedang aktif dirawat                  ▾ ]
[ Lanjut ke Pendaftaran ]

LANGKAH 3 — PENDAFTARAN
[ Scan KTP ] → Form identitas umum → Kontak darurat
[ Simpan & Lanjut ke Pembayaran ]
~~~

Skema SEBELUM mengikuti screenshot dan pembacaan source pada issue. Belum ada pemisahan tampilan perawatan bayi, identitas ibu/wali, dan rencana penagihan keluarga.

### 3.2 SESUDAH — kerangka halaman khusus admisi bayi

Jalur ini dibuka setelah petugas memilih kategori Bayi Baru Lahir pada admisi. Kategori yang sudah dipilih tampil pada header; petugas tidak mengisi kategori bayi berulang kali. Bayi yang sudah mempunyai RM masuk melalui pencarian pasien existing, lalu memakai tahapan yang sesuai.

~~~text
ADMISI BAYI
Daftarkan bayi, verifikasi hubungan ibu, dan siapkan perawatannya.

BAYI       Bayi Rani | RM: belum tersimpan | Lahir: 14-10-2026
IBU        Ny. Rani  | RM: RM-DEMO-I01    | Episode: RI-DEMO-I01
PERAWATAN  Belum diverifikasi            | Admisi: Sedang disiapkan

✓ Konteks   ● Identitas   ○ Perawatan   ○ Penjamin   ○ Rencana Deposit
○ Penempatan   ○ Persetujuan Wali   ○ Tinjau & Aktifkan   ○ Dokumen
Langkah 2 dari 9 — Identitas Bayi dan Ibu

[ ISI LANGKAH AKTIF                                               ]

Data tersimpan terakhir: 14-10-2026 04:05 WIB
[ Batalkan Admisi ]   [ Kembali ]           [ Simpan & Lanjut ]
~~~

Contoh memakai nama dan nomor samaran. Header diperbarui dari hasil penyimpanan yang sah. Sebelum ibu/bayi dipilih, wilayah tersebut menampilkan “Belum dipilih”, tanpa data contoh yang menyerupai hasil nyata.

| Wilayah | Isi | Sumber data | Komponen |
| --- | --- | --- | --- |
| Header | Bayi, ibu, rencana perawatan, keadaan admisi | Pilihan terverifikasi dan data tersimpan | Hero serta ringkasan pasien existing |
| Stepper | Tahap berlaku, tahap aktif, bagian perlu diperiksa ulang | Keadaan alur dan hasil simpan | InpatientAdmissionStepper diperluas untuk bayi |
| Isi tahap | Isian dan ringkasan khusus tahap aktif | Sumber sesuai tabel tiap langkah | Komponen form/view existing dengan mode bayi |
| Footer | Simpan, kembali, batalkan, waktu simpan | Hasil operasi yang berhasil | BaseButton dan InformationAlert |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Simpan & Lanjut | Utama | Dapat ditekan oleh petugas berhak; dicegah saat request berjalan | Validasi, simpan sesuai tahap, lalu maju bila berhasil |
| Kembali | Kedua | Tidak sedang menyimpan | Kembali dengan data dipertahankan; perubahan belum tersimpan diberi pilihan penanganan |
| Batalkan Admisi | Kedua | Sesuai hak dan status episode | Jelaskan dampak, konfirmasi pembatalan, lepaskan reservasi melalui operasi sah |

| Keadaan | Perilaku umum seluruh langkah |
| --- | --- |
| Memuat | Jelaskan data yang sedang dimuat; jangan tampilkan nilai nol atau status siap sementara |
| Kosong | Jelaskan data yang belum tersedia dan tindakan yang relevan |
| Gagal | Pertahankan isian, tampilkan pesan dan Coba Lagi; jangan menganggap operasi berhasil |
| Tidak berhak | Jelaskan tindakan/akses yang kurang tanpa membocorkan data pasien lain |
| Validasi gagal | Ringkasan kesalahan dapat diklik; fokus ke field pertama; tahap belum dinyatakan selesai |
| Perubahan data hulu | Tandai tahap terdampak “Perlu diperiksa ulang”, tanpa menghapus data tersimpan diam-diam |

Pada layar sempit, header ditumpuk; stepper menampilkan tahap aktif dan akses “Lihat semua langkah”. Label dan pesan tidak bergantung pada warna saja. Angka tahap dihitung dari daftar yang berlaku, bukan disimpan sebagai identitas tahap.

### 3.3 Daftar stepper dan keluaran

| ID dokumen | Urutan maksimum | Judul di layar | Keluaran | Berlaku |
| --- | ---: | --- | --- | --- |
| BAYI-S01 | 1 | Konteks Admisi Bayi | Jalur kelahiran lokal, datang dari luar, atau rawat ulang | Semua |
| BAYI-S02 | 2 | Identitas Bayi dan Ibu | RM bayi, data lahir, hubungan ibu, kontak wali | Semua; mode verifikasi untuk RM existing |
| BAYI-S03 | 3 | Perawatan dan DPJP Bayi | Kebutuhan perawatan terverifikasi dan dokter bayi | Semua |
| BAYI-S04 | 4 | Penjamin dan Penagihan | Penjamin bayi, penanggung biaya, preferensi dua invoice | Semua |
| BAYI-S05 | 5 | Rencana Deposit | Nominal/kebijakan deposit dan status pembayaran dari kasir | Bila berlaku |
| BAYI-S06 | 6 | Penempatan dan Pemesanan | Tempat bayi sesuai kebutuhan, reservasi yang sah | Semua |
| BAYI-S07 | 7 | Persetujuan Wali | Data wali dan persetujuan/revisi yang tersimpan | Sesuai SOP |
| BAYI-S08 | 8 | Tinjau dan Aktifkan | Hasil akhir admisi dan penempatan yang dikonfirmasi server | Semua |
| BAYI-S09 | 9 | Dokumen Bayi | Gelang dengan identitas ibu, label dan dokumen lain | Semua |

ID BAYI-Sxx hanya penanda dalam dokumen, bukan task ID atau nama endpoint. Jika deposit tidak berlaku, ada delapan tahap dan nomor sesudahnya bergeser. Tampilan contoh berikut memakai sembilan tahap agar seluruh skema terlihat.

### 3.4 BAYI-S01 — Konteks Admisi Bayi

**Tujuan:** menetapkan asal proses dan memeriksa apakah bayi sudah terdaftar. **Sebelum:** Jenis Kunjungan dan Tipe Pasien belum menjelaskan jalur bayi. **Sesudah:**

~~~text
LANGKAH 1 DARI 9 — KONTEKS ADMISI BAYI
Kategori: Bayi Baru Lahir

Jalur admisi *
(•) Episode kelahiran di RS ini
( ) Datang dari luar RS / rujukan
( ) Kembali dirawat setelah pernah pulang

EPISODE KELAHIRAN DI RS INI
Catatan kelahiran [ Cari ibu / catatan persalinan                 ▾ ]
Terpilih: Ny. Rani | Melahirkan 14-10-2026 03:20 WIB
Bayi terlapor: 1 | Sumber: catatan unit persalinan

Pemeriksaan pasien bayi
[ Cari bayi yang sudah mempunyai RM                             ▾ ]
Hasil: Belum ada pasien bayi yang dipilih.

[ Kembali ke Pilihan Kategori ]       [ Lanjut ke Identitas Bayi ]
~~~

| Wilayah | Isi/aturan isian | Sumber | Komponen |
| --- | --- | --- | --- |
| Jalur admisi | Pilihan tunggal wajib; rawat ulang dibedakan dari tempat kelahiran | Petugas + riwayat pasien | BaseCheckboxCard sebagai radio |
| Lahir lokal | Catatan kelahiran bila tersedia; jalur verifikasi manual jika integrasi belum tersedia | ClinicalManagement/unit persalinan | ResourceFilterSelect + ringkasan |
| Datang dari luar | Asal fasyankes, jenis kedatangan dan informasi rujukan sesuai konteks | Dokumen rujukan/isian terverifikasi | Field rujukan existing |
| Rawat ulang | Cari RM bayi, tampilkan episode terdahulu dan periksa episode aktif | PatientManagement dan InPatientManagement | Pencarian pasien existing |
| Pemeriksaan duplikasi | Jangan memilih bayi hanya berdasarkan nama ibu yang sama | Profil, tanggal lahir, urutan kembar | Daftar hasil dengan identitas pembeda |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Lanjut ke Identitas Bayi | Utama | Dapat ditekan kecuali proses berjalan | Validasi jalur dan referensi, lanjut dengan konteks yang dipilih |
| Gunakan Bayi Terdaftar | Kedua | Ada hasil yang dipilih dan berhak | Pakai RM existing; tahap berikutnya menjadi verifikasi |
| Kembali ke Pilihan Kategori | Kedua | Tidak sedang mutasi | Keluar dari cabang bayi dengan data sementara ditangani secara jelas |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | “Memuat catatan kelahiran/pasien bayi…” |
| Kosong | Tawarkan verifikasi sumber kelahiran manual yang berwenang; jangan membuat catatan klinis fiktif |
| Gagal | Coba Lagi; jalur manual hanya bila SOP/kontrak mengizinkan |
| Tidak berhak | Referensi yang tidak dapat dibaca tidak ditampilkan sebagai data kosong |
| Bayi mempunyai episode aktif | Tampilkan episode yang sah dan arahkan ke kelanjutan/detail, cegah admisi ganda |
| Bayi kembar | Tampilkan tiap bayi dengan urutan dan data lahir, bukan satu baris atas nama ibu saja |

**Validasi dan hasil:** jalur kelahiran lokal mengacu kelahiran hidup yang sudah terlapor. Bayi pernah pulang menggunakan RM yang sama dan episode baru. Untuk datang dari luar, tempat kelahiran sebenarnya tetap dicatat pada identitas; tidak otomatis dianggap lahir di fasyankes perujuk. Integrasi catatan kelahiran adalah target yang perlu diaudit, bukan kemampuan yang dinyatakan sudah tersedia.

### 3.5 BAYI-S02 — Identitas Bayi dan Ibu

**Tujuan:** menyimpan identitas bayi dan hubungan keluarga. **Sebelum:** form pendaftaran umum dengan scan KTP pasien. **Sesudah:**

~~~text
LANGKAH 2 DARI 9 — IDENTITAS BAYI DAN IBU

IBU BAYI
Pasien ibu * [ Cari nama / RM / identitas ibu                    ▾ ]
Ny. Rani | RM-DEMO-I01                         [ Lihat Ringkasan ]
Episode terkait [ RI-DEMO-I01 — Persalinan / masih dirawat        ▾ ]
Lokasi ibu: Lantai 2 / Sayap A / Kamar Melati / Bed IBU-A

IDENTITAS BAYI
Nama *
(•) Nama awal dari ibu: Bayi Rani
( ) Nama resmi sudah tersedia [ Nama lengkap bayi                 ]
Tanggal lahir * [ 14-10-2026 ]  Jam [ 03:20 ]  Zona [ WIB ]
Jenis kelamin * [ Perempuan                                      ▾ ]
Kelahiran * (•) Tunggal  ( ) Kembar → Urutan [  ] dari [  ]
NIK bayi [ Belum tersedia ]  Dokumen kelahiran [ Referensi         ]

ALAMAT DAN KONTAK
[✓] Gunakan alamat orang tua yang sudah diverifikasi
Alamat [ Alamat contoh keluarga                                  ]
Kontak * [ Nama orang tua/wali ] Hubungan * [ Ayah                ▾ ]
No. HP * [ Nomor kontak terverifikasi                             ]

[ Kembali ]                          [ Simpan Identitas & Lanjut ]
~~~

| Wilayah | Isi/aturan isian | Sumber | Komponen |
| --- | --- | --- | --- |
| Ibu | Pilih pasien ibu, tampilkan identitas pembeda; episode terkait berbeda dari identitas ibu | PatientManagement + episode | ResourceFilterSelect dan ringkasan |
| Nama bayi | Saran “Bayi [Nama Ibu]” mengikuti meeting; nama resmi jika tersedia | Ibu terverifikasi/keluarga | Mode bayi NewPatientForm |
| Data lahir | Tanggal dan jenis kelamin wajib pada jalur lengkap; jam tidak diketahui tidak diisi 00:00 | Catatan kelahiran/rujukan terverifikasi | Base date/time/select existing |
| Tunggal/kembar | Urutan dalam persalinan ini; bukan anak keberapa dalam keluarga | Catatan kelahiran | Radio dan field angka |
| Identitas kependudukan | NIK bayi bila ada; identitas ibu berada pada wilayah ibu | Dokumen terverifikasi | Field identitas kontekstual |
| Kontak/alamat | Saran dari orang tua perlu konfirmasi; kontak yang dapat dihubungi | Kontak pasien/keluarga | EmergencyContactSection disesuaikan |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Simpan Identitas & Lanjut | Utama | Dapat ditekan; cegah submit saat menyimpan | Validasi dan simpan bayi newborn atau gunakan RM existing, lalu maju |
| Lihat Ringkasan Ibu | Kedua | Ibu terpilih dan hak baca cukup | Buka ringkasan agar petugas memverifikasi orang yang tepat |
| Kembali | Kedua | Tidak sedang mutasi | Pertahankan isian; jangan membuat pasien kedua |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | “Memuat identitas ibu/bayi…”; nilai saran belum dianggap terverifikasi |
| Kosong | Ibu tidak ditemukan dapat melalui pencatatan identitas ibu yang sah; jangan membuat episode ibu fiktif |
| Gagal | Pertahankan data; retry melanjutkan operasi yang gagal tanpa duplikasi bayi |
| Tidak berhak | Koreksi identitas diarahkan ke petugas berwenang |
| RM existing | Identitas tersimpan tampil sebagai verifikasi; perubahan memakai jalur koreksi, bukan mengganti kategori sementara |
| Ibu tidak diketahui | Gunakan prosedur identitas sementara hanya setelah SOP/kontrak tersedia; jangan menebak ibu |
| Kemungkinan duplikasi | Tampilkan kandidat dengan tanggal/jam, jenis kelamin dan urutan kembar untuk verifikasi |

**Validasi dan hasil:** IsNewborn dan hubungan pasien ibu harus tersimpan konsisten. Scan KTP bila dipakai berada pada identitas ibu/wali dan tidak mengisi data bayi. Untuk rujukan atau rawat ulang, episode ibu lokal tidak wajib. Episode aktif/lokasi ibu menjadi syarat ketika rawat gabung dipilih pada tahap perawatan. Untuk kembar, label pembeda diusulkan setelah format disepakati. Perubahan menjadi nama resmi mempertahankan RM dan riwayat, bukan membuat pasien baru.

### 3.6 BAYI-S03 — Perawatan dan DPJP Bayi

**Tujuan:** menggunakan kebutuhan dari tim klinis untuk menentukan konteks perawatan dan dokter bayi. **Sebelum:** langkah Dokter tidak menjelaskan perawatan bayi. **Sesudah:**

~~~text
LANGKAH 3 DARI 9 — PERAWATAN DAN DPJP BAYI
Bayi Rani | RM-DEMO-B01 | Ibu: Ny. Rani

SUMBER KEBUTUHAN PERAWATAN
Permintaan [ Unit Persalinan / catatan terverifikasi             ▾ ]
Diverifikasi oleh: Petugas Klinis A | 14-10-2026 03:45 WIB

Rencana * (•) Rawat gabung dengan ibu   ( ) Neonatal terpisah
Kebutuhan unit [ Mengikuti keputusan tim klinis                  ]
Kebutuhan isolasi [ Tidak diperlukan — sesuai permintaan klinis  ]

DPJP BAYI *
[ Pilih dokter penanggung jawab bayi                            ▾ ]
Dokter tambahan [ Tambahkan sesuai penugasan yang sah            ▾ ]

RINGKASAN
Rawat gabung: ibu mempunyai penempatan aktif yang sesuai.
Dokter ibu: informasi pembanding; belum menjadi DPJP bayi.

[ Kembali ]                       [ Simpan Perawatan & Lanjut ]
~~~

| Wilayah | Isi/aturan isian | Sumber | Komponen |
| --- | --- | --- | --- |
| Sumber klinis | Referensi permintaan dan pelaku/waktu verifikasi | ClinicalManagement/keperawatan | Pencarian dan ringkasan existing |
| Rencana perawatan | Rawat gabung/neonatal terpisah berdasarkan keputusan klinis | Tim klinis | Kartu pilihan dan field kontekstual |
| Kebutuhan unit/isolasi | Dibaca dari kebutuhan terverifikasi; koreksi hanya sesuai kewenangan | Permintaan klinis | Select/summary existing |
| DPJP bayi | Wajib sesuai kontrak episode; pilih dokter bayi sendiri | Master dokter/penugasan | InpatientAdmissionDoctorStep |
| Dokter tambahan | Role penugasan ditampilkan terpisah dari DPJP utama | Penugasan yang sah | Reuse pemilih dokter tambahan |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Simpan Perawatan & Lanjut | Utama | Dapat ditekan kecuali proses berjalan | Validasi keputusan klinis, kelayakan rawat gabung dan dokter bayi |
| Tambah Dokter | Kedua | Hak penugasan cukup dan aturan jumlah mengizinkan | Tambah penugasan sesuai role, tidak otomatis DPJP kedua |
| Kembali | Kedua | Tidak sedang mutasi | Kembali ke identitas dengan data tetap tersimpan |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | Jelaskan permintaan atau daftar dokter yang sedang dimuat |
| Kosong | “Kebutuhan perawatan belum diverifikasi”; arahkan koordinasi tim klinis |
| Gagal | Coba Lagi; data klinis gagal dibaca tidak dianggap “tidak perlu isolasi” |
| Tidak berhak | Petugas admisi tidak mengubah kebutuhan klinis tanpa kewenangan |
| Ibu tidak sedang dirawat | Jelaskan alasan rawat gabung belum layak; jangan otomatis memilih perawatan lain |
| Kebutuhan berubah | Tandai penempatan/deposit/consent yang terdampak untuk diperiksa ulang |

**Validasi dan hasil:** DPJP bayi tidak disalin otomatis dari ibu. Ringkasan meeting tentang satu/dua DPJP dan maksimal lima dokter belum konsisten; jumlah dan role mengikuti kontrak resmi yang dikunci, bukan angka baru dari skema ini. Jenis unit neonatal dan level kemampuannya menunggu K-04. Kondisi rawat gabung mengikuti penilaian klinis; opsi ini tidak menjadi default hanya karena ibu terpilih.

### 3.7 BAYI-S04 — Penjamin dan Penagihan

**Tujuan:** memverifikasi penjamin bayi dan mencatat rencana penagihan keluarga. **Sebelum:** tahap “Pembayaran” belum membedakan pemilihan penjamin dari transaksi kasir. **Sesudah:**

~~~text
LANGKAH 4 DARI 9 — PENJAMIN DAN PENAGIHAN

PENJAMIN BAYI *
Jenis [ Asuransi                                                ▾ ]
Penjamin [ Penjamin contoh                                      ▾ ]
Peserta/polis [ Data bayi yang terverifikasi                       ]
Status cakupan: Menunggu verifikasi penjamin
Penjamin ibu: Ditampilkan sebagai informasi pembanding.

PENANGGUNG BIAYA
Nama * [ Penanggung biaya keluarga ] Hubungan [ Ayah             ▾ ]
Kontak [ Nomor terverifikasi                                      ]

RENCANA PENAGIHAN
( ) Invoice ibu dan bayi ditagihkan terpisah
(•) Dua invoice ditagihkan / dibayar bersama
Ibu terkait: Ny. Rani | RM-DEMO-I01 | RI-DEMO-I01
Invoice ibu [ Pilih invoice yang relevan / belum tersedia        ▾ ]
Invoice bayi: Belum tersedia pada tahap ini
Pengelompokan aktual diperiksa dan diproses kasir saat menagih.

[ Kembali ]                         [ Simpan Penjamin & Lanjut ]
~~~

| Wilayah | Isi/aturan isian | Sumber | Komponen |
| --- | --- | --- | --- |
| Penjamin bayi | Pilihan wajib dari master yang tersedia; data kepesertaan bayi diverifikasi sendiri | Master penjamin/kepesertaan | InpatientAdmissionPaymentStep dan payer modal |
| Pembanding ibu | Informasi ibu tidak membuktikan bayi ikut ditanggung | Penjamin episode ibu yang berhak dibaca | Ringkasan readonly |
| Penanggung biaya | Orang/pihak yang bertanggung jawab atas biaya; dapat berbeda dari wali penanda tangan | Data keluarga/penjamin | Field pihak dan kontak existing |
| Rencana penagihan | Pilihan terpisah/bersama; mempertahankan dua invoice sumber | Pilihan terkonfirmasi pengguna dan data Billing | Radio + ringkasan invoice |
| Cakupan | Status verifikasi dan batas cakupan yang benar-benar diketahui | Sumber penjamin yang berwenang | InformationAlert/status existing |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Simpan Penjamin & Lanjut | Utama | Dapat ditekan kecuali proses simpan | Validasi penjamin/penanggung dan simpan preferensi penagihan |
| Lihat Rincian Penjamin | Kedua | Ada data dan hak baca | Buka data cakupan yang tersedia tanpa menjanjikan penjaminan |
| Kembali | Kedua | Tidak sedang mutasi | Kembali ke rencana perawatan |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | “Memuat penjamin/invoice terkait…” |
| Kosong | “Invoice belum tersedia”; preferensi tersimpan tanpa invoice palsu |
| Gagal | Gagal membaca cakupan tidak ditampilkan sebagai “ditanggung” atau “pribadi” otomatis |
| Tidak berhak | Nominal dan transaksi Billing mengikuti permission rupiah yang berlaku |
| Penjamin berbeda | Ibu dan bayi tetap mempunyai cakupan dan kewajiban sendiri |
| Invoice ibu sudah ditutup/lunas | Tampilkan keadaan aktual; kelompok pembayaran diperiksa ulang oleh Billing |
| Pengelompokan belum didukung | Jelaskan bahwa preferensi memerlukan tindak lanjut kasir; jangan tampilkan “sudah digabung” |

**Validasi dan hasil:** dua invoice dapat ditagihkan bersama mengikuti klarifikasi pengguna. Tahap ini tidak menerima uang, tidak memindahkan biaya bayi ke ibu, dan tidak menyatakan invoice sudah digabung. Penjamin yang belum didukung integrasinya tidak ditawarkan sebagai kemampuan aktif. Untuk JKN/paket, tim penjamin menentukan pemetaan klaim sesuai bukti pelayanan; pilihan “bersama” bukan perintah membuat atau menggabungkan klaim. Kasus bayi rujukan dapat memakai penagihan bersama jika ada invoice ibu yang sah dan layak, tanpa membuat episode ibu fiktif.

### 3.8 BAYI-S05 — Rencana Deposit, kondisional

**Tujuan:** mencatat kebutuhan deposit dan membaca pembayaran sebenarnya dari kasir. **Sebelum:** langkah Deposit berpotensi dipahami sebagai pembayaran langsung di admisi. **Sesudah:**

~~~text
LANGKAH 5 DARI 9 — RENCANA DEPOSIT
Bayi Rani | RM-DEMO-B01

KEBIJAKAN YANG BERLAKU
Deposit bayi: Berlaku / sesuai kebijakan penjamin dan perawatan
Minimum untuk konteks ini: [ Nilai dari kebijakan RS             ]
Sumber kebijakan: [ Nama kebijakan yang berlaku                  ]

RENCANA DEPOSIT
Nominal * [ Rp ............ ]
Pihak pembayar [ Penanggung biaya keluarga                      ▾ ]

PEMBAYARAN DI KASIR
Status: Belum ada pembayaran deposit yang terkonfirmasi
Pembayaran terkonfirmasi: Rp0
Referensi pembayaran: —
Deposit ibu: informasi tersendiri, tidak otomatis digunakan bayi.

[ Kembali ]    [ Perbarui Status Kasir ]    [ Simpan Rencana & Lanjut ]
~~~

| Wilayah | Isi/aturan isian | Sumber | Komponen |
| --- | --- | --- | --- |
| Kebijakan | Berlaku/tidak berlaku, minimum dan konteks yang sah | Kebijakan Billing/RS terkonfigurasi | Ringkasan deposit existing |
| Nominal rencana | Wajib bila kebijakan berlaku; tidak menjadi uang masuk | Isian admisi tervalidasi | InpatientAdmissionDepositStep |
| Pembayar | Pihak pembayar yang diketahui | Penanggung biaya | Select existing |
| Pembayaran aktual | Jumlah/status/referensi dari transaksi berhasil | Billing/kasir | Ringkasan readonly |
| Deposit ibu | Ditampilkan terpisah bila berhak; transfer saldo memakai operasi khusus berwenang | Billing | Ringkasan pembanding |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Simpan Rencana & Lanjut | Utama | Dapat ditekan kecuali menyimpan | Simpan rencana, validasi aturan; bukan mencatat uang diterima |
| Perbarui Status Kasir | Kedua | Hak baca cukup | Baca ulang transaksi deposit dan tampilkan hasil aktual |
| Kembali | Kedua | Tidak sedang mutasi | Kembali ke penjamin; ganti penjamin memicu pemeriksaan kebijakan |
| Lanjut Tanpa Deposit | Kedua, kondisional | Hanya bila kebijakan/pengecualian yang sah mengizinkan | Simpan alasan/kewenangan yang diperlukan lalu lanjut |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | “Memuat kebijakan/status deposit…” |
| Kosong | Kebijakan tidak ditemukan tidak dianggap minimum nol; jelaskan tindak lanjut |
| Gagal | Nominal rencana tetap utuh; status pembayaran “Belum dapat dibaca”, bukan Rp0 |
| Tidak berhak | Petugas admisi tidak dapat mengesahkan penerimaan uang kasir |
| Tidak berlaku | Tahap disembunyikan dari nomor/progres; ringkasan akhir menyebut “Deposit tidak berlaku” |
| Belum dibayar | Tetap tulis “Belum dibayar”; apakah dapat melanjutkan mengikuti kebijakan yang disetujui |
| Dibayar sebagian | Tampilkan jumlah terkonfirmasi dan kekurangan dari Billing, bukan perhitungan bebas layar |

**Validasi dan hasil:** meeting menyebut pencatatan deposit di admisi dan pembayaran di kasir, serta angka minimum Rp10.000.000 dalam konteks umum. Skema ini tidak menetapkan angka tersebut sebagai deposit bayi, tidak menggandakannya untuk ibu–bayi, dan tidak menetapkan pembayaran deposit sebagai gerbang perawatan baru. K-06 harus menentukan kebijakan spesifik serta pengecualiannya. Saat data belum dapat dibaca, contoh Rp0 pada skema diganti keadaan gagal, bukan dipertahankan sebagai saldo aktual.

### 3.9 BAYI-S06 — Penempatan dan Pemesanan

**Tujuan:** memilih sumber daya bayi yang cocok dan melakukan reservasi. **Sebelum:** Pilih Bed dan Booking Bed merupakan dua langkah tampilan. **Sesudah:** satu langkah dengan dua keadaan yang jelas: memilih tempat, lalu reservasi tersimpan.

**Varian A — rawat gabung:**

~~~text
LANGKAH 6 DARI 9 — PENEMPATAN DAN PEMESANAN
Perawatan: Rawat gabung | DPJP bayi: Dokter Bayi A

LOKASI IBU
Ny. Rani | Lantai 2 / Sayap A / Kamar Melati / Bed IBU-A
Lokasi diverifikasi terakhir: 14-10-2026 04:10 WIB
Menampilkan boks bayi yang sesuai di kamar ibu.

PILIH BOKS
( ) BOKS-A | Lantai 2 / Sayap A / Melati | Siap
(×) BOKS-B | Lantai 2 / Sayap A / Melati | Sedang dibersihkan
[ Lihat alasan tidak tersedia ]

Boks terpilih: BOKS-A
[ Kembali ]                                     [ Pesan Boks ]
~~~

**Varian B — neonatal terpisah:**

~~~text
LANGKAH 6 DARI 9 — PENEMPATAN DAN PEMESANAN
Perawatan: Neonatal terpisah | Kebutuhan: dari keputusan klinis

Unit yang memenuhi kebutuhan [ Unit neonatal terverifikasi      ▾ ]
Lokasi [ Lantai / Sayap / Kamar                                  ▾ ]
Kelas/lokasi aktual [ Mengikuti tempat yang dipilih                ]
Hanya tempat bayi dengan kemampuan dan kebutuhan isolasi sesuai.

( ) NEO-A | Lantai 3 / Sayap B / Neonatal | Siap
(×) NEO-B | Lantai 3 / Sayap B / Neonatal | Dipesan pasien lain

[ Kembali ]                                    [ Pesan Tempat ]
~~~

| Wilayah | Isi/aturan isian | Sumber | Komponen |
| --- | --- | --- | --- |
| Konteks perawatan | Rencana dan kebutuhan yang sudah diverifikasi | Tahap 3/permintaan klinis | Ringkasan readonly |
| Lokasi ibu | Penempatan aktif, waktu pembacaan, kamar yang relevan | Episode/penempatan ibu | Ringkasan lokasi existing |
| Daftar tempat | Bayi saja, sesuai kemampuan, isolasi, kesiapan dan kapasitas | Engine kelayakan server | InpatientAdmissionBedStep dan useInpatientBedBoard |
| Lokasi terstruktur | Lantai → sayap → kamar → tempat; informasi yang belum ada ditulis belum tersedia | Master lokasi | Filter dan kartu tempat existing |
| Alasan tidak tersedia | Pembersihan, konflik reservasi, kemampuan tidak sesuai | Respons server | Status dan InformationAlert |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Pesan Boks/Pesan Tempat | Utama | Dapat ditekan untuk validasi pilihan; dicegah saat mutasi | Periksa ulang dan buat reservasi sah; tidak menyatakan penempatan aktif |
| Lihat Alasan Tidak Tersedia | Kedua | Ada alasan dari server dan hak baca | Jelaskan alasan tanpa membocorkan identitas penghuni lain |
| Kembali | Kedua | Tidak sedang mutasi | Pertahankan pilihan; validasi ulang setelah kebutuhan berubah |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | Jelaskan daftar tempat yang sedang dimuat; tidak menampilkan kapasitas fiktif |
| Kosong | “Tidak ada tempat bayi yang sesuai dan siap”; arahkan koordinasi penempatan |
| Gagal | Coba Lagi; jangan menampilkan gagal sebagai seluruh tempat terisi |
| Tidak berhak | Tampilkan batas unit/tindakan yang dapat diakses |
| Ibu pindah kamar | Daftar boks rawat gabung diperiksa ulang; jangan mengganti boks otomatis |
| Isolasi neonatal | Tempat harus memenuhi neonatal dan isolasi; bed isolasi dewasa tidak cukup |
| Konflik dua petugas | Satu reservasi sah; pilihan petugas lain ditolak dengan pesan yang jelas |

**Keadaan setelah reservasi berhasil:**

~~~text
LANGKAH 6 DARI 9 — TEMPAT BERHASIL DIPESAN
BOKS-A | Lantai 2 / Sayap A / Kamar Melati
Reservasi: RES-DEMO-B01 | Berlaku sampai 04:25 WIB
Sisa waktu: mengikuti status server
Status bayi: Penempatan belum diaktifkan

[ Batalkan Pemesanan ]  [ Perbarui Status ]  [ Lanjut ke Persetujuan Wali ]
~~~

| Wilayah | Isi | Sumber | Komponen |
| --- | --- | --- | --- |
| Ringkasan reservasi | Tempat, referensi, masa berlaku dan status | Reservasi server | Ringkasan/countdown existing |
| Status perawatan | Bed dipesan berbeda dari bayi sudah ditempatkan | Hasil operasi | InformationAlert |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Lanjut ke Persetujuan Wali | Utama | Tidak sedang mutasi; status dibaca ulang sesuai kebutuhan | Maju dengan reservasi yang dapat dipulihkan |
| Batalkan Pemesanan | Kedua | Reservasi aktif, berhak | Lepaskan melalui server; kembali memilih |
| Perbarui Status | Kedua | Berhak membaca | Muat ulang status aktual |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | Pertahankan ringkasan terakhir dengan penanda sedang diperbarui |
| Kosong/kedaluwarsa | Jelaskan reservasi tidak aktif dan arahkan memilih kembali |
| Gagal | Status “Belum dapat diverifikasi”; jangan menyatakan reservasi masih sah |
| Tidak berhak | Jelaskan keterbatasan pembatalan/pembacaan |
| Reload | Pulihkan reservasi server, bukan memesan ulang otomatis |

**Validasi dan hasil:** penanda tempat bayi berlaku di server pada pencarian, booking, admisi dengan SelectedBedId, penempatan dan transfer. Nama “Ruang Bayi” atau “NICU” saja tidak membuktikan kemampuan tempat. Masa berlaku pada contoh adalah data simulasi, bukan kebijakan reservasi baru. Bayi di neonatal terpisah tidak ikut dipindah ketika ibu berpindah kamar.

### 3.10 BAYI-S07 — Persetujuan Wali

**Tujuan:** merekam pihak yang berwenang memberi persetujuan untuk bayi. **Sebelum:** form persetujuan muncul sesudah Konfirmasi. **Sesudah:** persetujuan ditempatkan sebelum tindakan akhir aktivasi sesuai SOP yang dikunci K-05.

~~~text
LANGKAH 7 DARI 9 — PERSETUJUAN WALI
Bayi Rani | RM-DEMO-B01 | Ibu: Ny. Rani

PENANDA TANGAN *
(•) Orang tua / wali yang sah
Nama * [ Wali Contoh ]  Hubungan * [ Ayah                        ▾ ]
Identitas [ Jenis dan nomor sesuai kebutuhan verifikasi           ]
Kontak [ Nomor terverifikasi                                      ]
Verifikasi kewenangan [ Informasi sesuai prosedur RS               ]

PERSETUJUAN RAWAT INAP BAYI
Dokumen: Persetujuan RS | Versi yang berlaku | Tanggal dokumen
[ Teks persetujuan sesuai versi resmi RS                          ]
[✓] Pernyataan konfirmasi sesuai formulir RS telah dibaca/dijelaskan

Tanda tangan wali *
[ Area tanda tangan                                             ]
[ Hapus Tanda Tangan ]

[ Kembali ]                   [ Simpan Persetujuan & Lanjut ]
~~~

| Wilayah | Isi/aturan isian | Sumber | Komponen |
| --- | --- | --- | --- |
| Subjek | Identitas bayi yang akan disetujui perawatannya | Pasien/episode draft yang sah | Header pasien existing |
| Wali | Nama, hubungan dan dasar verifikasi sesuai SOP; ibu tidak otomatis penanda tangan | Kontak keluarga + verifikasi petugas | Form pihak existing |
| Isi dokumen | Versi resmi formulir RS, tidak ditulis ulang oleh skema ini | Template persetujuan yang berlaku | InpatientAdmissionConsentFormStep |
| Tanda tangan | Cara dan bukti penandatanganan sesuai kontrak persetujuan | Wali/petugas berwenang | Kanvas tanda tangan existing |
| Hasil simpan | Referensi/versi persetujuan, pelaku dan waktu | Penyimpanan server | Ringkasan dokumen |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Simpan Persetujuan & Lanjut | Utama | Dapat ditekan kecuali menyimpan | Validasi wali/dokumen/tanda tangan lalu simpan revisi yang sah |
| Hapus Tanda Tangan | Kedua | Kanvas belum disimpan atau dalam revisi berwenang | Bersihkan kanvas kerja; tidak menghapus bukti versi tersimpan |
| Kembali | Kedua | Tidak sedang mutasi | Kembali ke penempatan; reservasi tetap diperiksa terhadap masa berlaku |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | “Memuat formulir persetujuan…” |
| Kosong | Template/pihak belum tersedia dijelaskan; jangan memakai teks persetujuan karangan |
| Gagal | Pertahankan isian dan kanvas sesuai mekanisme aman; jangan menampilkan “sudah tersimpan” |
| Tidak berhak | Revisi/penandatanganan mengikuti hak yang sah |
| Ibu tidak dapat menandatangani | Verifikasi orang tua/wali lain sesuai SOP, tidak menyalin tanda tangan ibu |
| Reservasi habis saat pengisian | Persetujuan yang sah tidak hilang; penempatan perlu dipilih/diverifikasi ulang |
| Keadaan khusus/darurat | Jalur dan alasan pengecualian hanya jika SOP/kontrak mengizinkan; bukan tombol lewat umum |

**Validasi dan hasil:** bayi tidak mempunyai pilihan “Pasien Sendiri” sebagai penanda tangan. Wali penanda tangan dapat berbeda dari penanggung biaya. Tidak ditambahkan klausul persetujuan medis menyeluruh dalam skema ini. Perubahan identitas, isi atau konteks yang memerlukan revisi harus menghasilkan versi baru sesuai aturan dokumen, bukan menimpa bukti lama.

### 3.11 BAYI-S08 — Tinjau dan Aktifkan

**Tujuan:** memastikan identitas, hubungan, rencana perawatan, pembiayaan dan penempatan selaras sebelum tindakan akhir. **Sebelum:** Konfirmasi berada sebelum form persetujuan. **Sesudah:**

~~~text
LANGKAH 8 DARI 9 — TINJAU DAN AKTIFKAN

IDENTITAS                                  [ Ubah Identitas ]
Bayi Rani | RM-DEMO-B01 | Perempuan | Tunggal
Lahir 14-10-2026 03:20 WIB | Ibu: Ny. Rani / RM-DEMO-I01

PERAWATAN                                  [ Ubah Perawatan ]
Rawat gabung | DPJP Bayi: Dokter Bayi A
Tempat: Lantai 2 / Sayap A / Kamar Melati / BOKS-A
Reservasi: perlu diverifikasi kembali saat aktivasi

PEMBIAYAAN                                 [ Ubah Penjamin ]
Penjamin bayi: sesuai data terverifikasi
Penagihan: dua invoice sumber, preferensi pembayaran bersama
Rencana deposit: Rp ........ | Pembayaran: status dari kasir

PERSETUJUAN                                 [ Lihat Persetujuan ]
Wali: Wali Contoh / Ayah | Versi tersimpan | Waktu penandatanganan

PEMERIKSAAN
✓ Identitas dan hubungan ibu sesuai
✓ DPJP dan kebutuhan perawatan terverifikasi
✓ Persetujuan/pengecualian sesuai prosedur
○ Kelayakan tempat diperiksa kembali ketika diaktifkan

[ Kembali ]                                  [ Aktifkan Admisi Bayi ]
~~~

| Wilayah | Isi/aturan isian | Sumber | Komponen |
| --- | --- | --- | --- |
| Identitas/ibu | Tampilkan hasil tersimpan, bukan hanya state formulir | PatientManagement dan hubungan episode | InpatientAdmissionConfirmationStep diperluas |
| Perawatan/tempat | DPJP bayi, kebutuhan, unit/lokasi, reservasi | Episode/draft/booking yang sah | Ringkasan perawatan dan tempat existing |
| Pembiayaan | Penjamin, preferensi penagihan, rencana/aktual deposit terpisah | Data kunjungan dan Billing | Ringkasan readonly |
| Persetujuan | Wali, versi, waktu dan status sah | Dokumen tersimpan | Komponen ringkasan dokumen |
| Pemeriksaan | Hasil validasi dan bagian yang perlu diperiksa ulang | Server/keadaan alur | InformationAlert dan daftar tautan koreksi |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Aktifkan Admisi Bayi | Utama | Petugas berhak dapat menekan untuk validasi; dicegah saat proses | Pemeriksaan terakhir dan operasi aktivasi sesuai kontrak, lalu tampilkan hasil aktual |
| Ubah Identitas/Perawatan/Penjamin | Kedua | Sesuai hak, belum sedang aktivasi | Kembali ke tahap yang tepat; tandai data turunan yang perlu diperiksa |
| Lihat Persetujuan | Kedua | Dokumen tersimpan dan berhak | Buka versi yang dipakai untuk admisi |
| Kembali | Kedua | Tidak sedang mutasi | Kembali ke persetujuan tanpa membatalkan data tersimpan |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | “Memeriksa kesiapan admisi bayi…” |
| Kosong/tidak lengkap | Daftar bagian yang belum sah dengan tombol menuju tahap terkait |
| Gagal | Pertahankan ringkasan; baca ulang keadaan sebelum retry |
| Tidak berhak | Aktivasi tidak ditawarkan; jelaskan petugas yang berwenang |
| Tempat sudah dipegang pasien lain | Tampilkan konflik dan arahkan ke penempatan; jangan membuat episode/penempatan ganda |
| Ibu sudah pulang | Hubungan historis tetap; rawat gabung diperiksa sesuai keadaan lokasi saat ini |
| Operasi baru berhasil sebagian | Tampilkan episode/reservasi/penempatan aktual; lanjutkan pemulihan, jangan klaim selesai |

**Validasi dan hasil:** tekan dua kali atau retry tidak boleh menghasilkan pasien, episode, atau penempatan kedua. Status Admitted dan tempat aktif hanya ditampilkan setelah dikonfirmasi oleh server. Penjaminan belum terverifikasi, deposit belum dibayar, dan preferensi penagihan bersama tetap mempunyai status masing-masing sesuai kebijakan. Mengaktifkan bayi tidak mengaktifkan ulang atau menutup episode ibu.

### 3.12 BAYI-S09 — Dokumen Bayi

**Tujuan:** menghasilkan identifikasi bayi dan dokumen admisi dari data yang sah. **Sebelum:** Cetak Persetujuan dan Kartu Pasien menjadi dua langkah akhir umum. **Sesudah:**

~~~text
LANGKAH 9 DARI 9 — DOKUMEN BAYI
Admisi berhasil | Episode RI-DEMO-B01 | Tempat aktif: BOKS-A

IDENTITAS
Bayi Rani | RM-DEMO-B01 | Ibu: Ny. Rani / RM-DEMO-I01
Lahir 14-10-2026 03:20 WIB | Perempuan | Tunggal

PRATINJAU GELANG BAYI
┌─────────────────────────────────────────────────────────────┐
│ BAYI RANI             IBU: NY. RANI                          │
│ RM BAYI: RM-DEMO-B01   LAHIR: 14-10-2026 03:20 WIB             │
│ Perempuan | Tunggal   [ Kode identifikasi sesuai format RS ] │
└─────────────────────────────────────────────────────────────┘
[ Cetak Gelang Bayi ]   [ Cetak Label Bayi ]

DOKUMEN ADMISI
[ Lihat / Cetak Persetujuan Wali ]   [ Cetak Kartu jika berlaku ]
Riwayat cetak: petugas, waktu, versi, jumlah dan alasan cetak ulang

WAKTU LAYANAN
Permintaan diterima 03:45 | Admisi aktif 04:15 | Serah terima: belum tercatat

[ Buka Episode Bayi ]  [ Buka Dokumen Admisi ]  [ Selesai ]
~~~

| Wilayah | Isi/aturan isian | Sumber | Komponen |
| --- | --- | --- | --- |
| Hasil admisi | Nomor episode dan tempat aktif yang sudah berhasil | Episode/placement server | Ringkasan keberhasilan existing |
| Gelang | Nama bayi awal/resmi dan nama ibu; pengenal tambahan menurut format RS | Identitas dan template | Reuse identity-labels/wristband PPRI |
| Label/dokumen | Label pasien, persetujuan versi tersimpan, kartu bila berlaku | Dokumen admisi/master pasien | Admission print flow dan komponen cetak existing |
| Riwayat cetak | Pelaku/waktu/versi/jumlah/alasan sesuai kontrak | Log dokumen | Riwayat cetak existing |
| Waktu layanan | Event yang sudah terjadi; yang belum terjadi ditulis belum tercatat | Timestamp proses admisi/serah terima | Ringkasan proses, bukan input jam bebas |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Cetak Gelang/Label | Utama/kedua sesuai kebutuhan | Bahan cetak berhasil dimuat dan hak Print cukup | Cetak data bayi/ibu versi yang benar; catat operasi sesuai kontrak |
| Lihat/Cetak Persetujuan Wali | Kedua | Dokumen tersimpan dan hak baca/cetak | Gunakan versi persetujuan yang sah |
| Buka Episode Bayi | Utama | Episode berhasil dibuka dan hak baca | Buka detail episode bayi |
| Selesai | Kedua | Hasil admisi sudah dikonfirmasi | Tutup proses navigasi; tidak menutup episode rawat inap |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | “Memuat bahan gelang/dokumen…” |
| Kosong | Jelaskan dokumen yang belum tersedia dan tindakan yang sesuai |
| Gagal | Cetak dapat dicoba ulang tanpa mengulang admisi |
| Tidak berhak | Hak cetak terpisah dari hak membuat admisi |
| Kembar | Nama ibu saja tidak menjadi pembeda; data bayi/urutan mengikuti format terverifikasi |
| Ibu sudah pulang | Identitas ibu yang sah tetap dapat muncul; status pulang ibu tidak menghapus hubungan |
| Serah terima belum terjadi | Tampilkan belum tercatat; tombol Selesai tidak membuat waktu serah terima palsu |

**Validasi dan hasil:** nama ibu pada gelang adalah kebutuhan eksplisit meeting MTG-BAYI-04. Bentuk kotak, pengenal tambahan dan label kembar di atas adalah usulan format yang harus dicocokkan dengan SOP, ukuran printer dan kontrak dokumen. Cetak ulang/revisi nama tidak membuat pasien atau episode baru. “Selesai” pada stepper tidak berarti bayi sudah pulang ataupun seluruh pembayaran lunas.

### 3.13 Handoff ke kasir — dua invoice dalam satu penagihan

Tampilan berikut berada di alur Billing/kasir, bukan langkah ke-10 admisi. Admisi menyimpan preferensi pada BAYI-S04. Pengelompokan dan pembayaran aktual baru dilakukan terhadap invoice yang tersedia dan layak.

~~~text
KASIR — PENAGIHAN IBU DAN BAYI
[✓] INV-DEMO-IBU-01   Ny. Rani       Sisa Rp4.000.000
[✓] INV-DEMO-BAYI-01  Bayi Rani      Sisa Rp1.000.000
                         Total kelompok Rp5.000.000

Pembayaran diterima [ Rp3.000.000 ]
Alokasi ke ibu      [ Rp2.500.000 ]
Alokasi ke bayi     [   Rp500.000 ]
Sisa setelah pembayaran: ibu Rp1.500.000 | bayi Rp500.000

[ Tinjau Pembayaran Bersama ]   [ Bayar Invoice Terpilih ]
Bukti pembayaran: dua nomor invoice dan rincian alokasi.
~~~

| Wilayah | Isi | Sumber | Komponen |
| --- | --- | --- | --- |
| Anggota kelompok | Dua invoice sumber, subjek dan saldo sendiri | Billing | Reuse daftar invoice kasir setelah audit pemilik |
| Alokasi | Dana diterima dan pembagian sah per invoice | Transaksi/alokasi Billing | Komponen pembayaran sesuai kontrak |
| Bukti | Referensi transaksi, invoice asal dan alokasi | Pembayaran berhasil | Cetakan kasir |

| Tombol | Jenis | Kapan aktif | Hasil |
| --- | --- | --- | --- |
| Tinjau Pembayaran Bersama | Utama | Hak Billing cukup, invoice layak | Verifikasi alokasi/dampak saldo sebelum konfirmasi pembayaran |
| Bayar Invoice Terpilih | Kedua | Satu invoice dipilih dan layak | Pembayaran terpisah tetap tersedia |

| Keadaan | Perilaku khusus |
| --- | --- |
| Memuat | Baca saldo yang sah sebelum penagihan |
| Kosong | Invoice bayi belum tersedia tidak diganti invoice fiktif |
| Gagal | Saldo tidak diketahui bukan nol; uang yang statusnya belum pasti direkonsiliasi |
| Tidak berhak | Hanya petugas dengan hak Billing memproses pembayaran |
| Sudah lunas/ditutup | Tidak ditagih kembali; kelompok diperiksa ulang |
| Penjamin berbeda | Kewajiban/cakupan setiap pasien tetap dipertahankan |
| Timeout/refund | Rekonsiliasi dan referensi alokasi per invoice; jangan menggandakan uang masuk |
| Ibu pulang lebih dahulu | Episode/biaya bayi tetap berjalan menurut status bayi sendiri |

Ini sketsa handoff yang masih memerlukan audit dan kontrak pemilik Billing. Kebutuhan dua invoice bersumber dari klarifikasi pengguna; meeting hanya menyebut hubungan/pemisahan catatan secara umum. Contoh angka adalah simulasi pasien umum, bukan tarif RS atau besaran klaim.

### 3.14 Aturan lintas stepper, penyimpanan dan pencatatan waktu

**Penyimpanan.** Tahap identitas dapat menyimpan master pasien; tahap selanjutnya menyimpan hasil atau draft sesuai kontrak. Urutan tampilan tidak berarti seluruh data baru boleh disimpan pada langkah 8. Jika reservasi/persetujuan membutuhkan EpisodeId, draft internal hanya disiapkan setelah identitas, DPJP, penjamin dan prasyarat kontrak tersedia. Waktu persis pembukaan draft, hubungan kunjungan/deposit, dan operasi aktivasi harus dikunci pada amendemen kontrak; skema ini tidak mengarang endpoint baru. Membuat draft tidak boleh ditampilkan sebagai bayi sudah ditempatkan.

**Perubahan data yang sudah dipakai:**

| Perubahan | Tahap yang diperiksa ulang | Perilaku |
| --- | --- | --- |
| Jalur admisi/RM bayi | Identitas sampai dokumen yang terpengaruh | Konfirmasi dampak; jangan mengganti subjek episode tersimpan secara diam-diam |
| Ibu/hubungan keluarga | Nama awal, alamat/kontak, rawat gabung, penagihan keluarga dan dokumen | Verifikasi hubungan baru, audit koreksi, pertahankan riwayat |
| Data lahir/urutan kembar | Identitas dan dokumen, kebutuhan yang bergantung umur | Revisi berwenang; jangan menukar bayi kembar |
| Rencana klinis/DPJP | Penempatan dan dokumen/pembiayaan yang relevan | Periksa kemampuan tempat dan reservasi; identitas bayi tidak dibuat ulang |
| Penjamin | Deposit/cakupan/penagihan | Periksa ulang kebijakan; jangan memindahkan pembayaran/deposit yang sudah terjadi |
| Lokasi ibu | Penempatan rawat gabung | Validasi ulang; bayi neonatal terpisah tidak ikut pindah otomatis |
| Tempat bayi | Reservasi, ringkasan dan konteks persetujuan yang relevan | Operasi pembatalan/perubahan sah; histori tempat tetap ada |
| Nama resmi setelah aktif | Profil dan dokumen identitas | RM/episode tetap; revisi/cetak ulang terlacak |

**Waktu layanan.** Meeting meminta pencatatan waktu respons. Usulan event berikut adalah target desain, bukan daftar event yang telah terbukti tersedia:

| Waktu/event | Sumber yang direkomendasikan | Arti |
| --- | --- | --- |
| Waktu lahir | Catatan kelahiran terverifikasi | Fakta klinis; berbeda dari waktu petugas menginput |
| Permintaan rawat diterima | Event permintaan dari unit/perujuk | Kandidat awal pengukuran proses admisi |
| Identitas tersimpan | Hasil simpan identitas yang berhasil | Menandai pekerjaan registrasi, bukan waktu lahir |
| Kebutuhan/DPJP diverifikasi | Penugasan atau permintaan klinis yang sah | Dasar perawatan/penempatan |
| Tempat dipesan | Reservasi berhasil | Belum berarti bayi sudah datang ke tempat |
| Admisi/penempatan aktif | Operasi server yang berhasil | Fakta aktivasi sesuai kontrak |
| Serah terima diterima | Penerimaan oleh petugas unit tujuan | Akhir serah terima, bukan ketika tombol Selesai ditekan |
| Pembayaran deposit | Transaksi kasir berhasil | Fakta uang diterima, terpisah dari rencana deposit |
| Dokumen dicetak | Operasi cetak/log sesuai kontrak | Jejak versi dan petugas |

Contoh perhitungan usulan: permintaan diterima 03:45, admisi aktif 04:15 berarti selang 30 menit. Jika serah terima diterima 04:25, selang aktivasi ke serah terima 10 menit. Titik awal/akhir resmi, target waktu, penanganan data terlambat dan koreksi perlu ditetapkan pemilik proses. Semua tampilan waktu menyertakan tanggal/zona yang sesuai; timestamp retry tidak menggantikan waktu keberhasilan pertama.

### 3.15 Pemetaan dari alur umum dan keputusan yang masih terbuka

| Langkah umum sekarang | Tampilan bayi yang diusulkan | Perubahan utama |
| --- | --- | --- |
| 1 Jenis Kunjungan + 2 Tipe Pasien | BAYI-S01 Konteks Admisi Bayi | Kategori sudah diketahui; pilih jalur bayi dan cek RM |
| 3 Pendaftaran | BAYI-S02 Identitas Bayi dan Ibu | Nama awal dari ibu; identitas bayi dan ibu dibedakan |
| 6 Dokter | BAYI-S03 Perawatan dan DPJP Bayi | Kebutuhan klinis dan dokter bayi sendiri |
| 4 Pembayaran | BAYI-S04 Penjamin dan Penagihan | Preferensi dua invoice; transaksi uang tetap di kasir |
| 5 Deposit | BAYI-S05 Rencana Deposit | Rencana, kebijakan dan pembayaran aktual dibedakan |
| 7 Pilih Bed + 8 Booking Bed | BAYI-S06 Penempatan dan Pemesanan | Satu tahap tampilan dengan keadaan reservasi yang jelas |
| 10 Form Persetujuan | BAYI-S07 Persetujuan Wali | Wali sah dan versi dokumen sesuai SOP |
| 9 Konfirmasi | BAYI-S08 Tinjau dan Aktifkan | Verifikasi akhir dan hasil aktivasi aktual |
| 11 Cetak Persetujuan + 12 Kartu Pasien | BAYI-S09 Dokumen Bayi | Gelang memuat identitas ibu; cetak dapat diulang |

**Batas keputusan:** nama awal dan gelang dengan nama ibu mengikuti rekap meeting; kebutuhan dua invoice mengikuti klarifikasi pengguna. Sembilan tahap, rawat gabung/neonatal terpisah, field pembeda kembar, event durasi, dan penempatan persetujuan adalah usulan skema yang perlu diselaraskan dengan keputusan K-01–K-07. Tidak dianggap ada persetujuan baru terhadap minimum deposit bayi Rp10 juta, jumlah DPJP bayi tertentu, cakupan penjamin otomatis, atau mekanisme pembayaran bersama yang belum diimplementasikan.

## 4. Rincian perbaikan

### FIX-EPS-016-01 — Amendemen desain dan kontrak konteks bayi

**Menutup:** ISS-01, ISS-03. **Area:** dokumen. **Jenis:** DESIGN_CHANGE. **Target:** 00-interview-decisions.md tingkat modul, 03-frontend-architecture.md, 05-skema-tampilan.md, kontrak API/validasi/status/integrasi terkait. **Radius:** admisi baru/lama/transfer, PPRI, bangsal, integrasi Billing. **Dependency:** K-01, K-02, K-04, K-05.

**Langkah:** tetapkan cabang yang didukung; definisikan hubungan pasien ibu versus episode ibu; definisikan data dan status tersimpan; amendemen alur beserta role, consent, identitas sementara dan pemetaan unit. Penulisan dokumen hulu dilakukan pada pekerjaan blueprint berikutnya, bukan diagnosis ini.

**Acceptance criteria:**

1. Kasus bayi lahir luar RS dapat dijelaskan tanpa episode ibu fiktif; rawat gabung tetap mensyaratkan ibu/lokasi yang sesuai.
2. Dua bayi kembar mempunyai dua RM dan episode berbeda serta urutan lahir yang tidak tertukar.
3. Tiap tahap mempunyai input, keluaran tersimpan, validasi, hak akses, dan perilaku kembali/pemulihan.
4. Identitas sementara, batas usia, dan jalur kedaruratan ditetapkan atau secara eksplisit belum tersedia.
5. Titik pembukaan draft, prasyarat reservasi/persetujuan, serta event waktu layanan dipetakan ke kontrak yang nyata; membuka draft tidak dinyatakan sebagai penempatan aktif.

**Verifikasi:** tinjau contoh empat jalur bersama pemilik proses; cocokkan setiap field dan status dengan pemilik domain. **Risiko:** aturan baru bertentangan dengan desain lama; mitigasi dengan amandemen dan traceability yang eksplisit.

### FIX-EPS-016-02 — Mode pendaftaran bayi dan penyimpanan master yang benar

**Menutup:** ISS-02. **Area:** FE. **Jenis:** BUG/CONTRACT_MISMATCH. **Berkas calon:** inpatient-admission-registration-step.jsx, use-inpatient-admission-patient.jsx, inpatient-admission-patient-utils.jsx, inpatient-admission-view.jsx, dan konfigurasi form terkait. **Radius:** admisi baru; NewPatientForm dimiliki pendaftaran IGD dan juga dipakai registrasi lain. **Dependency:** kontrak data yang dikunci FIX-01; field newborn existing dapat direuse.

**Langkah:** teruskan konteks bayi/ibu; tampilkan field bayi; isi saran data orang tua secara terpisah; kirim IsNewborn dan MotherPatientId yang benar; buat dokumen identitas hanya bila identitas pasiennya memang tersedia dan valid; pertahankan pemulihan simpan yang sudah ada.

**Acceptance criteria:**

1. Memilih bayi dan ibu A mengirim isNewborn=true dan motherPatientId=A; hasil profil terbaca konsisten.
2. Bayi tanpa NIK tidak memerlukan NIK fiktif atau NIK ibu sebagai NIK pribadi; jalur ini mengikuti kontrak identitas yang disetujui.
3. NIK/scan ibu hanya mengisi wilayah ibu/wali; tanggal lahir bayi tidak berubah menjadi tanggal lahir ibu.
4. Klik simpan dua kali/retry sesudah kontak gagal tidak membuat RM bayi kedua.
5. Dua bayi kembar bisa didaftarkan dengan ibu sama dan urutan berbeda; pencarian tidak menganggapnya satu pasien hanya karena ibu sama.
6. Saran nama awal mengikuti “Bayi [Nama Ibu]” dari rekap meeting; nama resmi dan revisi identitas mempertahankan RM bayi yang sama. Format pembeda kembar dikunci sebelum implementasi.

**Verifikasi:** targeted unit test bermakna untuk payload/data kembar dan retry, lint, build, serta UAT form bayi. Uji regresi konsumen NewPatientForm yang terdampak. **Risiko:** perubahan komponen bersama mengubah pendaftaran IGD; mode default dipertahankan dan radius konsumennya didata sebelum implementasi.

### FIX-EPS-016-03 — Validasi hubungan ibu–bayi di server

**Menutup:** T1. **Area:** BE. **Berkas calon:** InpEpisodeService.Corrections.cs, InpEpisodeService.cs, query/DTO yang relevan. **Radius:** buka/perbarui admisi, detail episode, rawat ulang; kontrak master pasien dibaca dari PatientManagement. **Dependency:** FIX-01; pengecualian ibu tidak dikenal menunggu K-03.

**Langkah:** cocokkan episode ibu dengan hubungan pasien ibu yang tersimpan; bedakan tautan baru, koreksi berwenang, dan referensi historis yang tidak berubah; jangan memakai status ibu aktif sebagai syarat universal mempertahankan hubungan.

**Acceptance criteria:**

1. Bayi B beribu A ditolak bila diarahkan ke episode C tanpa koreksi hubungan yang sah.
2. Ibu yang sudah Closed tidak otomatis menutup bayi atau melepaskan boks bayi.
3. Referensi historis yang tidak berubah tidak menghalangi koreksi admisi bayi hanya karena ibu sudah Closed; tautan baru untuk rawat gabung tetap memeriksa kelayakan saat ini.
4. Perubahan hubungan disertai pelaku, waktu dan alasan sesuai kontrak audit; catatan klinis bayi tetap milik bayi.

**Verifikasi:** build dan pemeriksaan sesuai kebijakan backend, skenario API terkendali pada lingkungan yang diotorisasi. Diagnosis ini tidak menjalankan API/database. **Risiko:** aturan baru memblokir data legacy; siapkan hasil validasi yang jelas dan jalur koreksi berwenang, tanpa backfill diam-diam.

### FIX-EPS-016-04 — Aturan kecocokan neonatal pada engine bed

**Menutup:** ISS-04. **Area:** BE. **Berkas calon:** InpBedOccupancyService.cs, InpBedManagementQueryService.cs, DTO pencarian/reservasi, validasi SelectedBedId pada admisi, serta jalur transfer. **Radius:** admisi, transfer, penempatan/correction, bed board; pengguna dewasa ikut membutuhkan regresi. **Dependency:** FIX-01 dan K-04.

**Langkah:** tetapkan sumber kebenaran konteks pasien dan kemampuan tempat; validasi kebutuhan neonatal dua arah; terapkan aturan sama pada pencarian, reservasi, aktivasi/penempatan, dan transfer; tangani fase sebelum episode terbentuk tanpa bergantung pada boolean dari browser.

**Acceptance criteria:**

1. Bayi rawat gabung hanya mendapat boks yang siap dan cocok di kamar ibu; bed dewasa di kamar sama tidak dapat dipilih sebagai tempat bayi.
2. Bayi neonatal terpisah mendapat tempat sesuai kebutuhan klinis; bed dewasa dan unit dengan nama NICU tetapi kemampuan belum terverifikasi ditolak.
3. Pasien dewasa tidak dapat mengisi boks bayi melalui API langsung.
4. Bayi membutuhkan isolasi hanya mendapat tempat neonatal isolasi; bed isolasi dewasa saja tidak cukup.
5. Permintaan langsung dengan SelectedBedId tidak sesuai ditolak walaupun layar disiasati; reservasi diperebutkan tetap konsisten.

**Verifikasi:** skenario server yang sama di seluruh jalur, termasuk sebelum ada episode dan transfer NICU→rawat gabung. **Risiko:** boolean IsForNewborn belum mewakili level kemampuan; jangan menambah model/master sebelum dipastikan tidak ada kemampuan existing yang dapat direuse.

### FIX-EPS-016-05 — Pencarian dan pemesanan tempat bayi di frontend

**Menutup:** ISS-04. **Area:** FE. **Berkas calon:** use-inpatient-admission-bed.jsx, use-inpatient-bed-board.jsx, inpatient-bed-utils.jsx, inpatient-admission-bed-step.jsx, service terkait. **Radius:** admisi; helper board dipakai bed board/transfer sehingga semua pemakainya harus dipetakan. **Dependency:** FIX-04.

**Langkah:** kirim konteks yang disepakati; gunakan hasil kelayakan server; tampilkan jenis lokasi/boks dan alasan tidak tersedia; validasi ulang ketika ibu/lokasi/perawatan berubah.

**Acceptance criteria:**

1. Filter bayi sudah berlaku pada tampilan pertama sebelum maupun sesudah episode terbentuk.
2. Pengguna tidak dapat membuka kembali bed dewasa dengan menghapus filter tampilan.
3. Ibu pindah kamar membuat boks rawat gabung diperiksa ulang; bayi di NICU tidak ikut dipindah otomatis.
4. “Tidak ada tempat sesuai” dan “gagal memuat” terlihat berbeda; booking yang tersimpan dapat dipulihkan setelah reload.

**Verifikasi:** unit query mapping, lint/build, UAT pencarian/reservasi/konflik. **Risiko:** filter bersama mengubah board umum; konteks bayi diterapkan secara eksplisit pada alur yang memerlukannya.

### FIX-EPS-016-06 — Stepper bayi dan status tahap

**Menutup:** ISS-01, ISS-03. **Area:** FE. **Berkas calon:** inpatient-admission-flow-constants.jsx, use-inpatient-admission-flow.jsx, inpatient-admission-view.jsx, stepper dan ringkasan terkait. **Radius:** pasien baru/lama, resume, transfer/kamar pulih bila memakai daftar langkah bersama. **Dependency:** FIX-01.

**Langkah:** susun sembilan tahap pada bagian 3 dengan deposit kondisional; gunakan “Penjamin dan Penagihan” serta “Rencana Deposit” untuk membedakan administrasi dari uang diterima kasir; gunakan satu tampilan penempatan dengan status reservasi; cetak menjadi aksi akhir dengan gelang bayi memuat nama ibu; pulihkan state berdasarkan data tersimpan dan tampilkan waktu layanan yang tersedia dari sumber sah.

**Acceptance criteria:**

1. Jalur bayi menampilkan tahap yang berlaku dan tujuan tahap berikutnya; deposit hilang dari nomor/progres bila tidak berlaku.
2. Label dokter menyebut DPJP bayi; tidak otomatis menyalin DPJP ibu.
3. Navigasi maju/mundur, URL lama, dan resume tidak membuka tahap yang datanya belum sah.
4. Reservasi berhasil tidak ditampilkan sebagai penempatan aktif; konfirmasi sukses tidak berarti pembayaran lunas.
5. Ganti ibu/perawatan/penjamin menandai keluaran turunan yang perlu diverifikasi tanpa menghapus RM bayi.
6. Dokumen persetujuan/cetak mengikuti data tersimpan dan SOP urutan yang disetujui.
7. Gelang bayi memakai identitas bayi dan nama ibu sesuai kebutuhan meeting; cetak ulang tidak mengulang pembuatan pasien/episode.
8. Rencana deposit tidak ditampilkan “sudah dibayar” tanpa transaksi kasir yang sah; kegagalan membaca pembayaran bukan nominal nol.
9. Waktu lahir, input, aktivasi dan serah terima ditampilkan terpisah bila tersedia; tombol Selesai tidak mengarang waktu serah terima. Pencatatan event yang belum tersedia memerlukan kontrak dan task pemiliknya, tidak dipenuhi dengan jam lokal browser.

**Verifikasi:** skenario navigasi/resume serta UAT desktop dan layar sempit; lint/build. **Risiko:** slug URL dan simpanan lama merujuk langkah lama; siapkan pemetaan yang eksplisit.

### FIX-EPS-016-07 — Pencarian ibu yang dapat dipulihkan

**Menutup:** T2. **Area:** FE. **Berkas calon:** MotherEpisodeSelector pada inpatient-admission-view.jsx dan service pencarian ibu. **Radius:** pemilih ibu pada admisi. **Dependency:** tidak ada untuk perbaikan error yang sudah terbukti.

**Langkah:** tambahkan keadaan error/coba lagi; bedakan kosong dan gagal; pulihkan pilihan lewat detail yang sah; selaraskan pencarian/pagination; abaikan respons lama bila pencarian berubah.

**Acceptance criteria:**

1. Respons 500/timeout menampilkan gagal dan Coba Lagi; pilihan sebelumnya tidak hilang.
2. Ibu di luar 50 hasil pertama tetap dapat dicari/dipilih tanpa UUID manual.
3. Respons pencarian lama yang selesai belakangan tidak menimpa hasil pencarian terbaru.
4. Episode yang statusnya berubah ditampilkan dengan alasan kelayakan yang benar; tidak diam-diam diganti ke ibu lain.

**Verifikasi:** simulasi respons kosong/error/terbalik serta pagination; lint/build. **Risiko:** request berulang akibat perubahan callback; evaluasi dependensi hook bersama state pencarian.

### FIX-EPS-016-08 — Bayi yang sudah terdaftar

**Menutup:** T3. **Area:** FE. **Berkas calon:** inpatient-admission-visit-category-step.jsx, hook pasien dan view/alur terkait. **Radius:** pasien lama, rujukan, rawat ulang, resume. **Dependency:** FIX-01 dan K-01.

**Langkah:** baca identitas bayi dan hubungan ibu dari profil; pisahkan pemilihan konteks admisi dari mutasi kategori master; gunakan RM existing; arahkan hubungan yang tidak lengkap ke koreksi yang berwenang.

**Acceptance criteria:**

1. Bayi usia 10 hari yang pernah pulang memakai RM lama dan episode baru; episode ibu lama tidak harus dibuka kembali.
2. Pilihan kategori pada layar tidak mengubah pasien dewasa menjadi bayi atau melepas hubungan ibu tersimpan.
3. Bayi yang sedang mempunyai episode aktif mengikuti penjagaan duplikasi episode existing.
4. Source dan desain sepakat apakah opsi bayi dapat dipilih dan bagaimana validasinya.

**Verifikasi:** UAT bayi rujukan existing, rawat ulang, duplikasi episode aktif dan pasien dewasa. **Risiko:** salah menafsirkan IsNewborn sebagai kondisi seumur hidup; klasifikasi perawatan saat ini menggunakan umur/konteks menurut SOP, sementara riwayat kelahiran tetap terpelihara.

### FIX-EPS-016-09 — Handoff pembayaran bersama dua invoice

**Menutup:** ISS-05. **Area:** desain Billing dan integrasi. **Target:** dokumen BillingManagement dan kontrak integrasi setelah pemiliknya memberi kewenangan; source tidak ditargetkan oleh diagnosis ini. **Radius:** invoice, pembayaran, deposit, refund, finalisasi, clearance, laporan kasir, penjamin. **Dependency:** K-06, K-07 dan audit Billing lebih lanjut.

**Langkah:** audit dukungan pengelompokan/alokasi existing; tetapkan kelompok dan anggota invoice, saldo, hak akses, idempotency, pembayaran sebagian dan kegagalan provider; tetapkan bukti pembayaran dan perilaku perubahan kelompok; pecah implementasi menjadi task terpisah pada roadmap Billing yang berwenang.

**Acceptance criteria kontrak handoff:**

1. Dua invoice berbeda dan dua pasien tetap dapat ditelusuri dalam satu kelompok pembayaran; pembayaran terpisah tetap dapat dilakukan.
2. Total sisa ibu Rp4 juta dan bayi Rp1 juta menghasilkan kelompok Rp5 juta; membayar Rp3 juta harus mencatat jumlah alokasi yang tepat Rp3 juta dan sisa total Rp2 juta.
3. Satu uang masuk tidak tercatat dua kali; request berulang idempoten; jumlah alokasi tidak melebihi dana sah atau saldo masing-masing.
4. Pembayaran sebagian atau kegagalan provider menunjukkan hasil aktual per invoice dan proses rekonsiliasi; jangan mengandalkan dua panggilan settlement tanpa koordinasi.
5. Refund Rp200 ribu milik bayi merujuk pembayaran/alokasi bayi yang sah; deposit ibu tidak berpindah otomatis.
6. Ibu pulang, bayi tetap dirawat: status/clearance bayi dinilai sendiri dan tagihan baru bayi tidak masuk ke invoice ibu yang sudah ditutup diam-diam.
7. Aturan klaim JKN/paket dan hak penjamin dipisahkan dari pengelompokan pembayaran; tidak ada tagihan ganda pelayanan neonatal esensial yang sudah tercakup paket.

**Verifikasi:** review kontrak dengan pemilik Billing, kasir, keuangan dan tim penjamin; implementasi kelak memerlukan uji konkurensi, retry, pembayaran parsial/refund, serta rekonsiliasi provider. **Risiko:** duplikasi penerimaan atau pemindahan utang antar pasien; mitigasi dengan alokasi tersimpan, referensi invoice asal, audit, dan rekonsiliasi. Ini belum dinyatakan tersedia.

### 4.1 Peta API existing untuk pekerjaan berikutnya

Semua base URL di bawah berasal dari atribut controller pada source BE yang SHA-nya tercatat di issue. Tabel tidak memperkenalkan endpoint baru.

#### Health Services / Patient Management / Master Data / Patient

Base URL: api/v1/health-services/patient-management/master-data/patients

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| POST | /admin | Simpan pasien bayi melalui jalur petugas | Patient : Create | CreatePatientRequest; field newborn dan ibu existing | ApiResponse<PatientCreateResponse> |

Bukti controller PatientController.cs:43, 53, 766–773 pada snapshot akhir; field lahir PatientDtos.cs:392–403. Kode 400 berarti masukan tidak valid; 409 berarti konflik seperti identitas yang sudah terdaftar. Penolakan validasi hubungan ibu harus ditampilkan dalam bahasa petugas. Action petugas bernama CreatePatientForAdmin dan mendelegasikan operasi create existing.

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: api/v1/health-services/inpatient-management/episodes

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | /active-mothers | Cari calon episode ibu | InpatientEpisode : Read | ActiveMotherListQuery | ApiResponse<List<ActiveMotherItemResponse>> |
| POST | / | Buka admisi bayi | InpatientEpisode : Create | OpenAdmissionRequest | ApiResponse<InpatientEpisodeDetailResponse> |

Bukti InpatientEpisodeController.cs:42, 52, 76–86, 235–245. Kode 400 berarti masukan tidak sah; 404 referensi tidak ditemukan; 409 konflik keadaan; 422 aturan bisnis menolak. Isi akhir validasi baru ditetapkan dalam amendemen kontrak.

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: api/v1/health-services/inpatient-management/bed-occupancies

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | /available-beds | Baca tempat yang layak | InpatientBedOccupancy : Read | AvailableBedQuery | ApiResponse<AvailableBedPagedResult> |
| POST | /reservations | Pesan tempat | InpatientBedOccupancy : Create | ReserveBedRequest | ApiResponse<BedReservationResponse> |
| POST | /placements | Tempatkan bayi | InpatientBedOccupancy : Create | PlacePatientRequest | ApiResponse<BedPlacementResponse> |

Bukti InpatientBedOccupancyController.cs:32, 42, 74–86, 119–126, 209–216. HTTP 409 berarti tempat diperebutkan/sudah dipegang; 422 berarti tempat tidak memenuhi kebutuhan atau kesiapan. Keberadaan endpoint tidak berarti endpoint sudah menjaga kemampuan neonatal.

#### Health Services / Billing Management / Billing / Patient Funds

Base URL: api/v1/health-services/billing-management/billing/patient-funds

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| POST | /settlements | Mulai pembayaran untuk satu target existing | BillingPayment : Create | CreateSettlementRequest, satu InvoiceId atau DepositAccountId; header Idempotency-Key | ApiResponse<SettlementResponse> |

Bukti BillingSettlementsController.cs:14–58 dan BillingSettlementDtos.cs:6–20. Kode 201 berarti settlement baru; 200 berarti hasil request sebelumnya dikembalikan; 404 target tidak ditemukan; 409 konflik target atau request. Kelompok pembayaran dua invoice adalah **Rencana (belum tersedia pada kontrak yang diperiksa)**; route baru tidak ditentukan di diagnosis ini. Idempotency dan konflik pembayaran harus dipetakan ulang oleh pemilik Billing sebelum implementasi.

## 5. Urutan pengerjaan

~~~text
K-01, K-02, K-04, K-05
└── ⛔ FIX-01 — amendemen desain/kontrak konteks bayi
    ├── FIX-02 — form dan master bayi
    ├── FIX-03 — konsistensi hubungan ibu
    │   └── K-03 untuk cabang ibu tidak dikenal
    ├── ⛔ FIX-04 — aturan penempatan bayi di server
    │   └── ⛔ FIX-05 — daftar tempat bayi di frontend
    ├── ⛔ FIX-06 — stepper dan pemulihan alur
    └── ⛔ FIX-08 — jalur pasien bayi existing

FIX-07 — error/pemulihan pencarian ibu, tanpa dependency perubahan aturan

K-06, K-07 + audit pemilik Billing
└── ⛔ FIX-09 — kontrak pembayaran bersama dua invoice
    └── task implementasi Billing terpisah, belum dialokasikan
~~~

FIX-02 dan FIX-03 mempunyai source gap yang sudah terbukti; implementasinya tetap mengikuti kontrak/amendemen yang dikunci. Pohon bukan izin menulis source atau bekerja paralel dengan agent lain.

## 6. Keputusan pemilik yang dibutuhkan

| ID | Keputusan | Rekomendasi | Perbaikan yang ditahan |
| --- | --- | --- | --- |
| K-01 | Scope lahir lokal, rujukan, rawat ulang | Rancang empat jalur pada issue; urutan rilis dapat bertahap | 01, 06, 08 |
| K-02 | Sumber data lahir, detail nama/gelang, batas usia, urutan kembar | Nama awal “Bayi [Nama Ibu]” dan gelang dengan nama ibu didukung meeting; sumber klinis, format pembeda dan batas usia masih perlu dikunci | 01, 02 |
| K-03 | Ibu tidak diketahui/tidak ada master | Identitas sementara dan rekonsiliasi; kontrak newborn saat ini mewajibkan ibu | 03 untuk pengecualian |
| K-04 | Kemampuan tempat/unit dan rawat gabung | Boks tersendiri di kamar ibu; neonatal terpisah mengikuti kemampuan | 01, 04, 05 |
| K-05 | Persetujuan/aktivasi dan pengecualian keadaan darurat | SOP rumah sakit dan audit; dokumen cetak bukan penentu kesiapan perawatan | 01, 06 |
| K-06 | Tarif/paket, deposit bayi, penjamin, alokasi parsial dan refund | Catat rencana di admisi, uang diterima di kasir; minimum umum Rp10 juta pada rekap belum menjadi aturan deposit bayi; biaya/utang tetap per pasien/invoice | 09; aturan BAYI-S04–S05 |
| K-07 | Pemilik dan kontrak pembayaran bersama | Audit dan desain Billing dengan pemilik berwenang | 09 |

**Sudah diberikan pengguna:** dua invoice berbeda dapat disatukan; sebelumnya pengguna memilih dua tagihan tetap terpisah dengan pembayaran bersama. Tidak meminta ulang kebutuhan tersebut. Satu invoice pengganti yang membatalkan kedua invoice asal bukan bagian keputusan saat ini.

## 7. Dokumen hulu yang ikut direvisi

Amandemen scope admisi baru/lama sudah ditautkan pada arsitektur, skema, kontrak API/validasi/status/integrasi, dan kamus data. Desain settlement Billing, prosedur ibu tidak dikenal, dan kebijakan response time tetap pekerjaan pemilik domain terpisah.

| Dokumen | Bagian | Sekarang | Usulan menjadi |
| --- | --- | --- | --- |
| rawat-inap/00-interview-decisions.md | RWI-RULE-014 dan keputusan baru yang berwenang | Bayi rawat gabung mempunyai episode dan boks tersendiri | Pertahankan invariant; tambahkan konteks lahir luar/rawat ulang, sumber klinis, hubungan historis dan aturan neonatal |
| 03-frontend-architecture.md | Langkah pasien baru/lama :317–336 | Satu alur 12 langkah; bayi pasien lama dinonaktifkan menurut teks | Tahap kondisional bayi; profil existing dan episode baru didukung sesuai scope |
| 05-skema-tampilan.md | Pemilih bayi/ibu :246–274 | Semua bayi wajib episode ibu aktif | Episode terkait bersyarat konteks; hubungan pasien ibu terpisah |
| 05-skema-tampilan.md | Pendaftaran :278–320 | Form NIK/KTP pasien umum | Mode bayi dan wilayah identitas ibu/wali terpisah |
| 05-skema-tampilan.md | Dokter, bed, konfirmasi, persetujuan | Label umum dan reservasi langkah terpisah | DPJP bayi, kemampuan neonatal, status reservasi/penempatan, consent sesuai SOP |
| contracts/validation-matrix.md | Hubungan ibu dan kelayakan penempatan | Referensi episode dan aturan bed umum | Konsistensi ibu–bayi, referensi historis, kecocokan neonatal dua arah |
| contracts/api-contract.md | Query tempat dan konteks admisi | EpisodeId opsional; IsForNewborn opsional | Kontrak konteks sebelum/sesudah episode; server menentukan kebutuhan neonatal |
| contracts/state-transition-matrix.md | Admisi/penempatan/transfer | Status existing | Pisahkan rawat gabung, hubungan keluarga, riwayat lokasi dan status tagihan tanpa menambah status sembarang |
| contracts/integration-contract.md | Patient/Clinical/Billing | Titik sambung existing | Fakta kelahiran dan penjamin bayi, kelompok penagihan dengan sumber invoice terpelihara |
| Blueprint Billing dan integrasi-billing | Pembayaran/clearance/paket | Settlement satu target yang diperiksa | Kelompok pembayaran lintas invoice, alokasi, refund, rekonsiliasi, hak akses dan batas klaim |
| 03-frontend-architecture.md dan kontrak integrasi | Status deposit dan waktu perjalanan pasien | Waktu respons diminta meeting; dukungan setiap event belum dipetakan lengkap | Rencana deposit terpisah dari pembayaran kasir; daftar event/sumber/titik durasi dan koreksi ditetapkan sebelum implementasi |
| 05-skema-tampilan.md dan kontrak dokumen | Gelang bayi/label identitas | Kebutuhan gelang dengan nama ibu ada di rekap meeting | Pratinjau dari data bayi/ibu terverifikasi, pembeda kembar, versi dan cetak ulang sesuai SOP |

## 8. Verifikasi menyeluruh

| Skenario | Hasil yang harus dibuktikan |
| --- | --- |
| Lahir di RS, ibu dan bayi memenuhi syarat rawat gabung | RM/kunjungan/episode bayi sendiri; ibu benar; boks sesuai kamar; DPJP bayi; tidak menghuni bed ibu |
| Bayi perlu NICU sejak lahir | Relasi ibu ada; tempat sesuai neonatal; tidak dipaksa lewat rawat gabung |
| Rujukan bayi tanpa episode ibu lokal | Tidak membuat episode ibu fiktif; penjamin dan DPJP bayi diverifikasi |
| Ibu tidak dikenal | Hanya jalur sementara yang disetujui; tidak menebak ibu dari daftar pasien perempuan |
| Kembar 1 dan 2, ibu sama | Dua RM, dua episode, dua tempat; label dan urutan tidak tertukar |
| Belum mempunyai NIK/nama resmi | Identitas sementara sah; tidak duplikasi NIK ibu sebagai NIK bayi |
| Respons pencarian gagal/terbalik/lebih dari 50 calon | Keadaan benar; pilihan tepat; hasil terbaru tidak ditimpa |
| API langsung memilih bed dewasa atau boks untuk pasien dewasa | Ditolak server dengan alasan terbaca |
| Bayi isolasi, hanya tersedia isolasi dewasa | Tidak dianggap memenuhi kebutuhan neonatal |
| Ibu berpindah kamar, bayi rawat gabung | Ada validasi dan operasi perpindahan yang sah, bukan perubahan lokasi diam-diam |
| Bayi transfer rawat gabung→NICU→rawat gabung | Episode bayi tetap; histori lokasi dan waktu utuh; biaya sesuai periode |
| Ibu pulang/Closed, bayi masih dirawat | Bayi tidak ikut ditutup; hubungan historis terbaca; koreksi bayi tetap mungkin sesuai kewenangan |
| Bayi pulang kemudian masuk lagi | RM reuse, episode baru, penjagaan duplikasi aktif; tidak menyambung episode lama sembarangan |
| Dua invoice dibayar terpisah/bersama/sebagian | Saldo/alokasi tepat; tidak menggandakan dana; tidak mengubah subjek pelayanan |
| Deposit ibu, penjamin bayi berbeda | Tidak ada pemindahan saldo/cakupan otomatis |
| Provider timeout sesudah menerima uang | Hasil direkonsiliasi sebelum retry; tidak menggandakan penerimaan |
| Refund bayi sesudah pembayaran bersama | Referensi dan saldo sesuai alokasi bayi |
| JKN bayi sehat versus sakit | Catatan pasien terpisah; pemetaan paket/klaim oleh tim penjamin; tidak menjadikan dua invoice internal sebagai dua klaim otomatis |
| Reload dan mundur setelah penyimpanan sebagian | Tidak membuat pasien/episode/reservasi kedua; status tersimpan dipulihkan |
| Cetak/revisi dokumen | Identitas bayi, urutan kembar, wali dan sumber versi tepat |
| Nama awal dari ibu dan nama resmi kemudian | Saran “Bayi [Nama Ibu]” tampil benar; perubahan nama mempertahankan RM/episode dan revisi cetakan |
| Rencana deposit Rp10 juta diinput, belum dibayar kasir | Layar tidak menyatakan uang sudah diterima; angka minimum bayi hanya mengikuti kebijakan yang sah |
| Status deposit gagal dibaca | Tampil belum dapat dibaca, bukan Rp0 atau lunas |
| Membuka draft untuk reservasi/persetujuan | Draft tidak dianggap penempatan aktif; prasyarat identitas, DPJP dan penjamin tetap terpenuhi |
| Waktu lahir 03:20, input 04:05, aktivasi 04:15 | Ketiga waktu tidak tertukar; serah terima belum terjadi tetap belum tercatat |
| Retry simpan/aktivasi dan tombol Selesai | Event keberhasilan tidak digandakan; tidak membuat waktu serah terima palsu |

**Validasi pada analisis ini:** pembacaan source, dokumen desain, status Git, hash berkas, serta rujukan resmi. Tidak ada build/test/browser/API/database yang dijalankan. Pada implementasi berikutnya, jalankan pemeriksaan yang diwajibkan governance sesuai file yang diubah; frontend menggunakan lint:errors, unit test yang bermakna, build dan UAT terkait. Backend mengikuti kebijakan validasi yang berlaku dan otorisasi lingkungan. Tidak menerapkan migration ke database sebagai bagian diagnosis.

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-10-14 | Rencana dibuat, 9 perbaikan/handoff, skema sebelum–sesudah, 4 jalur bayi, dan verifikasi; implementasi belum dilakukan | diagnose-module-issue |
| 2026-10-14 | Setelah pengguna meminta analisis meeting, skema per tahap sempat ditunda; bukti meeting dicatat pada issue bagian 1.3 | diagnose-module-issue |
| 2026-10-14 | Atas instruksi “oke update stepper admisi episode bayi”, bagian 3 diperinci menjadi sembilan skema langkah, dua varian tempat, status reservasi, handoff dua invoice, rencana deposit/kasir, gelang bernama ibu, koreksi dan waktu layanan. Acceptance criteria serta skenario verifikasi diselaraskan; source aplikasi tidak diubah | diagnose-module-issue |

| 2026-10-14 | Pengguna menyetujui implementasi pasien baru dan pasien lama; source BE/FE, migration, lima uji browser dan laporan task ditambahkan. Packaging standalone/DB nyata/Billing settlement belum ditutup | build-module-backend / build-module-frontend |

## Handoff implementasi - 15 Oktober 2026

Pengguna membatasi pekerjaan berikutnya pada kode dan persiapan migration; build dan migration dilakukan mandiri oleh pengguna. Implementasi source admisi bayi pasien baru/lama sudah diserahkan, termasuk pemulihan konteks dan pembersihan identitas saat memulai pasien berikutnya. BE-RWI-231 dan FE-RWI-245 selesai pada scope source; readiness runtime dibuktikan pemilik setelah build/migration. Preferensi penagihan bersama tersedia, sedangkan settlement lintas invoice tetap pekerjaan Billing terpisah.
