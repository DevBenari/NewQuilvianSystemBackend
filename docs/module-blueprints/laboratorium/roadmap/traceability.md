# Traceability Roadmap — Modul Laboratorium

| Field | Value |
|---|---|
| `blueprint_id` | `LAB-BP-001` |
| Roadmap revision | `18` |
| Status | `DRAFT` |
| Tanggal | 2026-09-02; **`EPIC-LAB-11` ditambahkan 2026-09-14** |
| Manifest | `blueprint-manifest.md` revision `26` |
| Backend SHA | `466a7127`, diverifikasi tidak berubah pada `9067fa73` |
| Frontend SHA | `9cd4cd03f` |
| Contract version | `LAB-API-v1` **r7**, `LAB-STATE-v1` r2, `LAB-VAL-v1` **r4**, `LAB-INT-v1` r3, `LAB-PERM-v1` **rev 4** — seluruhnya `approved`; amandemen `MVP-5a` disetujui 2026-09-14 |
| Approval | Yoga Aji Pratama (`yogaaji452@gmail.com`), pemilik modul, 2026-09-02 |
| Masukan | Decisions rev `21`; capability map rev `2` |
| Input hash | `sha256:6504b18a327b9966526bd1df8f3cb878d7f6d6519dacc1f7df16b1066729ae82` (decisions), dihitung 2026-09-02 |

Dokumen ini menjawab satu pertanyaan: **untuk setiap kebutuhan, siapa yang mengerjakannya dan
apa buktinya kalau sudah benar.** Baris yang tidak punya bukti muncul sebagai *coverage gap* di
bagian 4, bukan disembunyikan.

---

## 1. Traceability per Epic

### `EPIC-LAB-01` — Penandaan cito dan batas waktunya

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-01.1` | `LAB-DEC-013`, `LAB-DEC-026` | `02-backend-architecture.md` §4.3 | `LAB-API-v1` r3 | `BE-LAB-10` | `FE-LAB-06` | `AC-18` dan `AC-39` **terbukti** — [`BE-LAB-10.md`](../task/report/backend/BE-LAB-10.md); satu pesanan memuat Kalium cito dan Kolesterol biasa sekaligus. Sisi layar **terbukti 2026-09-04** — [`FE-LAB-06.md`](../task/report/frontend/FE-LAB-06.md). | **`SELESAI`** |
| `FR-01.2` | `LAB-DEC-026` | `contracts/validation-matrix.md` `VAL-03` | `LAB-VAL-v1` r3 | `BE-LAB-10` | `FE-LAB-06` | `AC-18` jalur gagal **terbukti** — `VAL-03` menjawab `403` tanpa mengubah satu ruas pun, `VAL-04` menjawab `409`. Sisi layar **terbukti 2026-09-04** — [`FE-LAB-06.md`](../task/report/frontend/FE-LAB-06.md). | **`SELESAI`** |
| `FR-01.3` | `LAB-DEC-013` | `CAP-04`, `CAP-15` | `LAB-STATE-v1` r2 | `BE-LAB-10` | — | `AC-18` **terbukti** — setiap penandaan menghasilkan satu baris `LabTransitionHistory` berlingkup `LabExamination` yang menunjuk pemeriksaannya | **`SELESAI`** |
| `FR-01.4` | `LAB-DEC-013` | `02-backend-architecture.md` §4.4 | `LAB-API-v1` r3 | `BE-LAB-02` | `FE-LAB-02` | Kolom penyimpanannya ada — `LabValueBound.CitoTurnaroundMinutes`, [`task/report/backend/BE-LAB-02.md`](../task/report/backend/BE-LAB-02.md) bagian 3.2 dan 5.1. `AC-17` **terbukti 2026-09-04** lewat [`BE-LAB-14.md`](../task/report/backend/BE-LAB-14.md): batas waktu itu kini benar-benar dipakai menghitung keterlambatan | **`SELESAI`** |

### `EPIC-LAB-02` — Pemisahan wadah fisik dan pemeriksaan terpesan

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-02.1` | `LAB-DEC-024` | `02-backend-architecture.md` §4.3 | `LAB-API-v1` r3 | `BE-LAB-09`, `BE-LAB-16` | `FE-LAB-07` | `AC-35` **terbukti ujung-ke-ujung** — struktur pada [`BE-LAB-09.md`](../task/report/backend/BE-LAB-09.md) bagian 6.1, dan endpointnya pada [`BE-LAB-16.md`](../task/report/backend/BE-LAB-16.md) bagian 6.1: dua pemeriksaan ditambahkan lewat `POST /by-order` lalu keduanya terbaca lewat `GET /by-specimen` dengan satu barcode yang sama. `VAL-17` .. `VAL-20` masing-masing punya ujinya | **`SELESAI`** |
| `FR-02.2` | `LAB-DEC-024` | `contracts/state-transition-matrix.md` | `LAB-STATE-v1` r2 | `BE-LAB-12`, `BE-LAB-16` | `FE-LAB-07` | `AC-36` **terbukti** — [`BE-LAB-12.md`](../task/report/backend/BE-LAB-12.md) bagian 6.1; menolak wadah menggugurkan kedua pemeriksaan yang ditopangnya. `AC-37` terbukti pada tingkat status; penerbitan faktanya menunggu `BE-LAB-13` | **`SELESAI SEBAGIAN`** — status selesai `BE-LAB-12`; fakta per pemeriksaan menunggu `BE-LAB-13` |
| `FR-02.3` | `LAB-DEC-024` | `VAL-13` | `LAB-VAL-v1` r3 | `BE-LAB-12` | `FE-LAB-07` | `AC-36` **terbukti** — `VAL-13` ditegakkan secara struktural: tidak ada satu pun jalur pengubah yang menolak sebagian pemeriksaan | **`SELESAI`** |
| `FR-02.4` | `LAB-DEC-024` | `erd/data-dictionary.md` | — | `BE-LAB-11` | — | `AC-35` — keenam kolom lepas dari `LabSpecimen` dan utuh pada `LabExamination`, dibuktikan `LabSpecimenColumnSplitTests` 4/4; [`BE-LAB-11.md`](../task/report/backend/BE-LAB-11.md) | **`SELESAI`** |
| `FR-02.5` | `LAB-DEC-024` | `VAL-14` | `LAB-VAL-v1` r3 | `BE-LAB-12` | `FE-LAB-07` | `VAL-14` dan `VAL-15` **terbukti** — [`BE-LAB-12.md`](../task/report/backend/BE-LAB-12.md) bagian 5 | **`SELESAI`** |
| `FR-02.6` | `LAB-DEC-024` | `02-backend-architecture.md` §6 | — | `BE-LAB-11` | — | `AC-35`, `AC-38` — migration `SplitLabSpecimenIntoExamination` terbukti **dua arah** terhadap `QuilvianNewDevYoga`; [`BE-LAB-11.md`](../task/report/backend/BE-LAB-11.md) bagian 5.3 | **`SELESAI`** |

### `EPIC-LAB-03` — Batas nilai dan persetujuan klinis

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-03.1` | `LAB-DEC-006`, `LAB-DEC-018` | `02-backend-architecture.md` §4.4 | `LAB-API-v1` r3 | `BE-LAB-02`, `BE-LAB-04` | `FE-LAB-02` | `AC-24` **terbukti** — [`BE-LAB-02.md`](../task/report/backend/BE-LAB-02.md) bagian 6.1 untuk penyimpanannya, [`BE-LAB-04.md`](../task/report/backend/BE-LAB-04.md) bagian 6.1 untuk endpointnya; tiga baris Hemoglobin dibuat lewat `POST` dan terbaca lewat `GET /` | **`SELESAI`** |
| `FR-03.2` | `LAB-DEC-021` | `02-backend-architecture.md` §4.5 | `LAB-VAL-v1` r3 `VAL-22` .. `VAL-24` | `BE-LAB-02`, `BE-LAB-04` | `FE-LAB-02` | `AC-28` **terbukti** — [`BE-LAB-04.md`](../task/report/backend/BE-LAB-04.md) bagian 5; `VAL-22`, `VAL-23`, dan `VAL-24` masing-masing punya ujinya | **`SELESAI`** |
| `FR-03.3` | `LAB-DEC-023` | — | `LAB-API-v1` r3 | `BE-LAB-04` | `FE-LAB-02` | `AC-33` bagian "batas normal langsung berlaku" **terbukti** — [`BE-LAB-04.md`](../task/report/backend/BE-LAB-04.md) bagian 6.1 | **`SELESAI`** |
| `FR-03.4` | `LAB-DEC-023` | `02-backend-architecture.md` §4.6 | `LAB-API-v1` r3, `VAL-28`, `VAL-32`, `VAL-33` | `BE-LAB-03`, `BE-LAB-05` | `FE-LAB-02` | `AC-33` **terbukti seluruh jalur** — [`BE-LAB-05.md`](../task/report/backend/BE-LAB-05.md) bagian 6.1; `VAL-31` .. `VAL-35` masing-masing punya ujinya, dan `VAL-33` dibuktikan dua arah (menyetujui maupun menolak pengajuan sendiri) | **`SELESAI`** — dibangun; belum dapat dipakai sampai peran `LabCriticalBound : Approve` ditetapkan manajemen |
| `FR-03.5` | `LAB-DEC-023` | `02-backend-architecture.md` §4.7 | `LAB-STATE-v1` r2 | `BE-LAB-03`, `BE-LAB-04` | `FE-LAB-02` | `AC-34` **terbukti** — riwayat tersimpan ([`BE-LAB-03.md`](../task/report/backend/BE-LAB-03.md)) dan diterbitkan serta dibaca lewat endpoint ([`BE-LAB-04.md`](../task/report/backend/BE-LAB-04.md) bagian 6.1) | **`SELESAI`** |
| `FR-03.6` | `LAB-DEC-006` | `erd/data-dictionary.md` | — | `BE-LAB-02` | — | `AC-25` **terbukti** — [`task/report/backend/BE-LAB-02.md`](../task/report/backend/BE-LAB-02.md) bagian 6.1; nol kolom operasional laboratorium pada model EF, dan jumlah kolom `MstProcedure` tetap 35 pada keempat titik pengukuran migration, bagian 5.1 | **`SELESAI`** |

### `EPIC-LAB-04` — Daftar kerja dan pemantauan keterlambatan

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-04.1` | `LAB-DEC-013` | `LAB-FE-006` | `LAB-API-v1` r3 | `BE-LAB-14` | `FE-LAB-08` | `AC-10` dan `AC-39` **terbukti** — [`BE-LAB-14.md`](../task/report/backend/BE-LAB-14.md); satu cito pukul 10.05 di atas empat belas pesanan biasa pukul 10.00 | **`SELESAI`** |
| `FR-04.2` | `LAB-DEC-013` | — | `LAB-API-v1` r3 | `BE-LAB-14` | `FE-LAB-08` | `AC-17` **terbukti** pada kedua jalurnya, termasuk `VAL-39` | **`SELESAI`** |
| `FR-04.3` | `LAB-DEC-013` | `contracts/state-transition-matrix.md` | `LAB-STATE-v1` r2 | `BE-LAB-14` | `FE-LAB-08` | `AC-17` **terbukti** — keterlambatan dihitung sejak wadah dinyatakan layak; wadah yang belum diputuskan tidak pernah terhitung terlambat | **`SELESAI`** |
| `FR-04.4` | `LAB-DEC-013` | `02-backend-architecture.md` | — | `BE-LAB-14` | — | Tinjauan struktur **terbukti** — nol entity ber-nama `Worklist` dan nol jalur tulis pada grupnya | **`SELESAI`** |

