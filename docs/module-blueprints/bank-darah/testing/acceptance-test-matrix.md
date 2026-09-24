# Bank Darah — Acceptance Test Matrix

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version **`v5` — `approved`** (`Sukmagp` 2026-09-19; `v4` kini `superseded`). **Riwayat:** `v5` `draft` 18 September 2026 (arah disetujui `Sukmagp` 2026-09-18). **Riwayat:** `v4` — `approved` |
| `last_changed_in` | **`v5`** — bagian 11 (`AC-BD-103` sampai `AC-BD-112`). **23 September 2026:** bagian 12 baru (`AC-BD-113` sampai `AC-BD-117`, `v5` `D5`, task `BE-BD-019`). **24 September 2026:** bagian 13 baru (`AC-BD-118` sampai `AC-BD-124`, `v5` `D6`, task `BE-BD-020`). Bagian 1–10 tidak berubah |
| `approved_by` / `approved_at` | `Sukmagp` / `2026-09-03` (`v4`) · **`Sukmagp` / `2026-09-19` (`v5`)** |
| Sumber | `00-interview-decisions.md` revisi 9 (`AC-BD-001`..`097`) · `contracts/state-transition-matrix.md` · `contracts/validation-matrix.md` |

Wajib memuat **jalur gagal**, bukan hanya jalur berhasil. Jenis test: `Unit` (aturan service), `Integ`
(service + DB), `Concurrency` (perebutan data), `E2E/UAT` (jalur pengguna). Data samaran.

> **Cara membaca kolom "Jenis" sejak 12 September 2026 (roadmap revisi 11).** Kolom itu menyatakan
> **sifat skenario** — apakah ia menguji aturan murni, aturan bersama database, perebutan data, atau
> jalur pengguna utuh. Ia **bukan** perintah membuat project test otomatis. Repository backend saat ini
> tidak memuat project test sama sekali: `QuilvianSystemBackend.sln` berisi satu project produksi, dan
> folder `Tests/` di root **dilarang dibuat**. Bukti untuk baris-baris di bawah karena itu berbentuk
> bukti build produksi, inspeksi EF/skema, QBE, serta verifikasi manual API dan DB terkendali —
> selengkapnya di [backend-roadmap.md](../roadmap/backend-roadmap.md) bagian 0.1. **Angka test pada
> laporan task yang sudah ✅ SELESAI tetap berlaku sebagai catatan sejarah dan tidak ditulis ulang.**

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
| `AC-BD-023` | Kantong datang setelah `ClosedEncounter` | Integ | Penerimaan dicatat, kantong → `PendingReview`. **Milik `BE-BD-015` sejak roadmap revisi 9 (11 September 2026)**; semula `BE-BD-004`, yang sudah membuktikan bagian penerimaannya |
| `AC-BD-031` | Minta 2 PRC, datang 3 | Integ | `Fulfilled` sisa 0 (bukan −1); 3 kantong tercatat |
| `AC-BD-032` | Kantong ke-3 pada `AC-BD-031` | Integ | `PendingReview` + alasan "kiriman melebihi permintaan", muncul di daftar #2. **Milik `BE-BD-015` sejak roadmap revisi 9**; semula `BE-BD-004`, yang sudah membuktikan penanda berlebih dan alasannya |
| `AC-BD-033` | Kantong berlebih dialokasikan langsung ke order pasien sama | Integ | Ditolak (`VAL-BD-033`). **Milik `BE-BD-006` sejak roadmap revisi 9**; semula `BE-BD-004` |

## 3. Alokasi, bukti, pemberian, koreksi

