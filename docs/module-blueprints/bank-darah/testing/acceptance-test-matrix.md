# Bank Darah — Acceptance Test Matrix

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v4` — **`approved`** |
| Sumber | `00-interview-decisions.md` revisi 9 (`AC-BD-001`..`097`) · `contracts/state-transition-matrix.md` · `contracts/validation-matrix.md` |

Wajib memuat **jalur gagal**, bukan hanya jalur berhasil. Jenis test: `Unit` (aturan service), `Integ`
(service + DB), `Concurrency` (perebutan data), `E2E/UAT` (jalur pengguna). Data samaran.

---

## 1. Order darah & deteksi ganda

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-BD-001` | Ada order PRC aktif, dibuat order PRC lagi (pasien+kunjungan sama) | Integ | Ditahan, minta alasan (`VAL-BD-001`) |
| `AC-BD-002` | Ada order PRC aktif, dibuat order trombosit | Integ | Boleh dibuat |
| `AC-BD-003` | Kunjungan berbeda, dibuat order PRC | Integ | Boleh dibuat |
| `AC-BD-004` | Kunjungan RJ `Completed`, order PRC belum terpenuhi | Integ | Order berhenti menahan order baru |
| `AC-BD-010` | Order manual tanpa pasien/kunjungan/dokter/unit | Unit | Ditolak (`VAL-BD-010`) |
| `AC-BD-011` | Order tersimpan | Integ | Menyimpan pelaku input |
| `AC-BD-013` | Unit tak dikonfigurasi berwenang membuat order | Integ | Ditolak (`VAL-BD-013`) |
| `AC-BD-015` | Unit diberi kewenangan lewat konfigurasi, tanpa ubah kode | Integ | Unit langsung dapat membuat order |
| `AC-BD-016` | Unit tanpa konfigurasi kewenangan | Integ | Ditolak — tak ada kewenangan bawaan |
| `AC-BD-017` | Rawat inap `PhysicallyLeftAt` Senin siang, episode ditutup Rabu | Integ | Order tak aktif sejak Senin siang, bukan Rabu |

## 2. Permintaan PMI & penerimaan

| Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- |
| `AC-BD-005` | Minta 3 PRC, diterima 2 hari pertama | Integ | `PartiallyFulfilled`, sisa 1 |
| `AC-BD-006` | Permintaan belum dikirim, dibuat permintaan baru kebutuhan sama | Integ | Ditolak (`VAL-BD-006`) |
| `AC-BD-009` | Permintaan dikirim, darah belum diterima fisik | Integ | Stok tak bertambah |
| `AC-BD-014` | Permintaan tanpa jumlah kantong | Unit | Ditolak (`VAL-BD-007`) |
| `AC-BD-022` | Sisa 1 kantong saat kunjungan berakhir | Integ | `ClosedEncounter`, riwayat utuh |
| `AC-BD-023` | Kantong datang setelah `ClosedEncounter` | Integ | Penerimaan dicatat, kantong → `PendingReview` |
| `AC-BD-031` | Minta 2 PRC, datang 3 | Integ | `Fulfilled` sisa 0 (bukan −1); 3 kantong tercatat |
| `AC-BD-032` | Kantong ke-3 pada `AC-BD-031` | Integ | `PendingReview` + alasan "kiriman melebihi permintaan", muncul di daftar #2 |
| `AC-BD-033` | Kantong berlebih dialokasikan langsung ke order pasien sama | Integ | Ditolak (`VAL-BD-033`) |

## 3. Alokasi, bukti, pemberian, koreksi

| Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- |
| `AC-BD-018` | Kantong dialokasikan, bukti belum tercatat, tekan pemberian | Integ | Ditolak (`VAL-BD-018`) |
| `AC-BD-019` | Bukti lengkap lalu pemberian | Integ | Berhasil; kantong `Issued` |
| `AC-BD-020` | Jalur darurat oleh peran berwenang + alasan | Integ | Berhasil, ditandai tanpa bukti, muncul di daftar #3 |
| `AC-BD-021` | Jalur darurat oleh peran tak berwenang | Integ | Ditolak (`VAL-BD-021`) |
| `AC-BD-038` | Bukti tercatat, masa berlaku lewat, tekan pemberian | Integ | Ditolak (`VAL-BD-020`); bukti lama tetap riwayat |
| `AC-BD-039` | Bukti masih di dalam masa berlaku | Integ | Pemberian berhasil |
| `AC-BD-040` | Masa berlaku komponen belum dikonfigurasi | Integ | Pemberian ditahan (`VAL-BD-020b`), tak pakai nilai tebakan |
| `AC-BD-041` | Kantong berbukti pasien A dialihkan ke B, pemberian ke B | Integ | Ditolak (`VAL-BD-019`); bukti A tetap riwayat |
| `AC-BD-042` | Setelah pengalihan, bukti baru untuk B tercatat | Integ | Pemberian ke B berhasil |
| `AC-BD-043` | Batalkan alokasi, kantong belum diberikan, order aktif | Integ | Kantong kembali `Available`; riwayat tersimpan |
| `AC-BD-044` | Batalkan alokasi, order asal sudah berakhir | Integ | Kantong `PendingReview`, bukan `Available` |
| `AC-BD-045` | Batalkan alokasi tanpa alasan terkendali | Unit | Ditolak (`VAL-BD-016`) |
| `AC-BD-046` | Batalkan alokasi kantong yang sudah `Issued` | Integ | Ditolak (`VAL-BD-023`) |
| `AC-BD-047` | Koreksi oleh peran berwenang + alasan | Integ | Berhasil; pemberian asal terbaca; pemenuhan dihitung ulang |
| `AC-BD-048` | Pemberian dicoba dihapus/dianulir | Integ | Ditolak (`VAL-BD-025`) |
| `AC-BD-049` | Koreksi dipakai memindah pemberian ke pasien lain | Integ | Ditolak (`VAL-BD-049`) |
| `AC-BD-050` | Koreksi oleh peran tak berwenang | Integ | Ditolak (`VAL-BD-024`) |
| — konkurensi | Dua petugas alokasikan kantong sama untuk dua pasien | Concurrency | Tepat satu berhasil, satu `409` (`VAL-BD-018c`) |

## 4. Golongan darah & konflik

| Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- |
| `AC-BD-012` | Golongan darah pada permintaan dipakai menilai cocok | Integ | Ditolak (`VAL-BD-012`) |
| `AC-BD-028` | `MstPatient.BloodType` dipakai untuk keperluan klinis | Integ | Ditolak — bukan sumber sah (`VAL-BD-012`) |
| `AC-BD-030` | Hasil golongan darah tanpa pemeriksa/waktu | Unit | Ditolak (`VAL-BD-030`) |
| `AC-BD-034` | Hasil sah O+, muncul hasil tervalidasi baru A+ | Integ | Pasien tak punya hasil sah; gerbang tertahan; kedua hasil tersimpan |
| `AC-BD-035` | Hasil tervalidasi baru sama dengan hasil sah sebelumnya | Integ | Hasil terbaru berlaku tanpa penahanan |
| `AC-BD-036` | Konflik diselesaikan validator lewat pemeriksaan ulang | Integ | Tepat satu hasil sah; pelaku/alasan/waktu tersimpan |
| `AC-BD-037` | Konflik dicoba diselesaikan bukan validator | Integ | Ditolak (`VAL-BD-037`) |
| `AC-BD-051` | Konflik dicoba ditutup tanpa pemeriksaan ulang | Integ | Ditolak (`VAL-BD-051`) |
| `AC-BD-053` | Pemeriksaan ulang beri nilai ketiga, validator menyatakannya berlaku | Integ | Diterima — tak dipaksa cocok hasil lama |
| `AC-BD-054` | Konflik ditutup dengan sistem pilih mayoritas otomatis | Unit | Ditolak (`VAL-BD-054`) |

