# Hemodialisa — Matriks Uji Penerimaan

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Contract version | `HMD-CONTRACT-v1` — `last_changed_in: HMD-CONTRACT-v1`, status `approved` |
| Owner | Muhammad Hamzah |
| `approved_by` / `approved_at` | Muhammad Hamzah / 2026-09-18T15:40:07+07:00 |
| `input_revision` | `contracts/state-transition-matrix.md` r1; `contracts/validation-matrix.md` r1 |

Matriks ini memuat **jalur gagal, bukan hanya jalur berhasil**. Seluruh contoh memakai nama
samaran; tidak ada data pasien asli.

Jenis uji yang dipakai: **Unit** (satu aturan pada satu service), **Integrasi** (beberapa
komponen backend bersama basis data), **Otorisasi** (hak akses di server), dan **UAT** (dijalankan
orang non-teknis lewat layar).

---

## 1. Permintaan HD

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `HMD-AC-013` | Permintaan dikirim tanpa konteks kunjungan | Unit | Permintaan ditolak; tidak ada baris tersimpan |
| `HMD-AC-013` | Permintaan dikirim lengkap dari layar dokter bangsal | UAT | Permintaan muncul pada daftar permintaan masuk unit HD |
| `HMD-AC-015` | Permintaan dibuat dari bangsal, lalu satu lagi dari poliklinik | Integrasi | Rujukan episode rawat inap terisi pada yang pertama dan kosong pada yang kedua |
| `HMD-AC-014` | Permintaan diterima koordinator | UAT | Permintaan berubah menjadi diterima; **belum** ada sesi terjadwal yang terbentuk |
| `HMD-VAL-005` | Koordinator mencoba menolak permintaan | Otorisasi | Ditolak dengan kode 403; hanya dokter yang dapat menolak |
| `HMD-VAL-005` | Dokter menolak tanpa mengisi alasan klinis | Unit | Ditolak; alasan wajib diisi |
| `HMD-VAL-003` | Koordinator menahan permintaan tanpa alasan | Unit | Ditolak; alasan wajib diisi |
| `HMD-VAL-004` | Permintaan yang ditahan dilepas | Integrasi | Status kembali ke keadaan sebelum ditahan, bukan ke keadaan awal |
| Matriks status | Permintaan yang sudah ditolak dicoba diterima | Unit | Ditolak; penolakan bersifat akhir |
| `HMD-VAL-006` | Pembuat membatalkan permintaan yang sudah diterima unit HD | Unit | Ditolak dengan kode 409 |

---

## 2. Episode dan resep

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `HMD-AC-001` | Ibu Sinta sudah punya episode aktif; episode kedua dicoba diaktifkan | Integrasi | Ditolak dengan kode 409; hanya satu episode aktif tersimpan |
| `HMD-AC-001` | Dua petugas mengaktifkan episode untuk pasien yang sama pada saat hampir bersamaan | Integrasi | Tepat satu berhasil; index basis data menahan yang kedua |
| `HMD-VAL-013` | Episode ditutup padahal masih ada sesi yang belum disahkan | Integrasi | Ditolak; episode tetap aktif |
| Matriks status | Episode yang sudah ditutup dicoba diaktifkan kembali | Unit | Ditolak; pasien yang kembali mendapat episode baru |
| `HMD-VAL-024` | Dokter menyunting resep yang sudah aktif | Unit | Ditolak dengan kode 423 |
| `HMD-VAL-022` | Resep baru diaktifkan saat sudah ada resep aktif | Integrasi | Resep lama menjadi digantikan; tepat satu resep aktif tersisa; rujukan penggantinya terisi |
| `HMD-VAL-021` | Resep diaktifkan sementara akses vaskular yang dirujuk sudah tidak layak | Unit | Ditolak; resep tetap draf |
| `HMD-VAL-023` | Resep aktif dibatalkan saat ada sesi berjalan yang memakainya | Integrasi | Ditolak dengan kode 409 |

