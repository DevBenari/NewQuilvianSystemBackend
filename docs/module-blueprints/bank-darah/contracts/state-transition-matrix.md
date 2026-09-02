# Bank Darah — State Transition Matrix

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v2` — `draft` |
| `last_changed_in` | `v2` |
| Owner | Pemilik proses BDRS (lifecycle operasional) · pemilik proses klinis (golongan darah) |
| `approved_by` / `approved_at` | Kosong — `draft` |
| Sumber | `03-domain-architecture.md` revisi 6 §G · `00-interview-decisions.md` revisi 7 §5 |
| Nama status | **Nama teknis enum** (bagian F backend). Node status pada `flowcharts/` **MUST** memakai nama yang sama persis |

Aturan umum: setiap perpindahan meninggalkan baris `BbkTransitionHistory` (pelaku, waktu, from, to,
kode alasan + salinan teksnya bila ada). Pesan penolakan lengkap ada di `contracts/validation-matrix.md`
— kolom "Bila dilanggar" di sini hanya menyebut kodenya.

---

## 1. Order Darah — `BbkBloodOrderStatus`

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat order | `Active` | Unit pelayanan berwenang (elektronik) atau petugas Bank Darah (manual) | Pasien & kunjungan sah; unit `IsAvailableForBloodOrder=true`; deteksi ganda `BD-XINV-01` lolos atau dilanjutkan dengan alasan | `VAL-BD-013` unit tak berwenang · `VAL-BD-001` order ganda |
| `Active` | Sebagian kantong diberikan | `PartiallyFulfilled` | Petugas Bank Darah | Ada pemberian sah; jumlah dihitung dari transaksi | — |
| `Active` / `PartiallyFulfilled` | Seluruh kantong diberikan | `FullyFulfilled` | Petugas Bank Darah | Σ pemberian = Σ diminta | — |
| `Active` / `PartiallyFulfilled` | Batalkan | `Cancelled` | Pihak berwenang | Alasan dari daftar terkendali | `VAL-BD-016` alasan wajib |
| `Active` / `PartiallyFulfilled` | Kunjungan berakhir | `Expired` | Sistem | `BbkEncounterStatusReader` menyatakan kunjungan berakhir (`DEC-BD-014`) | — |

**Status terminal:** `FullyFulfilled`, `Cancelled`, `Expired`. Order `Expired` **tidak** dapat
dihidupkan kembali (`ASM-BD-002`) — pasien yang masih butuh darah dibuatkan order baru pada kunjungan
baru.

---

## 2. Permintaan PMI — `BbkProviderRequestStatus`

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Buat permintaan | `Requested` | Petugas Bank Darah | Ada order aktif; `BD-XINV-02` lolos | `VAL-BD-006` permintaan ganda |
| `Requested` | Terima sebagian | `PartiallyFulfilled` | Petugas Bank Darah | Kantong ada fisik | — |
| `Requested` / `PartiallyFulfilled` | Terima sisa | `Fulfilled` | Petugas Bank Darah | Σ diterima = Σ diminta | — |
| `Requested` / `PartiallyFulfilled` | Terima **melebihi** jumlah diminta | `Fulfilled`, sisa berhenti di 0 | Petugas Bank Darah | Token `Version` dijaga (`BD-XINV-03`); kantong berlebih ditandai `IsExcess` → `PendingReview` | `VAL-BD-014` (sisa tak boleh negatif — dijaga sistem, bukan ditolak ke petugas) |
| `Requested` / `PartiallyFulfilled` | Batalkan | `Cancelled` | Pihak berwenang | Alasan terkendali | `VAL-BD-016` |
| `Requested` / `PartiallyFulfilled` | Kunjungan berakhir | `ClosedEncounter` | Sistem | `BbkEncounterStatusReader` (`DEC-BD-020`) | — |
| `ClosedEncounter` | Kantong tetap datang | tetap `ClosedEncounter` | Petugas Bank Darah | Penerimaan tetap dicatat; kantong → `PendingReview` | — |

**Terminal:** `Fulfilled`, `Cancelled`. `ClosedEncounter` menerima penerimaan susulan tetapi tak
kembali aktif.

---

## 3. Kantong Darah Operasional — `BbkBloodUnitStatus`

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Diterima fisik | `Received` | Petugas Bank Darah | Terikat permintaan asal. **Belum punya lokasi, belum dapat dialokasikan** (`DEC-BD-036`) | — |
| `Received` | **Tetapkan lokasi penyimpanan** | `Stored` | Petugas Bank Darah | Lokasi dipilih dari master yang **sedang aktif** (`INV-BD-027`); kantong belum pernah ditempatkan | `VAL-BD-060` lokasi nonaktif · `VAL-BD-061` sudah ditempatkan |
| `Stored` | Masuk stok yang boleh dialokasikan | `Available` | Sistem, sebagai akibat penempatan | Tanpa tindakan manusia tambahan dan tanpa prasyarat tambahan (`ARCH-BD-POS-04`) | — |
| `Stored` | Kantong berlebih, atau permintaan asalnya sudah `ClosedEncounter` | `PendingReview` | Sistem | `DEC-BD-025`, `DEC-BD-020`. Kantong tetap wajib disimpan lebih dulu | — |
| `Stored` / `Available` / `Allocated` / `PendingReview` / `Reallocated` | **Pindahkan lokasi penyimpanan** | **status tidak berubah** | Petugas Bank Darah | Lokasi tujuan **sedang aktif** (`INV-BD-027`); kantong sudah pernah ditempatkan; kantong belum di status terminal. Berlaku juga ketika lokasi asalnya sudah dinonaktifkan | `VAL-BD-060` lokasi nonaktif · `VAL-BD-062` belum pernah ditempatkan |
| `Stored` / `Available` / `Allocated` | **Lokasi penyimpanannya dinonaktifkan** | **status tidak berubah** | Pengelola Setup Bank Darah | Kantong **tidak** dipindahkan sistem dan **tidak** masuk `PendingReview`. Gerbang alokasi & pemberian tertutup sampai kantong dipindahkan (`DEC-BD-037`) | Peringatan `VAL-BD-068` |
| `Available` | Alokasikan | `Allocated` | Petugas Bank Darah | Order aktif; **tak ada alokasi aktif lain** pada kantong ini; **sudah melewati `Stored`** (`INV-BD-025`); **lokasi penempatan terakhir sedang aktif** (`INV-BD-028`, `ARCH-BD-POS-06`) | `VAL-BD-018` alokasi ganda · `VAL-BD-063` belum disimpan · `VAL-BD-064` lokasi nonaktif |
| `Allocated` | Catat bukti kecocokan | tetap `Allocated` (bukti lengkap) | Petugas berwenang | Bukti terhadap pasien tujuan | — |
| `Allocated` (bukti berlaku) | Berikan | `Issued` | Petugas Bank Darah | **Tiga syarat sekaligus, dinilai ulang saat pemberian** (`INV-BD-029`, `ARCH-BD-POS-07`): sudah melewati `Stored` · lokasi penempatan terakhir **sedang aktif** · bukti kecocokan berlaku untuk pasien tujuan & belum lewat masa berlaku | `VAL-BD-018`..`020` gerbang bukti · `VAL-BD-065` lokasi nonaktif |
| `Allocated` | Berikan lewat jalur darurat | `Issued` (ditandai melewati gerbang) | **Peran berwenang** `DEF-BD-004` | Alasan wajib; penanda permanen; **wajib menyebut gerbang yang dilewati** — bukti, lokasi nonaktif, atau keduanya (`INV-BD-030`) | `VAL-BD-021` peran/alasan · `VAL-BD-066` keterangan gerbang |
| `Allocated` | Batalkan alokasi | `Available`, atau `PendingReview` bila order asal berakhir | Petugas Bank Darah | Kantong belum diberikan; alasan terkendali; keaktifan order dibaca `BbkEncounterStatusReader`. **Kembali ke `Available`, tidak pernah ke `Stored` maupun `Received`** — tonggak penempatan hanya dilewati sekali | `VAL-BD-023` sudah diberikan · `VAL-BD-016` alasan |
| `Available` / `Allocated` | Order berakhir | `PendingReview` | Sistem | Order `Cancelled`/`Expired` | — |
| `PendingReview` | Alihkan ke pasien lain | `Reallocated` | Petugas berwenang | Kelayakan dinyatakan manusia; alasan wajib; **bukti lama gugur** (`DEC-BD-028`); **lokasi penempatan terakhir sedang aktif** — pengalihan adalah alokasi dengan nama lain (`INV-BD-028`) | `VAL-BD-016` · `VAL-BD-064` lokasi nonaktif |
| `PendingReview` | Kembalikan ke PMI | `ReturnedToProvider` | Petugas berwenang | Proses PMI mendukung (`OQ-BD-010`) | `VAL-BD-016` |
| `PendingReview` | Nyatakan tidak layak | `NotUsable` | Petugas berwenang | Alasan wajib | `VAL-BD-016` |
| `Issued` | Catat koreksi pencatatan | tetap `Issued` (koreksi melekat) | Peran berwenang `DEF-BD-004` | Menunjuk pemberian yang ada; alasan terkendali | `VAL-BD-024` peran · `VAL-BD-025` |

**Terminal tak dapat dibatalkan:** `Issued`, `ReturnedToProvider`, `NotUsable`. Koreksi **tidak**
memindahkan kantong keluar dari `Issued`, tidak mengembalikan ke `Available`, tidak membatalkan apa pun.

**Kondisi turunan (bukan status).** Dua hal berikut **tidak** memindahkan kantong ke status mana pun;
yang berubah hanya jawaban gerbang saat diperiksa:

| Kondisi | Kantong tetap | Akibatnya |
| --- | --- | --- |
| Masa berlaku bukti kecocokan lewat | `Allocated` | Gerbang pemberian tertutup (`ARCH-BD-POS-01`) |
| Lokasi penempatan terakhir dinonaktifkan | status apa adanya | Gerbang alokasi **dan** pemberian tertutup (`ARCH-BD-POS-06`, `DEC-BD-037`) |

Keduanya sengaja **bukan** status, karena menjadikannya status menuntut penyuntingan massal setiap kali
satu kulkas dinonaktifkan atau setiap kali waktu berjalan.

**Transisi yang TIDAK sah** — disebutkan supaya tidak diusulkan sebagai "perbaikan":

| Yang tidak sah | Sebabnya |
| --- | --- |
| `Received` → `Allocated` langsung | Gerbang penyimpanan wajib dilewati (`INV-BD-025`, `VAL-BD-063`) |
| `Available`/`Allocated` → `Stored` atau `Received` | Tonggak penempatan hanya dilewati sekali; pembatalan alokasi mengembalikan ke `Available` |
| Lokasi dinonaktifkan → kantong otomatis `PendingReview` | Ditolak `DEC-BD-037`; sistem tidak memindahkan dan tidak mengubah status |
| Lokasi dinonaktifkan → kantong otomatis pindah lokasi | Ditolak `DEC-BD-037`; perpindahan fisik kewenangan petugas BDRS |
| Perpindahan lokasi → mengubah status kantong | Perpindahan tidak pernah menjadi perpindahan status (`DEC-BD-036`) |
| Perpindahan lokasi → mengubah/menghapus penempatan lama | Riwayat penempatan hanya bertambah (`INV-BD-026`) |
| `Issued` lewat jalur normal dari lokasi nonaktif | Ditolak `DEC-BD-038` (`VAL-BD-065`); satu-satunya jalan adalah jalur darurat |

---

## 4. Pemeriksaan Golongan Darah — `BbkBloodGroupExamStatus` + keadaan konflik

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Catat pengambilan sampel | `SampleTaken` | Petugas pengambil | Pasien sah | — |
| `SampleTaken` | Catat hasil ABO & Rhesus | `ResultRecorded` | Pemeriksa | Pemeriksa & waktu tersimpan | `VAL-BD-030` |
| `ResultRecorded` | Validasi | `Validated`, `IsValidResult=true` | **Peran validator** `DEF-BD-004` | Bila berbeda dari hasil sah sebelumnya → hasil lama & baru `IsConflictHeld=true`, pasien tak punya hasil sah (`BD-XINV-04`) | `VAL-BD-037` bukan validator |
| `Validated` (konflik) | Catat pemeriksaan ulang | konflik masih tertahan | Petugas Bank Darah | Sampel & hasil baru tervalidasi | `VAL-BD-051` selesai tanpa pemeriksaan ulang |
| konflik tertahan | Selesaikan konflik | satu `IsValidResult=true` kembali | **Peran validator** `DEF-BD-004` | **Wajib** menunjuk pemeriksaan ulang tervalidasi (`ResolvingExamId`); tak pernah hitung mayoritas (`DEC-BD-031`, `INV-BD-022`) | `VAL-BD-051` · `VAL-BD-037` |

**Catatan:** hasil `Validated` **tak pernah** ditimpa. Hasil ketiga yang berbeda dari keduanya tetap
boleh menjadi sah bila validator menyatakannya (`AC-BD-053`).

---

## 5. Tindakan Bank Darah — `BbkProcedureStatus`

| Dari | Tindakan | Ke | Siapa yang boleh | Syarat | Bila dilanggar |
| --- | --- | --- | --- | --- | --- |
| — | Catat tindakan | `Recorded` | Petugas Bank Darah | Menunjuk satu order sah; tarif dirujuk (snapshot) | `VAL-BD-026` |
| `Recorded` | Nyatakan selesai | `Completed` | Petugas Bank Darah | — | — |

**Penyaluran fakta biaya ke Billing tertahan `DEC-BD-016`** — tidak ada perpindahan yang menyalurkan
charge pada kontrak ini.

---

## 6. Perpindahan yang **tidak sah** (wajib ditolak)

| Percobaan | Kenapa ditolak | Kode |
| --- | --- | --- |
| Memberi kantong `Available` (belum dialokasikan) | Harus `Allocated` lebih dulu | `VAL-BD-017` |
| Memberi kantong tanpa bukti kecocokan & tanpa jalur darurat | Gerbang pemberian (`INV-BD-012`) | `VAL-BD-018` |
| Memberi dengan bukti milik pasien lain, atau bukti sudah lewat masa berlaku | Bukti terikat pasangan kantong+pasien & masa berlaku (`INV-BD-019/020`) | `VAL-BD-019`, `VAL-BD-020` |
| Membatalkan alokasi kantong yang sudah `Issued` | Pemberian terminal; jalurnya koreksi | `VAL-BD-023` |
| Menghapus / menganulir pemberian | Pemberian tak pernah dihapus/dibalik (`INV-BD-021`) | `VAL-BD-025` |
| Memakai koreksi untuk memindah pemberian ke pasien lain | Koreksi hanya mencatat kekeliruan pencatatan (`DEC-BD-030`) | `VAL-BD-049` |
| Mengalokasikan kantong `Excess`/`PendingReview` langsung ke order | Wajib lewat penyelesaian `DEC-BD-019` dulu | `VAL-BD-033` |
| Menghidupkan kembali order `Expired` | Tak dapat di-reopen (`ASM-BD-002`) | `VAL-BD-004` |
| Membuat sisa permintaan negatif | Batas bawah 0 (`INV-BD-017`) | dijaga sistem |
| Memakai golongan darah saat pasien `IsConflictHeld` | Gerbang tertahan (`DEC-BD-026`) | `VAL-BD-034` |
| Menutup konflik dengan sistem memilih mayoritas otomatis | Sistem tak memutus klinis (`INV-BD-013/022`) | `VAL-BD-054` |
| Memakai `MstPatient.BloodType` sebagai sumber klinis | Bukan sumber sah (`INV-BD-014`) | `VAL-BD-012` |