## 5. Kantong menunggu keputusan & tindakan

| Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- |
| `AC-BD-007` | Kunjungan berakhir, 2 kantong sudah diterima | Integ | Kedua kantong `PendingReview`, tak dapat dialokasikan ke pasien lain langsung |
| `AC-BD-008` | Ada kantong `PendingReview` | E2E | Muncul di daftar kerja #2 |
| `AC-BD-024` | Kantong `PendingReview` dialihkan dengan alasan | Integ | Berhasil; rantai pasien asal→alasan→tujuan tersimpan |
| `AC-BD-025` | Kantong `PendingReview` diselesaikan tanpa alasan | Unit | Ditolak (`VAL-BD-016`) |
| `AC-BD-029` | Alasan tidak layak diketik bebas | Unit | Ditolak (`VAL-BD-016`) |
| `AC-BD-026` | Satu tindakan selesai dengan 2 kantong diberikan | Integ | Satu fakta biaya (bila kontrak Billing turun), bukan dua — **ditandai tertunda `DEC-BD-016`** |
| `AC-BD-027` | Fakta biaya tindakan sama dikirim ulang | Integ | **Tertunda `DEC-BD-016`** — tidak diuji sampai kontrak Billing disetujui |

---

## 7. Penyimpanan kantong dan gerbang lokasi — baru pada `v2`