**Contoh `HMD-AC-001` sebagai UAT:** petugas administrasi membuka daftar pasien, memilih Ibu
Sinta yang sudah punya episode `HD-2026-00042` berstatus aktif, lalu membuat episode baru dan
menekan Aktifkan. Layar menampilkan pesan bahwa pasien sudah punya episode aktif. Daftar pasien
tetap menampilkan satu episode aktif untuk Ibu Sinta.

---

## 3. Penjadwalan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `HMD-AC-002` | Dua pasien dijadwalkan ke mesin `M-03` pada rentang waktu bertumpang tindih | Integrasi | Tepat satu berhasil; yang kedua ditolak dengan kode 409 |
| `HMD-AC-003` | Dua pasien dijadwalkan ke station `HD-02` pada rentang waktu bertumpang tindih | Integrasi | Tepat satu berhasil |
| `HMD-AC-004` | Satu pasien dijadwalkan dua kali pada rentang waktu bertumpang tindih | Integrasi | Tepat satu berhasil |
| `HMD-AC-005` | Mesin diblokir, lalu dipakai menjadwalkan | Unit | Ditolak; mesin tidak muncul sebagai pilihan yang sah |
| `HMD-AC-005` | Mesin diblokir, lalu dipakai memulai sesi yang tadi sudah terjadwal | Integrasi | Ditolak saat tombol Mulai ditekan |
| `HMD-VAL-035` | Pasien yang perlu isolasi dijadwalkan ke mesin umum | Integrasi | Ditolak; koordinator memilih mesin khusus |
| `HMD-AC-022` | Perawat ditugaskan saat pembacaan kewenangan belum tersedia | Integrasi | Status verifikasi tersimpan **belum dapat diverifikasi**, bukan terverifikasi |
| `HMD-AC-024` | Penegakan kewenangan dinyalakan, lalu petugas tanpa kewenangan ditugaskan | Integrasi | Ditolak dengan pesan yang jelas |
| `HMD-AC-023` | Penegakan kewenangan dinyalakan lewat pengaturan | Integrasi | Perilaku berubah; **tidak ada** perubahan struktur tabel yang dibutuhkan |
| `HMD-VAL-038` | Rasio pasien per perawat terlampaui, penegakan rasio mati | UAT | Peringatan tampil; penjadwalan **tetap berhasil** |
| `HMD-VAL-030` | Sesi dijadwalkan untuk pasien tanpa resep aktif | Unit | Ditolak |

**Contoh `HMD-AC-002` sebagai UAT:** dua koordinator membuka layar penjadwalan di dua komputer.
Keduanya memilih mesin `M-03` pada hari yang sama, satu pukul 07.00–11.00 dan satu pukul
09.00–13.00, lalu menekan Simpan pada waktu hampir bersamaan. Satu berhasil; yang lain menerima
pesan bahwa mesin sudah dipakai pasien lain pada jam tersebut. Daftar kerja menampilkan tepat
satu sesi pada mesin itu.

---

## 4. Kesiapan unit

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `HMD-VAL-101` | Unit dinyatakan siap padahal butir pengolahan air belum terpenuhi | Unit | Ditolak; unit tetap draf |
| `HMD-VAL-102` | Hasil pemeriksaan air berumur 40 hari, masa berlaku disetel 30 hari | Integrasi | Ditolak; unit tidak dapat dinyatakan siap |
| `HMD-VAL-102` | Masa berlaku diubah menjadi 60 hari lewat pengaturan, lalu dicoba lagi | Integrasi | Berhasil; **tidak ada** perubahan kode yang dibutuhkan |
| `HMD-VAL-100` | Lembar kesiapan dibuat dua kali untuk tanggal dan shift yang sama | Integrasi | Yang kedua ditolak dengan kode 409 |
| `HMD-VAL-042` | Unit dinyatakan tidak siap, lalu sesi pada shift itu dicoba dinyatakan siap | Integrasi | Ditolak |
| Flowchart kesiapan | Unit dinyatakan tidak siap **setelah** ada sesi yang sedang berjalan | Integrasi | Sesi yang berjalan **tidak** dihentikan otomatis |