| Requirement | Skenario | Jenis | Bukti |
| --- | --- | --- | --- |
| `AC-BD-018` | Kantong dialokasikan, bukti belum tercatat, tekan pemberian | Integ | Ditolak (`VAL-BD-018`) |
| `AC-BD-019` | Bukti lengkap lalu pemberian | Integ | Berhasil; kantong `Issued` |
| `AC-BD-020` | Jalur darurat oleh peran berwenang + alasan | Integ | Berhasil, ditandai tanpa bukti, muncul di daftar #3 |
| `AC-BD-021` | Jalur darurat oleh peran tak berwenang | Integ | **Ditolak** `VAL-BD-072` (`DEC-BD-050`; sebelumnya tertulis `VAL-BD-021`) |
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
| `AC-BD-026` | Satu tindakan selesai dengan 2 kantong diberikan | Integ | Satu fakta biaya (bila kontrak Billing turun), bukan dua — **ditandai tertunda `DEC-BD-016`**. **Milik `BE-BD-013` sejak roadmap revisi 8 (11 September 2026)**; semula `BE-BD-012` **Terbukti runtime 17 September 2026 — `BE-BD-013`** ([laporan](../task/report/backend/BE-BD-013.md)): satu tindakan dengan 2 kantong `Issued` → tepat 1 fakta `BloodBank`/`BloodBankCharge` dan 1 charge line. Penanda tertunda `DEC-BD-016` di atas adalah riwayat. |
| `AC-BD-027` | Fakta biaya tindakan sama dikirim ulang | Integ | **Tertunda `DEC-BD-016`** — tidak diuji sampai kontrak Billing disetujui **Terbukti runtime 17 September 2026 — `BE-BD-013`**: kiriman ulang → `Replayed`, fakta, charge line, dan processing effect tetap 1. Penanda tertunda di atas adalah riwayat. |
| `AC-BD-058` | Koreksi pencatatan pemberian dibuat; sistem mencoba otomatis membalik fakta biaya tindakan | Integ | Ditolak — koreksi tidak mengubah biaya (`DEC-BD-034`, `INV-BD-024`). **Tertunda `DEC-BD-016`**. **Milik `BE-BD-013` sejak roadmap revisi 8**; semula `BE-BD-012`. Baris ini baru ditambahkan 11 September 2026 — sebelumnya kriteria ini tidak punya baris uji **Terbukti runtime 17 September 2026 — `BE-BD-013`**: koreksi `Approved` oleh aktor berbeda → nol fakta pembatalan, charge line dan folio identik sebelum/sesudah. Penanda tertunda di atas adalah riwayat. |

---

## 7. Penyimpanan kantong dan gerbang lokasi — baru pada `v2`