Menutup `AC-BD-059` sampai `AC-BD-076`. Jalur gagal ditulis lebih dulu karena di sinilah keselamatan
pasien benar-benar dijaga.

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-BD-059` | Kantong baru diterima dari PMI | Integ | Status `Received`; belum dapat dialokasikan |
| `AC-BD-060` | Kantong `Received` dicoba dialokasikan | Integ | **Ditolak** `VAL-BD-063`; pesan menyebut kantong belum disimpan |
| `AC-BD-061` | Petugas menetapkan lokasi pada kantong `Received` | Integ | Kantong `Stored` lalu `Available`; satu baris riwayat penempatan bertambah |
| `AC-BD-062` | Lokasi nonaktif dipilih untuk penyimpanan baru | Integ | **Ditolak** `VAL-BD-060` |
| `AC-BD-063` | Kantong `Stored` dipindahkan ke lokasi lain | Integ | Riwayat bertambah; **status tidak berubah**; catatan penerimaan awal tidak tersentuh |
| `AC-BD-064` | Sistem diminta mencatat suhu atau kapasitas storage | Unit | Tidak ada kolom maupun endpoint — di luar scope MVP |
| `AC-BD-065` | Lokasi nonaktif dipilih untuk penyimpanan kantong baru | Integ | **Ditolak** `VAL-BD-060` |
| `AC-BD-066` | Lokasi nonaktif dipilih sebagai **tujuan perpindahan** | Integ | **Ditolak** `VAL-BD-060` — aturan berlaku untuk perpindahan, bukan hanya penempatan pertama |
| `AC-BD-067` | Lokasi dinonaktifkan sementara masih ada kantong di dalamnya | Integ | Penonaktifan **berhasil**; kantong tetap tercatat di sana; status kantong tidak berubah; peringatan `VAL-BD-068` menyebut jumlah |
| `AC-BD-068` | Kantong di lokasi nonaktif dicoba dialokasikan | Integ | **Ditolak** `VAL-BD-064` |
| `AC-BD-069` | Sistem diminta memindahkan sendiri kantong saat lokasi dinonaktifkan | Integ | **Tidak terjadi** — nol baris riwayat penempatan baru, nol perubahan status, nol job berjalan |
| `AC-BD-070` | Petugas memindahkan kantong dari lokasi nonaktif ke lokasi aktif lalu mengalokasikan | Integ | Berhasil; riwayat mencatat pelaku dan waktu; gerbang terbuka kembali |
| `AC-BD-071` | Kantong `PendingReview` di lokasi nonaktif dicoba dialihkan ke pasien lain | Integ | **Ditolak** `VAL-BD-064` — pengalihan adalah alokasi dengan nama lain |
| `AC-BD-072` | Kantong sudah dialokasikan dan bukti masih berlaku, lokasinya dinonaktifkan **sesudah** alokasi, lalu dicoba diberikan | **Integ** | **Ditolak** `VAL-BD-065`. Skenario inti `DEC-BD-038` — membuktikan gerbang dinilai **ulang**, bukan diwarisi |
| `AC-BD-073` | Kantong yang sama dipindahkan ke lokasi aktif lalu diberikan | Integ | Berhasil; alokasi ke pasien tujuan tidak pernah putus sepanjang kejadian |
| `AC-BD-074` | Kantong di lokasi nonaktif diberikan lewat otorisasi darurat | Integ | Diizinkan; alasan, pelaku, waktu tercatat; penanda permanen melekat |
| `AC-BD-075` | Pemberian darurat dicatat tanpa menyebut gerbang yang dilewati | Integ | **Ditolak** `VAL-BD-066` |
| `AC-BD-076` | Gerbang pemberian dicoba dilewati dengan mewarisi hasil pemeriksaan saat alokasi | Unit | **Ditolak** — kedua gerbang wajib dinilai pada saat tindakan dicoba |

Skenario tambahan yang tidak berasal dari `AC-BD-*` tetapi menutup risiko rancangan:

| Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- |
| Dua petugas memindahkan kantong yang sama ke dua lokasi berbeda hampir bersamaan | **Concurrency** | Tepat satu berhasil; kantong tidak pernah punya dua penempatan berlaku (filtered unique index + token `Version`) |
| Master lokasi kosong sama sekali, kantong baru diterima | Integ | Kantong berhenti di `Received`; modul **fail-closed**; pesan mengarahkan ke Setup |
| Lokasi dinonaktifkan lalu diaktifkan kembali | Integ | Gerbang kantong di dalamnya terbuka kembali tanpa satu pun kantong disunting |
| Perpindahan lokasi pada kantong yang sedang `Allocated` | Integ | Alokasi, pasien tujuan, dan bukti kecocokan seluruhnya tetap utuh |
| Percobaan mengubah atau menghapus baris riwayat penempatan | Unit | Tidak ada jalur bisnis yang menyediakannya (`INV-BD-026`) |
| Penonaktifan lokasi pada 500 kantong sekaligus | **Concurrency** | Satu `UPDATE` pada satu baris master; **nol** penyuntingan baris kantong; waktu tanggap tidak bergantung jumlah kantong |

---

## 8. Wewenang, jalur darurat, dan koreksi dua tahap — baru pada `v3`

Menutup `AC-BD-077` sampai `AC-BD-088`.

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-BD-077` | Petugas BDRS berwenang validasi memvalidasi hasil golongan darah rutin | Integ | Berhasil; tidak menunggu Dokter BDRS |
| `AC-BD-078` | Petugas BDRS berwenang validasi mencoba menutup konflik | Integ | **Ditolak** `VAL-BD-069` — dua butir hak akses berbeda (`DEC-BD-039`) |
| `AC-BD-079` | Validator klinis menutup konflik dengan menunjuk pemeriksaan ulang tervalidasi | Integ | Berhasil; satu hasil sah kembali; seluruh hasil tetap terbaca |
| `AC-BD-080` | Validator klinis mencoba menutup konflik **tanpa** pemeriksaan ulang tervalidasi | Integ | **Ditolak** `VAL-BD-051` — wewenang tidak menggantikan prasyarat (`DEC-BD-031`) |
| `AC-BD-081` | DPJP menerbitkan otorisasi darurat saat Dokter BDRS tidak di tempat | Integ | Berhasil; rekam menyimpan peran `AttendingPhysician` |
| `AC-BD-082` | Dokter BDRS menerbitkan otorisasi darurat | Integ | Berhasil; rekam menyimpan peran `BloodBankDoctor` |
| `AC-BD-083` | Petugas Bank Darah tanpa wewenang darurat mencoba menerbitkan otorisasi | Integ | **Ditolak** `VAL-BD-072` |
| `AC-BD-084` | Otorisasi darurat dicatat tanpa keterangan kondisi kedaruratan | Unit | **Ditolak** `VAL-BD-070` |
| `AC-BD-085` | Otorisasi darurat dicatat tanpa menyebut peran penerbit | Unit | **Ditolak** `VAL-BD-071` |
| `AC-BD-086` | Petugas BDRS mengajukan koreksi pencatatan | Integ | Koreksi tersimpan `Requested`; **angka pemenuhan order tidak bergerak** (`INV-BD-033`) |
| `AC-BD-087` | Dokter BDRS menyetujui koreksi yang menunggu | Integ | Koreksi `Approved`; pemenuhan dihitung ulang sejak persetujuan; pemberian asal tetap utuh |
| `AC-BD-088` | Pengaju koreksi mencoba menyetujui permintaannya sendiri | **Integ** | **Ditolak** `VAL-BD-073`, walaupun ia memegang kedua butir hak akses |