---

## 5. Pra-HD dan memulai sesi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `HMD-VAL-040` | Sesi dinyatakan siap padahal satu butir wajib belum terpenuhi | Unit | Ditolak |
| `HMD-VAL-044` | Dokter mencoba melewati butir persiapan saat seluruh butir bertanda tidak boleh dilewati | Integrasi | **Ditolak** — inilah keadaan bawaan sebelum badan klinis menetapkan |
| `HMD-VAL-044` | Satu butir diubah menjadi boleh dilewati lewat layar master, lalu dicoba lagi | Integrasi | Berhasil; alasan, nama dokter, dan waktunya tersimpan. **Tidak ada** perubahan kode |
| `HMD-VAL-046` | Butir dilewati tanpa mengisi alasan | Unit | Ditolak |
| `HMD-AC-016` | Sesi dinyatakan siap tanpa dokter penanggung jawab | Integrasi | Ditolak dengan pesan yang jelas |
| `HMD-AC-006` | Tombol Mulai ditekan dua kali dengan penanda idempotency yang sama | Integrasi | Tepat satu sesi berjalan dan tepat satu tindakan pasien terbentuk |
| `HMD-AC-006` | Tombol Mulai ditekan dua kali dari dua perangkat berbeda | Integrasi | Yang kedua ditolak dengan kode 409 |
| `HMD-VAL-051` | Mesin diblokir setelah sesi dinyatakan siap, sebelum Mulai ditekan | Integrasi | Ditolak saat Mulai; perawat memilih mesin lain |
| `HMD-VAL-052` | Kunjungan pasien dibatalkan setelah sesi dinyatakan siap | Integrasi | Ditolak saat Mulai |
| `HMD-AC-011` | Waktu palsu dikirim dari perangkat pengguna saat memulai sesi | Integrasi | Waktu mulai yang tersimpan tetap **waktu server** |

**Contoh `HMD-VAL-044` sebagai UAT, dan inilah uji terpenting bagi `HMD-ASM-001`:** perawat
membuka Pra-HD, sebelas butir hijau, satu butir akses vaskular merah. Dokter menekan Lewati dan
mengetik alasan. Layar menampilkan pesan bahwa butir itu tidak dapat dilewati. Setelah pemegang
akun tata kelola klinis mengubah penanda butir itu lewat layar master butir persiapan, dokter
mengulangi langkah yang sama dan kali ini berhasil — tanpa ada penerapan kode baru.

---

## 6. Pemantauan, obat, dan komplikasi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `HMD-AC-007` | Lima pemantauan disimpan berurutan lalu dibaca kembali | Integrasi | Kelimanya ada, urut waktu, tidak ada yang tertimpa |
| `HMD-AC-007` | Dua perawat menyimpan pemantauan pada saat hampir bersamaan | Integrasi | Keduanya tersimpan dengan nomor urut berbeda |
| `HMD-VAL-054` | Pemantauan dicatat saat sesi belum berjalan | Unit | Ditolak |
| `HMD-VAL-055` | Waktu pengamatan diisi lebih awal dari waktu sesi dimulai | Unit | Ditolak |
| `HMD-VAL-056` | Pemberian obat dicatat tanpa dosis | Unit | Ditolak |
| Kontrak integrasi | Penerusan pemakaian obat ke Farmasi gagal | Integrasi | Catatan klinis pemberian obat **tetap tersimpan**; penerusan masuk daftar untuk diulang |
| `HMD-VAL-057` | Komplikasi dicatat tanpa tindakan penanganan | Unit | Ditolak |
| Flowchart pelaksanaan | Komplikasi dicatat dengan dampak sesi dihentikan | Integrasi | Sesi berpindah ke dihentikan; alasan penghentian tersimpan |

---