### `EPIC-LAB-05` — Fakta kelayakan tagih per pemeriksaan

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-05.1` | `LAB-INH-013` | `CAP-11` | `LAB-INT-v1` r3 `INT-01` | `BE-LAB-13` | — | `AC-37` **terbukti** — [`BE-LAB-13.md`](../task/report/backend/BE-LAB-13.md) bagian 5: wadah dua pemeriksaan menerbitkan dua fakta dengan tarif masing-masing | **`SELESAI`** |
| `FR-05.2` | `LAB-INH-013` | `CAP-11` | `LAB-INT-v1` r3 | `BE-LAB-13` | — | `AC-37` — satuan `SourceItemId` berpindah ke `LabExamination.Id`; producer, enum, dan jalur dispatch tidak disentuh | **`SELESAI`** |
| `FR-05.3` | `LAB-INH-013` | `CAP-11` `Ready to reuse` | `LAB-INT-v1` r3 | `BE-LAB-13` | — | `AC-12` **terbukti** — menekan layak dua kali tetap menghasilkan dua fakta dengan identitas yang sama persis | **`SELESAI`** |
| `FR-05.4` | `LAB-DEC-011` | `CAP-12` | — | `BE-LAB-13` | — | `AC-13` **terbukti 2026-09-04** — `LaboratoryAuthorityTests` 18/18, nol properti dan nol method finansial pada Laboratorium | **`SELESAI`** |

### `EPIC-LAB-06` — Pengelolaan alasan penolakan

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-06.1` | `LAB-DEC-019` | `CAP-05` | `LAB-API-v1` r3 | `BE-LAB-06` | `FE-LAB-03` | `AC-26` **terbukti** — [`BE-LAB-06.md`](../task/report/backend/BE-LAB-06.md) bagian 6.1; alasan "Sampel tidak diberi label" ditambahkan lewat `POST /` lalu langsung terbaca petugas, dan penonaktifannya terbukti lewat `PUT /{id}/activation` | **`SELESAI`** |
| `FR-06.2` | `LAB-DEC-019` | `LAB-FE-012`, `VAL-37` | `LAB-PERM-v1` r3 | `BE-LAB-06` | `FE-LAB-03` | `AC-26` jalur gagal **terbukti** — [`BE-LAB-06.md`](../task/report/backend/BE-LAB-06.md) bagian 5; `VAL-37` diuji tiga arah: penanda kesalahan internal, penanda wajib catatan, dan sifat menyeluruh penolakannya. Hak akses `LabRejectionReason : SystemFlag` berdiri terpisah dari `: Update` | **`SELESAI`** — dibangun; penanda biaya belum dapat disetel sampai pemegang `SystemFlag` ditetapkan manajemen |
| `FR-06.3` | `LAB-DEC-019` | `CAP-05` | — | `BE-LAB-06` | — | Data awal terisi **terbukti** — [`BE-LAB-06.md`](../task/report/backend/BE-LAB-06.md) bagian 5; `LabRejectionReasonSeeder` mengisi sepuluh alasan baseline, tidak menimpa keputusan pengguna, dan memakai identitas yang sama dengan migration `20260824091610` | **`SELESAI`** |

### `EPIC-LAB-08` — Pendaftaran pasien datang langsung dan rujukan luar

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-08.1` | `LAB-DEC-032` | `CAP-09` | `LAB-API-v1` r3 | `BE-LAB-08` | `FE-LAB-05` | `AC-44` **terbukti** — pendaftaran datang langsung membentuk kunjungan ber-`IsWalkIn` benar dan bersumber `WalkIn`; [`BE-LAB-08.md`](../task/report/backend/BE-LAB-08.md) | **`SELESAI`** — sisi layar terbukti 2026-09-07, [`FE-LAB-05.md`](../task/report/frontend/FE-LAB-05.md) |
| `FR-08.2` | `LAB-DEC-032` | `LAB-INT-v1` r3 `INT-05` | `LAB-INT-v1` r3 | `BE-LAB-08`, `BE-EXT-03` | `FE-LAB-05` | `AC-45` **terbukti dua kali** — telusur source modul Laboratorium nol penulisan, dan uji dua penyimpanan terpisah; [`BE-LAB-08.md`](../task/report/backend/BE-LAB-08.md) | **`SELESAI`** — sisi layar terbukti 2026-09-07, [`FE-LAB-05.md`](../task/report/frontend/FE-LAB-05.md) |
| `FR-08.3` | `LAB-DEC-035` | `erd/data-dictionary.md` §9b | — | `BE-EXT-02`, `BE-EXT-03`, `BE-LAB-08` | `FE-LAB-05` | `AC-46` dan `AC-50` **terbukti** — rujukan luar menyimpan kedua penunjuk beserta nomor surat, dan permintaannya tidak punya ruas nama perujuk sama sekali; [`BE-LAB-08.md`](../task/report/backend/BE-LAB-08.md) | **`SELESAI`** — sisi layar terbukti 2026-09-07, [`FE-LAB-05.md`](../task/report/frontend/FE-LAB-05.md) |
| `FR-08.4` | `LAB-DEC-032` | `INT-05` idempotensi | `LAB-INT-v1` r3 | `BE-EXT-03`, `BE-LAB-08` | — | `AC-44` **terbukti** — kunci sama dikirim dua kali menghasilkan satu kunjungan; ditegakkan unique index tersaring, bukan kode aplikasi; [`BE-LAB-08.md`](../task/report/backend/BE-LAB-08.md) | **Selesai** |
| `FR-08.5` | `LAB-DEC-032` | `INT-05` perilaku tolak | `LAB-INT-v1` r3 | `BE-LAB-08` | `FE-LAB-05` | `AC-45` **terbukti** — penolakan Registrasi diteruskan apa adanya dan nol baris tersisa, termasuk sumber pembayaran; [`BE-LAB-08.md`](../task/report/backend/BE-LAB-08.md) | **`SELESAI`** — sisi layar terbukti 2026-09-07, [`FE-LAB-05.md`](../task/report/frontend/FE-LAB-05.md) |

### `EPIC-LAB-09` — Katalog, harga, dan cakupan penjamin

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-09.1` | `LAB-DEC-036` | `CAP-06` | `LAB-API-v1` r3 | `BE-LAB-07`, `BE-EXT-01` | `FE-LAB-04` | `AC-43` **terbukti** — tiga pemeriksaan menampilkan harga satuan dan total tanpa satu baris tagihan pun terbentuk; [`BE-LAB-07.md`](../task/report/backend/BE-LAB-07.md). Sisi layar **terbukti 2026-09-04** — [`FE-LAB-04.md`](../task/report/frontend/FE-LAB-04.md). | **`SELESAI`** |
| `FR-09.2` | `LAB-DEC-033` | `CAP-10` | `LAB-API-v1` r3 | `BE-LAB-07` | `FE-LAB-04` | `AC-43` **terbukti** — harga berasal dari `MstTariff` dengan aturan berlaku yang sama persis dengan jalur pemesanan; [`BE-LAB-07.md`](../task/report/backend/BE-LAB-07.md). Sisi layar **terbukti 2026-09-04** — [`FE-LAB-04.md`](../task/report/frontend/FE-LAB-04.md). | **`SELESAI`** |
| `FR-09.3` | `LAB-DEC-033` | `MstInsuranceTariff` | `LAB-INT-v1` r3 `INT-06` | `BE-LAB-07` | `FE-LAB-04` | `AC-43` **terbukti** — harga kontrak penjamin tampil bila ada, dan ketiadaannya ditandai tidak tercakup, bukan gratis; [`BE-LAB-07.md`](../task/report/backend/BE-LAB-07.md). Sisi layar **terbukti 2026-09-04** — [`FE-LAB-04.md`](../task/report/frontend/FE-LAB-04.md). | **`SELESAI`** |
| `FR-09.4` | `LAB-DEC-033` | `VAL-50` | `LAB-API-v1` r3 | `BE-LAB-07` | `FE-LAB-04` | `AC-47` dan `AC-48` **terbukti** — nol entity tarif milik Laboratorium, dan nol jalur ubah pada grup katalog; [`BE-LAB-07.md`](../task/report/backend/BE-LAB-07.md). Sisi layar **terbukti 2026-09-04** — [`FE-LAB-04.md`](../task/report/frontend/FE-LAB-04.md). | **`SELESAI`** |
| `FR-09.5` | `LAB-DEC-036` | `INV-22`, `VAL-46` | `LAB-VAL-v1` r3 | `BE-LAB-07` | — | `AC-51` **terbukti** — menambahkan Hemoglobin ke pesanan Mikrobiologi ditolak `422` `VAL-46`; [`BE-LAB-07.md`](../task/report/backend/BE-LAB-07.md). Penegakannya menuntut kedua disiplin diketahui, sehingga pengisian nilai disiplin katalog masih menentukan seberapa luas ia berlaku | **`SELESAI`** — cakupannya bergantung pada pengisian nilai disiplin |

