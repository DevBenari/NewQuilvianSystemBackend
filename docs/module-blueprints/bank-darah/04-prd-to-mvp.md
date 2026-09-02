# Bank Darah — PRD ke MVP

## 1. Identitas dokumen

| Field | Value |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Bank Darah (`bank-darah`) · kode PRD `BD` |
| Blueprint ID | `BD-BP-001` · Contract version `v2` — status **`draft`** |
| Repository target | `NewQuilvianSystemBackend` (backend) · `V2QuilvianSystemFrontendDev` (frontend) |
| Commit SHA baseline | backend `792acb9` · frontend `afbb8ab` |
| Arsitektur domain | `03-domain-architecture.md` revisi 6 — `DOMAIN_ARCHITECTURE_READY` · register keputusan revisi 7 |
| Ringkasan cakupan | MVP mencatat pemenuhan darah pasien dari order sampai kantong diberikan/diselesaikan, **tanpa** charge Billing, label cetak, dan integrasi luar |
| `approved_by` / `approved_at` | Kosong — approval adalah tindakan manusia |

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
| Peran berwenang (`DEF-BD-004`) | Jalur darurat, validasi & penyelesaian konflik golongan darah, koreksi pemberian |
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
| `EPIC BD-06` Koreksi pemberian | Catatan koreksi append-only | `MISSING / NEW` |
| `EPIC BD-07` Golongan darah | Sampel, hasil, validasi, konflik, penyelesaian lewat pemeriksaan ulang | `MISSING / NEW` |
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

Seluruh FR lain diturunkan dari `AC-BD-001`..`076` (`testing/acceptance-test-matrix.md`) dan
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
- **Baru:** 14 entity operasional `Bbk*` (prefix placeholder, `BD-DEP-008`) + 2 master `Mst*`.
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
| `POST` | `/{id}/correction` | Koreksi pencatatan pemberian | `BloodUnit : Correct` | `IssuanceCorrectionRequest` | `ApiResponse<BloodUnitDetailDto>` | `EPIC BD-06` | Rencana |

Daftar lengkap tujuh grup ada di `contracts/api-contract.md`; PRD ini tidak melebihinya.

## 14. Matriks kewenangan

String permission persis mengikuti `contracts/permission-audit-matrix.md`. Ringkas: unit pelayanan
`BloodOrder : Create/Read`; petugas Bank Darah alur normal; peran `DEF-BD-004` untuk
`BloodUnit : EmergencyIssue/Correct` dan `BloodGroupExam : Validate`; admin master untuk
`BloodComponent`/`BloodBankReason : *`. Peran final `UNRESOLVED` `DEF-BD-004`.

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
| Prefix `Bbk` terdaftar registry | `BD-DEP-008` | Belum |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi (epic) | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | Fondasi: `EPIC BD-09` (**tiga** master termasuk lokasi penyimpanan + flag unit), migration, prefix registry | Blueprint disetujui + `BD-DEP-008` beres |
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
| Peran jalur darurat, validator, pencatat koreksi (`DEF-BD-004`) | Pemilik proses BDRS & klinis | Endpoint berwenang tak dapat dipetakan ke peran | **Ya** untuk `EPIC BD-04/06/07` bagian peran |
| Nilai jam masa berlaku bukti per komponen (`OQ-BD-012`) | Pemilik proses klinis | Gerbang fail-closed sampai diisi | Tidak (desain jalan; nilai dari konfigurasi) |
| Persetujuan konteks sumber Bank Darah pada Billing (`DEC-BD-016`) | Pemilik BillingManagement | Penyaluran biaya tak dapat dirancang | Hanya epic Billing (`OPEN DECISION`) |
| Keadaan kantong setelah koreksi (`OQ-BD-014`) | Pemilik proses BDRS | Detail implementasi jalur koreksi | Tidak |
| Daftar lokasi penyimpanan darah MMC yang sebenarnya | Pemilik proses BDRS | Master kosong menghentikan seluruh alur — kantong tak dapat disimpan, dialokasikan, maupun diberikan | Tidak memblokir rancangan; **memblokir go-live** |

**Catatan `v2` untuk pertanyaan terbuka.** Rangkaian Storage Location (`DEC-BD-035` sampai
`DEC-BD-038`) **tidak menambah satu pun pertanyaan memblokir**. `ARCH-BD-GAP-10` dan `OQ-BD-015` yang
sempat terbuka sudah ditutup `DEC-BD-037` dan `DEC-BD-038`. Yang ditambahkan hanya satu baris
prasyarat operasional di atas: master lokasi wajib terisi sebelum go-live.

**Status dokumen `draft`.** Ada pertanyaan memblokir yang belum terjawab (`BD-DEP-008`, `DEF-BD-004`);
karena itu dokumen ini **belum boleh** diteruskan ke `/plan-module-delivery` sampai keduanya tuntas.
Keadaan ini **tidak berubah** pada `v2` — kedua pemblokir itu sudah ada sejak `v1` dan tidak berkaitan
dengan Storage Location. Approval manusia belum diklaim.