## 7. Pasca-HD, finalisasi, dan koreksi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `HMD-VAL-060` | Sesi diselesaikan tanpa mengisi tujuan pasien | Unit | Ditolak |
| `HMD-VAL-070` | Dokumentasi diselesaikan padahal berat badan akhir kosong | Unit | Ditolak |
| Matriks status | Perawat menyelesaikan dokumentasi | Integrasi | Sesi menunggu pengesahan; nama dan waktu penyelesai tersimpan |
| `HMD-VAL-072` | Perawat mencoba mengesahkan catatan | Otorisasi | Ditolak dengan kode 403 |
| `HMD-VAL-072` | Dokter lain yang bukan penanggung jawab sesi mencoba mengesahkan | Integrasi | Ditolak; mesin hak akses saja tidak cukup, aturan bisnis yang menahan |
| `HMD-VAL-074` | Orang yang sama menyelesaikan dokumentasi lalu mengesahkan, sementara pengaturan menghendaki dua orang berbeda | Integrasi | Ditolak |
| `HMD-VAL-073` | Pendaftaran dokumen ke rekam medis gagal saat pengesahan | Integrasi | Seluruh langkah dibatalkan; sesi **tetap** menunggu pengesahan, tidak terkunci setengah jalan |
| `HMD-AC-008` | Catatan sesi yang sudah disahkan dicoba diubah langsung | Integrasi | Ditolak dengan kode 423 |
| `HMD-AC-008` | **Uji khusus Temuan Kritis 1** — jenis dokumen ditambahkan hanya pada daftar jenis, tanpa ditambahkan pada himpunan yang ditegakkan | Integrasi | Uji ini **wajib gagal** bila langkah kedua terlewat. Inilah satu-satunya cara menangkap kegagalan yang tidak menampilkan pesan apa pun |
| Flowchart koreksi | Koreksi dibuat pada catatan yang sudah disahkan | Integrasi | Catatan asli utuh; koreksi tersimpan sebagai tambahan beserta alasan, penulis, dan waktunya |
| `HMD-VAL-071` | Dokter mengembalikan catatan tanpa mengisi alasan | Unit | Ditolak |

**Uji `HMD-AC-008` varian Temuan Kritis 1 perlu penjelasan.** Ia sengaja dirancang untuk gagal
bila langkah kedua terlewat. Caranya: finalisasi satu sesi, lalu kirim permintaan ubah langsung
pada catatan itu. Bila jawabannya **bukan** penolakan, berarti jenis dokumen belum terdaftar
pada himpunan yang ditegakkan — dan itu tidak akan terlihat dari layar mana pun, karena layar
tetap menampilkan sesi sudah disahkan.

---

## 8. Penyerahan ke penagihan

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `HMD-AC-020` | Sesi selesai normal lalu disahkan | Integrasi | Tepat satu tagihan bersumber tindakan terbit |
| `HMD-AC-019` | Sesi dihentikan pada menit ke-40 lalu disahkan | Integrasi | Tindakan terbentuk bertanda **tidak dapat ditagih** beserta alasan penghentian; **tidak ada** tagihan terbit |
| `HMD-AC-021` | Sesi dibatalkan sebelum dimulai | Integrasi | Tidak ada tindakan dan tidak ada tagihan |
| `HMD-AC-009` | Penyerahan ke penagihan digagalkan secara sengaja | Integrasi | Sesi **tetap** berstatus disahkan; status penyerahan gagal; catatan tidak terbuka kembali |
| `HMD-AC-009` | Penyerahan yang gagal dijalankan ulang tiga kali | Integrasi | Tepat satu tagihan terbentuk, bukan tiga |
| `HMD-AC-017` | Sesi dijalankan dengan dokter penanggung jawab dan dokter pembuat resep yang berbeda | Integrasi | Kolom dokter pada tindakan terisi dokter penanggung jawab; kolom dokter pemberi instruksi terisi dokter pembuat resep |
| `HMD-AC-018` | Dokter penanggung jawab sesi diganti setelah sesi disahkan | Integrasi | Dokter pada tindakan yang sudah diserahkan **tidak** ikut berubah |

