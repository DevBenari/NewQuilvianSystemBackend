# Register Aset — Kontrak Integrasi

| Field | Value |
|---|---|
| `last_changed_in` | `AST-REG-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |
| Traceability | `AST-DEC-008`, `015`, `033`, `034`, `040`; `TS-01..07`, `TS-12`, `TS-17` |

Sub-modul ini tidak memanggil sistem di luar Quilvian. Seluruh integrasinya internal: membaca data
modul lain, memakai kapabilitas platform, dan melayani empat sub-modul saudara.

## 1. Membaca modul lain (sinkron, di dalam proses)

| ID | Data | Pemilik | Cara | Bila gagal/kosong |
|---|---|---|---|---|
| `INT-REG-01` | Unit organisasi aktif | HR (`MstOrganizationUnit`) | Query EF `AsNoTracking`, hanya kolom kode, nama, induk, aktif | Unit tidak muncul di pilihan; lokasi tidak dapat dibuat untuk unit itu |
| `INT-REG-02` | Ruangan klinis | `MstRoom` | Query baca untuk pilihan tautan lokasi | Tautan dikosongkan; lokasi tetap sah |
| `INT-REG-03` | Pegawai aktif | `MstEmployee` + `MstWorkforceProfile` | Query baca nama dan nomor pegawai | Penanggung jawab tidak dapat dipilih |
| `INT-REG-04` | Unit dan Kepala Unit pengguna | `WfpOrganizationAssignment` (`IsActive`, rentang efektif, `IsManagerialAssignment`) | `AssetActorResolver` | Pengguna tanpa penugasan aktif melihat **nol** aset (fail-closed); Kepala Unit tanpa penugasan manajerial tidak dapat mengonfirmasi (`AST-UNK-004`) |
| `INT-REG-05` | GR dan PO | Finance (`FinGoodsReceipt`, `FinPurchaseOrder`) | Query baca nomor, tanggal, status, pemasok; FK nullable `Restrict` dari `AstAsset` | GR dibatalkan Finance sesudah ditautkan: tautan tetap, layar menandai "GR dibatalkan" |
| `INT-REG-06` | Badan hukum dan site | `MstLegalEntity`, `MstHospitalSite` | Satu badan hukum, satu site (`AST-DEC-035`) diambil dari `MstAssetSetting` | Bila pengaturan belum ada, pencatatan ditolak |

## 2. Kapabilitas platform

| ID | Kapabilitas | Pemakaian | Perilaku khusus |
|---|---|---|---|
| `INT-REG-10` | `NumberSeriesAllocator.AllocateAsync` | Nomor aset `AST-<KODE>-######` (`SequenceKey = AST_INV_<KODE>`, `NEVER`, 6 digit) dan nomor dokumen `AST-PNY`, `AST-KND`, `AST-PMF`, `AST-IMP` | Alokasi berjalan di transaksi sendiri; bila persetujuan aset gagal sesudah nomor terbit, nomor **hangus** dan tidak diisi ulang. Contoh: `AST-ALKMED-000124` hangus, aset berikutnya `000125` |
| `INT-REG-11` | QRCoder + QuestPDF | Label QR dan berita acara | QR hanya memuat nomor inventaris, tanpa data lain |
| `INT-REG-12` | ClosedXML | Templat dan pembacaan impor | — |

## 3. Melayani sub-modul saudara (sinkron, di dalam proses, ikut transaksi pemanggil)

| ID | Service | Dipanggil | Untuk | Jaminan |
|---|---|---|---|---|
| `INT-REG-20` | `AssetProcessLockService.Acquire/Release` | `pergerakan`, `penghapusan` | Kunci proses aktif | Gagal `409` bila kunci terpakai; memeriksa `RowVersion` |
| `INT-REG-21` | `AssetPlacementService.ApplyPlacement` | `pergerakan` (serah terima mutasi), `inventarisasi` (koreksi beda ruangan lewat penyerahan) | Menutup penempatan lama, membuka yang baru | Satu transaksi dengan dokumen pemanggil |
| `INT-REG-22` | `AssetConditionService.OpenReport` | `pergerakan` (pengembalian rusak), `inventarisasi` (tindak lanjut) | Membuka laporan kondisi bersumber sistem | Laporan tetap harus ditetapkan manusia |
| `INT-REG-23` | `AssetRecordService.MarkDisposed/Restore` | `penghapusan` | `Terdaftar` ↔ `Dihapus` | Hanya dari usulan `Final` / pemulihan disetujui |
| `INT-REG-24` | `AssetHistoryService.Append` | Seluruh sub-modul | Linimasa | Ikut transaksi pemanggil; tidak pernah menulis sendiri |
| `INT-REG-25` | `AssetActorResolver` | Seluruh sub-modul | Unit pengguna, Kepala Unit, `ReadAllUnits` | Fail-closed |
| `INT-REG-26` | `AssetQueryService.GetAssetsForUnit` | `inventarisasi` | Snapshot aset yang diharapkan saat opname dimulai | — |

## 4. Memanggil `nilai` (di dalam proses, ikut transaksi)

| ID | Service milik `nilai` | Saat | Isi |
|---|---|---|---|
| `INT-REG-30` | `AssetBookService.CreateForRegisteredAsset` | Aset disetujui | Buku nilai: dikapitalisasi atau tidak (harga ≥ batas berlaku **dan** jenis kepemilikan dapat dikapitalisasi **dan** kategori dapat disusutkan untuk penyusutan), harga perolehan, umur dan residu dari kebijakan kelompok |
| `INT-REG-31` | `AssetAccountingEventService.StageDonationAcquisition` | Aset hibah disetujui | Kejadian perolehan hibah ke kotak keluar (`AST-DEC-040`) |
| `INT-REG-32` | `AssetBookService.CreateOpeningBalance` | Batch impor diterapkan | Buku nilai dengan akumulasi per tanggal awal; **tanpa** kejadian Akuntansi (`AST-DEC-023`) |
| `INT-REG-33` | `AssetValuationParameterQueryService` | Pemanfaatan diajukan | Batas rupiah persetujuan Direktur yang berlaku |

Contoh `INT-REG-30`: batas kapitalisasi berlaku Rp 1.000.000. Kursi Rp 450.000 → buku dibuat
`IsCapitalized=false`. Monitor Rp 85.000.000 jenis kepemilikan `MILIK` → `IsCapitalized=true`, umur 60
bulan dari kebijakan kelompok `ALKMED`, penyusutan mulai bulan sesudah tanggal siap pakai.
