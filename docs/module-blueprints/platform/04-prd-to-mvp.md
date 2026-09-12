# PRD → MVP — Platform / Alokasi Nomor Bisnis

| Field | Nilai |
| --- | --- |
| Blueprint | `PLT-BP-001` revisi 1 |
| Slice | `PLT-SLICE-01` |
| Contract version | `v1` — ✅ **`approved`** `2026-09-09` |
| Status | ✅ **`APPROVED`** `2026-09-09` oleh **`Sukma Giri Pratama`** (`sukmagp`) |

---

## 1. Batas MVP

| Batas | Isi |
| --- | --- |
| **Titik mulai** | Sebuah service modul memanggil alokator dengan penanda deret, awalan, jumlah digit, dan kebijakan pengulangan miliknya sendiri |
| **Titik akhir** | Alokator memulangkan satu nomor jadi yang unik dan permanen, dan kenaikan pencacahnya sudah tersimpan lepas dari nasib pekerjaan pemanggil |
| **Di luar batas** | Pemindahan deret lama, penyeragaman format, penelusuran nomor kembar produksi, peringatan deret hampir habis |

**Ukuran keberhasilan yang paling jujur:** gerbang `G4` modul Bank Darah tertutup, dan sembilan
task backend yang tertahan di sana dapat dijadwalkan.

---

## 2. Kemampuan `MUST HAVE`

| Kemampuan | ID asal | Disposisi | Catatan |
| --- | --- | --- | --- |
| Alokasi nomor atomik ber-scope yang durabel | `PLT-CAP-001` | **`EXTEND`** | Mesin sudah ada pada `BillingNumberSeriesService`; diekstrak, bukan dibangun dari nol (`DEC-PLT-007`) |
| Pencacah bertahan saat transaksi bisnis batal | `PLT-CAP-007` | **`REPAIR`** | Perilaku sekarang kebalikannya (`CONF-PLT-002`) |
| Penyimpanan pencacah per deret dan periode | `PLT-CAP-001` | `EXTEND` | Bentuk tabel mengikuti `BilNumberSeries` supaya `PLT-SLICE-02` menjadi penyalinan baris |
| Penjagaan nomor kembar di tingkat database | `PLT-CAP-001` | `EXTEND` | Index unik `(SequenceKey, ScopeKey)` |
| Pembagian kewenangan platform/modul | — | **`MISSING / NEW`** | Tanda tangan method memaksa modul menyerahkan awalan dan formatnya sendiri (`DEC-PLT-005`) |

---

## 3. Kemampuan yang ditunda

| Ditunda | Sebab | Pengganti selama MVP berjalan |
| --- | --- | --- |
| Pemindahan empat deret Billing | `DEC-PLT-003` mengurutkan migrasi menurut risiko; keempatnya deret terpanas dan sudah produksi. Menuntut `DEC-PLT-006` | Keempatnya tetap dilayani mesin lama, perilakunya **tidak berubah** |
| Penyeragaman format dan panjang nomor | `OQ-PLT-005` terbuka | Deret baru memakai format yang ditetapkan modulnya masing-masing |
| Kode fasilitas dalam awalan | `OQ-PLT-006` terbuka | Awalan tetap ditulis modul; fasilitas belum menjadi dimensi |
| Penelusuran nomor kembar produksi | `PLT-SLICE-04`, menuntut akses data produksi | Tidak ada; risiko lama dibiarkan apa adanya sampai slice itu |
| Peringatan deret hampir habis | `OQ-PLT-005` terbuka | `VAL-PLT-007` menolak alokasi yang tidak muat, sehingga kegagalannya tegas dan tidak diam-diam |

---

## 4. Epic

### `EPIC-PLT-01` — Mesin alokasi durabel *(MUST HAVE)*

