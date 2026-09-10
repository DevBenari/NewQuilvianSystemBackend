# Bank Darah — PRD ke MVP

## 1. Identitas dokumen

| Field | Value |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Bank Darah (`bank-darah`) · kode PRD `BD` |
| Blueprint ID | `BD-BP-001` · Contract version `v4` — status **`approved`** |
| Repository target | `NewQuilvianSystemBackend` (backend) · `V2QuilvianSystemFrontendDev` (frontend) |
| Commit SHA baseline | backend `ab39b63` · frontend `afbb8ab` |
| Arsitektur domain | `03-domain-architecture.md` revisi 6 — `DOMAIN_ARCHITECTURE_READY` · register keputusan revisi 9 |
| Ringkasan cakupan | MVP mencatat pemenuhan darah pasien dari order sampai kantong diberikan/diselesaikan, **tanpa** charge Billing, label cetak, dan integrasi luar |
| `approved_by` / `approved_at` | `Sukmagp` / `2026-09-03` |

## 2. Ringkasan eksekutif

Bank Darah Rumah Sakit (BDRS) MMC memenuhi kebutuhan darah pasien dengan meminta darah ke PMI,
menerima kantong, memeriksa golongan darah, mengalokasikan, lalu memberikannya. Hari ini seluruh
proses itu tidak punya sistem: tidak ada satu pun entity, layar, atau catatan Bank Darah di Quilvian.
MVP ini memberi BDRS satu alur tertelusur dan aman — siapa meminta, kapan darah datang, siapa
memeriksa golongan darah, siapa menyatakan kecocokan, siapa memberikan — sehingga tidak ada kantong
yang berpindah tangan tanpa jejak, dan tidak ada darah yang diberikan tanpa bukti kecocokan yang sah.

## 3. Masalah produk

| Kondisi sekarang | Bukti |
| --- | --- |
| Tidak ada modul Bank Darah sama sekali | `BD-CAP-019` `Missing` — nihil entity/controller/service Bank Darah |
| Tidak ada layar Bank Darah | `BD-CAP-020` `Missing` |
| Tidak ada katalog komponen darah | `BD-CAP-018` `Missing` |
| Tidak ada sumber sah golongan darah tervalidasi; `MstPatient.BloodType` hanya administratif | `BD-CAP-017` `Missing`, `INV-BD-014` |
| Pola yang **sudah ada** dan dipakai ulang | pasien/kunjungan/dokter/unit (`BD-CAP-001/002/004/006`), pola order klinis & riwayat & konkurensi (`BD-CAP-007/009/010`), hak akses & response (`BD-CAP-012/013`), komponen frontend (`BD-CAP-021`) |

## 4. Visi produk (rantai keterhubungan data)

1. Kebutuhan klinis pasien pada satu kunjungan melahirkan **order darah**.
2. Order melahirkan **permintaan ke PMI** atas nama pasien itu.
3. Permintaan menerima **kantong fisik**; stok bertambah di titik ini.
4. Pasien punya **golongan darah sah** dari pemeriksaan Bank Darah.
5. Kantong **dialokasikan** ke baris kebutuhan order.
6. **Bukti kecocokan** terhadap pasien tujuan membuka gerbang.
7. Kantong **diberikan**; angka pemenuhan order diperbarui.
8. Setiap perpindahan meninggalkan **riwayat yang hanya bisa ditambah**.

## 5. Batas MVP

**Titik mulai:**
1. Pasien terdaftar dengan kunjungan aktif (milik modul lain).
2. Unit pelayanan dikonfigurasi berwenang memesan darah.
3. Katalog komponen darah dan daftar alasan terkendali sudah terisi.

**Titik akhir:**
1. Kantong berstatus `Issued` untuk pasien, atau diselesaikan sebagai `Reallocated`/`ReturnedToProvider`/`NotUsable`.
2. Order tercatat `FullyFulfilled`, `Cancelled`, atau `Expired`.
3. Seluruh riwayat pergerakan tersimpan dan dapat ditelusuri.

Di luar titik akhir MVP: penyaluran biaya ke Billing, pencetakan label, dan pertukaran data otomatis
dengan PMI/HCLAB.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab dalam MVP |
| --- | --- |
| Dokter / unit pelayanan | Membuat order darah, bertanggung jawab atas indikasi klinis |
| Petugas Bank Darah / BDRS | Memproses order, meminta ke PMI, menerima, memeriksa golongan darah, mengalokasikan, memberikan |
| Petugas BDRS berwenang validasi | Validasi hasil golongan darah rutin |
| Dokter BDRS / penanggung jawab klinis | Penyelesaian konflik golongan darah, jalur darurat, menyetujui/menolak koreksi pemberian |
| DPJP pasien | Jalur darurat untuk pasiennya |
| Admin master Bank Darah | Mengisi katalog komponen & daftar alasan |