Skenario tambahan yang tidak berasal dari `AC-BD-*` tetapi menutup risiko rancangan:

| Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- |
| Dokter BDRS **menolak** koreksi | Integ | Koreksi `Rejected` dan **tetap tersimpan**; angka pemenuhan tidak pernah bergerak; alasan penolakan wajib (`VAL-BD-077`) |
| Koreksi yang sudah disetujui dicoba diputuskan lagi | Integ | **Ditolak** `VAL-BD-075`; keputusan bersifat sekali |
| Dua Dokter BDRS memutuskan koreksi yang sama hampir bersamaan | **Concurrency** | Tepat satu keputusan tersimpan; yang kedua ditolak `VAL-BD-075` |
| Ringkasan pemenuhan order dihitung saat ada koreksi `Requested` dan `Approved` bercampur | **Unit** | Hanya yang `Approved` ikut diperhitungkan (`INV-BD-033`) |
| Pengajuan koreksi tanpa bukti pendukung | Unit | **Ditolak** `VAL-BD-076` |
| Seorang dokter memegang `Correct` dan `ApproveCorrection` sekaligus, mengajukan lalu meminta rekan memutuskan | Integ | Berhasil — yang ditahan adalah **orang yang sama**, bukan kepemilikan kedua butir hak akses |

---

## 9. Bukti kecocokan, penyelesaian bertingkat, dan pembatalan order — baru pada `v4`