Menutup `AC-BD-059` sampai `AC-BD-076`. Jalur gagal ditulis lebih dulu karena di sinilah keselamatan
pasien benar-benar dijaga.

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-BD-059` | Kantong baru diterima dari PMI | Integ | Status `Received`; belum dapat dialokasikan |
| `AC-BD-060` | Kantong `Received` dicoba dialokasikan | Integ | **Ditolak** `VAL-BD-063`; pesan menyebut kantong belum disimpan. **Verifikasi final lewat endpoint alokasi milik `BE-BD-006` sejak roadmap revisi 10 (11 September 2026)**; `BE-BD-015` sudah membuktikan bagian gerbangnya — kantong `Received` ditolak gerbang alokasi `VAL-BD-063` |
| `AC-BD-061` | Petugas menetapkan lokasi pada kantong `Received` | Integ | Kantong `Stored` lalu `Available`; satu baris riwayat penempatan bertambah |
| `AC-BD-062` | Lokasi nonaktif dipilih untuk penyimpanan baru | Integ | **Ditolak** `VAL-BD-060` |
| `AC-BD-063` | Kantong `Stored` dipindahkan ke lokasi lain | Integ | Riwayat bertambah; **status tidak berubah**; catatan penerimaan awal tidak tersentuh |
| `AC-BD-064` | Sistem diminta mencatat suhu atau kapasitas storage | Unit | Tidak ada kolom maupun endpoint — di luar scope MVP |
| `AC-BD-065` | Lokasi nonaktif dipilih untuk penyimpanan kantong baru | Integ | **Ditolak** `VAL-BD-060` |
| `AC-BD-066` | Lokasi nonaktif dipilih sebagai **tujuan perpindahan** | Integ | **Ditolak** `VAL-BD-060` — aturan berlaku untuk perpindahan, bukan hanya penempatan pertama |
| `AC-BD-067` | Lokasi dinonaktifkan sementara masih ada kantong di dalamnya | Integ | Penonaktifan **berhasil**; kantong tetap tercatat di sana; status kantong tidak berubah; peringatan `VAL-BD-068` menyebut jumlah |
| `AC-BD-068` | Kantong di lokasi nonaktif dicoba dialokasikan | Integ | **Ditolak** `VAL-BD-064`. **Verifikasi final lewat endpoint alokasi milik `BE-BD-006` sejak roadmap revisi 10**; `BE-BD-015` sudah membuktikan bagian gerbangnya — lokasi current nonaktif menutup gerbang alokasi `VAL-BD-064` |
| `AC-BD-069` | Sistem diminta memindahkan sendiri kantong saat lokasi dinonaktifkan | Integ | **Tidak terjadi** — nol baris riwayat penempatan baru, nol perubahan status, nol job berjalan |
| `AC-BD-070` | Petugas memindahkan kantong dari lokasi nonaktif ke lokasi aktif lalu mengalokasikan | Integ | Berhasil; riwayat mencatat pelaku dan waktu; gerbang terbuka kembali. **Langkah "lalu mengalokasikan → berhasil" diverifikasi final oleh `BE-BD-006` sejak roadmap revisi 10**; `BE-BD-015` sudah membuktikan perpindahan, riwayat beserta pelaku dan waktu, dan gerbang yang terbuka kembali |
| `AC-BD-071` | Kantong `PendingReview` di lokasi nonaktif dicoba dialihkan ke pasien lain **lewat `POST /blood-units/{id}/reallocate`** | Integ | **Ditolak** `VAL-BD-064` — pengalihan adalah alokasi dengan nama lain. **Milik `BE-BD-009` sejak roadmap revisi 11 (12 September 2026)**; semula `BE-BD-006`, yang tidak melahirkan endpoint `reallocate` dan karena itu tidak dapat membuktikannya. `BE-BD-006` tetap membuktikan penolakan `VAL-BD-064` lewat endpoint `allocate` pada `AC-BD-068` |
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

## 10. Pencatatan tindakan Bank Darah — tambahan 11 September 2026

Menutup `AC-BD-098` sampai `AC-BD-102`, milik `BE-BD-012`. Kriteria ini lahir dari keputusan pemilik
`DEC-BD-048` dan `DEC-BD-049` (`00-interview-decisions.md` §8.28), dan **tidak** mengubah set kontrak `v4`:
seluruh kolom yang diuji sudah ada di kamus data sejak `v1`.

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-BD-098` | Order sah, tindakan sah, petugas berwenang mencatat tindakan | Integ | Tersimpan dengan `ProcedureNumber` unik dari number-series, `BloodOrderId`, `ProcedureRefId`, dokter BDRS, petugas dari akun yang login, unit dan kelas dari kunjungan order, `TariffId`, status `Recorded` |
| `AC-BD-098` — jalur gagal | Order tidak ada, dihapus, atau tidak sah | Integ | **Ditolak** `VAL-BD-026` |
| `AC-BD-099` | Tindakan punya tarif umum dan tarif kelas VIP; kunjungan pasien berkelas VIP | Integ | Tarif VIP terpilih; salinan nominalnya sama dengan tarif VIP |
| `AC-BD-099` — cadangan | Tarif kelas pasien tidak ada, tarif umum ada | Integ | Tarif umum terpilih |
| `AC-BD-099` — jalur gagal | Tidak ada tarif aktif dan berlaku yang cocok; atau client mengirim nominal harga | Integ | **Ditolak** `422`; nominal dari client tidak pernah dipakai (`VAL-BD-027`) |
| `AC-BD-100` | Tindakan tercatat, lalu kode, nama tindakan, dan nominal tarif di data induk diubah | Integ | Ketiga kolom salinan pada tindakan lama **tidak berubah** |
| `AC-BD-101` | Tindakan `Recorded` diselesaikan | Integ | `Completed`; transisi, pelaku, dan waktu tersimpan |
| `AC-BD-101` — jalur gagal | Tindakan yang sudah `Completed` diselesaikan lagi | Integ | **Ditolak** secara terkendali; status dan audit tidak bergerak |
| `AC-BD-102` | Tindakan dibuat dan diselesaikan | Unit + Integ | Nol baris pada tabel Billing; nol pemanggilan service Billing; tidak ada endpoint maupun producer penyaluran biaya. — **HISTORICAL / SUPERSEDED oleh `DEC-BD-016` + `BE-BD-013`** (keputusan pemilik 18 September 2026): hasil ini sah saat `BE-BD-012` ditutup 11 September 2026; **tidak lagi diuji sebagai syarat saat ini**. Harapan yang berlaku untuk tindakan selesai adalah `AC-BD-026` — tepat satu fakta biaya |
| Konkurensi nomor | Beberapa tindakan dicatat serentak | Concurrency | Tidak ada `ProcedureNumber` ganda; dijaga provider number-series dan index unik |