## 7. Pemilihan kemampuan MVP (`MUST HAVE`)

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Order darah (elektronik + manual) + deteksi ganda | `BD-CAP-019`, `BD-CAP-002` | Wajib; tanpa ini tak ada pemicu proses |
| Kewenangan unit memesan darah | `BD-CAP-005` | Wajib; menentukan siapa boleh memesan |
| Permintaan PMI + penerimaan (partial/berlebih/tutup) | `BD-CAP-019`, `BD-CAP-007` | Wajib; tanpa ini stok tak pernah masuk |
| Katalog komponen darah | `BD-CAP-018` | Wajib; komponen tak boleh teks bebas |
| **Lokasi penyimpanan darah + penyimpanan kantong** | `BD-CAP-019` | **Wajib.** Tanpa satu pun lokasi aktif, tidak ada kantong yang dapat dialokasikan sama sekali (`DEC-BD-035`, `DEC-BD-036`) |
| **Perpindahan lokasi kantong** | `BD-CAP-019` | Wajib; satu-satunya jalan keluar kantong dari kulkas yang dinonaktifkan (`DEC-BD-037`) |
| Pemeriksaan & validasi golongan darah + konflik | `BD-CAP-017` | Wajib; sumber sah untuk gerbang klinis |
| Kantong: alokasi + pembatalan | `BD-CAP-019`, `BD-CAP-010` | Wajib; tanpa ini kantong tak sampai pasien |
| Bukti kecocokan + pemberian + jalur darurat + gerbang masa berlaku **+ gerbang lokasi aktif** | `BD-CAP-019` | Wajib; titik keselamatan inti (`DEC-BD-038`) |
| Penyelesaian kantong menunggu keputusan | `BD-CAP-019` | Wajib; kantong tak boleh jadi stok bebas |
| Koreksi pencatatan pemberian | `BD-CAP-009` | Wajib; satu-satunya jalur perbaikan tanpa hapus |
| Daftar alasan terkendali | `BD-CAP-019` | Wajib; alasan tak boleh teks bebas |
| Tiga daftar kerja operasional | `BD-CAP-019` | Wajib; alat menjalankan proses |
| Pencatatan tindakan Bank Darah (tanpa charge) | `BD-CAP-019` | Wajib bagian pencatatannya |
| Riwayat pergerakan append-only | `BD-CAP-009` | Wajib; keterlacakan |

## 8. Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Penyaluran fakta biaya ke Billing | `BD-CAP-015` | Konteks sumber Bank Darah pada `BillingSourceContract` belum disetujui pemilik Billing (`DEC-BD-016`) | Tindakan tetap **dicatat**; penagihan pengganti biaya (bila ada) berjalan di luar Quilvian (`ASM-BD-007`) |
| Cetak label golongan darah | `BD-CAP-017` | Isi/identifier/perilaku cetak ulang belum ditetapkan (`OQ-BD-011`) | Hasil golongan darah tetap tercatat & tervalidasi di layar, tanpa label cetak |
| Integrasi API PMI | `BD-CAP-023` | Tak dibutuhkan MVP; pengiriman manual (`DEC-BD-002`) | Permintaan dicatat, diteruskan manual di luar sistem |
| Integrasi HCLAB | `BD-CAP-024` | Tak ada kontrak/protokol/pemetaan (`DEC-BD-022`) | Tidak ada; hanya temuan penelusuran |
| Mesin crossmatch / kesesuaian | — | Quilvian tak menghitung kompatibilitas (`INV-BD-013`) | Bukti kecocokan dinyatakan manusia, sistem mencatat |
| Manajemen donor & turunannya | — | Di luar scope (BRD §9) | Tidak ada |

## 9. Alur bisnis target

`FLOW-BD-MVP-001` — pemenuhan darah satu pasien:

1. Unit pelayanan membuat order darah; sistem memeriksa order ganda.
2. Petugas Bank Darah membuat permintaan ke PMI atas nama pasien, meneruskan manual.
3. Kantong datang; petugas mencatat penerimaan; stok bertambah.
4. Petugas mengambil sampel & memeriksa golongan darah bila belum ada hasil sah; validator memvalidasi.
5. Petugas mengalokasikan kantong ke baris kebutuhan order.
6. Petugas berwenang mencatat bukti kecocokan terhadap pasien tujuan.
7. Petugas memberikan kantong; angka pemenuhan order diperbarui.
8. Bila kantong tak jadi dipakai, diselesaikan lewat alih/kembali/tidak layak.

Jalur tidak normal lengkap: `flowcharts/`.

## 10. Epic dan functional requirement