| ID | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-PLT-001` | Alokator memulangkan satu nomor jadi dari penanda deret, awalan, jumlah digit, dan kebijakan pengulangan yang diberikan pemanggil | `EXTEND` |
| `FR-PLT-002` | Kenaikan pencacah tersimpan permanen sebelum pemanggil menyelesaikan pekerjaannya | `MISSING / NEW` |
| `FR-PLT-003` | Nomor yang terbit lalu pekerjaannya dibatalkan **tidak pernah** diterbitkan lagi | `MISSING / NEW` |
| `FR-PLT-004` | Dua alokasi bersamaan pada deret sama menghasilkan dua nomor berbeda tanpa kegagalan | `EXTEND` |
| `FR-PLT-005` | Alokasi pada deret berbeda tidak saling menunggu | `EXTEND` |
| `FR-PLT-006` | Parameter yang tidak sah ditolak sebelum satu nomor pun terbit | `EXTEND` |
| `FR-PLT-007` | Nilai yang tidak muat pada jumlah digit ditolak, bukan diterbitkan menyimpang | `MISSING / NEW` |

**UAT jalur berhasil.** Dua order darah berurutan mendapat nomor berbeda dan berurutan.

**UAT jalur gagal.** Order yang ditolak validasi menghanguskan nomornya; order berikutnya
mendapat nomor **berikutnya**, bukan nomor yang hangus. Deret berlubang, dan lubang itu dibiarkan.

### `EPIC-PLT-02` — Penyimpanan dan penjagaan deret *(MUST HAVE)*

| ID | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-PLT-008` | Satu baris menyimpan pencacah satu deret pada satu periode | `MISSING / NEW` |
| `FR-PLT-009` | Pasangan penanda deret dan periode dijaga unik di tingkat database | `MISSING / NEW` |
| `FR-PLT-010` | Baris deret lahir sendiri pada alokasi pertama, tanpa penyemaian | `MISSING / NEW` |
| `FR-PLT-011` | Nol jalur kode yang menurunkan, menyetel ulang, atau menghapus pencacah | `MISSING / NEW` |

**UAT jalur berhasil.** Deret yang belum pernah dipakai menerbitkan nomor pertamanya tanpa
persiapan apa pun.

**UAT jalur gagal.** Percobaan menyisipkan baris kembar ditolak database.

### `EPIC-PLT-03` — Layar pemantauan deret *(SHOULD HAVE)*

| ID | Functional requirement | Disposisi |
| --- | --- | --- |
| `FR-PLT-012` | Administrator melihat daftar deret beserta nilai pencacah dan waktu alokasi terakhir | `MISSING / NEW` |
| `FR-PLT-013` | Nol tombol yang mengubah pencacah | `MISSING / NEW` |

**Bukan `MUST HAVE`, dan itu disengaja.** Gerbang `G4` tertutup tanpa satu layar pun. Menjadikan
layar ini `MUST HAVE` akan menahan sembilan task Bank Darah demi daftar yang dibuka administrator
beberapa kali setahun.

**UAT jalur berhasil.** Administrator membuka layar dan melihat ketiga deret Bank Darah beserta
nilainya.

**UAT jalur gagal.** Pengguna tanpa butir `NumberSeries : Read` tidak melihat menunya, dan route
langsung ditolak backend.

---

## 5. Urutan pengiriman

| Gelombang | Isi | Membuka |
| --- | --- | --- |
| **`MVP-0`** | Entri registry `Platform` / `Num` disetujui, lalu `PLANNED` → `ACTIVE` | Wewenang membuat model pertama (`QBE-MOD-002`) |
| **`MVP-1`** | `EPIC-PLT-01` + `EPIC-PLT-02` — tabel, migration, alokator, uji integrasi PostgreSQL | **Gerbang `G4` tertutup.** Sembilan task backend Bank Darah terbuka |
| **`MVP-2`** | `EPIC-PLT-03` — layar pemantauan | Kemudahan telusur bagi administrator |
| **`POST-MVP`** | `PLT-SLICE-02` migrasi deret lama · `PLT-SLICE-03` penyeragaman format · `PLT-SLICE-04` penelusuran nomor kembar | Menuntut `DEC-PLT-006`, `OQ-PLT-005`, `OQ-PLT-009` dijawab lebih dulu |