### `EPIC-LAB-10` — Monitoring per disiplin

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-10.1` | `LAB-DEC-025` | `03-domain-architecture.md` `S15` | `LAB-API-v1` r3 | `BE-LAB-15` | `FE-LAB-09` | `AC-41` **terbukti** — [`BE-LAB-15.md`](../task/report/backend/BE-LAB-15.md); tiga daftar dengan data campuran, tidak satu baris pun menyeberang | **`SELESAI`** |
| `FR-10.2` | `LAB-DEC-025` | — | `LAB-API-v1` r3 | `BE-LAB-15` | `FE-LAB-09` | `AC-41` **terbukti pada kedua sisinya** — backend: penyaring ditulis satu kali dan dipakai ketiga jalur, disiplin bukan ruas penyaring. Layar: ketiga disiplin menunjuk **objek definisi penyaring yang sama**, dan ujinya memeriksa identitas objeknya sehingga percabangan pertama langsung tertangkap; [`FE-LAB-09.md`](../task/report/frontend/FE-LAB-09.md) | **`SELESAI`** |
| `FR-10.3` | `LAB-DEC-025` | `erd/data-dictionary.md` | `LAB-API-v1` r3 | `BE-LAB-01` | — | `AC-11` **terbukti**, `AC-41` terbukti sebagian — [`task/report/backend/BE-LAB-01.md`](../task/report/backend/BE-LAB-01.md) bagian 6; migration terbukti dua arah pada `QuilvianNewDevYoga`, bagian 5.1 | **`SELESAI`** |

### `EPIC-LAB-07` — Layar Laboratorium

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-07.1` | `LAB-DEC-010` | `03-frontend-architecture.md` §3.1 | `LAB-API-v1` r3 | — | `FE-LAB-06` | `AC-18` dan `AC-43` **terbukti di sisi layar 2026-09-04** — [`FE-LAB-06.md`](../task/report/frontend/FE-LAB-06.md): penanda cito melekat pada baris pemeriksaan dan dijaga uji unit, dan harga tampil saat memesan lewat komponen pemilih katalog. | **`SELESAI`** — kontrol cito belum dapat disembunyikan dari dokter yang bukan pemesan, karena respons pesanan tidak membawa `requestedByUserId` |
| `FR-07.2` | `LAB-DEC-010` | §3.2, `LAB-FE-009`, `LAB-FE-010` | `LAB-API-v1` r3 | — | `FE-LAB-07` | `AC-36` **terbukti pada tingkat aturan dan uji** — `LAB-FE-009` berlapis tiga dan `LAB-FE-010` muncul sebelum konfirmasi; sembilan belas uji, termasuk penjaga ketiadaan aksi berlingkup pemeriksaan (`VAL-13`); [`FE-LAB-07.md`](../task/report/frontend/FE-LAB-07.md) | **`SELESAI`** — verifikasi manual menunggu backend dijalankan |
| `FR-07.3` | `LAB-DEC-010` | §3.3, `LAB-FE-006` | `LAB-API-v1` r3 | — | `FE-LAB-08` | `AC-10` dan `AC-17` **terbukti pada tingkat aturan dan uji** — `LAB-FE-006` ditegakkan dua lapis, dan ujinya menelusuri **seluruh** pilihan pengurutan yang ditawarkan layar sehingga tidak satu pun dapat menggeser cito ke bawah; `VAL-39` dijaga uji tersendiri; [`FE-LAB-08.md`](../task/report/frontend/FE-LAB-08.md) | **`SELESAI`** — verifikasi manual menunggu backend dijalankan |
| `FR-07.4` | `LAB-DEC-010` | §3.4, `LAB-FE-011`, `LAB-FE-013` | `LAB-API-v1` r3 | — | `FE-LAB-02` | `AC-33` **terbukti di sisi layar 2026-09-04** — [`FE-LAB-02.md`](../task/report/frontend/FE-LAB-02.md): formulir ubah tidak punya satu pun isian batas kritis, payload menyalin batas yang berlaku, dan jalur pengajuan berdiri sebagai route tersendiri. `LAB-FE-013` terbukti lewat uji unit `S2` | **`SELESAI`** — `AC-34` terpenuhi sebagian: riwayat belum memuat pelaku karena respons backend tanpa nama |
| `FR-07.5` | `LAB-DEC-010` | §3.5, `LAB-FE-012` | `LAB-API-v1` r3 | — | `FE-LAB-03` | `AC-26` **terbukti di sisi layar 2026-09-04** — [`FE-LAB-03.md`](../task/report/frontend/FE-LAB-03.md): kedua penanda sistem tidak pernah menjadi isian maupun ikut pada payload, tampil terkunci beserta nilainya, dan selisih ruas terkunci yang diumumkan backend terbaca sebagai peringatan. Kelima baris `AC-26` punya jawabannya di layar | **`SELESAI`** — penyembunyian aksi penanda sistem masih sedekat peran, karena frontend belum menerima daftar permission |

---

### `EPIC-LAB-11` — Penerimaan Sampling/Specimen

Ditambahkan 2026-09-14. Gelombang `MVP-5a`.

| FR | Keputusan | Desain | Kontrak | Task BE | Task FE | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `FR-11.1` | `LAB-DEC-040` | `02-backend-architecture.md` §11.3; `erd/data-dictionary.md` §12.1 | `LAB-API-v1` **r8**, `LAB-VAL-v1` r4, `LAB-PERM-v1` rev 4 | `BE-LAB-20` ✅, `BE-LAB-21` ✅ kode | `FE-LAB-10`, `FE-LAB-11` | `AC-58` **terpenuhi pada source, keduanya.** Sisi data induk **diterapkan ke `QuilvianNewDevYoga`**: 7 baris baseline terisi, kedua index unik parsial terbaca, `VAL-61` dan `VAL-62` ditolak `23505`, jalur `Down` lalu `Up` dibuktikan ([`BE-LAB-20.md`](../task/report/backend/BE-LAB-20.md)). Sisi wadah kini menolak teks bebas: `VAL-51`, `VAL-52`, `VAL-55` pada `LabSpecimenService.cs:1288-1321`, dan `AC-59` terpenuhi `VAL-53`/`VAL-54` pada `:1323-1338` ([`BE-LAB-21.md`](../task/report/backend/BE-LAB-21.md)). `VAL-63` tetap terverifikasi source; pembuktian runtime kedua task menunggu migration `BE-LAB-21` diterapkan | **`SELESAI`** |
| `FR-11.2` | `LAB-DEC-040` butir 4-5 | `02-backend-architecture.md` §11.7 | `LAB-API-v1` r7 `GET /other-usage` — **Tersedia** | `BE-LAB-25` ✅ | `FE-LAB-10` | **`AC-60` terpenuhi dan terbukti terhadap database.** `T-60d` dijalankan pada `QuilvianNewDevYoga`: tiga wadah `cairan kista` menjadi **satu baris berjumlah tiga** dengan waktu pemakaian terakhir yang benar, sementara `Cairan Kista` **tetap baris tersendiri** — ejaan tidak digabung diam-diam. `ToQueryString()` membuktikan pengelompokannya satu `GROUP BY` penuh di PostgreSQL, nol evaluasi sisi klien. **Nol tabel ringkasan**, nol migration; rekapnya berubah seketika. Nol baris uji tertinggal; [`BE-LAB-25.md`](../task/report/backend/BE-LAB-25.md) | **`SELESAI`** |
| `FR-11.3` | `LAB-DEC-041` | `erd/data-dictionary.md` §12.2 | `LAB-API-v1` r7, `LAB-VAL-v1` r4 `VAL-56`, `VAL-57` | `BE-LAB-21` ✅ | `FE-LAB-11` | `AC-62` terpenuhi `VAL-56` pada `LabSpecimenService.cs:1343`; `VAL-57` pada `:1350-1366`. **`AC-63` terpenuhi dan terbukti:** penelusuran seluruh rujukan `VolumeAmount` menemukan **nol** pembandingan terhadap batas apa pun, sehingga `RULE-021` tegak (`T-63c`). Kolom `numeric(12,3)` berdiri beserta FK `RESTRICT` ke `MstMeasurement`. Migration **diterapkan ke `QuilvianNewDevYoga` 2026-09-15**, `T-M4` ditolak `23503` dan jalur `Down` terbukti. **`mL` dan `gram` sudah tersedia**, sehingga volume dapat dipakai sekarang; [`BE-LAB-21.md`](../task/report/backend/BE-LAB-21.md) | **`SELESAI`** |
| `FR-11.4` | `LAB-DEC-041` | `erd/data-dictionary.md` §12.3 | — | `BE-LAB-21` ✅ | `FE-LAB-11` | `AC-64` **terpenuhi pada kode, dapat dipakai sebagian.** Tidak ada satu pun jalur kode yang membatasi satuan menurut jenis specimen, sehingga jaringan bebas memakai `gram`, `blok`, atau `slide`. **Datanya tersedia sebagian:** `gram` sudah ada dan ber-`IsForLaboratory`, sehingga jaringan dapat menyimpan volumenya sekarang; `blok` dan `slide` **belum ada**, dan hanya keduanya yang tertahan `DATA-MST-MEASUREMENT`. Lihat §4 dan [`BE-LAB-21.md`](../task/report/backend/BE-LAB-21.md) §7 | **`SELESAI SEBAGIAN`** — `gram` tersedia; `blok` dan `slide` menunggu Master Data |
| `FR-11.5` | `LAB-DEC-042` | `02-backend-architecture.md` §11.4; `erd/data-dictionary.md` §12.2 | `LAB-API-v1` r7, `LAB-VAL-v1` r4 `VAL-58`, **`VAL-59` terbukti tidak dapat dilaksanakan** | `BE-LAB-22` ◐ | `FE-LAB-12` | **`AC-65` terbukti secara struktural** — nol ruas permintaan bernama `ReceivedAt` pada seluruh DTO Laboratorium, satu-satunya jalur tulisnya diisi server (`LabSpecimenService.cs:1583`). **`AC-17` terbukti dua kali** — nol rujukan `PhysicallyReceivedAt` pada ketiga berkas perhitungan cito, dan nol diff pada ketiganya. `AC-67` terpenuhi pada source: rekap beralih ke waktu nyata (`:919-931`), selisih tercatat pada jejak audit (`:1507-1508`). **`AC-66` terpenuhi separuh** — `VAL-58` tegak (`:1421-1425`), `VAL-59` tertahan kontrak; [`BE-LAB-22.md`](../task/report/backend/BE-LAB-22.md) §6 | **`SELESAI SEBAGIAN`** — `AC-66` menunggu keputusan pemilik modul |
| ~~`FR-11.6`~~ | ~~`LAB-DEC-038`~~ → `LAB-DEC-050` | `02-backend-architecture.md` §11.5 | `LAB-API-v1` **r9** — ruas `Quantity` dicabut | ~~`BE-LAB-23`~~ ⛔ | `FE-LAB-11` | **Dicabut 2026-09-14.** `AC-52` terbukti tidak dapat dipenuhi: index unik `(SpecimenId, ProcedureId)` atas dasar `BR-20` dan `AC-35` menolak baris kedua. Pemilik modul mencabut `LAB-DEC-038`; kolom Jumlah tidak dibuat. `AC-52`..`AC-54` dan `VAL-60` ikut dicabut; [`BE-LAB-23.md`](../task/report/backend/BE-LAB-23.md) | **`DICABUT`** |
| `FR-11.7` | `LAB-DEC-039`, `LAB-DEC-049` | `02-backend-architecture.md` §11.6 | `LAB-VAL-v1` r3 `VAL-18` — **tidak berubah** | `BE-LAB-24` ✅ | `FE-LAB-11` | **Keempat AC terverifikasi terhadap source 2026-09-14.** `AC-55a` menambah diizinkan sebelum kelayakan; `AC-55b`/`AC-57` ditolak sesudahnya lewat `VAL-18` pada `LabExaminationService.cs:123`; `AC-55c` pembatalan **tidak terkunci** — nol rujukan `SpecimenStatus` pada `CancelAsync`; `AC-56` nol route `process` bertingkat pemeriksaan. **Nol baris source diubah, dan memang tidak perlu**; [`BE-LAB-24.md`](../task/report/backend/BE-LAB-24.md) | **`SELESAI`** |
| `FR-11.8` | `LAB-DEC-045` | `03-frontend-architecture.md` §10 | `LAB-API-v1` r7 | — | `FE-LAB-11`, `FE-LAB-12` | `AC-75`, `AC-76`, `AC-77` — belum dikerjakan | **`BELUM DIMULAI`** |
| `FR-11.9` | `LAB-DEC-043` | `02-backend-architecture.md` §11.9 — **titik sambung, tanpa kontrak** | — | **tidak ada** | **tidak ada** | Terblokir `LAB-COORD-006` | **`OPEN DECISION`** |
| `FR-11.10` | `LAB-DEC-044` `amended`, `LAB-DEC-046`, `LAB-DEC-047` | `02-backend-architecture.md` §11.9 — **titik sambung, tanpa kontrak** | — | **tidak ada** | **tidak ada** | Terblokir `LAB-COORD-007` | **`OPEN DECISION`** |