| Epic | Tujuan | Disposisi |
| --- | --- | --- |
| `EPIC BD-01` Order Darah | Buat/kelola order, deteksi ganda, kedaluwarsa | `MISSING / NEW` |
| `EPIC BD-02` Permintaan PMI | Buat permintaan, catat penerimaan termasuk kelebihan, tutup administratif | `MISSING / NEW` |
| `EPIC BD-03` Kantong: alokasi | Alokasi & pembatalan alokasi | `MISSING / NEW` |
| `EPIC BD-04` Bukti & pemberian | Bukti kecocokan, gerbang masa berlaku, pemberian, jalur darurat | `MISSING / NEW` |
| `EPIC BD-05` Penyelesaian kantong | Alih/kembali/tidak layak dari `PendingReview` | `MISSING / NEW` |
| `EPIC BD-06` Koreksi pemberian | Pengajuan koreksi oleh petugas, **persetujuan/penolakan oleh Dokter BDRS**, append-only | `MISSING / NEW` |
| `EPIC BD-07` Golongan darah | Sampel, hasil, **validasi rutin**, konflik, **penyelesaian oleh validator klinis** lewat pemeriksaan ulang | `MISSING / NEW` |
| `EPIC BD-08` Tindakan Bank Darah | Pencatatan tindakan (tanpa charge) | `MISSING / NEW` |
| `EPIC BD-09` Setup & kewenangan | Katalog komponen, daftar alasan, **master lokasi penyimpanan darah** (`NEW`); flag unit `IsAvailableForBloodOrder` (`EXTEND`) | `MISSING / NEW` + `EXTEND` |
| **`EPIC BD-11` Penyimpanan kantong** | Penetapan lokasi pertama (`Received`→`Stored`→`Available`), perpindahan lokasi, riwayat penempatan append-only, gerbang alokasi | `MISSING / NEW` |
| `EPIC BD-10` Daftar kerja | Tiga daftar operasional | `MISSING / NEW` |
| — Penyaluran biaya ke Billing | Kirim fakta biaya tindakan | **`OPEN DECISION`** (`DEC-BD-016`) — **tidak** masuk gelombang |

`EPIC BD-11` **bukan** epic tersendiri karena ada layar baru, melainkan karena ia memperkenalkan
entity, master, dan gerbang yang berdiri sendiri. Perluasan gerbang **pemberian** (`DEC-BD-038`)
sengaja **tidak** dijadikan epic terpisah: ia menambah satu syarat pada gerbang yang sudah menjadi
milik `EPIC BD-04`, dan memecahnya akan membuat dua task menyentuh satu fungsi yang sama.

FR bernomor yang dapat diuji (contoh kunci; lengkap dipetakan ke `AC-BD-*`):

> **`FR-BD-018` — Gerbang pemberian.** Sistem menolak pemberian kantong bila tidak ada bukti kecocokan
> untuk pasien tujuan yang belum lewat masa berlaku, kecuali lewat jalur darurat berwenang.
> **Contoh:** kantong `PMI-00871` dialokasikan untuk Tn. S, bukti diuji Senin 16.00, masa berlaku PRC
> 48 jam. Pemberian Rabu 10.00 berhasil; pemberian Kamis 09.00 ditolak "Bukti kecocokan sudah lewat
> masa berlaku" dan menuntut bukti baru. (`AC-BD-038/039`)

> **`FR-BD-031` — Kelebihan kiriman tak membuat sisa negatif.** Diminta 2, datang 3: permintaan
> `Fulfilled` sisa 0 (bukan −1); kantong ke-3 `PendingReview` alasan "kiriman melebihi permintaan".
> (`AC-BD-031/032`)

> **`FR-BD-051` — Penyelesaian konflik lewat pemeriksaan ulang.** Konflik golongan darah hanya ditutup
> setelah ada pemeriksaan ulang tervalidasi yang dinyatakan validator sebagai hasil berlaku; sistem tak
> menghitung mayoritas. (`AC-BD-036/051/053/054`)

> **`FR-BD-060` — Kantong wajib disimpan sebelum dapat dialokasikan.** Sistem menolak alokasi kantong
> yang belum punya lokasi penyimpanan tercatat. **Contoh:** kantong `PMI-00912` diterima Senin pagi dan
> langsung dicoba dialokasikan untuk Tn. S — ditolak dengan "Kantong belum disimpan pada lokasi
> penyimpanan". Petugas menaruhnya di "Kulkas Besar", mencatat lokasinya, lalu alokasi berhasil.
> (`AC-BD-059/060/061`)

> **`FR-BD-061` — Perpindahan lokasi tidak pernah mengubah status dan tidak pernah menimpa riwayat.**
> **Contoh:** kantong `PMI-00912` yang sedang dialokasikan untuk Tn. S dipindahkan dari "Kulkas Besar"
> ke "Kulkas Kecil". Statusnya tetap `Allocated`, alokasinya tetap milik Tn. S, buktinya tetap berlaku,
> dan riwayat menyimpan **dua** penempatan. Catatan penerimaan awal tidak tersentuh. (`AC-BD-063`)

> **`FR-BD-062` — Penonaktifan lokasi menutup gerbang tanpa memindahkan kantong.** Sistem tidak pernah
> memindahkan kantong sendiri dan tidak mengubah statusnya. **Contoh:** "Kulkas Lama" ditandai nonaktif
> saat berisi tiga kantong. Ketiganya tetap tercatat di sana dan tetap `Available`, tetapi tidak satu
> pun dapat dialokasikan sampai petugas memindahkannya. (`AC-BD-067/068/069/070`)

> **`FR-BD-063` — Gerbang pemberian memuat gerbang alokasi dan dinilai ulang.** Sistem menolak
> pemberian jalur normal bila lokasi penyimpanan kantong sedang nonaktif, walaupun alokasi dan bukti
> kecocokannya sah. **Contoh:** kantong dialokasikan Senin saat kulkas masih aktif; Selasa kulkas
> ditandai rusak; Selasa siang pemberian ditolak "Kantong berada di lokasi penyimpanan yang sudah tidak
> aktif". Setelah dipindahkan ke kulkas aktif, pemberian berhasil. (`AC-BD-072/073`)