**Contoh `HMD-AC-019` sebagai UAT:** Bapak Darma mulai cuci darah pukul 07.12. Pukul 07.52
tekanan darahnya turun tajam, perawat menghentikan sesi dan mengisi alasannya. Dokumentasi
diselesaikan, dokter mengesahkan. Petugas kasir membuka tagihan pasien: tidak ada baris tagihan
hemodialisa yang terbit. Pada catatan klinis, tindakan tetap tercatat lengkap beserta alasan
penghentiannya.

---

## 9. Hak akses dan privasi

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `HMD-AC-012` | Setiap endpoint dipanggil langsung oleh pengguna tanpa hak akses yang sesuai | Otorisasi | Seluruhnya ditolak dengan kode 403, tanpa kecuali |
| `HMD-AC-012` | Tombol yang tidak berhak diperiksa pada layar | UAT | Tombol **disembunyikan**, bukan tampil lalu ditolak |
| Matriks hak akses | Koordinator membuka layar master butir persiapan | Otorisasi | Dapat melihat daftar; tombol mengubah penanda boleh dilewati **tidak** tersedia |
| Matriks hak akses | Auditor berwenang membuka seluruh layar | Otorisasi | Seluruh data terbaca; tidak satu pun tombol yang mengubah data tersedia |
| Privasi | Daftar kerja harian diperiksa isinya | UAT | Penanda isolasi menampilkan perlu atau tidak; **tidak** menyebut jenis infeksinya |
| Privasi | Payload logger diperiksa setelah menyimpan tinjauan serologi | Integrasi | Payload memuat id baris, controller, aksi, dan status; **tidak** memuat hasil serologi |
| Matriks hak akses | Pembacaan tinjauan serologi | Integrasi | Tercatat di logger, walaupun jenisnya pembacaan |

---

## 10. Perilaku layar

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
|---|---|---|---|
| `HMD-AC-010` | Konteks sesi digagalkan, lalu ruang kerja sesi dibuka | UAT | Tidak ada data klinis tampil; tidak ada tombol yang menulis data; pesan beserta tombol coba lagi |
| Data basi | Pengguna berpindah dari ruang kerja pasien A ke pasien B | UAT | Data pasien A **tidak** tetap tampil selama data B dimuat |
| Empat keadaan | Setiap layar daftar dibuka dalam keadaan memuat, kosong, gagal, dan berisi | UAT | Keempatnya menampilkan bentuk yang sesuai; keadaan kosong menyebut saringan yang aktif |
| Pengiriman ganda | Tombol Simpan ditekan berkali-kali dengan cepat | UAT | Tombol terkunci selama pengiriman; tidak ada data ganda |
| Keterjangkauan | Seluruh layar dicoba dicapai lewat menu atau layar induknya | UAT | Tidak ada satu pun layar yang hanya dapat dibuka lewat pengetikan alamat langsung |

---

## 11. Yang Tidak Diuji pada Phase 1

| Hal | Sebabnya |
|---|---|
| Perhitungan adekuasi dan tren longitudinal | Di luar scope Phase 1 |
| Pengiriman ke SATUSEHAT | Di luar scope Phase 1 |
| Pembacaan status JKN dan SEP | Di luar scope Phase 1 |
| Indikator mutu | Di luar scope Phase 1 |
| Pembacaan hasil laboratorium per pasien | Milik Laboratorium, belum tersedia (`HMD-DEP-001`) |
| Penegakan kewenangan klinis yang benar-benar membaca data Human Resource | Milik Human Resource, belum tersedia (`HMD-DEP-002`). Yang diuji adalah **sakelarnya**, bukan pembacaannya |

Baris terakhir penting dibaca dengan benar: yang belum dapat diuji adalah apakah pembacaan
kewenangan menghasilkan jawaban yang benar. Yang **sudah** dapat diuji, dan wajib diuji, adalah
bahwa status tersimpan sebagai belum dapat diverifikasi, dan bahwa menyalakan sakelarnya mengubah
perilaku tanpa perubahan tabel.