**`FR-11.7` perlu dibaca dengan teliti.** Statusnya `SEBAGIAN SUDAH ADA`, bukan
`BELUM DIMULAI`. `VAL-18` sudah menegakkan penguncian pada jalur tambah sejak sebelum
`LAB-DEC-039` ditulis; keputusan itu menyamakan `AC-20` dengan kenyataan, bukan mengubah
perilaku. Yang tersisa adalah memeriksa jalur hapus dan menulis pengujian penjaganya.

**`FR-11.9` dan `FR-11.10` sengaja tidak punya kolom Task.** Keduanya berstatus `OPEN DECISION`
dan tidak diberi ID task sama sekali — bukan diberi ID lalu ditandai tertahan. ID task yang
sudah ada cenderung ikut masuk rencana kapasitas walaupun bertanda tertahan.

## 2. Rekapitulasi Cakupan

| Yang diperiksa | Jumlah | Tercakup | Keterangan |
|---|---:|---:|---|
| Functional requirement `FR-*` | 45 | **45** | Seluruhnya punya task backend, task frontend, atau keduanya |
| Acceptance criteria pada `testing/acceptance-test-matrix.md` | 30 | **30** | Naik dari 28 setelah `AC-11` dan `AC-19` ditambahkan 2026-09-02. Seluruhnya dirujuk minimal satu task |
| Epic dalam scope | 10 | **10** | `EPIC-LAB-01` .. `EPIC-LAB-10` |
| Slice dalam scope | 10 | **10** | `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |
| Endpoint To-Be pada `contracts/api-contract.md` | 37 | **37** | Diaudit 2026-09-02; empat endpoint Lab Examination semula tanpa pemilik, ditutup `BE-LAB-16` |
| Aturan validasi `VAL-*` | 50 | **50** | Seluruhnya berpemilik per bagian matriks. `VAL-09` — empat mata pada tingkat wadah — semula tidak dikutip task mana pun; kini tegas di `BE-LAB-12` |
| Entity pada `02-backend-architecture.md` §4 | 9 | **9** | Delapan berpemilik task; `TrxLabTransitionHistory` dipakai apa adanya tanpa pekerjaan struktur |
| Pasangan kewenangan `resource : action` | 29 | **29** | 16 baru dan berpemilik task; 13 sisanya sudah terdaftar pada `c87d9c0` |
| Kontrak integrasi `INT-*` | 6 | **6** | `INT-01`, `INT-05`, `INT-06` berpemilik task; `INT-02` sampai `INT-04` sudah berjalan dan tidak disentuh Rilis 1 |
| Task backend | 19 | — | 16 milik Laboratorium, 3 dependency eksternal |
| Task frontend | 9 | — | Seluruhnya milik Laboratorium |

---

## 3. Kemampuan Existing yang Dipakai Ulang

Delapan belas task di atas bersandar pada kemampuan yang **sudah ada dan terbukti**, bukan
dibangun ulang:

| Capability | Status | Dipakai oleh |
|---|---|---|
| `CAP-01` pesanan lab | `Extend` | `BE-LAB-01`, `BE-LAB-15` |
| `CAP-02` siklus hidup sampel | `Extend` | `BE-LAB-09`, `BE-LAB-12` |
| `CAP-04` riwayat perpindahan status | `Ready to reuse` | `BE-LAB-03`, `BE-LAB-10` |
| `CAP-05` alasan penolakan | `Reuse with adapter` | `BE-LAB-06` |
| `CAP-06` katalog `MstProcedure` | `Reuse with adapter` | `BE-LAB-07` |
| `CAP-08` kunjungan pasien | `Ready to reuse` | `BE-LAB-08`, `BE-LAB-15` |
| `CAP-09` identitas pasien dan dokter | `Ready to reuse` | `BE-LAB-08` |
| `CAP-10` tarif dan salinannya | `Ready to reuse` | `BE-LAB-07`, `BE-LAB-11` |
| `CAP-11` fakta kelayakan tagih | `Ready to reuse` | `BE-LAB-13` |
| `CAP-12` batas kewenangan finansial | `Ready to reuse` | `BE-LAB-13` |
| `CAP-13`, `CAP-14` kewenangan per aksi | `Ready to reuse` | `BE-LAB-04`, `BE-LAB-05`, `BE-LAB-06` |
| `CAP-15` identitas pelaku | `Ready to reuse` | `BE-LAB-05`, `BE-LAB-10` |
| `CAP-17` perlindungan konkurensi | `Ready to reuse` | `BE-LAB-03`, `BE-LAB-12` |
| `CAP-22` pola tujuh lapis frontend | `Ready to reuse` | `FE-LAB-01` — **terpakai** 2026-09-04. Tujuh lapis modul `laboratory-management` berdiri mengikuti pola yang sama, tanpa lapis kedelapan dan tanpa pola route baru. [`task/report/frontend/FE-LAB-01.md`](../task/report/frontend/FE-LAB-01.md) |
| `CAP-23` `axiosInstance` dan Redux | `Ready to reuse` | `FE-LAB-01` — **terpakai** 2026-09-04. `InstanceAxios` yang sudah ada dipakai apa adanya tanpa instance baru, dan potongan Redux `labOrder` terdaftar pada `store.jsx` mengikuti pola slice Health Services yang berjalan. [`task/report/frontend/FE-LAB-01.md`](../task/report/frontend/FE-LAB-01.md) |

**Satu kemampuan yang sengaja tidak dipakai ulang.** `CAP-16` — penegakan prinsip empat mata —
berstatus `Missing`, dan sistem permission yang ada **tidak dapat** menggantikannya:
`AccessPermissionService.HasAccessAsync` hanya menjawab boleh atau tidak, tidak pernah
membandingkan pelaku sebelumnya. `BE-LAB-05` wajib menegakkannya di dalam service.

---

## 4. Coverage Gap

### 4.1 Dua acceptance criteria dalam scope — ✅ **ditutup 2026-09-02**

| AC | Isi | Gap yang ditemukan | Yang dilakukan |
|---|---|---|---|
| `AC-11` | Pesanan lab dapat dibuat dari kunjungan Rawat Jalan, Rawat Inap, maupun IGD dengan alur kerja yang sama | Tercatat di decision log (`LAB-DEC-009`) tetapi tidak punya baris di `testing/acceptance-test-matrix.md`. `CAP-08` menyatakan kemampuannya sudah ada, tetapi `BE-LAB-01` menambah kolom pada `LabOrder` sehingga jalur pembuatan pesanan tersentuh | Bagian **1b. Alur Pemesanan Lintas Unit** ditambahkan ke matriks dengan empat skenario, termasuk pembuktian bahwa penambahan kolom disiplin tidak melahirkan cabang khusus per jenis kunjungan. `AC-11` menjadi bukti verifikasi `BE-LAB-01` |
| `AC-19` | Tidak ada satu pun tabel atau endpoint Laboratorium yang menyimpan stok, pembelian, atau pemakaian reagen | Penjaga batas scope sejenis `AC-42` Bank Darah, yang sudah punya baris uji. `AC-19` tidak punya | Ditambahkan sebagai uji unit penelusuran pada bagian 7e matriks, sejajar `AC-42`, dan menjadi bukti verifikasi `BE-LAB-15` |

Dengan keduanya ditutup, **seluruh acceptance criteria dalam scope kini punya baris uji.**

### 4.1b Gap yang dibuka amandemen 2026-09-14

| Gap | Isi | Kenapa tidak ditutup sekarang |
|---|---|---|
| Grafik urutan dependency gelombang lama | `backend-roadmap.md` dan `frontend-roadmap.md` tidak punya grafik urutan dependency untuk `MVP-0` sampai `MVP-3` — sembilan belas task backend dan sembilan task frontend | Kedua roadmap ditulis sebelum grafik itu menjadi kewajiban. Menurunkannya sekarang berarti menyimpulkan ulang dependency dari ingatan dokumen, dan itu berisiko keliru. Gelombang `MVP-5a` punya grafiknya sendiri |
| Verifikasi volume `BE-LAB-21` | **Dipersempit drastis 2026-09-15 setelah database dibaca langsung.** Dugaan bahwa `MstMeasurement` kosong dari satuan laboratorium **terbantah**: 17 baris ber-`IsForLaboratory` yang aktif sudah ada, dan **`mL` serta `gram` termasuk di dalamnya**. Volume bersatuan mililiter dan gram **dapat dipakai sekarang**. Yang tersisa hanya **tiga** satuan — `µL`, `blok`, `slide` — sehingga `AC-62` tidak lagi tertahan dan `AC-64` tertahan sebagian | Pengisian data induk global milik `master-data`. Permintaannya **perlu ditulis ulang**: tiga baris bukan lima, dan tanpa mendikte kode karena `MstMeasurement` memakai seri tergenerasi `STN`. Lihat `DATA-MST-MEASUREMENT` pada bagian 5 |
| Kelonggaran `IsForLaboratory` | **Baru 2026-09-15, temuan sampingan.** Penanda itu dibawa juga oleh `GALON`, `M3`, `KG`, `ONS`, `IU`, `mcg`, dan `Liter/Jam`, sehingga daftar pilihan satuan volume di layar penerimaan akan menawarkan **galon dan meter kubik** untuk sebuah tabung darah. Ada pula **dua baris gram bersimbol sama** (`GRAM` dan `GR`) | Kebersihan data induk global, milik `master-data`. **`VAL-57` sengaja tidak dipersempit** — penyaring tambahan berarti mengarang aturan yang tidak pernah diputuskan, dan justru akan menolak `blok` dan `slide` yang diminta `AC-64` |
| ~~Pembuktian runtime `BE-LAB-21` dan `BE-LAB-22`~~ | ✅ **Ditutup 2026-09-15.** Kedua migration diterapkan ke `QuilvianNewDevYoga` dalam satu jendela; `T-M2` dan `T-M4` (`23503`) terbukti, jalur `Down` lalu `Up` dibuktikan dengan verifikasi ulang yang tetap lulus, nol baris uji tertinggal. Butir DoD *"migration jalan maju dan mundur"* terpenuhi pada kedua task | Tersisa `T-58c`, `T-59c`, `T-61a`, `T-65a`, `T-67a`, `T-67b` yang memerlukan **aplikasi dijalankan**, bukan database. Bukan gap desain maupun gap kode |
| **`VAL-59` tidak dapat dilaksanakan** | **Baru 2026-09-15, gap desain yang sesungguhnya.** `VAL-59` membandingkan waktu penerimaan fisik terhadap waktu pengambilan, tetapi satu-satunya permintaan yang membawa waktu penerimaan fisik adalah `PlanLabSpecimenRequest` — dan di sana `CollectedAt` masih kosong karena diisi server pada tindakan pengambilan. Menegakkannya pada tindakan pengambilan justru menolak skenario contoh `BR-37` sendiri. `AC-66` terpenuhi separuh | **Pemilik modul.** Tiga pilihan pada [`BE-LAB-22.md`](../task/report/backend/BE-LAB-22.md) §6: tambah ruas waktu pengambilan lewat `r10`, persempit `VAL-59`, atau cabut seperti `VAL-60`. **Temuan ketiga dengan pola yang sama** setelah `BE-LAB-23` dan `BE-LAB-24` |

**Gap pertama dicatat, bukan dikerjakan diam-diam.** Menuliskan grafik yang dependency-nya
ditebak lebih berbahaya daripada tidak punya grafik sama sekali: yang pertama terlihat
otoritatif dan akan dipakai orang untuk mengurutkan pekerjaan.

**`AC-60` perlu satu catatan.** Ia dibuktikan `BE-LAB-25` dan `FE-LAB-10`, tetapi daftar
pantaunya baru berisi setelah ada wadah berjenis `Lainnya` yang tercatat. Pengujiannya karena
itu menyiapkan datanya sendiri, bukan menunggu pemakaian nyata.

### 4.2 Dua puluh satu acceptance criteria di luar scope — bukan gap

`AC-01` .. `AC-09`, `AC-14` .. `AC-16`, `AC-20` .. `AC-23`, `AC-27`, `AC-29` .. `AC-32` juga
tidak punya baris uji, tetapi seluruhnya milik slice yang **memang di luar scope**: pengisian
dan validasi hasil (`S4`), nilai kritis (`S5`), koreksi hasil (`S6`), pemberitahuan (`S8`), dan
penyuntingan pesanan oleh dokter (`S1b`). Ketiadaan barisnya wajar dan tidak perlu ditutup
sekarang.

### 4.3 Dua peran yang belum ditetapkan

Keduanya bukan penahan pembangunan, melainkan penahan pernyataan siap pakai. Task dapat
dibangun dan diuji dengan peran contoh, tetapi belum dapat dinyatakan siap dipakai sebelum
manajemen rumah sakit menetapkan pemegangnya.

| Hak akses | Dibangun oleh | Akibat selama pemegangnya belum ada |
|---|---|---|
| `LabCriticalBound : Approve` | `BE-LAB-05` | Tidak ada akun yang dapat menyetujui pengajuan, sehingga batas kritis tetap tidak dapat diubah lewat aplikasi |
| `LabRejectionReason : SystemFlag` | `BE-LAB-06` — **ditemukan 2026-09-03** | Tidak ada akun yang dapat menyetel penanda kesalahan internal, sehingga setiap alasan penolakan baru selalu bernilai "bukan kesalahan internal". Akibat nyatanya: pengambilan ulang untuk alasan-alasan baru itu **dapat ditagihkan kepada pasien** sampai peran ini ditetapkan. Perlu diketahui Billing |

### 4.4 Utang pembukuan — ✅ **keduanya ditutup 2026-09-02**

| Butir | Keadaan |
|---|---|
| ~~`input_hashes` pada manifest masih milik revisi lama~~ | ✅ **Ditutup.** Konvensinya ditemukan dari `pharmacy/blueprint-manifest.md` dan `billing-kasir/blueprint-manifest.md`: **sha256 penuh atas isi ber-line-ending LF**, bukan 16 digit. Metodenya diverifikasi cocok dengan keempat `artifact_hashes` pharmacy sebelum dipakai. Keempat hash Laboratorium dihitung ulang, dan nilai 16 digit lama dinyatakan keliru — tidak cocok dengan isi berkas mana pun |
| ~~`Riwayat Revisi` pada `00-interview-decisions.md` memuat baris revision 19 dua kali~~ | ✅ **Ditutup 2026-09-02.** Kedua salinan digabungkan; keduanya sempat bertentangan soal lokasi canonical dokumen tata kelola dan soal arti `LAB-OPEN-018`. Dicatat sebagai decisions revision 21 |

---

## 5. Penahan yang Masih Terbuka

| ID | Yang tertahan | Pencabut |
|---|---|---|
| ~~`LAB-OPEN-018`~~ | ~~Eksekusi seluruh task backend~~ | ✅ Ditutup 2026-09-02 — rules root runtime 32 berkas |
| `LAB-OPEN-018b` | Tidak menahan apa pun; `/plugin update` berikutnya mengembalikan rules root ke 13 berkas | Pendaftaran ulang marketplace ke `DevBenari/QuilvianEngineeringSkills` |
| ~~`LAB-OPEN-019`~~ | ~~Entity `Lab*` dan migration~~ | ✅ Ditutup 2026-09-02 — registry kini `ACTIVE` |
| ~~`LAB-OPEN-020`~~ | ~~Pemeriksaan konformansi QBE~~ | ✅ Ditutup 2026-09-02 — checker `PASS`, exit 0 |
| ~~`LAB-OPEN-021`~~ | ~~Penamaan dua tabel batas nilai~~ | ✅ Ditutup 2026-09-02 — ditetapkan `Lab` |
| `LAB-OPEN-012` | `BE-LAB-11`, `FR-02.4`, `FR-02.6` | Pemilik repository backend atau DBA. **Terjawab `0` untuk dev pemilik, diverifikasi ulang oleh mesin 2026-09-04**; angka produksi tetap belum diketahui. Sejak `BE-LAB-11` prasyarat ini ditegakkan migration itu sendiri: bila tabelnya masih berisi, migration berhenti sebelum satu kolom pun dihapus |
| `LAB-SIGN-001` | Slice `S4`, `S4b`, `S4c`, `S5`, `S6` — **di luar scope roadmap ini** | Dokter PJ laboratorium atau Komite Medis |
| `LAB-AMD-001` | Slice `S1b` — **di luar scope roadmap ini** | Pemilik blueprint `rawat-jalan` |
| `LAB-COORD-006` | `FR-11.9` — tidak diberi task sama sekali | Pemilik `master-data`, lewat `LAB-REQ-005` butir 1-3 |
| `LAB-COORD-007` | `FR-11.10` — tidak diberi task sama sekali | Pemilik `registration-management` dan `billing-kasir`, lewat `LAB-REQ-005` butir 4-7 |
| `DATA-MST-MEASUREMENT` | **Dipersempit 2026-09-15 oleh pembacaan database.** Bukan lima baris, melainkan **tiga**: `µL`, `blok`, dan `slide`. `mL` dan `gram` **sudah ada** dan ber-`IsForLaboratory`, sehingga volume sudah dapat dipakai dan `AC-62` tidak lagi tertahan. Yang tersisa menahan separuh `AC-64` — patologi anatomi belum dapat menyatakan blok maupun slide | Pemilik `master-data`. **Belum diajukan.** Permintaannya perlu **ditulis ulang** lebih dulu: tiga baris bukan lima, tanpa mendikte kode (seri `STN` tergenerasi), dan blok maupun slide tidak boleh mengizinkan desimal |
| ~~`LAB-CONFLICT-004`~~ | **Ditutup 2026-09-14 oleh `LAB-DEC-049`.** ~~`BE-LAB-24` dan `FR-11.7` bagian jalur batal. **`AC-55` dan `AC-57` bertentangan dengan rancangan warisan** yang mengizinkan pembatalan terkendali sesudah titik tagih | Yoga Aji Pratama. Bila pilihan B diambil, juga pemilik `billing-kasir` dan `rawat-jalan` karena menyentuh `RJ-BIL-GATE-DEC-003` |
| ~~`LAB-CONFLICT-005`~~ | **Ditutup 2026-09-14 oleh `LAB-DEC-050`.** ~~`BE-LAB-23` dan `FR-11.6`. **`AC-52` bertentangan dengan index unik `(SpecimenId, ProcedureId)`** yang dipasang atas dasar `BR-20` dan `AC-35`. Contoh pada `BR-33` juga keliru: Glukosa Puasa dan Glukosa 2 Jam PP adalah dua `MstProcedure` berbeda | Yoga Aji Pratama. Sebaiknya dijawab bersamaan dengan `LAB-CONFLICT-004` |

**Akar kedua pertentangan itu sama, dan pantas dicatat sebagai pola.** Keduanya adalah
acceptance criteria yang ditulis amandemen 2026-09-14 dari `LAB-EVD-001`, dan keduanya lolos
sampai tahap implementasi tanpa pernah diadu dengan model data yang dikunci `LAB-DEC-024` pada
2026-09-01. Artifact lapangan menggambarkan satu layar sebagaimana dipahami penggunanya; ia
tidak tahu wadah dan pemeriksaan sudah dipisahkan, dan bahwa pemisahan itu membawa aturan
keunikan.

Amandemen berikutnya yang berangkat dari artifact lapangan perlu satu langkah tambahan:
**mengadu setiap acceptance criteria baru dengan ERD dan index yang sudah berjalan sebelum
disetujui**, bukan sesudah implementasinya dicoba.
| ~~`DB-EXEC-MVP5A`~~ | **Ditutup 2026-09-14.** Migration diterapkan ke `QuilvianNewDevYoga` atas wewenang pemilik modul; `T-M3`, `VAL-61`, dan jalur `Down` terbukti. Tersisa `T-M4` yang menunggu foreign key `BE-LAB-21`, dan `VAL-63` yang pembuktian runtime-nya menunggu aplikasi dijalankan | — |
| ~~`DB-EXEC-BE-LAB-21`~~ | ✅ **Ditutup 2026-09-15, hari yang sama ia dibuka.** Kedua migration diterapkan ke `QuilvianNewDevYoga` atas wewenang pemilik modul yang menyebut targetnya secara tegas; `T-M2`, `T-M4`, dan jalur `Down` lalu `Up` terbukti. Uraian aslinya disimpan sebagai jejak: **Dibuka 2026-09-15, cakupannya diperluas hari yang sama.** Dua migration sudah dibuat tetapi **belum diterapkan ke database mana pun**: `20260915035317_AddLabSpecimenTypeAndVolumeColumns` dan `20260915042458_AddLabSpecimenPhysicallyReceivedAt`. Menahan `T-M2`, `T-M4`, `T-58c`, `T-59c`, `T-61a`, `T-65a`, `T-67a`, `T-67b`, dan butir DoD *"migration jalan maju dan mundur"* pada kedua task. **Inilah foreign key yang ditunggu `DB-EXEC-MVP5A` untuk `T-M4`** | Pemilik modul. Wewenang yang diberikan pada sesi 2026-09-15 adalah **pembuatan migration saja**. Keduanya dapat diterapkan dalam **satu jendela**. **Perlu dikoordinasikan dengan `FE-LAB-11`** — `specimenTypeId` kini wajib, sehingga layar wadah yang sudah berjalan akan menjawab `422` begitu migration diterapkan |
| `LAB-CONFLICT-006` | **Dibuka 2026-09-15.** `VAL-59` dan separuh `AC-66` **tidak dapat dilaksanakan** pada kontrak `r7`: pembandingnya — waktu pengambilan yang dinyatakan petugas — tidak ada pada satu pun DTO permintaan, dan `CollectedAt` yang tersimpan adalah cap waktu server yang justru lebih akhir daripada waktu kedatangan pada jalur rujukan luar | **Yoga Aji Pratama**, pemilik modul. Tiga pilihan pada [`BE-LAB-22.md`](../task/report/backend/BE-LAB-22.md) §6. Bila pilihan A diambil, `LAB-API-v1` perlu naik ke `r10` |

**`DATA-MST-MEASUREMENT` adalah penahan jenis ketiga**, berbeda dari dua di atasnya: ia tidak
menahan penulisan kode dan tidak menahan approval. Yang tertahan hanya pembuktiannya.
`BE-LAB-21` dapat dinyatakan selesai secara kode sambil verifikasi volumenya menunggu — dan
statusnya wajib ditulis **menunggu data, bukan menunggu kode** pada laporannya.

Pola ini sudah pernah terjadi di modul ini. `FE-LAB-05` selesai 2026-09-07 dan delapan skenario
verifikasi manualnya masih menunggu sampai hari ini karena daftar instansi perujuk kosong.
Mencatatnya sebagai penahan tersendiri sejak awal mencegah task terlihat "selesai" padahal
belum pernah dijalankan sekalipun.

---

## 6. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 18 | 2026-09-15 | **Penutupan gelombang `MVP-5a` sisi backend, ditulis `build-module-backend`.** `FR-11.2` menjadi **`SELESAI`**, dan dengan itu **keenam task backend gelombang ini selesai atau ditutup**. `AC-60` — acceptance criteria terakhir `LAB-DEC-040` yang masih terbuka — terpenuhi dan **terbukti terhadap database**, bukan ditelusuri pada source: tiga wadah `cairan kista` menjadi satu baris berjumlah tiga, sementara `Cairan Kista` tetap baris tersendiri sehingga keragaman ejaan yang menjadi alasan layar itu dibuat tetap terlihat. Pengelompokannya terbukti diterjemahkan menjadi satu `GROUP BY` penuh di PostgreSQL. **Nol tabel ringkasan dibuat**, sesuai butir DoD-nya. **`FE-LAB-10` tidak lagi tertahan** — kedua prasyaratnya selesai. **Yang tersisa pada gelombang ini bukan pekerjaan backend:** `LAB-CONFLICT-006` menunggu keputusan pemilik modul, `DATA-MST-MEASUREMENT` menunggu tiga baris satuan dari `master-data`, ketiga task frontend menunggu dikerjakan dengan `FE-LAB-11` berstatus mendesak karena layar wadah menjawab `422` sejak migration diterapkan, dan `LAB-REQ-005` menunggu jawaban tiga modul lain | `DRAFT` |
| 17 | 2026-09-15 | **Bukti eksekusi database dan satu koreksi asumsi, ditulis `build-module-backend`.** Kedua migration `MVP-5a` yang tertunda diterapkan ke `QuilvianNewDevYoga`; `DB-EXEC-BE-LAB-21` **ditutup pada hari yang sama ia dibuka**. `FR-11.1` dan `FR-11.3` menjadi **`SELESAI`**, `FR-11.4` menjadi **`SELESAI SEBAGIAN`**. `T-M4` terbukti `23503` — menutup butir yang digantung `BE-LAB-20` sejak 2026-09-14 — dan jalur `Down` lalu `Up` dibuktikan dengan verifikasi ulang yang tetap lulus. **Koreksi yang paling penting pada revisi ini bukan kemajuan, melainkan asumsi yang terbantah.** `DATA-MST-MEASUREMENT` sejak revision 14 mengandaikan `MstMeasurement` kosong dari satuan laboratorium; pembacaan database menunjukkan **17 baris ber-`IsForLaboratory` yang aktif sudah ada**, termasuk `mL` dan `gram`. Penahan itu karena itu **dipersempit dari lima baris menjadi tiga** — `µL`, `blok`, `slide` — `AC-62` tidak lagi tertahan sama sekali, dan `AC-64` tertahan separuh. Permintaan ke `master-data` perlu **ditulis ulang**, bukan diteruskan apa adanya: kode `ML`/`UL`/`G`/`BLOK`/`SLIDE` yang diminta blueprint tidak akan pernah ada karena `MstMeasurement` memakai seri tergenerasi `STN`. **Satu gap baru dicatat:** penanda `IsForLaboratory` jauh lebih longgar daripada yang diandaikan rancangan — galon dan meter kubik ikut membawanya — dan `VAL-57` **sengaja tidak dipersempit** untuk menutupinya. Pelajarannya sejajar dengan `LAB-CONFLICT-004` sampai `006`: yang di sana adalah acceptance criteria yang tidak pernah diadu dengan model data, dan yang di sini adalah **penahan yang tidak pernah diadu dengan isi database** | `DRAFT` |
| 16 | 2026-09-15 | **Pembaruan bukti pelaksanaan `BE-LAB-22`, ditulis `build-module-backend`.** `FR-11.5` berpindah menjadi **`SELESAI SEBAGIAN`**. Dua acceptance criteria terbukti **tanpa menunggu apa pun**, dan keduanya dibuktikan dengan pencarian yang tidak menemukan apa-apa: `AC-65` lewat nol ruas permintaan bernama `ReceivedAt` pada seluruh DTO Laboratorium, dan `AC-17` lewat nol rujukan `PhysicallyReceivedAt` pada ketiga berkas perhitungan cito beserta nol diff pada ketiganya. `AC-67` terpenuhi pada source. **Satu penahan baru dibuka sebagai `LAB-CONFLICT-006`:** `VAL-59` terbukti **tidak dapat dilaksanakan** pada kontrak `r7` — pembandingnya, waktu pengambilan yang dinyatakan petugas, tidak ada pada satu pun DTO permintaan, dan `CollectedAt` yang tersimpan adalah cap waktu server yang pada jalur rujukan luar justru lebih akhir daripada waktu kedatangan. Menegakkannya pada tindakan pengambilan akan menolak skenario contoh `BR-37` sendiri. **Ini temuan ketiga dengan pola yang sama** setelah `LAB-CONFLICT-004` dan `LAB-CONFLICT-005`, dan ketiganya menguatkan catatan pada bagian 5: acceptance criteria baru dari artifact lapangan wajib diadu dengan ERD dan alur yang sudah berjalan **sebelum** disetujui, bukan sesudah implementasinya dicoba. Gap eksekusi migration diperluas mencakup kedua migration yang kini menunggu pada tabel yang sama | `DRAFT` |
| 15 | 2026-09-15 | **Pembaruan bukti pelaksanaan `BE-LAB-21`, ditulis `build-module-backend`.** Tiga baris FR berpindah status. `FR-11.1` menjadi **`SELESAI SECARA KODE`**: `AC-58` kini terpenuhi di kedua sisinya — data induknya oleh `BE-LAB-20` dan penolakan teks bebas saat mencatat wadah oleh `BE-LAB-21`, sehingga kata "sebagian" dicabut. `FR-11.3` dan `FR-11.4` berpindah dari `BELUM DIMULAI`. **`AC-63` adalah satu-satunya AC gelombang ini yang terbukti penuh tanpa menunggu apa pun**, karena bentuknya ketiadaan aturan: penelusuran seluruh rujukan `VolumeAmount` menemukan nol pembandingan terhadap batas. Dua gap dicatat pada 4.1b, dan keduanya **bukan gap desain maupun gap kode**: pembuktian runtime menunggu wewenang eksekusi migration, dan verifikasi volume menunggu `DATA-MST-MEASUREMENT`. Penahan data itu **diperkuat, bukan sekadar diulang** — sekarang kodenya berdiri, akibatnya menjadi nyata: selama kelima baris satuan kosong, setiap satuan yang dikirim ditolak `VAL-57` | `DRAFT` |
| 14 | 2026-09-14 | **`EPIC-LAB-11` dipetakan** — sepuluh FR, delapan di antaranya bertask dan dua berstatus `OPEN DECISION` tanpa ID task sama sekali. `FR-11.7` bukan `BELUM DIMULAI` melainkan `SEBAGIAN SUDAH ADA`: `VAL-18` sudah menegakkan penguncian pada jalur tambah sejak sebelum `LAB-DEC-039` ditulis. Dua gap baru dicatat pada 4.1b: ketiadaan grafik urutan dependency pada gelombang lama, dan verifikasi volume yang tertahan lima baris `MstMeasurement`. Penahan `DATA-MST-MEASUREMENT` ditambahkan sebagai **penahan jenis ketiga** — menahan pembuktian, bukan kode maupun approval — mengikuti pola `FE-LAB-05` yang selesai 2026-09-07 tetapi verifikasinya menunggu sampai hari ini | `DRAFT` |
| 1 | 2026-09-02 | Traceability pertama. 45 FR, 28 AC, 10 epic, dan 10 slice dipetakan ke 18 task backend dan 9 task frontend. Dua coverage gap dalam scope ditemukan dan dicatat | `DRAFT` |
| 2 | 2026-09-02 | Kedua coverage gap ditutup: `AC-11` alur pemesanan lintas unit dan `AC-19` batas reagen ditambahkan ke matriks uji. Cakupan acceptance criteria naik menjadi 30 dari 30. Utang pembukuan riwayat revisi decisions ikut ditutup | `DRAFT` |
| 3 | 2026-09-02 | `input_hashes` dihitung ulang sebagai sha256 penuh setelah konvensinya ditemukan dari pharmacy dan billing-kasir dan diverifikasi. `LAB-OPEN-020` ditetapkan menjadi wewenang Andry Zain. Seluruh utang pembukuan tertutup | `DRAFT` |
| 4 | 2026-09-02 | Audit cakupan endpoint ditambahkan sebagai dimensi ketiga di samping FR dan AC. Empat endpoint Lab Examination ternyata tanpa pemilik task; `BE-LAB-16` ditambahkan pada roadmap backend. Total task backend menjadi 19 | `DRAFT` |
| 5 | 2026-09-02 | Audit cakupan diperluas ke aturan validasi, entity, kewenangan, dan integrasi. Seluruhnya berpemilik. Temuan terpenting: `VAL-09`, aturan empat mata pada tingkat wadah, semula tidak dikutip task mana pun — kini dibebankan ke `BE-LAB-12`. Tujuh dimensi cakupan kini terperiksa | `DRAFT` |
| 29 | 2026-09-07 | **Pembaruan bukti pelaksanaan frontend, ditulis `build-module-frontend`.** `FR-10.2` kini punya bukti pada **kedua sisinya** setelah `FE-LAB-09` selesai, dan dengan itu **seluruh sembilan task frontend Laboratorium selesai**. `AC-41` ditegakkan secara struktural di layar: ketiga disiplin menunjuk objek definisi penyaring yang sama, dan ujinya memeriksa identitas objek — bukan kesamaan isi — sehingga percabangan pertama langsung tertangkap. Keputusan `LAB-DEC-025` dijaga dua arah: penyaringnya tidak punya ruas disiplin, dan parameter permintaannya tidak pernah membawanya. Satu batas dicatat: verifikasi manual seluruh task frontend yang tertunda — `FE-LAB-05`, `FE-LAB-07`, `FE-LAB-08`, `FE-LAB-09` — belum dijalankan, menunggu backend dijalankan dan data induk perujuk diisi | `DRAFT` |
| 28 | 2026-09-07 | **Pembaruan bukti pelaksanaan frontend, ditulis `build-module-frontend`.** `FR-07.3` berpindah dari `Direncanakan` menjadi **`SELESAI`** lewat `FE-LAB-08`, sehingga `FR-04.1` sampai `FR-04.3` kini punya bukti pada kedua sisinya. Invariant `LAB-FE-006` ditegakkan dua lapis di layar, dan ujinya menelusuri seluruh pilihan pengurutan yang ditawarkan — bukan satu contoh yang dipilih tangan. Ancaman terhadap invariant itu datang dari tempat yang tidak terduga: `DataTable` mengurutkan ulang datanya sendiri secara bawaan, yang akan menaikkan pemeriksaan biasa di atas cito. `VAL-39` dijaga uji tersendiri. Satu batas dicatat: verifikasi manual delapan skenario belum dijalankan, menunggu backend dan data pekerjaan yang memadai | `DRAFT` |
| 27 | 2026-09-07 | **Pembaruan bukti pelaksanaan frontend, ditulis `build-module-frontend`.** `FR-07.2` berpindah dari `Direncanakan` menjadi **selesai untuk source dan uji** lewat `FE-LAB-07`. Kedua invariant keselamatan `LAB-FE-009` dan `LAB-FE-010` kini punya bukti: yang pertama berlapis tiga — pada data lewat `resolveSpecimenActions`, pada kartu wadah sebagai daftar terbuka, dan sekali lagi di dalam dialog penolakan; yang kedua muncul sebagai pesan utama dialog, sebelum tombol konfirmasi. `VAL-13` dijaga uji yang memeriksa katalog aksi tidak memuat satu pun aksi berlingkup pemeriksaan. Baris `FR-02.1`, `FR-02.2`, `FR-02.3`, dan `FR-02.5` sisi backendnya sudah selesai sejak `BE-LAB-12` dan `BE-LAB-16`; sisi layarnya kini ikut berdiri. **Satu kerusakan di luar Laboratorium ditemukan dan diperbaiki pada sesi yang sama:** merge `c71c02a07` membawa lima kelompok deklarasi kembar `billing-management` yang membuat `npm run build` gagal — nol error menunjuk ke `laboratory-management`. Salah satunya `addCase` kembar yang membuat Redux Toolkit melempar saat store dibentuk, sehingga seluruh aplikasi tidak dapat dijalankan. Atas instruksi eksplisit pemilik repository, salinan keduanya dibuang dan kedua gerbang kembali lolos. Yang tersisa hanya verifikasi manual delapan skenario, menunggu backend dijalankan | `DRAFT` |
| 26 | 2026-09-07 | **Pembaruan bukti pelaksanaan frontend, ditulis `build-module-frontend`.** Kelima baris `FR-08.1` sampai `FR-08.5` kini **`SELESAI`** seluruhnya: sisi backend terbukti lewat `BE-LAB-08`, sisi layar lewat `FE-LAB-05`. `AC-50` ditegakkan **secara struktural** pada layar — formulirnya tidak punya satu pun kotak isian nama perujuk, dan uji memeriksa muatan yang dikirim juga tidak punya ruas namanya. Satu penahan yang tidak terduga ditutup pada sesi yang sama: `MstReferralInstitution` dan `MstReferralDoctor` ternyata tidak punya endpoint sama sekali, sehingga daftar perujuk tidak punya sumber; dua endpoint bacanya dibangun, dan butir Verifikasi `BE-EXT-02` *"kedua data induk dapat dipilih dari daftar"* yang selama ini tidak pernah terpenuhi kini terpenuhi. Satu batas dicatat: verifikasi manual delapan skenario **belum dijalankan**, menunggu data induk perujuk diisi | `DRAFT` |
| 25 | 2026-09-07 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** Kelima baris `FR-08.1` sampai `FR-08.5` berpindah dari `Direncanakan`/`Menunggu` menjadi **backend selesai**, dengan `FR-08.4` **selesai seluruhnya** karena tidak punya pasangan frontend. Penahan yang selama ini melintang — pelaksana `INT-05` yang belum berpemilik — dicabut `BE-LAB-08` dengan membangunnya di sisi Registrasi. `AC-44`, `AC-45`, `AC-46`, dan `AC-50` seluruhnya **terbukti**; `AC-45` dibuktikan dua kali dengan cara yang berbeda. Dengan ini **`FE-LAB-05` tidak lagi terblokir** — ketiga endpoint yang dibutuhkannya tersedia dan terdokumentasi Swagger | `DRAFT` |
| 24 | 2026-09-04 | **Pembaruan bukti pelaksanaan frontend, ditulis `build-module-frontend`.** `FE-LAB-06` selesai: layar pesanan, formulir pemesanan, dan layar detail berdiri, dengan penanda cito dan duplo melekat pada baris pemeriksaan. `AC-40` dijaga uji unit lewat pemeriksaan bahwa daftar pesanan tidak punya kolom kesegeraan. Dua hal dicatat: **`FE-LAB-05` ditandai `BLOCKED`** karena `BE-LAB-08` belum ada sama sekali pada source backend, dan dependency itu diwaive pemilik modul agar `FE-LAB-06` dapat berjalan; serta satu batas kontrak — `LabOrderDetailResponse` tidak membawa `requestedByUserId`, sehingga layar belum dapat menyembunyikan tombol cito dari dokter yang bukan pemesan. Ini temuan ketiga dari pola yang sama setelah `VAL-33` dan `LabRejectionReason : SystemFlag` | `DRAFT` |
| 23 | 2026-09-04 | **Pembaruan bukti pelaksanaan frontend, ditulis `build-module-frontend`.** `FE-LAB-04` selesai, sehingga keempat baris `FR-09.1` sampai `FR-09.4` kini punya bukti dari kedua sisi. Menu tarif berdiri baca saja — tanpa kolom aksi, tanpa tombol ubah, dan tanpa satu pun fungsi maupun thunk tulis — sehingga `AC-48` dijaga sejak di layar, bukan hanya ditolak backend. Komponen pemilih katalog berdiri beserta perhitungan `AC-43`-nya, dan sengaja belum dipasang karena konsumennya `FE-LAB-06`. Dengan ini **gelombang `MVP-0` frontend selesai seluruhnya** | `DRAFT` |
| 22 | 2026-09-04 | **Pembaruan bukti pelaksanaan frontend, ditulis `build-module-frontend`.** `FE-LAB-03` selesai: `FR-07.5` berpindah dari `Direncanakan` menjadi **`SELESAI`**. Tiga layar alasan penolakan berdiri, dan `LAB-FE-012` ditegakkan empat lapis dengan empat uji unit menjaganya. Dua batas dibuka: grup endpoint ini tidak punya `GET /{id}` sehingga fitur ini tidak dapat punya halaman detail dan formulir ubahnya memuat baris dari daftar; dan frontend belum menerima daftar permission per pengguna, sehingga penyembunyian aksi Setel Penanda Sistem hanya dapat sedekat peran, bukan sedekat `LabRejectionReason : SystemFlag`. Keduanya menuntut pekerjaan backend | `DRAFT` |
| 21 | 2026-09-04 | **Pembaruan bukti pelaksanaan frontend, ditulis `build-module-frontend`.** `FE-LAB-02` selesai: `FR-07.4` berpindah dari `Direncanakan` menjadi **`SELESAI`**. Enam layar batas nilai berdiri, dan invariant `LAB-FE-011` ditegakkan tiga lapis dengan dua di antaranya dijaga uji unit. Tiga temuan dibuka: `AC-34` belum memuat pelaku karena `LabValueBoundHistoryResponse` hanya membawa penunjuk pengguna tanpa nama sehingga tidak boleh ditampilkan; status sebelas endpoint Lab Value Bound dan Lab Critical Bound Approval pada `contracts/api-contract.md` masih tertulis `Rencana (belum tersedia)` padahal seluruhnya sudah ada sejak `BE-LAB-04` dan `BE-LAB-05`; dan peran pemegang `LabCriticalBound : Approve` yang sudah tercatat pada bagian 4.3 kini benar-benar menahan pemakaian layar pengajuan | `DRAFT` |
| 20 | 2026-09-04 | **Pembaruan bukti pelaksanaan frontend, ditulis `build-module-frontend`.** `FE-LAB-01` selesai: kerangka tujuh lapis modul `laboratory-management` berdiri di frontend beserta kontrak penanganan state yang dipakai seluruh layar berikutnya. `CAP-22` dan `CAP-23` berpindah dari sekadar `Ready to reuse` menjadi **benar-benar terpakai**, dengan buktinya. Task ini tidak memindahkan satu pun baris FR karena memang tidak menjawab requirement bisnis; ia menopang sembilan task frontend sesudahnya. Satu catatan lingkungan dibuka: script `npm run test:unit` memakai pola glob yang tidak dikembangkan test runner Node 20 yang terpasang, sehingga suite hanya berjalan lewat bentuk perintah `--test tests/unit/` | `DRAFT` |
| 19 | 2026-09-04 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** Kelima baris `FR-09.1` sampai `FR-09.5` berpindah menjadi **`SELESAI`** oleh `BE-LAB-07`. `AC-43` terbukti tanpa satu baris tagihan pun terbentuk, `AC-47` dan `AC-48` terbukti lewat ketiadaan — nol entity tarif dan nol jalur ubah — dan `AC-51` terbukti lewat `VAL-46`. Satu batas dicatat: penegakan `INV-22` menuntut disiplin pesanan **dan** disiplin katalog sama-sama diketahui, sehingga cakupannya masih bergantung pada pengisian nilai disiplin yang menunggu penggolongan dari pihak klinis | `DRAFT` |
| 18 | 2026-09-04 | **Dependency eksternal dikerjakan, ditulis `build-module-backend`.** `BE-EXT-01`, `BE-EXT-02`, dan `BE-EXT-03` tidak lagi menahan siapa pun: kolom `MstProcedure.LabDiscipline`, dua data induk perujuk, dan dua penunjuk perujuk pada `TrxPatientEncounter` semuanya sudah ada dan sudah diterapkan ke dev pemilik. Lima baris `FR-08.2`, `FR-08.3`, `FR-08.4`, `FR-09.1`, dan `FR-09.5` berpindah dari `Menunggu eksternal` menjadi menunggu task Laboratorium sendiri — kecuali `FR-08.4`, yang idempotensinya masih menuntut endpoint `INT-05` milik `registration-management`. Bentuk teknis `INT-05` ditulis pada `contracts/integration-contract.md` bagian 2b | `DRAFT` |
| 17 | 2026-09-04 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-10.1` dan `FR-10.2` berpindah menjadi **`SELESAI`** oleh `BE-LAB-15`. `AC-42` terbukti bersama `AC-19`: penelusuran seluruh tipe, anggota, entity, dan route Laboratorium tidak menemukan satu pun yang melayani Bank Darah maupun stok, pembelian, dan pemakaian reagen | `DRAFT` |
| 16 | 2026-09-04 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** Keempat baris `FR-04.1` .. `FR-04.4` berpindah menjadi **`SELESAI`** oleh `BE-LAB-14`. `FR-01.4` ikut naik dari `SELESAI SEBAGIAN` menjadi **`SELESAI`**: kolom `CitoTurnaroundMinutes` yang disediakan `BE-LAB-02` kini benar-benar dipakai menghitung keterlambatan, sehingga `AC-17` yang selama ini menunggu akhirnya terbukti | `DRAFT` |
| 15 | 2026-09-04 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-01.1`, `FR-01.2`, dan `FR-01.3` berpindah menjadi **`SELESAI`** oleh `BE-LAB-10`: kesegeraan dan penanda duplo melekat pada pemeriksaan, `VAL-03` menjawab `403` dan `VAL-04` menjawab `409`, dan setiap penandaan meninggalkan satu baris riwayat berlingkup `LabExamination`. `AC-40` dijaga dua arah — penanda duplo hanya mengenai baris yang ditandai, dan ketiadaan endpoint kesegeraan pada grup `Lab Order` ikut diuji | `DRAFT` |
| 14 | 2026-09-04 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-02.4` dan `FR-02.6` berpindah menjadi **`SELESAI SEBAGIAN`** oleh `BE-LAB-11`: keenam kolom salinan tarif lepas dari `LabSpecimen` dan utuh pada `LabExamination`, dan migration `SplitLabSpecimenIntoExamination` berikut skrip maju dan mundurnya sudah ada. Migration dijalankan dua arah terhadap `QuilvianNewDevYoga` — maju, mundur, lalu maju lagi — sehingga keduanya menjadi **`SELESAI`**. `LAB-OPEN-012` tetap terbuka untuk produksi, tetapi prasyaratnya kini ditegakkan migration itu sendiri, dan penjaganya lolos dua kali pada dev pemilik sehingga jawaban `0` terverifikasi ulang oleh mesin. Empat baris `FR-05.1` .. `FR-05.4` yang masih tertulis `Direncanakan` diperbarui mengikuti `BE-LAB-13` yang sudah selesai, termasuk `AC-13` yang terbukti 2026-09-04 | `DRAFT` |
| 13 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-02.2`, `FR-02.3`, dan `FR-02.5` diperbarui oleh `BE-LAB-12`: `AC-36` terbukti — menolak wadah menggugurkan seluruh pemeriksaan yang ditopangnya — dan `VAL-13` ditegakkan secara struktural. `VAL-05` sampai `VAL-15` masing-masing punya ujinya, termasuk `VAL-09` empat mata yang ditulis sebagai kode di dalam service. `FR-02.4` dan `FR-02.6` keluar dari `BLOCKED` setelah `LAB-OPEN-012` dijawab dengan angka `0`. `AC-37` baru terbukti pada tingkat status; penerbitan fakta per pemeriksaan tetap milik `BE-LAB-13` | `DRAFT` |
| 12 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-02.1` naik dari `SELESAI SEBAGIAN` menjadi **`SELESAI`**: `BE-LAB-16` melengkapi entity dari `BE-LAB-09` dengan empat endpoint, sehingga `AC-35` kini terbukti ujung-ke-ujung — dua pemeriksaan ditambahkan lewat `POST /by-order` lalu terbaca lewat `GET /by-specimen` dengan satu barcode yang sama. `VAL-17` .. `VAL-20` masing-masing punya ujinya, dan batas terpenting task itu terbukti: membatalkan satu pemeriksaan tidak mengubah pemeriksaan lain maupun status wadahnya | `DRAFT` |
| 11 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-02.1` berpindah dari `Direncanakan` menjadi **`SELESAI SEBAGIAN`**: entity `LabExamination` tuntas pada `BE-LAB-09` berikut pembuatan dan eksekusi migration dua arah, sementara endpoint beserta penolakannya menunggu `BE-LAB-16`. `AC-35` dan `AC-40` terbukti pada tingkat struktur. Satu risiko dicatat: keenam kolom yang harus pindah dari `TrxLabSpecimen` belum dipindahkan karena `BE-LAB-11` masih `BLOCKED` oleh `LAB-OPEN-012`, sehingga salinan tarif untuk sementara ada di dua tempat. Satu pertentangan dokumen dicatat: kamus data bagian 4 menuntut kolom baru pada `TrxLabTransitionHistory`, sedangkan roadmap bagian 8.3 menyatakan tabel itu tanpa pekerjaan struktur | `DRAFT` |
| 10 | 2026-09-03 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** Ketiga baris `EPIC-LAB-06` berpindah dari `Direncanakan` menjadi **`SELESAI`**: `BE-LAB-06` tuntas pada tingkat source dan test, tanpa menyentuh schema sehingga tanpa migration. `AC-26` terbukti seluruh jalur — menambah, menonaktifkan, penolakan `VAL-37` dua arah, dan penyetelan sah oleh administrator sistem. `FR-06.3` terbukti lewat `LabRejectionReasonSeeder` yang mengisi sepuluh alasan baseline tanpa menimpa keputusan pengguna. Satu risiko organisasi dicatat pada `FR-06.2`: pemegang `LabRejectionReason : SystemFlag` belum ditetapkan, sehingga alasan baru selalu bernilai "bukan kesalahan internal" dan pengambilan ulangnya dapat ditagihkan kepada pasien sampai peran itu ada | `DRAFT` |
| 9 | 2026-09-02 | **Pembaruan bukti pelaksanaan, ditulis `build-module-backend`.** `FR-10.3` berpindah dari `Direncanakan` menjadi **`SELESAI`**: `BE-LAB-01` tuntas pada tingkat source, test, pembuatan migration, dan eksekusi migration ke `QuilvianNewDevYoga` beserta pembuktian jalur `Down`. `AC-11` terbukti lewat tiga skenario kunjungan; `AC-41` terbukti separuh karena daftar pantau per disiplin adalah cakupan `BE-LAB-15`. Satu temuan lintas modul dibuka sebagai `LAB-REQ-003` — penyimpangan status `FINAL`/`CLOSED` pada Billing yang mematikan koreksi AR. Catatan pembukuan: revision 6 sampai 8 tidak pernah tercatat pada tabel ini walaupun kepala dokumen sudah menyebut revision 8 — selisih itu peninggalan sebelum task ini dan menjadi utang pemilik blueprint | `DRAFT` |