> **`FR-BD-064` — Jalur darurat wajib menyebut gerbang yang dilewati.** Otorisasi darurat menyatakan
> apakah yang dilewati bukti kecocokan, lokasi nonaktif, atau keduanya. **Contoh:** Tn. S perdarahan
> hebat, kantong di kulkas rusak, buktinya ada dan berlaku. Penanda permanen menyebut **lokasi**, bukan
> bukti — sehingga pembaca rekam berikutnya tahu darahnya cocok dan yang dipertaruhkan penyimpanannya.
> (`AC-BD-074/075`)

> **`FR-BD-070` — Validasi rutin dan penyelesaian konflik dijaga wewenang berbeda.** **Contoh:**
> petugas BDRS berwenang validasi memvalidasi hasil A Positif Ny. R — berhasil, dan justru validasi itu
> yang memunculkan konflik dengan hasil O Positif sebelumnya. Petugas yang sama lalu mencoba menutup
> konflik — ditolak. Yang menutup adalah Dokter BDRS. (`AC-BD-077/078/079`)

> **`FR-BD-071` — Otorisasi darurat menyimpan dengan wewenang apa penerbit bertindak.** Sistem menolak
> otorisasi darurat yang tidak menyebutkan peran penerbit maupun kondisi kedaruratannya. **Contoh:**
> pukul 02.00 Dokter BDRS tidak di tempat; DPJP menerbitkan otorisasi sebagai DPJP, dan rekam menyimpan
> peran itu — sehingga saat ditinjau, jalur wewenang yang dipakai terbaca apa adanya.
> (`AC-BD-081/084/085`)

> **`FR-BD-072` — Koreksi belum berlaku sebelum disetujui.** Angka pemenuhan order tidak bergerak
> selama koreksi menunggu keputusan. **Contoh:** petugas mengajukan koreksi nomor kantong pada pemberian
> Tn. S. Sampai Dokter BDRS menyetujui, ringkasan pemenuhan Tn. S tetap seperti semula. Setelah
> disetujui, barulah dihitung ulang. Bila ditolak, angkanya tidak pernah berubah dan permintaannya tetap
> terbaca. (`AC-BD-086/087`)

> **`FR-BD-074` — Bukti kecocokan menyimpan hasil, dan hasil tidak cocok tidak membuka gerbang.**
> **Contoh:** uji cocok kantong `PMI-00871` terhadap Tn. S dinyatakan **tidak cocok** oleh petugas BDRS
> berwenang validasi. Buktinya tersimpan dan tetap terbaca, tetapi tombol Berikan tertutup, dan
> alasannya terbaca sebagai soal hasil pemeriksaan — bukan sebagai bukti yang belum tercatat.
> (`AC-BD-089`, `VAL-BD-079`)

> **`FR-BD-075` — Tiga jalur penyelesaian dijaga tiga wewenang berbeda.** **Contoh:** petugas dengan
> kewenangan operasional mengembalikan kantong ke PMI — berhasil. Petugas yang sama mencoba mengalihkan
> kantong ke Ny. R — ditolak, karena pengalihan memasukkan darah ke tubuh pasien baru dan menuntut
> kewenangan klinis BDRS. (`AC-BD-092/093/094`)

> **`FR-BD-076` — Tidak ada pembatalan order tanpa audit.** Dokter peminta membatalkan dengan alasan
> berkategori klinis; petugas BDRS membatalkan duplikat dengan alasan berkategori operasional. Keduanya
> menyimpan alasan, pelaku, waktu, dan jejaknya; keduanya ditolak bila alasannya kosong.
> (`AC-BD-095/096/097`)

> **`FR-BD-073` — Koreksi tidak dapat disetujui oleh pengajunya sendiri.** Berlaku walaupun orang itu
> memegang kedua butir hak akses. **Contoh:** Dokter BDRS menemukan sendiri kekeliruan pencatatan dan
> mengajukannya. Ia tidak dapat menyetujui permintaannya sendiri; rekan sejawatnya yang memutuskan.
> (`AC-BD-088`)

Seluruh FR lain diturunkan dari `AC-BD-001`..`088` (`testing/acceptance-test-matrix.md`) dan
`contracts/validation-matrix.md`.

## 11. Model status yang diusulkan

| Aggregate | Status | Invariant utama |
| --- | --- | --- |
| Order | `Active`→`PartiallyFulfilled`→`FullyFulfilled` / `Cancelled` / `Expired` | Pemenuhan dihitung dari transaksi; `Expired` tak reopen |
| Permintaan PMI | `Requested`→`PartiallyFulfilled`→`Fulfilled` / `Cancelled` / `ClosedEncounter` | Sisa ≥ 0 |
| Kantong | `Received`→`Stored`→`Available`→`Allocated`→`Issued`; `PendingReview`→`Reallocated`/`ReturnedToProvider`/`NotUsable` | 1 alokasi aktif; 1 penempatan berlaku; tak dapat dialokasikan sebelum `Stored` atau dari lokasi nonaktif; pemberian terminal |
| Golongan darah | `SampleTaken`→`ResultRecorded`→`Validated` (+ konflik ditahan) | Hasil sah tak ditimpa; konflik lewat pemeriksaan ulang |
| Tindakan | `Recorded`→`Completed` | 1 tindakan ≤ 1 fakta biaya (penyaluran tertunda) |