Menutup `AC-BD-089` sampai `AC-BD-097`.

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-BD-089` | Petugas BDRS berwenang validasi menyatakan bukti kecocokan selesai | Integ | Berhasil; rekam menyimpan validator, waktu, kantong, pasien, dan hasil keputusan |
| `AC-BD-090` | Petugas BDRS tanpa kewenangan validasi mencoba menyatakan bukti kecocokan | Integ | **Ditolak** `VAL-BD-078` |
| `AC-BD-091` | Pelaksana pemeriksaan yang juga memegang kewenangan validasi menyatakan buktinya sendiri | Integ | **Diizinkan** — `DEC-BD-042` mengizinkan pelaksana berbeda dari validator, tidak mewajibkannya |
| `AC-BD-092` | Pemegang kewenangan operasional mengembalikan kantong `PendingReview` ke PMI | Integ | Berhasil; alasan, pelaku, waktu tersimpan |
| `AC-BD-093` | Pemegang kewenangan operasional yang sama mencoba mengalihkan kantong ke pasien lain | **Integ** | **Ditolak** `VAL-BD-080` — tiga butir hak akses terpisah (`INV-BD-034`) |
| `AC-BD-094` | Pemegang kewenangan klinis BDRS mengalihkan kantong ke pasien lain | Integ | Berhasil; bukti kecocokan terhadap pasien asal gugur seketika (`DEC-BD-028`) |
| `AC-BD-095` | Dokter peminta membatalkan ordernya dengan alasan berkategori klinis | Integ | Berhasil; alasan, pelaku, waktu, dan riwayat tersimpan (`INV-BD-035`) |
| `AC-BD-096` | Petugas BDRS membatalkan order ganda dengan alasan berkategori operasional | Integ | Berhasil; kategori alasan membedakannya dari pembatalan klinis |
| `AC-BD-097` | Pembatalan order dicoba tanpa alasan terkendali | Unit | **Ditolak** `VAL-BD-016` — tidak ada pembatalan order tanpa audit |

Skenario tambahan yang tidak berasal dari `AC-BD-*` tetapi menutup risiko rancangan:

| Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- |
| Bukti kecocokan bertanda **tidak cocok**, lalu kantong dicoba diberikan | **Integ** | **Ditolak** `VAL-BD-079`. Skenario inti `v4` — membuktikan gerbang memeriksa **hasil**, bukan keberadaan bukti |
| Bukti bertanda tidak cocok tetap terbaca pada riwayat kantong | Integ | Tersimpan utuh; tidak dibuang, sehingga uji yang sama tidak diulang orang berikutnya |
| Petugas mencoba menetapkan kantong tidak layak tanpa kewenangan penetapan kelayakan | Integ | **Ditolak** `VAL-BD-082` |
| Pembatalan order memakai kategori alasan yang tidak sesuai peran pelaku | Integ | **Ditolak** `VAL-BD-083` |
| Seeder hak akses mendaftarkan `BloodUnit : Resolve` lama | **Unit** | Tidak ada; butir itu dihapus pada `v4`, dan keberadaannya membatalkan pemisahan `DEC-BD-043` |

---

## Definition of Done (ringkas — lengkap di `04-prd-to-mvp.md`)

| Butir | Bukti |
| --- | --- |
| Satu kasus darah berjalan order → permintaan → penerimaan → periksa golongan → alokasi → bukti → pemberian | `AC-BD-005/019` + UAT jalur utama |
| Satu kantong tak mungkin diberikan ke dua pasien | `AC-BD-018c` konkurensi |
| Darah tak dapat diberikan tanpa bukti berlaku / golongan darah konflik tertahan | `AC-BD-018/038/041/034` |
| Pemberian tak dapat dihapus | `AC-BD-048` |
| Validasi rutin dan penyelesaian konflik dijaga dua butir hak akses berbeda | `AC-BD-077/078` |
| Otorisasi darurat menyimpan peran penerbit dan kondisi kedaruratan | `AC-BD-081/082/084/085` |
| Koreksi tak berlaku sebelum disetujui, dan tak dapat disetujui sendiri | `AC-BD-086/087/088` |
| Bukti kecocokan menyimpan hasil, dan hasil tidak cocok tak membuka gerbang | `AC-BD-089` + skenario `VAL-BD-079` |
| Tiga jalur penyelesaian dijaga tiga butir hak akses berbeda | `AC-BD-092/093/094` |
| Tidak ada pembatalan order tanpa alasan terkendali dan jejak | `AC-BD-095/096/097` |
| Kantong tak dapat dialokasikan sebelum disimpan | `AC-BD-060/061` |
| Kantong di lokasi nonaktif tak dapat dialokasikan maupun diberikan lewat jalur normal | `AC-BD-068/072`; jalur darurat `AC-BD-074/075` |
| Riwayat penempatan tak pernah ditimpa, dan sistem tak pernah memindahkan kantong sendiri | `AC-BD-063/069` |
| Seluruh master MVP terisi — **termasuk minimal satu lokasi penyimpanan aktif** | Rencana data master awal `02-backend-architecture.md` §J. Tanpa ini modul berhenti total |
| Billing charge & label **tidak** diuji pada MVP | `DEC-BD-016`, `OQ-BD-011` — di luar cakupan |