---

## 11. Kontrak Order Darah `v5` untuk `FE-BD-002` — tambahan 18 September 2026

Menutup `AC-BD-103` sampai `AC-BD-112`, lahir dari `DEC-BD-055` sampai `DEC-BD-058`
(`00-interview-decisions.md` §8.32). Kolom "Jenis" dibaca seperti catatan di kepala dokumen: sifat
skenario, bukan perintah membuat project test.

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-BD-103` | Order elektronik, order manual, dan lanjutan order ganda dibuat dengan golongan darah diminta A Positif | Integ | Tersimpan pada `BbkBloodOrder.RequestedBloodGroup`; `GET /{id}` memulangkan nilai dan labelnya persis; nilai **tidak** tersalin ke baris order |
| `AC-BD-103` — "Tidak diketahui" | Order dibuat dengan golongan darah diminta `Unknown` | Integ | **Diterima**; tersimpan `Unknown`, bukan `NULL` |
| `AC-BD-104` — jalur gagal | Golongan darah diminta tidak dikirim, bernilai `NotDisclosed`, atau angka di luar enum — pada ketiga endpoint pembuatan | Integ | **Ditolak** `400 VAL-BD-085`; nol order tersimpan; nomor order **tidak** terbit |
| `AC-BD-105` | Order yang dibuat sebelum `v5` dibuka sesudah migration | Integ | `requestedBloodGroup = null`; **tidak** diisi dari `MstPatient.BloodType` maupun hasil pemeriksaan; migration tanpa backfill |
| `AC-BD-106` — batas klinis | Order meminta A Positif; pemeriksaan tervalidasi pasien B Positif | Integ | `GET /blood-group-exams/patient/{id}/valid` tetap B Positif; alokasi, bukti kecocokan, dan pemberian dinilai terhadap hasil pemeriksaan saja; nol pembacaan `RequestedBloodGroup` di luar layanan order (`INV-BD-011`) |
| `AC-BD-107` | Pasien sudah punya order PRC aktif; dibuat order PRC + trombosit pada kunjungan yang sama | Integ | **Tertahan** `422`; `errors.code = "VAL-BD-001"`; `errors.duplicateComponentIds` berisi **hanya** ID PRC; kalimat pesan sama persis dengan `validation-matrix.md` |
| `AC-BD-107` — tanpa perubahan lain | Order ditolak karena alasan selain order ganda | Integ | Bentuk jawabannya tetap seperti `v4` (`errors = null`) |
| `AC-BD-108` | Order `Active` dibuka oleh pengguna yang tertaut ke dokter peminta order itu, lalu oleh pemegang `BloodOrder : Cancel` lain | Integ | `cancellationReasonCategory` berturut-turut `OrderCancellationClinical` dan `OrderCancellationOperational`; pembatalan memakai alasan kategori itu berhasil |
| `AC-BD-108` — jalur gagal | Order terminal dibuka; lalu pembatalan dikirim dengan alasan kategori lawannya | Integ | Order terminal → `null`. Kategori lawan → **ditolak** `422 VAL-BD-083` — backend tetap penentu |
| `AC-BD-109` | Tiga order: hanya PRC; PRC + trombosit; hanya trombosit. Daftar disaring `bloodComponentId` = PRC | Integ | Dua order pertama muncul, yang ketiga tidak; paging dan penyaring lain tetap berlaku bersamaan |
| `AC-BD-110` | Order PRC 2 kantong, satu kantong `Issued`, lalu satu koreksi pemberian `Approved` | Integ | `components[].issuedQuantity` dan `totalIssuedQuantity` pada daftar sama persis dengan `GET /{id}/fulfillment`, sebelum dan sesudah koreksi (`DEC-BD-054`) |
| `AC-BD-111` | Satu halaman daftar berukuran maksimum yang berlaku berisi order multi-baris | Integ | Proyeksi komponen dan jumlah diberikan dihitung dengan kueri berkelompok untuk seluruh halaman — jumlah kueri **tidak** tumbuh mengikuti jumlah baris; nol kolom penghitung tersimpan |
| `AC-BD-112` — hak akses | Kebijakan akses pengembangan yang memberi `BloodOrder : Cancel` | Manual | Setiap kebijakan itu juga memberi `BloodBankReason : Read`; kebijakan yang tidak memenuhinya dicatat sebagai temuan, **tanpa** membuat butir hak akses baru (`DEC-BD-057`) |

---

## 12. Penyaring rentang tanggal daftar kerja order darah — tambahan 23 September 2026

Menutup `AC-BD-113` sampai `AC-BD-117`, lahir dari `DEC-BD-059` dan `DEC-BD-060`
(`00-interview-decisions.md` §8.34), dikerjakan task `BE-BD-019`.

> **Status: kelimanya ✅ TERBUKTI RUNTIME, 23 September 2026.** Dijalankan langsung agent terhadap
> `QuilvianNewDevSukma` dengan keluaran HTTP sungguhan; rinciannya pada
> [`BE-BD-019`](../task/report/backend/BE-BD-019.md) bagian 7. `AC-BD-115` terbukti sampai ke detik
> batasnya — rentang efektif `>= 2026-09-22T17:00:00Z` dan `< 2026-09-23T17:00:00Z`. Database
> dikembalikan ke keadaan semula sesudah pengujian dan diverifikasi.

**Data yang wajib disiapkan lebih dulu, dan paling mudah terlewat:** sekurang-kurangnya satu order yang
`CreateDateTime`-nya jatuh antara `00:00` dan `07:00` **WIB**. Tanpa order seperti itu, `AC-BD-115` tidak
dapat dibuktikan dan cacat zona waktu lolos tanpa terlihat — seluruh pengujian siang hari akan lulus
walaupun konversinya salah.

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-BD-113` — `startDate` | Tiga order dibuat pada 21, 22, dan 23 September waktu aplikasi. Daftar disaring `startDate = 2026-09-22`, tanpa `endDate` | Integ | Order 22 dan 23 muncul; order 21 **tidak**. Tanpa parameter apa pun, ketiganya muncul |
| `AC-BD-114` — `endDate` inklusif | Daftar disaring `endDate = 2026-09-22`, tanpa `startDate`. Salah satu order dibuat pukul `23:59` waktu aplikasi tanggal 22 | Integ | Order 21 dan 22 muncul — **termasuk yang pukul `23:59`**; order 23 **tidak**. Membuktikan batas atas eksklusif pada hari berikutnya, bukan `<= 23:59:59` |
| `AC-BD-115` — batas hari WIB | Order dibuat pukul `02:00` WIB tanggal 23 September, tersimpan `2026-09-22T19:00:00Z`. Disaring `startDate = endDate = 2026-09-23`, lalu disaring `startDate = endDate = 2026-09-22` | Integ | **Muncul** pada saringan 23 September; **tidak muncul** pada saringan 22 September. Rentang efektifnya `>= 2026-09-22T17:00:00Z` dan `< 2026-09-23T17:00:00Z` (`DEC-BD-060`) |
| `AC-BD-116` — kombinasi | Rentang tanggal digabung `bloodComponentId`, `orderStatus`, dan `search`, pada halaman kedua dengan `pageSize` kecil | Integ | Hasilnya irisan seluruh penyaring; paging berlaku atas hasil yang **sudah** tersaring — `totalData` dan `totalPage` menghitung hasil akhir, bukan seluruh order |
| `AC-BD-117` — rentang tidak sah | `startDate = 2026-09-30`, `endDate = 2026-09-23` | Integ | **Ditolak** `400 VAL-BD-086` dengan kalimat persis `validation-matrix.md`; **bukan** daftar kosong. `startDate` sama dengan `endDate` tetap **diterima** dan menyaring satu hari penuh |