## 12. Sasaran arsitektur

- **Dipakai ulang:** pasien/kunjungan/dokter/unit, pola order & riwayat & konkurensi, hak akses,
  `ApiResponse`/`PagedResult`, komponen frontend dasar, enum `BloodType`.
- **Diperluas:** `MstServiceUnit` (+1 kolom kewenangan).
- **Baru:** 14 entity operasional `Bbk*` (prefix **sudah disahkan registry**, `BD-DEP-008` tertutup) + 2 master `Mst*`.
- Detail: `02-backend-architecture.md`, `data/data-dictionary.md`.

## 13. Sasaran kemampuan API

Endpoint identik dengan `contracts/api-contract.md` (semua `Rencana (belum tersedia)`), ditambah epic
asal. Contoh:

### Health Services / Blood Bank Management / Blood Unit

Base URL: `api/v1/health-services/blood-bank-management/blood-units`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/allocate` | Alokasikan kantong | `BloodUnit : Allocate` | `AllocateUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | `EPIC BD-03` | Rencana |
| `POST` | `/{id}/issue` | Berikan kantong | `BloodUnit : Issue` | `IssueUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | `EPIC BD-04` | Rencana |
| `POST` | `/{id}/corrections` | **Ajukan** koreksi pencatatan pemberian | `BloodUnit : Correct` | `RequestIssuanceCorrectionRequest` | `ApiResponse<IssuanceCorrectionDto>` | `EPIC BD-06` | Rencana |
| `POST` | `/{id}/corrections/{correctionId}/approve` | **Setujui** koreksi | `BloodUnit : ApproveCorrection` | `DecideCorrectionRequest` | `ApiResponse<IssuanceCorrectionDto>` | `EPIC BD-06` | Rencana |
| `POST` | `/{id}/corrections/{correctionId}/reject` | **Tolak** koreksi | `BloodUnit : ApproveCorrection` | `DecideCorrectionRequest` | `ApiResponse<IssuanceCorrectionDto>` | `EPIC BD-06` | Rencana |

Daftar lengkap tujuh grup ada di `contracts/api-contract.md`; PRD ini tidak melebihinya.

## 14. Matriks kewenangan

String permission persis mengikuti `contracts/permission-audit-matrix.md`. Ringkas: unit pelayanan
`BloodOrder : Create/Read`; petugas Bank Darah alur normal; peran `DEF-BD-004` untuk
`BloodUnit : EmergencyIssue/Correct` dan `BloodGroupExam : Validate`; admin master untuk
`BloodComponent`/`BloodBankReason : *`. **Seluruh peran sudah ditetapkan** `DEC-BD-039` sampai `DEC-BD-044`; `DEF-BD-004` tertutup penuh.
Satu nama peran yang sempat menyusul — pemegang `BloodUnit : ResolveNotUsable` — **sudah ditetapkan**
`DEC-BD-045`: kewenangan operasional BDRS, peran yang sama dengan `BloodUnit : ResolveReturn`, dengan
dua baris seeder terpisah karena butirnya memang tidak boleh digabung (`INV-BD-034`).

## 15. Batas integrasi dan billing

Modul ini **MUST NOT** membuat sendiri: perhitungan tarif/charge, klien PMI, klien HCLAB, mesin
crossmatch, salinan master pasien/dokter/unit. Fakta biaya **tidak** disalurkan ke Billing pada MVP
(`DEC-BD-016`). Detail: `contracts/integration-contract.md`.

## 16. Guardrail regulasi

- Rekam jejak klinis **append-only**; penghapusan hanya penandaan (`IsDelete`), bukan hapus keras.
- Data pasien & golongan darah sensitif; **MUST NOT** masuk log; nomor kantong PMI diperlakukan sensitif.
- Golongan darah untuk keputusan klinis hanya dari hasil pemeriksaan tervalidasi (`INV-BD-014`).
- Pemberian darah tak dapat dihapus/dibalik — mencegah rekam menyatakan "belum diberikan" atas darah
  yang sudah masuk tubuh pasien.

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-001` | **Atomicity** — order+baris, permintaan+penerimaan, kantong+alokasi/bukti dalam satu transaksi |
| `NFR-002` | **Concurrency** — token `Version` menjaga satu alokasi aktif & sisa permintaan ≥ 0; bentrok → `409` terbaca |
| `NFR-003` | **Audit** — setiap perpindahan status meninggalkan `BbkTransitionHistory` + salinan teks alasan |
| `NFR-004` | **Otorisasi** — pola `[AccessPermission]`; tombol tak berhak disembunyikan |
| `NFR-005` | **Koreksi** — hanya lewat catatan koreksi append-only; tak ada hapus/balik |
| `NFR-006` | **Penanganan waktu** — masa berlaku bukti dihitung dari konfigurasi komponen, fail-closed bila kosong |