**`MVP-0` bukan formalitas.** Tanpa baris registry, pembuatan model pertama berstatus `BLOCKED`
menurut `QBE-MOD-002`, dan prefix tidak boleh disimpulkan dari nama folder (`QBE-NAM-004`).

---

## 6. Definition of Done

| Butir | Cara menjawabnya |
| --- | --- |
| Baris registry `Platform` / `Num` disetujui dan `ACTIVE` | Ya / belum — lihat tabel registry |
| Tabel `NumNumberSeries` ada beserta index unik dan dua check constraint | Ya / belum — lihat migration |
| Migration dibuat | Ya / belum |
| Migration **dijalankan** | Ya / belum — wewenang terpisah |
| `AC-PLT-003` lulus di PostgreSQL sungguhan | Ya / belum — bukan InMemory |
| `AC-PLT-004` lulus di PostgreSQL sungguhan | Ya / belum |
| Empat deret Billing terbukti tidak berubah perilakunya | Ya / belum — `AC-PLT-012` |
| Nol jalur kode yang menurunkan atau menyetel ulang pencacah | Ya / belum |
| Butir `NumberSeries : Read` terdaftar dan dapat dicentang di layar Akses Role | Ya / belum |
| Gerbang `G4` Bank Darah ditandai tertutup pada roadmapnya | Ya / belum |

---

## 7. Pertanyaan terbuka sebelum development lock

| ID | Pertanyaan | Memblokir? |
| --- | --- | --- |
| ~~`OQ-PLT-012`~~ | ~~Area registry modul Platform~~ ✅ **Tertutup 9 Sep 2026** → `DEC-PLT-009`: Area **`Platform`** baru | Tidak lagi |
| ~~`OQ-PLT-013`~~ | ~~Prefix entity modul Platform~~ ✅ **Tertutup 9 Sep 2026** → `DEC-PLT-010`: prefix **`Num`** | Tidak lagi |
| `OQ-PLT-014` | Baris registry hasil kedua keputusan itu **belum dicatat** ke `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, lifecycle belum `ACTIVE` | **Memblokir `MVP-1`, bukan perencanaan.** `QBE-MOD-002` menahan model pertama. Inilah isi gelombang `MVP-0` |
| **Baru** | `AddDbContextFactory` berdampingan dengan `AddDbContext` yang sudah ada — perlu penyesuaian pendaftaran? | **Tidak memblokir desain**, wajib diverifikasi saat implementasi |
| `OQ-PLT-005` | Panjang nomor dan nasib deret yang hampir habis | Tidak — `VAL-PLT-007` menahannya secara tegas untuk sementara |
| `OQ-PLT-006` | Kode fasilitas di dalam awalan | Tidak — `LATER SLICE` |
| `DEC-PLT-006` | Sampai kapan pelanggaran `INV-PLT-001` selama peralihan diterima | Tidak untuk slice ini — memblokir `PLT-SLICE-02` |
| `OQ-PLT-009` | Penelusuran nomor kembar yang mungkin sudah terbit | Tidak — `PLT-SLICE-04` |

**Kedua pertanyaan yang memblokir sudah tertutup** pada amendment pass 9 September 2026, sehingga
dokumen ini **kini boleh** diteruskan ke `plan-module-delivery` begitu blueprint disetujui.

`OQ-PLT-014` yang tersisa **tidak** menahan perencanaan — ia menahan **implementasi**, dan sudah
punya tempatnya sendiri sebagai gelombang `MVP-0` di bagian 5. Perbedaan itu disengaja: roadmap
justru perlu ada untuk menjadwalkan pencatatan baris registry itu.