---

## 13. Penyaring kantong tertahan di lokasi nonaktif — tambahan 24 September 2026

Menutup `AC-BD-118` sampai `AC-BD-124`, lahir dari keputusan pemilik `Sukmagp` 24 September 2026
(`D2`–`D4`, api-contract `v5` `D6`), dikerjakan task `BE-BD-020`. Menjadi prasyarat backend saringan
`inactiveLocation` pada `FE-BD-012`.

> **Status: ketujuhnya ✅ TERPENUHI, 24 September 2026.** Dijalankan langsung agent terhadap
> `QuilvianNewDevSukma` dengan keluaran HTTP sungguhan; rinciannya pada
> [`BE-BD-020`](../task/report/backend/BE-BD-020.md) bagian 5.2 (R1–R14 `PASS`). `AC-BD-121` terbukti
> dengan data alami (dua kantong `Diberikan` di `TBD008-LOCB`) dan terkendali (kulkas `TBD006-LOC2`
> dinonaktifkan: 6 kantong di stok muncul = angka `VAL-BD-068`, kantong `Diberikan` tidak). **Batas:**
> `AC-BD-124` terbukti pada tingkat atribut hak akses — aktor tunggal `superadmin`, jalur `403` belum
> ditembakkan dengan aktor tanpa hak baca. Database dipulihkan sesudah pengujian.