## 18. Skenario UAT

> **`UAT-01` — Pemenuhan darah satu pasien (berhasil).** Kondisi: pasien dengan kunjungan aktif, unit
> berwenang, master terisi **termasuk minimal satu lokasi penyimpanan aktif**. Langkah: buat order →
> buat permintaan → terima 1 kantong → **simpan kantong di kulkas** → periksa & validasi golongan darah
> → alokasikan → catat bukti → berikan. Hasil: kantong `Issued`, order pemenuhan bertambah, riwayat
> lengkap termasuk riwayat penempatan. (`EPIC BD-01/02/04/07/11`)

> **`UAT-02` — Dua petugas merebut kantong sama (gagal terkendali).** Kondisi: satu kantong `Available`.
> Langkah: dua petugas alokasikan ke dua pasien hampir bersamaan. Hasil: satu berhasil, satu ditolak
> "Kantong ini baru saja dialokasikan petugas lain" (`409`); tidak ada alokasi ganda. (`EPIC BD-03`)

> **`UAT-03` — Pemberian tanpa bukti ditolak (gagal).** Kantong dialokasikan, bukti belum ada, tekan
> berikan. Hasil: ditolak "Bukti pemeriksaan kecocokan belum tercatat." (`EPIC BD-04`)

> **`UAT-04` — Kelebihan kiriman (berhasil + terkendali).** Minta 2, datang 3. Hasil: permintaan
> `Fulfilled` sisa 0; kantong ke-3 `PendingReview`. (`EPIC BD-02/05`)

> **`UAT-05` — Konflik golongan darah (gagal lalu berhasil).** Hasil sah O+, muncul A+ tervalidasi.
> Hasil: pasien tak punya golongan darah sah, gerbang tertahan. Lalu pemeriksaan ulang tervalidasi B+
> dinyatakan validator. Hasil: satu hasil sah kembali; ketiga hasil terbaca. (`EPIC BD-07`)

> **`UAT-06` — Koreksi pemberian (berhasil) & hapus pemberian (gagal).** Koreksi nomor kantong oleh
> peran berwenang berhasil; pemberian asal tetap terbaca. Percobaan menghapus pemberian ditolak.
> (`EPIC BD-06`)

Setiap epic `MUST HAVE` memiliki minimal satu UAT berhasil dan satu gagal; pemetaan penuh di
`testing/acceptance-test-matrix.md`.

> **`UAT-11` — Alokasi kantong yang belum disimpan (gagal).** Kantong baru diterima, langsung dicoba
> dialokasikan. Hasil: ditolak "Kantong belum disimpan pada lokasi penyimpanan, sehingga belum dapat
> dialokasikan." Setelah lokasi ditetapkan, alokasi berhasil. (`EPIC BD-11`)

> **`UAT-12` — Perpindahan lokasi kantong yang sedang dialokasikan (berhasil).** Kantong `Allocated`
> untuk Tn. S dipindahkan dari "Kulkas Besar" ke "Kulkas Kecil". Hasil: status tetap `Allocated`,
> alokasi tetap milik Tn. S, bukti tetap berlaku, riwayat memuat dua penempatan, catatan penerimaan awal
> tidak berubah. (`EPIC BD-11`)

> **`UAT-13` — Kulkas rusak dinonaktifkan (berhasil + terkendali).** "Kulkas Lama" berisi tiga kantong
> ditandai nonaktif. Hasil: penonaktifan berhasil disertai peringatan jumlah kantong tertahan; ketiga
> kantong tetap tercatat di sana dengan status tidak berubah; **tidak satu pun dipindahkan sistem**;
> ketiganya tidak dapat dialokasikan. Setelah petugas memindahkan ke kulkas aktif, ketiganya dapat
> dialokasikan kembali. (`EPIC BD-09/11`)

> **`UAT-14` — Pemberian dari kulkas yang dinonaktifkan setelah alokasi (gagal).** Kantong dialokasikan
> Senin saat kulkas masih aktif dan buktinya sah; Selasa kulkas ditandai rusak. Langkah: tekan berikan.
> Hasil: ditolak "Kantong ini berada di lokasi penyimpanan yang sudah tidak aktif dan belum dapat
> diberikan." **Pesannya menyebut lokasi, bukan bukti.** Setelah dipindahkan, pemberian berhasil.
> (`EPIC BD-04/11`)

> **`UAT-15` — Pemberian darurat dari kulkas nonaktif (berhasil, ditandai).** Keadaan `UAT-14` tetapi
> pasien perdarahan hebat. Peran berwenang menerbitkan otorisasi darurat dengan alasan dan keterangan
> gerbang yang dilewati. Hasil: darah diberikan; penanda permanen menyebut **lokasi nonaktif**, bukan
> bukti kecocokan. Pencatatan tanpa keterangan gerbang ditolak. (`EPIC BD-04/11`)

> **`UAT-16` — Konflik golongan darah ditutup validator klinis (berhasil + gagal).** Petugas BDRS
> berwenang validasi memvalidasi hasil baru yang berbeda; konflik muncul. Petugas yang sama mencoba
> menutup konflik — **ditolak**. Dokter BDRS menutupnya dengan menunjuk pemeriksaan ulang tervalidasi —
> berhasil. (`EPIC BD-07`)

> **`UAT-17` — Otorisasi darurat oleh DPJP (berhasil, tercatat).** Dokter BDRS tidak di tempat; DPJP
> menerbitkan otorisasi darurat dengan alasan, kondisi kedaruratan, dan peran yang dipakainya. Hasil:
> darah diberikan; rekam menyimpan peran DPJP. Pencatatan tanpa kondisi kedaruratan atau tanpa peran
> **ditolak**. (`EPIC BD-04`)

> **`UAT-18` — Koreksi dua tahap (berhasil + gagal).** Petugas mengajukan koreksi dengan bukti
> pendukung. Hasil: koreksi menunggu persetujuan, **angka pemenuhan belum berubah**. Petugas yang sama
> mencoba menyetujui — **ditolak**. Dokter BDRS menyetujui — pemenuhan dihitung ulang, pemberian asal
> tetap utuh. Pada kasus lain Dokter BDRS menolak dengan alasan — rekam tidak berubah dan permintaan
> tetap terbaca. (`EPIC BD-06`)

> **`UAT-19` — Bukti kecocokan tidak cocok (gagal terkendali).** Petugas BDRS berwenang validasi
> menyatakan hasil uji cocok **tidak cocok**. Hasil: bukti tersimpan dan terbaca pada riwayat; tombol
> Berikan tertutup; pesan menyebut hasil pemeriksaan, bukan bukti yang belum ada. Kantong tetap dapat
> diselesaikan lewat jalur `PendingReview`. (`EPIC BD-04/05`)

> **`UAT-20` — Penyelesaian bertingkat (berhasil + gagal).** Petugas berkewenangan operasional
> mengembalikan satu kantong ke PMI — berhasil. Petugas yang sama mencoba mengalihkan kantong lain ke
> pasien baru — **ditolak**. Pemegang kewenangan klinis BDRS melakukannya — berhasil, dan bukti
> kecocokan terhadap pasien asal gugur seketika. (`EPIC BD-05`)

> **`UAT-21` — Pembatalan order oleh dua peran (berhasil + gagal).** Dokter membatalkan ordernya dengan
> alasan klinis; petugas BDRS membatalkan order duplikat dengan alasan operasional. Keduanya berhasil
> dan berjejak. Pembatalan tanpa alasan **ditolak**. (`EPIC BD-01`)

## 19. Definition of Done

| Butir | Bukti | Ya/Belum |
| --- | --- | --- |
| Satu pasien berjalan order → pemberian | `UAT-01`, `AC-BD-005/019` | Belum |
| Satu kantong tak mungkin ke dua pasien | `UAT-02`, `AC-BD` konkurensi | Belum |
| Darah tak diberikan tanpa bukti berlaku / saat konflik | `UAT-03`, `AC-BD-018/038/041/034` | Belum |
| Kelebihan kiriman tak membuat sisa negatif | `UAT-04`, `AC-BD-031/032` | Belum |
| Konflik golongan darah hanya lewat pemeriksaan ulang | `UAT-05`, `AC-BD-036/051/054` | Belum |
| Pemberian tak dapat dihapus; koreksi append-only | `UAT-06`, `AC-BD-047/048/049` | Belum |
| Tiga daftar kerja tersedia | `AC-BD-008` + `FE-BD-01/04` | Belum |
| Seluruh master MVP terisi | `02-backend-architecture.md` §J | Belum |
| Prefix `Bbk` terdaftar registry | `BD-DEP-008` | **Sudah** — 3 September 2026 |
| Modul diaktifkan (`PLANNED` → `ACTIVE`) | `BD-DEP-016` | **Sudah** — 3 September 2026, commit `8075784` |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi (epic) | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | Fondasi: `EPIC BD-09` (**tiga** master termasuk lokasi penyimpanan + flag unit), migration master, seeder hak akses | Blueprint disetujui. **Tidak menunggu `BD-DEP-016`** — seluruhnya master `Mst*` berstatus `ACTIVE` |
| `MVP-1` | `EPIC BD-01`, `EPIC BD-02` (order → permintaan → penerimaan) | `MVP-0` selesai |
| `MVP-1b` | **`EPIC BD-11`** (penyimpanan kantong: penetapan lokasi, perpindahan, riwayat penempatan) | `MVP-1` selesai. **Wajib mendahului `MVP-3`** — tanpa penyimpanan, tidak ada kantong yang dapat dialokasikan |
| `MVP-2` | `EPIC BD-07` (golongan darah + konflik), `EPIC BD-10` (daftar kerja) | `MVP-1` selesai |
| `MVP-3` | `EPIC BD-03`, `EPIC BD-04` (alokasi → bukti → pemberian → darurat, **beserta gerbang lokasi**) | `MVP-2` **dan `MVP-1b`** selesai |
| `MVP-4` | `EPIC BD-05`, `EPIC BD-06` (penyelesaian kantong, koreksi), `EPIC BD-08` (pencatatan tindakan) | `MVP-3` selesai |
| `POST-MVP` | Penyaluran biaya Billing, label cetak, integrasi PMI/HCLAB | Di luar rilis pertama |