**Data yang wajib disiapkan lebih dulu:** satu lokasi yang **dinonaktifkan** dan berisi sekurang-kurangnya
satu kantong di stok, satu kantong di lokasi aktif, satu kantong `Received` tanpa lokasi, dan satu kantong
berstatus akhir yang lokasi terakhirnya nonaktif. Tanpa kantong berstatus akhir itu, `AC-BD-121` tidak
dapat dibuktikan dan kebocoran status akhir lolos tanpa terlihat.

| Requirement | Skenario | Jenis | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-BD-118` — lokasi nonaktif muncul | Kantong `Available` tersimpan di lokasi A; lokasi A dinonaktifkan. Daftar disaring `inactiveLocation=true` | Integ | Kantong itu **muncul**, `isCurrentStorageLocationActive = false`, statusnya tetap `Available` |
| `AC-BD-119` — lokasi aktif tidak muncul | Kantong tersimpan di lokasi B yang aktif | Integ | **Tidak muncul** pada `inactiveLocation=true`; **muncul** pada `inactiveLocation=false` |
| `AC-BD-120` — tanpa lokasi tidak dianggap nonaktif | Kantong `Received` yang belum pernah ditempatkan | Integ | **Tidak muncul** pada `inactiveLocation=true`; **muncul** pada `inactiveLocation=false` |
| `AC-BD-121` — status akhir tidak muncul (`D2`) | Kantong `Issued`, `ReturnedToProvider`, atau `NotUsable` yang penempatan terakhirnya menunjuk lokasi nonaktif | Integ | **Tidak muncul** pada `inactiveLocation=true`. Himpunan yang muncul sama dengan hitungan peringatan `VAL-BD-068` untuk lokasi itu |
| `AC-BD-122` — lokasi terhapus dianggap nonaktif (`D3`) | Lokasi berisi kantong di stok ditandai terhapus (`IsDelete = true`) walaupun `IsActive` masih `true` | Integ | Kantongnya **muncul** pada `inactiveLocation=true` — sejalan dengan gerbang `VAL-BD-064` yang juga menolak alokasinya |
| `AC-BD-123` — kebalikan dan paging (`D4`) | `inactiveLocation=true` dan `false` masing-masing, lalu `true` digabung `unitStatus`/`search` dengan `pageSize` kecil pada halaman kedua | Integ | `totalData(true) + totalData(false) = totalData(tanpa penyaring)`, tanpa irisan. Paging berlaku atas hasil yang **sudah** tersaring — `totalData` dan `totalPage` menghitung hasil akhir |
| `AC-BD-124` — hak akses tetap | Pemanggil tanpa `BloodUnit : Read` | Integ | Ditolak `403` seperti sebelum `D6`. Nol butir `[AccessAction]` baru; `[AccessPermission("BloodUnit", "Read")]` tidak berubah |

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
| Tindakan tercatat dengan tarif dari data induk, salinannya tidak berubah, dan tanpa jalur Billing | `AC-BD-098/099/100/101/102`. **Catatan 18 September 2026:** bagian "tanpa jalur Billing" (`AC-BD-102`) **HISTORICAL / SUPERSEDED oleh `DEC-BD-016` + `BE-BD-013`**; tindakan selesai kini menyerahkan satu fakta biaya (`AC-BD-026/027/058`) |
| Label **tidak** diuji pada MVP. Billing charge **diuji sejak 17 September 2026** lewat `BE-BD-013` (`AC-BD-026/027/058`) | `OQ-BD-011` — di luar cakupan. **Riwayat:** `DEC-BD-016` — Billing charge juga di luar cakupan |