Epic `OPEN DECISION` (penyaluran biaya Billing) **tidak** dimasukkan ke gelombang mana pun.

**`EPIC BD-11` ditempatkan sebagai `MVP-1b`, bukan digabung ke `MVP-3`.** Alasannya urutan
ketergantungan yang keras, bukan besarnya pekerjaan: gerbang alokasi pada `MVP-3` menuntut kantong sudah
melewati `Stored`, sehingga mengerjakan `MVP-3` lebih dulu menghasilkan alur yang tidak dapat diuji
ujung ke ujung. `MVP-1b` juga menuntut master lokasi dari `MVP-0` sudah terisi — bukan hanya tabelnya
ada, tetapi **isinya ada**, karena master kosong menghentikan seluruh alur (`INV-BD-025`).

| Pertanyaan terbuka | Penjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | :---: |
| Pendaftaran prefix `Bbk` di registry | Pemilik registry engineering | Pembuatan entity operasional `BLOCKED` — seluruh gelombang implementasi | **Ya** |
| ~~Peran jalur darurat, validator, pencatat koreksi (`DEF-BD-004`)~~ | Pemilik proses BDRS & klinis | **Ditutup** `DEC-BD-039`, `DEC-BD-040`, `DEC-BD-041` | Tidak lagi |
| ~~**Sisa `DEF-BD-004`:** peran penyata bukti kecocokan selesai, peran penyelesai kantong `PendingReview`, peran pembatal order~~ | Pemilik proses BDRS & klinis | **Ditutup** `DEC-BD-042`, `DEC-BD-043`, `DEC-BD-044`, dan pemetaan peran terakhirnya oleh `DEC-BD-045` | Tidak lagi — baris seeder `BE-BD-016` sudah ada isinya |
| Nilai jam masa berlaku bukti per komponen (`OQ-BD-012`) | Pemilik proses klinis | Gerbang fail-closed sampai diisi | Tidak (desain jalan; nilai dari konfigurasi) |
| Persetujuan konteks sumber Bank Darah pada Billing (`DEC-BD-016`) | Pemilik BillingManagement | Penyaluran biaya tak dapat dirancang | Hanya epic Billing (`OPEN DECISION`) |
| Keadaan kantong setelah koreksi (`OQ-BD-014`) | Pemilik proses BDRS | Detail implementasi jalur koreksi | Tidak |
| Daftar lokasi penyimpanan darah MMC yang sebenarnya | Pemilik proses BDRS | Master kosong menghentikan seluruh alur — kantong tak dapat disimpan, dialokasikan, maupun diberikan | Tidak memblokir rancangan; **memblokir go-live** |

**Catatan `v2` untuk pertanyaan terbuka.** Rangkaian Storage Location (`DEC-BD-035` sampai
`DEC-BD-038`) **tidak menambah satu pun pertanyaan memblokir**. `ARCH-BD-GAP-10` dan `OQ-BD-015` yang
sempat terbuka sudah ditutup `DEC-BD-037` dan `DEC-BD-038`. Yang ditambahkan hanya satu baris
prasyarat operasional di atas: master lokasi wajib terisi sebelum go-live.

**Status dokumen `approved`.** Ketiga pemblokir yang pernah tercatat kini tertutup seluruhnya:
`DEF-BD-004` ditutup `DEC-BD-039` sampai `DEC-BD-044`, `BD-DEP-008` pendaftaran prefix dan `BD-DEP-016`
aktivasi modul keduanya tertutup 3 September 2026, dan **`G1` approval desain turun pada tanggal yang
sama** atas nama `Sukmagp`.

Karena `BD-DEP-016` beres, entity operasional `Bbk*` sudah berwenang dibuat; karena `G1` beres, seluruh
gelombang `MVP-0` sampai `MVP-4` boleh dijadwalkan. Urutannya tetap seperti bagian 19: `MVP-0` lebih
dulu, `MVP-1b` mendahului `MVP-3`, dan FE tidak mendahului task BE pasangannya.

Sisa `DEF-BD-004` sudah tidak ada; keenam wewenangnya dipetakan. Isian seeder terakhir — nama peran
pemegang `BloodUnit : ResolveNotUsable` — **juga sudah terisi** lewat `DEC-BD-045`, sehingga `BE-BD-016`
tidak lagi menunggu siapa pun. Bersama `DEC-BD-046` yang menegaskan gerbang hasil bukti kecocokan,
**tidak ada satu pun pertanyaan terbuka yang menempel pada set kontrak `v4`**.

**Approval manusia sudah tercatat: `Sukmagp` / `2026-09-03`.** Ia membuka penjadwalan task, bukan izin
menulis source — wewenang tulis backend, migration, eksekusi database di luar dev pemilik, dan
deployment tetap diminta terpisah per tindakan.
